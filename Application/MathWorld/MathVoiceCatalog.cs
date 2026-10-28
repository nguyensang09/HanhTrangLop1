namespace HanhTrangLop1.Application.MathWorld;

/// <summary>Một câu cần đọc: khóa dùng ở trình chơi, văn bản tiếng Việt và tiếng Anh tương ứng.</summary>
public sealed record MathVoiceLine(string Key, string Vi, string En)
{
    public string TextFor(bool english) => english ? En : Vi;
}

/// <summary>
/// Kịch bản Voice của Đảo Toán Học. Mọi câu được viết sẵn cho cả hai ngôn ngữ (không dịch máy) để
/// phép tính được đọc thành câu tự nhiên: "Ba cộng hai bằng mấy?" / "What is three plus two?".
/// Văn bản tiếng Việt là khóa của kho Voice dùng chung (TextToSpeechCache), cột tiếng Anh lưu bản đọc tương ứng.
/// </summary>
public static class MathVoiceCatalog
{
    public const string UsageType = "math";

    private static readonly string[] NumbersVi = ["không", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín", "mười"];
    private static readonly string[] NumbersEn = ["zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten"];

    public static readonly IReadOnlyList<MathVoiceLine> Praise =
    [
        new("praise0", "Chính xác!", "Correct!"),
        new("praise1", "Đúng rồi!", "That's right!")
    ];

    public static readonly IReadOnlyList<MathVoiceLine> Retry =
    [
        new("retry0", "Chưa đúng, con thử lại nhé!", "Not quite. Try again!"),
        new("retry1", "Con nghĩ thêm chút nữa nhé!", "Think a little more!"),
        new("retry2", "Không sao đâu, thử lại nào!", "That's okay. Try again!")
    ];

    public static readonly IReadOnlyList<MathVoiceLine> Cheer =
    [
        new("next0", "Cố lên nào!", "Keep going!"),
        new("next1", "Con làm được mà!", "You can do it!"),
        new("next2", "Tiếp tục nhé!", "Let's keep going!"),
        new("next3", "Cô tin con làm được!", "I believe in you!")
    ];

    private static readonly Lazy<IReadOnlyList<MathVoiceLine>> LazyCommon = new(() =>
    {
        var lines = new List<MathVoiceLine>();
        for (var n = 0; n <= MathCurriculum.MaxNumber; n++)
        {
            lines.Add(new($"n{n}", Cap(NumbersVi[n]), Cap(NumbersEn[n])));
        }
        lines.AddRange(Praise);
        lines.AddRange(Retry);
        lines.AddRange(Cheer);
        lines.AddRange(
        [
            new("start", "Chạm vào đây để bắt đầu học cùng cô nhé!", "Tap here to start learning with me!"),
            new("matchFirst", "Con chạm vào một phép tính ở cột bên trái trước nhé!", "First, tap a problem on the left."),
            new("matchPair", "Đúng rồi! Ghép tiếp nào.", "Yes! Keep matching."),
            new("matchDone", "Con đã ghép đủ các cặp rồi!", "You matched all the pairs!"),
            new("buildDoneAdd", "Giỏi lắm! Bây giờ con đếm xem trong giỏ có tất cả bao nhiêu nhé.", "Great! Now count how many are in the basket altogether."),
            new("buildDoneSub", "Giỏi lắm! Bây giờ con đếm xem trong giỏ còn lại bao nhiêu nhé.", "Great! Now count how many are left in the basket."),
            new("buildOver", "Ôi, nhiều quá rồi! Con trả bớt lại nhé.", "Oops, that's too many! Put some back."),
            new("basketFull", "Giỏ chỉ chứa được mười đồ vật thôi!", "The basket only holds ten things!"),
            new("hopStop", "Ếch dừng lại rồi! Ếch đang ở số mấy? Con chạm vào số đó nhé.", "The frog stopped! Which number is it on? Tap that number."),
            new("hopOver", "Ếch nhảy nhiều quá rồi. Con bấm Về chỗ cũ nhé!", "The frog hopped too far. Tap Start over!"),
            new("hopEnd", "Hết đường rồi! Con bấm Về chỗ cũ để nhảy lại nhé.", "That's the end of the path! Tap Start over to hop again."),
            new("finish1", "Hoàn thành rồi! Con được một sao. Chơi lại để lấy thêm sao nhé!", "All done! You earned one star. Play again to win more stars!"),
            new("finish2", "Hoàn thành rồi! Con được hai sao. Giỏi lắm!", "All done! You earned two stars. Great work!"),
            new("finish3", "Hoàn thành rồi! Con được ba sao. Xuất sắc quá!", "All done! You earned three stars. Amazing!"),
            new("saveError", "Ối, cô chưa cất được sao cho con. Con nhờ ba mẹ giúp nhé.", "Oops, I couldn't save your stars. Ask a grown-up for help.")
        ]);
        return lines;
    });

    /// <summary>Các câu dùng chung cho mọi bài: số đếm, khen, động viên, hướng dẫn thao tác.</summary>
    public static IReadOnlyList<MathVoiceLine> Common => LazyCommon.Value;

    /// <summary>Các câu riêng của một bài: giới thiệu bài và từng câu hỏi (câu hỏi, phép tính, kết quả, gợi ý).</summary>
    public static IReadOnlyList<MathVoiceLine> ForLesson(MathLessonDefinition lesson)
    {
        var lines = new List<MathVoiceLine>
        {
            new("intro", $"{Sentence(lesson.Title.Replace(" · ", ", "))} {lesson.Instruction}", $"{Sentence(lesson.TitleEn)} {InstructionEn(lesson)}")
        };

        for (var i = 0; i < lesson.Questions.Count; i++)
        {
            var q = lesson.Questions[i];
            var prefix = $"q{i}.";
            var (hint1, hint2) = MathCurriculum.BuildHints(q);
            var (hint1En, hint2En) = HintsEn(q);

            if (q.IsBoard)
            {
                lines.Add(new(prefix + "ask", "Con ghép mỗi phép tính với kết quả đúng nhé!", "Match each problem with the right answer!"));
                lines.Add(new(prefix + "hint1", hint1, hint1En));
                lines.Add(new(prefix + "hint2", hint2, hint2En));
                foreach (var pair in q.Pairs)
                {
                    lines.Add(new($"x:{pair}", Cap(ExpressionVi(pair)), Cap(ExpressionEn(pair))));
                    lines.Add(new($"r:{pair}", Cap(ResultVi(pair)), Cap(ResultEn(pair))));
                }
                continue;
            }

            var p = q.Problem!;
            var (askVi, askEn) = Ask(lesson, q);
            lines.Add(new(prefix + "ask", askVi, askEn));
            lines.Add(new(prefix + "eq", EquationVi(p), EquationEn(p)));
            lines.Add(new(prefix + "result", Cap(ResultVi(p)), Cap(ResultEn(p))));
            lines.Add(new(prefix + "hint1", hint1, hint1En));
            lines.Add(new(prefix + "hint2", hint2, hint2En));
        }

        return lines;
    }

    /// <summary>Toàn bộ câu cần Voice của Đảo Toán Học, mỗi văn bản tiếng Việt chỉ xuất hiện một lần.</summary>
    public static IReadOnlyList<MathVoiceLine> AllDistinct() =>
        Common.Concat(MathCurriculum.Lessons.SelectMany(ForLesson))
            .GroupBy(x => x.Vi.Trim().ToLowerInvariant())
            .Select(g => g.First())
            .ToList();

    /// <summary>Các văn bản tiếng Việt bị ánh xạ tới nhiều bản tiếng Anh khác nhau (phải rỗng).</summary>
    public static IReadOnlyList<string> FindConflicts() =>
        Common.Concat(MathCurriculum.Lessons.SelectMany(ForLesson))
            .GroupBy(x => x.Vi.Trim().ToLowerInvariant())
            .Where(g => g.Select(x => x.En).Distinct().Count() > 1)
            .Select(g => g.Key)
            .ToList();

    // ---------- Câu hỏi đọc thành lời ----------

    private static (string Vi, string En) Ask(MathLessonDefinition lesson, MathQuestionDefinition q)
    {
        var p = q.Problem!;
        var t = q.Theme;
        var e = t.En;
        var isAdd = p.Op == "+";
        var eq = (EquationVi(p), EquationEn(p));
        // Câu chuyện với "không quả táo" nghe gượng; những câu có số 0 đọc thẳng phép tính.
        var story = p.A > 0 && p.B > 0;

        switch (lesson.Style)
        {
            case MathCurriculum.Styles.Count when story:
                return isAdd
                    ? ($"Có {Vi(p.A)} {t.Noun} và {Vi(p.B)} {t.Noun}. Con đếm xem có tất cả bao nhiêu {t.Noun} nhé?",
                       $"{ThereAre(p.A, e)} and {En(p.B)} {e.NounFor(p.B)}. Count them. How many {e.Plural} are there altogether?")
                    : TakeAwayStory(p, t, "Còn lại bao nhiêu");
            case MathCurriculum.Styles.Build when story:
                return isAdd
                    ? ($"Trong giỏ có {Vi(p.A)} {t.Noun}. Con thêm {Vi(p.B)} {t.Unit} nữa, rồi cho cô biết có tất cả bao nhiêu nhé!",
                       $"{ThereAre(p.A, e)} in the basket. Add {En(p.B)} more, then tell me how many there are altogether.")
                    : ($"Trong giỏ có {Vi(p.A)} {t.Noun}. Con lấy bớt {Vi(p.B)} {t.Unit}, rồi cho cô biết còn lại bao nhiêu nhé!",
                       $"{ThereAre(p.A, e)} in the basket. Take away {En(p.B)}, then tell me how many are left.");
            case MathCurriculum.Styles.Hop when story:
                return isAdd
                    ? ($"Ếch đang đứng ở số {Vi(p.A)}. Ếch nhảy tới {Vi(p.B)} bước. Ếch đến số mấy?",
                       $"The frog is on number {En(p.A)}. It hops forward {En(p.B)} {Steps(p.B)}. Which number does it land on?")
                    : ($"Ếch đang đứng ở số {Vi(p.A)}. Ếch nhảy lùi {Vi(p.B)} bước. Ếch đến số mấy?",
                       $"The frog is on number {En(p.A)}. It hops back {En(p.B)} {Steps(p.B)}. Which number does it land on?");
            case MathCurriculum.Styles.Choose when story && p.Missing == "result" && lesson.Stage.Order <= 2:
                return isAdd
                    ? ($"Có {Vi(p.A)} {t.Noun}, {t.AddVerb} {Vi(p.B)} {t.Unit} nữa. Vậy có tất cả bao nhiêu {t.Noun}?",
                       $"{ThereAre(p.A, e)}. {Cap(En(p.B))} more {e.NounFor(p.B)} {(p.B == 1 ? "is" : "are")} added. How many {e.Plural} are there altogether?")
                    : TakeAwayStory(p, t, "Vậy còn lại bao nhiêu");
            default:
                return eq;
        }
    }

    private static (string Vi, string En) TakeAwayStory(MathProblem p, MathTheme t, string askVi)
    {
        var e = t.En;
        return ($"Có {Vi(p.A)} {t.Noun}, {Vi(p.B)} {t.Unit} {t.TakeVerb}. {askVi} {t.Noun}?",
                $"{ThereAre(p.A, e)}. {Cap(En(p.B))} {e.NounFor(p.B)} {(p.B == 1 ? e.TakeVerbOne : e.TakeVerbMany)}. How many {e.Plural} are left?");
    }

    private static string EquationVi(MathProblem p)
    {
        var op = OpVi(p.Op);
        return p.Missing switch
        {
            "a" => $"Mấy {op} {Vi(p.B)} bằng {Vi(p.Result)}?",
            "b" => Cap($"{Vi(p.A)} {op} mấy bằng {Vi(p.Result)}?"),
            _ => Cap($"{Vi(p.A)} {op} {Vi(p.B)} bằng mấy?")
        };
    }

    private static string EquationEn(MathProblem p)
    {
        var op = OpEn(p.Op);
        var verb = p.Op == "-" ? "leaves" : "makes";
        return p.Missing switch
        {
            "a" => $"What number {op} {En(p.B)} {verb} {En(p.Result)}?",
            "b" => $"{Cap(En(p.A))} {op} what number {verb} {En(p.Result)}?",
            _ => $"What is {En(p.A)} {op} {En(p.B)}?"
        };
    }

    private static string ExpressionVi(MathProblem p) => $"{Vi(p.A)} {OpVi(p.Op)} {Vi(p.B)}";
    private static string ExpressionEn(MathProblem p) => $"{En(p.A)} {OpEn(p.Op)} {En(p.B)}";
    private static string ResultVi(MathProblem p) => $"{ExpressionVi(p)} bằng {Vi(p.Result)}.";
    private static string ResultEn(MathProblem p) => $"{ExpressionEn(p)} equals {En(p.Result)}.";

    private static (string, string) HintsEn(MathQuestionDefinition q)
    {
        if (q.IsBoard)
        {
            return ("Solve each problem first, then find the answer that matches.",
                    "You can count on your fingers, or count the dots I drew for you.");
        }

        var p = q.Problem!;
        string Up(int from, int steps) => steps == 0 ? $"{from}" : string.Join(", ", Enumerable.Range(from + 1, steps));
        string Down(int from, int steps) => steps == 0 ? $"{from}" : string.Join(", ", Enumerable.Range(from - steps, steps).Reverse());

        if (p.Op == "+")
        {
            return p.Missing switch
            {
                "b" => ($"How many more does {p.A} need to make {p.Result}?",
                        $"Count on from {p.A} to {p.Result}: {Up(p.A, p.B)}. How many steps did you count?"),
                "a" => ($"Which number plus {p.B} makes {p.Result}?",
                        $"Count back {p.B} {Steps(p.B)} from {p.Result}: {Down(p.Result, p.B)}."),
                _ when p.B == 0 => ("Adding zero keeps the number the same!", $"{p.A} plus zero is still {p.A}."),
                _ when p.A == 0 => ("Zero plus a number is that same number!", $"Zero plus {p.B} is still {p.B}."),
                _ => ("Count again slowly, one at a time.",
                      $"Start at {p.A} and count on {p.B}: {Up(p.A, p.B)}.")
            };
        }

        return p.Missing switch
        {
            "b" => ($"From {p.A}, how many do you take away to leave {p.Result}?",
                    $"Count back from {p.A} to {p.Result}: {Down(p.A, p.B)}. How many steps did you count?"),
            "a" => ($"Which number minus {p.B} leaves {p.Result}?",
                    $"Start at {p.Result} and count on {p.B}: {Up(p.Result, p.B)}."),
            _ when p.B == 0 => ("Taking away zero keeps the number the same!", $"{p.A} minus zero is still {p.A}."),
            _ when p.B == p.A => ("Take them all away and nothing is left!", $"{p.A} minus {p.A} equals zero."),
            _ => ("Take them away one at a time, then count what is left.",
                  $"From {p.A}, count back {p.B} {Steps(p.B)}: {Down(p.A, p.B)}.")
        };
    }

    private static string InstructionEn(MathLessonDefinition lesson)
    {
        var isAdd = lesson.Operation.Key == "add";
        return lesson.Style switch
        {
            MathCurriculum.Styles.Count => isAdd ? "Tap each object to count them all, then choose the right number." : "Count the objects that are left, then choose the right number.",
            MathCurriculum.Styles.Build => isAdd ? "Tap the add button to put objects in the basket, then tell me how many there are." : "Tap objects to take them out of the basket, then tell me how many are left.",
            MathCurriculum.Styles.Choose => "Tap the balloon with the right answer.",
            MathCurriculum.Styles.Drag => "Drag the right number card into the empty box, or just tap the card.",
            MathCurriculum.Styles.Missing => "Find the hiding number and tap it on the number board.",
            MathCurriculum.Styles.Hop => isAdd ? "Tap the hop button to make the frog jump forward, then tap the number where it lands." : "Tap the hop button to make the frog jump back, then tap the number where it lands.",
            MathCurriculum.Styles.Match => "Tap a problem, then tap its answer.",
            MathCurriculum.Styles.Adventure => "Solve each problem to clear the way to the treasure.",
            MathCurriculum.Styles.Quick => "Solve in your head to make the rocket fly higher. No rush, take your time!",
            _ => "Follow my instructions."
        };
    }

    // ---------- Từ vựng ----------

    private static string Vi(int n) => n is >= 0 and <= 10 ? NumbersVi[n] : n.ToString();
    private static string En(int n) => n is >= 0 and <= 10 ? NumbersEn[n] : n.ToString();
    private static string OpVi(string op) => op switch { "-" => "trừ", "*" => "nhân", "/" => "chia", _ => "cộng" };
    private static string OpEn(string op) => op switch { "-" => "minus", "*" => "times", "/" => "divided by", _ => "plus" };
    private static string Steps(int n) => n == 1 ? "step" : "steps";
    private static string ThereAre(int n, MathThemeEn e) => n == 1 ? $"There is one {e.Noun}" : $"There are {En(n)} {e.Plural}";
    private static string Cap(string s) => string.IsNullOrEmpty(s) ? s : char.ToUpperInvariant(s[0]) + s[1..];
    private static string Sentence(string s) => s.EndsWith('?') || s.EndsWith('!') || s.EndsWith('.') ? s : s + ".";
}
