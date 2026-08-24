using HanhTrangLop1.Data;
using HanhTrangLop1.Models;

namespace HanhTrangLop1.Models.ViewModels;

public class AdminParentsAndKidsViewModel
{
    public string SearchQuery { get; set; } = string.Empty;
    public List<AdminParentItemViewModel> Parents { get; set; } = [];
    public List<AdminChildItemViewModel> GuestChildren { get; set; } = [];
    public int TotalParentsCount { get; set; }
    public int TotalChildrenCount { get; set; }
    public int TotalCompletedSessions { get; set; }
}

public class AdminParentItemViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public DateTimeOffset? CreatedAt { get; set; }
    public List<AdminChildItemViewModel> Children { get; set; } = [];
}

public class AdminChildItemViewModel
{
    public Guid Id { get; set; }
    public string Nickname { get; set; } = string.Empty;
    public string AvatarKey { get; set; } = string.Empty;
    public int? BirthYear { get; set; }
    public int DailyLearningMinutes { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string? ParentEmail { get; set; }
    public string? ParentDisplayName { get; set; }
    public int TotalStars { get; set; }
    public int CompletedLessonsCount { get; set; }
    public int TotalSessionsCount { get; set; }
    public int BadgesCount { get; set; }
    public DateTimeOffset? LastActiveAt { get; set; }
}

public class AdminChildDetailViewModel
{
    public ChildProfile Child { get; set; } = null!;
    public ApplicationUser? Parent { get; set; }
    public int TotalStars { get; set; }
    public int CompletedLessonsCount { get; set; }
    public int NeedsPracticeCount { get; set; }
    public int TotalSessionsCount { get; set; }
    public List<ChildReward> EarnedRewards { get; set; } = [];
    public List<LearningAttempt> RecentAttempts { get; set; } = [];
    public List<LearningSession> RecentSessions { get; set; } = [];
    public List<SkillProgress> SkillProgresses { get; set; } = [];
}
