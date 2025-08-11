namespace Argon.Application.Counterparties.Update;

/// <summary>
///   The request to update an existing counterparty
/// </summary>
/// <param name="Name">The name of the counterparty</param>
[PublicAPI]
public record CounterpartiesUpdateRequest(
  string Name
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