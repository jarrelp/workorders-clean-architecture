using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkOrders.Application.Validation;
using WorkOrders.Domain;

namespace WorkOrders.Infrastructure.Persistence.Configurations;

/// <summary>
/// Maps the aggregate without changing it for the database: EF Core uses the private constructor
/// and backing fields, so the domain keeps its get-only properties and read-only collections.
/// </summary>
internal sealed class WorkOrderConfiguration : IEntityTypeConfiguration<WorkOrder>
{
    public void Configure(EntityTypeBuilder<WorkOrder> builder)
    {
        builder.ToTable("WorkOrders");
        builder.HasKey(workOrder => workOrder.Id);
        builder.Property(workOrder => workOrder.Id).ValueGeneratedNever();

        builder.Property(workOrder => workOrder.Number)
            .HasConversion(number => number.Value, value => WorkOrderNumber.Parse(value))
            .HasMaxLength(20);
        builder.HasIndex(workOrder => workOrder.Number).IsUnique();

        builder.Property(workOrder => workOrder.CustomerName).HasMaxLength(FieldLimits.CustomerName);
        builder.Property(workOrder => workOrder.SiteDescription).HasMaxLength(FieldLimits.SiteDescription);
        builder.Property(workOrder => workOrder.ProblemDescription).HasMaxLength(FieldLimits.ProblemDescription);
        builder.Property(workOrder => workOrder.HourlyRate);
        builder.Property(workOrder => workOrder.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(workOrder => workOrder.ScheduledDate);
        builder.Property(workOrder => workOrder.AssignedTechnician).HasMaxLength(FieldLimits.Technician);
        builder.HasIndex(workOrder => workOrder.Status);

        builder.OwnsMany(workOrder => workOrder.TimeEntries, timeEntry =>
        {
            timeEntry.ToTable("TimeEntries");
            timeEntry.WithOwner().HasForeignKey("WorkOrderId");
            timeEntry.Property<int>("Id");
            timeEntry.HasKey("Id");
            timeEntry.Property(entry => entry.Technician).HasMaxLength(FieldLimits.Technician);
            timeEntry.Property(entry => entry.Date);
            timeEntry.Property(entry => entry.Minutes);
        });

        builder.OwnsMany(workOrder => workOrder.MaterialLines, materialLine =>
        {
            materialLine.ToTable("MaterialLines");
            materialLine.WithOwner().HasForeignKey("WorkOrderId");
            materialLine.Property<int>("Id");
            materialLine.HasKey("Id");
            materialLine.Property(line => line.Description).HasMaxLength(FieldLimits.MaterialDescription);
            materialLine.Property(line => line.Quantity).HasPrecision(18, 3);
            materialLine.Property(line => line.UnitPrice);
        });

        // The public collections are read-only views; EF Core fills the private lists behind them.
        builder.Navigation(workOrder => workOrder.TimeEntries).HasField("_timeEntries");
        builder.Navigation(workOrder => workOrder.MaterialLines).HasField("_materialLines");
    }
}
