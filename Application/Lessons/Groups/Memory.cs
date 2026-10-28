namespace HanhTrangLop1.Application.Lessons;

public static partial class LessonBank
{
    // Câu dùng lại nhiều lần trong nhóm: cùng một câu tiếng Việt luôn có cùng một bản tiếng Anh.
    private static readonly Say MemorySamplePrompt = new("Con tìm hình giống hệt hình mẫu nhé!", "Find the picture that matches the sample!");
    private static readonly Say MemoryTwinPrompt = new("Mỗi bạn có một bức tranh vẽ giống mình. Con nối hai hình giống nhau nhé!", "Each one has a drawing that looks just like it. Match the two that are the same!");
    private static readonly Say MemoryCountRetry = new("Con chạm tay vào từng bạn cần đếm, bỏ qua các bạn khác nhé.", "Touch each one you're counting, and skip the others.");
    private static readonly Say MemoryListenPrompt = new("Con nghe cô dặn thật kỹ rồi làm theo nhé!", "Listen carefully to what I say, then do it!");
    private static readonly Say MemoryStoryPrompt = new("Con nghe cô kể thật kỹ, rồi trả lời cô nhé!", "Listen closely to my little story, then answer me!");

    /// <summary>Nhóm ghi-nho-tap-trung.</summary>
    private static void AddMemory(LessonGroupBuilder g)
    {
        // ---------------- Chặng 1: tìm hình giống mẫu, tìm hai hình giống nhau ----------------
        g.Stage = "Tìm hình giống mẫu";
        g.Topic = "tap-trung";

        g.Pick("memory-single-animal", "Tìm bạn giống hình mẫu", MemorySamplePrompt,
            "🐯", ["Con sư tử", "Con hổ", "Con mèo"], "Con hổ", focus: "🐯",
            retry: new Say("Con nhìn kỹ các vằn đen trên mặt nhé!", "Look closely at the black stripes on the face!"));

        g.Pick("v5-mem-same-twice", "Bạn nào xuất hiện hai lần?",
            new Say("Trong hàng có một bạn xuất hiện hai lần. Đó là bạn nào?", "One friend shows up twice in the row. Who is it?"),
            "👀", ["Con chó", "Con mèo", "Con gấu"], "Con mèo", scene: ["🐶", "🐱", "🐰", "🐻", "🐱"],
            retry: new Say("Con chỉ tay vào từng bạn và tìm hai bạn giống nhau nhé.", "Point at each one and look for two that match."));

        g.Match("v5-mem-match-drawings", "Con vật và tranh vẽ", MemoryTwinPrompt,
            "🖼️", [("Con mèo", "Tranh vẽ con mèo"), ("Con cá", "Tranh vẽ con cá"), ("Con thỏ", "Tranh vẽ con thỏ")],
            retry: new Say("Con nhìn tai, đuôi và hình dáng của từng bạn nhé.", "Look at the ears, the tail and the shape of each one."));

        g.Pick("v5-mem-same-apple", "Quả nào giống mẫu?", MemorySamplePrompt,
            "🍎", ["Cà chua", "Quả đào", "Quả táo"], "Quả táo", focus: "🍎",
            retry: new Say("Quả mẫu có cuống nhỏ và một chiếc lá.", "The sample has a little stem and one leaf."));

        g.Pick("v5-mem-same-twice-fruit", "Quả nào xuất hiện hai lần?",
            new Say("Có một quả xuất hiện hai lần. Đó là quả gì?", "One fruit shows up twice. Which one is it?"),
            "🍋", ["Quả chanh", "Quả nho", "Quả đào"], "Quả chanh", scene: ["🍓", "🍋", "🍇", "🍑", "🍋", "🍉"],
            retry: new Say("Con nhìn lần lượt từng quả, từ trái sang phải nhé.", "Look at each fruit in turn, from left to right."));

        g.Pick("v5-mem-same-firetruck", "Xe nào giống mẫu?", MemorySamplePrompt,
            "🚒", ["Xe cứu thương", "Xe cứu hỏa", "Xe buýt"], "Xe cứu hỏa", focus: "🚒",
            retry: new Say("Xe mẫu màu đỏ và có cái thang dài.", "The sample is red and has a long ladder."));

        g.Match("v5-mem-match-drawings-things", "Đồ vật và tranh vẽ",
            new Say("Con nối mỗi đồ vật với bức tranh vẽ giống nó nhé!", "Match each thing with the drawing that looks like it!"),
            "🖍️", [("Quả táo", "Tranh vẽ quả táo"), ("Xe đạp", "Tranh vẽ xe đạp"), ("Cái ô", "Tranh vẽ cái ô")],
            retry: new Say("Con nhìn hình dáng: tròn, có bánh xe hay có cán cầm?", "Look at the shape: round, with wheels, or with a handle?"));

        g.Pick("v5-mem-same-turtle", "Bạn nào giống mẫu?", MemorySamplePrompt,
            "🐢", ["Con ếch", "Con rùa", "Con cá sấu"], "Con rùa", focus: "🐢",
            retry: new Say("Hình mẫu có cái mai tròn trên lưng.", "The sample has a round shell on its back."));

        // ---------------- Chặng 2: chú ý chi tiết ----------------
        g.Stage = "Chú ý chi tiết";
        g.Topic = "tap-trung";

        g.Pick("v5-mem-detail-green-apple", "Quả táo màu xanh",
            new Say("Trong rổ có táo đỏ và táo xanh. Con tìm quả táo màu xanh nhé!", "There are red apples and green apples. Find the green apple!"),
            "🍏", ["Hình quả táo đỏ", "Hình quả táo xanh", "Hình quả lê xanh"], "Hình quả táo xanh",
            retry: new Say("Quả lê cũng màu xanh nhưng dáng dài hơn đấy!", "The pear is green too, but it's a longer shape!"));

        g.Pick("v5-mem-detail-taxi", "Chiếc taxi màu vàng",
            new Say("Cô cần đi taxi. Con tìm giúp cô chiếc xe taxi màu vàng nhé!", "I need a taxi. Can you find the yellow taxi for me?"),
            "🚕", ["Hình ô tô đỏ", "Hình xe taxi vàng", "Hình ô tô xanh"], "Hình xe taxi vàng",
            retry: new Say("Xe taxi có một biển nhỏ trên nóc xe đấy.", "A taxi has a little sign on its roof."));

        g.Multi("v5-mem-detail-yellow", "Những bạn màu vàng",
            new Say("Con tìm tất cả những bạn màu vàng nhé!", "Find everything that is yellow!"),
            "💛", ["Quả chuối", "Con ếch", "Gà con", "Quả táo", "Quả chanh"], ["Quả chuối", "Gà con", "Quả chanh"],
            retry: new Say("Con ếch màu xanh lá, quả táo màu đỏ đấy.", "The frog is green and the apple is red."));

        g.Pick("v5-mem-detail-fingers", "Bàn tay giơ hai ngón",
            new Say("Bàn tay nào đang giơ hai ngón? Con đếm ngón tay thật kỹ nhé!", "Which hand is holding up two fingers? Count the fingers carefully!"),
            "✌️", ["Hình tay một ngón", "Hình tay hai ngón", "Hình tay năm ngón"], "Hình tay hai ngón",
            retry: new Say("Con giơ tay mình lên làm thử theo từng hình nhé!", "Hold up your own hand and copy each picture!"));

        g.Sort("v5-mem-sort-stripes-spots", "Sọc vằn hay chấm tròn?",
            new Say("Con nhìn kỹ bộ áo của từng bạn: có sọc vằn hay có chấm tròn?", "Look closely at each one's coat: does it have stripes or spots?"),
            "🦓", ["Nhóm có sọc vằn", "Nhóm có chấm tròn"],
            [("Con hổ", "Nhóm có sọc vằn"), ("Con ong", "Nhóm có sọc vằn"), ("Bọ rùa", "Nhóm có chấm tròn"), ("Con báo đốm", "Nhóm có chấm tròn")],
            retry: new Say("Sọc vằn là những đường kẻ dài, chấm tròn là những đốm nhỏ.", "Stripes are long lines, and spots are little dots."));

        g.Pick("v5-mem-detail-full-moon", "Ông trăng tròn",
            new Say("Đêm Trung thu, trăng tròn vành vạnh. Con tìm ông trăng tròn nhé!", "At the Mid-Autumn Festival, the moon is big and round. Find the full moon!"),
            "🌕", ["Hình trăng khuyết", "Hình ông mặt trời", "Hình trăng tròn"], "Hình trăng tròn",
            retry: new Say("Trăng khuyết cong như quả chuối, trăng tròn thì tròn như cái đĩa.", "A crescent moon is curved like a banana. A full moon is round like a plate."));

        g.Multi("v5-mem-detail-hats", "Ai đang đội mũ?",
            new Say("Con tìm tất cả những người đang đội mũ nhé!", "Find everyone who is wearing a hat!"),
            "🤠", ["Chú cao bồi", "Em bé sơ sinh", "Chú công nhân", "Bạn nhỏ cười tươi", "Ông già Noel"], ["Chú cao bồi", "Chú công nhân", "Ông già Noel"],
            retry: new Say("Con nhìn kỹ trên đầu từng người nhé.", "Look carefully at the top of each person's head."));

        g.Pick("v5-mem-detail-clock", "Đồng hồ chỉ ba giờ",
            new Say("Kim ngắn chỉ vào số 3 là ba giờ. Con tìm đồng hồ chỉ ba giờ nhé!", "When the short hand points to 3, it's three o'clock. Find the clock showing three o'clock!",
                "Kim ngắn chỉ vào số ba là ba giờ. Con tìm đồng hồ chỉ ba giờ nhé!"),
            "🕒", ["Hình đồng hồ chín giờ", "Hình đồng hồ ba giờ", "Hình đồng hồ mười hai giờ"], "Hình đồng hồ ba giờ",
            retry: new Say("Kim ngắn chỉ sang bên phải, về phía số 3.", "The short hand points to the right, toward the 3.", "Kim ngắn chỉ sang bên phải, về phía số ba."));

        // ---------------- Chặng 3: đếm đúng một loại trong cảnh lẫn lộn ----------------
        g.Stage = "Đếm trong cảnh lẫn lộn";
        g.Topic = "tap-trung";

        g.Pick("v5-mem-count-fish-pond", "Đếm cá trong ao",
            new Say("Trong ao có cá, rùa và cua. Con chỉ đếm cá thôi nhé! Có mấy con cá?", "The pond has fish, turtles and crabs. Count only the fish! How many fish?"),
            "🐟", LessonGroupBuilder.NumberChoices(4), "4", scene: ["🐟", "🐢", "🐟", "🦀", "🐟", "🐢", "🐟"], retry: MemoryCountRetry);

        g.Pick("v5-mem-count-apples-table", "Đếm táo trên bàn",
            new Say("Trên bàn có nhiều loại quả. Con chỉ đếm táo thôi nhé! Có mấy quả táo?", "There's lots of fruit on the table. Count only the apples! How many apples?"),
            "🍎", LessonGroupBuilder.NumberChoices(3), "3", scene: ["🍎", "🍌", "🍎", "🍇", "🍌", "🍎"], retry: MemoryCountRetry);

        g.Topic = "lam-theo-yeu-cau";
        g.Sort("v5-mem-sort-tidy-room", "Dọn phòng cùng cô",
            new Say("Phòng bừa bộn quá! Con cất đồ chơi vào hộp, quần áo vào ngăn tủ nhé.", "What a messy room! Put the toys in the toy box and the clothes in the drawer."),
            "🧹", ["Hộp đồ chơi", "Ngăn quần áo"],
            [("Gấu bông", "Hộp đồ chơi"), ("Quả bóng", "Hộp đồ chơi"), ("Con diều", "Hộp đồ chơi"),
             ("Cái áo", "Ngăn quần áo"), ("Đôi tất", "Ngăn quần áo"), ("Cái váy", "Ngăn quần áo")],
            retry: new Say("Đồ chơi để chơi, quần áo để mặc nhé.", "Toys are for playing, and clothes are for wearing."));

        g.Topic = "tap-trung";
        g.Pick("v5-mem-count-red-cars", "Đếm ô tô đỏ",
            new Say("Đường phố đông vui quá! Có mấy chiếc ô tô màu đỏ?", "The street is so busy! How many red cars are there?"),
            "🚗", LessonGroupBuilder.NumberChoices(5), "5", scene: ["🚗", "🚌", "🚲", "🚗", "🛵", "🚗", "🚌", "🚗", "🚗"], retry: MemoryCountRetry);

        g.Multi("v5-mem-find-in-garden", "Trong vườn có ai?",
            new Say("Con nhìn kỹ khu vườn. Con chạm vào những bạn có trong vườn nhé!", "Look closely at the garden. Tap the animals you can see in it!"),
            "🌻", ["Con ong", "Con cá", "Con sâu", "Con bướm", "Con voi"], ["Con ong", "Con sâu", "Con bướm"],
            scene: ["🐝", "🌸", "🐛", "🌻", "🦋", "🌸"],
            retry: new Say("Con nhìn lại tranh khu vườn, từng hình một nhé.", "Look at the garden picture again, one by one."));

        g.Pick("v5-mem-count-stars", "Đếm sao trên trời",
            new Say("Bầu trời đêm có trăng, mây và sao. Có mấy ngôi sao?", "The night sky has the moon, clouds and stars. How many stars?"),
            "⭐", LessonGroupBuilder.NumberChoices(6), "6", scene: ["⭐", "🌙", "⭐", "☁️", "⭐", "⭐", "☁️", "⭐", "⭐"], retry: MemoryCountRetry);

        g.Pick("v5-mem-count-ladybugs", "Đếm bọ rùa",
            new Say("Trên lá có kiến và bọ rùa. Có mấy con bọ rùa?", "There are ants and ladybugs on the leaves. How many ladybugs?"),
            "🐞", LessonGroupBuilder.NumberChoices(2), "2", scene: ["🐜", "🍃", "🐞", "🐜", "🍃", "🐜", "🐞", "🍃"], retry: MemoryCountRetry);

        g.Pick("v5-mem-count-balloons", "Đếm bóng bay ngày sinh nhật",
            new Say("Tiệc sinh nhật có bóng bay, quà và bánh. Có mấy quả bóng bay?", "The birthday party has balloons, presents and cake. How many balloons?"),
            "🎈", LessonGroupBuilder.NumberChoices(7), "7", scene: ["🎈", "🎁", "🎈", "🎂", "🎈", "🎈", "🎁", "🎈", "🎈", "🎈"], retry: MemoryCountRetry);

        // ---------------- Chặng 4: nghe và làm theo yêu cầu ----------------
        g.Stage = "Nghe và làm theo";
        g.Topic = "lam-theo-yeu-cau";

        g.Listen("memory-listen-cat", "Bạn kêu meo meo", MemoryListenPrompt,
            "🎧", ["Con chó", "Con mèo", "Con vịt"], "Con mèo",
            spoken: new Say("Con chạm vào bạn kêu meo meo nhé!", "Tap the one that says meow!"));

        g.Listen("memory-listen-apple", "Quả màu đỏ", MemoryListenPrompt,
            "🎧", ["Quả chuối", "Quả táo", "Quả nho"], "Quả táo",
            spoken: new Say("Con chạm vào quả màu đỏ, có cuống và lá xanh nhé!", "Tap the red fruit with a stem and a green leaf!"));

        g.Order("v5-mem-follow-three-taps", "Chạm theo lời cô",
            new Say("Con nghe cô dặn: chạm con mèo, rồi con chó, rồi con cá nhé!", "Listen: tap the cat, then the dog, then the fish!"),
            "👆", ["Con mèo", "Con chó", "Con cá"],
            retry: new Say("Con nghe lại nhé: mèo trước, rồi chó, cuối cùng là cá.", "Listen again: cat first, then dog, and the fish last."));

        g.Listen("v5-mem-listen-trunk", "Bạn có vòi dài", MemoryListenPrompt,
            "🎧", ["Con hươu cao cổ", "Con voi", "Con sư tử"], "Con voi",
            spoken: new Say("Con chạm vào bạn có cái vòi dài và đôi tai to nhé!", "Tap the one with a long trunk and big ears!"));

        g.Multi("v5-mem-follow-two-swimmers", "Chỉ chọn hai bạn",
            new Say("Cô dặn nhé: con chọn hai bạn biết bơi, chỉ hai bạn thôi!", "Here's your job: pick two animals that can swim. Just two!"),
            "🏊", ["Con cá", "Con mèo", "Con vịt", "Con gà"], ["Con cá", "Con vịt"],
            retry: new Say("Bạn vịt bơi giỏi lắm, còn bạn gà thì sợ nước.", "Ducks are great swimmers, but chickens don't like water."));

        g.Listen("v5-mem-listen-not-red", "Quả không phải màu đỏ", MemoryListenPrompt,
            "🎧", ["Quả táo", "Quả dâu tây", "Quả chuối"], "Quả chuối",
            spoken: new Say("Nghe kỹ nhé: con chạm vào quả không phải màu đỏ!", "Listen carefully: tap the fruit that is NOT red!"));

        g.Order("v5-mem-follow-pack-bag", "Xếp cặp theo lời cô",
            new Say("Con cho vào cặp theo thứ tự cô nói: quyển sách, bút chì, rồi thước kẻ nhé!", "Pack your bag in the order I say: the book, the pencil, then the ruler!"),
            "🎒", ["Quyển sách", "Bút chì", "Thước kẻ"],
            retry: new Say("Con nghe lại: sách trước, bút chì sau, thước kẻ cuối cùng.", "Listen again: book first, pencil next, ruler last."));

        g.Listen("v5-mem-listen-rain-thing", "Đồ dùng khi trời mưa", MemoryListenPrompt,
            "🎧", ["Kính râm", "Cái ô", "Que kem"], "Cái ô",
            spoken: new Say("Trời sắp mưa rồi! Con chạm vào đồ giúp con không bị ướt nhé!", "It's about to rain! Tap the thing that keeps you dry!"));

        g.Listen("v5-mem-listen-two-clues", "Hai gợi ý", MemoryListenPrompt,
            "🎧", ["Con cá", "Con vịt", "Con chim"], "Con vịt",
            spoken: new Say("Hai gợi ý nhé: bạn ấy có cánh và biết bơi. Con chạm vào bạn ấy!", "Two clues: it has wings, and it can swim. Tap it!"),
            retry: new Say("Bạn ấy có cánh như chim, lại bơi giỏi như cá.", "It has wings like a bird, and swims well like a fish."));

        // ---------------- Chặng 5: nhớ trình tự ----------------
        g.Stage = "Nhớ trình tự";
        g.Topic = "ghi-nho";

        g.Listen("v5-mem-recall-market", "Cô đi chợ", MemoryStoryPrompt,
            "🛒", ["Quả cam", "Cà rốt", "Quả dưa hấu"], "Cà rốt",
            spoken: new Say("Cô đi chợ mua cà rốt và trứng. Con chạm vào thứ cô đã mua nhé!", "I went to the market and bought carrots and eggs. Tap something I bought!"));

        g.Listen("v5-mem-recall-missing-friend", "Bạn nào ở nhà?", MemoryStoryPrompt,
            "🎧", ["Con thỏ", "Con gấu", "Con chó"], "Con gấu",
            spoken: new Say("Mèo, chó và thỏ cùng đi chơi. Bạn nào không đi chơi?", "Cat, dog and rabbit went out to play. Who did not go?"),
            retry: new Say("Con nhớ lại: mèo, chó và thỏ. Bạn nào không có tên trong đó?", "Remember: cat, dog and rabbit. Who wasn't in that list?"));

        g.Order("v5-mem-order-shopping", "Nhớ thứ tự đi chợ",
            new Say("Cô mua táo, rồi bánh mì, rồi sữa. Con xếp lại đúng thứ tự cô mua nhé!", "I bought apples, then bread, then milk. Put them in the order I bought them!"),
            "🛒", ["Quả táo", "Bánh mì", "Cốc sữa"],
            retry: new Say("Con nhớ lại nhé: táo trước, rồi bánh mì, cuối cùng là sữa.", "Remember: apples first, then bread, and milk last."));

        g.Listen("v5-mem-recall-first", "Cô ăn gì trước?", MemoryStoryPrompt,
            "🍌", ["Bánh mì", "Quả chuối", "Cốc sữa"], "Quả chuối",
            spoken: new Say("Sáng nay cô ăn chuối trước, rồi uống sữa. Cô ăn gì trước tiên?", "This morning I ate a banana first, then drank some milk. What did I have first?"));

        g.Order("memory-order-story", "Thỏ trồng cà rốt",
            new Say("Thỏ gieo hạt, hạt nảy mầm, rồi thành củ cà rốt. Con xếp lại câu chuyện nhé!", "Rabbit planted a seed, it sprouted, and it became a carrot. Put the story in order!"),
            "🐰", ["Hạt giống", "Mầm cây", "Cà rốt"],
            retry: new Say("Câu chuyện bắt đầu từ một hạt giống nhỏ.", "The story starts with a tiny seed."));

        g.Listen("v5-mem-recall-last", "Ai về đích cuối cùng?", MemoryStoryPrompt,
            "🏁", ["Con rùa", "Con thỏ", "Con ốc sên"], "Con ốc sên",
            spoken: new Say("Thỏ về đích đầu tiên, rùa về thứ hai, ốc sên về cuối cùng. Bạn nào về cuối cùng?", "Rabbit finished first, turtle came second, and snail came last. Who came last?"));

        g.Order("v5-mem-order-race", "Thứ tự về đích",
            new Say("Ngựa về nhất, chó về nhì, mèo về thứ ba. Con xếp lại đúng thứ tự nhé!", "Horse came first, dog second, and cat third. Put them in that order!"),
            "🏆", ["Con ngựa", "Con chó", "Con mèo"],
            retry: new Say("Con nghe lại: ngựa, rồi chó, rồi mèo.", "Listen again: horse, then dog, then cat."));

        g.Listen("v5-mem-recall-zoo", "Đi thăm sở thú", MemoryStoryPrompt,
            "🎧", ["Con hổ", "Con khỉ", "Con voi"], "Con voi",
            spoken: new Say("Ở sở thú, cô thấy voi, rồi hổ, rồi khỉ. Cô thấy bạn nào đầu tiên?", "At the zoo, I saw an elephant, then a tiger, then a monkey. Which one did I see first?"));

        g.Order("v5-mem-order-four", "Bốn bạn xếp hàng",
            new Say("Bốn bạn xếp hàng: gà, vịt, ếch, cá. Con xếp lại đúng như thế nhé!", "Four friends line up: chicken, duck, frog, fish. Put them in that same order!"),
            "🦆", ["Con gà", "Con vịt", "Con ếch", "Con cá"],
            retry: new Say("Con nói thầm theo cô: gà, vịt, ếch, cá.", "Whisper along with me: chicken, duck, frog, fish."));

        // ---------------- Chặng 6: siêu trí nhớ (tổng hợp, khó hơn) ----------------
        g.Stage = "Siêu trí nhớ";

        g.Topic = "tap-trung";
        g.Match("v5-mem-match-drawings-2", "Tìm đôi giống nhau", MemoryTwinPrompt,
            "🖼️", [("Con chó", "Tranh vẽ con chó"), ("Con vịt", "Tranh vẽ con vịt"), ("Con ong", "Tranh vẽ con ong")],
            retry: new Say("Con nhìn kỹ: mỏ dẹt, đôi cánh nhỏ có sọc hay đôi tai cụp?", "Look closely: a flat beak, little striped wings, or floppy ears?"));

        g.Topic = "ghi-nho";
        g.Listen("v5-mem-recall-band", "Ban nhạc tí hon", MemoryStoryPrompt,
            "🥁", ["Cái trống", "Cái kèn", "Đàn ghi-ta"], "Cái kèn",
            spoken: new Say("Ban nhạc chơi trống, rồi kèn, rồi đàn ghi-ta. Nhạc cụ nào chơi thứ hai?", "The band played the drum, then the trumpet, then the guitar. Which one played second?"),
            retry: new Say("Con đếm theo cô: trống là một, tiếp theo là hai...", "Count with me: the drum is one, and next is two..."));

        g.Topic = "tap-trung";
        g.Pick("v5-mem-same-twice-vehicles", "Phương tiện xuất hiện hai lần",
            new Say("Có một phương tiện xuất hiện hai lần. Đó là gì?", "One vehicle shows up twice. Which one is it?"),
            "🚌", ["Máy bay", "Xe buýt", "Tàu thủy"], "Xe buýt", scene: ["🚲", "🚌", "✈️", "🚢", "🚌", "🚀"],
            retry: new Say("Con chỉ tay vào từng hình và tìm hai hình giống nhau nhé.", "Point at each picture and look for two that match."));

        g.Sort("v5-mem-sort-red-yellow", "Hộp đỏ, hộp vàng",
            new Say("Con chú ý màu sắc nhé: đồ màu đỏ vào hộp đỏ, đồ màu vàng vào hộp vàng!", "Watch the colors: red things go in the red box, yellow things in the yellow box!"),
            "🎨", ["Hộp màu đỏ", "Hộp màu vàng"],
            [("Quả táo", "Hộp màu đỏ"), ("Quả dâu tây", "Hộp màu đỏ"), ("Cà chua", "Hộp màu đỏ"),
             ("Quả chuối", "Hộp màu vàng"), ("Quả chanh", "Hộp màu vàng"), ("Gà con", "Hộp màu vàng")],
            retry: new Say("Con nhìn màu của từng hình thật kỹ rồi mới đặt vào hộp nhé.", "Look carefully at each picture's color before you put it in a box."));

        g.Topic = "ghi-nho";
        g.Multi("v5-mem-recall-picnic", "Giỏ đồ đi dã ngoại",
            new Say("Cô bỏ vào giỏ bánh mì, quả táo và sữa. Con chọn đúng ba thứ đó nhé!", "I packed bread, an apple and milk in the picnic basket. Pick those three things!"),
            "🧺", ["Bánh mì", "Quả cam", "Quả táo", "Cốc sữa", "Que kem"], ["Bánh mì", "Quả táo", "Cốc sữa"],
            retry: new Say("Con nghe lại: bánh mì, quả táo và sữa.", "Listen again: bread, an apple, and milk."));

        g.Topic = "tap-trung";
        g.Pick("v5-mem-detail-sleeping", "Suỵt, bạn đang ngủ",
            new Say("Suỵt! Có một bạn đang ngủ. Con tìm bạn ấy nhé!", "Shh! Someone is sleeping. Can you find them?"),
            "😴", ["Hình bạn cười", "Hình bạn ngáp", "Hình bạn ngủ say"], "Hình bạn ngủ say",
            retry: new Say("Bạn đang ngủ thì nhắm mắt và có chữ z z z.", "A sleeping face has closed eyes and little Z's."));

        g.Topic = "lam-theo-yeu-cau";
        g.Order("v5-mem-order-bead-sample", "Xâu hạt theo mẫu",
            new Say("Con nhìn mẫu rồi xâu hạt đúng thứ tự như mẫu nhé!", "Look at the sample and thread the beads in the same order!"),
            "📿", ["Hạt vòng vàng", "Hạt vòng đỏ", "Hạt vòng xanh lá", "Hạt vòng xanh dương"], scene: ["🟡", "🔴", "🟢", "🔵"],
            retry: new Say("Con nhìn mẫu từ trái sang phải nhé.", "Look at the sample from left to right."));

        g.Topic = "ghi-nho";
        g.Listen("v5-mem-recall-where", "Mèo trốn ở đâu?", MemoryStoryPrompt,
            "🐱", ["Cặp sách", "Cái ô", "Cái ghế"], "Cái ghế",
            spoken: new Say("Con mèo trốn sau cái ghế, con chó nằm cạnh cặp sách. Mèo trốn ở đâu?", "The cat is hiding behind the chair, and the dog is lying by the school bag. Where is the cat hiding?"),
            retry: new Say("Con nghe lại: chó nằm cạnh cặp sách, còn mèo thì trốn ở chỗ khác.", "Listen again: the dog is by the school bag, and the cat is hiding somewhere else."));
    }
}

