namespace Argon.Application.Transactions.Update;

[UsedImplicitly]
public class TransactionsUpdateHandler : IRequestHandler<TransactionsUpdateRequest>
{
  private readonly IApplicationDbContext _dbContext;
  private readonly IMapper _mapper;

  public TransactionsUpdateHandler(IApplicationDbContext dbContext, IMapper mapper)
  {
    _dbContext = dbContext;
    _mapper = mapper;
  }

  public async Task Handle(TransactionsUpdateRequest request, CancellationToken cancellationToken)
  {
    Transaction? entity = await _dbContext
      .Transactions
      .Include(transaction => transaction.TransactionRows)
      .Where(transaction => transaction.Id == request.Id)
      .FirstOrDefaultAsync(cancellationToken);

    if (entity is null)
    {
      throw new NotFoundException(nameof(Transaction), request.Id);
    }

    _mapper.Map(request, entity);

    List<TransactionRowsUpdateRequest> tempRequestRows = request.TransactionRows;

    foreach (TransactionRow entityRow in entity.TransactionRows)
    {
      TransactionRowsUpdateRequest? requestRow = tempRequestRows.FirstOrDefault(r => r.Id == entityRow.Id);

      if (requestRow is not null)
      {
        _mapper.Map(requestRow, entityRow);
        tempRequestRows.Remove(requestRow);
      }
      else
      {
        entity.TransactionRows.Remove(entityRow);
      }
    }

    foreach (TransactionRowsUpdateRequest requestRow in tempRequestRows)
    {
      entity.TransactionRows.Add(_mapper.Map<TransactionRow>(requestRow));
    }

    await _dbContext.SaveChangesAsync(cancellationToken);
  }
}
