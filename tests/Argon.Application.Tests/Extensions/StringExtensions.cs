namespace Argon.Application.Tests.Extensions;

/// <summary>
///   Useful string extension methods
/// </summary>
public static class StringExtensions
{
  /// <summary>
  ///   Repeats the provided <see cref="value" /> parameter <see cref="times" /> number of times.
  ///   Useful if you want to generate a long string for testing MaximumLength validators.
  /// </summary>
  /// <param name="value">The string to repeat</param>
  /// <param name="times">The number of times the string needs to be repeated</param>
  /// <returns>The repeated string</returns>
  public static string Repeat(this string value, int times)
  {
    // allocating right now the number of characters in the StringBuilder allows for better performance
    // it also reduces memory pressure because we know the string is of fixed length
    StringBuilder builder = new(value.Length * times);

    for (int i = 0; i < times; i++)
    {
      builder.Append(value);
    }

    return builder.ToString();
  }
}
