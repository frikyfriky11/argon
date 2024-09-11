using Argon.Application.Transactions.Create;
using Argon.Application.Transactions.Get;
using Argon.Application.Transactions.GetList;
using Argon.Application.Transactions.Update;

namespace Argon.Application.Common.Mappings;

[UsedImplicitly]
public class TransactionsProfile : Profile
{
  public TransactionsProfile()
  {
    CreateMap<Transaction, TransactionsGetListResponse>()
      .ForCtorParam(nameof(TransactionsGetListResponse.TransactionRows), 
        options => options.MapFrom(entity => entity.TransactionRows.OrderBy(row => row.RowCounter).ThenBy(row => row.Id)));
    CreateMap<TransactionRow, TransactionRowsGetListResponse>();
    CreateMap<Transaction, TransactionsGetResponse>()
      .ForCtorParam(nameof(TransactionsGetResponse.TransactionRows),
        options => options.MapFrom(entity => entity.TransactionRows.OrderBy(row => row.RowCounter).ThenBy(row => row.Id)));
    CreateMap<TransactionRow, TransactionRowsGetResponse>();
    CreateMap<TransactionsCreateRequest, Transaction>();
    CreateMap<TransactionRowsCreateRequest, TransactionRow>();
    CreateMap<TransactionsUpdateRequest, Transaction>()
      .ForMember(transaction => transaction.TransactionRows, opt => opt.Ignore());
    CreateMap<TransactionRowsUpdateRequest, TransactionRow>();
  }
}
