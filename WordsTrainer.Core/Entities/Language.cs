using System;
using System.Collections.Generic;
using System.Text;

namespace WordsTrainer.Core.Entities
{
    public class Language
    {
        public Guid Id { get; set; }

        public string Code { get; set; } = string.Empty;
        // "ru", "de", "en"

        public string Name { get; set; } = string.Empty;
        // "Russian", "German", "English"

        public string NativeName { get; set; } = string.Empty;
        // "Русский", "Deutsch", "English"

        public bool IsActive { get; set; } = true;
    }
}
