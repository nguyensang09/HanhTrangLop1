namespace HanhTrangLop1.Application.Lessons;

public static partial class LessonBank
{
    /// <summary>Nhóm hinh-dang-khong-gian: gọi tên hình, hình quanh ta, ghép hình, lớn – nhỏ, vị trí.</summary>
    private static void AddShapes(LessonGroupBuilder g)
    {
        g.Topic = "hinh-dang";
        g.Stage = "Gọi tên các hình";
        var names = new (string Slug, string Name, string En, Say Hint)[]
        {
            ("shape-single-circle", "Hình tròn", "circle", new Say("Hình tròn tròn xoe, không có góc nào nhé!", "A circle is perfectly round, with no corners!")),
            ("shape-single-square", "Hình vuông", "square", new Say("Hình vuông có bốn cạnh dài bằng nhau nhé!", "A square has four sides that are all the same length!")),
            ("shape-single-triangle", "Hình tam giác", "triangle", new Say("Hình tam giác có ba cạnh và ba góc nhọn nhé!", "A triangle has three sides and three pointy corners!")),
            ("shape-single-rectangle", "Hình chữ nhật", "rectangle", new Say("Hình chữ nhật có hai cạnh dài và hai cạnh ngắn nhé!", "A rectangle has two long sides and two short sides!")),
            ("v5-shape-oval", "Hình bầu dục", "oval", new Say("Hình bầu dục giống quả trứng, dài hơn hình tròn nhé!", "An oval looks like an egg, longer than a circle!")),
            ("v5-shape-star", "Hình ngôi sao", "star", new Say("Hình ngôi sao có năm cánh nhọn nhé!", "A star has five pointy tips!")),
            ("v5-shape-heart", "Hình trái tim", "heart", new Say("Hình trái tim giống trái tim yêu thương nè!", "A heart shape looks like a love heart!")),
            ("v5-shape-diamond", "Hình thoi", "diamond", new Say("Hình thoi giống cánh diều, đứng trên một góc nhọn nhé!", "A diamond looks like a kite standing on one corner!"))
        };
        var all = names.Select(x => x.Name).ToArray();
        for (var i = 0; i < names.Length; i++)
        {
            var s = names[i];
            var others = all.Where(x => x != s.Name).ToArray();
            var choices = new[] { others[i % others.Length], s.Name, others[(i + 3) % others.Length] };
            g.Pick(s.Slug, $"Nhận biết {s.Name.ToLowerInvariant()}",
                new Say($"Con tìm giúp cô {s.Name.ToLowerInvariant()} nhé!", $"Can you find the {s.En} for me?"),
                ShapeIcon(s.Name), choices, s.Name, retry: s.Hint);
        }
        g.Match("shape-match", "Nối hình với tên gọi",
            new Say("Con nối mỗi hình với tên của nó nhé!", "Match each shape to its name!"),
            "🔷", [("Hình vẽ tròn", "Hình tròn"), ("Hình vẽ vuông", "Hình vuông"), ("Hình vẽ tam giác", "Hình tam giác")]);
        g.Match("v5-shape-match-names-2", "Nối hình với tên gọi (tiếp)",
            new Say("Thêm ba hình nữa nè, con nối với tên của chúng nhé!", "Here are three more shapes. Match them to their names!"),
            "🔶", [("Hình vẽ chữ nhật", "Hình chữ nhật"), ("Hình vẽ ngôi sao", "Hình ngôi sao"), ("Hình vẽ trái tim", "Hình trái tim")]);

        g.Stage = "Hình dạng quanh con";
        var finds = new (string Slug, string Shape, string En, string Right, string[] Wrong)[]
        {
            ("v5-shape-find-circle", "Hình tròn", "circle", "Đồng hồ", ["Cánh cửa", "Cái lều"]),
            ("v5-shape-find-square", "Hình vuông", "square", "Xúc xắc", ["Quả trứng", "Bánh sandwich"]),
            ("v5-shape-find-triangle", "Hình tam giác", "triangle", "Miếng pizza", ["Bánh quy", "Cánh cửa"]),
            ("v5-shape-find-rectangle", "Hình chữ nhật", "rectangle", "Cánh cửa", ["Bánh quy", "Miếng pizza"]),
            ("v5-shape-find-oval", "Hình bầu dục", "oval", "Quả trứng", ["Xúc xắc", "Cái lều"]),
            ("v5-shape-find-circle-2", "Hình tròn", "circle", "Bánh quy", ["Bánh sandwich", "Quyển sách"])
        };
        foreach (var f in finds)
        {
            g.Pick(f.Slug, $"Đồ vật {f.Shape.ToLowerInvariant()}",
                new Say($"Đồ vật nào có dạng {f.Shape.ToLowerInvariant()}?", $"Which thing is shaped like a {f.En}?"),
                ShapeIcon(f.Shape), [f.Wrong[0], f.Right, f.Wrong[1]], f.Right,
                retry: new Say($"Con nhìn viền của từng đồ vật, cái nào giống {f.Shape.ToLowerInvariant()}?", $"Look at the outline of each thing. Which one looks like a {f.En}?"));
        }
        g.Match("shape-match-object", "Nối đồ vật với hình dạng",
            new Say("Con nối mỗi đồ vật với hình giống nó nhé!", "Match each thing to the shape it looks like!"),
            "🧩", [("Quả bóng", "Hình tròn"), ("Hộp quà", "Hình vuông"), ("Cái lều", "Hình tam giác")]);
        g.Match("shape-match-same", "Nối đồ vật với hình dạng (tiếp)",
            new Say("Thêm ba đồ vật nữa, con nối với hình giống nó nhé!", "Three more things. Match each one to its shape!"),
            "🧩", [("Cánh cửa", "Hình chữ nhật"), ("Quả trứng", "Hình bầu dục"), ("Ngôi sao", "Hình ngôi sao")]);
        g.Sort("shape-sort-corners", "Hình có góc và không có góc",
            new Say("Con chia các hình thành hai nhóm: có góc và không có góc nhé!", "Sort the shapes into two groups: with corners and without corners!"),
            "📐", ["Có góc", "Không có góc"], [("Hình vuông", "Có góc"), ("Hình tam giác", "Có góc"), ("Hình tròn", "Không có góc"), ("Hình bầu dục", "Không có góc")],
            retry: new Say("Góc là chỗ nhọn nhọn. Hình tròn có góc không nhỉ?", "A corner is a pointy part. Does a circle have corners?"));
        g.Sort("shape-sort-round", "Đồ vật tròn và không tròn",
            new Say("Đồ vật nào tròn, đồ vật nào không tròn? Con xếp vào hai nhóm nhé!", "Which things are round and which are not? Sort them into two groups!"),
            "⚪", ["Dạng tròn", "Không tròn"], [("Quả bóng", "Dạng tròn"), ("Đồng hồ", "Dạng tròn"), ("Xúc xắc", "Không tròn"), ("Cánh cửa", "Không tròn")]);
        g.Sort("v5-shape-sort-corners-count", "Ba góc hay bốn góc?",
            new Say("Con đếm góc của mỗi hình rồi xếp vào nhóm ba góc hoặc bốn góc nhé!", "Count the corners of each shape, then sort them: three corners or four corners!"),
            "🔺", ["Ba góc", "Bốn góc"], [("Hình tam giác", "Ba góc"), ("Miếng pizza", "Ba góc"), ("Hình vuông", "Bốn góc"), ("Hình chữ nhật", "Bốn góc")]);

        g.Topic = "ghep-hinh";
        g.Stage = "Ghép hình, tạo hình";
        g.Pick("v5-shape-build-house", "Ngôi nhà gồm những hình nào?",
            new Say("Ngôi nhà có thân hình vuông và mái là hình gì nhỉ?", "The house has a square body. What shape is its roof?"),
            "🏠", ["Hình tròn", "Hình tam giác", "Hình trái tim"], "Hình tam giác", scene: ["🟥", "🔺", "➡️", "🏠"]);
        g.Pick("v5-shape-build-two-triangles", "Hai hình tam giác ghép lại",
            new Say("Hai hình tam giác ghép sát nhau thì thành hình gì?", "Put two triangles together. What shape do they make?"),
            "🔺", ["Hình vuông", "Hình tròn", "Hình ngôi sao"], "Hình vuông", scene: ["◤", "◢", "➡️", "?"],
            retry: new Say("Con tưởng tượng gấp đôi một tờ giấy vuông theo đường chéo nhé!", "Imagine folding a square piece of paper corner to corner!"));
        g.Pick("v5-shape-build-two-squares", "Hai hình vuông xếp cạnh nhau",
            new Say("Hai hình vuông xếp cạnh nhau thành hình gì?", "Put two squares side by side. What shape do they make?"),
            "🟦", ["Hình chữ nhật", "Hình tam giác", "Hình tròn"], "Hình chữ nhật", scene: ["🟦", "🟦", "➡️", "?"]);
        g.Pick("v5-shape-build-snowman", "Người tuyết làm từ hình gì?",
            new Say("Người tuyết được ghép từ những hình gì?", "What shape is a snowman made of?"),
            "⛄", ["Hình tròn", "Hình vuông", "Hình tam giác"], "Hình tròn", scene: ["⛄"]);
        g.Pick("v5-shape-build-ice-cream", "Que kem ốc quế",
            new Say("Phần ốc quế của que kem giống hình gì?", "What shape is the cone of an ice cream?"),
            "🍦", ["Hình tam giác", "Hình vuông", "Hình trái tim"], "Hình tam giác", scene: ["🍦"]);
        g.Drag("shape-drag-above", "Lắp mái cho ngôi nhà",
            new Say("Ngôi nhà chưa có mái. Con kéo hình làm mái nhà nhé!", "The house needs a roof. Drag the right shape onto it!"),
            "🏡", ["Hình tam giác", "Hình tròn", "Hình trái tim"], "Hình tam giác", "Mái nhà");
        g.Drag("shape-drag-inside", "Bánh xe cho ô tô",
            new Say("Ô tô cần bánh xe để chạy. Con kéo hình làm bánh xe nhé!", "The car needs wheels to drive. Drag the right shape!"),
            "🚗", ["Hình tròn", "Hình vuông", "Hình tam giác"], "Hình tròn", "Bánh xe ô tô");
        g.Drag("shape-drag-below", "Cửa sổ cho ngôi nhà",
            new Say("Ngôi nhà cần một ô cửa sổ vuông. Con kéo hình đúng vào nhé!", "The house needs a square window. Drag the right shape!"),
            "🪟", ["Hình vuông", "Hình ngôi sao", "Hình bầu dục"], "Hình vuông", "Cửa sổ");

        g.Topic = "kich-thuoc";
        g.Stage = "To hay nhỏ?";
        foreach (var x in new[] { ("circle", "●", "Hình tròn", "circle"), ("square", "■", "Hình vuông", "square"), ("triangle", "▲", "Hình tam giác", "triangle") })
        {
            var low = x.Item3.ToLowerInvariant();
            g.CompareShapes($"shape-{x.Item1}-large", $"{x.Item3} nào to hơn?",
                new Say($"Con chạm vào {low} to hơn nhé!", $"Tap the bigger {x.Item4}!"), "🔍", x.Item2, x.Item3, 132, 72, "more");
            g.CompareShapes($"shape-{x.Item1}-small", $"{x.Item3} nào nhỏ hơn?",
                new Say($"Còn bây giờ, con chạm vào {low} nhỏ hơn nhé!", $"Now tap the smaller {x.Item4}!"), "🔎", x.Item2, x.Item3, 80, 138, "less");
        }
        g.Pick("v5-size-biggest-animal", "Con vật to nhất",
            new Say("Con vật nào to nhất?", "Which animal is the biggest?"), "🐘", ["Con mèo", "Con voi", "Con kiến"], "Con voi");
        g.Pick("v5-size-smallest-animal", "Con vật nhỏ nhất",
            new Say("Con vật nào nhỏ xíu nhất?", "Which animal is the tiniest?"), "🐜", ["Con kiến", "Con ngựa", "Con gấu"], "Con kiến");
        g.Pick("v5-size-tallest-animal", "Con vật cao nhất",
            new Say("Con vật nào cao nhất, với tới cả ngọn cây?", "Which animal is the tallest? It can reach the treetops!"), "🦒", ["Con thỏ", "Con lợn", "Con hươu cao cổ"], "Con hươu cao cổ");

        g.Topic = "vi-tri";
        g.Stage = "Trái, phải, ở giữa";
        var rows = new (string Slug, string[] Row, Say Prompt, string Answer)[]
        {
            ("v5-pos-middle-1", ["🐶", "🐱", "🐰"], new Say("Con vật nào đứng ở giữa?", "Which animal is in the middle?"), "Con mèo"),
            ("v5-pos-first-left", ["🚗", "🚌", "🚲"], new Say("Xe nào đứng đầu tiên, phía bên trái?", "Which vehicle is first, on the left?"), "Ô tô"),
            ("v5-pos-last-right", ["🍎", "🍌", "🍇"], new Say("Quả nào nằm cuối cùng, phía bên phải?", "Which fruit is last, on the right?"), "Quả nho"),
            ("v5-pos-middle-2", ["⚽", "🎈", "🪁"], new Say("Đồ chơi nào nằm ở giữa?", "Which toy is in the middle?"), "Bóng bay"),
            ("v5-pos-left-of", ["🐸", "🐢", "🦆"], new Say("Con vật nào đứng ngay bên trái con rùa?", "Which animal is right next to the turtle, on its left?"), "Con ếch"),
            ("v5-pos-right-of", ["🌸", "🌳", "🍄"], new Say("Cái gì nằm ngay bên phải cái cây?", "What is right next to the tree, on its right?"), "Cây nấm")
        };
        var rowLabels = new Dictionary<string, string>
        {
            ["🐶"] = "Con chó", ["🐱"] = "Con mèo", ["🐰"] = "Con thỏ", ["🚗"] = "Ô tô", ["🚌"] = "Xe buýt", ["🚲"] = "Xe đạp",
            ["🍎"] = "Quả táo", ["🍌"] = "Quả chuối", ["🍇"] = "Quả nho", ["⚽"] = "Quả bóng", ["🎈"] = "Bóng bay", ["🪁"] = "Con diều",
            ["🐸"] = "Con ếch", ["🐢"] = "Con rùa", ["🦆"] = "Con vịt", ["🌸"] = "Bông hoa", ["🌳"] = "Cái cây", ["🍄"] = "Cây nấm"
        };
        foreach (var r in rows)
        {
            var choices = r.Row.Select(x => rowLabels[x]).ToArray();
            g.Pick(r.Slug, r.Prompt.Text.TrimEnd('?'), r.Prompt, "↔️", [choices[2], choices[0], choices[1]], r.Answer, scene: r.Row,
                retry: new Say("Con nhìn hàng hình phía trên, đếm từ bên trái sang nhé!", "Look at the row of pictures and count from the left!"));
        }
    }

