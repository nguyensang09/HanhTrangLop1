using HanhTrangLop1.Models;

namespace HanhTrangLop1.Models.ViewModels;

public class KidsGamesLobbyViewModel
{
    public ChildProfile? ChildProfile { get; set; }
    public int TotalStars { get; set; }
    public List<GameHubCardViewModel> Games { get; set; } = [];
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
    public int TotalLevels { get; set; }
    public int CompletedLevels { get; set; }
    public int EarnedStars { get; set; }
    public bool IsLocked { get; set; }
    public string UnlockHint { get; set; } = string.Empty;
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
}

public class SaveGameProgressRequest
{
    public string GameKey { get; set; } = string.Empty;
    public int Level { get; set; }
    public int StarsEarned { get; set; }
    public bool IsCompleted { get; set; }
}
