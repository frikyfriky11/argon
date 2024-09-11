namespace Argon.Application.Accounts.Get;

/// <summary>
///   The request to get an existing Account entity
/// </summary>
/// <param name="Id">The id of the account</param>
[PublicAPI]
public record AccountsGetRequest(
  Guid Id
) : IRequest<AccountsGetResponse>;