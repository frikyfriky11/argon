using Argon.Cli.Generated;

namespace Argon.Cli.Tests.Exceptions;

public class ProgramFormatApiExceptionTests
{
  private static readonly IReadOnlyDictionary<string, IEnumerable<string>> EmptyHeaders =
    new Dictionary<string, IEnumerable<string>>();

  [Test]
  public void FormatApiException_ShouldUseInnerMessageWithStatus_WhenResponseBodyIsEmpty()
  {
    // arrange
    ApiException ex = new("upstream said no", 502, response: "", headers: EmptyHeaders, innerException: null!);

    // act
    string formatted = Argon.Cli.Program.FormatApiException(ex);

    // assert
    formatted.Should().StartWith("API error (502):");
    formatted.Should().Contain("upstream said no");
  }

  [Test]
  public void FormatApiException_ShouldRenderValidationErrors_WhenResponseHasAnErrorsObject()
  {
    // arrange
    string body =
      """
      {
        "errors": {
          "name": ["The name is required."],
          "type": ["Must be a known account type.", "Cannot be empty."]
        }
      }
      """;
    ApiException ex = new("validation", 400, body, headers: EmptyHeaders, innerException: null!);

    // act
    string formatted = Argon.Cli.Program.FormatApiException(ex);

    // assert
    formatted.Should().StartWith("validation failed (400):");
    formatted.Should().Contain("name: The name is required.");
    formatted.Should().Contain("type: Must be a known account type.");
    formatted.Should().Contain("type: Cannot be empty.");
  }

  [Test]
  public void FormatApiException_ShouldRenderTheTitle_WhenResponseIsAProblemDetails()
  {
    // arrange
    string body =
      """
      {
        "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
        "title": "Account not found",
        "status": 404
      }
      """;
    ApiException ex = new("not found", 404, body, headers: EmptyHeaders, innerException: null!);

    // act
    string formatted = Argon.Cli.Program.FormatApiException(ex);

    // assert
    formatted.Should().Be("API error (404): Account not found");
  }

  [Test]
  public void FormatApiException_ShouldFallBackToTheRawResponse_WhenItIsNotValidJson()
  {
    // arrange
    ApiException ex = new("oops", 500, "<html>uh oh</html>", headers: EmptyHeaders, innerException: null!);

    // act
    string formatted = Argon.Cli.Program.FormatApiException(ex);

    // assert
    formatted.Should().Be("API error (500): <html>uh oh</html>");
  }

  [Test]
  public void FormatApiException_ShouldFallBackToTheRawResponse_WhenItIsValidJsonWithoutErrorsOrTitle()
  {
    // arrange
    string body = """{"foo": "bar"}""";
    ApiException ex = new("oops", 500, body, headers: EmptyHeaders, innerException: null!);

    // act
    string formatted = Argon.Cli.Program.FormatApiException(ex);

    // assert
    formatted.Should().Be($"API error (500): {body}");
  }

  [Test]
  public void FormatApiException_ShouldUseRawResponseString_WhenResponseIsWhitespace()
  {
    // arrange
    ApiException ex = new("nope", 504, "   \n  ", headers: EmptyHeaders, innerException: null!);

    // act
    string formatted = Argon.Cli.Program.FormatApiException(ex);

    // assert
    formatted.Should().StartWith("API error (504):");
    formatted.Should().Contain("nope",
      "an all-whitespace response is treated as empty and the inner message is used");
  }
}
