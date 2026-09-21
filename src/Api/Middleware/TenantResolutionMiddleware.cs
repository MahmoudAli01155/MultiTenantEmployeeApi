using Application.Common.Interfaces;
using Application.Common.Models;
using Infrastructure.Multitenancy;
using Microsoft.EntityFrameworkCore;

namespace Api.Middleware
{
    public class TenantResolutionMiddleware
    {
        public const string HeaderName = "X-Tenant-Id";

        private readonly RequestDelegate _next;

        public TenantResolutionMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context, TenantContext tenantContext, IApplicationDbContext db)
        {
            if (!context.Request.Path.StartsWithSegments("/api"))
            {
                await _next(context);
                return;
            }

            if (!context.Request.Headers.TryGetValue(HeaderName, out var rawValue)
                || !Guid.TryParse(rawValue.ToString(), out var tenantId))
            {
                await WriteError(context, StatusCodes.Status400BadRequest, "InvalidTenantHeader",
                    $"The {HeaderName} header is required and must be a valid GUID.");
                return;
            }

            var tenantExists = await db.Tenants.AnyAsync(t => t.Id == tenantId, context.RequestAborted);
            if (!tenantExists)
            {
                await WriteError(context, StatusCodes.Status404NotFound, "TenantNotFound",
                    "The specified tenant does not exist.");
                return;
            }

            tenantContext.SetTenant(tenantId);
            await _next(context);
        }

        private static Task WriteError(HttpContext context, int statusCode, string code, string message)
        {
            context.Response.StatusCode = statusCode;
            return context.Response.WriteAsJsonAsync(ApiResponse<object>.Fail(code, message));
        }
    }
}
