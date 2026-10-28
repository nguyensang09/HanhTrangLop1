namespace HanhTrangLop1.Application.Lessons;

public static partial class LessonBank
{
    // Câu dùng lại nhiều lần trong nhóm: cùng một câu tiếng Việt luôn có cùng một bản tiếng Anh.
    private static readonly Say LogicShadowPrompt = new("Bóng đen này là của ai? Con chạm vào bạn ấy nhé!", "Whose shadow is this? Tap the right one!");
    private static readonly Say LogicOddPrompt = new("Bạn nào không cùng nhóm với các bạn còn lại?", "Which one doesn't belong with the others?");

    /// <summary>Nhóm tu-duy-logic.</summary>
    private static void AddLogic(LessonGroupBuilder g)
    {
        // ---------------- Chặng 1: phân loại ----------------
        g.Stage = "Xếp vào nhóm";
        g.Topic = "phan-loai";

        g.Multi("logic-multi-animals", "Giải cứu các bạn con vật",
            new Say("Các bạn con vật bị lẫn vào đống đồ chơi. Con tìm các bạn ấy giúp cô nhé!", "Some animals got mixed up with the toys. Can you find all the animals for me?"),
            "🐾", ["Con mèo", "Ô tô", "Con cá", "Quả bóng", "Con chim"], ["Con mèo", "Con cá", "Con chim"],
            retry: new Say("Con vật thì biết ăn, biết chạy, biết bơi hoặc biết bay đấy!", "Animals eat, and they can run, swim or fly!"));

        g.Sort("logic-sort-food", "Giỏ trái cây và giỏ rau",
            new Say("Con giúp cô xếp trái cây vào một giỏ, rau củ vào giỏ kia nhé!", "Help me put the fruit in one basket and the vegetables in the other!"),
            "🧺", ["Giỏ trái cây", "Giỏ rau củ"],
            [("Quả táo", "Giỏ trái cây"), ("Quả chuối", "Giỏ trái cây"), ("Quả nho", "Giỏ trái cây"),
             ("Cà rốt", "Giỏ rau củ"), ("Bắp cải", "Giỏ rau củ"), ("Củ khoai tây", "Giỏ rau củ")],
            retry: new Say("Trái cây thường ngọt, ăn tráng miệng. Rau củ thường được nấu thành món ăn.", "Fruit is usually sweet, like a treat. Vegetables usually get cooked into a meal."));

        g.Pick("v5-logic-join-school-group", "Ai vào nhóm đồ dùng học tập?",
            new Say("Bút chì, thước kẻ, quyển vở là một nhóm. Bạn nào vào được nhóm này?", "A pencil, a ruler and a notebook go together. Which one can join them?"),
            "✏️", ["Quả chuối", "Bút màu", "Con mèo"], "Bút màu", scene: ["✏️", "📏", "📒", "?"],
            retry: new Say("Nhóm này là những đồ dùng để học, để viết và vẽ đấy!", "This group is things we use at school to write and draw!"));

        g.Match("logic-match-food", "Ai thích ăn gì?",
            new Say("Các bạn con vật đói bụng rồi! Con nối mỗi bạn với món bạn ấy thích nhé.", "The animals are hungry! Match each animal with its favorite food."),
            "🥕", [("Con thỏ", "Cà rốt"), ("Con khỉ", "Quả chuối"), ("Con gấu", "Mật ong")],
            retry: new Say("Con nghĩ xem: bạn nào hay ôm hũ mật ong nhỉ?", "Think: who loves to hug a honey pot?"));

        g.Sort("v5-logic-sort-fly-swim", "Bạn bay, bạn bơi",
            new Say("Bạn nào bay trên trời, bạn nào bơi dưới nước? Con xếp giúp cô nhé!", "Who flies in the sky and who swims in the water? Sort them for me!"),
            "🌊", ["Nhóm bay trên trời", "Nhóm bơi dưới nước"],
            [("Con chim", "Nhóm bay trên trời"), ("Con bướm", "Nhóm bay trên trời"), ("Con ong", "Nhóm bay trên trời"),
             ("Con cá", "Nhóm bơi dưới nước"), ("Con cá heo", "Nhóm bơi dưới nước"), ("Bạch tuộc", "Nhóm bơi dưới nước")],
            retry: new Say("Bạn nào có cánh thì bay được đấy!", "If it has wings, it can fly!"));

        g.Multi("logic-multi-school", "Chuẩn bị cặp sách vào lớp 1",
            new Say("Sắp vào lớp 1 rồi! Con chọn đồ dùng học tập để cho vào cặp nhé.", "First grade is coming! Pick the school things to put in your bag."),
            "🎒", ["Bút chì", "Gấu bông", "Quyển vở", "Que kem", "Thước kẻ"], ["Bút chì", "Quyển vở", "Thước kẻ"],
            retry: new Say("Que kem sẽ chảy mất, còn gấu bông thì để ở nhà nhé!", "Ice cream would melt, and the teddy bear can stay at home!"));

        g.Sort("logic-sort-weather", "Trời nắng, trời mưa",
            new Say("Trời nắng hay trời mưa? Con xếp đồ dùng hợp với từng kiểu thời tiết nhé!", "Sunny or rainy? Sort the things we need for each kind of weather!"),
            "🌦️", ["Đồ dùng ngày nắng", "Đồ dùng ngày mưa"],
            [("Kính râm", "Đồ dùng ngày nắng"), ("Mũ rộng vành", "Đồ dùng ngày nắng"),
             ("Cái ô", "Đồ dùng ngày mưa"), ("Đôi ủng", "Đồ dùng ngày mưa")],
            retry: new Say("Con nghĩ xem: đồ nào giúp con không bị ướt?", "Think: which things keep you dry?"));

        g.Match("v5-logic-match-go-together", "Đôi bạn thân",
            new Say("Mỗi đồ vật có một người bạn hay đi cùng. Con nối các đôi bạn nhé!", "Each thing has a buddy it goes with. Match the buddies!"),
            "🧦", [("Đôi tất", "Đôi giày"), ("Cái thìa", "Cái bát"), ("Bút chì", "Quyển vở")],
            retry: new Say("Con nghĩ xem: mình có dùng hai đồ này cùng lúc không?", "Think: do we use these two things together?"));

        g.Sort("logic-sort-living", "Ai biết lớn lên?",
            new Say("Con vật và cây cối biết lớn lên, còn đồ vật thì không. Con xếp giúp cô nhé!", "Animals and plants grow bigger, but things don't. Help me sort them!"),
            "🌱", ["Nhóm biết lớn lên", "Nhóm không tự lớn"],
            [("Gà con", "Nhóm biết lớn lên"), ("Cái cây", "Nhóm biết lớn lên"), ("Con mèo", "Nhóm biết lớn lên"),
             ("Cái ghế", "Nhóm không tự lớn"), ("Ô tô", "Nhóm không tự lớn"), ("Quả bóng", "Nhóm không tự lớn")],
            retry: new Say("Bạn nào cần ăn, uống nước để lớn lên nào?", "Which ones need food and water to grow?"));

        // ---------------- Chặng 2: quy luật hình ảnh ----------------
        g.Stage = "Đoán hình tiếp theo";
        g.Topic = "quy-luat";

        g.Pick("v5-logic-next-apple-banana", "Táo, chuối, táo, chuối...",
            new Say("Táo, chuối, táo, chuối... Tiếp theo là quả gì nhỉ?", "Apple, banana, apple, banana... What comes next?"),
            "🍌", ["Quả táo", "Quả chuối", "Quả nho"], "Quả chuối", scene: ["🍎", "🍌", "🍎", "🍌", "🍎", "?"],
            retry: new Say("Con đọc to theo cô: táo, chuối, táo, chuối, táo...", "Say it with me: apple, banana, apple, banana, apple..."));

        g.Pick("v5-logic-next-day-night", "Ngày rồi lại đêm",
            new Say("Mặt trời, mặt trăng, mặt trời... Tiếp theo là gì?", "Sun, moon, sun... What comes next?"),
            "🌙", ["Mặt trời", "Mặt trăng", "Ngôi sao"], "Mặt trăng", scene: ["☀️", "🌙", "☀️", "🌙", "☀️", "?"],
            retry: new Say("Ban ngày có mặt trời, ban đêm có mặt trăng, cứ thế lặp lại.", "Daytime has the sun, then night has the moon, again and again."));

        g.Pick("v5-logic-next-beads", "Xâu vòng hạt",
            new Say("Con giúp cô xâu nốt chiếc vòng. Hạt tiếp theo màu gì?", "Help me finish my necklace. What color is the next bead?"),
            "📿", ["Hạt vòng đỏ", "Hạt vòng xanh dương", "Hạt vòng vàng"], "Hạt vòng vàng", scene: ["🔴", "🔵", "🟡", "🔴", "🔵", "?"],
            retry: new Say("Cứ ba hạt lại lặp lại: đỏ, xanh, vàng.", "Every three beads repeat: red, blue, yellow."));

        g.Order("v5-logic-order-bead-pattern", "Xâu tiếp theo mẫu",
            new Say("Vòng hạt cứ lặp lại như trong hình. Con chạm ba hạt theo đúng thứ tự nhé!", "The beads repeat just like in the picture. Tap three beads in the right order!"),
            "📿", ["Hạt vòng xanh lá", "Hạt vòng đỏ", "Hạt vòng vàng"], scene: ["🟢", "🔴", "🟡", "🟢", "🔴", "🟡"],
            retry: new Say("Con nhìn hàng hạt từ trái sang phải: xanh lá, đỏ, vàng.", "Look at the beads from left to right: green, red, yellow."));

        g.Pick("v5-logic-next-cat-cat-dog", "Hai mèo, một chó",
            new Say("Mèo, mèo, chó... rồi lại mèo, mèo. Bạn nào đến tiếp theo?", "Cat, cat, dog... then cat, cat again. Who comes next?"),
            "🐶", ["Con mèo", "Con chó", "Con thỏ"], "Con chó", scene: ["🐱", "🐱", "🐶", "🐱", "🐱", "?"],
            retry: new Say("Cứ hai bạn mèo thì có một bạn chó.", "After every two cats comes one dog."));

        g.Multi("v5-logic-missing-beads", "Chiếc vòng bị rơi hạt",
            new Say("Chiếc vòng bị rơi mất hai hạt! Con tìm hai hạt còn thiếu nhé.", "My necklace lost two beads! Find the two missing beads."),
            "📿", ["Hạt vòng đỏ", "Hạt vòng xanh dương", "Hạt vòng vàng", "Hạt vòng xanh lá"], ["Hạt vòng đỏ", "Hạt vòng xanh lá"],
            scene: ["🔴", "🟢", "?", "🟢", "🔴", "?"],
            retry: new Say("Vòng này chỉ có hai màu, cứ xen kẽ nhau.", "This necklace has just two colors, taking turns."));

        g.Pick("v5-logic-next-flower-leaf", "Một hoa, hai lá",
            new Say("Hoa, lá, lá, hoa, lá... Tiếp theo là gì?", "Flower, leaf, leaf, flower, leaf... What comes next?"),
            "🌸", ["Bông hoa", "Chiếc lá", "Cái cây"], "Chiếc lá", scene: ["🌸", "🍃", "🍃", "🌸", "🍃", "?"],
            retry: new Say("Sau mỗi bông hoa có hai chiếc lá đấy.", "Each flower is followed by two leaves."));

        g.Pick("v5-logic-missing-middle", "Bạn nào trốn ở giữa?",
            new Say("Có một bạn trốn trong ô dấu hỏi. Đó là bạn nào?", "Someone is hiding in the question mark box. Who is it?"),
            "❓", ["Con ếch", "Con cá", "Con rùa"], "Con cá", scene: ["🐸", "🐟", "🐸", "?", "🐸", "🐟"],
            retry: new Say("Con đọc to cả hàng: ếch, cá, ếch... rồi đến ai?", "Read the row out loud: frog, fish, frog... then who?"));

        g.Pick("v5-logic-next-growing-apples", "Táo nhiều dần lên",
            new Say("Mỗi đĩa lại thêm một quả táo. Đĩa tiếp theo có mấy quả?", "Each plate gets one more apple. How many on the next plate?"),
            "🍎", [QuantityLabel(CountThemes[0], 3), QuantityLabel(CountThemes[0], 5), QuantityLabel(CountThemes[0], 4)], QuantityLabel(CountThemes[0], 4),
            scene: ["🍎", "🍎🍎", "🍎🍎🍎", "?"],
            retry: new Say("Con đếm từng đĩa: một, hai, ba... rồi đến mấy?", "Count each plate: one, two, three... and then?"));

        g.Pick("v5-logic-next-abc-fruit", "Ba quả xếp hàng",
            new Say("Táo, chuối, nho, táo, chuối... Quả nào đến tiếp theo?", "Apple, banana, grapes, apple, banana... Which fruit comes next?"),
            "🍇", ["Quả táo", "Quả nho", "Quả cam"], "Quả nho", scene: ["🍎", "🍌", "🍇", "🍎", "🍌", "?"],
            retry: new Say("Ba quả cứ lặp lại đúng thứ tự: táo, chuối, nho.", "The three fruits repeat in order: apple, banana, grapes."));

        // ---------------- Chặng 3: ghép bóng ----------------
        g.Stage = "Bóng của ai?";
        g.Topic = "ghep-bong";

        g.Pick("v5-logic-shadow-elephant", "Bóng của chú voi", LogicShadowPrompt,
            "🐘", ["Con voi", "Con chuột", "Con gà"], "Con voi", focus: "shadow:🐘",
            retry: new Say("Bóng này có cái vòi dài và đôi tai to đấy!", "This shadow has a long trunk and big ears!"));

        g.Pick("v5-logic-shadow-butterfly", "Bóng có bốn cánh", LogicShadowPrompt,
            "🦋", ["Con chim", "Con bướm", "Con ong"], "Con bướm", focus: "shadow:🦋",
            retry: new Say("Bóng này có bốn cánh rất to, xòe ra hai bên.", "This shadow has four big wings spread out wide."));

        g.Match("v5-logic-shadow-match-animals", "Nối bạn với bóng",
            new Say("Các bạn đang tìm cái bóng của mình. Con nối mỗi bạn với đúng bóng nhé!", "Everyone is looking for their shadow. Match each one with its own shadow!"),
            "🐰", [("Con mèo", "Bóng con mèo"), ("Con thỏ", "Bóng con thỏ"), ("Con ốc sên", "Bóng con ốc sên")],
            retry: new Say("Con nhìn hình dáng: tai dài, tai nhọn hay cái vỏ tròn?", "Look at the shape: long ears, pointy ears, or a round shell?"));

        g.Pick("v5-logic-shadow-bicycle", "Bóng chiếc xe", LogicShadowPrompt,
            "🚲", ["Xe máy", "Xe đạp", "Ô tô"], "Xe đạp", focus: "shadow:🚲",
            retry: new Say("Con nhìn kỹ: khung xe mảnh và có bàn đạp.", "Look closely: a thin frame and pedals."));

        g.Multi("v5-logic-shadow-flyers", "Bóng các bạn biết bay",
            new Say("Có bóng của bạn biết bay và bạn không biết bay. Con tìm bóng các bạn biết bay nhé!", "Some shadows belong to flyers and some don't. Find the shadows of the ones that fly!"),
            "🐦", ["Bóng con chim", "Bóng con cá", "Bóng con bướm", "Bóng con mèo"], ["Bóng con chim", "Bóng con bướm"],
            retry: new Say("Con tìm những cái bóng có đôi cánh nhé!", "Look for the shadows with wings!"));

        g.Match("v5-logic-shadow-match-things", "Bóng đồ vật",
            new Say("Đồ vật cũng có bóng! Con nối mỗi đồ vật với bóng của nó nhé.", "Things have shadows too! Match each thing with its shadow."),
            "✂️", [("Cái kéo", "Bóng cái kéo"), ("Cái ô", "Bóng cái ô"), ("Cái chìa khóa", "Bóng cái chìa khóa")],
            retry: new Say("Con nhìn kỹ: hai cái lỗ tròn, cán cong hay răng cưa nhỏ?", "Look closely: two round holes, a curved handle, or little teeth?"));

        g.Pick("v5-logic-shadow-giraffe", "Bóng cổ dài", LogicShadowPrompt,
            "🦒", ["Con ngựa", "Con hươu cao cổ", "Con bò"], "Con hươu cao cổ", focus: "shadow:🦒",
            retry: new Say("Bạn này có cái cổ dài nhất đấy!", "This one has the longest neck!"));

        g.Match("v5-logic-shadow-match-fruit", "Bóng trái cây",
            new Say("Trên đĩa chỉ còn lại bóng trái cây. Con nối mỗi quả với bóng của nó nhé!", "Only the fruit shadows are left on the plates! Match each fruit with its shadow."),
            "🍍", [("Quả táo", "Bóng quả táo"), ("Quả chuối", "Bóng quả chuối"), ("Quả dứa", "Bóng quả dứa")],
            retry: new Say("Quả chuối cong cong, quả dứa có chùm lá trên đầu.", "A banana is curvy, and a pineapple has leaves on top."));

        g.Pick("v5-logic-shadow-plane", "Bóng trên bầu trời", LogicShadowPrompt,
            "✈️", ["Trực thăng", "Máy bay", "Tên lửa"], "Máy bay", focus: "shadow:✈️",
            retry: new Say("Bóng này có đôi cánh dài dang rộng hai bên.", "This shadow has long wings stretched out."));

        // ---------------- Chặng 4: tìm điểm khác biệt ----------------
        g.Stage = "Ai khác nhóm?";
        g.Topic = "tim-khac-biet";

        g.Pick("v5-logic-odd-in-row", "Ai lạc vào hàng mèo?",
            new Say("Cả hàng đều là mèo, chỉ có một bạn khác. Đó là bạn nào?", "The whole row is cats except one. Who is different?"),
            "🐱", ["Con mèo", "Con chó", "Con thỏ"], "Con chó", scene: ["🐱", "🐱", "🐶", "🐱", "🐱"],
            retry: new Say("Con nhìn từng bạn một, từ trái sang phải nhé.", "Look at each one, from left to right."));

        g.Pick("v5-logic-odd-tomato", "Quả táo giả",
            new Say("Cô xếp một đĩa táo, nhưng có một quả không phải táo. Đó là quả gì?", "I made a plate of apples, but one isn't an apple. What is it?"),
            "🍅", ["Quả táo", "Cà chua", "Quả đào"], "Cà chua", scene: ["🍎", "🍎", "🍎", "🍅", "🍎"],
            retry: new Say("Con nhìn kỹ phần cuống ở trên mỗi quả nhé.", "Look closely at the top of each one."));

        g.Pick("v5-logic-odd-car", "Lạc vào giỏ trái cây", LogicOddPrompt,
            "🚗", ["Quả táo", "Quả chuối", "Ô tô", "Quả cam"], "Ô tô",
            retry: new Say("Ba bạn ăn được, còn một bạn thì không!", "You can eat three of them, but not the other one!"));

        g.Multi("v5-logic-odd-two-in-basket", "Hai vị khách lạ",
            new Say("Có hai thứ lạc vào giỏ trái cây. Con tìm và nhấc chúng ra nhé!", "Two things sneaked into the fruit basket. Find them and take them out!"),
            "🧺", ["Quả nho", "Cái kéo", "Quả lê", "Đôi giày", "Quả dưa hấu"], ["Cái kéo", "Đôi giày"],
            retry: new Say("Con tìm những thứ không ăn được nhé.", "Find the things you can't eat."));

        g.Pick("v5-logic-odd-water", "Ai không sống dưới nước?", LogicOddPrompt,
            "🐟", ["Con cá", "Con mèo", "Con cá heo", "Bạch tuộc"], "Con mèo",
            retry: new Say("Ba bạn sống dưới nước. Bạn nào sống trên cạn?", "Three of them live in the water. Which one lives on land?"));

        g.Multi("v5-logic-odd-no-wings", "Ai không có cánh?",
            new Say("Con tìm những bạn không có cánh nhé!", "Find the animals that don't have wings!"),
            "🐔", ["Con gà", "Con chó", "Con vịt", "Con chim", "Con mèo"], ["Con chó", "Con mèo"],
            retry: new Say("Bạn có cánh thì có lông vũ và hai chân.", "Animals with wings have feathers and two legs."));

        g.Pick("v5-logic-odd-clothes", "Lạc vào tủ quần áo", LogicOddPrompt,
            "👗", ["Cái áo", "Cái thìa", "Cái váy", "Cái quần"], "Cái thìa",
            retry: new Say("Ba thứ để mặc, còn một thứ để ăn cơm.", "Three are things to wear, and one is for eating."));

        g.Pick("v5-logic-odd-plane", "Ai không chạy trên đường?", LogicOddPrompt,
            "✈️", ["Xe đạp", "Ô tô", "Máy bay", "Xe buýt"], "Máy bay",
            retry: new Say("Ba bạn chạy trên đường. Bạn nào bay trên trời?", "Three of them go on the road. Which one flies in the sky?"));

        // ---------------- Chặng 5: sắp xếp theo kích thước và trình tự ----------------
        g.Stage = "Nhỏ, lớn, trước, sau";
        g.Topic = "quy-luat";

        g.Order("v5-logic-size-small-big", "Từ nhỏ đến lớn",
            new Say("Con xếp các bạn từ nhỏ nhất đến to nhất nhé!", "Line them up from the smallest to the biggest!"),
            "🐜", ["Con kiến", "Con mèo", "Con voi"],
            retry: new Say("Bạn nào bé tí, có thể bò trên lòng bàn tay con?", "Who is tiny enough to crawl on your hand?"));

        g.Pick("v5-logic-size-smallest", "Bạn nào bé nhất?",
            new Say("Bạn nào bé nhất? Con chạm vào bạn ấy nhé!", "Who is the smallest? Tap that one!"),
            "🔎", ["Con chó", "Con ong", "Con gà"], "Con ong",
            retry: new Say("Bạn ấy nhỏ đến mức đậu được trên một bông hoa!", "It's so small it can sit on a flower!"));

        g.Order("logic-order-plant", "Cây lớn lên",
            new Say("Cây lớn lên thế nào? Con xếp theo thứ tự trước, sau nhé!", "How does a tree grow? Put them in order!"),
            "🌱", ["Hạt giống", "Mầm cây", "Cái cây"],
            retry: new Say("Cây nào cũng bắt đầu từ một hạt nhỏ xíu.", "Every tree starts from a tiny seed."));

        g.Sort("v5-logic-sort-pocket", "Có vừa túi áo không?",
            new Say("Đồ nào bỏ vừa túi áo, đồ nào quá to? Con xếp giúp cô nhé!", "Which things fit in your pocket, and which are too big? Sort them for me!"),
            "👖", ["Bỏ vừa túi áo", "Quá to để bỏ túi"],
            [("Cái chìa khóa", "Bỏ vừa túi áo"), ("Kẹo", "Bỏ vừa túi áo"), ("Hạt dẻ", "Bỏ vừa túi áo"),
             ("Xe đạp", "Quá to để bỏ túi"), ("Cái ghế", "Quá to để bỏ túi"), ("Con voi", "Quá to để bỏ túi")],
            retry: new Say("Con thử tưởng tượng nhét nó vào túi áo xem có vừa không!", "Imagine putting it in your pocket. Does it fit?"));

        g.Order("v5-logic-size-big-small-sea", "Từ to đến nhỏ dưới biển",
            new Say("Dưới biển có bạn to, bạn nhỏ. Con xếp từ to nhất đến nhỏ nhất nhé!", "Big and small friends live in the sea. Line them up from biggest to smallest!"),
            "🐳", ["Cá voi", "Con cá heo", "Con tôm"],
            retry: new Say("Cá voi là con vật to nhất dưới biển đấy!", "The whale is the biggest animal in the sea!"));

        g.Order("v5-logic-order-chicken", "Từ quả trứng đến chú gà",
            new Say("Chú gà lớn lên từ đâu? Con xếp theo thứ tự nhé!", "Where does a chicken come from? Put them in order!"),
            "🥚", ["Quả trứng", "Gà con", "Con gà"],
            retry: new Say("Gà mẹ đẻ trứng, trứng nở ra gà con.", "A hen lays an egg, and a chick hatches out."));

        g.Order("v5-logic-order-rain", "Trời mưa rồi tạnh",
            new Say("Trời sắp mưa! Con xếp các hình theo thứ tự chuyện xảy ra nhé.", "Rain is coming! Put the pictures in the order they happen."),
            "🌈", ["Đám mây", "Trời đổ mưa", "Cầu vồng"],
            retry: new Say("Mây kéo đến trước, mưa xong mới có cầu vồng.", "Clouds come first. The rainbow comes after the rain."));

        g.Order("v5-logic-order-day", "Một ngày của ông mặt trời",
            new Say("Ông mặt trời đi đâu cả ngày? Con xếp từ sáng sớm đến tối nhé!", "Where does the sun go all day? Put them in order from morning to night!"),
            "🌅", ["Lúc mặt trời mọc", "Buổi trưa nắng chang chang", "Lúc hoàng hôn", "Đêm có trăng sao"],
            retry: new Say("Sáng sớm mặt trời mọc, buổi tối thì trăng lên.", "In the morning the sun comes up, and at night the moon comes out."));

        // ---------------- Chặng 6: thám tử nhí (tổng hợp, khó hơn) ----------------
        g.Stage = "Thám tử nhí";

        g.Topic = "phan-loai";
        g.Pick("v5-logic-riddle-wings", "Đố vui: có cánh mà không phải chim",
            new Say("Đố con: bạn này có cánh, biết bay, nhưng không phải là chim. Là ai?", "Riddle: it has wings and can fly, but it isn't a bird. Who is it?"),
            "🦋", ["Con chim", "Con bướm", "Con mèo"], "Con bướm",
            retry: new Say("Bạn ấy có đôi cánh nhiều màu, thích đậu trên hoa.", "It has colorful wings and loves to sit on flowers."));

        g.Topic = "quy-luat";
        g.Pick("v5-logic-next-aabb", "Từng đôi một",
            new Say("Chó, chó, mèo, mèo, chó, chó... Tiếp theo là bạn nào?", "Dog, dog, cat, cat, dog, dog... Who comes next?"),
            "🐱", ["Con chó", "Con mèo", "Con chuột"], "Con mèo", scene: ["🐶", "🐶", "🐱", "🐱", "🐶", "🐶", "?"],
            retry: new Say("Các bạn đứng từng đôi: hai chó rồi hai mèo.", "They come in pairs: two dogs, then two cats."));

        g.Topic = "phan-loai";
        g.Sort("v5-logic-sort-kitchen-bag", "Trong bếp hay trong cặp?",
            new Say("Đồ nào để trong bếp, đồ nào để trong cặp sách? Con xếp giúp cô nhé!", "Which things belong in the kitchen, and which go in the school bag? Sort them for me!"),
            "🍳", ["Để trong bếp", "Để trong cặp sách"],
            [("Cái nồi", "Để trong bếp"), ("Cái bát", "Để trong bếp"), ("Cái thìa", "Để trong bếp"),
             ("Bút chì", "Để trong cặp sách"), ("Thước kẻ", "Để trong cặp sách"), ("Quyển sách", "Để trong cặp sách")],
            retry: new Say("Đồ dùng để nấu ăn thì ở trong bếp nhé.", "Things for cooking stay in the kitchen."));

        g.Topic = "ghep-bong";
        g.Match("v5-logic-shadow-match-sky", "Bóng bay qua bầu trời",
            new Say("Có ba cái bóng bay qua trên trời. Con nối mỗi bóng với đúng bạn nhé!", "Three shadows are flying across the sky. Match each shadow to the right one!"),
            "🚁", [("Máy bay", "Bóng máy bay"), ("Trực thăng", "Bóng trực thăng"), ("Tên lửa", "Bóng tên lửa")],
            retry: new Say("Trực thăng có cánh quạt ở trên đầu, tên lửa thì thon và nhọn.", "A helicopter has blades on top, and a rocket is long and pointy."));

        g.Topic = "phan-loai";
        g.Multi("v5-logic-four-legs", "Ai có bốn chân?",
            new Say("Con tìm tất cả các bạn có bốn chân nhé!", "Find all the animals with four legs!"),
            "🐴", ["Con mèo", "Con gà", "Con bò", "Con cá", "Con ngựa"], ["Con mèo", "Con bò", "Con ngựa"],
            retry: new Say("Con gà chỉ có hai chân, còn con cá thì không có chân!", "A chicken has only two legs, and a fish has none!"));

        g.Topic = "quy-luat";
        g.Order("v5-logic-size-short-tall", "Từ thấp đến cao",
            new Say("Con xếp các bạn từ thấp nhất đến cao nhất nhé!", "Line them up from the shortest to the tallest!"),
            "🦒", ["Con gà", "Con ngựa", "Con hươu cao cổ"],
            retry: new Say("Bạn hươu cao cổ với tới tận ngọn cây đấy!", "A giraffe can reach the treetops!"));

        g.Topic = "tim-khac-biet";
        g.Pick("v5-logic-odd-color", "Ai khác màu?",
            new Say("Ba bạn có cùng một màu. Bạn nào khác màu?", "Three of them are the same color. Which one is different?"),
            "🎨", ["Quả chanh", "Quả chuối", "Quả dâu tây", "Hoa hướng dương"], "Quả dâu tây",
            retry: new Say("Con chỉ nhìn màu thôi nhé: vàng, vàng, vàng... và màu gì?", "Just look at the colors: yellow, yellow, yellow... and what?"));

        g.Topic = "quy-luat";
        g.Pick("v5-logic-analogy-grow", "Lớn lên thành gì?",
            new Say("Gà con lớn lên thành gà. Còn hạt giống lớn lên thành gì?", "A chick grows into a chicken. What does a seed grow into?"),
            "🌳", ["Quả trứng", "Cái cây", "Con chim"], "Cái cây", scene: ["🐤", "➡️", "🐔", "🫘", "➡️", "?"],
            retry: new Say("Gieo hạt xuống đất, tưới nước, hạt sẽ nảy mầm rồi lớn lên.", "Plant a seed and water it, and it sprouts and grows."));

        g.Topic = "phan-loai";
        g.Pick("v5-logic-analogy-weather", "Mưa cầm ô, nắng đội gì?",
            new Say("Trời mưa thì cầm ô. Trời nắng to thì đội gì nhỉ?", "When it rains, we take an umbrella. What do we wear when it's very sunny?"),
            "👒", ["Đôi ủng", "Mũ rộng vành", "Khăn quàng"], "Mũ rộng vành", scene: ["🌧️", "☂️", "☀️", "?"],
            retry: new Say("Đồ này che nắng cho đầu và mặt con.", "It shades your head and face from the sun."));
    }
}

