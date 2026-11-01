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

        void AddFind(int n, string[]? scene)
        {
            g.Topic = n <= 9 ? "so-0-9" : "so-10-20";
            var look = NumberLookAlikes[n];
            g.Pick($"number-single-{n:00}", $"Số {n}",
                new Say($"Con tìm giúp cô số {n} nhé!", $"Can you find the number {NumEn(n)} for me?", $"Con tìm giúp cô số {NumVi(n)} nhé!"),
                n.ToString(), [look[0].ToString(), n.ToString(), look[1].ToString()], n.ToString(), scene: scene, target: n.ToString(),
                retry: new Say($"Chưa đúng rồi. Số {n} trông thế này nè, con tìm lại nhé!", $"Not quite. Look at the number {NumEn(n)} again!", $"Chưa đúng rồi. Số {NumVi(n)} trông thế này nè, con tìm lại nhé!"));
        }

        void AddListen(int n)
        {
            g.Topic = n <= 9 ? "so-0-9" : "so-10-20";
            var look = NumberLookAlikes[n];
            g.Listen($"number-listen-{n:00}", $"Nghe và tìm số {n}",
                new Say("Cô đọc số mấy? Con chạm vào số đó nhé!", "Which number do you hear? Tap it!"),
                "🎧", [look[1].ToString(), look[0].ToString(), n.ToString()], n.ToString(), target: n.ToString(),
                spoken: new Say($"Số {NumVi(n)}. Con chạm vào số {NumVi(n)} nhé!", $"Listen: {NumEn(n)}. Tap the number {NumEn(n)}!"));
        }

        void AddNeighbor(int n)
        {
            // Số cần tìm luôn nằm giữa số đứng trước và số đứng sau nó, nên số 19 hỏi "đứng trước" để dãy không vượt quá 20.
            var after = n % 2 == 1 && n < 19;
            var answer = after ? n + 1 : n - 1;
            var left = answer - 1;
            var right = answer + 1;
            var (wrong, wrong2) = answer switch
            {
                <= 2 => (answer + 2, answer + 3),
                >= 19 => (answer - 2, answer - 3),
                _ => (answer - 2, answer + 2)
            };
            g.Topic = "thu-tu-so";
            g.Pick($"number-neighbor-{n:00}", after ? $"Số đứng sau {n}" : $"Số đứng trước {n}",
                after
                    ? new Say($"Đếm tiếp nào: sau số {left}, trước số {right} là số mấy?", $"Let's count on: what comes after {NumEn(left)} and before {NumEn(right)}?", $"Đếm tiếp nào: sau số {NumVi(left)}, trước số {NumVi(right)} là số mấy?")
                    : new Say($"Đếm lùi nào: trước số {right}, sau số {left} là số mấy?", $"Let's count back: what comes before {NumEn(right)} and after {NumEn(left)}?", $"Đếm lùi nào: trước số {NumVi(right)}, sau số {NumVi(left)} là số mấy?"),
                "🔢", new[] { answer, wrong, wrong2 }.OrderBy(x => x).Select(x => x.ToString()).ToArray(), answer.ToString(),
                scene: [left.ToString(), "?", right.ToString()],
                retry: new Say("Con đếm từ đầu theo cô nhé: một, hai, ba...", "Count along with me: one, two, three..."));
        }

        void AddQuantityMatch(int n)
        {
            g.Topic = "so-0-9";
            var start = Math.Min(n, 7);
            var pairs = Enumerable.Range(start, 3)
                .Select(x => (x.ToString(), QuantityLabel(CountThemes[(x + n) % CountThemes.Length], x)))
                .ToArray();
            g.Match($"number-quantity-{n}", $"Ghép số với nhóm đồ vật ({start}–{start + 2})",
                new Say("Con đếm từng nhóm đồ vật rồi nối với số đúng nhé!", "Count each group, then match it to the right number!"),
                "🧮", pairs);
        }

        // Dãy mẫu cùng chiều xếp nhưng khác số với bài, để bé thấy cách xếp mà không lộ đáp án.
        string[] ExampleRow(int n, bool down)
        {
            var start = n >= 4 ? 1 : n + 3;
            var row = Enumerable.Range(start, 3).Select(x => x.ToString());
            return (down ? row.Reverse() : row).ToArray();
        }

        void AddOrderUp(int n)
        {
            g.Topic = "thu-tu-so";
            var seq = Enumerable.Range(n, 3).Select(x => x.ToString()).ToArray();
            g.Order($"number-up-{n}", $"Xếp từ bé đến lớn: {n} đến {n + 2}",
                new Say("Con xếp các số từ bé đến lớn nhé!", "Put the numbers in order from smallest to biggest!"),
                "📶", seq, example: ExampleRow(n, false));
        }

        void AddOrderDown(int n)
        {
            g.Topic = "thu-tu-so";
            var seq = Enumerable.Range(n, 3).Select(x => x.ToString()).Reverse().ToArray();
            g.Order($"number-down-{n}", $"Xếp từ lớn đến bé: {n + 2} về {n}",
                new Say("Bây giờ con xếp các số từ lớn đến bé nhé!", "Now put the numbers in order from biggest to smallest!"),
                "📉", seq, example: ExampleRow(n, true));
        }

        // Mỗi số là một chặng ngắn trộn các kiểu thao tác; bài ghép và xếp thứ tự đan vào đúng lúc bé đã biết các số đó,
        // thay vì học hết các số rồi mới làm liền hàng chục bài cùng một kiểu. Mã bài giữ nguyên nên tiến độ không đổi.
        for (var n = 0; n <= 20; n++)
        {
            if (n % 5 == 0 && n < 20) g.Stage = n == 0 ? "Số 0 đến 4" : $"Số {n} đến {Math.Min(20, n + 4)}";
            if (n == 20) g.Stage = "Số 16 đến 20";
            var theme = CountThemes[n % CountThemes.Length];
            string[]? scene = n is >= 1 and <= 10 ? Enumerable.Repeat(theme.Emoji, n).ToArray() : null;
            var number = n;

            var steps = new List<Action> { () => AddFind(number, scene) };
            if (n is >= 1 and <= 9) steps.Add(() => AddQuantityMatch(number));
            steps.Add(() => AddListen(number));
            if (n is >= 3 and <= 9) steps.Add(() => AddOrderUp(number - 2));
            if (n is >= 1 and <= 19) steps.Add(() => AddNeighbor(number));
            if (n is >= 3 and <= 9) steps.Add(() => AddOrderDown(number - 2));
            // Sau bài làm quen, thứ tự các phần còn lại đổi theo từng số.
            if (n % 2 == 1) steps.Reverse(1, steps.Count - 1);
            foreach (var step in steps) step();

            g.Topic = "viet-so";
            g.Trace($"number-trace-{n:00}", $"Tập tô số {n}",
                new Say($"Con tô số {n} theo nét chấm nhé!", $"Trace the number {NumEn(n)} along the dotted lines!", $"Con tô số {NumVi(n)} theo nét chấm nhé!"),
                "✏️", n.ToString(), n > 9 ? 2 : 1);
        }
    }
}
