
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using NLog.Web;
using StackPulse.Api.Data;
using StackPulse.Api.Extensions;
using StackPulse.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseNLog();

builder.Services.AddStackPulseServices(builder.Configuration);

builder.Services.AddHealthChecks()
    .AddDbContextCheck<StackPulseDbContext>();

var app = builder.Build();

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "StackPulse API V1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".js"] = "application/javascript";

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/api/health");

var webRoot = Path.Combine(app.Environment.ContentRootPath, "wwwroot");
if (Directory.Exists(webRoot))
{
    app.UseDefaultFiles();
    app.UseStaticFiles();
    app.MapFallbackToFile("index.html");
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<StackPulseDbContext>();
    var dbType = db.Database.ProviderName;
    if (!string.Equals(dbType, "Microsoft.EntityFrameworkCore.InMemory", StringComparison.Ordinal))
    {
        db.Database.Migrate();
    }
}

app.Run();
