namespace Argon.Application.Accounts.Get;

/// <summary>
///   The result of the get request of a Account entity
/// </summary>
/// <param name="Id">The id of the account</param>
/// <param name="Name">The name of the account</param>
/// <param name="Type">The type of the account</param>
/// <param name="IsFavourite">Whether the account is marked as favourite</param>
/// <param name="TotalAmount">The total amount that the account has registered</param>
[PublicAPI]
public record AccountsGetResponse(
  Guid Id,
  string Name,
  AccountType Type,
  bool IsFavourite,
  decimal TotalAmount
);