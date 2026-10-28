namespace HanhTrangLop1.Application.Lessons;

public static partial class LessonBank
{
    /// <summary>Lời dặn chung cho bài nghe tiếng kêu (không lộ đáp án; lời cô đọc nằm ở "spoken").</summary>
    private static readonly Say WorldListenSound = new("Con lắng nghe tiếng kêu rồi chạm vào con vật đó nhé!", "Listen to the sound, then tap the animal that makes it!");
    /// <summary>Lời dặn chung cho bài nghe câu hỏi (câu hỏi thật nằm ở "spoken").</summary>
    private static readonly Say WorldListenAsk = new("Con lắng nghe cô hỏi rồi chạm vào câu trả lời đúng nhé!", "Listen to my question, then tap the right answer!");

    /// <summary>Nhóm kham-pha: Khám phá thế giới.</summary>
    private static void AddWorld(LessonGroupBuilder g)
    {
        // ───────────── Chặng 1: con vật quanh nhà ─────────────
        g.Stage = "Bạn thú quanh nhà";
        g.Topic = "con-vat";

        g.Listen("world-listen-cat", "Ai kêu meo meo?", WorldListenSound, "🐱",
            ["Con chó", "Con mèo", "Con gà"], "Con mèo",
            spoken: new Say("Meo meo! Con vật nào kêu meo meo?", "Meow, meow! Which animal says meow?"),
            retry: new Say("Bạn này thích bắt chuột và kêu meo meo đó!", "This one likes to chase mice and says meow!"));

        g.Pick("v5-world-cat-food", "Mèo thích ăn gì?",
            new Say("Bạn mèo đói bụng rồi. Con đoán xem mèo thích ăn gì nhất?", "The cat is hungry. Can you guess what a cat likes to eat most?"),
            "🐟", ["Cà rốt", "Con cá", "Bắp ngô"], "Con cá", scene: ["🐱", "🍽️", "?"],
            retry: new Say("Mèo rất thích món sống dưới nước đó con!", "Cats love a food that swims in the water!"));

        g.Listen("world-listen-dog", "Ai kêu gâu gâu?", WorldListenSound, "🐶",
            ["Con mèo", "Con bò", "Con chó"], "Con chó",
            spoken: new Say("Gâu gâu! Con vật nào kêu gâu gâu để giữ nhà?", "Woof, woof! Which animal barks to guard the house?"),
            retry: new Say("Bạn này vẫy đuôi mừng khi con đi học về đó!", "This one wags its tail when you come home!"));

        g.Match("world-food", "Món ăn của các bạn thú",
            new Say("Các bạn thú đói rồi! Con nối mỗi bạn với món ăn yêu thích nhé!", "The animals are hungry! Match each one with its favourite food!"),
            "🥕", [("Con thỏ", "Cà rốt"), ("Con khỉ", "Quả chuối"), ("Con gấu", "Mật ong")],
            retry: new Say("Thỏ gặm cà rốt, khỉ bóc chuối, gấu mê mật ong nhé!", "Rabbits munch carrots, monkeys peel bananas, bears love honey!"));

        g.Pick("v5-world-milk", "Sữa từ đâu mà có?",
            new Say("Cốc sữa thơm ngon này đến từ con vật nào nhỉ?", "This yummy glass of milk comes from which animal?"),
            "🥛", ["Con gà", "Con bò", "Con chó"], "Con bò", scene: ["🥛", "?"],
            retry: new Say("Bạn này có sừng, ăn cỏ và kêu ụm bò đó!", "This one has horns, eats grass and says moo!"));

        g.Sort("v5-world-sort-home-wild", "Thú nuôi hay thú rừng?",
            new Say("Bạn nào sống cùng nhà mình, bạn nào sống trong rừng? Con xếp giúp cô nhé!", "Who lives with us at home, and who lives in the forest? Sort them for me!"),
            "🏡", ["Vật nuôi trong nhà", "Thú trong rừng"],
            [("Con chó", "Vật nuôi trong nhà"), ("Con mèo", "Vật nuôi trong nhà"), ("Con gà", "Vật nuôi trong nhà"),
             ("Con hổ", "Thú trong rừng"), ("Con sư tử", "Thú trong rừng"), ("Con voi", "Thú trong rừng")],
            retry: new Say("Hổ và sư tử rất dữ, chúng sống ngoài rừng chứ không ở trong nhà đâu!", "Tigers and lions are wild. They live in the forest, not in our house!"));

        g.Listen("world-listen-duck", "Ai kêu cạp cạp?", WorldListenSound, "🦆",
            ["Con gà", "Con lợn", "Con vịt"], "Con vịt",
            spoken: new Say("Cạp cạp! Con vật nào vừa kêu cạp cạp vừa bơi dưới ao?", "Quack, quack! Which animal quacks and swims in the pond?"),
            retry: new Say("Bạn này có cái mỏ dẹt và bơi rất giỏi đó!", "This one has a flat beak and is a great swimmer!"));

        g.Story("v5-world-story-bear-honey", "Gấu nâu tìm mật",
            new Say("Gấu nâu đi dạo trong rừng. Bạn ấy thấy một tổ ong vàng treo trên cành cây. Gấu thò tay lấy mật ngọt, đầy cả một hũ. Các chú ong bay vo ve quanh tổ.",
                "Brown Bear went for a walk in the forest. He found a big yellow beehive hanging from a branch. He scooped out sweet honey and filled a whole jar. The bees buzzed all around."),
            new Say("Gấu nâu lấy món gì ngọt ngào từ tổ ong?", "What sweet treat did Brown Bear take from the beehive?"),
            "🐻", ["Quả táo", "Mật ong", "Que kem"], "Mật ong", "/images/lessons/doc-hieu/ill-gau-mat-ong.jpg",
            retry: new Say("Con nhìn cái hũ vàng gấu đang ôm nhé, ong làm ra món đó!", "Look at the yellow jar in Bear's arm. The bees make it!"));

        g.Listen("v5-world-listen-rooster", "Ai gáy ò ó o?", WorldListenSound, "🐓",
            ["Con vịt", "Con gà trống", "Con chó"], "Con gà trống",
            spoken: new Say("Ò ó o! Con vật nào gáy vang mỗi sáng gọi mọi người dậy?", "Cock-a-doodle-doo! Which animal crows every morning to wake everyone up?"),
            retry: new Say("Bạn này có cái mào đỏ trên đầu đó!", "This one has a red comb on its head!"));

        // ───────────── Chặng 2: nơi sống, biết bay, biết bơi ─────────────
        g.Stage = "Nhà của muôn loài";
        g.Topic = "con-vat";

        g.Sort("world-habitat", "Trên cạn hay dưới nước?",
            new Say("Mỗi bạn sống ở đâu? Con đưa bạn lên bờ hoặc xuống nước nhé!", "Where does each animal live? Put it on land or in the water!"),
            "🌊", ["Sống trên cạn", "Sống dưới nước"],
            [("Con mèo", "Sống trên cạn"), ("Con thỏ", "Sống trên cạn"), ("Con cá", "Sống dưới nước"), ("Con tôm", "Sống dưới nước"), ("Bạch tuộc", "Sống dưới nước")],
            retry: new Say("Bạn nào có vây hoặc nhiều chân để bơi thì sống dưới nước nhé!", "Animals with fins or many swimming legs live in the water!"));

        g.Multi("world-multi-fly", "Ai biết bay?",
            new Say("Bạn nào có thể bay lên trời? Con chọn hết các bạn đó nhé!", "Who can fly up into the sky? Tap all of them!"),
            "🦋", ["Con chim", "Con cá", "Con bướm", "Con mèo", "Con ong"], ["Con chim", "Con bướm", "Con ong"],
            retry: new Say("Con tìm các bạn có đôi cánh nhé, có ba bạn đấy!", "Look for animals with wings. There are three!"));

        g.Pick("v5-world-giraffe", "Ai với tới lá cao?",
            new Say("Lá non ngon nằm tít trên ngọn cây. Bạn nào ăn được nhờ cái cổ thật dài?", "The tasty leaves are at the top of the tree. Who can reach them with a very long neck?"),
            "🦒", ["Con ngựa", "Con hươu cao cổ", "Con lợn"], "Con hươu cao cổ", scene: ["🌳", "🍃", "?"],
            retry: new Say("Con tìm bạn có cái cổ dài nhất nhé!", "Find the animal with the longest neck!"));

        g.Listen("v5-world-listen-frog", "Ai kêu ộp ộp?", WorldListenSound, "🐸",
            ["Con ếch", "Con cá", "Con rùa"], "Con ếch",
            spoken: new Say("Ộp ộp! Con vật nào nhảy bên bờ ao và kêu ộp ộp?", "Ribbit, ribbit! Which animal hops by the pond and croaks?"),
            retry: new Say("Bạn này màu xanh, nhảy rất xa và kêu ộp ộp!", "This one is green, jumps far and goes ribbit!"));

        g.Match("v5-world-match-habitat", "Ai ở xứ nào?",
            new Say("Ba bạn bị lạc đường! Con đưa mỗi bạn về đúng nơi ở nhé!", "Three animals are lost! Take each one back to its home!"),
            "🧭", [("Con lạc đà", "Sa mạc cát nóng"), ("Chim cánh cụt", "Xứ băng tuyết"), ("Con cá heo", "Biển xanh")],
            retry: new Say("Lạc đà chịu nóng giỏi, chim cánh cụt thích lạnh, cá heo bơi ngoài biển nhé!", "Camels love the heat, penguins love the cold, and dolphins swim in the sea!"));

        g.Multi("world-multi-water", "Ai bơi giỏi?",
            new Say("Con chọn tất cả các bạn sống và bơi dưới nước nhé!", "Tap all the animals that live and swim in the water!"),
            "🐠", ["Con cá", "Con thỏ", "Con tôm", "Con gà", "Cá voi"], ["Con cá", "Con tôm", "Cá voi"],
            retry: new Say("Thỏ và gà sống trên cạn đó. Con tìm các bạn còn lại nhé!", "Rabbits and chickens live on land. Find the others!"));

        g.Pick("v5-world-penguin", "Chim cánh cụt giỏi gì?",
            new Say("Chim cánh cụt có cánh nhưng không bay được. Bạn ấy giỏi làm gì nhất?", "Penguins have wings but can't fly. What are they really good at?"),
            "🐧", ["Bay thật cao", "Bơi thật giỏi", "Leo lên cây cao"], "Bơi thật giỏi", scene: ["🐧", "🌊"],
            retry: new Say("Chim cánh cụt lặn xuống biển bắt cá rất nhanh đó con!", "Penguins dive into the sea and catch fish very fast!"));

        g.Pick("v5-world-elephant", "Cái vòi của voi",
            new Say("Bạn nào dùng cái vòi thật dài để hút nước và tắm mát?", "Who uses a long trunk to drink water and take a shower?"),
            "🐘", ["Con hổ", "Con khỉ", "Con voi"], "Con voi", scene: ["💦", "?"],
            retry: new Say("Con tìm bạn to nhất, có đôi tai to như chiếc quạt nhé!", "Find the biggest animal with ears like big fans!"));

        g.Sort("v5-world-sort-fly-swim", "Bay hay bơi?",
            new Say("Bạn nào có cánh để bay, bạn nào có vây để bơi? Con chia hai nhóm nhé!", "Who has wings to fly, and who has fins to swim? Make two groups!"),
            "🐦", ["Bay trên trời", "Bơi dưới nước"],
            [("Con chim", "Bay trên trời"), ("Con ong", "Bay trên trời"), ("Con bướm", "Bay trên trời"),
             ("Con cá", "Bơi dưới nước"), ("Cá voi", "Bơi dưới nước"), ("Bạch tuộc", "Bơi dưới nước")],
            retry: new Say("Bạn có cánh thì bay, bạn có vây hay xúc tu thì bơi nhé!", "Wings are for flying. Fins and tentacles are for swimming!"));

        g.Story("v5-world-story-turtle", "Rùa xanh qua suối",
            new Say("Rùa xanh bơi rất giỏi dưới nước và cũng bò được trên cạn. Hôm nay, rùa ngồi trên bè cùng thỏ trắng qua suối. Thỏ không biết bơi nên cầm sào đẩy bè thật cẩn thận.",
                "Green Turtle swims well in the water and can also walk on land. Today, Turtle rode a raft across the stream with White Rabbit. Rabbit can't swim, so he pushed the raft very carefully with a pole."),
            new Say("Bạn nào vừa bơi dưới nước, vừa bò được trên cạn?", "Who can swim in the water and also walk on land?"),
            "🐢", ["Con thỏ", "Con rùa", "Con mèo"], "Con rùa", "/images/lessons/doc-hieu/ill-rua-dong-suoi.jpg",
            retry: new Say("Thỏ không biết bơi đâu. Con nhìn bạn có mai xanh nhé!", "Rabbit can't swim. Look at the friend with the green shell!"));

        // ───────────── Chặng 3: con non và con mẹ ─────────────
        g.Stage = "Mẹ con nhà thú";
        g.Topic = "con-vat";

        g.Match("v5-world-match-mom-1", "Tìm mẹ cho bé: gà, mèo, chó",
            new Say("Các bạn nhỏ lạc mẹ rồi! Con giúp mỗi bạn tìm về với mẹ nhé!", "The little ones have lost their mums! Help each baby find its mum!"),
            "🐤", [("Gà mẹ", "Gà con"), ("Mèo mẹ", "Mèo con"), ("Chó mẹ", "Chó con")],
            retry: new Say("Con nhìn kỹ: con giống mẹ lắm đó!", "Look closely. Babies look just like their mums!"));

        g.Listen("v5-world-listen-calf", "Mẹ của bê con",
            WorldListenAsk, "🐮", ["Ngựa mẹ", "Bò mẹ", "Lợn mẹ"], "Bò mẹ",
            spoken: new Say("Bê con kêu ụm bò, đi tìm mẹ. Mẹ của bê con là ai?", "The little calf says moo and looks for its mum. Who is the calf's mum?"),
            retry: new Say("Bê con lớn lên sẽ thành con bò đó!", "A calf grows up to be a cow!"));

        g.Multi("v5-world-multi-eggs", "Ai nở ra từ trứng?",
            new Say("Bạn nào chào đời từ một quả trứng? Con chọn hết nhé!", "Which animals hatch from an egg? Tap all of them!"),
            "🥚", ["Con gà", "Con mèo", "Con vịt", "Con chó", "Con rùa"], ["Con gà", "Con vịt", "Con rùa"],
            retry: new Say("Con nhớ: chó và mèo không đẻ trứng đâu!", "Remember: cats and dogs don't lay eggs!"));

        g.Order("v5-world-order-chicken", "Từ quả trứng thành gà",
            new Say("Gà lớn lên thế nào? Con xếp từ quả trứng đến con gà lớn nhé!", "How does a chicken grow? Put them in order, from egg to grown-up hen!"),
            "🐣", ["Quả trứng", "Gà con mới nở", "Gà con", "Con gà"],
            retry: new Say("Đầu tiên là quả trứng, rồi gà con mổ vỏ chui ra nhé!", "First comes the egg, then the chick pecks its way out!"));

        g.Match("v5-world-match-mom-2", "Tìm mẹ cho bé: bò, lợn, ngựa",
            new Say("Các bạn nhỏ lạc mẹ rồi! Con giúp mỗi bạn tìm về với mẹ nhé!", "The little ones have lost their mums! Help each baby find its mum!"),
            "🐷", [("Bò mẹ", "Bê con"), ("Lợn mẹ", "Lợn con"), ("Ngựa mẹ", "Ngựa con")],
            retry: new Say("Con nhìn kỹ: con giống mẹ lắm đó!", "Look closely. Babies look just like their mums!"));

        g.Multi("v5-world-multi-milk", "Ai cho con bú sữa?",
            new Say("Những bạn nào cho con bú sữa mẹ? Con chọn hết nhé!", "Which mums feed their babies with milk? Tap all of them!"),
            "🍼", ["Bò mẹ", "Gà mẹ", "Mèo mẹ", "Chó mẹ"], ["Bò mẹ", "Mèo mẹ", "Chó mẹ"],
            retry: new Say("Gà mẹ ấp trứng và tìm thóc cho con, không cho bú đâu!", "A mother hen sits on eggs and finds seeds. She doesn't give milk!"));

        g.Order("world-order-butterfly", "Từ sâu nhỏ thành bướm",
            new Say("Bướm xinh lớn lên thế nào? Con xếp theo thứ tự nhé!", "How does a pretty butterfly grow? Put the pictures in order!"),
            "🦋", ["Quả trứng", "Con sâu", "Con bướm"],
            retry: new Say("Sâu nhỏ nở ra từ trứng, ăn thật nhiều lá rồi mới thành bướm nhé!", "A caterpillar hatches from an egg, eats lots of leaves, then becomes a butterfly!"));

        g.Story("v5-world-story-baby-birds", "Chim non tập bay",
            new Say("Ba chú chim non ngồi trong tổ trên cành cây. Chim mẹ dang đôi cánh bay lượn quanh tổ. Chim mẹ nói: Các con vẫy cánh theo mẹ nào!",
                "Three baby birds sat in a nest on a branch. Mother Bird spread her wings and flew around the nest. She said: Flap your wings like me, little ones!"),
            new Say("Chim mẹ đang dạy chim non làm gì?", "What is Mother Bird teaching her babies?"),
            "🐦", ["Tập bơi", "Tập bay", "Ngủ trong tổ"], "Tập bay", "/images/lessons/doc-hieu/ill-chim-non-tap-bay.jpg",
            retry: new Say("Con nghe lại: chim mẹ bảo các con vẫy cánh đó!", "Listen again: Mother Bird says flap your wings!"));

        g.Listen("v5-world-listen-pig", "Ai kêu ụt ịt?", WorldListenSound, "🐷",
            ["Con cừu", "Con lợn", "Con gà"], "Con lợn",
            spoken: new Say("Ụt ịt, ụt ịt! Con vật nào có cái mũi hếch và kêu ụt ịt?", "Oink, oink! Which animal has a snout and says oink?"),
            retry: new Say("Bạn này tròn trịa, màu hồng và thích lăn trong bùn!", "This one is round, pink and loves rolling in the mud!"));

        // ───────────── Chặng 4: cây cối ─────────────
        g.Stage = "Cây xanh lớn lên";
        g.Topic = "cay-co";

        g.Order("world-order-plant", "Hạt nhỏ thành cây to",
            new Say("Hạt giống lớn lên thành cây thế nào? Con xếp theo thứ tự nhé!", "How does a seed grow into a tree? Put the pictures in order!"),
            "🌱", ["Hạt giống", "Mầm cây", "Cây non", "Cái cây"],
            retry: new Say("Hạt nảy mầm trước, rồi mới thành cây non nhé!", "First the seed sprouts, then it becomes a young plant!"));

        g.Pick("world-single-plant", "Cây khát nước rồi",
            new Say("Chậu cây héo rũ, lá cụp xuống. Con đưa cây thứ gì nhé?", "The plant is drooping and its leaves are sad. What should we give it?"),
            "💧", ["Kẹo", "Nước tưới cây", "Gấu bông"], "Nước tưới cây", scene: ["🥀", "?"],
            retry: new Say("Cây đang khát đó, con cho cây uống gì nhỉ?", "The plant is thirsty. What can it drink?"));

        g.Multi("v5-world-multi-plant-needs", "Cây cần gì để lớn?",
            new Say("Cây cần những gì để lớn lên thật khỏe? Con chọn đủ nhé!", "What does a plant need to grow big and strong? Tap them all!"),
            "🪴", ["Nước tưới cây", "Kẹo", "Ánh nắng mặt trời", "Gấu bông", "Đất trồng cây"], ["Nước tưới cây", "Ánh nắng mặt trời", "Đất trồng cây"],
            retry: new Say("Cây cần uống nước, cần nắng ấm và cần đất để mọc rễ nhé!", "Plants need water to drink, warm sunshine and soil for their roots!"));

        g.Story("world-story-seed", "Hạt đậu nảy mầm",
            new Say("Cô và con gieo một hạt đậu xuống chậu đất. Ngày nào con cũng tưới nước cho hạt. Ít hôm sau, một mầm xanh nhỏ xíu nhú lên chào con!",
                "We planted a bean seed in a pot of soil. Every day, you watered it. A few days later, a tiny green sprout popped up to say hello!"),
            new Say("Mỗi ngày, con làm gì để hạt đậu nảy mầm?", "What did you do every day to help the bean sprout?"),
            "🌱", ["Cất hạt vào hộp", "Tưới nước cho cây", "Quên chăm cây"], "Tưới nước cho cây", "/images/pictograms/seedling.svg",
            retry: new Say("Hạt đậu cần uống nước mỗi ngày mới nảy mầm đó!", "The seed needs a drink every day to sprout!"));

        g.Sort("v5-world-sort-under-over", "Dưới đất hay trên cành?",
            new Say("Củ nào mọc dưới lòng đất, quả nào treo trên cành cây? Con xếp nhé!", "Which ones grow under the ground, and which hang on branches? Sort them!"),
            "🥕", ["Mọc dưới lòng đất", "Treo trên cành cây"],
            [("Cà rốt", "Mọc dưới lòng đất"), ("Củ khoai tây", "Mọc dưới lòng đất"),
             ("Quả táo", "Treo trên cành cây"), ("Quả cam", "Treo trên cành cây"), ("Quả dừa", "Treo trên cành cây")],
            retry: new Say("Cà rốt và khoai tây phải đào đất mới thấy đó con!", "We dig in the soil to find carrots and potatoes!"));

        g.Pick("v5-world-seed-inside", "Bí mật trong quả táo",
            new Say("Cô bổ đôi quả táo. Bên trong có gì để trồng thành cây táo mới?", "I cut the apple in half. What's inside that can grow a new apple tree?"),
            "🍎", ["Chiếc lá", "Hạt giống", "Bông hoa"], "Hạt giống", scene: ["🍎", "🔪", "?"],
            retry: new Say("Con nhìn những hạt nhỏ màu nâu nằm giữa quả nhé!", "Look for the little brown bits in the middle!"));

        g.Sort("world-plants", "Giỏ quả và bó hoa",
            new Say("Con giúp cô cho quả vào giỏ, hoa vào bó hoa nhé!", "Help me put the fruit in the basket and the flowers in the bunch!"),
            "💐", ["Giỏ quả", "Bó hoa"],
            [("Quả xoài", "Giỏ quả"), ("Quả chuối", "Giỏ quả"), ("Quả lê", "Giỏ quả"),
             ("Hoa hồng", "Bó hoa"), ("Hoa hướng dương", "Bó hoa"), ("Bông hoa", "Bó hoa")],
            retry: new Say("Quả thì ăn được, hoa thì ngắm cho đẹp nhé!", "Fruit is for eating, flowers are for looking at!"));

        g.Order("v5-world-order-flower-fruit", "Từ hoa thành quả",
            new Say("Cây táo ra quả thế nào? Con xếp từ bông hoa đến quả chín nhé!", "How does an apple tree make apples? Put them in order, from flower to ripe fruit!"),
            "🍏", ["Bông hoa", "Táo xanh còn non", "Quả táo"],
            retry: new Say("Hoa nở trước, rồi quả non xanh, sau cùng quả chín đỏ nhé!", "First the flower, then a little green apple, and last a ripe red one!"));

        g.Multi("v5-world-multi-tree-gifts", "Cây xanh tặng gì?",
            new Say("Cây xanh tặng chúng ta những món quà nào? Con chọn hết nhé!", "What gifts do trees give us? Tap all of them!"),
            "🌳", ["Quả xoài", "Ô tô", "Bóng cây mát rượi", "Điện thoại", "Cái ghế"], ["Quả xoài", "Bóng cây mát rượi", "Cái ghế"],
            retry: new Say("Cây cho quả ngọt, bóng mát và cả gỗ để làm bàn ghế đó!", "Trees give us sweet fruit, cool shade and wood for chairs!"));

        g.Story("v5-world-story-squirrel", "Sóc nhặt hạt dẻ",
            new Say("Mùa thu, lá cây chuyển màu cam rồi rơi xuống. Sóc nhỏ nhặt hạt dẻ rụng dưới gốc cây, bỏ đầy một giỏ. Sóc cất hạt để dành ăn khi trời lạnh.",
                "In autumn, the leaves turn orange and fall down. Little Squirrel picks up nuts under the tree and fills his basket. He saves them to eat when it gets cold."),
            new Say("Sóc nhặt gì dưới gốc cây?", "What does Squirrel pick up under the tree?"),
            "🐿️", ["Hạt dẻ", "Quả táo", "Cà rốt"], "Hạt dẻ", "/images/lessons/doc-hieu/ill-soc-hat-de.jpg",
            retry: new Say("Con nhìn cái giỏ của sóc, toàn hạt nâu tròn đó!", "Look in Squirrel's basket. It's full of round brown nuts!"));

        g.Story("v5-world-story-hedgehog", "Nhím và quả táo rơi",
            new Say("Nhím nhỏ đi dạo dưới gốc cây táo. Bịch! Một quả táo chín đỏ rơi xuống bãi cỏ. Nhím ôm quả táo, định mang về hang bên bờ suối.",
                "Little Hedgehog was walking under an apple tree. Thump! A ripe red apple fell onto the grass. Hedgehog hugged it and wanted to take it home to his burrow by the stream."),
            new Say("Quả táo rơi xuống từ đâu?", "Where did the apple fall from?"),
            "🦔", ["Đám mây", "Cái cây", "Ngôi nhà"], "Cái cây", "/images/lessons/doc-hieu/ill-nhim-qua-tao.jpg",
            retry: new Say("Quả táo mọc trên cành cây táo đó con!", "Apples grow on the branches of an apple tree!"));

        // ───────────── Chặng 5: thời tiết, trang phục và mùa ─────────────
        g.Stage = "Nắng mưa bốn mùa";
        g.Topic = "thoi-tiet";

        g.Match("world-weather", "Trời nào, mặc gì?",
            new Say("Con nối mỗi kiểu thời tiết với đồ dùng phù hợp nhé!", "Match each kind of weather with the right thing to wear or carry!"),
            "🌦️", [("Trời mưa rào", "Cái ô"), ("Trời nắng chang chang", "Mũ rộng vành"), ("Trời lạnh giá", "Áo ấm")],
            retry: new Say("Ô che mưa, mũ che nắng, áo ấm giữ ấm nhé!", "Umbrellas stop rain, hats block sun, and coats keep you warm!"));

        g.Pick("world-single-rain", "Mưa to rồi!",
            new Say("Ngoài trời mưa to quá! Con mang theo gì khi đi học nhỉ?", "It's pouring outside! What should you take to school?"),
            "☂️", ["Kính râm", "Cái ô", "Que kem"], "Cái ô", scene: ["🌧️", "🏫"],
            retry: new Say("Con chọn đồ giúp con không bị ướt nhé!", "Pick the thing that keeps you dry!"));

        g.Multi("v5-world-multi-cold", "Giữ ấm ngày lạnh",
            new Say("Gió lạnh thổi vù vù! Con chọn những đồ giữ ấm cho cơ thể nhé!", "Brr, the wind is cold! Tap all the things that keep you warm!"),
            "🧣", ["Áo ấm", "Kính râm", "Khăn quàng", "Que kem", "Găng tay"], ["Áo ấm", "Khăn quàng", "Găng tay"],
            retry: new Say("Áo ấm, khăn quàng, găng tay ôm ấm con đó!", "A coat, a scarf and gloves keep you cosy!"));

        g.Sort("v5-world-sort-sun-rain", "Đồ ngày nắng, đồ ngày mưa",
            new Say("Đồ nào dùng ngày nắng, đồ nào dùng ngày mưa? Con xếp giúp cô nhé!", "Which things are for sunny days, and which are for rainy days? Sort them for me!"),
            "🌤️", ["Trời nắng chang chang", "Trời mưa rào"],
            [("Kính râm", "Trời nắng chang chang"), ("Mũ rộng vành", "Trời nắng chang chang"),
             ("Cái ô", "Trời mưa rào"), ("Áo mưa", "Trời mưa rào"), ("Đôi ủng", "Trời mưa rào")],
            retry: new Say("Ủng và áo mưa giúp con không ướt khi lội nước đó!", "Boots and a raincoat keep you dry in puddles!"));

        g.Pick("v5-world-windy-kite", "Ngày lộng gió",
            new Say("Hôm nay gió thổi thật mạnh. Đồ chơi nào bay cao nhất trên trời?", "It's very windy today. Which toy can fly highest in the sky?"),
            "🪁", ["Gấu bông", "Quả bóng", "Con diều"], "Con diều", scene: ["🌬️", "☁️", "?"],
            retry: new Say("Đồ chơi này có dây dài, gió đẩy nó bay lên cao đó!", "This toy has a long string, and the wind lifts it up high!"));

        g.Pick("v5-world-rainbow", "Sau cơn mưa",
            new Say("Mưa vừa tạnh, mặt trời ló ra. Trên trời hiện ra gì đẹp thế?", "The rain just stopped and the sun came out. What beautiful thing is in the sky?"),
            "🌈", ["Ngôi sao", "Cầu vồng", "Mặt trăng"], "Cầu vồng", scene: ["🌧️", "☀️", "?"],
            retry: new Say("Có nắng sau mưa là có dải bảy màu đó con!", "Sunshine after rain makes a band of seven colours!"));

        g.Story("v5-world-story-yellow-umbrella", "Chiếc ô vàng",
            new Say("Trời mưa rào. Bạn Na mặc áo mưa vàng, đi ủng vàng, cầm chiếc ô vàng đến trường. Bạn thấy chú mèo con ướt lạnh dưới gốc cây, liền che ô cho mèo.",
                "It was raining. Na wore a yellow raincoat and yellow boots and carried a yellow umbrella to school. She saw a wet, cold kitten under a tree and held her umbrella over it."),
            new Say("Trời mưa, bạn Na mặc gì để không bị ướt?", "What did Na wear so she wouldn't get wet in the rain?"),
            "☂️", ["Kính râm", "Áo mưa", "Mũ rộng vành"], "Áo mưa", "/images/lessons/doc-hieu/ill-chiec-o-vang.jpg",
            retry: new Say("Con nhìn chiếc áo vàng có mũ trùm đầu của bạn Na nhé!", "Look at Na's yellow coat with a hood!"));

        g.Pick("v5-world-thunder", "Sấm chớp ầm ầm",
            new Say("Mây đen kéo đến, sấm chớp ầm ầm. Con nên làm gì cho an toàn?", "Dark clouds and loud thunder! What should you do to stay safe?"),
            "⛈️", ["Chạy ra sân chơi", "Vào nhà trú mưa", "Đứng dưới gốc cây to"], "Vào nhà trú mưa", scene: ["⛈️", "⚡"],
            retry: new Say("Khi có sấm sét, đứng dưới cây to rất nguy hiểm. Con vào nhà ngay nhé!", "In a storm, big trees are dangerous. Go inside right away!"));

        g.Order("v5-world-order-rain", "Mưa đến rồi đi",
            new Say("Trời đổi thế nào? Con xếp từ lúc mây kéo đến đến lúc cầu vồng hiện ra nhé!", "How does the weather change? Put it in order, from clouds rolling in to the rainbow!"),
            "🌧️", ["Đám mây", "Trời mưa rào", "Cầu vồng"],
            retry: new Say("Mây đen kéo đến trước, rồi mới mưa nhé!", "First the clouds come, then the rain!"));

        g.Match("v5-world-match-seasons", "Bốn mùa vui",
            new Say("Con nối mỗi mùa với điều đặc biệt của mùa đó nhé!", "Match each season with something special about it!"),
            "🍂", [("Mùa hè", "Que kem"), ("Mùa thu", "Lá vàng rơi"), ("Mùa đông", "Áo ấm")],
            retry: new Say("Mùa hè nóng nực, mùa thu lá rụng, mùa đông lạnh giá nhé!", "Summer is hot, leaves fall in autumn, and winter is cold!"));

        g.Order("v5-world-order-seasons", "Vòng quay bốn mùa",
            new Say("Một năm có bốn mùa. Con xếp các mùa theo thứ tự, bắt đầu từ mùa xuân nhé!", "A year has four seasons. Put them in order, starting with spring!"),
            "🌷", ["Mùa xuân", "Mùa hè", "Mùa thu", "Mùa đông"],
            retry: new Say("Xuân, hạ, thu, đông. Con đọc theo cô nhé!", "Spring, summer, autumn, winter. Say it with me!"));

        // ───────────── Chặng 6: ngày và đêm ─────────────
        g.Stage = "Ngày và đêm";
        g.Topic = "thoi-tiet";

        g.Pick("world-single-sun", "Ai chiếu sáng ban ngày?",
            new Say("Ban ngày, trên bầu trời có gì chiếu sáng rực rỡ?", "In the daytime, what shines brightly in the sky?"),
            "☀️", ["Mặt trăng", "Ngôi sao", "Mặt trời"], "Mặt trời", scene: ["🌤️", "?"],
            retry: new Say("Thứ này tròn, nóng và sáng chói vào ban ngày đó!", "It's round, hot and very bright in the day!"));

        g.Sort("v5-world-sort-day-night", "Ban ngày hay ban đêm?",
            new Say("Con thấy những thứ này vào ban ngày hay ban đêm? Con xếp nhé!", "Do we see these in the daytime or at night? Sort them!"),
            "🌗", ["Ban ngày", "Ban đêm"],
            [("Mặt trời", "Ban ngày"), ("Cầu vồng", "Ban ngày"), ("Mặt trăng", "Ban đêm"), ("Ngôi sao", "Ban đêm"), ("Con cú", "Ban đêm")],
            retry: new Say("Cú mèo thức dậy khi trời tối đó con!", "Owls wake up when it gets dark!"));

        g.Order("world-order-day", "Một ngày của con",
            new Say("Con xếp các buổi trong ngày, từ sáng sớm đến tối nhé!", "Put the parts of the day in order, from early morning to evening!"),
            "🕰️", ["Sáng sớm", "Giữa trưa", "Chiều tà", "Trời tối"],
            retry: new Say("Mặt trời mọc buổi sáng và lặn vào buổi chiều nhé!", "The sun rises in the morning and sets in the evening!"));

        g.Listen("v5-world-listen-morning", "Gà gáy là buổi nào?",
            WorldListenAsk, "🌄", ["Trời tối", "Sáng sớm", "Giữa trưa"], "Sáng sớm",
            spoken: new Say("Ò ó o! Gà trống gáy vang, mặt trời vừa mọc. Bây giờ là lúc nào?", "Cock-a-doodle-doo! The rooster crows and the sun is coming up. What time of day is it?"),
            retry: new Say("Mặt trời vừa mọc là lúc mình thức dậy đó!", "When the sun comes up, it's time to wake up!"));

        g.Multi("v5-world-multi-night-animals", "Ai thức ban đêm?",
            new Say("Trời tối rồi, bạn nào mới thức dậy đi kiếm ăn? Con chọn hết nhé!", "It's dark now. Which animals wake up to look for food? Tap them all!"),
            "🦉", ["Con cú", "Con gà", "Con dơi", "Con bướm"], ["Con cú", "Con dơi"],
            retry: new Say("Gà và bướm đi ngủ khi trời tối đó con!", "Chickens and butterflies go to sleep when it gets dark!"));

        g.Pick("v5-world-evening", "Mặt trời đi ngủ",
            new Say("Mặt trời lặn, trời tối dần, đèn đường bật sáng. Đó là lúc nào?", "The sun goes down, the sky gets dark and the street lights turn on. What time is it?"),
            "🌇", ["Sáng sớm", "Giữa trưa", "Trời tối"], "Trời tối", scene: ["🌇", "💡"],
            retry: new Say("Đèn bật lên khi trời không còn nắng nữa nhé!", "Lights turn on when the sunshine is gone!"));

        g.Pick("v5-world-bedtime", "Đêm rồi, làm gì?",
            new Say("Đêm đã khuya, trăng sao lấp lánh. Con nên làm gì cho khỏe?", "It's late at night and the stars are twinkling. What should you do to stay healthy?"),
            "🌙", ["Xem ti vi cả đêm", "Ngủ sớm cho khỏe", "Chơi đến nửa đêm"], "Ngủ sớm cho khỏe", scene: ["🌙", "⭐", "🛏️"],
            retry: new Say("Ban đêm là lúc cơ thể nghỉ ngơi để mai thật khỏe đó!", "Night is when your body rests so you feel great tomorrow!"));

        // ───────────── Chặng 7: phương tiện giao thông ─────────────
        g.Stage = "Xe chạy khắp nơi";
        g.Topic = "giao-thong";

        g.Listen("v5-world-listen-train", "Xình xịch tu tu",
            WorldListenAsk, "🚆", ["Xe buýt", "Tàu hỏa", "Máy bay"], "Tàu hỏa",
            spoken: new Say("Tu tu, xình xịch! Xe nào chạy trên đường ray dài?", "Choo choo! Which one runs along the long railway tracks?"),
            retry: new Say("Xe này có nhiều toa nối nhau như đoàn sâu dài đó!", "This one has lots of cars joined together like a long caterpillar!"));

        g.Sort("v5-world-sort-road-water", "Đường bộ hay đường thủy?",
            new Say("Xe nào chạy trên đường, thuyền nào đi trên sông biển? Con xếp nhé!", "Which ones go on the road, and which go on the water? Sort them!"),
            "🚢", ["Chạy trên đường", "Đi trên sông biển"],
            [("Ô tô", "Chạy trên đường"), ("Xe buýt", "Chạy trên đường"), ("Xe đạp", "Chạy trên đường"),
             ("Tàu thủy", "Đi trên sông biển"), ("Thuyền buồm", "Đi trên sông biển"), ("Ca nô", "Đi trên sông biển")],
            retry: new Say("Xe có bánh thì chạy trên đường, thuyền thì nổi trên nước nhé!", "Things with wheels go on roads. Boats float on water!"));

        g.Multi("v5-world-multi-sky", "Ai bay trên trời?",
            new Say("Phương tiện nào bay trên bầu trời? Con chọn hết nhé!", "Which ones fly up in the sky? Tap them all!"),
            "✈️", ["Máy bay", "Ô tô", "Trực thăng", "Tàu thủy", "Xe đạp"], ["Máy bay", "Trực thăng"],
            retry: new Say("Con tìm những phương tiện có cánh hoặc cánh quạt nhé!", "Look for the ones with wings or spinning blades!"));

        g.Match("v5-world-match-special", "Xe làm nhiệm vụ đặc biệt",
            new Say("Mỗi xe có một nhiệm vụ riêng. Con nối xe với việc nó làm nhé!", "Each of these has a special job. Match each one with its job!"),
            "🚒", [("Xe cứu hỏa", "Dập lửa"), ("Xe cứu thương", "Chở người ốm"), ("Xe cảnh sát", "Giữ trật tự")],
            retry: new Say("Xe cứu hỏa màu đỏ có vòi phun nước đó con!", "The red fire truck has a big water hose!"));

        g.Listen("v5-world-listen-fire", "Có đám cháy!",
            WorldListenAsk, "🔥", ["Xe cứu thương", "Xe buýt", "Xe cứu hỏa"], "Xe cứu hỏa",
            spoken: new Say("Ò e ò e! Có đám cháy! Xe nào chạy đến phun nước dập lửa?", "Nee-naw, nee-naw! There's a fire! Which truck comes to spray water on it?"),
            retry: new Say("Xe này màu đỏ, có thang dài và vòi nước to!", "This one is red, with a long ladder and a big hose!"));

        g.Pick("v5-world-river-boat", "Qua sông bằng gì?",
            new Say("Bạn Tôm muốn sang bên kia sông nhưng không có cầu. Bạn đi bằng gì?", "Tom wants to cross the river, but there's no bridge. How can he get across?"),
            "🛶", ["Xe đạp", "Con đò", "Tàu hỏa"], "Con đò", scene: ["🧒", "🌊", "?"],
            retry: new Say("Muốn qua sông thì phải đi thứ nổi được trên nước nhé!", "To cross a river, you need something that floats!"));

        g.Pick("v5-world-school-bus", "Chở cả lớp đi học",
            new Say("Phương tiện nào chở được nhiều bạn nhỏ đến trường cùng một lúc?", "Which one can take lots of children to school at the same time?"),
            "🚌", ["Xe đạp", "Xe máy", "Xe buýt"], "Xe buýt", scene: ["🧒", "👧", "🧒", "👦", "🏫"],
            retry: new Say("Con tìm xe to nhất, có thật nhiều ghế ngồi nhé!", "Find the biggest one with lots of seats!"));

        g.Story("v5-world-story-helmet", "Bin đến trường",
            new Say("Mẹ đưa bạn Bin đến trường. Bin đội chiếc mũ bảo hiểm màu xanh và nắm chặt tay mẹ. Hai mẹ con đứng chờ đèn xanh rồi mới qua đường.",
                "Mum took Bin to school. Bin wore a blue helmet and held Mum's hand tightly. They waited for the green light before crossing the road."),
            new Say("Bin đội gì trên đầu để an toàn khi đi đường?", "What did Bin wear on his head to stay safe on the road?"),
            "⛑️", ["Mũ len", "Mũ bảo hiểm", "Mũ rộng vành"], "Mũ bảo hiểm", "/images/lessons/visual-road-safety.png",
            retry: new Say("Con nhìn chiếc mũ cứng màu xanh trên đầu Bin nhé!", "Look at the hard blue hat on Bin's head!"));

        g.Pick("v5-world-tractor", "Giúp bác nông dân",
            new Say("Bác nông dân cần cày ruộng thật nhanh. Xe nào giúp bác?", "The farmer needs to plough the field quickly. Which one can help?"),
            "🚜", ["Xe đua", "Máy cày", "Xe cứu thương"], "Máy cày", scene: ["👨‍🌾", "🌾", "?"],
            retry: new Say("Xe này có bánh to, chạy được trên ruộng bùn đó!", "This one has huge wheels for muddy fields!"));

        g.Multi("v5-world-multi-give-way", "Nhường đường cho xe khẩn cấp",
            new Say("Nghe tiếng còi ò e, mọi người nhường đường cho những xe nào? Con chọn hết nhé!", "When you hear a siren, which ones should everyone let go first? Tap them all!"),
            "🚨", ["Xe cứu hỏa", "Xe đạp", "Xe cứu thương", "Ô tô", "Xe cảnh sát"], ["Xe cứu hỏa", "Xe cứu thương", "Xe cảnh sát"],
            retry: new Say("Con tìm các xe có đèn nháy và còi hú đi làm nhiệm vụ nhé!", "Find the ones with flashing lights and sirens!"));

        g.Match("v5-world-match-vehicle-place", "Xe về bến",
            new Say("Mỗi phương tiện có chỗ đỗ riêng. Con đưa chúng về đúng chỗ nhé!", "Each one has its own place to stop. Take each one to the right place!"),
            "🛤️", [("Tàu hỏa", "Đường ray"), ("Máy bay", "Sân bay"), ("Tàu thủy", "Bến cảng")],
            retry: new Say("Máy bay cất cánh ở sân bay, tàu thủy cập bến cảng nhé!", "Planes take off at the airport, and ships dock at the harbour!"));

        g.Sort("v5-world-sort-sky-water", "Trên trời hay dưới nước?",
            new Say("Phương tiện nào bay trên trời, phương tiện nào đi trên sông biển? Con xếp nhé!", "Which ones fly in the sky, and which go on the water? Sort them!"),
            "🚁", ["Bay trên trời", "Đi trên sông biển"],
            [("Máy bay", "Bay trên trời"), ("Trực thăng", "Bay trên trời"), ("Tên lửa", "Bay trên trời"),
             ("Tàu thủy", "Đi trên sông biển"), ("Con đò", "Đi trên sông biển")],
            retry: new Say("Cái gì có cánh hay bay vút lên thì xếp lên trời nhé!", "If it has wings or zooms up, put it in the sky!"));
    }
}

