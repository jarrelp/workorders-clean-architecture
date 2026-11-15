using WorkOrders.Domain.Common;

namespace WorkOrders.Application.Validation;

/// <summary>
/// Collects field errors for one request, so the client sees all of them at once.
/// The domain model still enforces its own rules; this only checks the shape of the input.
/// </summary>
internal sealed class RequestValidator
{
    private readonly Dictionary<string, List<string>> _errors = new(StringComparer.Ordinal);

    public RequestValidator RequireText(string field, string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return AddError(field, "Must not be empty.");
        }

        return value.Trim().Length > maxLength
            ? AddError(field, $"Must be at most {maxLength} characters.")
            : this;
    }

    public RequestValidator RequireGreaterThanZero(string field, decimal value) =>
        value > 0 ? this : AddError(field, "Must be greater than zero.");

    public RequestValidator RequireNotNegative(string field, decimal value) =>
        value >= 0 ? this : AddError(field, "Must not be negative.");

    public Result ToResult() =>
        _errors.Count == 0
            ? Result.Success()
            : new ValidationError(_errors.ToDictionary(pair => pair.Key, pair => pair.Value.ToArray(), StringComparer.Ordinal));

    private RequestValidator AddError(string field, string message)
    {
        if (!_errors.TryGetValue(field, out var messages))
        {
            messages = [];
            _errors.Add(field, messages);
        }

        messages.Add(message);
        return this;
    }
}
