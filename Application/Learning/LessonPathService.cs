using HanhTrangLop1.Data;
using HanhTrangLop1.Models;
using Microsoft.EntityFrameworkCore;

namespace HanhTrangLop1.Application.Learning;

/// <summary>Trạng thái lộ trình của một nhóm bài với một bé.</summary>
public sealed class SkillPathState
{
    public required SkillGroup SkillGroup { get; init; }
    public required IReadOnlyList<LearningItem> Items { get; init; }
    public required IReadOnlyDictionary<Guid, ChildLessonProgress> Progress { get; init; }

    /// <summary>Vị trí bài đang học (bài đầu tiên chưa xong trên lộ trình hiện tại); -1 khi bé đã về đích.</summary>
    public int CurrentIndex { get; init; }

    public int IndexOf(Guid itemId)
    {
        for (var i = 0; i < Items.Count; i++)
        {
            if (Items[i].Id == itemId) return i;
        }
        return -1;
    }

    public bool IsPathCompleted(Guid itemId) =>
        Progress.TryGetValue(itemId, out var progress) && progress.PathCompletedAt is not null;

    /// <summary>Bài nằm sau bài đang học thì khóa, kể cả khi trước đây bé đã từng hoàn thành.</summary>
    public bool IsLocked(Guid itemId)
    {
        var index = IndexOf(itemId);
        return index >= 0 && CurrentIndex >= 0 && index > CurrentIndex;
    }
}

/// <summary>
/// Quy tắc lộ trình học của các nhóm bài (bản đồ /kids/skills): thứ tự bài, bài đang học, bài bị khóa và
/// thao tác "học lại" đưa tiến trình về một bài cũ. Mọi nơi hiển thị hay kiểm tra khóa/mở đều dùng chung lớp này
/// để bản đồ, trang làm bài và nút "Học tiếp" luôn thống nhất.
/// </summary>
public sealed class LessonPathService
{
    private readonly ApplicationDbContext _db;

    public LessonPathService(ApplicationDbContext db) => _db = db;

    /// <summary>Các bài của nhóm theo đúng thứ tự trên bản đồ.</summary>
    public async Task<List<LearningItem>> LoadPathItemsAsync(SkillGroup skillGroup, CancellationToken cancellationToken = default)
    {
        var items = await _db.LearningItems
            .AsNoTracking()
            .Include(x => x.SkillGroup)
            .Include(x => x.Topic)
            .Include(x => x.Questions)
            .Where(x => x.SkillGroupId == skillGroup.Id && x.Status == ContentStatus.Published)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Level)
            .ThenBy(x => x.Title)
            .ToListAsync(cancellationToken);

        // Bài tập tô học riêng ở /kids/tracing (trừ nhóm Vận động tinh, nơi tô nét là một phần lộ trình).
        if (skillGroup.Code != "van-dong-tinh")
        {
            items = items.Where(x => x.InteractionType != InteractionTypes.Tracing).ToList();
        }
        return items.Where(ActivityTemplateCatalog.IsItemAllowed).ToList();
    }

    public async Task<SkillPathState?> GetStateAsync(ChildProfile child, Guid skillGroupId, CancellationToken cancellationToken = default)
    {
        var skillGroup = await _db.SkillGroups.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == skillGroupId && x.IsActive, cancellationToken);
        return skillGroup is null ? null : await GetStateAsync(child, skillGroup, cancellationToken);
    }

    public async Task<SkillPathState> GetStateAsync(ChildProfile child, SkillGroup skillGroup, CancellationToken cancellationToken = default)
    {
        var items = await LoadPathItemsAsync(skillGroup, cancellationToken);
        var itemIds = items.Select(x => x.Id).ToList();
        var progress = itemIds.Count == 0
            ? new Dictionary<Guid, ChildLessonProgress>()
            : await _db.ChildLessonProgresses.AsNoTracking()
                .Where(x => x.ChildProfileId == child.Id && x.ProgressEpoch == child.ProgressEpoch && itemIds.Contains(x.LearningItemId))
                .ToDictionaryAsync(x => x.LearningItemId, cancellationToken);

        var currentIndex = items.FindIndex(x => !progress.TryGetValue(x.Id, out var p) || p.PathCompletedAt is null);
        return new SkillPathState
        {
            SkillGroup = skillGroup,
            Items = items,
            Progress = progress,
            CurrentIndex = currentIndex
        };
    }

    /// <summary>
    /// Học lại một bài đã hoàn thành: bài đó trở thành bài đang học, các bài trước giữ trạng thái hoàn thành,
    /// mọi bài phía sau bị khóa lại (kể cả bài đã từng xong). Lịch sử (lần hoàn thành đầu, sao, số lần làm, các lượt
    /// làm bài) được giữ nguyên cho thống kê; chỉ tiến độ trên lộ trình hiện tại được đưa về vị trí mới.
    /// Trả về false nếu bài không thuộc lộ trình hoặc chưa từng được mở.
    /// </summary>
    public async Task<bool> RewindToAsync(ChildProfile child, SkillPathState state, Guid itemId, CancellationToken cancellationToken = default)
    {
        var index = state.IndexOf(itemId);
        if (index < 0) return false;
        // Chỉ được học lại bài đã xong hoặc chính bài đang học; không nhảy cóc tới bài đang khóa.
        if (!state.IsPathCompleted(itemId) && index != state.CurrentIndex) return false;

        var resetIds = state.Items.Skip(index).Select(x => x.Id).ToList();
        await _db.ChildLessonProgresses
            .Where(x => x.ChildProfileId == child.Id && x.ProgressEpoch == child.ProgressEpoch &&
                        resetIds.Contains(x.LearningItemId) && x.PathCompletedAt != null)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.PathCompletedAt, (DateTimeOffset?)null), cancellationToken);
        return true;
    }
}
