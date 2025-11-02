using System.Diagnostics.CodeAnalysis;

using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace PaymentGateway.Api.Filters;

/// <summary>
/// This filter validates that, when a request body is expected, it is present.
/// </summary>
/// <remarks>This is built-in if <see cref="ApiBehaviorOptions.SuppressModelStateInvalidFilter"/> is not
/// set to <c>true</c>, as it is in our case. Because... FluentValidation.</remarks>
[ExcludeFromCodeCoverage]
public class RequireBodyFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid && !context.HttpContext.Features
                .GetRequiredFeature<IHttpRequestBodyDetectionFeature>().CanHaveBody)
        {
            var pdf = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
            context.Result =
                new BadRequestObjectResult(pdf.CreateValidationProblemDetails(context.HttpContext, context.ModelState));
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}