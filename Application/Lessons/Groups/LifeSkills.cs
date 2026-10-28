namespace HanhTrangLop1.Application.Lessons;

public static partial class LessonBank
{
    /// <summary>Nhóm ky-nang-song: tự phục vụ, an toàn, cảm xúc, giao tiếp và chia sẻ.</summary>
    private static void AddLifeSkills(LessonGroupBuilder g)
    {
        const string washImage = "/images/lessons/story-wash-hands.png";
        const string crossingImage = "/images/lessons/story-safe-crossing.png";
        const string roadImage = "/images/lessons/visual-road-safety.png";
        const string sharingImage = "/images/lessons/story-sharing.png";

        // ───────────── Chặng 1: Bé tự phục vụ ─────────────
        g.Stage = "Bé tự phục vụ";
        g.Topic = "tu-phuc-vu";

        var washStory = new Say(
            "Tí vừa chơi cát ngoài sân về. Tí mở vòi nước, xoa xà phòng nổi đầy bọt. Rửa xong, Tí lau tay thật khô.",
            "Ti just came in from playing in the sand. He turns on the tap and rubs on soap until it's all bubbly. Then he dries his hands really well.");

        g.Story("v5-life-wash-story-soap", "Tí rửa tay", washStory,
            new Say("Tí dùng gì để đôi tay sạch vi khuẩn?", "What does Ti use to wash the germs off his hands?"),
            "🧼", ["Bút màu", "Xà phòng", "Gấu bông"], "Xà phòng", washImage,
            retry: new Say("Con nhìn tranh nhé: tay Tí đầy bọt trắng. Bọt đó từ đâu ra?", "Look at the picture: Ti's hands are full of white bubbles. Where do they come from?"));

        g.Multi("v5-life-when-wash", "Lúc nào cần rửa tay?",
            new Say("Lúc nào con cần rửa tay? Con chọn hết các lúc đúng nhé!", "When should you wash your hands? Pick all the right times!"),
            "🫧", ["Trước khi ăn cơm", "Khi đang ngủ", "Sau khi đi vệ sinh", "Khi nghe nhạc", "Sau khi chơi ngoài sân"],
            ["Trước khi ăn cơm", "Sau khi đi vệ sinh", "Sau khi chơi ngoài sân"],
            retry: new Say("Con nhớ: trước khi ăn và sau khi tay bị bẩn là phải rửa tay nhé!", "Remember: wash before eating and whenever your hands get dirty!"));

        g.Order("life-order-hands", "Các bước rửa tay",
            new Say("Con xếp các bước rửa tay theo đúng thứ tự giúp cô nhé!", "Help me put the hand-washing steps in the right order!"),
            "🚰", ["Làm ướt tay", "Xoa xà phòng", "Xả sạch bọt", "Lau khô tay"],
            retry: new Say("Phải làm ướt tay trước, rồi mới xoa xà phòng con nhé!", "First wet your hands, then rub on the soap!"));

        g.Story("v5-life-wash-story-towel", "Tí lau tay", washStory,
            new Say("Rửa xong, Tí lau tay vào đâu?", "After washing, what does Ti dry his hands on?"),
            "🧻", ["Cái áo", "Gấu bông", "Khăn mặt"], "Khăn mặt", washImage,
            retry: new Say("Con nhìn chiếc khăn vàng treo trên tường nhé!", "Look at the yellow towel hanging on the wall!"));

        g.Pick("v5-life-toothbrush", "Đồ dùng đánh răng",
            new Say("Cô muốn đánh răng. Con đưa cô đồ dùng nào nhỉ?", "I want to brush my teeth. Which one should you give me?"),
            "🪥", ["Cái thìa", "Bàn chải đánh răng", "Bút chì"], "Bàn chải đánh răng",
            retry: new Say("Đồ dùng đánh răng có lông chải mềm mềm đó con!", "The one for teeth has soft little bristles!"));

        g.Match("v5-life-match-body", "Đồ dùng giữ sạch cơ thể",
            new Say("Mỗi đồ dùng giúp giữ sạch một phần cơ thể. Con nối cho đúng nhé!", "Each thing keeps one part of the body clean. Match them up!"),
            "🦷", [("Bàn chải đánh răng", "Hàm răng"), ("Xà phòng", "Đôi bàn tay"), ("Đôi tất", "Bàn chân")]);

        g.Order("life-order-morning", "Buổi sáng của con",
            new Say("Buổi sáng con làm gì trước, làm gì sau? Con xếp lại nhé!", "What do you do first in the morning, and what comes next? Put them in order!"),
            "🌅", ["Thức dậy", "Đánh răng rửa mặt", "Ăn sáng", "Đeo cặp đi học"],
            retry: new Say("Con phải thức dậy trước đã, rồi mới đánh răng con nhé!", "You have to wake up first, then brush your teeth!"));

        g.Pick("v5-life-bedtime", "Trước khi đi ngủ",
            new Say("Sắp đến giờ đi ngủ rồi. Con cần làm gì trước?", "It's almost bedtime. What should you do first?"),
            "🌙", ["Ăn thêm kẹo", "Đánh răng", "Xem điện thoại"], "Đánh răng",
            retry: new Say("Đánh răng trước khi ngủ để răng không bị sâu nhé!", "Brush before bed so your teeth stay strong!"));

        g.Order("v5-life-order-dress", "Tự mặc đồ đi học",
            new Say("Con tự mặc đồ đi học nào! Con xếp các việc theo thứ tự nhé.", "Let's get dressed for school by yourself! Put the steps in order."),
            "👟", ["Mặc quần áo", "Đi tất", "Đi giày", "Đeo cặp đi học"],
            retry: new Say("Phải đi tất trước rồi mới đi giày được con ạ!", "Socks go on first, then shoes!"));

        g.Pick("v5-life-tidy-toys", "Chơi xong thì cất",
            new Say("Chơi xong, đồ chơi nằm khắp sàn nhà. Con làm gì nhỉ?", "Playtime is over and toys are all over the floor. What should you do?"),
            "🧺", ["Để đồ chơi bừa trên sàn", "Đá đồ chơi vào gầm giường", "Cất đồ chơi vào hộp"], "Cất đồ chơi vào hộp",
            retry: new Say("Đồ chơi cũng cần về nhà của nó, đó là chiếc hộp!", "Toys need to go home too, and their home is the box!"));

        g.Sort("v5-life-sort-tidy", "Cất đồ đúng chỗ",
            new Say("Con giúp cô cất đồ: đồ chơi vào hộp, sách vở lên giá nhé!", "Help me tidy up: toys in the toy box, books on the shelf!"),
            "📦", ["Hộp đồ chơi", "Giá sách"],
            [("Gấu bông", "Hộp đồ chơi"), ("Quả bóng", "Hộp đồ chơi"), ("Con diều", "Hộp đồ chơi"), ("Quyển sách", "Giá sách"), ("Quyển vở", "Giá sách")]);

        g.Multi("life-multi-school", "Soạn cặp đi học",
            new Say("Mai con đi học rồi! Con chọn những đồ cần cho vào cặp nhé.", "School is tomorrow! Pick the things to put in your school bag."),
            "🎒", ["Bút chì", "Que kem", "Quyển vở", "Gấu bông", "Thước kẻ"], ["Bút chì", "Quyển vở", "Thước kẻ"],
            retry: new Say("Con chọn những đồ dùng để học, để viết nhé!", "Pick the things you use for learning and writing!"));

        // ───────────── Chặng 2: Đi đường an toàn ─────────────
        g.Stage = "Đi đường an toàn";
        g.Topic = "an-toan";

        var crossingStory = new Say(
            "Mẹ đưa Bin đến chỗ có vạch kẻ trắng trên đường. Đèn hình người chuyển sang màu xanh. Mẹ và Bin nhìn trái, nhìn phải rồi mới sang đường.",
            "Mum takes Bin to the white stripes on the road. The little walking man turns green. Mum and Bin look left, look right, and then they cross.");

        g.Story("v5-life-crossing-light", "Bin qua đường", crossingStory,
            new Say("Đèn hình người màu gì thì Bin được sang đường?", "What colour does the walking man need to be before Bin can cross?"),
            "🚦", ["Đèn tín hiệu đỏ", "Đèn tín hiệu xanh", "Đèn tín hiệu vàng"], "Đèn tín hiệu xanh", crossingImage,
            retry: new Say("Con nhìn cột đèn trong tranh: hình người đang sáng màu gì?", "Look at the light in the picture. What colour is the walking man?"));

        g.Match("v5-life-match-lights", "Đèn giao thông nói gì?",
            new Say("Mỗi màu đèn bảo mình làm một việc. Con nối cho đúng nhé!", "Each light colour tells us what to do. Match them up!"),
            "🚦", [("Đèn tín hiệu đỏ", "Dừng lại chờ"), ("Đèn tín hiệu vàng", "Đi chậm, chuẩn bị dừng"), ("Đèn tín hiệu xanh", "Được đi qua")],
            retry: new Say("Con nhớ: đỏ là dừng, vàng là chậm lại, xanh là đi nhé!", "Remember: red means stop, yellow means slow down, green means go!"));

        g.Order("v5-life-order-crossing", "Ba bước qua đường",
            new Say("Qua đường thế nào cho an toàn? Con xếp các bước nhé!", "How do we cross the road safely? Put the steps in order!"),
            "🚸", ["Dừng lại ở mép đường", "Nhìn trái, nhìn phải", "Đi trên vạch kẻ đường"],
            retry: new Say("Mình phải dừng lại và nhìn thật kỹ trước khi bước đi con nhé!", "We always stop and look carefully before we walk!"));

        var roadStory = new Say(
            "Sáng nay, Tũn đội chiếc mũ bảo hiểm màu xanh đi học. Trường của Tũn ở ngay bên kia đường. Mẹ nắm chặt tay Tũn, hai mẹ con đứng trên vỉa hè chờ đèn.",
            "This morning, Tun wears his blue helmet to school. His school is just across the road. Mum holds his hand tight, and they wait on the pavement for the light.");

        g.Story("v5-life-road-helmet", "Chiếc mũ của Tũn", roadStory,
            new Say("Tũn đội gì để bảo vệ cái đầu?", "What does Tun wear to protect his head?"),
            "⛑️", ["Mũ rộng vành", "Mũ len", "Mũ bảo hiểm"], "Mũ bảo hiểm", roadImage,
            retry: new Say("Con nhìn chiếc mũ cứng màu xanh trên đầu Tũn nhé!", "Look at the hard blue hat on Tun's head!"));

        g.Story("v5-life-road-hand", "Tũn chờ qua đường", roadStory,
            new Say("Đứng chờ qua đường, Tũn làm gì?", "What does Tun do while he waits to cross?"),
            "🤝", ["Chạy ra lòng đường", "Nắm tay người lớn khi qua đường", "Đá bóng dưới lòng đường"], "Nắm tay người lớn khi qua đường", roadImage,
            retry: new Say("Con nhìn tay Tũn: bạn ấy đang nắm tay ai?", "Look at Tun's hand. Whose hand is he holding?"));

        g.Multi("life-multi-safe", "Việc làm an toàn",
            new Say("Việc nào giúp con an toàn khi đi đường? Con chọn hết nhé!", "Which of these keep you safe on the road? Pick them all!"),
            "🦺", ["Đội mũ bảo hiểm khi đi xe", "Chạy ra lòng đường", "Nắm tay người lớn khi qua đường", "Đá bóng dưới lòng đường", "Ngồi ngay ngắn trên xe"],
            ["Đội mũ bảo hiểm khi đi xe", "Nắm tay người lớn khi qua đường", "Ngồi ngay ngắn trên xe"],
            retry: new Say("Lòng đường là chỗ xe chạy, không phải chỗ để chơi đâu con!", "The road is for cars, not for playing!"));

        g.Pick("v5-life-play-place", "Chơi bóng ở đâu?",
            new Say("Con muốn đá bóng với các bạn. Chỗ nào an toàn nhất?", "You want to play football with your friends. Where is the safest place?"),
            "⚽", ["Dưới lòng đường", "Ở sân chơi", "Cạnh bờ ao"], "Ở sân chơi",
            retry: new Say("Ngoài đường có xe, cạnh ao có nước sâu. Con chọn lại nhé!", "The road has cars and the pond has deep water. Try again!"));

        // ───────────── Chặng 3: An toàn ở nhà ─────────────
        g.Stage = "An toàn ở nhà";
        g.Topic = "an-toan";

        g.Pick("v5-life-socket", "Không chạm ổ điện",
            new Say("Ở nhà có một thứ con tuyệt đối không được chạm vào. Đó là gì?", "There's one thing at home you must never touch. Which one is it?"),
            "🔌", ["Gấu bông", "Ổ cắm điện", "Quyển sách"], "Ổ cắm điện",
            retry: new Say("Điện rất nguy hiểm, có thể làm con bị thương đấy!", "Electricity is very dangerous. It can hurt you!"));

        g.Multi("v5-life-multi-danger", "Đồ vật nguy hiểm",
            new Say("Những thứ nào nóng hoặc sắc, con không được chạm vào?", "Which things are hot or sharp, so you must not touch them?"),
            "🔥", ["Bếp lửa đang cháy", "Quả bóng", "Con dao sắc", "Gấu bông", "Ổ cắm điện"], ["Bếp lửa đang cháy", "Con dao sắc", "Ổ cắm điện"],
            retry: new Say("Có ba thứ nguy hiểm đấy: thứ rất nóng, thứ rất sắc và thứ có điện.", "There are three dangerous ones: something hot, something sharp and something with electricity."));

        g.Pick("v5-life-stranger", "Khi người lạ cho kẹo",
            new Say("Một người lạ cho con kẹo và rủ con đi theo. Con làm gì?", "A stranger offers you candy and asks you to come along. What do you do?"),
            "🙅", ["Nhận kẹo và đi theo", "Từ chối và tìm bố mẹ", "Đứng im không nói gì"], "Từ chối và tìm bố mẹ",
            scene: ["🧔", "🍬", "🧒"],
            retry: new Say("Con không bao giờ đi theo người lạ. Hãy chạy về với bố mẹ nhé!", "Never go with a stranger. Run back to mum or dad!"));

        g.Pick("v5-life-lost", "Khi bị lạc",
            new Say("Con bị lạc mẹ ở chỗ đông người. Con nhờ ai giúp đỡ?", "You've lost your mum in a busy place. Who should you ask for help?"),
            "👮", ["Đi theo người lạ", "Chú công an", "Một mình chạy ra đường"], "Chú công an",
            retry: new Say("Con tìm cô chú mặc đồng phục như chú công an để nhờ giúp nhé!", "Look for someone in uniform, like a police officer!"));

        g.Pick("v5-life-medicine", "Viên thuốc không phải kẹo",
            new Say("Con thấy mấy viên thuốc nhiều màu trông như kẹo. Con làm gì?", "You find some colourful pills that look like candy. What do you do?"),
            "💊", ["Ăn thử một viên", "Chia cho bạn ăn", "Đưa cho người lớn"], "Đưa cho người lớn",
            retry: new Say("Thuốc không phải kẹo. Chỉ uống thuốc khi bố mẹ, bác sĩ cho thôi!", "Medicine isn't candy. Only take it when mum, dad or the doctor gives it to you!"));

        g.Sort("v5-life-sort-home", "An toàn hay nguy hiểm?",
            new Say("Việc nào an toàn, việc nào nguy hiểm? Con xếp vào đúng nhóm nhé!", "Which things are safe and which are dangerous? Sort them!"),
            "⚠️", ["Làm thế là an toàn", "Làm thế là nguy hiểm"],
            [("Ngồi xếp hình", "Làm thế là an toàn"), ("Tô màu tranh", "Làm thế là an toàn"), ("Leo lên bậu cửa sổ", "Làm thế là nguy hiểm"), ("Nghịch ổ cắm điện", "Làm thế là nguy hiểm")]);

        g.Pick("v5-life-water", "Đi bơi an toàn",
            new Say("Con muốn xuống hồ bơi. Thế nào mới an toàn?", "You want to go swimming. How can you stay safe?"),
            "🏊", ["Đi bơi một mình", "Đi với bạn nhỏ", "Có người lớn trông"], "Có người lớn trông",
            retry: new Say("Nước sâu rất nguy hiểm. Phải có người lớn ở bên con nhé!", "Deep water is dangerous. A grown-up must be with you!"));

        g.Match("v5-life-match-emergency", "Số điện thoại khẩn cấp",
            new Say("Khi cần giúp gấp, mình gọi số nào? Con nối nhé!", "Who do we call in an emergency? Match each one to its number!"),
            "📞", [("Chú công an", "Gọi 113"), ("Xe cứu hỏa", "Gọi 114"), ("Xe cứu thương", "Gọi 115")],
            retry: new Say("Con nhớ: cháy nhà gọi 114, cần xe cứu thương gọi 115 nhé!", "Remember: a fire means one one four, and an ambulance is one one five!",
                "Con nhớ: cháy nhà gọi một một bốn, cần xe cứu thương gọi một một năm nhé!"));

        // ───────────── Chặng 4: Cảm xúc của con ─────────────
        g.Stage = "Cảm xúc của con";
        g.Topic = "cam-xuc";

        var feelingPrompt = new Say("Nhìn khuôn mặt này, con đoán bạn ấy đang cảm thấy thế nào?", "Look at this face. How do you think they feel?");

        g.Pick("v5-life-face-happy", "Khuôn mặt vui", feelingPrompt, "😄",
            ["Buồn bã", "Vui vẻ", "Sợ hãi"], "Vui vẻ", focus: "😁",
            retry: new Say("Con nhìn cái miệng cười thật tươi kia nhé!", "Look at that big smiling mouth!"));

        g.Pick("v5-life-face-sad", "Khuôn mặt buồn", feelingPrompt, "😢",
            ["Vui vẻ", "Tức giận", "Buồn bã"], "Buồn bã", focus: "😞",
            retry: new Say("Con nhìn giọt nước mắt trên má bạn ấy nhé!", "Look at the tear on the cheek!"));

        g.Multi("v5-life-find-happy-faces", "Tìm những bạn đang vui",
            new Say("Con tìm tất cả các bạn đang vui nhé!", "Find all the friends who are feeling happy!"),
            "😊", ["Bạn cười tươi", "Bạn đang khóc", "Bạn cười híp mắt", "Bạn đang cau có", "Bạn đang hoảng sợ"],
            ["Bạn cười tươi", "Bạn cười híp mắt"],
            retry: new Say("Bạn đang vui thì miệng sẽ cười con ạ!", "When we're happy, our mouths smile!"));

        g.Match("v5-life-match-feelings", "Chuyện gì, cảm xúc nấy",
            new Say("Mỗi chuyện làm mình thấy thế nào? Con nối cho đúng nhé!", "How would each of these make you feel? Match them up!"),
            "💞", [("Được tặng quà", "Vui vẻ"), ("Làm hỏng đồ chơi", "Buồn bã"), ("Nghe sấm nổ to", "Sợ hãi")]);

        var sharingStory = new Say(
            "Giờ vẽ, bút màu của Tôm bị gãy hết. Tôm chỉ vẽ được đám mây xám nên ngồi buồn thiu. Mai thấy vậy liền đưa cho Tôm chiếc bút màu đỏ của mình.",
            "In drawing time, all of Tom's colouring pencils are broken. He can only draw a grey cloud, so he sits there feeling sad. Mai sees this and hands Tom her red pencil.");

        g.Story("v5-life-sharing-feel", "Tôm buồn thiu", sharingStory,
            new Say("Lúc đầu, Tôm cảm thấy thế nào?", "How does Tom feel at first?"),
            "😢", ["Vui vẻ", "Buồn bã", "Tức giận"], "Buồn bã", sharingImage,
            retry: new Say("Con nhìn mặt bạn trai đang chống cằm trong tranh nhé!", "Look at the boy's face, with his chin in his hand!"));

        g.Pick("v5-life-surprise", "Chiếc hộp bất ngờ",
            new Say("Con mở hộp ra, một chú thỏ nhảy vọt lên! Con thấy thế nào?", "You open a box and a bunny jumps out! How do you feel?"),
            "😲", ["Ngạc nhiên", "Buồn bã", "Tức giận"], "Ngạc nhiên", scene: ["📦", "🐰", "❗"],
            retry: new Say("Khi có chuyện mình không ngờ tới, mình sẽ tròn mắt ngạc nhiên đấy!", "When something unexpected happens, our eyes go wide with surprise!"));

        g.Pick("v5-life-calm-down", "Khi con tức giận",
            new Say("Bạn lấy mất đồ chơi làm con tức giận. Con nên làm gì?", "A friend took your toy and you feel angry. What should you do?"),
            "🌬️", ["Ném đồ chơi", "Hít thở thật sâu", "Đánh bạn"], "Hít thở thật sâu",
            retry: new Say("Hít vào thật sâu, thở ra thật chậm, cơn giận sẽ bay đi đấy!", "Breathe in deep and breathe out slow, and the angry feeling floats away!"));

        g.Sort("v5-life-sort-feelings", "Điều làm con vui, điều làm con buồn",
            new Say("Điều nào làm con vui, điều nào làm con buồn? Con xếp nhé!", "Which things make you happy, and which make you sad? Sort them!"),
            "🎭", ["Điều làm con vui", "Điều làm con buồn"],
            [("Được mẹ ôm", "Điều làm con vui"), ("Đi công viên chơi", "Điều làm con vui"), ("Bị ngã đau", "Điều làm con buồn"), ("Bị mất đồ chơi", "Điều làm con buồn")]);

        g.Pick("v5-life-scared", "Khi con sợ hãi",
            new Say("Đêm qua con nằm mơ thấy ác mộng, con rất sợ. Con nên làm gì?", "Last night you had a bad dream and you feel scared. What should you do?"),
            "🫂", ["Trốn một mình", "Giấu chuyện không nói", "Tìm bố mẹ để kể"], "Tìm bố mẹ để kể",
            retry: new Say("Kể cho bố mẹ nghe, con sẽ thấy đỡ sợ hơn nhiều đấy!", "Telling mum or dad helps you feel much less scared!"));

        g.Pick("v5-life-friend-fell", "Khi bạn bị ngã",
            new Say("Bạn Na bị ngã và đang khóc. Con nên làm gì?", "Na fell down and she's crying. What should you do?"),
            "🤕", ["Cười chê bạn", "Đỡ bạn dậy, hỏi han", "Chạy đi chỗ khác"], "Đỡ bạn dậy, hỏi han",
            retry: new Say("Nếu con bị ngã, con muốn bạn làm gì cho con nào?", "If you fell down, what would you want your friend to do?"));

        // ───────────── Chặng 5: Lời hay, việc tốt ─────────────
        g.Stage = "Lời hay, việc tốt";
        g.Topic = "giao-tiep";

        g.Pick("v5-life-say-thanks", "Lời cảm ơn",
            new Say("Bà cho con một quả cam thật ngon. Con nói gì với bà?", "Grandma gives you a yummy orange. What do you say?"),
            "🙏", ["Con xin lỗi ạ!", "Con cảm ơn ạ!", "Con chào cô ạ!"], "Con cảm ơn ạ!", scene: ["👵", "🍊"],
            retry: new Say("Khi được ai cho quà, mình nói lời cảm ơn con nhé!", "When someone gives you something, we say thank you!"));

        g.Pick("v5-life-say-sorry", "Lời xin lỗi",
            new Say("Con lỡ làm đổ nước vào vở của bạn. Con nói gì?", "Oops! You spilled water on your friend's notebook. What do you say?"),
            "🙇", ["Con cảm ơn ạ!", "Con chào cô ạ!", "Con xin lỗi ạ!"], "Con xin lỗi ạ!", scene: ["🥤", "💦", "📒"],
            retry: new Say("Khi mình làm bạn buồn, mình nói lời xin lỗi con nhé!", "When we make a mistake that upsets someone, we say sorry!"));

        g.Pick("v5-life-say-hello", "Lời chào buổi sáng",
            new Say("Sáng đến lớp, con gặp cô giáo ở cửa. Con nói gì?", "You arrive at school and see your teacher at the door. What do you say?"),
            "👋", ["Con chào cô ạ!", "Con xin lỗi ạ!", "Con cảm ơn ạ!"], "Con chào cô ạ!", scene: ["🏫", "👩‍🏫"],
            retry: new Say("Gặp cô buổi sáng, con khoanh tay chào cô nhé!", "When you see your teacher in the morning, say hello!"));

        g.Story("v5-life-sharing-help", "Mai cho bạn mượn bút", sharingStory,
            new Say("Bạn Mai đã làm gì để giúp Tôm?", "What does Mai do to help Tom?"),
            "🖍️", ["Cười chê bạn", "Cho bạn mượn bút màu", "Giành đồ chơi của bạn"], "Cho bạn mượn bút màu", sharingImage,
            retry: new Say("Con nhìn tay bạn Mai đang cầm gì đưa cho Tôm nhé!", "Look at what Mai is holding out to Tom!"));

        g.Story("v5-life-sharing-thanks", "Tôm nói gì với Mai?", sharingStory,
            new Say("Được bạn cho mượn bút, Tôm nên nói gì?", "Mai lent Tom a pencil. What should Tom say?"),
            "🙏", ["Con xin lỗi ạ!", "Tránh ra!", "Con cảm ơn ạ!"], "Con cảm ơn ạ!", sharingImage,
            retry: new Say("Bạn giúp mình thì mình nói lời cảm ơn con nhé!", "When a friend helps us, we say thank you!"));

        g.Multi("v5-life-multi-sharing", "Bé biết chia sẻ",
            new Say("Việc nào là biết chia sẻ? Con chọn hết nhé!", "Which of these are kind ways to share? Pick them all!"),
            "🤲", ["Cho bạn mượn bút màu", "Giành đồ chơi của bạn", "Chia bánh cho em", "Giấu kẹo ăn một mình", "Cùng chơi chung đồ chơi"],
            ["Cho bạn mượn bút màu", "Chia bánh cho em", "Cùng chơi chung đồ chơi"],
            retry: new Say("Chia sẻ là cho bạn cùng dùng, cùng chơi, cùng ăn con ạ!", "Sharing means letting others use, play and eat together with you!"));

        g.Order("v5-life-order-borrow", "Mượn đồ chơi của bạn",
            new Say("Con muốn mượn ô tô của bạn. Con làm theo thứ tự nào?", "You want to borrow your friend's toy car. What do you do, step by step?"),
            "🚗", ["Hỏi bạn: Cho tớ mượn nhé?", "Chờ bạn đồng ý", "Chơi thật nhẹ nhàng", "Trả lại và cảm ơn"],
            retry: new Say("Mình phải hỏi bạn trước, bạn đồng ý rồi mới được chơi nhé!", "First we ask, and we only play once our friend says yes!"));

        g.Pick("v5-life-new-friend", "Bạn mới đến lớp",
            new Say("Có bạn mới đến lớp, đứng một mình rất buồn. Con làm gì?", "A new kid joins the class and stands all alone, looking sad. What do you do?"),
            "🤗", ["Trêu chọc bạn", "Rủ bạn chơi cùng", "Không chơi với bạn"], "Rủ bạn chơi cùng",
            retry: new Say("Nếu con là bạn mới, con có muốn được rủ chơi cùng không?", "If you were the new kid, wouldn't you want someone to play with?"));

        g.Sort("v5-life-sort-words", "Lời nói dễ thương",
            new Say("Lời nào dễ thương, lời nào làm bạn buồn? Con xếp nhé!", "Which words are kind, and which make a friend sad? Sort them!"),
            "💬", ["Lời nói dễ thương", "Lời nói làm bạn buồn"],
            [("Con cảm ơn ạ!", "Lời nói dễ thương"), ("Con xin lỗi ạ!", "Lời nói dễ thương"), ("Tránh ra!", "Lời nói làm bạn buồn"), ("Tớ không cho đâu!", "Lời nói làm bạn buồn")]);

        g.Pick("v5-life-listen-teacher", "Khi cô đang nói",
            new Say("Cô đang kể chuyện cho cả lớp. Con làm gì nhỉ?", "Your teacher is telling the class a story. What should you do?"),
            "👂", ["Nói chuyện riêng", "Chạy quanh lớp", "Ngồi ngoan lắng nghe"], "Ngồi ngoan lắng nghe",
            retry: new Say("Muốn nghe được chuyện hay, mình phải ngồi yên lắng nghe con nhé!", "To hear the story, we sit still and listen!"));

        g.Pick("v5-life-raise-hand", "Muốn trả lời cô",
            new Say("Con biết câu trả lời và muốn nói cho cô nghe. Con làm gì?", "You know the answer and want to tell your teacher. What do you do?"),
            "✋", ["Hét thật to", "Giơ tay xin phát biểu", "Đứng lên ghế"], "Giơ tay xin phát biểu",
            retry: new Say("Trong lớp, mình giơ tay và chờ cô gọi tên nhé!", "In class, we put up our hand and wait for the teacher to call on us!"));

        g.Match("v5-life-match-polite", "Nói lời hay đúng lúc",
            new Say("Mỗi lúc mình nói một lời hay. Con nối cho đúng nhé!", "Each moment needs its own polite words. Match them up!"),
            "💖", [("Được tặng quà", "Con cảm ơn ạ!"), ("Làm bạn bị ngã", "Con xin lỗi ạ!"), ("Gặp cô giáo buổi sáng", "Con chào cô ạ!")]);
    }
}

