using System;
using System.Collections.Generic;
using System.Text;

namespace WordsTrainer.Contracts.LanguageLevels
{
    public class LanguageLevelResponse
    {
        public Guid Id { get; set; }

        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public int Order { get; set; }
    }
}
