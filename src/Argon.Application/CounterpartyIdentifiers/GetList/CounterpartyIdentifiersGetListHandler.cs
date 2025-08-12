using Argon.Application.Common.Models;
using Argon.Application.Extensions;

namespace Argon.Application.CounterpartyIdentifiers.GetList;

[UsedImplicitly]
public class CounterpartyIdentifiersGetListHandler(
  IApplicationDbContext dbContext
) : IRequestHandler<CounterpartyIdentifiersGetListRequest, PaginatedList<CounterpartyIdentifiersGetListResponse>>
{
  public async Task<PaginatedList<CounterpartyIdentifiersGetListResponse>> Handle(CounterpartyIdentifiersGetListRequest request, CancellationToken cancellationToken)
  {
    return await dbContext
      .CounterpartyIdentifiers
      .AsNoTracking()
      .Where(counterpartyIdentifier => request.CounterpartyId == null || counterpartyIdentifier.CounterpartyId == request.CounterpartyId)
      .Where(counterpartyIdentifier => string.IsNullOrWhiteSpace(request.IdentifierText) || counterpartyIdentifier.IdentifierText.ToLower().Contains(request.IdentifierText.ToLower()))
      .OrderBy(counterpartyIdentifier => counterpartyIdentifier.IdentifierText)
      .Select(counterpartyIdentifier => new CounterpartyIdentifiersGetListResponse(
        counterpartyIdentifier.Id,
        counterpartyIdentifier.CounterpartyId,
        counterpartyIdentifier.IdentifierText
      ))
      .PaginatedListAsync(request.PageNumber, request.PageSize, cancellationToken);
  }
}