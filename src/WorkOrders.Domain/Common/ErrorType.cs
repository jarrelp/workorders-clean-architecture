namespace WorkOrders.Domain.Common;

/// <summary>The category of an <see cref="Error"/>. The API maps it to an HTTP status code.</summary>
public enum ErrorType
{
    /// <summary>The input breaks a rule. The caller can correct it and try again.</summary>
    Validation,

    /// <summary>The requested item does not exist.</summary>
    NotFound,

    /// <summary>The request is valid, but not allowed in the current state.</summary>
    Conflict,
}
