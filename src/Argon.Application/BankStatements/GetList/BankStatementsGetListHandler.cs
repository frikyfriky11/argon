namespace Argon.Application.BankStatements.GetList;

[UsedImplicitly]
public class BankStatementsGetListHandler(
  IApplicationDbContext dbContext,
  IEnumerable<IParser> parsers
) : IRequestHandler<BankStatementsGetListRequest, List<BankStatementsGetListResponse>>
{
  public async Task<List<BankStatementsGetListResponse>> Handle(BankStatementsGetListRequest request,
    CancellationToken cancellationToken)
  {
    List<BankStatementsGetListResponse> result = await dbContext
      .BankStatements
      .AsNoTracking()
      .OrderByDescending(bankStatement => bankStatement.Created)
      .Select(bankStatement => new BankStatementsGetListResponse(
        bankStatement.Id,
        bankStatement.FileName,
        bankStatement.ParserId,
        string.Empty,
        bankStatement.Transactions.Count
      ))
      .ToListAsync(cancellationToken);

    result = result
      .Select(r => r with { ParserName = parsers.First(p => p.ParserId == r.ParserId).ParserDisplayName })
      .ToList();

    return result;
  }
}