namespace Argon.Application.Counterparties.Get;

/// <summary>
///   The result of the get request of a Counterparty entity
/// </summary>
/// <param name="Id">The id of the counterparty</param>
/// <param name="Name">The name of the counterparty</param>
[PublicAPI]
public record CounterpartiesGetResponse(
  Guid Id,
  string Name
);