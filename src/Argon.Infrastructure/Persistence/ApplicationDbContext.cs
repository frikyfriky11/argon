namespace Argon.Infrastructure.Persistence;

/// <summary>
///   The main application database context
/// </summary>
public class ApplicationDbContext(
  DbContextOptions<ApplicationDbContext> options,
  ISaveChangesInterceptor saveChangesInterceptor
) : DbContext(options),
  IApplicationDbContext
{
  /// <inheritdoc />
  public DbSet<Account> Accounts => Set<Account>();

  /// <inheritdoc />
  public DbSet<Transaction> Transactions => Set<Transaction>();

  /// <inheritdoc />
  public DbSet<TransactionRow> TransactionRows => Set<TransactionRow>();

  /// <inheritdoc />
  public DbSet<BudgetItem> BudgetItems => Set<BudgetItem>();

  /// <inheritdoc />
  public DbSet<Counterparty> Counterparties => Set<Counterparty>();

  /// <inheritdoc />
  public DbSet<CounterpartyIdentifier> CounterpartyIdentifiers => Set<CounterpartyIdentifier>();

  /// <inheritdoc />
  public DbSet<BankStatement> BankStatements => Set<BankStatement>();

  protected override void OnModelCreating(ModelBuilder builder)
  {
    // apply all the configurations for every entity that implements IEntityTypeConfiguration<T>
    builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

    base.OnModelCreating(builder);
  }

  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
  {
    // add the interceptors (usually the AuditableEntitySaveChangesInterceptor)
    optionsBuilder.AddInterceptors(saveChangesInterceptor);

    // add nicer exception objects using https://github.com/Giorgi/EntityFramework.Exceptions
    optionsBuilder.UseExceptionProcessor();
  }
}