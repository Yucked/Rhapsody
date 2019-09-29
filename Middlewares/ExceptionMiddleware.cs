using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Concept.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, ILogger<ExceptionMiddleware> logger)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                HandleException(ex, logger, context);
            }
        }

        public void HandleException(Exception ex, ILogger<ExceptionMiddleware> logger, HttpContext context)
        {
            logger.Log(LogLevel.Error, "Exception throwed", ex);
            context.Response.StatusCode = 500;
        }
    }
}