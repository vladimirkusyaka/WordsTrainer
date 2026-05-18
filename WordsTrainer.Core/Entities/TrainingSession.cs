using System;
using System.Collections.Generic;
using System.Text;

namespace WordsTrainer.Core.Entities
{
    public class TrainingSession
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public AppUser User { get; set; } = null!;

        public DateTime StartedAtUtc { get; set; } = DateTime.UtcNow;

        public DateTime? FinishedAtUtc { get; set; }

        public int NewConceptLimit { get; set; } = 10;

        public int ReviewLimit { get; set; } = 40;

        public List<TrainingAnswer> Answers { get; set; } = [];

        public List<TrainingQuestionAttempt> Attempts { get; set; } = [];
    }
}
