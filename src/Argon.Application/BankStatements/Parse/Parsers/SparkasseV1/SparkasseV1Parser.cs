using Argon.Application.BankStatements.Parse.Parsers.SparkasseV1.CardPayments;
using Argon.Application.BankStatements.Parse.Parsers.SparkasseV1.Cash;
using Argon.Application.BankStatements.Parse.Parsers.SparkasseV1.Emoluments;
using Argon.Application.BankStatements.Parse.Parsers.SparkasseV1.IncomingBankTransfers;
using Argon.Application.BankStatements.Parse.Parsers.SparkasseV1.Loans;
using Argon.Application.BankStatements.Parse.Parsers.SparkasseV1.OutgoingBankTransfers;
using Argon.Application.BankStatements.Parse.Parsers.SparkasseV1.Sepa;
using Argon.Application.BankStatements.Parse.Parsers.SparkasseV1.Taxes;
using ClosedXML.Excel;
using Microsoft.Extensions.Logging;

namespace Argon.Application.BankStatements.Parse.Parsers.SparkasseV1;

[UsedImplicitly]
public class SparkasseV1Parser(ILogger<SparkasseV1Parser> logger) : IParser
{
  private readonly List<ILineParser> _lineParsers =
  [
    new DebitCardPaymentLineParser(),
    new DrScharEmolumentLineParser(),
    new AziendaSanitariaEmolumentLineParser(),
    new OutgoingBankTransferLineParser(),
    new BancomatCardPaymentLineParser(),
    new BankFeeLineParser(),
    new StampDutyLineParser(),
    new DirectDebitLineParser(),
    new IncomingBankTransferLineParser(),
    new LoanPaymentLineParser(),
    new WithdrawalLineParser(),
  ];

  public Guid ParserId => Guid.Parse("17b55c47-0557-47a8-bfc5-c305534bf2ed");
  public string ParserDisplayName => "Sparkasse V1";

  public Task<List<BankStatementItem>> ParseAsync(Stream file)
  {
    logger.LogDebug("Reading stream as an Excel workbook");
    using XLWorkbook workbook = new(file);

    logger.LogDebug("Reading first worksheet");
    IXLWorksheet? worksheet = workbook.Worksheets.FirstOrDefault();

    if (worksheet is null) throw new Exception("No worksheet found");

    var result = new List<BankStatementItem>();
    int total = 0;
    int errors = 0;

    logger.LogDebug("Iterating over rows of autofilter enabled cells, skipping first row as header row");
    foreach (IXLRangeRow? row in worksheet.AutoFilter.VisibleRows.Skip(1))
    {
      total++;

      DateOnly accountingDate = DateOnly.FromDateTime(row.Cell(1).Value.GetDateTime());
      DateOnly currencyDate = DateOnly.FromDateTime(row.Cell(2).Value.GetDateTime());
      string? rawDescription = row.Cell(3).Value.GetText();
      decimal amount = (decimal)row.Cell(4).Value.GetNumber();
      string rawInput = $"{accountingDate}~{currencyDate}~{amount}~{rawDescription}";

      logger.LogDebug(
        "Accounting date is {AccountingDate}, currency date is {CurrencyDate}, amount is {Amount} and raw description is {RawDescription}",
        accountingDate, currencyDate, amount, rawDescription);

      BankStatementItem resultItem = new()
      {
        RawInput = rawInput,
      };

      try
      {
        logger.LogDebug("Trying to parse line into concrete object");
        BaseItem item = ParseLine(accountingDate, currencyDate, rawDescription, amount);
        logger.LogInformation("Parsed line into {ObjectType} object with content {ObjectContent}", item.GetType(),
          item);

        resultItem.Amount = item.Amount;
        resultItem.Date = item.CurrencyDate;
        resultItem.CounterpartyName = item.CounterpartyName;
        resultItem.SpecificParsedItem = item;
      }
      catch (NotSupportedException ex)
      {
        logger.LogInformation(ex, "Unable to parse line into concrete object");

        resultItem.ErrorMessage = $"Parsing not supported: {ex.Message}";
        resultItem.RawInput = $"{accountingDate}~{currencyDate}~{amount}~{rawDescription}";

        errors++;
      }

      result.Add(resultItem);
    }

    logger.LogInformation("Parsing of {TotalRowsCount} rows completed with {TotalErrorsCount} errors", total, errors);

    return Task.FromResult(result);
  }

  private BaseItem ParseLine(
    DateOnly accountingDate,
    DateOnly currencyDate,
    string rawDescription,
    decimal amount)
  {
    ILineParser? parser = _lineParsers.FirstOrDefault(p => p.CanParse(rawDescription));

    if (parser is null) throw new NotSupportedException($"{rawDescription} is not a known statement description");

    return parser.Parse(accountingDate, currencyDate, rawDescription, amount);
  }
}