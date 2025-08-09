namespace Argon.Application.BankStatements.Parse.Parsers;

public class BankStatementItem
{
  // the transaction date (e.g., CurrencyDate)
  public DateOnly Date { get; set; }

  // the transaction amount
  public decimal Amount { get; set; }
  
  // the derived counterparty name string
  public string? CounterpartyName { get; set; }
  
  // the original raw text/line from the file
  public string RawInput { get; set; } = null!;
  
  // indicates parsing errors for this item
  public string? ErrorMessage { get; set; } 

  // holds the specific, detailed parsed item
  public BaseItem? SpecificParsedItem { get; set; }
}