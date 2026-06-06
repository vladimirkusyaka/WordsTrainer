using Microsoft.EntityFrameworkCore;
using WordsTrainer.Api.Services;
using WordsTrainer.Contracts.Training;
using WordsTrainer.Core.Entities;
using WordsTrainer.Infrastructure.Data;

namespace WordsTrainer.Tests;

public class TrainingServiceTests
{
    [Fact]
    public async Task GetNextAsync_WhenUserDoesNotExist_ReturnsNoWordsAvailable()
    {
        await using var db = CreateDb();
        var service = new TrainingService(db);

        var result = await service.GetNextAsync(Guid.NewGuid());

        Assert.Equal(TrainingNextStatus.NoWordsAvailable, result.Status);
        Assert.Equal("User not found.", result.Message);
        Assert.Null(result.Question);
    }

    [Fact]
    public async Task GetNextAsync_WhenNoMatchingConcepts_ReturnsNoWordsAvailable()
    {
        await using var db = CreateDb();
        var seed = SeedBaseData(db, userLevelCode: "A1");
        await db.SaveChangesAsync();

        var service = new TrainingService(db);

        var result = await service.GetNextAsync(seed.User.Id);

        Assert.Equal(TrainingNextStatus.NoWordsAvailable, result.Status);
        Assert.Equal("No words available for training.", result.Message);
        Assert.Null(result.Question);
    }

    [Fact]
    public async Task GetNextAsync_ForB1User_StartsWithB1NewConcept()
    {
        await using var db = CreateDb();
        var seed = SeedBaseData(db, userLevelCode: "B1");

        AddConcept(db, seed, "a1_house", "A1", "Haus", "dom");
        AddConcept(db, seed, "a2_city", "A2", "Stadt", "gorod");
        var b1Concept = AddConcept(db, seed, "b1_decision", "B1", "Entscheidung", "reshenie");
        AddConcept(db, seed, "b2_context", "B2", "Zusammenhang", "kontekst");

        await db.SaveChangesAsync();

        var service = new TrainingService(db);

        var result = await service.GetNextAsync(seed.User.Id);

        Assert.Equal(TrainingNextStatus.Available, result.Status);
        Assert.NotNull(result.Question);
        Assert.Equal(b1Concept.Id, result.Question.ConceptId);
        Assert.Equal("B1", result.Question.TargetLevelCode);
        Assert.False(result.Question.IsReview);
    }

    [Fact]
    public async Task GetNextAsync_WhenUserLevelConceptsAreAlreadyShown_UsesNextHigherLevel()
    {
        await using var db = CreateDb();
        var seed = SeedBaseData(db, userLevelCode: "B1");

        var b1Concept = AddConcept(db, seed, "b1_decision", "B1", "Entscheidung", "reshenie");
        var b2Concept = AddConcept(db, seed, "b2_context", "B2", "Zusammenhang", "kontekst");
        AddUserConcept(db, seed.User.Id, b1Concept.Id, nextReviewAtUtc: DateTime.UtcNow.AddDays(3));

        await db.SaveChangesAsync();

        var service = new TrainingService(db);

        var result = await service.GetNextAsync(seed.User.Id);

        Assert.Equal(TrainingNextStatus.Available, result.Status);
        Assert.NotNull(result.Question);
        Assert.Equal(b2Concept.Id, result.Question.ConceptId);
        Assert.Equal("B2", result.Question.TargetLevelCode);
        Assert.False(result.Question.IsReview);
    }

