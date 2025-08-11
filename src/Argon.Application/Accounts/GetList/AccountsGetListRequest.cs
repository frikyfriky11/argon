namespace Argon.Application.Accounts.GetList;

/// <summary>
///   The request to get a list of Account entities
///   <param name="TotalAmountsFrom">The start date to use to compute the total amounts</param>
///   <param name="TotalAmountsTo">The end date to use to compute the total amounts</param>
/// </summary>
[PublicAPI]
public record AccountsGetListRequest(DateTimeOffset? TotalAmountsFrom, DateTimeOffset? TotalAmountsTo) : IRequest<List<AccountsGetListResponse>>;

// TODO find out how to replace DateTimeOffset with DateOnly and how to make it go along well with NSwag