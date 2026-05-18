using System;
using System.Collections.Generic;
using System.Text;

namespace WordsTrainer.Contracts.Training
{
    public class SubmitTrainingAnswerResponse
    {
        public bool IsCorrect { get; set; }

        public Guid CorrectWordId { get; set; }

        public string CorrectAnswer { get; set; } = string.Empty;

        public int ScoreBefore { get; set; }

        public int ScoreAfter { get; set; }

        public int ScoreDelta { get; set; }

        public bool IsLearned { get; set; }

        public DateTime? NextReviewAtUtc { get; set; }
    }
}
