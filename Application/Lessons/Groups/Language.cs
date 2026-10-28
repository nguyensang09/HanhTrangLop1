namespace HanhTrangLop1.Application.Lessons;

public static partial class LessonBank
{
    /// <summary>Nhóm ngon-ngu: vốn từ, nghe hiểu, từ trái nghĩa, tiếng kêu con vật, đọc hiểu truyện tranh, vần và chữ.</summary>
    private static void AddLanguage(LessonGroupBuilder g)
    {
        static string Ill(string slug) => $"/images/lessons/doc-hieu/ill-{slug}.jpg";

        var listenPrompt = new Say("Con nghe cô nói rồi chạm vào hình đúng nhé!", "Listen to me, then tap the right picture!");
        var riddlePrompt = new Say("Con nghe cô đố rồi chạm vào hình đúng nhé!", "Listen to my riddle, then tap the right picture!");
        var soundPrompt = new Say("Con nghe tiếng kêu rồi đoán xem đó là con gì nhé!", "Listen to the sound and guess the animal!");

        // ───────────── Chặng 1: Từ vựng quanh em ─────────────
        g.Stage = "Từ vựng quanh em";
        g.Topic = "von-tu";

        g.Listen("v5-lang-word-spoon", "Nghe và tìm: cái thìa", listenPrompt, "🥄",
            ["Cái bát", "Cái thìa", "Cái đĩa"], "Cái thìa",
            spoken: new Say("Cái thìa. Con tìm cái thìa cho cô nhé!", "Spoon. Can you find the spoon for me?"),
            retry: new Say("Cái thìa dùng để xúc cơm, xúc canh đấy con!", "A spoon is what we scoop our food with!"));

        g.Multi("v5-lang-kitchen", "Đồ dùng trong bếp",
            new Say("Con tìm những đồ dùng ở trong bếp nhé!", "Find all the things that belong in the kitchen!"),
            "🍲", ["Cái nồi", "Xe đạp", "Cái bát", "Quả bóng", "Cái thìa"], ["Cái nồi", "Cái bát", "Cái thìa"],
            retry: new Say("Đồ dùng trong bếp là đồ để nấu và để ăn con nhé!", "Kitchen things are for cooking and eating!"));

        g.Match("language-match-use", "Đồ vật dùng để làm gì?",
            new Say("Mỗi đồ vật dùng để làm gì? Con nối cho đúng nhé!", "What is each thing used for? Match them up!"),
            "✂️", [("Cái kéo", "Để cắt giấy"), ("Bút chì", "Để viết chữ"), ("Cái thìa", "Để xúc cơm")]);

        g.Listen("v5-lang-action-sleep", "Nghe và tìm: đang ngủ", listenPrompt, "😴",
            ["Đang chạy", "Đang ăn", "Đang ngủ"], "Đang ngủ",
            spoken: new Say("Đang ngủ. Con tìm bạn nhỏ đang ngủ nhé!", "Sleeping. Find the one who is sleeping!"),
            retry: new Say("Bạn đang ngủ thì nhắm mắt lại, khò khò đó con!", "When you're asleep, your eyes are closed. Zzz!"));

        g.Sort("v5-lang-sort-wear-eat", "Để mặc hay để ăn?",
            new Say("Thứ nào để mặc, thứ nào để ăn? Con xếp vào đúng nhóm nhé!", "Which things do we wear, and which do we eat? Sort them!"),
            "👗", ["Đồ để mặc", "Đồ để ăn"],
            [("Cái áo", "Đồ để mặc"), ("Cái váy", "Đồ để mặc"), ("Đôi tất", "Đồ để mặc"), ("Bánh mì", "Đồ để ăn"), ("Quả chuối", "Đồ để ăn")]);

        g.Multi("v5-lang-can-fly", "Con vật biết bay",
            new Say("Con tìm những con vật biết bay trên trời nhé!", "Find the animals that can fly!"),
            "🦋", ["Con chim", "Con cá", "Con bướm", "Con mèo", "Con ong"], ["Con chim", "Con bướm", "Con ong"],
            retry: new Say("Con vật biết bay thì có đôi cánh đấy!", "Animals that fly have wings!"));

        // ───────────── Chặng 2: Nghe cô đố ─────────────
        g.Stage = "Nghe cô đố";
        g.Topic = "nghe-hieu";

        g.Listen("language-listen-cat", "Đố con: con gì kêu meo meo?", riddlePrompt, "🐱",
            ["Con chó", "Con mèo", "Con gà"], "Con mèo",
            spoken: new Say("Con gì kêu meo meo, thích nằm sưởi nắng và bắt chuột?", "Which animal says meow, loves lying in the sun and chases mice?"));

        g.Listen("language-listen-apple", "Đố con: quả gì giòn rụm?", riddlePrompt, "🍎",
            ["Quả chuối", "Quả nho", "Quả táo"], "Quả táo",
            spoken: new Say("Quả gì tròn tròn, vỏ màu đỏ, cắn vào nghe giòn rụm?", "Which fruit is round and red, and goes crunch when you bite it?"),
            retry: new Say("Quả này tròn và màu đỏ con nhé!", "This fruit is round and red!"));

        g.Listen("v5-lang-sound-dog", "Ai kêu gâu gâu?", soundPrompt, "🐶",
            ["Con mèo", "Con chó", "Con lợn"], "Con chó",
            spoken: new Say("Gâu gâu! Con vật nào kêu như thế?", "Woof woof! Which animal says that?"));

        g.Listen("v5-lang-riddle-elephant", "Đố con: con gì có vòi?", riddlePrompt, "🐘",
            ["Con hươu cao cổ", "Con voi", "Con khỉ"], "Con voi",
            spoken: new Say("Con gì to ơi là to, có cái vòi thật dài?", "Which animal is huge and has a really long trunk?"));

        g.Listen("v5-lang-sound-rooster", "Ai gáy ò ó o?", soundPrompt, "🐔",
            ["Con vịt", "Con chim", "Con gà"], "Con gà",
            spoken: new Say("Ò ó o! Con vật nào gáy vang mỗi sáng?", "Cock-a-doodle-doo! Which animal crows every morning?"));

        g.Listen("v5-lang-riddle-umbrella", "Đố con: cái gì che mưa?", riddlePrompt, "☂️",
            ["Cái ghế", "Quả bóng", "Cái ô"], "Cái ô",
            spoken: new Say("Cái gì trời mưa thì mở ra, che trên đầu cho con khỏi ướt?", "What do you open when it rains, to keep your head dry?"));

        g.Listen("v5-lang-sound-duck", "Ai kêu cạp cạp?", soundPrompt, "🦆",
            ["Con gà", "Con vịt", "Con ếch"], "Con vịt",
            spoken: new Say("Cạp cạp! Con vật nào vừa bơi vừa kêu như thế?", "Quack quack! Which animal swims and says that?"));

        g.Listen("v5-lang-riddle-moon", "Đố con: cái gì sáng ban đêm?", riddlePrompt, "🌙",
            ["Mặt trời", "Đám mây", "Mặt trăng"], "Mặt trăng",
            spoken: new Say("Cái gì sáng trên trời ban đêm, có hôm tròn, có hôm cong như quả chuối?", "What shines in the night sky, sometimes round and sometimes curved like a banana?"),
            retry: new Say("Mặt trời chỉ chiếu sáng ban ngày thôi con ạ!", "The sun only shines in the daytime!"));

        g.Listen("v5-lang-sound-frog", "Ai kêu ộp ộp?", soundPrompt, "🐸",
            ["Con cá", "Con ếch", "Con rùa"], "Con ếch",
            spoken: new Say("Ộp ộp! Con vật nào nhảy bên bờ ao kêu như thế?", "Ribbit ribbit! Which animal hops by the pond and says that?"));

        g.Listen("v5-lang-riddle-penguin", "Đố con: chim gì không biết bay?", riddlePrompt, "🐧",
            ["Con chim", "Chim cánh cụt", "Con vịt"], "Chim cánh cụt",
            spoken: new Say("Con gì có cánh nhưng không biết bay, sống ở nơi băng tuyết lạnh giá?", "Which bird has wings but can't fly, and lives where it's icy cold?"),
            retry: new Say("Bạn này đi lạch bạch trên băng tuyết đấy con!", "This bird waddles around on the ice!"));

        g.Match("v5-lang-match-sounds", "Ai kêu thế nào?",
            new Say("Mỗi con vật kêu một kiểu. Con nối con vật với tiếng kêu nhé!", "Every animal makes its own sound. Match each animal to its sound!"),
            "🔊", [("Con mèo", "Meo meo"), ("Con lợn", "Ụt ịt"), ("Con cừu", "Be be")]);

        // ───────────── Chặng 3: Truyện rừng xanh ─────────────
        g.Stage = "Truyện rừng xanh";
        g.Topic = "doc-hieu";

        var bear = new Say(
            "Gấu nâu đi dạo trong rừng, ngửi thấy mùi mật ong thơm phức. Tổ ong treo trên cành cây cao. Gấu chỉ lấy một ít mật vào hũ nhỏ, để phần còn lại cho các bạn ong.",
            "Brown Bear is walking in the forest when he smells sweet honey. The beehive hangs high up in a tree. Bear takes just a little honey in his small pot and leaves the rest for the bees.");
        g.Story("v5-lang-gau-smell", "Gấu nâu và mật ong", bear,
            new Say("Gấu nâu ngửi thấy mùi gì?", "What does Brown Bear smell?"),
            "🍯", ["Bông hoa", "Mật ong", "Quả táo"], "Mật ong", Ill("gau-mat-ong"),
            retry: new Say("Con nhìn cái hũ gấu đang ôm, bên trong vàng óng là gì nào?", "Look at the pot Bear is holding. What's that golden stuff inside?"));
        g.Story("v5-lang-gau-why", "Gấu nâu biết nhường", bear,
            new Say("Vì sao gấu chỉ lấy một ít mật ong?", "Why does Bear take only a little honey?"),
            "🐝", ["Vì mật ong không ngon", "Để phần cho các bạn ong", "Vì gấu sắp đi ngủ"], "Để phần cho các bạn ong", Ill("gau-mat-ong"),
            retry: new Say("Con nghe lại cuối truyện: gấu để phần còn lại cho ai?", "Listen to the end again: who does Bear leave the rest for?"));

        var fox = new Say(
            "Mùa đông, tuyết rơi trắng rừng. Cáo nhỏ thấy chiếc khăn xanh trên cành cây liền quàng vào cổ. Sóc nâu chạy đến: “Khăn của tớ bị rơi đấy!”. Cáo vội trả lại khăn cho sóc.",
            "It's winter and snow covers the forest. Little Fox finds a blue scarf on a branch and wraps it around his neck. Then Squirrel runs up: \"That's my scarf! I dropped it!\" Fox gives it back right away.");
        g.Story("v5-lang-cao-find", "Cáo nhỏ nhặt được gì?", fox,
            new Say("Cáo nhỏ thấy gì trên cành cây?", "What does Little Fox find on the branch?"),
            "🧣", ["Găng tay", "Khăn quàng", "Mũ len"], "Khăn quàng", Ill("cao-chiec-khan"),
            retry: new Say("Con nhìn thứ màu xanh đang treo trên cành cây nhé!", "Look at the blue thing hanging on the branch!"));
        g.Story("v5-lang-cao-owner", "Chiếc khăn của ai?", fox,
            new Say("Chiếc khăn xanh là của ai?", "Whose blue scarf is it?"),
            "🐿️", ["Con thỏ", "Con gấu", "Con sóc"], "Con sóc", Ill("cao-chiec-khan"),
            retry: new Say("Ai chạy đến nói “Khăn của tớ bị rơi đấy”?", "Who ran up and said, \"That's my scarf\"?"));
        g.Story("v5-lang-cao-lesson", "Nhặt được thì trả lại", fox,
            new Say("Nhặt được đồ của bạn, mình nên làm gì?", "If you find something that belongs to someone else, what should you do?"),
            "🤲", ["Giữ làm của mình", "Trả lại cho bạn", "Vứt đi"], "Trả lại cho bạn", Ill("cao-chiec-khan"),
            retry: new Say("Cáo nhỏ đã làm gì khi biết đó là khăn của sóc?", "What did Little Fox do when he found out it was Squirrel's scarf?"));

        var squirrel = new Say(
            "Mùa thu, lá vàng rơi đầy rừng. Sóc nâu nhặt từng hạt dẻ bỏ vào giỏ. Sóc cất giỏ hạt dẻ thật kỹ để dành ăn khi mùa đông đến.",
            "It's autumn and golden leaves cover the forest. Squirrel picks up chestnuts one by one and puts them in a basket. Then he hides the basket safely to eat when winter comes.");
        g.Story("ngon-ngu-story-soc-hat-de", "Chú sóc và hạt dẻ", squirrel,
            new Say("Sóc nâu nhặt gì bỏ vào giỏ?", "What does Squirrel put in his basket?"),
            "🌰", ["Cây nấm", "Hạt dẻ", "Quả táo"], "Hạt dẻ", Ill("soc-hat-de"),
            retry: new Say("Con nhìn vào chiếc giỏ trong tranh, bên trong có gì nào?", "Look inside the basket in the picture. What's in there?"));
        g.Story("v5-lang-soc-why", "Sóc để dành hạt dẻ", squirrel,
            new Say("Sóc cất hạt dẻ để làm gì?", "Why does Squirrel save the chestnuts?"),
            "❄️", ["Để ném chơi với bạn", "Để trồng thành cây", "Để dành ăn mùa đông"], "Để dành ăn mùa đông", Ill("soc-hat-de"),
            retry: new Say("Mùa đông lạnh, rừng ít thức ăn lắm. Sóc lo xa đấy con!", "In cold winter there's little food, so Squirrel plans ahead!"));

        g.Topic = "ke-chuyen";
        g.Order("language-order-story", "Kể lại chuyện chú sóc",
            new Say("Con xếp lại những việc sóc nâu làm theo đúng thứ tự nhé!", "Put the things Squirrel does in the right order!"),
            "📖", ["Nhặt hạt dẻ", "Bỏ vào giỏ", "Cất cho mùa đông"],
            retry: new Say("Sóc phải nhặt hạt dẻ trước, rồi mới bỏ vào giỏ được con nhé!", "Squirrel has to pick up the chestnuts before he can put them in the basket!"));

        g.Match("v5-lang-match-forest", "Ai thích gì nhất?",
            new Say("Các bạn trong rừng thích món gì nhất? Con nối cho đúng nhé!", "What does each forest friend love most? Match them up!"),
            "🌲", [("Con gấu", "Mật ong"), ("Con sóc", "Hạt dẻ"), ("Con nhím", "Quả táo")]);

        // ───────────── Chặng 4: Từ trái nghĩa ─────────────
        g.Stage = "Từ trái nghĩa";
        g.Topic = "von-tu";

        g.Match("v5-lang-opposites-1", "Đôi bạn trái ngược",
            new Say("Mỗi từ có một bạn ngược lại với nó. Con nối các đôi bạn nhé!", "Every word has an opposite. Match the opposite pairs!"),
            "🔄", [("To như voi", "Nhỏ như chuột"), ("Nóng bỏng", "Lạnh buốt"), ("Nhanh như thỏ", "Chậm như sên")]);

        g.Pick("v5-lang-opposite-slow", "Nhanh và chậm",
            new Say("Thỏ chạy nhanh. Còn ốc sên đi thế nào nhỉ?", "The rabbit runs fast. How does the snail move?"),
            "🐌", ["Nhanh như thỏ", "Chậm như sên", "To như voi"], "Chậm như sên", scene: ["🐇", "🐌"],
            retry: new Say("Ốc sên bò từng chút, từng chút một đấy con!", "A snail crawls little by little!"));

        g.Sort("v5-lang-sort-hot-cold", "Nóng hay lạnh?",
            new Say("Thứ nào nóng, thứ nào lạnh? Con xếp vào đúng nhóm nhé!", "Which things are hot, and which are cold? Sort them!"),
            "🌡️", ["Nóng bỏng", "Lạnh buốt"],
            [("Cốc trà nóng", "Nóng bỏng"), ("Ngọn lửa", "Nóng bỏng"), ("Viên đá lạnh", "Lạnh buốt"), ("Người tuyết", "Lạnh buốt")]);

        g.Multi("v5-lang-fast-animals", "Con vật chạy nhanh",
            new Say("Con tìm những con vật chạy thật nhanh nhé!", "Find the animals that run really fast!"),
            "🐎", ["Con ngựa", "Con rùa", "Con thỏ", "Con ốc sên", "Con hổ"], ["Con ngựa", "Con thỏ", "Con hổ"],
            retry: new Say("Rùa và ốc sên đi chậm lắm con ạ!", "Turtles and snails are very slow!"));

        g.Match("v5-lang-opposites-2", "Thêm những đôi trái ngược",
            new Say("Con tìm bạn ngược lại cho từng từ nhé!", "Find the opposite for each word!"),
            "🌗", [("Cao như hươu", "Thấp như cây nấm"), ("Ban ngày", "Ban đêm"), ("Sạch sẽ", "Lấm bẩn")]);

        g.Pick("v5-lang-opposite-small", "To và nhỏ",
            new Say("Bạn voi to ơi là to. Con tìm một bạn nhỏ xíu nhé!", "The elephant is enormous. Find a tiny little animal!"),
            "🐜", ["Con hươu cao cổ", "Con kiến", "Con ngựa"], "Con kiến", scene: ["🐘"],
            retry: new Say("Bạn nhỏ xíu này bé hơn cả hạt gạo đấy!", "This little one is even smaller than a grain of rice!"));

        // ───────────── Chặng 5: Bạn bè giúp nhau ─────────────
        g.Stage = "Bạn bè giúp nhau";
        g.Topic = "doc-hieu";

        var hedgehog = new Say(
            "Chiều nay, nhím nhỏ thấy một quả táo đỏ dưới gốc cây. Quả táo to quá, nhím đẩy từ từ, mệt thì nghỉ một lát rồi đẩy tiếp. Nhím rửa táo ở bờ suối rồi mang về hang.",
            "This afternoon, Little Hedgehog finds a red apple under a tree. It's so big that he pushes it slowly, resting when he gets tired. He washes it in the stream and takes it home to his burrow.");
        g.Story("ngon-ngu-story-nhim-qua-tao", "Chú nhím và quả táo", hedgehog,
            new Say("Nhím thấy quả gì dưới gốc cây?", "What fruit does Hedgehog find under the tree?"),
            "🦔", ["Quả lê", "Quả chuối", "Quả táo"], "Quả táo", Ill("nhim-qua-tao"),
            retry: new Say("Con nhìn quả màu đỏ to tướng bên cạnh nhím nhé!", "Look at the big red fruit next to Hedgehog!"));
        g.Story("v5-lang-nhim-wash", "Nhím rửa táo", hedgehog,
            new Say("Nhím rửa quả táo ở đâu?", "Where does Hedgehog wash the apple?"),
            "💧", ["Trong bồn tắm", "Ở bờ suối", "Dưới vòi nước"], "Ở bờ suối", Ill("nhim-qua-tao"),
            retry: new Say("Con nhìn dòng nước xanh chảy róc rách trong tranh nhé!", "Look at the blue water flowing in the picture!"));

        g.Topic = "ke-chuyen";
        g.Order("v5-lang-order-nhim", "Kể lại chuyện chú nhím",
            new Say("Con xếp lại chuyện chú nhím theo đúng thứ tự nhé!", "Put Hedgehog's story in the right order!"),
            "📖", ["Thấy quả táo", "Đẩy táo từ từ", "Rửa táo ở suối", "Mang táo về hang"],
            retry: new Say("Nhím phải thấy quả táo trước, rồi mới đẩy đi được con nhé!", "Hedgehog has to find the apple before he can push it!"));

        g.Topic = "doc-hieu";
        var turtle = new Say(
            "Rùa nhỏ muốn sang bờ bên kia ăn lá non, nhưng nước suối chảy xiết quá. Thỏ trắng buộc cành cây thành một chiếc bè. Thỏ chèo bè đưa rùa qua suối, rùa cảm ơn thỏ rối rít.",
            "Little Turtle wants to cross the stream to eat fresh leaves, but the water is too fast. White Rabbit ties some branches together into a raft. Rabbit paddles Turtle across, and Turtle says thank you again and again.");
        g.Story("v5-lang-rua-helper", "Ai giúp rùa nhỏ?", turtle,
            new Say("Ai đã giúp rùa nhỏ qua suối?", "Who helps Little Turtle cross the stream?"),
            "🐢", ["Con sóc", "Con thỏ", "Con gà"], "Con thỏ", Ill("rua-dong-suoi"),
            retry: new Say("Con nhìn bạn tai dài đang cầm mái chèo nhé!", "Look at the friend with long ears holding the paddle!"));
        g.Story("v5-lang-rua-raft", "Qua suối bằng gì?", turtle,
            new Say("Rùa và thỏ qua suối bằng gì?", "How do Turtle and Rabbit get across the stream?"),
            "🪵", ["Thuyền buồm", "Chiếc bè gỗ", "Xe đạp"], "Chiếc bè gỗ", Ill("rua-dong-suoi"),
            retry: new Say("Thỏ đã buộc những cành cây lại với nhau thành gì nhỉ?", "Rabbit tied branches together. What did he make?"));

        var bird = new Say(
            "Trong tổ có ba chú chim non. Một chú dang cánh tập bay nhưng rơi xuống cành thấp. Mẹ chim bảo: “Con cứ tập mỗi ngày, con sẽ bay giỏi!”. Chú chim non cười và lại tập tiếp.",
            "There are three baby birds in the nest. One spreads its wings to fly, but drops onto a low branch. Mother Bird says, \"Keep practising every day and you'll fly so well!\" The little bird smiles and tries again.");
        g.Story("v5-lang-chim-count", "Tổ chim non", bird,
            new Say("Trong tổ có mấy chú chim non?", "How many baby birds are in the nest?"),
            "🪺", ["2", "3", "4"], "3", Ill("chim-non-tap-bay"),
            retry: new Say("Con đếm các chú chim đang ngồi trong tổ nhé!", "Count the birds sitting in the nest!"));
        g.Story("v5-lang-chim-try", "Chim non không bỏ cuộc", bird,
            new Say("Bị rơi xuống cành thấp, chim non làm gì?", "After dropping onto the low branch, what does the baby bird do?"),
            "💪", ["Bỏ cuộc không tập nữa", "Khóc mãi không thôi", "Cố gắng tập tiếp"], "Cố gắng tập tiếp", Ill("chim-non-tap-bay"),
            retry: new Say("Mẹ chim dặn gì? Chú chim non có nghe lời mẹ không?", "What did Mother Bird say? Did the little bird listen?"));

        var umbrella = new Say(
            "Sáng nay trời mưa. Bé Na mặc áo mưa vàng, cầm ô vàng đi học. Dưới gốc cây, Na thấy một chú mèo con ướt lạnh. Na ngồi xuống, che ô cho mèo con.",
            "It's raining this morning. Na puts on her yellow raincoat and takes her yellow umbrella to school. Under a tree she finds a wet, cold little kitten. Na crouches down and holds her umbrella over it.");
        g.Story("v5-lang-o-vang-animal", "Chiếc ô màu vàng", umbrella,
            new Say("Na gặp con vật nào dưới gốc cây?", "Which animal does Na find under the tree?"),
            "☂️", ["Con chó", "Con mèo", "Con thỏ"], "Con mèo", Ill("chiec-o-vang"),
            retry: new Say("Con nhìn bạn nhỏ lông xám ngồi dưới gốc cây nhé!", "Look at the little grey animal under the tree!"));
        g.Story("v5-lang-o-vang-help", "Na giúp mèo con", umbrella,
            new Say("Na làm gì để giúp mèo con?", "What does Na do to help the kitten?"),
            "🐱", ["Bế mèo về nhà", "Cho mèo ăn cá", "Che ô cho mèo con"], "Che ô cho mèo con", Ill("chiec-o-vang"),
            retry: new Say("Con nhìn chiếc ô vàng của Na đang che cho ai nhé!", "Look at who Na's yellow umbrella is covering!"));
        g.Story("v5-lang-o-vang-kind", "Na là bạn thế nào?", umbrella,
            new Say("Theo con, Na là bạn thế nào?", "What kind of friend do you think Na is?"),
            "💖", ["Lười biếng", "Tốt bụng", "Hay cáu giận"], "Tốt bụng", Ill("chiec-o-vang"));

        // ───────────── Chặng 6: Vần và chữ ─────────────
        g.Stage = "Vần và chữ";
        g.Topic = "am-van";

        g.Pick("v5-lang-rhyme-meo", "Tìm tiếng cùng vần với mèo",
            new Say("Tiếng nào có vần giống tiếng “mèo”?", "Listen: mèo. Which word rhymes with mèo?", "Mèo. Tiếng nào có vần eo giống tiếng mèo?"),
            "🐱", ["Bò", "Kéo", "Cá"], "Kéo", scene: ["🐱"],
            retry: new Say("Con đọc chậm: mèo, kéo. Cùng có vần “eo” đấy!", "Say them slowly: mèo, kéo. They both end in 'eo'!", "Con đọc chậm nhé: mèo, kéo. Cùng có vần eo đấy!"));

        g.Multi("v5-lang-rhyme-sao", "Cùng vần với sao",
            new Say("Con tìm hai tiếng có vần giống tiếng “sao” nhé!", "Star is sao. Find two words that rhyme with sao!", "Sao. Con tìm hai tiếng có vần ao giống tiếng sao nhé!"),
            "⭐", ["Táo", "Gà", "Cáo", "Bò"], ["Táo", "Cáo"], scene: ["⭐"],
            retry: new Say("Con đọc to: sao, táo, cáo. Nghe giống nhau ở cuối không?", "Say them out loud: sao, táo, cáo. Do they sound the same at the end?"));

        g.Match("v5-lang-match-rhymes", "Nối tiếng cùng vần",
            new Say("Con nối hai tiếng có vần giống nhau nhé!", "Match the words that rhyme!"),
            "🎶", [("Mưa", "Dưa"), ("Trăng", "Răng"), ("Cây", "Mây")]);

        g.Listen("v5-lang-rhyme-listen-ong", "Nghe và tìm tiếng cùng vần",
            new Say("Con nghe cô đọc rồi tìm tiếng cùng vần nhé!", "Listen to my word, then find one that rhymes!"),
            "🐝", ["Cá", "Bóng", "Mây"], "Bóng",
            spoken: new Say("Ong. Tiếng nào có vần giống tiếng ong?", "Ong, that's bee. Which word rhymes with ong?"),
            retry: new Say("Ong, bóng. Con nghe thấy “ong” ở cuối không?", "Ong, bóng. Can you hear 'ong' at the end?", "Ong, bóng. Con có nghe thấy vần ong ở cuối không?"));

        g.Sort("v5-lang-sort-rhymes", "Nhà của vần",
            new Say("Tiếng nào cùng vần với “gà”, tiếng nào cùng vần với “thỏ”? Con xếp nhé!", "Which words rhyme with gà, and which rhyme with thỏ? Sort them!",
                "Tiếng nào cùng vần với gà, tiếng nào cùng vần với thỏ? Con xếp nhé!"),
            "🏠", ["Cùng vần với gà", "Cùng vần với thỏ"],
            [("Cá", "Cùng vần với gà"), ("Lá", "Cùng vần với gà"), ("Cỏ", "Cùng vần với thỏ"), ("Bò", "Cùng vần với thỏ")]);

        g.Pick("v5-lang-rhyme-odd", "Tiếng nào khác vần?",
            new Say("Tiếng nào không cùng vần với hai tiếng còn lại?", "Which word doesn't rhyme with the other two?"),
            "🔎", ["Hoa", "Bóng", "Loa"], "Bóng",
            retry: new Say("Hoa và loa cùng có vần “oa”. Còn tiếng nào khác?", "Hoa and loa both end in 'oa'. Which one is different?", "Hoa và loa cùng có vần oa. Còn tiếng nào khác nhỉ?"));

        g.Match("v5-lang-read-match", "Đọc chữ, nối hình",
            new Say("Con đọc từng chữ rồi nối với hình đúng nhé!", "Read each word and match it to its picture!"),
            "📖", [("cá", "Hình con cá"), ("gà", "Hình con gà"), ("bò", "Hình con bò")]);

        g.Pick("v5-lang-read-pick", "Chọn chữ đúng với hình",
            new Say("Đây là con gì? Con chọn chữ đúng nhé!", "What animal is this? Pick the right word!"),
            "🔤", ["bò", "gà", "cá"], "gà", focus: "🐔",
            retry: new Say("Con gà kêu ò ó o. Chữ “gà” bắt đầu bằng chữ g đấy!", "The chicken says cock-a-doodle-doo. Gà starts with the letter g!", "Con gà kêu ò ó o. Chữ gà bắt đầu bằng chữ gờ đấy!"));

        // ───────────── Chặng 7: Bé vui mỗi ngày ─────────────
        g.Stage = "Bé vui mỗi ngày";
        g.Topic = "doc-hieu";

        var gift = new Say(
            "Hôm nay là sinh nhật Bin. Mẹ tặng Bin một hộp quà màu xanh buộc nơ vàng. Bin mở ra, thấy quyển sách tranh về các con vật. Bin ôm mẹ: “Con cảm ơn mẹ ạ!”",
            "Today is Bin's birthday. Mum gives him a green present tied with a yellow bow. Bin opens it and finds a picture book about animals. He hugs Mum and says, \"Thank you, Mum!\"");
        g.Story("v5-lang-hop-day", "Chiếc hộp bí mật", gift,
            new Say("Hôm nay là ngày gì của Bin?", "What special day is it for Bin?"),
            "🎂", ["Ngày khai giảng", "Ngày sinh nhật", "Ngày Tết"], "Ngày sinh nhật", Ill("chiec-hop-bi-mat"),
            retry: new Say("Con nhìn bóng bay và dòng chữ trên tường nhé!", "Look at the balloons and the banner on the wall!"));
        g.Story("v5-lang-hop-inside", "Trong hộp có gì?", gift,
            new Say("Trong hộp quà có gì?", "What's inside the present?"),
            "🎁", ["Gấu bông", "Quả bóng", "Quyển sách"], "Quyển sách", Ill("chiec-hop-bi-mat"),
            retry: new Say("Con nhìn thứ Bin đang cầm trên tay nhé!", "Look at what Bin is holding in his hands!"));
        g.Story("v5-lang-hop-thanks", "Bin nói gì với mẹ?", gift,
            new Say("Nhận quà, Bin nói gì với mẹ?", "What does Bin say to Mum when he gets the present?"),
            "🙏", ["Con xin lỗi ạ!", "Con cảm ơn ạ!", "Con chào cô ạ!"], "Con cảm ơn ạ!", Ill("chiec-hop-bi-mat"));

        var boat = new Say(
            "Mưa vừa tạnh, sân có một vũng nước to. An đi ủng, gấp một chiếc thuyền giấy rồi thả xuống vũng nước. Thuyền trôi qua chiếc lá, qua viên sỏi. An cười tít mắt.",
            "The rain has just stopped and there's a big puddle in the yard. An puts on his boots, folds a paper boat and sets it on the puddle. The boat floats past a leaf and a pebble. An grins from ear to ear.");
        g.Story("ngon-ngu-story-chiec-thuyen-giay", "Chiếc thuyền giấy", boat,
            new Say("An gấp cái gì bằng giấy?", "What does An fold out of paper?"),
            "⛵", ["Máy bay", "Chiếc thuyền giấy", "Con diều"], "Chiếc thuyền giấy", Ill("chiec-thuyen-giay"),
            retry: new Say("Con nhìn thứ màu trắng đang nổi trên vũng nước nhé!", "Look at the white thing floating on the puddle!"));
        g.Story("v5-lang-thuyen-boots", "An đi gì dưới chân?", boat,
            new Say("An đi gì dưới chân để lội nước?", "What is An wearing on his feet to splash in the water?"),
            "👢", ["Đôi giày", "Đôi tất", "Đôi ủng"], "Đôi ủng", Ill("chiec-thuyen-giay"),
            retry: new Say("Con nhìn đôi chân của An, bạn ấy đi đôi gì màu xanh?", "Look at An's feet. What blue things is he wearing?"));

        g.Topic = "ke-chuyen";
        g.Order("v5-lang-order-boat", "Kể lại chuyện thuyền giấy",
            new Say("Con xếp lại chuyện thuyền giấy của An theo thứ tự nhé!", "Put An's paper boat story in order!"),
            "📖", ["Trời tạnh mưa", "Gấp thuyền giấy", "Thả thuyền xuống nước"],
            retry: new Say("Phải gấp thuyền xong rồi mới thả xuống nước được con nhé!", "An has to fold the boat before he can float it!"));

        g.Topic = "doc-hieu";
        var bigCarrot = new Say(
            "Sáng nay, thỏ trắng ra vườn. Thỏ thấy một củ cà rốt thật to. Thỏ nắm lá, kéo thật mạnh. Cà rốt bật lên, thỏ ngã ngồi rồi cười khúc khích.",
            "This morning, White Rabbit hops into the garden and spots a really big carrot. He grabs the leaves and pulls with all his might. Pop! Out comes the carrot, Rabbit lands on his bottom and giggles.");
        g.Story("ngon-ngu-story-tho-cu-ca-rot", "Chú thỏ và củ cà rốt", bigCarrot,
            new Say("Thỏ tìm thấy gì trong vườn?", "What does Rabbit find in the garden?"),
            "🥕", ["Bắp cải", "Cà rốt", "Cà chua"], "Cà rốt", Ill("tho-cu-ca-rot"));
        g.Story("v5-lang-tho-pull", "Thỏ nhổ cà rốt", bigCarrot,
            new Say("Thỏ làm thế nào để nhổ củ cà rốt lên?", "How does Rabbit get the carrot out of the ground?"),
            "💪", ["Gọi cà rốt tự chui lên", "Nắm lá kéo thật mạnh", "Tưới nước thật nhiều"], "Nắm lá kéo thật mạnh", Ill("tho-cu-ca-rot"),
            retry: new Say("Con nhìn hai tay thỏ đang nắm vào đâu nhé!", "Look at what Rabbit is holding with both hands!"));

        var smallCarrots = new Say(
            "Cà rốt của thỏ con nhỏ xíu. Bác thỏ đội mũ rơm dạy: “Cháu nhổ cỏ, tưới nước mỗi ngày nhé.” Thỏ con chăm chỉ làm theo. Ít lâu sau, cà rốt to và ngọt, đầy cả giỏ.",
            "Little Bunny's carrots are tiny. Grandpa Rabbit in his straw hat says, \"Pull the weeds and water them every day.\" Little Bunny works hard, and soon there's a whole basket of big, sweet carrots.");
        g.Story("ngon-ngu-story-tho-con-ca-rot", "Thỏ con chăm vườn", smallCarrots,
            new Say("Ai dạy thỏ con trồng cà rốt?", "Who teaches Little Bunny how to grow carrots?"),
            "👒", ["Mẹ chim", "Bác thỏ đội mũ rơm", "Bạn sóc nâu"], "Bác thỏ đội mũ rơm", Ill("tho-con-ca-rot"),
            retry: new Say("Con nhìn bác thỏ đeo kính, đội mũ rơm trong tranh nhé!", "Look at the rabbit with glasses and a straw hat!"));
        g.Story("v5-lang-tho-con-water", "Thỏ con tưới cây", smallCarrots,
            new Say("Thỏ con cầm gì để tưới cây?", "What is Little Bunny using to water the plants?"),
            "💧", ["Cái ô", "Bình tưới nước", "Cái cốc"], "Bình tưới nước", Ill("tho-con-ca-rot"),
            retry: new Say("Con nhìn thứ màu xanh thỏ con đang cầm trên tay nhé!", "Look at the blue thing in Little Bunny's hands!"));

        g.Topic = "ke-chuyen";
        g.Order("v5-lang-order-carrots", "Kể lại chuyện thỏ con",
            new Say("Vườn cà rốt của thỏ con thay đổi thế nào? Con xếp lại nhé!", "How does Little Bunny's carrot garden change? Put it in order!"),
            "📖", ["Cà rốt còn nhỏ xíu", "Nhổ cỏ, tưới nước", "Cà rốt to đầy giỏ"],
            retry: new Say("Lúc đầu cà rốt còn nhỏ, chăm sóc rồi mới to con nhé!", "First the carrots are tiny, then they grow big with care!"));

        g.Match("v5-lang-match-kids", "Bạn nào, đồ nấy",
            new Say("Mỗi bạn nhỏ trong truyện có một món đồ. Con nối cho đúng nhé!", "Each child in our stories has something special. Match them up!"),
            "🧒", [("Bạn Na", "Cái ô"), ("Bạn Bin", "Quyển sách"), ("Bạn An", "Chiếc thuyền giấy")]);
    }
}

