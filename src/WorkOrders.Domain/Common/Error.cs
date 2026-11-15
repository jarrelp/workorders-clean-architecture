using System.Diagnostics.CodeAnalysis;

namespace WorkOrders.Domain.Common;

/// <summary>
/// An expected failure the caller can act on, such as invalid input or an action that is not
/// allowed in the current state. Bugs and infrastructure failures are still exceptions.
/// </summary>
/// <param name="Code">Stable, machine-readable identifier, for example <c>WorkOrder.NotInProgress</c>.</param>
/// <param name="Description">Explanation for a human reader.</param>
/// <param name="Type">The category, which decides how the error is reported.</param>
[SuppressMessage(
    "Naming",
    "CA1716:Identifiers should not match keywords",
    Justification = "Error is only a keyword in Visual Basic; this code base is C#.")]
public record Error(string Code, string Description, ErrorType Type)
{
    public static Error Validation(string code, string description) => new(code, description, ErrorType.Validation);

    public static Error NotFound(string code, string description) => new(code, description, ErrorType.NotFound);

    public static Error Conflict(string code, string description) => new(code, description, ErrorType.Conflict);
}
