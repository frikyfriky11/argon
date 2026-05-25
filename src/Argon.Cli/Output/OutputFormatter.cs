using System.Collections;
using System.Reflection;

namespace Argon.Cli.Output;

public static class OutputFormatter
{
  private static readonly JsonSerializerOptions JsonOptions = new()
  {
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
  };

  public static void Write(object? value, OutputFormat format, TextWriter? writer = null)
  {
    writer ??= Console.Out;

    switch (format)
    {
      case OutputFormat.Json:
        WriteJson(value, writer);
        return;
      case OutputFormat.Csv:
        WriteCsv(value, writer);
        return;
      case OutputFormat.Table:
      default:
        WriteTable(value, writer);
        return;
    }
  }

  private static void WriteJson(object? value, TextWriter writer)
  {
    writer.WriteLine(JsonSerializer.Serialize(value, JsonOptions));
  }

  private static IReadOnlyList<object> AsRowList(object? value)
  {
    if (value is null)
    {
      return Array.Empty<object>();
    }

    if (value is string)
    {
      return new[] { value };
    }

    if (value is IEnumerable enumerable && value is not IDictionary)
    {
      List<object> items = new();
      foreach (object? item in enumerable)
      {
        if (item is not null)
        {
          items.Add(item);
        }
      }

      return items;
    }

    return new[] { value };
  }

  private static void WriteTable(object? value, TextWriter writer)
  {
    IReadOnlyList<object> rows = AsRowList(value);
    if (rows.Count == 0)
    {
      writer.WriteLine("(no results)");
      return;
    }

    PropertyInfo[] props = rows[0].GetType()
      .GetProperties(BindingFlags.Public | BindingFlags.Instance)
      .Where(p => p.GetIndexParameters().Length == 0 && IsScalar(p.PropertyType))
      .ToArray();

    if (props.Length == 0)
    {
      // fall back to json for objects without scalar properties
      WriteJson(value, writer);
      return;
    }

    string[] headers = props.Select(p => p.Name).ToArray();
    string[][] body = rows
      .Select(row => props.Select(p => Render(p.GetValue(row))).ToArray())
      .ToArray();

    int[] widths = headers
      .Select((h, i) => Math.Max(h.Length, body.Select(r => r[i].Length).DefaultIfEmpty(0).Max()))
      .ToArray();

    writer.WriteLine(string.Join("  ", headers.Select((h, i) => h.PadRight(widths[i]))));
    writer.WriteLine(string.Join("  ", widths.Select(w => new string('-', w))));
    foreach (string[] row in body)
    {
      writer.WriteLine(string.Join("  ", row.Select((c, i) => c.PadRight(widths[i]))));
    }
  }

  private static void WriteCsv(object? value, TextWriter writer)
  {
    IReadOnlyList<object> rows = AsRowList(value);
    if (rows.Count == 0)
    {
      return;
    }

    PropertyInfo[] props = rows[0].GetType()
      .GetProperties(BindingFlags.Public | BindingFlags.Instance)
      .Where(p => p.GetIndexParameters().Length == 0 && IsScalar(p.PropertyType))
      .ToArray();

    writer.WriteLine(string.Join(",", props.Select(p => Escape(p.Name))));
    foreach (object row in rows)
    {
      writer.WriteLine(string.Join(",", props.Select(p => Escape(Render(p.GetValue(row))))));
    }
  }

  private static string Render(object? value) => value switch
  {
    null => "",
    DateOnly d => d.ToString("yyyy-MM-dd"),
    DateTimeOffset dto => dto.ToString("u"),
    DateTime dt => dt.ToString("u"),
    decimal m => m.ToString("0.##"),
    double d => d.ToString("0.##"),
    float f => f.ToString("0.##"),
    bool b => b ? "true" : "false",
    _ => value.ToString() ?? "",
  };

  private static string Escape(string field)
  {
    if (field.IndexOfAny(new[] { ',', '"', '\n', '\r' }) < 0)
    {
      return field;
    }

    return $"\"{field.Replace("\"", "\"\"")}\"";
  }

  private static bool IsScalar(Type type)
  {
    Type t = Nullable.GetUnderlyingType(type) ?? type;
    if (t.IsPrimitive || t.IsEnum)
    {
      return true;
    }

    return t == typeof(string)
           || t == typeof(decimal)
           || t == typeof(Guid)
           || t == typeof(DateOnly)
           || t == typeof(DateTime)
           || t == typeof(DateTimeOffset)
           || t == typeof(TimeSpan);
  }
}
