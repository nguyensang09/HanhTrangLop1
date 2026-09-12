/**
 * GAME_LEVELS_CATALOG
 * Dữ liệu màn chơi chuẩn hóa cho 4 trò chơi: Bubble, Number Train, Balloon Word, Gold Miner
 * Tuân thủ nguyên tắc: KHÔNG điểm số, KHÔNG coin, nhiệm vụ rõ ràng và voice tiếng Anh ngắn gọn.
 */
window.GAME_LEVELS = {
    // 1. 🎈 BẮN BONG BÓNG CHỮ CÁI (BUBBLE POP ROCKET)
    "bubble": [
        {
            level: 1,
            target: "A",
            choices: ["A", "B"],
            targetCount: 2,
            instruction: "Bé ngắm bắn 2 bong bóng chữ A nhé!",
            instructionEn: "Pop letter A!"
        },
        {
            level: 2,
            target: "B",
            choices: ["B", "A", "C"],
            targetCount: 2,
            instruction: "Bé ngắm bắn 2 bong bóng chữ B nhé!",
            instructionEn: "Pop letter B!"
        },
        {
            level: 3,
            target: "C",
            choices: ["C", "B", "D"],
            targetCount: 2,
            instruction: "Bé hãy bắn các bong bóng chữ C nhé!",
            instructionEn: "Pop letter C!"
        },
        {
            level: 4,
            target: "D",
            choices: ["D", "O", "B"],
            targetCount: 3,
            instruction: "Bé hãy bắn 3 bong bóng chữ D!",
            instructionEn: "Pop letter D!"
        },
        {
            level: 5,
            target: "E",
            choices: ["E", "F", "C"],
            targetCount: 3,
            instruction: "Bé hãy bắn các bong bóng chữ E!",
            instructionEn: "Pop letter E!"
        },
        {
            level: 6,
            target: "a",
            choices: ["a", "A", "b"],
            targetCount: 2,
            instruction: "Bé hãy tìm và bắn chữ a nhỏ (thường)!",
            instructionEn: "Pop lowercase a!"
        },
        {
            level: 7,
            target: "b",
            choices: ["b", "B", "d"],
            targetCount: 2,
            instruction: "Bé hãy bắn chữ b nhỏ (thường)!",
            instructionEn: "Pop lowercase b!"
        },
        {
            level: 8,
            target: "C",
            hintWord: "CAT",
            hintEmoji: "🐱",
            choices: ["C", "A", "T"],
            targetCount: 3,
            instruction: "Cat chú mèo bắt đầu bằng chữ C. Hãy bắn chữ C!",
            instructionEn: "C is for Cat! Pop letter C!"
        },
        {
            level: 9,
            target: "D",
            hintWord: "DOG",
            hintEmoji: "🐶",
            choices: ["D", "O", "G"],
            targetCount: 3,
            instruction: "Dog chú cún bắt đầu bằng chữ D. Hãy bắn chữ D!",
            instructionEn: "D is for Dog! Pop letter D!"
        },
        {
            level: 10,
            target: "S",
            hintWord: "SUN",
            hintEmoji: "☀️",
            choices: ["S", "U", "N"],
            targetCount: 3,
            instruction: "Sun mặt trời bắt đầu bằng chữ S. Hãy bắn chữ S!",
            instructionEn: "S is for Sun! Pop letter S!"
        },
        {
            level: 11,
            target: "M",
            choices: ["M", "W", "N"],
            targetCount: 3,
            speed: "fast",
            instruction: "Gió thổi bóng bay nhanh hơn! Bắn chữ M!",
            instructionEn: "Fast wind! Pop letter M!"
        },
        {
            level: 12,
            target: "O",
            choices: ["O", "Q", "C"],
            targetCount: 4,
            hasRainbow: true,
            instruction: "Bắn chữ O và khám phá bong bóng cầu vồng ma thuật!",
            instructionEn: "Pop letter O!"
        }
    ],

    // 2. 🚂 ĐOÀN TÀU CHỞ SỐ (NUMBER TRAIN)
    "number-train": [
        {
            level: 1,
            type: "missing-wagon",
            trainSequence: [1, null, 3],
            choices: [2, 4, 5],
            answer: 2,
            instruction: "Bé hãy tìm toa số 2 nối vào đoàn tàu nhé!",
            instructionEn: "Find wagon 2!"
        },
        {
            level: 2,
            type: "missing-wagon",
            trainSequence: [2, 3, null],
            choices: [4, 1, 5],
            answer: 4,
            instruction: "Toa nào chạy ngay sau toa số 3 nhỉ?",
            instructionEn: "Find wagon 4!"
        },
        {
            level: 3,
            type: "missing-wagon",
            trainSequence: [1, 2, null, 4],
            choices: [3, 5, 6],
            answer: 3,
            instruction: "Tìm toa số 3 còn thiếu giữa số 2 và 4!",
            instructionEn: "Find wagon 3!"
        },
        {
            level: 4,
            type: "missing-wagon",
            trainSequence: [null, 6, 7],
            choices: [5, 4, 8],
            answer: 5,
            instruction: "Toa số mấy chạy ngay trước toa số 6?",
            instructionEn: "Find wagon 5!"
        },
        {
            level: 5,
            type: "wagon-sort",
            wagons: [3, 1, 2],
            correctOrder: [1, 2, 3],
            instruction: "Bé chạm lần lượt các toa theo thứ tự: 1, 2, 3!",
            instructionEn: "Sort wagons: 1, 2, 3!"
        },
        {
            level: 6,
            type: "wagon-sort",
            wagons: [4, 2, 1, 3],
            correctOrder: [1, 2, 3, 4],
            instruction: "Bé chạm nối các toa theo thứ tự từ 1 đến 4!",
            instructionEn: "Sort wagons: 1, 2, 3, 4!"
        },
        {
            level: 7,
            type: "cargo-count",
            cargoEmoji: "🍎",
            cargoName: "Táo",
            countRequired: 3,
            instruction: "Bác lái tàu nhờ bé chất 3 quả táo vào toa xe!",
            instructionEn: "Load 3 apples onto the train!"
        },
        {
            level: 8,
            type: "cargo-count",
            cargoEmoji: "🐥",
            cargoName: "Gà Con",
            countRequired: 4,
            instruction: "Bé hãy đưa 4 chú gà con lên toa tàu nhé!",
            instructionEn: "Load 4 chicks onto the train!"
        },
        {
            level: 9,
            type: "cargo-count",
            cargoEmoji: "⭐",
            cargoName: "Ngôi Sao",
            countRequired: 5,
            instruction: "Bé hãy thu thập 5 ngôi sao lấp lánh vào toa tàu!",
            instructionEn: "Load 5 stars onto the train!"
        },
        {
            level: 10,
            type: "missing-wagon",
            trainSequence: [7, 8, null, 10],
            choices: [9, 6, 5],
            answer: 9,
            instruction: "Toa số mấy đứng ngay trước số 10 để tàu về ga?",
            instructionEn: "Find wagon 9!"
        }
    ],

    // 3. ☁️ KHINH KHÍ CẦU GHÉP VẦN (HOT AIR BALLOON PHONICS)
    "balloon-word": [
        {
            level: 1,
            targetWord: "BA",
            letters: ["B", "A"],
            decoys: ["C", "M"],
            emoji: "👶",
            wordMeaning: "Em Bé / Ba",
            instruction: "Bé chạm mây chữ B và A để nạp lửa cho khinh khí cầu bay lên: BA!",
            instructionEn: "Fly with word BA!"
        },
        {
            level: 2,
            targetWord: "BO",
            letters: ["B", "O"],
            decoys: ["D", "E"],
            emoji: "🐮",
            wordMeaning: "Chú Bò",
            instruction: "Bé chạm mây chữ B và O để khinh khí cầu bay cao: BO!",
            instructionEn: "Fly with word BO!"
        },
        {
            level: 3,
            targetWord: "CO",
            letters: ["C", "O"],
            decoys: ["A", "N"],
            emoji: "🦆",
            wordMeaning: "Con Cò",
            instruction: "Bé chạm mây chữ C và O để vượt qua tầng mây: CO!",
            instructionEn: "Fly with word CO!"
        },
        {
            level: 4,
            targetWord: "ME",
            letters: ["M", "E"],
            decoys: ["B", "I"],
            emoji: "👩",
            wordMeaning: "Mẹ Yêu",
            instruction: "Bé chạm mây chữ M và E nạp năng lượng cho khinh khí cầu: ME!",
            instructionEn: "Fly with word ME!"
        },
        {
            level: 5,
            targetWord: "CAT",
            letters: ["C", "A", "T"],
            decoys: ["B", "O"],
            emoji: "🐱",
            wordMeaning: "Con Mèo",
            instruction: "Ghép từ CAT (Con mèo): C - A - T để bay qua núi!",
            instructionEn: "Fly with word CAT!"
        },
        {
            level: 6,
            targetWord: "SUN",
            letters: ["S", "U", "N"],
            decoys: ["M", "O"],
            emoji: "☀️",
            wordMeaning: "Mặt Trời",
            instruction: "Ghép từ SUN (Mặt trời): S - U - N hướng về phía ánh sáng!",
            instructionEn: "Fly with word SUN!"
        },
        {
            level: 7,
            targetWord: "DOG",
            letters: ["D", "O", "G"],
            decoys: ["C", "A"],
            emoji: "🐶",
            wordMeaning: "Chú Cún",
            instruction: "Ghép từ DOG (Chú cún): D - O - G đưa khinh khí cầu bay cao!",
            instructionEn: "Fly with word DOG!"
        },
        {
            level: 8,
            targetWord: "BUS",
            letters: ["B", "U", "S"],
            decoys: ["P", "T"],
            emoji: "🚌",
            wordMeaning: "Xe Buýt",
            instruction: "Ghép từ BUS (Xe buýt): B - U - S đưa khinh khí cầu vượt mây xanh!",
            instructionEn: "Fly with word BUS!"
        },
        {
            level: 9,
            targetWord: "CUP",
            letters: ["C", "U", "P"],
            decoys: ["D", "A"],
            emoji: "☕",
            wordMeaning: "Chiếc Cốc",
            instruction: "Ghép từ CUP (Chiếc cốc): C - U - P tiếp lửa cho chuyến bay!",
            instructionEn: "Fly with word CUP!"
        },
        {
            level: 10,
            targetWord: "STAR",
            letters: ["S", "T", "A", "R"],
            decoys: ["B", "O"],
            emoji: "⭐",
            wordMeaning: "Ngôi Sao",
            instruction: "Ghép từ STAR (Ngôi sao): S - T - A - R bay qua cầu vồng tuyệt đẹp!",
            instructionEn: "Fly with word STAR!"
        }
    ],

    // 4. ⛏️ MỎ VÀNG TRI THỨC (GOLD MINER - 29 CHỮ CÁI TIẾNG VIỆT + 10 CHỮ SỐ)
    "gold-miner": [
        // --- 29 CHỮ CÁI TIẾNG VIỆT ---
        { level: 1, target: "A", targetType: "letter", instruction: "Con hãy gắp các khối chữ A màu vàng nhé!", instructionEn: "Mine letter A!" },
        { level: 2, target: "Ă", targetType: "letter", instruction: "Con hãy tìm chữ Ă có chiếc mũ ngược xinh xắn!", instructionEn: "Mine letter Ă!" },
        { level: 3, target: "Â", targetType: "letter", instruction: "Con hãy tìm chữ Â có chiếc nón úp xinh xắn!", instructionEn: "Mine letter Â!" },
        { level: 4, target: "B", targetType: "letter", instruction: "Con hãy gắp các khối chữ B màu vàng nhé!", instructionEn: "Mine letter B!" },
        { level: 5, target: "C", targetType: "letter", instruction: "Con hãy tìm khối chữ C cong tròn vầng trăng!", instructionEn: "Mine letter C!" },
        { level: 6, target: "D", targetType: "letter", instruction: "Con hãy tìm khối chữ D màu vàng óng!", instructionEn: "Mine letter D!" },
        { level: 7, target: "Đ", targetType: "letter", instruction: "Con hãy tìm chữ Đ có chiếc gạch ngang!", instructionEn: "Mine letter Đ!" },
        { level: 8, target: "E", targetType: "letter", instruction: "Con hãy gắp khối chữ E màu vàng nhé!", instructionEn: "Mine letter E!" },
        { level: 9, target: "Ê", targetType: "letter", instruction: "Con hãy tìm chữ Ê đội chiếc mũ nhỏ xinh!", instructionEn: "Mine letter Ê!" },
        { level: 10, target: "G", targetType: "letter", instruction: "Con hãy tìm khối chữ G màu vàng!", instructionEn: "Mine letter G!" },
        { level: 11, target: "H", targetType: "letter", instruction: "Con hãy tìm chữ H nét thẳng cao!", instructionEn: "Mine letter H!" },
        { level: 12, target: "I", targetType: "letter", instruction: "Con hãy tìm khối chữ I có dấu chấm nhỏ!", instructionEn: "Mine letter I!" },
        { level: 13, target: "K", targetType: "letter", instruction: "Con hãy tìm chữ K màu vàng rực rỡ!", instructionEn: "Mine letter K!" },
        { level: 14, target: "L", targetType: "letter", instruction: "Con hãy gắp khối chữ L nét thẳng!", instructionEn: "Mine letter L!" },
        { level: 15, target: "M", targetType: "letter", instruction: "Con hãy tìm chữ M có hai chiếc cầu cong!", instructionEn: "Mine letter M!" },
        { level: 16, target: "N", targetType: "letter", instruction: "Con hãy tìm khối chữ N màu vàng!", instructionEn: "Mine letter N!" },
        { level: 17, target: "O", targetType: "letter", instruction: "Con hãy tìm chữ O tròn như quả trứng!", instructionEn: "Mine letter O!" },
        { level: 18, target: "Ô", targetType: "letter", instruction: "Con hãy gắp chữ Ô đội chiếc nón lá!", instructionEn: "Mine letter Ô!" },
        { level: 19, target: "Ơ", targetType: "letter", instruction: "Con hãy tìm chữ Ơ có chiếc râu nhỏ xinh!", instructionEn: "Mine letter Ơ!" },
        { level: 20, target: "P", targetType: "letter", instruction: "Con hãy tìm khối chữ P màu vàng!", instructionEn: "Mine letter P!" },
        { level: 21, target: "Q", targetType: "letter", instruction: "Con hãy gắp chữ Q tròn xinh có nét móc!", instructionEn: "Mine letter Q!" },
        { level: 22, target: "R", targetType: "letter", instruction: "Con hãy tìm chữ R màu vàng lấp lánh!", instructionEn: "Mine letter R!" },
        { level: 23, target: "S", targetType: "letter", instruction: "Con hãy tìm chữ S uốn lượn như dòng suối!", instructionEn: "Mine letter S!" },
        { level: 24, target: "T", targetType: "letter", instruction: "Con hãy gắp khối chữ T màu vàng!", instructionEn: "Mine letter T!" },
        { level: 25, target: "U", targetType: "letter", instruction: "Con hãy tìm khối chữ U như chiếc võng nhỏ!", instructionEn: "Mine letter U!" },
        { level: 26, target: "Ư", targetType: "letter", instruction: "Con hãy tìm chữ Ư có thêm chiếc móc câu!", instructionEn: "Mine letter Ư!" },
        { level: 27, target: "V", targetType: "letter", instruction: "Con hãy tìm chữ V nét vát nhọn xinh xắn!", instructionEn: "Mine letter V!" },
        { level: 28, target: "X", targetType: "letter", instruction: "Con hãy gắp chữ X chéo nhau như cánh quạt!", instructionEn: "Mine letter X!" },
        { level: 29, target: "Y", targetType: "letter", instruction: "Con hãy tìm chữ Y dài màu vàng óng ánh!", instructionEn: "Mine letter Y!" },

        // --- 10 CHỮ SỐ (0 ĐẾN 9) ---
        { level: 30, target: "0", targetType: "number", instruction: "Bé hãy tìm và gắp số 0 tròn trĩnh nhé!", instructionEn: "Mine number 0!" },
        { level: 31, target: "1", targetType: "number", instruction: "Con hãy tìm khối số 1 thẳng đứng như cây nến!", instructionEn: "Mine number 1!" },
        { level: 32, target: "2", targetType: "number", instruction: "Con hãy tìm số 2 cong cong như chú vịt nhỏ!", instructionEn: "Mine number 2!" },
        { level: 33, target: "3", targetType: "number", instruction: "Con hãy chạm vào khối số 3 may mắn!", instructionEn: "Mine number 3!" },
        { level: 34, target: "4", targetType: "number", instruction: "Con hãy tìm khối số 4 như chiếc ghế gập!", instructionEn: "Mine number 4!" },
        { level: 35, target: "5", targetType: "number", instruction: "Con hãy tìm khối số 5 có chiếc bụng tròn nhé!", instructionEn: "Mine number 5!" },
        { level: 36, target: "6", targetType: "number", instruction: "Con hãy gắp số 6 có nét móc xoắn phía dưới!", instructionEn: "Mine number 6!" },
        { level: 37, target: "7", targetType: "number", instruction: "Con hãy tìm khối số 7 như chiếc cuốc nhỏ!", instructionEn: "Mine number 7!" },
        { level: 38, target: "8", targetType: "number", instruction: "Con hãy tìm số 8 tròn xinh hai vòng!", instructionEn: "Mine number 8!" },
        { level: 39, target: "9", targetType: "number", instruction: "Con hãy tìm số 9 có chiếc đuôi uốn cong xinh xắn!", instructionEn: "Mine number 9!" }
    ]
};
