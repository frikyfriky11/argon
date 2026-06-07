using Argon.Application.Common.Behaviours;
using MediatR;
using ValidationException = Argon.Application.Common.Exceptions.ValidationException;

namespace Argon.Application.Tests.Common.Behaviours;

public class ValidationBehaviourTests
{
  [Test]
  public async Task Handle_ShouldCallNext_WhenNoValidatorsAreRegistered()
  {
    // arrange
    ValidationBehaviour<TestRequest, string> behaviour = new(Array.Empty<IValidator<TestRequest>>());
    bool nextCalled = false;
    RequestHandlerDelegate<string> next = _ =>
    {
      nextCalled = true;
      return Task.FromResult("handled");
    };

    // act
    string result = await behaviour.Handle(new TestRequest(string.Empty), next, CancellationToken.None);

    // assert
    nextCalled.Should().BeTrue();
    result.Should().Be("handled");
  }

  [Test]
  public async Task Handle_ShouldCallNext_WhenValidationPasses()
  {
    // arrange
    ValidationBehaviour<TestRequest, string> behaviour = new(new IValidator<TestRequest>[] { new TestRequestValidator() });
    bool nextCalled = false;
    RequestHandlerDelegate<string> next = _ =>
    {
      nextCalled = true;
      return Task.FromResult("handled");
    };

    // act
    string result = await behaviour.Handle(new TestRequest("valid"), next, CancellationToken.None);

    // assert
    nextCalled.Should().BeTrue();
    result.Should().Be("handled");
  }

  [Test]
  public async Task Handle_ShouldThrowValidationExceptionAndNotCallNext_WhenValidationFails()
  {
    // arrange
    ValidationBehaviour<TestRequest, string> behaviour = new(new IValidator<TestRequest>[] { new TestRequestValidator() });
    bool nextCalled = false;
    RequestHandlerDelegate<string> next = _ =>
    {
      nextCalled = true;
      return Task.FromResult("handled");
    };

    // act
    Func<Task> act = async () => await behaviour.Handle(new TestRequest(string.Empty), next, CancellationToken.None);

    // assert
    await act.Should().ThrowAsync<ValidationException>();
    nextCalled.Should().BeFalse();
  }

  private record TestRequest(string Name) : IRequest<string>;

  private class TestRequestValidator : AbstractValidator<TestRequest>
  {
    public TestRequestValidator()
    {
      RuleFor(request => request.Name).NotEmpty();
    }
  }
}
