namespace HanhTrangLop1.Application.Lessons;

/// <summary>
/// Bài ôn tập bổ sung của từng nhóm. Luôn gọi sau khi nhóm đã thêm hết bài cũ để bài mới nằm cuối đường học:
/// thứ tự và tiến độ các bài cũ của bé không đổi. Mỗi nhóm dùng thêm dạng bài nó còn ít gặp thay vì lặp lại dạng cũ.
/// </summary>
public static partial class LessonBank
{
    private const string PracticeStage = "Ôn tập vui";

    private static void AddLetterPractice(LessonGroupBuilder g)
    {
        g.Stage = PracticeStage;
        string[][] sortSets = [["A", "M", "b", "n"], ["C", "G", "e", "h"], ["D", "T", "k", "s"], ["E", "L", "o", "u"], ["H", "R", "i", "v"], ["Ô", "Ơ", "ă", "ê"]];
        for (var i = 0; i < sortSets.Length; i++)
        {
            var set = sortSets[i];
            g.Sort($"x-letter-sort-{i + 1}", $"Chữ in hoa, chữ in thường ({i + 1})",
                new Say("Con xếp chữ in hoa vào một nhóm, chữ in thường vào nhóm kia nhé!", "Put the capital letters in one group and the small letters in the other!"),
                "🔡", ["Chữ in hoa", "Chữ in thường"],
                set.Select((x, index) => (x, index < 2 ? "Chữ in hoa" : "Chữ in thường")).ToArray(),
                retry: new Say("Chữ in hoa to và cao, chữ in thường nhỏ hơn con nhé!", "Capital letters are big and tall. Small letters are smaller!"));
        }

        (string[] Row, int[] Blanks, string Decoy)[] runs =
        [
            (["A", "Ă", "Â", "B", "C"], [1, 3], "D"),
            (["B", "C", "D", "Đ", "E"], [2, 4], "Ê"),
            (["E", "Ê", "G", "H", "I"], [1, 3], "K"),
            (["N", "O", "Ô", "Ơ", "P"], [2, 3], "Q"),
            (["T", "U", "Ư", "V", "X"], [2, 4], "Y")
        ];
        for (var i = 0; i < runs.Length; i++)
        {
            var run = runs[i];
            g.OrderRow($"x-letter-order-{i + 1}", $"Bảng chữ cái: {run.Row[0]} đến {run.Row[^1]}",
                new Say("Các toa xếp theo bảng chữ cái. Con chọn chữ còn thiếu cho toa trống nhé!", "The train cars follow the alphabet. Choose the missing letters for the empty cars!"),
                "🔤", run.Row, run.Blanks, run.Decoy,
                new Say("Con đọc lại bảng chữ cái từ đầu rồi chọn lại nhé!", "Say the alphabet from the start, then try again!"));
        }

        g.Multi("x-letter-multi-hat", "Những chữ đội mũ",
            new Say("Những chữ nào có dấu mũ trên đầu? Con chọn hết nhé!", "Which letters wear a little hat? Tap them all!"),
            "🎩", ["Â", "A", "Ê", "E", "Ô"], ["Â", "Ê", "Ô"]);
        g.Multi("x-letter-multi-hook", "Những chữ có râu",
            new Say("Những chữ nào có cái râu nhỏ bên cạnh? Con chọn hết nhé!", "Which letters have a little hook on the side? Tap them all!"),
            "🪝", ["O", "Ơ", "U", "Ư"], ["Ơ", "Ư"]);

        (string Up, string[] Choices)[] friends =
        [
            ("G", ["g", "q", "y"]), ("A", ["a", "o", "d"]), ("Đ", ["d", "đ", "b"]), ("Q", ["p", "g", "q"]), ("B", ["d", "b", "p"]), ("Y", ["v", "y", "g"])
        ];
        for (var i = 0; i < friends.Length; i++)
        {
            var up = friends[i].Up;
            var sound = Lexicon.LetterSound(up);
            var en = Lexicon.LetterNameOnlyEn(up);
            g.Pick($"x-letter-friend-{i + 1}", $"Bạn in thường của chữ {up}",
                new Say($"Chữ {up} in hoa có bạn in thường là chữ nào?", $"Which small letter goes with capital {en}?", $"Chữ {sound} in hoa có bạn in thường là chữ nào?"),
                up, friends[i].Choices, Lexicon.Lower(up), scene: [up, "➡️", "?"]);
        }
    }

