namespace Argon.Infrastructure.Persistence.Configurations;

public class CounterpartyIdentifierConfiguration : IEntityTypeConfiguration<CounterpartyIdentifier>
{
  public void Configure(EntityTypeBuilder<CounterpartyIdentifier> builder)
  {
    builder.Property(counterpartyIdentifier => counterpartyIdentifier.IdentifierText)
      .HasMaxLength(250)
      .IsRequired();
    
    builder.HasOne(counterpartyIdentifier => counterpartyIdentifier.Counterparty)
      .WithMany(counterparty => counterparty.Identifiers)
      .HasForeignKey(counterpartyIdentifier => counterpartyIdentifier.CounterpartyId);
  }
}