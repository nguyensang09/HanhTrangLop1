using System.Text.Json.Nodes;
using HanhTrangLop1.Application.MathWorld;
using HanhTrangLop1.Models;

namespace HanhTrangLop1.Application.Lessons;

/// <summary>
/// Một câu song ngữ: chữ hiển thị trên màn hình (tiếng Việt), bản đọc tiếng Anh và bản đọc tiếng Việt
/// khi cách đọc khác chữ hiển thị (ví dụ hiển thị "chữ B" nhưng cô đọc "chữ bờ").
/// Lời luôn theo ngữ cảnh cô và con: cô nói với con ("Con tìm giúp cô...", "Tell me...").
/// </summary>
public readonly record struct Say(string Text, string En, string? Vi = null)
{
    public string ViSpeech => string.IsNullOrWhiteSpace(Vi) ? Text : Vi!;
}

public sealed record BankLesson(
    string Code,
    string Group,
    string? Topic,
    string Type,
    string Title,
    string Instruction,
    string Prompt,
    JsonObject Payload,
    string Answer,
    string Hint,
    string Correct,
    string Retry,
    string Symbol,
    int Strokes,
    int SortOrder);

/// <summary>
/// Kho bài học (trừ Đảo Toán Học có kho riêng). Mỗi bài được viết sẵn lời đọc hai ngôn ngữ, hình minh họa
/// cho từng đáp án (payload "visuals"), cảnh minh họa (payload "scene"), biểu tượng bản đồ ("mapIcon") và
/// tên chặng ("stage"). Mã bài giữ nguyên mã cũ khi bài cũ được làm mới để tiến độ của bé không mất.
/// </summary>
public static partial class LessonBank
{
    public const string CodePrefix = "curriculum-v4-";

    /// <summary>Tăng khi nội dung kho thay đổi; bài có phiên bản cũ hơn sẽ được nâng cấp khi khởi động.</summary>
    public const int Version = 1;

    private static readonly Lazy<IReadOnlyList<BankLesson>> LazyAll = new(Build);
    public static IReadOnlyList<BankLesson> All => LazyAll.Value;

    private static IReadOnlyList<BankLesson> Build()
    {
        var lessons = new List<BankLesson>();
        AddLetters(new LessonGroupBuilder("chu-cai", lessons));
        AddNumbers(new LessonGroupBuilder("chu-so", lessons));
        AddQuantity(new LessonGroupBuilder("so-luong-toan", lessons));
        AddLogic(new LessonGroupBuilder("tu-duy-logic", lessons));
        AddLifeSkills(new LessonGroupBuilder("ky-nang-song", lessons));
        AddLanguage(new LessonGroupBuilder("ngon-ngu", lessons));
        AddShapes(new LessonGroupBuilder("hinh-dang-khong-gian", lessons));
        AddMemory(new LessonGroupBuilder("ghi-nho-tap-trung", lessons));
        AddMotor(new LessonGroupBuilder("van-dong-tinh", lessons));
        AddWorld(new LessonGroupBuilder("kham-pha", lessons));
        Validate(lessons);
        return lessons;
    }

    private static void Validate(IReadOnlyList<BankLesson> lessons)
    {
        var duplicates = lessons.GroupBy(x => x.Code).Where(x => x.Count() > 1).Select(x => x.Key).ToList();
        if (duplicates.Count > 0) throw new InvalidOperationException($"Kho bài học bị trùng mã: {string.Join(", ", duplicates)}");

        // Cùng một câu tiếng Việt phải luôn có cùng một bản đọc tiếng Anh, vì kho Voice dùng chung theo câu tiếng Việt.
        var speech = new Dictionary<string, (string En, string Code)>(StringComparer.OrdinalIgnoreCase);
        var conflicts = new List<string>();
        foreach (var lesson in lessons)
        {
            if (lesson.Payload["speech"] is not JsonObject map) continue;
            foreach (var (_, value) in map)
            {
                if (value is not JsonArray pair || pair.Count < 2) continue;
                var vi = pair[0]!.GetValue<string>().Trim();
                var en = pair[1]!.GetValue<string>().Trim();
                if (speech.TryGetValue(vi, out var known))
                {
                    if (!string.Equals(known.En, en, StringComparison.Ordinal)) conflicts.Add($"\"{vi}\": \"{known.En}\" ({known.Code}) ≠ \"{en}\" ({lesson.Code})");
                }
                else speech[vi] = (en, lesson.Code);
            }
        }
        foreach (var line in MathVoiceCatalog.AllDistinct())
        {
            if (speech.TryGetValue(line.Vi, out var known) && !string.Equals(known.En, line.En, StringComparison.Ordinal))
                conflicts.Add($"\"{line.Vi}\" khác bản tiếng Anh của Đảo Toán Học: \"{known.En}\" ≠ \"{line.En}\"");
        }
        if (conflicts.Count > 0) throw new InvalidOperationException("Một câu tiếng Việt có hai bản tiếng Anh khác nhau:\n" + string.Join("\n", conflicts.Take(20)));
    }

