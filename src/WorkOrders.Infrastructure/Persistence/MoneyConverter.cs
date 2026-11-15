using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WorkOrders.Domain.Common;

namespace WorkOrders.Infrastructure.Persistence;

/// <summary>Stores <see cref="Money"/> as a plain decimal amount in euros.</summary>
internal sealed class MoneyConverter() : ValueConverter<Money, decimal>(
    money => money.Amount,
    amount => Money.FromEuros(amount).Value);
