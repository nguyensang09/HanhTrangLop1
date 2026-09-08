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

    // 2. MỎ VÀNG TRI THỨC - ĐÀO VÀNG CHỮ & SỐ (CHUẨN BẢNG CHỮ CÁI TIẾNG VIỆT 29 CHỮ + 10 SỐ)
    "gold-miner": [
        // --- 29 CHỮ CÁI TIẾNG VIỆT ---
        { level: 1, target: "A", targetType: "letter", instruction: "Con hãy tìm và gắp các khối chữ A màu vàng nhé", praiseText: "Đúng rồi! Đây là chữ A. Bé giỏi quá!", rewardStars: 1 },
        { level: 2, target: "Ă", targetType: "letter", instruction: "Con hãy tìm chữ Ă có chiếc mũ ngược xinh xắn", praiseText: "Tuyệt vời! Chữ Ă có chiếc mũ trăng khuyết.", rewardStars: 1 },
        { level: 3, target: "Â", targetType: "letter", instruction: "Con hãy tìm chữ Â có chiếc nón úp xinh xắn", praiseText: "Chính xác! Chữ Â đội chiếc nón lá che mưa nắng.", rewardStars: 1 },
        { level: 4, target: "B", targetType: "letter", instruction: "Con hãy gắp các khối chữ Bờ (B) màu vàng nhé", praiseText: "Hoan hô! Bờ - Quả Bóng tròn xinh.", rewardStars: 1 },
        { level: 5, target: "C", targetType: "letter", instruction: "Con hãy tìm khối chữ Cờ (C) cong tròn như vầng trăng khuyết", praiseText: "Tuyệt vời! Cờ - Con Cá đang bơi lội.", rewardStars: 1 },
        { level: 6, target: "D", targetType: "letter", instruction: "Con hãy tìm khối chữ Dờ (D) màu vàng óng", praiseText: "Đúng rồi! Dờ - Quả Dưa hấu ngọt lành.", rewardStars: 1 },
        { level: 7, target: "Đ", targetType: "letter", instruction: "Con hãy tìm chữ Đờ (Đ) có chiếc gạch ngang trên đầu", praiseText: "Chính xác! Đờ - Chiếc Đèn lồng đỏ tươi.", rewardStars: 1 },
        { level: 8, target: "E", targetType: "letter", instruction: "Con hãy gắp khối chữ E màu vàng nhé", praiseText: "Bé giỏi quá! E - Bé ngoan đến trường.", rewardStars: 1 },
        { level: 9, target: "Ê", targetType: "letter", instruction: "Con hãy tìm chữ Ê đội chiếc mũ nhỏ xinh", praiseText: "Tuyệt vời! Ê - Quả Khế thơm lừng.", rewardStars: 1 },
        { level: 10, target: "G", targetType: "letter", instruction: "Con hãy tìm khối chữ Gờ (G) màu vàng", praiseText: "Đúng rồi! Gờ - Chú Gà Trống gáy vang ó o.", rewardStars: 1 },
        { level: 11, target: "H", targetType: "letter", instruction: "Con hãy tìm chữ Hờ (H) có nét thẳng cao", praiseText: "Chính xác! Hờ - Bông Hoa tươi thắm.", rewardStars: 1 },
        { level: 12, target: "I", targetType: "letter", instruction: "Con hãy tìm khối chữ I có dấu chấm nhỏ", praiseText: "Bé tinh mắt quá! I - Viên Kẹo ngọt ngào.", rewardStars: 1 },
        { level: 13, target: "K", targetType: "letter", instruction: "Con hãy tìm chữ Ca (K) màu vàng rực rỡ", praiseText: "Tuyệt đỉnh! Ca - Chiếc Kem mát lạnh.", rewardStars: 1 },
        { level: 14, target: "L", targetType: "letter", instruction: "Con hãy gắp khối chữ Lờ (L) nét thẳng", praiseText: "Đúng rồi! Lờ - Lá cây xanh mướt.", rewardStars: 1 },
        { level: 15, target: "M", targetType: "letter", instruction: "Con hãy tìm chữ Mờ (M) có hai chiếc cầu cong", praiseText: "Hoan hô! Mờ - Mẹ yêu của con.", rewardStars: 1 },
        { level: 16, target: "N", targetType: "letter", instruction: "Con hãy tìm khối chữ Nờ (N) màu vàng", praiseText: "Tuyệt vời! Nờ - Nụ hoa chớm nở.", rewardStars: 1 },
        { level: 17, target: "O", targetType: "letter", instruction: "Con hãy tìm chữ O tròn như quả trứng gà", praiseText: "Chính xác! O tròn như quả trứng gà.", rewardStars: 1 },
        { level: 18, target: "Ô", targetType: "letter", instruction: "Con hãy gắp chữ Ô đội chiếc nón lá", praiseText: "Đúng rồi! Ô đội nón che mưa che nắng.", rewardStars: 1 },
        { level: 19, target: "Ơ", targetType: "letter", instruction: "Con hãy tìm chữ Ơ có chiếc râu nhỏ xinh", praiseText: "Tuyệt vời! Ơ thêm chiếc râu nhỏ xíu.", rewardStars: 1 },
        { level: 20, target: "P", targetType: "letter", instruction: "Con hãy tìm khối chữ Pờ (P) màu vàng", praiseText: "Đúng rồi! Pờ - Chiếc Phao cứu sinh.", rewardStars: 1 },
        { level: 21, target: "Q", targetType: "letter", instruction: "Con hãy gắp chữ Quy (Q) tròn xinh có nét móc", praiseText: "Chính xác! Quy - Quả Cam mọng nước.", rewardStars: 1 },
        { level: 22, target: "R", targetType: "letter", instruction: "Con hãy tìm chữ Rờ (R) màu vàng lấp lánh", praiseText: "Bé giỏi quá! Rờ - Con Rùa chăm chỉ.", rewardStars: 1 },
        { level: 23, target: "S", targetType: "letter", instruction: "Con hãy tìm chữ Sờ (S) uốn lượn như dòng suối", praiseText: "Tuyệt vời! Sờ - Ngôi Sao lấp lánh.", rewardStars: 1 },
        { level: 24, target: "T", targetType: "letter", instruction: "Con hãy gắp khối chữ Tờ (T) màu vàng", praiseText: "Đúng rồi! Tờ - Chú Thỏ trắng tinh nhanh.", rewardStars: 1 },
        { level: 25, target: "U", targetType: "letter", instruction: "Con hãy tìm khối chữ U như chiếc võng nhỏ", praiseText: "Chính xác! U - Chiếc Túi xách xinh xinh.", rewardStars: 1 },
        { level: 26, target: "Ư", targetType: "letter", instruction: "Con hãy tìm chữ Ư có thêm chiếc móc câu", praiseText: "Tuyệt đỉnh! Ư - Dòng Nước mát trong veo.", rewardStars: 1 },
        { level: 27, target: "V", targetType: "letter", instruction: "Con hãy tìm chữ Vờ (V) nét vát nhọn xinh xắn", praiseText: "Hoan hô! Vờ - Con Vịt bơi tung tăng.", rewardStars: 1 },
        { level: 28, target: "X", targetType: "letter", instruction: "Con hãy gắp chữ Xờ (X) chéo nhau như cánh quạt", praiseText: "Bé siêu quá! Xờ - Chiếc Xe đạp bon bon.", rewardStars: 1 },
        { level: 29, target: "Y", targetType: "letter", instruction: "Con hãy tìm chữ Y dài màu vàng óng ánh", praiseText: "Tuyệt vời! Y - Chim Yến bay liệng trên bầu trời.", rewardStars: 1 },
        
        // --- 10 CHỮ SỐ (0 ĐẾN 9) ---
        { level: 30, target: "0", targetType: "number", instruction: "Bé hãy tìm và gắp số 0 tròn trĩnh nhé", praiseText: "Đúng số 0 rồi! Bé thật thông minh.", rewardStars: 1 },
        { level: 31, target: "1", targetType: "number", instruction: "Con hãy tìm khối số 1 thẳng đứng như cây nến", praiseText: "Chính xác! Số 1 như một cây nến sáng rực.", rewardStars: 1 },
        { level: 32, target: "2", targetType: "number", instruction: "Con hãy tìm số 2 cong cong như chú vịt nhỏ", praiseText: "Tuyệt vời! Số 2 uốn cong như chú vịt bơi.", rewardStars: 1 },
        { level: 33, target: "3", targetType: "number", instruction: "Con hãy chạm vào khối số 3 may mắn", praiseText: "Đúng số 3 rồi! Bé đếm giỏi lắm.", rewardStars: 1 },
        { level: 34, target: "4", targetType: "number", instruction: "Con hãy tìm khối số 4 như chiếc ghế gập", praiseText: "Hoan hô! Số 4 vàng óng đã được gắp lên.", rewardStars: 1 },
        { level: 35, target: "5", targetType: "number", instruction: "Con hãy tìm khối số 5 có chiếc bụng tròn nhé", praiseText: "Bé giỏi quá! Số 5 béo tròn đáng yêu.", rewardStars: 1 },
        { level: 36, target: "6", targetType: "number", instruction: "Con hãy gắp số 6 có nét móc xoắn phía dưới", praiseText: "Đúng số 6 rồi! Bé tinh mắt lắm.", rewardStars: 1 },
        { level: 37, target: "7", targetType: "number", instruction: "Con hãy tìm khối số 7 như chiếc cuốc nhỏ của bác thợ", praiseText: "Tuyệt vời! Số 7 của bác thợ mỏ chăm chỉ.", rewardStars: 1 },
        { level: 38, target: "8", targetType: "number", instruction: "Cẩn thận nhầm chữ B với số 8 nhé! Hãy gắp số 8", praiseText: "Mắt bé tinh quá! Đây chính là số 8 tròn xinh.", rewardStars: 1 },
        { level: 39, target: "9", targetType: "number", instruction: "Con hãy tìm số 9 có chiếc đuôi uốn cong xinh xắn", praiseText: "Chúc mừng bé! Bé đã xuất sắc hoàn thành toàn bộ bảng chữ và số!", rewardStars: 2 }
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
