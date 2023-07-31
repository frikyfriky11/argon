namespace Argon.Infrastructure.Persistence.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
  public void Configure(EntityTypeBuilder<Account> builder)
  {
    builder.Property(account => account.Name)
      .HasMaxLength(50)
      .IsRequired();
  }
}
