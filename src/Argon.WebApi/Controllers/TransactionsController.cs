using Argon.Application.Common.Models;
using Argon.Application.Transactions.CategorizeRow;
using Argon.Application.Transactions.Create;
using Argon.Application.Transactions.Delete;
using Argon.Application.Transactions.Get;
using Argon.Application.Transactions.GetList;
using Argon.Application.Transactions.SetCounterparty;
using Argon.Application.Transactions.Update;

namespace Argon.WebApi.Controllers;

/// <summary>
///   The Transactions endpoint allows you to create, read, update and delete Transaction entities from the application.
/// </summary>
[Authorize]
[ApiController]
[Route("[controller]")]
public class TransactionsController(
  ISender mediator
) : ControllerBase
{
  /// <summary>
  ///   Gets a list of Transactions
  /// </summary>
  [HttpGet("")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public async Task<ActionResult<PaginatedList<TransactionsGetListResponse>>> GetList([FromQuery] TransactionsGetListRequest request)
  {
    return await mediator.Send(request);
  }

  /// <summary>
  ///   Gets an existing Transaction
  /// </summary>
  /// <param name="id">The id of the Transaction</param>
  /// <returns>The Transaction with the specified id</returns>
  /// <response code="200">The Transaction with the specified id</response>
  /// <response code="404">A Transaction with the specified id could not be found</response>
  [HttpGet("{id:guid}")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<TransactionsGetResponse>> Get([FromRoute] Guid id)
  {
    TransactionsGetRequest request = new(id);

    return await mediator.Send(request);
  }

  /// <summary>
  ///   Creates a new Transaction
  /// </summary>
  /// <param name="request">The Transaction entity to create</param>
  /// <returns>The Id of the newly created Transaction</returns>
  /// <response code="200">The id of the newly created Transaction</response>
  /// <response code="400">The supplied Transaction object did not pass validation checks</response>
  [HttpPost("")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<ActionResult<TransactionsCreateResponse>> Create([FromBody] TransactionsCreateRequest request)
  {
    return await mediator.Send(request);
  }

  /// <summary>
  ///   Updates an existing Transaction
  /// </summary>
  /// <param name="id">The id of the Transaction</param>
  /// <param name="request">The Transaction entity to update</param>
  /// <returns>Nothing</returns>
  /// <response code="204">The Transaction was correctly updated</response>
  /// <response code="400">The supplied Transaction object did not pass validation checks</response>
  /// <response code="404">A Transaction with the specified id could not be found</response>
  [HttpPut("{id:guid}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> Update([FromRoute] Guid id, [FromBody] TransactionsUpdateRequest request)
  {
    request.Id = id;

    await mediator.Send(request);

    return NoContent();
  }

  /// <summary>
  ///   Reassigns the counterparty of a Transaction without resending the rest of the
  ///   transaction. Used when the importer auto-matched the wrong counterparty.
  /// </summary>
  /// <param name="id">The id of the Transaction</param>
  /// <param name="request">The new counterparty id</param>
  /// <response code="204">The counterparty was updated successfully</response>
  /// <response code="400">The supplied request did not pass validation checks</response>
  /// <response code="404">The Transaction could not be found</response>
  [HttpPatch("{id:guid}/counterparty")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> SetCounterparty(
    [FromRoute] Guid id,
    [FromBody] TransactionsSetCounterpartyRequest request)
  {
    request.TransactionId = id;

    await mediator.Send(request);

    return NoContent();
  }

  /// <summary>
  ///   Assigns an account to a single row of a Transaction without resending the rest
  ///   of the transaction. Intended for the import-review reconciliation flow.
  /// </summary>
  /// <param name="id">The id of the Transaction</param>
  /// <param name="rowId">The id of the TransactionRow to update</param>
  /// <param name="request">The categorization payload (account id)</param>
  /// <response code="204">The row was categorized successfully</response>
  /// <response code="400">The supplied request did not pass validation checks</response>
  /// <response code="404">The Transaction or TransactionRow could not be found</response>
  [HttpPatch("{id:guid}/rows/{rowId:guid}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> CategorizeRow(
    [FromRoute] Guid id,
    [FromRoute] Guid rowId,
    [FromBody] TransactionsCategorizeRowRequest request)
  {
    request.TransactionId = id;
    request.RowId = rowId;

    await mediator.Send(request);

    return NoContent();
  }

  /// <summary>
  ///   Deletes an existing Transaction
  /// </summary>
  /// <param name="id">The id of the Transaction</param>
  /// <returns>Nothing</returns>
  /// <response code="204">The Transaction was correctly deleted</response>
  /// <response code="404">A Transaction with the specified id could not be found</response>
  [HttpDelete("{id:guid}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> Delete([FromRoute] Guid id)
  {
    TransactionsDeleteRequest request = new(id);

    await mediator.Send(request);

    return NoContent();
  }
}
