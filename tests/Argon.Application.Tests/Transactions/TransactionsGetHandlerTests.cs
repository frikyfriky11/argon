using Argon.Application.Transactions;
using Argon.Application.Transactions.Get;

namespace Argon.Application.Tests.Transactions;

[TestFixture]
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
    Transaction sampleEntity = new()
    {
      Date = new DateOnly(2023, 04, 05),
      Description = "test description",
      TransactionRows = new List<TransactionRow>
      {
        row1,
        row2,
      },
    };

    await _dbContext.Transactions.AddAsync(sampleEntity);
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    TransactionsGetRequest request = new(sampleEntity.Id);

    TransactionsGetResponse expected = new(
      sampleEntity.Id,
      sampleEntity.Date,
      sampleEntity.Description,
      new List<TransactionRowsGetResponse>()
      {
        new(row1.Id, row1.RowCounter, row1.AccountId, row1.Debit, row1.Credit, row1.Description),
        new(row2.Id, row2.RowCounter, row2.AccountId, row2.Debit, row2.Credit, row2.Description),
      }
    );

    // act
    TransactionsGetResponse result = await _sut.Handle(request, CancellationToken.None);

    // assert
    result.Should().BeEquivalentTo(expected);
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
