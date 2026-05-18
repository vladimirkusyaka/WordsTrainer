using System;
using System.Collections.Generic;
using System.Text;

namespace WordsTrainer.Contracts.Training
{
    public class TrainingSessionResponse
    {
        public Guid Id { get; set; }

        public DateTime StartedAtUtc { get; set; }

        public DateTime? FinishedAtUtc { get; set; }

        public int NewConceptLimit { get; set; }

        public int ReviewLimit { get; set; }

        public int AnsweredCount { get; set; }

        public int CorrectCount { get; set; }
    }
}
