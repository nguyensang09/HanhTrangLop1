using HanhTrangLop1.Models;

namespace HanhTrangLop1.Infrastructure;

public static class LearningAnswerEvaluator
{
    public static bool IsCorrect(string interactionType, string? submittedAnswer, string? correctAnswer)
    {
        var submitted = submittedAnswer?.Trim() ?? string.Empty;
        var expected = correctAnswer?.Trim() ?? string.Empty;

        return interactionType is InteractionTypes.MultiSelect
            or InteractionTypes.Matching
            or InteractionTypes.Classification
            ? AsSet(submitted).SetEquals(AsSet(expected))
            : string.Equals(submitted, expected, StringComparison.OrdinalIgnoreCase);
    }

    private static HashSet<string> AsSet(string value) => value
        .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .ToHashSet(StringComparer.OrdinalIgnoreCase);
}
