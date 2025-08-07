namespace Argon.Domain.Entities;

/// <summary>
///   This entity holds the header data for the uploaded bank statement
/// </summary>
public class BankStatement : BaseAuditableEntity
{
  /// <summary>
  ///   The id of the bank statement
  /// </summary>
  public Guid Id { get; set; }

  /// <summary>
  ///   The name of the file
  /// </summary>
  public string FileName { get; set; } = default!;

  /// <summary>
  ///   The content of the file
  /// </summary>
  public byte[] FileContent { get; set; } = default!;

  /// <summary>
  ///   The id of the account against which the bank statement is imported to
  /// </summary>
  public Guid ImportedToAccountId { get; set; }

  /// <summary>
  ///   The account against which the bank statement is imported to
  /// </summary>
  public Account ImportedToAccount { get; set; } = default!;

  /// <summary>
  ///   The name of the parser used
  /// </summary>
  public Guid ParserId { get; set; }

  /// <summary>
  ///   All the transactions that this bank statement generated
  /// </summary>
  public ICollection<Transaction> Transactions { get; set; } = [];
}