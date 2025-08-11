using Argon.Application.Accounts.Create;
using Argon.Application.Accounts.Delete;
using Argon.Application.Accounts.Favourite;
using Argon.Application.Accounts.Get;
using Argon.Application.Accounts.GetList;
using Argon.Application.Accounts.Update;

namespace Argon.WebApi.Controllers;

/// <summary>
///   The Accounts endpoint allows you to create, read, update and delete Account entities from the application.
/// </summary>
[Authorize]
[ApiController]
[Route("[controller]")]
public class AccountsController(
  ISender mediator
) : ControllerBase
{
  /// <summary>
  ///   Gets a list of Accounts
  /// </summary>
  [HttpGet("")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public async Task<ActionResult<List<AccountsGetListResponse>>> GetList([FromQuery] AccountsGetListRequest request)
  {
    return await mediator.Send(request);
  }

  /// <summary>
  ///   Gets an existing Account
  /// </summary>
  /// <param name="id">The id of the Account</param>
  /// <returns>The Account with the specified id</returns>
  /// <response code="200">The Account with the specified id</response>
  /// <response code="404">A Account with the specified id could not be found</response>
  [HttpGet("{id:guid}")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<AccountsGetResponse>> Get([FromRoute] Guid id)
  {
    AccountsGetRequest request = new(id);

    return await mediator.Send(request);
  }

  /// <summary>
  ///   Creates a new Account
  /// </summary>
  /// <param name="request">The Account entity to create</param>
  /// <returns>The Id of the newly created Account</returns>
  /// <response code="200">The id of the newly created Account</response>
  /// <response code="400">The supplied Account object did not pass validation checks</response>
  [HttpPost("")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<ActionResult<AccountsCreateResponse>> Create([FromBody] AccountsCreateRequest request)
  {
    return await mediator.Send(request);
  }

  /// <summary>
  ///   Updates an existing Account
  /// </summary>
  /// <param name="id">The id of the Account</param>
  /// <param name="request">The Account entity to update</param>
  /// <returns>Nothing</returns>
  /// <response code="204">The Account was correctly updated</response>
  /// <response code="400">The supplied Account object did not pass validation checks</response>
  /// <response code="404">A Account with the specified id could not be found</response>
  [HttpPut("{id:guid}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> Update([FromRoute] Guid id, [FromBody] AccountsUpdateRequest request)
  {
    request.Id = id;

    await mediator.Send(request);

    return NoContent();
  }

  /// <summary>
  ///   Deletes an existing Account
  /// </summary>
  /// <param name="id">The id of the Account</param>
  /// <returns>Nothing</returns>
  /// <response code="204">The Account was correctly deleted</response>
  /// <response code="404">A Account with the specified id could not be found</response>
  [HttpDelete("{id:guid}")]
  public async Task<ActionResult> Delete([FromRoute] Guid id)
  {
    AccountsDeleteRequest request = new(id);

    await mediator.Send(request);

    return NoContent();
  }

  /// <summary>
  ///   Toggles the favourite status on an account
  /// </summary>
  /// <param name="id">The id of the Account</param>
  /// <param name="request">The Account entity to update</param>
  /// <returns>Nothing</returns>
  /// <response code="204">The Account was correctly updated</response>
  /// <response code="400">The supplied Account object did not pass validation checks</response>
  /// <response code="404">A Account with the specified id could not be found</response>
  [HttpPut("{id:guid}/Favourite")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> Favourite([FromRoute] Guid id, [FromBody] AccountsFavouriteRequest request)
  {
    request.Id = id;

    await mediator.Send(request);

    return NoContent();
  }
}
