namespace Argon.Domain.Entities;

/// <summary>
///   Represents the status of a transaction
/// </summary>
public enum TransactionStatus
{
  /// <summary>
  ///   The transaction is confirmed because it was either manually inserted or confirmed during an import
  /// </summary>
  Confirmed = 0,
  
  /// <summary>
  ///   The transaction is pending a review before being definitively imported and confirmed
  /// </summary>
  PendingImportReview = 1,
  
  /// <summary>
  ///   The transaction is probably a duplicate of another transaction and was detected during an import request
  /// </summary>
  PotentialDuplicate = 2,
}
