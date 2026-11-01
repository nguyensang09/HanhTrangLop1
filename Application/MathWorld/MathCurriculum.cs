using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace HanhTrangLop1.Application.MathWorld;

/// <summary>
/// Chương trình Đảo Toán Học được định nghĩa hoàn toàn bằng mã (không phụ thuộc người quản trị tạo tay).
/// Phân tách ba khái niệm:
/// <list type="bullet">
/// <item>Bài học (<see cref="MathLessonDefinition"/>) — một chặng chơi, ánh xạ 1-1 với LearningItem.</item>
/// <item>Phong cách tương tác (<see cref="MathStyleDefinition"/>) — cách bé thao tác (đếm, kéo thả, nhảy tia số...).</item>
/// <item>Bộ câu hỏi (<see cref="MathQuestionDefinition"/>) — các phép tính cụ thể lấy từ ngân hàng phép tính 0–10.</item>
/// </list>
/// Mọi dữ liệu sinh ra đều tất định (cùng mã → cùng Id, cùng thứ tự, cùng đáp án nhiễu) để seed chạy lại an toàn.
/// </summary>
public static class MathCurriculum
{
    public const string SkillGroupCode = "toan-hoc";
    public const string LessonCodePrefix = "math10-";
    public const string Engine = "math-world";
    public const int MaxNumber = 10;

    /// <summary>Tăng số này khi đổi nội dung sinh bài để seed cập nhật các bài chưa bị quản trị viên chỉnh sửa.</summary>
    public const int ContentVersion = 6;

    public static readonly MathOperationDefinition Addition = new(
        "add", "+", "+", "Phép cộng", "phep-cong", true, "#ff8a3d", "Gộp lại, thêm vào");
    public static readonly MathOperationDefinition Subtraction = new(
        "sub", "-", "−", "Phép trừ", "phep-tru", true, "#2f9bff", "Bớt đi, lấy ra");
    public static readonly MathOperationDefinition Multiplication = new(
        "mul", "*", "×", "Phép nhân", "phep-nhan", false, "#22c08a", "Sắp ra mắt");
    public static readonly MathOperationDefinition Division = new(
        "div", "/", "÷", "Phép chia", "phep-chia", false, "#e255a8", "Sắp ra mắt");

    public static readonly IReadOnlyList<MathOperationDefinition> Operations = [Addition, Subtraction, Multiplication, Division];

    public static class Styles
    {
        public const string Count = "count";
        public const string Build = "build";
        public const string Choose = "choose";
        public const string Drag = "drag";
        public const string Missing = "missing";
        public const string Hop = "hop";
        public const string Match = "match";
        public const string Adventure = "adventure";
        public const string Quick = "quick";
        public const string Balance = "balance";
        public const string TrueFalse = "truefalse";
        public const string Train = "train";
        public const string Memory = "memory";
        public const string Sort = "sort";
        public const string Pop = "pop";
        public const string Fish = "fish";
        public const string Bus = "bus";
    }

    public static readonly IReadOnlyList<MathStyleDefinition> StyleCatalog =
    [
        new(Styles.Count, "Đếm đồ vật", "🍎", "Quan sát hình, chạm để đếm rồi chọn kết quả."),
        new(Styles.Build, "Thêm hoặc bớt", "🧺", "Tự tay thêm vào hoặc lấy bớt đồ vật trong giỏ."),
        new(Styles.Choose, "Chọn đáp án", "🎈", "Chạm vào quả bóng có kết quả đúng."),
        new(Styles.Drag, "Kéo thả số", "🧩", "Kéo thẻ số phù hợp vào ô trống của phép tính."),
        new(Styles.Missing, "Điền số còn thiếu", "🔍", "Tìm số bị trốn trong phép tính bằng bảng số to."),
        new(Styles.Hop, "Ếch nhảy tia số", "🐸", "Giúp ếch nhảy tới hoặc lùi trên tia số."),
        new(Styles.Match, "Ghép cặp", "🔗", "Nối mỗi phép tính với kết quả tương ứng."),
        new(Styles.Adventure, "Phiêu lưu", "🗺️", "Giải phép tính để vượt chướng ngại vật, tìm kho báu."),
        new(Styles.Quick, "Tên lửa tính nhẩm", "🚀", "Tính nhẩm nhanh dần, không tính giờ, đúng liên tiếp để bay cao."),
        new(Styles.Balance, "Cân thăng bằng", "⚖️", "Đặt quả cân số lên đĩa để cân thăng bằng."),
        new(Styles.TrueFalse, "Đúng hay sai", "👍", "Nhìn phép tính rồi giơ ngón cái lên hoặc xuống."),
        new(Styles.Train, "Đoàn tàu đếm tiếp", "🚂", "Đếm theo từng toa tàu rồi chạm vào một số đúng."),
        new(Styles.Memory, "Lật thẻ", "🃏", "Lật thẻ tìm phép tính và kết quả của nó."),
        new(Styles.Sort, "Nhà số", "🏠", "Đưa từng phép tính về đúng ngôi nhà mang kết quả của nó."),
        new(Styles.Pop, "Chọc bong bóng", "🫧", "Chọc hết bong bóng có phép tính bằng số cho trước."),
        new(Styles.Fish, "Câu cá", "🎣", "Chạm chú cá mang kết quả đúng của phép tính."),
        new(Styles.Bus, "Xe buýt", "🚌", "Nhìn các bạn lên, xuống xe rồi chọn số bạn trên xe.")
    ];

    public static MathStyleDefinition GetStyle(string? key) =>
        StyleCatalog.FirstOrDefault(x => x.Key == key) ?? StyleCatalog[2];

    public static MathOperationDefinition? FindOperationByTopic(string? topicCode) =>
        Operations.FirstOrDefault(x => x.TopicCode == topicCode);

