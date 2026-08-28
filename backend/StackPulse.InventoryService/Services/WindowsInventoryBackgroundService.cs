using System.Management;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
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
                var mongo = scope.ServiceProvider.GetRequiredService<MongoStackPulseContext>();

                if (!mongo.IsConfigured)
                {
                    _logger.LogWarning("MongoDB is not configured; skipping integration synchronization.");
                }
                else
                {
                    var jiraIssues = await FetchJiraIssuesAsync();
                    if (jiraIssues is not null)
                    {
                        await SaveIntegrationSyncAsync(db, mongo, "Jira", jiraIssues.Select(x => x.ToBsonDocument()), stoppingToken);
                    }

                    var bbprs = await FetchBitbucketPullRequestsAsync();
                    if (bbprs is not null)
                    {
                        await SaveIntegrationSyncAsync(db, mongo, "Bitbucket", bbprs.Select(x => x.ToBsonDocument()), stoppingToken);
                    }

                    var githubPrs = await FetchGitHubPullRequestsAsync();
                    if (githubPrs is not null)
                    {
                        await SaveIntegrationSyncAsync(db, mongo, "GitHub", githubPrs.Select(x => x.ToBsonDocument()), stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error collecting/saving inventory");
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }

    private static async Task SaveIntegrationSyncAsync(
        StackPulseDbContext db,
        MongoStackPulseContext mongo,
        string provider,
        IEnumerable<BsonDocument> items,
        CancellationToken cancellationToken)
    {
        var masterIntegrationAccessId = await db.IntegrationAccesses
            .Where(x => x.Provider == provider && x.IsActive)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        await mongo.IntegrationSync.InsertOneAsync(new BsonDocument
        {
            ["masterIntegrationAccessId"] = masterIntegrationAccessId.HasValue ? masterIntegrationAccessId.Value.ToString() : BsonNull.Value,
            ["provider"] = provider,
            ["startedAt"] = DateTime.UtcNow,
            ["completedAt"] = DateTime.UtcNow,
            ["status"] = "Completed",
            ["itemsProcessed"] = items.Count(),
            ["payload"] = new BsonArray(items)
        }, cancellationToken: cancellationToken);
    }

    private async Task<List<WindowsServiceInfo>> GetServicesAsync(CancellationToken cancellationToken)
    {
        if (OperatingSystem.IsWindows())
        {
            return GetWindowsServices();
        }

        if (OperatingSystem.IsLinux())
        {
            return await GetLinuxServicesAsync(cancellationToken);
        }

        if (OperatingSystem.IsMacOS())
        {
            return await GetMacServicesAsync(cancellationToken);
        }

        return new List<WindowsServiceInfo>();
    }

    [SupportedOSPlatform("windows")]
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
        if (OperatingSystem.IsWindows())
        {
            return GetWindowsInstalledSoftware();
        }

        if (OperatingSystem.IsLinux())
        {
            return GetLinuxInstalledSoftware();
        }

        if (OperatingSystem.IsMacOS())
        {
            return GetMacInstalledSoftware();
        }

        return new List<InstalledSoftwareInfo>();
    }

    [SupportedOSPlatform("windows")]
    private List<InstalledSoftwareInfo> GetWindowsInstalledSoftware()
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

                        var version = subKey?.GetValue("DisplayVersion")?.ToString();
                        var publisher = subKey?.GetValue("Publisher")?.ToString();

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

    private async Task<List<WindowsServiceInfo>> GetLinuxServicesAsync(CancellationToken cancellationToken)
    {
        var output = await RunCommandAsync("systemctl", "list-units --type=service --all --no-pager --plain --no-legend", cancellationToken);
        if (string.IsNullOrWhiteSpace(output))
        {
            return new List<WindowsServiceInfo>();
        }

        return output.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Take(300)
            .Select(line => line.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .Where(parts => parts.Length >= 4)
            .Select(parts => new WindowsServiceInfo
            {
                ServiceName = parts[0],
                DisplayName = parts[0],
                State = parts[3],
                StartMode = parts[2]
            })
            .ToList();
    }

    private async Task<List<WindowsServiceInfo>> GetMacServicesAsync(CancellationToken cancellationToken)
    {
        var output = await RunCommandAsync("launchctl", "list", cancellationToken);
        if (string.IsNullOrWhiteSpace(output))
        {
            return new List<WindowsServiceInfo>();
        }

        return output.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Skip(1)
            .Take(300)
            .Select(line => line.Split('\t', StringSplitOptions.RemoveEmptyEntries))
            .Where(parts => parts.Length >= 3)
            .Select(parts => new WindowsServiceInfo
            {
                ServiceName = parts[2],
                DisplayName = parts[2],
                State = parts[0] == "-" ? "Stopped" : "Running",
                StartMode = "launchd"
            })
            .ToList();
    }

    private List<InstalledSoftwareInfo> GetLinuxInstalledSoftware()
    {
        var results = new List<InstalledSoftwareInfo>();
        foreach (var dbPath in new[] { "/var/lib/dpkg/status", "/var/lib/rpm/Packages" })
        {
            if (!File.Exists(dbPath))
            {
                continue;
            }

            if (dbPath.EndsWith("status", StringComparison.Ordinal))
            {
                var packageBlocks = File.ReadAllText(dbPath).Split("\n\n", StringSplitOptions.RemoveEmptyEntries);
                foreach (var block in packageBlocks.Take(500))
                {
                    var lines = block.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                    var name = lines.FirstOrDefault(x => x.StartsWith("Package:", StringComparison.OrdinalIgnoreCase))?.Replace("Package:", string.Empty).Trim();
                    var version = lines.FirstOrDefault(x => x.StartsWith("Version:", StringComparison.OrdinalIgnoreCase))?.Replace("Version:", string.Empty).Trim();
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        results.Add(new InstalledSoftwareInfo { Name = name, Version = version, Publisher = "dpkg" });
                    }
                }
            }
            else
            {
                results.Add(new InstalledSoftwareInfo { Name = "RPM package database detected", Publisher = "rpm" });
            }
        }

        return results.GroupBy(x => x.Name).Select(g => g.First()).ToList();
    }

    private List<InstalledSoftwareInfo> GetMacInstalledSoftware()
    {
        var appDirectories = new[] { "/Applications", "/System/Applications" };
        return appDirectories
            .Where(Directory.Exists)
            .SelectMany(path => Directory.EnumerateDirectories(path, "*.app", SearchOption.TopDirectoryOnly))
            .Take(500)
            .Select(path => new InstalledSoftwareInfo
            {
                Name = Path.GetFileNameWithoutExtension(path),
                Publisher = "Application bundle"
            })
            .GroupBy(x => x.Name)
            .Select(g => g.First())
            .ToList();
    }

    private async Task<string?> RunCommandAsync(string fileName, string arguments, CancellationToken cancellationToken)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process is null)
            {
                return null;
            }

            var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);
            return process.ExitCode == 0 ? output : null;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Command {Command} was unavailable", fileName);
            return null;
        }
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
                    var projectKey = key?.Split('-', 2)[0];
                    var fields = item.GetProperty("fields");
                    var summary = fields.GetProperty("summary").GetString();
                    var status = fields.GetProperty("status").GetProperty("name").GetString();

                    issues.Add(new JiraIssue
                    {
                        Key = key,
                        ProjectKey = projectKey,
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
                                RepoSlug = repoSlug,
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

    private async Task<List<GitHubPullRequest>?> FetchGitHubPullRequestsAsync()
    {
        try
        {
            var baseUrl = _configuration["GitHub:BaseUrl"] ?? "https://api.github.com";
            var token = _configuration["GitHub:Token"];
            var repositories = (_configuration["GitHub:Repositories"] ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (string.IsNullOrWhiteSpace(token) || repositories.Length == 0)
            {
                _logger.LogDebug("GitHub settings not configured, skipping GitHub fetch.");
                return null;
            }

            using var client = new System.Net.Http.HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("StackPulse-InventoryService/1.0");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
            var prs = new List<GitHubPullRequest>();

            foreach (var repository in repositories)
            {
                var url = $"{baseUrl.TrimEnd('/')}/repos/{repository.Trim('/')}/pulls?state=open&per_page=50";
                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("GitHub pull request fetch for {Repository} returned {Status}", repository, response.StatusCode);
                    continue;
                }

                using var stream = await response.Content.ReadAsStreamAsync();
                using var document = await System.Text.Json.JsonDocument.ParseAsync(stream);
                if (document.RootElement.ValueKind != System.Text.Json.JsonValueKind.Array) continue;

                foreach (var pullRequest in document.RootElement.EnumerateArray())
                {
                    prs.Add(new GitHubPullRequest
                    {
                        Repository = repository,
                        Number = pullRequest.GetProperty("number").GetInt32(),
                        Title = pullRequest.GetProperty("title").GetString(),
                        Author = pullRequest.GetProperty("user").GetProperty("login").GetString(),
                        State = pullRequest.GetProperty("state").GetString(),
                        Url = pullRequest.GetProperty("html_url").GetString(),
                        CollectedAt = DateTime.UtcNow
                    });
                }
            }

            return prs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching GitHub pull requests");
            return null;
        }
    }
}
