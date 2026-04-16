using Application.Dtos.WordsNote;
using Application.Helpers;
using Application.IServices.AS;
using Domain.Entities.WordsNote;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace FeatureFusion.Controllers.WordsNote;

[ApiController]
[Authorize]
[Route("api/study")]
public class StudyController : ControllerBase
{
    private const string DeskCollectionName = "wordsnote_desks";
    private const string CardCollectionName = "wordsnote_cards";

    private readonly IMongoCollection<CardDocument> _cards;
    private readonly IMongoCollection<DeskDocument> _desks;
    private readonly ICurrentUserService _currentUserService;

    public StudyController(IMongoDatabase database, ICurrentUserService currentUserService)
    {
        _cards = database.GetCollection<CardDocument>(CardCollectionName);
        _desks = database.GetCollection<DeskDocument>(DeskCollectionName);
        _currentUserService = currentUserService;
    }

    [HttpGet("quick")]
    public async Task<ActionResult<QuickStudyResponseDTO>> GetQuickAsync([FromQuery] string collectionId, [FromQuery] int limit = 20)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
        {
            return Unauthorized(new { Error = "Invalid or unsupported token subject." });
        }

        if (string.IsNullOrWhiteSpace(collectionId))
        {
            return BadRequest(new { Error = "collectionId is required." });
        }

        var desk = await _desks.Find(item => item.Id == collectionId && item.UserId == userId.Value).FirstOrDefaultAsync();
        if (desk is null)
        {
            return NotFound(new { Error = "Collection not found." });
        }

        var cards = await _cards.Find(card => card.UserId == userId.Value && card.DeskId == collectionId)
            .SortBy(card => card.DueAt)
            .ThenBy(card => card.Front)
            .ToListAsync();

        var now = DateTime.UtcNow;
        var dueCards = cards.Where(card => card.DueAt <= now).ToList();
        var source = dueCards.Count > 0 ? dueCards : cards;

        var safeLimit = limit <= 0 ? 20 : Math.Clamp(limit, 1, 100);
        var selected = source.Take(safeLimit).Select(MapCard).ToList();

        return Ok(new QuickStudyResponseDTO
        {
            CollectionId = collectionId,
            TotalCards = cards.Count,
            DueCards = dueCards.Count,
            Cards = selected,
        });
    }

    [HttpPost("review")]
    public async Task<ActionResult<StudyCardDTO>> ReviewAsync([FromBody] StudyReviewRequestDTO request)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
        {
            return Unauthorized(new { Error = "Invalid or unsupported token subject." });
        }

        if (string.IsNullOrWhiteSpace(request.CardId))
        {
            return BadRequest(new { Error = "cardId is required." });
        }

        var difficulty = request.Difficulty.Trim().ToLowerInvariant();
        if (difficulty is not ("hard" or "medium" or "easy"))
        {
            return BadRequest(new { Error = "Difficulty must be hard, medium, or easy." });
        }

        var card = await _cards.Find(item => item.Id == request.CardId && item.UserId == userId.Value).FirstOrDefaultAsync();
        if (card is null)
        {
            return NotFound(new { Error = "Card not found." });
        }

        var now = DateTime.UtcNow;
        var streak = difficulty == "hard" ? 0 : card.Streak + 1;
        var intervalDays = NextIntervalDays(difficulty, streak);

        card.Streak = streak;
        card.LastReviewedAt = now;
        card.DueAt = now.AddDays(intervalDays);

        await _cards.ReplaceOneAsync(item => item.Id == card.Id && item.UserId == userId.Value, card);
        await TouchDeskAsync(card.DeskId, userId.Value);

        return Ok(MapCard(card));
    }

    [HttpGet("deep/session")]
    public async Task<ActionResult<DeepStudySessionDTO>> GetDeepSessionAsync([FromQuery] string collectionId)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
        {
            return Unauthorized(new { Error = "Invalid or unsupported token subject." });
        }

        if (string.IsNullOrWhiteSpace(collectionId))
        {
            return BadRequest(new { Error = "collectionId is required." });
        }

        var desk = await _desks.Find(item => item.Id == collectionId && item.UserId == userId.Value).FirstOrDefaultAsync();
        if (desk is null)
        {
            return NotFound(new { Error = "Collection not found." });
        }

        var cards = await _cards.Find(card => card.UserId == userId.Value && card.DeskId == collectionId)
            .SortBy(card => card.DueAt)
            .ThenBy(card => card.Front)
            .ToListAsync();

        var dueCount = cards.Count(card => card.DueAt <= DateTime.UtcNow);

        return Ok(new DeepStudySessionDTO
        {
            CollectionId = collectionId,
            TotalCards = cards.Count,
            DueCards = dueCount,
            Cards = cards.Select(MapCard).ToList(),
        });
    }

    [HttpPost("deep/answer")]
    public async Task<ActionResult<DeepStudyAnswerResultDTO>> CheckDeepAnswerAsync([FromBody] DeepStudyAnswerRequestDTO request)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
        {
            return Unauthorized(new { Error = "Invalid or unsupported token subject." });
        }

        if (string.IsNullOrWhiteSpace(request.CardId))
        {
            return BadRequest(new { Error = "cardId is required." });
        }

        var card = await _cards.Find(item => item.Id == request.CardId && item.UserId == userId.Value).FirstOrDefaultAsync();
        if (card is null)
        {
            return NotFound(new { Error = "Card not found." });
        }

        var normalizedExpected = TextNormalizer.NormalizeForCompare(card.Back);
        var normalizedAnswer = TextNormalizer.NormalizeForCompare(request.Answer);
        var isCorrect = !string.IsNullOrWhiteSpace(normalizedAnswer)
            && normalizedAnswer.Equals(normalizedExpected, StringComparison.Ordinal);

        return Ok(new DeepStudyAnswerResultDTO
        {
            CardId = card.Id,
            IsCorrect = isCorrect,
            ExpectedAnswer = card.Back,
            SubmittedAnswer = request.Answer,
            RecommendedDifficulty = isCorrect ? "easy" : "hard",
        });
    }

    private async Task TouchDeskAsync(string collectionId, Guid userId)
    {
        await _desks.UpdateOneAsync(
            desk => desk.Id == collectionId && desk.UserId == userId,
            Builders<DeskDocument>.Update.Set(desk => desk.UpdatedAt, DateTime.UtcNow));
    }

    private static int NextIntervalDays(string difficulty, int streak)
    {
        var baseDays = difficulty switch
        {
            "hard" => 1,
            "medium" => 2,
            _ => 4,
        };

        return baseDays + Math.Max(streak - 1, 0);
    }

    private static StudyCardDTO MapCard(CardDocument card)
    {
        return new StudyCardDTO
        {
            Id = card.Id,
            CollectionId = card.DeskId,
            DeckId = card.DeskId,
            Front = card.Front,
            Back = card.Back,
            Hint = card.Hint,
            Tags = card.Tags,
            DueAt = card.DueAt.ToString("O"),
            LastReviewedAt = card.LastReviewedAt?.ToString("O"),
            Streak = card.Streak,
        };
    }
}
