namespace Argon.Application.CounterpartyIdentifiers.Create;

/// <summary>
///   The result of the creation of a new CounterpartyIdentifier entity
/// </summary>
/// <param name="Id">The id of the newly created CounterpartyIdentifier</param>
[PublicAPI]
public record CounterpartyIdentifiersCreateResponse(
  Guid Id
);