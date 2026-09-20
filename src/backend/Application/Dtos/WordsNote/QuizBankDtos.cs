namespace Application.Dtos.WordsNote;

public class QuizSetDTO
{
    public string Id { get; set; } = default!;
    public string Code { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string Description { get; set; } = string.Empty;
    public string Color { get; set; } = "#2563eb";
    public int TotalQuestions { get; set; }
    public bool IsRestricted { get; set; }
    public bool IsUnlocked { get; set; }
}

public class QuestionDTO
{
    public string Id { get; set; } = default!;
    public string SubjectId { get; set; } = default!;
    public int QuestionNumber { get; set; }
    public string Question { get; set; } = default!;
    public Dictionary<string, string> Options { get; set; } = new();
    public List<string> Answers { get; set; } = new();
    public int Choose { get; set; } = 1;
    public string? Explanation { get; set; }
    public string? Note { get; set; }
    public string? Source { get; set; }
    public string? Exam { get; set; }
}

public class QuizSubmitRequestDTO
{
    public Dictionary<string, List<string>> Answers { get; set; } = new();
}

public class QuizSubmitResultDTO
{
    public int TotalQuestions { get; set; }
    public int CorrectCount { get; set; }
    public int IncorrectCount { get; set; }
    public double ScorePercentage { get; set; }
    public List<QuestionReviewDetailDTO> Details { get; set; } = new();
}

public class QuestionReviewDetailDTO
{
    public string QuestionId { get; set; } = default!;
    public List<string> UserAnswers { get; set; } = new();
    public List<string> CorrectAnswers { get; set; } = new();
    public bool IsCorrect { get; set; }
    public string? Explanation { get; set; }
}

public class UnlockKeyDTO
{
    public string Id { get; set; } = default!;
    public string Code { get; set; } = default!;
    public bool IsUsed { get; set; }
    public DateTime? UsedAt { get; set; }
    public string? UsedByEmail { get; set; }
    public List<string> TargetSubjects { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class GenerateKeysRequestDTO
{
    public string? Code { get; set; }
    public List<string>? TargetSubjects { get; set; }
    public int BatchCount { get; set; } = 1;
}

public class RedeemKeyRequestDTO
{
    public string Code { get; set; } = default!;
}

public class RedeemKeyResponseDTO
{
    public bool Success { get; set; }
    public string Message { get; set; } = default!;
    public List<string> UnlockedSubjects { get; set; } = new();
}

public class GrantUserAccessRequestDTO
{
    public string Email { get; set; } = default!;
    public List<string> Subjects { get; set; } = new();
}

public class UserAccessDTO
{
    public string Id { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string? Name { get; set; }
    public string? AvatarUrl { get; set; }
    public List<string> UnlockedSubjects { get; set; } = new();
    public bool IsAdmin { get; set; }
}

