using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Dtos.WordsNote;
using Domain.Entities.WordsNote;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace FeatureFusion.Controllers.WordsNote;

[ApiController]
[Route("api/quiz-sets")]
public class QuizSetsController : ControllerBase
{
    private const string QuizSetsCollectionName = "wordsnote_quiz_sets";
    private const string QuestionsCollectionName = "wordsnote_questions";

    private readonly IMongoCollection<QuizSetDocument> _quizSets;
    private readonly IMongoCollection<QuestionDocument> _questions;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<QuizSetsController> _logger;

    public QuizSetsController(
        IMongoDatabase database,
        IWebHostEnvironment env,
        ILogger<QuizSetsController> logger)
    {
        _quizSets = database.GetCollection<QuizSetDocument>(QuizSetsCollectionName);
        _questions = database.GetCollection<QuestionDocument>(QuestionsCollectionName);
        _env = env;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<QuizSetDTO>>> GetAllAsync()
    {
        await EnsureSeededAsync();

        var sets = await _quizSets.Find(FilterDefinition<QuizSetDocument>.Empty)
            .SortBy(s => s.Code)
            .ToListAsync();

        return Ok(sets.Select(s => new QuizSetDTO
        {
            Id = s.Id,
            Code = s.Code,
            Title = s.Title,
            Description = s.Description,
            Color = s.Color,
            TotalQuestions = s.TotalQuestions
        }));
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<QuizSetDTO>> GetByIdAsync(string id)
    {
        await EnsureSeededAsync();

        var set = await _quizSets.Find(s => s.Id == id).FirstOrDefaultAsync();
        if (set == null)
        {
            return NotFound(new { Error = $"Quiz set '{id}' not found." });
        }

        return Ok(new QuizSetDTO
        {
            Id = set.Id,
            Code = set.Code,
            Title = set.Title,
            Description = set.Description,
            Color = set.Color,
            TotalQuestions = set.TotalQuestions
        });
    }

    [HttpGet("{id}/questions")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<QuestionDTO>>> GetQuestionsAsync(
        string id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] bool exam = false,
        [FromQuery] int examCount = 40,
        [FromQuery] string? search = null)
    {
        await EnsureSeededAsync();

        var filterBuilder = Builders<QuestionDocument>.Filter;
        var filter = filterBuilder.Eq(q => q.SubjectId, id);

        if (!string.IsNullOrWhiteSpace(search))
        {
            filter &= filterBuilder.Regex(q => q.Question, new MongoDB.Bson.BsonRegularExpression(search, "i"));
        }

        if (exam)
        {
            var count = Math.Clamp(examCount, 5, 100);
            var allQuestions = await _questions.Find(filter).ToListAsync();
            if (allQuestions.Count == 0)
            {
                return Ok(Enumerable.Empty<QuestionDTO>());
            }

            var randomSample = allQuestions
                .OrderBy(_ => Random.Shared.Next())
                .Take(count)
                .Select(MapQuestionDTO);

            return Ok(randomSample);
        }

        var p = Math.Max(1, page);
        var size = Math.Clamp(pageSize, 1, 200);

        var questions = await _questions.Find(filter)
            .SortBy(q => q.QuestionNumber)
            .Skip((p - 1) * size)
            .Limit(size)
            .ToListAsync();

        return Ok(questions.Select(MapQuestionDTO));
    }

    [HttpPost("{id}/submit")]
    [AllowAnonymous]
    public async Task<ActionResult<QuizSubmitResultDTO>> SubmitQuizAsync(
        string id,
        [FromBody] QuizSubmitRequestDTO request)
    {
        if (request?.Answers == null || request.Answers.Count == 0)
        {
            return BadRequest(new { Error = "Answers payload cannot be empty." });
        }

        var questionIds = request.Answers.Keys.ToList();
        var questions = await _questions.Find(q => q.SubjectId == id && questionIds.Contains(q.Id)).ToListAsync();
        var questionMap = questions.ToDictionary(q => q.Id);

        var result = new QuizSubmitResultDTO
        {
            TotalQuestions = request.Answers.Count
        };

        foreach (var (qId, userAnswers) in request.Answers)
        {
            if (!questionMap.TryGetValue(qId, out var question))
            {
                continue;
            }

            var cleanUserAnswers = (userAnswers ?? new List<string>())
                .Select(a => a.Trim().ToUpperInvariant())
                .OrderBy(a => a)
                .ToList();

            var cleanCorrectAnswers = (question.Answers ?? new List<string>())
                .Select(a => a.Trim().ToUpperInvariant())
                .OrderBy(a => a)
                .ToList();

            var isCorrect = cleanUserAnswers.SequenceEqual(cleanCorrectAnswers);
            if (isCorrect)
            {
                result.CorrectCount++;
            }
            else
            {
                result.IncorrectCount++;
            }

            result.Details.Add(new QuestionReviewDetailDTO
            {
                QuestionId = qId,
                UserAnswers = cleanUserAnswers,
                CorrectAnswers = cleanCorrectAnswers,
                IsCorrect = isCorrect,
                Explanation = question.Explanation
            });
        }

        result.ScorePercentage = result.TotalQuestions > 0
            ? Math.Round((double)result.CorrectCount / result.TotalQuestions * 100, 2)
            : 0;

        return Ok(result);
    }

    [HttpPost("seed")]
    [AllowAnonymous]
    public async Task<ActionResult> SeedQuizBanksAsync([FromQuery] bool force = false)
    {
        var count = await SeedDataInternalAsync(force);
        return Ok(new { Message = $"Seeding completed. Total questions seeded/verified: {count}" });
    }

    private async Task EnsureSeededAsync()
    {
        var existingSets = await _quizSets.CountDocumentsAsync(FilterDefinition<QuizSetDocument>.Empty);
        if (existingSets == 0)
        {
            await SeedDataInternalAsync(force: false);
        }
    }

    private async Task<int> SeedDataInternalAsync(bool force)
    {
        var basePath = Path.Combine(_env.ContentRootPath, "Data", "QuizBanks");
        if (!Directory.Exists(basePath))
        {
            _logger.LogWarning("QuizBanks directory not found at {BasePath}", basePath);
            return 0;
        }

        var catalogPath = Path.Combine(basePath, "catalog.json");
        if (!System.IO.File.Exists(catalogPath))
        {
            _logger.LogWarning("catalog.json not found at {CatalogPath}", catalogPath);
            return 0;
        }

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var catalogJson = await System.IO.File.ReadAllTextAsync(catalogPath);
        var catalog = JsonSerializer.Deserialize<CatalogJsonModel>(catalogJson, jsonOptions);
        if (catalog?.Subjects == null)
        {
            return 0;
        }

        int totalQuestions = 0;

        foreach (var sub in catalog.Subjects)
        {
            var fileName = $"{sub.Id}.json";
            var filePath = Path.Combine(basePath, fileName);
            if (!System.IO.File.Exists(filePath))
            {
                continue;
            }

            var subjectJson = await System.IO.File.ReadAllTextAsync(filePath);
            var subjectData = JsonSerializer.Deserialize<SubjectFileJsonModel>(subjectJson, jsonOptions);
            if (subjectData?.Questions == null || subjectData.Questions.Count == 0)
            {
                continue;
            }

            // Upsert QuizSet
            var quizSetDoc = new QuizSetDocument
            {
                Id = sub.Id,
                Code = sub.Code,
                Title = sub.Name,
                Color = sub.Color ?? "#2563eb",
                TotalQuestions = subjectData.Questions.Count,
                UpdatedAt = DateTime.UtcNow
            };

            await _quizSets.ReplaceOneAsync(
                s => s.Id == sub.Id,
                quizSetDoc,
                new ReplaceOptions { IsUpsert = true });

            if (force)
            {
                await _questions.DeleteManyAsync(q => q.SubjectId == sub.Id);
            }

            var existingQuestionCount = await _questions.CountDocumentsAsync(q => q.SubjectId == sub.Id);
            if (existingQuestionCount == 0 || force)
            {
                var questionDocs = subjectData.Questions.Select(q => new QuestionDocument
                {
                    Id = $"{sub.Id}-{q.Id}",
                    SubjectId = sub.Id,
                    QuestionNumber = q.Id,
                    Question = q.Question ?? string.Empty,
                    Options = q.Options ?? new Dictionary<string, string>(),
                    Answers = q.Answers ?? new List<string>(),
                    Choose = q.Choose > 0 ? q.Choose : 1,
                    Explanation = q.Explanation,
                    Note = q.Note,
                    Source = q.Source,
                    Exam = q.Exam
                }).ToList();

                if (questionDocs.Count > 0)
                {
                    await _questions.InsertManyAsync(questionDocs, new InsertManyOptions { IsOrdered = false });
                }
            }

            totalQuestions += subjectData.Questions.Count;
        }

        return totalQuestions;
    }

    private static QuestionDTO MapQuestionDTO(QuestionDocument q)
    {
        return new QuestionDTO
        {
            Id = q.Id,
            SubjectId = q.SubjectId,
            QuestionNumber = q.QuestionNumber,
            Question = q.Question,
            Options = q.Options,
            Answers = q.Answers,
            Choose = q.Choose,
            Explanation = q.Explanation,
            Note = q.Note,
            Source = q.Source,
            Exam = q.Exam
        };
    }

    private sealed class CatalogJsonModel
    {
        public string? Title { get; set; }
        public List<CatalogSubjectJsonModel>? Subjects { get; set; }
    }

    private sealed class CatalogSubjectJsonModel
    {
        public string Id { get; set; } = default!;
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string? File { get; set; }
        public int Total { get; set; }
        public string? Color { get; set; }
    }

    private sealed class SubjectFileJsonModel
    {
        public string? Title { get; set; }
        public string? Code { get; set; }
        public string? Subject { get; set; }
        public List<QuestionItemJsonModel>? Questions { get; set; }
    }

    private sealed class QuestionItemJsonModel
    {
        public int Id { get; set; }
        public string? Question { get; set; }
        public Dictionary<string, string>? Options { get; set; }
        public List<string>? Answers { get; set; }
        public int Choose { get; set; } = 1;
        public string? Note { get; set; }
        public string? Explanation { get; set; }
        public string? Source { get; set; }
        public string? Exam { get; set; }
    }
}
