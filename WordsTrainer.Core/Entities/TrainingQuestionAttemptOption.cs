using System;
using System.Collections.Generic;
using System.Text;

namespace WordsTrainer.Core.Entities
{
    public class TrainingQuestionAttemptOption
    {
        public Guid Id { get; set; }

        public Guid AttemptId { get; set; }
        public TrainingQuestionAttempt Attempt { get; set; } = null!;

        public Guid WordId { get; set; }
        public Word Word { get; set; } = null!;

        public string TextSnapshot { get; set; } = string.Empty;

        public bool IsCorrect { get; set; }
    }
}
