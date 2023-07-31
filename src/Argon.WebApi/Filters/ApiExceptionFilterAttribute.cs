namespace Argon.WebApi.Filters;

/// <summary>
///   This filter handles the business logic exceptions that are raised during normal execution.
///   It also ensures that the correct response is returned using the correct HTTP status code.
/// </summary>
[UsedImplicitly]
public class ApiExceptionFilterAttribute : ExceptionFilterAttribute
{
  private readonly IDictionary<Type, Action<ExceptionContext>> _exceptionHandlers;

  public ApiExceptionFilterAttribute()
  {
    // register known exception types and handlers
    _exceptionHandlers = new Dictionary<Type, Action<ExceptionContext>>
    {
      { typeof(ValidationException), HandleValidationException },
      { typeof(NotFoundException), HandleNotFoundException },
      { typeof(UnauthorizedAccessException), HandleUnauthorizedAccessException },
      { typeof(ForbiddenAccessException), HandleForbiddenAccessException },
      { typeof(UniqueConstraintException), HandleUniqueConstraintException },
      { typeof(CannotInsertNullException), HandleCannotInsertNullException },
      { typeof(MaxLengthExceededException), HandleMaxLengthExceededException },
      { typeof(NumericOverflowException), HandleNumericOverflowException },
      { typeof(ReferenceConstraintException), HandleReferenceConstraintException },
    };
  }

  public override void OnException(ExceptionContext context)
  {
    HandleException(context);

    base.OnException(context);
  }

  private void HandleException(ExceptionContext context)
  {
    Type type = context.Exception.GetType();
    if (_exceptionHandlers.TryGetValue(type, out Action<ExceptionContext>? handler))
    {
      handler.Invoke(context);
      return;
    }

    if (!context.ModelState.IsValid)
    {
      HandleInvalidModelStateException(context);
    }
  }

  private void HandleValidationException(ExceptionContext context)
  {
    ValidationException exception = (ValidationException)context.Exception;

    ValidationProblemDetails details = new(exception.Errors)
    {
      Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
    };

    context.Result = new BadRequestObjectResult(details);

    context.ExceptionHandled = true;
  }

  private void HandleInvalidModelStateException(ExceptionContext context)
  {
    ValidationProblemDetails details = new(context.ModelState)
    {
      Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
    };

    context.Result = new BadRequestObjectResult(details);

    context.ExceptionHandled = true;
  }

  private void HandleNotFoundException(ExceptionContext context)
  {
    NotFoundException exception = (NotFoundException)context.Exception;

    ProblemDetails details = new()
    {
      Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
      Title = "The specified resource was not found.",
      Detail = exception.Message,
    };

    context.Result = new NotFoundObjectResult(details);

    context.ExceptionHandled = true;
  }

  private void HandleUnauthorizedAccessException(ExceptionContext context)
  {
    ProblemDetails details = new()
    {
      Status = StatusCodes.Status401Unauthorized,
      Title = "Unauthorized",
      Type = "https://tools.ietf.org/html/rfc7235#section-3.1",
    };

    context.Result = new ObjectResult(details)
    {
      StatusCode = StatusCodes.Status401Unauthorized,
    };

    context.ExceptionHandled = true;
  }

  private void HandleForbiddenAccessException(ExceptionContext context)
  {
    ProblemDetails details = new()
    {
      Status = StatusCodes.Status403Forbidden,
      Title = "Forbidden",
      Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
    };

    context.Result = new ObjectResult(details)
    {
      StatusCode = StatusCodes.Status403Forbidden,
    };

    context.ExceptionHandled = true;
  }

  private void HandleUniqueConstraintException(ExceptionContext context)
  {
    ProblemDetails details = new()
    {
      Status = StatusCodes.Status500InternalServerError,
      Title = "This action cannot be completed because it invalidates a uniqueness check",
      Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
    };

    context.Result = new ObjectResult(details)
    {
      StatusCode = StatusCodes.Status500InternalServerError,
    };

    context.ExceptionHandled = true;
  }

  private void HandleCannotInsertNullException(ExceptionContext context)
  {
    ProblemDetails details = new()
    {
      Status = StatusCodes.Status500InternalServerError,
      Title = "Null or empty value is not allowed in one or more fields",
      Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
    };

    context.Result = new ObjectResult(details)
    {
      StatusCode = StatusCodes.Status500InternalServerError,
    };

    context.ExceptionHandled = true;
  }

  private void HandleMaxLengthExceededException(ExceptionContext context)
  {
    ProblemDetails details = new()
    {
      Status = StatusCodes.Status500InternalServerError,
      Title = "One or more fields contain data that is too long or big",
      Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
    };

    context.Result = new ObjectResult(details)
    {
      StatusCode = StatusCodes.Status500InternalServerError,
    };

    context.ExceptionHandled = true;
  }

  private void HandleNumericOverflowException(ExceptionContext context)
  {
    ProblemDetails details = new()
    {
      Status = StatusCodes.Status500InternalServerError,
      Title = "One or more fields contain numbers that are too big",
      Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
    };

    context.Result = new ObjectResult(details)
    {
      StatusCode = StatusCodes.Status500InternalServerError,
    };

    context.ExceptionHandled = true;
  }

  private void HandleReferenceConstraintException(ExceptionContext context)
  {
    ProblemDetails details = new()
    {
      Status = StatusCodes.Status500InternalServerError,
      Title = "This action cannot be completed because one or more objects depend on this",
      Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
    };

    context.Result = new ObjectResult(details)
    {
      StatusCode = StatusCodes.Status500InternalServerError,
    };

    context.ExceptionHandled = true;
  }
}
