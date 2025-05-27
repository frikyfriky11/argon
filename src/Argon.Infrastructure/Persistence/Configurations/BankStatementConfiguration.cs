namespace Argon.Infrastructure.Persistence.Configurations;

public class BankStatementConfiguration : IEntityTypeConfiguration<BankStatement>
{
  public void Configure(EntityTypeBuilder<BankStatement> builder)
  {
    builder.Property(bankStatement => bankStatement.FileName)
      .HasMaxLength(250)
      .IsRequired();
    
    builder.Property(bankStatement => bankStatement.ParserName)
      .HasMaxLength(250)
      .IsRequired();
    
    builder.HasOne(bankStatement => bankStatement.ImportedToAccount)
      .WithMany(account => account.BankStatements)
      .HasForeignKey(bankStatement => bankStatement.ImportedToAccountId);
  }
}