    private static readonly MathTheme[] Themes =
    [
        new("🍎", "quả táo", "quả", "hái thêm", "bị ăn mất", new("apple", "apples", "is eaten", "are eaten")),
        new("🐟", "con cá", "con", "bơi tới", "bơi đi", new("fish", "fish", "swims away", "swim away")),
        new("🎈", "quả bóng bay", "quả", "thổi thêm", "bay mất", new("balloon", "balloons", "floats away", "float away")),
        new("🐥", "chú gà con", "chú", "chạy tới", "chạy đi", new("chick", "chicks", "runs away", "run away")),
        new("🦋", "con bướm", "con", "bay tới", "bay đi", new("butterfly", "butterflies", "flies away", "fly away")),
        new("🍓", "quả dâu", "quả", "hái thêm", "bị ăn mất", new("strawberry", "strawberries", "is eaten", "are eaten")),
        new("🚗", "chiếc ô tô", "chiếc", "chạy tới", "chạy đi", new("car", "cars", "drives away", "drive away")),
        new("🍪", "chiếc bánh", "chiếc", "nướng thêm", "bị ăn mất", new("cookie", "cookies", "is eaten", "are eaten")),
        new("🌼", "bông hoa", "bông", "nở thêm", "được hái đi", new("flower", "flowers", "is picked", "are picked")),
        new("⭐", "ngôi sao", "ngôi", "hiện thêm", "lặn mất", new("star", "stars", "fades away", "fade away"))
    ];

    private static readonly Lazy<IReadOnlyList<MathLessonDefinition>> LazyLessons = new(() =>
    {
        var lessons = new List<MathLessonDefinition>();
        BuildOperation(Addition, lessons);
        BuildOperation(Subtraction, lessons);
        Validate(lessons);
        return lessons;
    });

    public static IReadOnlyList<MathLessonDefinition> Lessons => LazyLessons.Value;

    public static IReadOnlyList<MathStageDefinition> StagesFor(MathOperationDefinition op) => op.Key switch
    {
        "add" =>
        [
            new(op.Key, 1, "kham-pha", "Phép cộng là gì?", "Gộp lại, thêm vào là phép cộng", "🧺"),
            new(op.Key, 2, "pham-vi-5", "Cộng trong phạm vi 5", "Những phép cộng nhỏ xinh", "🌱"),
            new(op.Key, 3, "pham-vi-10", "Cộng trong phạm vi 10", "Cộng ra 6, 7, 8, 9 và 10", "🌳"),
            new(op.Key, 4, "so-thieu", "Tìm số còn thiếu", "Trở thành thám tử số bí ẩn", "🔍"),
            new(op.Key, 5, "tinh-nham", "Luyện tập và tính nhẩm", "Ghép cặp, phiêu lưu và tên lửa", "🚀")
        ],
        "sub" =>
        [
            new(op.Key, 1, "kham-pha", "Phép trừ là gì?", "Bớt đi, lấy ra là phép trừ", "🧺"),
            new(op.Key, 2, "pham-vi-5", "Trừ trong phạm vi 5", "Những phép trừ nhỏ xinh", "🌱"),
            new(op.Key, 3, "pham-vi-10", "Trừ trong phạm vi 10", "Trừ từ 6, 7, 8, 9 và 10", "🌳"),
            new(op.Key, 4, "so-thieu", "Tìm số còn thiếu", "Trở thành thám tử số bí ẩn", "🔍"),
            new(op.Key, 5, "tinh-nham", "Luyện tập và tính nhẩm", "Ghép cặp, phiêu lưu và tên lửa", "🚀")
        ],
        _ => []
    };

    /// <summary>Phép cộng trong phạm vi 10, không có số 0 (45 phép tính): cộng với 0 làm bé khó hiểu hơn hẳn các bài khác.</summary>
    public static IReadOnlyList<MathProblem> AdditionFacts() =>
        (from sum in Enumerable.Range(2, MaxNumber - 1)
         from a in Enumerable.Range(1, sum - 1)
         select new MathProblem("+", a, sum - a)).ToList();

    /// <summary>Phép trừ trong phạm vi 10, không có số 0 ở số trừ lẫn kết quả (45 phép tính).</summary>
    public static IReadOnlyList<MathProblem> SubtractionFacts() =>
        (from a in Enumerable.Range(2, MaxNumber - 1)
         from b in Enumerable.Range(1, a - 1)
         select new MathProblem("-", a, b)).ToList();

    public static Guid StableGuid(string key)
    {
        var hash = MD5.HashData(Encoding.UTF8.GetBytes("hanhtranglop1:math-world:" + key));
        return new Guid(hash);
    }

