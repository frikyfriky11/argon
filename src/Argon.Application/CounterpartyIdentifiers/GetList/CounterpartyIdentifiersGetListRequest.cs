using Argon.Application.Common.Models;

namespace Argon.Application.CounterpartyIdentifiers.GetList;

/// <summary>
///   The request to get a list of CounterpartyIdentifier entities
/// </summary>
/// <param name="CounterpartyId">The id of the counterparty</param>
/// <param name="IdentifierText">The actual text of the counterpartyIdentifiers</param>
/// <param name="PageNumber">The page number (defaults to 1)</param>
/// <param name="PageSize">The page size (defaults to 25)</param>
[PublicAPI]
public record CounterpartyIdentifiersGetListRequest(
  Guid? CounterpartyId,
  string? IdentifierText,
  int PageNumber = 1,
  int PageSize = 25
) : PaginatedListRequest(PageNumber, PageSize),
  IRequest<PaginatedList<CounterpartyIdentifiersGetListResponse>>;