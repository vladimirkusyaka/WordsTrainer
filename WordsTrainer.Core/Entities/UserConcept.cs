using System;
using System.Collections.Generic;
using System.Text;

namespace WordsTrainer.Core.Entities
{
    public class UserConcept
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public AppUser User { get; set; } = null!;

        public Guid ConceptId { get; set; }
        public Concept Concept { get; set; } = null!;

        public int Score { get; set; }

        public int CorrectCount { get; set; }
        public int WrongCount { get; set; }
        public int TranslationViewCount { get; set; }

        public int TotalReviews { get; set; }

        public int CorrectStreak { get; set; }

        public int IntervalDays { get; set; }

        public double EaseFactor { get; set; } = 2.5;

        public DateTime FirstShownAtUtc { get; set; } = DateTime.UtcNow;

        public DateTime? LastShownAtUtc { get; set; }

        public DateTime? LastCorrectAtUtc { get; set; }

        public DateTime? LastWrongAtUtc { get; set; }

        public DateTime? NextReviewAtUtc { get; set; }

        public bool IsLearned { get; set; }

        public DateTime? LearnedAtUtc { get; set; }
    }
}
