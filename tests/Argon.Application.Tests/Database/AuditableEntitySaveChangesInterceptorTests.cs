namespace Argon.Application.Tests.Database;

public class AuditableEntitySaveChangesInterceptorTests
{
  [SetUp]
  public void SetUp()
  {
    _dbContext = DatabaseTestHelpers.GetInMemoryDbContext();
  }

  private IApplicationDbContext _dbContext = null!;

  [Test]
  public async Task SaveChanges_ShouldStampCreatedAndLastModified_OnAddedEntities()
  {
    // arrange
    Account account = new() { Name = "Groceries" };
    await _dbContext.Accounts.AddAsync(account);

    // act
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    // assert
    account.Created.Should().Be(DatabaseTestHelpers.FixedInstant);
    account.LastModified.Should().Be(DatabaseTestHelpers.FixedInstant);
  }
}