    private static void BuildOperation(MathOperationDefinition op, List<MathLessonDefinition> lessons)
    {
        var isAdd = op.Key == "add";
        var facts = isAdd ? AdditionFacts() : SubtractionFacts();
        var stages = StagesFor(op);
        var opIndex = Array.IndexOf(Operations.ToArray(), op) + 1;
        var themeCursor = isAdd ? 0 : 3;

        // "Kích thước" để phân chặng: phép cộng theo tổng, phép trừ theo số bị trừ.
        int Size(MathProblem p) => isAdd ? p.Result : p.A;
        string Word(int lo, int hi) => lo == hi ? $"{lo}" : $"{lo} đến {hi}";
        string WordEn(int lo, int hi) => lo == hi ? $"{lo}" : $"{lo} to {hi}";

        void AddLesson(MathStageDefinition stage, string title, string titleEn, string style, IEnumerable<MathProblem> problems, int optionCount = 3)
        {
            var index = lessons.Count(x => x.Operation == op && x.Stage == stage) + 1;
            var code = $"{LessonCodePrefix}{op.Key}-s{stage.Order}-{index:00}";
            var questions = problems.Select(p => SingleQuestion(p, optionCount, Themes[themeCursor++ % Themes.Length], $"{code}:{p}")).ToList();
            lessons.Add(new MathLessonDefinition(code, op, stage, index, opIndex * 1000 + stage.Order * 100 + index * 10,
                title, titleEn, style, BuildInstruction(op, style), questions));
        }

        void AddBoardLesson(MathStageDefinition stage, string title, string titleEn, IReadOnlyList<IReadOnlyList<MathProblem>> boards)
        {
            var index = lessons.Count(x => x.Operation == op && x.Stage == stage) + 1;
            var code = $"{LessonCodePrefix}{op.Key}-s{stage.Order}-{index:00}";
            var questions = boards.Select(b => new MathQuestionDefinition(null, b, [], Themes[themeCursor++ % Themes.Length])).ToList();
            lessons.Add(new MathLessonDefinition(code, op, stage, index, opIndex * 1000 + stage.Order * 100 + index * 10,
                title, titleEn, Styles.Match, BuildInstruction(op, Styles.Match), questions));
        }

        // Chặng 1: hiểu ý nghĩa phép tính bằng thao tác với đồ vật và tia số.
        var s1 = stages[0];
        var meaning = Shuffle(facts.Where(p => p.B >= 1 && p.Result >= (isAdd ? 2 : 1) && Size(p) <= 5 && (isAdd ? p.A >= 1 : true)), $"{op.Key}-meaning");
        AddLesson(s1, isAdd ? "Thêm vào là phép cộng" : "Bớt đi là phép trừ", isAdd ? "Adding means putting more in" : "Subtracting means taking away", Styles.Build, meaning.Take(5));
        AddLesson(s1, isAdd ? "Đếm tất cả" : "Đếm số còn lại", isAdd ? "Count them all" : "Count what is left", Styles.Count, meaning.Skip(5));
        var hopFacts = Shuffle(facts.Where(p => p.B is >= 1 and <= 3 && p.A >= 1 && Size(p) is >= 3 and <= 7 && p.Result >= 1), $"{op.Key}-hop").Take(5);
        AddLesson(s1, isAdd ? "Ếch con nhảy tới" : "Ếch con nhảy lùi", isAdd ? "The little frog hops forward" : "The little frog hops back", Styles.Hop, hopFacts);

        // Chặng 2: toàn bộ phép tính trong phạm vi 5, từ dễ đến khó.
        var s2 = stages[1];
        var small = facts.Where(p => Size(p) <= 5).OrderBy(Size).ThenBy(p => p.B).ToList();
        var s2Styles = new[] { Styles.Choose, Styles.Drag, Styles.Count };
        var s2Chunks = BalancedChunks(small, 3);
        for (var i = 0; i < s2Chunks.Count; i++)
        {
            var chunk = s2Chunks[i];
            var title = isAdd
                ? $"Cộng ra {Word(chunk.Min(Size), chunk.Max(Size))}"
                : $"Trừ từ {Word(chunk.Min(Size), chunk.Max(Size))}";
            var titleEn = isAdd
                ? $"Sums from {WordEn(chunk.Min(Size), chunk.Max(Size))}"
                : $"Subtracting from {WordEn(chunk.Min(Size), chunk.Max(Size))}";
            AddLesson(s2, title, titleEn, s2Styles[i % s2Styles.Length], Shuffle(chunk, $"{op.Key}-s2-{i}"));
        }

        // Chặng 3: toàn bộ phép tính có tổng (hoặc số bị trừ) từ 6 đến 10, mỗi bài tối đa 8 câu.
        var s3 = stages[2];
        var s3Styles = new[] { Styles.Choose, Styles.Build, Styles.Drag, Styles.Count, Styles.Hop, Styles.Balance, Styles.Drag, Styles.Build };
        var s3Index = 0;
        for (var size = 6; size <= MaxNumber; size++)
        {
            var family = facts.Where(p => Size(p) == size).OrderBy(p => p.B).ToList();
            // Số phần cố định theo từng số để số bài của chặng không đổi: mã các bài chen sau chặng này giữ nguyên, tiến độ của bé không lệch.
            var parts = BalancedChunks(family, size >= 8 ? 2 : 1);
            for (var part = 0; part < parts.Count; part++)
            {
                var title = (isAdd ? $"Cộng ra {size}" : $"Trừ từ {size}") + (parts.Count > 1 ? $" · Phần {part + 1}" : string.Empty);
                var titleEn = (isAdd ? $"Sums of {size}" : $"Subtracting from {size}") + (parts.Count > 1 ? $", part {part + 1}" : string.Empty);
                AddLesson(s3, title, titleEn, s3Styles[s3Index % s3Styles.Length], Shuffle(parts[part], $"{op.Key}-s3-{size}-{part}"));
                s3Index++;
            }
        }

        // Chặng 4: tìm số còn thiếu ở vị trí số hạng (hoặc số bị trừ / số trừ).
        var s4 = stages[3];
        IEnumerable<MathProblem> WithMissing(IEnumerable<MathProblem> source) =>
            source.Select((p, i) => p with { Missing = i % 2 == 0 ? "b" : "a" });
        var m1 = Shuffle(facts.Where(p => p.A >= 1 && p.B >= 1 && p.Result >= 1 && Size(p) is >= 2 and <= 5), $"{op.Key}-m1").Take(6);
        var m2 = Shuffle(facts.Where(p => p.A >= 1 && p.B >= 1 && p.Result >= 1 && Size(p) is >= 6 and <= 8), $"{op.Key}-m2").Take(6);
        var m3 = Shuffle(facts.Where(p => Size(p) is >= 9 and <= 10), $"{op.Key}-m3").Take(6);
        AddLesson(s4, "Số nào trốn mất?", "Which number is hiding?", Styles.Missing, WithMissing(m1));
        AddLesson(s4, "Kéo số vào ô trống", "Drag the number into the box", Styles.Drag, WithMissing(m2));
        AddLesson(s4, "Thám tử số bí ẩn", "Mystery number detective", Styles.Missing, WithMissing(m3));

        // Chặng 5: củng cố, tính nhẩm qua ghép cặp, phiêu lưu và thử thách tăng dần.
        var s5 = stages[4];
        AddBoardLesson(s5, "Ghép cặp phạm vi 5", "Matching up to 5", BuildBoards(facts.Where(p => Size(p) <= 5), 3, $"{op.Key}-board-5"));
        AddBoardLesson(s5, "Ghép cặp phạm vi 10", "Matching up to 10", BuildBoards(facts.Where(p => Size(p) >= 6), 3, $"{op.Key}-board-10"));
        var trip = Shuffle(facts.Where(p => Size(p) >= 3 && p.B >= 1), $"{op.Key}-trip").Take(6).OrderBy(Size);
        AddLesson(s5, "Vượt suối tìm kho báu", "Crossing the stream to the treasure", Styles.Adventure, trip);
        var climbEasy = Shuffle(facts.Where(p => Size(p) >= 6 && p.B >= 1), $"{op.Key}-climb-a").Take(4).OrderBy(Size);
        var climbHard = WithMissing(Shuffle(facts.Where(p => Size(p) >= 5 && p.A >= 1 && p.B >= 1 && p.Result >= 1), $"{op.Key}-climb-b").Take(4));
        AddLesson(s5, "Leo núi tìm kho báu", "Climbing the mountain to find treasure", Styles.Adventure, climbEasy.Concat(climbHard));
        AddLesson(s5, "Tên lửa tính nhẩm", "The mental math rocket", Styles.Quick, Ramp(facts, Size, $"{op.Key}-quick"), optionCount: 4);

        if (!isAdd)
        {
            // Bài tổng kết trộn lẫn cộng và trừ: bé phải nhìn dấu để quyết định cách tính.
            var addRamp = Ramp(AdditionFacts(), p => p.Result, "mix-add");
            var subRamp = Ramp(facts, Size, "mix-sub");
            var mixed = Enumerable.Range(0, 10).Select(i => i % 2 == 0 ? addRamp[i] : subRamp[i]);
            AddLesson(s5, "Đại thử thách cộng trừ", "The big plus and minus challenge", Styles.Quick, mixed, optionCount: 4);
        }

        // Các cách chơi bổ sung: mã bài nối tiếp nên bài cũ giữ nguyên mã và tiến độ; SortOrder chen giữa bài cũ
        // (after = chỉ số bài đứng trước) để bé gặp dạng mới sớm, không phải làm hết kiểu cũ mới tới kiểu mới.
        void AddExtra(MathStageDefinition stage, int after, string title, string titleEn, string style, IEnumerable<MathQuestionDefinition> questions)
        {
            var index = lessons.Count(x => x.Operation == op && x.Stage == stage) + 1;
            var code = $"{LessonCodePrefix}{op.Key}-s{stage.Order}-{index:00}";
            var sortOrder = opIndex * 1000 + stage.Order * 100 + after * 10 + 5;
            lessons.Add(new MathLessonDefinition(code, op, stage, index, sortOrder, title, titleEn, style, BuildInstruction(op, style), questions.ToList()));
        }

        MathTheme NextTheme() => Themes[themeCursor++ % Themes.Length];
        MathQuestionDefinition Single(MathProblem p, string seed) => SingleQuestion(p, 3, NextTheme(), $"{seed}:{p}");
        var word = isAdd ? "cộng" : "trừ";
        var wordEn = isAdd ? "adding" : "subtracting";
        var counted = facts.Where(p => p.A >= 1 && p.B >= 1 && p.Result >= 1).ToList();

        AddExtra(s1, 2, isAdd ? "Đoàn tàu đếm tiếp" : "Đoàn tàu đếm lùi", isAdd ? "The counting-on train" : "The counting-back train", Styles.Train,
            Shuffle(counted.Where(p => p.B <= 3 && Size(p) is >= 3 and <= 7), $"{op.Key}-train-1").Take(5).Select(p => Single(p, "train-1")));

        AddExtra(s2, 1, "Cân thăng bằng", "Balance the scale", Styles.Balance,
            Shuffle(counted.Where(p => Size(p) <= 5), $"{op.Key}-balance-1").Take(5).Select(p => Single(p, "balance-1")));
        AddExtra(s2, 2, $"Đúng hay sai: phép {word}", $"True or false: {wordEn}", Styles.TrueFalse,
            ClaimQuestions(Shuffle(counted.Where(p => Size(p) <= 5), $"{op.Key}-claim-1").Take(6), NextTheme, $"{op.Key}-claim-1"));
        AddExtra(s2, 3, "Lật thẻ tìm đôi", "Flip and find the pairs", Styles.Memory,
            BuildBoards(counted.Where(p => Size(p) <= 5), 2, $"{op.Key}-flip-5", 3).Select(b => Board(b, NextTheme())));

        AddExtra(s3, 2, "Nhà số vui vẻ", "The happy number houses", Styles.Sort,
            HouseBoards(counted.Where(p => Size(p) >= 6), 3, $"{op.Key}-house").Select(b => Board(b, NextTheme())));
        AddExtra(s3, 4, "Chọc bong bóng phép tính", "Pop the problem bubbles", Styles.Pop,
            PopBoards(counted.Where(p => Size(p) >= 4), 3, $"{op.Key}-pop").Select(b => b with { Theme = NextTheme() }));
        AddExtra(s3, 6, isAdd ? "Tàu đếm tiếp đường dài" : "Tàu đếm lùi đường dài", isAdd ? "The long counting-on ride" : "The long counting-back ride", Styles.Train,
            Shuffle(counted.Where(p => p.B is >= 2 and <= 5 && Size(p) >= 6), $"{op.Key}-train-2").Take(5).Select(p => Single(p, "train-2")));

        AddExtra(s5, 1, "Lật thẻ phạm vi 10", "Flip the cards up to 10", Styles.Memory,
            BuildBoards(counted.Where(p => Size(p) >= 6), 3, $"{op.Key}-flip-10", 4).Select(b => Board(b, NextTheme())));
        AddExtra(s5, 3, $"Thử tài đúng sai: phép {word}", $"True or false challenge: {wordEn}", Styles.TrueFalse,
            ClaimQuestions(Shuffle(counted.Where(p => Size(p) >= 6), $"{op.Key}-claim-2").Take(8), NextTheme, $"{op.Key}-claim-2"));
        AddExtra(s5, 4, "Cân thử thách", "The scale challenge", Styles.Balance,
            Shuffle(counted.Where(p => Size(p) >= 6), $"{op.Key}-balance-2").Take(6).Select(p => Single(p, "balance-2")));

        // Mã bài mới nối tiếp sau các bài trên trong từng chặng, không làm đổi mã bài cũ.
        AddExtra(s1, 3, "Xe buýt đến trường", "The school bus", Styles.Bus,
            Shuffle(counted.Where(p => Size(p) is >= 2 and <= 6), $"{op.Key}-bus-1").Take(5).Select(p => Single(p, "bus-1")));
        AddExtra(s2, 4, "Câu cá phép tính", "Fishing for answers", Styles.Fish,
            Shuffle(counted.Where(p => Size(p) <= 6), $"{op.Key}-fish-1").Take(5).Select(p => Single(p, "fish-1")));
        AddExtra(s3, 7, "Xe buýt đông vui", "The busy bus", Styles.Bus,
            Shuffle(counted.Where(p => Size(p) >= 5), $"{op.Key}-bus-2").Take(5).Select(p => Single(p, "bus-2")));
        AddExtra(s5, 5, "Câu cá phạm vi 10", "Fishing up to 10", Styles.Fish,
            Shuffle(counted.Where(p => Size(p) >= 6), $"{op.Key}-fish-2").Take(6).Select(p => Single(p, "fish-2")));
    }

