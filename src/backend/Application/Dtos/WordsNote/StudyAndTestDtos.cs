namespace Application.Dtos.WordsNote;

public class QuickStudyResponseDTO
{
    public string CollectionId { get; set; } = string.Empty;

    public int TotalCards { get; set; }

    public int DueCards { get; set; }

    public List<StudyCardDTO> Cards { get; set; } = [];
}

public class DeepStudySessionDTO
{
    public string CollectionId { get; set; } = string.Empty;

    public int TotalCards { get; set; }

    public int DueCards { get; set; }

    public List<StudyCardDTO> Cards { get; set; } = [];
}

public class StudyReviewRequestDTO
{
    public string CardId { get; set; } = string.Empty;

    public string Difficulty { get; set; } = string.Empty;
}

public class DeepStudyAnswerRequestDTO
{
    public string CardId { get; set; } = string.Empty;

    public string Answer { get; set; } = string.Empty;
}

public class DeepStudyAnswerResultDTO
{
    public string CardId { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }

    public string ExpectedAnswer { get; set; } = string.Empty;

    public string SubmittedAnswer { get; set; } = string.Empty;

    public string RecommendedDifficulty { get; set; } = string.Empty;
}

public class McqStartRequestDTO
{
    public string CollectionId { get; set; } = string.Empty;

    public int QuestionCount { get; set; } = 10;

    public int OptionCount { get; set; } = 4;
}

public class McqQuestionDTO
{
    public string QuestionId { get; set; } = string.Empty;

    public string Prompt { get; set; } = string.Empty;

    public List<string> Options { get; set; } = [];
}

public class McqStartResponseDTO
{
    public string SessionId { get; set; } = string.Empty;

    public string CollectionId { get; set; } = string.Empty;

    public List<McqQuestionDTO> Questions { get; set; } = [];
}

public class McqSubmitAnswerDTO
{
    public string QuestionId { get; set; } = string.Empty;

    public List<int> SelectedOptionIndexes { get; set; } = [];
}

public class McqSubmitRequestDTO
{
    public string SessionId { get; set; } = string.Empty;

    public List<McqSubmitAnswerDTO> Answers { get; set; } = [];
}

public class McqQuestionResultDTO
{
    public string QuestionId { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }

    public List<int> CorrectOptionIndexes { get; set; } = [];

    public List<int> SelectedOptionIndexes { get; set; } = [];
}

public class McqSubmitResultDTO
{
    public string SessionId { get; set; } = string.Empty;

    public int TotalQuestions { get; set; }

    public int CorrectAnswers { get; set; }

    public double ScorePercent { get; set; }

    public List<McqQuestionResultDTO> Results { get; set; } = [];
}

public class WrittenStartRequestDTO
{
    public string CollectionId { get; set; } = string.Empty;

    public int QuestionCount { get; set; } = 10;
}

public class WrittenQuestionDTO
{
    public string QuestionId { get; set; } = string.Empty;

    public string Prompt { get; set; } = string.Empty;
}

public class WrittenStartResponseDTO
{
    public string SessionId { get; set; } = string.Empty;

    public string CollectionId { get; set; } = string.Empty;

    public List<WrittenQuestionDTO> Questions { get; set; } = [];
}

public class WrittenSubmitAnswerDTO
{
    public string QuestionId { get; set; } = string.Empty;

    public string Answer { get; set; } = string.Empty;
}

public class WrittenSubmitRequestDTO
{
    public string SessionId { get; set; } = string.Empty;

    public List<WrittenSubmitAnswerDTO> Answers { get; set; } = [];
}

public class WrittenQuestionResultDTO
{
    public string QuestionId { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }

    public string ExpectedAnswer { get; set; } = string.Empty;

    public string SubmittedAnswer { get; set; } = string.Empty;
}

public class WrittenSubmitResultDTO
{
    public string SessionId { get; set; } = string.Empty;

    public int TotalQuestions { get; set; }

    public int CorrectAnswers { get; set; }

    public double ScorePercent { get; set; }

    public List<WrittenQuestionResultDTO> Results { get; set; } = [];
}
