using Argon.Application.BudgetItems.GetList;
using Argon.Application.BudgetItems.Upsert;

namespace Argon.Application.Common.Mappings;

[UsedImplicitly]
public class BudgetItemsProfile : Profile
{
  public BudgetItemsProfile()
  {
    CreateMap<BudgetItem, BudgetItemsGetListResponse>();
    CreateMap<BudgetItemsUpsertRequest, BudgetItem>();
  }
}