    [Fact]
    public async Task GetNextAsync_AfterTwoConsecutiveReviews_ForcesNewConcept()
    {
        await using var db = CreateDb();
        var seed = SeedBaseData(db, userLevelCode: "A1");

        var reviewConcept = AddConcept(db, seed, "a1_review", "A1", "heute", "segodnya");
        var newConcept = AddConcept(db, seed, "a1_new", "A1", "morgen", "zavtra");

        AddUserConcept(db, seed.User.Id, reviewConcept.Id, nextReviewAtUtc: DateTime.UtcNow.AddHours(-1));
        AddRecentAnswer(db, seed.User.Id, reviewConcept.Id, wasNewConcept: false, minutesAgo: 2);
        AddRecentAnswer(db, seed.User.Id, reviewConcept.Id, wasNewConcept: false, minutesAgo: 1);

        await db.SaveChangesAsync();

        var service = new TrainingService(db);

        var result = await service.GetNextAsync(seed.User.Id);

        Assert.Equal(TrainingNextStatus.Available, result.Status);
        Assert.NotNull(result.Question);
        Assert.Equal(newConcept.Id, result.Question.ConceptId);
        Assert.False(result.Question.IsReview);
    }

    [Fact]
    public async Task SubmitAnswerAsync_WhenTranslationWasViewed_TreatsCorrectOptionAsAgain()
    {
        await using var db = CreateDb();
        var seed = SeedBaseData(db, userLevelCode: "A1");

        AddConcept(db, seed, "a1_today", "A1", "heute", "segodnya");

        await db.SaveChangesAsync();

        var service = new TrainingService(db);
        var next = await service.GetNextAsync(seed.User.Id);

        Assert.NotNull(next.Question);

        var attempt = await db.TrainingQuestionAttempts
            .SingleAsync(x => x.Id == next.Question.AttemptId);

        var response = await service.SubmitAnswerAsync(
            seed.User.Id,
            new SubmitTrainingAnswerRequest
            {
                AttemptId = attempt.Id,
                SelectedWordId = attempt.CorrectWordId,
                TranslationViewed = true,
                DurationMs = 0
            });

        Assert.NotNull(response);
        Assert.False(response.IsCorrect);
        Assert.Equal(0, response.ScoreBefore);
        Assert.Equal(-2, response.ScoreAfter);
        Assert.Equal(-2, response.ScoreDelta);

        var userConcept = await db.UserConcepts
            .SingleAsync(x => x.UserId == seed.User.Id && x.ConceptId == attempt.ConceptId);

        Assert.Equal(1, userConcept.TranslationViewCount);
        Assert.Equal(1, userConcept.WrongCount);
        Assert.Equal(0, userConcept.CorrectCount);
    }

    [Fact]
    public async Task SubmitAnswerAsync_WhenWrongOptionSelected_StoresWrongAnswerAndSchedulesSoonReview()
    {
        await using var db = CreateDb();
        var seed = SeedBaseData(db, userLevelCode: "A1");

        var concept = AddConcept(db, seed, "a1_today", "A1", "heute", "segodnya");
        AddConcept(db, seed, "a1_store", "A1", "Laden", "magazin");
        AddConcept(db, seed, "a1_child", "A1", "Kind", "rebenok");
        AddConcept(db, seed, "a1_big", "A1", "gross", "bolshoy");

        await db.SaveChangesAsync();

        var service = new TrainingService(db);
        var next = await service.GetNextAsync(seed.User.Id);

        Assert.NotNull(next.Question);

        var attempt = await db.TrainingQuestionAttempts
            .Include(x => x.Options)
            .SingleAsync(x => x.Id == next.Question.AttemptId);

        var wrongOption = attempt.Options.First(x => !x.IsCorrect);

        var response = await service.SubmitAnswerAsync(
            seed.User.Id,
            new SubmitTrainingAnswerRequest
            {
                AttemptId = attempt.Id,
                SelectedWordId = wrongOption.WordId,
                TranslationViewed = false,
                DurationMs = 3000
            });

        Assert.NotNull(response);
        Assert.False(response.IsCorrect);
        Assert.Equal(-2, response.ScoreDelta);
        Assert.True(response.NextReviewAtUtc <= DateTime.UtcNow.AddMinutes(11));

        var userConcept = await db.UserConcepts
            .SingleAsync(x => x.UserId == seed.User.Id && x.ConceptId == attempt.ConceptId);

        Assert.Equal(1, userConcept.WrongCount);
        Assert.Equal(0, userConcept.CorrectCount);
        Assert.Equal(0, userConcept.CorrectStreak);

        var answer = await db.TrainingAnswers.SingleAsync();
        Assert.False(answer.IsCorrect);
        Assert.True(answer.WasNewConcept);
        Assert.Equal(wrongOption.TextSnapshot, answer.SelectedAnswer);
    }

