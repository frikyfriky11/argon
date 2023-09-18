using Argon.Application.BudgetItems;

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
