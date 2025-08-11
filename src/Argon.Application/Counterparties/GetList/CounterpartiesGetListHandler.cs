using Argon.Application.Common.Models;
using Argon.Application.Extensions;

namespace Argon.Application.Counterparties.GetList;

[UsedImplicitly]
public class CounterpartiesGetListHandler(
  IApplicationDbContext dbContext
): IRequestHandler<CounterpartiesGetListRequest, PaginatedList<CounterpartiesGetListResponse>>
{
  public async Task<PaginatedList<CounterpartiesGetListResponse>> Handle(CounterpartiesGetListRequest request, CancellationToken cancellationToken)
  {
    return await dbContext
      .Counterparties
      .AsNoTracking()
      .Where(counterparty => string.IsNullOrWhiteSpace(request.Name) || counterparty.Name.ToLower().Contains(request.Name.ToLower()))
      .OrderBy(counterparty => counterparty.Name)
      .Select(counterparty => new CounterpartiesGetListResponse(
        counterparty.Id,
        counterparty.Name
      ))
      .PaginatedListAsync(request.PageNumber, request.PageSize, cancellationToken);
  }
}
