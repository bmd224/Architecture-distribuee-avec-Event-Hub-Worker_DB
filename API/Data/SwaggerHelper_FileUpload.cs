using Microsoft.OpenApi.Models;
using MVC.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

public class FileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var fileParams = context.MethodInfo.GetParameters()
            .Where(p => p.ParameterType == typeof(IFormFile) || p.ParameterType == typeof(PostCreateDTO))
            .ToArray();

        if (fileParams.Any())
        {
            operation.Parameters.Clear();
            foreach (var param in fileParams)
            {
                var schema = context.SchemaGenerator.GenerateSchema(param.ParameterType, context.SchemaRepository);
                operation.RequestBody = new OpenApiRequestBody
                {
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["multipart/form-data"] = new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Type = "object",
                                Properties = new Dictionary<string, OpenApiSchema>
                                {
                                    ["title"] = new OpenApiSchema { Type = "string" },
                                    ["category"] = new OpenApiSchema { Type = "integer", Format = "int32" },
                                    ["user"] = new OpenApiSchema { Type = "string" },
                                    ["image"] = new OpenApiSchema { Type = "string", Format = "binary" }
                                },
                                Required = new HashSet<string> { "title", "category", "user", "image" }
                            }
                        }
                    }
                };
            }
        }
    }
}
