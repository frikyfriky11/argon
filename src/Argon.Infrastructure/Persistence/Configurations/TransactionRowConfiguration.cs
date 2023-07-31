namespace Argon.Infrastructure.Persistence.Configurations;

public class TransactionRowConfiguration : IEntityTypeConfiguration<TransactionRow>
{
  public void Configure(EntityTypeBuilder<TransactionRow> builder)
  {
    builder.HasOne(row => row.Transaction)
      .WithMany(transaction => transaction.TransactionRows)
      .HasForeignKey(row => row.TransactionId);

    builder.HasOne(row => row.Account)
      .WithMany(account => account.TransactionRows)
      .HasForeignKey(row => row.AccountId)
      .OnDelete(DeleteBehavior.Restrict);

    builder.Property(row => row.Debit)
      .HasPrecision(12, 2);

    builder.Property(row => row.Credit)
      .HasPrecision(12, 2);

    builder.Property(row => row.Description)
      .HasMaxLength(100);
  }
}