public static partial class Lexicon
{
    private const string LifeTable = """
        // Tự phục vụ
        Trước khi ăn cơm | 🍚 | before eating
        Sau khi đi vệ sinh | 🚽 | after using the toilet
        Sau khi chơi ngoài sân | 🛝 | after playing outside
        Khi đang ngủ | 😴 | while sleeping
        Khi nghe nhạc | 🎵 | while listening to music
        Làm ướt tay | 💧 | wet your hands
        Xoa xà phòng | 🫧 | rub on soap
        Xả sạch bọt | 🚰 | rinse off the bubbles
        Lau khô tay | 🧻 | dry your hands
        Thức dậy | 🌅 | wake up
        Đánh răng rửa mặt | 🪥 | brush teeth and wash face
        Ăn sáng | 🥣 | eat breakfast
        Đeo cặp đi học | 🎒 | grab my school bag and go
        Hàm răng | 🦷 | teeth
        Đôi bàn tay | 👐 | hands
        Bàn chân | 🦶 | feet
        Mặc quần áo | 👕 | put on clothes
        Đi tất | 🧦 | put on socks
        Đi giày | 👟 | put on shoes
        Hộp đồ chơi | 📦 | toy box
        Giá sách | 📚 | bookshelf
        Cất đồ chơi vào hộp | 🧺 | put the toys back in the box
        Để đồ chơi bừa trên sàn | 🧩 | leave the toys on the floor
        Đá đồ chơi vào gầm giường | 🦶 | kick the toys under the bed
        Đánh răng | 🪥 | brush my teeth
        Ăn thêm kẹo | 🍬 | eat more candy
        Xem điện thoại | 📱 | watch the phone
        // Đi đường an toàn
        Đèn tín hiệu đỏ | /images/pictograms/traffic-red.svg | red light
        Đèn tín hiệu vàng | /images/pictograms/traffic-yellow.svg | yellow light
        Đèn tín hiệu xanh | /images/pictograms/traffic-green.svg | green light
        Dừng lại chờ | ✋ | stop and wait
        Đi chậm, chuẩn bị dừng | ⚠️ | slow down and get ready to stop
        Được đi qua | 🚶 | you can go
        Dừng lại ở mép đường | 🛑 | stop at the edge of the road
        Nhìn trái, nhìn phải | 👀 | look left and right
        Đi trên vạch kẻ đường | 🚸 | walk on the zebra crossing
        Đội mũ bảo hiểm khi đi xe | ⛑️ | wear a helmet on a bike
        Nắm tay người lớn khi qua đường | 🤝 | hold a grown-up's hand to cross
        Ngồi ngay ngắn trên xe | 💺 | sit still in my seat
        Chạy ra lòng đường | 🏃 | run into the road
        Đá bóng dưới lòng đường | ⚽ | play ball in the road
        Dưới lòng đường | 🛣️ | in the road
        Ở sân chơi | 🛝 | at the playground
        Cạnh bờ ao | 🌊 | next to the pond
        // An toàn ở nhà
        Ổ cắm điện | 🔌 | electric socket
        Bếp lửa đang cháy | 🔥 | a hot stove
        Con dao sắc | 🔪 | a sharp knife
        Nhận kẹo và đi theo | 🍬 | take the candy and go
        Từ chối và tìm bố mẹ | 🙅 | say no and find mum or dad
        Đứng im không nói gì | 🤐 | stand still and say nothing
        Đi theo người lạ | 🧔 | go with a stranger
        Chú công an | 👮 | police officer
        Một mình chạy ra đường | 🏃 | run outside alone
        Ăn thử một viên | 💊 | try one
        Chia cho bạn ăn | 👫 | share them with a friend
        Đưa cho người lớn | 🙋 | give them to a grown-up
        Làm thế là an toàn | ✅ | that's safe
        Làm thế là nguy hiểm | ⛔ | that's dangerous
        Ngồi xếp hình | 🧩 | doing a puzzle
        Tô màu tranh | 🖍️ | colouring a picture
        Leo lên bậu cửa sổ | 🪟 | climbing onto the window
        Nghịch ổ cắm điện | 🔌 | playing with the socket
        Đi bơi một mình | 🏊 | swim alone
        Đi với bạn nhỏ | 👫 | go with a little friend
        Có người lớn trông | 👨‍👩‍👧 | with a grown-up watching
        Gọi 113 | 🚓 | call one one three | Gọi một một ba
        Gọi 114 | 🧯 | call one one four | Gọi một một bốn
        Gọi 115 | 🏥 | call one one five | Gọi một một năm
        // Cảm xúc
        Vui vẻ | 😄 | happy
        Buồn bã | 😢 | sad
        Tức giận | 😠 | angry
        Sợ hãi | 😨 | scared
        Ngạc nhiên | 😲 | surprised
        Bạn cười tươi | pic:😄 | a big smile
        Bạn cười híp mắt | pic:😊 | a happy smile
        Bạn đang khóc | pic:😭 | crying
        Bạn đang cau có | pic:😠 | grumpy
        Bạn đang hoảng sợ | pic:😱 | frightened
        Được tặng quà | 🎁 | getting a present
        Làm hỏng đồ chơi | 💔 | breaking a toy
        Nghe sấm nổ to | ⛈️ | hearing loud thunder
        Ném đồ chơi | 🧸 | throw my toys
        Hít thở thật sâu | 🌬️ | take a deep breath
        Đánh bạn | 👊 | hit my friend
        Điều làm con vui | 😄 | things that make me happy
        Điều làm con buồn | 😢 | things that make me sad
        Được mẹ ôm | 🤗 | a hug from mum
        Đi công viên chơi | 🎡 | a trip to the park
        Bị ngã đau | 🤕 | falling and getting hurt
        Bị mất đồ chơi | 🔍 | losing a toy
        Trốn một mình | 🙈 | hide all alone
        Giấu chuyện không nói | 🤐 | keep it a secret
        Tìm bố mẹ để kể | 🫂 | find mum or dad and tell them
        Cười chê bạn | 😆 | laugh at them
        Đỡ bạn dậy, hỏi han | 🤝 | help them up and ask if they're okay
        Chạy đi chỗ khác | 💨 | run away
        // Giao tiếp và chia sẻ
        Con cảm ơn ạ! | 🙏 | Thank you!
        Con xin lỗi ạ! | 🙇 | I'm sorry!
        Con chào cô ạ! | 👋 | Good morning, teacher!
        Tránh ra! | 😤 | Go away!
        Tớ không cho đâu! | 🙅 | No, you can't have it!
        Cho bạn mượn bút màu | 🖍️ | lend a friend a colouring pencil
        Giành đồ chơi của bạn | ✊ | grab a friend's toy
        Chia bánh cho em | 🍪 | share a cookie with my little sister
        Giấu kẹo ăn một mình | 🍬 | hide candy to eat alone
        Cùng chơi chung đồ chơi | 🧸 | play with toys together
        Hỏi bạn: Cho tớ mượn nhé? | 🙋 | ask, "Can I borrow it, please?"
        Chờ bạn đồng ý | ⏳ | wait for them to say yes
        Chơi thật nhẹ nhàng | 🤲 | play gently
        Trả lại và cảm ơn | 🙏 | give it back and say thank you
        Trêu chọc bạn | 😝 | tease them
        Rủ bạn chơi cùng | 🤗 | invite them to play
        Không chơi với bạn | 🙅 | leave them out
        Lời nói dễ thương | 💖 | kind words
        Lời nói làm bạn buồn | 💔 | unkind words
        Nói chuyện riêng | 🗣️ | chat with friends
        Chạy quanh lớp | 🏃 | run around the classroom
        Ngồi ngoan lắng nghe | 👂 | sit nicely and listen
        Hét thật to | 📢 | shout really loudly
        Giơ tay xin phát biểu | /images/pictograms/raised-hand.svg | put up my hand
        Đứng lên ghế | 🪑 | stand on the chair
        Làm bạn bị ngã | 🤕 | making a friend fall
        Gặp cô giáo buổi sáng | 🏫 | seeing my teacher in the morning
        """;
}
