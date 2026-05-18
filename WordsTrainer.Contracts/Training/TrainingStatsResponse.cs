using System;
using System.Collections.Generic;
using System.Text;

namespace WordsTrainer.Contracts.Training
{
    public class TrainingStatsResponse
    {
        public int AnsweredToday { get; set; }
        public int CorrectToday { get; set; }
        public int NewCorrectToday { get; set; }
        public int LearnedTotal { get; set; }

        public int NewConceptsToday { get; set; }
        public int ReviewsToday { get; set; }

        public int NewConceptLimit { get; set; } = 10;
        public int ReviewLimit { get; set; } = 40;

        public bool NewConceptLimitReached => NewConceptsToday >= NewConceptLimit;
        public bool ReviewLimitReached => ReviewsToday >= ReviewLimit;
    }
}
