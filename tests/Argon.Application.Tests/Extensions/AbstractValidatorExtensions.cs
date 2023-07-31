namespace Argon.Application.Tests.Extensions;

/// <summary>
///   Useful AbstractValidator extension methods
/// </summary>
public static class AbstractValidatorExtensions
{
  /// <summary>
  ///   Tests if the provided <see cref="propertyName" /> caused a validation error on the supplied <see cref="obj" />.
  ///   If it caused a validation error, it asserts a failure.
  ///   Please note that this methods skips other properties and focuses only on the provided one.
  /// </summary>
  /// <param name="validator">The validator to use</param>
  /// <param name="obj">The object to validate</param>
  /// <param name="propertyName">The property name to check for</param>
  /// <typeparam name="TValidator">A class implementing <see cref="AbstractValidator{T}" /></typeparam>
  /// <typeparam name="TObject">An object to validate</typeparam>
  public static async Task<ITestValidationWith> ShouldFailOnProperty<TValidator, TObject>(
    this TValidator validator,
    TObject obj,
    string propertyName)
    where TValidator : AbstractValidator<TObject>
  {
    TestValidationResult<TObject>? result = await validator.TestValidateAsync(obj);

    return result.ShouldHaveValidationErrorFor(propertyName);
  }

  /// <summary>
  ///   Tests if the provided <see cref="propertyName" /> does not cause a validation error on the supplied
  ///   <see cref="obj" />.
  ///   If it didn't cause a validation error, it asserts a pass.
  ///   Please note that this methods skips other properties and focuses only on the provided one.
  /// </summary>
  /// <param name="validator">The validator to use</param>
  /// <param name="obj">The object to validate</param>
  /// <param name="propertyName">The property name to check for</param>
  /// <typeparam name="TValidator">A class implementing <see cref="AbstractValidator{T}" /></typeparam>
  /// <typeparam name="TObject">An object to validate</typeparam>
  public static async Task ShouldNotFailOnProperty<TValidator, TObject>(
    this TValidator validator,
    TObject obj,
    string propertyName)
    where TValidator : AbstractValidator<TObject>
  {
    TestValidationResult<TObject>? result = await validator.TestValidateAsync(obj);

    result.ShouldNotHaveValidationErrorFor(propertyName);
  }
}
