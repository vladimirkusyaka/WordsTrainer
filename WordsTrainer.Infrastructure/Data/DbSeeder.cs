using Microsoft.EntityFrameworkCore;
using WordsTrainer.Core.Entities;


namespace WordsTrainer.Infrastructure.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            if (!await db.Languages.AnyAsync())
            {
                db.Languages.AddRange(
                    new Language { Code = "ru", Name = "Russian", NativeName = "Русский" },
                    new Language { Code = "en", Name = "English", NativeName = "English" },
                    new Language { Code = "de", Name = "German", NativeName = "Deutsch" }
                );

                await db.SaveChangesAsync();
            }

            if (!await db.LanguageLevels.AnyAsync())
            {
                db.LanguageLevels.AddRange(
                    new LanguageLevel { Code = "A1", Name = "Beginner", Order = 1 },
                    new LanguageLevel { Code = "A2", Name = "Elementary", Order = 2 },
                    new LanguageLevel { Code = "B1", Name = "Intermediate", Order = 3 },
                    new LanguageLevel { Code = "B2", Name = "Upper Intermediate", Order = 4 },
                    new LanguageLevel { Code = "C1", Name = "Advanced", Order = 5 }
                );

                await db.SaveChangesAsync();
            }

            if (await db.Concepts.AnyAsync())
                return;

            var ru = await db.Languages.SingleAsync(x => x.Code == "ru");
            var en = await db.Languages.SingleAsync(x => x.Code == "en");
            var de = await db.Languages.SingleAsync(x => x.Code == "de");

            await AddConcept(db, "eat_verb", "to eat", "есть", "eat", "essen", ru, en, de);
            await AddConcept(db, "drink_verb", "to drink", "пить", "drink", "trinken", ru, en, de);
            await AddConcept(db, "house_noun", "house", "дом", "house", "Haus", ru, en, de);
            await AddConcept(db, "water_noun", "water", "вода", "water", "Wasser", ru, en, de);
            await AddConcept(db, "book_noun", "book", "книга", "book", "Buch", ru, en, de);

            await db.SaveChangesAsync();
        }

        private static async Task AddConcept(
            AppDbContext db,
            string key,
            string description,
            string ruText,
            string enText,
            string deText,
            Language ru,
            Language en,
            Language de)
        {
            var a1 = await db.LanguageLevels.SingleAsync(x => x.Code == "A1");
            var concept = new Concept
            {
                MeaningKey = key,
                Description = description,
                LanguageLevelId = a1.Id
            };

            var ruWord = new Word { Language = ru, Text = ruText, Difficulty = 1 };
            var enWord = new Word { Language = en, Text = enText, Difficulty = 1 };
            var deWord = new Word { Language = de, Text = deText, Difficulty = 1 };

            db.Concepts.Add(concept);

            db.ConceptWords.AddRange(
                new ConceptWord { Concept = concept, Word = ruWord },
                new ConceptWord { Concept = concept, Word = enWord },
                new ConceptWord { Concept = concept, Word = deWord }
            );

            await Task.CompletedTask;
        }
    }
}
