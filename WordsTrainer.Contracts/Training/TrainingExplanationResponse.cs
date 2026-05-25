using System;
using System.Collections.Generic;
using System.Text;

namespace WordsTrainer.Contracts.Training
{
    public class TrainingExplanationResponse
    {
        public Guid ConceptId { get; set; }

        public Guid AttemptId { get; set; }

        public Guid CorrectWordId { get; set; }

        public string TargetWord { get; set; } = string.Empty;

        public string NativeTranslation { get; set; } = string.Empty;

        public string Explanation { get; set; } = string.Empty;

        public string TargetLanguageCode { get; set; } = string.Empty;

        public string TargetLevelCode { get; set; } = string.Empty;

        public string NativeLanguageCode { get; set; } = string.Empty;

        public string? AudioUrl { get; set; }
    }
}
