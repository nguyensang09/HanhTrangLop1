/**
 * GAME_LEVELS_CATALOG
 * Dữ liệu màn chơi chuẩn hóa toàn diện cho 4 trò chơi:
 * - 🎈 Bubble Game (50 màn): Toàn bộ 29 chữ cái Tiếng Việt (A-Y) + Toàn bộ số đếm 0 đến 20.
 * - 🚂 Number Train (40 màn): Nối toa 0-10, xếp toa 1-10, đếm hoa quả con vật 1-10 món, toa số lớn 11-20, siêu tốc.
 * - ☁️ Balloon Word (40 màn): 37 màn ghép âm đầu & nguyên âm bao phủ 29 chữ cái Tiếng Việt + 3 màn từ Phonics sinh động.
 * - ⛏️ Gold Miner (39 màn): Toàn bộ 29 chữ cái Tiếng Việt + 10 chữ số (0-9).
 *
 * Tuân thủ nguyên tắc: KHÔNG điểm số, KHÔNG coin, KHÔNG ngôi sao xếp hạng phức tạp.
 * Tự động chuyển cảnh êm ái sau 1.8s.
 */
window.GAME_LEVELS = {
    // =========================================================================
    // 1. 🎈 BẮN BONG BÓNG CHỮ CÁI & CHỮ SỐ (BUBBLE POP - 50 MÀN CHƠI)
    // =========================================================================
    "bubble": [
        // --- 29 CHỮ CÁI TIẾNG VIỆT (MÀN 1 ĐẾN 29) ---
        { level: 1, target: "A", choices: ["A", "Ă", "B"], targetCount: 2, instruction: "Bé ngắm bắn 2 bong bóng chữ A nhé!", instructionEn: "Pop letter A!", hintWord: "APPLE", hintEmoji: "🍎" },
        { level: 2, target: "Ă", choices: ["Ă", "A", "Â"], targetCount: 2, instruction: "Bé ngắm bắn 2 bong bóng chữ Ă (có mũ ngược)!", instructionEn: "Pop letter Ă!", hintWord: "KHĂN", hintEmoji: "🧣" },
        { level: 3, target: "Â", choices: ["Â", "A", "Ă"], targetCount: 2, instruction: "Bé ngắm bắn bong bóng chữ Â (đội nón úp)!", instructionEn: "Pop letter Â!", hintWord: "NÓN", hintEmoji: "👒" },
        { level: 4, target: "B", choices: ["B", "D", "Đ"], targetCount: 2, instruction: "Bé ngắm bắn 2 bong bóng chữ B!", instructionEn: "Pop letter B!", hintWord: "BÒ", hintEmoji: "🐮" },
        { level: 5, target: "C", choices: ["C", "O", "G"], targetCount: 2, instruction: "Bé ngắm bắn bong bóng chữ C cong tròn!", instructionEn: "Pop letter C!", hintWord: "CÁ", hintEmoji: "🐟" },
        { level: 6, target: "D", choices: ["D", "Đ", "B"], targetCount: 2, instruction: "Bé ngắm bắn bong bóng chữ D!", instructionEn: "Pop letter D!", hintWord: "DÊ", hintEmoji: "🐐" },
        { level: 7, target: "Đ", choices: ["Đ", "D", "B"], targetCount: 2, instruction: "Bé ngắm bắn chữ Đ có chiếc gạch ngang!", instructionEn: "Pop letter Đ!", hintWord: "ĐÈN", hintEmoji: "🏮" },
        { level: 8, target: "E", choices: ["E", "Ê", "C"], targetCount: 2, instruction: "Bé hãy ngắm bắn các bong bóng chữ E!", instructionEn: "Pop letter E!", hintWord: "EM BÉ", hintEmoji: "👶" },
        { level: 9, target: "Ê", choices: ["Ê", "E", "C"], targetCount: 2, instruction: "Bé ngắm bắn chữ Ê có chiếc nón nhỏ!", instructionEn: "Pop letter Ê!", hintWord: "BÚP BÊ", hintEmoji: "🪆" },
        { level: 10, target: "G", choices: ["G", "C", "Q"], targetCount: 2, instruction: "Bé hãy ngắm bắn bong bóng chữ G!", instructionEn: "Pop letter G!", hintWord: "GÀ", hintEmoji: "🐔" },
        { level: 11, target: "H", choices: ["H", "K", "L"], targetCount: 2, instruction: "Bé hãy bắn bong bóng chữ H nét cao!", instructionEn: "Pop letter H!", hintWord: "HOA", hintEmoji: "🌸" },
        { level: 12, target: "I", choices: ["I", "T", "L"], targetCount: 2, instruction: "Bé hãy bắn bong bóng chữ I ngắn!", instructionEn: "Pop letter I!", hintWord: "HÒN BI", hintEmoji: "🔮" },
        { level: 13, target: "K", choices: ["K", "H", "X"], targetCount: 2, instruction: "Bé hãy bắn bong bóng chữ K!", instructionEn: "Pop letter K!", hintWord: "KÍNH", hintEmoji: "👓" },
        { level: 14, target: "L", choices: ["L", "I", "T"], targetCount: 2, instruction: "Bé hãy bắn các bong bóng chữ L!", instructionEn: "Pop letter L!", hintWord: "LÁ CÂY", hintEmoji: "🍃" },
        { level: 15, target: "M", choices: ["M", "N", "W"], targetCount: 2, instruction: "Bé hãy bắn bong bóng chữ M hai nhịp cong!", instructionEn: "Pop letter M!", hintWord: "MÈO", hintEmoji: "🐱" },
        { level: 16, target: "N", choices: ["N", "M", "U"], targetCount: 2, instruction: "Bé hãy bắn bong bóng chữ N một nhịp cong!", instructionEn: "Pop letter N!", hintWord: "QUẢ NHO", hintEmoji: "🍇" },
        { level: 17, target: "O", choices: ["O", "Ô", "Ơ"], targetCount: 2, instruction: "Bé hãy bắn chữ O tròn như quả trứng gà!", instructionEn: "Pop letter O!", hintWord: "ONG", hintEmoji: "🐝" },
        { level: 18, target: "Ô", choices: ["Ô", "O", "Ơ"], targetCount: 2, instruction: "Bé hãy bắn chữ Ô đội chiếc nón lá!", instructionEn: "Pop letter Ô!", hintWord: "CÁI Ô", hintEmoji: "☂️" },
        { level: 19, target: "Ơ", choices: ["Ơ", "O", "Ô"], targetCount: 2, instruction: "Bé hãy bắn chữ Ơ có chiếc râu nhỏ xinh!", instructionEn: "Pop letter Ơ!", hintWord: "LÁ CỜ", hintEmoji: "🚩" },
        { level: 20, target: "P", choices: ["P", "B", "Q"], targetCount: 2, instruction: "Bé hãy ngắm bắn bong bóng chữ P!", instructionEn: "Pop letter P!", hintWord: "ĐÈN PIN", hintEmoji: "🔦" },
        { level: 21, target: "Q", choices: ["Q", "P", "O"], targetCount: 2, instruction: "Bé hãy ngắm bắn bong bóng chữ Q!", instructionEn: "Pop letter Q!", hintWord: "QUẢ QUÝT", hintEmoji: "🍊" },
        { level: 22, target: "R", choices: ["R", "S", "B"], targetCount: 2, instruction: "Bé hãy bắn các bong bóng chữ R!", instructionEn: "Pop letter R!", hintWord: "CON RÙA", hintEmoji: "🐢" },
        { level: 23, target: "S", choices: ["S", "X", "C"], targetCount: 2, instruction: "Bé hãy ngắm bắn chữ S uốn lượn như dòng suối!", instructionEn: "Pop letter S!", hintWord: "NGÔI SAO", hintEmoji: "⭐" },
        { level: 24, target: "T", choices: ["T", "L", "I"], targetCount: 2, instruction: "Bé hãy ngắm bắn bong bóng chữ T!", instructionEn: "Pop letter T!", hintWord: "TÀU HỎA", hintEmoji: "🚂" },
        { level: 25, target: "U", choices: ["U", "Ư", "V"], targetCount: 2, instruction: "Bé hãy ngắm bắn bong bóng chữ U!", instructionEn: "Pop letter U!", hintWord: "CON CÚ", hintEmoji: "🦉" },
        { level: 26, target: "Ư", choices: ["Ư", "U", "O"], targetCount: 2, instruction: "Bé hãy ngắm bắn chữ Ư có chiếc móc câu xinh!", instructionEn: "Pop letter Ư!", hintWord: "HƯƠU CAO CỔ", hintEmoji: "🦒" },
        { level: 27, target: "V", choices: ["V", "U", "Y"], targetCount: 2, instruction: "Bé hãy ngắm bắn bong bóng chữ V!", instructionEn: "Pop letter V!", hintWord: "CON VOI", hintEmoji: "🐘" },
        { level: 28, target: "X", choices: ["X", "S", "K"], targetCount: 2, instruction: "Bé hãy ngắm bắn bong bóng chữ X chéo nhau!", instructionEn: "Pop letter X!", hintWord: "XE HƠI", hintEmoji: "🚗" },
        { level: 29, target: "Y", choices: ["Y", "V", "U"], targetCount: 2, instruction: "Bé hãy ngắm bắn bong bóng chữ Y dài!", instructionEn: "Pop letter Y!", hintWord: "YÊU THƯƠNG", hintEmoji: "💖" },

        // --- 21 CHỮ SỐ TỪ 0 ĐẾN 20 (MÀN 30 ĐẾN 50) ---
        { level: 30, target: "0", choices: ["0", "8", "9"], targetCount: 2, instruction: "Khám phá chữ số: Bắn bong bóng số 0 tròn xoe!", instructionEn: "Pop number 0!", hintWord: "SỐ 0", hintEmoji: "⭕" },
        { level: 31, target: "1", choices: ["1", "7", "4"], targetCount: 2, instruction: "Bé hãy bắn số 1 thẳng đứng như cây nến!", instructionEn: "Pop number 1!", hintWord: "SỐ 1", hintEmoji: "🕯️" },
        { level: 32, target: "2", choices: ["2", "5", "3"], targetCount: 2, instruction: "Bé hãy ngắm bắn số 2 bơi cong như chú vịt nhỏ!", instructionEn: "Pop number 2!", hintWord: "SỐ 2", hintEmoji: "🦆" },
        { level: 33, target: "3", choices: ["3", "8", "5"], targetCount: 2, instruction: "Bé hãy ngắm bắn bong bóng số 3 đôi tai thỏ!", instructionEn: "Pop number 3!", hintWord: "SỐ 3", hintEmoji: "🐰" },
        { level: 34, target: "4", choices: ["4", "1", "7"], targetCount: 2, instruction: "Bé hãy ngắm bắn bong bóng số 4 như chiếc ghế!", instructionEn: "Pop number 4!", hintWord: "SỐ 4", hintEmoji: "🪑" },
        { level: 35, target: "5", choices: ["5", "2", "6"], targetCount: 2, instruction: "Bé hãy bắn bong bóng số 5 có chiếc bụng tròn!", instructionEn: "Pop number 5!", hintWord: "SỐ 5", hintEmoji: "🖐️" },
        { level: 36, target: "6", choices: ["6", "9", "5"], targetCount: 2, instruction: "Bé hãy ngắm bắn bong bóng số 6 nét cuộn tròn!", instructionEn: "Pop number 6!", hintWord: "SỐ 6", hintEmoji: "🐚" },
        { level: 37, target: "7", choices: ["7", "1", "4"], targetCount: 2, instruction: "Bé hãy ngắm bắn số 7 như chiếc cuốc nhỏ xinh!", instructionEn: "Pop number 7!", hintWord: "SỐ 7", hintEmoji: "⛏️" },
        { level: 38, target: "8", choices: ["8", "3", "0"], targetCount: 2, instruction: "Bé hãy ngắm bắn số 8 tròn xinh như người tuyết!", instructionEn: "Pop number 8!", hintWord: "SỐ 8", hintEmoji: "⛄" },
        { level: 39, target: "9", choices: ["9", "6", "8"], targetCount: 2, instruction: "Bé hãy bắn bong bóng mang số 9!", instructionEn: "Pop number 9!", hintWord: "SỐ 9", hintEmoji: "🎈" },
        { level: 40, target: "10", choices: ["10", "1", "0"], targetCount: 2, instruction: "Bé hãy bắn bong bóng số 10 tròn trĩnh đạt điểm mười!", instructionEn: "Pop number 10!", hintWord: "SỐ 10", hintEmoji: "💯" },
        { level: 41, target: "11", choices: ["11", "1", "12"], targetCount: 2, instruction: "Bé hãy bắn số 11 gồm hai chữ số 1 đứng cạnh nhau!", instructionEn: "Pop number 11!", hintWord: "SỐ 11", hintEmoji: "🕯️" },
        { level: 42, target: "12", choices: ["12", "21", "10"], targetCount: 2, instruction: "Bé hãy ngắm bắn bong bóng số 12!", instructionEn: "Pop number 12!", hintWord: "SỐ 12", hintEmoji: "🍎" },
        { level: 43, target: "13", choices: ["13", "31", "15"], targetCount: 2, instruction: "Bé hãy ngắm bắn bong bóng số 13 may mắn!", instructionEn: "Pop number 13!", hintWord: "SỐ 13", hintEmoji: "🐱" },
        { level: 44, target: "14", choices: ["14", "41", "11"], targetCount: 2, instruction: "Bé hãy ngắm bắn bong bóng số 14!", instructionEn: "Pop number 14!", hintWord: "SỐ 14", hintEmoji: "🍓" },
        { level: 45, target: "15", choices: ["15", "51", "12"], targetCount: 2, instruction: "Bé hãy bắn các bong bóng mang số 15!", instructionEn: "Pop number 15!", hintWord: "SỐ 15", hintEmoji: "🍇" },
        { level: 46, target: "16", choices: ["16", "19", "10"], targetCount: 2, instruction: "Bé hãy ngắm bắn bong bóng số 16!", instructionEn: "Pop number 16!", hintWord: "SỐ 16", hintEmoji: "⭐" },
        { level: 47, target: "17", choices: ["17", "71", "14"], targetCount: 2, instruction: "Bé hãy bắn bong bóng mang số 17!", instructionEn: "Pop number 17!", hintWord: "SỐ 17", hintEmoji: "🐟" },
        { level: 48, target: "18", choices: ["18", "81", "13"], targetCount: 2, instruction: "Bé hãy ngắm bắn bong bóng số 18!", instructionEn: "Pop number 18!", hintWord: "SỐ 18", hintEmoji: "🚗" },
        { level: 49, target: "19", choices: ["19", "16", "91"], targetCount: 2, instruction: "Bé hãy bắn bong bóng mang số 19!", instructionEn: "Pop number 19!", hintWord: "SỐ 19", hintEmoji: "⚽" },
        { level: 50, target: "20", choices: ["20", "12", "10"], targetCount: 3, hasRainbow: true, instruction: "Màn 50 khải hoàn: Bắn nổ bong bóng số 20 rực rỡ!", instructionEn: "Pop number 20!", hintWord: "CHIẾN THẮNG", hintEmoji: "🏆" }
    ],

    // =========================================================================
    // 2. 🚂 ĐOÀN TÀU CHỞ SỐ (NUMBER TRAIN - 40 MÀN CHƠI TỪ 0 ĐẾN 20)
    // =========================================================================
    "number-train": [
        // --- PHASE 1: DÃY SỐ CƠ BẢN 0 ĐẾN 10 (MÀN 1 ĐẾN 11) ---
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
            trainSequence: [null, 2, 3, 4],
            choices: [1, 5, 6],
            answer: 1,
            instruction: "Toa đầu tiên còn thiếu trước số 2 là số mấy?",
            instructionEn: "Find wagon 1!"
        },
        {
            level: 5,
            type: "missing-wagon",
            trainSequence: [2, null, 4, 5],
            choices: [3, 1, 6],
            answer: 3,
            instruction: "Tìm toa số 3 nằm giữa số 2 và 4!",
            instructionEn: "Find wagon 3!"
        },
        {
            level: 6,
            type: "missing-wagon",
            trainSequence: [null, 5, 6],
            choices: [4, 3, 7],
            answer: 4,
            instruction: "Toa số mấy chạy ngay trước toa số 5?",
            instructionEn: "Find wagon 4!"
        },
        {
            level: 7,
            type: "missing-wagon",
            trainSequence: [4, null, 6, 7],
            choices: [5, 2, 8],
            answer: 5,
            instruction: "Điền toa số 5 vào vị trí còn thiếu!",
            instructionEn: "Find wagon 5!"
        },
        {
            level: 8,
            type: "missing-wagon",
            trainSequence: [5, 6, null, 8],
            choices: [7, 9, 4],
            answer: 7,
            instruction: "Toa số mấy đứng giữa số 6 và 8?",
            instructionEn: "Find wagon 7!"
        },
        {
            level: 9,
            type: "missing-wagon",
            trainSequence: [6, 7, 8, null],
            choices: [9, 5, 10],
            answer: 9,
            instruction: "Toa nào chạy tiếp theo sau toa số 8?",
            instructionEn: "Find wagon 9!"
        },
        {
            level: 10,
            type: "missing-wagon",
            trainSequence: [7, 8, null, 10],
            choices: [9, 6, 5],
            answer: 9,
            instruction: "Toa số mấy đứng ngay trước số 10 để tàu về ga?",
            instructionEn: "Find wagon 9!"
        },
        {
            level: 11,
            type: "missing-wagon",
            trainSequence: [null, 1, 2, 3],
            choices: [0, 4, 5],
            answer: 0,
            instruction: "Toa số 0 khởi đầu đoàn tàu bắt đầu lăn bánh!",
            instructionEn: "Find wagon 0!"
        },

        // --- PHASE 2: SẮP XẾP THỨ TỰ TOA 1 ĐẾN 10 (MÀN 12 ĐẾN 17) ---
        {
            level: 12,
            type: "wagon-sort",
            wagons: [3, 1, 2],
            correctOrder: [1, 2, 3],
            instruction: "Bé chạm lần lượt các toa theo thứ tự: 1, 2, 3!",
            instructionEn: "Sort wagons: 1, 2, 3!"
        },
        {
            level: 13,
            type: "wagon-sort",
            wagons: [4, 2, 1, 3],
            correctOrder: [1, 2, 3, 4],
            instruction: "Bé nối các toa theo thứ tự tăng dần từ 1 đến 4!",
            instructionEn: "Sort wagons: 1, 2, 3, 4!"
        },
        {
            level: 14,
            type: "wagon-sort",
            wagons: [2, 5, 4, 3],
            correctOrder: [2, 3, 4, 5],
            instruction: "Nối tiếp dãy số: 2, 3, 4, 5!",
            instructionEn: "Sort wagons: 2, 3, 4, 5!"
        },
        {
            level: 15,
            type: "wagon-sort",
            wagons: [5, 3, 4, 6],
            correctOrder: [3, 4, 5, 6],
            instruction: "Sắp xếp dãy số tăng dần: 3, 4, 5, 6!",
            instructionEn: "Sort wagons: 3, 4, 5, 6!"
        },
        {
            level: 16,
            type: "wagon-sort",
            wagons: [7, 5, 6, 8],
            correctOrder: [5, 6, 7, 8],
            instruction: "Sắp xếp dãy số tăng dần: 5, 6, 7, 8!",
            instructionEn: "Sort wagons: 5, 6, 7, 8!"
        },
        {
            level: 17,
            type: "wagon-sort",
            wagons: [8, 10, 7, 9],
            correctOrder: [7, 8, 9, 10],
            instruction: "Hoàn thành dãy số lớn: 7, 8, 9, 10!",
            instructionEn: "Sort wagons: 7, 8, 9, 10!"
        },

        // --- PHASE 3: CHẤT HÀNG ĐẾM SỐ LƯỢNG 1 ĐẾN 10 (MÀN 18 ĐẾN 27) ---
        {
            level: 18,
            type: "cargo-count",
            cargoEmoji: "🧸",
            cargoName: "Gấu Bông",
            countRequired: 1,
            instruction: "Bé hãy chất 1 chú gấu bông xinh xắn lên toa tàu!",
            instructionEn: "Load 1 teddy bear!"
        },
        {
            level: 19,
            type: "cargo-count",
            cargoEmoji: "🚗",
            cargoName: "Xe Đồ Chơi",
            countRequired: 2,
            instruction: "Bác tài nhờ bé chất 2 chiếc xe hơi đồ chơi!",
            instructionEn: "Load 2 toy cars!"
        },
        {
            level: 20,
            type: "cargo-count",
            cargoEmoji: "🍎",
            cargoName: "Táo",
            countRequired: 3,
            instruction: "Bác lái tàu nhờ bé chất 3 quả táo đỏ vào toa xe!",
            instructionEn: "Load 3 apples onto the train!"
        },
        {
            level: 21,
            type: "cargo-count",
            cargoEmoji: "🐥",
            cargoName: "Gà Con",
            countRequired: 4,
            instruction: "Bé hãy đưa 4 chú gà con lên toa tàu nhé!",
            instructionEn: "Load 4 chicks onto the train!"
        },
        {
            level: 22,
            type: "cargo-count",
            cargoEmoji: "⭐",
            cargoName: "Ngôi Sao",
            countRequired: 5,
            instruction: "Bé hãy thu thập 5 ngôi sao lấp lánh vào toa tàu!",
            instructionEn: "Load 5 stars onto the train!"
        },
        {
            level: 23,
            type: "cargo-count",
            cargoEmoji: "🍓",
            cargoName: "Dâu Tây",
            countRequired: 6,
            instruction: "Bác tài nhờ bé chất 6 quả dâu tây ngọt ngào!",
            instructionEn: "Load 6 strawberries onto the train!"
        },
        {
            level: 24,
            type: "cargo-count",
            cargoEmoji: "🎈",
            cargoName: "Bong Bóng",
            countRequired: 7,
            instruction: "Đưa 7 chùm bóng bay sắc màu lên đoàn tàu!",
            instructionEn: "Load 7 balloons onto the train!"
        },
        {
            level: 25,
            type: "cargo-count",
            cargoEmoji: "🎁",
            cargoName: "Hộp Quà",
            countRequired: 8,
            instruction: "Bé hãy chất đủ 8 hộp quà sinh nhật nhé!",
            instructionEn: "Load 8 gifts onto the train!"
        },
        {
            level: 26,
            type: "cargo-count",
            cargoEmoji: "🐟",
            cargoName: "Cá Vàng",
            countRequired: 9,
            instruction: "Bé hãy đưa 9 chú cá vàng bơi lội lên toa nước!",
            instructionEn: "Load 9 goldfish onto the train!"
        },
        {
            level: 27,
            type: "cargo-count",
            cargoEmoji: "💎",
            cargoName: "Kim Cương",
            countRequired: 10,
            instruction: "Thu thập trọn vẹn 10 viên ngọc quý rực rỡ!",
            instructionEn: "Load 10 gems onto the train!"
        },

        // --- PHASE 4: KHÁM PHÁ SỐ LỚN 11 ĐẾN 20 (MÀN 28 ĐẾN 37) ---
        {
            level: 28,
            type: "missing-wagon",
            trainSequence: [10, 11, null, 13],
            choices: [12, 14, 9],
            answer: 12,
            instruction: "Tìm toa số 12 còn thiếu nối vào đoàn tàu!",
            instructionEn: "Find wagon 12!"
        },
        {
            level: 29,
            type: "missing-wagon",
            trainSequence: [11, null, 13, 14],
            choices: [12, 10, 15],
            answer: 12,
            instruction: "Toa số mấy nằm giữa toa 11 và 13?",
            instructionEn: "Find wagon 12!"
        },
        {
            level: 30,
            type: "missing-wagon",
            trainSequence: [12, 13, null, 15],
            choices: [14, 11, 16],
            answer: 14,
            instruction: "Toa số 14 đứng giữa số 13 và 15!",
            instructionEn: "Find wagon 14!"
        },
        {
            level: 31,
            type: "missing-wagon",
            trainSequence: [null, 14, 15, 16],
            choices: [13, 12, 17],
            answer: 13,
            instruction: "Toa số mấy chạy ngay trước toa số 14?",
            instructionEn: "Find wagon 13!"
        },
        {
            level: 32,
            type: "missing-wagon",
            trainSequence: [14, 15, null, 17],
            choices: [16, 18, 13],
            answer: 16,
            instruction: "Tìm toa số 16 nằm giữa 15 và 17!",
            instructionEn: "Find wagon 16!"
        },
        {
            level: 33,
            type: "missing-wagon",
            trainSequence: [15, null, 17, 18],
            choices: [16, 14, 19],
            answer: 16,
            instruction: "Toa số mấy nằm giữa 15 và 17?",
            instructionEn: "Find wagon 16!"
        },
        {
            level: 34,
            type: "missing-wagon",
            trainSequence: [16, 17, null, 19],
            choices: [18, 15, 20],
            answer: 18,
            instruction: "Tìm toa số 18 đứng sau số 17!",
            instructionEn: "Find wagon 18!"
        },
        {
            level: 35,
            type: "missing-wagon",
            trainSequence: [null, 17, 18, 19],
            choices: [16, 15, 20],
            answer: 16,
            instruction: "Toa số mấy chạy ngay trước toa 17?",
            instructionEn: "Find wagon 16!"
        },
        {
            level: 36,
            type: "missing-wagon",
            trainSequence: [17, 18, null, 20],
            choices: [19, 16, 15],
            answer: 19,
            instruction: "Số 19 đứng ngay trước số 20 để về đích!",
            instructionEn: "Find wagon 19!"
        },
        {
            level: 37,
            type: "missing-wagon",
            trainSequence: [18, 19, null],
            choices: [20, 17, 16],
            answer: 20,
            instruction: "Toa số 20 đưa đoàn tàu khải hoàn về ga lớn!",
            instructionEn: "Find wagon 20!"
        },

        // --- PHASE 5: THỬ THÁCH ĐOÀN TÀU SIÊU TỐC SỐ LỚN (MÀN 38 ĐẾN 40) ---
        {
            level: 38,
            type: "wagon-sort",
            wagons: [13, 11, 12],
            correctOrder: [11, 12, 13],
            instruction: "Sắp xếp dãy số lớn: 11, 12, 13!",
            instructionEn: "Sort wagons: 11, 12, 13!"
        },
        {
            level: 39,
            type: "wagon-sort",
            wagons: [16, 14, 15],
            correctOrder: [14, 15, 16],
            instruction: "Sắp xếp dãy số lớn: 14, 15, 16!",
            instructionEn: "Sort wagons: 14, 15, 16!"
        },
        {
            level: 40,
            type: "wagon-sort",
            wagons: [19, 17, 18, 20],
            correctOrder: [17, 18, 19, 20],
            instruction: "Về đích khải hoàn: Sắp xếp 17, 18, 19, 20!",
            instructionEn: "Sort wagons: 17, 18, 19, 20!"
        }
    ],

    // =========================================================================
    // 3. ☁️ KHINH KHÍ CẦU GHÉP VẦN (BALLOON WORD PHONICS - 40 MÀN CHƠI)
    // =========================================================================
    "balloon-word": [
        // --- PHASE 1: 37 MÀN GHÉP TIẾNG QUEN THUỘC (BAO PHỦ 29 CHỮ CÁI TIẾNG VIỆT) ---
        {
            level: 1,
            targetWord: "BA",
            letters: ["B", "A"],
            decoys: ["C", "M"],
            emoji: "👶",
            wordMeaning: "Em Bé / Ba",
            instruction: "Bé chạm mây chữ B và A để nạp lửa: BA!",
            instructionEn: "Fly with word BA!"
        },
        {
            level: 2,
            targetWord: "BĂ",
            letters: ["B", "Ă"],
            decoys: ["C", "A"],
            emoji: "🩹",
            wordMeaning: "Băng Gạc",
            instruction: "Bé chạm mây chữ B và Ă (có mũ ngược): BĂ!",
            instructionEn: "Fly with word BĂ!"
        },
        {
            level: 3,
            targetWord: "BÂ",
            letters: ["B", "Â"],
            decoys: ["D", "Ă"],
            emoji: "⛅",
            wordMeaning: "Bầu Trời",
            instruction: "Bé chạm mây chữ B và Â (đội nón úp): BÂ!",
            instructionEn: "Fly with word BÂ!"
        },
        {
            level: 4,
            targetWord: "BO",
            letters: ["B", "O"],
            decoys: ["D", "E"],
            emoji: "🐮",
            wordMeaning: "Chú Bò",
            instruction: "Bé chạm mây chữ B và O để khinh khí cầu bay cao: BO!",
            instructionEn: "Fly with word BO!"
        },
        {
            level: 5,
            targetWord: "BÔ",
            letters: ["B", "Ô"],
            decoys: ["A", "O"],
            emoji: "🪆",
            wordMeaning: "Búp Bê / Bô Xinh",
            instruction: "Bé chạm mây chữ B và Ô có nón lá: BÔ!",
            instructionEn: "Fly with word BÔ!"
        },
        {
            level: 6,
            targetWord: "BƠ",
            letters: ["B", "Ơ"],
            decoys: ["C", "Ô"],
            emoji: "🥑",
            wordMeaning: "Quả Bơ",
            instruction: "Bé chạm mây chữ B và Ơ có râu nhỏ: BƠ!",
            instructionEn: "Fly with word BƠ!"
        },
        {
            level: 7,
            targetWord: "CA",
            letters: ["C", "A"],
            decoys: ["B", "O"],
            emoji: "🐟",
            wordMeaning: "Con Cá",
            instruction: "Bé chạm mây chữ C và A để cất cánh: CA!",
            instructionEn: "Fly with word CA!"
        },
        {
            level: 8,
            targetWord: "CO",
            letters: ["C", "O"],
            decoys: ["A", "N"],
            emoji: "🦆",
            wordMeaning: "Con Cò",
            instruction: "Bé chạm mây chữ C và O vượt qua tầng mây: CO!",
            instructionEn: "Fly with word CO!"
        },
        {
            level: 9,
            targetWord: "CÔ",
            letters: ["C", "Ô"],
            decoys: ["B", "Ơ"],
            emoji: "👩‍🏫",
            wordMeaning: "Cô Giáo",
            instruction: "Bé chạm mây chữ C và Ô: CÔ!",
            instructionEn: "Fly with word CÔ!"
        },
        {
            level: 10,
            targetWord: "CU",
            letters: ["C", "U"],
            decoys: ["O", "A"],
            emoji: "🦉",
            wordMeaning: "Con Cú Mèo",
            instruction: "Bé chạm mây chữ C và U: CU!",
            instructionEn: "Fly with word CU!"
        },
        {
            level: 11,
            targetWord: "CƯ",
            letters: ["C", "Ư"],
            decoys: ["U", "O"],
            emoji: "🪚",
            wordMeaning: "Cưa Gỗ",
            instruction: "Bé chạm mây chữ C và Ư có móc: CƯ!",
            instructionEn: "Fly with word CƯ!"
        },
        {
            level: 12,
            targetWord: "DA",
            letters: ["D", "A"],
            decoys: ["B", "E"],
            emoji: "🍈",
            wordMeaning: "Quả Dưa / Làn Da",
            instruction: "Bé chạm mây chữ D và A nạp năng lượng: DA!",
            instructionEn: "Fly with word DA!"
        },
        {
            level: 13,
            targetWord: "DE",
            letters: ["D", "E"],
            decoys: ["B", "I"],
            emoji: "🐐",
            wordMeaning: "Con Dê",
            instruction: "Bé chạm mây chữ D và E: DE!",
            instructionEn: "Fly with word DE!"
        },
        {
            level: 14,
            targetWord: "DO",
            letters: ["D", "O"],
            decoys: ["C", "A"],
            emoji: "🥥",
            wordMeaning: "Quả Dừa / Màu Đỏ",
            instruction: "Bé chạm mây chữ D và O: DO!",
            instructionEn: "Fly with word DO!"
        },
        {
            level: 15,
            targetWord: "ĐA",
            letters: ["Đ", "A"],
            decoys: ["D", "B"],
            emoji: "🌳",
            wordMeaning: "Cây Đa",
            instruction: "Bé chạm mây chữ Đ có gạch ngang và A: ĐA!",
            instructionEn: "Fly with word ĐA!"
        },
        {
            level: 16,
            targetWord: "ĐE",
            letters: ["Đ", "E"],
            decoys: ["D", "C"],
            emoji: "🏮",
            wordMeaning: "Chiếc Đèn",
            instruction: "Bé chạm mây chữ Đ và E: ĐE!",
            instructionEn: "Fly with word ĐE!"
        },
        {
            level: 17,
            targetWord: "GA",
            letters: ["G", "A"],
            decoys: ["C", "Q"],
            emoji: "🐔",
            wordMeaning: "Con Gà",
            instruction: "Bé chạm mây chữ G và A để bay lên: GA!",
            instructionEn: "Fly with word GA!"
        },
        {
            level: 18,
            targetWord: "GO",
            letters: ["G", "O"],
            decoys: ["D", "E"],
            emoji: "🪵",
            wordMeaning: "Khúc Gỗ",
            instruction: "Bé chạm mây chữ G và O: GO!",
            instructionEn: "Fly with word GO!"
        },
        {
            level: 19,
            targetWord: "HA",
            letters: ["H", "A"],
            decoys: ["K", "L"],
            emoji: "🎤",
            wordMeaning: "Hát Ca",
            instruction: "Bé chạm mây chữ H và A vang tiếng hát ca: HA!",
            instructionEn: "Fly with word HA!"
        },
        {
            level: 20,
            targetWord: "HO",
            letters: ["H", "O"],
            decoys: ["B", "M"],
            emoji: "🐯",
            wordMeaning: "Con Hổ",
            instruction: "Bé chạm mây chữ H và O dũng mãnh: HO!",
            instructionEn: "Fly with word HO!"
        },
        {
            level: 21,
            targetWord: "KE",
            letters: ["K", "E"],
            decoys: ["H", "A"],
            emoji: "🍬",
            wordMeaning: "Cái Kẹo",
            instruction: "Bé chạm mây chữ K và E ngọt ngào: KE!",
            instructionEn: "Fly with word KE!"
        },
        {
            level: 22,
            targetWord: "KI",
            letters: ["K", "I"],
            decoys: ["L", "T"],
            emoji: "👓",
            wordMeaning: "Kính Mắt",
            instruction: "Bé chạm mây chữ K và I tinh mắt: KI!",
            instructionEn: "Fly with word KI!"
        },
        {
            level: 23,
            targetWord: "LA",
            letters: ["L", "A"],
            decoys: ["I", "T"],
            emoji: "🍃",
            wordMeaning: "Chiếc Lá",
            instruction: "Bé chạm mây chữ L và A bay theo làn gió: LA!",
            instructionEn: "Fly with word LA!"
        },
        {
            level: 24,
            targetWord: "LE",
            letters: ["L", "E"],
            decoys: ["I", "T"],
            emoji: "🐦",
            wordMeaning: "Chim Le Le",
            instruction: "Bé chạm mây chữ L và E: LE!",
            instructionEn: "Fly with word LE!"
        },
        {
            level: 25,
            targetWord: "ME",
            letters: ["M", "E"],
            decoys: ["B", "I"],
            emoji: "👩",
            wordMeaning: "Mẹ Yêu",
            instruction: "Bé chạm mây chữ M và E nạp năng lượng: ME!",
            instructionEn: "Fly with word ME!"
        },
        {
            level: 26,
            targetWord: "MO",
            letters: ["M", "O"],
            decoys: ["N", "U"],
            emoji: "🦜",
            wordMeaning: "Cái Mỏ",
            instruction: "Bé chạm mây chữ M và O: MO!",
            instructionEn: "Fly with word MO!"
        },
        {
            level: 27,
            targetWord: "NA",
            letters: ["N", "A"],
            decoys: ["M", "O"],
            emoji: "🍈",
            wordMeaning: "Quả Na",
            instruction: "Bé chạm mây chữ N và A: NA!",
            instructionEn: "Fly with word NA!"
        },
        {
            level: 28,
            targetWord: "NO",
            letters: ["N", "O"],
            decoys: ["M", "A"],
            emoji: "🎀",
            wordMeaning: "Chiếc Nơ",
            instruction: "Bé chạm mây chữ N và O: NO!",
            instructionEn: "Fly with word NO!"
        },
        {
            level: 29,
            targetWord: "PA",
            letters: ["P", "A"],
            decoys: ["B", "O"],
            emoji: "🥐",
            wordMeaning: "Bánh Patê",
            instruction: "Bé chạm mây chữ P và A thơm ngon: PA!",
            instructionEn: "Fly with word PA!"
        },
        {
            level: 30,
            targetWord: "QU",
            letters: ["Q", "U"],
            decoys: ["P", "O"],
            emoji: "🍊",
            wordMeaning: "Quả Quýt",
            instruction: "Bé chạm mây chữ Q và U: QU!",
            instructionEn: "Fly with word QU!"
        },
        {
            level: 31,
            targetWord: "RA",
            letters: ["R", "A"],
            decoys: ["S", "B"],
            emoji: "🥬",
            wordMeaning: "Rau Xanh",
            instruction: "Bé chạm mây chữ R và A: RA!",
            instructionEn: "Fly with word RA!"
        },
        {
            level: 32,
            targetWord: "RO",
            letters: ["R", "O"],
            decoys: ["C", "A"],
            emoji: "🧺",
            wordMeaning: "Chiếc Rổ",
            instruction: "Bé chạm mây chữ R và O: RO!",
            instructionEn: "Fly with word RO!"
        },
        {
            level: 33,
            targetWord: "SA",
            letters: ["S", "A"],
            decoys: ["X", "C"],
            emoji: "⭐",
            wordMeaning: "Ngôi Sao",
            instruction: "Bé chạm mây chữ S và A lấp lánh: SA!",
            instructionEn: "Fly with word SA!"
        },
        {
            level: 34,
            targetWord: "TO",
            letters: ["T", "O"],
            decoys: ["L", "I"],
            emoji: "🥚",
            wordMeaning: "Quả Trứng To",
            instruction: "Bé chạm mây chữ T và O: TO!",
            instructionEn: "Fly with word TO!"
        },
        {
            level: 35,
            targetWord: "VO",
            letters: ["V", "O"],
            decoys: ["U", "Y"],
            emoji: "🐘",
            wordMeaning: "Con Voi",
            instruction: "Bé chạm mây chữ V và O: VO!",
            instructionEn: "Fly with word VO!"
        },
        {
            level: 36,
            targetWord: "XE",
            letters: ["X", "E"],
            decoys: ["S", "C"],
            emoji: "🚗",
            wordMeaning: "Xe Hơi",
            instruction: "Bé chạm mây chữ X và E vi vu: XE!",
            instructionEn: "Fly with word XE!"
        },
        {
            level: 37,
            targetWord: "YÊ",
            letters: ["Y", "Ê"],
            decoys: ["V", "U"],
            emoji: "💖",
            wordMeaning: "Yêu Thương",
            instruction: "Bé chạm mây chữ Y dài và Ê đong đầy: YÊ!",
            instructionEn: "Fly with word YÊ!"
        },

        // --- PHASE 2: TỪ PHONICS 3-4 KÝ TỰ SINH ĐỘNG (MÀN 38 ĐẾN 40) ---
        {
            level: 38,
            targetWord: "CAT",
            letters: ["C", "A", "T"],
            decoys: ["B", "O"],
            emoji: "🐱",
            wordMeaning: "Con Mèo",
            instruction: "Ghép từ CAT (Con mèo): C - A - T để bay qua ngọn núi!",
            instructionEn: "Fly with word CAT!"
        },
        {
            level: 39,
            targetWord: "SUN",
            letters: ["S", "U", "N"],
            decoys: ["M", "O"],
            emoji: "☀️",
            wordMeaning: "Mặt Trời",
            instruction: "Ghép từ SUN (Mặt trời): S - U - N đón ánh nắng mai!",
            instructionEn: "Fly with word SUN!"
        },
        {
            level: 40,
            targetWord: "STAR",
            letters: ["S", "T", "A", "R"],
            decoys: ["B", "O"],
            emoji: "⭐",
            wordMeaning: "Ngôi Sao",
            instruction: "Màn 40 khải hoàn: S - T - A - R bay qua cầu vồng tuyệt đẹp!",
            instructionEn: "Fly with word STAR!"
        }
    ],

    // =========================================================================
    // 4. ⛏️ MỎ VÀNG TRI THỨC (GOLD MINER - 39 MÀN: 29 CHỮ CÁI + 10 CHỮ SỐ)
    // =========================================================================
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
