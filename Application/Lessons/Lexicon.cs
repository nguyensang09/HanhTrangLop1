using System.Globalization;

namespace HanhTrangLop1.Application.Lessons;

/// <summary>Một nhãn đáp án: hình minh họa, cách cô đọc bằng tiếng Việt và bản đọc tiếng Anh.</summary>
/// <param name="Visual">Emoji, đường dẫn ảnh "/images/...", "shape:circle" (hình vẽ sẵn) hoặc "shadow:🐱" (bóng đen của hình).</param>
public sealed record Term(string Label, string Visual, string En, string ViSpeech);

/// <summary>
/// Từ điển hình + lời đọc cho mọi nhãn đáp án của kho bài. Mỗi bảng là một chuỗi nhiều dòng
/// "nhãn | hình | tiếng Anh | cách đọc tiếng Việt (nếu khác nhãn)". Chữ cái và số 0–20 được sinh tự động.
/// Bảng được chia theo nhóm bài ở các tệp Lexicon.*.cs.
/// </summary>
public static partial class Lexicon
{
    private static readonly CultureInfo Vi = new("vi-VN");
    private static readonly object Gate = new();
    private static Dictionary<string, Term>? _terms;

    public static readonly string[] Alphabet = ["A", "Ă", "Â", "B", "C", "D", "Đ", "E", "Ê", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "Ô", "Ơ", "P", "Q", "R", "S", "T", "U", "Ư", "V", "W", "X", "Y", "Z"];

    /// <summary>Âm cô đọc cho từng chữ cái theo cách dạy lớp 1 (bờ, cờ, dờ...).</summary>
    private static readonly Dictionary<string, string> LetterSoundVi = new()
    {
        ["A"] = "a", ["Ă"] = "á", ["Â"] = "ớ", ["B"] = "bờ", ["C"] = "cờ", ["D"] = "dờ", ["Đ"] = "đờ", ["E"] = "e", ["Ê"] = "ê",
        ["F"] = "ép", ["G"] = "gờ", ["H"] = "hờ", ["I"] = "i", ["J"] = "gi", ["K"] = "ca", ["L"] = "lờ", ["M"] = "mờ", ["N"] = "nờ",
        ["O"] = "o", ["Ô"] = "ô", ["Ơ"] = "ơ", ["P"] = "pờ", ["Q"] = "quờ", ["R"] = "rờ", ["S"] = "sờ", ["T"] = "tờ", ["U"] = "u",
        ["Ư"] = "ư", ["V"] = "vờ", ["W"] = "vê kép", ["X"] = "xờ", ["Y"] = "i dài", ["Z"] = "dét"
    };

    /// <summary>Tên chữ bằng tiếng Anh; chữ riêng của tiếng Việt được gọi theo dấu cho bé dễ hình dung.</summary>
    private static readonly Dictionary<string, string> LetterNameEn = new()
    {
        ["Ă"] = "A with a smile", ["Â"] = "A with a hat", ["Đ"] = "D with a line", ["Ê"] = "E with a hat",
        ["Ô"] = "O with a hat", ["Ơ"] = "O with a hook", ["Ư"] = "U with a hook"
    };

    private static readonly string[] NumberVi = ["Không", "Một", "Hai", "Ba", "Bốn", "Năm", "Sáu", "Bảy", "Tám", "Chín", "Mười",
        "Mười một", "Mười hai", "Mười ba", "Mười bốn", "Mười lăm", "Mười sáu", "Mười bảy", "Mười tám", "Mười chín", "Hai mươi"];
    private static readonly string[] NumberEnWords = ["Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten",
        "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen", "Twenty"];

    public static string NumberWordVi(int n) => NumberVi[n];
    public static string NumberEn(int n) => NumberEnWords[n];
    public static string LetterSound(string letter) => LetterSoundVi[letter.ToUpper(Vi)];
    public static string LetterEn(string letter)
    {
        var upper = letter.ToUpper(Vi);
        var name = LetterNameEn.TryGetValue(upper, out var special) ? special : upper;
        return letter == upper ? $"capital {name}" : $"small {name}";
    }
    public static string Lower(string letter) => letter.ToLower(Vi);

    public static Term Require(string label, string where) =>
        Find(label) ?? throw new InvalidOperationException($"Nhãn \"{label}\" ({where}) chưa có trong từ điển hình và lời đọc (Lexicon).");

    public static Term? Find(string label)
    {
        var terms = Terms();
        lock (Gate)
        {
            if (terms.TryGetValue(label, out var term)) return term;
        }

        var upper = label.ToUpper(Vi);
        if (label.Length == 1 && LetterSoundVi.ContainsKey(upper))
        {
            return new Term(label, "", LetterEn(label), $"chữ {LetterSoundVi[upper]} {(label == upper ? "in hoa" : "in thường")}");
        }
        if (int.TryParse(label, NumberStyles.None, CultureInfo.InvariantCulture, out var n) && n is >= 0 and <= 20 && label == n.ToString(CultureInfo.InvariantCulture))
        {
            return new Term(label, "", NumberEnWords[n], NumberVi[n]);
        }
        return null;
    }

    /// <summary>Thêm nhãn sinh trong lúc dựng bài (ví dụ "Mục tiêu: 5"). Định nghĩa lại phải giống hệt.</summary>
    public static void Define(string label, string visual, string en, string? viSpeech = null)
    {
        var terms = Terms();
        lock (Gate) AddTerm(terms, new Term(label, visual, en, viSpeech ?? label), "Define");
    }

    private static Dictionary<string, Term> Terms()
    {
        if (_terms is not null) return _terms;
        lock (Gate)
        {
            if (_terms is not null) return _terms;
            var terms = new Dictionary<string, Term>(StringComparer.Ordinal);
            foreach (var (name, table) in Tables()) Load(terms, table, name);
            _terms = terms;
            return terms;
        }
    }

    private static IEnumerable<(string Name, string Table)> Tables()
    {
        yield return (nameof(CoreTable), CoreTable);
        yield return (nameof(LetterWordsTable), LetterWordsTable);
        yield return (nameof(LogicTable), LogicTable);
        yield return (nameof(LifeTable), LifeTable);
        yield return (nameof(LanguageTable), LanguageTable);
        yield return (nameof(ShapeTable), ShapeTable);
        yield return (nameof(MemoryTable), MemoryTable);
        yield return (nameof(MotorTable), MotorTable);
        yield return (nameof(WorldTable), WorldTable);
    }

    private static void Load(Dictionary<string, Term> terms, string table, string source)
    {
        foreach (var raw in table.Split('\n'))
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith("//", StringComparison.Ordinal)) continue;
            var parts = line.Split('|').Select(x => x.Trim()).ToArray();
            if (parts.Length < 3) throw new InvalidOperationException($"Dòng từ điển sai định dạng ({source}): {line}");
            var vi = parts.Length > 3 && parts[3].Length > 0 ? parts[3] : parts[0];
            AddTerm(terms, new Term(parts[0], parts[1], parts[2], vi), source);
        }
    }

    private static void AddTerm(Dictionary<string, Term> terms, Term term, string source)
    {
        if (terms.TryGetValue(term.Label, out var existing))
        {
            if (existing != term) throw new InvalidOperationException($"Nhãn \"{term.Label}\" được định nghĩa hai lần khác nhau ({source}): {existing} / {term}");
            return;
        }
        terms[term.Label] = term;
    }
}
