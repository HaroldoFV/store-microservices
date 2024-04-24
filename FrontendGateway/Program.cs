using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Serilog;
using Serilog.Events;
using Middleware;

var builder = WebApplication.CreateBuilder(args);

// Logging
builder.Host.UseSerilog((_, config) =>
{
    config
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .WriteTo.Console();
});

// Configuration
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile("ocelot.json", false, true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"ocelot.{builder.Environment.EnvironmentName}.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);
}

// Services
builder.Services.AddControllers();
builder.Services.AddOcelot(builder.Configuration);
builder.Services.AddJwtAuthenctication(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder =>
        builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

builder.Services.AddHealthChecks()
    .AddMongoDb(
        mongodbConnectionString: (
            builder.Configuration.GetSection("mongo")
                .Get<MongoOptions>() ?? throw new Exception("mongo configuration section not found")
        ).ConnectionString,
        name: "mongo",
        failureStatus: HealthStatus.Unhealthy
    );

builder.Services.AddHealthChecksUI().AddInMemoryStorage();

// App pipeline configuration
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseMiddleware<RequestResponseLogging>();
app.UseCors("CorsPolicy");
app.UseAuthentication();

app.UseHealthChecks("/healthz",
    new HealthCheckOptions {Predicate = _ => true, ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse});

app.UseHealthChecksUI();

var option = new RewriteOptions().AddRedirect("^$", "healthchecks-ui");
app.UseRewriter(option);

app.UseOcelot().Wait();  // Ensure this runs correctly as middleware

app.Run();