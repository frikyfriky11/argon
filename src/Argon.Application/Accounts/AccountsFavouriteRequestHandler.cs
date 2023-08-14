namespace Argon.Application.Accounts;

/// <summary>
///   The request to update the favourite status on an account
///   <param name="IsFavourite">Whether the account is marked as favourite</param>
/// </summary>
[PublicAPI]
public record AccountsFavouriteRequest(bool IsFavourite) : IRequest
{
  /// <summary>
  ///   This field is used only internally to manually bind the [FromRoute] Guid id attribute.
  ///   It is not displayed in the documentation because the user of the API should use the route parameter.
  ///   This cannot be made internal because it would cause conflicts since you couldn't ever set it.
  /// </summary>
  [OpenApiIgnore]
  [JsonIgnore]
  public Guid Id { get; set; }
}

[UsedImplicitly]
public class AccountsFavouriteRequestHandler : IRequestHandler<AccountsFavouriteRequest>
{
  private readonly IApplicationDbContext _dbContext;

  public AccountsFavouriteRequestHandler(IApplicationDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task Handle(AccountsFavouriteRequest request, CancellationToken cancellationToken)
  {
    Account? entity = await _dbContext
      .Accounts
      .Where(account => account.Id == request.Id)
      .FirstOrDefaultAsync(cancellationToken);

    if (entity is null)
    {
      throw new NotFoundException(nameof(Account), request.Id);
    }

    entity.IsFavourite = request.IsFavourite;

    await _dbContext.SaveChangesAsync(cancellationToken);
  }
}
