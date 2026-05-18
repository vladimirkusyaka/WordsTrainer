using System;
using System.Collections.Generic;
using System.Text;

namespace WordsTrainer.Core.Entities
{
    public class Word
    {
        public Guid Id { get; set; }

        public Guid LanguageId { get; set; }
        public Language Language { get; set; } = null!;

        public string Text { get; set; } = string.Empty;
        // essen, eat, есть

        public string? PartOfSpeech { get; set; }
        // verb, noun, adjective

        public int Difficulty { get; set; } = 1;

        public List<ConceptWord> ConceptWords { get; set; } = [];

        public string? AudioUrl { get; set; }
    }
}
