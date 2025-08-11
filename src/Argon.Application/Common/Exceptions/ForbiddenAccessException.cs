namespace Argon.Application.Common.Exceptions;

/// <summary>
///   Represents an exception that can be raised when the user should not access the resource.
///   This will be handled by the ASP.NET Core pipeline returning an HTTP 403 result.
/// </summary>
[ExcludeFromCodeCoverage]
public class ForbiddenAccessException : Exception
{
}