public static partial class Lexicon
{
    private const string WorldTable = """
        // Con vật
        Con gà trống | 🐓 | rooster
        Con lạc đà | 🐪 | camel
        Con dơi | 🦇 | bat
        Gà mẹ | 🐔 | mother hen
        Mèo mẹ | 🐈 | mother cat
        Mèo con | 🐱 | kitten
        Chó mẹ | 🐕 | mother dog
        Chó con | 🐶 | puppy
        Bò mẹ | 🐄 | mother cow
        Bê con | 🐮 | calf
        Lợn mẹ | 🐖 | mother pig
        Lợn con | 🐷 | piglet
        Ngựa mẹ | 🐎 | mother horse
        Ngựa con | 🐴 | foal
        Gà con mới nở | 🐣 | newly hatched chick
        Vật nuôi trong nhà | 🏡 | animals that live with us
        Thú trong rừng | 🌲 | wild forest animals
        Sống trên cạn | 🏞️ | lives on land
        Sống dưới nước | 🌊 | lives in water
        Sa mạc cát nóng | 🏜️ | the hot sandy desert
        Xứ băng tuyết | 🏔️ | the icy, snowy land
        Biển xanh | 🐚 | the blue sea
        Bay trên trời | ☁️ | flies in the sky
        Bơi dưới nước | 🌊 | swims in the water
        Bay thật cao | 🕊️ | flying up high
        Bơi thật giỏi | 🏊 | swimming really well
        Leo lên cây cao | 🌴 | climbing tall trees
        Tập bay | 🪶 | learning to fly
        Tập bơi | 🏊 | learning to swim
        Ngủ trong tổ | 😴 | sleeping in the nest
        // Cây cối
        Cây non | 🪴 | young plant
        Nước tưới cây | 💧 | water for the plant
        Ánh nắng mặt trời | 🌞 | sunshine
        Đất trồng cây | 🟫 | soil
        Tưới nước cho cây | 💦 | water the plant
        Cất hạt vào hộp | 📦 | put the seed in a box
        Quên chăm cây | 🥀 | forget to look after it
        Mọc dưới lòng đất | 🕳️ | grows under the ground
        Treo trên cành cây | 🌳 | hangs on a branch
        Giỏ quả | 🧺 | fruit basket
        Bó hoa | 💐 | bunch of flowers
        Hoa hồng | 🌹 | rose
        Táo xanh còn non | 🍏 | young green apple
        Bóng cây mát rượi | ⛱️ | cool shade
        // Thời tiết, mùa, ngày đêm
        Trời nắng chang chang | ☀️ | a hot sunny day
        Trời mưa rào | 🌧️ | a rainy day
        Trời lạnh giá | ❄️ | a freezing cold day
        Vào nhà trú mưa | 🏠 | go inside out of the storm
        Chạy ra sân chơi | 🏃 | run outside to play
        Đứng dưới gốc cây to | 🌳 | stand under a big tree
        Mùa xuân | 🌷 | spring
        Mùa hè | 🏖️ | summer
        Mùa thu | 🍁 | autumn
        Mùa đông | ⛄ | winter
        Lá vàng rơi | 🍂 | falling yellow leaves
        Sáng sớm | 🌄 | early morning
        Giữa trưa | 🕛 | midday
        Chiều tà | 🌇 | late afternoon
        Trời tối | 🌃 | evening
        Ngủ sớm cho khỏe | 😴 | go to bed early
        Xem ti vi cả đêm | 📺 | watch TV all night
        Chơi đến nửa đêm | 🧸 | play until midnight
        // Giao thông
        Chạy trên đường | 🛣️ | goes on the road
        Đi trên sông biển | ⚓ | goes on the water
        Ca nô | 🚤 | speedboat
        Con đò | 🛶 | small boat
        Xe cảnh sát | 🚓 | police car
        Máy cày | 🚜 | tractor
        Xe đua | 🏎️ | race car
        Dập lửa | 🔥 | puts out fires
        Chở người ốm | 🤒 | takes sick people to hospital
        Giữ trật tự | 👮 | keeps everyone safe
        Đường ray | 🛤️ | railway tracks
        Sân bay | 🛫 | the airport
        Bến cảng | 🏗️ | the harbour
        """;
}
