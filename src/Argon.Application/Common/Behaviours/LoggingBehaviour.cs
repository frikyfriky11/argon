namespace Argon.Application.Common.Behaviours;

/// <summary>
///   This behaviour represents a MediatR pre process behaviour that can be registered as a part of the MediatR pipeline
///   handling process.
///   This means that this behaviour will fire at every request and can decide if the request handling should continue or
///   stop.
///   In this case, we're only logging the incoming request and passing on to the next handler in the pipeline.
/// </summary>
/// <typeparam name="TRequest">The generic request object</typeparam>
/// <typeparam name="TResponse">The generic response object</typeparam>
[ExcludeFromCodeCoverage]
public class LoggingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
  private readonly ILogger _logger;

  // ReSharper disable once ContextualLoggerProblem
  // ReSharper disable once SuggestBaseTypeForParameterInConstructor
  public LoggingBehaviour(ILogger logger)
  {
    _logger = logger.ForContext<TRequest>();
  }

  public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
  {
    // log the request
    _logger.Information("Request {Name} incoming: {@Request}", typeof(TRequest).Name, request);

    // no other processing required, the next handler can fire
    return await next();
  }
}
