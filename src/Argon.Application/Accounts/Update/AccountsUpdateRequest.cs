namespace Argon.Application.Accounts.Update;

/// <summary>
///   The request to update an existing account
/// </summary>
/// <param name="Name">The name of the account</param>
/// <param name="Type">The type of the account</param>
[PublicAPI]
public record AccountsUpdateRequest(
  string Name,
  AccountType Type
) : IRequest
{
  /// <summary>
  ///   This field is used only internally to manually bind the [FromRoute] Guid id attribute.
  ///   It is not displayed in the documentation because the user of the API should use the route parameter.
  ///   This cannot be made internal because it would cause conflicts since you couldn't ever set it.
  /// </summary>
  [OpenApiIgnore]
  [JsonIgnore]
  public Guid Id { get; set; }
}