using System;
using System.Collections.Generic;
using System.Text;

namespace WordsTrainer.Core.Entities
{
    public class ConceptExplanation
    {
        public Guid Id { get; set; }

        public Guid ConceptId { get; set; }
        public Concept Concept { get; set; } = null!;

        public Guid LanguageId { get; set; }
        public Language Language { get; set; } = null!;

        public string Text { get; set; } = string.Empty;
    }
}
