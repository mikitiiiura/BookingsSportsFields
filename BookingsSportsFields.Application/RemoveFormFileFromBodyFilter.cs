using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BookingsSportsFields.Application;

public class RemoveFormFileFromBodyFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var formFileParams = context.MethodInfo.GetParameters()
            .Where(p => p.ParameterType == typeof(IFormFile) || p.ParameterType == typeof(IFormFileCollection))
            .ToList();

        if (!formFileParams.Any()) return;

        // Видаляємо параметри IFormFile з body (бо вони йдуть у form-data)
        foreach (var param in formFileParams)
        {
            var paramName = param.Name;
            var bodyParam = operation.Parameters?.FirstOrDefault(p => p.Name == paramName);
            if (bodyParam != null)
            {
                operation.Parameters.Remove(bodyParam);
            }
        }

        // Якщо є [FromForm] DTO — залишаємо його як body
        var formDtoParam = operation.Parameters?.FirstOrDefault(p => p.In == ParameterLocation.Query || p.In == ParameterLocation.Header);
        if (formDtoParam != null)
        {
            operation.RequestBody = new OpenApiRequestBody
            {
                Content = {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = formDtoParam.Schema?.Properties ?? new Dictionary<string, OpenApiSchema>()
                        }
                    }
                }
            };
        }
    }
}