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
  ///   The description of the transaction
  /// </summary>
  public string Description { get; set; } = default!;

  /// <summary>
  ///   All the transaction rows of this transaction
  /// </summary>
  public ICollection<TransactionRow> TransactionRows { get; set; } = default!;
}
