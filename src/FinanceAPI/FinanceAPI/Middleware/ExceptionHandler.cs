using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace FinanceAPI.Middleware
{
    public class ExceptionHandler
    {
        private readonly RequestDelegate _next;

        public ExceptionHandler(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IWebHostEnvironment env)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, env, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, IWebHostEnvironment env, Exception exception)
        {
            string result;
            var code = HttpStatusCode.InternalServerError;

            if (env.IsDevelopment())
            {
                var errorMessage = new
                {
                    error = exception.Message,
                    stack = exception.StackTrace,
                    innerException = exception.InnerException
                };

                result = JsonConvert.SerializeObject(errorMessage);
            }
            else
            {
                var errorMessage = new
                {
                    error = exception.Message
                };

                result = JsonConvert.SerializeObject(errorMessage);
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int) code;
            return context.Response.WriteAsync(result);
        }
    }

    public static class ExceptionHandlerMiddleware
    {
        public static IApplicationBuilder UseExceptionHandlingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandler>();
        }
    }
}