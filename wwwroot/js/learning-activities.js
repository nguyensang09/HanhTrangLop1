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
    const createButton = (text, className = "activity-option") => {
        const button = document.createElement("button");
        button.type = "button";
        button.className = className;
        decorateButton(button, text);
        if (button.matches(".activity-option, .activity-drop-zone, .comparison-group")) {
            const color = activityColors[optionColorIndex % activityColors.length];
            button.style.setProperty("--option-color", color);
            button.style.setProperty("--selection-color", color);
            optionColorIndex += 1;
        }
        return button;
    };
    const setAnswer = (value, ready = true, autoSubmit = false, autoSubmitDelay = 500) => {
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
    const activityColors = ["#ff8a5b", "#29b6a6", "#3e9ed6", "#f4b740", "#8b79d1"];
    const pictogramPath = "/images/pictograms/";
    const pictograms = new Map([
        ["🍎", "apple.svg"], ["🍊", "orange.svg"], ["🐟", "fish.svg"], ["⭐", "star.svg"], ["🌼", "flower.svg"], ["🍓", "strawberry.svg"],
        ["táo", "apple.svg"], ["cam", "orange.svg"], ["cà rốt", "carrot.svg"], ["bắp cải", "leafy-green.svg"],
        ["cá", "fish.svg"], ["tôm", "shrimp.svg"], ["mèo", "cat.svg"], ["chó", "dog.svg"], ["vịt", "duck.svg"], ["gà", "chicken.svg"], ["chim", "bird.svg"], ["ong", "bee.svg"], ["thỏ", "rabbit.svg"],
        ["áo mưa", "coat.svg"], ["ô", "umbrella.svg"], ["mũ rộng vành", "sun-hat.svg"], ["kính râm", "sunglasses.svg"],
        ["bút", "pencil.svg"], ["vở", "notebook.svg"], ["sách", "book.svg"], ["ba lô", "backpack.svg"], ["bát", "bowl.svg"], ["thìa", "spoon.svg"], ["nồi", "cooking-pot.svg"],
        ["thuyền", "sailboat.svg"], ["xe đạp", "bicycle.svg"], ["ô tô", "car.svg"], ["xe", "car.svg"], ["xe buýt", "bus.svg"], ["máy bay", "airplane.svg"], ["gara", "house.svg"],
        ["bàn chải", "toothbrush.svg"], ["áo", "shirt.svg"], ["quần", "pants.svg"], ["giày", "shoe.svg"], ["tất", "socks.svg"], ["mũ", "hat.svg"], ["khăn", "scarf.svg"],
        ["kem", "ice-cream.svg"], ["nước đá", "ice-cube.svg"], ["canh", "cooking-pot.svg"], ["trà", "tea.svg"], ["gối", "pillow.svg"], ["bông", "cloud.svg"], ["đá", "rock.svg"], ["gạch", "brick.svg"],
        ["mặt trời", "sun.svg"], ["mặt trăng", "moon.svg"], ["quả bóng", "ball.svg"], ["xà phòng", "soap.svg"], ["cây", "seedling.svg"], ["kéo", "scissors.svg"], ["hạt", "thread.svg"], ["tô màu", "artist-palette.svg"],
        ["mũ bảo hiểm", "helmet.svg"], ["ổ điện", "electric-plug.svg"], ["chia sẻ", "handshake.svg"], ["xin lỗi", "folded-hands.svg"], ["buồn", "sad-face.svg"], ["người lớn", "handshake.svg"],
        ["trái cây", "apple.svg"], ["rau củ", "carrot.svg"], ["dưới nước", "fish.svg"], ["trên cạn", "cat.svg"],
        ["trời mưa", "umbrella.svg"], ["trời nắng", "sun.svg"], ["học tập", "notebook.svg"], ["nhà bếp", "cooking-pot.svg"],
        ["ban ngày", "sun.svg"], ["ban đêm", "moon.svg"], ["lạnh", "ice-cube.svg"], ["nóng", "tea.svg"], ["mềm", "pillow.svg"], ["cứng", "brick.svg"],
        ["đứng lên", "standing.svg"], ["đùa nghịch", "game.svg"], ["từ chối và gọi người thân", "telephone.svg"],
        ["đi theo ngay", "walking.svg"], ["không nói với ai", "zipper-mouth.svg"], ["nói với người con tin tưởng", "speaking.svg"],
        ["đập đồ", "hammer.svg"], ["la hét vào bạn", "speaking.svg"], ["không chạm vào", "prohibited.svg"],
        ["cho tay vào", "raised-hand.svg"], ["đổ nước lên", "water.svg"], ["giấu đồ chơi", "package.svg"],
        ["đẩy bạn ra", "raised-hand.svg"], ["không phải mình", "shrug.svg"], ["bạn tự chịu", "sad-face.svg"],
        ["tự chạy thật nhanh", "running.svg"], ["đứng chơi giữa đường", "standing.svg"], ["đi ngủ", "sleeping.svg"],
        ["cất sách", "book.svg"], ["cất hết bút đi", "package.svg"], ["bỏ ra ngoài", "walking.svg"]
    ]);
    const shapeClasses = new Map([
        ["hình tròn", "circle"], ["tròn", "circle"], ["○", "circle"],
        ["hình vuông", "square"], ["vuông", "square"], ["□", "square"],
        ["hình tam giác", "triangle"], ["tam giác", "triangle"], ["△", "triangle"],
        ["hình chữ nhật", "rectangle"], ["hình bầu dục", "oval"], ["hình thoi", "diamond"],
        ["hình ngôi sao", "star"], ["hình trái tim", "heart"]
    ]);
    const colorValues = new Map([
        ["đỏ", "#ff654d"], ["xanh", "#28a9d8"], ["vàng", "#f7c948"], ["tím", "#8873dc"]
    ]);
    [
        ["táo", "apple.svg"], ["cam", "orange.svg"], ["cà rốt", "carrot.svg"], ["bắp cải", "leafy-green.svg"],
        ["cá", "fish.svg"], ["tôm", "shrimp.svg"], ["mèo", "cat.svg"], ["chó", "dog.svg"], ["vịt", "duck.svg"], ["gà", "chicken.svg"], ["chim", "bird.svg"], ["ong", "bee.svg"], ["thỏ", "rabbit.svg"],
        ["áo mưa", "coat.svg"], ["ô", "umbrella.svg"], ["mũ rộng vành", "sun-hat.svg"], ["kính râm", "sunglasses.svg"],
        ["bút", "pencil.svg"], ["vở", "notebook.svg"], ["sách", "book.svg"], ["ba lô", "backpack.svg"], ["bát", "bowl.svg"], ["thìa", "spoon.svg"], ["nồi", "cooking-pot.svg"],
        ["thuyền", "sailboat.svg"], ["xe đạp", "bicycle.svg"], ["ô tô", "car.svg"], ["xe", "car.svg"], ["xe buýt", "bus.svg"], ["máy bay", "airplane.svg"],
        ["kem", "ice-cream.svg"], ["nước đá", "ice-cube.svg"], ["canh", "cooking-pot.svg"], ["trà", "tea.svg"],
        ["dưới nước", "fish.svg"], ["trên đường", "car.svg"], ["trên cạn", "cat.svg"], ["trái cây", "apple.svg"], ["rau củ", "carrot.svg"],
        ["mặt trời", "sun.svg"], ["mặt trăng", "moon.svg"], ["quả bóng", "ball.svg"], ["cây", "seedling.svg"],
        ["trời mưa", "umbrella.svg"], ["trời nắng", "sun.svg"], ["học tập", "notebook.svg"], ["nhà bếp", "cooking-pot.svg"],
        ["ban ngày", "sun.svg"], ["ban đêm", "moon.svg"], ["lạnh", "ice-cube.svg"], ["nóng", "tea.svg"], ["mềm", "pillow.svg"], ["cứng", "brick.svg"]
    ].forEach(([label, file]) => pictograms.set(label, file));
    [
        ["hình tròn", "circle"], ["tròn", "circle"], ["○", "circle"],
        ["hình vuông", "square"], ["vuông", "square"], ["□", "square"],
        ["hình tam giác", "triangle"], ["tam giác", "triangle"], ["△", "triangle"],
        ["hình chữ nhật", "rectangle"], ["hình bầu dục", "oval"], ["hình thoi", "diamond"],
        ["hình ngôi sao", "star"], ["hình trái tim", "heart"]
    ].forEach(([label, shape]) => shapeClasses.set(label, shape));
    [["đỏ", "#ff654d"], ["xanh", "#28a9d8"], ["vàng", "#f7c948"], ["tím", "#8873dc"]]
        .forEach(([label, color]) => colorValues.set(label, color));

    const itemMedia = new Map(Object.entries(payload.itemMedia || {})
        .map(([label, url]) => [label.trim().toLocaleLowerCase("vi-VN"), String(url || "").trim()]));
    const resolveItemMedia = (text) => {
        const normalized = String(text || "").trim().toLocaleLowerCase("vi-VN");
        const candidates = [
            normalized,
            normalized.replace(/^con\s+/, ""),
            normalized.replace(/^chú\s+/, ""),
            normalized.replace(/^cái\s+/, ""),
            normalized.replace(/^quả\s+/, "")
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
    const decorateButton = (button, text) => {
        const value = String(text ?? "");
        const normalized = value.trim().toLocaleLowerCase("vi-VN");
        const pictogram = resolvePictogram(value);
        const mediaUrl = resolveItemMedia(value);
        const shape = shapeClasses.get(normalized);
        const color = colorValues.get(normalized);
        button.replaceChildren();
        if (mediaUrl) {
            const image = document.createElement("img");
            image.className = "answer-photo";
            image.src = mediaUrl;
            image.alt = value;
            button.classList.add("has-answer-visual", "has-answer-photo");
            button.append(image);
        } else if (pictogram) {
            const image = document.createElement("img");
            image.className = "answer-pictogram";
            image.src = `${pictogramPath}${pictogram}`;
            image.alt = "";
            image.setAttribute("aria-hidden", "true");
            button.classList.add("has-answer-visual");
            button.append(image);
        } else if (shape || color) {
            const visual = document.createElement("span");
            visual.className = shape ? `answer-shape answer-shape-${shape}` : "answer-color-swatch";
            if (color) visual.style.backgroundColor = color;
            visual.setAttribute("aria-hidden", "true");
            button.classList.add("has-answer-visual");
            button.append(visual);
        }
        const label = document.createElement("span");
        label.className = "answer-label";
        label.textContent = value;
        button.append(label);
    };
    const appendRepeatedVisuals = (container, value, count) => {
        container.replaceChildren();
        for (let index = 0; index < Number(count || 0); index += 1) {
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
                setAnswer(answer, selected.size > 0, !allowMultiple, 1200);
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
        target.className = "activity-drop-zone";
        const targetIcon = document.createElement("span");
        targetIcon.className = "material-symbols-outlined";
        targetIcon.textContent = "move_to_inbox";
        targetIcon.setAttribute("aria-hidden", "true");
        const targetLabel = document.createElement("strong");
        targetLabel.textContent = payload.targetLabel || "Thả vào đây";
        target.append(targetIcon, targetLabel);
        let activeValue = "";
        let dragGhost = null;

        const dropValue = (value) => {
            activeValue = value;
            playAnswerAudio(value);
            decorateButton(target, value);
            target.classList.add("filled");
            setAnswer(value, true, true, 1200);
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
            const button = createButton(choice, "activity-option draggable-option");
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
                const start = document.createElementNS("http://www.w3.org/2000/svg", "circle");
                const end = document.createElementNS("http://www.w3.org/2000/svg", "circle");
                [[start, leftRect.right - boardRect.left, leftRect.top + leftRect.height / 2 - boardRect.top],
                    [end, rightRect.left - boardRect.left, rightRect.top + rightRect.height / 2 - boardRect.top]]
                    .forEach(([circle, x, y]) => {
                        circle.setAttribute("cx", String(x));
                        circle.setAttribute("cy", String(y));
                        circle.setAttribute("r", "6");
                        circle.setAttribute("fill", activityColors[index % activityColors.length]);
                    });
                lines.append(line, start, end);
            });
        };

        pairs.forEach((pair, index) => {
            const button = createButton(pair.left);
            button.classList.remove("has-answer-marker");
            delete button.dataset.optionMarker;
            button.dataset.value = pair.left;
            button.style.setProperty("--selection-color", activityColors[index % activityColors.length]);
            button.addEventListener("click", () => {
                playAnswerAudio(pair.left);
                selectedLeft = pair.left;
                leftColumn.querySelectorAll("button").forEach((item) => item.classList.remove("selected"));
                button.classList.add("selected");
                rightColumn.querySelectorAll("button").forEach((item) => item.classList.add("target-ready"));
            });
            leftColumn.append(button);
        });
        rights.forEach((right, index) => {
            const button = createButton(right);
            button.classList.remove("has-answer-marker");
            delete button.dataset.optionMarker;
            button.dataset.value = right;
            button.style.setProperty("--selection-color", activityColors[index % activityColors.length]);
            button.addEventListener("click", () => {
                if (!selectedLeft) return;
                playAnswerAudio(right);
                Object.entries(mappings).forEach(([left, mappedRight]) => {
                    if (mappedRight === right && left !== selectedLeft) delete mappings[left];
                });
                mappings[selectedLeft] = right;
                leftColumn.querySelectorAll("button").forEach((item) => {
                    item.classList.toggle("matched", Object.hasOwn(mappings, item.dataset.value));
                    item.classList.remove("selected");
                });
                rightColumn.querySelectorAll("button").forEach((item) => {
                    item.classList.toggle("matched", Object.values(mappings).includes(item.dataset.value));
                });
                selectedLeft = "";
                rightColumn.querySelectorAll("button").forEach((item) => item.classList.remove("target-ready"));
                setAnswer(canonicalMappings(mappings), Object.keys(mappings).length === pairs.length);
                requestAnimationFrame(drawLines);
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
        const sync = () => setAnswer([...list.querySelectorAll(".ordering-value")].map((node) => node.textContent).join("|"));
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
                row.className = "ordering-row";
                row.draggable = true;
                row.dataset.index = String(index);
                const handle = document.createElement("span");
                handle.className = "material-symbols-outlined ordering-drag-handle";
                handle.textContent = "drag_indicator";
                handle.setAttribute("aria-hidden", "true");
                const value = document.createElement("strong");
                value.className = "ordering-value";
                value.textContent = item;
                const up = createButton("↑", "ordering-control");
                const down = createButton("↓", "ordering-control");
                up.disabled = index === 0;
                down.disabled = index === items.length - 1;
                up.setAttribute("aria-label", `Đưa ${item} lên`);
                down.setAttribute("aria-label", `Đưa ${item} xuống`);
                up.addEventListener("click", () => moveItem(index, index - 1));
                down.addEventListener("click", () => moveItem(index, index + 1));
                row.addEventListener("click", (event) => {
                    if (event.target.closest(".ordering-control")) return;
                    playAnswerAudio(item);
                });
                row.addEventListener("dragstart", () => { draggingIndex = index; row.classList.add("dragging"); });
                row.addEventListener("dragover", (event) => { event.preventDefault(); row.classList.add("drag-over"); });
                row.addEventListener("dragleave", () => row.classList.remove("drag-over"));
                row.addEventListener("drop", (event) => { event.preventDefault(); moveItem(draggingIndex, index); });
                row.addEventListener("dragend", () => { draggingIndex = -1; row.classList.remove("dragging"); });
                row.append(handle, value, up, down);
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
            const button = createButton(payload.objectSymbol || "●", "counting-object");
            button.addEventListener("click", () => {
                button.classList.toggle("counted");
                [...objects.children].forEach((item) => item.querySelector(".count-order")?.remove());
                [...objects.querySelectorAll(".counted")].forEach((item, countIndex) => {
                    const badge = document.createElement("span");
                    badge.className = "count-order";
                    badge.textContent = String(countIndex + 1);
                    item.append(badge);
                });
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
                setAnswer(String(choice), true, true, 1200);
            });
            choices.append(button);
        });
        runtime.append(objects, choices);
    };

    const renderQuantityBuilder = () => {
        let count = 0;
        const target = document.createElement("div");
        target.className = "quantity-target";
        const counter = document.createElement("strong");
        const objects = document.createElement("div");
        objects.className = "quantity-objects";
        const update = () => {
            counter.textContent = `${payload.targetLabel || "Đã có"}: ${count}/${payload.targetCount}`;
            appendRepeatedVisuals(objects, payload.objectSymbol || "●", count);
            setAnswer(String(count), count > 0);
        };
        const add = createButton(`Thêm ${payload.objectSymbol || "●"}`, "activity-option");
        const remove = createButton("Bớt một", "activity-option muted");
        add.addEventListener("click", () => { if (count < Number(payload.maxItems || 20)) count += 1; update(); });
        remove.addEventListener("click", () => { if (count > 0) count -= 1; update(); });
        const controls = document.createElement("div");
        controls.className = "quantity-controls";
        controls.append(add, remove);
        target.append(counter, objects);
        runtime.append(target, controls);
        update();
    };

    const renderComparison = () => {
        const board = document.createElement("div");
        board.className = "comparison-board";
        const group = (label, count, value) => {
            const button = createButton("", "comparison-group");
            const title = document.createElement("strong");
            title.textContent = label;
            const objects = document.createElement("span");
            objects.className = "comparison-objects";
            if (Number(count) === 0) objects.textContent = "=";
            else appendRepeatedVisuals(objects, payload.objectSymbol || "●", count);
            button.append(title, objects);
            button.addEventListener("click", () => { playAnswerAudio(label); board.querySelectorAll("button").forEach((item) => item.classList.remove("selected")); button.classList.add("selected"); setAnswer(value, true, true, 1200); });
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
        const source = document.createElement("div");
        source.className = "activity-option-grid";
        mappings.forEach((mapping, index) => {
            const button = createButton(mapping.left);
            button.style.setProperty("--selection-color", activityColors[index % activityColors.length]);
            button.addEventListener("click", () => {
                playAnswerAudio(mapping.left);
                selectedItem = mapping.left;
                source.querySelectorAll("button").forEach((item) => item.classList.remove("selected"));
                button.classList.add("selected");
            });
            source.append(button);
        });
        const categories = document.createElement("div");
        categories.className = "classification-zones";
        (payload.categories || []).forEach((category, categoryIndex) => {
            const button = createButton(category, "activity-drop-zone classification-zone");
            const assigned = document.createElement("span");
            assigned.className = "classification-assigned";
            button.append(assigned);
            const categoryColor = activityColors[categoryIndex % activityColors.length];
            button.style.setProperty("--category-color", categoryColor);
            button.addEventListener("click", () => {
                if (!selectedItem) return;
                playAnswerAudio(category);
                answers[selectedItem] = category;
                const itemButton = [...source.children].find((item) => item.textContent === selectedItem);
                itemButton.classList.add("matched");
                itemButton.classList.remove("selected");
                itemButton.style.setProperty("--selection-color", categoryColor);
                itemButton.dataset.category = category;
                categories.querySelectorAll(".classification-assigned").forEach((container) => {
                    container.querySelector(`[data-assigned-item="${CSS.escape(selectedItem)}"]`)?.remove();
                });
                const chip = document.createElement("span");
                chip.className = "classification-chip";
                chip.dataset.assignedItem = selectedItem;
                decorateButton(chip, selectedItem);
                assigned.append(chip);
                selectedItem = "";
                categories.querySelectorAll(".activity-drop-zone").forEach((zone) => zone.classList.remove("ready"));
                setAnswer(canonicalMappings(answers), Object.keys(answers).length === mappings.length);
            });
            categories.append(button);
        });
        source.addEventListener("click", () => {
            categories.querySelectorAll(".activity-drop-zone").forEach((zone) => zone.classList.toggle("ready", Boolean(selectedItem)));
        });
        runtime.append(source, categories);
    };

    const renderStoryChoice = () => {
        if (payload.audioUrl || payload.speechText) {
            const audioButton = createButton(type === "story_choice" ? "Nghe truyện" : "Nghe", "activity-audio-button");
            audioButton.addEventListener("click", playPromptAudio);
            runtime.append(audioButton);
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
