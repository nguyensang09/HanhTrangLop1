using HanhTrangLop1.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HanhTrangLop1.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<ChildProfile> ChildProfiles => Set<ChildProfile>();
    public DbSet<SkillGroup> SkillGroups => Set<SkillGroup>();
    public DbSet<Topic> Topics => Set<Topic>();
    public DbSet<MediaAsset> MediaAssets => Set<MediaAsset>();
    public DbSet<TextToSpeechCache> TextToSpeechCaches => Set<TextToSpeechCache>();
    public DbSet<LearningItem> LearningItems => Set<LearningItem>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<TracingTemplate> TracingTemplates => Set<TracingTemplate>();
    public DbSet<LearningSession> LearningSessions => Set<LearningSession>();
    public DbSet<LearningAttempt> LearningAttempts => Set<LearningAttempt>();
    public DbSet<QuestionAttempt> QuestionAttempts => Set<QuestionAttempt>();
    public DbSet<SkillProgress> SkillProgress => Set<SkillProgress>();
    public DbSet<RewardDefinition> RewardDefinitions => Set<RewardDefinition>();
    public DbSet<ChildReward> ChildRewards => Set<ChildReward>();
    public DbSet<GardenItem> GardenItems => Set<GardenItem>();
    public DbSet<ContentReview> ContentReviews => Set<ContentReview>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<SkillGroup>()
            .HasIndex(x => x.Code)
            .IsUnique();

        builder.Entity<Topic>()
            .HasIndex(x => new { x.SkillGroupId, x.Code })
            .IsUnique();

        builder.Entity<LearningItem>()
            .HasIndex(x => x.Code)
            .IsUnique();

        builder.Entity<LearningItem>()
            .HasIndex(x => new { x.SkillGroupId, x.TopicId, x.SortOrder });

        builder.Entity<TextToSpeechCache>()
            .HasIndex(x => new { x.Provider, x.Voice, x.ModelId, x.Format, x.TextHash })
            .IsUnique();

        builder.Entity<TextToSpeechCache>()
            .HasIndex(x => x.Name);

        builder.Entity<RewardDefinition>()
            .HasIndex(x => x.Code)
            .IsUnique();

        builder.Entity<SkillProgress>()
            .HasIndex(x => new { x.ChildProfileId, x.SkillGroupId })
            .IsUnique();

        builder.Entity<SkillProgress>()
            .Property(x => x.MasteryLevel)
            .HasPrecision(5, 2);

        builder.Entity<ChildProfile>()
            .HasOne(x => x.ParentUser)
            .WithMany()
            .HasForeignKey(x => x.ParentUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<LearningAttempt>()
            .HasOne(x => x.Session)
            .WithMany()
            .HasForeignKey(x => x.SessionId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<QuestionAttempt>()
            .HasOne(x => x.LearningAttempt)
            .WithMany()
            .HasForeignKey(x => x.LearningAttemptId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<QuestionAttempt>()
            .HasOne(x => x.Question)
            .WithMany()
            .HasForeignKey(x => x.QuestionId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
