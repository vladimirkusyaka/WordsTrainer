using System;
using System.Collections.Generic;
using System.Text;

namespace WordsTrainer.Infrastructure.Seed
{
    public class SeedConceptDto
    {
        public string Key { get; set; } = string.Empty;

        public string Level { get; set; } = string.Empty;

        public string PartOfSpeech { get; set; } = string.Empty;

        public string? Description { get; set; }

        public List<SeedWordDto> Words { get; set; } = [];

        public List<SeedExplanationDto> Explanations { get; set; } = [];
    }

    public class SeedWordDto
    {
        public string LanguageCode { get; set; } = string.Empty;

        public string Text { get; set; } = string.Empty;
    }

    public class SeedExplanationDto
    {
        public string LanguageCode { get; set; } = string.Empty;

        public string Text { get; set; } = string.Empty;
    }
}
