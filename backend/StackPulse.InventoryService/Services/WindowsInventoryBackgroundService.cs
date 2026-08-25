using System.Management;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using StackPulse.Api.Data;
using StackPulse.Api.Models;
using Microsoft.Win32;

namespace StackPulse.InventoryService.Services;

public class WindowsInventoryBackgroundService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<WindowsInventoryBackgroundService> _logger;
    private readonly IConfiguration _configuration;

    public WindowsInventoryBackgroundService(IServiceProvider services, ILogger<WindowsInventoryBackgroundService> logger, IConfiguration configuration)
    {
        _services = services;
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("WindowsInventoryBackgroundService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<StackPulseDbContext>();

                var inventory = new MachineInventory
                {
                    Hostname = Environment.MachineName,
                    OSVersion = Environment.OSVersion.ToString(),
                    CollectedAt = DateTime.UtcNow,
                    WindowsServices = GetWindowsServices(),
                    InstalledSoftwares = GetInstalledSoftware(),
                    Drives = GetDrives()
                };

                db.MachineInventories.Add(inventory);
                await db.SaveChangesAsync(stoppingToken);
                _logger.LogInformation("Saved machine inventory with {Id}", inventory.Id);

                var jiraIssues = await FetchJiraIssuesAsync();
                if (jiraIssues?.Any() == true)
                {
                    foreach (var j in jiraIssues)
                    {
                        j.MachineInventoryId = inventory.Id;
                        db.JiraIssues.Add(j);
                    }
                }

                var bbprs = await FetchBitbucketPullRequestsAsync();
                if (bbprs?.Any() == true)
                {
                    foreach (var p in bbprs)
                    {
                        p.MachineInventoryId = inventory.Id;
                        db.BitbucketPullRequests.Add(p);
                    }
                }

                await db.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error collecting/saving inventory");
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }

    private List<WindowsServiceInfo> GetWindowsServices()
    {
        var list = new List<WindowsServiceInfo>();
        try
        {
            var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Service");
            foreach (ManagementObject service in searcher.Get())
            {
                list.Add(new WindowsServiceInfo
                {
                    ServiceName = service["Name"]?.ToString(),
                    DisplayName = service["DisplayName"]?.ToString(),
                    State = service["State"]?.ToString(),
                    StartMode = service["StartMode"]?.ToString()
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enumerate Windows services");
        }

        return list;
    }

    private List<InstalledSoftwareInfo> GetInstalledSoftware()
    {
        var results = new List<InstalledSoftwareInfo>();
        try
        {
            var hives = new[]
            {
                Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall"),
                Registry.LocalMachine.OpenSubKey("SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall")
            };

            foreach (var hive in hives)
            {
                if (hive == null) continue;
                foreach (var subKeyName in hive.GetSubKeyNames())
                {
                    try
                    {
                        using var subKey = hive.OpenSubKey(subKeyName);
                        var displayName = subKey?.GetValue("DisplayName")?.ToString();
                        if (string.IsNullOrWhiteSpace(displayName)) continue;

                        var version = subKey.GetValue("DisplayVersion")?.ToString();
                        var publisher = subKey.GetValue("Publisher")?.ToString();

                        results.Add(new InstalledSoftwareInfo
                        {
                            Name = displayName,
                            Version = version,
                            Publisher = publisher
                        });
                    }
                    catch { }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enumerate installed software");
        }

        return results.GroupBy(x => x.Name).Select(g => g.First()).ToList();
    }

    private List<DriveInfoEntry> GetDrives()
    {
        var drives = new List<DriveInfoEntry>();
        try
        {
            foreach (var d in System.IO.DriveInfo.GetDrives())
            {
                try
                {
                    drives.Add(new DriveInfoEntry
                    {
                        Name = d.Name,
                        DriveType = d.DriveType.ToString(),
                        TotalBytes = d.IsReady ? d.TotalSize : (long?)null,
                        FreeBytes = d.IsReady ? d.AvailableFreeSpace : (long?)null
                    });
                }
                catch { }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enumerate drives");
        }

        return drives;
    }

    private async Task<List<JiraIssue>?> FetchJiraIssuesAsync()
    {
        try
        {
            var baseUrl = _configuration["Jira:BaseUrl"];
            var username = _configuration["Jira:Username"];
            var apiToken = _configuration["Jira:ApiToken"];
            var jql = _configuration["Jira:Jql"] ?? "ORDER BY created DESC";

            if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(apiToken))
            {
                _logger.LogDebug("Jira settings not configured, skipping Jira fetch.");
                return null;
            }

            var client = new System.Net.Http.HttpClient();
            var auth = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{username}:{apiToken}"));
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", auth);

            var url = baseUrl.TrimEnd('/') + "/rest/api/2/search?jql=" + Uri.EscapeDataString(jql) + "&maxResults=50";
            var resp = await client.GetAsync(url);
            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogWarning("Jira fetch returned {Status}", resp.StatusCode);
                return null;
            }

            using var stream = await resp.Content.ReadAsStreamAsync();
            using var doc = await System.Text.Json.JsonDocument.ParseAsync(stream);
            var issues = new List<JiraIssue>();

            if (doc.RootElement.TryGetProperty("issues", out var issuesElem) && issuesElem.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                foreach (var item in issuesElem.EnumerateArray())
                {
                    var key = item.GetProperty("key").GetString();
                    var fields = item.GetProperty("fields");
                    var summary = fields.GetProperty("summary").GetString();
                    var status = fields.GetProperty("status").GetProperty("name").GetString();

                    issues.Add(new JiraIssue
                    {
                        Key = key,
                        Summary = summary,
                        Status = status,
                        Url = baseUrl.TrimEnd('/') + "/browse/" + key,
                        CollectedAt = DateTime.UtcNow
                    });
                }
            }

            return issues;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Jira issues");
            return null;
        }
    }

    private async Task<List<BitbucketPullRequest>?> FetchBitbucketPullRequestsAsync()
    {
        try
        {
            var baseUrl = _configuration["Bitbucket:BaseUrl"] ?? "https://api.bitbucket.org/2.0";
            var username = _configuration["Bitbucket:Username"];
            var appPassword = _configuration["Bitbucket:AppPassword"];
            var workspace = _configuration["Bitbucket:Workspace"];

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(appPassword) || string.IsNullOrWhiteSpace(workspace))
            {
                _logger.LogDebug("Bitbucket settings not configured, skipping Bitbucket fetch.");
                return null;
            }

            var client = new System.Net.Http.HttpClient();
            var auth = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{username}:{appPassword}"));
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", auth);

            var reposUrl = $"{baseUrl.TrimEnd('/')}/repositories/{workspace}?pagelen=50";
            var reposResp = await client.GetAsync(reposUrl);
            if (!reposResp.IsSuccessStatusCode)
            {
                _logger.LogWarning("Bitbucket repos fetch returned {Status}", reposResp.StatusCode);
                return null;
            }

            using var reposStream = await reposResp.Content.ReadAsStreamAsync();
            using var reposDoc = await System.Text.Json.JsonDocument.ParseAsync(reposStream);
            var prs = new List<BitbucketPullRequest>();

            if (reposDoc.RootElement.TryGetProperty("values", out var values) && values.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                foreach (var repo in values.EnumerateArray())
                {
                    var repoSlug = repo.GetProperty("slug").GetString();
                    var repoName = repo.GetProperty("name").GetString();

                    var prUrl = $"{baseUrl.TrimEnd('/')}/repositories/{workspace}/{repoSlug}/pullrequests?state=OPEN&pagelen=50";
                    var prResp = await client.GetAsync(prUrl);
                    if (!prResp.IsSuccessStatusCode) continue;

                    using var prStream = await prResp.Content.ReadAsStreamAsync();
                    using var prDoc = await System.Text.Json.JsonDocument.ParseAsync(prStream);
                    if (prDoc.RootElement.TryGetProperty("values", out var prValues) && prValues.ValueKind == System.Text.Json.JsonValueKind.Array)
                    {
                        foreach (var pr in prValues.EnumerateArray())
                        {
                            var title = pr.GetProperty("title").GetString();
                            var state = pr.GetProperty("state").GetString();
                            var author = pr.GetProperty("author").GetProperty("display_name").GetString();
                            var links = pr.GetProperty("links").GetProperty("html").GetProperty("href").GetString();

                            prs.Add(new BitbucketPullRequest
                            {
                                Repo = repoName,
                                Title = title,
                                State = state,
                                Author = author,
                                Url = links,
                                CollectedAt = DateTime.UtcNow
                            });
                        }
                    }
                }
            }

            return prs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Bitbucket PRs");
            return null;
        }
    }
}
