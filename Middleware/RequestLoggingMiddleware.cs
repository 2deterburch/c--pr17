using System.Diagnostics;

namespace lab11.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            Console.WriteLine($"Request: {context.Request.Method} {context.Request.Path}");

            await _next(context);

            stopwatch.Stop();

            Console.WriteLine($"Response: {context.Response.StatusCode}");
            Console.WriteLine($"Time: {stopwatch.ElapsedMilliseconds} ms");
        }
    }
}