using Api.Middleware;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Api.Swagger
{
    public class TenantHeaderOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Parameters ??= new List<OpenApiParameter>();

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = TenantResolutionMiddleware.HeaderName,
                In = ParameterLocation.Header,
                Required = true,
                Schema = new OpenApiSchema { Type = "string", Format = "uuid" },
                Description = "Tenant identifier. Seeded: 11111111-1111-1111-1111-111111111111 (Tenant A), 22222222-2222-2222-2222-222222222222 (Tenant B)."
            });
        }
    }
}
