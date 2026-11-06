using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModularMonolith.Infrastructure.Correlation;
using ModularMonolith.SharedKernel.Exceptions;

namespace ModularMonolith.Infrastructure.Errors;

public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
        {
            httpContext.Response.StatusCode = 499;
            logger.LogDebug("The client closed the request before it completed.");
            return true;
        }

        var mapping = Map(exception);

        if (mapping.StatusCode >= 500)
        {
            logger.LogError(exception, "Unhandled exception for {Method} {Path}.",
                httpContext.Request.Method,
                httpContext.Request.Path);
        }
        else
        {
            logger.LogWarning(exception, "Request failed with {StatusCode} for {Method} {Path}.",
                mapping.StatusCode,
                httpContext.Request.Method,
                httpContext.Request.Path);
        }

        httpContext.Response.StatusCode = mapping.StatusCode;

        var problemDetails = new ProblemDetails
        {
            Status = mapping.StatusCode,
            Title = mapping.Title,
            Detail = mapping.Detail,
            Type = $"https://httpstatuses.com/{mapping.StatusCode}",
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["code"] = mapping.Code;
        problemDetails.Extensions["traceId"] =
            System.Diagnostics.Activity.Current?.TraceId.ToString() ?? httpContext.TraceIdentifier;
        problemDetails.Extensions["correlationId"] =
            httpContext.Items[CorrelationIdMiddleware.ItemKey] as string;

        if (mapping.Errors is { Count: > 0 })
        {
            problemDetails.Extensions["errors"] = mapping.Errors;
        }

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problemDetails
        });
    }

    private static ExceptionMapping Map(Exception exception)
    {
        return exception switch
        {
            AppException appException => new ExceptionMapping(
                appException.StatusCode,
                TitleFor(appException.StatusCode),
                appException.Message,
                appException.Code,
                appException.Errors.Count > 0 ? appException.Errors : null),

            ValidationException validationException => new ExceptionMapping(
                PlatformStatusCodes.BadRequest,
                "Validation failed",
                "One or more validation errors occurred.",
                "validation.failed",
                validationException.Errors
                    .GroupBy(error => error.PropertyName)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(error => error.ErrorMessage).ToArray())),

            BadHttpRequestException badRequest => new ExceptionMapping(
                badRequest.StatusCode,
                "Bad request",
                "The request could not be processed.",
                "request.invalid",
                null),

            JsonException => new ExceptionMapping(
                PlatformStatusCodes.BadRequest,
                "Invalid JSON payload",
                "The request body contains invalid JSON.",
                "request.invalid_json",
                null),

            DbUpdateConcurrencyException => new ExceptionMapping(
                PlatformStatusCodes.Conflict,
                "Concurrency conflict",
                "The resource was modified by another request. Reload it and retry.",
                "resource.concurrency_conflict",
                null),

            UnauthorizedAccessException => new ExceptionMapping(
                PlatformStatusCodes.Forbidden,
                "Forbidden",
                "You do not have permission to perform this action.",
                "auth.forbidden",
                null),

            KeyNotFoundException => new ExceptionMapping(
                PlatformStatusCodes.NotFound,
                "Not found",
                "The requested resource was not found.",
                "resource.not_found",
                null),

            _ => new ExceptionMapping(
                PlatformStatusCodes.InternalServerError,
                "An unexpected error occurred",
                "The server encountered an unexpected condition.",
                "server.error",
                null)
        };
    }

    private static string TitleFor(int statusCode)
    {
        return statusCode switch
        {
            PlatformStatusCodes.BadRequest => "Bad request",
            PlatformStatusCodes.Unauthorized => "Unauthorized",
            PlatformStatusCodes.Forbidden => "Forbidden",
            PlatformStatusCodes.NotFound => "Not found",
            PlatformStatusCodes.Conflict => "Conflict",
            PlatformStatusCodes.UnprocessableEntity => "Validation failed",
            PlatformStatusCodes.TooManyRequests => "Too many requests",
            PlatformStatusCodes.ServiceUnavailable => "Service unavailable",
            _ => "Request failed"
        };
    }

    private sealed record ExceptionMapping(
        int StatusCode,
        string Title,
        string Detail,
        string Code,
        IReadOnlyDictionary<string, string[]>? Errors);
}
