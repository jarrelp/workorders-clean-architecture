using WorkOrders.Domain.Common;

namespace WorkOrders.Domain;

/// <summary>
/// A job at a customer's site, from the reported problem until the work is done or called off.
/// This is the aggregate root: time entries and material lines only change through its methods,
/// so every rule is checked in one place.
/// </summary>
public sealed class WorkOrder
{
    private readonly List<TimeEntry> _timeEntries = [];
    private readonly List<MaterialLine> _materialLines = [];

    private WorkOrder(
        Guid id,
        WorkOrderNumber number,
        string customerName,
        string siteDescription,
        string problemDescription,
        Money hourlyRate)
    {
        Id = id;
        Number = number;
        CustomerName = customerName;
        SiteDescription = siteDescription;
        ProblemDescription = problemDescription;
        HourlyRate = hourlyRate;
        Status = WorkOrderStatus.Draft;
    }

    public Guid Id { get; }

    public WorkOrderNumber Number { get; }

    public string CustomerName { get; }

    public string SiteDescription { get; }

    public string ProblemDescription { get; }

    /// <summary>The labour rate agreed with the customer for this work order.</summary>
    public Money HourlyRate { get; }

    public WorkOrderStatus Status { get; private set; }

    public DateOnly? ScheduledDate { get; private set; }

    public string? AssignedTechnician { get; private set; }

    public IReadOnlyList<TimeEntry> TimeEntries => _timeEntries.AsReadOnly();

    public IReadOnlyList<MaterialLine> MaterialLines => _materialLines.AsReadOnly();

    /// <summary>All logged minutes at the hourly rate, rounded once over the total to whole cents.</summary>
    public Money LabourTotal => HourlyRate.MultiplyBy(_timeEntries.Sum(entry => entry.Minutes) / 60m);

    public Money MaterialsTotal => _materialLines.Aggregate(Money.Zero, (total, line) => total + line.LineTotal);

    public Money Total => LabourTotal + MaterialsTotal;

    public static Result<WorkOrder> Create(
        WorkOrderNumber number,
        string customerName,
        string siteDescription,
        string problemDescription,
        Money hourlyRate)
    {
        ArgumentNullException.ThrowIfNull(number);

        if (string.IsNullOrWhiteSpace(customerName))
        {
            return WorkOrderErrors.CustomerNameRequired;
        }

        if (string.IsNullOrWhiteSpace(siteDescription))
        {
            return WorkOrderErrors.SiteDescriptionRequired;
        }

        if (string.IsNullOrWhiteSpace(problemDescription))
        {
            return WorkOrderErrors.ProblemDescriptionRequired;
        }

        return new WorkOrder(
            Guid.CreateVersion7(),
            number,
            customerName.Trim(),
            siteDescription.Trim(),
            problemDescription.Trim(),
            hourlyRate);
    }

    /// <summary>Plans the visit: a technician on a date that is today or later.</summary>
    /// <param name="technician">The technician who will do the work.</param>
    /// <param name="scheduledDate">The planned date of the visit.</param>
    /// <param name="today">The current date from the caller's clock, which keeps this rule testable.</param>
    public Result Schedule(string technician, DateOnly scheduledDate, DateOnly today)
    {
        var canMove = CheckTransitionTo(WorkOrderStatus.Scheduled);
        if (canMove.IsFailure)
        {
            return canMove;
        }

        if (string.IsNullOrWhiteSpace(technician))
        {
            return WorkOrderErrors.TechnicianRequired;
        }

        if (scheduledDate < today)
        {
            return WorkOrderErrors.ScheduledDateInPast(scheduledDate, today);
        }

        AssignedTechnician = technician.Trim();
        ScheduledDate = scheduledDate;
        Status = WorkOrderStatus.Scheduled;
        return Result.Success();
    }

    public Result Start() => TransitionTo(WorkOrderStatus.InProgress);

    public Result LogTime(string technician, DateOnly date, int minutes)
    {
        var inProgress = CheckInProgress();
        if (inProgress.IsFailure)
        {
            return inProgress;
        }

        var entry = TimeEntry.Create(technician, date, minutes);
        if (entry.IsFailure)
        {
            return entry.Error;
        }

        _timeEntries.Add(entry.Value);
        return Result.Success();
    }

    public Result AddMaterial(string description, decimal quantity, Money unitPrice)
    {
        var inProgress = CheckInProgress();
        if (inProgress.IsFailure)
        {
            return inProgress;
        }

        var line = MaterialLine.Create(description, quantity, unitPrice);
        if (line.IsFailure)
        {
            return line.Error;
        }

        _materialLines.Add(line.Value);
        return Result.Success();
    }

    /// <summary>Finishes the work. Requires at least one time entry, so there is labour to bill.</summary>
    public Result Complete()
    {
        var canMove = CheckTransitionTo(WorkOrderStatus.Completed);
        if (canMove.IsFailure)
        {
            return canMove;
        }

        if (_timeEntries.Count == 0)
        {
            return WorkOrderErrors.NoTimeLogged;
        }

        Status = WorkOrderStatus.Completed;
        return Result.Success();
    }

    public Result Cancel() => TransitionTo(WorkOrderStatus.Cancelled);

    private Result TransitionTo(WorkOrderStatus next)
    {
        var canMove = CheckTransitionTo(next);
        if (canMove.IsSuccess)
        {
            Status = next;
        }

        return canMove;
    }

    private Result CheckTransitionTo(WorkOrderStatus next)
    {
        if (Status.IsClosed())
        {
            return WorkOrderErrors.Closed(Status);
        }

        return Status.CanTransitionTo(next)
            ? Result.Success()
            : WorkOrderErrors.InvalidTransition(Status, next);
    }

    private Result CheckInProgress()
    {
        if (Status.IsClosed())
        {
            return WorkOrderErrors.Closed(Status);
        }

        return Status == WorkOrderStatus.InProgress
            ? Result.Success()
            : WorkOrderErrors.NotInProgress(Status);
    }
}
