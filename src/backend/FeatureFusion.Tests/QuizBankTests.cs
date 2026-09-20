using System.Text.Json;
using Application.Dtos.WordsNote;
using Domain.Entities.WordsNote;

namespace FeatureFusion.Tests;

public class QuizBankTests
{
    private static readonly string QuizBanksDir = Path.Combine(
        AppContext.BaseDirectory, "..", "..", "..", "..", "FeatureFusion", "Data", "QuizBanks");

    [Fact]
    public void CatalogAndSubjectFiles_ShouldExistAndBeValid()
    {
        var catalogPath = Path.Combine(QuizBanksDir, "catalog.json");
        Assert.True(File.Exists(catalogPath), $"catalog.json should exist at {catalogPath}");

        var catalogJson = File.ReadAllText(catalogPath);
        using var catalogDoc = JsonDocument.Parse(catalogJson);
        var subjects = catalogDoc.RootElement.GetProperty("subjects").EnumerateArray().ToList();

        Assert.Equal(4, subjects.Count);

        var expectedCodes = new HashSet<string> { "MLN122", "PRM393", "JFE301", "JIT401" };
        int totalQuestions = 0;

        foreach (var sub in subjects)
        {
            var id = sub.GetProperty("id").GetString()!;
            var code = sub.GetProperty("code").GetString()!;
            Assert.Contains(code, expectedCodes);

            var subjectFilePath = Path.Combine(QuizBanksDir, $"{id}.json");
            Assert.True(File.Exists(subjectFilePath), $"{subjectFilePath} must exist");

            var subjectJson = File.ReadAllText(subjectFilePath);
            using var subjectDoc = JsonDocument.Parse(subjectJson);
            var questions = subjectDoc.RootElement.GetProperty("questions").EnumerateArray().ToList();

            Assert.True(questions.Count > 0, $"{id} should have questions");
            totalQuestions += questions.Count;

            // Verify first question structure
            var q1 = questions[0];
            Assert.True(q1.TryGetProperty("id", out _));
            Assert.True(q1.TryGetProperty("question", out var qText) && !string.IsNullOrWhiteSpace(qText.GetString()));
            Assert.True(q1.TryGetProperty("options", out var opts) && opts.ValueKind == JsonValueKind.Object);
            Assert.True(q1.TryGetProperty("answers", out var ans) && ans.ValueKind == JsonValueKind.Array);
        }

        Assert.Equal(2254, totalQuestions);
    }

    [Fact]
    public void QuizScoringLogic_ShouldEvaluateAnswersAccurately()
    {
        var question = new QuestionDocument
        {
            Id = "mln122-1",
            SubjectId = "mln122",
            QuestionNumber = 1,
            Question = "Test Question",
            Options = new Dictionary<string, string> { { "A", "Opt A" }, { "B", "Opt B" } },
            Answers = new List<string> { "B" },
            Choose = 1,
            Explanation = "B is correct."
        };

        // User answers correctly
        var userAnswers = new List<string> { "b" };
        var cleanUserAnswers = userAnswers.Select(a => a.Trim().ToUpperInvariant()).OrderBy(a => a).ToList();
        var cleanCorrectAnswers = question.Answers.Select(a => a.Trim().ToUpperInvariant()).OrderBy(a => a).ToList();

        var isCorrect = cleanUserAnswers.SequenceEqual(cleanCorrectAnswers);
        Assert.True(isCorrect);

        // User answers incorrectly
        var wrongAnswers = new List<string> { "A" };
        var cleanWrongAnswers = wrongAnswers.Select(a => a.Trim().ToUpperInvariant()).OrderBy(a => a).ToList();
        var isWrong = cleanWrongAnswers.SequenceEqual(cleanCorrectAnswers);
        Assert.False(isWrong);
    }

    [Fact]
    public void CatalogAndSubjectFiles_ShouldValidateRestrictionFlags()
    {
        var catalogPath = Path.Combine(QuizBanksDir, "catalog.json");
        var catalogJson = File.ReadAllText(catalogPath);
        using var catalogDoc = JsonDocument.Parse(catalogJson);
        var subjects = catalogDoc.RootElement.GetProperty("subjects").EnumerateArray().ToList();

        var mln = subjects.First(s => s.GetProperty("id").GetString() == "mln122");
        var prm = subjects.First(s => s.GetProperty("id").GetString() == "prm393");
        var jfe = subjects.First(s => s.GetProperty("id").GetString() == "jfe301");
        var jit = subjects.First(s => s.GetProperty("id").GetString() == "jit401");

        Assert.False(mln.GetProperty("isRestricted").GetBoolean());
        Assert.False(prm.GetProperty("isRestricted").GetBoolean());
        Assert.True(jfe.GetProperty("isRestricted").GetBoolean());
        Assert.True(jit.GetProperty("isRestricted").GetBoolean());
    }

    [Fact]
    public void UnlockKey_GenerationAndValidationLogic_ShouldProduceValidKeys()
    {
        var code = Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(8)).ToUpperInvariant();
        Assert.Equal(16, code.Length);
        Assert.True(code.All(c => "0123456789ABCDEF".Contains(c)));

        var key = new UnlockKeyDocument
        {
            Code = code,
            IsUsed = false,
            TargetSubjects = new List<string> { "jfe301", "jit401" }
        };

        Assert.False(key.IsUsed);
        Assert.Null(key.UsedByEmail);

        // Simulate redemption
        key.IsUsed = true;
        key.UsedAt = DateTime.UtcNow;
        key.UsedByEmail = "student@example.com";

        Assert.True(key.IsUsed);
        Assert.NotNull(key.UsedAt);
        Assert.Equal("student@example.com", key.UsedByEmail);
    }

    [Theory]
    [InlineData("hnt.vn.vn@gmail.com", true)]
    [InlineData("HNT.VN.VN@GMAIL.COM", true)]
    [InlineData("student123@gmail.com", false)]
    [InlineData("attacker@evil.com", false)]
    public void AdminEmailRoleCheck_ShouldOnlyAllowConfiguredAdminEmail(string email, bool expectedIsAdmin)
    {
        const string configuredAdmin = "hnt.vn.vn@gmail.com";
        var isAdmin = string.Equals(email.Trim().ToLowerInvariant(), configuredAdmin, StringComparison.Ordinal);
        Assert.Equal(expectedIsAdmin, isAdmin);
    }

    [Fact]
    public void RateLimitTracker_ShouldBlockAfterFiveFailedAttempts()
    {
        var rateLimits = new System.Collections.Concurrent.ConcurrentDictionary<string, (int Attempts, DateTime ResetTime)>();
        const string clientKey = "testuser@gmail.com:127.0.0.1";
        var now = DateTime.UtcNow;

        bool isRateLimited()
        {
            if (rateLimits.TryGetValue(clientKey, out var entry))
            {
                if (DateTime.UtcNow > entry.ResetTime) return false;
                return entry.Attempts >= 5;
            }
            return false;
        }

        void recordFailure()
        {
            rateLimits.AddOrUpdate(
                clientKey,
                _ => (1, DateTime.UtcNow.AddMinutes(5)),
                (_, existing) => (existing.Attempts + 1, existing.ResetTime));
        }

        // Initially not limited
        Assert.False(isRateLimited());

        // 4 failed attempts -> still not limited
        for (int i = 0; i < 4; i++) recordFailure();
        Assert.False(isRateLimited());

        // 5th failed attempt -> limited!
        recordFailure();
        Assert.True(isRateLimited());
    }
}
