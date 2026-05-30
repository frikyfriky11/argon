using Argon.Application.BankStatements.Parse.Parsers;

namespace Argon.Application.Tests.BankStatements.Parse.Parsers;

public class ParsersFactoryTests
{
  [Test]
  public async Task CreateParserAsync_ShouldReturnTheParser_WhoseParserIdMatches()
  {
    // arrange
    StubParser first = new(Guid.NewGuid());
    StubParser second = new(Guid.NewGuid());
    ParsersFactory factory = new(new IParser[] { first, second });

    // act
    IParser result = await factory.CreateParserAsync(second.ParserId);

    // assert
    result.Should().BeSameAs(second);
  }

  [Test]
  public async Task CreateParserAsync_ShouldThrowArgumentException_WhenParserIdIsUnknown()
  {
    // arrange
    Guid unknownId = Guid.NewGuid();
    ParsersFactory factory = new(new IParser[] { new StubParser(Guid.NewGuid()) });

    // act
    Func<Task> act = async () => await factory.CreateParserAsync(unknownId);

    // assert
    await act.Should().ThrowAsync<ArgumentException>().WithMessage($"*{unknownId}*");
  }

  private sealed class StubParser(Guid parserId) : IParser
  {
    public Guid ParserId { get; } = parserId;
    public string ParserDisplayName => "Stub";

    public Task<List<BankStatementItem>> ParseAsync(Stream file) => Task.FromResult(new List<BankStatementItem>());
  }
}
