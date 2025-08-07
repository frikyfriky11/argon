namespace Argon.Application.BankStatements.GetList;

/// <summary>
///   The result of the BankStatement entities get list
/// </summary>
/// <param name="Id">The id of the bank statement</param>
/// <param name="FileName">The file name of the bank statement</param>
/// <param name="ParserId">The parser id used for parsing the bank statement</param>
/// <param name="ParserName">The parser name used for parsing the bank statement</param>
/// <param name="TransactionsCount">The number of transactions parsed from the bank statement</param>
[PublicAPI]
public record BankStatementsGetListResponse(
  Guid Id,
  string FileName,
  Guid ParserId,
  string ParserName,
  int TransactionsCount
);