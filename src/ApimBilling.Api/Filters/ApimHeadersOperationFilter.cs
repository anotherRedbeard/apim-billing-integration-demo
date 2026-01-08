using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ApimBilling.Api.Filters;

public class ApimHeadersOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= new List<IOpenApiParameter>();

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-APIM-ServiceName",
            In = ParameterLocation.Header,
            Description = "Azure APIM service name (required)",
            Required = true,
            Schema = new OpenApiSchema
            {
                Type = JsonSchemaType.String
            }
        });

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-APIM-ResourceGroup",
            In = ParameterLocation.Header,
            Description = "Azure resource group name (required)",
            Required = true,
            Schema = new OpenApiSchema
            {
                Type = JsonSchemaType.String
            }
        });
    }
}
