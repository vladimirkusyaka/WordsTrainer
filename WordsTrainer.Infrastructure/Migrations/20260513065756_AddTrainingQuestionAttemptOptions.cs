using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordsTrainer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTrainingQuestionAttemptOptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrainingQuestionAttemptOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AttemptId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TextSnapshot = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingQuestionAttemptOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainingQuestionAttemptOptions_TrainingQuestionAttempts_AttemptId",
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
                name: "IX_TrainingQuestionAttemptOptions_AttemptId_WordId",
                table: "TrainingQuestionAttemptOptions",
                columns: new[] { "AttemptId", "WordId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrainingQuestionAttemptOptions_WordId",
                table: "TrainingQuestionAttemptOptions",
                column: "WordId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrainingQuestionAttemptOptions");
        }
    }
}
