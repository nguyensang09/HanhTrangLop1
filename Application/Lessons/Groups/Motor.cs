namespace HanhTrangLop1.Application.Lessons;

public static partial class LessonBank
{
    /// <summary>Tranh tô cũ (giữ nguyên mã "picture-&lt;id&gt;" và ký hiệu để khớp mẫu nét): id, tên tranh, cụm từ cô đọc, tiếng Anh, biểu tượng.</summary>
    private static readonly (string Id, string Name, string Vi, string En, string Icon)[] MotorPictures =
    [
        ("meo-con", "Chú mèo con", "chú mèo con", "a little kitten", "🐱"),
        ("ca-heo", "Cá heo tung tăng", "chú cá heo tung tăng", "a playful dolphin", "🐬"),
        ("chu-tho", "Chú thỏ và cà rốt", "chú thỏ và củ cà rốt", "a bunny with a carrot", "🐰"),
        ("ong-vang", "Ong vàng và hoa", "chú ong vàng bên bông hoa", "a bee next to a flower", "🐝"),
        ("chu-buom", "Bướm xinh trong vườn", "chú bướm xinh trong vườn", "a pretty butterfly in the garden", "🦋"),
        ("khung-long", "Khủng long tí hon", "chú khủng long tí hon", "a tiny dinosaur", "🦕"),
        ("trai-tao", "Quả táo và chú sâu", "quả táo có chú sâu nhỏ", "an apple with a little worm", "🍎"),
        ("phong-canh", "Phong cảnh thiên nhiên", "phong cảnh núi đồi thật đẹp", "a lovely nature scene", "🏞️"),
        ("do-dung", "Đồ dùng học tập", "những đồ dùng học tập", "your school things", "✏️"),
        ("hinh-hoc", "Hình học kỳ diệu", "những hình học kỳ diệu", "some magic shapes", "🔷"),
        ("o-che-mua", "Chiếc ô che mưa", "chiếc ô che mưa", "an umbrella in the rain", "☂️"),
        ("tau-hoa", "Đoàn tàu tí hon", "đoàn tàu tí hon", "a tiny train", "🚂"),
        ("may-bay", "Máy bay trên trời", "chiếc máy bay trên trời", "an airplane in the sky", "✈️"),
        ("thuyen", "Thuyền buồm lướt sóng", "chiếc thuyền buồm lướt sóng", "a sailboat on the waves", "⛵"),
        ("cau-vong", "Cầu vồng và lâu đài", "cầu vồng và tòa lâu đài", "a rainbow and a castle", "🏰"),
        ("ten-lua", "Tên lửa và các vì sao", "tên lửa bay giữa các vì sao", "a rocket among the stars", "🚀")
    ];

    private static void MotorTracePicture(LessonGroupBuilder g, int index)
    {
        var p = MotorPictures[index];
        g.Trace($"picture-{p.Id}", $"Tô tranh: {p.Name}",
            new Say($"Con tô theo nét chấm để vẽ {p.Vi} nhé!", $"Trace the dotted lines to draw {p.En}!"),
            p.Icon, p.Id, 6);
    }

