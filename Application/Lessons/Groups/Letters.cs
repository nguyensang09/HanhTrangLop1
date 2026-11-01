namespace HanhTrangLop1.Application.Lessons;

public static partial class LessonBank
{
    /// <summary>Chữ dễ nhầm với từng chữ in hoa (dùng làm đáp án nhiễu có chủ đích, không chọn ngẫu nhiên).</summary>
    private static readonly Dictionary<string, string[]> UpperLookAlikes = new()
    {
        ["A"] = ["Ă", "Â"], ["Ă"] = ["A", "Â"], ["Â"] = ["A", "Ă"], ["B"] = ["D", "P"], ["C"] = ["G", "O"], ["D"] = ["Đ", "B"],
        ["Đ"] = ["D", "B"], ["E"] = ["Ê", "F"], ["Ê"] = ["E", "Ô"], ["F"] = ["E", "P"], ["G"] = ["C", "Q"], ["H"] = ["K", "N"],
        ["I"] = ["L", "T"], ["J"] = ["I", "L"], ["K"] = ["H", "X"], ["L"] = ["I", "T"], ["M"] = ["N", "W"], ["N"] = ["M", "H"],
        ["O"] = ["Ô", "Ơ"], ["Ô"] = ["O", "Ơ"], ["Ơ"] = ["O", "Ô"], ["P"] = ["B", "R"], ["Q"] = ["O", "G"], ["R"] = ["P", "B"],
        ["S"] = ["Z", "X"], ["T"] = ["I", "L"], ["U"] = ["Ư", "V"], ["Ư"] = ["U", "V"], ["V"] = ["U", "Y"], ["W"] = ["M", "V"],
        ["X"] = ["K", "Y"], ["Y"] = ["V", "X"], ["Z"] = ["S", "N"]
    };

    private static readonly Dictionary<string, string[]> LowerLookAlikes = new()
    {
        ["a"] = ["ă", "â"], ["ă"] = ["a", "â"], ["â"] = ["a", "ă"], ["b"] = ["d", "p"], ["c"] = ["e", "o"], ["d"] = ["b", "đ"],
        ["đ"] = ["d", "b"], ["e"] = ["ê", "c"], ["ê"] = ["e", "ô"], ["f"] = ["t", "l"], ["g"] = ["q", "y"], ["h"] = ["n", "k"],
        ["i"] = ["l", "j"], ["j"] = ["i", "g"], ["k"] = ["h", "x"], ["l"] = ["i", "t"], ["m"] = ["n", "u"], ["n"] = ["m", "u"],
        ["o"] = ["ô", "ơ"], ["ô"] = ["o", "ơ"], ["ơ"] = ["o", "ô"], ["p"] = ["q", "b"], ["q"] = ["p", "g"], ["r"] = ["n", "v"],
        ["s"] = ["x", "z"], ["t"] = ["l", "f"], ["u"] = ["ư", "n"], ["ư"] = ["u", "n"], ["v"] = ["y", "u"], ["w"] = ["m", "v"],
        ["x"] = ["s", "k"], ["y"] = ["g", "v"], ["z"] = ["s", "x"]
    };

    /// <summary>Tiếng mở đầu bằng chữ cái (tiếng Việt): hình, tiếng đọc, tên tiếng Anh của hình.</summary>
    private static readonly Dictionary<string, (string Emoji, string Word, string En)> LetterWords = new()
    {
        ["A"] = ("👕", "áo", "a shirt"), ["Â"] = ("🫖", "ấm", "a teapot"), ["B"] = ("⚽", "bóng", "a ball"), ["C"] = ("🐟", "cá", "a fish"),
        ["D"] = ("🥥", "dừa", "a coconut"), ["Đ"] = ("💡", "đèn", "a lamp"), ["E"] = ("👶", "em bé", "a baby"), ["Ê"] = ("🐸", "ếch", "a frog"),
        ["G"] = ("🐔", "gà", "a chicken"), ["H"] = ("🌸", "hoa", "a flower"), ["K"] = ("🍦", "kem", "an ice cream"), ["L"] = ("🍃", "lá", "a leaf"),
        ["M"] = ("🐱", "mèo", "a cat"), ["N"] = ("🍄", "nấm", "a mushroom"), ["O"] = ("🐝", "ong", "a bee"), ["Ô"] = ("☂️", "ô", "an umbrella"),
        ["Ơ"] = ("🌶️", "ớt", "a chili pepper"), ["P"] = ("🔋", "pin", "a battery"), ["Q"] = ("🎁", "quà", "a gift"), ["R"] = ("🐢", "rùa", "a turtle"),
        ["S"] = ("⭐", "sao", "a star"), ["T"] = ("🍎", "táo", "an apple"), ["U"] = ("👢", "ủng", "a pair of boots"), ["V"] = ("🐘", "voi", "an elephant"),
        ["X"] = ("🚗", "xe", "a car"), ["Y"] = ("🧑‍⚕️", "y tá", "a nurse")
    };

    /// <summary>
    /// Thứ tự các phần của một chữ, đổi theo từng chữ để bé không gặp một trình tự cố định suốt 33 chữ cái.
    /// Mã bài không đổi nên tiến độ và lịch sử của bé được giữ nguyên.
    /// </summary>
    private static readonly string[][] LetterRecipes =
    [
        ["upper", "lower", "word", "pair", "match", "listen"],
        ["word", "upper", "match", "lower", "listen", "pair"],
        ["upper", "match", "lower", "listen", "word", "pair"],
        ["lower", "upper", "pair", "word", "listen", "match"]
    ];

