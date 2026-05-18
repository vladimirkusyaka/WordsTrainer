using System;
using System.Collections.Generic;
using System.Text;

namespace WordsTrainer.Core.Entities
{
    public class ConceptWord
    {
        public Guid Id { get; set; }

        public Guid ConceptId { get; set; }
        public Concept Concept { get; set; } = null!;

        public Guid WordId { get; set; }
        public Word Word { get; set; } = null!;

        public bool IsPrimary { get; set; } = true;
    }
}
