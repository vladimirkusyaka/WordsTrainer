using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordsTrainer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LanguageLevels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LanguageLevels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Languages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NativeName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Languages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Concepts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MeaningKey = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    LanguageLevelId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Concepts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Concepts_LanguageLevels_LanguageLevelId",
                        column: x => x.LanguageLevelId,
                        principalTable: "LanguageLevels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    NativeLanguageId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetLanguageId = table.Column<Guid>(type: "uuid", nullable: false),
                    LanguageLevelId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_LanguageLevels_LanguageLevelId",
                        column: x => x.LanguageLevelId,
                        principalTable: "LanguageLevels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Users_Languages_NativeLanguageId",
                        column: x => x.NativeLanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Users_Languages_TargetLanguageId",
                        column: x => x.TargetLanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Words",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LanguageId = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PartOfSpeech = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Difficulty = table.Column<int>(type: "integer", nullable: false),
                    AudioUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Words", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Words_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConceptExplanations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConceptId = table.Column<Guid>(type: "uuid", nullable: false),
                    LanguageId = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConceptExplanations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConceptExplanations_Concepts_ConceptId",
                        column: x => x.ConceptId,
                        principalTable: "Concepts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConceptExplanations_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrainingSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FinishedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NewConceptLimit = table.Column<int>(type: "integer", nullable: false),
                    ReviewLimit = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainingSessions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserConcepts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConceptId = table.Column<Guid>(type: "uuid", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    CorrectCount = table.Column<int>(type: "integer", nullable: false),
                    WrongCount = table.Column<int>(type: "integer", nullable: false),
                    TranslationViewCount = table.Column<int>(type: "integer", nullable: false),
                    TotalReviews = table.Column<int>(type: "integer", nullable: false),
                    CorrectStreak = table.Column<int>(type: "integer", nullable: false),
                    IntervalDays = table.Column<int>(type: "integer", nullable: false),
                    EaseFactor = table.Column<double>(type: "double precision", precision: 5, scale: 2, nullable: false),
                    FirstShownAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastShownAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastCorrectAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastWrongAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NextReviewAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsLearned = table.Column<bool>(type: "boolean", nullable: false),
                    LearnedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserConcepts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserConcepts_Concepts_ConceptId",
                        column: x => x.ConceptId,
                        principalTable: "Concepts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserConcepts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConceptWords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConceptId = table.Column<Guid>(type: "uuid", nullable: false),
                    WordId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConceptWords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConceptWords_Concepts_ConceptId",
                        column: x => x.ConceptId,
                        principalTable: "Concepts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConceptWords_Words_WordId",
                        column: x => x.WordId,
                        principalTable: "Words",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrainingQuestionAttempts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConceptId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionWordId = table.Column<Guid>(type: "uuid", nullable: false),
                    CorrectWordId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AnsweredAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsAnswered = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingQuestionAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainingQuestionAttempts_TrainingSessions_TrainingSessionId",
                        column: x => x.TrainingSessionId,
                        principalTable: "TrainingSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrainingAnswers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConceptId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingSessionId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsCorrect = table.Column<bool>(type: "boolean", nullable: false),
                    TranslationViewed = table.Column<bool>(type: "boolean", nullable: false),
                    Quality = table.Column<int>(type: "integer", nullable: false),
                    ScoreDelta = table.Column<int>(type: "integer", nullable: false),
                    ScoreBefore = table.Column<int>(type: "integer", nullable: false),
                    ScoreAfter = table.Column<int>(type: "integer", nullable: false),
                    QuestionText = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CorrectAnswer = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SelectedAnswer = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    DurationMs = table.Column<int>(type: "integer", nullable: false),
                    WasNewConcept = table.Column<bool>(type: "boolean", nullable: false),
                    AnsweredAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TrainingQuestionAttemptId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainingAnswers_TrainingQuestionAttempts_TrainingQuestionAt~",
                        column: x => x.TrainingQuestionAttemptId,
                        principalTable: "TrainingQuestionAttempts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TrainingAnswers_TrainingSessions_TrainingSessionId",
                        column: x => x.TrainingSessionId,
                        principalTable: "TrainingSessions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TrainingQuestionAttemptOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AttemptId = table.Column<Guid>(type: "uuid", nullable: false),
                    WordId = table.Column<Guid>(type: "uuid", nullable: false),
                    TextSnapshot = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    IsCorrect = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingQuestionAttemptOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainingQuestionAttemptOptions_TrainingQuestionAttempts_Att~",
                        column: x => x.AttemptId,
                        principalTable: "TrainingQuestionAttempts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrainingQuestionAttemptOptions_Words_WordId",
                        column: x => x.WordId,
                        principalTable: "Words",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConceptExplanations_ConceptId_LanguageId",
                table: "ConceptExplanations",
                columns: new[] { "ConceptId", "LanguageId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConceptExplanations_LanguageId",
                table: "ConceptExplanations",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_Concepts_LanguageLevelId",
                table: "Concepts",
                column: "LanguageLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_ConceptWords_ConceptId_WordId",
                table: "ConceptWords",
                columns: new[] { "ConceptId", "WordId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConceptWords_WordId",
                table: "ConceptWords",
                column: "WordId");

            migrationBuilder.CreateIndex(
                name: "IX_LanguageLevels_Code",
                table: "LanguageLevels",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Languages_Code",
                table: "Languages",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrainingAnswers_TrainingQuestionAttemptId",
                table: "TrainingAnswers",
                column: "TrainingQuestionAttemptId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingAnswers_TrainingSessionId",
                table: "TrainingAnswers",
                column: "TrainingSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingAnswers_UserId_AnsweredAtUtc",
                table: "TrainingAnswers",
                columns: new[] { "UserId", "AnsweredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_TrainingAnswers_UserId_ConceptId_AnsweredAtUtc",
                table: "TrainingAnswers",
                columns: new[] { "UserId", "ConceptId", "AnsweredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_TrainingQuestionAttemptOptions_AttemptId_WordId",
                table: "TrainingQuestionAttemptOptions",
                columns: new[] { "AttemptId", "WordId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrainingQuestionAttemptOptions_WordId",
                table: "TrainingQuestionAttemptOptions",
                column: "WordId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingQuestionAttempts_TrainingSessionId_CreatedAtUtc",
                table: "TrainingQuestionAttempts",
                columns: new[] { "TrainingSessionId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_TrainingQuestionAttempts_UserId_ConceptId_IsAnswered",
                table: "TrainingQuestionAttempts",
                columns: new[] { "UserId", "ConceptId", "IsAnswered" });

            migrationBuilder.CreateIndex(
                name: "IX_TrainingQuestionAttempts_UserId_CreatedAtUtc",
                table: "TrainingQuestionAttempts",
                columns: new[] { "UserId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_TrainingSessions_UserId_StartedAtUtc",
                table: "TrainingSessions",
                columns: new[] { "UserId", "StartedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_UserConcepts_ConceptId",
                table: "UserConcepts",
                column: "ConceptId");

            migrationBuilder.CreateIndex(
                name: "IX_UserConcepts_UserId_ConceptId",
                table: "UserConcepts",
                columns: new[] { "UserId", "ConceptId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_LanguageLevelId",
                table: "Users",
                column: "LanguageLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_NativeLanguageId",
                table: "Users",
                column: "NativeLanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_TargetLanguageId",
                table: "Users",
                column: "TargetLanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_Words_LanguageId_Text",
                table: "Words",
                columns: new[] { "LanguageId", "Text" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConceptExplanations");

            migrationBuilder.DropTable(
                name: "ConceptWords");

            migrationBuilder.DropTable(
                name: "TrainingAnswers");

            migrationBuilder.DropTable(
                name: "TrainingQuestionAttemptOptions");

            migrationBuilder.DropTable(
                name: "UserConcepts");

            migrationBuilder.DropTable(
                name: "TrainingQuestionAttempts");

            migrationBuilder.DropTable(
                name: "Words");

            migrationBuilder.DropTable(
                name: "Concepts");

            migrationBuilder.DropTable(
                name: "TrainingSessions");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "LanguageLevels");

            migrationBuilder.DropTable(
                name: "Languages");
        }
    }
}
