namespace Argon.Application.CounterpartyIdentifiers.Delete;

/// <summary>
///   The request to delete an existing CounterpartyIdentifier entity
/// </summary>
/// <param name="Id">The id of the CounterpartyIdentifier</param>
[PublicAPI]
public record CounterpartyIdentifiersDeleteRequest(
  Guid Id
) : IRequest;