    private static MathQuestionDefinition Board(IReadOnlyList<MathProblem> pairs, MathTheme theme) => new(null, pairs, [], theme);

    /// <summary>Câu đúng/sai: xen kẽ phép tính đúng và phép tính kết quả lệch 1–2 đơn vị.</summary>
    private static IEnumerable<MathQuestionDefinition> ClaimQuestions(IEnumerable<MathProblem> problems, Func<MathTheme> theme, string seed)
    {
        var rng = new StableRandom(seed);
        return problems.Select((p, i) =>
        {
            var offset = rng.Next(2) + 1;
            var wrong = p.Result + offset <= MaxNumber && (p.Result - offset < 1 || rng.Next(2) == 0) ? p.Result + offset : p.Result - offset;
            return new MathQuestionDefinition(p, [], [], theme()) { ClaimedResult = i % 2 == 0 ? p.Result : wrong };
        }).ToList();
    }

    /// <summary>Bảng "nhà số": 5 phép tính thuộc đúng hai kết quả khác nhau.</summary>
    private static IReadOnlyList<IReadOnlyList<MathProblem>> HouseBoards(IEnumerable<MathProblem> source, int boardCount, string seed)
    {
        var byResult = Shuffle(source, seed).GroupBy(p => p.Result).Where(g => g.Count() >= 3).OrderBy(g => g.Key).ToList();
        var boards = new List<IReadOnlyList<MathProblem>>();
        for (var i = 0; i < boardCount; i++)
        {
            var first = byResult[i % byResult.Count];
            var second = byResult[(i + byResult.Count / 2) % byResult.Count];
            boards.Add(first.Take(3).Concat(second.Take(2)).ToList());
        }
        return boards;
    }

