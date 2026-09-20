using System.Collections.Concurrent;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Dtos.WordsNote;
using Domain.Entities.AS;
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
    private const string UnlockKeysCollectionName = "wordsnote_unlock_keys";
    private const string UsersCollectionName = "wordsnote_users";

    private readonly IMongoCollection<QuizSetDocument> _quizSets;
    private readonly IMongoCollection<QuestionDocument> _questions;
    private readonly IMongoCollection<UnlockKeyDocument> _unlockKeys;
    private readonly IMongoCollection<User> _users;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<QuizSetsController> _logger;
    private readonly IConfiguration _configuration;

    public QuizSetsController(
        IMongoDatabase database,
        IWebHostEnvironment env,
        ILogger<QuizSetsController> logger,
        IConfiguration configuration)
    {
        _quizSets = database.GetCollection<QuizSetDocument>(QuizSetsCollectionName);
        _questions = database.GetCollection<QuestionDocument>(QuestionsCollectionName);
        _unlockKeys = database.GetCollection<UnlockKeyDocument>(UnlockKeysCollectionName);
        _users = database.GetCollection<User>(UsersCollectionName);
        _env = env;
        _logger = logger;
        _configuration = configuration;
    }

    [HttpGet]
    [HttpGet("/api/catalog")]
    [AllowAnonymous]
    public async Task<ActionResult> GetAllAsync()
    {
        await EnsureSeededAsync();

        var (email, isAdmin, unlockedSubjects) = await GetCurrentUserContextAsync();

        var sets = await _quizSets.Find(FilterDefinition<QuizSetDocument>.Empty)
            .SortBy(s => s.Code)
            .ToListAsync();

        var dtoList = sets.Select(s => new QuizSetDTO
        {
            Id = s.Id,
            Code = s.Code,
            Title = s.Title,
            Description = s.Description,
            Color = s.Color,
            TotalQuestions = s.TotalQuestions,
            IsRestricted = s.IsRestricted,
            IsUnlocked = !s.IsRestricted || isAdmin || unlockedSubjects.Contains(s.Id.ToLowerInvariant())
        }).ToList();

        if (Request.Path.Value?.Contains("/api/catalog") == true)
        {
            return Ok(new { subjects = dtoList });
        }

        return Ok(dtoList);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<QuizSetDTO>> GetByIdAsync(string id)
    {
        await EnsureSeededAsync();

        var (email, isAdmin, unlockedSubjects) = await GetCurrentUserContextAsync();

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
            TotalQuestions = set.TotalQuestions,
            IsRestricted = set.IsRestricted,
            IsUnlocked = !set.IsRestricted || isAdmin || unlockedSubjects.Contains(set.Id.ToLowerInvariant())
        });
    }

    [HttpGet("{id}/questions")]
    [HttpGet("/api/questions/{id}")]
    [AllowAnonymous]
    public async Task<ActionResult> GetQuestionsAsync(
        string id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 1000,
        [FromQuery] bool exam = false,
        [FromQuery] int examCount = 40,
        [FromQuery] string? search = null)
    {
        await EnsureSeededAsync();

        var (email, isAdmin, unlockedSubjects) = await GetCurrentUserContextAsync();

        var set = await _quizSets.Find(s => s.Id == id).FirstOrDefaultAsync();
        if (set != null && set.IsRestricted)
        {
            var isUnlocked = isAdmin || unlockedSubjects.Contains(id.ToLowerInvariant());
            if (!isUnlocked)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    error = "RESTRICTED_SUBJECT",
                    message = $"Môn {set.Code} đang bị khóa. Bạn cần đăng nhập và có mã mở khóa 1 lần để học môn này."
                });
            }
        }

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
                return Ok(new { subject = set, questions = Enumerable.Empty<QuestionDTO>() });
            }

            var randomSample = allQuestions
                .OrderBy(_ => Random.Shared.Next())
                .Take(count)
                .Select(MapQuestionDTO)
                .ToList();

            if (Request.Path.Value?.StartsWith("/api/questions") == true)
            {
                return Ok(new { subject = set, questions = randomSample });
            }

            return Ok(randomSample);
        }

        var p = Math.Max(1, page);
        var size = Math.Clamp(pageSize, 1, 2000);

        var questions = await _questions.Find(filter)
            .SortBy(q => q.QuestionNumber)
            .Skip((p - 1) * size)
            .Limit(size)
            .ToListAsync();

        var dtos = questions.Select(MapQuestionDTO).ToList();

        if (Request.Path.Value?.StartsWith("/api/questions") == true)
        {
            return Ok(new { subject = set, questions = dtos });
        }

        return Ok(dtos);
    }

    private static readonly ConcurrentDictionary<string, (int Attempts, DateTime ResetTime)> _rateLimits = new();

    private static bool IsRateLimited(string clientKey)
    {
        var now = DateTime.UtcNow;
        if (_rateLimits.TryGetValue(clientKey, out var entry))
        {
            if (now > entry.ResetTime)
            {
                _rateLimits.TryRemove(clientKey, out _);
                return false;
            }
            return entry.Attempts >= 5;
        }
        return false;
    }

    private static void RecordFailedAttempt(string clientKey)
    {
        var now = DateTime.UtcNow;
        _rateLimits.AddOrUpdate(
            clientKey,
            _ => (1, now.AddMinutes(5)),
            (_, existing) =>
            {
                if (now > existing.ResetTime)
                {
                    return (1, now.AddMinutes(5));
                }
                return (existing.Attempts + 1, existing.ResetTime);
            });
    }

    private static void ClearRateLimit(string clientKey)
    {
        _rateLimits.TryRemove(clientKey, out _);
    }

    [HttpPost("unlock")]
    [HttpPost("/api/unlock")]
    [AllowAnonymous]
    public async Task<IActionResult> RedeemUnlockKeyAsync([FromBody] RedeemKeyRequestDTO request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Code))
        {
            return BadRequest(new { error = "MISSING_CODE", message = "Vui lòng nhập mã mở khóa." });
        }

        var cleanCode = request.Code.Trim().ToUpperInvariant();

        var (email, isAdmin, _) = await GetCurrentUserContextAsync();
        if (string.IsNullOrWhiteSpace(email))
        {
            return Unauthorized(new { error = "UNAUTHORIZED", message = "Vui lòng đăng nhập bằng Google trước khi mở khóa." });
        }

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var rateLimitKey = $"{email.ToLowerInvariant()}:{ip}";

        if (IsRateLimited(rateLimitKey))
        {
            return StatusCode(StatusCodes.Status429TooManyRequests, new
            {
                error = "TOO_MANY_ATTEMPTS",
                message = "Bạn đã nhập sai mã mở khóa quá nhiều lần. Vui lòng đợi 5 phút trước khi thử lại."
            });
        }

        // Atomic update: only update if matching code and NOT yet used (prevents race conditions)
        var updateDef = Builders<UnlockKeyDocument>.Update
            .Set(k => k.IsUsed, true)
            .Set(k => k.UsedAt, DateTime.UtcNow)
            .Set(k => k.UsedByEmail, email);

        var keyRecord = await _unlockKeys.FindOneAndUpdateAsync(
            k => k.Code == cleanCode && !k.IsUsed,
            updateDef,
            new FindOneAndUpdateOptions<UnlockKeyDocument> { ReturnDocument = ReturnDocument.After }
        );

        if (keyRecord == null)
        {
            RecordFailedAttempt(rateLimitKey);

            var existingKey = await _unlockKeys.Find(k => k.Code == cleanCode).FirstOrDefaultAsync();
            if (existingKey != null && existingKey.IsUsed)
            {
                return BadRequest(new
                {
                    error = "KEY_ALREADY_USED",
                    message = $"Mã này đã được sử dụng bởi {existingKey.UsedByEmail ?? "người khác"} vào {existingKey.UsedAt:dd/MM/yyyy HH:mm}."
                });
            }

            return BadRequest(new { error = "INVALID_KEY", message = "Mã mở khóa không tồn tại hoặc sai ký tự." });
        }

        // Successful redemption clears rate limit tracker
        ClearRateLimit(rateLimitKey);

        // Update user's unlocked subjects
        var user = await _users.Find(u => u.Email != null && u.Email.ToLower() == email.ToLower()).FirstOrDefaultAsync();
        if (user == null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                UserName = email.Split('@')[0],
                Email = email,
                PasswordHash = string.Empty,
                Name = email.Split('@')[0],
                Role = "User",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true,
                UnlockedSubjects = new List<string>()
            };
            await _users.InsertOneAsync(user);
        }

        var targets = keyRecord.TargetSubjects != null && keyRecord.TargetSubjects.Count > 0
            ? keyRecord.TargetSubjects
            : new List<string> { "jfe301", "jit401" };

        var normalizedTargets = targets.Select(t => t.Trim().ToLowerInvariant()).ToList();
        var userUpdate = Builders<User>.Update
            .AddToSetEach(u => u.UnlockedSubjects, normalizedTargets)
            .Set(u => u.UpdatedAt, DateTime.UtcNow);

        await _users.UpdateOneAsync(u => u.Id == user.Id, userUpdate);

        var updatedUser = await _users.Find(u => u.Id == user.Id).FirstOrDefaultAsync();
        var targetLabels = string.Join(", ", targets.Select(t => t.ToUpperInvariant()));

        return Ok(new RedeemKeyResponseDTO
        {
            Success = true,
            Message = $"Mở khóa thành công môn ({targetLabels})!",
            UnlockedSubjects = updatedUser?.UnlockedSubjects ?? normalizedTargets
        });
    }

    [HttpGet("admin/keys")]
    [HttpGet("/api/admin/keys")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAdminKeysAsync()
    {
        var (_, isAdmin, _) = await GetCurrentUserContextAsync();
        if (!isAdmin)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = "ADMIN_UNAUTHORIZED", message = "Bạn không có quyền Admin. Yêu cầu đăng nhập tài khoản Google Admin." });
        }

        var keys = await _unlockKeys.Find(FilterDefinition<UnlockKeyDocument>.Empty)
            .SortByDescending(k => k.CreatedAt)
            .ToListAsync();

        var dtos = keys.Select(k => new UnlockKeyDTO
        {
            Id = k.Id,
            Code = k.Code,
            IsUsed = k.IsUsed,
            UsedAt = k.UsedAt,
            UsedByEmail = k.UsedByEmail,
            TargetSubjects = k.TargetSubjects,
            CreatedAt = k.CreatedAt
        });

        return Ok(new { keys = dtos });
    }

    [HttpPost("admin/keys")]
    [HttpPost("admin/keys/generate")]
    [HttpPost("/api/admin/keys")]
    [AllowAnonymous]
    public async Task<IActionResult> GenerateKeysAsync([FromBody] GenerateKeysRequestDTO request)
    {
        var (_, isAdmin, _) = await GetCurrentUserContextAsync();
        if (!isAdmin)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = "ADMIN_UNAUTHORIZED", message = "Bạn không có quyền Admin. Yêu cầu đăng nhập tài khoản Google Admin." });
        }

        var allSubjects = new List<string> { "mln122", "prm393", "jfe301", "jit401" };
        var validTargets = request.TargetSubjects != null && request.TargetSubjects.Count > 0
            ? request.TargetSubjects.Where(s => allSubjects.Contains(s.ToLowerInvariant())).Select(s => s.ToLowerInvariant()).ToList()
            : new List<string> { "jfe301", "jit401" };

        if (validTargets.Count == 0)
        {
            validTargets = new List<string> { "jfe301", "jit401" };
        }

        var createdKeys = new List<UnlockKeyDocument>();

        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            var clean = request.Code.Trim().ToUpperInvariant();
            var existing = await _unlockKeys.Find(k => k.Code == clean).FirstOrDefaultAsync();
            if (existing != null)
            {
                return BadRequest(new { error = "KEY_EXISTS", message = "Mã này đã tồn tại trong hệ thống." });
            }

            var newKey = new UnlockKeyDocument
            {
                Code = clean,
                IsUsed = false,
                TargetSubjects = validTargets,
                CreatedAt = DateTime.UtcNow
            };
            await _unlockKeys.InsertOneAsync(newKey);
            createdKeys.Add(newKey);
        }
        else
        {
            var count = Math.Clamp(request.BatchCount, 1, 50);
            for (int i = 0; i < count; i++)
            {
                var randomCode = Convert.ToHexString(RandomNumberGenerator.GetBytes(8)).ToUpperInvariant();
                var newKey = new UnlockKeyDocument
                {
                    Code = randomCode,
                    IsUsed = false,
                    TargetSubjects = validTargets,
                    CreatedAt = DateTime.UtcNow
                };
                await _unlockKeys.InsertOneAsync(newKey);
                createdKeys.Add(newKey);
            }
        }

        var dtos = createdKeys.Select(k => new UnlockKeyDTO
        {
            Id = k.Id,
            Code = k.Code,
            IsUsed = k.IsUsed,
            UsedAt = k.UsedAt,
            UsedByEmail = k.UsedByEmail,
            TargetSubjects = k.TargetSubjects,
            CreatedAt = k.CreatedAt
        }).ToList();

        return Ok(new
        {
            success = true,
            message = $"Đã tạo thành công {dtos.Count} mã mở khóa 16 ký tự mới cho [{string.Join(", ", validTargets.Select(t => t.ToUpperInvariant()))}].",
            keys = dtos
        });
    }

    [HttpDelete("admin/keys/{code}")]
    [HttpDelete("/api/admin/keys/{code}")]
    [AllowAnonymous]
    public async Task<IActionResult> DeleteKeyAsync(string code)
    {
        var (_, isAdmin, _) = await GetCurrentUserContextAsync();
        if (!isAdmin)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = "ADMIN_UNAUTHORIZED", message = "Bạn không có quyền Admin. Yêu cầu đăng nhập tài khoản Google Admin." });
        }

        var filter = Builders<UnlockKeyDocument>.Filter.Or(
            Builders<UnlockKeyDocument>.Filter.Eq(k => k.Code, code.ToUpperInvariant()),
            Builders<UnlockKeyDocument>.Filter.Eq(k => k.Id, code)
        );

        var result = await _unlockKeys.DeleteOneAsync(filter);
        if (result.DeletedCount == 0)
        {
            return NotFound(new { error = "KEY_NOT_FOUND", message = "Không tìm thấy mã mở khóa để xóa." });
        }

        return Ok(new { success = true, message = "Đã xóa mã mở khóa." });
    }

    [HttpGet("admin/users")]
    [HttpGet("/api/admin/users")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAdminUsersAsync()
    {
        var (_, isAdmin, _) = await GetCurrentUserContextAsync();
        if (!isAdmin)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = "ADMIN_UNAUTHORIZED", message = "Bạn không có quyền Admin. Yêu cầu đăng nhập tài khoản Google Admin." });
        }

        var users = await _users.Find(FilterDefinition<User>.Empty)
            .SortByDescending(u => u.UpdatedAt)
            .ToListAsync();

        var adminEmail = (_configuration["AuthProviders:Google:AdminEmail"] ?? "hnt.vn.vn@gmail.com").Trim().ToLowerInvariant();

        var dtos = users.Select(u => new UserAccessDTO
        {
            Id = u.Id.ToString(),
            Email = u.Email ?? string.Empty,
            Name = u.Name,
            AvatarUrl = u.AvatarUrl,
            UnlockedSubjects = u.UnlockedSubjects ?? new List<string>(),
            IsAdmin = string.Equals(u.Email?.Trim().ToLowerInvariant(), adminEmail, StringComparison.Ordinal)
        }).ToList();

        return Ok(new { users = dtos });
    }

    [HttpPost("admin/users/grant")]
    [HttpPost("/api/admin/users/grant")]
    [AllowAnonymous]
    public async Task<IActionResult> GrantUserAccessAsync([FromBody] GrantUserAccessRequestDTO request)
    {
        var (_, isAdmin, _) = await GetCurrentUserContextAsync();
        if (!isAdmin)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = "ADMIN_UNAUTHORIZED", message = "Bạn không có quyền Admin. Yêu cầu đăng nhập tài khoản Google Admin." });
        }

        if (string.IsNullOrWhiteSpace(request?.Email))
        {
            return BadRequest(new { error = "MISSING_EMAIL", message = "Vui lòng cung cấp email học viên." });
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _users.Find(u => u.Email != null && u.Email.ToLower() == normalizedEmail).FirstOrDefaultAsync();
        if (user == null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                UserName = normalizedEmail.Split('@')[0],
                Email = normalizedEmail,
                PasswordHash = string.Empty,
                Name = normalizedEmail.Split('@')[0],
                Role = "User",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true,
                UnlockedSubjects = new List<string>()
            };
            await _users.InsertOneAsync(user);
        }

        foreach (var sub in request.Subjects)
        {
            var normalizedSub = sub.Trim().ToLowerInvariant();
            if (!user.UnlockedSubjects.Contains(normalizedSub))
            {
                user.UnlockedSubjects.Add(normalizedSub);
            }
        }

        user.UpdatedAt = DateTime.UtcNow;
        await _users.ReplaceOneAsync(u => u.Id == user.Id, user);

        return Ok(new
        {
            success = true,
            message = $"Đã cấp quyền mở khóa các môn [{string.Join(", ", request.Subjects.Select(s => s.ToUpperInvariant()))}] cho {user.Email}.",
            user = new UserAccessDTO
            {
                Id = user.Id.ToString(),
                Email = user.Email ?? string.Empty,
                Name = user.Name,
                AvatarUrl = user.AvatarUrl,
                UnlockedSubjects = user.UnlockedSubjects,
                IsAdmin = false
            }
        });
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

            var cleanUserAnswers = userAnswers.Select(a => a.Trim().ToUpperInvariant()).OrderBy(a => a).ToList();
            var cleanCorrectAnswers = question.Answers.Select(a => a.Trim().ToUpperInvariant()).OrderBy(a => a).ToList();

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
                UserAnswers = userAnswers,
                CorrectAnswers = question.Answers,
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
    public async Task<ActionResult> ForceSeedAsync()
    {
        var (email, isAdmin, _) = await GetCurrentUserContextAsync();
        if (!isAdmin)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { Error = "Admin access required." });
        }
        var count = await SeedFromFilesAsync(force: true);
        return Ok(new { Message = $"Quiz banks successfully seeded with {count} questions." });
    }

    private async Task<(string? Email, bool IsAdmin, List<string> UnlockedSubjects)> GetCurrentUserContextAsync()
    {
        string? email = null;
        bool isAdmin = false;
        var unlockedSubjects = new List<string>();

        // Check JWT or Claim strictly (No header spoofing or secret bypass)
        if (User?.Identity?.IsAuthenticated == true)
        {
            email = User.FindFirst(ClaimTypes.Email)?.Value
                ?? User.FindFirst("email")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (User.IsInRole("Admin"))
            {
                isAdmin = true;
            }
        }

        if (!string.IsNullOrWhiteSpace(email))
        {
            var adminEmail = (_configuration["AuthProviders:Google:AdminEmail"]
                ?? _configuration["ADMIN_EMAIL"]
                ?? "hnt.vn.vn@gmail.com").Trim().ToLowerInvariant();

            if (string.Equals(email.Trim().ToLowerInvariant(), adminEmail, StringComparison.Ordinal))
            {
                isAdmin = true;
            }

            var user = await _users.Find(u => u.Email != null && u.Email.ToLower() == email.Trim().ToLowerInvariant()).FirstOrDefaultAsync();
            if (user != null)
            {
                if (string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase))
                {
                    isAdmin = true;
                }

                if (user.UnlockedSubjects != null)
                {
                    unlockedSubjects.AddRange(user.UnlockedSubjects.Select(s => s.ToLowerInvariant()));
                }
            }
        }

        return (email, isAdmin, unlockedSubjects);
    }

    private static bool _metadataSynced = false;

    private async Task EnsureSeededAsync()
    {
        var count = await _quizSets.CountDocumentsAsync(FilterDefinition<QuizSetDocument>.Empty);
        if (count == 0)
        {
            await SeedFromFilesAsync(force: false);
            _metadataSynced = true;
        }
        else if (!_metadataSynced)
        {
            await SyncCatalogMetadataAsync();
            _metadataSynced = true;
        }
    }

    private async Task SyncCatalogMetadataAsync()
    {
        try
        {
            var dataDirectory = Path.Combine(_env.ContentRootPath, "Data", "QuizBanks");
            var catalogFile = Path.Combine(dataDirectory, "catalog.json");
            if (!System.IO.File.Exists(catalogFile))
            {
                return;
            }

            var catalogJson = await System.IO.File.ReadAllTextAsync(catalogFile);
            var catalog = JsonSerializer.Deserialize<CatalogJsonModel>(catalogJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (catalog?.Subjects == null) return;

            foreach (var sub in catalog.Subjects)
            {
                var update = Builders<QuizSetDocument>.Update
                    .Set(s => s.IsRestricted, sub.IsRestricted)
                    .Set(s => s.UpdatedAt, DateTime.UtcNow);

                await _quizSets.UpdateOneAsync(s => s.Id == sub.Id, update);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing catalog metadata from catalog.json");
        }
    }

    private async Task<int> SeedFromFilesAsync(bool force)
    {
        var dataDirectory = Path.Combine(_env.ContentRootPath, "Data", "QuizBanks");
        if (!Directory.Exists(dataDirectory))
        {
            _logger.LogWarning("QuizBanks data directory not found at {Path}", dataDirectory);
            return 0;
        }

        var catalogFile = Path.Combine(dataDirectory, "catalog.json");
        if (!System.IO.File.Exists(catalogFile))
        {
            _logger.LogWarning("catalog.json not found at {Path}", catalogFile);
            return 0;
        }

        var catalogJson = await System.IO.File.ReadAllTextAsync(catalogFile);
        var catalog = JsonSerializer.Deserialize<CatalogJsonModel>(catalogJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (catalog?.Subjects == null)
        {
            return 0;
        }

        var totalQuestions = 0;

        foreach (var sub in catalog.Subjects)
        {
            var subjectFile = Path.Combine(dataDirectory, $"{sub.Id}.json");
            if (!System.IO.File.Exists(subjectFile))
            {
                continue;
            }

            var subjectJson = await System.IO.File.ReadAllTextAsync(subjectFile);
            var subjectData = JsonSerializer.Deserialize<SubjectFileJsonModel>(subjectJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (subjectData?.Questions == null)
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
                IsRestricted = sub.IsRestricted,
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
        public bool IsRestricted { get; set; }
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
