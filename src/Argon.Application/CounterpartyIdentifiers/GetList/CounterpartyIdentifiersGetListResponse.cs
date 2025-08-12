namespace Argon.Application.CounterpartyIdentifiers.GetList;

/// <summary>
///   The result of the CounterpartyIdentifier entities get list
/// </summary>
/// <param name="Id">The id of the counterpartyIdentifier</param>
/// <param name="CounterpartyId">The id of the counterparty</param>
/// <param name="IdentifierText">The actual text of the counterpartyIdentifier</param>
[PublicAPI]
public record CounterpartyIdentifiersGetListResponse(Guid Id, Guid CounterpartyId, string IdentifierText);