    private static void AddNumberPractice(LessonGroupBuilder g)
    {
        g.Topic = "thu-tu-so";
        g.Stage = PracticeStage;
        (string Slug, string Small, string Big, string[] Low, string[] High)[] sorts =
        [
            ("x-number-sort-5a", "Bé hơn 5", "Lớn hơn 5", ["1", "3"], ["7", "9"]),
            ("x-number-sort-5b", "Bé hơn 5", "Lớn hơn 5", ["2", "4"], ["6", "8"]),
            ("x-number-sort-10a", "Bé hơn 10", "Lớn hơn 10", ["3", "8"], ["12", "15"]),
            ("x-number-sort-10b", "Bé hơn 10", "Lớn hơn 10", ["5", "9"], ["11", "18"])
        ];
        foreach (var s in sorts)
        {
            var limit = s.Small == "Bé hơn 5" ? 5 : 10;
            g.Sort(s.Slug, $"Bé hơn hay lớn hơn {limit}?",
                new Say($"Con xếp các số bé hơn {limit} vào một nhóm, lớn hơn {limit} vào nhóm kia nhé!", $"Put the numbers less than {NumEn(limit)} in one group, and the numbers more than {NumEn(limit)} in the other!", $"Con xếp các số bé hơn {NumVi(limit)} vào một nhóm, lớn hơn {NumVi(limit)} vào nhóm kia nhé!"),
                "⚖️", [s.Small, s.Big],
                s.Low.Select(x => (x, s.Small)).Concat(s.High.Select(x => (x, s.Big))).ToArray());
        }

        g.Multi("x-number-multi-big", "Các số lớn hơn 5",
            new Say("Con chọn tất cả các số lớn hơn 5 nhé!", "Tap all the numbers more than five!", "Con chọn tất cả các số lớn hơn năm nhé!"),
            "🔼", ["2", "6", "4", "8", "9"], ["6", "8", "9"]);
        g.Multi("x-number-multi-small", "Các số bé hơn 3",
            new Say("Con chọn tất cả các số bé hơn 3 nhé!", "Tap all the numbers less than three!", "Con chọn tất cả các số bé hơn ba nhé!"),
            "🔽", ["1", "5", "2", "7", "4"], ["1", "2"]);

        (string[] Scene, string Answer, string[] Choices)[] gaps =
        [
            (["5", "6", "?", "8"], "7", ["4", "7", "9"]),
            (["10", "11", "?", "13"], "12", ["12", "14", "9"]),
            (["14", "?", "16", "17"], "15", ["13", "18", "15"]),
            (["?", "3", "4", "5"], "2", ["2", "6", "7"])
        ];
        for (var i = 0; i < gaps.Length; i++)
        {
            g.Pick($"x-number-gap-{i + 1}", $"Số nào trốn mất? ({i + 1})",
                new Say("Một số đã trốn khỏi dãy số. Con tìm giúp cô nhé!", "One number is hiding from the row. Can you find it?"),
                "🔍", gaps[i].Choices, gaps[i].Answer, scene: gaps[i].Scene,
                retry: new Say("Con đếm dãy số từ trái sang phải nhé!", "Count along the row from left to right!"));
        }

        (string Slug, bool Biggest, string[] Choices, string Answer)[] extremes =
        [
            ("x-number-biggest-1", true, ["3", "9", "5"], "9"),
            ("x-number-biggest-2", true, ["12", "7", "15"], "15"),
            ("x-number-smallest-1", false, ["8", "2", "6"], "2"),
            ("x-number-smallest-2", false, ["14", "11", "19"], "11")
        ];
        foreach (var e in extremes)
        {
            g.Pick(e.Slug, e.Biggest ? "Số lớn nhất" : "Số bé nhất",
                e.Biggest ? new Say("Số nào lớn nhất? Con chạm vào số đó nhé!", "Which number is the biggest? Tap it!") : new Say("Số nào bé nhất? Con chạm vào số đó nhé!", "Which number is the smallest? Tap it!"),
                e.Biggest ? "🏔️" : "🐜", e.Choices, e.Answer,
                retry: new Say("Con đếm từ một lên: số nào đếm tới sau cùng là số lớn hơn đấy!", "Count up from one: the number you reach later is the bigger one!"));
        }
    }

