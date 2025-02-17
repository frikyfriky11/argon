using Argon.Application.Transactions.Get;

namespace Argon.Application.Tests.Transactions.Get;

public class TransactionsGetHandlerTests
{
  [SetUp]
  public void SetUp()
  {
    _dbContext = DatabaseTestHelpers.GetInMemoryDbContext();

    _sut = new TransactionsGetHandler(_dbContext);
  }

  private TransactionsGetHandler _sut = null!;
  private IApplicationDbContext _dbContext = null!;

  [Test]
  public async Task Handle_ShouldCompleteCorrectly_WithExistingId()
  {
    // arrange
    EntityEntry<Account> accountGroceries = await _dbContext.Accounts.AddAsync(new Account { Name = "Groceries" });
    EntityEntry<Account> accountBank = await _dbContext.Accounts.AddAsync(new Account { Name = "Bank" });
    EntityEntry<Counterparty> marketCounterparty = await _dbContext.Counterparties.AddAsync(new Counterparty() { Name = "Market" });

    TransactionRow row1 = new()
    {
      RowCounter = 1,
      Account = accountGroceries.Entity,
      Debit = 100.00m,
      Credit = null,
      Description = "test row 1 description",
    };
    TransactionRow row2 = new()
    {
      RowCounter = 2,
      Account = accountBank.Entity,
      Debit = null,
      Credit = 100.00m,
      Description = "test row 2 description",
    };
    Transaction transaction = new()
    {
      Date = new DateOnly(2023, 04, 05),
      Counterparty = marketCounterparty.Entity,
      TransactionRows = new List<TransactionRow>
      {
        row1,
        row2,
      },
    };

    await _dbContext.Transactions.AddAsync(transaction);
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    TransactionsGetRequest request = new(transaction.Id);

    // act
    TransactionsGetResponse result = await _sut.Handle(request, CancellationToken.None);

    // assert
    result.Id.Should().Be(transaction.Id);
    result.Date.Should().Be(transaction.Date);
    result.CounterpartyId.Should().Be(transaction.CounterpartyId);
    
    result.TransactionRows[0].Id.Should().Be(row1.Id);
    result.TransactionRows[0].RowCounter.Should().Be(row1.RowCounter);
    result.TransactionRows[0].AccountId.Should().Be(row1.AccountId);
    result.TransactionRows[0].Debit.Should().Be(row1.Debit);
    result.TransactionRows[0].Credit.Should().Be(row1.Credit);
    result.TransactionRows[0].Description.Should().Be(row1.Description);
    
    result.TransactionRows[1].Id.Should().Be(row2.Id);
    result.TransactionRows[1].RowCounter.Should().Be(row2.RowCounter);
    result.TransactionRows[1].AccountId.Should().Be(row2.AccountId);
    result.TransactionRows[1].Debit.Should().Be(row2.Debit);
    result.TransactionRows[1].Credit.Should().Be(row2.Credit);
    result.TransactionRows[1].Description.Should().Be(row2.Description);
  }

  [Test]
  public async Task Handle_ShouldThrowNotFoundException_WithNonExistingId()
  {
    // arrange
    TransactionsGetRequest request = new(Guid.NewGuid());

    // act
    Func<Task<TransactionsGetResponse>> act = async () => await _sut.Handle(request, CancellationToken.None);

    // assert
    await act.Should().ThrowAsync<NotFoundException>();
  }
}
