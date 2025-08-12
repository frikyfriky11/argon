namespace Argon.Application.CounterpartyIdentifiers.Create;

/// <summary>
///   The request to create a new CounterpartyIdentifier entity
/// </summary>
/// <param name="CounterpartyId">The id of the counterparty</param>
/// <param name="IdentifierText">The actual text of the counterparty identifier</param>
[PublicAPI]
public record CounterpartyIdentifiersCreateRequest(
  Guid CounterpartyId,
  string IdentifierText
) : IRequest<CounterpartyIdentifiersCreateResponse>;