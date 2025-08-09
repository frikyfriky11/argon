namespace Argon.Domain.Entities;

/// <summary>
///   A Transaction is an object that groups multiple transaction rows, and the sum of all of them must be zero.
/// </summary>
public class Transaction : BaseAuditableEntity
{
  /// <summary>
  ///   The id of the transaction
  /// </summary>
  public Guid Id { get; set; }

  /// <summary>
  ///   The date of the transaction
  /// </summary>
  public DateOnly Date { get; set; }

  /// <summary>
  ///   The id of the counterparty of the transaction.
  ///   Could be null if this transaction was imported and no suitable counterparty was found.
  /// </summary>
  public Guid? CounterpartyId { get; set; }

  /// <summary>
  ///   The counterparty of the transaction.
  ///   Could be null if this transaction was imported and no suitable counterparty was found.
  /// </summary>
  public Counterparty? Counterparty { get; set; }

  /// <summary>
  ///   The status of the transaction
  /// </summary>
  public TransactionStatus Status { get; set; }

  /// <summary>
  ///   The id of another transaction identified as a potential duplicate of the current transaction
  /// </summary>
  public Guid? PotentialDuplicateOfTransactionId { get; set; }

  /// <summary>
  ///   Another transaction identified as a potential duplicate of the current transaction
  /// </summary>
  public Transaction? PotentialDuplicateOfTransaction { get; set; }

  /// <summary>
  ///   The id of the bank statement from where this transaction originated
  /// </summary>
  public Guid? BankStatementId { get; set; }

  /// <summary>
  ///   The bank statement from where this transaction originated
  /// </summary>
  public BankStatement? BankStatement { get; set; }

  /// <summary>
  ///   The JSON representation of the raw import data of a bank statement
  /// </summary>
  public string? RawImportData { get; set; }

  /// <summary>
  ///   All the transaction rows of this transaction
  /// </summary>
  public ICollection<TransactionRow> TransactionRows { get; set; } = default!;

  /// <summary>
  ///   All the transactions that refer to this transaction as duplicate
  /// </summary>
  public ICollection<Transaction> DuplicateTransactions { get; set; } = default!;
}