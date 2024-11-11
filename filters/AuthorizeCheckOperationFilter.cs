using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public class AuthorizeCheckOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Check if the action has the Authorize attribute
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        var hasAuthorize = context.MethodInfo.DeclaringType.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any() ||
                           context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any();
#pragma warning restore CS8602 // Dereference of a possibly null reference.

        if (hasAuthorize)
        {
            operation.Security = new List<OpenApiSecurityRequirement>
            {
                new OpenApiSecurityRequirement
                {
                    { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, new string[] { } }
                }
            };

            // Add parameter for Authorization header to indicate the expected input
            // Enables to take token only without this have to write Bearer each time before writing token in swagger ui authorize
            // Adds authorization parameter for each authorize api accessing it
            // operation.Parameters.Add(new OpenApiParameter
            // {
            //     Name = "Authorization",
            //     In = ParameterLocation.Header,
            //     Required = true,
            //     Description = "Enter your token without 'Bearer' prefix",
            //     Schema = new OpenApiSchema { Type = "string" }
            // });
        }
    }
}