    private static void AddQuantityPractice(LessonGroupBuilder g)
    {
        g.Stage = PracticeStage;
        (string Slug, int Left, int Right, string Mode, int Theme)[] compares =
        [
            ("x-compare-more-8-4", 8, 4, "more", 2), ("x-compare-less-3-6", 3, 6, "less", 5),
            ("x-compare-equal-5-5", 5, 5, "equal", 7), ("x-compare-more-7-10", 7, 10, "more", 9)
        ];
        g.Topic = "so-sanh";
        foreach (var c in compares)
        {
            var t = CountThemes[c.Theme % CountThemes.Length];
            var say = c.Mode switch
            {
                "more" => new Say($"Nhóm nào có nhiều {t.Vi} hơn? Con chạm vào nhóm đó nhé!", $"Which group has more {t.EnPlural}? Tap that group!"),
                "less" => new Say($"Nhóm nào có ít {t.Vi} hơn? Con chạm vào nhóm đó nhé!", $"Which group has fewer {t.EnPlural}? Tap that group!"),
                _ => new Say($"Hai nhóm {t.Vi} này thế nào? Nếu bằng nhau thì con chạm vào nút Bằng nhau nhé!", $"Look at these two groups of {t.EnPlural}. If they are the same, tap the Same button!")
            };
            g.Compare(c.Slug, c.Mode == "equal" ? $"Hai nhóm {t.Vi} bằng nhau" : c.Mode == "more" ? $"Nhóm nào nhiều {t.Vi} hơn?" : $"Nhóm nào ít {t.Vi} hơn?",
                say, c.Mode == "equal" ? "⚖️" : c.Mode == "more" ? "⬆️" : "⬇️", t.Emoji, c.Left, c.Right, c.Mode,
                retry: new Say("Con đếm từng nhóm rồi so sánh lại nhé!", "Count each group, then compare again!"));
        }

        g.Topic = "dem-so-luong";
        foreach (var (n, theme) in new[] { (7, 1), (9, 4), (6, 8) })
        {
            var t = CountThemes[theme % CountThemes.Length];
            g.Count($"x-count-{n}", $"Đếm {t.Vi}: {n}",
                new Say($"Có bao nhiêu {t.Vi}? Con chạm vào từng {t.Vi} để đếm nhé!", $"How many {t.EnPlural} are there? Tap each one to count!"),
                t.Emoji, t.Emoji, n,
                retry: new Say("Con đếm lại thật chậm, chạm mỗi hình một lần nhé!", "Count again slowly. Tap each picture once!"));
        }

        g.Topic = "tao-so-luong";
        foreach (var (n, theme) in new[] { (4, 3), (7, 6), (9, 10) })
        {
            var t = CountThemes[theme % CountThemes.Length];
            g.Build($"x-make-{n}", $"Cô cần {n} {t.Vi}",
                new Say($"Cô cần {n} {t.Vi}. Con thêm hoặc bớt cho đủ nhé!", $"I need {NumEn(n)} {t.EnPlural}. Add or take away until you have enough!", $"Cô cần {NumVi(n)} {t.Vi}. Con thêm hoặc bớt cho đủ nhé!"),
                "🧺", t.Emoji, n,
                retry: new Say("Con đếm lại số hình trong giỏ nhé!", "Count the pictures in the basket again!"));
        }
    }

