using Argon.Application.Statistics.Cashflow;
using Argon.Application.Statistics.Liquidity;
using Argon.Application.Statistics.TopCategories;
using Argon.Application.Statistics.TopCounterparties;

namespace Argon.WebApi.Controllers;

/// <summary>
///   The Statistics endpoint exposes aggregated, read-only views over the ledger that power
///   the dashboard charts (liquidity over time, income vs expense, top categories, top counterparties).
/// </summary>
[Authorize]
[ApiController]
[Route("[controller]")]
public class StatisticsController(
  ISender mediator
) : ControllerBase
{
  /// <summary>
  ///   Gets the running balance of all Cash accounts at the end of each month.
  /// </summary>
  [HttpGet("liquidity")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public async Task<ActionResult<List<StatisticsLiquidityResponse>>> Liquidity(
    [FromQuery] StatisticsLiquidityRequest request)
  {
    return await mediator.Send(request);
  }

  /// <summary>
  ///   Gets monthly income vs expense for a period.
  /// </summary>
  [HttpGet("cashflow")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public async Task<ActionResult<List<StatisticsCashflowResponse>>> Cashflow(
    [FromQuery] StatisticsCashflowRequest request)
  {
    return await mediator.Send(request);
  }

  /// <summary>
  ///   Gets the top spending categories for a period with their cumulative percentage (Pareto).
  /// </summary>
  [HttpGet("top-categories")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public async Task<ActionResult<List<StatisticsTopCategoriesResponse>>> TopCategories(
    [FromQuery] StatisticsTopCategoriesRequest request)
  {
    return await mediator.Send(request);
  }

  /// <summary>
  ///   Gets the top counterparties by spend for a period.
  /// </summary>
  [HttpGet("top-counterparties")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public async Task<ActionResult<List<StatisticsTopCounterpartiesResponse>>> TopCounterparties(
    [FromQuery] StatisticsTopCounterpartiesRequest request)
  {
    return await mediator.Send(request);
  }
}
