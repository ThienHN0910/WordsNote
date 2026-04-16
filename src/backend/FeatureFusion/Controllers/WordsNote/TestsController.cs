using System.Collections.Concurrent;
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
[Route("api/tests")]
public class TestsController : ControllerBase
{
    private const string DeskCollectionName = "wordsnote_desks";
    private const string CardCollectionName = "wordsnote_cards";

    private static readonly ConcurrentDictionary<string, McqSessionState> McqSessions = new();
    private static readonly ConcurrentDictionary<string, WrittenSessionState> WrittenSessions = new();

    private readonly IMongoCollection<CardDocument> _cards;
    private readonly IMongoCollection<DeskDocument> _desks;
    private readonly ICurrentUserService _currentUserService;

    public TestsController(IMongoDatabase database, ICurrentUserService currentUserService)
    {
        _cards = database.GetCollection<CardDocument>(CardCollectionName);
        _desks = database.GetCollection<DeskDocument>(DeskCollectionName);
        _currentUserService = currentUserService;
    }

    [HttpPost("mcq/start")]
    public async Task<ActionResult<McqStartResponseDTO>> StartMcqAsync([FromBody] McqStartRequestDTO request)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
        {
            return Unauthorized(new { Error = "Invalid or unsupported token subject." });
        }

        if (string.IsNullOrWhiteSpace(request.CollectionId))
        {
            return BadRequest(new { Error = "collectionId is required." });
        }

        var cards = await GetCollectionCardsAsync(userId.Value, request.CollectionId);
        if (cards.Count < 2)
        {
            return BadRequest(new { Error = "At least 2 cards are required to start MCQ test." });
        }

        var questionCount = request.QuestionCount <= 0 ? 10 : request.QuestionCount;
        questionCount = Math.Clamp(questionCount, 1, cards.Count);

        var optionCount = request.OptionCount <= 0 ? 4 : request.OptionCount;
        optionCount = Math.Clamp(optionCount, 2, 6);

        var selectedCards = Shuffle(cards).Take(questionCount).ToList();

        var responseQuestions = new List<McqQuestionDTO>();
        var stateQuestions = new List<McqQuestionState>();

        foreach (var card in selectedCards)
        {
            var questionId = $"q-{Guid.NewGuid():N}";
            var distractors = cards
                .Where(item => item.Id != card.Id)
                .Select(item => item.Back.Trim())
                .Where(item => !string.IsNullOrWhiteSpace(item) && !item.Equals(card.Back, StringComparison.OrdinalIgnoreCase))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var targetOptionCount = Math.Min(optionCount, distractors.Count + 1);

            var options = new List<string> { card.Back };
            options.AddRange(Shuffle(distractors).Take(targetOptionCount - 1));
            options = Shuffle(options).ToList();

            var correctIndexes = options
                .Select((option, index) => new { option, index })
                .Where(item => item.option.Equals(card.Back, StringComparison.Ordinal))
                .Select(item => item.index)
                .ToList();

            responseQuestions.Add(new McqQuestionDTO
            {
                QuestionId = questionId,
                Prompt = card.Front,
                Options = options,
            });

            stateQuestions.Add(new McqQuestionState
            {
                QuestionId = questionId,
                CorrectOptionIndexes = correctIndexes,
            });
        }

        var sessionId = $"mcq-{Guid.NewGuid():N}";
        McqSessions[sessionId] = new McqSessionState
        {
            SessionId = sessionId,
            UserId = userId.Value,
            CollectionId = request.CollectionId,
            Questions = stateQuestions,
            CreatedAt = DateTime.UtcNow,
        };

