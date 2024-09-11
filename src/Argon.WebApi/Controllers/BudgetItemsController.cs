using Argon.Application.BudgetItems.GetList;
using Argon.Application.BudgetItems.Upsert;

namespace Argon.WebApi.Controllers;

/// <summary>
///   The Budget Items endpoint allows you to upsert, read and delete Budget Item entities from the application.
/// </summary>
[ApiController]
[Route("[controller]")]
public class BudgetItemsController : ControllerBase
{
  private readonly ISender _mediator;

  public BudgetItemsController(ISender mediator)
  {
    _mediator = mediator;
  }

  /// <summary>
  ///   Gets a list of Budget Items
  /// </summary>
  [HttpGet("")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public async Task<ActionResult<List<BudgetItemsGetListResponse>>> GetList([FromQuery] BudgetItemsGetListRequest request)
  {
    return await _mediator.Send(request);
  }

  /// <summary>
  ///   Creates, updates or deletes a Budget Item
  /// </summary>
  /// <param name="request">The Budget Item entity to create, update or delete</param>
  /// <returns>The Id of the newly created or updated Budget Item, or null if it was deleted</returns>
  /// <response code="200">The id of the newly created or updated Budget Item, or null if it was deleted</response>
  /// <response code="400">The supplied Budget Item object did not pass validation checks</response>
  [HttpPut("")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<ActionResult<BudgetItemsUpsertResponse>> Upsert([FromBody] BudgetItemsUpsertRequest request)
  {
    return await _mediator.Send(request);
  }
}
