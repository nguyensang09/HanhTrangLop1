using HanhTrangLop1.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

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
        await SeedRolesAsync(roleManager);
        await SeedAdminAsync(userManager, configuration, logger);
        await SeedCurriculumCatalogAsync(db);
        await BackfillTextToSpeechCacheAsync(db, configuration, logger);

        var createdLessons = await LearningContentSeed.SeedAsync(db);
        if (createdLessons > 0)
        {
            logger.LogInformation("Đã khởi tạo {LessonCount} bài học nền còn thiếu.", createdLessons);
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

    private static async Task BackfillTextToSpeechCacheAsync(
        ApplicationDbContext db,
        IConfiguration configuration,
        ILogger logger)
    {
        var provider = configuration["TextToSpeech:Provider"]?.Trim();
        var voice = configuration["TextToSpeech:Voice"]?.Trim();
        var modelId = configuration["TextToSpeech:ModelId"]?.Trim();
        var format = configuration["TextToSpeech:Format"]?.Trim();
        if (string.IsNullOrWhiteSpace(provider) ||
            string.IsNullOrWhiteSpace(voice) ||
            string.IsNullOrWhiteSpace(modelId) ||
            string.IsNullOrWhiteSpace(format))
        {
            return;
        }

        var existingHashes = await db.TextToSpeechCaches
            .Select(x => x.TextHash)
            .ToHashSetAsync(StringComparer.OrdinalIgnoreCase);
        var audioAssets = await db.MediaAssets
            .Where(x => x.AssetType == "audio" && !string.IsNullOrWhiteSpace(x.AltText))
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();

        var added = 0;
        foreach (var asset in audioAssets)
        {
            var normalizedText = NormalizeSpeechText(asset.AltText!);
            if (string.IsNullOrWhiteSpace(normalizedText) ||
                normalizedText.StartsWith("tts:v1:", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var hash = TextToSpeechHash(provider, voice, modelId, format, normalizedText);
            if (existingHashes.Contains(hash))
            {
                continue;
            }

            db.TextToSpeechCaches.Add(new TextToSpeechCache
            {
                Id = Guid.NewGuid(),
                Provider = provider,
                Voice = voice,
                ModelId = modelId,
                Format = format,
                TextHash = hash,
                NormalizedText = TrimMax(normalizedText, 500),
                OriginalText = TrimMax(asset.AltText!, 1000),
                AudioUrl = asset.StoragePath,
                Status = "ready",
                CreatedAt = asset.CreatedAt,
                UpdatedAt = DateTimeOffset.UtcNow
            });
            existingHashes.Add(hash);
            added += 1;
        }

        if (added > 0)
        {
            await db.SaveChangesAsync();
            logger.LogInformation("Đã chuẩn hóa {Count} file âm thanh cũ vào bảng kiểm soát voice.", added);
        }
    }

    private static string NormalizeSpeechText(string text)
    {
        return string.Join(' ', (text ?? string.Empty).Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    private static string TextToSpeechHash(string provider, string voice, string modelId, string format, string normalizedText)
    {
        var source = $"{provider}|{voice}|{modelId}|{format}|{normalizedText.ToLowerInvariant()}";
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(source))).ToLowerInvariant();
    }

    private static string TrimMax(string value, int maxLength)
    {
        return value.Length <= maxLength ? value : value[..maxLength];
    }
}
