(() => {
    const runtime = document.querySelector("[data-activity-runtime]");
    const payloadElement = document.querySelector("[data-activity-payload]");
    const answerInput = document.querySelector("[data-activity-answer]");
    const submitButton = document.querySelector("[data-activity-submit]");
    const form = runtime?.closest("form");
    if (!runtime || !payloadElement || !answerInput || !submitButton) return;

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
    const autoSubmitTypes = new Set(["single_choice", "listen_choose", "story_choice", "drag_drop", "counting", "comparison"]);
    let submitTimer = 0;
    let optionColorIndex = 0;
    if (autoSubmitTypes.has(type)) submitButton.hidden = true;

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

        if (!repeatedPictureGroup) {
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

    const hostVoiceEl = document.querySelector("[data-learning-voice]") || document.querySelector(".learning-stage");
    const isEnglishVoice = hostVoiceEl?.dataset?.englishVoice === "true";

    const loadBrowserVoices = () => new Promise((resolve) => {
        const voices = window.speechSynthesis?.getVoices?.() || [];
        if (voices.length) {
            resolve(voices);
            return;
        }
        const timeout = window.setTimeout(() => {
            window.speechSynthesis?.removeEventListener?.("voiceschanged", onVoicesChanged);
            resolve(window.speechSynthesis?.getVoices?.() || []);
        }, 600);
        function onVoicesChanged() {
            window.clearTimeout(timeout);
            resolve(window.speechSynthesis?.getVoices?.() || []);
        }
        window.speechSynthesis?.addEventListener?.("voiceschanged", onVoicesChanged, { once: true });
    });

    const isVietnameseText = (str) => /[àáạảãâầấậẩẫăằắặẳẵèéẹẻẽêềếệểễìíịỉĩòóọỏõôồốộổỗơờớợởỡùúụủũưừứựửữỳýỵỷỹđĐ]/i.test(str || "");

    const speak = async (text) => {
        return;
    };

    const playPromptAudio = () => {
        const audioUrl = isEnglishVoice
            ? (payload.audioUrlEn || payload.questionAudioUrlEn || payload.audioUrl)
            : (payload.audioUrl || payload.questionAudioUrl);
        if (audioUrl) {
            new Audio(audioUrl).play().catch(() => {});
        }
    };

    const optionAudio = payload.optionAudio && typeof payload.optionAudio === "object" ? payload.optionAudio : {};
    const optionAudioEn = payload.optionAudioEn && typeof payload.optionAudioEn === "object" ? payload.optionAudioEn : {};
    const normalizeOptionKey = (value) => String(value || "").trim().toLocaleLowerCase("vi-VN");

    const resolveOptionAudioUrl = (value) => {
        const cleanKey = String(value || "").trim();
        const normalized = normalizeOptionKey(value);

        if (isEnglishVoice) {
            const directEn = optionAudioEn[cleanKey];
            if (directEn) return String(directEn);
            const matchEn = Object.entries(optionAudioEn)
                .find(([label]) => normalizeOptionKey(label) === normalized);
            if (matchEn?.[1]) return String(matchEn[1]);
        }

        const directVi = optionAudio[cleanKey];
        if (directVi) return String(directVi);
        const matchVi = Object.entries(optionAudio)
            .find(([label]) => normalizeOptionKey(label) === normalized);
        return matchVi?.[1] ? String(matchVi[1]) : "";
    };

    const playAnswerAudio = (value) => {
        const audioUrl = resolveOptionAudioUrl(value);
        window.speechSynthesis?.cancel?.();
        if (audioUrl) {
            const audio = new Audio(audioUrl);
            audio.play().catch(() => {});
            return true;
        }
        return false;
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

    const setAnswer = (value, ready = true, autoSubmit = false, autoSubmitDelay = 800) => {
        answerInput.value = value;
        submitButton.disabled = !ready;
        window.clearTimeout(submitTimer);
        if (ready && autoSubmit && autoSubmitTypes.has(type)) {
            submitTimer = window.setTimeout(() => form?.requestSubmit(), autoSubmitDelay);
        }
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
        const grid = document.createElement("div");
        grid.className = "activity-option-grid";
        const hasTextOptions = (payload.choices || []).some((choice) => String(choice ?? "").trim().length > 3);
        if (hasTextOptions) {
            grid.classList.add("grid-text-options");
        }
        if (payload.focusVisual) {
            const focusVisual = document.createElement("div");
            focusVisual.className = "activity-focus-visual";
            decorateButton(focusVisual, payload.focusVisual);
            focusVisual.querySelector(".answer-label")?.remove();
            focusVisual.setAttribute("role", "img");
            focusVisual.setAttribute("aria-label", `Hình cần nhận biết: ${payload.focusVisual}`);
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
                setAnswer(answer, selected.size > 0, !allowMultiple, 1000);
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

    const renderOrdering = () => {
        const items = [...(payload.items || [])].reverse();
        const list = document.createElement("div");
        list.className = "ordering-list";
        const isNumberOrdering = items.every((it) => /^\d+$/.test(String(it || "").trim()));
        if (isNumberOrdering) list.classList.add("ordering-number-list");
        const sync = () => setAnswer([...list.querySelectorAll(".ordering-value")].map((node) => node.dataset.rawItem).join("|"));
        let draggingIndex = -1;
        const moveItem = (from, to) => {
            if (from < 0 || to < 0 || from === to) return;
            const [moved] = items.splice(from, 1);
            items.splice(to, 0, moved);
            draw();
        };
        const draw = () => {
            list.replaceChildren();
            items.forEach((item, index) => {
                const row = document.createElement("div");
                row.className = "ordering-row clay-card";
                row.draggable = true;
                row.dataset.index = String(index);

                const value = document.createElement("div");
                value.className = "ordering-value-wrap ordering-value";
                value.dataset.rawItem = item;
                const itemClean = String(item || "").trim();
                const isNumeric = /^\d+$/.test(itemClean);
                const mediaUrl = isNumeric ? "" : resolveItemMedia(itemClean);
                const pictogram = isNumeric ? "" : resolvePictogram(itemClean);
                if (mediaUrl) {
                    const img = document.createElement("img");
                    img.className = "ordering-photo";
                    img.src = mediaUrl;
                    img.alt = itemClean;
                    img.loading = "lazy";
                    value.append(img);
                    row.classList.add("has-ordering-visual");
                } else if (pictogram) {
                    const img = document.createElement("img");
                    img.className = "ordering-pictogram";
                    img.src = `${pictogramPath}${pictogram}`;
                    img.alt = itemClean;
                    img.loading = "lazy";
                    value.append(img);
                    row.classList.add("has-ordering-visual");
                }
                const label = document.createElement("span");
                label.className = "ordering-label";
                if (isNumeric || itemClean.length <= 2) {
                    label.classList.add("short-label");
                    if (isNumeric) {
                        label.classList.add("ordering-number-label");
                        row.classList.add("ordering-number-row");
                    }
                } else {
                    label.classList.add("long-label");
                }
                label.textContent = itemClean;
                value.append(label);

                const actions = document.createElement("div");
                actions.className = "ordering-actions";

                const up = document.createElement("button");
                up.type = "button";
                up.className = "ordering-control ordering-up clay-button";
                up.innerHTML = '<span class="material-symbols-outlined">arrow_upward</span>';
                up.disabled = index === 0;
                up.setAttribute("aria-label", `Đưa ${item} lên`);
                up.addEventListener("click", (event) => {
                    event.stopPropagation();
                    moveItem(index, index - 1);
                });

                const down = document.createElement("button");
                down.type = "button";
                down.className = "ordering-control ordering-down clay-button";
                down.innerHTML = '<span class="material-symbols-outlined">arrow_downward</span>';
                down.disabled = index === items.length - 1;
                down.setAttribute("aria-label", `Đưa ${item} xuống`);
                down.addEventListener("click", (event) => {
                    event.stopPropagation();
                    moveItem(index, index + 1);
                });

                actions.append(up, down);

                row.addEventListener("click", (event) => {
                    if (event.target.closest(".ordering-control")) return;
                    playAnswerAudio(item);
                });
                row.addEventListener("dragstart", () => { draggingIndex = index; row.classList.add("dragging"); });
                row.addEventListener("dragover", (event) => { event.preventDefault(); row.classList.add("drag-over"); });
                row.addEventListener("dragleave", () => row.classList.remove("drag-over"));
                row.addEventListener("drop", (event) => { event.preventDefault(); moveItem(draggingIndex, index); });
                row.addEventListener("dragend", () => { draggingIndex = -1; row.classList.remove("dragging"); });
                // Bỏ badge STT ở đầu hàng để không gây nhầm lẫn cho trẻ
                row.append(value, actions);
                list.append(row);
            });
            sync();
        };
        draw();
        runtime.append(list);
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
    renderers[type]?.();
})();