    private static void AddShapePractice(LessonGroupBuilder g)
    {
        g.Topic = "hinh-dang";
        g.Stage = PracticeStage;
        g.Multi("x-shape-multi-corners", "Những hình có góc",
            new Say("Những hình nào có góc nhọn? Con chọn hết nhé!", "Which shapes have pointy corners? Tap them all!"),
            "📐", ["Hình tròn", "Hình vuông", "Hình tam giác", "Hình bầu dục", "Hình thoi"], ["Hình vuông", "Hình tam giác", "Hình thoi"],
            retry: new Say("Hình tròn và hình bầu dục tròn trịa, không có góc đâu nhé!", "Circles and ovals are smooth, with no corners!"));
        g.Multi("x-shape-multi-round", "Những đồ vật tròn",
            new Say("Những đồ vật nào có dạng tròn? Con chọn hết nhé!", "Which things are round? Tap them all!"),
            "⚪", ["Quả bóng", "Cánh cửa", "Đồng hồ", "Xúc xắc", "Bánh quy"], ["Quả bóng", "Đồng hồ", "Bánh quy"]);

        var nextPrompt = new Say("Các hình xếp theo quy luật. Hình nào đến tiếp theo?", "The shapes follow a pattern. Which one comes next?");
        (string[] Scene, string Answer, string[] Choices)[] patterns =
        [
            (["🔺", "🟦", "🔺", "🟦", "?"], "Hình tam giác", ["Hình tròn", "Hình tam giác", "Hình vuông"]),
            (["⚪", "🔺", "⚪", "🔺", "?"], "Hình tròn", ["Hình tròn", "Hình ngôi sao", "Hình tam giác"]),
            (["⭐", "❤️", "⭐", "❤️", "?"], "Hình ngôi sao", ["Hình trái tim", "Hình vuông", "Hình ngôi sao"]),
            (["🟦", "🟦", "🔺", "🟦", "🟦", "?"], "Hình tam giác", ["Hình vuông", "Hình tam giác", "Hình tròn"])
        ];
        for (var i = 0; i < patterns.Length; i++)
        {
            g.Pick($"x-shape-pattern-{i + 1}", $"Hình tiếp theo ({i + 1})", nextPrompt, "🔁", patterns[i].Choices, patterns[i].Answer, scene: patterns[i].Scene,
                retry: new Say("Con đọc các hình từ trái sang phải, xem chúng lặp lại thế nào nhé!", "Read the shapes from left to right and see how they repeat!"));
        }

        var oddPrompt = new Say("Hình nào khác với các bạn còn lại?", "Which shape is different from the others?");
        (string[] Scene, string Answer, string[] Choices)[] odds =
        [
            (["⚪", "⚪", "🔺", "⚪"], "Hình tam giác", ["Hình tròn", "Hình tam giác", "Hình vuông"]),
            (["🟦", "❤️", "❤️", "❤️"], "Hình vuông", ["Hình trái tim", "Hình ngôi sao", "Hình vuông"])
        ];
        for (var i = 0; i < odds.Length; i++)
        {
            g.Pick($"x-shape-odd-{i + 1}", $"Hình khác loại ({i + 1})", oddPrompt, "🕵️", odds[i].Choices, odds[i].Answer, scene: odds[i].Scene);
        }

        g.CompareShapes("x-shape-diamond-large", "Hình thoi nào to hơn?",
            new Say("Con chạm vào hình thoi to hơn nhé!", "Tap the bigger diamond!"), "🔍", "◆", "Hình thoi", 76, 136, "more");
        g.CompareShapes("x-shape-star-small", "Hình ngôi sao nào nhỏ hơn?",
            new Say("Con chạm vào hình ngôi sao nhỏ hơn nhé!", "Tap the smaller star!"), "🔎", "★", "Hình ngôi sao", 138, 78, "less");
    }

