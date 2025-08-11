namespace Argon.Application.Accounts.Delete;

/// <summary>
///   The request to delete an existing Account entity
/// </summary>
/// <param name="Id">The id of the account</param>
[PublicAPI]
public record AccountsDeleteRequest(
  Guid Id
) : IRequest;