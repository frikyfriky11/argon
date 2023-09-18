namespace Argon.Domain.Entities;

/// <summary>
///   An Account is an object that can represent expenses, revenue sources and liquid assets.
///   It is used in a transaction to determine from where to where the money flows.
/// </summary>
public class Account : BaseAuditableEntity
{
  /// <summary>
  ///   The id of the account
  /// </summary>
  public Guid Id { get; set; }

  /// <summary>
  ///   The name of the account
  /// </summary>
  public string Name { get; set; } = default!;

  /// <summary>
  ///   The type of the account
  /// </summary>
  public AccountType Type { get; set; }

  /// <summary>
  ///   Whether the account is marked as favourite
  /// </summary>
  public bool IsFavourite { get; set; }

  /// <summary>
  ///   All the transaction rows of this account
  /// </summary>
  public ICollection<TransactionRow> TransactionRows { get; set; } = default!;

  /// <summary>
  ///   All the budget items of this account
  /// </summary>
  public ICollection<BudgetItem> BudgetItems { get; set; } = default!;
}
