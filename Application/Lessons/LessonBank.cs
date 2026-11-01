using System.Text.Json.Nodes;
using HanhTrangLop1.Application.MathWorld;
using HanhTrangLop1.Data;
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

    /// <summary>Phiên bản gốc của kho. Nhóm nào đổi nội dung thì đặt <see cref="LessonGroupBuilder.Version"/> cao hơn: chỉ bài của nhóm đó được nâng cấp khi khởi động.</summary>
    public const int Version = 1;

    /// <summary>Lần sửa nội dung gần nhất áp dụng cho toàn kho: tăng khi sửa dữ liệu ở nhiều nhóm một lúc.</summary>
    public const int ContentRevision = 6;

    /// <summary>Lần sửa riêng của nhóm Số và Đếm (gộp thêm bài đếm và so sánh), không làm mới bài của các nhóm khác.</summary>
    private const int NumbersRevision = 7;

    /// <summary>Thứ tự chặng của Số và Đếm: chặng đếm xen giữa các chặng số để bé đếm được ngay khi vừa biết số, không phải học hết số 20 trước.</summary>
    private static readonly string[] NumberStageOrder =
    [
        "Số 0 đến 4", "Đếm từ 1 đến 3", "Số 5 đến 9", "Đếm từ 4 đến 6", "Đếm từ 7 đến 10", "Số 10 đến 14", "Số 15 đến 19", "Số 16 đến 20", "Ôn tập vui"
    ];

    private static IReadOnlySet<string> _quantityCodes = new HashSet<string>();

    private static readonly Lazy<IReadOnlyList<BankLesson>> LazyAll = new(Build);
    public static IReadOnlyList<BankLesson> All => LazyAll.Value;

    /// <summary>Bài đếm và so sánh, trước đây là nhóm riêng: dùng cho huy hiệu và gợi ý trò chơi của mảng này.</summary>
    public static bool IsQuantityLesson(string? code)
    {
        _ = LazyAll.Value;
        return code is not null && _quantityCodes.Contains(code);
    }

    private static IReadOnlyList<BankLesson> Build()
    {
        var lessons = new List<BankLesson>();
        // Bài ôn tập luôn thêm sau bài cũ của nhóm để không đổi thứ tự đường học của bé.
        void Group(string code, Action<LessonGroupBuilder> add, Action<LessonGroupBuilder> practice, int revision = ContentRevision)
        {
            var builder = new LessonGroupBuilder(code, lessons) { Version = revision };
            add(builder);
            practice(builder);
        }

        Group("chu-cai", AddLetters, AddLetterPractice);
        Group("chu-so", AddNumbers, AddNumberPractice, NumbersRevision);
        var quantityStart = lessons.Count;
        Group("chu-so", AddQuantity, AddQuantityPractice, NumbersRevision);
        _quantityCodes = lessons.Skip(quantityStart).Select(x => x.Code).ToHashSet();
        ArrangeStages(lessons, "chu-so", NumberStageOrder);
        Group("tu-duy-logic", AddLogic, AddLogicPractice);
        Group("ky-nang-song", AddLifeSkills, AddLifePractice);
        Group("ngon-ngu", AddLanguage, AddLanguagePractice);
        Group("hinh-dang-khong-gian", AddShapes, AddShapePractice);
        Group("ghi-nho-tap-trung", AddMemory, AddMemoryPractice);
        Group("van-dong-tinh", AddMotor, AddMotorPractice);
        Group("kham-pha", AddWorld, AddWorldPractice);
        Validate(lessons);
        return lessons;
    }

    /// <summary>Xếp lại bài của nhóm theo thứ tự chặng, giữ nguyên thứ tự bài trong từng chặng.</summary>
    private static void ArrangeStages(List<BankLesson> lessons, string group, string[] stageOrder)
    {
        int Rank(BankLesson lesson)
        {
            var rank = Array.IndexOf(stageOrder, lesson.Payload["stage"]?.GetValue<string>());
            return rank >= 0 ? rank : throw new InvalidOperationException($"Bài {lesson.Code} có chặng chưa được xếp thứ tự.");
        }

        var order = 0;
        var arranged = lessons.Where(x => x.Group == group).OrderBy(Rank).Select(x => x with { SortOrder = ++order * 10 }).ToList();
        lessons.RemoveAll(x => x.Group == group);
        lessons.AddRange(arranged);
    }

    private static void Validate(IReadOnlyList<BankLesson> lessons)
    {
        var duplicates = lessons.GroupBy(x => x.Code).Where(x => x.Count() > 1).Select(x => x.Key).ToList();
        if (duplicates.Count > 0) throw new InvalidOperationException($"Kho bài học bị trùng mã: {string.Join(", ", duplicates)}");

        // Bài có dạng không được phép trong nhóm sẽ không hiện với bé: báo ngay khi khởi động thay vì để bài mất tích.
        var blocked = lessons.Where(x => !ActivityTemplateCatalog.IsAllowedForSkillGroup(x.Group, x.Type)).Select(x => $"{x.Code} ({x.Type})").ToList();
        if (blocked.Count > 0) throw new InvalidOperationException($"Dạng bài chưa được phép trong nhóm: {string.Join(", ", blocked.Take(20))}");

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
    /// <summary>Phiên bản nội dung ghi vào các bài thêm sau đó.</summary>
    public int Version { get; set; } = LessonBank.ContentRevision;

    private static readonly Say[] DefaultRetry =
    [
        new("Chưa đúng rồi. Con nhìn kỹ lại nhé!", "Not quite. Look again carefully!"),
        new("Gần đúng rồi! Con thử lại nhé.", "Almost! Try again."),
        new("Không sao đâu, con thử lại nào!", "That's okay. Try again!")
    ];

    public void Pick(string slug, string title, Say prompt, string icon, string[] choices, string answer,
        string[]? scene = null, string? focus = null, Say? retry = null, string? image = null, string type = InteractionTypes.SingleChoice, Say? spoken = null, string? target = null)
    {
        Require(choices.Contains(answer, StringComparer.Ordinal), slug, "đáp án không nằm trong lựa chọn");
        var payload = new JsonObject { ["choices"] = Strings(choices), ["targetLabel"] = "" };
        if (focus is not null) payload["focusVisual"] = focus;
        // Hình mẫu bé nhìn thấy: trẻ chưa biết đọc chỉ cần so hình dáng, không phải nhớ tên hay phân biệt bằng tai.
        if (target is not null) payload["playTarget"] = target;
        Add(slug, type, title, prompt, icon, payload, answer, retry, choices, scene, image,
            type == InteractionTypes.ListenAndChoose ? "Con nghe cô đọc rồi chạm vào đáp án đúng." : "Con nhìn hình rồi chạm vào đáp án đúng.", spoken: spoken);
    }

    public void Listen(string slug, string title, Say prompt, string icon, string[] choices, string answer, Say? spoken = null, string[]? scene = null, Say? retry = null, string? target = null) =>
        Pick(slug, title, prompt, icon, choices, answer, scene, null, retry, null, InteractionTypes.ListenAndChoose, spoken, target);

    /// <summary>
    /// Làm quen chữ qua tranh: tranh và từ có chữ cái được tô màu, bé kéo chữ giống hệt vào ô có mờ sẵn hình chữ.
    /// Thay cho câu "âm đầu là gì", vốn đòi trẻ phải tách được âm khi chưa biết đọc.
    /// </summary>
    public void Word(string slug, string title, Say prompt, string icon, string[] choices, string answer, string picture, string word, Say? retry = null)
    {
        Require(choices.Contains(answer, StringComparer.Ordinal), slug, "đáp án không nằm trong lựa chọn");
        var payload = new JsonObject
        {
            ["choices"] = Strings(choices),
            ["targetLabel"] = "",
            ["play"] = "word",
            ["playTarget"] = answer,
            ["wordPicture"] = picture,
            ["wordText"] = word
        };
        Add(slug, InteractionTypes.SingleChoice, title, prompt, icon, payload, answer, retry, choices, null, null,
            "Con kéo chữ giống hệt vào ô, hoặc chạm vào chữ đó là được.");
    }

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

    /// <summary>
    /// Đoàn tàu thiếu toa: bé nhìn cả dãy đã có sẵn và chọn đáp án cho từng toa trống, nên chính dãy là ví dụ để bé biết xếp gì tiếp theo.
    /// Đáp án là các giá trị còn thiếu theo thứ tự toa; đáp án nhiễu ở ngoài dãy để bé phải nhìn dãy chứ không chỉ chạm hết các thẻ.
    /// </summary>
    public void OrderRow(string slug, string title, Say prompt, string icon, string[] row, int[] blanks, string decoy, Say retry)
    {
        Require(blanks.Length >= 2 && blanks.Distinct().Count() == blanks.Length && blanks.All(x => x >= 0 && x < row.Length), slug, "dãy cần ít nhất 2 toa trống hợp lệ");
        Require(row.Length - blanks.Length >= 2 && !blanks.Contains(0), slug, "dãy phải giữ toa đầu và ít nhất 2 toa để bé thấy chiều xếp");
        Require(!row.Contains(decoy, StringComparer.Ordinal), slug, "đáp án nhiễu không được nằm trong dãy");
        var items = blanks.Order().Select(x => row[x]).ToArray();
        var payload = new JsonObject
        {
            ["items"] = Strings(items),
            ["orderRow"] = Strings(row.Select((value, index) => blanks.Contains(index) ? string.Empty : value)),
            ["orderDecoys"] = Strings([decoy]),
            ["play"] = "gaptrain"
        };
        Add(slug, InteractionTypes.Ordering, title, prompt, icon, payload, string.Join('|', items), retry, row.Append(decoy), null, null,
            "Con nhìn cả đoàn tàu rồi chạm đáp án cho từng toa trống. Chạm lại toa vừa điền để đổi.");
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
            "Con chạm vào từng hình để đếm, rồi chọn số đúng.", extraSpeech: CountingSpeech(count));
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
            "Con bấm thêm hoặc bớt cho đủ số lượng cô cần.", extraSpeech: CountingSpeech(Math.Min(12, count + 3)));
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
        payload["bankVersion"] = Version;

        sink.Add(new BankLesson(LessonBank.CodePrefix + slug, Group, Topic, type, title, instruction, prompt.Text, payload, answer,
            retrySay.Text, correct.Text, retrySay.Text, symbol, strokes, _order));
    }

    private static JsonArray Pair(Say say) => new(say.ViSpeech, say.En);

    /// <summary>Lời đọc các số bé đếm tới trong lúc làm bài, để Voice đọc "Ba" chứ không đọc ký tự "3".</summary>
    private static JsonObject CountingSpeech(int upTo)
    {
        var speech = new JsonObject();
        for (var n = 1; n <= upTo; n++)
        {
            speech[n.ToString()] = new JsonArray(Lexicon.NumberWordVi(n), Lexicon.NumberEn(n));
        }
        return speech;
    }

    private static JsonArray Strings(IEnumerable<string> values) => new(values.Select(x => (JsonNode)JsonValue.Create(x)!).ToArray());

    public static string[] NumberChoices(int n) =>
        (n <= 1 ? new[] { 1, 2, 3 } : [n - 1, n, n + 1]).Select(x => x.ToString()).ToArray();

    private void Require(bool condition, string slug, string message)
    {
        if (!condition) throw new InvalidOperationException($"Bài {Group}/{slug}: {message}.");
    }
}
