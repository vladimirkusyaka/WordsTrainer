using System;
using System.Collections.Generic;
using System.Text;

namespace WordsTrainer.Contracts.Auth
{
    public class RegisterRequest
    {
        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public Guid NativeLanguageId { get; set; }

        public Guid TargetLanguageId { get; set; }

        public Guid LanguageLevelId { get; set; }
    }
}
