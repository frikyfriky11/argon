using Argon.Application.Transactions.SetCounterparty;

namespace Argon.Application.Tests.Transactions.SetCounterparty;

public class TransactionsSetCounterpartyHandlerTests
{
  [SetUp]
  public void SetUp()
  {
    _dbContext = DatabaseTestHelpers.GetInMemoryDbContext();
    _sut = new TransactionsSetCounterpartyHandler(_dbContext);
  }

  private TransactionsSetCounterpartyHandler _sut = null!;
  private IApplicationDbContext _dbContext = null!;

  [Test]
  public async Task Handle_ShouldReassignCounterparty_WhenTransactionExists()
  {
    // arrange
    EntityEntry<Account> bankAccount = await _dbContext.Accounts.AddAsync(new Account { Name = "Bank" });
    EntityEntry<Account> groceriesAccount = await _dbContext.Accounts.AddAsync(new Account { Name = "Groceries" });
    EntityEntry<Counterparty> wrongCounterparty = await _dbContext.Counterparties.AddAsync(new Counterparty { Name = "Wrong" });
    EntityEntry<Counterparty> rightCounterparty = await _dbContext.Counterparties.AddAsync(new Counterparty { Name = "Right" });

    TransactionRow rowOne = new() { RowCounter = 1, Account = bankAccount.Entity, Credit = 30m };
    TransactionRow rowTwo = new() { RowCounter = 2, Account = groceriesAccount.Entity, Debit = 30m };

    EntityEntry<Transaction> transaction = await _dbContext.Transactions.AddAsync(new Transaction
    {
      Date = new DateOnly(2024, 1, 1),
      Counterparty = wrongCounterparty.Entity,
      Status = TransactionStatus.Confirmed,
      TransactionRows = new List<TransactionRow> { rowOne, rowTwo },
    });
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    TransactionsSetCounterpartyRequest request = new(rightCounterparty.Entity.Id)
    {
      TransactionId = transaction.Entity.Id,
    };

    // act
    await _sut.Handle(request, CancellationToken.None);

    // assert
    Transaction? saved = await _dbContext.Transactions.FirstOrDefaultAsync(t => t.Id == transaction.Entity.Id);
    saved!.CounterpartyId.Should().Be(rightCounterparty.Entity.Id);
  }

  [Test]
  public async Task Handle_ShouldThrowNotFound_WhenTransactionDoesNotExist()
  {
    TransactionsSetCounterpartyRequest request = new(Guid.NewGuid())
    {
      TransactionId = Guid.NewGuid(),
    };

    Func<Task> act = () => _sut.Handle(request, CancellationToken.None);

    await act.Should().ThrowAsync<NotFoundException>();
  }
}