    private static void AddLetters(LessonGroupBuilder g)
    {
        g.Topic = null;
        var letters = Lexicon.Alphabet;
        var vi = System.Globalization.CultureInfo.GetCultureInfo("vi-VN");
        for (var i = 0; i < letters.Length; i++)
        {
            var up = letters[i];
            var low = Lexicon.Lower(up);
            var sound = Lexicon.LetterSound(up);
            var en = Lexicon.LetterEn(up)["capital ".Length..];
            var n = i.ToString("00");
            if (i % 3 == 0) g.Stage = $"Chữ {string.Join(", ", letters.Skip(i).Take(3))}";

            var ups = UpperLookAlikes[up];
            var lows = LowerLookAlikes[low];

            foreach (var step in LetterRecipes[i % LetterRecipes.Length])
            {
                switch (step)
                {
                    case "upper":
                        g.Pick($"letter-combined-single-upper-{n}", $"Chữ {up} in hoa",
                            new Say($"Con tìm giúp cô chữ {up} in hoa nhé!", $"Can you find capital {en} for me?", $"Con tìm giúp cô chữ {sound} in hoa nhé!"),
                            up, [ups[0], up, ups[1]], up, target: up,
                            retry: new Say($"Chưa đúng rồi. Con nhìn kỹ hình dáng chữ {up} nhé!", $"Not quite. Look closely at the shape of {en}!", $"Chưa đúng rồi. Con nhìn kỹ hình dáng chữ {sound} nhé!"));
                        break;

                    case "lower":
                        g.Pick($"letter-combined-single-lower-{n}", $"Chữ {low} in thường",
                            new Say($"Còn đây là chữ in thường. Con tìm chữ {low} nhé!", $"Now small letters. Find small {en}!", $"Còn đây là chữ in thường. Con tìm chữ {sound} nhé!"),
                            low, [lows[0], lows[1], low], low, target: low,
                            retry: new Say($"Gần đúng rồi! Chữ {low} trông thế này nè, con thử lại nhé.", $"Almost! Look at small {en} again and try once more.", $"Gần đúng rồi! Chữ {sound} trông thế này nè, con thử lại nhé."));
                        break;

                    case "pair":
                        g.Multi($"letter-combined-multi-{n}", $"Đôi bạn {up} và {low}",
                            new Say($"Chữ {up} in hoa và chữ {low} in thường là đôi bạn. Con tìm cả hai bạn nhé!",
                                $"Capital {en} and small {en} are best friends. Find them both!",
                                $"Chữ {sound} in hoa và chữ {sound} in thường là đôi bạn. Con tìm cả hai bạn nhé!"),
                            up, [up, ups[0], low, lows[0]], [up, low]);
                        break;

                    case "listen":
                        // Chữ mẫu hiện ngay trên bảng: nghe chỉ để củng cố tên chữ, không bắt bé phân biệt chữ chỉ bằng tai.
                        g.Listen($"letter-combined-listen-upper-{n}", $"Nghe và tìm chữ {up}",
                            new Say("Con nghe cô đọc rồi chạm vào chữ đó nhé!", "Listen and tap the letter you hear!"),
                            "🎧", [ups[1], up, ups[0]], up, target: up,
                            spoken: new Say($"Chữ {sound}. Con chạm vào chữ {sound} in hoa nhé!", $"Listen: {en}. Tap capital {en}!"));
                        break;

                    case "word":
                        if (LetterWords.TryGetValue(up, out var word))
                        {
                            var capitalWord = word.Word.ToUpper(vi);
                            g.Word($"letter-combined-listen-lower-{n}", $"Chữ {up} trong tiếng {word.Word}",
                                new Say($"Đây là {word.Word}. Con tìm chữ {up} giống trong tranh nhé!", $"This is {word.En}. In Vietnamese it is {word.Word}. Find the letter {en} from the picture!", $"Đây là {word.Word}. Con tìm chữ {sound} giống trong tranh nhé!"),
                                word.Emoji, [up, ups[0], ups[1]], up, word.Emoji, capitalWord,
                                retry: new Say("Con nhìn chữ có màu trong tranh rồi tìm chữ giống như vậy nhé!", "Look at the colored letter in the picture and find one just like it!"));
                        }
                        else
                        {
                            g.Listen($"letter-combined-listen-lower-{n}", $"Nghe và tìm chữ {low}",
                                new Say("Con nghe cô đọc rồi chạm vào chữ in thường đó nhé!", "Listen and tap the small letter you hear!"),
                                "🎧", [lows[0], low, lows[1]], low, target: low,
                                spoken: new Say($"Chữ {sound}. Con chạm vào chữ {sound} in thường nhé!", $"Listen: small {en}. Tap small {en}!"));
                        }
                        break;

                    case "match":
                        var pairUps = new[] { up, ups[0], ups[1] };
                        g.Match($"letter-combined-match-{n}", $"Ghép chữ hoa với chữ thường: {up}",
                            new Say("Con nối mỗi chữ in hoa với chữ in thường của nó nhé!", "Match each capital letter with its small letter!"),
                            "🔗", pairUps.Select(x => (x, Lexicon.Lower(x))).ToArray());
                        break;
                }
            }

            g.Trace($"letter-combined-trace-upper-{n}", $"Tập tô chữ {up}",
                new Say($"Con tô chữ {up} in hoa theo nét chấm nhé!", $"Trace capital {en} along the dotted lines!", $"Con tô chữ {sound} in hoa theo nét chấm nhé!"),
                "✏️", up, 2);
            g.Trace($"letter-combined-trace-lower-{n}", $"Tập tô nét chữ {low}",
                new Say($"Con tô chữ {low} in thường theo nét chấm nhé!", $"Trace small {en} along the dotted lines!", $"Con tô chữ {sound} in thường theo nét chấm nhé!"),
                "✏️", low, 2);
        }
    }
}
