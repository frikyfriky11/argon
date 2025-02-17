using Argon.Application.Common.Models;
using Argon.Application.Counterparties.Create;
using Argon.Application.Counterparties.Delete;
using Argon.Application.Counterparties.Get;
using Argon.Application.Counterparties.GetList;
using Argon.Application.Counterparties.Update;

namespace Argon.WebApi.Controllers;

/// <summary>
///   The Counterparties endpoint allows you to create, read, update and delete Counterparty entities from the application.
/// </summary>
[Authorize]
[ApiController]
[Route("[controller]")]
public class CounterpartiesController(
  ISender mediator
) : ControllerBase
{
  /// <summary>
  ///   Gets a list of Counterparties
  /// </summary>
  [HttpGet("")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public async Task<ActionResult<PaginatedList<CounterpartiesGetListResponse>>> GetList([FromQuery] CounterpartiesGetListRequest request)
  {
    return await mediator.Send(request);
  }

  /// <summary>
  ///   Gets an existing Counterparty
  /// </summary>
  /// <param name="id">The id of the Counterparty</param>
  /// <returns>The Counterparty with the specified id</returns>
  /// <response code="200">The Counterparty with the specified id</response>
  /// <response code="404">A Counterparty with the specified id could not be found</response>
  [HttpGet("{id:guid}")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<CounterpartiesGetResponse>> Get([FromRoute] Guid id)
  {
    CounterpartiesGetRequest request = new(id);

    return await mediator.Send(request);
  }

  /// <summary>
  ///   Creates a new Counterparty
  /// </summary>
  /// <param name="request">The Counterparty entity to create</param>
  /// <returns>The Id of the newly created Counterparty</returns>
  /// <response code="200">The id of the newly created Counterparty</response>
  /// <response code="400">The supplied Counterparty object did not pass validation checks</response>
  [HttpPost("")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<ActionResult<CounterpartiesCreateResponse>> Create([FromBody] CounterpartiesCreateRequest request)
  {
    return await mediator.Send(request);
  }

  /// <summary>
  ///   Updates an existing Counterparty
  /// </summary>
  /// <param name="id">The id of the Counterparty</param>
  /// <param name="request">The Counterparty entity to update</param>
  /// <returns>Nothing</returns>
  /// <response code="204">The Counterparty was correctly updated</response>
  /// <response code="400">The supplied Counterparty object did not pass validation checks</response>
  /// <response code="404">A Counterparty with the specified id could not be found</response>
  [HttpPut("{id:guid}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> Update([FromRoute] Guid id, [FromBody] CounterpartiesUpdateRequest request)
  {
    request.Id = id;

    await mediator.Send(request);

    return NoContent();
  }

  /// <summary>
  ///   Deletes an existing Counterparty
  /// </summary>
  /// <param name="id">The id of the Counterparty</param>
  /// <returns>Nothing</returns>
  /// <response code="204">The Counterparty was correctly deleted</response>
  /// <response code="404">A Counterparty with the specified id could not be found</response>
  [HttpDelete("{id:guid}")]
  public async Task<ActionResult> Delete([FromRoute] Guid id)
  {
    CounterpartiesDeleteRequest request = new(id);

    await mediator.Send(request);

    return NoContent();
  }
}