    private static void AddLogicPractice(LessonGroupBuilder g)
    {
        g.Topic = "phan-loai";
        g.Stage = PracticeStage;
        var nextPrompt = new Say("Các bạn xếp hàng theo quy luật. Bạn nào đến tiếp theo?", "They are lined up in a pattern. Who comes next?");
        (string[] Scene, string Answer, string[] Choices)[] patterns =
        [
            (["🍎", "🍌", "🍎", "🍌", "?"], "Quả táo", ["Quả chuối", "Quả nho", "Quả táo"]),
            (["🐱", "🐶", "🐶", "🐱", "🐶", "?"], "Con chó", ["Con mèo", "Con chó", "Con thỏ"]),
            (["🚗", "🚌", "🚲", "🚗", "🚌", "?"], "Xe đạp", ["Ô tô", "Xe buýt", "Xe đạp"])
        ];
        for (var i = 0; i < patterns.Length; i++)
        {
            g.Pick($"x-logic-pattern-{i + 1}", $"Ai đến tiếp theo? ({i + 1})", nextPrompt, "🔁", patterns[i].Choices, patterns[i].Answer, scene: patterns[i].Scene,
                retry: new Say("Con đọc hàng từ trái sang phải, xem các bạn lặp lại thế nào nhé!", "Read the row from left to right and see how it repeats!"));
        }

        (string[] Choices, string Answer)[] odds =
        [
            (["Quả táo", "Quả chuối", "Ô tô", "Quả cam"], "Ô tô"),
            (["Con mèo", "Cái ghế", "Con chó", "Con thỏ"], "Cái ghế"),
            (["Bút chì", "Thước kẻ", "Quyển vở", "Quả chuối"], "Quả chuối")
        ];
        for (var i = 0; i < odds.Length; i++)
        {
            g.Pick($"x-logic-odd-{i + 1}", $"Bạn nào khác nhóm? ({i + 1})", LogicOddPrompt, "🕵️", odds[i].Choices, odds[i].Answer);
        }

        g.Order("x-logic-order-size", "Từ nhỏ đến to",
            new Say("Con xếp các bạn thành hàng, từ bạn bé xíu đến bạn to lớn nhé!", "Line up the animals, from the tiniest to the largest!"),
            "📏", ["Con kiến", "Con thỏ", "Con ngựa", "Con voi"]);
        g.Sort("x-logic-sort-eat", "Ăn được hay không?",
            new Say("Thứ nào ăn được, thứ nào không ăn được? Con xếp giúp cô nhé!", "Which things can we eat, and which can't we eat? Sort them for me!"),
            "🍽️", ["Ăn được", "Không ăn được"],
            [("Quả táo", "Ăn được"), ("Bánh quy", "Ăn được"), ("Bát cơm", "Ăn được"), ("Cái ghế", "Không ăn được"), ("Xe đạp", "Không ăn được"), ("Cái kéo", "Không ăn được")]);
        g.Match("x-logic-match-work", "Đồ dùng để làm gì?",
            new Say("Con nối mỗi đồ dùng với việc của nó nhé!", "Match each thing with what it is for!"),
            "🧠", [("Cái kéo", "Để cắt giấy"), ("Bút chì", "Để viết chữ"), ("Cái thìa", "Để xúc cơm")]);
    }

