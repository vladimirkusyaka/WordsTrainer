using System;
using System.Collections.Generic;
using System.Text;

namespace WordsTrainer.Contracts.Training
{
    public enum TrainingNextStatus
    {
        Available = 1,
        SessionCompleted = 2,
        NoWordsAvailable = 3,
        DailyLimitReached = 4
    }
}
