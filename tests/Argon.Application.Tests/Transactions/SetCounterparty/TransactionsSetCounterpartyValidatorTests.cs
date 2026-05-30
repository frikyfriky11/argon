using Argon.Application.Tests.Extensions;
using Argon.Application.Transactions.SetCounterparty;

namespace Argon.Application.Tests.Transactions.SetCounterparty;

public class TransactionsSetCounterpartyValidatorTests
{
  [SetUp]
  public void SetUp()
  {
    _dbContext = DatabaseTestHelpers.GetInMemoryDbContext();

    _sut = new TransactionsSetCounterpartyValidator(_dbContext);
  }

  private IApplicationDbContext _dbContext = null!;
  private TransactionsSetCounterpartyValidator _sut = null!;

  [Test]
  public async Task Validator_ShouldReturnZeroErrors_WhenCounterpartyExists()
  {
    // arrange
    EntityEntry<Counterparty> counterparty = await _dbContext.Counterparties.AddAsync(new Counterparty { Name = "Market" });
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    TransactionsSetCounterpartyRequest request = new(counterparty.Entity.Id);

    // act
    TestValidationResult<TransactionsSetCounterpartyRequest> result = await _sut.TestValidateAsync(request);

    // assert
    result.ShouldNotHaveAnyValidationErrors();
  }

  [Test]
  public async Task Validator_ShouldReturnError_WhenCounterpartyDoesNotExist()
  {
    // arrange
    TransactionsSetCounterpartyRequest request = new(Guid.NewGuid());

    // act + assert
    await _sut.ShouldFailOnProperty(request, nameof(request.CounterpartyId));
  }
}
