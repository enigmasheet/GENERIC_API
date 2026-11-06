using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace ModularMonolith.Infrastructure.Api;

public sealed class ValidationFilter<TRequest>(IValidator<TRequest> validator) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var request = context.Arguments.OfType<TRequest>().FirstOrDefault();

        if (request is null)
        {
            return await next(context);
        }

        var validationResult = await validator.ValidateAsync(
            request,
            context.HttpContext.RequestAborted);

        if (validationResult.IsValid)
        {
            return await next(context);
        }

        var errors = validationResult.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.ErrorMessage).ToArray());

        return TypedResults.ValidationProblem(
            errors,
            title: "One or more validation errors occurred.");
    }
}

public static class ValidationFilterExtensions
{
    public static RouteHandlerBuilder WithValidation<TRequest>(this RouteHandlerBuilder builder)
    {
        return builder.AddEndpointFilter<ValidationFilter<TRequest>>();
    }
}
