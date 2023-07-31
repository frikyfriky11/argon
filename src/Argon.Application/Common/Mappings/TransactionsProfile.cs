using Argon.Application.Transactions;

namespace Argon.Application.Common.Mappings;

[UsedImplicitly]
public class TransactionsProfile : Profile
{
  public TransactionsProfile()
  {
    CreateMap<Transaction, TransactionsGetListResponse>();
    CreateMap<TransactionRow, TransactionRowsGetListResponse>();
    CreateMap<Transaction, TransactionsGetResponse>();
    CreateMap<TransactionRow, TransactionRowsGetResponse>();
    CreateMap<TransactionsCreateRequest, Transaction>();
    CreateMap<TransactionRowsCreateRequest, TransactionRow>();
    CreateMap<TransactionsUpdateRequest, Transaction>()
      .ForMember(transaction => transaction.TransactionRows, opt => opt.Ignore());
    CreateMap<TransactionRowsUpdateRequest, TransactionRow>();
  }
}