    [Fact]
    public async Task GetStatsAsync_CountsTodayAnswersAndLearnedConcepts()
    {
        await using var db = CreateDb();
        var seed = SeedBaseData(db, userLevelCode: "A1");

        var newConcept = AddConcept(db, seed, "a1_today", "A1", "heute", "segodnya");
        var reviewConcept = AddConcept(db, seed, "a1_store", "A1", "Laden", "magazin");
        var learnedConcept = AddConcept(db, seed, "a1_child", "A1", "Kind", "rebenok");

        db.TrainingAnswers.AddRange(
            new TrainingAnswer
            {
                Id = Guid.NewGuid(),
                UserId = seed.User.Id,
                ConceptId = newConcept.Id,
                WasNewConcept = true,
                IsCorrect = true,
                AnsweredAtUtc = DateTime.UtcNow.AddMinutes(-10),
                QuestionText = "q",
                CorrectAnswer = "a",
                SelectedAnswer = "a"
            },
            new TrainingAnswer
            {
                Id = Guid.NewGuid(),
                UserId = seed.User.Id,
                ConceptId = reviewConcept.Id,
                WasNewConcept = false,
                IsCorrect = false,
                AnsweredAtUtc = DateTime.UtcNow.AddMinutes(-5),
                QuestionText = "q",
                CorrectAnswer = "a",
                SelectedAnswer = "b"
            },
            new TrainingAnswer
            {
                Id = Guid.NewGuid(),
                UserId = seed.User.Id,
                ConceptId = reviewConcept.Id,
                WasNewConcept = false,
                IsCorrect = true,
                AnsweredAtUtc = DateTime.UtcNow.Date.AddDays(-1).AddHours(12),
                QuestionText = "old",
                CorrectAnswer = "a",
                SelectedAnswer = "a"
            });

        AddUserConcept(db, seed.User.Id, learnedConcept.Id, nextReviewAtUtc: DateTime.UtcNow.AddDays(30), isLearned: true);

        await db.SaveChangesAsync();

        var service = new TrainingService(db);

        var stats = await service.GetStatsAsync(seed.User.Id);

        Assert.Equal(2, stats.AnsweredToday);
        Assert.Equal(1, stats.CorrectToday);
        Assert.Equal(1, stats.NewCorrectToday);
        Assert.Equal(1, stats.NewConceptsToday);
        Assert.Equal(1, stats.ReviewsToday);
        Assert.Equal(1, stats.LearnedTotal);
        Assert.Equal(10, stats.NewConceptLimit);
        Assert.Equal(40, stats.ReviewLimit);
    }

    [Fact]
    public async Task GetExplanationByAttemptAsync_ReturnsNativeExplanationAndTargetLevel()
    {
        await using var db = CreateDb();
        var seed = SeedBaseData(db, userLevelCode: "A1");

        var concept = AddConcept(db, seed, "a1_house", "A1", "Haus", "dom");
        var targetWord = concept.ConceptWords
            .Select(x => x.Word)
            .Single(x => x.LanguageId == seed.TargetLanguage.Id);
        targetWord.AudioUrl = "https://example.com/haus.mp3";

        db.ConceptExplanations.Add(new ConceptExplanation
        {
            Id = Guid.NewGuid(),
            ConceptId = concept.Id,
            Concept = concept,
            LanguageId = seed.NativeLanguage.Id,
            Language = seed.NativeLanguage,
            Text = "A building where people live."
        });

        await db.SaveChangesAsync();

        var service = new TrainingService(db);
        var next = await service.GetNextAsync(seed.User.Id);

        Assert.NotNull(next.Question);

        var explanation = await service.GetExplanationByAttemptAsync(
            seed.User.Id,
            next.Question.AttemptId);

        Assert.NotNull(explanation);
        Assert.Equal(concept.Id, explanation.ConceptId);
        Assert.Equal("Haus", explanation.TargetWord);
        Assert.Equal("dom", explanation.NativeTranslation);
        Assert.Equal("A building where people live.", explanation.Explanation);
        Assert.Equal("de", explanation.TargetLanguageCode);
        Assert.Equal("ru", explanation.NativeLanguageCode);
        Assert.Equal("A1", explanation.TargetLevelCode);
        Assert.Equal("https://example.com/haus.mp3", explanation.AudioUrl);
    }

