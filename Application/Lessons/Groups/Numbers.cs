namespace HanhTrangLop1.Application.Lessons;

public static partial class LessonBank
{
    /// <summary>Số dễ nhầm với từng số (hình dáng gần giống hoặc cùng chữ số).</summary>
    private static readonly int[][] NumberLookAlikes =
    [
        [8, 6], [7, 4], [5, 7], [8, 5], [1, 9], [2, 3], [9, 0], [1, 2], [3, 0], [6, 4], [0, 11],
        [1, 17], [15, 2], [18, 3], [4, 11], [12, 5], [19, 6], [11, 7], [13, 8], [16, 9], [2, 12]
    ];

    /// <summary>Đồ vật dùng để minh họa số lượng: hình, tên tiếng Việt (kèm loại từ), tên tiếng Anh số ít / số nhiều.</summary>
    private static readonly (string Emoji, string Vi, string En, string EnPlural)[] CountThemes =
    [
        ("🍎", "quả táo", "apple", "apples"), ("⭐", "ngôi sao", "star", "stars"), ("🐟", "con cá", "fish", "fish"),
        ("🌸", "bông hoa", "flower", "flowers"), ("🐞", "con bọ rùa", "ladybug", "ladybugs"), ("🚗", "chiếc ô tô", "car", "cars"),
        ("🍓", "quả dâu", "strawberry", "strawberries"), ("🐤", "chú gà con", "chick", "chicks"), ("🎈", "quả bóng bay", "balloon", "balloons"),
        ("🦋", "con bướm", "butterfly", "butterflies"), ("🍄", "cây nấm", "mushroom", "mushrooms")
    ];

    private static string NumVi(int n) => Lexicon.NumberWordVi(n).ToLower(System.Globalization.CultureInfo.GetCultureInfo("vi-VN"));
    private static string NumEn(int n) => Lexicon.NumberEn(n).ToLowerInvariant();

    /// <summary>Nhãn "🍎 🍎 🍎" (nhóm đồ vật) kèm lời đọc "ba quả táo" / "three apples".</summary>
    private static string QuantityLabel((string Emoji, string Vi, string En, string EnPlural) theme, int n)
    {
        var label = string.Join(' ', Enumerable.Repeat(theme.Emoji, n));
        Lexicon.Define(label, "", $"{NumEn(n)} {(n == 1 ? theme.En : theme.EnPlural)}", $"{NumVi(n)} {theme.Vi}");
        return label;
    }

    private static void AddNumbers(LessonGroupBuilder g)
    {
        for (var n = 0; n <= 20; n++)
        {
            if (n % 5 == 0 && n < 20) g.Stage = n == 0 ? "Số 0 đến 4" : $"Số {n} đến {Math.Min(20, n + 4)}";
            if (n == 20) g.Stage = "Số 16 đến 20";
            g.Topic = n <= 9 ? "so-0-9" : "so-10-20";
            var key = n.ToString("00");
            var look = NumberLookAlikes[n];
            var theme = CountThemes[n % CountThemes.Length];
            string[]? scene = n is >= 1 and <= 10 ? Enumerable.Repeat(theme.Emoji, n).ToArray() : null;

            g.Pick($"number-single-{key}", $"Số {n}",
                new Say($"Con tìm giúp cô số {n} nhé!", $"Can you find the number {NumEn(n)} for me?", $"Con tìm giúp cô số {NumVi(n)} nhé!"),
                n.ToString(), [look[0].ToString(), n.ToString(), look[1].ToString()], n.ToString(), scene: scene,
                retry: new Say($"Chưa đúng rồi. Số {n} trông thế này nè, con tìm lại nhé!", $"Not quite. Look at the number {NumEn(n)} again!", $"Chưa đúng rồi. Số {NumVi(n)} trông thế này nè, con tìm lại nhé!"));

            g.Listen($"number-listen-{key}", $"Nghe và tìm số {n}",
                new Say("Cô đọc số mấy? Con chạm vào số đó nhé!", "Which number do you hear? Tap it!"),
                "🎧", [look[1].ToString(), look[0].ToString(), n.ToString()], n.ToString(),
                spoken: new Say($"Số {NumVi(n)}. Con chạm vào số {NumVi(n)} nhé!", $"Listen: {NumEn(n)}. Tap the number {NumEn(n)}!"));

            if (n is >= 1 and <= 19)
            {
                var after = n % 2 == 1;
                var answer = after ? n + 1 : n - 1;
                var wrong = after ? n - 1 : n + 1;
                var wrong2 = after ? Math.Min(20, n + 2) : Math.Max(0, n - 2);
                g.Topic = "thu-tu-so";
                g.Pick($"number-neighbor-{key}", after ? $"Số đứng sau {n}" : $"Số đứng trước {n}",
                    after
                        ? new Say($"Đếm tiếp nào: sau số {n} là số mấy?", $"Let's count on: what comes after {NumEn(n)}?", $"Đếm tiếp nào: sau số {NumVi(n)} là số mấy?")
                        : new Say($"Đếm lùi nào: số nào đứng ngay trước số {n}?", $"Let's count back: what comes just before {NumEn(n)}?", $"Đếm lùi nào: số nào đứng ngay trước số {NumVi(n)}?"),
                    "🔢", new[] { answer, wrong, wrong2 }.Distinct().OrderBy(x => x).Select(x => x.ToString()).ToArray(), answer.ToString(),
                    scene: after ? [n.ToString(), "?"] : ["?", n.ToString()],
                    retry: new Say("Con đếm từ đầu theo cô nhé: một, hai, ba...", "Count along with me: one, two, three..."));
            }

            g.Topic = "viet-so";
            g.Trace($"number-trace-{key}", $"Tập tô số {n}",
                new Say($"Con tô số {n} theo nét chấm nhé!", $"Trace the number {NumEn(n)} along the dotted lines!", $"Con tô số {NumVi(n)} theo nét chấm nhé!"),
                "✏️", n.ToString(), n > 9 ? 2 : 1);
        }

        g.Stage = "Ghép số với số lượng";
        g.Topic = "so-0-9";
        for (var n = 1; n <= 9; n++)
        {
            var start = Math.Min(n, 7);
            var pairs = Enumerable.Range(start, 3)
                .Select(x => (x.ToString(), QuantityLabel(CountThemes[(x + n) % CountThemes.Length], x)))
                .ToArray();
            g.Match($"number-quantity-{n}", $"Ghép số với nhóm đồ vật ({start}–{start + 2})",
                new Say("Con đếm từng nhóm đồ vật rồi nối với số đúng nhé!", "Count each group, then match it to the right number!"),
                "🧮", pairs);
        }

        g.Stage = "Xếp thứ tự số";
        g.Topic = "thu-tu-so";
        for (var n = 1; n <= 7; n++)
        {
            var seq = Enumerable.Range(n, 3).Select(x => x.ToString()).ToArray();
            g.Order($"number-up-{n}", $"Xếp từ bé đến lớn: {n} đến {n + 2}",
                new Say("Con xếp các số từ bé đến lớn nhé!", "Put the numbers in order from smallest to biggest!"),
                "📶", seq);
            g.Order($"number-down-{n}", $"Xếp từ lớn đến bé: {n + 2} về {n}",
                new Say("Bây giờ con xếp các số từ lớn đến bé nhé!", "Now put the numbers in order from biggest to smallest!"),
                "📉", seq.Reverse().ToArray());
        }
    }
}