public static partial class Lexicon
{
    /// <summary>
    /// Nhãn riêng của nhóm Ghi nhớ và tập trung. Nhãn có hình "pic:" chỉ hiện hình (ẩn chữ) và cô đọc "hình này"
    /// để không lộ đáp án.
    /// </summary>
    private const string MemoryTable = """
        // Tranh vẽ (ẩn chữ) để tìm hai hình giống nhau
        Tranh vẽ con mèo | pic:/images/pictograms/cat.svg | this picture | hình này
        Tranh vẽ con cá | pic:/images/pictograms/fish.svg | this picture | hình này
        Tranh vẽ con thỏ | pic:/images/pictograms/rabbit.svg | this picture | hình này
        Tranh vẽ con chó | pic:/images/pictograms/dog.svg | this picture | hình này
        Tranh vẽ con vịt | pic:/images/pictograms/duck.svg | this picture | hình này
        Tranh vẽ con ong | pic:/images/pictograms/bee.svg | this picture | hình này
        Tranh vẽ quả táo | pic:/images/pictograms/apple.svg | this picture | hình này
        Tranh vẽ xe đạp | pic:/images/pictograms/bicycle.svg | this picture | hình này
        Tranh vẽ cái ô | pic:/images/pictograms/umbrella.svg | this picture | hình này
        // Chú ý chi tiết (ẩn chữ)
        Hình quả táo đỏ | pic:🍎 | this picture | hình này
        Hình quả táo xanh | pic:🍏 | this picture | hình này
        Hình quả lê xanh | pic:🍐 | this picture | hình này
        Hình ô tô đỏ | pic:🚗 | this picture | hình này
        Hình xe taxi vàng | pic:🚕 | this picture | hình này
        Hình ô tô xanh | pic:🚙 | this picture | hình này
        Hình tay một ngón | pic:☝️ | this picture | hình này
        Hình tay hai ngón | pic:✌️ | this picture | hình này
        Hình tay năm ngón | pic:✋ | this picture | hình này
        Hình trăng tròn | pic:🌕 | this picture | hình này
        Hình trăng khuyết | pic:🌙 | this picture | hình này
        Hình ông mặt trời | pic:☀️ | this picture | hình này
        Hình đồng hồ ba giờ | pic:🕒 | this picture | hình này
        Hình đồng hồ chín giờ | pic:🕘 | this picture | hình này
        Hình đồng hồ mười hai giờ | pic:🕛 | this picture | hình này
        Hình bạn ngủ say | pic:😴 | this picture | hình này
        Hình bạn cười | pic:😄 | this picture | hình này
        Hình bạn ngáp | pic:🥱 | this picture | hình này
        // Người và con vật
        Chú cao bồi | 🤠 | cowboy
        Chú công nhân | 👷 | builder
        Ông già Noel | 🎅 | Santa Claus
        Em bé sơ sinh | 👶 | baby
        Bạn nhỏ cười tươi | 😀 | smiling child
        Con báo đốm | 🐆 | leopard
        // Nhóm phân loại
        Nhóm có sọc vằn | 🦓 | striped ones
        Nhóm có chấm tròn | ⚫ | spotted ones
        Hộp đồ chơi | 📦 | toy box
        Ngăn quần áo | 👚 | clothes drawer
        Hộp màu đỏ | 🟥 | red box
        Hộp màu vàng | 🟨 | yellow box
        """;
}
