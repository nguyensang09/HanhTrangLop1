(() => {
    const runtime = document.querySelector("[data-activity-runtime]");
    const payloadElement = document.querySelector("[data-activity-payload]");
    const answerInput = document.querySelector("[data-activity-answer]");
    // Nút "Kiểm tra đáp án" đã được loại bỏ ở màn hình học của bé; chỉ trang xem trước
    // của quản trị viên còn giữ nút này nên mọi tham chiếu tới nút đều phải chịu được null.
    const submitButton = document.querySelector("[data-activity-submit]");
    const form = runtime?.closest("form");
    if (!runtime || !payloadElement || !answerInput) return;

    let payload = {};
    try {
        payload = JSON.parse(payloadElement.value || "{}");
    } catch {
        runtime.textContent = "Nội dung bài học chưa đúng định dạng.";
        return;
    }

    // Chỉ xáo trộn bản sao dùng để hiển thị; dữ liệu gốc và đáp án đúng vẫn được
    // chấm theo giá trị nội dung. Fisher–Yates cho mọi vị trí xác suất như nhau,
    // tránh việc trẻ học thuộc "đáp án luôn ở lựa chọn 2".
    const shuffleForDisplay = (values) => {
        const shuffled = Array.isArray(values) ? [...values] : [];
        for (let index = shuffled.length - 1; index > 0; index -= 1) {
            const swapIndex = Math.floor(Math.random() * (index + 1));
            [shuffled[index], shuffled[swapIndex]] = [shuffled[swapIndex], shuffled[index]];
        }
        return shuffled;
    };

    // Kho bài lưu các mục sắp xếp đúng thứ tự: giữ bản gốc trước khi xáo cho cách chơi "leo cầu thang".
    const orderedItems = Array.isArray(payload.items) ? [...payload.items] : [];

    if (Array.isArray(payload.choices)) payload.choices = shuffleForDisplay(payload.choices);
    if (Array.isArray(payload.items)) payload.items = shuffleForDisplay(payload.items);
    if (Array.isArray(payload.pairs)) payload.pairs = shuffleForDisplay(payload.pairs);
    if (Array.isArray(payload.mappings)) payload.mappings = shuffleForDisplay(payload.mappings);

    const type = runtime.dataset.activityType;

    // ==========================================
    // Cách chơi: cùng một bài, cùng cách chấm, nhưng nhiều kiểu thao tác
    // ==========================================
    // Tách "cách chơi" khỏi "dạng bài" như Đảo Toán Học tách phong cách khỏi bộ câu hỏi: đáp án gửi lên máy chủ
    // không đổi nên dữ liệu bài, tiến độ và cách chấm giữ nguyên. Mỗi lần mở bài chọn một cách chơi hợp với nội dung
    // và tránh hai cách vừa dùng, kể cả khi bài trước thuộc nhóm khác (Bài học hôm nay trộn nhiều nhóm).
    const playTarget = String(payload.playTarget || "").trim();
    // Danh mục cách chơi. Thêm một cách chơi mới chỉ cần một dòng ở đây (dùng cho dạng bài nào, khi nào) và một hàm vẽ
    // trong SINGLE_PLAYS, CHOICE_SKINS hoặc PLAY_RENDERERS; dữ liệu bài và cách chấm không phải sửa.
    const CHOICE_TYPES = ["single_choice", "listen_choose"];
    const choiceCount = () => (payload.choices || []).length;
    const choicesUpTo = (max) => () => choiceCount() >= 2 && choiceCount() <= max;
    const hasPlayTarget = () => Boolean(playTarget) && (payload.choices || []).includes(playTarget) && choiceCount() >= 2;
    const expectsCount = () => Number(payload.expectedAnswerCount || 0) > 0;
    const fewPairs = () => (payload.pairs || []).length <= 4;
    const PLAY_CATALOG = [
        { key: "tap", types: CHOICE_TYPES },
        { key: "float", types: CHOICE_TYPES, when: choicesUpTo(6) },
        { key: "tree", types: CHOICE_TYPES, when: choicesUpTo(6) },
        { key: "feed", types: CHOICE_TYPES, when: choicesUpTo(5) },
        { key: "pond", types: CHOICE_TYPES, when: choicesUpTo(4) },
        { key: "race", types: CHOICE_TYPES, when: choicesUpTo(4) },
        { key: "hunt", types: CHOICE_TYPES, when: hasPlayTarget },
        { key: "stones", types: CHOICE_TYPES, when: hasPlayTarget },
        { key: "reveal", types: CHOICE_TYPES, when: hasPlayTarget },
        { key: "burrow", types: CHOICE_TYPES, when: hasPlayTarget },
        { key: "tap", types: ["multi_select"] },
        { key: "basket", types: ["multi_select"], when: expectsCount },
        { key: "net", types: ["multi_select"], when: expectsCount },
        { key: "lines", types: ["matching"] },
        { key: "memory", types: ["matching"], when: fewPairs },
        { key: "homes", types: ["matching"], when: fewPairs },
        { key: "slots", types: ["ordering"] },
        { key: "stairs", types: ["ordering"], when: () => payload.bankVersion && orderedItems.length >= 2 },
        { key: "carriages", types: ["ordering"], when: () => payload.bankVersion && orderedItems.length >= 2 && orderedItems.length <= 6 },
        { key: "count", types: ["counting"] },
        { key: "frame", types: ["counting"], when: () => Number(payload.targetCount || 0) <= 10 },
        { key: "stepper", types: ["quantity_builder"] },
        { key: "gather", types: ["quantity_builder"] },
        { key: "cells", types: ["quantity_builder"], when: () => Number(payload.maxItems || 99) <= 12 },
        { key: "seesaw", types: ["comparison"] },
        { key: "towers", types: ["comparison"], when: () => payload.visualMode !== "shape" },
        { key: "baskets", types: ["classification"] },
        { key: "onebyone", types: ["classification"] }
    ];
    const playOptionsFor = () => PLAY_CATALOG
        .filter((entry) => entry.types.includes(type) && (!entry.when || entry.when()))
        .map((entry) => entry.key);
    const pickPlay = () => {
        if (payload.play) return String(payload.play);
        const options = playOptionsFor();
        if (options.length < 2) return options[0] || "";
        let recent = [];
        try {
            recent = JSON.parse(window.sessionStorage.getItem("lw-recent-plays") || "[]");
        } catch { /* trình duyệt chặn lưu trữ: chỉ mất phần tránh lặp */ }
        let pool = options.filter((option) => !recent.includes(option));
        if (pool.length === 0) pool = options.filter((option) => option !== recent[0]);
        if (pool.length === 0) pool = options;
        const chosen = pool[Math.floor(Math.random() * pool.length)];
        try {
            window.sessionStorage.setItem("lw-recent-plays", JSON.stringify([chosen, ...recent].slice(0, 2)));
        } catch { /* như trên */ }
        return chosen;
    };
    const play = pickPlay();
    if (play) runtime.dataset.play = play;

    // ==========================================
    // Cơ chế tự động kiểm tra đáp án dùng chung
    // ==========================================
    // Mọi dạng bài đều tự chấm khi bé trả lời xong, không cần thao tác xác nhận thêm.
    // Khác biệt duy nhất giữa các dạng là thời điểm được coi là "đã trả lời xong":
    //   - instant: chọn một đáp án là xong (chọn một, nghe và chọn, kéo thả, đếm, so sánh...)
    //   - complete: phải điền đủ toàn bộ cặp/nhóm (nối cặp, phân loại, chọn nhiều đáp án)
    //   - settle: bé có thể chỉnh nhiều lần, chấm sau khi dừng chỉnh (sắp xếp, tạo số lượng)
    const INSTANT_DELAY = 250;
    const COMPLETE_DELAY = 300;
    const SETTLE_DELAY = 1500;
    // Sau khi đúng: giữ hiệu ứng tối thiểu ngần này, chờ Voice "đáp án → Chính xác!" đọc xong nhưng không quá mức trần.
    const CORRECT_MIN_HOLD = 700;
    const CORRECT_MAX_HOLD = 2300;
    const settleTypes = new Set(["ordering", "quantity_builder"]);
    const autoCheckEnabled = runtime.dataset.activityAutocheck !== "false";

    const statusElement = document.querySelector("[data-activity-autocheck-status]");
    const statusText = document.querySelector("[data-activity-autocheck-text]");
    const statusProgress = document.querySelector("[data-activity-autocheck-progress]");

    let submitTimer = 0;
    let optionColorIndex = 0;
    let hasSubmitted = false;

    if (submitButton && autoCheckEnabled) submitButton.hidden = true;

    const hideStatus = () => {
        window.clearTimeout(submitTimer);
        submitTimer = 0;
        if (!statusElement) return;
        statusElement.hidden = true;
        statusElement.classList.remove("is-active");
        if (statusProgress) statusProgress.style.transition = "none";
        if (statusProgress) statusProgress.style.width = "0%";
    };

    const showStatus = (message, delay) => {
        if (!statusElement) return;
        statusElement.hidden = false;
        if (statusText) statusText.textContent = message;
        if (statusProgress) {
            statusProgress.style.transition = "none";
            statusProgress.style.width = "0%";
            // Buộc trình duyệt áp dụng mốc 0% trước khi chạy hiệu ứng, nếu không thanh
            // tiến trình sẽ nhảy thẳng tới 100% và bé không thấy phản hồi nào.
            void statusProgress.offsetWidth;
            statusProgress.style.transition = `width ${delay}ms linear`;
            statusProgress.style.width = "100%";
        }
        setResultState("is-checking", "auto_awesome");
        window.requestAnimationFrame(() => statusElement.classList.add("is-active"));
    };

    // ==========================================
    // Thanh kết quả: một thanh duy nhất vừa báo "cô đang xem bài" vừa báo đúng / chưa đúng
    // ==========================================
    const resultIcon = document.querySelector("[data-lw-result-icon]");
    const resultActions = document.querySelector("[data-lw-result-actions]");
    const RESULT_STATES = ["is-checking", "is-correct", "is-retry"];
    const setResultState = (state, icon) => {
        if (!statusElement) return;
        statusElement.classList.remove(...RESULT_STATES, "is-holding");
        if (state) statusElement.classList.add(state);
        if (resultIcon && icon) resultIcon.textContent = icon;
        if (resultActions && state !== "is-correct") resultActions.replaceChildren();
    };
    const runProgress = (duration) => {
        if (!statusProgress) return;
        statusProgress.style.transition = "none";
        statusProgress.style.width = "0%";
        void statusProgress.offsetWidth;
        statusProgress.style.transition = `width ${duration}ms linear`;
        statusProgress.style.width = "100%";
    };
    const wait = (ms) => new Promise((resolve) => window.setTimeout(resolve, ms));
    // Đọc phản hồi của đúng lần chấm này: kết quả, lời nhắc và file Voice đều lấy từ cùng một phản hồi máy chủ.
    const playFeedback = (data) => {
        try {
            return Promise.resolve(window.learningVoice?.playFeedback?.(data)).catch(() => {});
        } catch {
            return Promise.resolve();
        }
    };
    const actionLink = (href, label, icon, extraClass = "") => {
        const link = document.createElement("a");
        link.className = `lw-result-btn ${extraClass}`.trim();
        link.href = href;
        const text = document.createElement("span");
        text.textContent = label;
        const glyph = document.createElement("span");
        glyph.className = "material-symbols-outlined";
        glyph.textContent = icon;
        if (extraClass.includes("gift")) link.append(glyph, text);
        else link.append(text, glyph);
        return link;
    };

    const celebrate = () => {
        const burst = document.createElement("div");
        burst.className = "lw-burst";
        burst.setAttribute("aria-hidden", "true");
        ["⭐", "🎉", "✨", "🌟", "🎈", "💛", "⭐", "✨"].forEach((symbol, index) => {
            const piece = document.createElement("span");
            piece.textContent = symbol;
            piece.style.setProperty("--i", String(index));
            burst.append(piece);
        });
        document.body.append(burst);
        window.setTimeout(() => burst.remove(), 1800);
    };

    const showCorrect = async (data) => {
        setResultState("is-correct", "celebration");
        runtime.classList.add("is-solved");
        if (statusText) statusText.textContent = data.message || "Chính xác!";
        celebrate();
        const nextUrl = data.nextUrl || "/kids/today";
        if (resultActions) {
            resultActions.replaceChildren();
            if (data.milestone && data.rewardsUrl) resultActions.append(actionLink(data.rewardsUrl, "Mở quà", "redeem", "lw-result-btn--gift"));
            resultActions.append(actionLink(nextUrl, data.nextLabel || "Sang bài tiếp theo", "arrow_forward"));
        }
        const spoken = playFeedback(data);
        if (data.milestone) {
            // Vừa mở rương quà: bé tự chọn mở quà ngay hay học tiếp.
            if (statusText) statusText.textContent = `${data.message || "Đúng rồi!"} ${data.milestone}`;
            statusElement?.classList.add("is-holding");
            return;
        }
        runProgress(CORRECT_MIN_HOLD);
        await Promise.race([Promise.all([spoken, wait(CORRECT_MIN_HOLD)]), wait(CORRECT_MAX_HOLD)]);
        window.location.assign(nextUrl);
    };

    const showRetry = async (data) => {
        setResultState("is-retry", "replay");
        if (statusText) statusText.textContent = data.message || "Con thử lại nhé";
        runtime.classList.add("is-shaking");
        const spoken = playFeedback(data);
        await wait(900);
        runtime.classList.remove("is-shaking");
        await Promise.race([spoken, wait(4000)]);
        // Làm lại ngay tại chỗ: vẽ lại bảng làm bài, giữ thanh nhắc cho tới khi bé chọn lại.
        answerInput.value = "";
        optionColorIndex = 0;
        runtime.replaceChildren();
        renderActivity();
        lockBoard(false);
        hasSubmitted = false;
    };

    // Các cách chơi nhiều lượt (bắt bóng, qua suối, leo cầu thang, lật thẻ) tự biết thẻ bé vừa chạm là đúng hay sai,
    // chưa gửi lên máy chủ: chỉ báo để bé thử lại, không tính là một lần làm sai. Voice nhắc đọc sau tên thẻ bé vừa chạm.
    const RETRY_TEXT = "Chưa đúng rồi, con thử lại nhé!";
    const notifyWrong = (...tiles) => {
        tiles.forEach((tile) => {
            tile.classList.remove("is-wrong");
            void tile.offsetWidth;
            tile.classList.add("is-wrong");
            window.setTimeout(() => tile.classList.remove("is-wrong"), 700);
        });
        if (statusElement) {
            statusElement.hidden = false;
            setResultState("is-retry", "replay");
            if (statusText) statusText.textContent = RETRY_TEXT;
            if (statusProgress) {
                statusProgress.style.transition = "none";
                statusProgress.style.width = "0%";
            }
            window.requestAnimationFrame(() => statusElement.classList.add("is-active"));
        }
        void playFeedback({ correct: false, message: RETRY_TEXT });
    };
    const clearWrongNotice = () => {
        if (statusElement?.classList.contains("is-retry") && !hasSubmitted) hideStatus();
    };

    // Khóa bảng làm bài trong lúc chấm: bé chạm thêm đáp án khác lúc này sẽ làm màn hình lệch với kết quả
    // và Voice của lần chấm đang chờ. Mở lại khi bé được làm lại.
    const lockBoard = (locked) => {
        runtime.inert = locked;
        runtime.style.pointerEvents = locked ? "none" : "";
    };

    const submitAnswer = async () => {
        if (hasSubmitted || !form) return;
        hasSubmitted = true;
        lockBoard(true);
        window.clearTimeout(submitTimer);
        if (statusText) statusText.textContent = "Cô đang xem bài của con nhé...";
        let data = null;
        try {
            const response = await fetch(form.action, {
                method: "POST",
                body: new FormData(form),
                credentials: "same-origin",
                headers: { Accept: "application/json" }
            });
            if (response.ok && (response.headers.get("content-type") || "").includes("json")) data = await response.json();
        } catch {
            data = null;
        }
        if (!data) {
            // Mạng chập chờn hoặc phiên hết hạn: nộp kiểu thường để máy chủ xử lý như trước.
            HTMLFormElement.prototype.submit.call(form);
            return;
        }
        const questionInput = form.querySelector("input[name=\"QuestionId\"]");
        if (data.questionId && questionInput && String(data.questionId) !== questionInput.value) {
            // Kết quả không thuộc câu đang hiển thị (phiên cũ): tải lại thay vì báo nhầm.
            window.location.reload();
            return;
        }
        if (data.correct) await showCorrect(data);
        else await showRetry(data);
    };

    // Lên lịch tự chấm bài. Mỗi lần bé thao tác lại thì hẹn giờ được đặt lại nên bé
    // luôn có thể sửa đáp án trước khi hệ thống chấm.
    const scheduleAutoSubmit = (delay, message) => {
        if (!autoCheckEnabled || hasSubmitted || !form) return;
        window.clearTimeout(submitTimer);
        showStatus(message || "Cô đang xem bài của con nhé...", delay);
        submitTimer = window.setTimeout(submitAnswer, delay);
    };

    const activityColors = ["#ff8a5b", "#29b6a6", "#3e9ed6", "#f4b740", "#8b79d1", "#ec407a", "#26a69a"];
    const pictogramPath = "/images/pictograms/";
    const pictograms = new Map([
        ["🍎", "apple.svg"], ["🍊", "orange.svg"], ["🐟", "fish.svg"], ["⭐", "star.svg"], ["🌼", "flower.svg"], ["🍓", "strawberry.svg"],
        ["táo", "apple.svg"], ["quả táo", "apple.svg"], ["cam", "orange.svg"], ["quả cam", "orange.svg"],
        ["cà rốt", "carrot.svg"], ["củ cà rốt", "carrot.svg"], ["bắp cải", "leafy-green.svg"], ["chuối", "apple.svg"], ["dâu tây", "strawberry.svg"],
        ["cá", "fish.svg"], ["con cá", "fish.svg"], ["chú cá", "fish.svg"], ["tôm", "shrimp.svg"], ["con tôm", "shrimp.svg"],
        ["mèo", "cat.svg"], ["con mèo", "cat.svg"], ["chó", "dog.svg"], ["con chó", "dog.svg"],
        ["vịt", "duck.svg"], ["con vịt", "duck.svg"], ["gà", "chicken.svg"], ["con gà", "chicken.svg"],
        ["chim", "bird.svg"], ["con chim", "bird.svg"], ["ong", "bee.svg"], ["con ong", "bee.svg"],
        ["thỏ", "rabbit.svg"], ["con thỏ", "rabbit.svg"], ["bướm", "bee.svg"], ["con bướm", "bee.svg"],
        ["áo mưa", "coat.svg"], ["ô", "umbrella.svg"], ["chiếc ô", "umbrella.svg"], ["cái ô", "umbrella.svg"], ["che mưa", "umbrella.svg"], ["mũ rộng vành", "sun-hat.svg"], ["kính râm", "sunglasses.svg"],
        ["bút", "pencil.svg"], ["bút chì", "pencil.svg"], ["bút màu", "artist-palette.svg"], ["vẽ tranh", "artist-palette.svg"], ["vẽ nét", "pencil.svg"], ["vẽ", "artist-palette.svg"], ["vở", "notebook.svg"], ["quyển vở", "notebook.svg"],
        ["sách", "book.svg"], ["quyển sách", "book.svg"], ["ba lô", "backpack.svg"], ["cặp sách", "backpack.svg"],
        ["bát", "bowl.svg"], ["cái bát", "bowl.svg"], ["cốc", "bowl.svg"], ["cái cốc", "bowl.svg"], ["thìa", "spoon.svg"], ["cái thìa", "spoon.svg"], ["nồi", "cooking-pot.svg"], ["uống nước", "water.svg"],
        ["thuyền", "sailboat.svg"], ["xe đạp", "bicycle.svg"], ["ô tô", "car.svg"], ["xe ô tô", "car.svg"], ["xe", "car.svg"], ["xe buýt", "bus.svg"], ["máy bay", "airplane.svg"], ["gara", "house.svg"],
        ["bàn chải", "toothbrush.svg"], ["kem đánh răng", "toothbrush.svg"], ["áo", "shirt.svg"], ["quần", "pants.svg"], ["giày", "shoe.svg"], ["cởi giày", "shoe.svg"], ["tất", "socks.svg"], ["mũ", "hat.svg"], ["khăn", "scarf.svg"],
        ["kem", "ice-cream.svg"], ["nước đá", "ice-cube.svg"], ["canh", "cooking-pot.svg"], ["trà", "tea.svg"], ["gối", "pillow.svg"], ["bông", "cloud.svg"], ["đá", "rock.svg"], ["gạch", "brick.svg"],
        ["mặt trời", "sun.svg"], ["mặt trăng", "moon.svg"], ["quả bóng", "ball.svg"], ["xà phòng", "soap.svg"], ["cây", "seedling.svg"], ["bông hoa", "flower.svg"], ["kéo", "scissors.svg"], ["hạt", "thread.svg"], ["tô màu", "artist-palette.svg"],
        ["mũ bảo hiểm", "helmet.svg"], ["đội mũ bảo hiểm", "helmet.svg"], ["ổ điện", "electric-plug.svg"], ["chia sẻ", "handshake.svg"], ["xin lỗi", "folded-hands.svg"], ["buồn", "sad-face.svg"], ["người lớn", "handshake.svg"],
        ["trái cây", "apple.svg"], ["rau củ", "carrot.svg"], ["dưới nước", "fish.svg"], ["trên cạn", "cat.svg"],
        ["trời mưa", "umbrella.svg"], ["trời nắng", "sun.svg"], ["học tập", "notebook.svg"], ["nhà bếp", "cooking-pot.svg"],
        ["ban ngày", "sun.svg"], ["ban đêm", "moon.svg"], ["lạnh", "ice-cube.svg"], ["nóng", "tea.svg"],
        ["đứng lên", "standing.svg"], ["đùa nghịch", "game.svg"], ["đứng nhảy nhót", "game.svg"], ["từ chối và gọi người thân", "telephone.svg"],
        ["đi theo ngay", "walking.svg"], ["không nói với ai", "zipper-mouth.svg"], ["nói với người con tin tưởng", "speaking.svg"],
        ["đập đồ", "hammer.svg"], ["la hét vào bạn", "speaking.svg"], ["không chạm vào", "prohibited.svg"],
        ["cho tay vào", "raised-hand.svg"], ["đổ nước lên", "water.svg"], ["giấu đồ chơi", "package.svg"],
        ["đẩy bạn ra", "raised-hand.svg"], ["không phải mình", "shrug.svg"], ["bạn tự chịu", "sad-face.svg"],
        ["tự chạy thật nhanh", "running.svg"], ["chạy thật nhanh qua đường", "running.svg"], ["đứng chơi giữa đường", "standing.svg"], ["đi ngủ", "sleeping.svg"],
        ["cất sách", "book.svg"], ["cất hết bút đi", "package.svg"], ["bỏ ra ngoài", "walking.svg"],
        ["rửa tay", "soap.svg"], ["làm ướt tay", "water.svg"], ["lấy xà phòng", "soap.svg"], ["chà sạch tay", "soap.svg"], ["xả nước", "water.svg"], ["lau khô", "shirt.svg"],
        ["chào cô", "speaking.svg"], ["cất ba lô", "backpack.svg"], ["ngồi vào chỗ", "standing.svg"], ["mở sách", "book.svg"],
        ["ăn cơm", "bowl.svg"], ["dọn bát", "bowl.svg"], ["chọn bút", "pencil.svg"], ["cầm bằng ba ngón", "pencil.svg"],
        ["đặt giấy ngay ngắn", "notebook.svg"], ["gấp hai mép", "notebook.svg"], ["miết nếp gấp", "notebook.svg"],
        ["gieo hạt", "seedling.svg"], ["tưới nước", "water.svg"], ["hạt nảy mầm", "seedling.svg"], ["cây lớn lên", "seedling.svg"],
        ["meo meo", "cat.svg"], ["gâu gâu", "dog.svg"], ["cạp cạp", "duck.svg"],
        ["cao", "sun.svg"], ["thấp", "flower.svg"],
        // Traffic lights & Safety actions
        ["đèn đỏ", "traffic-red.svg"], ["tín hiệu đèn đỏ", "traffic-red.svg"],
        ["đèn vàng", "traffic-yellow.svg"], ["tín hiệu đèn vàng", "traffic-yellow.svg"],
        ["đèn xanh", "traffic-green.svg"], ["tín hiệu đèn xanh", "traffic-green.svg"],
        ["dừng lại", "stop-sign.svg"], ["dừng", "stop-sign.svg"], ["dừng lại trước vạch kẻ đường", "stop-sign.svg"],
        ["đi chậm", "slow-sign.svg"], ["chậm lại", "slow-sign.svg"], ["giảm tốc độ", "slow-sign.svg"],
        ["được đi", "go-sign.svg"], ["đi tiếp", "go-sign.svg"], ["được phép đi", "go-sign.svg"],
        ["khi đèn người đi bộ màu xanh", "go-sign.svg"], ["khi xe đang chạy", "car.svg"], ["khi đèn người đi bộ màu đỏ", "stop-sign.svg"],
        ["chia sẻ bút màu", "artist-palette.svg"], ["chia sẻ và chơi cùng", "handshake.svg"],
        ["mình xin lỗi bạn", "folded-hands.svg"], ["đi cùng người lớn", "walking.svg"],
        ["gà mẹ", "chicken.svg"], ["gà con", "chicken.svg"],
        ["mèo mẹ", "cat.svg"], ["mèo con", "cat.svg"],
        ["vịt mẹ", "duck.svg"], ["vịt con", "duck.svg"],
        ["tổ cây", "seedling.svg"], ["hồ nước", "water.svg"], ["tổ ong", "bee.svg"],
        ["hạt thóc", "seedling.svg"], ["cỏ tươi", "leafy-green.svg"]
    ]);

    const shapeClasses = new Map([
        ["hình tròn", "circle"], ["tròn", "circle"], ["○", "circle"],
        ["hình vuông", "square"], ["vuông", "square"], ["□", "square"],
        ["hình tam giác", "triangle"], ["tam giác", "triangle"], ["△", "triangle"],
        ["hình chữ nhật", "rectangle"], ["hình bầu dục", "oval"], ["hình thoi", "diamond"],
        ["hình ngôi sao", "star"], ["ngôi sao", "star"], ["⭐", "star"], ["★", "star"],
        ["hình trái tim", "heart"], ["trái tim", "heart"], ["❤️", "heart"]
    ]);

    const colorValues = new Map([
        ["đỏ", "#ff5252"], ["xanh", "#1e88e5"], ["vàng", "#fbc02d"], ["tím", "#8e24aa"],
        ["hồng", "#ec407a"], ["xanh lá", "#43a047"], ["cam", "#fb8c00"], ["nâu", "#6d4c41"]
    ]);

    // Kho bài mới: hình cho từng nhãn đáp án (emoji, ảnh "/images/...", "shape:...", "shadow:emoji" = bóng đen,
    // "pic:..." = chỉ hiện hình, ẩn chữ để không lộ đáp án).
    const visuals = new Map(Object.entries(payload.visuals || {})
        .map(([label, value]) => [label.trim().toLocaleLowerCase("vi-VN"), String(value || "").trim()]));
    const resolveVisual = (text) => visuals.get(String(text || "").trim().toLocaleLowerCase("vi-VN")) || "";
    const renderVisual = (button, visual, value) => {
        let source = visual;
        let hideLabel = false;
        let shadow = false;
        if (source.startsWith("pic:")) { source = source.slice(4); hideLabel = true; }
        else if (source.startsWith("shadow:")) { source = source.slice(7); hideLabel = true; shadow = true; }
        let element;
        if (source.startsWith("/")) {
            element = document.createElement("img");
            element.className = "answer-photo";
            element.src = source;
            element.alt = hideLabel ? "" : value;
            element.loading = "lazy";
            button.classList.add("has-answer-photo");
        } else if (source.startsWith("shape:")) {
            element = document.createElement("span");
            element.className = `answer-shape answer-shape-${source.slice(6)}`;
        } else {
            element = document.createElement("span");
            element.className = "answer-emoji";
            element.textContent = source;
        }
        if (shadow) element.classList.add("is-shadow");
        element.setAttribute("aria-hidden", "true");
        button.classList.add("has-answer-visual");
        if (hideLabel) {
            button.classList.add("is-picture-only");
            button.setAttribute("aria-label", shadow ? "Cái bóng" : "Hình");
        }
        button.append(element);
        return hideLabel;
    };

    const itemMedia = new Map(Object.entries(payload.itemMedia || {})
        .map(([label, url]) => [label.trim().toLocaleLowerCase("vi-VN"), String(url || "").trim()]));

    const resolveItemMedia = (text) => {
        const normalized = String(text || "").trim().toLocaleLowerCase("vi-VN");
        const candidates = [
            normalized,
            normalized.replace(/^con\s+/, ""),
            normalized.replace(/^chú\s+/, ""),
            normalized.replace(/^cái\s+/, ""),
            normalized.replace(/^quả\s+/, ""),
            normalized.replace(/^trái\s+/, ""),
            normalized.replace(/^chiếc\s+/, "")
        ];
        for (const candidate of candidates) {
            if (itemMedia.has(candidate)) return itemMedia.get(candidate);
        }
        return "";
    };

    const resolvePictogram = (text) => {
        const normalized = String(text || "").trim().toLocaleLowerCase("vi-VN");
        if (pictograms.has(normalized)) return pictograms.get(normalized);
        const match = [...pictograms.entries()]
            .filter(([label]) => label.length > 1 && normalized.includes(label))
            .sort(([left], [right]) => right.length - left.length)[0];
        return match?.[1] || "";
    };

    // Chữ cái hoặc ký hiệu đơn: độ dài 1-2 ký tự (chữ cái tiếng Việt, tiếng Anh, hoặc chữ số)
    const isSingleSymbol = (text) => {
        const clean = String(text || "").trim();
        return clean.length >= 1 && clean.length <= 2 && /^[\p{L}\p{N}]+$/u.test(clean);
    };

    const getRepeatedPictureGroup = (text) => {
        const parts = String(text || "").trim().split(/\s+/).filter(Boolean);
        if (parts.length < 2 || parts.length > 9 || !parts.every((part) => part === parts[0])) {
            return null;
        }

        // Only convert repeated pictorial symbols. Repeated words remain literal text.
        if (/[\p{L}\p{N}]/u.test(parts[0])) return null;
        return { symbol: parts[0], count: parts.length };
    };

    const decorateButton = (button, text, forceIcon = "") => {
        const rawValue = String(text ?? "").trim();
        // Nhãn hiển thị phải đúng tuyệt đối với dữ liệu đã soạn. Không tự rút gọn
        // "Chữ E" thành "E" hoặc "Số 1" thành "1" vì sẽ làm lệch nội dung/voice.
        const value = rawValue;
        const normalized = value.trim().toLocaleLowerCase("vi-VN");
        let mediaUrl = resolveItemMedia(rawValue);
        let pictogram = resolvePictogram(value);
        const shape = shapeClasses.get(normalized);
        const color = colorValues.get(normalized);
        const repeatedPictureGroup = getRepeatedPictureGroup(value);
        const visual = resolveVisual(rawValue);
        let hideLabel = false;
        const singleSymbol = isSingleSymbol(value);

        // Với bài tập chữ cái / ký hiệu đơn (Ă, A, â, b, B, c, C...) hoặc nếu mediaUrl là hình quyển sách book.svg bị gán mặc định:
        // Tuyệt đối loại bỏ hình ảnh thừa để chữ cái hiển thị to, rõ nét, trực quan.
        if (singleSymbol || (mediaUrl && mediaUrl.includes("book.svg") && !normalized.includes("sách") && !normalized.includes("vở"))) {
            mediaUrl = "";
            pictogram = "";
        }
        // Bài trong kho có sẵn hình cho từng nhãn: không đoán hình theo chữ, tránh nhãn không có hình lại mượn nhầm hình khác.
        if (payload.bankVersion) pictogram = "";

        button.replaceChildren();

        if (repeatedPictureGroup) {
            button.classList.add("is-quantity-visual");
        } else if (singleSymbol) {
            button.classList.add("is-single-symbol", "matching-letter-item");
        } else {
            button.classList.add("is-text-word");
            if (value.trim().length > 5) button.classList.add("is-long-phrase");
        }

        if (repeatedPictureGroup) {
            // Khi các nhóm số lượng trong bài tập dùng chung một hình (như cùng là hoa),
            // tự động dùng hình ảnh khác nhau cho từng số lượng để trẻ dễ phân biệt và sinh động hơn.
            const countDistinctSymbols = {
                1: "🍎", // Quả táo
                2: "🚗", // Ô tô
                3: "⭐", // Ngôi sao
                4: "🐟", // Chú cá
                5: "🌼", // Bông hoa
                6: "🍓", // Quả dâu
                7: "🐤", // Gà con
                8: "🎈", // Bóng bay
                9: "🦋", // Con bướm
                10: "🍄" // Cây nấm
            };
            const rightGroups = (payload.pairs || []).map((p) => getRepeatedPictureGroup(p.right)).filter(Boolean);
            const isAllSameSymbol = rightGroups.length > 1 && rightGroups.every((g) => g.symbol === rightGroups[0].symbol);
            const displaySymbol = isAllSameSymbol
                ? (countDistinctSymbols[repeatedPictureGroup.count] || repeatedPictureGroup.symbol)
                : repeatedPictureGroup.symbol;

            const visual = document.createElement("span");
            visual.className = "answer-quantity-visual";
            visual.setAttribute("aria-hidden", "true");
            visual.style.setProperty("--quantity-columns", String(Math.min(3, repeatedPictureGroup.count)));
            for (let index = 0; index < repeatedPictureGroup.count; index += 1) {
                const picture = document.createElement("span");
                picture.className = "answer-quantity-symbol";
                picture.textContent = displaySymbol;
                visual.append(picture);
            }
            button.setAttribute("aria-label", `Nhóm có ${repeatedPictureGroup.count} đồ vật`);
            button.append(visual);
        } else if (visual) {
            hideLabel = renderVisual(button, visual, value);
        } else if (mediaUrl && !singleSymbol) {
            const image = document.createElement("img");
            image.className = "answer-photo";
            image.src = mediaUrl;
            image.alt = value;
            image.loading = "lazy";
            button.classList.add("has-answer-visual", "has-answer-photo");
            button.append(image);
        } else if (pictogram && !singleSymbol) {
            const image = document.createElement("img");
            image.className = "answer-pictogram";
            image.src = `${pictogramPath}${pictogram}`;
            image.alt = value;
            image.loading = "lazy";
            button.classList.add("has-answer-visual", "has-answer-pictogram");
            button.append(image);
        } else if ((shape || color) && !singleSymbol) {
            const visual = document.createElement("span");
            visual.className = shape ? `answer-shape answer-shape-${shape}` : "answer-color-swatch";
            if (color) visual.style.backgroundColor = color;
            visual.setAttribute("aria-hidden", "true");
            button.classList.add("has-answer-visual");
            button.append(visual);
        } else if (forceIcon && !singleSymbol) {
            const iconSpan = document.createElement("span");
            iconSpan.className = "material-symbols-outlined answer-icon-glyph";
            iconSpan.textContent = forceIcon;
            button.classList.add("has-answer-visual");
            button.append(iconSpan);
        }

        if (!repeatedPictureGroup && !hideLabel) {
            const label = document.createElement("span");
            label.className = "answer-label";
            if (value.trim().length > 4) label.classList.add("long-label");
            label.textContent = value;
            button.append(label);
        }
    };

    const appendRepeatedVisuals = (container, value, count) => {
        container.replaceChildren();
        const num = Number(count || 0);
        for (let index = 0; index < num; index += 1) {
            const pictogram = resolvePictogram(value);
            const mediaUrl = resolveItemMedia(value);
            if (mediaUrl) {
                const image = document.createElement("img");
                image.className = "counting-photo";
                image.src = mediaUrl;
                image.alt = "";
                image.setAttribute("aria-hidden", "true");
                container.append(image);
            } else if (pictogram) {
                const image = document.createElement("img");
                image.className = "counting-pictogram";
                image.src = `${pictogramPath}${pictogram}`;
                image.alt = "";
                image.setAttribute("aria-hidden", "true");
                container.append(image);
            } else {
                const symbol = document.createElement("span");
                symbol.className = "counting-symbol";
                symbol.textContent = value || "●";
                container.append(symbol);
            }
        }
    };

    // Mọi Voice của trang làm bài đi qua bộ điều khiển chung trong learning-voice.js (window.learningVoice):
    // chạm đáp án sẽ dừng Voice câu hỏi đang đọc rồi đọc đúng đáp án đó, không bao giờ có hai Voice chồng nhau.
    const voice = () => window.learningVoice || null;

    // Đếm / tạo số lượng: đọc con số bé vừa đếm tới (dùng chung kho Voice đáp án theo nhãn số).
    const speak = (text) => voice()?.playOption?.(String(text ?? ""));

    const playPromptAudio = () => voice()?.playQuestion?.();

    const playAnswerAudio = (value) => {
        void voice()?.playOption?.(String(value ?? "").trim());
        return true;
    };

    const createButton = (text, className = "activity-option clay-button", forceIcon = "") => {
        const button = document.createElement("button");
        button.type = "button";
        button.className = className;
        button.dataset.answerValue = String(text ?? "").trim();
        decorateButton(button, text, forceIcon);
        if (button.matches(".activity-option, .activity-drop-zone, .comparison-group, .lw-tile")) {
            const color = activityColors[optionColorIndex % activityColors.length];
            button.style.setProperty("--option-color", color);
            button.style.setProperty("--selection-color", color);
            optionColorIndex += 1;
        }
        return button;
    };

    // ready: bé đã trả lời đủ điều kiện hoàn thành của dạng bài này chưa.
    // instant: dạng bài chốt ngay khi chọn (không cần chờ bé điền thêm phần nào khác).
    const setAnswer = (value, ready = true, instant = false, instantDelay = INSTANT_DELAY) => {
        answerInput.value = value;
        if (submitButton) submitButton.disabled = !ready;

        if (!ready) {
            hideStatus();
            return;
        }

        if (instant) {
            scheduleAutoSubmit(instantDelay, "Con chọn xong rồi, cô xem nhé...");
            return;
        }

        if (settleTypes.has(type)) {
            // Bé còn có thể đổi ý: chỉ chấm sau khi bé dừng thao tác.
            scheduleAutoSubmit(SETTLE_DELAY, "Con cứ sắp xếp tiếp, cô chờ con xong nhé...");
            return;
        }

        // Nối cặp / phân loại / chọn nhiều đáp án: đã điền đủ thì chấm luôn.
        scheduleAutoSubmit(COMPLETE_DELAY, "Con làm xong rồi, cô xem nhé...");
    };

    const canonicalMappings = (mappings) => Object.entries(mappings)
        .sort(([leftA], [leftB]) => leftA < leftB ? -1 : leftA > leftB ? 1 : 0)
        .map(([left, right]) => `${left}=>${right}`).join("|");

    // ==========================================
    // Bộ khung chung của bảng làm bài: thẻ nhiệm vụ, thẻ đáp án, kéo thả bằng Pointer Events
    // ==========================================
    const el = (tag, className = "", text = "") => {
        const node = document.createElement(tag);
        if (className) node.className = className;
        if (text) node.textContent = text;
        return node;
    };

    const icon = (name, className = "") => {
        const node = el("span", `material-symbols-outlined ${className}`.trim(), name);
        node.setAttribute("aria-hidden", "true");
        return node;
    };

    const reduceMotion = () => window.matchMedia?.("(prefers-reduced-motion: reduce)")?.matches === true;

    // Chạy lại một hiệu ứng ngắn (nảy, rung...) bằng cách gỡ rồi gắn lại lớp CSS.
    const flash = (node, className) => {
        if (!node || reduceMotion()) return;
        node.classList.remove(className, "is-entering");
        void node.offsetWidth;
        node.classList.add(className);
        window.setTimeout(() => node.classList.remove(className), 700);
    };

    // Thẻ "nhiệm vụ": một biểu tượng lớn + vài chữ ngắn để bé (và bố mẹ) hiểu ngay phải làm gì.
    const goalPill = (glyph, text, extra = null) => {
        const goal = el("div", "lw-goal");
        goal.append(icon(glyph, "lw-goal-icon"), el("span", "lw-goal-text", text));
        if (extra) goal.append(extra);
        return goal;
    };

    // Thẻ đáp án dạng "kẹo dẻo": hình to + nhãn, có huy hiệu ✓ khi được chọn.
    const makeTile = (text, extraClass = "", forceIcon = "") => {
        const tile = createButton(text, `lw-tile ${extraClass}`.trim(), forceIcon);
        tile.classList.add("is-entering");
        tile.addEventListener("animationend", (event) => {
            if (event.target === tile && event.animationName === "lw-tile-in") tile.classList.remove("is-entering");
        });
        tile.append(icon("check", "lw-check"));
        return tile;
    };

    const ghostHost = () => runtime.closest(".lw") || document.body;

    // Tìm đích gần nhất dưới ngón tay, nới rộng mỗi đích thêm vài điểm ảnh để bé thả "gần đúng" vẫn được.
    const hitTarget = (x, y, targets, pad = 28) => {
        let best = null;
        let bestDistance = Infinity;
        (targets || []).forEach((target) => {
            if (!target || !target.isConnected) return;
            const rect = target.getBoundingClientRect();
            if (rect.width === 0 && rect.height === 0) return;
            if (x < rect.left - pad || x > rect.right + pad || y < rect.top - pad || y > rect.bottom + pad) return;
            const dx = x - (rect.left + rect.width / 2);
            const dy = y - (rect.top + rect.height / 2);
            const distance = dx * dx + dy * dy;
            if (distance < bestDistance) {
                best = target;
                bestDistance = distance;
            }
        });
        return best;
    };

    // Kéo thả bằng Pointer Events (chạy trên iPad Safari, chuột và bút). Chạm nhẹ không kéo là "chạm"
    // (onTap qua sự kiện click); chỉ khi ngón tay đi quá vài điểm ảnh thẻ mới được nhấc lên thành "bóng"
    // bay theo tay. Theo dõi trên window để vẫn dọn dẹp được nếu bảng bị khóa giữa chừng.
    const enableDrag = (source, options) => {
        const { targets, onStart, onMove, onDrop, onEnd, onTap, ghost: useGhost = true } = options;
        let pointerId = null;
        let startX = 0;
        let startY = 0;
        let moved = false;
        let ghost = null;
        let hovered = null;
        let suppressClickUntil = 0;

        const setHover = (target) => {
            if (hovered === target) return;
            hovered?.classList.remove("is-hover");
            hovered = target;
            hovered?.classList.add("is-hover");
        };

        const cleanup = () => {
            ghost?.remove();
            ghost = null;
            setHover(null);
            source.classList.remove("is-lifted");
            document.body.classList.remove("drag-interaction-active");
            runtime.classList.remove("is-dragging");
            window.removeEventListener("pointermove", move);
            window.removeEventListener("pointerup", end);
            window.removeEventListener("pointercancel", end);
        };

        function move(event) {
            if (pointerId !== event.pointerId) return;
            if (!moved) {
                if (Math.abs(event.clientX - startX) + Math.abs(event.clientY - startY) < 10) return;
                moved = true;
                if (useGhost) {
                    const rect = source.getBoundingClientRect();
                    ghost = source.cloneNode(true);
                    ghost.classList.remove("is-hover", "is-lifted", "is-entering", "is-bounce");
                    ghost.classList.add("lw-ghost");
                    ghost.setAttribute("aria-hidden", "true");
                    ghost.removeAttribute("data-answer-value");
                    ghost.style.width = `${rect.width}px`;
                    ghost.style.height = `${rect.height}px`;
                    ghostHost().append(ghost);
                }
                source.classList.add("is-lifted");
                document.body.classList.add("drag-interaction-active");
                runtime.classList.add("is-dragging");
                onStart?.();
            }
            if (event.cancelable) event.preventDefault();
            if (ghost) {
                ghost.style.left = `${event.clientX}px`;
                ghost.style.top = `${event.clientY}px`;
            }
            const target = hitTarget(event.clientX, event.clientY, targets());
            setHover(target);
            onMove?.(event.clientX, event.clientY, target);
        }

        function end(event) {
            if (pointerId !== event.pointerId) return;
            pointerId = null;
            const wasDragging = moved;
            moved = false;
            const target = wasDragging && event.type === "pointerup" ? hitTarget(event.clientX, event.clientY, targets()) : null;
            cleanup();
            if (!wasDragging) return; // Chạm nhẹ: sự kiện click sẽ lo.
            suppressClickUntil = Date.now() + 450;
            onEnd?.();
            if (target && !hasSubmitted && !runtime.inert) onDrop?.(target);
        }

        source.addEventListener("pointerdown", (event) => {
            if (hasSubmitted || runtime.inert || pointerId !== null) return;
            if (event.pointerType === "mouse" && event.button !== 0) return;
            pointerId = event.pointerId;
            startX = event.clientX;
            startY = event.clientY;
            moved = false;
            try { source.setPointerCapture(pointerId); } catch { /* trình duyệt cũ: vẫn theo dõi trên window */ }
            window.addEventListener("pointermove", move, { passive: false });
            window.addEventListener("pointerup", end);
            window.addEventListener("pointercancel", end);
        });

        source.addEventListener("click", (event) => {
            if (Date.now() < suppressClickUntil) {
                event.preventDefault();
                return;
            }
            onTap?.(event);
        });
    };

    // ==========================================
    // Activity Renderers
    // ==========================================

    const appendEquationVisual = () => {
        if (payload.visualMode !== "equation" || !payload.equation) return;
        const equation = payload.equation;
        const board = document.createElement("div");
        board.className = "equation-visual clay-card";
        const group = (count) => {
            const box = document.createElement("div");
            box.className = "equation-object-group";
            appendRepeatedVisuals(box, equation.objectSymbol || "●", Number(count || 0));
            return box;
        };
        const operator = document.createElement("strong");
        operator.className = "equation-operator";
        operator.textContent = equation.operator || "+";
        const equals = document.createElement("strong");
        equals.className = "equation-operator";
        equals.textContent = "=";
        const unknown = document.createElement("span");
        unknown.className = "equation-unknown";
        unknown.textContent = "?";
        board.append(group(equation.leftCount), operator, group(equation.rightCount), equals, unknown);
        runtime.append(board);
    };

    // Hình mẫu của câu hỏi (ví dụ cái bóng cần đoán).
    const appendFocusVisual = () => {
        if (!payload.focusVisual) return;
        const focusVisual = document.createElement("div");
        focusVisual.className = "activity-focus-visual lw-focus";
        const focusValue = String(payload.focusVisual);
        // Kho bài mới: hình mẫu có thể là bóng đen ("shadow:🐘"), chỉ hình ("pic:..."), hình vẽ ("shape:...") hoặc emoji.
        const isBankVisual = /^(shadow|pic|shape):/.test(focusValue) || focusValue.startsWith("/") || resolveVisual(focusValue) || !/[\p{L}\p{N}]/u.test(focusValue);
        if (isBankVisual) {
            focusVisual.classList.add("is-bank-visual");
            renderVisual(focusVisual, resolveVisual(focusValue) || focusValue, "");
        } else {
            decorateButton(focusVisual, payload.focusVisual);
        }
        focusVisual.querySelector(".answer-label")?.remove();
        focusVisual.setAttribute("role", "img");
        focusVisual.setAttribute("aria-label", focusValue.startsWith("shadow:") ? "Cái bóng cần đoán" : "Hình mẫu");
        runtime.append(focusVisual);
    };

    // Lưới thẻ: số cột theo số đáp án (CSS đọc data-count), đáp án chữ dài được xếp ít cột hơn.
    const tileGrid = (className, values) => {
        const grid = el("div", `lw-grid ${className}`.trim());
        const list = Array.isArray(values) ? values : [];
        grid.dataset.count = String(list.length);
        if (list.some((value) => String(value ?? "").trim().length > 9)) grid.classList.add("has-long");
        if (list.length > 0 && list.every((value) => isSingleSymbol(value))) grid.classList.add("is-symbols");
        return grid;
    };

    const finishGrid = (grid) => {
        grid.classList.add(grid.querySelector(".has-answer-visual, .is-quantity-visual") ? "is-visual" : "is-text");
    };

    // Nút nghe to ngay trên bảng (bài nghe và chọn, bài kể chuyện): đọc lại câu hỏi qua bộ Voice chung.
    const listenButton = (label, glyph) => {
        const button = el("button", "lw-listen");
        button.type = "button";
        const art = el("span", "lw-listen-art");
        art.append(icon(glyph));
        const waves = el("span", "lw-listen-waves");
        waves.setAttribute("aria-hidden", "true");
        waves.append(el("i"), el("i"), el("i"));
        button.append(art, el("span", "lw-listen-label", label), waves);
        button.addEventListener("click", () => {
            flash(button, "is-bounce");
            playPromptAudio();
        });
        return button;
    };

    const renderChoice = (allowMultiple = false, goal = null) => {
        appendEquationVisual();
        appendFocusVisual();
        const selected = new Set();
        // Số đáp án cần chọn do máy chủ cung cấp (chỉ là số lượng, không tiết lộ đáp án).
        // Nhờ đó dạng "chọn nhiều đáp án" biết lúc nào bé đã trả lời xong để tự chấm.
        const expectedCount = Number(payload.expectedAnswerCount || 0);
        // Nếu không biết số đáp án cần chọn (dữ liệu cũ) thì không chấm ngay khi bé
        // vừa chọn một ô: phải chờ bé chọn xong, nếu không bé luôn bị sai.
        if (allowMultiple && expectedCount <= 0) settleTypes.add(type);

        // Chọn nhiều: mỗi chấm tròn trên thẻ nhiệm vụ là một lựa chọn cần chọn, sáng lên khi bé chọn.
        let counterDots = [];
        let dots = null;
        if (allowMultiple && expectedCount > 1) {
            dots = el("span", "lw-goal-dots");
            counterDots = Array.from({ length: expectedCount }, () => {
                const dot = el("i");
                dots.append(dot);
                return dot;
            });
        }
        const updateCounter = (count) => counterDots.forEach((dot, index) => dot.classList.toggle("is-on", index < count));

        const goalIcon = goal?.icon || (allowMultiple ? "checklist" : type === "listen_choose" ? "hearing" : "touch_app");
        const goalText = goal?.text || (allowMultiple
            ? (expectedCount > 1 ? `Chọn đủ ${expectedCount} đáp án` : "Chọn tất cả đáp án đúng")
            : type === "listen_choose" ? "Nghe rồi chạm đáp án đúng" : "Chạm vào đáp án đúng");
        runtime.append(goalPill(goalIcon, goalText, dots));

        const choices = payload.choices || [];
        const grid = tileGrid(`lw-choice-grid${allowMultiple ? " is-multi" : ""}${goal?.gridClass ? ` ${goal.gridClass}` : ""}`, choices);
        const tiles = [];
        const refresh = () => {
            tiles.forEach(({ tile, choice }) => tile.classList.toggle("is-selected", selected.has(choice)));
            grid.classList.toggle("has-pick", selected.size > 0);
        };

        choices.forEach((choice, index) => {
            const tile = makeTile(choice, allowMultiple ? "lw-tile--check" : "");
            tile.style.setProperty("--i", String(index));
            tile.style.setProperty("--selection-color", activityColors[index % activityColors.length]);
            tiles.push({ tile, choice });
            tile.addEventListener("click", () => {
                playAnswerAudio(choice);
                if (!allowMultiple) {
                    selected.clear();
                    selected.add(choice);
                    refresh();
                    flash(tile, "is-bounce");
                    setAnswer(choice, true, true, INSTANT_DELAY);
                    return;
                }
                if (selected.has(choice)) selected.delete(choice);
                else {
                    selected.add(choice);
                    flash(tile, "is-bounce");
                }
                if (expectedCount > 0 && selected.size > expectedCount) {
                    // Chọn quá số cần chọn: bỏ lựa chọn cũ nhất để bé không bị kẹt.
                    const oldest = [...selected].find((value) => value !== choice);
                    selected.delete(oldest);
                }
                refresh();
                updateCounter(selected.size);
                const isComplete = expectedCount > 0
                    ? selected.size === expectedCount
                    : selected.size > 0;
                setAnswer([...selected].sort().join("|"), isComplete);
            });
            grid.append(tile);
        });
        finishGrid(grid);
        runtime.append(grid);
    };

    const renderMultiSelect = () => renderChoice(true);

    // Kéo vào ô: một ô đích thật to; chạm vào hình là hình bay vào ô, hoặc kéo hình thả vào ô.
    const renderDragDrop = (options = {}) => {
        appendEquationVisual();
        appendFocusVisual();
        runtime.append(goalPill(options.goalIcon || "pan_tool_alt", options.goalText || "Kéo hình vào ô"));
        const choices = payload.choices || [];
        const stage = el("div", "lw-dnd");
        const source = tileGrid("lw-dnd-source", choices);

        const zone = el("button", "lw-drop");
        zone.type = "button";
        const zoneArt = el("span", "lw-drop-art");
        zoneArt.setAttribute("aria-hidden", "true");
        const targetText = String(payload.targetLabel || "").trim();
        const zoneVisual = resolveVisual(targetText);
        const zonePictogram = targetText ? resolvePictogram(targetText) : "";
        if (options.art) {
            zoneArt.append(el("span", "answer-emoji", options.art));
        } else if (zoneVisual) {
            renderVisual(zoneArt, zoneVisual, targetText);
        } else if (zonePictogram) {
            const image = el("img", "answer-pictogram");
            image.src = `${pictogramPath}${zonePictogram}`;
            image.alt = "";
            zoneArt.append(image);
        } else {
            zoneArt.append(icon("inventory_2", "lw-drop-icon"));
        }
        const zoneSlot = el("span", "lw-drop-slot");
        if (options.ghost) zoneSlot.append(el("span", "lw-drop-ghost", options.ghost));
        else zoneSlot.append(icon("south", "lw-drop-arrow"));
        zone.append(zoneArt, zoneSlot);
        if (options.word) {
            // Tô màu chữ đầu của từ để bé thấy chữ cần tìm nằm ở đâu trong từ.
            const [first, ...rest] = Array.from(options.word);
            const label = el("strong", "lw-drop-label lw-word");
            label.append(el("span", "lw-word-first", first), document.createTextNode(rest.join("")));
            zone.append(label);
        } else if (targetText) {
            zone.append(el("strong", "lw-drop-label", targetText));
        }
        zone.setAttribute("aria-label", targetText || "Ô thả hình");

        let current = null;
        const tiles = [];
        const put = (choice) => {
            current = choice;
            playAnswerAudio(choice);
            tiles.forEach(({ tile, value }) => tile.classList.toggle("is-used", value === choice));
            const item = el("span", "lw-drop-item");
            decorateButton(item, choice);
            zoneSlot.replaceChildren(item);
            zone.classList.add("is-filled");
            flash(zone, "is-gulp");
            setAnswer(choice, true, true);
        };

        choices.forEach((choice, index) => {
            const tile = makeTile(choice, "lw-tile--drag");
            tile.style.setProperty("--i", String(index));
            tiles.push({ tile, value: choice });
            enableDrag(tile, {
                targets: () => [zone],
                onStart: () => zone.classList.add("is-ready"),
                onEnd: () => zone.classList.remove("is-ready"),
                onDrop: () => put(choice),
                onTap: () => put(choice)
            });
            source.append(tile);
        });
        finishGrid(source);
        // Chạm vào ô đích: đọc lại hình đang nằm trong ô (nếu có).
        zone.addEventListener("click", () => {
            if (current !== null) playAnswerAudio(current);
            else flash(source, "is-nudge");
        });
        stage.append(source, zone);
        runtime.append(stage);
    };

    // Nối cặp: chạm một thẻ bên này rồi chạm thẻ hợp với nó bên kia (hoặc kéo dây nối từ thẻ này sang thẻ kia).
    // Dây nối màu được vẽ bằng SVG, tính lại khi đổi cỡ màn hình, xoay iPad hoặc ảnh tải xong.
    const renderMatching = () => {
        const pairs = payload.pairs || [];
        const mappings = {};
        let picked = null;
        let live = null;

        runtime.append(goalPill("link", "Nối hai hình hợp nhau"));

        const board = el("div", "lw-match");
        const isQuantityMatching = pairs.length > 0 && pairs.every((pair) =>
            /^\d+$/.test(String(pair.left || "").trim()) && getRepeatedPictureGroup(pair.right));
        if (isQuantityMatching) board.classList.add("is-quantity");
        const isLetterMatching = pairs.length > 0 && pairs.every((pair) =>
            isSingleSymbol(pair.left) && isSingleSymbol(pair.right));
        if (isLetterMatching) board.classList.add("is-letters");

        const NS = "http://www.w3.org/2000/svg";
        const lines = document.createElementNS(NS, "svg");
        lines.setAttribute("class", "lw-match-lines");
        lines.setAttribute("aria-hidden", "true");
        lines.setAttribute("preserveAspectRatio", "none");
        const leftColumn = el("div", "lw-match-col is-left");
        const rightColumn = el("div", "lw-match-col is-right");
        const leftTiles = new Map();
        const rightTiles = new Map();
        const leftColors = new Map();
        const tileValues = new Map();
        // Giá trị nối luôn là chuỗi (giống khóa đối tượng trước đây), chuỗi đáp án "trái=>phải" không đổi.
        const rights = shuffleForDisplay(pairs.map((pair) => String(pair.right ?? "")));

        const pegPoint = (tile) => {
            const peg = tile.querySelector(".lw-peg") || tile;
            const rect = peg.getBoundingClientRect();
            return { x: rect.left + rect.width / 2, y: rect.top + rect.height / 2 };
        };

        const addLine = (from, to, color, live) => {
            const dx = Math.max(24, Math.abs(to.x - from.x) * 0.45) * (to.x >= from.x ? 1 : -1);
            const d = `M ${from.x} ${from.y} C ${from.x + dx} ${from.y}, ${to.x - dx} ${to.y}, ${to.x} ${to.y}`;
            const under = document.createElementNS(NS, "path");
            under.setAttribute("d", d);
            under.setAttribute("class", "lw-line-under");
            const path = document.createElementNS(NS, "path");
            path.setAttribute("d", d);
            path.setAttribute("class", live ? "lw-line is-live" : "lw-line");
            path.style.stroke = color;
            const dot = document.createElementNS(NS, "circle");
            dot.setAttribute("cx", String(to.x));
            dot.setAttribute("cy", String(to.y));
            dot.setAttribute("r", live ? "11" : "8");
            dot.style.fill = color;
            lines.append(under, path, dot);
        };

        let frame = 0;
        const onResize = () => scheduleDraw();
        const draw = () => {
            frame = 0;
            if (!board.isConnected) {
                // Bảng đã được vẽ lại (bé làm lại): gỡ các bộ lắng nghe của bảng cũ.
                window.removeEventListener("resize", onResize);
                window.removeEventListener("orientationchange", onResize);
                resizeObserver?.disconnect();
                return;
            }
            const box = board.getBoundingClientRect();
            if (box.width === 0 || box.height === 0) return;
            lines.setAttribute("viewBox", `0 0 ${box.width} ${box.height}`);
            lines.replaceChildren();
            const local = (point) => ({ x: point.x - box.left, y: point.y - box.top });
            Object.entries(mappings).forEach(([left, right]) => {
                const leftTile = leftTiles.get(left);
                const rightTile = rightTiles.get(right);
                if (!leftTile || !rightTile) return;
                addLine(local(pegPoint(leftTile)), local(pegPoint(rightTile)), leftColors.get(left) || activityColors[0], false);
            });
            if (live) addLine(local(pegPoint(live.tile)), local({ x: live.x, y: live.y }), live.color, true);
        };
        function scheduleDraw() {
            if (!frame) frame = window.requestAnimationFrame(draw);
        }
        const resizeObserver = typeof ResizeObserver === "function" ? new ResizeObserver(scheduleDraw) : null;

        const refresh = () => {
            const owners = new Map(Object.entries(mappings).map(([left, right]) => [right, left]));
            leftTiles.forEach((tile, value) => {
                tile.classList.toggle("is-matched", Object.hasOwn(mappings, value));
                tile.classList.toggle("is-selected", picked?.tile === tile);
            });
            rightTiles.forEach((tile, value) => {
                const owner = owners.get(value);
                tile.classList.toggle("is-matched", owner !== undefined);
                if (owner !== undefined) tile.style.setProperty("--match-color", leftColors.get(owner) || activityColors[0]);
                else tile.style.removeProperty("--match-color");
                tile.classList.toggle("is-selected", picked?.tile === tile);
            });
            board.classList.toggle("is-picking", Boolean(picked));
            board.dataset.pickSide = picked?.side || "";
            scheduleDraw();
        };

        // spokenValue: thẻ bé vừa chạm/thả vào — đọc đúng thẻ đó.
        const connect = (leftValue, rightValue, spokenValue = rightValue) => {
            if (leftValue === undefined || rightValue === undefined) return;
            Object.keys(mappings).forEach((left) => {
                if (mappings[left] === rightValue && left !== leftValue) delete mappings[left];
            });
            mappings[leftValue] = rightValue;
            picked = null;
            playAnswerAudio(spokenValue);
            refresh();
            flash(leftTiles.get(leftValue), "is-bounce");
            flash(rightTiles.get(rightValue), "is-bounce");
            setAnswer(canonicalMappings(mappings), Object.keys(mappings).length === pairs.length);
        };

        const pick = (side, value, tile) => {
            if (picked && picked.side !== side) {
                connect(side === "left" ? value : picked.value, side === "right" ? value : picked.value, value);
                return;
            }
            if (picked?.tile === tile) {
                picked = null;
                refresh();
                return;
            }
            picked = { side, value, tile };
            playAnswerAudio(value);
            refresh();
        };

        const wire = (tile, side, value) => {
            const peg = el("span", "lw-peg");
            peg.setAttribute("aria-hidden", "true");
            tile.append(peg);
            const opposite = side === "left" ? rightTiles : leftTiles;
            enableDrag(tile, {
                ghost: false,
                targets: () => [...opposite.values()],
                onStart: () => {
                    picked = { side, value, tile };
                    playAnswerAudio(value);
                    refresh();
                },
                onMove: (x, y) => {
                    live = { tile, x, y, color: side === "left" ? leftColors.get(value) : "#ff7d4d" };
                    scheduleDraw();
                },
                onEnd: () => {
                    live = null;
                    scheduleDraw();
                },
                onDrop: (target) => {
                    const targetValue = tileValues.get(target);
                    if (targetValue === undefined) return;
                    if (side === "left") connect(value, targetValue, targetValue);
                    else connect(targetValue, value, targetValue);
                },
                onTap: () => pick(side, value, tile)
            });
        };

        pairs.forEach((pair, index) => {
            const left = String(pair.left ?? "");
            const tile = makeTile(left, "lw-tile--match");
            const color = activityColors[index % activityColors.length];
            tile.dataset.value = left;
            tile.style.setProperty("--option-color", color);
            tile.style.setProperty("--i", String(index));
            leftColors.set(left, color);
            leftTiles.set(left, tile);
            tileValues.set(tile, left);
            wire(tile, "left", left);
            leftColumn.append(tile);
        });

        rights.forEach((right, index) => {
            const isSoundText = /^(meo|gâu|cạp|chíp|ò ó|reng|cục)/i.test(right.trim());
            const tile = makeTile(right, "lw-tile--match", isSoundText ? "volume_up" : "");
            tile.dataset.value = right;
            tileValues.set(tile, right);
            tile.style.setProperty("--option-color", "#8f8aa8");
            tile.style.setProperty("--i", String(index + pairs.length));
            rightTiles.set(right, tile);
            wire(tile, "right", right);
            rightColumn.append(tile);
        });

        board.append(lines, leftColumn, el("div", "lw-match-gap"), rightColumn);
        runtime.append(board);
        resizeObserver?.observe(board);
        window.addEventListener("resize", onResize, { passive: true });
        window.addEventListener("orientationchange", onResize, { passive: true });
        // Ảnh tải xong hoặc thẻ vừa nhấc/hạ (hiệu ứng) làm dịch vị trí: vẽ lại dây nối.
        board.addEventListener("load", scheduleDraw, true);
        board.addEventListener("transitionend", scheduleDraw);
        board.addEventListener("animationend", scheduleDraw);
        scheduleDraw();
    };

    // Sắp xếp thứ tự: hàng ô số 1-2-3 ở trên, các thẻ ở dưới. Chạm thẻ là thẻ bay vào ô trống đầu tiên
    // (chạm lại thẻ đã xếp để trả về), hoặc kéo thẻ thả vào đúng ô (thả vào ô đã có thẻ thì đổi chỗ).
    // Đáp án vẫn là chuỗi "a|b|c" theo thứ tự ô.
    const renderOrdering = () => {
        const items = [...(payload.items || [])].reverse();
        const slots = new Array(items.length).fill(null);

        runtime.append(goalPill("format_list_numbered", "Xếp theo thứ tự"));

        const board = el("div", "lw-seq");
        board.dataset.count = String(items.length);
        const track = el("div", "lw-seq-track");
        track.setAttribute("aria-label", "Thứ tự bé đã xếp");
        const pool = el("div", "lw-seq-pool");
        const reset = el("button", "lw-mini-btn lw-seq-reset");
        reset.type = "button";
        reset.append(icon("restart_alt"), el("span", "", "Xếp lại"));

        const slotElements = items.map((_, slotIndex) => {
            const slot = el("div", "lw-seq-slot");
            slot.dataset.slot = String(slotIndex);
            const number = el("span", "lw-seq-num", String(slotIndex + 1));
            number.setAttribute("aria-hidden", "true");
            slot.append(number);
            track.append(slot);
            return slot;
        });

        const commit = () => {
            const done = slots.every((value) => value !== null);
            // Chỉ chấm khi đã xếp đủ; xếp dở thì hủy hẹn giờ chấm.
            setAnswer(slots.filter((value) => value !== null).map((index) => items[index]).join("|"), done);
        };

        let tiles = [];
        const draw = (bounceIndex = -1) => {
            const firstEmpty = slots.indexOf(null);
            slotElements.forEach((slot, slotIndex) => {
                const index = slots[slotIndex];
                const wanted = index === null ? null : tiles[index];
                slot.querySelectorAll(".lw-tile").forEach((node) => {
                    if (node !== wanted) node.remove();
                });
                if (wanted && wanted.parentNode !== slot) slot.append(wanted);
                slot.classList.toggle("is-filled", index !== null);
                slot.classList.toggle("is-next", slotIndex === firstEmpty);
            });
            tiles.forEach((tile, index) => {
                const isPlaced = slots.includes(index);
                tile.classList.toggle("is-placed", isPlaced);
                if (!isPlaced) pool.append(tile);
            });
            pool.classList.toggle("is-empty", firstEmpty === -1);
            reset.hidden = slots.every((value) => value === null);
            if (bounceIndex >= 0) flash(tiles[bounceIndex], "is-bounce");
        };

        const placeAt = (index, slotIndex) => {
            if (!Number.isInteger(slotIndex) || slotIndex < 0 || slotIndex >= slots.length) return;
            const from = slots.indexOf(index);
            if (from === slotIndex) return;
            const occupant = slots[slotIndex];
            // Kéo từ ô này sang ô khác: hai thẻ đổi chỗ. Kéo từ dưới lên ô đã có thẻ: thẻ cũ trở về hàng dưới.
            if (from >= 0) slots[from] = occupant;
            slots[slotIndex] = index;
            playAnswerAudio(items[index]);
            draw(index);
            commit();
        };

        const tapTile = (index) => {
            const from = slots.indexOf(index);
            if (from >= 0) {
                slots[from] = null;
                draw();
                commit();
                return;
            }
            const free = slots.indexOf(null);
            if (free < 0) return;
            slots[free] = index;
            playAnswerAudio(items[index]);
            draw(index);
            commit();
        };

        tiles = items.map((item, index) => {
            const tile = makeTile(item, "lw-tile--seq");
            tile.style.setProperty("--i", String(index));
            enableDrag(tile, {
                targets: () => (slots.includes(index) ? [...slotElements, pool] : slotElements),
                onDrop: (target) => {
                    if (target === pool) {
                        const from = slots.indexOf(index);
                        if (from >= 0) {
                            slots[from] = null;
                            draw();
                            commit();
                        }
                        return;
                    }
                    placeAt(index, Number(target.dataset.slot));
                },
                onTap: () => tapTile(index)
            });
            return tile;
        });

        reset.addEventListener("click", () => {
            slots.fill(null);
            draw();
            commit();
        });

        board.append(track, pool, reset);
        runtime.append(board);
        draw();
    };

    // Đếm: các hình xếp gọn như bảng mười ô; chạm từng hình để đánh số (hình nảy lên, cô đọc số), rồi chọn số.
    const renderCounting = () => {
        const total = Math.max(0, Number(payload.targetCount || 0));
        const symbol = payload.objectSymbol || "●";
        let counted = 0;

        runtime.append(goalPill("touch_app", "Chạm từng hình để đếm"));

        const scene = el("div", "lw-count");
        const field = el("div", "lw-count-field");
        field.style.setProperty("--cols", String(total <= 5 ? Math.max(1, total) : Math.min(5, Math.ceil(total / 2))));
        const tally = el("div", "lw-count-tally");
        const tallyNumber = el("strong", "lw-count-number", "0");
        tally.append(icon("front_hand"), tallyNumber);
        tally.setAttribute("aria-live", "polite");
        const again = el("button", "lw-mini-btn lw-count-reset");
        again.type = "button";
        again.append(icon("restart_alt"), el("span", "", "Đếm lại"));
        again.hidden = true;

        const objects = [];
        for (let index = 0; index < total; index += 1) {
            const object = el("button", "lw-count-obj");
            object.type = "button";
            object.style.setProperty("--i", String(index));
            object.setAttribute("aria-label", `Hình ${index + 1}`);
            const art = el("span", "lw-count-art");
            appendRepeatedVisuals(art, symbol, 1);
            const badge = el("span", "lw-count-badge");
            badge.setAttribute("aria-hidden", "true");
            object.append(art, badge);
            object.addEventListener("click", () => {
                if (object.dataset.n) {
                    // Đã đếm rồi: chỉ nhắc lại số của hình đó, không bỏ đếm (tránh bé chạm nhầm hai lần).
                    speak(object.dataset.n);
                    flash(object, "is-wiggle");
                    return;
                }
                counted += 1;
                object.dataset.n = String(counted);
                badge.textContent = String(counted);
                object.classList.add("is-counted");
                flash(object, "is-bounce");
                tallyNumber.textContent = String(counted);
                flash(tally, "is-bounce");
                again.hidden = false;
                speak(String(counted));
            });
            objects.push(object);
            field.append(object);
        }
        again.addEventListener("click", () => {
            counted = 0;
            objects.forEach((object) => {
                delete object.dataset.n;
                object.classList.remove("is-counted");
                object.querySelector(".lw-count-badge").textContent = "";
            });
            tallyNumber.textContent = "0";
            again.hidden = true;
        });

        const tools = el("div", "lw-count-tools");
        tools.append(tally, again);
        scene.append(field, tools);
        runtime.append(scene);

        appendCountChoices();
    };

    const appendCountChoices = () => {
        const choices = payload.choices || [];
        runtime.append(goalPill("123","Có tất cả mấy hình?"));
        const grid = tileGrid("lw-num-grid", choices);
        const tiles = [];
        choices.forEach((choice, index) => {
            const tile = makeTile(choice, "lw-tile--num");
            tile.style.setProperty("--i", String(index));
            tiles.push(tile);
            tile.addEventListener("click", () => {
                playAnswerAudio(choice);
                tiles.forEach((item) => item.classList.toggle("is-selected", item === tile));
                grid.classList.add("has-pick");
                flash(tile, "is-bounce");
                setAnswer(String(choice), true, true);
            });
            grid.append(tile);
        });
        finishGrid(grid);
        runtime.append(grid);
    };

    // Tạo số lượng: đĩa/khay ở giữa, nút − và + thật to hai bên, số đếm lớn. Mỗi lần thêm một hình nảy vào khay.
    const renderQuantityBuilder = () => {
        const maxItems = Math.max(1, Number(payload.maxItems || 20));
        const symbol = payload.objectSymbol || "●";
        let count = 0;

        runtime.append(goalPill("add_circle", "Thêm cho đủ số lượng"));

        const card = el("div", "lw-build");
        const flag = el("div", "lw-build-goal");
        flag.append(icon("flag"), el("strong", "", payload.targetLabel || `Mục tiêu: ${payload.targetCount ?? ""}`));

        const tray = el("div", "lw-build-tray is-empty");
        tray.setAttribute("aria-hidden", "true");
        const trayHint = icon("add", "lw-build-hint");

        const controls = el("div", "lw-build-controls");
        const minus = el("button", "lw-round lw-round--minus");
        minus.type = "button";
        minus.setAttribute("aria-label", "Bớt một");
        minus.append(icon("remove"));
        const plus = el("button", "lw-round lw-round--plus");
        plus.type = "button";
        plus.setAttribute("aria-label", "Thêm một");
        plus.append(icon("add"));
        const number = el("output", "lw-build-count", "0");
        number.setAttribute("aria-live", "polite");
        controls.append(minus, number, plus);

        const paint = () => {
            number.textContent = String(count);
            tray.classList.toggle("is-empty", count === 0);
            minus.classList.toggle("is-off", count === 0);
            plus.classList.toggle("is-off", count >= maxItems);
            flash(number, "is-bounce");
            setAnswer(String(count), count > 0);
        };

        plus.addEventListener("click", () => {
            if (count >= maxItems) {
                flash(plus, "is-wiggle");
                return;
            }
            count += 1;
            const object = el("span", "lw-build-obj");
            appendRepeatedVisuals(object, symbol, 1);
            tray.append(object);
            speak(String(count));
            paint();
        });

        minus.addEventListener("click", () => {
            if (count <= 0) {
                flash(minus, "is-wiggle");
                return;
            }
            count -= 1;
            const objects = tray.querySelectorAll(".lw-build-obj");
            objects[objects.length - 1]?.remove();
            speak(String(count));
            paint();
        });

        tray.append(trayHint);
        card.append(flag, tray, controls);
        runtime.append(card);
        minus.classList.add("is-off");
    };

    // So sánh: hai thẻ nhóm thật to đặt trên một chiếc bập bênh; chạm nhóm đúng (hoặc dấu "=" ở giữa).
    // Bập bênh nghiêng theo lựa chọn của bé (không theo đáp án) để bé thấy mình vừa chọn bên nào.
    const renderComparison = () => {
        const isShapeComparison = payload.visualMode === "shape";
        const mode = String(payload.comparisonMode || "").toLowerCase();
        const goalText = isShapeComparison
            ? (mode === "less" ? "Chạm vào hình nhỏ hơn" : mode === "more" ? "Chạm vào hình to hơn" : "So sánh hai hình")
            : (mode === "less" ? "Chạm vào nhóm ít hơn" : mode === "more" ? "Chạm vào nhóm nhiều hơn" : "So sánh hai nhóm");
        runtime.append(goalPill("balance", goalText));

        const board = el("div", `lw-cmp ${isShapeComparison ? "is-shape" : "is-quantity"}${play === "towers" ? " is-towers" : ""}`);
        const row = el("div", "lw-cmp-row");
        const seesaw = el("div", "lw-cmp-seesaw");
        seesaw.setAttribute("aria-hidden", "true");
        seesaw.append(el("span", "lw-cmp-plank"), el("span", "lw-cmp-pivot"));
        const choices = [];

        const tiltFor = (value) => {
            if (value === "equal") return "level";
            const other = value === "left" ? "right" : "left";
            return mode === "less" ? other : value;
        };

        const select = (button, value, spokenLabel) => {
            playAnswerAudio(spokenLabel);
            choices.forEach((item) => item.classList.toggle("is-selected", item === button));
            board.classList.add("has-pick");
            board.dataset.tilt = tiltFor(value);
            flash(button, "is-bounce");
            setAnswer(value, true, true);
        };

        const group = (label, count, value, side) => {
            const button = el("button", `lw-cmp-card is-${side}`);
            button.type = "button";
            button.dataset.answerValue = value;
            button.setAttribute("aria-label", label);
            const objects = el("span", "lw-cmp-objects");
            const amount = Number(count || 0);
            if (isShapeComparison) {
                const shape = el("span", "lw-cmp-shape", payload.objectSymbol || "●");
                const size = Math.max(52, Math.min(150, amount));
                shape.style.fontSize = `min(${size}px, ${Math.round(size / 5)}vw)`;
                shape.setAttribute("aria-label", payload.shapeName || label);
                objects.append(shape);
            } else {
                objects.style.setProperty("--cols", String(amount <= 3 ? Math.max(1, amount) : amount <= 6 ? 3 : 4));
                appendRepeatedVisuals(objects, payload.objectSymbol || "●", amount);
            }
            button.append(objects, el("span", "lw-cmp-label", label), icon("check", "lw-check"));
            button.addEventListener("click", () => select(button, value, label));
            choices.push(button);
            return button;
        };

        const leftCard = group(payload.leftLabel || "Nhóm A", payload.leftCount, "left", "left");
        const rightCard = group(payload.rightLabel || "Nhóm B", payload.rightCount, "right", "right");
        const middle = el("div", "lw-cmp-mid");
        // So sánh kích thước hình không có đáp án "bằng nhau"; so sánh số lượng thì có.
        if (!isShapeComparison) {
            const equalButton = el("button", "lw-cmp-equal");
            equalButton.type = "button";
            equalButton.dataset.answerValue = "equal";
            equalButton.setAttribute("aria-label", "Hai nhóm bằng nhau");
            equalButton.append(el("strong", "lw-cmp-equal-sign", "="), el("span", "lw-cmp-equal-label", "Bằng nhau"), icon("check", "lw-check"));
            equalButton.addEventListener("click", () => select(equalButton, "equal", "Bằng nhau"));
            choices.push(equalButton);
            middle.append(equalButton);
        } else {
            middle.append(el("span", "lw-cmp-vs", "?"));
        }
        row.append(leftCard, middle, rightCard);
        board.dataset.tilt = "level";
        board.append(row, seesaw);
        runtime.append(board);
    };

    // Phân loại: các hình nằm trên khay, bên dưới là các giỏ có hình đại diện. Chạm hình rồi chạm giỏ,
    // hoặc kéo hình thả vào giỏ. Chạm hình đã nằm trong giỏ để lấy ra.
    const renderClassification = () => {
        const mappings = payload.mappings || [];
        const categories = payload.categories || [];
        const answers = {};
        let selectedItem = null;

        runtime.append(goalPill("shopping_basket", "Xếp mỗi hình vào đúng giỏ"));

        const tray = tileGrid("lw-sort-tray", mappings.map((mapping) => mapping.left));
        const baskets = el("div", "lw-sort-baskets");
        baskets.dataset.count = String(categories.length);
        const tiles = new Map();
        const basketElements = new Map();
        const zoneCategories = new Map();

        const refresh = () => {
            tiles.forEach((tile, item) => {
                tile.hidden = Object.hasOwn(answers, item);
                tile.classList.toggle("is-selected", selectedItem === item);
            });
            tray.classList.toggle("is-empty", Object.keys(answers).length === mappings.length);
            basketElements.forEach(({ zone, inner }, category) => {
                zone.classList.toggle("is-ready", selectedItem !== null);
                inner.replaceChildren();
                mappings.forEach((mapping) => {
                    if (answers[mapping.left] !== category) return;
                    const chip = el("span", "lw-basket-chip");
                    chip.dataset.answerValue = String(mapping.left ?? "").trim();
                    chip.setAttribute("role", "button");
                    decorateButton(chip, mapping.left);
                    chip.addEventListener("click", (event) => {
                        if (selectedItem !== null) return; // Đang cầm hình khác: để giỏ nhận hình đó.
                        event.stopPropagation();
                        delete answers[mapping.left];
                        refresh();
                        setAnswer(canonicalMappings(answers), false);
                    });
                    inner.append(chip);
                });
                zone.classList.toggle("has-items", inner.childElementCount > 0);
            });
        };

        const assign = (item, category) => {
            if (item === null || item === undefined || category === undefined) return;
            playAnswerAudio(category);
            answers[item] = category;
            selectedItem = null;
            refresh();
            flash(basketElements.get(category)?.zone, "is-gulp");
            setAnswer(canonicalMappings(answers), Object.keys(answers).length === mappings.length);
        };

        mappings.forEach((mapping, index) => {
            const item = mapping.left;
            const tile = makeTile(item, "lw-tile--drag");
            tile.dataset.itemName = item;
            tile.style.setProperty("--i", String(index));
            tile.style.setProperty("--selection-color", activityColors[index % activityColors.length]);
            tiles.set(item, tile);
            enableDrag(tile, {
                targets: () => [...basketElements.values()].map(({ zone }) => zone),
                onStart: () => {
                    selectedItem = item;
                    refresh();
                },
                onDrop: (zone) => assign(item, zoneCategories.get(zone)),
                onTap: () => {
                    if (selectedItem === item) {
                        selectedItem = null;
                        refresh();
                        return;
                    }
                    selectedItem = item;
                    playAnswerAudio(item);
                    refresh();
                    flash(tile, "is-bounce");
                }
            });
            tray.append(tile);
        });
        finishGrid(tray);

        categories.forEach((category, categoryIndex) => {
            const zone = el("div", "lw-basket");
            zone.dataset.categoryName = category;
            zone.setAttribute("role", "button");
            zone.setAttribute("aria-label", `Giỏ ${category}`);
            zone.style.setProperty("--basket-color", activityColors[(categoryIndex + 2) % activityColors.length]);
            zone.style.setProperty("--i", String(categoryIndex));
            const head = el("div", "lw-basket-head");
            decorateButton(head, category);
            const inner = el("div", "lw-basket-items");
            const rim = el("span", "lw-basket-rim");
            rim.setAttribute("aria-hidden", "true");
            zone.append(head, inner, rim);
            zone.addEventListener("click", () => {
                if (selectedItem !== null) assign(selectedItem, category);
                else {
                    playAnswerAudio(category);
                    flash(tray, "is-nudge");
                }
            });
            basketElements.set(category, { zone, inner });
            zoneCategories.set(zone, category);
            baskets.append(zone);
        });

        runtime.append(tray, baskets);
        refresh();
    };

    // Kể chuyện: tranh minh họa được trình bày như một trang sách (CSS), nút nghe lại câu chuyện thật to.
    const renderStoryChoice = () => {
        if (payload.audioUrl || payload.speechText) {
            const storyContainer = el("div", "story-audio-container lw-listen-row");
            storyContainer.append(listenButton("Nghe lại câu chuyện", "auto_stories"));
            runtime.append(storyContainer);
        }
        renderChoice(false, { icon: "auto_stories", text: "Chọn câu trả lời đúng" });
    };

    // Thẻ hình mẫu: bé so hình dáng với mẫu, không cần biết đọc hay nhớ tên.
    const targetCard = () => {
        const card = el("div", "lw-target");
        card.setAttribute("role", "img");
        card.setAttribute("aria-label", `Hình mẫu ${playTarget}`);
        card.append(icon("search", "lw-target-icon"), el("strong", "lw-target-glyph", playTarget));
        return card;
    };

    // Bắt bóng: chọc hết các quả bóng giống mẫu. Chạm nhầm thì báo để bé thử lại.
    const renderHunt = () => {
        const others = (payload.choices || []).filter((choice) => choice !== playTarget);
        const tokens = shuffleForDisplay([playTarget, playTarget, playTarget, ...others, ...others].slice(0, 8));
        let remaining = tokens.filter((token) => token === playTarget).length;
        runtime.append(goalPill("toys", "Chọc hết bóng giống mẫu"), targetCard());
        const field = tileGrid("lw-hunt", tokens);
        tokens.forEach((token, index) => {
            const tile = makeTile(token, "lw-tile--balloon");
            tile.style.setProperty("--i", String(index));
            tile.addEventListener("click", () => {
                playAnswerAudio(token);
                if (token !== playTarget) {
                    notifyWrong(tile);
                    return;
                }
                clearWrongNotice();
                tile.disabled = true;
                tile.classList.add("is-popped");
                remaining -= 1;
                if (remaining === 0) setAnswer(playTarget, true, true);
            });
            field.append(tile);
        });
        finishGrid(field);
        runtime.append(field);
    };

    // Qua suối: mỗi lượt chọn hòn đá giống mẫu để bạn thỏ nhảy tới, đủ lượt là tới đích.
    const renderStones = () => {
        const others = (payload.choices || []).filter((choice) => choice !== playTarget);
        const rounds = 3;
        let position = 0;
        runtime.append(goalPill("directions_walk", "Chạm hòn đá giống mẫu"), targetCard());
        const path = el("div", "lw-path");
        path.style.setProperty("--stops", String(rounds));
        path.append(el("span", "lw-path-goal", "🥕"));
        const walker = el("span", "lw-path-walker", "🐰");
        path.append(walker);
        const stones = el("div", "lw-stones");
        const drawRound = () => {
            walker.style.setProperty("--pos", String(position));
            stones.replaceChildren();
            const distractors = shuffleForDisplay(others).slice(0, 2);
            shuffleForDisplay([playTarget, ...distractors]).forEach((value, index) => {
                const stone = makeTile(value, "lw-tile--stone");
                stone.style.setProperty("--i", String(index));
                stone.addEventListener("click", () => {
                    playAnswerAudio(value);
                    if (value !== playTarget) {
                        notifyWrong(stone);
                        return;
                    }
                    clearWrongNotice();
                    position += 1;
                    if (position >= rounds) {
                        walker.style.setProperty("--pos", String(position));
                        stones.replaceChildren();
                        setAnswer(playTarget, true, true);
                        return;
                    }
                    drawRound();
                });
                stones.append(stone);
            });
        };
        runtime.append(path, stones);
        drawRound();
    };

    const FEED_FRIENDS = ["🐻", "🐰", "🐵", "🐸", "🐶", "🐱"];

    const renderSingle = () => {
        if (type === "listen_choose") {
            const row = el("div", "lw-listen-row");
            row.append(listenButton("Nghe lại", "hearing"));
            runtime.append(row);
        }
        if (SINGLE_PLAYS[play]) {
            SINGLE_PLAYS[play]();
            return;
        }
        if (playTarget) runtime.append(targetCard());
        if (play === "feed") {
            renderDragDrop({
                goalText: playTarget ? "Kéo thẻ giống mẫu cho bạn nhỏ" : "Kéo đáp án đúng cho bạn nhỏ",
                goalIcon: "pets",
                art: FEED_FRIENDS[Math.floor(Math.random() * FEED_FRIENDS.length)]
            });
            return;
        }
        renderChoice(false, CHOICE_SKINS[play] || null);
    };

    // Lật tranh: mỗi lần chọn đúng thẻ giống mẫu, một mảnh che được mở ra; đủ bốn mảnh là thấy cả bức tranh.
    const REVEAL_PICTURES = ["🦁", "🐼", "🦄", "🐙", "🦋", "🚀", "🐳", "🦜"];
    const renderReveal = () => {
        const others = (payload.choices || []).filter((choice) => choice !== playTarget);
        const panels = [];
        let opened = 0;
        runtime.append(goalPill("image_search", "Chọn thẻ giống mẫu để mở tranh"), targetCard());
        const picture = el("div", "lw-reveal");
        picture.append(el("span", "lw-reveal-art", REVEAL_PICTURES[Math.floor(Math.random() * REVEAL_PICTURES.length)]));
        const cover = el("div", "lw-reveal-cover");
        for (let index = 0; index < 4; index += 1) {
            const panel = el("span", "lw-reveal-panel", "?");
            panels.push(panel);
            cover.append(panel);
        }
        picture.append(cover);
        const row = el("div", "lw-stones");
        const draw = () => {
            row.replaceChildren();
            shuffleForDisplay([playTarget, ...shuffleForDisplay(others).slice(0, 2)]).forEach((value, index) => {
                const tile = makeTile(value, "lw-tile--stone");
                tile.style.setProperty("--i", String(index));
                tile.addEventListener("click", () => {
                    playAnswerAudio(value);
                    if (value !== playTarget) {
                        notifyWrong(tile);
                        return;
                    }
                    clearWrongNotice();
                    panels[opened].classList.add("is-open");
                    opened += 1;
                    if (opened >= panels.length) {
                        row.replaceChildren();
                        setAnswer(playTarget, true, true);
                        return;
                    }
                    draw();
                });
                row.append(tile);
            });
        };
        runtime.append(picture, row);
        draw();
    };

    // Thỏ chui hang: các bạn thỏ mang thẻ thò lên khỏi hang, thẻ giống mẫu luôn có mặt nên bé không phải thi tốc độ.
    const renderBurrow = () => {
        const others = (payload.choices || []).filter((choice) => choice !== playTarget);
        const need = 3;
        let caught = 0;
        let timer = 0;
        const dots = el("span", "lw-goal-dots");
        const dotItems = Array.from({ length: need }, () => {
            const dot = el("i");
            dots.append(dot);
            return dot;
        });
        runtime.append(goalPill("cruelty_free", "Chạm bạn thỏ cầm thẻ giống mẫu", dots), targetCard());
        const field = el("div", "lw-burrows");
        const holes = Array.from({ length: 6 }, () => {
            const hole = el("div", "lw-burrow");
            field.append(hole);
            return hole;
        });
        const show = () => {
            window.clearTimeout(timer);
            if (!field.isConnected || hasSubmitted) return;
            holes.forEach((hole) => hole.replaceChildren());
            const spots = shuffleForDisplay(holes).slice(0, 2);
            const values = shuffleForDisplay([playTarget, shuffleForDisplay(others)[0]]);
            values.forEach((value, index) => {
                const tile = makeTile(value, "lw-tile--burrow");
                tile.addEventListener("click", () => {
                    playAnswerAudio(value);
                    if (value !== playTarget) {
                        notifyWrong(tile);
                        return;
                    }
                    clearWrongNotice();
                    dotItems[caught]?.classList.add("is-on");
                    caught += 1;
                    window.clearTimeout(timer);
                    tile.disabled = true;
                    tile.classList.add("is-popped");
                    if (caught >= need) setAnswer(playTarget, true, true);
                    else timer = window.setTimeout(show, 450);
                });
                spots[index].append(tile);
            });
            timer = window.setTimeout(show, 3600);
        };
        runtime.append(field);
        show();
    };

    // Cách chơi có bảng riêng và cách chơi chỉ đổi hình thức của bảng chọn đáp án.
    const SINGLE_PLAYS = {
        word: () => renderDragDrop({ goalText: "Kéo chữ giống chữ có màu vào ô", goalIcon: "image", art: payload.wordPicture, ghost: playTarget, word: payload.wordText }),
        hunt: () => renderHunt(),
        stones: () => renderStones(),
        reveal: () => renderReveal(),
        burrow: () => renderBurrow()
    };
    const CHOICE_SKINS = {
        float: { icon: "toys", text: "Chạm vào quả bóng đúng", gridClass: "lw-float" },
        tree: { icon: "park", text: "Hái quả đúng trên cây", gridClass: "lw-tree" },
        pond: { icon: "phishing", text: "Câu chú cá mang đáp án đúng", gridClass: "lw-pond" },
        race: { icon: "sports_score", text: "Chọn xe mang đáp án đúng", gridClass: "lw-race" }
    };

    // Bỏ vào giỏ: chạm hoặc kéo từng thẻ đúng vào giỏ; chạm thẻ trong giỏ để lấy ra. Đáp án vẫn như chọn nhiều.
    const renderBasket = () => {
        const expected = Number(payload.expectedAnswerCount || 0);
        const choices = payload.choices || [];
        const picked = new Set();
        const dots = el("span", "lw-goal-dots");
        const dotItems = Array.from({ length: expected }, () => {
            const dot = el("i");
            dots.append(dot);
            return dot;
        });
        runtime.append(goalPill("shopping_basket", `Bỏ đủ ${expected} thẻ vào giỏ`, dots));
        const tray = tileGrid("lw-sort-tray", choices);
        const zone = el("div", "lw-basket lw-basket--single");
        zone.setAttribute("role", "button");
        zone.setAttribute("aria-label", "Giỏ của bé");
        zone.style.setProperty("--basket-color", activityColors[3]);
        const inner = el("div", "lw-basket-items");
        const rim = el("span", "lw-basket-rim");
        rim.setAttribute("aria-hidden", "true");
        zone.append(el("div", "lw-basket-head", "🧺"), inner, rim);
        const tiles = new Map();
        const refresh = () => {
            tiles.forEach((tile, choice) => { tile.hidden = picked.has(choice); });
            inner.replaceChildren();
            picked.forEach((choice) => {
                const chip = el("span", "lw-basket-chip");
                chip.setAttribute("role", "button");
                decorateButton(chip, choice);
                chip.addEventListener("click", (event) => {
                    event.stopPropagation();
                    picked.delete(choice);
                    refresh();
                });
                inner.append(chip);
            });
            zone.classList.toggle("has-items", picked.size > 0);
            dotItems.forEach((dot, index) => dot.classList.toggle("is-on", index < picked.size));
            setAnswer([...picked].sort().join("|"), picked.size === expected);
        };
        const put = (choice) => {
            playAnswerAudio(choice);
            picked.add(choice);
            flash(zone, "is-gulp");
            refresh();
        };
        choices.forEach((choice, index) => {
            const tile = makeTile(choice, "lw-tile--drag");
            tile.style.setProperty("--i", String(index));
            tiles.set(choice, tile);
            enableDrag(tile, { targets: () => [zone], onDrop: () => put(choice), onTap: () => put(choice) });
            tray.append(tile);
        });
        finishGrid(tray);
        runtime.append(tray, zone);
    };

    // Lật thẻ: thẻ úp vẫn thấy mờ mờ nội dung để bé dễ hình dung và làm quen. Lật nhầm thì báo rồi úp lại.
    const renderMemory = () => {
        const pairs = payload.pairs || [];
        const mappings = {};
        const deck = shuffleForDisplay(pairs.flatMap((pair, index) => [
            { index, label: String(pair.left ?? "") },
            { index, label: String(pair.right ?? "") }
        ]));
        runtime.append(goalPill("flip", "Lật thẻ tìm đôi bạn"));
        const grid = tileGrid("lw-memory", deck.map((card) => card.label));
        let open = [];
        let busy = false;
        deck.forEach((card, position) => {
            const tile = makeTile(card.label, "lw-tile--flip");
            tile.classList.add("is-covered");
            tile.style.setProperty("--i", String(position));
            tile.addEventListener("click", () => {
                if (busy || !tile.classList.contains("is-covered")) return;
                clearWrongNotice();
                tile.classList.remove("is-covered");
                flash(tile, "is-flipping");
                playAnswerAudio(card.label);
                open.push({ tile, card });
                if (open.length < 2) return;
                const [first, second] = open;
                open = [];
                if (first.card.index === second.card.index) {
                    first.tile.classList.add("is-found");
                    second.tile.classList.add("is-found");
                    const pair = pairs[first.card.index];
                    mappings[String(pair.left ?? "")] = String(pair.right ?? "");
                    setAnswer(canonicalMappings(mappings), Object.keys(mappings).length === pairs.length);
                    return;
                }
                busy = true;
                notifyWrong(first.tile, second.tile);
                window.setTimeout(() => {
                    first.tile.classList.add("is-covered");
                    second.tile.classList.add("is-covered");
                    flash(first.tile, "is-flipping");
                    flash(second.tile, "is-flipping");
                    busy = false;
                }, reduceMotion() ? 700 : 1100);
            });
            grid.append(tile);
        });
        finishGrid(grid);
        runtime.append(grid);
    };

    // Leo cầu thang: chạm đúng thẻ tiếp theo thì bạn nhỏ leo lên một bậc. Chạm nhầm thì báo để bé thử lại.
    const renderStairs = () => {
        let next = 0;
        runtime.append(goalPill("stairs", "Chạm lần lượt để leo cầu thang"));
        const stairs = el("div", "lw-stairs");
        stairs.dataset.count = String(orderedItems.length);
        const steps = orderedItems.map((_, index) => {
            const step = el("div", "lw-stair");
            step.style.setProperty("--step", String(index + 1));
            stairs.append(step);
            return step;
        });
        const climber = el("span", "lw-climber", "🐿️");
        steps[0]?.append(climber);
        const pool = tileGrid("lw-seq-pool lw-stairs-pool", payload.items || []);
        (payload.items || []).forEach((item, index) => {
            const tile = makeTile(item, "lw-tile--seq");
            tile.style.setProperty("--i", String(index));
            tile.addEventListener("click", () => {
                playAnswerAudio(item);
                if (item !== orderedItems[next]) {
                    notifyWrong(tile);
                    return;
                }
                clearWrongNotice();
                tile.hidden = true;
                const chip = el("span", "lw-stair-item");
                decorateButton(chip, item);
                steps[next].prepend(chip);
                steps[next].classList.add("is-done");
                next += 1;
                if (next < steps.length) steps[next].append(climber);
                else setAnswer(orderedItems.join("|"), true, true);
            });
            pool.append(tile);
        });
        finishGrid(pool);
        runtime.append(stairs, pool);
    };

    // Nhặt đồ: chạm từng món trên bãi cỏ để bỏ vào giỏ, chạm món trong giỏ để trả lại, đủ thì cô tự chấm.
    const renderGather = () => {
        const maxItems = Math.max(1, Number(payload.maxItems || 10));
        const symbol = payload.objectSymbol || "●";
        let count = 0;
        runtime.append(goalPill("touch_app", "Chạm từng món để nhặt vào giỏ"));
        const card = el("div", "lw-build");
        const flag = el("div", "lw-build-goal");
        flag.append(icon("flag"), el("strong", "", payload.targetLabel || `Mục tiêu: ${payload.targetCount ?? ""}`));
        const meadow = el("div", "lw-meadow");
        const tray = el("div", "lw-build-tray is-empty");
        const number = el("output", "lw-build-count", "0");
        number.setAttribute("aria-live", "polite");
        for (let index = 0; index < maxItems; index += 1) {
            const object = el("button", "lw-gather-obj");
            object.type = "button";
            object.setAttribute("aria-label", `Món ${index + 1}`);
            object.style.setProperty("--i", String(index));
            appendRepeatedVisuals(object, symbol, 1);
            object.addEventListener("click", () => {
                const inTray = object.parentNode === tray;
                (inTray ? meadow : tray).append(object);
                count += inTray ? -1 : 1;
                number.textContent = String(count);
                tray.classList.toggle("is-empty", count === 0);
                flash(number, "is-bounce");
                speak(String(count));
                setAnswer(String(count), count > 0);
            });
            meadow.append(object);
        }
        card.append(flag, meadow, tray, number);
        runtime.append(card);
    };

    // Về đúng nhà: mỗi ô có sẵn một thẻ bên trái, bé đưa thẻ hợp với nó vào ô (chạm rồi chạm ô, hoặc kéo thả).
    const renderHomes = () => {
        const pairs = payload.pairs || [];
        const mappings = {};
        const rights = shuffleForDisplay(pairs.map((pair) => String(pair.right ?? "")));
        let picked = null;
        runtime.append(goalPill("home_pin", "Đưa mỗi thẻ về đúng nhà"));
        const homes = el("div", "lw-homes");
        const pool = tileGrid("lw-sort-tray", rights);
        const slots = new Map();
        const tiles = [];
        const refresh = () => {
            tiles.forEach(({ tile }) => tile.classList.toggle("is-selected", picked === tile));
            homes.classList.toggle("is-ready", picked !== null);
            setAnswer(canonicalMappings(mappings), Object.keys(mappings).length === pairs.length);
        };
        const empty = (slot) => {
            const left = slot.dataset.left;
            const holder = tiles.find(({ tile }) => tile.dataset.home === left);
            if (holder) {
                holder.tile.hidden = false;
                delete holder.tile.dataset.home;
            }
            delete mappings[left];
            slot.replaceChildren();
            slot.classList.remove("has-item");
        };
        const place = (tile, right, slot) => {
            if (slot.classList.contains("has-item")) empty(slot);
            const left = slot.dataset.left;
            mappings[left] = right;
            tile.dataset.home = left;
            tile.hidden = true;
            const chip = el("span", "lw-home-chip");
            decorateButton(chip, right);
            slot.append(chip);
            slot.classList.add("has-item");
            picked = null;
            playAnswerAudio(right);
            flash(slot, "is-gulp");
            refresh();
        };
        pairs.forEach((pair) => {
            const left = String(pair.left ?? "");
            const row = el("div", "lw-home");
            const owner = el("div", "lw-home-owner");
            decorateButton(owner, left);
            owner.addEventListener("click", () => playAnswerAudio(left));
            const slot = el("button", "lw-home-slot");
            slot.type = "button";
            slot.dataset.left = left;
            slot.setAttribute("aria-label", `Ô của ${left}`);
            slot.addEventListener("click", () => {
                if (picked) place(picked, picked.dataset.right, slot);
                else if (slot.classList.contains("has-item")) {
                    empty(slot);
                    refresh();
                }
            });
            slots.set(left, slot);
            row.append(owner, icon("arrow_forward", "lw-home-arrow"), slot);
            homes.append(row);
        });
        rights.forEach((right, index) => {
            const tile = makeTile(right, "lw-tile--drag");
            tile.dataset.right = right;
            tile.style.setProperty("--i", String(index));
            tiles.push({ tile, right });
            enableDrag(tile, {
                targets: () => [...slots.values()],
                onDrop: (slot) => place(tile, right, slot),
                onTap: () => {
                    picked = picked === tile ? null : tile;
                    playAnswerAudio(right);
                    refresh();
                }
            });
            pool.append(tile);
        });
        finishGrid(pool);
        runtime.append(homes, pool);
    };

    // Nối toa tàu: chạm đúng thẻ tiếp theo thì toa đó nối vào đoàn tàu. Chạm nhầm thì báo để bé thử lại.
    const renderCarriages = () => {
        let next = 0;
        runtime.append(goalPill("train", "Chạm lần lượt để nối toa tàu"));
        const train = el("div", "lw-carriages");
        train.append(el("span", "lw-carriage-engine", "🚂"));
        const cars = orderedItems.map(() => {
            const car = el("div", "lw-carriage");
            train.append(car);
            return car;
        });
        const pool = tileGrid("lw-seq-pool", payload.items || []);
        (payload.items || []).forEach((item, index) => {
            const tile = makeTile(item, "lw-tile--seq");
            tile.style.setProperty("--i", String(index));
            tile.addEventListener("click", () => {
                playAnswerAudio(item);
                if (item !== orderedItems[next]) {
                    notifyWrong(tile);
                    return;
                }
                clearWrongNotice();
                tile.hidden = true;
                const chip = el("span", "lw-stair-item");
                decorateButton(chip, item);
                cars[next].append(chip);
                cars[next].classList.add("is-done");
                flash(cars[next], "is-gulp");
                next += 1;
                if (next === orderedItems.length) setAnswer(orderedItems.join("|"), true, true);
            });
            pool.append(tile);
        });
        finishGrid(pool);
        runtime.append(train, pool);
    };

    // Đoàn tàu thiếu toa: cả dãy hiện sẵn, chạm đáp án là đáp án vào toa trống đầu tiên, chạm lại toa vừa điền để đổi.
    // Điền đủ toa thì gửi lên chấm như mọi bài sắp xếp, nên chọn sai vẫn được tính là một lần làm sai.
    const renderGapTrain = () => {
        const row = (payload.orderRow || []).map((value) => String(value ?? "").trim());
        const options = shuffleForDisplay([...orderedItems, ...(payload.orderDecoys || [])]);
        const blanks = [];
        const filled = [];
        runtime.append(goalPill("train", "Chọn đáp án cho toa trống"));
        const train = el("div", "lw-carriages lw-gaptrain");
        train.append(el("span", "lw-carriage-engine", "🚂"));
        const pool = tileGrid("lw-seq-pool", options);
        const draw = () => {
            const next = filled.indexOf(null);
            blanks.forEach((car, index) => {
                car.textContent = filled[index]?.value ?? "";
                car.classList.toggle("is-done", filled[index] !== null);
                car.classList.toggle("is-next", index === next);
            });
        };
        row.forEach((value) => {
            const car = el("button", "lw-carriage", value);
            car.type = "button";
            if (value) {
                car.classList.add("is-given");
                car.addEventListener("click", () => playAnswerAudio(value));
            } else {
                const index = blanks.length;
                car.setAttribute("aria-label", `Toa trống ${index + 1}`);
                car.addEventListener("click", () => {
                    if (filled[index] === null) return;
                    filled[index].tile.hidden = false;
                    filled[index] = null;
                    draw();
                    setAnswer("", false);
                });
                blanks.push(car);
                filled.push(null);
            }
            train.append(car);
        });
        options.forEach((option, index) => {
            const value = String(option);
            const tile = makeTile(value, "lw-tile--seq");
            tile.style.setProperty("--i", String(index));
            tile.addEventListener("click", () => {
                const free = filled.indexOf(null);
                if (free < 0) return;
                filled[free] = { value, tile };
                tile.hidden = true;
                playAnswerAudio(value);
                draw();
                flash(blanks[free], "is-gulp");
                if (filled.every(Boolean)) setAnswer(filled.map((entry) => entry.value).join("|"), true, true, 500);
            });
            pool.append(tile);
        });
        finishGrid(pool);
        draw();
        runtime.append(train, pool);
    };

    // Bảng mười ô: các hình xếp sẵn trong hai hàng năm ô để bé nhìn là thấy số lượng, rồi chọn số.
    const renderFrame = () => {
        const total = Math.max(0, Number(payload.targetCount || 0));
        const symbol = payload.objectSymbol || "●";
        runtime.append(goalPill("grid_view", "Nhìn bảng mười ô rồi chọn số"));
        const frame = el("div", "lw-frame");
        for (let index = 0; index < 10; index += 1) {
            const cell = el("span", "lw-frame-cell");
            if (index < total) appendRepeatedVisuals(cell, symbol, 1);
            frame.append(cell);
        }
        runtime.append(frame);
        appendCountChoices();
    };

    // Xếp vào ô: chạm từng ô trống để đặt một hình, chạm lại để bỏ ra; đủ thì cô tự chấm.
    const renderCells = () => {
        const maxItems = Math.max(1, Number(payload.maxItems || 10));
        const symbol = payload.objectSymbol || "●";
        let count = 0;
        runtime.append(goalPill("grid_on", "Chạm từng ô để đặt hình vào"));
        const card = el("div", "lw-build");
        const flag = el("div", "lw-build-goal");
        flag.append(icon("flag"), el("strong", "", payload.targetLabel || `Mục tiêu: ${payload.targetCount ?? ""}`));
        const grid = el("div", "lw-cells");
        const number = el("output", "lw-build-count", "0");
        number.setAttribute("aria-live", "polite");
        for (let index = 0; index < maxItems; index += 1) {
            const cell = el("button", "lw-cell");
            cell.type = "button";
            cell.setAttribute("aria-label", `Ô ${index + 1}`);
            cell.addEventListener("click", () => {
                const filled = cell.classList.toggle("is-filled");
                cell.replaceChildren();
                if (filled) appendRepeatedVisuals(cell, symbol, 1);
                count += filled ? 1 : -1;
                number.textContent = String(count);
                flash(number, "is-bounce");
                if (count > 0) speak(String(count));
                setAnswer(String(count), count > 0);
            });
            grid.append(cell);
        }
        card.append(flag, grid, number);
        runtime.append(card);
    };

    // Từng món một: mỗi lượt một món hiện thật to, bé chạm giỏ đúng hoặc kéo món vào giỏ.
    const renderOneByOne = () => {
        const queue = [...(payload.mappings || [])];
        const categories = payload.categories || [];
        const answers = {};
        runtime.append(goalPill("low_priority", "Đưa từng món vào đúng giỏ"));
        const stage = el("div", "lw-onebyone");
        const baskets = el("div", "lw-sort-baskets");
        baskets.dataset.count = String(categories.length);
        const zones = new Map();
        let current = null;
        const drawCurrent = () => {
            stage.replaceChildren();
            current = queue[0] || null;
            if (!current) {
                setAnswer(canonicalMappings(answers), true);
                return;
            }
            const item = current.left;
            const tile = makeTile(item, "lw-tile--drag lw-tile--hero");
            enableDrag(tile, {
                targets: () => [...zones.keys()],
                onDrop: (zone) => assign(zones.get(zone).category),
                onTap: () => playAnswerAudio(item)
            });
            stage.append(tile);
            playAnswerAudio(item);
        };
        const assign = (category) => {
            if (!current) return;
            const item = current.left;
            answers[item] = category;
            queue.shift();
            const entry = [...zones.values()].find((value) => value.category === category);
            const chip = el("span", "lw-basket-chip");
            decorateButton(chip, item);
            entry.inner.append(chip);
            entry.zone.classList.add("has-items");
            flash(entry.zone, "is-gulp");
            playAnswerAudio(category);
            window.setTimeout(drawCurrent, reduceMotion() ? 0 : 500);
            current = null;
            stage.replaceChildren();
        };
        categories.forEach((category, index) => {
            const zone = el("div", "lw-basket");
            zone.setAttribute("role", "button");
            zone.setAttribute("aria-label", `Giỏ ${category}`);
            zone.style.setProperty("--basket-color", activityColors[(index + 2) % activityColors.length]);
            const head = el("div", "lw-basket-head");
            decorateButton(head, category);
            const inner = el("div", "lw-basket-items");
            const rim = el("span", "lw-basket-rim");
            rim.setAttribute("aria-hidden", "true");
            zone.append(head, inner, rim);
            zone.addEventListener("click", () => assign(category));
            zones.set(zone, { category, zone, inner });
            baskets.append(zone);
        });
        runtime.append(stage, baskets);
        drawCurrent();
    };

    // Cách chơi vẽ bảng riêng, thay cho bảng mặc định của dạng bài.
    const PLAY_RENDERERS = {
        basket: () => renderBasket(),
        net: () => renderChoice(true, { icon: "flutter_dash", text: `Bắt đủ ${Number(payload.expectedAnswerCount || 0)} chú bướm đúng`, gridClass: "lw-net" }),
        memory: () => renderMemory(),
        homes: () => renderHomes(),
        stairs: () => renderStairs(),
        carriages: () => renderCarriages(),
        gaptrain: () => renderGapTrain(),
        frame: () => renderFrame(),
        gather: () => renderGather(),
        cells: () => renderCells(),
        onebyone: () => renderOneByOne()
    };

    const renderers = {
        single_choice: renderSingle,
        listen_choose: renderSingle,
        multi_select: renderMultiSelect,
        drag_drop: () => renderDragDrop(),
        matching: renderMatching,
        ordering: renderOrdering,
        counting: renderCounting,
        quantity_builder: renderQuantityBuilder,
        comparison: renderComparison,
        classification: renderClassification,
        story_choice: renderStoryChoice
    };
    // Cảnh minh họa phía trên bảng làm bài (payload "scene"): một hàng hình, "?" là ô bí ẩn cần tìm.
    const appendScene = () => {
        if (!Array.isArray(payload.scene) || payload.scene.length === 0) return;
        const scene = document.createElement("div");
        scene.className = "lw-scene-card";
        scene.setAttribute("aria-hidden", "true");
        payload.scene.forEach((token, index) => {
            const value = String(token ?? "").trim();
            let element;
            if (value.startsWith("/")) {
                element = document.createElement("img");
                element.src = value;
                element.alt = "";
                element.className = "lw-scene-img";
            } else {
                element = document.createElement("span");
                element.className = value === "?" ? "lw-scene-token is-mystery" : /^[\p{L}\p{N}]{1,3}$/u.test(value) ? "lw-scene-token is-glyph" : "lw-scene-token";
                element.textContent = value;
            }
            element.style.setProperty("--i", String(index));
            scene.append(element);
        });
        runtime.append(scene);
    };
    const renderActivity = () => {
        // Vẽ lại từ đầu theo payload (bé làm lại sau khi sai): dọn mọi "bóng" kéo thả còn sót.
        document.querySelectorAll(".lw-ghost").forEach((node) => node.remove());
        document.body.classList.remove("drag-interaction-active");
        runtime.classList.remove("is-dragging");
        runtime.classList.add("lw-act", `lw-act--${type}`);
        appendScene();
        (PLAY_RENDERERS[play] || renderers[type])?.();
    };
    renderActivity();
})();
