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

        await EnsureMigrationHistoryForLegacyDatabaseAsync(db);
        await db.Database.MigrateAsync();

        // Dữ liệu nền chỉ được tạo ở lần khởi tạo CSDL đầu tiên. Không quét lại toàn bộ
        // bài học và kho voice trong luồng khởi động thông thường; các thao tác đồng bộ
        // bổ sung đã có lệnh/nút quản trị riêng.
        if (await db.LearningItems.AsNoTracking().AnyAsync())
        {
            logger.LogInformation("CSDL đã có dữ liệu bài học; bỏ qua seed và đồng bộ Voice khi khởi động.");
            return;
        }

        await SeedRolesAsync(roleManager);
        await SeedAdminAsync(userManager, configuration, logger);
        await SeedCurriculumCatalogAsync(db);
        await SeedRewardsAsync(db);

        var createdLessons = await LearningContentSeed.SeedAsync(db);
        if (createdLessons > 0)
        {
            logger.LogInformation("Đã khởi tạo {LessonCount} bài học nền còn thiếu.", createdLessons);
        }

        if (configuration.GetValue("VoiceLibrary:GenerateOnSeed", true))
        {
            var voiceLibrary = scope.ServiceProvider.GetRequiredService<VoiceLibraryMaintenanceService>();
            var voiceResult = await voiceLibrary.GenerateMissingAndRelinkAsync();
            logger.LogInformation(
                "Đồng bộ Voice dữ liệu ban đầu hoàn tất: tạo {Created} file VI/EN, lỗi {Failed}, cập nhật liên kết cho {UpdatedItems} bài học.",
                voiceResult.Created,
                voiceResult.Failed,
                voiceResult.UpdatedItems);
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
        var username = configuration["SeedAdmin:Username"] ?? "admin";
        var email = configuration["SeedAdmin:Email"] ?? "admin@hanhtranglop1.local";
        var password = configuration["SeedAdmin:Password"] ?? "admin@123";

        var admin = await userManager.FindByNameAsync(username) ?? await userManager.FindByEmailAsync(email);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = username,
                Email = email,
                EmailConfirmed = true,
                DisplayName = "Quản trị viên"
            };

            var result = await userManager.CreateAsync(admin, password);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(x => x.Description));
                logger.LogWarning("Không tạo được tài khoản quản trị nền tảng: {Errors}", errors);
                return;
            }
        }
        else
        {
            if (admin.UserName != username)
            {
                admin.UserName = username;
                await userManager.UpdateAsync(admin);
            }

            var passwordCheck = await userManager.CheckPasswordAsync(admin, password);
            if (!passwordCheck)
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(admin);
                var resetResult = await userManager.ResetPasswordAsync(admin, token, password);
                if (!resetResult.Succeeded)
                {
                    logger.LogWarning("Không reset được mật khẩu admin: {Errors}", string.Join("; ", resetResult.Errors.Select(x => x.Description)));
                }
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
            // Huy hiệu tiến trình & chuyên cần
            ("badge-first-step", "Bước Chân Đầu Tiên", "badge", "hotel_class", "Hoàn thành bài học đầu tiên"),
            ("badge-daily-champion", "Chiến Binh Chăm Chỉ", "badge", "military_tech", "Hoàn thành trọn vẹn buổi học hôm nay"),
            ("badge-streak-3d", "Ong Vàng Siêng Năng", "badge", "local_fire_department", "Hoàn thành 3 ngày học liên tiếp"),
            ("badge-streak-7d", "Bậc Thầy Chuyên Cần", "badge", "workspace_premium", "Kiên trì học 7 ngày cùng Sóc Nâu"),
            ("badge-super-scholar", "Đại Sứ Sóc Nâu", "badge", "emoji_events", "Tích lũy trên 10 ngôi sao vàng"),
            ("badge-star-collector", "Nhà Sưu Tầm Sao", "badge", "stars", "Tích lũy trên 25 ngôi sao vàng"),

            // Huy hiệu nhóm kỹ năng
            ("badge-alphabet-star", "Ngôi Sao Chữ Cái", "badge", "menu_book", "Chinh phục các bài học chữ cái tiếng Việt"),
            ("badge-handwriting-hero", "Bàn Tay Khéo Léo", "badge", "edit", "Hoàn thành các bài luyện tô nét chữ chuẩn"),
            ("badge-math-whiz", "Nhà Toán Học Nhí", "badge", "calculate", "Làm quen các con số và đếm số lượng"),
            ("badge-logic-explorer", "Thám Tử Thông Minh", "badge", "psychology", "Vượt qua các câu đố tư duy logic"),
            ("badge-habit-hero", "Bé Ngoan Tự Lập", "badge", "volunteer_activism", "Học tốt các kỹ năng sống và thói quen"),
            ("badge-story-teller", "Nhà Kể Chuyện Nhí", "badge", "auto_stories", "Mở rộng vốn từ và nghe hiểu câu chuyện"),
            ("badge-shape-master", "Kiến Trúc Sư Tí Hon", "badge", "category", "Phân biệt thành thạo các hình khối và không gian"),

            // Vật phẩm trang trí khu vườn của bé
            ("item-golden-acorn", "Quả Sồi Hoàng Gia", "item", "nature", "Vật phẩm quý giá nhận khi chăm chỉ học tập"),
            ("item-magic-pencil", "Bút Chì Cầu Vồng", "item", "draw", "Bút chì thần kỳ tô điểm những nét chữ đẹp"),
            ("item-knowledge-tree", "Cây Tri Thức 3D", "item", "park", "Khu vườn nở hoa khi bé học thêm nhiều điều mới"),
            ("item-tiny-crown", "Vương Miện Tí Hon", "item", "royalty", "Vương miện vinh danh bạn nhỏ xuất sắc"),
            ("item-trophy-gold", "Cúp Sóc Nâu Danh Dự", "item", "trophy", "Cúp vàng cao quý nhất của trường mầm non Sóc Nâu")
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