    private static void AddLifePractice(LessonGroupBuilder g)
    {
        g.Stage = PracticeStage;
        g.Sort("x-life-sort-food", "Món tốt cho cơ thể",
            new Say("Món nào con nên ăn nhiều, món nào chỉ nên ăn ít thôi? Con xếp nhé!", "Which foods should you eat a lot of, and which only a little? Sort them!"),
            "🥗", ["Nên ăn nhiều", "Chỉ ăn ít thôi"],
            [("Cà rốt", "Nên ăn nhiều"), ("Quả táo", "Nên ăn nhiều"), ("Bát cơm", "Nên ăn nhiều"), ("Kẹo", "Chỉ ăn ít thôi"), ("Que kem", "Chỉ ăn ít thôi"), ("Cái bánh", "Chỉ ăn ít thôi")],
            retry: new Say("Rau và quả giúp con khỏe. Đồ ngọt thì chỉ ăn một ít thôi nhé!", "Fruit and vegetables keep you strong. Sweet things are only for a little treat!"));
        g.Multi("x-life-multi-bag", "Chuẩn bị cặp đi học",
            new Say("Ngày mai đi học, con bỏ những gì vào cặp? Con chọn hết nhé!", "School is tomorrow. What goes in your school bag? Tap them all!"),
            "🎒", ["Quyển vở", "Cái nồi", "Bút chì", "Gấu bông", "Thước kẻ"], ["Quyển vở", "Bút chì", "Thước kẻ"]);
        g.Order("x-life-order-cross", "Các bước qua đường",
            new Say("Khi qua đường, con làm gì trước, làm gì sau? Con xếp lại nhé!", "When you cross the road, what do you do first, and what comes next? Put them in order!"),
            "🚸", ["Dừng lại ở mép đường", "Nhìn trái, nhìn phải", "Đi trên vạch kẻ đường"]);
        g.Match("x-life-match-wear", "Mặc gì cho hợp trời?",
            new Say("Trời thế nào thì con dùng đồ gì? Con nối cho đúng nhé!", "What do you use for each kind of weather? Match them up!"),
            "🌦️", [("Trời mưa rào", "Cái ô"), ("Trời lạnh giá", "Áo ấm"), ("Trời nắng chang chang", "Kính râm")]);
        g.Pick("x-life-feel-gift", "Được tặng quà",
            new Say("Con được bạn tặng một món quà. Con cảm thấy thế nào?", "A friend gives you a present. How do you feel?"),
            "🎁", ["Vui vẻ", "Tức giận", "Sợ hãi"], "Vui vẻ", scene: ["🎁", "➡️", "❓"]);
    }

    private static void AddLanguagePractice(LessonGroupBuilder g)
    {
        g.Stage = PracticeStage;
        g.Match("x-lang-match-opposite", "Từ trái nghĩa",
            new Say("Con nối mỗi từ với từ có nghĩa ngược lại nhé!", "Match each word with its opposite!"),
            "🔄", [("To như voi", "Nhỏ như chuột"), ("Nóng bỏng", "Lạnh buốt"), ("Nhanh như thỏ", "Chậm như sên")]);
        g.Match("x-lang-match-rhyme", "Từ cùng vần",
            new Say("Con nối hai từ có vần giống nhau nhé!", "Match the two words that rhyme!"),
            "🎵", [("Mèo", "Kéo"), ("Táo", "Cáo"), ("Mưa", "Dưa")]);
        g.Sort("x-lang-sort-day", "Ban ngày hay ban đêm?",
            new Say("Con xếp mỗi hình vào ban ngày hoặc ban đêm nhé!", "Put each picture with daytime or night-time!"),
            "🌗", ["Ban ngày", "Ban đêm"],
            [("Mặt trời", "Ban ngày"), ("Buổi trưa nắng chang chang", "Ban ngày"), ("Mặt trăng", "Ban đêm"), ("Đêm có trăng sao", "Ban đêm")]);
        g.Listen("x-lang-listen-animal", "Đố con: ai có vòi dài?",
            new Say("Con nghe câu đố rồi chạm vào câu trả lời nhé!", "Listen to the riddle, then tap the answer!"),
            "🎧", ["Con hổ", "Con voi", "Con thỏ"], "Con voi",
            spoken: new Say("Con gì thân to, tai lớn, có cái vòi thật dài?", "Which animal has a big body, big ears and a very long trunk?"));
        g.Listen("x-lang-listen-thing", "Đố con: cái gì để viết?",
            new Say("Con nghe câu đố rồi chạm vào câu trả lời nhé!", "Listen to the riddle, then tap the answer!"),
            "🎧", ["Cái kéo", "Cái thìa", "Bút chì"], "Bút chì",
            spoken: new Say("Cái gì con cầm trên tay để viết chữ vào vở?", "What do you hold in your hand to write in your notebook?"));
    }

