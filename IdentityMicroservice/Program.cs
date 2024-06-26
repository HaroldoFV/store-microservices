using HealthChecks.UI.Client;
using System.Configuration;
using IdentityMicroservice.Repository;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using Middleware;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddMongoDb(builder.Configuration);
builder.Services.AddJwt(builder.Configuration);
builder.Services.AddTransient<IEncryptor, Encryptor>();
builder.Services.AddSingleton<IUserRepository>(sp =>
    new UserRepository(sp.GetService<IMongoDatabase>() ??
                       throw new Exception(
                           "IMongoDatabase not found"))
);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo {Title = "User", Version = "v1"}); });

builder.Services.AddHealthChecks()
    .AddMongoDb(
        mongodbConnectionString: (
            builder.Configuration.GetSection("mongo").Get<MongoOptions>()
            ?? throw new Exception("mongo configuration section not found")
        ).ConnectionString,
        name: "mongo",
        failureStatus: HealthStatus.Unhealthy
    );
builder.Services.AddHealthChecksUI().AddInMemoryStorage();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger();

app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity V1"); });

var option = new RewriteOptions();
option.AddRedirect("^$", "swagger");
app.UseRewriter(option);

app.UseHealthChecks("/healthz",
    new HealthCheckOptions {Predicate = _ => true, ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse});

app.UseHealthChecksUI();

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();