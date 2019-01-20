using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace Common.Utilities.Handlers
{
    public class GlobalExceptionHandler
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandler> _logger;
        public GlobalExceptionHandler(RequestDelegate next, ILogger<GlobalExceptionHandler> logger)
        {
            _next = next;
            _logger = logger;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }

        }

        public async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            if (context != null && exception != null)
            {
                var code = HttpStatusCode.NotFound;
                //  if you want to support different exception types you can test the exception type
                //  here and create custom error numbers and messages (including globalized messages).
                //  The default implementation just use the raw exception message
                if (exception.GetType() == typeof(UnauthorizedAccessException))
                    code = HttpStatusCode.Unauthorized;
                else
                    code = HttpStatusCode.InternalServerError; // 500 if unexpected
                var result = JsonConvert.SerializeObject(new { exception.Message });
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)code;
                await context.Response.WriteAsync(result);
                _logger.LogError(result + ": Status Code: " + code, MethodBase.GetCurrentMethod(), exception);
            }
            else
            {
                _logger.LogError("Context and exception message is empty. Status Code: " + HttpStatusCode.ExpectationFailed, MethodBase.GetCurrentMethod());
            }

        }
    }
}
