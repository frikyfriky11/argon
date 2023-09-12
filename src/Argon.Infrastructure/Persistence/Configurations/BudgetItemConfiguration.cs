namespace Argon.Infrastructure.Persistence.Configurations;

public class BudgetItemConfiguration : IEntityTypeConfiguration<BudgetItem>
{
  public void Configure(EntityTypeBuilder<BudgetItem> builder)
  {
    builder.HasOne(budgetItem => budgetItem.Account)
      .WithMany(account => account.BudgetItems)
      .HasForeignKey(budgetItem => budgetItem.AccountId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.Property(budgetItem => budgetItem.Amount)
      .HasPrecision(12, 2)
      .IsRequired();
  }
}