    private static void AddMemoryPractice(LessonGroupBuilder g)
    {
        g.Stage = PracticeStage;
        (string Slug, string[] Scene, string Prompt, string En, string Answer, string[] Choices)[] seen =
        [
            ("x-mem-row-1", ["🍎", "🐱", "🚗"], "Bạn nào đứng đầu hàng, bên trái?", "Who is first in the row, on the left?", "Quả táo", ["Con mèo", "Quả táo", "Ô tô"]),
            ("x-mem-row-2", ["🐸", "⭐", "🎈"], "Thứ gì nằm cuối hàng, bên phải?", "What is last in the row, on the right?", "Bóng bay", ["Bóng bay", "Con ếch", "Ngôi sao"]),
            ("x-mem-row-3", ["🐝", "🌸", "🍄"], "Thứ gì nằm ở giữa hàng?", "What is in the middle of the row?", "Bông hoa", ["Cây nấm", "Con ong", "Bông hoa"])
        ];
        foreach (var s in seen)
        {
            g.Pick(s.Slug, s.Prompt.TrimEnd('?'), new Say(s.Prompt, s.En), "👀", s.Choices, s.Answer, scene: s.Scene);
        }
        g.Match("x-mem-match-pairs", "Nhớ đôi bạn",
            new Say("Con nhìn thật kỹ rồi tìm đôi cho từng bạn nhé!", "Look carefully and find the partner for each one!"),
            "🧠", [("Con thỏ", "Cà rốt"), ("Con khỉ", "Quả chuối"), ("Con gấu", "Mật ong")]);
        g.Order("x-mem-order-day", "Một ngày của con",
            new Say("Con nhớ lại và xếp các buổi trong ngày theo thứ tự nhé!", "Think back and put the parts of the day in order!"),
            "🕰️", ["Sáng sớm", "Giữa trưa", "Chiều tà", "Trời tối"]);
    }

    private static void AddMotorPractice(LessonGroupBuilder g)
    {
        g.Stage = PracticeStage;
        g.Order("x-motor-order-card", "Làm thiệp từng bước",
            new Say("Con xếp các bước làm thiệp theo đúng thứ tự nhé!", "Put the steps for making a card in order!"),
            "💌", ["Vẽ bông hoa lên giấy", "Cắt theo nét vẽ", "Bôi hồ mặt sau", "Dán lên tấm thiệp"]);
        g.Sort("x-motor-sort-tools", "Đồ thủ công và đồ nhà bếp",
            new Say("Đồ nào dùng làm thủ công, đồ nào để trong bếp? Con xếp giúp cô nhé!", "Which things are for crafts, and which go in the kitchen? Sort them for me!"),
            "🧰", ["Hộp đồ thủ công", "Để trong bếp"],
            [("Cái kéo", "Hộp đồ thủ công"), ("Lọ hồ dán", "Hộp đồ thủ công"), ("Cái nồi", "Để trong bếp"), ("Cái bát", "Để trong bếp")]);
        g.Multi("x-motor-multi-fine", "Việc của đôi bàn tay khéo",
            new Say("Việc nào con phải dùng đôi tay thật khéo? Con chọn hết nhé!", "Which jobs need really careful hands? Tap them all!"),
            "🖐️", ["Gấp đôi tờ giấy", "Xem điện thoại", "Xâu vòng hạt", "Ngủ cả ngày", "Tô màu tranh"], ["Gấp đôi tờ giấy", "Xâu vòng hạt", "Tô màu tranh"]);
        g.Match("x-motor-match-tool", "Đồ dùng và việc làm",
            new Say("Con nối mỗi đồ dùng với việc nó giúp con làm nhé!", "Match each tool with the job it helps you do!"),
            "✂️", [("Đôi đũa", "Gắp sợi mì"), ("Cái thìa", "Múc canh"), ("Bàn chải đánh răng", "Chải răng trắng sạch")]);
    }

