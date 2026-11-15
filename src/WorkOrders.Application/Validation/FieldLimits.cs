namespace WorkOrders.Application.Validation;

/// <summary>Maximum text lengths, used by request validation and by the database schema.</summary>
public static class FieldLimits
{
    public const int CustomerName = 200;
    public const int SiteDescription = 500;
    public const int ProblemDescription = 2000;
    public const int Technician = 100;
    public const int MaterialDescription = 200;
}
