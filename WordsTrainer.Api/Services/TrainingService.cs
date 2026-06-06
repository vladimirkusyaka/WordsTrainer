using Microsoft.EntityFrameworkCore;
using WordsTrainer.Contracts.Training;
using WordsTrainer.Core.Entities;
using WordsTrainer.Core.Enums;
using WordsTrainer.Infrastructure.Data;

namespace WordsTrainer.Api.Services;

public class TrainingService
{
    private const int DailyNewConceptLimit = 10;
    private const int DailyReviewLimit = 40;
    private const int MaxConsecutiveReviewsBeforeNew = 2;
    private const int RecentlyShownConceptCount = 5;

    private readonly AppDbContext _db;

    public TrainingService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<TrainingNextResponse> GetNextAsync(Guid userId)
    {
        var user = await _db.Users
            .Include(x => x.NativeLanguage)
            .Include(x => x.TargetLanguage)
            .Include(x => x.LanguageLevel)
            .FirstOrDefaultAsync(x => x.Id == userId);

        if (user == null)
        {
            return new TrainingNextResponse
            {
                Status = TrainingNextStatus.NoWordsAvailable,
                Message = "User not found."
            };
        }

        var session = await GetActiveSessionAsync(userId);

        if (session == null)
        {
            session = new TrainingSession
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                StartedAtUtc = DateTime.UtcNow,
                NewConceptLimit = DailyNewConceptLimit,
                ReviewLimit = DailyReviewLimit
            };

            _db.TrainingSessions.Add(session);
            await _db.SaveChangesAsync();
        }

        var now = DateTime.UtcNow;
        var consecutiveReviews = await GetConsecutiveReviewsTodayAsync(userId);

        var dueReviews = await GetDueReviewConceptsAsync(
            userId,
            user,
            now,
            session);

        var newConcepts = await GetNewConceptsAsync(
            userId,
            user,
            session);

        var selected = SelectNextCandidate(
            dueReviews,
            newConcepts,
            consecutiveReviews);

        if (selected == null)
        {
            return new TrainingNextResponse
            {
                Status = TrainingNextStatus.NoWordsAvailable,
                Message = "No words available for training."
            };
        }

        var question = await BuildQuestionAsync(
            user,
            session,
            selected.Concept,
            selected.UserConcept);

