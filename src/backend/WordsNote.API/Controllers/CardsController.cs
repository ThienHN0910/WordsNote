using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordsNote.Application.Commands.Cards;
using WordsNote.Application.DTOs;
using WordsNote.Application.Interfaces;
using WordsNote.Application.Queries.Cards;

namespace WordsNote.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CardsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public CardsController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CardDto>>> GetCards([FromQuery] Guid deckId)
    {
        var result = await _mediator.Send(new GetCardsQuery(deckId));
        return Ok(result);
    }

    [HttpGet("due")]
    public async Task<ActionResult<IEnumerable<CardDto>>> GetDueCards()
    {
        var result = await _mediator.Send(new GetDueCardsQuery(_currentUserService.UserId));
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<CardDto>> CreateCard([FromBody] CreateCardDto dto)
    {
        var result = await _mediator.Send(new CreateCardCommand(dto.Front, dto.Back, dto.Notes, dto.DeckId));
        return CreatedAtAction(nameof(GetCards), new { deckId = result.DeckId }, result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCard(Guid id)
    {
        var result = await _mediator.Send(new DeleteCardCommand(id));
        return result ? NoContent() : NotFound();
    }

    [HttpPost("{id}/review")]
    public async Task<ActionResult<CardDto>> ReviewCard(Guid id, [FromBody] ReviewCardDto dto)
    {
        var result = await _mediator.Send(new ReviewCardCommand(id, dto.Result));
        return result == null ? NotFound() : Ok(result);
    }
}
