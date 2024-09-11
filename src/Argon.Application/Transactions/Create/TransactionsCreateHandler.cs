namespace Argon.Application.Transactions.Create;

[UsedImplicitly]
public class TransactionsCreateHandler : IRequestHandler<TransactionsCreateRequest, TransactionsCreateResponse>
{
  private readonly IApplicationDbContext _dbContext;
  private readonly IMapper _mapper;

  public TransactionsCreateHandler(IApplicationDbContext dbContext, IMapper mapper)
  {
    _dbContext = dbContext;
    _mapper = mapper;
  }

  public async Task<TransactionsCreateResponse> Handle(TransactionsCreateRequest request, CancellationToken cancellationToken)
  {
    Transaction entity = _mapper.Map<Transaction>(request);

    await _dbContext.Transactions.AddAsync(entity, cancellationToken);

    await _dbContext.SaveChangesAsync(cancellationToken);

    return new TransactionsCreateResponse(entity.Id);
  }
}
