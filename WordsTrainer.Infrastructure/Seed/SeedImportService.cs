using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using WordsTrainer.Core.Entities;
using WordsTrainer.Infrastructure.Data;

namespace WordsTrainer.Infrastructure.Seed
{
    public class SeedImportService
    {
        private readonly AppDbContext _db;

        public SeedImportService(AppDbContext db)
        {
            _db = db;
        }

        public async Task ImportAsync(string filePath)
        {
            if (!File.Exists(filePath))
                return;

            var json = await File.ReadAllTextAsync(filePath);

            var items = JsonSerializer.Deserialize<List<SeedConceptDto>>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (items == null || items.Count == 0)
                return;

            foreach (var item in items)
            {
                await ImportConceptAsync(item);
            }

            await _db.SaveChangesAsync();
        }

        private async Task ImportConceptAsync(SeedConceptDto item)
        {
            var languageLevel = await _db.LanguageLevels.FirstOrDefaultAsync(x => x.Code == item.Level);

            if (languageLevel == null)
                return;

            var existingConcept = await _db.Concepts
                .Include(x => x.ConceptWords)
                .Include(x => x.Explanations)
                .FirstOrDefaultAsync(x => x.MeaningKey == item.Key);

            if (existingConcept != null)
                return;

            var concept = new Concept
            {
                Id = Guid.NewGuid(),
                MeaningKey = item.Key,
                Description = item.Description,
                LanguageLevelId = languageLevel.Id

            };

            _db.Concepts.Add(concept);

            foreach (var seedWord in item.Words)
            {
                var language = await _db.Languages
                    .FirstOrDefaultAsync(x => x.Code == seedWord.LanguageCode);

                if (language == null)
                    continue;

                var word = await _db.Words
                    .FirstOrDefaultAsync(x =>
                        x.LanguageId == language.Id &&
                        x.Text == seedWord.Text);

                if (word == null)
                {
                    word = new Word
                    {
                        Id = Guid.NewGuid(),
                        LanguageId = language.Id,
                        Text = seedWord.Text,
                        PartOfSpeech = item.PartOfSpeech,
                        Difficulty = 1
                    };

                    _db.Words.Add(word);
                }

                _db.ConceptWords.Add(new ConceptWord
                {
                    Id = Guid.NewGuid(),
                    Concept = concept,
                    Word = word,
                    IsPrimary = true
                });
            }

            foreach (var seedExplanation in item.Explanations)
            {
                var language = await _db.Languages
                    .FirstOrDefaultAsync(x => x.Code == seedExplanation.LanguageCode);

                if (language == null)
                    continue;

                _db.ConceptExplanations.Add(new ConceptExplanation
                {
                    Id = Guid.NewGuid(),
                    Concept = concept,
                    LanguageId = language.Id,
                    Text = seedExplanation.Text
                });
            }
        }
    }
}
