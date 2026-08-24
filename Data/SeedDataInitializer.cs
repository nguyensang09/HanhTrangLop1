using HanhTrangLop1.Models;
using HanhTrangLop1.Application.Voice;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HanhTrangLop1.Data;

public static class SeedDataInitializer
{
    private const string InitialMigrationId = "20260822074812_InitialCreate";
    private const string EfProductVersion = "9.0.14";

    public static async Task InitializeAsync(IServiceProvider services, IConfiguration configuration, ILogger logger)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var voiceLibrary = scope.ServiceProvider.GetRequiredService<VoiceLibraryMaintenanceService>();

        await EnsureMigrationHistoryForLegacyDatabaseAsync(db);
        await db.Database.MigrateAsync();
        await SeedRolesAsync(roleManager);
        await SeedAdminAsync(userManager, configuration, logger);
        await SeedCurriculumCatalogAsync(db);
        await SeedRewardsAsync(db);

        var createdLessons = await LearningContentSeed.SeedAsync(db);
        if (createdLessons > 0)
        {
            logger.LogInformation("Đã khởi tạo {LessonCount} bài học nền còn thiếu.", createdLessons);
        }

        await LegacyLearningItemNormalizer.NormalizeAsync(db, logger);
        var voiceResult = await voiceLibrary.EnsureVoiceRowsAndRelinkAsync();
        if (voiceResult.LegacyAudioRowsBackfilled > 0 || voiceResult.LearningItemsUpdated > 0)
        {
            logger.LogInformation(
                "Đã đồng bộ voice cho dữ liệu cũ: backfill {BackfilledCount} audio, quét {ScannedCount} bài, cập nhật {UpdatedCount} bài.",
                voiceResult.LegacyAudioRowsBackfilled,
                voiceResult.LearningItemsScanned,
                voiceResult.LearningItemsUpdated);
        }
    }

    private static async Task EnsureMigrationHistoryForLegacyDatabaseAsync(ApplicationDbContext db)
    {
        if (!await db.Database.CanConnectAsync())
        {
            return;
        }

        await db.Database.ExecuteSqlRawAsync($"""
            IF OBJECT_ID(N'[dbo].[AspNetUsers]', N'U') IS NOT NULL
               AND OBJECT_ID(N'[dbo].[__EFMigrationsHistory]', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[__EFMigrationsHistory] (
                    [MigrationId] nvarchar(150) NOT NULL,
                    [ProductVersion] nvarchar(32) NOT NULL,
                    CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
                );

                INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
                VALUES (N'{InitialMigrationId}', N'{EfProductVersion}');
            END
            """);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        var roles = new[] { "Parent", "Admin", "ContentEditor", "Reviewer" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private static async Task SeedAdminAsync(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        ILogger logger)
    {
        var email = configuration["SeedAdmin:Email"] ?? "admin@hanhtranglop1.local";
        var password = configuration["SeedAdmin:Password"] ?? "Admin@123456";

        var admin = await userManager.FindByEmailAsync(email);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                DisplayName = "Quản trị Hành Trang Lớp 1"
            };

            var result = await userManager.CreateAsync(admin, password);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(x => x.Description));
                logger.LogWarning("Không tạo được tài khoản quản trị nền tảng: {Errors}", errors);
                return;
            }
        }

        if (!await userManager.IsInRoleAsync(admin, "Admin"))
        {
            await userManager.AddToRoleAsync(admin, "Admin");
        }
    }

    private static async Task SeedCurriculumCatalogAsync(ApplicationDbContext db)
    {
        var existingGroups = await db.SkillGroups.ToDictionaryAsync(x => x.Id);
        var existingTopics = await db.Topics.ToDictionaryAsync(x => x.Id);

        foreach (var definition in CurriculumCatalog.Groups)
        {
            if (!existingGroups.TryGetValue(definition.Id, out var group))
            {
                group = new SkillGroup { Id = definition.Id };
                db.SkillGroups.Add(group);
            }

            group.Code = definition.Code;
            group.Name = definition.Name;
            group.Description = definition.Description;
            group.IconKey = definition.IconKey;
            group.Color = definition.Color;
            group.SortOrder = definition.SortOrder;
            group.IsActive = true;

            foreach (var topicDefinition in definition.Topics)
            {
                if (!existingTopics.TryGetValue(topicDefinition.Id, out var topic))
                {
                    topic = new Topic { Id = topicDefinition.Id };
                    db.Topics.Add(topic);
                }

                topic.SkillGroupId = definition.Id;
                topic.Code = topicDefinition.Code;
                topic.Name = topicDefinition.Name;
                topic.SortOrder = topicDefinition.SortOrder;
                topic.IsActive = true;
            }
        }

        await db.SaveChangesAsync();
    }

    private static async Task SeedRewardsAsync(ApplicationDbContext db)
    {
        var rewardSeeds = new (string Code, string Name, string Type, string Icon, string Rule)[]
        {
            ("badge-first-step", "Bước Chân Đầu Tiên", "badge", "hotel_class", "Hoàn thành bài học đầu tiên"),
            ("badge-daily-champion", "Chiến Binh Chăm Chỉ", "badge", "military_tech", "Hoàn thành trọn vẹn buổi học hôm nay"),
            ("badge-alphabet-star", "Ngôi Sao Chữ Cái", "badge", "menu_book", "Chinh phục các chữ cái tiếng Việt"),
            ("badge-math-whiz", "Nhà Toán Học Nhí", "badge", "calculate", "Làm quen các con số và đếm số lượng"),
            ("badge-logic-explorer", "Thám Tử Thông Minh", "badge", "psychology", "Vượt qua các câu đố tư duy logic"),
            ("badge-habit-hero", "Bé Ngoan Tự Lập", "badge", "volunteer_activism", "Học tốt các kỹ năng sống và thói quen"),
            ("badge-super-scholar", "Đại Sứ Sóc Nâu", "badge", "emoji_events", "Tích lũy trên 10 ngôi sao vàng")
        };

        var existing = await db.RewardDefinitions.ToDictionaryAsync(x => x.Code);
        foreach (var (code, name, type, icon, rule) in rewardSeeds)
        {
            if (!existing.TryGetValue(code, out var item))
            {
                item = new RewardDefinition
                {
                    Id = Guid.NewGuid(),
                    Code = code
                };
                db.RewardDefinitions.Add(item);
            }
            item.Name = name;
            item.RewardType = type;
            item.IconKey = icon;
            item.RuleJson = rule;
            item.IsActive = true;
        }

        await db.SaveChangesAsync();
    }
}
