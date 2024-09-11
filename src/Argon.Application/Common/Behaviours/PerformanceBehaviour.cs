namespace Argon.Application.Common.Behaviours;

/// <summary>
///   This behaviour represents a MediatR pre process behaviour that can be registered as a part of the MediatR pipeline
///   handling process.
///   This means that this behaviour will fire at every request and can decide if the request handling should continue or
///   stop.
///   It tracks how long a request takes, and if it exceeds a certain threshold, it logs a warning.
/// </summary>
/// <typeparam name="TRequest">The generic request object</typeparam>
/// <typeparam name="TResponse">The generic response object</typeparam>
[ExcludeFromCodeCoverage]
public class PerformanceBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
  private readonly ILogger _logger;
  private readonly Stopwatch _timer;

  // ReSharper disable once ContextualLoggerProblem
  // ReSharper disable once SuggestBaseTypeForParameterInConstructor
  public PerformanceBehaviour(ILogger logger)
  {
    _timer = new Stopwatch();

    _logger = logger.ForContext<TRequest>();
  }

  public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
  {
    // start the timer
    _timer.Start();

    // execute the request
    TResponse response = await next();

    // stop the timer
    _timer.Stop();

    long elapsedMilliseconds = _timer.ElapsedMilliseconds;

    // if the request exceeded the threshold, log a warning
    if (elapsedMilliseconds > 500)
    {
      // log the request as a long running one
      _logger.Information("Request {Name} incoming took {ElapsedMilliseconds} milliseconds: {@Request}",
        typeof(TRequest).Name, elapsedMilliseconds, request);
    }

    return response;
  }
}
