namespace Argon.Application.CounterpartyIdentifiers.Resolve;

/// <summary>
///   The request to match a free-form raw text against existing counterparties using
///   the same substring rules the bank-statement importer relies on.
/// </summary>
/// <param name="RawText">The raw text to resolve (typically a snippet from a bank line)</param>
[PublicAPI]
public record CounterpartyIdentifiersResolveRequest(
  string RawText
) : IRequest<List<CounterpartyIdentifiersResolveResponse>>;
