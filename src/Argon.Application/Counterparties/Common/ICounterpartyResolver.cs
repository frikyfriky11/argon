namespace Argon.Application.Counterparties.Common;

/// <summary>
///   Matches a free-form raw string (typically a snippet pulled from a bank statement
///   line) against existing counterparties and counterparty identifiers using the
///   same bidirectional substring rules the importer relies on. Sharing this logic
///   makes the importer and the `cpi resolve` endpoint behave identically.
/// </summary>
public interface ICounterpartyResolver
{
  Task<List<CounterpartyResolution>> ResolveAsync(string? rawText, CancellationToken cancellationToken);
}

/// <param name="Id">The matched counterparty id</param>
/// <param name="Name">The matched counterparty name</param>
/// <param name="MatchedByIdentifier">True if any of the counterparty's identifiers matched the raw text</param>
/// <param name="MatchedByName">True if the counterparty's name itself matched the raw text</param>
[PublicAPI]
public record CounterpartyResolution(
  Guid Id,
  string Name,
  bool MatchedByIdentifier,
  bool MatchedByName);
