using System;
using System.Collections.Generic;
using System.Text;

namespace WordsTrainer.Core.Entities
{
    public class LanguageLevel
    {
        public Guid Id { get; set; }

        public string Code { get; set; } = string.Empty; // A1, A2, B1

        public string Name { get; set; } = string.Empty; // Beginner, Elementary

        public int Order { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
