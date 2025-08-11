namespace Argon.Application.BankStatements.ParsersGetList;

[UsedImplicitly]
public class BankStatementParsersGetListHandler(
  IEnumerable<IParser> parsers
) : IRequestHandler<BankStatementParsersGetListRequest, List<BankStatementParsersGetListResponse>>
{
  public Task<List<BankStatementParsersGetListResponse>> Handle(BankStatementParsersGetListRequest request,
    CancellationToken cancellationToken)
  {
    List<BankStatementParsersGetListResponse> result = parsers
      .Select(p => new BankStatementParsersGetListResponse(p.ParserId, p.ParserDisplayName))
      .ToList();

    return Task.FromResult(result);
  }
}