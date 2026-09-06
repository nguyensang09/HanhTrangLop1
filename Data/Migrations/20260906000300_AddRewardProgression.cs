using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HanhTrangLop1.Data.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260906000300_AddRewardProgression")]
public sealed class AddRewardProgression : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(name: "ProgressEpoch", table: "ChildProfiles", type: "int", nullable: false, defaultValue: 0);
        migrationBuilder.AddColumn<int>(name: "ProgressEpoch", table: "LearningSessions", type: "int", nullable: false, defaultValue: 0);
        migrationBuilder.AddColumn<int>(name: "ProgressEpoch", table: "LearningAttempts", type: "int", nullable: false, defaultValue: 0);

        migrationBuilder.Sql("""
            ;WITH DuplicateRewards AS (
                SELECT Id, ROW_NUMBER() OVER (
                    PARTITION BY ChildProfileId, RewardDefinitionId
                    ORDER BY EarnedAt, Id) AS rn
                FROM ChildRewards
            )
            DELETE FROM DuplicateRewards WHERE rn > 1;
            """);
        migrationBuilder.CreateIndex(
            name: "IX_ChildRewards_ChildProfileId_RewardDefinitionId",
            table: "ChildRewards",
            columns: new[] { "ChildProfileId", "RewardDefinitionId" },
            unique: true);

        migrationBuilder.CreateTable(
            name: "ChildInventoryItems",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ChildProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                RewardDefinitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Quantity = table.Column<int>(type: "int", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ChildInventoryItems", x => x.Id);
                table.ForeignKey("FK_ChildInventoryItems_ChildProfiles_ChildProfileId", x => x.ChildProfileId, "ChildProfiles", "Id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_ChildInventoryItems_RewardDefinitions_RewardDefinitionId", x => x.RewardDefinitionId, "RewardDefinitions", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ChildLessonProgresses",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ChildProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                LearningItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                FirstCompletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                LastAttemptedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                LatestStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                BestStars = table.Column<int>(type: "int", nullable: false),
                AttemptCount = table.Column<int>(type: "int", nullable: false),
                ReviewCount = table.Column<int>(type: "int", nullable: false),
                ProgressEpoch = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ChildLessonProgresses", x => x.Id);
                table.ForeignKey("FK_ChildLessonProgresses_ChildProfiles_ChildProfileId", x => x.ChildProfileId, "ChildProfiles", "Id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_ChildLessonProgresses_LearningItems_LearningItemId", x => x.LearningItemId, "LearningItems", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "RewardGrants",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ChildProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                RewardDefinitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                SourceType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                SourceKey = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                SkillGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                MilestoneValue = table.Column<int>(type: "int", nullable: false),
                State = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                UnlockedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                ClaimedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                ProgressEpoch = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RewardGrants", x => x.Id);
                table.ForeignKey("FK_RewardGrants_ChildProfiles_ChildProfileId", x => x.ChildProfileId, "ChildProfiles", "Id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_RewardGrants_RewardDefinitions_RewardDefinitionId", x => x.RewardDefinitionId, "RewardDefinitions", "Id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_RewardGrants_SkillGroups_SkillGroupId", x => x.SkillGroupId, "SkillGroups", "Id", onDelete: ReferentialAction.NoAction);
            });

        migrationBuilder.CreateIndex(name: "IX_ChildInventoryItems_ChildProfileId_RewardDefinitionId", table: "ChildInventoryItems", columns: new[] { "ChildProfileId", "RewardDefinitionId" }, unique: true);
        migrationBuilder.CreateIndex(name: "IX_ChildInventoryItems_RewardDefinitionId", table: "ChildInventoryItems", column: "RewardDefinitionId");
        migrationBuilder.CreateIndex(name: "IX_ChildLessonProgresses_ChildProfileId_LearningItemId", table: "ChildLessonProgresses", columns: new[] { "ChildProfileId", "LearningItemId" }, unique: true);
        migrationBuilder.CreateIndex(name: "IX_ChildLessonProgresses_ChildProfileId_ProgressEpoch_FirstCompletedAt", table: "ChildLessonProgresses", columns: new[] { "ChildProfileId", "ProgressEpoch", "FirstCompletedAt" });
        migrationBuilder.CreateIndex(name: "IX_ChildLessonProgresses_LearningItemId", table: "ChildLessonProgresses", column: "LearningItemId");
        migrationBuilder.CreateIndex(name: "IX_RewardGrants_ChildProfileId_ProgressEpoch_State", table: "RewardGrants", columns: new[] { "ChildProfileId", "ProgressEpoch", "State" });
        migrationBuilder.CreateIndex(name: "IX_RewardGrants_ChildProfileId_SourceType_SourceKey", table: "RewardGrants", columns: new[] { "ChildProfileId", "SourceType", "SourceKey" }, unique: true);
        migrationBuilder.CreateIndex(name: "IX_RewardGrants_RewardDefinitionId", table: "RewardGrants", column: "RewardDefinitionId");
        migrationBuilder.CreateIndex(name: "IX_RewardGrants_SkillGroupId", table: "RewardGrants", column: "SkillGroupId");

        migrationBuilder.Sql("""
            ;WITH AttemptRollup AS (
                SELECT a.ChildProfileId, a.LearningItemId,
                       MIN(CASE WHEN a.Status = 'completed' THEN COALESCE(a.CompletedAt, a.StartedAt) END) AS FirstCompletedAt,
                       MAX(a.StartedAt) AS LastAttemptedAt,
                       CASE
                           WHEN MAX(CASE WHEN a.Status = 'completed' THEN a.StarsEarned ELSE 0 END) < 0 THEN 0
                           WHEN MAX(CASE WHEN a.Status = 'completed' THEN a.StarsEarned ELSE 0 END) > 3 THEN 3
                           ELSE MAX(CASE WHEN a.Status = 'completed' THEN a.StarsEarned ELSE 0 END)
                       END AS BestStars,
                       COUNT(*) AS AttemptCount
                FROM LearningAttempts a
                GROUP BY a.ChildProfileId, a.LearningItemId
            ), Latest AS (
                SELECT a.ChildProfileId, a.LearningItemId, a.Status,
                       ROW_NUMBER() OVER (PARTITION BY a.ChildProfileId, a.LearningItemId ORDER BY a.StartedAt DESC, a.Id DESC) AS rn
                FROM LearningAttempts a
            )
            INSERT INTO ChildLessonProgresses
                (Id, ChildProfileId, LearningItemId, FirstCompletedAt, LastAttemptedAt, LatestStatus, BestStars, AttemptCount, ReviewCount, ProgressEpoch)
            SELECT NEWID(), r.ChildProfileId, r.LearningItemId, r.FirstCompletedAt, r.LastAttemptedAt,
                   l.Status, r.BestStars, r.AttemptCount,
                   CASE WHEN r.FirstCompletedAt IS NULL THEN 0 ELSE (
                       SELECT COUNT(*)
                       FROM LearningAttempts review
                       WHERE review.ChildProfileId = r.ChildProfileId
                         AND review.LearningItemId = r.LearningItemId
                         AND review.StartedAt > r.FirstCompletedAt
                   ) END, 0
            FROM AttemptRollup r
            JOIN Latest l ON l.ChildProfileId = r.ChildProfileId AND l.LearningItemId = r.LearningItemId AND l.rn = 1;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "ChildInventoryItems");
        migrationBuilder.DropTable(name: "ChildLessonProgresses");
        migrationBuilder.DropTable(name: "RewardGrants");
        migrationBuilder.DropIndex(name: "IX_ChildRewards_ChildProfileId_RewardDefinitionId", table: "ChildRewards");
        migrationBuilder.DropColumn(name: "ProgressEpoch", table: "ChildProfiles");
        migrationBuilder.DropColumn(name: "ProgressEpoch", table: "LearningSessions");
        migrationBuilder.DropColumn(name: "ProgressEpoch", table: "LearningAttempts");
    }
}
