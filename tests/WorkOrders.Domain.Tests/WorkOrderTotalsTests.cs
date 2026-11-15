using static WorkOrders.Domain.Tests.TestWorkOrders;

namespace WorkOrders.Domain.Tests;

public class WorkOrderTotalsTests
{
    [Fact]
    public void Total_IsLabourAtTheHourlyRatePlusMaterials()
    {
        var workOrder = InProgress(hourlyRate: 65m);
        ResultAssert.Succeeded(workOrder.LogTime("Alex Example", Today, 90));
        ResultAssert.Succeeded(workOrder.LogTime("Sam Sample", Today, 45));
        ResultAssert.Succeeded(workOrder.AddMaterial("Pressure relief valve 3 bar", 1m, Euros(24.95m)));
        ResultAssert.Succeeded(workOrder.AddMaterial("Copper pipe 15 mm, per metre", 2.5m, Euros(6.40m)));

        Assert.Equal(Euros(146.25m), workOrder.LabourTotal); // 135 minutes at 65.00 per hour
        Assert.Equal(Euros(40.95m), workOrder.MaterialsTotal); // 24.95 + 2.5 x 6.40
        Assert.Equal(Euros(187.20m), workOrder.Total);
    }

    [Fact]
    public void LabourTotal_IsRoundedOnceOverAllMinutes()
    {
        // Three entries of 20 minutes at 65.00 per hour are exactly one hour.
        // Rounding each entry on its own would give 3 x 21.67 = 65.01.
        var workOrder = InProgress(hourlyRate: 65m);
        for (var visit = 0; visit < 3; visit++)
        {
            ResultAssert.Succeeded(workOrder.LogTime(Technician, Today, 20));
        }

        Assert.Equal(Euros(65.00m), workOrder.LabourTotal);
    }

    [Fact]
    public void LineTotal_IsRoundedToWholeCents()
    {
        var workOrder = InProgress();

        ResultAssert.Succeeded(workOrder.AddMaterial("Installation cable, per metre", 0.333m, Euros(1.99m)));

        Assert.Equal(Euros(0.66m), Assert.Single(workOrder.MaterialLines).LineTotal); // 0.66267 rounded
    }
}
