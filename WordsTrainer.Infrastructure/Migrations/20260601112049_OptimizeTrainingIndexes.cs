using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordsTrainer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OptimizeTrainingIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_UserConcepts_UserId_NextReviewAtUtc",
                table: "UserConcepts",
                columns: new[] { "UserId", "NextReviewAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_TrainingAnswers_AnsweredAtUtc",
                table: "TrainingAnswers",
                column: "AnsweredAtUtc",
                descending: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserConcepts_UserId_NextReviewAtUtc",
                table: "UserConcepts");

            migrationBuilder.DropIndex(
                name: "IX_TrainingAnswers_AnsweredAtUtc",
                table: "TrainingAnswers");
        }
    }
}
