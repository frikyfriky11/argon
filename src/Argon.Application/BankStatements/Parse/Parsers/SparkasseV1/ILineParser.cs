namespace Argon.Application.BankStatements.Parse.Parsers.SparkasseV1;

public interface ILineParser
{
  bool CanParse(string rawDescription);
  BaseItem Parse(DateOnly accountingDate, DateOnly currencyDate, string rawDescription, decimal amount);
}