using System.Text.Json;
using System.Text.Json.Nodes;
using Argon.Cli.Output;

namespace Argon.Cli.Tests.Output;

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
}
