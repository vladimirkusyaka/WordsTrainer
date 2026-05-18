using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordsTrainer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTrainingQuestionAttempts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TrainingQuestionAttemptId",
                table: "TrainingAnswers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TrainingQuestionAttempts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrainingSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConceptId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionWordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CorrectWordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AnsweredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsAnswered = table.Column<bool>(type: "bit", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_TrainingAnswers_TrainingQuestionAttemptId",
                table: "TrainingAnswers",
                column: "TrainingQuestionAttemptId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_TrainingAnswers_TrainingQuestionAttempts_TrainingQuestionAttemptId",
                table: "TrainingAnswers",
                column: "TrainingQuestionAttemptId",
                principalTable: "TrainingQuestionAttempts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrainingAnswers_TrainingQuestionAttempts_TrainingQuestionAttemptId",
                table: "TrainingAnswers");

            migrationBuilder.DropTable(
                name: "TrainingQuestionAttempts");

            migrationBuilder.DropIndex(
                name: "IX_TrainingAnswers_TrainingQuestionAttemptId",
                table: "TrainingAnswers");

            migrationBuilder.DropColumn(
                name: "TrainingQuestionAttemptId",
                table: "TrainingAnswers");
        }
    }
}
