using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EngilishCore.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Role = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CurrentLevel = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    HighestReachedLevel = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    DailyWordLimit = table.Column<int>(type: "integer", nullable: false),
                    DailySentenceLimit = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sentences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EnglishText = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    TurkishText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Level = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sentences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Words",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EnglishText = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TurkishText = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Level = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Words", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserSentenceProgresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AppUserId = table.Column<int>(type: "integer", nullable: false),
                    SentenceId = table.Column<int>(type: "integer", nullable: false),
                    ViewCount = table.Column<int>(type: "integer", nullable: false),
                    LastReviewed = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSentenceProgresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSentenceProgresses_AppUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserSentenceProgresses_Sentences_SentenceId",
                        column: x => x.SentenceId,
                        principalTable: "Sentences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserWordProgresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AppUserId = table.Column<int>(type: "integer", nullable: false),
                    WordId = table.Column<int>(type: "integer", nullable: false),
                    ViewCount = table.Column<int>(type: "integer", nullable: false),
                    LastReviewed = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserWordProgresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserWordProgresses_AppUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserWordProgresses_Words_WordId",
                        column: x => x.WordId,
                        principalTable: "Words",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppUsers_Username",
                table: "AppUsers",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSentenceProgresses_AppUserId_SentenceId",
                table: "UserSentenceProgresses",
                columns: new[] { "AppUserId", "SentenceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSentenceProgresses_SentenceId",
                table: "UserSentenceProgresses",
                column: "SentenceId");

            migrationBuilder.CreateIndex(
                name: "IX_UserWordProgresses_AppUserId_WordId",
                table: "UserWordProgresses",
                columns: new[] { "AppUserId", "WordId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserWordProgresses_WordId",
                table: "UserWordProgresses",
                column: "WordId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserSentenceProgresses");

            migrationBuilder.DropTable(
                name: "UserWordProgresses");

            migrationBuilder.DropTable(
                name: "Sentences");

            migrationBuilder.DropTable(
                name: "AppUsers");

            migrationBuilder.DropTable(
                name: "Words");
        }
    }
}
