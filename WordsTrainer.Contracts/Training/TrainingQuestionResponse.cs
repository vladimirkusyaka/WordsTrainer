using System;
using System.Collections.Generic;
using System.Text;

namespace WordsTrainer.Contracts.Training
{
    public class TrainingQuestionResponse
    {
        public Guid AttemptId { get; set; }

        public Guid ConceptId { get; set; }

        public Guid QuestionWordId { get; set; }

        public string Question { get; set; } = string.Empty;

        public List<TrainingOptionDto> Options { get; set; } = [];

        public string TargetLanguageCode { get; set; } = string.Empty;

        public string NativeLanguageCode { get; set; } = string.Empty;

        public bool IsReview { get; set; }

        public int? CurrentScore { get; set; }

        public DateTime? NextReviewAtUtc { get; set; }
    }
}
