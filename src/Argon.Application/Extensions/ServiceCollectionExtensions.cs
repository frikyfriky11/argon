namespace Argon.Application.Extensions;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
  /// <summary>
  ///   Add the application services to the Dependency Injection container
  /// </summary>
  /// <param name="services">The service collection container</param>
  public static void AddApplicationServices(this IServiceCollection services)
  {
    // add all the FluentValidation validators
    services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

    // add the MediatR pipeline behaviours
    services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>));
    services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
    services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));

    // add all the MediatR handlers
    services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
  }
}
