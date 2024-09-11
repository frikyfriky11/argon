namespace Argon.Application.Accounts.Favourite;

/// <summary>
///   The request to update the favourite status on an account
///   <param name="IsFavourite">Whether the account is marked as favourite</param>
/// </summary>
[PublicAPI]
public record AccountsFavouriteRequest(bool IsFavourite) : IRequest
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