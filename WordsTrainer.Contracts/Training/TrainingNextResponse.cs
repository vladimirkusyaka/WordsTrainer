using System;
using System.Collections.Generic;
using System.Text;

namespace WordsTrainer.Contracts.Training
{
    public class TrainingNextResponse
    {
        public TrainingNextStatus Status { get; set; }

        public TrainingQuestionResponse? Question { get; set; }

        public string? Message { get; set; }
    }
}
