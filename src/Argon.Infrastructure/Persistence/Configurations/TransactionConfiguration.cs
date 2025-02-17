namespace Argon.Infrastructure.Persistence.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
  public void Configure(EntityTypeBuilder<Transaction> builder)
  {
    builder.HasOne(transaction => transaction.Counterparty)
      .WithMany(counterparties => counterparties.Transactions)
      .HasForeignKey(transaction => transaction.CounterpartyId);
  }
}