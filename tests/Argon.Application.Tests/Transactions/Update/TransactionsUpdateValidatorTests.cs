using Argon.Application.Tests.Extensions;
using Argon.Application.Transactions.Update;

namespace Argon.Application.Tests.Transactions.Update;

[TestFixture]
public class TransactionsUpdateValidatorTests
{
  [SetUp]
  public void SetUp()
  {
    _dbContext = DatabaseTestHelpers.GetInMemoryDbContext();

    _sut = new TransactionsUpdateValidator(_dbContext);
  }

  private TransactionsUpdateValidator _sut = null!;
  private IApplicationDbContext _dbContext = null!;

  [Test]
  public async Task Validator_ShouldReturnZeroErrors_WhenObjectIsValid()
  {
    EntityEntry<Account> accountGroceries = await _dbContext.Accounts.AddAsync(new Account { Name = "Groceries" });
    EntityEntry<Account> accountBank = await _dbContext.Accounts.AddAsync(new Account { Name = "Bank" });
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    TransactionsUpdateRequest request = new(new DateOnly(2023, 04, 05), "test description", new List<TransactionRowsUpdateRequest>
    {
      new(Guid.NewGuid(), 1, accountGroceries.Entity.Id, 100.00m, null, "test row 1 description"),
      new(Guid.NewGuid(), 2, accountBank.Entity.Id, null, 100.00m, "test row 2 description"),
    });
    TestValidationResult<TransactionsUpdateRequest>? result = await _sut.TestValidateAsync(request);

    result.ShouldNotHaveAnyValidationErrors();
  }

  [Test]
  public async Task Validator_ShouldReturnError_WhenDescriptionIsTooLong()
  {
    TransactionsUpdateRequest request = new(new DateOnly(2023, 04, 05), "x".Repeat(101), new List<TransactionRowsUpdateRequest>());

    await _sut.ShouldFailOnProperty(request, nameof(request.Description));
  }

  [Test]
  [TestCase(null)]
  [TestCase("")]
  [TestCase(" ")]
  public async Task Validator_ShouldReturnError_WhenDescriptionIsNullOrWhiteSpace(string value)
  {
    TransactionsUpdateRequest request = new(new DateOnly(2023, 04, 05), value, new List<TransactionRowsUpdateRequest>());

    await _sut.ShouldFailOnProperty(request, nameof(request.Description));
  }

  [Test]
  public async Task Validator_ShouldReturnError_WhenRowListIsEmpty()
  {
    TransactionsUpdateRequest request = new(new DateOnly(2023, 04, 05), string.Empty, new List<TransactionRowsUpdateRequest>());

    await _sut.ShouldFailOnProperty(request, nameof(request.TransactionRows));
  }

  [Test]
  public async Task Validator_ShouldReturnError_WhenRowListHasLengthLessThanTwo()
  {
    List<TransactionRowsUpdateRequest> rowList = new()
    {
      new TransactionRowsUpdateRequest(Guid.NewGuid(), 1, Guid.NewGuid(), null, null, null),
    };

    TransactionsUpdateRequest request = new(new DateOnly(2023, 04, 05), string.Empty, rowList);

    await _sut.ShouldFailOnProperty(request, nameof(request.TransactionRows));
  }

  [Test]
  public async Task Validator_ShouldReturnError_WhenRowAccountIdDoesNotExist()
  {
    TransactionsUpdateRequest request = new(new DateOnly(2023, 04, 05), string.Empty, new List<TransactionRowsUpdateRequest>
    {
      new(Guid.NewGuid(), 1, Guid.NewGuid(), null, null, null),
    });

    await _sut.ShouldFailOnProperty(request, "TransactionRows[0].AccountId");
  }

  [Test]
  public async Task Validator_ShouldReturnError_WhenRowCreditAndDebitAreNull()
  {
    TransactionsUpdateRequest request = new(new DateOnly(2023, 04, 05), string.Empty, new List<TransactionRowsUpdateRequest>
    {
      new(Guid.NewGuid(), 1, Guid.NewGuid(), null, null, null),
    });

    await _sut.ShouldFailOnProperty(request, nameof(request.TransactionRows));
  }

  [Test]
  [TestCase(100, 300, 200)]
  [TestCase(5, 40, 35)]
  public async Task Validator_ShouldReturnError_WhenRowSumIsNotZeroAndIsMissingSomeDebitAmounts(decimal debit, decimal credit, decimal missing)
  {
    TransactionsUpdateRequest request = new(new DateOnly(2023, 04, 05), string.Empty, new List<TransactionRowsUpdateRequest>
    {
      new(Guid.NewGuid(), 1, Guid.NewGuid(), debit, null, null),
      new(Guid.NewGuid(), 1, Guid.NewGuid(), null, credit, null),
    });

    (await _sut.ShouldFailOnProperty(request, nameof(request.TransactionRows)))
      .WithErrorMessage($"The sum of the transaction rows should be zero but {missing} are missing in debit amounts");
  }

  [Test]
  [TestCase(300, 100, 200)]
  [TestCase(40, 5, 35)]
  public async Task Validator_ShouldReturnError_WhenRowSumIsNotZeroAndIsMissingSomeCreditAmounts(decimal debit, decimal credit, decimal missing)
  {
    TransactionsUpdateRequest request = new(new DateOnly(2023, 04, 05), string.Empty, new List<TransactionRowsUpdateRequest>
    {
      new(Guid.NewGuid(), 1, Guid.NewGuid(), debit, null, null),
      new(Guid.NewGuid(), 1, Guid.NewGuid(), null, credit, null),
    });

    (await _sut.ShouldFailOnProperty(request, nameof(request.TransactionRows)))
      .WithErrorMessage($"The sum of the transaction rows should be zero but {missing} are missing in credit amounts");
  }

  [Test]
  [TestCase(1234567890123.0)]
  [TestCase(12345678901.230)]
  [TestCase(123456789.01230)]
  public async Task Validator_ShouldReturnError_WhenRowDebitHasInvalidPrecisionScale(decimal value)
  {
    TransactionsUpdateRequest request = new(new DateOnly(2023, 04, 05), string.Empty, new List<TransactionRowsUpdateRequest>
    {
      new(Guid.NewGuid(), 1, Guid.NewGuid(), value, null, null),
    });

    await _sut.ShouldFailOnProperty(request, "TransactionRows[0].Debit");
  }

  [Test]
  [TestCase(1234567890123.0)]
  [TestCase(12345678901.230)]
  [TestCase(123456789.01230)]
  public async Task Validator_ShouldReturnError_WhenRowCreditHasInvalidPrecisionScale(decimal value)
  {
    TransactionsUpdateRequest request = new(new DateOnly(2023, 04, 05), string.Empty, new List<TransactionRowsUpdateRequest>
    {
      new(Guid.NewGuid(), 1, Guid.NewGuid(), null, value, null),
    });

    await _sut.ShouldFailOnProperty(request, "TransactionRows[0].Credit");
  }

  [Test]
  public async Task Validator_ShouldReturnError_WhenRowDescriptionIsTooLong()
  {
    TransactionsUpdateRequest request = new(new DateOnly(2023, 04, 05), string.Empty, new List<TransactionRowsUpdateRequest>
    {
      new(Guid.NewGuid(), 1, Guid.NewGuid(), null, null, "x".Repeat(101)),
    });

    await _sut.ShouldFailOnProperty(request, "TransactionRows[0].Description");
  }
}