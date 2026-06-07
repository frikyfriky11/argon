using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Argon.Cli.Output;

namespace Argon.Cli.Tests.Output;

[NonParallelizable]
public class OutputFormatterTests
{
  [Test]
  public void WriteJson_ShouldFlattenRawImportDataFields_OntoEnclosingObject()
  {
    // arrange
    object payload = new
    {
      id = Guid.NewGuid(),
      rawImportData = JsonSerializer.Serialize(new
      {
        Amount = 12.34m,
        CurrencyDate = "2024-01-02",
        RawDescription = "AMAZON EU SARL",
        CounterpartyName = "Amazon",
      }),
    };

    using StringWriter writer = new();

    // act
    OutputFormatter.Write(payload, OutputFormat.Json, writer);

    // assert
    JsonNode node = JsonNode.Parse(writer.ToString())!;
    node["amount"]!.GetValue<decimal>().Should().Be(12.34m);
    node["currencyDate"]!.GetValue<string>().Should().Be("2024-01-02");
    node["rawDescription"]!.GetValue<string>().Should().Be("AMAZON EU SARL");
    node["counterpartyName"]!.GetValue<string>().Should().Be("Amazon");
    node["rawImportData"]!.GetValue<string>().Should().NotBeNullOrEmpty(
      "the original string is preserved for fidelity");
  }

  [Test]
  public void WriteJson_ShouldFlattenRawImportData_WhenInsideArrayItems()
  {
    object[] payload =
    {
      new
      {
        id = Guid.NewGuid(),
        rawImportData = JsonSerializer.Serialize(new { Amount = 1m }),
      },
      new
      {
        id = Guid.NewGuid(),
        rawImportData = JsonSerializer.Serialize(new { Amount = 2m }),
      },
    };

    using StringWriter writer = new();

    OutputFormatter.Write(payload, OutputFormat.Json, writer);

    JsonArray array = JsonNode.Parse(writer.ToString())!.AsArray();
    array[0]!["amount"]!.GetValue<decimal>().Should().Be(1m);
    array[1]!["amount"]!.GetValue<decimal>().Should().Be(2m);
  }

  [Test]
  public void WriteJson_ShouldNotOverwriteExistingFields_OnConflict()
  {
    object payload = new
    {
      amount = 99m,
      rawImportData = JsonSerializer.Serialize(new { Amount = 1m }),
    };

    using StringWriter writer = new();

    OutputFormatter.Write(payload, OutputFormat.Json, writer);

    JsonNode node = JsonNode.Parse(writer.ToString())!;
    node["amount"]!.GetValue<decimal>().Should().Be(99m,
      "existing top-level fields take precedence over flattened ones");
  }

  [Test]
  public void WriteJson_ShouldLeaveRawImportDataAlone_WhenItIsNotValidJson()
  {
    object payload = new
    {
      id = Guid.NewGuid(),
      rawImportData = "this is not json",
    };

    using StringWriter writer = new();

    OutputFormatter.Write(payload, OutputFormat.Json, writer);

    JsonNode node = JsonNode.Parse(writer.ToString())!;
    node["rawImportData"]!.GetValue<string>().Should().Be("this is not json");
  }

  [Test]
  public void WriteJson_ShouldBeAStraightSerialization_WhenNoRawImportDataIsPresent()
  {
    object payload = new { id = 1, name = "x" };

    using StringWriter writer = new();

    OutputFormatter.Write(payload, OutputFormat.Json, writer);

    JsonNode node = JsonNode.Parse(writer.ToString())!;
    node["id"]!.GetValue<int>().Should().Be(1);
    node["name"]!.GetValue<string>().Should().Be("x");
  }

  [Test]
  public void WriteJson_ShouldAddAccountTypeName_NextToNumericAccountType()
  {
    object payload = new
    {
      id = Guid.NewGuid(),
      name = "Sparkasse",
      accountType = 0,
    };

    using StringWriter writer = new();

    OutputFormatter.Write(payload, OutputFormat.Json, writer);

    JsonNode node = JsonNode.Parse(writer.ToString())!;
    node["accountType"]!.GetValue<int>().Should().Be(0,
      "the numeric value is kept so existing consumers keep working");
    node["accountTypeName"]!.GetValue<string>().Should().Be("Cash");
  }

