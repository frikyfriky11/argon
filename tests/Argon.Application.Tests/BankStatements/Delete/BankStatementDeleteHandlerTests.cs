using Argon.Application.BankStatements.Delete;

namespace Argon.Application.Tests.BankStatements.Delete;

public class BankStatementDeleteHandlerTests
{
  private IApplicationDbContext _dbContext = null!;
  private BankStatementDeleteHandler _sut = null!;

  [SetUp]
  public void SetUp()
  {
    _dbContext = DatabaseTestHelpers.GetInMemoryDbContext();

    _sut = new BankStatementDeleteHandler(_dbContext);
  }

  [Test]
  public async Task Handle_ShouldCompleteCorrectly_WithExistingId()
  {
    // arrange
    EntityEntry<BankStatement> bankStatement = await _dbContext.BankStatements.AddAsync(new BankStatement
    {
      FileName = "test file.xlsx",
      FileContent = [0x01],
      Transactions =
      [
        new Transaction
        {
          Date = new DateOnly(2023, 04, 05),
          TransactionRows =
          [
            new TransactionRow
            {
              RowCounter = 1,
              Description = "row 1",
            },
            new TransactionRow
            {
              RowCounter = 2,
              Description = "row 2",
            },
          ],
        },
      ],
    });

    await _dbContext.SaveChangesAsync(CancellationToken.None);

    BankStatementDeleteRequest request = new(bankStatement.Entity.Id);

    // act
    await _sut.Handle(request, CancellationToken.None);

    // assert
    BankStatement? dbBankStatement = await _dbContext.BankStatements.FirstOrDefaultAsync(x => x.Id == bankStatement.Entity.Id);
    dbBankStatement.Should().BeNull();

    List<Transaction> dbTransactions = await _dbContext.Transactions.Where(x => x.BankStatementId == bankStatement.Entity.Id).ToListAsync();
    dbTransactions.Should().BeEmpty();
  }
}