using HanhTrangLop1.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HanhTrangLop1.Data;

public static class SeedDataInitializer
{
    private const string InitialMigrationId = "20260822074812_InitialCreate";
    private const string EfProductVersion = "9.0.14";

    private static readonly Guid AlphabetGroupId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid NumberGroupId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid MathGroupId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid LogicGroupId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    private static readonly Guid LifeSkillGroupId = Guid.Parse("55555555-5555-5555-5555-555555555555");

    public static async Task InitializeAsync(IServiceProvider services, IConfiguration configuration, ILogger logger)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Tạo database local cho môi trường phát triển.
        await EnsureMigrationHistoryForLegacyDatabaseAsync(db);
        await db.Database.MigrateAsync();

        await SeedRolesAsync(roleManager);
        await SeedAdminAsync(userManager, configuration, logger);
        var parentUser = await SeedParentAsync(userManager, configuration, logger);
        await SeedLearningContentAsync(db);
        await SeedChildProfileAsync(db, parentUser);
    }

    private static async Task EnsureMigrationHistoryForLegacyDatabaseAsync(ApplicationDbContext db)
    {
        if (!await db.Database.CanConnectAsync())
        {
            return;
        }

        // Ghi nhận migration nền cho DB cũ đã tạo bằng EnsureCreated.
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

    private static async Task SeedAdminAsync(UserManager<ApplicationUser> userManager, IConfiguration configuration, ILogger logger)
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
                logger.LogWarning("Không tạo được tài khoản admin mẫu: {Errors}", errors);
                return;
            }
        }

        if (!await userManager.IsInRoleAsync(admin, "Admin"))
        {
            await userManager.AddToRoleAsync(admin, "Admin");
        }
    }

    private static async Task<ApplicationUser?> SeedParentAsync(UserManager<ApplicationUser> userManager, IConfiguration configuration, ILogger logger)
    {
        var email = configuration["SeedParent:Email"] ?? "phuhuynh@hanhtranglop1.local";
        var password = configuration["SeedParent:Password"] ?? "Phuhuynh@123456";

        var parent = await userManager.FindByEmailAsync(email);
        if (parent is null)
        {
            parent = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                DisplayName = "Phụ huynh mẫu"
            };

            var result = await userManager.CreateAsync(parent, password);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(x => x.Description));
                logger.LogWarning("Không tạo được tài khoản phụ huynh mẫu: {Errors}", errors);
                return null;
            }
        }

        if (!await userManager.IsInRoleAsync(parent, "Parent"))
        {
            await userManager.AddToRoleAsync(parent, "Parent");
        }

        return parent;
    }

    private static async Task SeedLearningContentAsync(ApplicationDbContext db)
    {
        if (!await db.SkillGroups.AnyAsync())
        {
            db.SkillGroups.AddRange(
                new SkillGroup { Id = AlphabetGroupId, Code = "chu-cai", Name = "Chữ cái", Description = "Làm quen âm, mặt chữ và nét viết.", IconKey = "auto_stories", Color = "#ff8542", SortOrder = 1 },
                new SkillGroup { Id = NumberGroupId, Code = "chu-so", Name = "Chữ số", Description = "Nhận biết chữ số và cách viết số.", IconKey = "looks_5", Color = "#46e6b3", SortOrder = 2 },
                new SkillGroup { Id = MathGroupId, Code = "toan-truc-quan", Name = "Toán trực quan", Description = "Đếm, so sánh, tách gộp bằng hình ảnh.", IconKey = "calculate", Color = "#67b7dc", SortOrder = 3 },
                new SkillGroup { Id = LogicGroupId, Code = "tu-duy", Name = "Tư duy", Description = "Quan sát, ghi nhớ, phân loại và quy luật.", IconKey = "extension", Color = "#ffd45a", SortOrder = 4 },
                new SkillGroup { Id = LifeSkillGroupId, Code = "ky-nang-song", Name = "Kỹ năng sống", Description = "Tự phục vụ, an toàn và giao tiếp ở trường.", IconKey = "volunteer_activism", Color = "#b98cff", SortOrder = 5 });
        }

        if (!await db.Topics.AnyAsync())
        {
            db.Topics.AddRange(
                new Topic { Id = Guid.Parse("aaaaaaaa-1111-1111-1111-111111111111"), SkillGroupId = AlphabetGroupId, Code = "lam-quen-chu", Name = "Làm quen chữ", SortOrder = 1 },
                new Topic { Id = Guid.Parse("aaaaaaaa-2222-2222-2222-222222222222"), SkillGroupId = NumberGroupId, Code = "so-0-9", Name = "Số 0-9", SortOrder = 1 },
                new Topic { Id = Guid.Parse("aaaaaaaa-3333-3333-3333-333333333333"), SkillGroupId = MathGroupId, Code = "dem-so-luong", Name = "Đếm số lượng", SortOrder = 1 },
                new Topic { Id = Guid.Parse("aaaaaaaa-4444-4444-4444-444444444444"), SkillGroupId = LogicGroupId, Code = "hinh-dang", Name = "Hình dạng", SortOrder = 1 },
                new Topic { Id = Guid.Parse("aaaaaaaa-5555-5555-5555-555555555555"), SkillGroupId = LifeSkillGroupId, Code = "o-truong", Name = "Ở trường", SortOrder = 1 });
        }

        if (!await db.LearningItems.AnyAsync())
        {
            var now = DateTimeOffset.UtcNow;
            db.LearningItems.AddRange(
                new LearningItem
                {
                    Id = Guid.Parse("bbbbbbbb-1111-1111-1111-111111111111"),
                    Code = "ve-chu-a-in-hoa",
                    Title = "Tập vẽ chữ A",
                    SkillGroupId = AlphabetGroupId,
                    TopicId = Guid.Parse("aaaaaaaa-1111-1111-1111-111111111111"),
                    InteractionType = InteractionTypes.Tracing,
                    InstructionText = "Bé vẽ theo đường nét đứt nhé!",
                    ContentJson = """{"symbol":"A","steps":["Xem mẫu","Tô nét thứ 1","Tô nét thứ 2","Tô nét ngang"]}""",
                    Status = ContentStatus.Published,
                    PublishedAt = now
                },
                new LearningItem
                {
                    Id = Guid.Parse("bbbbbbbb-2222-2222-2222-222222222222"),
                    Code = "nhan-biet-so-5",
                    Title = "Nhận biết số 5",
                    SkillGroupId = NumberGroupId,
                    TopicId = Guid.Parse("aaaaaaaa-2222-2222-2222-222222222222"),
                    InteractionType = InteractionTypes.SingleChoice,
                    InstructionText = "Con hãy chọn số năm.",
                    ContentJson = """{"choices":["3","5","8"],"answer":"5"}""",
                    Status = ContentStatus.Published,
                    PublishedAt = now
                },
                new LearningItem
                {
                    Id = Guid.Parse("bbbbbbbb-3333-3333-3333-333333333333"),
                    Code = "dem-1-den-5",
                    Title = "Đếm 1 đến 5",
                    SkillGroupId = MathGroupId,
                    TopicId = Guid.Parse("aaaaaaaa-3333-3333-3333-333333333333"),
                    InteractionType = InteractionTypes.ListenAndChoose,
                    InstructionText = "Con hãy chọn nhóm có năm đồ vật.",
                    ContentJson = """{"targetCount":5,"objects":"hạt dẻ"}""",
                    Status = ContentStatus.Published,
                    PublishedAt = now
                });
        }

        if (!await db.Questions.AnyAsync())
        {
            db.Questions.AddRange(
                new Question
                {
                    Id = Guid.Parse("cccccccc-2222-2222-2222-222222222222"),
                    LearningItemId = Guid.Parse("bbbbbbbb-2222-2222-2222-222222222222"),
                    PromptText = "Số nào là số 5?",
                    QuestionType = "choice",
                    PayloadJson = """{"choices":["3","5","8"]}""",
                    CorrectAnswerJson = """{"value":"5"}""",
                    HintJson = """{"level1":"Số 5 có nét ngang ở phía trên.","level2":"Con nhìn số ở giữa nhé."}""",
                    FeedbackJson = """{"correct":"Giỏi lắm! Đúng là số 5.","retry":"Mình thử lại nhẹ nhàng nhé."}""",
                    SortOrder = 1
                },
                new Question
                {
                    Id = Guid.Parse("cccccccc-3333-3333-3333-333333333333"),
                    LearningItemId = Guid.Parse("bbbbbbbb-3333-3333-3333-333333333333"),
                    PromptText = "Nhóm nào có 5 hạt dẻ?",
                    QuestionType = "choice",
                    PayloadJson = """{"choices":[{"id":"a","count":4},{"id":"b","count":5},{"id":"c","count":2}]}""",
                    CorrectAnswerJson = """{"value":"b"}""",
                    HintJson = """{"level1":"Con đếm từng hạt từ trái sang phải nhé."}""",
                    FeedbackJson = """{"correct":"Tuyệt vời, con đếm đúng rồi!","retry":"Không sao, mình đếm lại cùng nhau nhé."}""",
                    SortOrder = 1
                });
        }

        if (!await db.TracingTemplates.AnyAsync())
        {
            db.TracingTemplates.Add(new TracingTemplate
            {
                Id = Guid.Parse("dddddddd-1111-1111-1111-111111111111"),
                SymbolType = "uppercase",
                Symbol = "A",
                DisplayName = "Chữ A in hoa",
                GuideJson = """
                {
                  "strokes": [
                    {"order":1,"startPoint":{"x":290,"y":520},"endPoint":{"x":360,"y":170},"checkpoints":[{"x":320,"y":360}]},
                    {"order":2,"startPoint":{"x":360,"y":170},"endPoint":{"x":450,"y":520},"checkpoints":[{"x":405,"y":360}]},
                    {"order":3,"startPoint":{"x":325,"y":380},"endPoint":{"x":420,"y":380},"checkpoints":[{"x":370,"y":380}]}
                  ],
                  "tolerance": 44
                }
                """
            });
        }

        if (!await db.RewardDefinitions.AnyAsync())
        {
            db.RewardDefinitions.AddRange(
                new RewardDefinition { Id = Guid.Parse("eeeeeeee-1111-1111-1111-111111111111"), Code = "ngoi-sao-dau-tien", Name = "Ngôi sao đầu tiên", RewardType = "badge", IconKey = "star" },
                new RewardDefinition { Id = Guid.Parse("eeeeeeee-2222-2222-2222-222222222222"), Code = "ban-cua-soc-nau", Name = "Bạn của Sóc Nâu", RewardType = "garden_item", IconKey = "park" });
        }

        await SeedTracingQuestionAsync(db);
        await SeedMvpInteractionItemsAsync(db);
        await db.SaveChangesAsync();
    }

    private static async Task SeedTracingQuestionAsync(ApplicationDbContext db)
    {
        var questionId = Guid.Parse("cccccccc-1111-1111-1111-111111111111");
        if (await db.Questions.AnyAsync(x => x.Id == questionId))
        {
            return;
        }

        db.Questions.Add(new Question
        {
            Id = questionId,
            LearningItemId = Guid.Parse("bbbbbbbb-1111-1111-1111-111111111111"),
            PromptText = "Con tô chữ A theo nét gợi ý.",
            QuestionType = InteractionTypes.Tracing,
            PayloadJson = """{"symbol":"A","templateId":"dddddddd-1111-1111-1111-111111111111"}""",
            CorrectAnswerJson = """{"minPoints":20}""",
            HintJson = """{"level1":"Con bắt đầu từ chấm màu cam nhé."}""",
            FeedbackJson = """{"correct":"Tốt lắm, con đã tô xong chữ A!","retry":"Mình thử tô lại một nét nhé."}""",
            SortOrder = 1
        });
    }

    private static async Task SeedMvpInteractionItemsAsync(ApplicationDbContext db)
    {
        var now = DateTimeOffset.UtcNow;
        var seeds = new[]
        {
            new
            {
                ItemId = Guid.Parse("bbbbbbbb-4444-4444-4444-444444444444"),
                QuestionId = Guid.Parse("cccccccc-4444-4444-4444-444444444444"),
                Code = "nghe-chon-chu-a",
                Title = "Nghe và chọn chữ A",
                SkillGroupId = AlphabetGroupId,
                TopicId = Guid.Parse("aaaaaaaa-1111-1111-1111-111111111111"),
                Type = InteractionTypes.ListenAndChoose,
                Instruction = "Con nghe âm rồi chọn chữ đúng nhé.",
                Prompt = "Con hãy chọn chữ A.",
                Choices = new[] { "A", "B", "D" },
                Answer = "A"
            },
            new
            {
                ItemId = Guid.Parse("bbbbbbbb-5555-5555-5555-555555555555"),
                QuestionId = Guid.Parse("cccccccc-5555-5555-5555-555555555555"),
                Code = "keo-tha-so-voi-luong",
                Title = "Kéo số vào nhóm đúng",
                SkillGroupId = MathGroupId,
                TopicId = Guid.Parse("aaaaaaaa-3333-3333-3333-333333333333"),
                Type = InteractionTypes.DragDrop,
                Instruction = "Con chọn số phù hợp với nhóm đồ vật.",
                Prompt = "Nhóm có 4 quả bóng cần số nào?",
                Choices = new[] { "2", "4", "6" },
                Answer = "4"
            },
            new
            {
                ItemId = Guid.Parse("bbbbbbbb-6666-6666-6666-666666666666"),
                QuestionId = Guid.Parse("cccccccc-6666-6666-6666-666666666666"),
                Code = "noi-cap-hoa-thuong",
                Title = "Nối chữ hoa và chữ thường",
                SkillGroupId = AlphabetGroupId,
                TopicId = Guid.Parse("aaaaaaaa-1111-1111-1111-111111111111"),
                Type = InteractionTypes.Matching,
                Instruction = "Con chọn cặp chữ giống nhau.",
                Prompt = "Cặp nào nối đúng chữ hoa với chữ thường?",
                Choices = new[] { "A - a", "A - b", "B - a" },
                Answer = "A - a"
            },
            new
            {
                ItemId = Guid.Parse("bbbbbbbb-7777-7777-7777-777777777777"),
                QuestionId = Guid.Parse("cccccccc-7777-7777-7777-777777777777"),
                Code = "sap-xep-so-1-3",
                Title = "Sắp xếp số 1 đến 3",
                SkillGroupId = NumberGroupId,
                TopicId = Guid.Parse("aaaaaaaa-2222-2222-2222-222222222222"),
                Type = InteractionTypes.Ordering,
                Instruction = "Con chọn dãy số đúng thứ tự.",
                Prompt = "Dãy nào đi từ bé đến lớn?",
                Choices = new[] { "1 - 2 - 3", "3 - 2 - 1", "2 - 1 - 3" },
                Answer = "1 - 2 - 3"
            }
        };

        foreach (var seed in seeds)
        {
            if (await db.LearningItems.AnyAsync(x => x.Code == seed.Code))
            {
                continue;
            }

            var item = new LearningItem
            {
                Id = seed.ItemId,
                Code = seed.Code,
                Title = seed.Title,
                SkillGroupId = seed.SkillGroupId,
                TopicId = seed.TopicId,
                InteractionType = seed.Type,
                InstructionText = seed.Instruction,
                ContentJson = System.Text.Json.JsonSerializer.Serialize(new { choices = seed.Choices, answer = seed.Answer }),
                Status = ContentStatus.Published,
                PublishedAt = now,
                CreatedAt = now,
                UpdatedAt = now
            };

            item.Questions.Add(new Question
            {
                Id = seed.QuestionId,
                LearningItemId = seed.ItemId,
                PromptText = seed.Prompt,
                QuestionType = seed.Type,
                PayloadJson = System.Text.Json.JsonSerializer.Serialize(new { choices = seed.Choices }),
                CorrectAnswerJson = System.Text.Json.JsonSerializer.Serialize(new { value = seed.Answer }),
                HintJson = System.Text.Json.JsonSerializer.Serialize(new { level1 = "Con quan sát từng lựa chọn nhé." }),
                FeedbackJson = System.Text.Json.JsonSerializer.Serialize(new
                {
                    correct = "Tuyệt vời, con làm đúng rồi!",
                    retry = "Không sao, mình thử lại từng bước nhé."
                }),
                SortOrder = 1
            });

            db.LearningItems.Add(item);
        }
    }

    private static async Task SeedChildProfileAsync(ApplicationDbContext db, ApplicationUser? parentUser)
    {
        if (parentUser is null || await db.ChildProfiles.AnyAsync())
        {
            return;
        }

        db.ChildProfiles.Add(new ChildProfile
        {
            Id = Guid.Parse("99999999-1111-1111-1111-111111111111"),
            ParentUserId = parentUser.Id,
            Nickname = "Bé Sóc",
            BirthYear = DateTimeOffset.Now.Year - 5,
            AvatarKey = "soc-nau",
            DailyLearningMinutes = 15,
            SoundEnabled = true,
            PreferredSkillGroupIdsJson = $"""["{AlphabetGroupId}","{NumberGroupId}","{MathGroupId}"]"""
        });

        await db.SaveChangesAsync();
    }
}
