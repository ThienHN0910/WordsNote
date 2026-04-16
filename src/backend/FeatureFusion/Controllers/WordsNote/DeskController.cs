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
    [Route("api/desk")]
    [Route("api/collections")]
    public class DeskController : ControllerBase
    {
        private const string DeskCollectionName = "wordsnote_desks";
        private const string CardCollectionName = "wordsnote_cards";

        private readonly IMongoCollection<DeskDocument> _desks;
        private readonly IMongoCollection<CardDocument> _cards;
        private readonly ICurrentUserService _currentUserService;

        public DeskController(IMongoDatabase database, ICurrentUserService currentUserService)
        {
            _desks = database.GetCollection<DeskDocument>(DeskCollectionName);
            _cards = database.GetCollection<CardDocument>(CardCollectionName);
            _currentUserService = currentUserService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<StudyDeskDTO>>> GetAllAsync()
        {
            AddLegacyRouteHeader();

            var userId = _currentUserService.UserId;
            var desks = userId is null
                ? await _desks.Find(FilterDefinition<DeskDocument>.Empty)
                    .SortByDescending(desk => desk.UpdatedAt)
                    .ToListAsync()
                : await _desks.Find(desk => desk.UserId == userId.Value)
                    .SortByDescending(desk => desk.UpdatedAt)
                    .ToListAsync();

            return Ok(desks.Select(MapDesk));
        }

        [HttpPost]
        public async Task<ActionResult<StudyDeskDTO>> CreateAsync([FromBody] DeskUpsertRequestDTO request)
        {
            AddLegacyRouteHeader();

            var userId = _currentUserService.UserId;
            if (userId is null)
            {
                return Unauthorized(new { Error = "Invalid or unsupported token subject." });
            }

            var title = request.Title.Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                return BadRequest(new { Error = "Collection title is required." });
            }

            var now = DateTime.UtcNow;
            var desk = new DeskDocument
            {
                Id = CreateId("deck"),
                UserId = userId.Value,
                Title = title,
                Description = request.Description.Trim(),
                CreatedAt = now,
                UpdatedAt = now,
            };

            await _desks.InsertOneAsync(desk);
            return Created($"/api/collections/{desk.Id}", MapDesk(desk));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<StudyDeskDTO>> UpdateAsync(string id, [FromBody] DeskUpsertRequestDTO request)
        {
            AddLegacyRouteHeader();

            var userId = _currentUserService.UserId;
            if (userId is null)
            {
                return Unauthorized(new { Error = "Invalid or unsupported token subject." });
            }

            var title = request.Title.Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                return BadRequest(new { Error = "Collection title is required." });
            }

            var update = Builders<DeskDocument>.Update
                .Set(desk => desk.Title, title)
                .Set(desk => desk.Description, request.Description.Trim())
                .Set(desk => desk.UpdatedAt, DateTime.UtcNow);

            var options = new FindOneAndUpdateOptions<DeskDocument>
            {
                ReturnDocument = ReturnDocument.After,
            };

            var updated = await _desks.FindOneAndUpdateAsync(
                desk => desk.Id == id && desk.UserId == userId.Value,
                update,
                options);

            if (updated is null)
            {
                return NotFound();
            }

            return Ok(MapDesk(updated));
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

            var deskDelete = await _desks.DeleteOneAsync(desk => desk.Id == id && desk.UserId == userId.Value);

            if (deskDelete.DeletedCount == 0)
            {
                return NotFound();
            }

            await _cards.DeleteManyAsync(card => card.DeskId == id && card.UserId == userId.Value);
            return NoContent();
        }

        private void AddLegacyRouteHeader()
        {
            if (Request.Path.StartsWithSegments("/api/desk"))
            {
                Response.Headers["X-Deprecated-Route"] = "Route /api/desk is deprecated. Use /api/collections.";
            }
        }

        private static string CreateId(string prefix)
        {
            return $"{prefix}-{Guid.NewGuid():N}";
        }

        private static StudyDeskDTO MapDesk(DeskDocument desk)
        {
            return new StudyDeskDTO
            {
                Id = desk.Id,
                Title = desk.Title,
                Description = desk.Description,
                CreatedAt = desk.CreatedAt.ToString("O"),
                UpdatedAt = desk.UpdatedAt.ToString("O"),
            };
        }
    }
}