    [Fact]
    public async Task StartSessionAsync_WhenNoActiveSession_CreatesSession()
    {
        await using var db = CreateDb();
        var seed = SeedBaseData(db, userLevelCode: "A1");
        await db.SaveChangesAsync();

        var service = new TrainingService(db);

        var session = await service.StartSessionAsync(seed.User.Id);

        Assert.NotEqual(Guid.Empty, session.Id);
        Assert.Null(session.FinishedAtUtc);
        Assert.Equal(10, session.NewConceptLimit);
        Assert.Equal(40, session.ReviewLimit);
        Assert.Equal(0, session.AnsweredCount);
        Assert.Equal(0, session.CorrectCount);
        Assert.Equal(1, await db.TrainingSessions.CountAsync(x => x.UserId == seed.User.Id));
    }

    [Fact]
    public async Task StartSessionAsync_WhenActiveSessionExists_ReusesIt()
    {
        await using var db = CreateDb();
        var seed = SeedBaseData(db, userLevelCode: "A1");
        await db.SaveChangesAsync();

        var service = new TrainingService(db);

        var first = await service.StartSessionAsync(seed.User.Id);
        var second = await service.StartSessionAsync(seed.User.Id);

        Assert.Equal(first.Id, second.Id);
        Assert.Equal(1, await db.TrainingSessions.CountAsync(x => x.UserId == seed.User.Id));
    }

    [Fact]
    public async Task FinishSessionAsync_MarksActiveSessionFinished_AndCurrentSessionBecomesNull()
    {
        await using var db = CreateDb();
        var seed = SeedBaseData(db, userLevelCode: "A1");
        await db.SaveChangesAsync();

        var service = new TrainingService(db);
        var started = await service.StartSessionAsync(seed.User.Id);

        var finished = await service.FinishSessionAsync(seed.User.Id);
        var current = await service.GetCurrentSessionAsync(seed.User.Id);

        Assert.NotNull(finished);
        Assert.Equal(started.Id, finished.Id);
        Assert.NotNull(finished.FinishedAtUtc);
        Assert.Null(current);
    }

    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static SeedContext SeedBaseData(AppDbContext db, string userLevelCode)
    {
        var native = new Language
        {
            Id = Guid.NewGuid(),
            Code = "ru",
            Name = "Russian",
            NativeName = "Russkiy"
        };

        var target = new Language
        {
            Id = Guid.NewGuid(),
            Code = "de",
            Name = "German",
            NativeName = "Deutsch"
        };

        var levels = new[]
        {
            new LanguageLevel { Id = Guid.NewGuid(), Code = "A1", Name = "Beginner", Order = 1 },
            new LanguageLevel { Id = Guid.NewGuid(), Code = "A2", Name = "Elementary", Order = 2 },
            new LanguageLevel { Id = Guid.NewGuid(), Code = "B1", Name = "Intermediate", Order = 3 },
            new LanguageLevel { Id = Guid.NewGuid(), Code = "B2", Name = "Upper intermediate", Order = 4 },
            new LanguageLevel { Id = Guid.NewGuid(), Code = "C1", Name = "Advanced", Order = 5 },
            new LanguageLevel { Id = Guid.NewGuid(), Code = "C2", Name = "Proficient", Order = 6 }
        };

        var userLevel = levels.First(x => x.Code == userLevelCode);
        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            PasswordHash = "hash",
            NativeLanguageId = native.Id,
            NativeLanguage = native,
            TargetLanguageId = target.Id,
            TargetLanguage = target,
            LanguageLevelId = userLevel.Id,
            LanguageLevel = userLevel
        };

