using Application.Dtos.WordsNote;
using Application.IServices.AS;
using Domain.Entities.WordsNote;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace FeatureFusion.Controllers.WordsNote
{
    [ApiController]
    [Authorize]
    [Route("api/card")]
    [Route("api/cards")]
    public class CardController : ControllerBase
    {
        private const string DeskCollectionName = "wordsnote_desks";
        private const string CardCollectionName = "wordsnote_cards";

        private readonly IMongoCollection<CardDocument> _cards;
        private readonly IMongoCollection<DeskDocument> _desks;
        private readonly ICurrentUserService _currentUserService;

        public CardController(IMongoDatabase database, ICurrentUserService currentUserService)
        {
            _cards = database.GetCollection<CardDocument>(CardCollectionName);
            _desks = database.GetCollection<DeskDocument>(DeskCollectionName);
            _currentUserService = currentUserService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<StudyCardDTO>>> GetAllAsync(
            [FromQuery] string? collectionId = null,
            [FromQuery(Name = "deckId")] string? deckId = null,
            [FromQuery(Name = "deskId")] string? deskId = null)
        {
            AddLegacyRouteHeader();

            var userId = _currentUserService.UserId;
            var filter = userId is null
                ? Builders<CardDocument>.Filter.Empty
                : Builders<CardDocument>.Filter.Eq(card => card.UserId, userId.Value);
            var resolvedCollectionId = ResolveCollectionId(collectionId, deckId, deskId);

            if (!string.IsNullOrWhiteSpace(resolvedCollectionId))
            {
                filter &= Builders<CardDocument>.Filter.Eq(card => card.DeskId, resolvedCollectionId);
            }

            var cards = await _cards.Find(filter)
                .SortBy(card => card.DueAt)
                .ThenBy(card => card.Front)
                .ToListAsync();

            return Ok(cards.Select(MapCard));
        }

        [HttpGet("due")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<StudyCardDTO>>> GetDueAsync(
            [FromQuery] string? collectionId = null,
            [FromQuery(Name = "deckId")] string? deckId = null,
            [FromQuery(Name = "deskId")] string? deskId = null)
        {
            AddLegacyRouteHeader();

            var userId = _currentUserService.UserId;
            var filter = userId is null
                ? Builders<CardDocument>.Filter.Empty
                : Builders<CardDocument>.Filter.Eq(card => card.UserId, userId.Value);
            var resolvedCollectionId = ResolveCollectionId(collectionId, deckId, deskId);
            if (!string.IsNullOrWhiteSpace(resolvedCollectionId))
            {
                filter &= Builders<CardDocument>.Filter.Eq(card => card.DeskId, resolvedCollectionId);
            }

            var now = DateTime.UtcNow;
            filter &= Builders<CardDocument>.Filter.Lte(card => card.DueAt, now);

            var cards = await _cards.Find(filter)
                .SortBy(card => card.DueAt)
                .ThenBy(card => card.Front)
                .ToListAsync();

            return Ok(cards.Select(MapCard));
        }

        [HttpPost]
        public async Task<ActionResult<StudyCardDTO>> CreateAsync([FromBody] CardUpsertRequestDTO request)
        {
            AddLegacyRouteHeader();

            var userId = _currentUserService.UserId;
            if (userId is null)
            {
                return Unauthorized(new { Error = "Invalid or unsupported token subject." });
            }

            var collectionId = request.ResolveCollectionId();
            var desk = await RequireDeskAsync(collectionId);
            if (desk is null)
            {
                return NotFound(new { Error = "Collection not found." });
            }

            var front = request.Front.Trim();
            var back = request.Back.Trim();
            if (string.IsNullOrWhiteSpace(front) || string.IsNullOrWhiteSpace(back))
            {
                return BadRequest(new { Error = "Front and back are required." });
            }

            var now = DateTime.UtcNow;
            var card = new CardDocument
            {
                Id = CreateId("card"),
                DeskId = desk.Id,
                UserId = userId.Value,
                Front = front,
                Back = back,
                Hint = NormalizeHint(request.Hint),
                Tags = NormalizeTags(request.Tags),
                DueAt = now,
                Streak = 0,
            };

            await _cards.InsertOneAsync(card);
            await TouchDeskAsync(desk.Id);
            return Created($"/api/cards/{card.Id}", MapCard(card));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<StudyCardDTO>> UpdateAsync(string id, [FromBody] CardUpsertRequestDTO request)
        {
            AddLegacyRouteHeader();

            var userId = _currentUserService.UserId;
            if (userId is null)
            {
                return Unauthorized(new { Error = "Invalid or unsupported token subject." });
            }

            var card = await _cards.Find(item => item.Id == id && item.UserId == userId.Value).FirstOrDefaultAsync();
            if (card is null)
            {
                return NotFound();
            }

            var front = request.Front.Trim();
            var back = request.Back.Trim();
            if (string.IsNullOrWhiteSpace(front) || string.IsNullOrWhiteSpace(back))
            {
                return BadRequest(new { Error = "Front and back are required." });
            }

            card.Front = front;
            card.Back = back;
            card.Hint = NormalizeHint(request.Hint);
            card.Tags = NormalizeTags(request.Tags);

            await _cards.ReplaceOneAsync(item => item.Id == id && item.UserId == userId.Value, card);
            await TouchDeskAsync(card.DeskId);
            return Ok(MapCard(card));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(string id)
        {
            AddLegacyRouteHeader();

            var userId = _currentUserService.UserId;
            if (userId is null)
            {
                return Unauthorized(new { Error = "Invalid or unsupported token subject." });
            }

            var existing = await _cards.Find(card => card.Id == id && card.UserId == userId.Value).FirstOrDefaultAsync();
            if (existing is null)
            {
                return NotFound();
            }

            await _cards.DeleteOneAsync(card => card.Id == id && card.UserId == userId.Value);
            await TouchDeskAsync(existing.DeskId);
            return NoContent();
        }

        [HttpPost("import")]
        public async Task<ActionResult<ImportCardsResultDTO>> ImportAsync([FromBody] CardImportRequestDTO request)
        {
            AddLegacyRouteHeader();

            var userId = _currentUserService.UserId;
            if (userId is null)
            {
                return Unauthorized(new { Error = "Invalid or unsupported token subject." });
            }

            var collectionId = request.ResolveCollectionId();
            var desk = await RequireDeskAsync(collectionId);
            if (desk is null)
            {
                return NotFound(new { Error = "Collection not found." });
            }

            var lines = request.RawText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            var now = DateTime.UtcNow;
            var cardsToInsert = new List<CardDocument>();
            var skipped = 0;

            foreach (var line in lines)
            {
                var normalized = line.Trim();
                if (string.IsNullOrWhiteSpace(normalized))
                {
                    continue;
                }

                var separatorIndex = normalized.IndexOf(':');
                if (separatorIndex <= 0 || separatorIndex == normalized.Length - 1)
                {
                    skipped += 1;
                    continue;
                }

                var front = normalized[..separatorIndex].Trim();
                var back = normalized[(separatorIndex + 1)..].Trim();
                if (string.IsNullOrWhiteSpace(front) || string.IsNullOrWhiteSpace(back))
                {
                    skipped += 1;
                    continue;
                }

                cardsToInsert.Add(new CardDocument
                {
                    Id = CreateId("card"),
                    DeskId = desk.Id,
                    UserId = userId.Value,
                    Front = front,
                    Back = back,
                    Tags = [],
                    DueAt = now,
                    Streak = 0,
                });
            }

            if (cardsToInsert.Count > 0)
            {
                await _cards.InsertManyAsync(cardsToInsert);
                await TouchDeskAsync(desk.Id);
            }

            return Ok(new ImportCardsResultDTO
            {
                Imported = cardsToInsert.Count,
                Skipped = skipped,
            });
        }

        [HttpPost("{id}/review")]
        public async Task<ActionResult<StudyCardDTO>> ReviewAsync(string id, [FromBody] CardReviewRequestDTO request)
        {
            AddLegacyRouteHeader();

            var userId = _currentUserService.UserId;
            if (userId is null)
            {
                return Unauthorized(new { Error = "Invalid or unsupported token subject." });
            }

            var card = await _cards.Find(item => item.Id == id && item.UserId == userId.Value).FirstOrDefaultAsync();
            if (card is null)
            {
                return NotFound();
            }

            var difficulty = request.Difficulty.Trim().ToLowerInvariant();
            if (difficulty is not ("hard" or "medium" or "easy"))
            {
                return BadRequest(new { Error = "Difficulty must be hard, medium, or easy." });
            }

            var streak = difficulty == "hard" ? 0 : card.Streak + 1;
            var intervalDays = NextIntervalDays(difficulty, streak);
            var now = DateTime.UtcNow;

            card.Streak = streak;
            card.LastReviewedAt = now;
            card.DueAt = now.AddDays(intervalDays);

            await _cards.ReplaceOneAsync(item => item.Id == id && item.UserId == userId.Value, card);
            await TouchDeskAsync(card.DeskId);
            return Ok(MapCard(card));
        }

        private async Task<DeskDocument?> RequireDeskAsync(string deskId)
        {
            if (string.IsNullOrWhiteSpace(deskId))
            {
                return null;
            }

            var userId = _currentUserService.UserId;
            if (userId is null)
            {
                return null;
            }

            return await _desks.Find(desk => desk.Id == deskId && desk.UserId == userId.Value).FirstOrDefaultAsync();
        }

        private async Task TouchDeskAsync(string deskId)
        {
            var userId = _currentUserService.UserId;
            if (userId is null)
            {
                return;
            }

            await _desks.UpdateOneAsync(
                desk => desk.Id == deskId && desk.UserId == userId.Value,
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

        private static string? NormalizeHint(string? hint)
        {
            var normalized = hint?.Trim();
            return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
        }

        private static List<string> NormalizeTags(IEnumerable<string>? tags)
        {
            return tags?
                .Select(tag => tag.Trim())
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList() ?? [];
        }

        private static string CreateId(string prefix)
        {
            return $"{prefix}-{Guid.NewGuid():N}";
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

        private static string? ResolveCollectionId(string? collectionId, string? deckId, string? deskId)
        {
            if (!string.IsNullOrWhiteSpace(collectionId))
            {
                return collectionId;
            }

            if (!string.IsNullOrWhiteSpace(deckId))
            {
                return deckId;
            }

            if (!string.IsNullOrWhiteSpace(deskId))
            {
                return deskId;
            }

            return null;
        }

        private void AddLegacyRouteHeader()
        {
            if (Request.Path.StartsWithSegments("/api/card"))
            {
                Response.Headers["X-Deprecated-Route"] = "Route /api/card is deprecated. Use /api/cards.";
            }
        }
    }
}