    /// <summary>Bảng "chọc bong bóng": 3 phép tính bằng số đích, 3 phép tính khác.</summary>
    private static IEnumerable<MathQuestionDefinition> PopBoards(IEnumerable<MathProblem> source, int boardCount, string seed)
    {
        var pool = Shuffle(source, seed);
        var targets = pool.GroupBy(p => p.Result).Where(g => g.Count() >= 3).Select(g => g.Key).OrderBy(x => x).ToList();
        for (var i = 0; i < boardCount; i++)
        {
            var target = targets[(i * 2 + 1) % targets.Count];
            var board = pool.Where(p => p.Result == target).Take(3).Concat(pool.Where(p => p.Result != target).Skip(i * 3).Take(3)).ToList();
            yield return new MathQuestionDefinition(null, board, [], Themes[0]) { Target = target };
        }
    }

    /// <summary>10 phép tính tăng dần: 3 dễ (≤5), 4 vừa (6–8), 3 khó (9–10).</summary>
    private static List<MathProblem> Ramp(IReadOnlyList<MathProblem> facts, Func<MathProblem, int> size, string seed)
    {
        var easy = Shuffle(facts.Where(p => size(p) is >= 2 and <= 5 && p.A >= 1 && p.B >= 1), seed + "-e").Take(3).OrderBy(size);
        var mid = Shuffle(facts.Where(p => size(p) is >= 6 and <= 8 && p.A >= 1 && p.B >= 1), seed + "-m").Take(4).OrderBy(size);
        var hard = Shuffle(facts.Where(p => size(p) >= 9 && p.A >= 1 && p.B >= 1), seed + "-h").Take(3).OrderBy(size);
        return easy.Concat(mid).Concat(hard).ToList();
    }

    private static IReadOnlyList<IReadOnlyList<MathProblem>> BuildBoards(IEnumerable<MathProblem> source, int boardCount, string seed, int pairsPerBoard = 4)
    {
        var pool = Shuffle(source.Where(p => p.B >= 1 || p.A >= 1), seed);
        var used = new HashSet<string>();
        var boards = new List<IReadOnlyList<MathProblem>>();
        for (var i = 0; i < boardCount; i++)
        {
            var board = new List<MathProblem>();
            foreach (var p in pool.Where(p => !used.Contains(p.ToString())))
            {
                if (board.Any(x => x.Result == p.Result)) continue;
                board.Add(p);
                if (board.Count == pairsPerBoard) break;
            }
            foreach (var p in board) used.Add(p.ToString());
            boards.Add(board);
        }
        return boards;
    }

    private static MathQuestionDefinition SingleQuestion(MathProblem problem, int optionCount, MathTheme theme, string seed)
    {
        var options = BuildOptions(problem, optionCount, seed);
        return new MathQuestionDefinition(problem, [], options, theme);
    }

    private static int[] BuildOptions(MathProblem p, int count, string seed)
    {
        var answer = p.Answer;
        var candidates = new[] { answer + 1, answer - 1, answer + 2, answer - 2, p.A, p.B, p.Result, answer + 3, answer - 3, answer + 4, answer - 4 }
            .Where(x => x is >= 1 and <= MaxNumber && x != answer)
            .Distinct()
            .ToList();
        // Lấy ngẫu nhiên tất định trong nhóm gần đáp án để đáp án nhiễu hợp lý nhưng không lặp mẫu.
        var near = Shuffle(candidates.Take(4), seed + "-near");
        var picked = near.Concat(candidates.Skip(4)).Take(count - 1).Append(answer);
        return Shuffle(picked, seed + "-opts").ToArray();
    }

