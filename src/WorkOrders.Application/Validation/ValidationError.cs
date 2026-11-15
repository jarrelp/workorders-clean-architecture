using WorkOrders.Domain.Common;

namespace WorkOrders.Application.Validation;

/// <summary>A request with invalid fields. Lists every invalid field, not only the first one.</summary>
/// <param name="FieldErrors">Messages per field, keyed by the request property name.</param>
public sealed record ValidationError(IReadOnlyDictionary<string, string[]> FieldErrors)
    : Error("Request.Invalid", "One or more fields are invalid.", ErrorType.Validation);
