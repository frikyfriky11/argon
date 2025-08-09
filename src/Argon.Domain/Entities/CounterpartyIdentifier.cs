namespace Argon.Domain.Entities;

/// <summary>
///   A counterparty identifier is an alternative way to identify a specific counterparty.
///   It allows the same counterparty to have different names (or internal names) that are useful for search
///   or auto matching during imports.
/// </summary>
public class CounterpartyIdentifier : BaseAuditableEntity
{
  /// <summary>
  ///   The id of the counterparty identifier
  /// </summary>
  public Guid Id { get; set; }

  /// <summary>
  ///   The id of the counterparty
  /// </summary>
  public Guid CounterpartyId { get; set; }

  /// <summary>
  ///   The counterparty
  /// </summary>
  public Counterparty Counterparty { get; set; } = default!;

  /// <summary>
  ///   The actual text of the counterparty identifier
  /// </summary>
  public string IdentifierText { get; set; } = default!;
}