using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using WordsTrainer.Core.Entities;

namespace WordsTrainer.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<AppUser> Users => Set<AppUser>();
        public DbSet<Language> Languages => Set<Language>();
        public DbSet<Concept> Concepts => Set<Concept>();
        public DbSet<Word> Words => Set<Word>();
        public DbSet<ConceptWord> ConceptWords => Set<ConceptWord>();
        public DbSet<UserConcept> UserConcepts => Set<UserConcept>();
        public DbSet<TrainingAnswer> TrainingAnswers => Set<TrainingAnswer>();
        public DbSet<ConceptExplanation> ConceptExplanations => Set<ConceptExplanation>();
        public DbSet<TrainingSession> TrainingSessions => Set<TrainingSession>();
        public DbSet<TrainingQuestionAttempt> TrainingQuestionAttempts => Set<TrainingQuestionAttempt>();
        public DbSet<TrainingQuestionAttemptOption> TrainingQuestionAttemptOptions => Set<TrainingQuestionAttemptOption>();
        public DbSet<LanguageLevel> LanguageLevels => Set<LanguageLevel>();
        public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Language>()
                .HasIndex(x => x.Code)
                .IsUnique();

            modelBuilder.Entity<Language>()
                .Property(x => x.Code)
                .HasMaxLength(10);

            modelBuilder.Entity<Language>()
                .Property(x => x.Name)
                .HasMaxLength(100);

            modelBuilder.Entity<Language>()
                .Property(x => x.NativeName)
                .HasMaxLength(100);

            modelBuilder.Entity<AppUser>()
                .HasIndex(x => x.Email)
                .IsUnique();

            modelBuilder.Entity<AppUser>()
                .Property(x => x.Email)
                .HasMaxLength(256);

            modelBuilder.Entity<Word>()
                .HasIndex(x => new { x.LanguageId, x.Text });

            modelBuilder.Entity<Word>()
                .Property(x => x.Text)
                .HasMaxLength(256);

            modelBuilder.Entity<Word>()
                .Property(x => x.PartOfSpeech)
                .HasMaxLength(50);

            modelBuilder.Entity<Word>()
                .Property(x => x.AudioUrl)
                .HasMaxLength(1000);

            modelBuilder.Entity<ConceptWord>()
                .HasIndex(x => new { x.ConceptId, x.WordId })
                .IsUnique();

            modelBuilder.Entity<UserConcept>()
                .HasIndex(x => new { x.UserId, x.ConceptId })
                .IsUnique();

            modelBuilder.Entity<AppUser>()
                .HasOne(x => x.NativeLanguage)
                .WithMany()
                .HasForeignKey(x => x.NativeLanguageId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AppUser>()
                .HasOne(x => x.TargetLanguage)
                .WithMany()
                .HasForeignKey(x => x.TargetLanguageId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ConceptExplanation>()
                .HasIndex(x => new { x.ConceptId, x.LanguageId })
                .IsUnique();

            modelBuilder.Entity<ConceptExplanation>()
                .Property(x => x.Text)
                .HasMaxLength(2000);

            modelBuilder.Entity<TrainingSession>()
                .HasIndex(x => new { x.UserId, x.StartedAtUtc });

            modelBuilder.Entity<TrainingAnswer>()
                .HasIndex(x => new { x.UserId, x.AnsweredAtUtc });

            modelBuilder.Entity<TrainingAnswer>()
                .HasIndex(x => new { x.UserId, x.ConceptId, x.AnsweredAtUtc });

            modelBuilder.Entity<TrainingAnswer>()
                .Property(x => x.QuestionText)
                .HasMaxLength(256);

            modelBuilder.Entity<TrainingAnswer>()
                .Property(x => x.CorrectAnswer)
                .HasMaxLength(256);

            modelBuilder.Entity<TrainingAnswer>()
                .Property(x => x.SelectedAnswer)
                .HasMaxLength(256);

            modelBuilder.Entity<UserConcept>()
                .Property(x => x.EaseFactor)
                .HasPrecision(5, 2);

            modelBuilder.Entity<TrainingQuestionAttempt>()
                .HasIndex(x => new { x.UserId, x.CreatedAtUtc });

            modelBuilder.Entity<TrainingQuestionAttempt>()
                .HasIndex(x => new { x.UserId, x.ConceptId, x.IsAnswered });

            modelBuilder.Entity<TrainingQuestionAttempt>()
                .HasIndex(x => new { x.TrainingSessionId, x.CreatedAtUtc });

            modelBuilder.Entity<TrainingQuestionAttemptOption>()
                .HasIndex(x => new { x.AttemptId, x.WordId })
                .IsUnique();

            modelBuilder.Entity<TrainingQuestionAttemptOption>()
                .Property(x => x.TextSnapshot)
                .HasMaxLength(256);

            modelBuilder.Entity<LanguageLevel>()
                .HasIndex(x => x.Code)
                .IsUnique();

            modelBuilder.Entity<LanguageLevel>()
                .Property(x => x.Code)
                .HasMaxLength(10);

            modelBuilder.Entity<LanguageLevel>()
                .Property(x => x.Name)
                .HasMaxLength(100);

            modelBuilder.Entity<AppUser>()
                .HasOne(x => x.LanguageLevel)
                .WithMany()
                .HasForeignKey(x => x.LanguageLevelId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Concept>()
                .HasOne(x => x.LanguageLevel)
                .WithMany()
                .HasForeignKey(x => x.LanguageLevelId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PasswordResetToken>()
                .HasIndex(x => x.TokenHash)
                .IsUnique();

            modelBuilder.Entity<PasswordResetToken>()
                .HasIndex(x => x.UserId);

            modelBuilder.Entity<PasswordResetToken>()
                .HasIndex(x => x.ExpiresAtUtc);

            modelBuilder.Entity<PasswordResetToken>()
                .Property(x => x.TokenHash)
                .HasMaxLength(128);

            modelBuilder.Entity<PasswordResetToken>()
                .Property(x => x.CreatedIp)
                .HasMaxLength(64);

            modelBuilder.Entity<PasswordResetToken>()
                .HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
