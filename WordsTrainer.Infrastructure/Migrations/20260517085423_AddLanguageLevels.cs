using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordsTrainer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLanguageLevels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Level",
                table: "Words");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "Users");

            migrationBuilder.AddColumn<Guid>(
                name: "LanguageLevelId",
                table: "Users",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "LanguageLevelId",
                table: "Concepts",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "LanguageLevels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LanguageLevels", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_LanguageLevelId",
                table: "Users",
                column: "LanguageLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_Concepts_LanguageLevelId",
                table: "Concepts",
                column: "LanguageLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_LanguageLevels_Code",
                table: "LanguageLevels",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Concepts_LanguageLevels_LanguageLevelId",
                table: "Concepts",
                column: "LanguageLevelId",
                principalTable: "LanguageLevels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_LanguageLevels_LanguageLevelId",
                table: "Users",
                column: "LanguageLevelId",
                principalTable: "LanguageLevels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Concepts_LanguageLevels_LanguageLevelId",
                table: "Concepts");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_LanguageLevels_LanguageLevelId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "LanguageLevels");

            migrationBuilder.DropIndex(
                name: "IX_Users_LanguageLevelId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Concepts_LanguageLevelId",
                table: "Concepts");

            migrationBuilder.DropColumn(
                name: "LanguageLevelId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LanguageLevelId",
                table: "Concepts");

            migrationBuilder.AddColumn<string>(
                name: "Level",
                table: "Words",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Level",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
