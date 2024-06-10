using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WhatToCook.Application.Exceptions;

namespace WhatToCook.WebAPI;

public class GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger) : IExceptionFilter
{
    private readonly ILogger<GlobalExceptionFilter> _logger = logger;

    public void OnException(ExceptionContext context)
    {
        var exception = context.Exception;

        _logger.LogError(exception, "An unhandled exception occurred.");

        var errorDetails = new ErrorDetails
        {
            StatusCode = GetStatusCode(exception),
            Message = GetUserFriendlyMessage(exception),
        };

        context.Result = new ObjectResult(errorDetails) { StatusCode = errorDetails.StatusCode };

        context.ExceptionHandled = true;
    }

    private static int GetStatusCode(Exception exception)
    {
        return exception switch
        {
            ArgumentNullException _ => 400,
            UnauthorizedAccessException _ => 401,
            InvalidOperationException _ => 403,
            NotFoundException _ => 404,
            NotImplementedException _ => 501,
            _ => 500,
        };
    }

    private static string GetUserFriendlyMessage(Exception exception)
    {
        return !string.IsNullOrEmpty(exception.Message)
            ? exception.Message
            : exception switch
            {
                ArgumentNullException _ => "Bad request. Please check your input and try again.",
                UnauthorizedAccessException _ =>
                    "Unauthorized access. Please log in and try again.",
                InvalidOperationException _ =>
                    "Forbidden. You do not have permission to perform this action.",
                NotFoundException _ => "Not found. The requested resource could not be found.",
                NotImplementedException _ =>
                    "Not implemented. The requested functionality is not available.",
                _ => "Internal server error. Please try again later.",
            };
    }
}

public class ErrorDetails
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = "";
}