    public static string Canonical(IEnumerable<string> values) =>
        string.Join('|', values.Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal));

    public static string CanonicalMappings(IEnumerable<(string Left, string Right)> pairs) =>
        string.Join('|', pairs.OrderBy(x => x.Left, StringComparer.Ordinal).Select(x => $"{x.Left}=>{x.Right}"));
}

/// <summary>
/// Dựng bài cho một nhóm. Mọi nhãn đáp án phải có trong <see cref="Lexicon"/> (hình + tiếng Anh),
/// nhờ vậy không có đáp án nào thiếu hình hay thiếu lời đọc tiếng Anh.
/// </summary>
internal sealed class LessonGroupBuilder(string group, List<BankLesson> sink)
{
    private int _order;

    public string Group { get; } = group;
    /// <summary>Mã chủ đề (Topic.Code) gán cho các bài thêm sau đó.</summary>
    public string? Topic { get; set; }
    /// <summary>Tên chặng hiển thị trên bản đồ cho các bài thêm sau đó.</summary>
    public string Stage { get; set; } = string.Empty;

    private static readonly Say[] DefaultRetry =
    [
        new("Chưa đúng rồi. Con nhìn kỹ lại nhé!", "Not quite. Look again carefully!"),
        new("Gần đúng rồi! Con thử lại nhé.", "Almost! Try again."),
        new("Không sao đâu, con thử lại nào!", "That's okay. Try again!")
    ];

    public void Pick(string slug, string title, Say prompt, string icon, string[] choices, string answer,
        string[]? scene = null, string? focus = null, Say? retry = null, string? image = null, string type = InteractionTypes.SingleChoice, Say? spoken = null)
    {
        Require(choices.Contains(answer, StringComparer.Ordinal), slug, "đáp án không nằm trong lựa chọn");
        var payload = new JsonObject { ["choices"] = Strings(choices), ["targetLabel"] = "" };
        if (focus is not null) payload["focusVisual"] = focus;
        Add(slug, type, title, prompt, icon, payload, answer, retry, choices, scene, image,
            type == InteractionTypes.ListenAndChoose ? "Con nghe cô đọc rồi chạm vào đáp án đúng." : "Con nhìn hình rồi chạm vào đáp án đúng.", spoken: spoken);
    }

    public void Listen(string slug, string title, Say prompt, string icon, string[] choices, string answer, Say? spoken = null, string[]? scene = null, Say? retry = null) =>
        Pick(slug, title, prompt, icon, choices, answer, scene, null, retry, null, InteractionTypes.ListenAndChoose, spoken);

    public void Multi(string slug, string title, Say prompt, string icon, string[] choices, string[] answers, string[]? scene = null, Say? retry = null)
    {
        Require(answers.Length >= 2 && answers.All(a => choices.Contains(a, StringComparer.Ordinal)), slug, "chọn nhiều cần ít nhất 2 đáp án nằm trong lựa chọn");
        var payload = new JsonObject { ["choices"] = Strings(choices), ["targetLabel"] = "" };
        Add(slug, InteractionTypes.MultiSelect, title, prompt, icon, payload, LessonBank.Canonical(answers), retry, choices, scene, null,
            "Con chọn đủ tất cả đáp án đúng nhé.");
    }

    public void Drag(string slug, string title, Say prompt, string icon, string[] choices, string answer, string target, string[]? scene = null, Say? retry = null)
    {
        Require(choices.Contains(answer, StringComparer.Ordinal), slug, "đáp án không nằm trong lựa chọn");
        var payload = new JsonObject { ["choices"] = Strings(choices), ["targetLabel"] = target };
        Add(slug, InteractionTypes.DragDrop, title, prompt, icon, payload, answer, retry, choices.Append(target), scene, null,
            "Con kéo hình vào ô, hoặc chạm vào hình là được.");
    }

