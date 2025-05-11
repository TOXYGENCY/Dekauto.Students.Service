using Dekauto.Students.Service.Students.Service.Domain.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using System.Threading.Tasks;

namespace Dekauto.Students.Service.Students.Service.Infrastructure
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class MetricsMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IRequestMetricsService requestMetricsService;

        public MetricsMiddleware(RequestDelegate next, IRequestMetricsService requestMetricsService)
        {
            this.next = next;
            this.requestMetricsService = requestMetricsService;
        }

        public Task Invoke(HttpContext httpContext)
        {
            requestMetricsService.Increment();
            return next(httpContext);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class MetricsMiddlewareExtensions
    {
        public static IApplicationBuilder UseMetricsMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<MetricsMiddleware>();
        }
    }
}
