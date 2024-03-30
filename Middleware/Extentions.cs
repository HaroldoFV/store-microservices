using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;

namespace Middleware;

public static class Extensions
{
    public static void AddMongoDb(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MongoOptions>(configuration.GetSection("mongo"));

        services.AddSingleton(c =>
        {
            var options = c.GetService<IOptions<MongoOptions>>();

            return new MongoClient(options.Value.ConnectionString);
        });

        services.AddSingleton(c =>
        {
            var options = c.GetService<IOptions<MongoOptions>>();
            var client = c.GetService<MongoClient>();

            return client.GetDatabase(options.Value.Database);
        });
    }

    public static void AddJwt(this IServiceCollection services, IConfiguration configuration)
    {
        var options = new JwtOptions();
        var section = configuration.GetSection("jwt");
        section.Bind(options);
        services.Configure<JwtOptions>(section);
        services.AddSingleton<IJwtBuilder, JwtBuilder>();

        services.AddAuthentication()
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;

                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Secret))
                };
            });
    }
}