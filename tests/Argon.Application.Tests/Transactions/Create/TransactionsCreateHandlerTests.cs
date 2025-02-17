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
    EntityEntry<Counterparty> marketCounterparty = await _dbContext.Counterparties.AddAsync(new Counterparty { Name = "Market" });
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    TransactionRowsCreateRequest row1 = new(1, accountGroceries.Entity.Id, 100.00m, null, "test row 1 description");
    TransactionRowsCreateRequest row2 = new(2, accountBank.Entity.Id, null, 100.00m, "test row 2 description");
    TransactionsCreateRequest request = new(new DateOnly(2023, 04, 05), marketCounterparty.Entity.Id, new List<TransactionRowsCreateRequest>
    {
      row1,
      row2,
    });

    // act
    TransactionsCreateResponse result = await _sut.Handle(request, CancellationToken.None);

    // assert
    Transaction? dbTransaction = await _dbContext
      .Transactions
      .Include(transaction => transaction.TransactionRows)
      .FirstOrDefaultAsync(x => x.Id == result.Id);

    dbTransaction.Should().NotBeNull();
    dbTransaction!.CounterpartyId.Should().Be(request.CounterpartyId);
    dbTransaction.Date.Should().Be(request.Date);
    dbTransaction.TransactionRows.Should().NotBeEmpty();
    dbTransaction.TransactionRows.Should().HaveCount(2);
    
    dbTransaction.TransactionRows.First().RowCounter.Should().Be(row1.RowCounter);
    dbTransaction.TransactionRows.First().AccountId.Should().Be(row1.AccountId);
    dbTransaction.TransactionRows.First().Debit.Should().Be(row1.Debit);
    dbTransaction.TransactionRows.First().Credit.Should().Be(row1.Credit);
    dbTransaction.TransactionRows.First().Description.Should().Be(row1.Description);

    dbTransaction.TransactionRows.Last().RowCounter.Should().Be(row2.RowCounter);
    dbTransaction.TransactionRows.Last().AccountId.Should().Be(row2.AccountId);
    dbTransaction.TransactionRows.Last().Debit.Should().Be(row2.Debit);
    dbTransaction.TransactionRows.Last().Credit.Should().Be(row2.Credit);
    dbTransaction.TransactionRows.Last().Description.Should().Be(row2.Description);
  }
}
