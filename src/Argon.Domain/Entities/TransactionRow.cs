namespace Argon.Domain.Entities;

/// <summary>
///   A TransactionRow is an object that describes a transaction in detail and belongs to a transaction.
/// </summary>
public class TransactionRow : BaseAuditableEntity
{
  /// <summary>
  ///   The id of the transaction row
  /// </summary>
  public Guid Id { get; set; }

  /// <summary>
  ///   The id of the transaction
  /// </summary>
  public Guid TransactionId { get; set; }

  /// <summary>
  ///   The transaction
  /// </summary>
  public Transaction Transaction { get; set; } = default!;

  /// <summary>
  ///   The progressive number of the transaction row in the scope of the transaction
  /// </summary>
  public int RowCounter { get; set; }

  /// <summary>
  ///   The id of the account
  /// </summary>
  public Guid AccountId { get; set; }

  /// <summary>
  ///   The account
  /// </summary>
  public Account Account { get; set; } = default!;

  /// <summary>
  ///   The debit amount of the transaction row
  /// </summary>
  public decimal? Debit { get; set; }

  /// <summary>
  ///   The credit amount of the transaction row
  /// </summary>
  public decimal? Credit { get; set; }

  /// <summary>
  ///   The description of the transaction row
  /// </summary>
  public string? Description { get; set; }
}
