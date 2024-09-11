namespace Argon.Application.Transactions.Get;

[UsedImplicitly]
public class TransactionsGetHandler : IRequestHandler<TransactionsGetRequest, TransactionsGetResponse>
{
  private readonly IApplicationDbContext _dbContext;
  private readonly IMapper _mapper;

  public TransactionsGetHandler(IApplicationDbContext dbContext, IMapper mapper)
  {
    _dbContext = dbContext;
    _mapper = mapper;
  }

  public async Task<TransactionsGetResponse> Handle(TransactionsGetRequest request, CancellationToken cancellationToken)
  {
    TransactionsGetResponse? result = await _dbContext
      .Transactions
      .AsNoTracking()
      .Where(transaction => transaction.Id == request.Id)
      .ProjectTo<TransactionsGetResponse>(_mapper.ConfigurationProvider)
      .FirstOrDefaultAsync(cancellationToken);

    if (result is null)
    {
      throw new NotFoundException();
    }

    return result;
  }
}
