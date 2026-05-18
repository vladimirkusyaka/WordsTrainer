using System;
using System.Collections.Generic;
using System.Text;

namespace WordsTrainer.Contracts.Training
{
    public class TrainingOptionDto
    {
        public Guid WordId { get; set; }

        public string Text { get; set; } = string.Empty;
    }
}
