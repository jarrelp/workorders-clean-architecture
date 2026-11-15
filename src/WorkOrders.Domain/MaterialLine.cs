using WorkOrders.Domain.Common;

namespace WorkOrders.Domain;

/// <summary>Material used on a work order. Only created through <see cref="WorkOrder.AddMaterial"/>.</summary>
public sealed class MaterialLine
{
    private MaterialLine(string description, decimal quantity, Money unitPrice)
    {
        Description = description;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public string Description { get; }

    /// <summary>A decimal quantity, so materials sold by the metre or litre fit as well.</summary>
    public decimal Quantity { get; }

    public Money UnitPrice { get; }

    /// <summary>Unit price times quantity, rounded to whole cents like a line on an invoice.</summary>
    public Money LineTotal => UnitPrice.MultiplyBy(Quantity);

    internal static Result<MaterialLine> Create(string description, decimal quantity, Money unitPrice)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return WorkOrderErrors.MaterialDescriptionRequired;
        }

        if (quantity <= 0)
        {
            return WorkOrderErrors.QuantityMustBePositive;
        }

        return new MaterialLine(description.Trim(), quantity, unitPrice);
    }
}