    private static List<List<T>> BalancedChunks<T>(IReadOnlyList<T> source, int chunkCount)
    {
        var result = new List<List<T>>();
        var start = 0;
        for (var i = 0; i < chunkCount; i++)
        {
            var size = source.Count / chunkCount + (i < source.Count % chunkCount ? 1 : 0);
            result.Add(source.Skip(start).Take(size).ToList());
            start += size;
        }
        return result;
    }

    private static List<T> Shuffle<T>(IEnumerable<T> source, string seed)
    {
        var list = source.ToList();
        var rng = new StableRandom(seed);
        for (var i = list.Count - 1; i > 0; i--)
        {
            var j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
        return list;
    }

    private static string BuildInstruction(MathOperationDefinition op, string style)
    {
        var isAdd = op.Key == "add";
        return style switch
        {
            Styles.Count => isAdd ? "Chạm vào từng đồ vật để đếm tất cả, rồi chọn số đúng." : "Đếm những đồ vật còn lại, rồi chọn số đúng.",
            Styles.Build => isAdd ? "Bấm nút thêm để bỏ đồ vật vào giỏ, rồi cho cô biết trong giỏ có mấy." : "Chạm vào đồ vật để lấy bớt ra khỏi giỏ, rồi cho cô biết còn lại mấy.",
            Styles.Choose => "Chạm vào quả bóng có kết quả đúng.",
            Styles.Drag => "Kéo thẻ số đúng thả vào ô trống (hoặc chạm vào thẻ số).",
            Styles.Missing => "Tìm số bị trốn và chạm vào số đó trên bảng số.",
            Styles.Hop => isAdd ? "Bấm nút nhảy để ếch nhảy tới, rồi chạm vào số ếch đứng." : "Bấm nút nhảy để ếch lùi lại, rồi chạm vào số ếch đứng.",
            Styles.Match => "Chạm vào một phép tính, rồi chạm vào kết quả đúng của nó.",
            Styles.Adventure => "Giải phép tính để phá chướng ngại vật và tìm đường tới kho báu.",
            Styles.Quick => "Tính nhẩm thật giỏi để tên lửa bay cao. Không cần vội, con cứ từ từ nhé!",
            Styles.Balance => "Chạm quả cân có số đúng để cân thăng bằng.",
            Styles.TrueFalse => "Nhìn phép tính, đúng thì giơ ngón cái lên, sai thì giơ ngón cái xuống.",
            Styles.Train => isAdd ? "Đếm tiếp theo từng toa tàu rồi chạm vào số đúng." : "Đếm lùi theo từng toa tàu rồi chạm vào số đúng.",
            Styles.Fish => "Chạm vào chú cá mang kết quả đúng.",
            Styles.Bus => isAdd ? "Nhìn các bạn lên xe rồi chạm vào số bạn trên xe." : "Nhìn các bạn xuống xe rồi chạm vào số bạn còn trên xe.",
            Styles.Memory => "Lật hai thẻ để tìm phép tính và kết quả của nó.",
            Styles.Sort => "Đưa từng phép tính về ngôi nhà mang kết quả của nó.",
            Styles.Pop => "Chọc hết bong bóng có phép tính bằng số cô đang giơ.",
            _ => "Con làm theo hướng dẫn của cô nhé."
        };
    }

    // ---------- Văn bản câu hỏi và gợi ý ----------

    public static string BuildPrompt(MathLessonDefinition lesson, MathQuestionDefinition q)
    {
        if (q.IsBoard)
        {
            return lesson.Style switch
            {
                Styles.Memory => "Lật thẻ tìm phép tính và kết quả của nó nhé!",
                Styles.Sort => "Đưa mỗi phép tính về đúng ngôi nhà nhé!",
                Styles.Pop => $"Chọc hết bong bóng có phép tính bằng {q.Target} nhé!",
                _ => "Ghép mỗi phép tính với kết quả đúng nhé!"
            };
        }

        var p = q.Problem!;
        var t = q.Theme;
        var isAdd = p.Op == "+";
        return lesson.Style switch
        {
            Styles.Count => isAdd
                ? $"Có {p.A} {t.Noun} và {p.B} {t.Noun}. Đếm xem có tất cả mấy {t.Noun}?"
                : $"Có {p.A} {t.Noun}, {p.B} {t.Unit} {t.TakeVerb}. Còn lại mấy {t.Noun}?",
            Styles.Build => isAdd
                ? $"Trong giỏ có {p.A} {t.Noun}. Con thêm {p.B} {t.Unit} nữa nhé!"
                : $"Trong giỏ có {p.A} {t.Noun}. Con lấy bớt {p.B} {t.Unit} nhé!",
            Styles.Hop => isAdd
                ? $"Ếch đang ở số {p.A}. Nhảy tới {p.B} bước thì ếch đến số mấy?"
                : $"Ếch đang ở số {p.A}. Nhảy lùi {p.B} bước thì ếch đến số mấy?",
            Styles.Choose when p.Missing == "result" && lesson.Stage.Order <= 2 => isAdd
                ? $"Có {p.A} {t.Noun}, {t.AddVerb} {p.B} {t.Unit}. Có tất cả mấy {t.Noun}?"
                : $"Có {p.A} {t.Noun}, {p.B} {t.Unit} {t.TakeVerb}. Còn lại mấy {t.Noun}?",
            Styles.Missing or Styles.Drag when p.Missing != "result" => $"Số nào còn thiếu: {p.ToDisplay()}",
            Styles.TrueFalse => $"{p.A} {p.DisplayOp} {p.B} = {q.ClaimedResult} — đúng hay sai?",
            Styles.Balance => $"{p.ToDisplay()} — quả cân nào làm cân thăng bằng?",
            Styles.Train => $"{p.ToDisplay()} — con đếm theo đoàn tàu nhé!",
            _ => $"{p.ToDisplay()} — bằng mấy?"
        };
    }

    public static (string Hint1, string Hint2) BuildHints(MathQuestionDefinition q)
    {
        if (q.IsBoard)
        {
            return ("Con tính từng phép tính trước, rồi tìm kết quả giống vậy nhé.",
                    "Con có thể đếm trên ngón tay hoặc đếm các chấm tròn cô vẽ giúp con.");
        }

        var p = q.Problem!;
        string Up(int from, int steps) => steps == 0 ? $"{from}" : string.Join(", ", Enumerable.Range(from + 1, steps));
        string Down(int from, int steps) => steps == 0 ? $"{from}" : string.Join(", ", Enumerable.Range(from - steps, steps).Reverse());

        if (p.Op == "+")
        {
            return p.Missing switch
            {
                "b" => ($"{p.A} cần thêm mấy nữa để được {p.Result}?",
                        $"Đếm tiếp từ {p.A} đến {p.Result}: {Up(p.A, p.B)}. Con đếm được mấy bước?"),
                "a" => ($"Số nào thêm {p.B} thì được {p.Result}?",
                        $"Con đếm lùi {p.B} bước từ {p.Result}: {Down(p.Result, p.B)}."),
                _ => ("Con đếm lại thật chậm từng cái một nhé.",
                      $"Bắt đầu từ {p.A}, đếm thêm {p.B}: {Up(p.A, p.B)}.")
            };
        }

        return p.Missing switch
        {
            "b" => ($"Từ {p.A} phải bớt đi mấy để còn {p.Result}?",
                    $"Đếm lùi từ {p.A} xuống {p.Result}: {Down(p.A, p.B)}. Con đếm được mấy bước?"),
            "a" => ($"Số nào bớt đi {p.B} thì còn {p.Result}?",
                    $"Con lấy {p.Result} đếm thêm {p.B}: {Up(p.Result, p.B)}."),
            _ => ("Con thử bớt từng cái một rồi đếm phần còn lại nhé.",
                  $"Từ {p.A}, đếm lùi {p.B} bước: {Down(p.A, p.B)}.")
        };
    }

    public static string BuildQuestionPayloadJson(MathLessonDefinition lesson, MathQuestionDefinition q)
    {
        var (hint1, hint2) = BuildHints(q);
        var t = q.Theme;
        object payload = q.IsBoard
            ? new
            {
                kind = "board",
                pairs = q.Pairs.Select(p => new { op = p.Op, a = p.A, b = p.B, result = p.Result }).ToArray(),
                target = q.Target,
                emoji = t.Emoji,
                hint1,
                hint2
            }
            : new
            {
                kind = "single",
                op = q.Problem!.Op,
                a = q.Problem.A,
                b = q.Problem.B,
                result = q.Problem.Result,
                missing = q.Problem.Missing,
                options = q.Options,
                claimed = q.ClaimedResult,
                emoji = t.Emoji,
                noun = t.Noun,
                unit = t.Unit,
                hint1,
                hint2
            };
        return JsonSerializer.Serialize(payload);
    }

    public static string BuildLessonContentJson(MathLessonDefinition lesson)
    {
        var style = GetStyle(lesson.Style);
        return JsonSerializer.Serialize(new
        {
            engine = Engine,
            version = ContentVersion,
            operation = lesson.Operation.Key,
            stageKey = lesson.Stage.Key,
            stageOrder = lesson.Stage.Order,
            stageTitle = lesson.Stage.Title,
            stageSubtitle = lesson.Stage.Subtitle,
            stageEmoji = lesson.Stage.Emoji,
            style = style.Key,
            styleName = style.Name,
            styleEmoji = style.Emoji,
            range = new { min = 0, max = MaxNumber }
        });
    }

    // ---------- Kiểm tra toàn vẹn dữ liệu ----------

    private static void Validate(IReadOnlyList<MathLessonDefinition> lessons)
    {
        var errors = new List<string>();
        var duplicateCodes = lessons.GroupBy(x => x.Code).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (duplicateCodes.Count > 0) errors.Add("Trùng mã bài: " + string.Join(", ", duplicateCodes));

        foreach (var lesson in lessons)
        {
            if (lesson.Questions.Count == 0) errors.Add($"{lesson.Code}: không có câu hỏi.");
            if (lesson.Code.Length > 80 || lesson.Title.Length > 200) errors.Add($"{lesson.Code}: mã hoặc tiêu đề quá dài.");

            foreach (var q in lesson.Questions)
            {
                var problems = q.IsBoard ? q.Pairs : [q.Problem!];
                foreach (var p in problems)
                {
                    if (!p.IsValid) errors.Add($"{lesson.Code}: phép tính ngoài phạm vi 0–10: {p}");
                    if (p.A < 1 || p.B < 1 || p.Result < 1) errors.Add($"{lesson.Code}: phép tính có số 0: {p}");
                    if (p.Missing != "result" && lesson.Style is Styles.Count or Styles.Build or Styles.Hop or Styles.Match)
                        errors.Add($"{lesson.Code}: phong cách {lesson.Style} chỉ hỗ trợ ẩn kết quả.");
                }

                if (q.IsBoard)
                {
                    if (q.Pairs.Count < 2) errors.Add($"{lesson.Code}: bảng ghép cặp quá ít.");
                    // Nhà số và chọc bong bóng cố ý có nhiều phép tính cùng kết quả; ghép cặp thì mỗi kết quả chỉ một lần.
                    var allowsSameResult = lesson.Style is Styles.Sort or Styles.Pop;
                    if (!allowsSameResult && q.Pairs.Select(x => x.Result).Distinct().Count() != q.Pairs.Count) errors.Add($"{lesson.Code}: bảng ghép cặp có kết quả trùng.");
                    if (lesson.Style == Styles.Sort && q.Pairs.Select(x => x.Result).Distinct().Count() != 2) errors.Add($"{lesson.Code}: nhà số phải có đúng 2 kết quả.");
                    if (lesson.Style == Styles.Pop && (q.Target is null || q.Pairs.Count(x => x.Result == q.Target) < 2)) errors.Add($"{lesson.Code}: chọc bong bóng thiếu phép tính đúng.");
                    continue;
                }

                var answer = q.Problem!.Answer;
                if (lesson.Style == Styles.TrueFalse)
                {
                    if (q.ClaimedResult is not (>= 1 and <= MaxNumber)) errors.Add($"{lesson.Code}: câu đúng/sai có kết quả ngoài phạm vi ở {q.Problem}.");
                    continue;
                }
                if (lesson.Style == Styles.Train && q.Problem.B < 1) errors.Add($"{lesson.Code}: đoàn tàu cần ít nhất một toa trống ở {q.Problem}.");

                var usesOptions = lesson.Style is not (Styles.Missing or Styles.Hop);
                if (usesOptions && !q.Options.Contains(answer)) errors.Add($"{lesson.Code}: đáp án {answer} không nằm trong lựa chọn của {q.Problem}.");
                if (q.Options.Distinct().Count() != q.Options.Count) errors.Add($"{lesson.Code}: lựa chọn bị trùng ở {q.Problem}.");
                if (q.Options.Any(x => x is < 1 or > MaxNumber)) errors.Add($"{lesson.Code}: lựa chọn ngoài phạm vi ở {q.Problem}.");
            }
        }

        // Bao phủ: mọi phép cộng/trừ hợp lệ 1–10 phải xuất hiện ít nhất một lần dưới dạng tìm kết quả.
        var covered = lessons.SelectMany(l => l.Questions)
            .SelectMany(q => q.IsBoard ? q.Pairs : [q.Problem!])
            .Where(p => p.Missing == "result")
            .Select(p => p.ToString())
            .ToHashSet();
        var missingFacts = AdditionFacts().Concat(SubtractionFacts()).Where(p => !covered.Contains(p.ToString())).ToList();
        if (missingFacts.Count > 0) errors.Add("Chưa bao phủ các phép tính: " + string.Join(", ", missingFacts));

        if (errors.Count > 0)
        {
            throw new InvalidOperationException("Chương trình Đảo Toán Học không hợp lệ:\n" + string.Join("\n", errors));
        }
    }

    /// <summary>Bộ sinh số ngẫu nhiên tất định (xorshift32 + FNV-1a) — không phụ thuộc string.GetHashCode.</summary>
    private sealed class StableRandom
    {
        private uint _state;

        public StableRandom(string seed)
        {
            var hash = 2166136261u;
            foreach (var ch in seed)
            {
                hash ^= ch;
                hash *= 16777619u;
            }
            _state = hash == 0 ? 0x9E3779B9u : hash;
        }

        public int Next(int maxExclusive)
        {
            _state ^= _state << 13;
            _state ^= _state >> 17;
            _state ^= _state << 5;
            return (int)(_state % (uint)maxExclusive);
        }
    }
}

public sealed record MathOperationDefinition(
    string Key,
    string Op,
    string DisplaySymbol,
    string Name,
    string TopicCode,
    bool IsAvailable,
    string Color,
    string Tagline);

public sealed record MathStyleDefinition(string Key, string Name, string Emoji, string Description);

public sealed record MathStageDefinition(string OperationKey, int Order, string Key, string Title, string Subtitle, string Emoji);

public sealed record MathTheme(string Emoji, string Noun, string Unit, string AddVerb, string TakeVerb, MathThemeEn En);

/// <summary>Từ vựng tiếng Anh của chủ đề, dùng cho Voice khi phụ huynh chọn giọng tiếng Anh.</summary>
public sealed record MathThemeEn(string Noun, string Plural, string TakeVerbOne, string TakeVerbMany)
{
    public string NounFor(int count) => count == 1 ? Noun : Plural;
}

/// <summary>Một phép tính. <paramref name="Missing"/> cho biết ô bị ẩn: "result", "a" hoặc "b".</summary>
public sealed record MathProblem(string Op, int A, int B, string Missing = "result")
{
    public int Result => Op switch
    {
        "+" => A + B,
        "-" => A - B,
        "*" => A * B,
        "/" => B == 0 ? 0 : A / B,
        _ => throw new InvalidOperationException($"Phép toán không hỗ trợ: {Op}")
    };

