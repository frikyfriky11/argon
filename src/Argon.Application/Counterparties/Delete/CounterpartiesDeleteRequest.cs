namespace Argon.Application.Counterparties.Delete;

/// <summary>
///   The request to delete an existing Counterparty entity
/// </summary>
/// <param name="Id">The id of the counterparty</param>
[PublicAPI]
public record CounterpartiesDeleteRequest(
  Guid Id
) : IRequest;