    /// <summary>Nhóm van-dong-tinh: Vận động tinh.</summary>
    private static void AddMotor(LessonGroupBuilder g)
    {
        // ───────────── Chặng 1: nét thẳng ─────────────
        g.Stage = "Nét thẳng, nét xiên";
        g.Topic = "net-co-ban";

        g.Pick("v5-motor-pencil-grip", "Cầm bút thật đúng",
            new Say("Trước khi tô, mình cầm bút bằng mấy ngón tay nhỉ?", "Before we trace, how many fingers do we use to hold the pencil?"),
            "✍️", ["2", "3", "5"], "3", scene: ["✍️", "✏️"],
            retry: new Say("Ngón cái, ngón trỏ và ngón giữa cùng giữ bút nhé!", "Your thumb, pointer finger and middle finger hold the pencil together!"));

        g.Trace("motor-vertical", "Tập tô nét sổ thẳng",
            new Say("Con tô nét sổ thẳng, kéo bút từ trên xuống dưới nhé!", "Trace the straight line, from top to bottom!"),
            "⬇️", "nét sổ thẳng", 1);
        g.Trace("motor-horizontal", "Tập tô nét ngang",
            new Say("Con tô nét ngang, kéo bút từ trái sang phải nhé!", "Trace the flat line, from left to right!"),
            "➡️", "nét ngang", 1);

        g.Pick("v5-motor-line-candle", "Nét nào giống cây nến?",
            new Say("Cây nến đứng thẳng tắp. Con chọn nét giống cây nến nhé!", "The candle stands up straight. Pick the line that looks like it!"),
            "🕯️", ["Nét nằm ngang", "Nét thẳng đứng", "Nét cong"], "Nét thẳng đứng", scene: ["🕯️"],
            retry: new Say("Cây nến đứng từ trên xuống dưới, không nằm đâu con!", "The candle stands up from top to bottom. It isn't lying down!"));

        g.Trace("motor-diagonal-left", "Tập tô nét xiên trái",
            new Say("Con tô nét xiên trái theo nét chấm nhé!", "Trace the line that slants to the left!"),
            "↙️", "nét xiên trái", 1);
        g.Trace("motor-diagonal-right", "Tập tô nét xiên phải",
            new Say("Con tô nét xiên phải theo nét chấm nhé!", "Trace the line that slants to the right!"),
            "↘️", "nét xiên phải", 1);

        g.Match("motor-match-line", "Nét nào, đồ vật nấy",
            new Say("Con nối mỗi nét với đồ vật có hình dáng giống nó nhé!", "Match each line with the thing that has the same shape!"),
            "🔗", [("Nét thẳng đứng", "Cây nến"), ("Nét tròn khép kín", "Quả bóng"), ("Nét cong", "Cầu vồng")],
            retry: new Say("Quả bóng tròn xoe, cầu vồng cong cong, cây nến thẳng tắp nhé!", "A ball is round, a rainbow curves, and a candle is straight!"));

        // ───────────── Chặng 2: nét móc, nét cong ─────────────
        g.Stage = "Nét móc, nét cong";
        g.Topic = "net-co-ban";

        g.Trace("motor-hook-forward", "Tập tô nét móc xuôi",
            new Say("Con tô nét móc xuôi: uốn cong ở trên rồi kéo thẳng xuống nhé!", "Trace the hook line: curve at the top, then go straight down!"),
            "↪️", "nét móc xuôi", 1);
        g.Trace("motor-hook-reverse", "Tập tô nét móc ngược",
            new Say("Con tô nét móc ngược: kéo thẳng xuống rồi móc lên ở dưới nhé!", "Trace the hook line: go straight down, then hook up at the bottom!"),
            "↩️", "nét móc ngược", 1);
        g.Trace("motor-double-hook", "Tập tô nét móc hai đầu",
            new Say("Con tô nét móc hai đầu, có móc ở trên và móc ở dưới nhé!", "Trace the line with a little hook at the top and at the bottom!"),
            "〰️", "nét móc hai đầu", 1);

        g.Pick("v5-motor-line-rainbow", "Nét nào giống cầu vồng?",
            new Say("Cầu vồng cong cong trên trời. Con chọn nét giống cầu vồng nhé!", "The rainbow curves across the sky. Pick the line that looks like it!"),
            "🌈", ["Nét thẳng đứng", "Nét nằm ngang", "Nét cong"], "Nét cong", scene: ["🌈"],
            retry: new Say("Cầu vồng uốn cong như chiếc cầu, con tìm nét cong nhé!", "A rainbow bends like a bridge. Look for the curvy line!"));

        g.Trace("motor-open-curve-right", "Tập tô nét cong hở phải",
            new Say("Con tô nét cong hở phải, giống chữ c nhé!", "Trace the curve that opens to the right, just like the letter c!", "Con tô nét cong hở phải, giống chữ cờ nhé!"),
            "🌙", "nét cong hở phải", 1);
        g.Trace("motor-open-curve-left", "Tập tô nét cong hở trái",
            new Say("Con tô nét cong hở trái, phần hở quay sang bên trái nhé!", "Trace the curve that opens to the left!"),
            "🌜", "nét cong hở trái", 1);
        g.Trace("motor-closed-curve", "Tập tô nét cong kín",
            new Say("Con tô nét cong kín, tròn như chữ o nhé!", "Trace the closed curve, round like the letter o!"),
            "⭕", "nét cong kín", 1);

        g.Pick("v5-motor-line-ball", "Nét nào giống quả bóng?",
            new Say("Quả bóng tròn xoe. Con chọn nét giống quả bóng nhé!", "The ball is perfectly round. Pick the line that looks like it!"),
            "⚽", ["Nét tròn khép kín", "Nét nghiêng", "Nét thẳng đứng"], "Nét tròn khép kín", scene: ["⚽"],
            retry: new Say("Con tìm nét đi một vòng rồi quay về chỗ cũ nhé!", "Find the line that goes all the way around and meets itself!"));

        // ───────────── Chặng 3: nét khuyết, nét thắt ─────────────
        g.Stage = "Nét khuyết, nét thắt";
        g.Topic = "net-co-ban";

        g.Trace("motor-upper-loop", "Tập tô nét khuyết trên",
            new Say("Con tô nét khuyết trên, cao như trong chữ l nhé!", "Trace the tall loop, like the one in the letter l!", "Con tô nét khuyết trên, cao như trong chữ lờ nhé!"),
            "➰", "nét khuyết trên", 1);
        g.Trace("motor-lower-loop", "Tập tô nét khuyết dưới",
            new Say("Con tô nét khuyết dưới, thả xuống như trong chữ g nhé!", "Trace the loop that hangs down, like the one in the letter g!", "Con tô nét khuyết dưới, thả xuống như trong chữ gờ nhé!"),
            "➿", "nét khuyết dưới", 1);

        g.Match("v5-motor-match-line-name", "Nét giống đồ vật nào?",
            new Say("Con nối mỗi nét với đồ vật giống nó: thước kẻ, bánh xe, cầu vồng nhé!", "Match each line with a ruler, a wheel or a rainbow!"),
            "📏", [("Nét nghiêng", "Thước kẻ"), ("Nét tròn khép kín", "Bánh xe tròn"), ("Nét cong", "Cầu vồng")],
            retry: new Say("Thước kẻ nằm nghiêng, bánh xe tròn, cầu vồng cong nhé!", "The ruler slants, the wheel is round and the rainbow curves!"));

        g.Trace("motor-knot", "Tập tô nét thắt",
            new Say("Con tô nét thắt, có một vòng nhỏ xíu như trong chữ b nhé!", "Trace the line with a tiny loop, like the one in the letter b!", "Con tô nét thắt, có một vòng nhỏ xíu như trong chữ bờ nhé!"),
            "🎀", "nét thắt", 1);

        // ───────────── Chặng 4: tô tranh con vật ─────────────
        g.Stage = "Tô tranh con vật";
        g.Topic = "tao-hinh";

        MotorTracePicture(g, 0);
        MotorTracePicture(g, 1);
        g.Pick("v5-motor-color-sky", "Tô màu bầu trời",
            new Say("Cô muốn tô bầu trời và biển cả. Con đưa cô bút màu nào?", "I want to colour the sky and the sea. Which crayon should you give me?"),
            "🖍️", ["Bút màu đỏ", "Bút màu xanh dương", "Bút màu vàng"], "Bút màu xanh dương", scene: ["☁️", "🌊", "?"],
            retry: new Say("Bầu trời trong xanh, biển cũng xanh nữa đó con!", "The sky is blue, and so is the sea!"));
        MotorTracePicture(g, 2);
        MotorTracePicture(g, 3);
        g.Match("v5-motor-match-colors", "Chọn màu cho tranh",
            new Say("Con nối mỗi hình với bút màu phù hợp để tô nhé!", "Match each picture with the right crayon to colour it!"),
            "🎨", [("Chiếc lá", "Bút màu xanh lá"), ("Quả dâu tây", "Bút màu đỏ"), ("Quả cam", "Bút màu cam")],
            retry: new Say("Lá màu xanh, dâu tây màu đỏ, quả cam màu cam nhé!", "Leaves are green, strawberries are red, and oranges are orange!"));
        MotorTracePicture(g, 4);
        MotorTracePicture(g, 5);
        g.Topic = "kheo-tay";
        g.Order("motor-order-color", "Tô màu gọn gàng",
            new Say("Tô màu thế nào cho đẹp? Con xếp các bước theo thứ tự nhé!", "How do we colour neatly? Put the steps in order!"),
            "🖍️", ["Chọn bút màu", "Tô gọn trong viền", "Cất bút vào hộp"],
            retry: new Say("Chọn màu trước, tô xong thì cất bút gọn gàng nhé!", "Pick a colour first, and tidy up your crayons at the end!"));

        // ───────────── Chặng 5: tô tranh khắp nơi ─────────────
        g.Stage = "Tô tranh khắp nơi";
        g.Topic = "tao-hinh";

        MotorTracePicture(g, 6);
        MotorTracePicture(g, 7);
        g.Pick("v5-motor-color-sun", "Tô màu mặt trời",
            new Say("Cô sắp tô ông mặt trời rực rỡ. Con chọn bút màu nào?", "I'm going to colour the bright sun. Which crayon should I use?"),
            "☀️", ["Bút màu vàng", "Bút màu xanh lá", "Bút màu xanh dương"], "Bút màu vàng", scene: ["☀️", "?"],
            retry: new Say("Mặt trời vàng rực như bông hoa hướng dương đó!", "The sun is as yellow as a sunflower!"));
        MotorTracePicture(g, 8);
        MotorTracePicture(g, 9);
        MotorTracePicture(g, 10);
        g.Pick("v5-motor-neat-coloring", "Bức tranh nào đẹp hơn?",
            new Say("Hai bạn cùng tô quả táo. Bạn nào tô đẹp hơn?", "Two friends coloured an apple. Whose picture looks neater?"),
            "🍎", ["Tô gọn trong viền", "Tô lem ra ngoài"], "Tô gọn trong viền", scene: ["🍎"],
            retry: new Say("Tô đẹp là màu nằm gọn bên trong đường viền nhé!", "Neat colouring stays inside the lines!"));
        MotorTracePicture(g, 11);
        MotorTracePicture(g, 12);
        MotorTracePicture(g, 13);
        MotorTracePicture(g, 14);
        MotorTracePicture(g, 15);

        // ───────────── Chặng 6: đồ dùng thủ công ─────────────
        g.Stage = "Góc thủ công";
        g.Topic = "kheo-tay";

        g.Pick("motor-single-cut", "Dụng cụ cắt giấy",
            new Say("Cô cần cắt tờ giấy làm đôi. Con đưa cô dụng cụ nào?", "I need to cut this paper in half. Which tool should you give me?"),
            "✂️", ["Bút chì", "Cái kéo", "Cái thìa"], "Cái kéo", scene: ["📄", "?"],
            retry: new Say("Dụng cụ này có hai lưỡi, mở ra khép vào để cắt đó!", "This tool has two blades that open and close to cut!"));

        g.Multi("motor-multi-draw", "Đồ dùng để vẽ",
            new Say("Mình sắp vẽ tranh! Con chọn hết những đồ dùng để vẽ nhé!", "Let's draw a picture! Tap all the things we need for drawing!"),
            "🖍️", ["Bút chì", "Cái bát", "Bút màu", "Đôi giày", "Tờ giấy trắng"], ["Bút chì", "Bút màu", "Tờ giấy trắng"],
            retry: new Say("Mình cần bút để vẽ và giấy để vẽ lên nữa đó!", "We need pencils to draw with and paper to draw on!"));

        g.Match("motor-match-craft", "Dụng cụ làm ra gì?",
            new Say("Con nối mỗi dụng cụ với món đồ làm được từ nó nhé!", "Match each tool with the thing we can make with it!"),
            "🧰", [("Cái kéo", "Bông tuyết cắt giấy"), ("Lọ hồ dán", "Tranh dán lá"), ("Đất nặn", "Con giun đất nặn")],
            retry: new Say("Kéo để cắt, hồ để dán, đất nặn để nặn nhé!", "Scissors cut, glue sticks, and play dough is for shaping!"));

        g.Order("motor-order-paper", "Gấp giấy làm đôi",
            new Say("Con giúp cô gấp đôi tờ giấy cho thẳng. Con xếp các bước nhé!", "Help me fold the paper neatly in half. Put the steps in order!"),
            "📄", ["Đặt giấy ngay ngắn", "Gấp đôi tờ giấy", "Miết thẳng nếp gấp"],
            retry: new Say("Đặt giấy ngay ngắn trước, miết nếp gấp là bước cuối nhé!", "Lay the paper flat first, and press the fold last!"));

        g.Multi("motor-multi-craft", "Làm thiệp tặng mẹ",
            new Say("Mình làm thiệp tặng mẹ nhé! Con chọn những đồ cần dùng nào!", "Let's make a card for Mum! Tap everything we need!"),
            "💌", ["Giấy màu", "Cái nồi", "Lọ hồ dán", "Quả táo", "Cái kéo"], ["Giấy màu", "Lọ hồ dán", "Cái kéo"],
            retry: new Say("Làm thiệp cần giấy, kéo để cắt và hồ để dán đó!", "To make a card we need paper, scissors to cut and glue to stick!"));

        g.Drag("v5-motor-drag-craft-box", "Cất kéo vào hộp",
            new Say("Dùng kéo xong rồi! Con kéo chiếc kéo cất vào hộp thủ công nhé!", "We're done with the scissors! Drag them back into the craft box!"),
            "🧰", ["Cái thìa", "Cái kéo", "Bàn chải đánh răng"], "Cái kéo", "Hộp đồ thủ công",
            retry: new Say("Con tìm chiếc kéo có hai lưỡi nhé!", "Look for the scissors with two blades!"));

        g.Order("v5-motor-order-snail", "Nặn chú ốc sên",
            new Say("Mình nặn chú ốc sên bằng đất nặn! Con xếp các bước theo thứ tự nhé!", "Let's make a snail out of play dough! Put the steps in order!"),
            "🐌", ["Lăn đất thành sợi dài", "Cuộn tròn sợi đất", "Gắn thêm đầu và râu"],
            retry: new Say("Lăn đất thành sợi dài trước rồi mới cuộn tròn nhé!", "Roll a long snake first, then curl it up!"));

        g.Pick("v5-motor-paper-boat", "Thuyền giấy của bạn nhỏ",
            new Say("Bạn nhỏ gấp giấy thành món đồ chơi thả xuống vũng nước. Đó là gì?", "This boy folded paper into a toy and put it in the puddle. What is it?"),
            "⛵", ["Máy bay giấy", "Chiếc thuyền giấy", "Quả bóng"], "Chiếc thuyền giấy",
            image: "/images/lessons/doc-hieu/ill-chiec-thuyen-giay.jpg",
            retry: new Say("Con nhìn món đồ màu trắng đang nổi trên mặt nước nhé!", "Look at the white thing floating on the water!"));

        g.Order("v5-motor-order-plane", "Gấp máy bay giấy",
            new Say("Mình gấp máy bay giấy nào! Con xếp các bước theo thứ tự nhé!", "Let's fold a paper airplane! Put the steps in order!"),
            "🛩️", ["Gấp đôi tờ giấy", "Gấp hai góc nhọn", "Gấp hai cánh", "Phóng máy bay bay xa"],
            retry: new Say("Phóng máy bay là việc cuối cùng, khi đã gấp xong cánh nhé!", "We throw the airplane last, after the wings are folded!"));

        g.Order("v5-motor-order-collage", "Cắt dán bông hoa",
            new Say("Mình cắt dán bông hoa lên thiệp. Con xếp các bước theo thứ tự nhé!", "Let's cut out a flower and stick it on a card. Put the steps in order!"),
            "🌸", ["Vẽ bông hoa lên giấy", "Cắt theo nét vẽ", "Bôi hồ mặt sau", "Dán lên tấm thiệp"],
            retry: new Say("Phải vẽ rồi mới cắt, cắt xong mới bôi hồ để dán nhé!", "Draw first, then cut, then add glue and stick!"));

        g.Pick("v5-motor-scissors-safe", "Dùng kéo an toàn",
            new Say("Khi đưa kéo cho bạn, con cầm kéo thế nào cho an toàn?", "When you pass scissors to a friend, how do you hold them safely?"),
            "✂️", ["Chĩa mũi kéo vào bạn", "Nắm lưỡi kéo, đưa tay cầm", "Vừa chạy vừa đưa kéo"], "Nắm lưỡi kéo, đưa tay cầm",
            retry: new Say("Con nắm phần lưỡi đã khép lại, để bạn cầm vào tay cầm nhé!", "Hold the closed blades and give your friend the handles!"));

        // ───────────── Chặng 7: tự làm lấy, khéo tay ─────────────
        g.Stage = "Đôi tay khéo léo";
        g.Topic = "kheo-tay";

        g.Drag("motor-drag-rabbit", "Đưa thỏ về vườn cà rốt",
            new Say("Bạn thỏ đói bụng rồi! Con kéo bạn thỏ về vườn cà rốt nhé!", "Bunny is hungry! Drag the bunny to the carrot patch!"),
            "🐰", ["Con mèo", "Con thỏ", "Con cá"], "Con thỏ", "Vườn cà rốt",
            retry: new Say("Con tìm bạn có đôi tai dài nhé!", "Find the friend with long ears!"));

        g.Order("v5-motor-order-button", "Cài cúc áo",
            new Say("Con tự cài cúc áo nào! Con xếp các bước theo thứ tự nhé!", "Let's do up a button all by yourself! Put the steps in order!"),
            "🔘", ["Một tay cầm cúc áo", "Một tay mở khuyết áo", "Đẩy cúc chui qua khuyết"],
            retry: new Say("Cầm cúc và mở khuyết trước, rồi mới đẩy cúc qua nhé!", "Hold the button and open the hole first, then push it through!"));

        g.Drag("v5-motor-drag-button", "Cúc áo vào khuyết",
            new Say("Áo của gấu bông bị bung cúc! Con kéo chiếc cúc vào khuyết áo nhé!", "Teddy's shirt came undone! Drag the button into the buttonhole!"),
            "👔", ["Chiếc cúc áo", "Chiếc lá", "Kẹo"], "Chiếc cúc áo", "Khuyết áo",
            retry: new Say("Con tìm chiếc cúc tròn có lỗ nhỏ nhé!", "Find the round button with little holes!"));

        g.Order("v5-motor-order-shoelace", "Buộc dây giày tai thỏ",
            new Say("Mình buộc dây giày kiểu tai thỏ nhé! Con xếp các bước theo thứ tự nào!", "Let's tie our shoes the bunny-ears way! Put the steps in order!"),
            "👟", ["Bắt chéo hai dây giày", "Luồn dây và kéo chặt", "Làm hai tai thỏ", "Thắt thành nơ xinh"],
            retry: new Say("Bắt chéo dây trước, làm tai thỏ rồi mới thắt nơ nhé!", "Cross the laces first, make bunny ears, then tie the bow!"));

        g.Match("v5-motor-match-daily-tools", "Đồ dùng hằng ngày",
            new Say("Đôi tay khéo dùng được nhiều thứ. Con nối đồ dùng với việc làm nhé!", "Clever hands can use lots of things. Match each one with what it does!"),
            "🥢", [("Đôi đũa", "Gắp sợi mì"), ("Cái thìa", "Múc canh"), ("Bàn chải đánh răng", "Chải răng trắng sạch")],
            retry: new Say("Đũa để gắp, thìa để múc, bàn chải để chải răng nhé!", "Chopsticks pick up, spoons scoop, and toothbrushes clean teeth!"));

        g.Pick("v5-motor-block-bottom", "Xây tháp không đổ",
            new Say("Con xây tháp bằng ba khối vuông. Khối nào đặt dưới cùng để tháp không đổ?", "You're building a tower with three blocks. Which block goes at the bottom so it won't fall?"),
            "🧱", ["Khối vuông nhỏ", "Khối vuông to", "Khối vuông vừa"], "Khối vuông to",
            retry: new Say("Khối to nhất đặt dưới cùng thì tháp đứng vững nhé!", "Put the biggest block at the bottom and the tower stays strong!"));

        g.Order("v5-motor-order-tower", "Xếp tháp từ dưới lên",
            new Say("Con xếp tháp từ dưới lên trên: khối to trước, khối nhỏ sau nhé!", "Build the tower from the bottom up: big blocks first, small blocks last!"),
            "🗼", ["Khối vuông to", "Khối vuông vừa", "Khối vuông nhỏ"],
            retry: new Say("Con bắt đầu bằng khối to nhất nhé!", "Start with the biggest block!"));

        g.Pick("v5-motor-bead-pattern", "Xâu vòng hạt",
            new Say("Con xâu vòng hạt theo màu đỏ, xanh. Hạt tiếp theo màu gì?", "You're threading red and blue beads. Which bead comes next?"),
            "📿", ["Hạt cườm xanh", "Hạt cườm vàng", "Hạt cườm đỏ"], "Hạt cườm đỏ", scene: ["🔴", "🔵", "🔴", "🔵", "?"],
            retry: new Say("Đỏ, xanh, đỏ, xanh... con đọc tiếp nào!", "Red, blue, red, blue... what comes next?"));

        g.Drag("motor-drag-bee", "Đưa ong về vườn hoa",
            new Say("Ong vàng muốn đi hút mật! Con kéo ong về vườn hoa nhé!", "The bee wants to find nectar! Drag the bee to the flower garden!"),
            "🐝", ["Con cá", "Con chó", "Con ong"], "Con ong", "Vườn hoa",
            retry: new Say("Con tìm bạn có cánh và sọc vàng đen nhé!", "Find the one with wings and yellow and black stripes!"));

        g.Multi("v5-motor-multi-clever-hands", "Việc làm đôi tay khéo",
            new Say("Việc nào giúp đôi tay con khéo léo hơn? Con chọn hết nhé!", "Which activities make your hands more clever? Tap them all!"),
            "🧩", ["Xếp hình", "Xem ti vi", "Xâu vòng hạt", "Nặn đất", "Ngủ cả ngày"], ["Xếp hình", "Xâu vòng hạt", "Nặn đất"],
            retry: new Say("Con tìm những việc mà ngón tay phải làm thật nhiều nhé!", "Look for the things that keep your fingers busy!"));
    }
}

