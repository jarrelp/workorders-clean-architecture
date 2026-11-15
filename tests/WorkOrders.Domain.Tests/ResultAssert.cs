using WorkOrders.Domain.Common;

namespace WorkOrders.Domain.Tests;

internal static class ResultAssert
{
    public static void Succeeded(Result result) =>
        Assert.True(
            result.IsSuccess,
            result.IsFailure ? $"Expected success, got {result.Error.Code}: {result.Error.Description}" : null);

    public static void Failed(Error expected, Result result)
    {
        Assert.True(result.IsFailure, "Expected a failure, but the result was a success.");
        Assert.Equal(expected, result.Error);
    }
}