        return Ok(new McqStartResponseDTO
        {
            SessionId = sessionId,
            CollectionId = request.CollectionId,
            Questions = responseQuestions,
        });
    }

    [HttpPost("mcq/submit")]
    public ActionResult<McqSubmitResultDTO> SubmitMcqAsync([FromBody] McqSubmitRequestDTO request)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
        {
            return Unauthorized(new { Error = "Invalid or unsupported token subject." });
        }

        if (string.IsNullOrWhiteSpace(request.SessionId))
        {
            return BadRequest(new { Error = "sessionId is required." });
        }

        if (!McqSessions.TryGetValue(request.SessionId, out var session) || session.UserId != userId.Value)
        {
            return NotFound(new { Error = "MCQ session not found." });
        }

        var answerMap = request.Answers
            .GroupBy(item => item.QuestionId)
            .ToDictionary(group => group.Key, group => group.Last().SelectedOptionIndexes.Distinct().OrderBy(i => i).ToList());

        var results = new List<McqQuestionResultDTO>();
        var correctAnswers = 0;

        foreach (var question in session.Questions)
        {
            var selected = answerMap.TryGetValue(question.QuestionId, out var selectedIndexes)
                ? selectedIndexes
                : [];

            var normalizedCorrect = question.CorrectOptionIndexes.Distinct().OrderBy(i => i).ToList();
            var isCorrect = normalizedCorrect.SequenceEqual(selected);
            if (isCorrect)
            {
                correctAnswers += 1;
            }

            results.Add(new McqQuestionResultDTO
            {
                QuestionId = question.QuestionId,
                IsCorrect = isCorrect,
                CorrectOptionIndexes = normalizedCorrect,
                SelectedOptionIndexes = selected,
            });
        }

        McqSessions.TryRemove(request.SessionId, out _);

        var totalQuestions = session.Questions.Count;
        var scorePercent = totalQuestions == 0
            ? 0
            : Math.Round(correctAnswers * 100.0 / totalQuestions, 2);

        return Ok(new McqSubmitResultDTO
        {
            SessionId = request.SessionId,
            TotalQuestions = totalQuestions,
            CorrectAnswers = correctAnswers,
            ScorePercent = scorePercent,
            Results = results,
        });
    }

    [HttpPost("written/start")]
    public async Task<ActionResult<WrittenStartResponseDTO>> StartWrittenAsync([FromBody] WrittenStartRequestDTO request)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
        {
            return Unauthorized(new { Error = "Invalid or unsupported token subject." });
        }

        if (string.IsNullOrWhiteSpace(request.CollectionId))
        {
            return BadRequest(new { Error = "collectionId is required." });
        }

        var cards = await GetCollectionCardsAsync(userId.Value, request.CollectionId);
        if (cards.Count == 0)
        {
            return BadRequest(new { Error = "No cards found in this collection." });
        }

        var questionCount = request.QuestionCount <= 0 ? 10 : request.QuestionCount;
        questionCount = Math.Clamp(questionCount, 1, cards.Count);

        var selectedCards = Shuffle(cards).Take(questionCount).ToList();
        var responseQuestions = new List<WrittenQuestionDTO>();
        var stateQuestions = new List<WrittenQuestionState>();

        foreach (var card in selectedCards)
        {
            var questionId = $"q-{Guid.NewGuid():N}";

            responseQuestions.Add(new WrittenQuestionDTO
            {
                QuestionId = questionId,
                Prompt = card.Front,
            });

            stateQuestions.Add(new WrittenQuestionState
            {
                QuestionId = questionId,
                ExpectedAnswer = card.Back,
            });
        }

        var sessionId = $"written-{Guid.NewGuid():N}";
        WrittenSessions[sessionId] = new WrittenSessionState
        {
            SessionId = sessionId,
            UserId = userId.Value,
            CollectionId = request.CollectionId,
            Questions = stateQuestions,
            CreatedAt = DateTime.UtcNow,
        };

        return Ok(new WrittenStartResponseDTO
        {
            SessionId = sessionId,
            CollectionId = request.CollectionId,
            Questions = responseQuestions,
        });
    }

    [HttpPost("written/submit")]
    public ActionResult<WrittenSubmitResultDTO> SubmitWrittenAsync([FromBody] WrittenSubmitRequestDTO request)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
        {
            return Unauthorized(new { Error = "Invalid or unsupported token subject." });
        }

        if (string.IsNullOrWhiteSpace(request.SessionId))
        {
            return BadRequest(new { Error = "sessionId is required." });
        }

        if (!WrittenSessions.TryGetValue(request.SessionId, out var session) || session.UserId != userId.Value)
        {
            return NotFound(new { Error = "Written session not found." });
        }

        var answerMap = request.Answers
            .GroupBy(item => item.QuestionId)
            .ToDictionary(group => group.Key, group => group.Last().Answer ?? string.Empty);

        var results = new List<WrittenQuestionResultDTO>();
        var correctAnswers = 0;

        foreach (var question in session.Questions)
        {
            var submitted = answerMap.TryGetValue(question.QuestionId, out var answer)
                ? answer
                : string.Empty;

            var isCorrect = IsEquivalentAnswer(question.ExpectedAnswer, submitted);
            if (isCorrect)
            {
                correctAnswers += 1;
            }

            results.Add(new WrittenQuestionResultDTO
            {
                QuestionId = question.QuestionId,
                IsCorrect = isCorrect,
                ExpectedAnswer = question.ExpectedAnswer,
                SubmittedAnswer = submitted,
            });
        }

        WrittenSessions.TryRemove(request.SessionId, out _);

        var totalQuestions = session.Questions.Count;
        var scorePercent = totalQuestions == 0
            ? 0
            : Math.Round(correctAnswers * 100.0 / totalQuestions, 2);

        return Ok(new WrittenSubmitResultDTO
        {
            SessionId = request.SessionId,
            TotalQuestions = totalQuestions,
            CorrectAnswers = correctAnswers,
            ScorePercent = scorePercent,
            Results = results,
        });
    }

    private async Task<List<CardDocument>> GetCollectionCardsAsync(Guid userId, string collectionId)
    {
        var desk = await _desks.Find(item => item.Id == collectionId && item.UserId == userId).FirstOrDefaultAsync();
        if (desk is null)
        {
            return [];
        }

        return await _cards.Find(card => card.UserId == userId && card.DeskId == collectionId)
            .ToListAsync();
    }

    private static bool IsEquivalentAnswer(string expected, string submitted)
    {
        var normalizedExpected = TextNormalizer.NormalizeForCompare(expected);
        var normalizedSubmitted = TextNormalizer.NormalizeForCompare(submitted);

        if (string.IsNullOrWhiteSpace(normalizedSubmitted))
        {
            return false;
        }

        return normalizedExpected.Equals(normalizedSubmitted, StringComparison.Ordinal);
    }

    private static List<T> Shuffle<T>(IEnumerable<T> source)
    {
        return source.OrderBy(_ => Random.Shared.Next()).ToList();
    }

    private sealed class McqSessionState
    {
        public string SessionId { get; set; } = string.Empty;

        public Guid UserId { get; set; }

        public string CollectionId { get; set; } = string.Empty;

        public List<McqQuestionState> Questions { get; set; } = [];

        public DateTime CreatedAt { get; set; }
    }

    private sealed class McqQuestionState
    {
        public string QuestionId { get; set; } = string.Empty;

        public List<int> CorrectOptionIndexes { get; set; } = [];
    }

    private sealed class WrittenSessionState
    {
        public string SessionId { get; set; } = string.Empty;

        public Guid UserId { get; set; }

        public string CollectionId { get; set; } = string.Empty;

        public List<WrittenQuestionState> Questions { get; set; } = [];

        public DateTime CreatedAt { get; set; }
    }

    private sealed class WrittenQuestionState
    {
        public string QuestionId { get; set; } = string.Empty;

        public string ExpectedAnswer { get; set; } = string.Empty;
    }
}
