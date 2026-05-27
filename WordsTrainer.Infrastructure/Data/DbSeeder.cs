using Microsoft.EntityFrameworkCore;
using WordsTrainer.Core.Entities;

namespace WordsTrainer.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await SeedLanguagesAsync(db);
        await SeedLanguageLevelsAsync(db);

        await db.SaveChangesAsync();
    }

    private static async Task SeedLanguagesAsync(AppDbContext db)
    {
        var languages = new[]
        {
            new Language { Code = "ru", Name = "Russian", NativeName = "Русский" },
            new Language { Code = "en", Name = "English", NativeName = "English" },
            new Language { Code = "de", Name = "German", NativeName = "Deutsch" }
        };

        foreach (var language in languages)
        {
            var existing = await db.Languages
                .FirstOrDefaultAsync(x => x.Code == language.Code);

            if (existing == null)
            {
                db.Languages.Add(language);
                continue;
            }

            existing.Name = language.Name;
            existing.NativeName = language.NativeName;
        }
    }

    private static async Task SeedLanguageLevelsAsync(AppDbContext db)
    {
        var levels = new[]
        {
            new LanguageLevel { Code = "A1", Name = "Beginner", Order = 1 },
            new LanguageLevel { Code = "A2", Name = "Elementary", Order = 2 },
            new LanguageLevel { Code = "B1", Name = "Intermediate", Order = 3 },
            new LanguageLevel { Code = "B2", Name = "Upper Intermediate", Order = 4 },
            new LanguageLevel { Code = "C1", Name = "Advanced", Order = 5 },
            new LanguageLevel { Code = "C2", Name = "Proficient", Order = 6 }
        };

        foreach (var level in levels)
        {
            var existing = await db.LanguageLevels
                .FirstOrDefaultAsync(x => x.Code == level.Code);

            if (existing == null)
            {
                db.LanguageLevels.Add(level);
                continue;
            }

            existing.Name = level.Name;
            existing.Order = level.Order;
            existing.IsActive = true;
        }
    }
}
