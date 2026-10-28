using HanhTrangLop1.Models;

namespace HanhTrangLop1.Models.ViewModels;

public class KidsGamesLobbyViewModel
{
    public ChildProfile? ChildProfile { get; set; }
    public int TotalStars { get; set; }
    public List<GameHubCardViewModel> Games { get; set; } = [];
    /// <summary>Gợi ý "học mà chơi": trò chơi ôn lại đúng thứ bé vừa học (null khi bé chưa học bài nào).</summary>
    public GameSuggestionViewModel? Suggestion { get; set; }
}

public class GameHubCardViewModel
{
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // "letter" | "number"
    public string Emoji { get; set; } = string.Empty;
    public string ThemeClass { get; set; } = "card-theme-coral";
    public string Badge { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public string ButtonClass { get; set; } = "btn-3d-green";
    public int TotalLevels { get; set; }
    public int CompletedLevels { get; set; }
    public int EarnedStars { get; set; }
    public int NextLevel { get; set; } = 1;
    public bool IsLocked { get; set; }
    public string UnlockHint { get; set; } = string.Empty;
    /// <summary>Nhóm bài học mà trò chơi giúp ôn lại (để bé đi qua lại giữa học và chơi).</summary>
    public string LinkedGroupName { get; set; } = string.Empty;
    public Guid? LinkedGroupId { get; set; }
}

public class GameSuggestionViewModel
{
    public string GameKey { get; set; } = string.Empty;
    public string GameTitle { get; set; } = string.Empty;
    public string Emoji { get; set; } = string.Empty;
    public int Level { get; set; } = 1;
    public string Message { get; set; } = string.Empty;
}

public class KidsGamePlayViewModel
{
    public ChildProfile? ChildProfile { get; set; }
    public string GameKey { get; set; } = string.Empty;
    public string GameTitle { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int InitialLevel { get; set; } = 1;
    public int TotalStars { get; set; }
    public bool SoundEnabled { get; set; } = true;
    public bool EnglishVoice { get; set; }
    /// <summary>Số sao tốt nhất của từng màn bé đã qua (màn → sao), lưu theo bé trên máy chủ.</summary>
    public Dictionary<int, int> LevelStars { get; set; } = [];
}

public class SaveGameProgressRequest
{
    public string GameKey { get; set; } = string.Empty;
    public int Level { get; set; }
    public int StarsEarned { get; set; }
    public bool IsCompleted { get; set; }
}
