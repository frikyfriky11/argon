namespace Argon.Application.Accounts.Create;

/// <summary>
///   The request to create a new Account entity
/// </summary>
/// <param name="Name">The name of the account</param>
/// <param name="Type">The type of the account</param>
[PublicAPI]
public record AccountsCreateRequest(
  string Name,
  AccountType Type
) : IRequest<AccountsCreateResponse>;