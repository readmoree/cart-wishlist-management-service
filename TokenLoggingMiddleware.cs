// TokenLoggingMiddleware.cs
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;
public class TokenLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TokenLoggingMiddleware> _logger;

        public TokenLoggingMiddleware(RequestDelegate next, ILogger<TokenLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var token = context.Request.Headers["Authorization"].ToString();

            // Log the token for debugging purposes
            if (!string.IsNullOrEmpty(token))
            {
                _logger.LogInformation($"Received Token: {token}");
            }
            else
            {
                _logger.LogWarning("Authorization header is missing or empty.");
            }

            await _next(context);
        }
    }
