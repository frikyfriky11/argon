namespace Argon.Infrastructure.Persistence.Configurations;

public class BankStatementFileConfiguration : IEntityTypeConfiguration<BankStatementFile>
{
  public void Configure(EntityTypeBuilder<BankStatementFile> builder)
  {
    builder.Property(bankStatementFile => bankStatementFile.FileName)
      .HasMaxLength(250)
      .IsRequired();
    
    builder.Property(bankStatementFile => bankStatementFile.ParserName)
      .HasMaxLength(250)
      .IsRequired();
    
    builder.HasOne(bankStatementFile => bankStatementFile.ImportedToAccount)
      .WithMany(account => account.BankStatementFiles)
      .HasForeignKey(bankStatementFile => bankStatementFile.ImportedToAccountId);
  }
}