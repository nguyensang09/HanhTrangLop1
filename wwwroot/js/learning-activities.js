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

    if (Array.isArray(payload.choices)) payload.choices = shuffleForDisplay(payload.choices);
    if (Array.isArray(payload.items)) payload.items = shuffleForDisplay(payload.items);
    if (Array.isArray(payload.pairs)) payload.pairs = shuffleForDisplay(payload.pairs);
    if (Array.isArray(payload.mappings)) payload.mappings = shuffleForDisplay(payload.mappings);

    const type = runtime.dataset.activityType;

    // ==========================================
    // Cơ chế tự động kiểm tra đáp án dùng chung
    // ==========================================
    // Mọi dạng bài đều tự chấm khi bé trả lời xong, không cần thao tác xác nhận thêm.
    // Khác biệt duy nhất giữa các dạng là thời điểm được coi là "đã trả lời xong":
    //   - instant: chọn một đáp án là xong (chọn một, nghe và chọn, kéo thả, đếm, so sánh...)
    //   - complete: phải điền đủ toàn bộ cặp/nhóm (nối cặp, phân loại, chọn nhiều đáp án)
    //   - settle: bé có thể chỉnh nhiều lần, chấm sau khi dừng chỉnh (sắp xếp, tạo số lượng)
    const INSTANT_DELAY = 1000;
    const COMPLETE_DELAY = 900;
    const SETTLE_DELAY = 2600;
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
        const minDelay = 2200;
        runProgress(minDelay);
        await Promise.race([Promise.all([spoken, wait(minDelay)]), wait(6000)]);
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
        if (button.matches(".activity-option, .activity-drop-zone, .comparison-group")) {
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

    const renderChoice = (allowMultiple = false) => {
        appendEquationVisual();
        const selected = new Set();
        // Số đáp án cần chọn do máy chủ cung cấp (chỉ là số lượng, không tiết lộ đáp án).
        // Nhờ đó dạng "chọn nhiều đáp án" biết lúc nào bé đã trả lời xong để tự chấm.
        const expectedCount = Number(payload.expectedAnswerCount || 0);
        // Nếu không biết số đáp án cần chọn (dữ liệu cũ) thì không chấm ngay khi bé
        // vừa chọn một ô: phải chờ bé chọn xong, nếu không bé luôn bị sai.
        if (allowMultiple && expectedCount <= 0) settleTypes.add(type);
        // Chọn nhiều: cho bé thấy cần chọn mấy hình và đã chọn được mấy (mỗi chấm là một lựa chọn).
        let counterDots = [];
        if (allowMultiple && expectedCount > 1) {
            const counter = document.createElement("div");
            counter.className = "lw-pick-counter";
            const label = document.createElement("span");
            label.textContent = `Con chọn đủ ${expectedCount} đáp án nhé`;
            const dots = document.createElement("span");
            dots.className = "lw-pick-dots";
            counterDots = Array.from({ length: expectedCount }, () => {
                const dot = document.createElement("i");
                dots.append(dot);
                return dot;
            });
            counter.append(label, dots);
            runtime.append(counter);
        }
        const updateCounter = (count) => counterDots.forEach((dot, index) => dot.classList.toggle("is-on", index < count));
        const grid = document.createElement("div");
        grid.className = "activity-option-grid";
        const hasTextOptions = (payload.choices || []).some((choice) => String(choice ?? "").trim().length > 3);
        if (hasTextOptions) {
            grid.classList.add("grid-text-options");
        }
        if (payload.focusVisual) {
            const focusVisual = document.createElement("div");
            focusVisual.className = "activity-focus-visual";
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
        }
        (payload.choices || []).forEach((choice, index) => {
            const button = createButton(choice);
            button.style.setProperty("--selection-color", activityColors[index % activityColors.length]);
            button.addEventListener("click", () => {
                if (!allowMultiple) {
                    selected.clear();
                    grid.querySelectorAll("button").forEach((item) => item.classList.remove("selected"));
                }
                playAnswerAudio(choice);
                selected.has(choice) ? selected.delete(choice) : selected.add(choice);
                button.classList.toggle("selected", selected.has(choice));
                const answer = allowMultiple ? [...selected].sort().join("|") : [...selected][0] || "";
                if (!allowMultiple) {
                    setAnswer(answer, selected.size > 0, true, INSTANT_DELAY);
                    return;
                }
                if (expectedCount > 0 && selected.size > expectedCount) {
                    // Chọn quá số cần chọn: bỏ lựa chọn cũ nhất để bé không bị kẹt.
                    const oldest = [...selected].find((value) => value !== choice);
                    selected.delete(oldest);
                    grid.querySelectorAll("button").forEach((item) => item.classList.toggle("selected", selected.has(item.dataset.answerValue)));
                }
                updateCounter(selected.size);
                const isComplete = expectedCount > 0
                    ? selected.size === expectedCount
                    : selected.size > 0;
                setAnswer([...selected].sort().join("|"), isComplete);
            });
            grid.append(button);
        });
        if (grid.querySelector(".has-answer-visual")) {
            grid.classList.add("has-visual-options");
            grid.classList.remove("grid-text-options");
        }
        runtime.append(grid);
    };

    const renderMultiSelect = () => renderChoice(true);

    const renderDragDrop = () => {
        appendEquationVisual();
        const source = document.createElement("div");
        source.className = "activity-option-grid drag-source";
        const target = document.createElement("button");
        target.type = "button";
        target.className = "activity-drop-zone clay-card";
        const targetIcon = document.createElement("span");
        targetIcon.className = "material-symbols-outlined";
        targetIcon.textContent = "inbox";
        const targetVisual = resolveVisual(payload.targetLabel);
        if (targetVisual && !targetVisual.includes(":") && !targetVisual.startsWith("/")) {
            targetIcon.className = "answer-emoji drop-zone-emoji";
            targetIcon.textContent = targetVisual;
        }
        targetIcon.setAttribute("aria-hidden", "true");
        const targetLabel = document.createElement("strong");
        targetLabel.textContent = payload.targetLabel || "Vùng đích";
        target.append(targetIcon, targetLabel);
        let activeValue = "";
        let dragGhost = null;

        const dropValue = (value) => {
            if (!value) return;
            activeValue = value;
            playAnswerAudio(value);
            source.querySelectorAll("button").forEach((btn) => {
                btn.classList.toggle("selected", btn.dataset.answerValue === value.trim());
            });
            decorateButton(target, value);
            target.classList.add("filled");
            setAnswer(value, true, true, 1000);
        };
        const movePointerDrag = (event) => {
            if (!dragGhost) return;
            event.preventDefault();
            dragGhost.style.left = `${event.clientX}px`;
            dragGhost.style.top = `${event.clientY}px`;
            const dropTarget = document.elementFromPoint(event.clientX, event.clientY);
            target.classList.toggle("drop-hover", Boolean(dropTarget?.closest?.(".activity-drop-zone")));
        };
        const finishPointerDrag = (event) => {
            if (!dragGhost) return;
            event.preventDefault();
            dragGhost.remove();
            dragGhost = null;
            // Giải phóng khóa cuộn khi kéo thả xong
            document.body.classList.remove("drag-interaction-active");
            const dropTarget = document.elementFromPoint(event.clientX, event.clientY);
            target.classList.remove("drop-hover");
            if (dropTarget?.closest?.(".activity-drop-zone") && activeValue) {
                dropValue(activeValue);
            }
        };
        const startPointerDrag = (event, choice, button) => {
            if (event.pointerType === "mouse") return;
            event.preventDefault();
            activeValue = choice;
            source.querySelectorAll("button").forEach((item) => item.classList.remove("selected"));
            button.classList.add("selected");
            target.classList.add("ready");
            dragGhost = button.cloneNode(true);
            dragGhost.classList.add("drag-ghost");
            document.body.append(dragGhost);
            // Khóa cuộn trang khi đang kéo thả – tránh trang bị scroll giữa chừng
            document.body.classList.add("drag-interaction-active");
            button.setPointerCapture(event.pointerId);
            movePointerDrag(event);
        };
        (payload.choices || []).forEach((choice) => {
            const button = createButton(choice, "activity-option draggable-option clay-button");
            button.draggable = true;
            button.addEventListener("click", () => {
                playAnswerAudio(choice);
                activeValue = choice;
                source.querySelectorAll("button").forEach((item) => item.classList.remove("selected"));
                button.classList.add("selected");
                target.classList.add("ready");
            });
            button.addEventListener("dragstart", (event) => event.dataTransfer.setData("text/plain", choice));
            button.addEventListener("pointerdown", (event) => startPointerDrag(event, choice, button));
            button.addEventListener("pointermove", movePointerDrag);
            button.addEventListener("pointerup", finishPointerDrag);
            button.addEventListener("pointercancel", finishPointerDrag);
            source.append(button);
        });
        target.addEventListener("click", () => activeValue && dropValue(activeValue));
        target.addEventListener("dragover", (event) => event.preventDefault());
        target.addEventListener("drop", (event) => {
            event.preventDefault();
            dropValue(event.dataTransfer.getData("text/plain"));
        });
        runtime.append(source, target);
    };

    const renderMatching = () => {
        const pairs = payload.pairs || [];
        const mappings = {};
        let selectedLeft = "";
        let isDraggingLine = false;
        let dragSourceBtn = null;
        let currentPointerPos = null;

        const board = document.createElement("div");
        board.className = "matching-board";
        const isQuantityMatching = pairs.length > 0 && pairs.every((pair) =>
            /^\d+$/.test(String(pair.left || "").trim()) && getRepeatedPictureGroup(pair.right));
        if (isQuantityMatching) board.classList.add("matching-quantity-board");

        const isLetterMatching = pairs.length > 0 && pairs.every((pair) =>
            isSingleSymbol(pair.left) && isSingleSymbol(pair.right));
        if (isLetterMatching) board.classList.add("matching-letter-board");
        const lines = document.createElementNS("http://www.w3.org/2000/svg", "svg");
        lines.classList.add("matching-lines");
        lines.setAttribute("aria-hidden", "true");
        const leftColumn = document.createElement("div");
        leftColumn.className = "matching-column matching-left";
        const rightColumn = document.createElement("div");
        rightColumn.className = "matching-column matching-right";
        const rights = shuffleForDisplay(pairs.map((pair) => pair.right));

        const drawLines = () => {
            lines.replaceChildren();
            const boardRect = board.getBoundingClientRect();

            // 1. Draw established connections
            Object.entries(mappings).forEach(([left, right], index) => {
                const leftButton = [...leftColumn.children].find((item) => item.dataset.value === left);
                const rightButton = [...rightColumn.children].find((item) => item.dataset.value === right);
                if (!leftButton || !rightButton) return;
                const leftRect = leftButton.getBoundingClientRect();
                const rightRect = rightButton.getBoundingClientRect();
                const line = document.createElementNS("http://www.w3.org/2000/svg", "line");
                line.setAttribute("x1", String(leftRect.right - boardRect.left));
                line.setAttribute("y1", String(leftRect.top + leftRect.height / 2 - boardRect.top));
                line.setAttribute("x2", String(rightRect.left - boardRect.left));
                line.setAttribute("y2", String(rightRect.top + rightRect.height / 2 - boardRect.top));
                line.setAttribute("stroke", activityColors[index % activityColors.length]);
                line.setAttribute("stroke-width", "5");
                line.setAttribute("stroke-dasharray", "8,5");

                const start = document.createElementNS("http://www.w3.org/2000/svg", "circle");
                const end = document.createElementNS("http://www.w3.org/2000/svg", "circle");
                [[start, leftRect.right - boardRect.left, leftRect.top + leftRect.height / 2 - boardRect.top],
                    [end, rightRect.left - boardRect.left, rightRect.top + rightRect.height / 2 - boardRect.top]]
                    .forEach(([circle, x, y]) => {
                        circle.setAttribute("cx", String(x));
                        circle.setAttribute("cy", String(y));
                        circle.setAttribute("r", "8");
                        circle.setAttribute("fill", activityColors[index % activityColors.length]);
                    });
                lines.append(line, start, end);
            });

            // 2. Draw live dragging line following pointer
            if (isDraggingLine && dragSourceBtn && currentPointerPos) {
                const srcRect = dragSourceBtn.getBoundingClientRect();
                const startX = srcRect.right - boardRect.left;
                const startY = srcRect.top + srcRect.height / 2 - boardRect.top;
                const endX = currentPointerPos.x - boardRect.left;
                const endY = currentPointerPos.y - boardRect.top;

                const liveLine = document.createElementNS("http://www.w3.org/2000/svg", "line");
                liveLine.setAttribute("x1", String(startX));
                liveLine.setAttribute("y1", String(startY));
                liveLine.setAttribute("x2", String(endX));
                liveLine.setAttribute("y2", String(endY));
                liveLine.setAttribute("stroke", "#ff7d4d");
                liveLine.setAttribute("stroke-width", "6");
                liveLine.setAttribute("stroke-linecap", "round");
                liveLine.setAttribute("stroke-dasharray", "6,6");

                const liveStart = document.createElementNS("http://www.w3.org/2000/svg", "circle");
                liveStart.setAttribute("cx", String(startX));
                liveStart.setAttribute("cy", String(startY));
                liveStart.setAttribute("r", "9");
                liveStart.setAttribute("fill", "#ff7d4d");

                const liveEnd = document.createElementNS("http://www.w3.org/2000/svg", "circle");
                liveEnd.setAttribute("cx", String(endX));
                liveEnd.setAttribute("cy", String(endY));
                liveEnd.setAttribute("r", "8");
                liveEnd.setAttribute("fill", "#ff7d4d");

                lines.append(liveLine, liveStart, liveEnd);
            }
        };

        const connectPair = (leftVal, rightVal) => {
            if (!leftVal || !rightVal) return;
            playAnswerAudio(rightVal);
            Object.entries(mappings).forEach(([left, mappedRight]) => {
                if (mappedRight === rightVal && left !== leftVal) delete mappings[left];
            });
            mappings[leftVal] = rightVal;
            leftColumn.querySelectorAll("button").forEach((item) => {
                item.classList.toggle("matched", Object.hasOwn(mappings, item.dataset.value));
                item.classList.remove("selected");
            });
            rightColumn.querySelectorAll("button").forEach((item) => {
                item.classList.toggle("matched", Object.values(mappings).includes(item.dataset.value));
                item.classList.remove("target-hover", "target-ready");
            });
            selectedLeft = "";
            isDraggingLine = false;
            dragSourceBtn = null;
            currentPointerPos = null;
            setAnswer(canonicalMappings(mappings), Object.keys(mappings).length === pairs.length);
            requestAnimationFrame(drawLines);
        };

        pairs.forEach((pair, index) => {
            const isLeftSymbol = isSingleSymbol(pair.left);
            const button = createButton(pair.left, `activity-option clay-button matching-item${isLeftSymbol ? " matching-letter-item" : ""}`);
            button.dataset.value = pair.left;
            button.style.setProperty("--selection-color", activityColors[index % activityColors.length]);

            // Drag to Connect Pointer Handlers
            button.addEventListener("pointerdown", (event) => {
                event.preventDefault();
                playAnswerAudio(pair.left);
                selectedLeft = pair.left;
                isDraggingLine = true;
                dragSourceBtn = button;
                currentPointerPos = { x: event.clientX, y: event.clientY };
                leftColumn.querySelectorAll("button").forEach((item) => item.classList.remove("selected"));
                button.classList.add("selected");
                rightColumn.querySelectorAll("button").forEach((item) => item.classList.add("target-ready"));
                // Khóa cuộn trang khi đang kéo nối – tránh trang bị scroll giữa chừng
                document.body.classList.add("drag-interaction-active");
                button.setPointerCapture(event.pointerId);
                requestAnimationFrame(drawLines);
            });

            button.addEventListener("pointermove", (event) => {
                if (!isDraggingLine) return;
                currentPointerPos = { x: event.clientX, y: event.clientY };
                const dropTarget = document.elementFromPoint(event.clientX, event.clientY)?.closest(".matching-right .matching-item");
                rightColumn.querySelectorAll("button").forEach((item) => {
                    item.classList.toggle("target-hover", item === dropTarget);
                });
                requestAnimationFrame(drawLines);
            });

            button.addEventListener("pointerup", (event) => {
                if (!isDraggingLine) return;
                // Giải phóng khóa cuộn khi nối xong
                document.body.classList.remove("drag-interaction-active");
                const dropTarget = document.elementFromPoint(event.clientX, event.clientY)?.closest(".matching-right .matching-item");
                if (dropTarget && selectedLeft) {
                    connectPair(selectedLeft, dropTarget.dataset.value);
                } else {
                    isDraggingLine = false;
                    dragSourceBtn = null;
                    currentPointerPos = null;
                    rightColumn.querySelectorAll("button").forEach((item) => item.classList.remove("target-hover"));
                    requestAnimationFrame(drawLines);
                }
            });

            button.addEventListener("pointercancel", () => {
                isDraggingLine = false;
                dragSourceBtn = null;
                currentPointerPos = null;
                // Đảm bảo khóa được giải phóng khi sự kiện bị hủy
                document.body.classList.remove("drag-interaction-active");
                requestAnimationFrame(drawLines);
            });

            leftColumn.append(button);
        });

        rights.forEach((right, index) => {
            const isSoundText = /^(meo|gâu|cạp|chíp|ò ó|reng|cục)/i.test(right.trim());
            const isRightSymbol = isSingleSymbol(right);
            const button = createButton(right, `activity-option clay-button matching-item${isRightSymbol ? " matching-letter-item" : ""}`, isSoundText ? "volume_up" : "");
            button.dataset.value = right;
            button.style.setProperty("--selection-color", activityColors[index % activityColors.length]);

            button.addEventListener("click", () => {
                if (!selectedLeft) return;
                connectPair(selectedLeft, right);
            });
            rightColumn.append(button);
        });

        board.append(lines, leftColumn, rightColumn);
        runtime.append(board);
        window.addEventListener("resize", drawLines, {passive: true});
    };

    // Sắp xếp thứ tự bằng chạm: bé chạm lần lượt từng thẻ theo đúng trình tự, thẻ bay lên hàng
    // đích (ô 1, 2, 3...). Chạm lại thẻ đã xếp để trả về. Không cần kéo (kéo HTML5 không chạy
    // trên iPad) và không cần độ chính xác cao. Đáp án vẫn là chuỗi "a|b|c" như trước.
    const renderOrdering = () => {
        const items = [...(payload.items || [])].reverse();
        const placed = [];
        const board = document.createElement("div");
        board.className = "lw-order";
        const track = document.createElement("div");
        track.className = "lw-order-track";
        track.setAttribute("aria-label", "Thứ tự bé đã xếp");
        const hint = document.createElement("p");
        hint.className = "lw-order-hint";
        const pool = document.createElement("div");
        pool.className = "lw-order-pool";
        const reset = document.createElement("button");
        reset.type = "button";
        reset.className = "lw-order-reset";
        reset.innerHTML = '<span class="material-symbols-outlined">restart_alt</span> Xếp lại';

        const chipContent = (chip, item) => {
            const itemClean = String(item || "").trim();
            const isNumeric = /^\d+$/.test(itemClean);
            const mediaUrl = isNumeric ? "" : resolveItemMedia(itemClean);
            const pictogram = isNumeric ? "" : resolvePictogram(itemClean);
            const src = mediaUrl || (pictogram ? `${pictogramPath}${pictogram}` : "");
            if (src) {
                const img = document.createElement("img");
                img.src = src;
                img.alt = "";
                img.loading = "lazy";
                chip.append(img);
            }
            const label = document.createElement("span");
            label.className = "ordering-value";
            label.dataset.rawItem = item;
            label.textContent = itemClean;
            chip.append(label);
            chip.setAttribute("aria-label", itemClean);
        };

        const draw = () => {
            track.replaceChildren();
            pool.replaceChildren();
            items.forEach((_, index) => {
                const slot = document.createElement("div");
                slot.className = "lw-order-slot";
                const item = placed[index];
                if (item !== undefined) {
                    const chip = document.createElement("button");
                    chip.type = "button";
                    chip.className = "lw-order-chip is-placed";
                    chipContent(chip, item);
                    chip.addEventListener("click", () => {
                        placed.splice(index, 1);
                        draw();
                    });
                    const badge = document.createElement("span");
                    badge.className = "lw-order-badge";
                    badge.textContent = String(index + 1);
                    slot.append(badge, chip);
                } else {
                    slot.textContent = String(index + 1);
                    if (index === placed.length) slot.classList.add("is-next");
                }
                track.append(slot);
            });

            const remaining = [...items];
            placed.forEach((item) => remaining.splice(remaining.indexOf(item), 1));
            remaining.forEach((item) => {
                const chip = document.createElement("button");
                chip.type = "button";
                chip.className = "lw-order-chip";
                chipContent(chip, item);
                chip.addEventListener("click", () => {
                    placed.push(item);
                    playAnswerAudio(item);
                    draw();
                });
                pool.append(chip);
            });

            const done = placed.length === items.length;
            hint.textContent = done
                ? "Bé xếp xong rồi! Chạm vào thẻ ở trên nếu muốn đổi."
                : `Chạm vào thẻ bé chọn làm số ${placed.length + 1} nhé!`;
            reset.hidden = placed.length === 0;
            pool.hidden = done;
            // Chỉ chấm khi đã xếp đủ; xếp dở thì hủy hẹn giờ chấm.
            setAnswer(placed.join("|"), done);
        };

        reset.addEventListener("click", () => {
            placed.length = 0;
            draw();
        });

        board.append(track, hint, pool, reset);
        runtime.append(board);
        draw();
    };

    const renderCounting = () => {
        const objects = document.createElement("div");
        objects.className = "counting-objects";
        for (let index = 0; index < Number(payload.targetCount || 0); index += 1) {
            const button = createButton(payload.objectSymbol || "●", "counting-object clay-button");
            button.addEventListener("click", () => {
                button.classList.toggle("counted");
                [...objects.children].forEach((item) => item.querySelector(".count-order")?.remove());
                const allCounted = objects.querySelectorAll(".counted");
                allCounted.forEach((item, countIndex) => {
                    const badge = document.createElement("span");
                    badge.className = "count-order";
                    badge.textContent = String(countIndex + 1);
                    item.append(badge);
                });
                speak(String(allCounted.length));
            });
            objects.append(button);
        }
        const choices = document.createElement("div");
        choices.className = "activity-option-grid";
        (payload.choices || []).forEach((choice) => {
            const button = createButton(choice);
            button.addEventListener("click", () => {
                playAnswerAudio(choice);
                choices.querySelectorAll("button").forEach((item) => item.classList.remove("selected"));
                button.classList.add("selected");
                setAnswer(String(choice), true, true, 1000);
            });
            choices.append(button);
        });
        runtime.append(objects, choices);
    };

    const renderQuantityBuilder = () => {
        let count = 0;
        const target = document.createElement("div");
        target.className = "quantity-target clay-card";
        const counter = document.createElement("div");
        counter.className = "quantity-counter-badge";
        const objects = document.createElement("div");
        objects.className = "quantity-objects";
        const update = () => {
            counter.textContent = `${payload.targetLabel || "Số lượng đã tạo"}: ${count} / ${payload.targetCount}`;
            appendRepeatedVisuals(objects, payload.objectSymbol || "●", count);
            setAnswer(String(count), count > 0);
        };

        const controls = document.createElement("div");
        controls.className = "quantity-controls";

        const add = document.createElement("button");
        add.type = "button";
        add.className = "quantity-btn quantity-btn-add clay-button";
        add.innerHTML = '<span class="material-symbols-outlined">add_circle</span><span>Thêm một</span>';
        add.addEventListener("click", () => {
            if (count < Number(payload.maxItems || 20)) {
                count += 1;
                speak(String(count));
            }
            update();
        });

        const remove = document.createElement("button");
        remove.type = "button";
        remove.className = "quantity-btn quantity-btn-remove clay-button";
        remove.innerHTML = '<span class="material-symbols-outlined">remove_circle</span><span>Bớt một</span>';
        remove.addEventListener("click", () => {
            if (count > 0) {
                count -= 1;
                speak(String(count));
            }
            update();
        });

        controls.append(add, remove);
        target.append(counter, objects);
        runtime.append(target, controls);
        update();
    };

    const renderComparison = () => {
        const board = document.createElement("div");
        const isShapeComparison = payload.visualMode === "shape";
        board.className = `comparison-board ${isShapeComparison ? "comparison-shape-board" : "comparison-quantity-board"}`;
        const selectComparison = (button, value, spokenLabel) => {
            playAnswerAudio(spokenLabel);
            board.querySelectorAll("button").forEach((item) => item.classList.remove("selected"));
            button.classList.add("selected");
            setAnswer(value, true, true, 1000);
        };
        const group = (label, count, value, side) => {
            const button = createButton("", `comparison-group comparison-side-card comparison-side-${side} clay-card`);
            button.querySelector(".answer-label")?.remove();
            const title = document.createElement("strong");
            title.className = "comparison-group-title";
            title.textContent = label;
            const objects = document.createElement("span");
            objects.className = "comparison-objects";
            if (isShapeComparison) {
                const shape = document.createElement("span");
                shape.className = "comparison-shape";
                shape.textContent = payload.objectSymbol || "●";
                shape.style.fontSize = `${Math.max(52, Math.min(150, Number(count)))}px`;
                shape.setAttribute("aria-label", payload.shapeName || label);
                objects.append(shape);
                button.classList.add("comparison-shape-card");
            } else appendRepeatedVisuals(objects, payload.objectSymbol || "●", count);
            button.append(title, objects);
            button.addEventListener("click", () => selectComparison(button, value, label));
            return button;
        };

        const equalButton = createButton("", "comparison-group comparison-equal-card clay-card");
        equalButton.querySelector(".answer-label")?.remove();
        const equalIcon = document.createElement("span");
        equalIcon.className = "material-symbols-outlined comparison-balance-icon";
        equalIcon.textContent = "balance";
        const equalSign = document.createElement("strong");
        equalSign.className = "comparison-equal-sign";
        equalSign.textContent = "=";
        const equalLabel = document.createElement("span");
        equalLabel.className = "comparison-equal-label";
        equalLabel.textContent = "Bằng nhau";
        equalButton.append(equalIcon, equalSign, equalLabel);
        equalButton.setAttribute("aria-label", "Hai nhóm bằng nhau");
        equalButton.addEventListener("click", () => selectComparison(equalButton, "equal", "Bằng nhau"));

        board.append(
            group(payload.leftLabel || "Nhóm A", payload.leftCount, "left", "left"),
            equalButton,
            group(payload.rightLabel || "Nhóm B", payload.rightCount, "right", "right"));
        runtime.append(board);
    };

    const renderClassification = () => {
        const mappings = payload.mappings || [];
        const answers = {};
        let selectedItem = "";
        let dragGhost = null;

        const assignItemToCategory = (itemName, categoryName) => {
            if (!itemName || !categoryName) return;
            playAnswerAudio(categoryName);
            answers[itemName] = categoryName;

            // Update Source item status
            const itemButton = [...source.children].find((item) => item.dataset.itemName === itemName);
            if (itemButton) {
                itemButton.classList.add("matched");
                itemButton.classList.remove("selected");
            }

            // Remove from other category trays if previously assigned
            categories.querySelectorAll(".classification-chip").forEach((chip) => {
                if (chip.dataset.assignedItem === itemName) chip.remove();
            });

            // Add to this category tray
            const targetZone = [...categories.children].find((z) => z.dataset.categoryName === categoryName);
            if (targetZone) {
                const tray = targetZone.querySelector(".classification-zone-tray");
                const emptyHint = targetZone.querySelector(".classification-tray-hint");
                if (emptyHint) emptyHint.style.display = "none";
                const chip = document.createElement("div");
                chip.className = "classification-chip clay-card";
                chip.dataset.assignedItem = itemName;
                decorateButton(chip, itemName);
                tray.append(chip);
            }

            selectedItem = "";
            source.querySelectorAll("button").forEach((item) => item.classList.remove("selected"));
            categories.querySelectorAll(".classification-zone").forEach((z) => z.classList.remove("ready", "drop-hover"));
            setAnswer(canonicalMappings(answers), Object.keys(answers).length === mappings.length);
        };

        const movePointerDrag = (event) => {
            if (!dragGhost) return;
            event.preventDefault();
            dragGhost.style.left = `${event.clientX}px`;
            dragGhost.style.top = `${event.clientY}px`;
            const dropTarget = document.elementFromPoint(event.clientX, event.clientY);
            const zone = dropTarget?.closest?.(".classification-zone");
            categories.querySelectorAll(".classification-zone").forEach((z) => z.classList.toggle("drop-hover", z === zone));
        };

        const finishPointerDrag = (event) => {
            if (!dragGhost) return;
            event.preventDefault();
            dragGhost.remove();
            dragGhost = null;
            const dropTarget = document.elementFromPoint(event.clientX, event.clientY);
            const zone = dropTarget?.closest?.(".classification-zone");
            categories.querySelectorAll(".classification-zone").forEach((z) => z.classList.remove("drop-hover"));
            if (zone?.dataset?.categoryName && selectedItem) {
                assignItemToCategory(selectedItem, zone.dataset.categoryName);
            }
        };

        const startPointerDrag = (event, itemText, button) => {
            if (event.pointerType === "mouse") return;
            event.preventDefault();
            selectedItem = itemText;
            source.querySelectorAll("button").forEach((item) => item.classList.remove("selected"));
            button.classList.add("selected");
            categories.querySelectorAll(".classification-zone").forEach((z) => z.classList.add("ready"));
            dragGhost = button.cloneNode(true);
            dragGhost.classList.add("drag-ghost");
            document.body.append(dragGhost);
            button.setPointerCapture(event.pointerId);
            movePointerDrag(event);
        };

        // 1. Source Items Row
        const source = document.createElement("div");
        source.className = "activity-option-grid classification-source-grid";
        mappings.forEach((mapping, index) => {
            const button = createButton(mapping.left, "activity-option classification-source-item draggable-option clay-button");
            button.dataset.itemName = mapping.left;
            button.draggable = true;
            button.style.setProperty("--selection-color", activityColors[index % activityColors.length]);
            button.addEventListener("click", () => {
                playAnswerAudio(mapping.left);
                selectedItem = mapping.left;
                source.querySelectorAll("button").forEach((item) => item.classList.remove("selected"));
                button.classList.add("selected");
                categories.querySelectorAll(".classification-zone").forEach((zone) => zone.classList.add("ready"));
            });
            button.addEventListener("dragstart", (event) => {
                selectedItem = mapping.left;
                event.dataTransfer.setData("text/plain", mapping.left);
            });
            button.addEventListener("pointerdown", (event) => startPointerDrag(event, mapping.left, button));
            button.addEventListener("pointermove", movePointerDrag);
            button.addEventListener("pointerup", finishPointerDrag);
            button.addEventListener("pointercancel", finishPointerDrag);
            source.append(button);
        });

        // 2. Categories Drop Bins/Trays
        const categories = document.createElement("div");
        categories.className = "classification-zones";
        
        (payload.categories || []).forEach((category, categoryIndex) => {
            const zone = document.createElement("div");
            zone.className = "classification-zone clay-card";
            zone.dataset.categoryName = category;
            const categoryColor = activityColors[categoryIndex % activityColors.length];
            zone.style.setProperty("--category-color", categoryColor);

            // Category Header (Clear Label with Basket Icon)
            const header = document.createElement("div");
            header.className = "classification-zone-header";
            const headerIcon = document.createElement("span");
            headerIcon.className = "material-symbols-outlined";
            headerIcon.textContent = "inventory_2";
            const headerLabel = document.createElement("strong");
            headerLabel.textContent = `Nhóm ${category}`;
            header.append(headerIcon, headerLabel);

            // Item drop tray
            const tray = document.createElement("div");
            tray.className = "classification-zone-tray";
            tray.dataset.category = category;

            const emptyHint = document.createElement("span");
            emptyHint.className = "classification-tray-hint";
            emptyHint.textContent = "Kéo hoặc chạm vào đây để xếp";
            tray.append(emptyHint);

            zone.append(header, tray);

            zone.addEventListener("click", () => {
                if (selectedItem) assignItemToCategory(selectedItem, category);
            });
            zone.addEventListener("dragover", (event) => {
                event.preventDefault();
                zone.classList.add("drop-hover");
            });
            zone.addEventListener("dragleave", () => {
                zone.classList.remove("drop-hover");
            });
            zone.addEventListener("drop", (event) => {
                event.preventDefault();
                const dropped = event.dataTransfer.getData("text/plain") || selectedItem;
                assignItemToCategory(dropped, category);
            });

            categories.append(zone);
        });

        runtime.append(source, categories);
    };

    const renderStoryChoice = () => {
        if (payload.audioUrl || payload.speechText) {
            const storyContainer = document.createElement("div");
            storyContainer.className = "story-audio-container";
            
            const audioButton = document.createElement("button");
            audioButton.type = "button";
            audioButton.className = "activity-audio-button clay-button";
            
            const speakerIcon = document.createElement("span");
            speakerIcon.className = "material-symbols-outlined audio-btn-icon";
            speakerIcon.textContent = "volume_up";
            
            const labelSpan = document.createElement("span");
            labelSpan.className = "audio-btn-label";
            labelSpan.textContent = "Nghe lại câu chuyện";
            
            audioButton.append(speakerIcon, labelSpan);
            audioButton.addEventListener("click", playPromptAudio);
            storyContainer.append(audioButton);
            runtime.append(storyContainer);
        }
        renderChoice(false);
    };

    const renderers = {
        single_choice: () => renderChoice(false),
        listen_choose: () => renderChoice(false),
        multi_select: renderMultiSelect,
        drag_drop: renderDragDrop,
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
        appendScene();
        renderers[type]?.();
    };
    renderActivity();
})();
