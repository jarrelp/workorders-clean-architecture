namespace WorkOrders.Domain.Tests;

public class WorkOrderNumberTests
{
    [Theory]
    [InlineData(2026, 1, "WO-2026-0001")]
    [InlineData(2026, 123, "WO-2026-0123")]
    [InlineData(2027, 12345, "WO-2027-12345")]
    public void Create_CombinesYearAndZeroPaddedSequence(int year, int sequence, string expected)
    {
        Assert.Equal(expected, WorkOrderNumber.Create(year, sequence).Value);
    }

    [Theory]
    [InlineData(2026, 0)]
    [InlineData(999, 1)]
    public void Create_WithInvalidYearOrSequence_Throws(int year, int sequence)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => WorkOrderNumber.Create(year, sequence));
    }

    [Fact]
    public void Parse_RestoresAnEqualNumber()
    {
        Assert.Equal(WorkOrderNumber.Create(2026, 42), WorkOrderNumber.Parse("WO-2026-0042"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("WO-26-0001")]
    [InlineData("XX-2026-0001")]
    [InlineData("WO-2026-01")]
    public void Parse_WithInvalidFormat_Throws(string value)
    {
        Assert.Throws<FormatException>(() => WorkOrderNumber.Parse(value));
    }
}
