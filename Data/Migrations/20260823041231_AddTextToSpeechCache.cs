using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HanhTrangLop1.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTextToSpeechCache : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TextToSpeechCaches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Voice = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ModelId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Format = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TextHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    NormalizedText = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    OriginalText = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    AudioUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    LastError = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ReuseCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TextToSpeechCaches", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TextToSpeechCaches_Provider_Voice_ModelId_Format_TextHash",
                table: "TextToSpeechCaches",
                columns: new[] { "Provider", "Voice", "ModelId", "Format", "TextHash" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TextToSpeechCaches");
        }
    }
}
