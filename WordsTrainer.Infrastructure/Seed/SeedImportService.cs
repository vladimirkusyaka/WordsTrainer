using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WordsTrainer.Core.Entities;
using WordsTrainer.Infrastructure.Data;

namespace WordsTrainer.Infrastructure.Seed;

public class SeedImportService
{
    private static readonly string[] RequiredLanguageCodes = ["de", "ru", "en"];

    private readonly AppDbContext _db;

    public SeedImportService(AppDbContext db)
    {
        _db = db;
    }

    public async Task ImportDirectoryAsync(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
            return;

        var filePaths = Directory
            .EnumerateFiles(directoryPath, "*.json")
            .OrderBy(x => x)
            .ToList();

        if (filePaths.Count == 0)
            return;

        var items = new List<SeedConceptDto>();

        foreach (var filePath in filePaths)
            items.AddRange(await ReadFileAsync(filePath));

        await ImportItemsAsync(items);
    }

    public async Task ImportAsync(string filePath)
    {
        if (!File.Exists(filePath))
            return;

        var items = await ReadFileAsync(filePath);

        await ImportItemsAsync(items);
    }

    private static async Task<List<SeedConceptDto>> ReadFileAsync(string filePath)
    {
        var json = await File.ReadAllTextAsync(filePath);

        return JsonSerializer.Deserialize<List<SeedConceptDto>>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? [];
    }

    private async Task ImportItemsAsync(List<SeedConceptDto> items)
    {
        if (items.Count == 0)
            return;

        var languages = await _db.Languages
            .ToDictionaryAsync(x => x.Code, StringComparer.OrdinalIgnoreCase);

        var levels = await _db.LanguageLevels
            .ToDictionaryAsync(x => x.Code, StringComparer.OrdinalIgnoreCase);

        Validate(items, languages, levels);

        foreach (var item in items)
            await UpsertConceptAsync(item, languages, levels);

        await _db.SaveChangesAsync();
    }

    private static void Validate(
        List<SeedConceptDto> items,
        Dictionary<string, Language> languages,
        Dictionary<string, LanguageLevel> levels)
    {
        var duplicatedKeys = items
            .GroupBy(x => x.Key.Trim(), StringComparer.OrdinalIgnoreCase)
            .Where(x => x.Count() > 1)
            .Select(x => x.Key)
            .ToList();

        if (duplicatedKeys.Count > 0)
        {
            throw new InvalidOperationException(
                $"Duplicated seed concept keys: {string.Join(", ", duplicatedKeys)}.");
        }

        foreach (var requiredLanguageCode in RequiredLanguageCodes)
        {
            if (!languages.ContainsKey(requiredLanguageCode))
            {
                throw new InvalidOperationException(
                    $"Required language '{requiredLanguageCode}' is missing in database.");
            }
        }

        foreach (var item in items)
        {
            if (string.IsNullOrWhiteSpace(item.Key))
                throw new InvalidOperationException("Seed concept key is required.");

            if (string.IsNullOrWhiteSpace(item.Level) ||
                !levels.ContainsKey(item.Level))
            {
                throw new InvalidOperationException(
                    $"Unknown language level '{item.Level}' for concept '{item.Key}'.");
            }

            if (string.IsNullOrWhiteSpace(item.PartOfSpeech))
            {
                throw new InvalidOperationException(
                    $"Part of speech is required for concept '{item.Key}'.");
            }

            var duplicatedWordLanguages = item.Words
                .GroupBy(x => x.LanguageCode, StringComparer.OrdinalIgnoreCase)
                .Where(x => x.Count() > 1)
                .Select(x => x.Key)
                .ToList();

            if (duplicatedWordLanguages.Count > 0)
            {
                throw new InvalidOperationException(
                    $"Concept '{item.Key}' has duplicate word languages: " +
                    $"{string.Join(", ", duplicatedWordLanguages)}.");
            }

            foreach (var requiredLanguageCode in RequiredLanguageCodes)
            {
                var word = item.Words.FirstOrDefault(x =>
                    string.Equals(
                        x.LanguageCode,
                        requiredLanguageCode,
                        StringComparison.OrdinalIgnoreCase));

                if (word == null || string.IsNullOrWhiteSpace(word.Text))
                {
                    throw new InvalidOperationException(
                        $"Concept '{item.Key}' must contain a '{requiredLanguageCode}' word.");
                }
            }

            var duplicatedExplanationLanguages = item.Explanations
                .GroupBy(x => x.LanguageCode, StringComparer.OrdinalIgnoreCase)
                .Where(x => x.Count() > 1)
                .Select(x => x.Key)
                .ToList();

            if (duplicatedExplanationLanguages.Count > 0)
            {
                throw new InvalidOperationException(
                    $"Concept '{item.Key}' has duplicate explanation languages: " +
                    $"{string.Join(", ", duplicatedExplanationLanguages)}.");
            }
        }
    }

    private async Task UpsertConceptAsync(
        SeedConceptDto item,
        Dictionary<string, Language> languages,
        Dictionary<string, LanguageLevel> levels)
    {
        var key = item.Key.Trim();
        var level = levels[item.Level];

        var concept = await _db.Concepts
            .Include(x => x.ConceptWords)
                .ThenInclude(x => x.Word)
            .Include(x => x.Explanations)
            .FirstOrDefaultAsync(x => x.MeaningKey == key);

        if (concept == null)
        {
            concept = new Concept
            {
                Id = Guid.NewGuid(),
                MeaningKey = key
            };

            _db.Concepts.Add(concept);
        }

        concept.Description = item.Description?.Trim();
        concept.LanguageLevelId = level.Id;

        foreach (var seedWord in item.Words)
        {
            if (!languages.TryGetValue(seedWord.LanguageCode, out var language))
                continue;

            var text = seedWord.Text.Trim();

            if (string.IsNullOrWhiteSpace(text))
                continue;

            var word = await _db.Words.FirstOrDefaultAsync(x =>
                x.LanguageId == language.Id &&
                x.Text == text);

            if (word == null)
            {
                word = new Word
                {
                    Id = Guid.NewGuid(),
                    LanguageId = language.Id,
                    Text = text,
                    PartOfSpeech = item.PartOfSpeech.Trim(),
                    Difficulty = level.Order
                };

                _db.Words.Add(word);
            }
            else
            {
                word.PartOfSpeech = item.PartOfSpeech.Trim();
                word.Difficulty = level.Order;
            }

            var conceptWord = concept.ConceptWords.FirstOrDefault(x =>
                x.Word.LanguageId == language.Id);

            if (conceptWord == null)
            {
                concept.ConceptWords.Add(new ConceptWord
                {
                    Id = Guid.NewGuid(),
                    Concept = concept,
                    Word = word,
                    IsPrimary = true
                });
            }
            else
            {
                conceptWord.Word = word;
                conceptWord.WordId = word.Id;
                conceptWord.IsPrimary = true;
            }
        }

        foreach (var seedExplanation in item.Explanations)
        {
            if (!languages.TryGetValue(seedExplanation.LanguageCode, out var language))
                continue;

            var text = seedExplanation.Text.Trim();

            if (string.IsNullOrWhiteSpace(text))
                continue;

            var explanation = concept.Explanations.FirstOrDefault(x =>
                x.LanguageId == language.Id);

            if (explanation == null)
            {
                concept.Explanations.Add(new ConceptExplanation
                {
                    Id = Guid.NewGuid(),
                    Concept = concept,
                    LanguageId = language.Id,
                    Text = text
                });
            }
            else
            {
                explanation.Text = text;
            }
        }
    }
}
