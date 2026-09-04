using System.Text.Json;
using HanhTrangLop1.Data;
using HanhTrangLop1.Models;
using HanhTrangLop1.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace HanhTrangLop1.Application.Learning;

public class TodayLessonService
{
    private readonly ApplicationDbContext _db;

    public TodayLessonService(ApplicationDbContext db)
    {
        _db = db;
    }

    public static readonly IReadOnlyList<(int Day, string Title, string Description, string Icon, string Color)> DayThemes = new (int, string, string, string, string)[]
    {
        (1, "Ngày 1: Khởi đầu nét chữ & Chữ cái A", "Luyện nét sổ thẳng, nét ngang và khám phá chữ A", "menu_book", "#ff7a00"),
        (2, "Ngày 2: Chữ Ă & Số 1 đầu tiên", "Luyện viết chữ Ă, số 1 và đếm 1 đồ vật", "pin", "#f59e0b"),
        (3, "Ngày 3: Chữ Â & Nhận biết Hình Tròn", "Làm quen chữ Â, hình tròn và ghép bóng", "category", "#0284c7"),
        (4, "Ngày 4: Chữ B & Số 2 đáng yêu", "Khám phá chữ B, số 2 và đếm số lượng 2", "calculate", "#10b981"),
        (5, "Ngày 5: Chữ C & Kỹ năng rửa tay sạch", "Chữ C, nét cong hở và thói quen vệ sinh", "volunteer_activism", "#8b5cf6"),
        (6, "Ngày 6: Ôn tập tuần 1 & Số 3", "Tổng hợp chữ A-Ă-Â-B-C và số lượng 3", "stars", "#ec4899"),
        (7, "Ngày 7: Chữ D & So sánh nhiều - ít", "Nhận biết chữ D và so sánh số lượng", "balance", "#0d9488"),
        (8, "Ngày 8: Chữ Đ & Số 4 xinh xắn", "Khám phá chữ Đ và nhận biết số 4", "pin", "#ff7a00"),
        (9, "Ngày 9: Chữ E, Ê & Hình Vuông", "Luyện chữ E, Ê, hình vuông và phân loại", "category", "#0284c7"),
        (10, "Ngày 10: Số 5 & Quy luật hình ảnh", "Đếm số lượng 5 và tìm quy luật logic", "psychology", "#f59e0b"),
        (11, "Ngày 11: Chữ G & Kỹ năng qua đường an toàn", "Nhận biết chữ G và quy tắc an toàn", "volunteer_activism", "#10b981"),
        (12, "Ngày 12: Chữ H, I & Số 6", "Làm quen chữ H, chữ I và số lượng 6", "calculate", "#8b5cf6"),
        (13, "Ngày 13: Chữ K, L & Hình Tam Giác", "Chữ K, chữ L và nhận biết hình tam giác", "category", "#0d9488"),
        (14, "Ngày 14: Ôn tập chữ cái & Số 7", "Rèn luyện nhận biết chữ cái và đếm số 7", "stars", "#ec4899"),
        (15, "Ngày 15: Chữ M, N & Tách gộp số lượng", "Khám phá chữ M, N và tách gộp nhóm", "calculate", "#ff7a00"),
        (16, "Ngày 16: Chữ O, Ô, Ơ & Số 8", "Bộ 3 chữ O-Ô-Ơ và số lượng 8", "menu_book", "#f59e0b"),
        (17, "Ngày 17: Chữ P, Q & Vị trí không gian", "Chữ P, Q và phân biệt trên - dưới, trái - phải", "explore", "#0284c7"),
        (18, "Ngày 18: Chữ R, S & Số 9", "Làm quen chữ R, S và nhận biết số 9", "pin", "#10b981"),
        (19, "Ngày 19: Chữ T, U, Ư & Phép cộng trực quan", "Chữ T-U-Ư và làm quen phép cộng hoa quả", "calculate", "#8b5cf6"),
        (20, "Ngày 20: Chữ V, X, Y & Bé tự lập", "Hoàn thành 29 chữ cái và thói quen tự phục vụ", "emoji_events", "#0d9488")
    };

    public async Task<int> GetCurrentDayNumberAsync(ChildProfile child)
    {
        var completedSessions = await _db.LearningSessions
            .AsNoTracking()
            .Where(x => x.ChildProfileId == child.Id && x.Status == "completed")
            .CountAsync();

        return Math.Clamp(completedSessions + 1, 1, DayThemes.Count);
    }

