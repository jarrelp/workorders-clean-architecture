using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WorkOrders.Application.Validation;
using WorkOrders.Domain.Common;

namespace WorkOrders.Api.Endpoints;

/// <summary>Turns the application's <see cref="Result"/> into HTTP: the value, or an RFC 9457 problem.</summary>
internal static class ProblemResults
{
    public static Results<Ok<TValue>, ProblemHttpResult> ToOkOrProblem<TValue>(this Result<TValue> result) =>
        result.IsSuccess ? TypedResults.Ok(result.Value) : result.Error.ToProblem();

    /// <summary>
    /// Validation becomes 400, not found 404 and a conflict with the work order's state 409.
    /// The stable error code goes in a <c>code</c> member, so clients do not have to parse the text.
    /// </summary>
    public static ProblemHttpResult ToProblem(this Error error)
    {
        var problem = error is ValidationError validation
            ? new HttpValidationProblemDetails(CamelCaseFieldNames(validation.FieldErrors))
            : new ProblemDetails();

        problem.Status = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError,
        };
        problem.Detail = error.Description;
        problem.Extensions["code"] = error.Code;

        return TypedResults.Problem(problem);
    }

    // The application names fields after its C# properties; clients know them by their JSON names.
    private static Dictionary<string, string[]> CamelCaseFieldNames(IReadOnlyDictionary<string, string[]> fieldErrors) =>
        fieldErrors.ToDictionary(
            pair => JsonNamingPolicy.CamelCase.ConvertName(pair.Key),
            pair => pair.Value,
            StringComparer.Ordinal);
}