    public void Match(string slug, string title, Say prompt, string icon, (string Left, string Right)[] pairs, Say? retry = null)
    {
        var payload = new JsonObject
        {
            ["pairs"] = new JsonArray(pairs.Select(p => (JsonNode)new JsonObject { ["left"] = p.Left, ["right"] = p.Right }).ToArray())
        };
        Add(slug, InteractionTypes.Matching, title, prompt, icon, payload, LessonBank.CanonicalMappings(pairs), retry,
            pairs.SelectMany(p => new[] { p.Left, p.Right }), null, null, "Con chạm một hình bên trái rồi chạm hình hợp với nó bên phải.");
    }

    public void Order(string slug, string title, Say prompt, string icon, string[] items, string[]? scene = null, Say? retry = null)
    {
        var payload = new JsonObject { ["items"] = Strings(items) };
        Add(slug, InteractionTypes.Ordering, title, prompt, icon, payload, string.Join('|', items), retry, items, scene, null,
            "Con chạm lần lượt từng thẻ theo đúng thứ tự.");
    }

    public void Sort(string slug, string title, Say prompt, string icon, string[] categories, (string Item, string Category)[] mappings, Say? retry = null)
    {
        Require(mappings.All(m => categories.Contains(m.Category, StringComparer.Ordinal)), slug, "nhóm phân loại không khớp");
        var payload = new JsonObject
        {
            ["categories"] = Strings(categories),
            ["mappings"] = new JsonArray(mappings.Select(m => (JsonNode)new JsonObject { ["left"] = m.Item, ["right"] = m.Category }).ToArray())
        };
        Add(slug, InteractionTypes.Classification, title, prompt, icon, payload, LessonBank.CanonicalMappings(mappings), retry,
            categories.Concat(mappings.Select(m => m.Item)), null, null, "Con đưa từng hình vào đúng nhóm của nó.");
    }

    public void Count(string slug, string title, Say prompt, string icon, string emoji, int count, Say? retry = null)
    {
        var choices = NumberChoices(count);
        var payload = new JsonObject
        {
            ["choices"] = Strings(choices),
            ["objectSymbol"] = emoji,
            ["targetCount"] = count,
            ["visualMode"] = "symbols-only"
        };
        Add(slug, InteractionTypes.Counting, title, prompt, icon, payload, count.ToString(), retry, choices, null, null,
            "Con chạm vào từng hình để đếm, rồi chọn số đúng.");
    }

    public void Build(string slug, string title, Say prompt, string icon, string emoji, int count, Say? retry = null)
    {
        var target = $"Mục tiêu: {count}";
        Lexicon.Define(target, "", $"Goal: {Lexicon.NumberEn(count).ToLowerInvariant()}");
        var payload = new JsonObject
        {
            ["objectSymbol"] = emoji,
            ["targetCount"] = count,
            ["maxItems"] = Math.Min(12, count + 3),
            ["targetLabel"] = target
        };
        Add(slug, InteractionTypes.QuantityBuilder, title, prompt, icon, payload, count.ToString(), retry, [target], null, null,
            "Con bấm thêm hoặc bớt cho đủ số lượng cô cần.");
    }

    public void Compare(string slug, string title, Say prompt, string icon, string emoji, int left, int right, string mode, Say? retry = null)
    {
        var payload = new JsonObject
        {
            ["visualMode"] = "quantity",
            ["objectSymbol"] = emoji,
            ["leftLabel"] = "Nhóm bên trái",
            ["leftCount"] = left,
            ["rightLabel"] = "Nhóm bên phải",
            ["rightCount"] = right,
            ["comparisonMode"] = mode
        };
        var answer = mode == "equal" ? "equal" : mode == "less" ? (left < right ? "left" : "right") : (left > right ? "left" : "right");
        Add(slug, InteractionTypes.Comparison, title, prompt, icon, payload, answer, retry, ["Nhóm bên trái", "Nhóm bên phải", "Bằng nhau"], null, null,
            "Con quan sát hai nhóm rồi chạm vào nhóm đúng.");
    }

