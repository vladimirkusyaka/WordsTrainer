using System;
using System.Collections.Generic;
using System.Text;

namespace WordsTrainer.Core.Entities
{
    public class Concept
    {
        public Guid Id { get; set; }

        public string? MeaningKey { get; set; }
        // например: "eat_verb", "house_noun"

        public string? Description { get; set; }

        public List<ConceptWord> ConceptWords { get; set; } = [];
        public List<ConceptExplanation> Explanations { get; set; } = [];
        public Guid LanguageLevelId { get; set; }
        public LanguageLevel LanguageLevel { get; set; } = null!;
    }
}
