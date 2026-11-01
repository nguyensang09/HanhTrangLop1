using HanhTrangLop1.Application.Learning;
using HanhTrangLop1.Models;
using Microsoft.EntityFrameworkCore;

namespace HanhTrangLop1.Data;

/// <summary>
/// Gộp nhóm "Số lượng và toán học" vào "Số và Đếm". Chạy một lần khi nhóm cũ còn trong CSDL: bài, chủ đề, tiến độ tổng hợp
/// và phần quà chuyển sang nhóm chung rồi mới bỏ nhóm cũ. Id bài không đổi nên tiến độ và lịch sử làm bài của bé giữ nguyên.
/// </summary>
public static class SkillGroupMerge
{
    public static async Task MergeQuantityIntoNumbersAsync(ApplicationDbContext db)
    {
        var from = CurriculumCatalog.RetiredQuantityGroupId;
        var to = CurriculumCatalog.NumbersGroupId;
        var retired = await db.SkillGroups.FirstOrDefaultAsync(x => x.Id == from);
        if (retired is null) return;

        await using var transaction = await db.Database.BeginTransactionAsync();

        await db.LearningItems.Where(x => x.SkillGroupId == from).ExecuteUpdateAsync(s => s.SetProperty(x => x.SkillGroupId, to));
        await db.Topics.Where(x => x.SkillGroupId == from).ExecuteUpdateAsync(s => s.SetProperty(x => x.SkillGroupId, to));

        var progressRows = await db.SkillProgress.Where(x => x.SkillGroupId == from || x.SkillGroupId == to).ToListAsync();
        foreach (var old in progressRows.Where(x => x.SkillGroupId == from).ToList())
        {
            var target = progressRows.FirstOrDefault(x => x.SkillGroupId == to && x.ChildProfileId == old.ChildProfileId);
            if (target is null)
            {
                old.SkillGroupId = to;
                continue;
            }

            // Số liệu cộng dồn như thể bé luôn học trong một nhóm; mức thành thạo vốn cộng dần và chặn ở 100.
            target.CompletedItems += old.CompletedItems;
            target.NeedsPracticeItems += old.NeedsPracticeItems;
            target.MasteryLevel = Math.Min(100, target.MasteryLevel + old.MasteryLevel);
            if (old.LastPracticedAt > target.LastPracticedAt || target.LastPracticedAt is null)
            {
                target.LastPracticedAt = old.LastPracticedAt;
                target.SummaryJson = old.SummaryJson;
            }
            db.SkillProgress.Remove(old);
        }

        // Mốc quà đếm theo số bài trong nhóm: quà cũ nối tiếp sau các mốc của nhóm chung để bé giữ quà đã có mà không bị tính trùng mốc.
        var grants = await db.RewardGrants.Where(x => x.SkillGroupId == from || x.SkillGroupId == to).ToListAsync();
        foreach (var childGrants in grants.GroupBy(x => x.ChildProfileId))
        {
            var milestone = childGrants.Where(x => x.SkillGroupId == to && x.SourceType == "skill_milestone").Select(x => x.MilestoneValue).DefaultIfEmpty(0).Max();
            foreach (var grant in childGrants.Where(x => x.SkillGroupId == from).OrderBy(x => x.MilestoneValue))
            {
                grant.SkillGroupId = to;
                if (grant.SourceType != "skill_milestone") continue;
                milestone += RewardProgressionService.LessonsPerMilestone;
                grant.MilestoneValue = milestone;
                grant.SourceKey = $"skill:{to:N}:milestone:{milestone}";
            }
        }

        var setting = await db.KidsModuleSettings.FirstOrDefaultAsync(x => x.Key == retired.Code);
        if (setting is not null) db.KidsModuleSettings.Remove(setting);
        db.SkillGroups.Remove(retired);

        await db.SaveChangesAsync();
        await transaction.CommitAsync();
    }
}