public static partial class Lexicon
{
    /// <summary>
    /// Nhãn riêng của nhóm Tư duy logic. Nhãn "Bóng ..." hiện bóng đen và ẩn chữ; cô chỉ đọc "cái bóng này"
    /// để không lộ đáp án.
    /// </summary>
    private const string LogicTable = """
        // Nhóm phân loại
        Giỏ trái cây | 🧺 | fruit basket
        Giỏ rau củ | 🥗 | vegetable basket
        Nhóm bay trên trời | 🌤️ | sky flyers
        Nhóm bơi dưới nước | 🌊 | water swimmers
        Đồ dùng ngày nắng | ☀️ | things for sunny days
        Đồ dùng ngày mưa | 🌧️ | things for rainy days
        Nhóm biết lớn lên | 🌱 | things that grow
        Nhóm không tự lớn | 🧱 | things that don't grow
        Bỏ vừa túi áo | 👖 | fits in a pocket
        Quá to để bỏ túi | 🙅 | too big for a pocket
        Để trong bếp | 🍳 | goes in the kitchen
        Để trong cặp sách | 🎒 | goes in the school bag
        // Hạt vòng cho bài quy luật
        Hạt vòng đỏ | 🔴 | red bead
        Hạt vòng xanh dương | 🔵 | blue bead
        Hạt vòng vàng | 🟡 | yellow bead
        Hạt vòng xanh lá | 🟢 | green bead
        // Bóng đen (ẩn chữ, không đọc lộ đáp án)
        Bóng con voi | shadow:🐘 | this shadow | cái bóng này
        Bóng con mèo | shadow:🐱 | this shadow | cái bóng này
        Bóng con thỏ | shadow:🐰 | this shadow | cái bóng này
        Bóng con ốc sên | shadow:🐌 | this shadow | cái bóng này
        Bóng con chim | shadow:🐦 | this shadow | cái bóng này
        Bóng con cá | shadow:🐟 | this shadow | cái bóng này
        Bóng con bướm | shadow:🦋 | this shadow | cái bóng này
        Bóng cái kéo | shadow:✂️ | this shadow | cái bóng này
        Bóng cái ô | shadow:☂️ | this shadow | cái bóng này
        Bóng cái chìa khóa | shadow:🔑 | this shadow | cái bóng này
        Bóng quả táo | shadow:🍎 | this shadow | cái bóng này
        Bóng quả chuối | shadow:🍌 | this shadow | cái bóng này
        Bóng quả dứa | shadow:🍍 | this shadow | cái bóng này
        Bóng máy bay | shadow:✈️ | this shadow | cái bóng này
        Bóng trực thăng | shadow:🚁 | this shadow | cái bóng này
        Bóng tên lửa | shadow:🚀 | this shadow | cái bóng này
        // Trình tự
        Trời đổ mưa | 🌧️ | rain falling
        Lúc mặt trời mọc | 🌅 | sunrise
        Buổi trưa nắng chang chang | ☀️ | sunny midday
        Lúc hoàng hôn | 🌇 | sunset
        Đêm có trăng sao | 🌃 | night with moon and stars
        """;
}
