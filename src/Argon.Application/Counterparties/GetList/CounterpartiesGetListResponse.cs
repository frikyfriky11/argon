namespace Argon.Application.Counterparties.GetList;

/// <summary>
///   The result of the Counterparty entities get list
/// </summary>
/// <param name="Id">The id of the counterparty</param>
/// <param name="Name">The name of the counterparty</param>
[PublicAPI]
public record CounterpartiesGetListResponse(Guid Id, string Name);