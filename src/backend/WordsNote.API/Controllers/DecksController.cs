using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordsNote.Application.Commands.Cards;
using WordsNote.Application.Commands.Decks;
using WordsNote.Application.DTOs;
using WordsNote.Application.Interfaces;
using WordsNote.Application.Queries.Decks;

namespace WordsNote.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DecksController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public DecksController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DeckDto>>> GetDecks()
    {
        var result = await _mediator.Send(new GetDecksQuery(_currentUserService.UserId));
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DeckDto>> GetDeck(Guid id)
    {
        var result = await _mediator.Send(new GetDeckQuery(id));
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<DeckDto>> CreateDeck([FromBody] CreateDeckDto dto)
    {
        var result = await _mediator.Send(new CreateDeckCommand(dto.Name, dto.Description, _currentUserService.UserId));
        return CreatedAtAction(nameof(GetDeck), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<DeckDto>> UpdateDeck(Guid id, [FromBody] UpdateDeckDto dto)
    {
        var result = await _mediator.Send(new UpdateDeckCommand(id, dto.Name, dto.Description));
        return result == null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDeck(Guid id)
    {
        var result = await _mediator.Send(new DeleteDeckCommand(id));
        return result ? NoContent() : NotFound();
    }

    [HttpPost("{id}/import")]
    public async Task<ActionResult<int>> ImportFromCsv(Guid id, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file provided.");

        string csvContent;
        using (var reader = new StreamReader(file.OpenReadStream()))
            csvContent = await reader.ReadToEndAsync();

        var count = await _mediator.Send(new ImportFromCsvCommand(id, csvContent));
        return Ok(new { ImportedCount = count });
    }

    [HttpPost("{id}/reset")]
    public async Task<IActionResult> ResetProgress(Guid id)
    {
        var result = await _mediator.Send(new ResetDeckProgressCommand(id));
        return result ? NoContent() : NotFound();
    }
}
