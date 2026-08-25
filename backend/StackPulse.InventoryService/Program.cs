using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.WindowsServices;
using StackPulse.Api.Configuration;
using StackPulse.Api.Data;
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

        var provider = configuration["DatabaseSettings:Provider"] ?? "SqlServer";
        var connectionString = configuration.GetConnectionString("DefaultConnection") ?? configuration["DatabaseSettings:ConnectionString"] ?? string.Empty;

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

        services.AddHostedService<WindowsInventoryBackgroundService>();
    })
    .UseWindowsService();

var host = builder.Build();
await host.RunAsync();
