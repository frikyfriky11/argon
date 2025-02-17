namespace Argon.Application.Counterparties.Get;

/// <summary>
///   The request to get an existing Counterparty entity
/// </summary>
/// <param name="Id">The id of the counterparty</param>
[PublicAPI]
public record CounterpartiesGetRequest(
  Guid Id
) : IRequest<CounterpartiesGetResponse>;