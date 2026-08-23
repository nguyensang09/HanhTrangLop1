using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HanhTrangLop1.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLearningItemSortOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LearningItems_SkillGroupId",
                table: "LearningItems");

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "LearningItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_LearningItems_SkillGroupId_TopicId_SortOrder",
                table: "LearningItems",
                columns: new[] { "SkillGroupId", "TopicId", "SortOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LearningItems_SkillGroupId_TopicId_SortOrder",
                table: "LearningItems");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "LearningItems");

            migrationBuilder.CreateIndex(
                name: "IX_LearningItems_SkillGroupId",
                table: "LearningItems",
                column: "SkillGroupId");
        }
    }
}