  [Test]
  public void WriteJson_ShouldAddAccountTypeName_InsideArrayItems()
  {
    object[] payload =
    {
      new { id = Guid.NewGuid(), accountType = 1 },
      new { id = Guid.NewGuid(), accountType = 2 },
    };

    using StringWriter writer = new();

    OutputFormatter.Write(payload, OutputFormat.Json, writer);

    JsonArray array = JsonNode.Parse(writer.ToString())!.AsArray();
    array[0]!["accountTypeName"]!.GetValue<string>().Should().Be("Expense");
    array[1]!["accountTypeName"]!.GetValue<string>().Should().Be("Revenue");
  }

  [Test]
  public void WriteJson_ShouldLeaveAccountTypeAlone_WhenValueIsUnknown()
  {
    object payload = new { id = Guid.NewGuid(), accountType = 99 };

    using StringWriter writer = new();

    OutputFormatter.Write(payload, OutputFormat.Json, writer);

    JsonNode node = JsonNode.Parse(writer.ToString())!;
    node["accountType"]!.GetValue<int>().Should().Be(99);
    node["accountTypeName"].Should().BeNull();
  }

  [Test]
  public void WriteJson_ShouldAddAccountTypeName_FromTypeField_OnAccountResponses()
  {
    // The NSwag-generated account DTOs serialise the AccountType property as "type",
    // not "accountType" — make sure the helper still annotates them.
    object payload = new { id = Guid.NewGuid(), name = "Sparkasse", type = 0 };

    using StringWriter writer = new();

    OutputFormatter.Write(payload, OutputFormat.Json, writer);

    JsonNode node = JsonNode.Parse(writer.ToString())!;
    node["type"]!.GetValue<int>().Should().Be(0, "the numeric value stays for consumers that rely on it");
    node["accountTypeName"]!.GetValue<string>().Should().Be("Cash");
  }

  [Test]
  public void WriteJson_ShouldPreferAccountType_WhenBothAccountTypeAndTypeArePresent()
  {
    object payload = new { id = Guid.NewGuid(), accountType = 1, type = 0 };

    using StringWriter writer = new();

    OutputFormatter.Write(payload, OutputFormat.Json, writer);

    JsonNode node = JsonNode.Parse(writer.ToString())!;
    node["accountTypeName"]!.GetValue<string>().Should().Be("Expense",
      "when both fields are present the explicit `accountType` wins over the generic `type`");
  }

  [Test]
  public void WriteJson_ShouldLeaveTypeAlone_WhenItIsNotAnIntegerInTheAccountTypeRange()
  {
    object payload = new { id = Guid.NewGuid(), type = "string-value" };

    using StringWriter writer = new();

    OutputFormatter.Write(payload, OutputFormat.Json, writer);

    JsonNode node = JsonNode.Parse(writer.ToString())!;
    node["type"]!.GetValue<string>().Should().Be("string-value");
    node["accountTypeName"].Should().BeNull(
      "non-integer or out-of-range `type` fields stay untouched so we don't falsely tag unrelated objects");
  }

  // ----- CSV output -----

  [Test]
  public void WriteCsv_ShouldEmitHeaderAndRow_ForASingleObject()
  {
    object payload = new { id = 1, name = "Cash" };

    using StringWriter writer = new();

    OutputFormatter.Write(payload, OutputFormat.Csv, writer);

    string[] lines = writer.ToString().TrimEnd().Split(Environment.NewLine);
    lines.Should().HaveCount(2);
    lines[0].Should().Be("id,name");
    lines[1].Should().Be("1,Cash");
  }

  [Test]
  public void WriteCsv_ShouldEmitOneRowPerItem_ForACollection()
  {
    object payload = new[]
    {
      new { id = 1, name = "Cash" },
      new { id = 2, name = "Groceries" },
      new { id = 3, name = "Salary" },
    };

    using StringWriter writer = new();

    OutputFormatter.Write(payload, OutputFormat.Csv, writer);

    string[] lines = writer.ToString().TrimEnd().Split(Environment.NewLine);
    lines.Should().HaveCount(4, "1 header + 3 data rows");
    lines[0].Should().Be("id,name");
    lines[1].Should().Be("1,Cash");
    lines[2].Should().Be("2,Groceries");
    lines[3].Should().Be("3,Salary");
  }

  [Test]
  public void WriteCsv_ShouldEmitNothing_ForAnEmptyCollection()
  {
    object payload = Array.Empty<object>();

    using StringWriter writer = new();

    OutputFormatter.Write(payload, OutputFormat.Csv, writer);

    writer.ToString().Should().BeEmpty(
      "no header is emitted when there are no rows — there's no first row to reflect on");
  }

