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

        var provider = configuration["DatabaseSettings:Provider"] ?? "SqlServer";
        var connectionString = configuration.GetConnectionString("DefaultConnection") ?? configuration["DatabaseSettings:ConnectionString"] ?? string.Empty;

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

        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        return services;
    }
}
