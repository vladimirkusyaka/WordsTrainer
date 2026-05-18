using System;
using System.Collections.Generic;
using System.Text;

namespace WordsTrainer.Contracts.Auth
{
    public class CurrentUserResponse
    {
        public Guid Id { get; set; }

        public string Email { get; set; } = string.Empty;

        public string NativeLanguageCode { get; set; } = string.Empty;

        public string TargetLanguageCode { get; set; } = string.Empty;

        public Guid LanguageLevelId { get; set; }

        public string LanguageLevelCode { get; set; } = string.Empty;

        public string LanguageLevelName { get; set; } = string.Empty;
    }
}
