namespace Argon.Domain.Entities;

/// <summary>
///   A Counterparty is an object that can represent a customer, a supplier, an employer and in general, an entity
///   that sends or receives money. It is used in place of the simpler historical Description field on the Transaction.
/// </summary>
public class Counterparty : BaseAuditableEntity
{
  /// <summary>
  ///   The id of the counterparty
  /// </summary>
  public Guid Id { get; set; }

  /// <summary>
  ///   The name of the counterparty
  /// </summary>
  public string Name { get; set; } = default!;

  /// <summary>
  ///   All the transactions of this counterparty
  /// </summary>
  public ICollection<Transaction> Transactions { get; set; } = default!;

  /// <summary>
  ///   All the identifiers of this counterparty
  /// </summary>
  public ICollection<CounterpartyIdentifier> Identifiers { get; set; } = default!;
}