    public int Answer => Missing switch
    {
        "a" => A,
        "b" => B,
        _ => Result
    };

    public bool IsValid =>
        A is >= 0 and <= MathCurriculum.MaxNumber &&
        B is >= 0 and <= MathCurriculum.MaxNumber &&
        Result is >= 0 and <= MathCurriculum.MaxNumber &&
        (Op != "/" || (B != 0 && A % B == 0));

    public string DisplayOp => Op switch { "-" => "−", "*" => "×", "/" => "÷", _ => "+" };

    public string ToDisplay()
    {
        string Slot(string key, int value) => Missing == key ? "?" : value.ToString();
        return $"{Slot("a", A)} {DisplayOp} {Slot("b", B)} = {Slot("result", Result)}";
    }

    public override string ToString() => $"{A}{Op}{B}";
}

public sealed record MathQuestionDefinition(
    MathProblem? Problem,
    IReadOnlyList<MathProblem> Pairs,
    IReadOnlyList<int> Options,
    MathTheme Theme)
{
    public bool IsBoard => Pairs.Count > 0;

    /// <summary>Số đích của bảng "chọc bong bóng".</summary>
    public int? Target { get; init; }

    /// <summary>Kết quả Bíp đưa ra ở câu đúng/sai (có thể sai); có giá trị thì đáp án là "dung" hoặc "sai".</summary>
    public int? ClaimedResult { get; init; }

    /// <summary>Đáp án chuẩn hóa: một số, dãy kết quả của bảng ghép cặp, hoặc "dung"/"sai" ở câu đúng/sai.</summary>
    public string AnswerValue => IsBoard
        ? string.Join(",", Pairs.Select(p => p.Result))
        : ClaimedResult is { } claimed
            ? (claimed == Problem!.Result ? "dung" : "sai")
            : Problem!.Answer.ToString();
}

public sealed record MathLessonDefinition(
    string Code,
    MathOperationDefinition Operation,
    MathStageDefinition Stage,
    int IndexInStage,
    int SortOrder,
    string Title,
    string TitleEn,
    string Style,
    string Instruction,
    IReadOnlyList<MathQuestionDefinition> Questions)
{
    public Guid Id => MathCurriculum.StableGuid(Code);

    public Guid QuestionId(int index) => MathCurriculum.StableGuid($"{Code}:q{index:00}");

    public byte Level => (byte)Stage.Order;
}
