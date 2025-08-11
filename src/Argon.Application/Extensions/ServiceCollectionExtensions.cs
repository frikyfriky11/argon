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

    // add all the parsers
    List<Type> parsers = Assembly.GetExecutingAssembly()
      .DefinedTypes
      .Where(t => typeof(IParser).IsAssignableFrom(t) && t is { IsInterface: false, IsAbstract: false })
      .Select(t => t.AsType())
      .ToList();

    foreach (Type parser in parsers) services.AddScoped(typeof(IParser), parser);

    // add all the factories
    services.AddScoped<IParsersFactory, ParsersFactory>();
  }
}