using Argon.Application.Accounts.Create;
using Argon.Application.Accounts.Get;
using Argon.Application.Accounts.Update;

namespace Argon.Application.Common.Mappings;

[UsedImplicitly]
public class AccountsProfile : Profile
{
  public AccountsProfile()
  {
    CreateMap<Account, AccountsGetResponse>()
      .ForCtorParam(nameof(AccountsGetResponse.TotalAmount), options => options.MapFrom(account => account.TransactionRows.Sum(x => (x.Debit ?? 0) - (x.Credit ?? 0))));
    CreateMap<AccountsCreateRequest, Account>();
    CreateMap<AccountsUpdateRequest, Account>();
  }
}
