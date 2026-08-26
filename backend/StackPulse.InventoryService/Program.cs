using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.WindowsServices;
using StackPulse.Api.Configuration;
using StackPulse.Api.Data;
using StackPulse.Api.Services;
using StackPulse.InventoryService.Services;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, cfg) =>
    {
        // default configuration sources already added (appsettings.json)
    })
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;
        services.Configure<DatabaseSettings>(configuration.GetSection("DatabaseSettings"));
        services.Configure<MongoDbSettings>(configuration.GetSection("MongoDbSettings"));
        services.Configure<AwsSecretsManagerSettings>(configuration.GetSection("AwsSecretsManager"));

        var awsSettings = configuration.GetSection("AwsSecretsManager").Get<AwsSecretsManagerSettings>() ?? new AwsSecretsManagerSettings();
        var secrets = new AwsSecretsManagerConnectionStringResolver(Microsoft.Extensions.Options.Options.Create(awsSettings))
            .ResolveAsync()
            .GetAwaiter()
            .GetResult();

        var provider = configuration["DatabaseSettings:Provider"] ?? "SqlServer";
        var connectionString = secrets.MySqlConnectionString
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? configuration["DatabaseSettings:ConnectionString"]
            ?? string.Empty;
        var mongoConnectionString = secrets.MongoConnectionString ?? configuration["MongoDbSettings:ConnectionString"] ?? string.Empty;

        services.PostConfigure<MongoDbSettings>(settings =>
        {
            if (!string.IsNullOrWhiteSpace(mongoConnectionString))
            {
                settings.ConnectionString = mongoConnectionString;
            }
        });

        if (provider.Equals("MySql", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddDbContext<StackPulseDbContext>(options => options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
        }
        else if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddDbContext<StackPulseDbContext>(options => options.UseSqlServer(connectionString));
        }
        else
        {
            services.AddDbContext<StackPulseDbContext>(options => options.UseInMemoryDatabase("StackPulseDb"));
        }

        services.AddSingleton<MongoStackPulseContext>();
        services.AddHostedService<WindowsInventoryBackgroundService>();
    })
    .UseWindowsService()
    .UseSystemd();

var host = builder.Build();
await host.RunAsync();
