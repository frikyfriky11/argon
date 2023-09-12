namespace Argon.Infrastructure.Persistence;

/// <summary>
///   The main application database context
/// </summary>
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
  private readonly ISaveChangesInterceptor _saveChangesInterceptor;

  public ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    ISaveChangesInterceptor saveChangesInterceptor)
    : base(options)
  {
    _saveChangesInterceptor = saveChangesInterceptor;
  }

  /// <inheritdoc />
  public DbSet<Account> Accounts => Set<Account>();

  /// <inheritdoc />
  public DbSet<Transaction> Transactions => Set<Transaction>();

  /// <inheritdoc />
  public DbSet<TransactionRow> TransactionRows => Set<TransactionRow>();

  /// <inheritdoc />
  public DbSet<BudgetItem> BudgetItems => Set<BudgetItem>();

  protected override void OnModelCreating(ModelBuilder builder)
  {
    // apply all the configurations for every entity that implements IEntityTypeConfiguration<T>
    builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

    base.OnModelCreating(builder);
  }

  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
  {
    // add the interceptors (usually the AuditableEntitySaveChangesInterceptor)
    optionsBuilder.AddInterceptors(_saveChangesInterceptor);

    // add nicer exception objects using https://github.com/Giorgi/EntityFramework.Exceptions
    optionsBuilder.UseExceptionProcessor();
  }
}
