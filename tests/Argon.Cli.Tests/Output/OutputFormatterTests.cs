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
}
