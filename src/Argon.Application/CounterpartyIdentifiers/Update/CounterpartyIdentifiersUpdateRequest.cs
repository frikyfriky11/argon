namespace Argon.Application.CounterpartyIdentifiers.Update;

/// <summary>
///   The request to update an existing counterpartyIdentifier
/// </summary>
/// <param name="CounterpartyId">The id of the counterparty</param>
/// <param name="IdentifierText">The actual text of the counterpartyIdentifier</param>
[PublicAPI]
public record CounterpartyIdentifiersUpdateRequest(
  Guid CounterpartyId,
  string IdentifierText
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