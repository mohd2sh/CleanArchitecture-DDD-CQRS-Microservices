using CleanArchitecture.Core.Application.Abstractions.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CleanArchitecture.Core.Api.Configurations;

public class ResultResponseOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        AddResponseIfNotExists(operation, "400", "Bad Request - Validation or business rule failure", typeof(Result));
        AddResponseIfNotExists(operation, "404", "Not Found - Resource not found", typeof(Result));
        AddResponseIfNotExists(operation, "409", "Conflict - Resource conflict", typeof(Result));
        AddResponseIfNotExists(operation, "500", "Internal Server Error", typeof(ProblemDetails));
        AddResponseIfNotExists(operation, "401", "Unauthorized", typeof(ProblemDetails));
    }

    private static void AddResponseIfNotExists(OpenApiOperation operation, string statusCode,
        string description, Type responseType)
    {
        if (!operation.Responses.ContainsKey(statusCode))
        {
            IOpenApiSchema schema;

            if (responseType == typeof(ProblemDetails))
            {
                schema = new OpenApiSchema
                {
                    Type = JsonSchemaType.Object,
                    Properties = new Dictionary<string, IOpenApiSchema>
                    {
                        ["type"] = new OpenApiSchema { Type = JsonSchemaType.String },
                        ["title"] = new OpenApiSchema { Type = JsonSchemaType.String },
                        ["status"] = new OpenApiSchema { Type = JsonSchemaType.Integer, Format = "int32" },
                        ["detail"] = new OpenApiSchema { Type = JsonSchemaType.String },
                        ["instance"] = new OpenApiSchema { Type = JsonSchemaType.String }
                    }
                };
            }
            else
            {
                schema = new OpenApiSchemaReference(responseType.Name);
            }

            operation.Responses.Add(statusCode, new OpenApiResponse
            {
                Description = description,
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new OpenApiMediaType
                    {
                        Schema = schema
                    }
                }
            });
        }
    }
}
