using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HanhTrangLop1.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBilingualVoiceSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AudioUrlEn",
                table: "TextToSpeechCaches",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastErrorEn",
                table: "TextToSpeechCaches",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusEn",
                table: "TextToSpeechCaches",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TextEn",
                table: "TextToSpeechCaches",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VoiceEn",
                table: "TextToSpeechCaches",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EnglishVoice",
                table: "ChildProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AudioUrlEn",
                table: "TextToSpeechCaches");

            migrationBuilder.DropColumn(
                name: "LastErrorEn",
                table: "TextToSpeechCaches");

            migrationBuilder.DropColumn(
                name: "StatusEn",
                table: "TextToSpeechCaches");

            migrationBuilder.DropColumn(
                name: "TextEn",
                table: "TextToSpeechCaches");

            migrationBuilder.DropColumn(
                name: "VoiceEn",
                table: "TextToSpeechCaches");

            migrationBuilder.DropColumn(
                name: "EnglishVoice",
                table: "ChildProfiles");
        }
    }
}
