using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordsTrainer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ImproveTrainingModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CorrectStreak",
                table: "UserConcepts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "EaseFactor",
                table: "UserConcepts",
                type: "float(5)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "IntervalDays",
                table: "UserConcepts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastCorrectAtUtc",
                table: "UserConcepts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastWrongAtUtc",
                table: "UserConcepts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LearnedAtUtc",
                table: "UserConcepts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalReviews",
                table: "UserConcepts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CorrectAnswer",
                table: "TrainingAnswers",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DurationMs",
                table: "TrainingAnswers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Quality",
                table: "TrainingAnswers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "QuestionText",
                table: "TrainingAnswers",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ScoreAfter",
                table: "TrainingAnswers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ScoreBefore",
                table: "TrainingAnswers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SelectedAnswer",
                table: "TrainingAnswers",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "TrainingSessionId",
                table: "TrainingAnswers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "WasNewConcept",
                table: "TrainingAnswers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "TrainingSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FinishedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NewConceptLimit = table.Column<int>(type: "int", nullable: false),
                    ReviewLimit = table.Column<int>(type: "int", nullable: false)
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
                name: "IX_TrainingSessions_UserId_StartedAtUtc",
                table: "TrainingSessions",
                columns: new[] { "UserId", "StartedAtUtc" });

            migrationBuilder.AddForeignKey(
                name: "FK_TrainingAnswers_TrainingSessions_TrainingSessionId",
                table: "TrainingAnswers",
                column: "TrainingSessionId",
                principalTable: "TrainingSessions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrainingAnswers_TrainingSessions_TrainingSessionId",
                table: "TrainingAnswers");

            migrationBuilder.DropTable(
                name: "TrainingSessions");

            migrationBuilder.DropIndex(
                name: "IX_TrainingAnswers_TrainingSessionId",
                table: "TrainingAnswers");

            migrationBuilder.DropIndex(
                name: "IX_TrainingAnswers_UserId_AnsweredAtUtc",
                table: "TrainingAnswers");

            migrationBuilder.DropIndex(
                name: "IX_TrainingAnswers_UserId_ConceptId_AnsweredAtUtc",
                table: "TrainingAnswers");

            migrationBuilder.DropColumn(
                name: "CorrectStreak",
                table: "UserConcepts");

            migrationBuilder.DropColumn(
                name: "EaseFactor",
                table: "UserConcepts");

            migrationBuilder.DropColumn(
                name: "IntervalDays",
                table: "UserConcepts");

            migrationBuilder.DropColumn(
                name: "LastCorrectAtUtc",
                table: "UserConcepts");

            migrationBuilder.DropColumn(
                name: "LastWrongAtUtc",
                table: "UserConcepts");

            migrationBuilder.DropColumn(
                name: "LearnedAtUtc",
                table: "UserConcepts");

            migrationBuilder.DropColumn(
                name: "TotalReviews",
                table: "UserConcepts");

            migrationBuilder.DropColumn(
                name: "CorrectAnswer",
                table: "TrainingAnswers");

            migrationBuilder.DropColumn(
                name: "DurationMs",
                table: "TrainingAnswers");

            migrationBuilder.DropColumn(
                name: "Quality",
                table: "TrainingAnswers");

            migrationBuilder.DropColumn(
                name: "QuestionText",
                table: "TrainingAnswers");

            migrationBuilder.DropColumn(
                name: "ScoreAfter",
                table: "TrainingAnswers");

            migrationBuilder.DropColumn(
                name: "ScoreBefore",
                table: "TrainingAnswers");

            migrationBuilder.DropColumn(
                name: "SelectedAnswer",
                table: "TrainingAnswers");

            migrationBuilder.DropColumn(
                name: "TrainingSessionId",
                table: "TrainingAnswers");

            migrationBuilder.DropColumn(
                name: "WasNewConcept",
                table: "TrainingAnswers");
        }
    }
}
