namespace Argon.Application.BankStatements.Parse.Parsers;

/// <summary>
///   Represents a parsing engine that can take a stream as an input and output a list of bank statement items
/// </summary>
public interface IParser
{
  /// <summary>
  ///   A globally unique identifier that identifies the single parser
  /// </summary>
  Guid ParserId { get; }

  /// <summary>
  ///   The display name of the parser
  /// </summary>
  string ParserDisplayName { get; }

  /// <summary>
  ///   Parses the input stream into a collection of bank statement items
  /// </summary>
  /// <param name="file">The input stream to parse</param>
  /// <returns>A collection of bank statement items</returns>
  Task<List<BankStatementItem>> ParseAsync(Stream file);
}