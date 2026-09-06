using HanhTrangLop1.Data;
using HanhTrangLop1.Models;
using Microsoft.EntityFrameworkCore;

namespace HanhTrangLop1.Application.Learning;

public sealed record RewardProgressResult(bool IsFirstCompletion, int BestStars, int UniqueCompletedInSkill, RewardGrant? NewlyUnlockedGrant);

public sealed class RewardProgressionService
{
    public const int LessonsPerMilestone = 10;
    private readonly ApplicationDbContext _db;

    public RewardProgressionService(ApplicationDbContext db) => _db = db;

    public async Task<RewardProgressResult> RecordAttemptAsync(
        ChildProfile child,
        LearningItem item,
        bool isCompleted,
        int stars,
        string latestStatus,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var currentEpoch = await _db.ChildProfiles
            .Where(x => x.Id == child.Id)
            .Select(x => x.ProgressEpoch)
            .SingleAsync(cancellationToken);
        if (currentEpoch != child.ProgressEpoch)
            throw new InvalidOperationException("Tiến độ học đã được quản trị viên đặt lại. Vui lòng tải lại bài học.");

        var progress = await _db.ChildLessonProgresses.FirstOrDefaultAsync(
            x => x.ChildProfileId == child.Id && x.LearningItemId == item.Id,
            cancellationToken);
        if (progress is null)
        {
            progress = new ChildLessonProgress
            {
                Id = Guid.NewGuid(),
                ChildProfileId = child.Id,
                LearningItemId = item.Id,
                ProgressEpoch = child.ProgressEpoch
            };
            _db.ChildLessonProgresses.Add(progress);
        }

        if (progress.ProgressEpoch != child.ProgressEpoch)
            throw new InvalidOperationException("Tiến độ học đã được quản trị viên đặt lại. Vui lòng tải lại bài học.");

        var isFirstCompletion = isCompleted && progress.FirstCompletedAt is null;
        progress.AttemptCount++;
        if (progress.FirstCompletedAt is not null) progress.ReviewCount++;
        progress.LastAttemptedAt = now;
        progress.LatestStatus = latestStatus;
        progress.BestStars = Math.Max(progress.BestStars, isCompleted ? Math.Clamp(stars, 0, 3) : 0);
        if (isFirstCompletion) progress.FirstCompletedAt = now;

        var completedBefore = await _db.ChildLessonProgresses
            .Where(x => x.ChildProfileId == child.Id && x.ProgressEpoch == child.ProgressEpoch &&
                        x.FirstCompletedAt != null && x.LearningItem!.SkillGroupId == item.SkillGroupId &&
                        x.LearningItemId != item.Id)
            .CountAsync(cancellationToken);
        var uniqueCompleted = completedBefore + (isCompleted && progress.FirstCompletedAt is not null ? 1 : 0);
        RewardGrant? grant = null;
        if (isFirstCompletion && uniqueCompleted > 0 && uniqueCompleted % LessonsPerMilestone == 0)
            grant = await EnsureMilestoneGrantAsync(child, item.SkillGroupId, uniqueCompleted, cancellationToken);

        return new RewardProgressResult(isFirstCompletion, progress.BestStars, uniqueCompleted, grant);
    }

