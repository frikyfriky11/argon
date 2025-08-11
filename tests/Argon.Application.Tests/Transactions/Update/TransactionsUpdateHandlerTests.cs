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
    EntityEntry<Counterparty> marketCounterparty = await _dbContext.Counterparties.AddAsync(new Counterparty { Name = "Market" });
    EntityEntry<Counterparty> restaurantCounterparty = await _dbContext.Counterparties.AddAsync(new Counterparty { Name = "Restaurant" });

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

    EntityEntry<Transaction> existingTransaction = await _dbContext.Transactions.AddAsync(new Transaction
    {
      Date = new DateOnly(2023, 04, 05),
      Counterparty = marketCounterparty.Entity,
      TransactionRows = new List<TransactionRow>
      {
        existingFirstRow,
        existingSecondRow,
      },
    });

    await _dbContext.SaveChangesAsync(CancellationToken.None);

    TransactionRowsUpdateRequest newRow1 = new(existingFirstRow.Id, 1, accountRestaurants.Entity.Id, 200.00m, null, "new test row 1 description");
    TransactionRowsUpdateRequest newRow2 = new(existingSecondRow.Id, 2, accountCash.Entity.Id, null, 200.00m, "new test row 2 description");

    List<TransactionRowsUpdateRequest> rowList = new()
    {
      newRow1,
      newRow2,
    };

    TransactionsUpdateRequest request = new(new DateOnly(2023, 04, 06), restaurantCounterparty.Entity.Id, rowList) { Id = existingTransaction.Entity.Id };

    // act
    await _sut.Handle(request, CancellationToken.None);

    // assert
    Transaction? dbTransaction = await _dbContext
      .Transactions
      .Include(transaction => transaction.TransactionRows)
      .FirstOrDefaultAsync(x => x.Id == existingTransaction.Entity.Id);

    dbTransaction.Should().NotBeNull();
    dbTransaction!.Date.Should().Be(request.Date);
    dbTransaction.CounterpartyId.Should().Be(request.CounterpartyId);
    dbTransaction.Status.Should().Be(TransactionStatus.Confirmed);
    dbTransaction.PotentialDuplicateOfTransactionId.Should().BeNull();

    dbTransaction.TransactionRows.Should().HaveCount(2);

    dbTransaction.TransactionRows.First().Id.Should().Be(newRow1.Id!.Value);
    dbTransaction.TransactionRows.First().RowCounter.Should().Be(newRow1.RowCounter);
    dbTransaction.TransactionRows.First().Debit.Should().Be(newRow1.Debit);
    dbTransaction.TransactionRows.First().Credit.Should().Be(newRow1.Credit);
    dbTransaction.TransactionRows.First().Description.Should().Be(newRow1.Description);
    dbTransaction.TransactionRows.First().AccountId.Should().Be(newRow1.AccountId);

    dbTransaction.TransactionRows.Last().Id.Should().Be(newRow2.Id!.Value);
    dbTransaction.TransactionRows.Last().RowCounter.Should().Be(newRow2.RowCounter);
    dbTransaction.TransactionRows.Last().Debit.Should().Be(newRow2.Debit);
    dbTransaction.TransactionRows.Last().Credit.Should().Be(newRow2.Credit);
    dbTransaction.TransactionRows.Last().Description.Should().Be(newRow2.Description);
    dbTransaction.TransactionRows.Last().AccountId.Should().Be(newRow2.AccountId);
  }

  [Test]
  public async Task Handle_ShouldThrowNotFoundException_WithNonExistingId()
  {
    // arrange
    TransactionsUpdateRequest request = new(new DateOnly(2023, 04, 06), Guid.NewGuid(), new List<TransactionRowsUpdateRequest>()) { Id = Guid.NewGuid() };

    // act
    Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

    // assert
    await act.Should().ThrowAsync<NotFoundException>();
  }
}
