namespace Argon.Application.CounterpartyIdentifiers.Get;

/// <summary>
///   The request to get an existing CounterpartyIdentifier entity
/// </summary>
/// <param name="Id">The id of the counterpartyIdentifier</param>
[PublicAPI]
public record CounterpartyIdentifiersGetRequest(
  Guid Id
) : IRequest<CounterpartyIdentifiersGetResponse>;