    public async Task<LearningSession> GetOrCreateActiveSessionAsync(ChildProfile child, int? specificDay = null)
    {
        var currentDay = await GetCurrentDayNumberAsync(child);
        var targetDay = specificDay ?? currentDay;
        targetDay = Math.Clamp(targetDay, 1, DayThemes.Count);

        // Tìm phiên của ngày được chọn
        var existingSession = await _db.LearningSessions
            .Where(x => x.ChildProfileId == child.Id && x.PlannedMinutes == targetDay)
            .OrderByDescending(x => x.StartedAt)
            .FirstOrDefaultAsync();

        if (existingSession is not null)
        {
            var ids = ReadPlanIds(existingSession.SessionPlanJson);
            // Nếu phiên đang học có ít hơn 12 bài, tự động mở rộng lên đủ 12 bài
            if (ids.Count < 12)
            {
                var fullPlan = await BuildDayPlanAsync(targetDay);
                var combined = ids.Concat(fullPlan.Where(id => !ids.Contains(id))).Take(12).ToList();
                existingSession.SessionPlanJson = JsonSerializer.Serialize(combined);
                await _db.SaveChangesAsync();
            }
            return existingSession;
        }

        var planItemIds = await BuildDayPlanAsync(targetDay);
        var session = new LearningSession
        {
            Id = Guid.NewGuid(),
            ChildProfileId = child.Id,
            PlannedMinutes = targetDay, // Dùng PlannedMinutes để lưu số thứ tự Ngày học (Day 1, 2, 3...)
            Status = "active",
            SessionPlanJson = JsonSerializer.Serialize(planItemIds),
            StartedAt = DateTimeOffset.UtcNow
        };

        _db.LearningSessions.Add(session);
        await _db.SaveChangesAsync();
        return session;
    }

    public async Task<TodayLessonViewModel> BuildTodayViewModelAsync(ChildProfile child, LearningSession session, int selectedDay)
    {
        var currentDay = await GetCurrentDayNumberAsync(child);
        selectedDay = Math.Clamp(selectedDay, 1, DayThemes.Count);

        var items = await GetSessionItemsAsync(session);
        var attempts = await _db.LearningAttempts
            .AsNoTracking()
            .Where(x => x.SessionId == session.Id)
            .ToListAsync();

        var latestAttemptByItemId = attempts
            .GroupBy(x => x.LearningItemId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(x => x.StartedAt).First());

        var firstOpenStepFound = false;
        var steps = items.Select(item =>
        {
            latestAttemptByItemId.TryGetValue(item.Id, out var attempt);

            if (attempt?.Status == "completed")
            {
                return new TodayLessonStepViewModel
                {
                    Item = item,
                    Status = TodayLessonStepStatus.Completed,
                    StarsEarned = attempt.StarsEarned
                };
            }

            if (attempt?.Status == "needs_practice")
            {
                return new TodayLessonStepViewModel
                {
                    Item = item,
                    Status = TodayLessonStepStatus.NeedsPractice,
                    StarsEarned = attempt.StarsEarned
                };
            }

            if (!firstOpenStepFound)
            {
                firstOpenStepFound = true;
                return new TodayLessonStepViewModel
                {
                    Item = item,
                    Status = TodayLessonStepStatus.Active
                };
            }

            return new TodayLessonStepViewModel
            {
                Item = item,
                Status = TodayLessonStepStatus.Locked
            };
        }).ToList();

        var roadmapDays = await BuildRoadmapDaysAsync(child, currentDay);
        var theme = DayThemes.FirstOrDefault(x => x.Day == selectedDay);

        return new TodayLessonViewModel
        {
            ChildProfile = child,
            Session = session,
            CurrentDayNumber = currentDay,
            SelectedDayNumber = selectedDay,
            DayThemeTitle = theme.Title ?? $"Ngày {selectedDay}: Học vui cùng Sóc Nâu",
            Steps = steps,
            RoadmapDays = roadmapDays
        };
    }

    public async Task<List<DailyRoadmapItemViewModel>> BuildRoadmapDaysAsync(ChildProfile child, int currentDay)
    {
        var completedDayNumbers = await _db.LearningSessions
            .AsNoTracking()
            .Where(x => x.ChildProfileId == child.Id && x.Status == "completed")
            .Select(x => x.PlannedMinutes)
            .ToHashSetAsync();

        return DayThemes.Select(theme =>
        {
            var isCompleted = completedDayNumbers.Contains(theme.Day);
            var isCurrent = theme.Day == currentDay;
            var isLocked = theme.Day > currentDay && !isCompleted;

            return new DailyRoadmapItemViewModel
            {
                DayNumber = theme.Day,
                Title = theme.Title,
                Description = theme.Description,
                IconKey = theme.Icon,
                ColorHex = theme.Color,
                IsCompleted = isCompleted,
                IsCurrent = isCurrent,
                IsLocked = isLocked,
                StarsEarned = isCompleted ? 3 : 0
            };
        }).ToList();
    }

    public async Task<IReadOnlyList<LearningItem>> GetSessionItemsAsync(LearningSession session)
    {
        var ids = ReadPlanIds(session.SessionPlanJson);
        if (ids.Count == 0)
        {
            return [];
        }

        var items = await _db.LearningItems
            .AsNoTracking()
            .Include(x => x.SkillGroup)
            .Include(x => x.Topic)
            .Where(x => ids.Contains(x.Id) && x.Status == ContentStatus.Published)
            .ToListAsync();
        var itemById = items
            .Where(ActivityTemplateCatalog.IsItemAllowed)
            .ToDictionary(x => x.Id);

        return ids
            .Where(itemById.ContainsKey)
            .Select(id => itemById[id])
            .ToList();
    }

