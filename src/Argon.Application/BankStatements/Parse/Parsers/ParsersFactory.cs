namespace Argon.Application.BankStatements.Parse.Parsers;

/// <inheritdoc />
public class ParsersFactory(IEnumerable<IParser> parsers) : IParsersFactory
{
  /// <inheritdoc />
  public Task<IParser> CreateParserAsync(Guid parserId)
  {
    foreach (IParser parser in parsers)
      if (parser.ParserId == parserId)
        return Task.FromResult(parser);

    throw new ArgumentException($"Unknown parser: {parserId}");
  }
}