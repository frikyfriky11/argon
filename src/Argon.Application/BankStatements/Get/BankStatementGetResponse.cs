using Argon.Application.Transactions.GetList;

namespace Argon.Application.BankStatements.Get;

/// <summary>
///   The result of the BankStatement entity get
/// </summary>
/// <param name="Id">The id of the bank statement</param>
/// <param name="FileName">The file name of the bank statement</param>
/// <param name="ParserName">The parser name used for parsing the bank statement</param>
/// <param name="Transactions">The list of transactions parsed from the bank statement</param>
[PublicAPI]
public record BankStatementGetResponse(
  Guid Id,
  string FileName,
  string ParserName,
  List<TransactionsGetListResponse> Transactions
);
