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

    public async Task<LearningSession> GetOrCreateActiveSessionAsync(ChildProfile child)
    {
        var today = DateTimeOffset.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var activeSession = await _db.LearningSessions
            .FirstOrDefaultAsync(x =>
                x.ChildProfileId == child.Id &&
                x.Status == "active" &&
                x.StartedAt >= today &&
                x.StartedAt < tomorrow);

        if (activeSession is not null)
        {
            return activeSession;
        }

        var planItemIds = await BuildPlanAsync(child);
        var session = new LearningSession
        {
            Id = Guid.NewGuid(),
            ChildProfileId = child.Id,
            PlannedMinutes = child.DailyLearningMinutes,
            Status = "active",
            SessionPlanJson = JsonSerializer.Serialize(planItemIds)
        };

        _db.LearningSessions.Add(session);
        await _db.SaveChangesAsync();
        return session;
    }

    public async Task<IReadOnlyList<LearningItem>> GetSessionItemsAsync(LearningSession session)
    {
        var ids = ReadPlanIds(session.SessionPlanJson);
        if (ids.Count == 0)
        {
            return [];
        }

        var items = await _db.LearningItems
            .Include(x => x.SkillGroup)
            .Include(x => x.Topic)
            .Where(x => ids.Contains(x.Id) && x.Status == ContentStatus.Published)
            .ToListAsync();
        items = items.Where(ActivityTemplateCatalog.IsItemAllowed).ToList();

        return ids
            .Select(id => items.FirstOrDefault(x => x.Id == id))
            .Where(x => x is not null)
            .Cast<LearningItem>()
            .ToList();
    }

    public async Task<Guid?> FindNextItemIdAsync(LearningSession session, Guid currentItemId)
    {
        var ids = ReadPlanIds(session.SessionPlanJson);
        var currentIndex = ids.IndexOf(currentItemId);
        return currentIndex >= 0 && currentIndex + 1 < ids.Count ? ids[currentIndex + 1] : null;
    }

    public async Task<TodayLessonViewModel> BuildTodayViewModelAsync(ChildProfile child, LearningSession session)
    {
        var items = await GetSessionItemsAsync(session);
        var attempts = await _db.LearningAttempts
            .Where(x => x.SessionId == session.Id)
            .ToListAsync();

        var firstOpenStepFound = false;
        var steps = items.Select(item =>
        {
            var attempt = attempts
                .Where(x => x.LearningItemId == item.Id)
                .OrderByDescending(x => x.StartedAt)
                .FirstOrDefault();

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

        return new TodayLessonViewModel
        {
            ChildProfile = child,
            Session = session,
            Steps = steps
        };
    }

    public async Task CompleteSessionAsync(LearningSession session)
    {
        var attempts = await _db.LearningAttempts
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

    private async Task<List<Guid>> BuildPlanAsync(ChildProfile child)
    {
        var preferredSkillIds = ReadPreferredSkillIds(child.PreferredSkillGroupIdsJson);
        var needsPracticeSkillIds = await _db.SkillProgress
            .Where(x => x.ChildProfileId == child.Id && x.NeedsPracticeItems > 0)
            .OrderByDescending(x => x.NeedsPracticeItems)
            .Select(x => x.SkillGroupId)
            .ToListAsync();

        var items = await _db.LearningItems
            .Include(x => x.SkillGroup)
            .Include(x => x.Topic)
            .Where(x => x.Status == ContentStatus.Published)
            .ToListAsync();

        return items
            .Where(ActivityTemplateCatalog.IsItemAllowed)
            .OrderByDescending(x => needsPracticeSkillIds.Contains(x.SkillGroupId))
            .ThenByDescending(x => preferredSkillIds.Contains(x.SkillGroupId))
            .ThenBy(x => x.SkillGroup!.SortOrder)
            .ThenBy(x => x.Topic!.SortOrder)
            .ThenBy(x => x.SortOrder)
            .ThenBy(x => x.Title)
            .Take(8)
            .Select(x => x.Id)
            .ToList();
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

    private static List<Guid> ReadPreferredSkillIds(string preferredSkillGroupIdsJson)
    {
        try
        {
            return JsonSerializer.Deserialize<List<Guid>>(preferredSkillGroupIdsJson) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
