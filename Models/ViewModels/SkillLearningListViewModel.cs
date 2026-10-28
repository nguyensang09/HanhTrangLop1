namespace HanhTrangLop1.Models.ViewModels;

public class SkillLearningListViewModel
{
    public ChildProfile? ChildProfile { get; set; }
    public SkillGroup SkillGroup { get; set; } = new();
    public IReadOnlyList<SkillLearningItemViewModel> Items { get; set; } = [];
    public Guid? LastPracticedItemId { get; set; }
    public Guid? CurrentItemId { get; set; }
    public IReadOnlyList<SkillMilestoneViewModel> Milestones { get; set; } = [];
    /// <summary>Số bài đã xong trên lộ trình hiện tại (sau khi học lại một bài cũ, các bài phía sau không còn tính).</summary>
    public int CompletedCount => Items.Count(x => x.IsPathCompleted);
    /// <summary>Số bài từng hoàn thành (lịch sử) — dùng cho rương quà, không bị giảm khi học lại.</summary>
    public int EverCompletedCount => Items.Count(x => x.EverCompleted);
}

public class SkillLearningItemViewModel
{
    public LearningItem Item { get; set; } = new();
    public string? LatestStatus { get; set; }
    public int StarsEarned { get; set; }
    public bool EverCompleted { get; set; }
    /// <summary>Đã xong trên lộ trình hiện tại (quyết định trạng thái xong / đang học / khóa trên bản đồ).</summary>
    public bool IsPathCompleted { get; set; }
}

public class SkillMilestoneViewModel
{
    public int MilestoneValue { get; set; }
    public string State { get; set; } = "locked";
    public Guid? GrantId { get; set; }
    public string RewardName { get; set; } = string.Empty;
    public string RewardIconKey { get; set; } = "redeem";
}
