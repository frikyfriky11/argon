namespace Argon.Application.BankStatements.Parse.Parsers;

/// <summary>
///   Represents a factory that can create the correct parser
/// </summary>
public interface IParsersFactory
{
  /// <summary>
  ///   Creates a new parser based on the specified parser id
  /// </summary>
  /// <param name="parserId">The id of the parser</param>
  /// <returns>The parser implementation</returns>
  Task<IParser> CreateParserAsync(Guid parserId);
}