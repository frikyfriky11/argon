namespace Argon.Infrastructure.Persistence.Configurations;

public class CounterpartiesConfiguration : IEntityTypeConfiguration<Counterparty>
{
  public void Configure(EntityTypeBuilder<Counterparty> builder)
  {
    builder.Property(counterparty => counterparty.Name)
      .HasMaxLength(100)
      .IsRequired();
  }
}
