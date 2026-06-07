namespace Argon.Application.CounterpartyIdentifiers.Resolve;

/// <param name="CounterpartyId">The matched counterparty id</param>
/// <param name="CounterpartyName">The matched counterparty name</param>
/// <param name="MatchedByIdentifier">True if any of the counterparty's identifiers matched the raw text</param>
/// <param name="MatchedByName">True if the counterparty's name itself matched the raw text</param>
[PublicAPI]
public record CounterpartyIdentifiersResolveResponse(
  Guid CounterpartyId,
  string CounterpartyName,
  bool MatchedByIdentifier,
  bool MatchedByName);
