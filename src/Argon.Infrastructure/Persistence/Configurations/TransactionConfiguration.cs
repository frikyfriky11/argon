namespace Argon.Infrastructure.Persistence.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
  public void Configure(EntityTypeBuilder<Transaction> builder)
  {
    builder.Property(transaction => transaction.Description)
      .HasMaxLength(100)
      .IsRequired();
  }
}
