using Argon.Application.Transactions.Create;

namespace Argon.Application.Tests.Transactions.Create;

public class TransactionsCreateHandlerTests
{
  [SetUp]
  public void SetUp()
  {
    _dbContext = DatabaseTestHelpers.GetInMemoryDbContext();

    _sut = new TransactionsCreateHandler(_dbContext);
  }

  private TransactionsCreateHandler _sut = null!;
  private IApplicationDbContext _dbContext = null!;

  [Test]
  public async Task Handle_ShouldCompleteCorrectly_WithValidRequest()
  {
    // arrange
    EntityEntry<Account> accountGroceries = await _dbContext.Accounts.AddAsync(new Account { Name = "Groceries" });
    EntityEntry<Account> accountBank = await _dbContext.Accounts.AddAsync(new Account { Name = "Bank" });
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    TransactionRowsCreateRequest row1 = new(1, accountGroceries.Entity.Id, 100.00m, null, "test row 1 description");
    TransactionRowsCreateRequest row2 = new(2, accountBank.Entity.Id, null, 100.00m, "test row 2 description");
    TransactionsCreateRequest request = new(new DateOnly(2023, 04, 05), "test description", new List<TransactionRowsCreateRequest>
    {
      row1,
      row2,
    });

    Transaction expected = new()
    {
      Description = request.Description,
      Date = request.Date,
      TransactionRows = new List<TransactionRow>
      {
        new()
        {
          RowCounter = row1.RowCounter,
          AccountId = row1.AccountId,
          Debit = row1.Debit,
          Credit = row1.Credit,
          Description = row1.Description,
        },
        new()
        {
          RowCounter = row2.RowCounter,
          AccountId = row2.AccountId,
          Debit = row2.Debit,
          Credit = row2.Credit,
          Description = row2.Description,
        },
      },
    };

    // act
    TransactionsCreateResponse result = await _sut.Handle(request, CancellationToken.None);

    // assert
    Transaction? entity = await _dbContext
      .Transactions
      .Include(transaction => transaction.TransactionRows)
      .FirstOrDefaultAsync(x => x.Id == result.Id);

    entity.Should().BeEquivalentTo(expected, config => config
      .Excluding(x => x.Id)
      .For(x => x.TransactionRows).Exclude(x => x.Id)
      .For(x => x.TransactionRows).Exclude(x => x.Transaction)
      .For(x => x.TransactionRows).Exclude(x => x.TransactionId)
      .For(x => x.TransactionRows).Exclude(x => x.Account)
      .Excluding(x => x.Created)
      .Excluding(x => x.LastModified));
  }
}
