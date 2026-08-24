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
        ["áo mưa", "coat.svg"], ["ô", "umbrella.svg"], ["chiếc ô", "umbrella.svg"], ["mũ rộng vành", "sun-hat.svg"], ["kính râm", "sunglasses.svg"],
        ["bút", "pencil.svg"], ["bút chì", "pencil.svg"], ["bút màu", "artist-palette.svg"], ["vở", "notebook.svg"], ["quyển vở", "notebook.svg"],
        ["sách", "book.svg"], ["quyển sách", "book.svg"], ["ba lô", "backpack.svg"], ["cặp sách", "backpack.svg"],
        ["bát", "bowl.svg"], ["cái bát", "bowl.svg"], ["thìa", "spoon.svg"], ["cái thìa", "spoon.svg"], ["nồi", "cooking-pot.svg"],
        ["thuyền", "sailboat.svg"], ["xe đạp", "bicycle.svg"], ["ô tô", "car.svg"], ["xe ô tô", "car.svg"], ["xe", "car.svg"], ["xe buýt", "bus.svg"], ["máy bay", "airplane.svg"], ["gara", "house.svg"],
        ["bàn chải", "toothbrush.svg"], ["áo", "shirt.svg"], ["quần", "pants.svg"], ["giày", "shoe.svg"], ["cởi giày", "shoe.svg"], ["tất", "socks.svg"], ["mũ", "hat.svg"], ["khăn", "scarf.svg"],
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
        ["tự chạy thật nhanh", "running.svg"], ["đứng chơi giữa đường", "standing.svg"], ["đi ngủ", "sleeping.svg"],
        ["cất sách", "book.svg"], ["cất hết bút đi", "package.svg"], ["bỏ ra ngoài", "walking.svg"],
        ["rửa tay", "soap.svg"], ["làm ướt tay", "water.svg"], ["lấy xà phòng", "soap.svg"], ["chà sạch tay", "soap.svg"], ["xả nước", "water.svg"], ["lau khô", "shirt.svg"],
        ["chào cô", "speaking.svg"], ["cất ba lô", "backpack.svg"], ["ngồi vào chỗ", "standing.svg"], ["mở sách", "book.svg"],
        ["ăn cơm", "bowl.svg"], ["dọn bát", "bowl.svg"], ["chọn bút", "pencil.svg"], ["cầm bằng ba ngón", "pencil.svg"],
        ["đặt giấy ngay ngắn", "notebook.svg"], ["gấp hai mép", "notebook.svg"], ["miết nếp gấp", "notebook.svg"],
        ["gieo hạt", "seedling.svg"], ["tưới nước", "water.svg"], ["hạt nảy mầm", "seedling.svg"], ["cây lớn lên", "seedling.svg"],
        ["meo meo", "cat.svg"], ["gâu gâu", "dog.svg"], ["cạp cạp", "duck.svg"],
        ["cao", "sun.svg"], ["thấp", "flower.svg"],
        ["khi đèn người đi bộ màu xanh", "walking.svg"], ["khi xe đang chạy", "car.svg"], ["khi đèn người đi bộ màu đỏ", "prohibited.svg"],
        ["chia sẻ bút màu", "artist-palette.svg"], ["chia sẻ và chơi cùng", "handshake.svg"],
        ["mình xin lỗi bạn", "folded-hands.svg"], ["đi cùng người lớn", "walking.svg"]
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

    // ONLY single characters (length === 1) get a letter badge to prevent words like 'Cao' having duplicate text
    const isSingleSymbol = (text) => {
        const clean = String(text || "").trim();
        return clean.length === 1 && (/^[A-Za-z0-9ĂÂĐÊÔƠƯăâđêôơư]+$/.test(clean) || /^[0-9]+$/.test(clean));
    };

    const decorateButton = (button, text, forceIcon = "") => {
        const value = String(text ?? "");
        const normalized = value.trim().toLocaleLowerCase("vi-VN");
        const isLetterOrDigit = isSingleSymbol(value) && !pictograms.has(normalized);
        const shouldHideOptionPhoto = (type === "single_choice" || type === "listen_choose" || type === "multi_select") && isLetterOrDigit;
        const mediaUrl = shouldHideOptionPhoto ? "" : resolveItemMedia(value);
        const pictogram = shouldHideOptionPhoto ? "" : resolvePictogram(value);
        const shape = shapeClasses.get(normalized);
        const color = colorValues.get(normalized);
        button.replaceChildren();

        if (mediaUrl) {
            const image = document.createElement("img");
            image.className = "answer-photo";
            image.src = mediaUrl;
            image.alt = value;
            image.loading = "lazy";
            button.classList.add("has-answer-visual", "has-answer-photo");
            button.append(image);
        } else if (pictogram) {
            const image = document.createElement("img");
            image.className = "answer-pictogram";
            image.src = `${pictogramPath}${pictogram}`;
            image.alt = value;
            image.loading = "lazy";
            button.classList.add("has-answer-visual", "has-answer-pictogram");
            button.append(image);
        } else if (shape || color) {
            const visual = document.createElement("span");
            visual.className = shape ? `answer-shape answer-shape-${shape}` : "answer-color-swatch";
            if (color) visual.style.backgroundColor = color;
            visual.setAttribute("aria-hidden", "true");
            button.classList.add("has-answer-visual");
            button.append(visual);
        } else if (forceIcon) {
            const iconSpan = document.createElement("span");
            iconSpan.className = "material-symbols-outlined answer-icon-glyph";
            iconSpan.textContent = forceIcon;
            button.classList.add("has-answer-visual");
            button.append(iconSpan);
        }

        const label = document.createElement("span");
        label.className = "answer-label";
        if (value.trim().length > 4) label.classList.add("long-label");
        label.textContent = value;
        button.append(label);
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

    const speak = (text) => {
        if (!text || !window.speechSynthesis) return;
        const vietnameseVoice = window.speechSynthesis.getVoices()
            .find((voice) => voice.lang.toLowerCase().startsWith("vi"));
        if (!vietnameseVoice) return;
        window.speechSynthesis.cancel();
        const utterance = new SpeechSynthesisUtterance(text);
        utterance.lang = "vi-VN";
        utterance.rate = 0.86;
        utterance.voice = vietnameseVoice;
        window.speechSynthesis.speak(utterance);
    };

    const playPromptAudio = () => {
        if (payload.audioUrl) {
            new Audio(payload.audioUrl).play();
        } else {
            speak(payload.speechText);
        }
    };

    const optionAudio = payload.optionAudio && typeof payload.optionAudio === "object" ? payload.optionAudio : {};
    const normalizeOptionKey = (value) => String(value || "").trim().toLocaleLowerCase("vi-VN");
    const resolveOptionAudioUrl = (value) => {
        const direct = optionAudio[String(value || "").trim()];
        if (direct) return String(direct);
        const normalized = normalizeOptionKey(value);
        const match = Object.entries(optionAudio)
            .find(([label]) => normalizeOptionKey(label) === normalized);
        return match?.[1] ? String(match[1]) : "";
    };

    const playAnswerAudio = (value) => {
        const audioUrl = resolveOptionAudioUrl(value);
        window.speechSynthesis?.cancel?.();
        if (audioUrl) {
            const audio = new Audio(audioUrl);
            audio.play().catch(() => speak(value));
            return true;
        }
        speak(value);
        return Boolean(value);
    };

    const createButton = (text, className = "activity-option clay-button", forceIcon = "") => {
        const button = document.createElement("button");
        button.type = "button";
        button.className = className;
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

    const renderChoice = (allowMultiple = false) => {
        const selected = new Set();
        const grid = document.createElement("div");
        grid.className = "activity-option-grid";
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
        runtime.append(grid);
    };

    const renderMultiSelect = () => renderChoice(true);

    const renderDragDrop = () => {
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
                const labelText = (btn.querySelector(".answer-label")?.textContent || btn.textContent || "").trim();
                btn.classList.toggle("selected", labelText === value.trim());
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
        const lines = document.createElementNS("http://www.w3.org/2000/svg", "svg");
        lines.classList.add("matching-lines");
        lines.setAttribute("aria-hidden", "true");
        const leftColumn = document.createElement("div");
        leftColumn.className = "matching-column matching-left";
        const rightColumn = document.createElement("div");
        rightColumn.className = "matching-column matching-right";
        const rights = pairs.map((pair) => pair.right).reverse();

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
            const button = createButton(pair.left, "activity-option clay-button matching-item");
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
                requestAnimationFrame(drawLines);
            });

            leftColumn.append(button);
        });

        rights.forEach((right, index) => {
            const isSoundText = /^(meo|gâu|cạp|chíp|ò ó|reng|cục)/i.test(right.trim());
            const button = createButton(right, "activity-option clay-button matching-item", isSoundText ? "volume_up" : "");
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

                const badge = document.createElement("span");
                badge.className = "ordering-badge";
                badge.textContent = String(index + 1);

                const value = document.createElement("div");
                value.className = "ordering-value-wrap ordering-value";
                value.dataset.rawItem = item;
                const itemClean = String(item || "").trim();
                const pictogram = resolvePictogram(itemClean);
                if (pictogram) {
                    const img = document.createElement("img");
                    img.className = "ordering-pictogram";
                    img.src = `${pictogramPath}${pictogram}`;
                    img.alt = itemClean;
                    value.append(img);
                }
                const label = document.createElement("span");
                label.className = "ordering-label";
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
                row.append(badge, value, actions);
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
        board.className = "comparison-board";
        const group = (label, count, value) => {
            const button = createButton("", "comparison-group clay-card");
            const title = document.createElement("strong");
            title.textContent = label;
            const objects = document.createElement("span");
            objects.className = "comparison-objects";
            if (Number(count) === 0) objects.textContent = "=";
            else appendRepeatedVisuals(objects, payload.objectSymbol || "●", count);
            button.append(title, objects);
            button.addEventListener("click", () => {
                playAnswerAudio(label);
                board.querySelectorAll("button").forEach((item) => item.classList.remove("selected"));
                button.classList.add("selected");
                setAnswer(value, true, true, 1000);
            });
            return button;
        };
        board.append(
            group(payload.leftLabel || "Nhóm A", payload.leftCount, "left"),
            group("Bằng nhau", 0, "equal"),
            group(payload.rightLabel || "Nhóm B", payload.rightCount, "right"));
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
            labelSpan.textContent = type === "story_choice" ? "Nghe câu chuyện" : "Nghe âm thanh";
            
            audioButton.append(speakerIcon, labelSpan);
            audioButton.addEventListener("click", playPromptAudio);
            storyContainer.append(audioButton);
            runtime.append(storyContainer);
        }
        renderChoice(false);
    };

    const renderers = {
        single_choice: () => renderChoice(false),
        listen_choose: renderStoryChoice,
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
