using ValidationException = Argon.Application.Common.Exceptions.ValidationException;

namespace Argon.Application.Common.Behaviours;

/// <summary>
///   This behaviour represents a MediatR pre process behaviour that can be registered as a part of the MediatR pipeline
///   handling process.
///   This means that this behaviour will fire at every request and can decide if the request handling should continue or
///   stop.
///   It validates incoming requests that have an associated <see cref="IValidator" /> implementation and throws an
///   exception if any of the validation rules fails.
/// </summary>
/// <typeparam name="TRequest">The generic request object</typeparam>
/// <typeparam name="TResponse">The generic response object</typeparam>
[ExcludeFromCodeCoverage]
public class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
  private readonly IEnumerable<IValidator<TRequest>> _validators;

  /// <summary>
  ///   Constructs a new ValidationBehaviour that accepts <see cref="IValidator" /> implementations.
  /// </summary>
  /// <param name="validators"><see cref="IValidator" /> implementations are passed from the Dependency Injection container</param>
  public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators)
  {
    _validators = validators;
  }

  public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
  {
    // if there are no validators, we can skip this behaviour
    if (!_validators.Any())
    {
      return await next();
    }

    // create a new validation context for the current request
    ValidationContext<TRequest> context = new(request);

    // fire all the validators asynchronously
    ValidationResult[] validationResults = await Task.WhenAll(
      _validators.Select(v =>
        v.ValidateAsync(context, cancellationToken)));

    // flatten the validation results
    List<ValidationFailure> failures = validationResults
      .Where(r => r.Errors.Any())
      .SelectMany(r => r.Errors)
      .ToList();

    if (failures.Any())
    {
      // at least one validation rule failed, raise an exception
      throw new ValidationException(failures);
    }

    // no errors occured, continue processing the pipeline
    return await next();
  }
}
