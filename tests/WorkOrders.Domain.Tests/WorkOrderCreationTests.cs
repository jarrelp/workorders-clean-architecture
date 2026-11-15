using WorkOrders.Domain.Common;
using static WorkOrders.Domain.Tests.TestWorkOrders;

namespace WorkOrders.Domain.Tests;

public class WorkOrderCreationTests
{
    public static TheoryData<string, string, string, string> MissingText => new()
    {
        { " ", "Shop floor", "Leaking tap", "WorkOrder.CustomerNameRequired" },
        { "Example Bakery", "", "Leaking tap", "WorkOrder.SiteDescriptionRequired" },
        { "Example Bakery", "Shop floor", "   ", "WorkOrder.ProblemDescriptionRequired" },
    };

    [Fact]
    public void Create_StartsAsDraftWithoutPlanningTimeOrMaterials()
    {
        var workOrder = Draft();

        Assert.Equal(WorkOrderStatus.Draft, workOrder.Status);
        Assert.Equal("WO-2026-0001", workOrder.Number.Value);
        Assert.Null(workOrder.ScheduledDate);
        Assert.Null(workOrder.AssignedTechnician);
        Assert.Empty(workOrder.TimeEntries);
        Assert.Empty(workOrder.MaterialLines);
        Assert.Equal(Money.Zero, workOrder.Total);
    }

    [Theory]
    [MemberData(nameof(MissingText))]
    public void Create_WithoutRequiredText_Fails(string customerName, string siteDescription, string problemDescription, string expectedCode)
    {
        var result = WorkOrder.Create(WorkOrderNumber.Create(2026, 1), customerName, siteDescription, problemDescription, Euros(65m));

        Assert.True(result.IsFailure);
        Assert.Equal(expectedCode, result.Error.Code);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public void Create_TrimsSurroundingWhitespace()
    {
        var workOrder = WorkOrder.Create(
            WorkOrderNumber.Create(2026, 7),
            "  Example Bakery ",
            " Shop floor ",
            " Leaking tap ",
            Euros(65m)).Value;

        Assert.Equal("Example Bakery", workOrder.CustomerName);
        Assert.Equal("Shop floor", workOrder.SiteDescription);
        Assert.Equal("Leaking tap", workOrder.ProblemDescription);
    }
}
