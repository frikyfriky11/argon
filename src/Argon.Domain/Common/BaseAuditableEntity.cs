namespace Argon.Domain.Common;

/// <summary>
///   This abstract class represents a base entity that can be inherited from other
///   classes to obtain default properties that enable the change tracker of EF Core
///   to track the creation and modification of entities at the Infrastructure layer.
/// </summary>
public abstract class BaseAuditableEntity
{
  /// <summary>
  ///   The date when the entity was first created
  /// </summary>
  public Instant Created { get; set; }

  /// <summary>
  ///   The date when the entity was last modified.
  ///   It can be null if the entity has never been modified.
  /// </summary>
  public Instant? LastModified { get; set; }
}
