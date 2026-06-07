namespace Argon.Application.Statistics.NetWorth;

/// <summary>
///   The request to fetch total net worth as it stands today: the balance-sheet identity
///   Assets − Liabilities, summed across every Cash, Asset and Receivable account (assets)
///   net of every Liability account (payables). Unlike the liquid headline this includes
///   illiquid assets such as the house, so it surfaces real equity rather than just cash.
/// </summary>
[PublicAPI]
public record StatisticsNetWorthRequest : IRequest<StatisticsNetWorthResponse>;
