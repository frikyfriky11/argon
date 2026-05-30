using FluentValidation.Results;
using ValidationException = Argon.Application.Common.Exceptions.ValidationException;

namespace Argon.Application.Tests.Common.Exceptions;

public class ValidationExceptionTests
{
  [Test]
  public void Constructor_ShouldExposeEmptyErrors_WhenParameterless()
  {
    // act
    ValidationException exception = new();

    // assert
    exception.Errors.Should().BeEmpty();
    exception.Message.Should().Be("One or more validation failures have occurred.");
  }

  [Test]
  public void Constructor_ShouldGroupFailuresByPropertyName()
  {
    // arrange
    List<ValidationFailure> failures = new()
    {
      new ValidationFailure("Name", "Name is required"),
      new ValidationFailure("Name", "Name is too short"),
      new ValidationFailure("Age", "Age is invalid"),
    };

    // act
    ValidationException exception = new(failures);

    // assert
    exception.Errors.Should().HaveCount(2);
    exception.Errors["Name"].Should().BeEquivalentTo("Name is required", "Name is too short");
    exception.Errors["Age"].Should().BeEquivalentTo("Age is invalid");
  }
}
