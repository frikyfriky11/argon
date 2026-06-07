namespace Argon.Cli.Output;

/// <summary>
///   Shared rendering of the pagination footer for the list verbs (tx/cp/cpi).
///   In table mode it writes the page summary to stdout and, when more results
///   exist beyond the current page, a hint to fetch them all. In json/csv mode the
///   "more results" hint goes to <b>stderr</b> so it never corrupts the machine-
///   readable stdout a `| jq` pipeline is consuming — this is what stops a caller
///   from concluding "it doesn't exist" when it was merely on page 2.
/// </summary>
public static class PaginationFooter
{
  public static void Write(
    OutputFormat format,
    int pageNumber,
    int totalPages,
    int totalCount,
    int shownCount,
    bool hasNextPage)
  {
    int notShown = totalCount - shownCount;

    if (format == OutputFormat.Table)
    {
      Console.WriteLine();
      Console.WriteLine($"page {pageNumber}/{totalPages}  ({totalCount} total)");
      if (hasNextPage && notShown > 0)
      {
        Console.WriteLine($"… {notShown} more not shown — use --page-size -1 for all");
      }

      return;
    }

    if (hasNextPage && notShown > 0)
    {
      Console.Error.WriteLine(
        $"note: showing {shownCount} of {totalCount} — {notShown} more not shown (use --page-size -1 for all)");
    }
  }
}
