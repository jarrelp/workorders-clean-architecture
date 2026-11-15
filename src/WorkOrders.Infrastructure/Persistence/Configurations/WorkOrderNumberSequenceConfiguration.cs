using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WorkOrders.Infrastructure.Persistence.Configurations;

internal sealed class WorkOrderNumberSequenceConfiguration : IEntityTypeConfiguration<WorkOrderNumberSequence>
{
    public void Configure(EntityTypeBuilder<WorkOrderNumberSequence> builder)
    {
        builder.ToTable("WorkOrderNumberSequences");
        builder.HasKey(sequence => sequence.Year);
        builder.Property(sequence => sequence.Year).ValueGeneratedNever();
        builder.Property(sequence => sequence.LastValue).IsConcurrencyToken();
    }
}
