(() => {
    const runtime = document.querySelector("[data-activity-runtime]");
    const payloadElement = document.querySelector("[data-activity-payload]");
    const answerInput = document.querySelector("[data-activity-answer]");
    const submitButton = document.querySelector("[data-activity-submit]");
    if (!runtime || !payloadElement || !answerInput || !submitButton) return;

    let payload = {};
    try {
        payload = JSON.parse(payloadElement.value || "{}");
    } catch {
        runtime.textContent = "Nội dung bài học chưa đúng định dạng.";
        return;
    }

    const type = runtime.dataset.activityType;
    const createButton = (text, className = "activity-option") => {
        const button = document.createElement("button");
        button.type = "button";
        button.className = className;
        button.textContent = text;
        return button;
    };
    const setAnswer = (value, ready = true) => {
        answerInput.value = value;
        submitButton.disabled = !ready;
    };
    const canonicalMappings = (mappings) => Object.entries(mappings)
        .sort(([leftA], [leftB]) => leftA < leftB ? -1 : leftA > leftB ? 1 : 0)
        .map(([left, right]) => `${left}=>${right}`).join("|");

    const renderMultiSelect = () => {
        const selected = new Set();
        const grid = document.createElement("div");
        grid.className = "activity-option-grid";
        (payload.choices || []).forEach((choice) => {
            const button = createButton(choice);
            button.addEventListener("click", () => {
                selected.has(choice) ? selected.delete(choice) : selected.add(choice);
                button.classList.toggle("selected", selected.has(choice));
                setAnswer([...selected].sort().join("|"), selected.size > 0);
            });
            grid.append(button);
        });
        runtime.append(grid);
    };

    const renderDragDrop = () => {
        const source = document.createElement("div");
        source.className = "activity-option-grid drag-source";
        const target = document.createElement("button");
        target.type = "button";
        target.className = "activity-drop-zone";
        target.textContent = payload.targetLabel || "Thả vào đây";
        let activeValue = "";

        const dropValue = (value) => {
            activeValue = value;
            target.textContent = value;
            target.classList.add("filled");
            setAnswer(value);
        };
        (payload.choices || []).forEach((choice) => {
            const button = createButton(choice, "activity-option draggable-option");
            button.draggable = true;
            button.addEventListener("click", () => {
                activeValue = choice;
                source.querySelectorAll("button").forEach((item) => item.classList.remove("selected"));
                button.classList.add("selected");
            });
            button.addEventListener("dragstart", (event) => event.dataTransfer.setData("text/plain", choice));
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
        const leftColumn = document.createElement("div");
        const rightColumn = document.createElement("div");
        const rights = pairs.map((pair) => pair.right).reverse();

        pairs.forEach((pair) => {
            const button = createButton(pair.left);
            button.addEventListener("click", () => {
                selectedLeft = pair.left;
                leftColumn.querySelectorAll("button").forEach((item) => item.classList.remove("selected"));
                button.classList.add("selected");
            });
            leftColumn.append(button);
        });
        rights.forEach((right) => {
            const button = createButton(right);
            button.addEventListener("click", () => {
                if (!selectedLeft) return;
                mappings[selectedLeft] = right;
                const leftButton = [...leftColumn.children].find((item) => item.textContent === selectedLeft);
                leftButton.classList.add("matched");
                leftButton.classList.remove("selected");
                selectedLeft = "";
                setAnswer(canonicalMappings(mappings), Object.keys(mappings).length === pairs.length);
            });
            rightColumn.append(button);
        });
        board.append(leftColumn, rightColumn);
        runtime.append(board);
    };

    const renderOrdering = () => {
        const items = [...(payload.items || [])].reverse();
        const list = document.createElement("div");
        list.className = "ordering-list";
        const sync = () => setAnswer([...list.querySelectorAll(".ordering-value")].map((node) => node.textContent).join("|"));
        const draw = () => {
            list.replaceChildren();
            items.forEach((item, index) => {
                const row = document.createElement("div");
                row.className = "ordering-row";
                const value = document.createElement("strong");
                value.className = "ordering-value";
                value.textContent = item;
                const up = createButton("↑", "ordering-control");
                const down = createButton("↓", "ordering-control");
                up.disabled = index === 0;
                down.disabled = index === items.length - 1;
                up.addEventListener("click", () => { [items[index - 1], items[index]] = [items[index], items[index - 1]]; draw(); });
                down.addEventListener("click", () => { [items[index], items[index + 1]] = [items[index + 1], items[index]]; draw(); });
                row.append(value, up, down);
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
            button.addEventListener("click", () => button.classList.toggle("counted"));
            objects.append(button);
        }
        const choices = document.createElement("div");
        choices.className = "activity-option-grid";
        (payload.choices || []).forEach((choice) => {
            const button = createButton(choice);
            button.addEventListener("click", () => {
                choices.querySelectorAll("button").forEach((item) => item.classList.remove("selected"));
                button.classList.add("selected");
                setAnswer(String(choice));
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
            objects.textContent = Array.from({length: count}, () => payload.objectSymbol || "●").join(" ");
            setAnswer(String(count), count > 0);
        };
        const add = createButton(`Thêm ${payload.objectSymbol || "●"}`, "activity-option");
        const remove = createButton("Bớt một", "activity-option muted");
        add.addEventListener("click", () => { if (count < Number(payload.maxItems || 20)) count += 1; update(); });
        remove.addEventListener("click", () => { if (count > 0) count -= 1; update(); });
        target.append(counter, objects);
        runtime.append(target, add, remove);
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
            objects.textContent = Array.from({length:Number(count)}, () => payload.objectSymbol || "●").join(" ") || "0";
            button.append(title, objects);
            button.addEventListener("click", () => { board.querySelectorAll("button").forEach((item) => item.classList.remove("selected")); button.classList.add("selected"); setAnswer(value); });
            return button;
        };
        board.append(group("Nhóm A", payload.leftCount, "left"), group("Bằng nhau", 0, "equal"), group("Nhóm B", payload.rightCount, "right"));
        runtime.append(board);
    };

    const renderClassification = () => {
        const mappings = payload.mappings || [];
        const answers = {};
        let selectedItem = "";
        const source = document.createElement("div");
        source.className = "activity-option-grid";
        mappings.forEach((mapping) => {
            const button = createButton(mapping.left);
            button.addEventListener("click", () => {
                selectedItem = mapping.left;
                source.querySelectorAll("button").forEach((item) => item.classList.remove("selected"));
                button.classList.add("selected");
            });
            source.append(button);
        });
        const categories = document.createElement("div");
        categories.className = "classification-zones";
        (payload.categories || []).forEach((category) => {
            const button = createButton(category, "activity-drop-zone");
            button.addEventListener("click", () => {
                if (!selectedItem) return;
                answers[selectedItem] = category;
                const itemButton = [...source.children].find((item) => item.textContent === selectedItem);
                itemButton.classList.add("matched");
                itemButton.classList.remove("selected");
                selectedItem = "";
                setAnswer(canonicalMappings(answers), Object.keys(answers).length === mappings.length);
            });
            categories.append(button);
        });
        runtime.append(source, categories);
    };

    const renderStoryChoice = () => {
        if (payload.audioUrl) {
            const audioButton = createButton("Nghe câu chuyện", "activity-audio-button");
            const audio = new Audio(payload.audioUrl);
            audioButton.addEventListener("click", () => audio.play());
            runtime.append(audioButton);
        }
        const choices = document.createElement("div");
        choices.className = "activity-option-grid";
        (payload.choices || []).forEach((choice) => {
            const button = createButton(choice);
            button.addEventListener("click", () => { choices.querySelectorAll("button").forEach((item) => item.classList.remove("selected")); button.classList.add("selected"); setAnswer(choice); });
            choices.append(button);
        });
        runtime.append(choices);
    };

    const renderers = {
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
