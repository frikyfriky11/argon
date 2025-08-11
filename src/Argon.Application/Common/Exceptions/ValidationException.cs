namespace Argon.Application.Common.Exceptions;

/// <summary>
///   Represents an exception that can be raised when the request object failed one or more validation rules.
///   This will be handled by the ASP.NET Core pipeline returning an HTTP 400 result.
/// </summary>
public class ValidationException() : Exception("One or more validation failures have occurred.")
{
  public ValidationException(IEnumerable<ValidationFailure> failures) : this()
  {
    Errors = failures
      .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
      .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());
  }

  public IDictionary<string, string[]> Errors { get; } = new Dictionary<string, string[]>();
}
