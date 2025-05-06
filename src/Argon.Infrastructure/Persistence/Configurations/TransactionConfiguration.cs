namespace Argon.Infrastructure.Persistence.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
  public void Configure(EntityTypeBuilder<Transaction> builder)
  {
    builder.HasOne(transaction => transaction.Counterparty)
      .WithMany(counterparties => counterparties.Transactions)
      .HasForeignKey(transaction => transaction.CounterpartyId);

    builder.HasOne(transaction => transaction.PotentialDuplicateOfTransaction)
      .WithMany(transaction => transaction.DuplicateTransactions)
      .HasForeignKey(transaction => transaction.PotentialDuplicateOfTransactionId);

    builder.HasOne(transaction => transaction.BankStatementFile)
      .WithMany(bankStatementFile => bankStatementFile.Transactions)
      .HasForeignKey(transaction => transaction.BankStatementFileId);

    // if db json support is needed for querying, this should do it:
    // builder.OwnsOne(transaction => transaction.RawImportData, action => action.ToJson());

    builder.Property(transaction => transaction.RawImportData)
      .HasColumnType("jsonb");
  }
}