using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Middleware;

public class RequestResponseLogging(RequestDelegate next, ILogger<RequestResponseLogging> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        context.Request.EnableBuffering();
        var builder = new StringBuilder();
        var request = await FormatRequest(context.Request);
        builder.Append("Request: ").AppendLine(request);
        builder.AppendLine("Request headers:");

        foreach (var header in context.Request.Headers)
        {
            builder.Append(header.Key).Append(": ").AppendLine(header.Value);
        }

        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;
        await next(context);
        
        var response = await FormatRe
    }

    private static async Task<string> FormatRequest(HttpRequest request)
    {
        using var reader = new StreamReader(
            request.Body,
            encoding: Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            leaveOpen: true);
        var body = await reader.ReadToEndAsync();

        var formattedRequest =
            $"{request.Method} {request.Scheme}://{request.Host}{request.Path}{request.QueryString} {body}";
        request.Body.Position = 0;
        return formattedRequest;
    }
}