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
}