        db.Languages.AddRange(native, target);
        db.LanguageLevels.AddRange(levels);
        db.Users.Add(user);

        return new SeedContext(user, native, target, levels.ToDictionary(x => x.Code));
    }

    private static Concept AddConcept(
        AppDbContext db,
        SeedContext seed,
        string meaningKey,
        string levelCode,
        string targetWordText,
        string nativeWordText)
    {
        var level = seed.Levels[levelCode];
        var concept = new Concept
        {
            Id = Guid.NewGuid(),
            MeaningKey = meaningKey,
            LanguageLevelId = level.Id,
            LanguageLevel = level
        };

        var targetWord = new Word
        {
            Id = Guid.NewGuid(),
            LanguageId = seed.TargetLanguage.Id,
            Language = seed.TargetLanguage,
            Text = targetWordText,
            PartOfSpeech = "noun"
        };

        var nativeWord = new Word
        {
            Id = Guid.NewGuid(),
            LanguageId = seed.NativeLanguage.Id,
            Language = seed.NativeLanguage,
            Text = nativeWordText,
            PartOfSpeech = "noun"
        };

        var targetConceptWord = new ConceptWord
        {
            Id = Guid.NewGuid(),
            ConceptId = concept.Id,
            Concept = concept,
            WordId = targetWord.Id,
            Word = targetWord
        };

        var nativeConceptWord = new ConceptWord
        {
            Id = Guid.NewGuid(),
            ConceptId = concept.Id,
            Concept = concept,
            WordId = nativeWord.Id,
            Word = nativeWord
        };

        concept.ConceptWords.Add(targetConceptWord);
        concept.ConceptWords.Add(nativeConceptWord);
        targetWord.ConceptWords.Add(targetConceptWord);
        nativeWord.ConceptWords.Add(nativeConceptWord);

        db.Concepts.Add(concept);
        db.Words.AddRange(targetWord, nativeWord);
        db.ConceptWords.AddRange(targetConceptWord, nativeConceptWord);

        return concept;
    }

    private static void AddUserConcept(
        AppDbContext db,
        Guid userId,
        Guid conceptId,
        DateTime? nextReviewAtUtc,
        bool isLearned = false)
    {
        db.UserConcepts.Add(new UserConcept
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ConceptId = conceptId,
            FirstShownAtUtc = DateTime.UtcNow.AddDays(-1),
            LastShownAtUtc = DateTime.UtcNow.AddDays(-1),
            NextReviewAtUtc = nextReviewAtUtc,
            Score = isLearned ? 10 : 1,
            IsLearned = isLearned,
            LearnedAtUtc = isLearned ? DateTime.UtcNow.AddDays(-1) : null
        });
    }

    private static void AddRecentAnswer(
        AppDbContext db,
        Guid userId,
        Guid conceptId,
        bool wasNewConcept,
        int minutesAgo)
    {
        db.TrainingAnswers.Add(new TrainingAnswer
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ConceptId = conceptId,
            WasNewConcept = wasNewConcept,
            AnsweredAtUtc = DateTime.UtcNow.AddMinutes(-minutesAgo),
            QuestionText = "q",
            CorrectAnswer = "a",
            SelectedAnswer = "a",
            IsCorrect = true
        });
    }

    private sealed record SeedContext(
        AppUser User,
        Language NativeLanguage,
        Language TargetLanguage,
        Dictionary<string, LanguageLevel> Levels);
}
