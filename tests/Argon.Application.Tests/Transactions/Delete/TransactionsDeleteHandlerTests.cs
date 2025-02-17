using Argon.Application.Transactions.Delete;

namespace Argon.Application.Tests.Transactions.Delete;

public class TransactionsDeleteHandlerTests
{
  [SetUp]
  public void SetUp()
  {
    _dbContext = DatabaseTestHelpers.GetInMemoryDbContext();

    _sut = new TransactionsDeleteHandler(_dbContext);
  }

  private TransactionsDeleteHandler _sut = null!;
  private IApplicationDbContext _dbContext = null!;

  [Test]
  public async Task Handle_ShouldCompleteCorrectly_WithExistingId()
  {
    // arrange
    EntityEntry<Transaction> transaction = await _dbContext.Transactions.AddAsync(new Transaction { Date = new DateOnly(2023, 04, 05) });

    await _dbContext.SaveChangesAsync(CancellationToken.None);

    TransactionsDeleteRequest request = new(transaction.Entity.Id);

    // act
    await _sut.Handle(request, CancellationToken.None);

    // assert
    Transaction? dbTransaction = await _dbContext.Transactions.FirstOrDefaultAsync(x => x.Id == transaction.Entity.Id);
    dbTransaction.Should().BeNull();
  }

  [Test]
  public async Task Handle_ShouldThrowNotFoundException_WithNonExistingId()
  {
    // arrange
    TransactionsDeleteRequest request = new(Guid.NewGuid());

    // act
    Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

    // assert
    await act.Should().ThrowAsync<NotFoundException>();
  }
}
