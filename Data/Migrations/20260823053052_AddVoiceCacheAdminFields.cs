using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HanhTrangLop1.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVoiceCacheAdminFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "TextToSpeechCaches",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UsageType",
                table: "TextToSpeechCaches",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_TextToSpeechCaches_Name",
                table: "TextToSpeechCaches",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TextToSpeechCaches_Name",
                table: "TextToSpeechCaches");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "TextToSpeechCaches");

            migrationBuilder.DropColumn(
                name: "UsageType",
                table: "TextToSpeechCaches");
        }
    }
}
