using Argon.Application.Common.Models;
using Argon.Application.CounterpartyIdentifiers.Create;
using Argon.Application.CounterpartyIdentifiers.Delete;
using Argon.Application.CounterpartyIdentifiers.Get;
using Argon.Application.CounterpartyIdentifiers.GetList;
using Argon.Application.CounterpartyIdentifiers.Update;

namespace Argon.WebApi.Controllers;

/// <summary>
///   The CounterpartyIdentifiers endpoint allows you to create, read, update and delete CounterpartyIdentifier entities from the application.
/// </summary>
[Authorize]
[ApiController]
[Route("[controller]")]
public class CounterpartyIdentifiersController(
  ISender mediator
) : ControllerBase
{
  /// <summary>
  ///   Gets a list of CounterpartyIdentifiers
  /// </summary>
  [HttpGet("")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public async Task<ActionResult<PaginatedList<CounterpartyIdentifiersGetListResponse>>> GetList([FromQuery] CounterpartyIdentifiersGetListRequest request)
  {
    return await mediator.Send(request);
  }

  /// <summary>
  ///   Gets an existing CounterpartyIdentifier
  /// </summary>
  /// <param name="id">The id of the CounterpartyIdentifier</param>
  /// <returns>The CounterpartyIdentifier with the specified id</returns>
  /// <response code="200">The CounterpartyIdentifier with the specified id</response>
  /// <response code="404">A CounterpartyIdentifier with the specified id could not be found</response>
  [HttpGet("{id:guid}")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<CounterpartyIdentifiersGetResponse>> Get([FromRoute] Guid id)
  {
    CounterpartyIdentifiersGetRequest request = new(id);

    return await mediator.Send(request);
  }

  /// <summary>
  ///   Creates a new CounterpartyIdentifier
  /// </summary>
  /// <param name="request">The CounterpartyIdentifier entity to create</param>
  /// <returns>The Id of the newly created CounterpartyIdentifier</returns>
  /// <response code="200">The id of the newly created CounterpartyIdentifier</response>
  /// <response code="400">The supplied CounterpartyIdentifier object did not pass validation checks</response>
  [HttpPost("")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<ActionResult<CounterpartyIdentifiersCreateResponse>> Create([FromBody] CounterpartyIdentifiersCreateRequest request)
  {
    return await mediator.Send(request);
  }

  /// <summary>
  ///   Updates an existing CounterpartyIdentifier
  /// </summary>
  /// <param name="id">The id of the CounterpartyIdentifier</param>
  /// <param name="request">The CounterpartyIdentifier entity to update</param>
  /// <returns>Nothing</returns>
  /// <response code="204">The CounterpartyIdentifier was correctly updated</response>
  /// <response code="400">The supplied CounterpartyIdentifier object did not pass validation checks</response>
  /// <response code="404">A CounterpartyIdentifier with the specified id could not be found</response>
  [HttpPut("{id:guid}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> Update([FromRoute] Guid id, [FromBody] CounterpartyIdentifiersUpdateRequest request)
  {
    request.Id = id;

    await mediator.Send(request);

    return NoContent();
  }

  /// <summary>
  ///   Deletes an existing CounterpartyIdentifier
  /// </summary>
  /// <param name="id">The id of the CounterpartyIdentifier</param>
  /// <returns>Nothing</returns>
  /// <response code="204">The CounterpartyIdentifier was correctly deleted</response>
  /// <response code="404">A CounterpartyIdentifier with the specified id could not be found</response>
  [HttpDelete("{id:guid}")]
  public async Task<ActionResult> Delete([FromRoute] Guid id)
  {
    CounterpartyIdentifiersDeleteRequest request = new(id);

    await mediator.Send(request);

    return NoContent();
  }
}
