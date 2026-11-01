namespace HanhTrangLop1.Application.Lessons;

public static partial class LessonBank
{
    private static void AddQuantity(LessonGroupBuilder g)
    {

        var comparisons = new (string Slug, int Left, int Right, string Mode)[]
        {
            ("math-compare-more-2-5", 2, 5, "more"), ("math-compare-more-7-3", 7, 3, "more"), ("math-compare-equal-4-4", 4, 4, "equal"),
            ("math-compare-equal-6-6", 6, 6, "equal"), ("math-compare-less-2-8", 2, 8, "less"), ("math-compare-less-5-9", 5, 9, "less"),
            ("v5-compare-more-6-4", 6, 4, "more"), ("v5-compare-less-3-1", 3, 1, "less"), ("v5-compare-more-5-8", 5, 8, "more"),
            ("v5-compare-equal-3-3", 3, 3, "equal"), ("v5-compare-less-7-9", 7, 9, "less"), ("v5-compare-more-9-6", 9, 6, "more")
        };
        var finds = new (int Target, int ThemeA, int ThemeB, int OtherA, int OtherB)[]
        {
            (2, 0, 1, 3, 1), (3, 2, 3, 2, 4), (4, 4, 5, 5, 3), (5, 6, 7, 4, 6), (3, 8, 9, 4, 2), (6, 10, 0, 5, 7)
        };

        void AddCount(int n)
        {
            g.Topic = "dem-so-luong";
            var t = CountThemes[(n + 3) % CountThemes.Length];
            g.Count($"math-count-{n}", $"Đếm {t.Vi}: {n}",
                new Say($"Có bao nhiêu {t.Vi}? Con chạm vào từng {t.Vi} để đếm nhé!", $"How many {t.EnPlural} are there? Tap each one to count!"),
                t.Emoji, t.Emoji, n,
                retry: new Say("Con đếm lại thật chậm, chạm mỗi hình một lần nhé!", "Count again slowly. Tap each picture once!"));
        }

        void AddMake(int n)
        {
            g.Topic = "tao-so-luong";
            var t = CountThemes[(n + 6) % CountThemes.Length];
            g.Build($"math-make-{n}", $"Cô cần {n} {t.Vi}",
                new Say($"Cô cần {n} {t.Vi}. Con bấm thêm hoặc bớt cho đủ nhé!", $"I need {NumEn(n)} {(n == 1 ? t.En : t.EnPlural)}. Add or take away until it's just right!", $"Cô cần {NumVi(n)} {t.Vi}. Con bấm thêm hoặc bớt cho đủ nhé!"),
                "🧺", t.Emoji, n,
                retry: new Say("Con đếm lại số hình trong giỏ nhé!", "Count the pictures in the basket again!"));
        }

        void AddCompare(int i)
        {
            g.Topic = "so-sanh";
            var c = comparisons[i];
            var t = CountThemes[i % CountThemes.Length];
            var (title, say) = c.Mode switch
            {
                "more" => ($"Nhóm nào nhiều {t.Vi} hơn?", new Say($"Nhóm nào có nhiều {t.Vi} hơn? Con chạm vào nhóm đó nhé!", $"Which group has more {t.EnPlural}? Tap that group!")),
                "less" => ($"Nhóm nào ít {t.Vi} hơn?", new Say($"Nhóm nào có ít {t.Vi} hơn? Con chạm vào nhóm đó nhé!", $"Which group has fewer {t.EnPlural}? Tap that group!")),
                _ => ($"Hai nhóm {t.Vi} bằng nhau", new Say($"Hai nhóm {t.Vi} này thế nào? Nếu bằng nhau thì con chạm vào nút Bằng nhau nhé!", $"Look at the two groups of {t.EnPlural}. If they are the same, tap Equal!"))
            };
            g.Compare(c.Slug, title, say, c.Mode == "equal" ? "⚖️" : c.Mode == "more" ? "⬆️" : "⬇️", t.Emoji, c.Left, c.Right, c.Mode,
                retry: new Say("Con đếm từng nhóm rồi so sánh lại nhé!", "Count each group, then compare again!"));
        }

        void AddFind(int i)
        {
            g.Topic = "ghep-so-luong";
            var f = finds[i];
            var a = CountThemes[f.ThemeA];
            var b = CountThemes[f.ThemeB];
            var right1 = QuantityLabel(a, f.Target);
            var right2 = QuantityLabel(b, f.Target);
            var wrong1 = QuantityLabel(a, f.OtherA);
            var wrong2 = QuantityLabel(b, f.OtherB);
            g.Multi($"v5-find-groups-{f.Target}-{f.ThemeA}", $"Tìm hai nhóm có {f.Target} hình",
                new Say($"Con tìm hai nhóm có đúng {f.Target} hình nhé!", $"Find the two groups that have exactly {NumEn(f.Target)} pictures!", $"Con tìm hai nhóm có đúng {NumVi(f.Target)} hình nhé!"),
                f.Target.ToString(), [right1, wrong1, wrong2, right2], [right1, right2],
                retry: new Say("Con đếm từng nhóm thật kỹ nhé!", "Count each group carefully!"));
        }

        // Mỗi số là một chặng ngắn trộn nhiều kiểu thao tác (đếm, tự tạo, so sánh, tìm nhóm) thay vì
        // làm liền hàng chục bài cùng một kiểu. Mã bài giữ nguyên nên tiến độ của bé không đổi.
        for (var n = 1; n <= 10; n++)
        {
            g.Stage = n <= 3 ? "Số 1 đến 3" : n <= 6 ? "Số 4 đến 6" : "Số 7 đến 10";
            if (n % 2 == 1) AddCount(n);
            else AddMake(n);

            AddCompare(n - 1);
            if (n is >= 3 and <= 8) AddFind(n - 3);

            if (n % 2 == 1) AddMake(n);
            else AddCount(n);
            if (n >= 9) AddCompare(n + 1);
        }
    }
}
