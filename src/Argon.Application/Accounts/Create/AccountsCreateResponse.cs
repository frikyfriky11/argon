namespace Argon.Application.Accounts.Create;

/// <summary>
///   The result of the creation of a new Account entity
/// </summary>
/// <param name="Id">The id of the newly created Account</param>
[PublicAPI]
public record AccountsCreateResponse(
  Guid Id
);