        return new TrainingNextResponse
        {
            Status = TrainingNextStatus.Available,
            Question = question
        };
    }

    public async Task<SubmitTrainingAnswerResponse?> SubmitAnswerAsync(
        Guid userId,
        SubmitTrainingAnswerRequest request)
    {
        var user = await _db.Users
            .Include(x => x.NativeLanguage)
            .Include(x => x.TargetLanguage)
            .FirstOrDefaultAsync(x => x.Id == userId);

        if (user == null)
            return null;

        var attempt = await _db.TrainingQuestionAttempts
            .Include(x => x.Options)
            .FirstOrDefaultAsync(x =>
                x.Id == request.AttemptId &&
                x.UserId == userId &&
                !x.IsAnswered);

        if (attempt == null)
            return null;

        var selectedOption = attempt.Options
            .FirstOrDefault(x => x.WordId == request.SelectedWordId);

        if (selectedOption == null)
            return null;

        var concept = await _db.Concepts
            .Include(x => x.ConceptWords)
                .ThenInclude(x => x.Word)
            .FirstOrDefaultAsync(x => x.Id == attempt.ConceptId);

        var correctWord = await _db.Words
            .FirstOrDefaultAsync(x => x.Id == attempt.CorrectWordId);

        var questionWord = await _db.Words
            .FirstOrDefaultAsync(x => x.Id == attempt.QuestionWordId);

        if (concept == null || correctWord == null || questionWord == null)
            return null;

        var isCorrect = selectedOption.IsCorrect && !request.TranslationViewed;

        var userConcept = await _db.UserConcepts
            .FirstOrDefaultAsync(x =>
                x.UserId == userId &&
                x.ConceptId == attempt.ConceptId);

        var isNewConcept = userConcept == null;

        if (userConcept == null)
        {
            userConcept = new UserConcept
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ConceptId = attempt.ConceptId,
                FirstShownAtUtc = DateTime.UtcNow,
                EaseFactor = 2.5,
                IntervalDays = 0
            };

            _db.UserConcepts.Add(userConcept);
        }

        var scoreBefore = userConcept.Score;

        var quality = DetermineAnswerQuality(
            isCorrect,
            request.TranslationViewed,
            request.DurationMs);

        var scoreDelta = CalculateScoreDelta(quality);

        userConcept.Score += scoreDelta;
        userConcept.TotalReviews++;
        userConcept.LastShownAtUtc = DateTime.UtcNow;

        if (isCorrect)
        {
            userConcept.CorrectCount++;
            userConcept.CorrectStreak++;
            userConcept.LastCorrectAtUtc = DateTime.UtcNow;
        }
        else
        {
            userConcept.WrongCount++;
            userConcept.CorrectStreak = 0;
            userConcept.LastWrongAtUtc = DateTime.UtcNow;
        }

        if (request.TranslationViewed)
            userConcept.TranslationViewCount++;

        userConcept.NextReviewAtUtc = CalculateNextReviewAt(userConcept.Score);

        userConcept.IsLearned = userConcept.Score >= 10;

        if (userConcept.IsLearned && userConcept.LearnedAtUtc == null)
            userConcept.LearnedAtUtc = DateTime.UtcNow;

        var scoreAfter = userConcept.Score;

        attempt.IsAnswered = true;
        attempt.AnsweredAtUtc = DateTime.UtcNow;

        _db.TrainingAnswers.Add(new TrainingAnswer
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ConceptId = attempt.ConceptId,
            TrainingSessionId = attempt.TrainingSessionId,
            TrainingQuestionAttemptId = attempt.Id,

            IsCorrect = isCorrect,
            TranslationViewed = request.TranslationViewed,
            Quality = quality,

            ScoreDelta = scoreDelta,
            ScoreBefore = scoreBefore,
            ScoreAfter = scoreAfter,

            QuestionText = questionWord.Text,
            CorrectAnswer = correctWord.Text,
            SelectedAnswer = selectedOption.TextSnapshot,

            DurationMs = request.DurationMs,
            WasNewConcept = isNewConcept,
            AnsweredAtUtc = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        return new SubmitTrainingAnswerResponse
        {
            IsCorrect = isCorrect,
            CorrectWordId = correctWord.Id,
            CorrectAnswer = correctWord.Text,
            ScoreBefore = scoreBefore,
            ScoreAfter = scoreAfter,
            ScoreDelta = scoreDelta,
            IsLearned = userConcept.IsLearned,
            NextReviewAtUtc = userConcept.NextReviewAtUtc
        };
    }

    public async Task<TrainingStatsResponse> GetStatsAsync(Guid userId)
    {
        var (todayStart, todayEnd) = GetTodayRangeUtc();

        var todayAnswers = _db.TrainingAnswers
            .Where(x =>
                x.UserId == userId &&
                x.AnsweredAtUtc >= todayStart &&
                x.AnsweredAtUtc < todayEnd);

        var answeredToday = await todayAnswers.CountAsync();

        var correctToday = await todayAnswers
            .CountAsync(x => x.IsCorrect);

        var learnedTotal = await _db.UserConcepts
            .CountAsync(x =>
                x.UserId == userId &&
                x.IsLearned);

        var newCorrectToday = await todayAnswers
            .Where(x => x.IsCorrect && x.WasNewConcept)
            .Select(x => x.ConceptId)
            .Distinct()
            .CountAsync();

        var newConceptsToday = await todayAnswers
            .Where(x => x.WasNewConcept)
            .Select(x => x.ConceptId)
            .Distinct()
            .CountAsync();

        var reviewsToday = await todayAnswers
            .CountAsync(x => !x.WasNewConcept);

        return new TrainingStatsResponse
        {
            AnsweredToday = answeredToday,
            CorrectToday = correctToday,
            NewCorrectToday = newCorrectToday,
            LearnedTotal = learnedTotal,
            NewConceptsToday = newConceptsToday,
            ReviewsToday = reviewsToday,
            NewConceptLimit = DailyNewConceptLimit,
            ReviewLimit = DailyReviewLimit
        };
    }

    public async Task<TrainingExplanationResponse?> GetExplanationByAttemptAsync(
        Guid userId,
        Guid attemptId)
    {
        var user = await _db.Users
            .Include(x => x.NativeLanguage)
            .Include(x => x.TargetLanguage)
            .FirstOrDefaultAsync(x => x.Id == userId);

        if (user == null)
            return null;

        var attempt = await _db.TrainingQuestionAttempts
            .FirstOrDefaultAsync(x =>
                x.Id == attemptId &&
                x.UserId == userId);

        if (attempt == null)
            return null;

        var concept = await _db.Concepts
            .Include(x => x.LanguageLevel)
            .Include(x => x.ConceptWords)
                .ThenInclude(x => x.Word)
                    .ThenInclude(x => x.Language)
            .Include(x => x.Explanations)
                .ThenInclude(x => x.Language)
            .FirstOrDefaultAsync(x => x.Id == attempt.ConceptId);

        if (concept == null)
            return null;

        var targetWord = concept.ConceptWords
            .Select(x => x.Word)
            .FirstOrDefault(x => x.Id == attempt.QuestionWordId);

        var nativeWord = concept.ConceptWords
            .Select(x => x.Word)
            .FirstOrDefault(x => x.Id == attempt.CorrectWordId);

        if (targetWord == null || nativeWord == null)
            return null;

        var explanation = concept.Explanations
            .FirstOrDefault(x => x.LanguageId == user.NativeLanguageId)
            ?? concept.Explanations.FirstOrDefault(x => x.Language.Code == "en");

        return new TrainingExplanationResponse
        {
            AttemptId = attempt.Id,
            ConceptId = concept.Id,
            CorrectWordId = attempt.CorrectWordId,
            TargetWord = targetWord.Text,
            NativeTranslation = nativeWord.Text,
            Explanation = explanation?.Text ?? concept.Description ?? string.Empty,
            TargetLanguageCode = user.TargetLanguage.Code,
            TargetLevelCode = concept.LanguageLevel.Code,
            NativeLanguageCode = user.NativeLanguage.Code,
            AudioUrl = targetWord.AudioUrl
        };
    }

    public async Task<TrainingSessionResponse> StartSessionAsync(Guid userId)
    {
        var activeSession = await GetActiveSessionAsync(userId);

        if (activeSession != null)
            return MapSession(activeSession);

        var session = new TrainingSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            StartedAtUtc = DateTime.UtcNow,
            NewConceptLimit = DailyNewConceptLimit,
            ReviewLimit = DailyReviewLimit
        };

        _db.TrainingSessions.Add(session);
        await _db.SaveChangesAsync();

        return MapSession(session);
    }

    public async Task<TrainingSessionResponse?> GetCurrentSessionAsync(Guid userId)
    {
        var session = await GetActiveSessionAsync(userId);

        return session == null ? null : MapSession(session);
    }

    public async Task<TrainingSessionResponse?> FinishSessionAsync(Guid userId)
    {
        var session = await GetActiveSessionAsync(userId);

        if (session == null)
            return null;

        session.FinishedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return MapSession(session);
    }

    private async Task<List<TrainingCandidate>> GetDueReviewConceptsAsync(
        Guid userId,
        AppUser user,
        DateTime now,
        TrainingSession session)
    {
        var recentlyShownConceptIds = session.Attempts
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(RecentlyShownConceptCount)
            .Select(x => x.ConceptId)
            .ToList();

        var items = await _db.UserConcepts
            .AsNoTracking()
            .Include(x => x.Concept)
                .ThenInclude(x => x.LanguageLevel)
            .Include(x => x.Concept)
                .ThenInclude(x => x.ConceptWords)
                    .ThenInclude(x => x.Word)
            .Where(x =>
                x.UserId == userId &&
                !x.IsLearned &&
                x.NextReviewAtUtc != null &&
                x.NextReviewAtUtc <= now &&
                !recentlyShownConceptIds.Contains(x.ConceptId) &&
                x.Concept.ConceptWords.Any(cw => cw.Word.LanguageId == user.TargetLanguageId) &&
                x.Concept.ConceptWords.Any(cw => cw.Word.LanguageId == user.NativeLanguageId))
            .OrderBy(x => x.NextReviewAtUtc)
            .ThenBy(x => x.ConceptId)
            .Take(10)
            .ToListAsync();

        return items
            .Select(x => new TrainingCandidate(
                x.Concept,
                x,
                CalculatePriority(x, now)))
            .OrderByDescending(x => x.Priority)
            .ToList();
    }

    private async Task<List<TrainingCandidate>> GetNewConceptsAsync(
    Guid userId,
    AppUser user,
    TrainingSession session)
    {
        var eligibleConcepts = _db.Concepts
            .AsNoTracking()
            .Where(concept =>
                concept.LanguageLevel.IsActive &&
                concept.LanguageLevel.Order >= user.LanguageLevel.Order)
            .Where(concept =>
                concept.ConceptWords.Any(cw =>
                    cw.Word.LanguageId == user.TargetLanguageId) &&
                concept.ConceptWords.Any(cw =>
                    cw.Word.LanguageId == user.NativeLanguageId))
            .Where(concept =>
                !_db.UserConcepts.Any(uc =>
                    uc.UserId == userId &&
                    uc.ConceptId == concept.Id));

        var nextLevelOrder = await eligibleConcepts
            .Select(x => (int?)x.LanguageLevel.Order)
            .OrderBy(x => x)
            .FirstOrDefaultAsync();

        if (nextLevelOrder == null)
            return [];

        var concepts = await eligibleConcepts
            .Include(x => x.LanguageLevel)
            .Include(x => x.ConceptWords)
                .ThenInclude(x => x.Word)
            .Where(x => x.LanguageLevel.Order == nextLevelOrder.Value)
            .OrderBy(x => x.Id)
            .Take(10)
            .ToListAsync();

        return concepts
            .Select(x => new TrainingCandidate(
                x,
                null,
                50))
            .ToList();
    }

    private async Task<TrainingQuestionResponse> BuildQuestionAsync(
        AppUser user,
        TrainingSession session,
        Concept concept,
        UserConcept? userConcept)
    {
        var questionWord = GetWordForLanguage(concept, user.TargetLanguageId);
        var correctAnswer = GetWordForLanguage(concept, user.NativeLanguageId);

        var wrongOptions = await GetDistractorsAsync(
            correctAnswer,
            concept.Id,
            user.NativeLanguageId);

        var optionWords = wrongOptions
            .Append(correctAnswer)
            .OrderBy(_ => Random.Shared.Next())
            .ToList();

        var attempt = new TrainingQuestionAttempt
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TrainingSessionId = session.Id,
            ConceptId = concept.Id,
            QuestionWordId = questionWord.Id,
            CorrectWordId = correctAnswer.Id,
            CreatedAtUtc = DateTime.UtcNow,
            IsAnswered = false
        };

        _db.TrainingQuestionAttempts.Add(attempt);

        foreach (var optionWord in optionWords)
        {
            _db.TrainingQuestionAttemptOptions.Add(new TrainingQuestionAttemptOption
            {
                Id = Guid.NewGuid(),
                AttemptId = attempt.Id,
                WordId = optionWord.Id,
                TextSnapshot = optionWord.Text,
                IsCorrect = optionWord.Id == correctAnswer.Id
            });
        }

        await _db.SaveChangesAsync();

        return new TrainingQuestionResponse
        {
            AttemptId = attempt.Id,
            ConceptId = concept.Id,
            QuestionWordId = questionWord.Id,
            Question = questionWord.Text,
            Options = optionWords
                .Select(x => new TrainingOptionDto
                {
                    WordId = x.Id,
                    Text = x.Text
                })
                .ToList(),
            TargetLanguageCode = user.TargetLanguage.Code,
            TargetLevelCode = concept.LanguageLevel?.Code ?? user.LanguageLevel.Code,
            NativeLanguageCode = user.NativeLanguage.Code,
            IsReview = userConcept != null,
            CurrentScore = userConcept?.Score,
            NextReviewAtUtc = userConcept?.NextReviewAtUtc
        };
    }

    private async Task<List<Word>> GetDistractorsAsync(
        Word correctAnswer,
        Guid conceptId,
        Guid nativeLanguageId)
    {
        var query = _db.Words
            .Include(x => x.ConceptWords)
            .Where(x =>
                x.LanguageId == nativeLanguageId &&
                x.Id != correctAnswer.Id &&
                !x.ConceptWords.Any(cw => cw.ConceptId == conceptId));

        if (!string.IsNullOrWhiteSpace(correctAnswer.PartOfSpeech))
            query = query.Where(x => x.PartOfSpeech == correctAnswer.PartOfSpeech);

        var candidates = await query
            .Take(100)
            .ToListAsync();

        if (candidates.Count < 3)
        {
            candidates = await _db.Words
                .Include(x => x.ConceptWords)
                .Where(x =>
                    x.LanguageId == nativeLanguageId &&
                    x.Id != correctAnswer.Id &&
                    !x.ConceptWords.Any(cw => cw.ConceptId == conceptId))
                .Take(100)
                .ToListAsync();
        }

        return candidates
                .GroupBy(x => x.Text.ToLower())
                .Select(x => x.First())
                .OrderBy(x => Math.Abs(x.Text.Length - correctAnswer.Text.Length))
                .ThenBy(_ => Random.Shared.Next())
                .Take(3)
                .ToList();
    }

    private async Task<TrainingSession?> GetActiveSessionAsync(Guid userId)
    {
        return await _db.TrainingSessions
            .Include(x => x.Answers)
            .Include(x => x.Attempts)
            .Where(x => x.UserId == userId && x.FinishedAtUtc == null && x.StartedAtUtc.Date == DateTime.UtcNow.Date)
            .OrderByDescending(x => x.StartedAtUtc)
            .FirstOrDefaultAsync();
    }

    private async Task<int> GetConsecutiveReviewsTodayAsync(Guid userId)
    {
        var (todayStart, todayEnd) = GetTodayRangeUtc();

        var recentAnswers = await _db.TrainingAnswers
            .Where(x =>
                x.UserId == userId &&
                x.AnsweredAtUtc >= todayStart &&
                x.AnsweredAtUtc < todayEnd)
            .OrderByDescending(x => x.AnsweredAtUtc)
            .Select(x => x.WasNewConcept)
            .Take(MaxConsecutiveReviewsBeforeNew)
            .ToListAsync();

        var consecutiveReviews = 0;

        foreach (var wasNewConcept in recentAnswers)
        {
            if (wasNewConcept)
                break;

            consecutiveReviews++;
        }

        return consecutiveReviews;
    }

    private static TrainingCandidate? SelectNextCandidate(
    List<TrainingCandidate> dueReviews,
    List<TrainingCandidate> newConcepts,
    int consecutiveReviews)
    {
        if (dueReviews.Count == 0 && newConcepts.Count == 0)
            return null;

        if (dueReviews.Count == 0)
            return PickWeighted(newConcepts);

        if (newConcepts.Count == 0)
            return PickWeighted(dueReviews);

        if (consecutiveReviews >= MaxConsecutiveReviewsBeforeNew)
            return PickWeighted(newConcepts);

        return PickWeighted(dueReviews);
    }

    private static TrainingCandidate PickWeighted(List<TrainingCandidate> items)
    {
        var total = items.Sum(x => Math.Max(1, x.Priority));
        var roll = Random.Shared.Next(1, total + 1);

        var current = 0;

        foreach (var item in items)
        {
            current += Math.Max(1, item.Priority);

            if (roll <= current)
                return item;
        }

        return items[^1];
    }

    private static int CalculatePriority(UserConcept userConcept, DateTime now)
    {
        var overdueDays = userConcept.NextReviewAtUtc == null
            ? 0
            : Math.Max(0, (int)(now - userConcept.NextReviewAtUtc.Value).TotalDays);

        return 100 +
               overdueDays * 10 +
               userConcept.WrongCount * 3 -
               userConcept.CorrectStreak * 2;
    }

    private static AnswerQuality DetermineAnswerQuality(
        bool isCorrect,
        bool translationViewed,
        int durationMs)
    {
        if (translationViewed)
            return AnswerQuality.Again;

        if (!isCorrect)
            return AnswerQuality.Again;

        if (durationMs > 12000)
            return AnswerQuality.Hard;

        if (durationMs < 2500)
            return AnswerQuality.Easy;

        return AnswerQuality.Good;
    }

    private static int CalculateScoreDelta(AnswerQuality quality)
    {
        return quality switch
        {
            AnswerQuality.Again => -2,
            AnswerQuality.Hard => 1,
            AnswerQuality.Good => 2,
            AnswerQuality.Easy => 3,
            _ => 0
        };
    }

    private static DateTime CalculateNextReviewAt(int score)
    {
        var now = DateTime.UtcNow;

        return score switch
        {
            <= 0 => now.AddMinutes(10),
            <= 2 => now.AddHours(6),
            <= 4 => now.AddDays(1),
            <= 6 => now.AddDays(3),
            <= 8 => now.AddDays(7),
            _ => now.AddDays(30)
        };
    }

    private static (DateTime Start, DateTime End) GetTodayRangeUtc()
    {
        var start = DateTime.UtcNow.Date;
        return (start, start.AddDays(1));
    }

    private static Word GetWordForLanguage(Concept concept, Guid languageId)
    {
        return concept.ConceptWords
            .Select(x => x.Word)
            .First(x => x.LanguageId == languageId);
    }

    private static TrainingSessionResponse MapSession(TrainingSession session)
    {
        return new TrainingSessionResponse
        {
            Id = session.Id,
            StartedAtUtc = session.StartedAtUtc,
            FinishedAtUtc = session.FinishedAtUtc,
            NewConceptLimit = session.NewConceptLimit,
            ReviewLimit = session.ReviewLimit,
            AnsweredCount = session.Answers.Count,
            CorrectCount = session.Answers.Count(x => x.IsCorrect)
        };
    }

    private sealed record TrainingCandidate(
        Concept Concept,
        UserConcept? UserConcept,
        int Priority);
}
