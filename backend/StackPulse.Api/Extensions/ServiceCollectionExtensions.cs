using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackPulse.Api.Configuration;
using StackPulse.Api.Data;
using StackPulse.Api.Services;
using StackPulse.Api.Services.Interfaces;

namespace StackPulse.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddStackPulseServices(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>() ?? new JwtSettings();

        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
        services.Configure<DatabaseSettings>(configuration.GetSection("DatabaseSettings"));
        services.Configure<MongoDbSettings>(configuration.GetSection("MongoDbSettings"));
        services.Configure<AwsSecretsManagerSettings>(configuration.GetSection("AwsSecretsManager"));
        services.AddSingleton<ISecretConnectionStringResolver, AwsSecretsManagerConnectionStringResolver>();

        var resolvedSecrets = ResolveConnectionSecrets(configuration);

        var provider = configuration["DatabaseSettings:Provider"] ?? "SqlServer";
        var connectionString = resolvedSecrets.MySqlConnectionString
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? configuration["DatabaseSettings:ConnectionString"]
            ?? string.Empty;
        var mongoConnectionString = resolvedSecrets.MongoConnectionString
            ?? configuration["MongoDbSettings:ConnectionString"]
            ?? string.Empty;

        services.PostConfigure<MongoDbSettings>(settings =>
        {
            if (!string.IsNullOrWhiteSpace(mongoConnectionString))
            {
                settings.ConnectionString = mongoConnectionString;
            }
        });

        if (provider.Equals("MySql", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddDbContext<StackPulseDbContext>(options =>
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
        }
        else if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddDbContext<StackPulseDbContext>(options =>
                options.UseSqlServer(connectionString));
        }
        else
        {
            services.AddDbContext<StackPulseDbContext>(options =>
                options.UseInMemoryDatabase("StackPulseDb"));
        }

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtSettings.Key)),
                ClockSkew = TimeSpan.FromMinutes(1)
            };
        });

        services.AddAuthorization();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddSingleton<MongoStackPulseContext>();

        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        return services;
    }

    private static (string? MySqlConnectionString, string? MongoConnectionString) ResolveConnectionSecrets(IConfiguration configuration)
    {
        var settings = configuration.GetSection("AwsSecretsManager").Get<AwsSecretsManagerSettings>() ?? new AwsSecretsManagerSettings();
        if (!settings.Enabled)
        {
            return (null, null);
        }

        var resolver = new AwsSecretsManagerConnectionStringResolver(Microsoft.Extensions.Options.Options.Create(settings));
        return resolver.ResolveAsync().GetAwaiter().GetResult();
    }
}
