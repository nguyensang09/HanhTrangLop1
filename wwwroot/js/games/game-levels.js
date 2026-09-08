/**
 * GAME_LEVELS_CATALOG
 * Dữ liệu bài học cho 4 trò chơi: Bubble, Gold Miner, Balloon Word, Number Train
 */
window.GAME_LEVELS = {
    // 1. BẮN BONG BÓNG CHỮ CÁI
    "bubble": [
        {
            level: 1,
            target: "A",
            choices: ["A", "B"],
            instruction: "Con hãy tìm chữ A",
            praiseText: "Giỏi quá! Đây là chữ A.",
            rewardStars: 1
        },
        {
            level: 2,
            target: "B",
            choices: ["A", "B", "C"],
            instruction: "Con hãy tìm chữ B",
            praiseText: "Tuyệt vời! Đây là chữ B.",
            rewardStars: 1
        },
        {
            level: 3,
            target: "Ă",
            choices: ["A", "Ă", "Â"],
            instruction: "Con hãy tìm chữ Ă có dấu trăng cười",
            praiseText: "Đúng rồi! Đây là chữ Ă.",
            rewardStars: 1
        },
        {
            level: 4,
            target: "Â",
            choices: ["A", "Ă", "Â"],
            instruction: "Con hãy tìm chữ Â đội mũ xinh",
            praiseText: "Hoan hô bé! Đây là chữ Â.",
            rewardStars: 1
        },
        {
            level: 5,
            target: "Ô",
            choices: ["O", "Ô", "Ơ"],
            instruction: "Con hãy tìm chữ Ô có chiếc mũ",
            praiseText: "Rất giỏi! Đây là chữ Ô.",
            rewardStars: 1
        },
        {
            level: 6,
            target: "Ơ",
            choices: ["O", "Ô", "Ơ"],
            instruction: "Con hãy tìm chữ Ơ có râu nhỏ",
            praiseText: "Chính xác! Đây là chữ Ơ.",
            rewardStars: 1
        },
        {
            level: 7,
            target: "Đ",
            choices: ["D", "Đ", "B"],
            instruction: "Con hãy tìm chữ Đ có nét gạch ngang",
            praiseText: "Bé giỏi lắm! Đây là chữ Đ.",
            rewardStars: 1
        },
        {
            level: 8,
            target: "Ê",
            choices: ["E", "Ê", "C"],
            instruction: "Con hãy tìm chữ Ê đội nón nhé",
            praiseText: "Đúng rồi! Đây là chữ Ê.",
            rewardStars: 1
        },
        {
            level: 9,
            target: "Ư",
            choices: ["U", "Ư", "V"],
            instruction: "Con hãy tìm chữ Ư có chiếc móc",
            praiseText: "Tuyệt vời! Đây là chữ Ư.",
            rewardStars: 1
        },
        {
            level: 10,
            target: "a",
            choices: ["A", "a", "B", "b"],
            instruction: "Con hãy tìm chữ a viết thường",
            praiseText: "Giỏi quá! Đây là chữ a thường.",
            rewardStars: 2
        },
        {
            level: 11,
            target: "C",
            choices: ["B", "C", "D"],
            instruction: "Con cá bắt đầu bằng chữ gì nhỉ?",
            hintImage: "🐟",
            hintWord: "CON CÁ",
            praiseText: "Cờ - Cá! Rất chính xác.",
            rewardStars: 2
        },
        {
            level: 12,
            target: "G",
            choices: ["G", "H", "K"],
            instruction: "Con gà bắt đầu bằng chữ gì nào?",
            hintImage: "🐔",
            hintWord: "CON GÀ",
            praiseText: "Gờ - Gà! Bé làm xuất sắc lắm.",
            rewardStars: 2
        }
    ],

    // 2. MỎ VÀNG TRI THỨC - ĐÀO VÀNG CHỮ & SỐ (CHUẨN ĐẶC TẢ TƯƠNG TÁC 1 CHẠM)
    "gold-miner": [
        {
            level: 1,
            target: "A",
            targetType: "letter",
            instruction: "Con hãy tìm và chạm vào khối chữ A nhé",
            mode: "find-letter",
            items: [
                { text: "A", type: "gold", x: 220, y: 340, size: 56, points: 10 },
                { text: "B", type: "rock", x: 120, y: 260, size: 50, points: 0 },
                { text: "C", type: "gem", x: 380, y: 280, size: 52, points: 0 },
                { text: "D", type: "amethyst", x: 500, y: 330, size: 50, points: 0 },
                { text: "E", type: "rock", x: 320, y: 390, size: 52, points: 0 },
                { text: "💎", type: "gem", x: 170, y: 390, size: 42, points: 5 }
            ],
            praiseText: "Đúng rồi! Đây là chữ A. Bé giỏi quá!",
            rewardStars: 1
        },
        {
            level: 2,
            target: "Ă",
            targetType: "letter",
            instruction: "Con hãy tìm chữ Ă có chiếc mũ ngược xinh xắn",
            mode: "find-letter",
            items: [
                { text: "A", type: "rock", x: 140, y: 270, size: 50, points: 0 },
                { text: "Â", type: "ruby", x: 460, y: 270, size: 50, points: 0 },
                { text: "Ă", type: "gold", x: 300, y: 340, size: 56, points: 10 },
                { text: "B", type: "amethyst", x: 200, y: 380, size: 52, points: 0 },
                { text: "C", type: "rock", x: 420, y: 370, size: 50, points: 0 },
                { text: "⭐", type: "star", x: 110, y: 360, size: 42, points: 5 }
            ],
            praiseText: "Tuyệt vời! Chữ Ă có chiếc mũ trăng khuyết.",
            rewardStars: 1
        },
        {
            level: 3,
            target: "Ơ",
            targetType: "letter",
            instruction: "Con hãy tìm khối chữ Ơ có chiếc râu nhỏ nào",
            mode: "find-letter",
            items: [
                { text: "O", type: "rock", x: 130, y: 300, size: 52, points: 0 },
                { text: "Ô", type: "amethyst", x: 250, y: 260, size: 52, points: 0 },
                { text: "Ơ", type: "gold", x: 380, y: 330, size: 56, points: 10 },
                { text: "C", type: "gem", x: 490, y: 290, size: 50, points: 0 },
                { text: "Q", type: "rock", x: 240, y: 380, size: 50, points: 0 },
                { text: "🎁", type: "chest", x: 430, y: 390, size: 44, points: 5 }
            ],
            praiseText: "Chính xác! Ơ tròn như quả trứng có thêm chiếc râu.",
            rewardStars: 1
        },
        {
            level: 4,
            target: "3",
            targetType: "number",
            instruction: "Con hãy chạm vào khối số 3 may mắn",
            mode: "find-number",
            items: [
                { text: "1", type: "rock", x: 130, y: 270, size: 50, points: 0 },
                { text: "2", type: "gem", x: 470, y: 280, size: 50, points: 0 },
                { text: "3", type: "gold", x: 280, y: 340, size: 56, points: 10 },
                { text: "4", type: "amethyst", x: 170, y: 370, size: 50, points: 0 },
                { text: "5", type: "ruby", x: 410, y: 370, size: 50, points: 0 },
                { text: "💎", type: "gem", x: 310, y: 260, size: 42, points: 5 }
            ],
            praiseText: "Đúng số 3 rồi! Bé đếm giỏi lắm.",
            rewardStars: 1
        },
        {
            level: 5,
            target: "5",
            targetType: "number",
            instruction: "Con hãy tìm khối số 5 có chiếc bụng tròn nhé",
            mode: "find-number",
            items: [
                { text: "2", type: "rock", x: 140, y: 320, size: 50, points: 0 },
                { text: "4", type: "amethyst", x: 240, y: 270, size: 50, points: 0 },
                { text: "5", type: "gold", x: 360, y: 350, size: 56, points: 10 },
                { text: "6", type: "gem", x: 480, y: 310, size: 50, points: 0 },
                { text: "7", type: "rock", x: 250, y: 390, size: 50, points: 0 },
                { text: "⭐", type: "star", x: 430, y: 390, size: 42, points: 5 }
            ],
            praiseText: "Hoan hô! Bé đã đào được số 5 vàng óng.",
            rewardStars: 1
        },
        {
            level: 6,
            target: "8",
            targetType: "number",
            instruction: "Cẩn thận nhầm chữ B với số 8 nhé! Hãy đào số 8",
            mode: "find-number",
            items: [
                { text: "B", type: "rock", x: 160, y: 310, size: 52, points: 0 },
                { text: "3", type: "amethyst", x: 260, y: 260, size: 50, points: 0 },
                { text: "8", type: "gold", x: 370, y: 340, size: 56, points: 10 },
                { text: "9", type: "gem", x: 480, y: 280, size: 50, points: 0 },
                { text: "0", type: "ruby", x: 220, y: 390, size: 50, points: 0 },
                { text: "💎", type: "gem", x: 450, y: 370, size: 42, points: 5 }
            ],
            praiseText: "Mắt bé tinh quá! Đây chính là số 8 tròn xinh.",
            rewardStars: 2
        },
        {
            level: 7,
            target: "Đ",
            targetType: "letter",
            instruction: "Bé hãy lắng nghe và tìm chữ Đ có nét gạch ngang",
            mode: "listen-letter",
            audioTarget: "Đ",
            items: [
                { text: "D", type: "rock", x: 140, y: 290, size: 52, points: 0 },
                { text: "Đ", type: "gold", x: 300, y: 350, size: 56, points: 10 },
                { text: "B", type: "amethyst", x: 430, y: 280, size: 50, points: 0 },
                { text: "O", type: "gem", x: 200, y: 380, size: 50, points: 0 },
                { text: "P", type: "ruby", x: 470, y: 360, size: 50, points: 0 },
                { text: "🎁", type: "chest", x: 330, y: 260, size: 44, points: 5 }
            ],
            praiseText: "Tuyệt vời! Bé nghe rất chuẩn chữ Đ.",
            rewardStars: 2
        },
        {
            level: 8,
            target: "m",
            targetType: "letter",
            instruction: "Con hãy tìm chữ thường m của chữ M in hoa",
            mode: "case-match",
            hintWord: "M in hoa ➔ m thường",
            items: [
                { text: "n", type: "rock", x: 150, y: 310, size: 52, points: 0 },
                { text: "m", type: "gold", x: 320, y: 340, size: 56, points: 10 },
                { text: "u", type: "amethyst", x: 460, y: 270, size: 50, points: 0 },
                { text: "w", type: "gem", x: 220, y: 260, size: 50, points: 0 },
                { text: "v", type: "rock", x: 410, y: 360, size: 50, points: 0 },
                { text: "⭐", type: "star", x: 190, y: 380, size: 42, points: 5 }
            ],
            praiseText: "Đúng rồi! Đây là chữ m viết thường.",
            rewardStars: 2
        },
        {
            level: 9,
            target: "4",
            targetType: "count",
            instruction: "Có bao nhiêu viên kim cương 💎 đang tỏa sáng? Bé chọn số đúng nhé",
            mode: "count-objects",
            countIcons: "💎 💎 💎 💎",
            items: [
                { text: "2", type: "rock", x: 130, y: 280, size: 50, points: 0 },
                { text: "3", type: "amethyst", x: 240, y: 350, size: 50, points: 0 },
                { text: "4", type: "gold", x: 360, y: 320, size: 56, points: 10 },
                { text: "5", type: "gem", x: 480, y: 290, size: 50, points: 0 },
                { text: "6", type: "ruby", x: 400, y: 380, size: 50, points: 0 },
                { text: "💎", type: "gem", x: 170, y: 370, size: 42, points: 5 }
            ],
            praiseText: "Chính xác! Có đúng 4 viên kim cương lấp lánh.",
            rewardStars: 2
        },
        {
            level: 10,
            target: "5",
            targetType: "math",
            instruction: "Bé tính giúp bác thợ mỏ: 2 + 3 bằng mấy nào?",
            mode: "math-add",
            mathFormula: "2 + 3 = ?",
            items: [
                { text: "4", type: "rock", x: 150, y: 290, size: 50, points: 0 },
                { text: "5", type: "gold", x: 300, y: 350, size: 56, points: 10 },
                { text: "6", type: "amethyst", x: 440, y: 300, size: 50, points: 0 },
                { text: "7", type: "gem", x: 210, y: 370, size: 50, points: 0 },
                { text: "3", type: "ruby", x: 420, y: 380, size: 50, points: 0 },
                { text: "⭐", type: "star", x: 330, y: 260, size: 42, points: 5 }
            ],
            praiseText: "Tuyệt đỉnh! 2 cộng 3 bằng 5. Bé làm toán siêu quá!",
            rewardStars: 2
        },
        {
            level: 11,
            target: "3",
            targetType: "math",
            instruction: "Phép trừ thử thách: 5 - 2 bằng mấy nhỉ?",
            mode: "math-sub",
            mathFormula: "5 - 2 = ?",
            items: [
                { text: "2", type: "rock", x: 140, y: 320, size: 50, points: 0 },
                { text: "3", type: "gold", x: 330, y: 330, size: 56, points: 10 },
                { text: "4", type: "amethyst", x: 460, y: 270, size: 50, points: 0 },
                { text: "1", type: "gem", x: 220, y: 260, size: 50, points: 0 },
                { text: "6", type: "ruby", x: 430, y: 370, size: 50, points: 0 },
                { text: "💎", type: "gem", x: 180, y: 390, size: 42, points: 5 }
            ],
            praiseText: "Đúng rồi! 5 bớt 2 còn 3. Bé thật thông minh!",
            rewardStars: 3
        },
        {
            level: 12,
            target: "10",
            targetType: "number",
            instruction: "Màn kho báu đặc biệt! Con hãy chạm vào khối số 10 vàng rực rỡ",
            mode: "find-number",
            items: [
                { text: "7", type: "rock", x: 130, y: 290, size: 50, points: 0 },
                { text: "8", type: "amethyst", x: 240, y: 260, size: 50, points: 0 },
                { text: "9", type: "gem", x: 450, y: 280, size: 50, points: 0 },
                { text: "10", type: "gold", x: 310, y: 350, size: 62, points: 20 },
                { text: "0", type: "ruby", x: 180, y: 380, size: 50, points: 0 },
                { text: "1", type: "rock", x: 430, y: 370, size: 50, points: 0 },
                { text: "🎁", type: "chest", x: 330, y: 250, size: 46, points: 10 }
            ],
            praiseText: "Chúc mừng bé! Bé đã chinh phục trọn vẹn Mỏ Vàng Tri Thức!",
            rewardStars: 3
        }
    ],

    // 3. KHINH KHÍ CẦU GHÉP VẦN
    "balloon-word": [
        {
            level: 1,
            targetWord: "BA",
            slots: ["B", "A"],
            balloons: ["B", "A", "C", "M"],
            instruction: "Bé hãy ghép tiếng BA nhé: Bờ - A - BA",
            praiseText: "Bờ - A - BA! Bé ghép đúng rồi.",
            rewardStars: 1
        },
        {
            level: 2,
            targetWord: "BO",
            slots: ["B", "O"],
            balloons: ["B", "O", "D", "E"],
            instruction: "Bé hãy ghép tiếng BO: Bờ - O - BO",
            praiseText: "Bờ - O - BO! Bé rất thông minh.",
            rewardStars: 1
        },
        {
            level: 3,
            targetWord: "CO",
            slots: ["C", "O"],
            balloons: ["C", "O", "A", "N"],
            instruction: "Hãy ghép tiếng CO: Cờ - O - CO",
            praiseText: "Cờ - O - CO! Hoan hô bé.",
            rewardStars: 1
        },
        {
            level: 4,
            targetWord: "ME",
            slots: ["M", "E"],
            balloons: ["M", "E", "B", "I"],
            instruction: "Hãy ghép tiếng ME: Mờ - E - ME",
            praiseText: "Mờ - E - ME! Quá giỏi luôn.",
            rewardStars: 1
        },
        {
            level: 5,
            targetWord: "CÁ",
            slots: ["C", "A", "´"],
            balloons: ["C", "A", "´", "`"],
            imageEmoji: "🐟",
            imageDesc: "Con Cá",
            instruction: "Hãy ghép chữ CÁ: Cờ - A - Ca - sắc - CÁ",
            praiseText: "Con Cá bơi lội! Bé ghép đúng từ CÁ.",
            rewardStars: 2
        },
        {
            level: 6,
            targetWord: "GÀ",
            slots: ["G", "A", "`"],
            balloons: ["G", "A", "`", "´"],
            imageEmoji: "🐔",
            imageDesc: "Con Gà",
            instruction: "Hãy ghép chữ GÀ: Gờ - A - Ga - huyền - GÀ",
            praiseText: "Chú Gà trống gáy vang! Bé ghép đúng từ GÀ.",
            rewardStars: 2
        },
        {
            level: 7,
            targetWord: "BÒ",
            slots: ["B", "O", "`"],
            balloons: ["B", "O", "`", "?"],
            imageEmoji: "🐮",
            imageDesc: "Con Bò",
            instruction: "Hãy ghép chữ BÒ: Bờ - O - Bo - huyền - BÒ",
            praiseText: "Chú Bò chăm chỉ! Bé ghép rất chuẩn.",
            rewardStars: 2
        },
        {
            level: 8,
            targetWord: "HỔ",
            slots: ["H", "O", "?"],
            balloons: ["H", "O", "?", "~"],
            imageEmoji: "🐯",
            imageDesc: "Con Hổ",
            instruction: "Hãy ghép chữ HỔ: Hờ - Ô - Hô - hỏi - HỔ",
            praiseText: "Chúa sơn lâm Hổ! Bé ghép rất cừ.",
            rewardStars: 2
        },
        {
            level: 9,
            targetWord: "NƠ",
            slots: ["N", "Ơ"],
            balloons: ["N", "Ơ", "M", "A"],
            imageEmoji: "🎀",
            imageDesc: "Cái Nơ",
            instruction: "Hãy ghép chữ NƠ xinh xắn: Nờ - Ơ - NƠ",
            praiseText: "Chiếc Nơ xinh! Bé giỏi tuyệt vời.",
            rewardStars: 2
        },
        {
            level: 10,
            targetWord: "CỜ",
            slots: ["C", "Ơ", "`"],
            balloons: ["C", "Ơ", "`", "´"],
            imageEmoji: "🚩",
            imageDesc: "Lá Cờ",
            instruction: "Hãy ghép chữ CỜ: Cờ - Ơ - Cơ - huyền - CỜ",
            praiseText: "Lá Cờ đỏ thắm! Bé đã hoàn thành xuất sắc.",
            rewardStars: 3
        }
    ],

    // 4. ĐOÀN TÀU CHỞ SỐ
    "number-train": [
        {
            level: 1,
            type: "missing-number",
            trainSequence: [1, null, 3],
            choices: [2, 5, 7],
            answer: 2,
            instruction: "Bé hãy tìm toa số 2 nối vào đoàn tàu nhé",
            praiseText: "Một - Hai - Ba! Tàu chạy nào: Tu tu xình xịch!",
            rewardStars: 1
        },
        {
            level: 2,
            type: "missing-number",
            trainSequence: [2, 3, null],
            choices: [4, 1, 6],
            answer: 4,
            instruction: "Toa số mấy chạy ngay sau toa số 3 nhỉ?",
            praiseText: "Chính xác! Toa số 4 đã vào vị trí.",
            rewardStars: 1
        },
        {
            level: 3,
            type: "missing-number",
            trainSequence: [1, 2, null, 4, 5],
            choices: [3, 6, 8],
            answer: 3,
            instruction: "Số nào còn thiếu giữa số 2 và số 4?",
            praiseText: "Rất giỏi! Số 3 là số còn thiếu.",
            rewardStars: 1
        },
        {
            level: 4,
            type: "missing-number",
            trainSequence: [6, 7, 8, null, 10],
            choices: [9, 5, 4],
            answer: 9,
            instruction: "Số nào chạy ngay trước số 10?",
            praiseText: "Đúng rồi! Số 9 hoàn tất đoàn tàu.",
            rewardStars: 1
        },
        {
            level: 5,
            type: "sort-numbers",
            wagons: [3, 1, 2],
            correctOrder: [1, 2, 3],
            instruction: "Bé hãy xếp các toa tàu theo thứ tự từ nhỏ đến lớn: 1, 2, 3",
            praiseText: "Một, hai, ba! Đoàn tàu thẳng hàng đẹp quá.",
            rewardStars: 2
        },
        {
            level: 6,
            type: "sort-numbers",
            wagons: [4, 2, 1, 3],
            correctOrder: [1, 2, 3, 4],
            instruction: "Bé hãy xếp các toa tàu theo thứ tự từ 1 đến 4",
            praiseText: "Tuyệt vời! 1, 2, 3, 4 ngay ngắn rồi.",
            rewardStars: 2
        },
        {
            level: 7,
            type: "quantity-counting",
            itemsEmoji: "🍎 🍎 🍎",
            itemDesc: "3 quả táo",
            choices: [2, 3, 5],
            answer: 3,
            instruction: "Đoàn tàu chở mấy quả táo đỏ ngon lành?",
            praiseText: "Có 3 quả táo! Bé đếm chuẩn lắm.",
            rewardStars: 2
        },
        {
            level: 8,
            type: "quantity-counting",
            itemsEmoji: "⭐ ⭐ ⭐ ⭐ ⭐",
            itemDesc: "5 ngôi sao",
            choices: [4, 5, 6],
            answer: 5,
            instruction: "Bé đếm xem toa tàu chở mấy ngôi sao vàng?",
            praiseText: "Có 5 ngôi sao lấp lánh! Bé làm đúng rồi.",
            rewardStars: 2
        },
        {
            level: 9,
            type: "quantity-counting",
            itemsEmoji: "🐥 🐥 🐥 🐥",
            itemDesc: "4 chú gà con",
            choices: [3, 4, 7],
            answer: 4,
            instruction: "Có mấy chú gà con lon ton trên tàu?",
            praiseText: "Bốn chú gà con xinh xắn! Rất xuất sắc.",
            rewardStars: 2
        },
        {
            level: 10,
            type: "number-neighbor",
            trainSequence: [null, 7, 8],
            choices: [6, 5, 9],
            answer: 6,
            instruction: "Số nào đứng liền trước số 7 nhỉ bé yêu?",
            praiseText: "Số 6 đứng trước số 7! Bé tốt nghiệp đoàn tàu số rồi.",
            rewardStars: 3
        }
    ]
};
