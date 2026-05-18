using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordsTrainer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddConceptExplanationsAndAudioUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AudioUrl",
                table: "Words",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ConceptExplanations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConceptId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LanguageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_ConceptExplanations_ConceptId_LanguageId",
                table: "ConceptExplanations",
                columns: new[] { "ConceptId", "LanguageId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConceptExplanations_LanguageId",
                table: "ConceptExplanations",
                column: "LanguageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConceptExplanations");

            migrationBuilder.DropColumn(
                name: "AudioUrl",
                table: "Words");
        }
    }
}