public static partial class Lexicon
{
    private const string LanguageTable = """
        // Từ vựng
        Để cắt giấy | 📄 | for cutting paper
        Để viết chữ | 📝 | for writing
        Để xúc cơm | 🍚 | for scooping rice
        Đang chạy | 🏃 | running
        Đang ăn | 😋 | eating
        Đang ngủ | 😴 | sleeping
        Đồ để mặc | 👚 | things we wear
        Đồ để ăn | 🍴 | things we eat
        // Tiếng kêu con vật
        Meo meo | 🔊 | meow meow
        Ụt ịt | 🔊 | oink oink
        Be be | 🔊 | baa baa
        // Truyện
        Con nhím | 🦔 | hedgehog
        Vì mật ong không ngon | 😖 | because the honey isn't tasty
        Để phần cho các bạn ong | 🐝 | to leave some for the bees
        Vì gấu sắp đi ngủ | 😴 | because Bear is going to sleep
        Giữ làm của mình | 🙈 | keep it for myself
        Trả lại cho bạn | 🤲 | give it back
        Vứt đi | 🗑️ | throw it away
        Để ném chơi với bạn | 🤾 | to throw at friends for fun
        Để trồng thành cây | 🌱 | to grow into trees
        Để dành ăn mùa đông | ❄️ | to eat in winter
        Nhặt hạt dẻ | 🌰 | pick up chestnuts
        Bỏ vào giỏ | 🧺 | put them in the basket
        Cất cho mùa đông | 🍂 | hide them for winter
        Trong bồn tắm | 🛁 | in the bathtub
        Ở bờ suối | 🏞️ | at the stream
        Dưới vòi nước | 🚰 | under the tap
        Thấy quả táo | 🍎 | find the apple
        Đẩy táo từ từ | 🦔 | push the apple slowly
        Rửa táo ở suối | 💧 | wash the apple in the stream
        Mang táo về hang | 🕳️ | take the apple home
        Chiếc bè gỗ | 🪵 | a wooden raft
        Bỏ cuộc không tập nữa | 🏳️ | give up
        Khóc mãi không thôi | 😭 | cry and cry
        Cố gắng tập tiếp | 💪 | keep on trying
        Bế mèo về nhà | 🏠 | carry the kitten home
        Cho mèo ăn cá | 🐟 | feed the kitten a fish
        Che ô cho mèo con | ☂️ | hold the umbrella over the kitten
        Lười biếng | 😪 | lazy
        Tốt bụng | 💖 | kind
        Hay cáu giận | 😠 | grumpy
        Ngày khai giảng | 🏫 | the first day of school
        Ngày sinh nhật | 🎂 | birthday
        Ngày Tết | 🧧 | Lunar New Year
        Chiếc thuyền giấy | ⛵ | paper boat
        Trời tạnh mưa | 🌦️ | the rain stops
        Gấp thuyền giấy | 📄 | fold a paper boat
        Thả thuyền xuống nước | 🌊 | float the boat on the water
        Gọi cà rốt tự chui lên | 🗣️ | ask the carrot to pop out by itself
        Nắm lá kéo thật mạnh | 💪 | grab the leaves and pull hard
        Tưới nước thật nhiều | 💦 | pour lots of water on it
        Mẹ chim | 🐦 | Mother Bird
        Bác thỏ đội mũ rơm | 👒 | Grandpa Rabbit in a straw hat
        Bạn sóc nâu | 🐿️ | Brown Squirrel
        Bình tưới nước | 🚿 | watering can
        Cà rốt còn nhỏ xíu | 🥕 | the carrots are tiny
        Nhổ cỏ, tưới nước | 🌿 | pull weeds and water
        Cà rốt to đầy giỏ | 🧺 | a basket full of big carrots
        Bạn Na | 👧 | Na
        Bạn Bin | 👦 | Bin
        Bạn An | 🧒 | An
        // Từ trái nghĩa
        To như voi | 🐘 | as big as an elephant
        Nhỏ như chuột | 🐭 | as small as a mouse
        Nóng bỏng | 🥵 | hot
        Lạnh buốt | 🥶 | cold
        Nhanh như thỏ | 🐇 | as fast as a rabbit
        Chậm như sên | 🐌 | as slow as a snail
        Cao như hươu | 🦒 | as tall as a giraffe
        Thấp như cây nấm | 🍄 | as short as a mushroom
        Ban ngày | ☀️ | daytime
        Ban đêm | 🌙 | night-time
        Sạch sẽ | 🛁 | clean
        Lấm bẩn | 🐷 | muddy
        Cốc trà nóng | ☕ | hot tea
        Ngọn lửa | 🔥 | fire
        Viên đá lạnh | 🧊 | ice cube
        Người tuyết | ☃️ | snowman
        // Vần (tiếng một âm tiết)
        Mèo | 🐱 | cat
        Kéo | ✂️ | scissors
        Bò | 🐮 | cow
        Cá | 🐟 | fish
        Lá | 🍃 | leaf
        Gà | 🐔 | chicken
        Cỏ | 🌿 | grass
        Sao | ⭐ | star
        Táo | 🍎 | apple
        Cáo | 🦊 | fox
        Mưa | 🌧️ | rain
        Dưa | 🍉 | watermelon
        Trăng | 🌙 | moon
        Răng | 🦷 | tooth
        Cây | 🌳 | tree
        Mây | ☁️ | cloud
        Hoa | 🌸 | flower
        Loa | 📢 | loudspeaker
        Bóng | ⚽ | ball
        Cùng vần với gà | 🐔 | rhymes with gà
        Cùng vần với thỏ | 🐰 | rhymes with thỏ
        // Chữ để đọc (không kèm hình, bé đọc chữ) và hình chỉ có ảnh
        cá |  | fish
        gà |  | chicken
        bò |  | cow
        Hình con cá | pic:🐟 | a picture of a fish
        Hình con gà | pic:🐔 | a picture of a chicken
        Hình con bò | pic:🐮 | a picture of a cow
        """;
}