public static partial class Lexicon
{
    private const string MotorTable = """
        // Nét vẽ
        Nét thẳng đứng | │ | a straight up-and-down line
        Nét nằm ngang | ─ | a flat line
        Nét nghiêng | ╱ | a slanted line
        Nét cong | ◠ | a curved line
        Nét tròn khép kín | ◯ | a closed circle
        Cây nến | 🕯️ | candle
        Bánh xe tròn | 🛞 | wheel
        // Bút màu, tô màu
        Bút màu đỏ | 🟥 | red crayon
        Bút màu xanh dương | 🟦 | blue crayon
        Bút màu vàng | 🟨 | yellow crayon
        Bút màu xanh lá | 🟩 | green crayon
        Bút màu cam | 🟧 | orange crayon
        Chọn bút màu | 🖍️ | choose a crayon
        Tô gọn trong viền | 🎨 | colour inside the lines
        Tô lem ra ngoài | 💥 | colour outside the lines
        Cất bút vào hộp | 📦 | put the crayons away
        // Thủ công
        Tờ giấy trắng | 📄 | sheet of paper
        Giấy màu | 📄 | coloured paper
        Lọ hồ dán | 🧴 | glue
        Đất nặn | 🟠 | play dough
        Bông tuyết cắt giấy | ❄️ | paper snowflake
        Tranh dán lá | 🍁 | leaf collage
        Con giun đất nặn | 🪱 | play dough worm
        Hộp đồ thủ công | 🧰 | craft box
        Đặt giấy ngay ngắn | 📄 | lay the paper flat
        Gấp đôi tờ giấy | 📁 | fold the paper in half
        Miết thẳng nếp gấp | 👆 | press the fold flat
        Lăn đất thành sợi dài | 〰️ | roll a long snake
        Cuộn tròn sợi đất | 🌀 | curl it up
        Gắn thêm đầu và râu | 🐌 | add a head and feelers
        Máy bay giấy | 🛩️ | paper airplane
        Chiếc thuyền giấy | ⛵ | paper boat
        Gấp hai góc nhọn | 🔺 | fold the two corners down
        Gấp hai cánh | 📐 | fold the wings
        Phóng máy bay bay xa | ✈️ | throw the plane far
        Vẽ bông hoa lên giấy | ✏️ | draw a flower on paper
        Cắt theo nét vẽ | ✂️ | cut along the lines
        Bôi hồ mặt sau | 🧴 | put glue on the back
        Dán lên tấm thiệp | 💌 | stick it on the card
        Chĩa mũi kéo vào bạn | ⚠️ | point the tips at a friend
        Nắm lưỡi kéo, đưa tay cầm | 🤝 | hold the blades, offer the handles
        Vừa chạy vừa đưa kéo | 🏃 | run while holding scissors
        // Tự làm lấy, khéo tay
        Vườn cà rốt | 🥕 | the carrot patch
        Vườn hoa | 🌼 | the flower garden
        Chiếc cúc áo | 🔘 | button
        Khuyết áo | 👔 | buttonhole
        Một tay cầm cúc áo | 🔘 | hold the button
        Một tay mở khuyết áo | 👔 | open the buttonhole
        Đẩy cúc chui qua khuyết | 👉 | push the button through
        Bắt chéo hai dây giày | ❌ | cross the laces
        Luồn dây và kéo chặt | 🪢 | tuck one under and pull tight
        Làm hai tai thỏ | 🐰 | make two bunny ears
        Thắt thành nơ xinh | 🎀 | tie a pretty bow
        Đôi đũa | 🥢 | chopsticks
        Gắp sợi mì | 🍜 | pick up noodles
        Múc canh | 🍲 | scoop soup
        Chải răng trắng sạch | 🦷 | brush teeth clean
        Khối vuông to | ⬛ | big block
        Khối vuông vừa | ◼️ | medium block
        Khối vuông nhỏ | ▪️ | small block
        Hạt cườm đỏ | 🔴 | red bead
        Hạt cườm xanh | 🔵 | blue bead
        Hạt cườm vàng | 🟡 | yellow bead
        Xếp hình | 🧩 | doing puzzles
        Xem ti vi | 📺 | watching TV
        Xâu vòng hạt | 📿 | threading beads
        Nặn đất | 🤲 | shaping play dough
        Ngủ cả ngày | 🛌 | sleeping all day
        """;
}
