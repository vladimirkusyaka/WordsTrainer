using System;
using System.Collections.Generic;
using System.Text;

namespace WordsTrainer.Contracts.Languages
{
    public class LanguageResponse
    {
        public Guid Id { get; set; }

        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string NativeName { get; set; } = string.Empty;
    }
}