    private static void AddWorldPractice(LessonGroupBuilder g)
    {
        g.Stage = PracticeStage;
        g.Match("x-world-match-baby", "Mẹ và con",
            new Say("Con nối mỗi bạn mẹ với em bé của mình nhé!", "Match each mother with her baby!"),
            "🐣", [("Mèo mẹ", "Mèo con"), ("Chó mẹ", "Chó con"), ("Lợn mẹ", "Lợn con")]);
        g.Sort("x-world-sort-land-water", "Trên cạn hay dưới nước?",
            new Say("Bạn nào sống trên cạn, bạn nào sống dưới nước? Con xếp nhé!", "Who lives on land, and who lives in the water? Sort them!"),
            "🏞️", ["Sống trên cạn", "Sống dưới nước"],
            [("Con voi", "Sống trên cạn"), ("Con thỏ", "Sống trên cạn"), ("Con cá", "Sống dưới nước"), ("Cá voi", "Sống dưới nước")]);
        g.Multi("x-world-multi-sky", "Trên bầu trời có gì?",
            new Say("Con chọn những thứ con nhìn thấy trên bầu trời nhé!", "Tap the things you can see in the sky!"),
            "🌤️", ["Đám mây", "Cái ghế", "Mặt trời", "Con cua", "Cầu vồng"], ["Đám mây", "Mặt trời", "Cầu vồng"]);
        g.Order("x-world-order-grow", "Cây lớn lên thế nào?",
            new Say("Con xếp các bước cây lớn lên theo thứ tự nhé!", "Put the steps of a growing plant in order!"),
            "🌱", ["Hạt giống", "Mầm cây", "Cái cây"]);
        g.Pick("x-world-vehicle-sky", "Xe nào bay trên trời?",
            new Say("Trong ba loại xe này, loại nào bay được trên trời?", "Which one of these can fly in the sky?"),
            "✈️", ["Xe đạp", "Máy bay", "Tàu thủy"], "Máy bay");
        g.Pick("x-world-vehicle-sea", "Xe nào đi trên biển?",
            new Say("Trong ba loại xe này, loại nào đi được trên biển?", "Which one of these can travel on the sea?"),
            "🌊", ["Tàu thủy", "Xe buýt", "Xe máy"], "Tàu thủy");
    }
}

public static partial class Lexicon
{
    /// <summary>Tên chữ bằng tiếng Anh, không kèm "capital" hay "small".</summary>
    public static string LetterNameOnlyEn(string letter) => LetterEn(letter.ToUpper(Vi))["capital ".Length..];

    private const string PracticeTable = """
        Chữ in hoa | 🔠 | capital letters
        Chữ in thường | 🔡 | small letters
        Bé hơn 5 | 🔽 | less than five | Bé hơn năm
        Lớn hơn 5 | 🔼 | more than five | Lớn hơn năm
        Bé hơn 10 | 🔽 | less than ten | Bé hơn mười
        Lớn hơn 10 | 🔼 | more than ten | Lớn hơn mười
        Ăn được | 😋 | we can eat it
        Không ăn được | 🚫 | we can't eat it
        Nên ăn nhiều | 🥗 | eat plenty
        Chỉ ăn ít thôi | 🤏 | just a little
        Dầu gội đầu | 🧴 | shampoo
        Mái tóc | 💇 | hair
        Hoa đào | 🌸 | peach blossoms
        Gỗ làm bàn ghế | 🪵 | wood for tables and chairs
        """;
}