  [Test]
  public void WriteCsv_ShouldQuoteAndEscape_ValuesContainingSpecialCharacters()
  {
    object payload = new[]
    {
      new { id = 1, description = "Lidl, Bozen" },
      new { id = 2, description = "She said \"hi\"" },
      new { id = 3, description = "line one\nline two" },
      new { id = 4, description = "carriage\rreturn" },
    };

    using StringWriter writer = new();

    OutputFormatter.Write(payload, OutputFormat.Csv, writer);

    string body = writer.ToString();
    body.Should().Contain("\"Lidl, Bozen\"", "commas force quoting");
    body.Should().Contain("\"She said \"\"hi\"\"\"", "internal double-quotes are escaped by doubling");
    body.Should().Contain("\"line one\nline two\"", "newlines force quoting");
    body.Should().Contain("\"carriage\rreturn\"", "carriage returns force quoting");
  }

  [Test]
  public void WriteCsv_ShouldNotQuote_ValuesWithoutSpecialCharacters()
  {
    object payload = new { id = 1, name = "PlainText" };

    using StringWriter writer = new();

    OutputFormatter.Write(payload, OutputFormat.Csv, writer);

    writer.ToString().Should().NotContain("\"",
      "values that contain none of [, \" \\n \\r] are emitted verbatim");
  }

  // ----- WriteTable edges -----

  [Test]
  public void WriteTable_ShouldPrintNoResultsMarker_ForAnEmptyCollection()
  {
    object payload = Array.Empty<object>();

    using StringWriter writer = new();

    OutputFormatter.Write(payload, OutputFormat.Table, writer);

    writer.ToString().Trim().Should().Be("(no results)");
  }

  [Test]
  public void WriteTable_ShouldFallBackToJson_WhenTheFirstRowHasNoScalarProperties()
  {
    // arrays + nested objects aren't scalar, so the table renderer has no columns to draw
    object payload = new { children = new[] { 1, 2, 3 } };

    using StringWriter writer = new();

    OutputFormatter.Write(payload, OutputFormat.Table, writer);

    string output = writer.ToString().Trim();
    output.Should().StartWith("{").And.EndWith("}");
    JsonNode node = JsonNode.Parse(output)!;
    node["children"]!.AsArray().Count.Should().Be(3);
  }

  // ----- Render scalar types -----

  [Test]
  public void Render_ShouldFormatAllScalarTypes_ViaPredictableInvariantFormats()
  {
    // Exercises Render through the CSV path; CSV emits raw Render output (no padding)
    // so we can assert on exact strings. The thread is pinned to it-IT (comma decimal
    // separator) for the duration of the test so a regression to CurrentCulture
    // formatting would surface as commas inside numeric CSV cells.
    CultureInfo previous = CultureInfo.CurrentCulture;
    CultureInfo.CurrentCulture = new CultureInfo("it-IT");
    try
    {
      DateTimeOffset dto = new(2024, 3, 14, 9, 30, 0, TimeSpan.Zero);
      DateTime dt = new(2024, 3, 14, 9, 30, 0, DateTimeKind.Utc);
      object payload = new[]
      {
        new
        {
          d = new DateOnly(2024, 3, 14),
          dto,
          dt,
          m = 12.5m,
          f = 1.5f,
          dbl = 2.75d,
          bTrue = true,
          bFalse = false,
        },
      };

      using StringWriter writer = new();

      OutputFormatter.Write(payload, OutputFormat.Csv, writer);

      string body = writer.ToString();
      body.Should().Contain("2024-03-14", "DateOnly uses yyyy-MM-dd");
      body.Should().Contain("2024-03-14 09:30:00Z", "DateTimeOffset uses the 'u' format");
      body.Should().Contain(",12.5,", "decimal trims trailing zeros via 0.## and stays dot-separated under it-IT");
      body.Should().Contain(",1.5,", "float uses 0.## under InvariantCulture");
      body.Should().Contain(",2.75,", "double uses 0.## under InvariantCulture");
      body.Should().Contain(",true,", "bool true → \"true\"");
      body.Should().Contain(",false", "bool false → \"false\"");
    }
    finally
    {
      CultureInfo.CurrentCulture = previous;
    }
  }

  [Test]
  public void Render_ShouldEmitEmptyString_ForNullScalarValues()
  {
    object payload = new[]
    {
      new { id = 1, optional = (string?)null },
    };

    using StringWriter writer = new();

    OutputFormatter.Write(payload, OutputFormat.Csv, writer);

    string[] lines = writer.ToString().TrimEnd().Split(Environment.NewLine);
    lines[1].Should().Be("1,",
      "a null cell renders as the empty string between the comma and the line end");
  }
}
