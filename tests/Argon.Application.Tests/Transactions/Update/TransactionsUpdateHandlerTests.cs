using Argon.Application.Transactions.Update;

namespace Argon.Application.Tests.Transactions.Update;

public class TransactionsUpdateHandlerTests
{
  [SetUp]
  public void SetUp()
  {
    _dbContext = DatabaseTestHelpers.GetInMemoryDbContext();
    
    _sut = new TransactionsUpdateHandler(_dbContext);
  }

  private TransactionsUpdateHandler _sut = null!;
  private IApplicationDbContext _dbContext = null!;

  [Test]
  public async Task Handle_ShouldCompleteCorrectly_WithExistingId()
  {
    // arrange
    EntityEntry<Account> accountGroceries = await _dbContext.Accounts.AddAsync(new Account { Name = "Groceries" });
    EntityEntry<Account> accountBank = await _dbContext.Accounts.AddAsync(new Account { Name = "Bank" });
    EntityEntry<Account> accountRestaurants = await _dbContext.Accounts.AddAsync(new Account { Name = "Restaurants" });
    EntityEntry<Account> accountCash = await _dbContext.Accounts.AddAsync(new Account { Name = "Cash" });

    TransactionRow existingFirstRow = new()
    {
      RowCounter = 1,
      Account = accountGroceries.Entity,
      Debit = 100.00m,
      Credit = null,
      Description = "test row 1 description",
    };

    TransactionRow existingSecondRow = new()
    {
      RowCounter = 2,
      Account = accountBank.Entity,
      Debit = null,
      Credit = 100.00m,
      Description = "test row 2 description",
    };

    EntityEntry<Transaction> existingEntity = await _dbContext.Transactions.AddAsync(new Transaction
    {
      Date = new DateOnly(2023, 04, 05),
      Description = "test description",
      TransactionRows = new List<TransactionRow>
      {
        existingFirstRow,
        existingSecondRow,
      },
    });

    await _dbContext.SaveChangesAsync(CancellationToken.None);

    List<TransactionRowsUpdateRequest> rowList = new()
    {
      new TransactionRowsUpdateRequest(existingFirstRow.Id, 1, accountRestaurants.Entity.Id, 200.00m, null, "new test row 1 description"),
      new TransactionRowsUpdateRequest(existingSecondRow.Id, 2, accountCash.Entity.Id, null, 200.00m, "new test row 2 description"),
    };

    TransactionsUpdateRequest request = new(new DateOnly(2023, 04, 06), "new test description", rowList) { Id = existingEntity.Entity.Id };

    Transaction expected = new()
    {
      Date = request.Date,
      Description = request.Description,
      TransactionRows = rowList
        .Select(row => new TransactionRow
        {
          RowCounter = row.RowCounter,
          AccountId = row.AccountId,
          Description = row.Description,
          Debit = row.Debit,
          Credit = row.Credit,
        })
        .ToList(),
    };

    // act
    await _sut.Handle(request, CancellationToken.None);

    // assert
    Transaction? entity = await _dbContext.Transactions.FirstOrDefaultAsync(x => x.Id == existingEntity.Entity.Id);

    entity.Should().BeEquivalentTo(expected, config => config
      .For(x => x.TransactionRows).Exclude(x => x.Id)
      .For(x => x.TransactionRows).Exclude(x => x.Transaction)
      .For(x => x.TransactionRows).Exclude(x => x.TransactionId)
      .For(x => x.TransactionRows).Exclude(x => x.Account)
      .Excluding(x => x.Created)
      .Excluding(x => x.LastModified));
  }

  [Test]
  public async Task Handle_ShouldThrowNotFoundException_WithNonExistingId()
  {
    // arrange
    TransactionsUpdateRequest request = new(new DateOnly(2023, 04, 06), "new test description", new List<TransactionRowsUpdateRequest>()) { Id = Guid.NewGuid() };

    // act
    Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

    // assert
    await act.Should().ThrowAsync<NotFoundException>();
  }
}