    public void CompareShapes(string slug, string title, Say prompt, string icon, string symbol, string shapeName, int left, int right, string mode, Say? retry = null)
    {
        var payload = new JsonObject
        {
            ["visualMode"] = "shape",
            ["objectSymbol"] = symbol,
            ["shapeName"] = shapeName,
            ["leftLabel"] = "Bên trái",
            ["leftCount"] = left,
            ["rightLabel"] = "Bên phải",
            ["rightCount"] = right,
            ["comparisonMode"] = mode
        };
        var answer = mode == "less" ? (left < right ? "left" : "right") : (left > right ? "left" : "right");
        Add(slug, InteractionTypes.Comparison, title, prompt, icon, payload, answer, retry, ["Bên trái", "Bên phải"], null, null,
            "Con nhìn kích thước rồi chạm vào hình đúng.");
    }

    public void Story(string slug, string title, Say story, Say question, string icon, string[] choices, string answer, string image, Say? retry = null)
    {
        Require(choices.Contains(answer, StringComparer.Ordinal), slug, "đáp án không nằm trong lựa chọn");
        var payload = new JsonObject
        {
            ["choices"] = Strings(choices),
            ["speechText"] = story.Text,
            ["imageAltText"] = title
        };
        var speech = new JsonObject { [story.Text] = Pair(story) };
        Add(slug, InteractionTypes.StoryChoice, title, question, icon, payload, answer, retry, choices, null, image,
            "Con nghe cô kể chuyện, xem tranh rồi trả lời câu hỏi.", extraSpeech: speech);
    }

    /// <summary>Bài tô nét: chỉ làm mới tên bài và lời hướng dẫn, giữ nguyên mẫu nét đã có.</summary>
    public void Trace(string slug, string title, Say prompt, string icon, string symbol, int strokes = 1)
    {
        var payload = new JsonObject { ["symbol"] = symbol, ["expectedStrokeCount"] = strokes };
        Add(slug, InteractionTypes.Tracing, title, prompt, icon, payload, symbol, null, [], null, null,
            "Con tô theo nét chấm, bắt đầu từ chấm xanh nhé.", symbol: symbol, strokes: strokes);
    }

    private void Add(string slug, string type, string title, Say prompt, string icon, JsonObject payload, string answer, Say? retry,
        IEnumerable<string> labels, string[]? scene, string? image, string instruction, string symbol = "", int strokes = 1, JsonObject? extraSpeech = null, Say? spoken = null)
    {
        _order += 10;
        var speech = new JsonObject { [prompt.Text] = Pair(spoken ?? prompt) };
        var visuals = new JsonObject();
        foreach (var label in labels.Distinct(StringComparer.Ordinal))
        {
            var term = Lexicon.Require(label, $"{Group}/{slug}");
            speech[label] = new JsonArray(term.ViSpeech, term.En);
            if (!string.IsNullOrEmpty(term.Visual)) visuals[label] = term.Visual;
        }
        if (extraSpeech is not null)
        {
            foreach (var (key, value) in extraSpeech) speech[key] = value?.DeepClone();
        }

        var praise = MathVoiceCatalog.Praise[(_order / 10) % MathVoiceCatalog.Praise.Count];
        var correct = new Say(praise.Vi, praise.En);
        var retrySay = retry ?? DefaultRetry[(_order / 10) % DefaultRetry.Length];
        speech[correct.Text] = Pair(correct);
        speech[retrySay.Text] = Pair(retrySay);

        payload["imageUrl"] = image ?? string.Empty;
        if (!payload.ContainsKey("imageAltText")) payload["imageAltText"] = title;
        if (scene is { Length: > 0 }) payload["scene"] = Strings(scene);
        payload["visuals"] = visuals;
        payload["speech"] = speech;
        payload["mapIcon"] = icon;
        payload["stage"] = Stage;
        payload["bankVersion"] = LessonBank.Version;

        sink.Add(new BankLesson(LessonBank.CodePrefix + slug, Group, Topic, type, title, instruction, prompt.Text, payload, answer,
            retrySay.Text, correct.Text, retrySay.Text, symbol, strokes, _order));
    }

    private static JsonArray Pair(Say say) => new(say.ViSpeech, say.En);
    private static JsonArray Strings(IEnumerable<string> values) => new(values.Select(x => (JsonNode)JsonValue.Create(x)!).ToArray());

    public static string[] NumberChoices(int n) =>
        new[] { Math.Max(0, n - 1), n, n + 1 }.Distinct().Select(x => x.ToString()).ToArray();

    private void Require(bool condition, string slug, string message)
    {
        if (!condition) throw new InvalidOperationException($"Bài {Group}/{slug}: {message}.");
    }
}
