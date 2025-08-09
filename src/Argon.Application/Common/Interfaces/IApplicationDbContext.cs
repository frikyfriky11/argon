namespace Argon.Application.Common.Interfaces;

/// <summary>
///   Represents an interface for the application database context.
/// </summary>
public interface IApplicationDbContext
{
  /// <summary>
  ///   This table contains all the Account entities
  /// </summary>
  DbSet<Account> Accounts { get; }

  /// <summary>
  ///   This table contains all the Transaction entities
  /// </summary>
  DbSet<Transaction> Transactions { get; }

  /// <summary>
  ///   This table contains all the TransactionRow entities
  /// </summary>
  DbSet<TransactionRow> TransactionRows { get; }

  /// <summary>
  ///   This table contains all the BudgetItem entities
  /// </summary>
  DbSet<BudgetItem> BudgetItems { get; }

  /// <summary>
  ///   This table contains all the Counterparty entities
  /// </summary>
  DbSet<Counterparty> Counterparties { get; }

  /// <summary>
  ///   This table contains all the CounterpartyIdentifier entities
  /// </summary>
  DbSet<CounterpartyIdentifier> CounterpartyIdentifiers { get; }

  /// <summary>
  ///   This table contains all the BankStatement entities
  /// </summary>
  DbSet<BankStatement> BankStatements { get; }

  /// <summary>
  ///   Saves the changes asynchronously
  /// </summary>
  /// <param name="cancellationToken">The cancellation token to use</param>
  /// <returns>The number of state entries written to the database</returns>
  Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}