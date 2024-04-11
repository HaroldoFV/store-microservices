using Microsoft.AspNetCore.Http;
using MongoDB.Bson;

namespace Middleware;

public class JwtMiddleware(IJwtBuilder jwtBuilder) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var bearer = context.Request.Headers["Authorization"].ToString();
        var token = bearer.Replace("Bearer ", string.Empty);

        if (!string.IsNullOrEmpty(token))
        {
            var userId = jwtBuilder.ValidateToken(token);

            if (ObjectId.TryParse(userId, out _))
            {
                context.Items["userId"] = userId;
            }
            else
            {
                context.Response.StatusCode = 401;
            }
        }

        await next(context);
    }
}