    public Task<Guid?> FindNextItemIdAsync(LearningSession session, Guid currentItemId)
    {
        var ids = ReadPlanIds(session.SessionPlanJson);
        var currentIndex = ids.IndexOf(currentItemId);
        Guid? nextId = currentIndex >= 0 && currentIndex + 1 < ids.Count ? ids[currentIndex + 1] : null;
        return Task.FromResult(nextId);
    }

    public async Task CompleteSessionAsync(LearningSession session)
    {
        var attempts = await _db.LearningAttempts
            .AsNoTracking()
            .Where(x => x.SessionId == session.Id)
            .ToListAsync();

        session.Status = "completed";
        session.EndedAt = DateTimeOffset.UtcNow;
        session.ActualSeconds = attempts.Sum(x => Math.Max(30, x.DurationSeconds));
        if (session.ActualSeconds == 0)
        {
            session.ActualSeconds = 60;
        }

        await _db.SaveChangesAsync();
    }

    private async Task<List<Guid>> BuildDayPlanAsync(int dayNumber)
    {
        var allItems = await _db.LearningItems
            .AsNoTracking()
            .Include(x => x.SkillGroup)
            .Include(x => x.Topic)
            .Where(x => x.Status == ContentStatus.Published)
            .OrderBy(x => x.SkillGroup!.SortOrder)
            .ThenBy(x => x.Topic!.SortOrder)
            .ThenBy(x => x.SortOrder)
            .ToListAsync();

        allItems = allItems.Where(ActivityTemplateCatalog.IsItemAllowed).ToList();
        if (allItems.Count == 0) return [];

        // Phân nhóm theo kỹ năng để bốc bài phối hợp
        var letterItems = allItems.Where(x => x.SkillGroup?.Code == "chu-cai").ToList();
        var numberItems = allItems.Where(x => x.SkillGroup?.Code == "chu-so" || x.SkillGroup?.Code == "so-luong-toan").ToList();
        var logicAndLifeItems = allItems.Where(x => x.SkillGroup?.Code == "tu-duy-logic" || x.SkillGroup?.Code == "ky-nang-song" || x.SkillGroup?.Code == "hinh-dang-khong-gian" || x.SkillGroup?.Code == "ngon-ngu").ToList();
        var tracingItems = allItems.Where(x => x.InteractionType == InteractionTypes.Tracing).ToList();

        var selected = new List<Guid>(capacity: 12);
        var selectedIds = new HashSet<Guid>();

        void AddFromList(List<LearningItem> list, int offset)
        {
            if (list.Count > 0)
            {
                var idx = Math.Abs(offset) % list.Count;
                var item = list[idx];
                if (selectedIds.Add(item.Id))
                {
                    selected.Add(item.Id);
                }
            }
        }

        // Mỗi ngày gồm 12 bài phối hợp hài hòa các môn (4 nhóm x 3 bài = 12 bài)
        // 1. Nhận biết chữ cái (3 bài)
        AddFromList(letterItems, (dayNumber - 1) * 3);
        AddFromList(letterItems, (dayNumber - 1) * 3 + 1);
        AddFromList(letterItems, (dayNumber - 1) * 3 + 2);

        // 2. Chữ số & Số lượng toán học (3 bài)
        AddFromList(numberItems, (dayNumber - 1) * 3);
        AddFromList(numberItems, (dayNumber - 1) * 3 + 1);
        AddFromList(numberItems, (dayNumber - 1) * 3 + 2);

        // 3. Tư duy Logic, Không gian & Kỹ năng sống (3 bài)
        AddFromList(logicAndLifeItems, (dayNumber - 1) * 3);
        AddFromList(logicAndLifeItems, (dayNumber - 1) * 3 + 1);
        AddFromList(logicAndLifeItems, (dayNumber - 1) * 3 + 2);

        // 4. Luyện nét & Tập tô (3 bài)
        AddFromList(tracingItems, (dayNumber - 1) * 3);
        AddFromList(tracingItems, (dayNumber - 1) * 3 + 1);
        AddFromList(tracingItems, (dayNumber - 1) * 3 + 2);

        // Nếu danh sách chưa đủ 12 bài, bù thêm các bài học tiếp theo
        var stepOffset = (dayNumber - 1) * 12;
        for (var i = 0; i < allItems.Count && selected.Count < 12; i++)
        {
            var idx = (stepOffset + i) % allItems.Count;
            var item = allItems[idx];
            if (selectedIds.Add(item.Id))
            {
                selected.Add(item.Id);
            }
        }

        return selected;
    }

    private static List<Guid> ReadPlanIds(string sessionPlanJson)
    {
        try
        {
            return JsonSerializer.Deserialize<List<Guid>>(sessionPlanJson) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
