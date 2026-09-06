using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HanhTrangLop1.Data.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260906000200_StandardizeCorrectFeedback")]
public sealed class StandardizeCorrectFeedback : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            UPDATE [Questions]
            SET [FeedbackJson] = JSON_MODIFY(
                    CASE WHEN ISJSON([FeedbackJson]) = 1 THEN [FeedbackJson] ELSE N'{}' END,
                    '$.correct', N'Giỏi lắm'),
                [PayloadJson] = CASE WHEN ISJSON([PayloadJson]) = 1 THEN
                    JSON_MODIFY(
                        JSON_MODIFY(
                            JSON_MODIFY([PayloadJson], '$.correctSpeechText', N'Giỏi lắm'),
                            '$.correctAudioUrl', N''),
                        '$.correctAudioUrlEn', N'')
                    ELSE [PayloadJson] END;

            UPDATE [LearningItems]
            SET [ContentJson] = CASE WHEN ISJSON([ContentJson]) = 1 THEN
                JSON_MODIFY(
                    JSON_MODIFY(
                        JSON_MODIFY([ContentJson], '$.correctSpeechText', N'Giỏi lắm'),
                        '$.correctAudioUrl', N''),
                    '$.correctAudioUrlEn', N'')
                ELSE [ContentJson] END;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // This content migration is intentionally irreversible: restoring the old
        // phrase would overwrite later administrator edits.
    }
}
