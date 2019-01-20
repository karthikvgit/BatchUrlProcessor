using Common.Utilities.Handlers;
using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Utilities.Extensions
{
    public static class ExceptionHandlerExtension
    {
        public static IApplicationBuilder UseCustomExceptionHandlerMiddleware
                                 (this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GlobalExceptionHandler>();
        }
    }
}
