using System;
using System.Collections.Generic;
using System.Text;

namespace WordsTrainer.Contracts.Training
{
    public class SubmitTrainingAnswerRequest
    {
        public Guid AttemptId { get; set; }

        public Guid SelectedWordId { get; set; }

        public bool TranslationViewed { get; set; }

        public int DurationMs { get; set; }
    }
}
