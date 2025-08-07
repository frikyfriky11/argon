using Argon.Application.BankStatements.GetList;
using Argon.Application.BankStatements.Parse;
using Argon.Application.BankStatements.ParsersGetList;

namespace Argon.WebApi.Controllers;

/// <summary>
///   The BankStatements endpoint allows you to parse and read BankStatement entities from the application.
/// </summary>
[ApiController]
[Route("[controller]")]
public class BankStatementsController(
  ISender mediator
) : ControllerBase
{
  /// <summary>
  ///   Parses a new BankStatement
  /// </summary>
  /// <param name="request">The BankStatement entity to parse</param>
  /// <returns>The Id of the newly created BankStatement</returns>
  /// <response code="200">The id of the newly created BankStatement</response>
  /// <response code="400">The supplied BankStatement object did not pass validation checks</response>
  [HttpPost("Parse")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<ActionResult<BankStatementsParseResponse>> Parse([FromBody] BankStatementsParseRequest request)
  {
    return await mediator.Send(request);
  }

  /// <summary>
  ///   Gets a list of BankStatements
  /// </summary>
  [HttpGet("")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public async Task<ActionResult<List<BankStatementsGetListResponse>>> GetList(
    [FromQuery] BankStatementsGetListRequest request)
  {
    return await mediator.Send(request);
  }

  /// <summary>
  ///   Gets a list of Parsers
  /// </summary>
  [HttpGet("Parsers")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public async Task<ActionResult<List<BankStatementParsersGetListResponse>>> ParsersGetList(
    [FromQuery] BankStatementParsersGetListRequest request)
  {
    return await mediator.Send(request);
  }
}