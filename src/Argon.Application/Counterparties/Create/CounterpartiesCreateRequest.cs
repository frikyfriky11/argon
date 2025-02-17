namespace Argon.Application.Counterparties.Create;

/// <summary>
///   The request to create a new Counterparty entity
/// </summary>
/// <param name="Name">The name of the counterparty</param>
[PublicAPI]
public record CounterpartiesCreateRequest(
  string Name
) : IRequest<CounterpartiesCreateResponse>;