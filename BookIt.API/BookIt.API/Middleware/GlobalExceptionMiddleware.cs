using BookIt.API.Models.Responses;
using BookIt.BLL.Exceptions;
using System.Net;
using System.Text.Json;

namespace BookIt.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var errorResponse = CreateErrorResponse(exception);
        response.StatusCode = errorResponse.StatusCode;

        var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await response.WriteAsync(jsonResponse);
    }

    private static ErrorResponse CreateErrorResponse(Exception exception)
    {
        return exception switch
        {
            EntityNotFoundException ex => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.NotFound,
                Error = "Not Found",
                Message = ex.Message,
                ErrorCode = ex.ErrorCode,
                Details = ex.Properties
            },

            EntityAlreadyExistsException ex => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.Conflict,
                Error = "Conflict",
                Message = ex.Message,
                ErrorCode = ex.ErrorCode,
                Details = ex.Properties
            },

            ValidationException ex => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Error = "Business Validation Failed",
                Message = ex.Message,
                ErrorCode = ex.ErrorCode,
                Details = new Dictionary<string, object> { { "businessValidationErrors", ex.Errors } }
            },

            BookingConflictException ex => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.Conflict,
                Error = "Booking Conflict",
                Message = ex.Message,
                ErrorCode = ex.ErrorCode,
                Details = ex.Properties
            },

            BusinessRuleViolationException ex => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Error = "Business Rule Violation",
                Message = ex.Message,
                ErrorCode = ex.ErrorCode,
                Details = ex.Properties
            },

            UnauthorizedOperationException ex => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.Forbidden,
                Error = "Forbidden",
                Message = ex.Message,
                ErrorCode = ex.ErrorCode,
                Details = ex.Properties
            },

            ExternalServiceException ex => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.BadGateway,
                Error = "External Service Error",
                Message = ex.Message,
                ErrorCode = ex.ErrorCode,
                Details = ex.Properties
            },

            UnauthorizedAccessException => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.Unauthorized,
                Error = "Unauthorized",
                Message = "Authentication is required to access this resource",
                ErrorCode = "UNAUTHORIZED"
            },

            ArgumentException ex => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Error = "Bad Request",
                Message = ex.Message,
                ErrorCode = "INVALID_ARGUMENT"
            },

            InvalidOperationException ex => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Error = "Invalid Operation",
                Message = ex.Message,
                ErrorCode = "INVALID_OPERATION"
            },

            _ => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Error = "Internal Server Error",
                Message = "An unexpected error occurred. Please try again later.",
                ErrorCode = "INTERNAL_ERROR"
            }
        };
    }
}