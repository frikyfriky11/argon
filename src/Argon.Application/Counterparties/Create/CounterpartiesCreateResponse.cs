namespace Argon.Application.Counterparties.Create;

/// <summary>
///   The result of the creation of a new Counterparty entity
/// </summary>
/// <param name="Id">The id of the newly created Counterparty</param>
[PublicAPI]
public record CounterpartiesCreateResponse(
  Guid Id
);