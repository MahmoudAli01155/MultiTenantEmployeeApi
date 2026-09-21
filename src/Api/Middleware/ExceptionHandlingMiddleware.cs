using Application.Common.Exceptions;
using Application.Common.Models;
using FluentValidation;
namespace Api.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            catch (ValidationException ex)
            {
                var details = ex.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                await Write(context, StatusCodes.Status400BadRequest,
                    ApiResponse<object>.Fail("ValidationError", "One or more validation errors occurred.", details));
            }
            catch (NotFoundException ex)
            {
                await Write(context, StatusCodes.Status404NotFound,
                    ApiResponse<object>.Fail("NotFound", ex.Message));
            }
            catch (ConflictException ex)
            {
                await Write(context, StatusCodes.Status409Conflict,
                    ApiResponse<object>.Fail("Conflict", ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");
                await Write(context, StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail("InternalError", "An unexpected error occurred."));
            }
        }

        private static Task Write(HttpContext context, int statusCode, ApiResponse<object> body)
        {
            context.Response.StatusCode = statusCode;
            return context.Response.WriteAsJsonAsync(body);
        }
    }
}