    private static string ShapeIcon(string shape) => shape switch
    {
        "Hình tròn" => "⚪",
        "Hình vuông" => "🟦",
        "Hình tam giác" => "🔺",
        "Hình chữ nhật" => "▬",
        "Hình bầu dục" => "🥚",
        "Hình ngôi sao" => "⭐",
        "Hình trái tim" => "❤️",
        _ => "🔷"
    };
}

public static partial class Lexicon
{
    private const string ShapeTable = """
        Hình tròn | shape:circle | circle
        Hình vuông | shape:square | square
        Hình tam giác | shape:triangle | triangle
        Hình chữ nhật | shape:rectangle | rectangle
        Hình bầu dục | shape:oval | oval
        Hình ngôi sao | shape:star | star shape
        Hình trái tim | shape:heart | heart shape
        Hình thoi | shape:diamond | diamond
        Hình vẽ tròn | pic:⚪ | this picture | hình này
        Hình vẽ vuông | pic:🟦 | this picture | hình này
        Hình vẽ tam giác | pic:🔺 | this picture | hình này
        Hình vẽ chữ nhật | pic:▬ | this picture | hình này
        Hình vẽ ngôi sao | pic:⭐ | this picture | hình này
        Hình vẽ trái tim | pic:❤️ | this picture | hình này
        Cánh cửa | 🚪 | door
        Cái lều | ⛺ | tent
        Xúc xắc | 🎲 | dice
        Bánh sandwich | 🥪 | sandwich
        Miếng pizza | 🍕 | slice of pizza
        Bánh quy | 🍪 | cookie
        Có góc | 📐 | has corners
        Không có góc | ⭕ | no corners
        Dạng tròn | 🔵 | round
        Không tròn | 🔷 | not round
        Ba góc | 🔺 | three corners
        Bốn góc | 🟧 | four corners
        Mái nhà | 🏠 | the roof
        Bánh xe ô tô | 🛞 | the car wheels
        Cửa sổ | 🪟 | the window
        """;
}
