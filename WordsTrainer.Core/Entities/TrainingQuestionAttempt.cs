using System;
using System.Collections.Generic;
using System.Text;

namespace WordsTrainer.Core.Entities
{
    public class TrainingQuestionAttempt
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public Guid TrainingSessionId { get; set; }
        public TrainingSession TrainingSession { get; set; } = null!;

        public Guid ConceptId { get; set; }

        public Guid QuestionWordId { get; set; }

        public Guid CorrectWordId { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public DateTime? AnsweredAtUtc { get; set; }

        public bool IsAnswered { get; set; }

        public List<TrainingQuestionAttemptOption> Options { get; set; } = [];
    }
}
