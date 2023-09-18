namespace Argon.Domain.Entities;

/// <summary>
///   A Budget Item is an object that represents a predicted amount for a specific year, month and account.
/// </summary>
public class BudgetItem : BaseAuditableEntity
{
  /// <summary>
  ///   The id of the budget item
  /// </summary>
  public Guid Id { get; set; }

  /// <summary>
  ///   The id of the account
  /// </summary>
  public Guid AccountId { get; set; }

  /// <summary>
  ///   The account
  /// </summary>
  public Account Account { get; set; } = default!;

  /// <summary>
  ///   The year of reference
  /// </summary>
  public int Year { get; set; }

  /// <summary>
  ///   The month of reference
  /// </summary>
  public int Month { get; set; }

  /// <summary>
  ///   The amount of the budget item
  /// </summary>
  public decimal Amount { get; set; }
}
