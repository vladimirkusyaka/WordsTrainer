using System;
using System.Collections.Generic;
using System.Text;
using WordsTrainer.Core.Enums;

namespace WordsTrainer.Core.Entities
{
    public class TrainingAnswer
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public Guid ConceptId { get; set; }

        public Guid? TrainingSessionId { get; set; }
        public TrainingSession? TrainingSession { get; set; }

        public bool IsCorrect { get; set; }

        public bool TranslationViewed { get; set; }

        public AnswerQuality Quality { get; set; }

        public int ScoreDelta { get; set; }

        public int ScoreBefore { get; set; }

        public int ScoreAfter { get; set; }

        public string QuestionText { get; set; } = string.Empty;

        public string CorrectAnswer { get; set; } = string.Empty;

        public string SelectedAnswer { get; set; } = string.Empty;

        public int DurationMs { get; set; }

        public bool WasNewConcept { get; set; }

        public DateTime AnsweredAtUtc { get; set; } = DateTime.UtcNow;

        public Guid? TrainingQuestionAttemptId { get; set; }

        public TrainingQuestionAttempt? TrainingQuestionAttempt { get; set; }
    }
}
