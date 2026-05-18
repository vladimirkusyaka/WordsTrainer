using System;
using System.Collections.Generic;
using System.Text;

namespace WordsTrainer.Core.Entities
{
    public class AppUser
    {
        public Guid Id { get; set; }

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public Guid NativeLanguageId { get; set; }
        public Language NativeLanguage { get; set; } = null!;

        public Guid TargetLanguageId { get; set; }
        public Language TargetLanguage { get; set; } = null!;

        public Guid LanguageLevelId { get; set; }
        public LanguageLevel LanguageLevel { get; set; } = null!;
        // A1, A2, B1

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public List<UserConcept> UserConcepts { get; set; } = [];

        public List<TrainingSession> TrainingSessions { get; set; } = [];
    }
}
