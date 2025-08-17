namespace Argon.Application.CounterpartyIdentifiers.Get;

/// <summary>
///   The result of the get request of a CounterpartyIdentifier entity
/// </summary>
/// <param name="Id">The id of the counterpartyIdentifier</param>
/// <param name="CounterpartyId">The id of the counterparty</param>
/// <param name="IdentifierText">The actual text of the counterpartyIdentifier</param>
[PublicAPI]
public record CounterpartyIdentifiersGetResponse(
  Guid Id,
  Guid CounterpartyId,
  string IdentifierText
);