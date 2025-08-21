using BookIt.API.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace BookIt.API.Validation;

public static class InvalidModelBehaviorConfigurator
{
    public static IServiceCollection ConfigureInvalidModelBehavior(this IServiceCollection services)
    {
        return services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToList() ?? []
                    );

                var errorResponse = new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Error = "Model Validation Failed",
                    Message = "One or more model validation errors occurred",
                    ErrorCode = "MODEL_VALIDATION_ERROR",
                    Details = new Dictionary<string, object> { { "modelValidationErrors", errors } }
                };

                return new BadRequestObjectResult(errorResponse);
            };
        });
    }
}
