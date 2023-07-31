using Argon.Application.Transactions;

namespace Argon.Application.Tests.Transactions;

[TestFixture]
public class TransactionsDeleteRequestHandlerTests
{
  [SetUp]
  public void SetUp()
  {
    _dbContext = DatabaseTestHelpers.GetInMemoryDbContext();

    _sut = new TransactionsDeleteRequestHandler(_dbContext);
  }

  private TransactionsDeleteRequestHandler _sut = null!;
  private IApplicationDbContext _dbContext = null!;

  [Test]
  public async Task Handle_ShouldCompleteCorrectly_WithExistingId()
  {
    // arrange
    EntityEntry<Transaction> transaction = await _dbContext.Transactions.AddAsync(new Transaction { Date = new DateOnly(2023, 04, 05), Description = "test description" });

    await _dbContext.SaveChangesAsync(CancellationToken.None);

    TransactionsDeleteRequest request = new(transaction.Entity.Id);

    // act
    await _sut.Handle(request, CancellationToken.None);

    // assert
    bool entityExists = await _dbContext.Transactions.AnyAsync(x => x.Id == transaction.Entity.Id);
    entityExists.Should().BeFalse();
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