    public async Task EnsureBackfilledMilestoneGrantsAsync(CancellationToken cancellationToken = default)
    {
        var counts = await _db.ChildLessonProgresses
            .Where(x => x.FirstCompletedAt != null && x.ProgressEpoch == x.ChildProfile!.ProgressEpoch)
            .GroupBy(x => new { x.ChildProfileId, x.ChildProfile!.ProgressEpoch, x.LearningItem!.SkillGroupId })
            .Select(g => new { g.Key.ChildProfileId, Epoch = g.Key.ProgressEpoch, g.Key.SkillGroupId, Count = g.Count() })
            .Where(x => x.Count >= LessonsPerMilestone)
            .ToListAsync(cancellationToken);

        if (counts.Count == 0) return;
        var pool = await _db.RewardDefinitions.AsNoTracking()
            .Where(x => x.IsActive && x.RewardType == "item")
            .OrderBy(x => x.Code)
            .ToListAsync(cancellationToken);
        if (pool.Count == 0) return;
        var groupOrders = await _db.SkillGroups.AsNoTracking().ToDictionaryAsync(x => x.Id, x => x.SortOrder, cancellationToken);
        var childIds = counts.Select(x => x.ChildProfileId).Distinct().ToList();
        var ownedItems = await _db.ChildRewards.AsNoTracking()
            .Where(x => childIds.Contains(x.ChildProfileId) && x.RewardDefinition!.RewardType == "item")
            .Select(x => new { x.ChildProfileId, x.RewardDefinitionId, x.EarnedAt })
            .ToListAsync(cancellationToken);
        var inventoryKeys = (await _db.ChildInventoryItems.AsNoTracking()
            .Where(x => childIds.Contains(x.ChildProfileId))
            .Select(x => new { x.ChildProfileId, x.RewardDefinitionId })
            .ToListAsync(cancellationToken))
            .Select(x => $"{x.ChildProfileId:N}|{x.RewardDefinitionId:N}")
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var owned in ownedItems)
        {
            var inventoryKey = $"{owned.ChildProfileId:N}|{owned.RewardDefinitionId:N}";
            if (!inventoryKeys.Add(inventoryKey)) continue;
            _db.ChildInventoryItems.Add(new ChildInventoryItem
            {
                Id = Guid.NewGuid(), ChildProfileId = owned.ChildProfileId,
                RewardDefinitionId = owned.RewardDefinitionId, Quantity = 1,
                UpdatedAt = owned.EarnedAt
            });
        }
        var existingKeys = (await _db.RewardGrants.AsNoTracking()
            .Where(x => childIds.Contains(x.ChildProfileId) && x.SourceType == "skill_milestone")
            .Select(x => new { x.ChildProfileId, x.SourceKey })
            .ToListAsync(cancellationToken))
            .Select(x => $"{x.ChildProfileId:N}|{x.SourceKey}")
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var count in counts)
        {
            for (var milestone = LessonsPerMilestone; milestone <= count.Count; milestone += LessonsPerMilestone)
            {
                var sourceKey = $"skill:{count.SkillGroupId:N}:milestone:{milestone}";
                if (!existingKeys.Add($"{count.ChildProfileId:N}|{sourceKey}")) continue;
                groupOrders.TryGetValue(count.SkillGroupId, out var groupOrder);
                var reward = pool[Math.Abs(groupOrder + milestone / LessonsPerMilestone - 2) % pool.Count];
                _db.RewardGrants.Add(new RewardGrant
                {
                    Id = Guid.NewGuid(), ChildProfileId = count.ChildProfileId,
                    RewardDefinitionId = reward.Id, SourceType = "skill_milestone", SourceKey = sourceKey,
                    SkillGroupId = count.SkillGroupId, MilestoneValue = milestone,
                    State = RewardGrantStates.Claimable,
                    UnlockedAt = DateTimeOffset.UtcNow,
                    ProgressEpoch = count.Epoch
                });
            }
        }
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task<RewardGrant?> EnsureMilestoneGrantAsync(
        ChildProfile child,
        Guid skillGroupId,
        int milestone,
        CancellationToken cancellationToken)
    {
        var sourceKey = $"skill:{skillGroupId:N}:milestone:{milestone}";
        var tracked = _db.ChangeTracker.Entries<RewardGrant>().Select(x => x.Entity)
            .FirstOrDefault(x => x.ChildProfileId == child.Id && x.SourceKey == sourceKey);
        if (tracked is not null) return tracked;

        var existing = await _db.RewardGrants.FirstOrDefaultAsync(
            x => x.ChildProfileId == child.Id && x.SourceType == "skill_milestone" && x.SourceKey == sourceKey,
            cancellationToken);
        if (existing is not null) return existing;

        var pool = await _db.RewardDefinitions.AsNoTracking()
            .Where(x => x.IsActive && x.RewardType == "item")
            .OrderBy(x => x.Code)
            .ToListAsync(cancellationToken);
        if (pool.Count == 0) return null;

        var groupOrder = await _db.SkillGroups.AsNoTracking()
            .Where(x => x.Id == skillGroupId)
            .Select(x => x.SortOrder)
            .FirstOrDefaultAsync(cancellationToken);
        var reward = pool[Math.Abs(groupOrder + milestone / LessonsPerMilestone - 2) % pool.Count];
        var grant = new RewardGrant
        {
            Id = Guid.NewGuid(),
            ChildProfileId = child.Id,
            RewardDefinitionId = reward.Id,
            SourceType = "skill_milestone",
            SourceKey = sourceKey,
            SkillGroupId = skillGroupId,
            MilestoneValue = milestone,
            State = RewardGrantStates.Claimable,
            UnlockedAt = DateTimeOffset.UtcNow,
            ProgressEpoch = child.ProgressEpoch
        };
        _db.RewardGrants.Add(grant);
        return grant;
    }
}
