document.querySelectorAll("[data-admin-learning-form]").forEach((form) => {
    const skillGroupSelect = form.querySelector("[data-skill-group-select]");
    const topicSelect = form.querySelector("[data-topic-select]");
    const interactionSelect = form.querySelector("[data-interaction-select]");
    const choiceFields = form.querySelector("[data-choice-fields]");
    const tracingFields = form.querySelector("[data-tracing-fields]");
    const choiceInputs = [...form.querySelectorAll("[data-choice-input]")];
    const correctAnswerSelect = form.querySelector("[data-correct-answer]");
    const builderFieldGroups = [...form.querySelectorAll("[data-builder-fields]")];
    const templateButtons = [...form.querySelectorAll("[data-template-option]")];
    const tracingLink = form.querySelector("[data-tracing-link]");
    const isEditing = Boolean(form.querySelector('[name="Id"]')?.value);

    const selectedTopicRule = () => {
        const option = topicSelect?.options[topicSelect.selectedIndex];
        return {
            allowedTypes: (option?.dataset.allowedTypes || "").split(",").filter(Boolean),
            allowsTracing: option?.dataset.allowsTracing === "true"
        };
    };

    const filterTemplates = () => {
        if (!interactionSelect || !topicSelect) return;
        const rule = selectedTopicRule();

        [...interactionSelect.options].forEach((option) => {
            const allowed = rule.allowedTypes.includes(option.value);
            option.hidden = !allowed;
            option.disabled = !allowed;
        });
        templateButtons.forEach((button) => {
            button.hidden = !rule.allowedTypes.includes(button.dataset.templateOption);
        });

        if (!rule.allowedTypes.includes(interactionSelect.value)) {
            interactionSelect.value = rule.allowedTypes[0] || "";
            interactionSelect.dispatchEvent(new Event("change", {bubbles: true}));
        }

        if (tracingLink) {
            tracingLink.hidden = !rule.allowsTracing;
            const selectedTopicId = topicSelect.value;
            const selectedGroupId = skillGroupSelect?.value || "";
            tracingLink.href = `/admin/learning-items/create-tracing?skillGroupId=${encodeURIComponent(selectedGroupId)}&topicId=${encodeURIComponent(selectedTopicId)}`;
        }
    };

    const filterTopics = () => {
        if (!skillGroupSelect || !topicSelect) {
            return;
        }

        const selectedGroupId = skillGroupSelect.value;
        const selectedTopic = topicSelect.options[topicSelect.selectedIndex];
        const selectedTopicIsValid = selectedTopic?.value &&
            selectedTopic.dataset.skillGroupId === selectedGroupId &&
            (!form.matches("[data-activity-builder]") || (selectedTopic.dataset.allowedTypes || "").length > 0);

        [...topicSelect.options].forEach((option) => {
            const hasActivity = !form.matches("[data-activity-builder]") || !option.value || (option.dataset.allowedTypes || "").length > 0;
            const isVisible = (!option.value || option.dataset.skillGroupId === selectedGroupId) && hasActivity;
            option.hidden = !isVisible;
            option.disabled = !isVisible;
        });

        if (!selectedTopicIsValid) {
            const firstTopic = [...topicSelect.options]
                .find((option) => option.value && option.dataset.skillGroupId === selectedGroupId &&
                    (!form.matches("[data-activity-builder]") || (option.dataset.allowedTypes || "").length > 0));
            topicSelect.value = firstTopic?.value || "";
        }
        filterTemplates();
    };

    const syncCorrectAnswer = () => {
        if (!correctAnswerSelect) {
            return;
        }

        const currentValue = correctAnswerSelect.value;
        const choices = [...new Set(choiceInputs.map((input) => input.value.trim()).filter(Boolean))];
        correctAnswerSelect.replaceChildren();

        const emptyOption = document.createElement("option");
        emptyOption.value = "";
        emptyOption.textContent = "Chọn đáp án đúng";
        correctAnswerSelect.append(emptyOption);

        choices.forEach((choice) => {
            const option = document.createElement("option");
            option.value = choice;
            option.textContent = choice;
            correctAnswerSelect.append(option);
        });

        correctAnswerSelect.value = choices.includes(currentValue) ? currentValue : "";
    };

    const toggleInteractionFields = () => {
        if (!interactionSelect) {
            return;
        }

        const isTracing = interactionSelect.value === "tracing";
        if (choiceFields) {
            choiceFields.hidden = isTracing;
            choiceFields.querySelectorAll("input, select").forEach((field) => field.disabled = isTracing);
        }
        if (tracingFields) {
            tracingFields.hidden = !isTracing;
            tracingFields.querySelectorAll("input, select").forEach((field) => field.disabled = !isTracing);
        }


        builderFieldGroups.forEach((group) => {
            const supportedTypes = group.dataset.builderFields.split(",");
            const isVisible = supportedTypes.includes(interactionSelect.value);
            group.hidden = !isVisible;
            group.querySelectorAll("input, select, textarea").forEach((field) => field.disabled = !isVisible);
        });

        templateButtons.forEach((button) => {
            button.classList.toggle("active", button.dataset.templateOption === interactionSelect.value);
        });
    };

    const templateNames = {
        single_choice: "Chọn một đáp án",
        multi_select: "Chọn nhiều đáp án",
        listen_choose: "Nghe và chọn",
        drag_drop: "Kéo vào vùng đích",
        matching: "Nối cặp",
        ordering: "Sắp xếp",
        counting: "Đếm đồ vật",
        quantity_builder: "Tạo đúng số lượng",
        comparison: "So sánh hai nhóm",
        classification: "Phân loại",
        story_choice: "Nghe truyện và chọn"
    };

    const applyTemplateDefaults = () => {
        if (!interactionSelect || isEditing) return;
        const option = interactionSelect.options[interactionSelect.selectedIndex];
        const instructionInput = form.querySelector('[data-preview-source="instruction"]');
        const promptInput = form.querySelector('[data-preview-source="prompt"]');
        if (instructionInput && option?.dataset.defaultInstruction) instructionInput.value = option.dataset.defaultInstruction;
        if (promptInput && option?.dataset.defaultPrompt) promptInput.value = option.dataset.defaultPrompt;
    };

    const updateBuilderPreview = () => {
        if (!form.matches("[data-activity-builder]")) {
            return;
        }

        const type = interactionSelect?.value || "single_choice";
        const instruction = form.querySelector('[data-preview-source="instruction"]')?.value.trim();
        const prompt = form.querySelector('[data-preview-source="prompt"]')?.value.trim();
        const previewInstruction = form.querySelector("[data-preview-instruction]");
        const previewPrompt = form.querySelector("[data-preview-prompt]");
        const previewOptions = form.querySelector("[data-preview-options]");
        const previewName = form.querySelector("[data-preview-template-name]");
        const previewMedia = form.querySelector("[data-preview-media]");

        if (previewInstruction) previewInstruction.textContent = instruction || "Lời hướng dẫn của bài học";
        if (previewPrompt) previewPrompt.textContent = prompt || "Câu hỏi sẽ hiển thị tại đây";
        if (previewName) previewName.textContent = templateNames[type] || type;
        if (previewMedia) previewMedia.hidden = !["story_choice", "single_choice", "multi_select", "drag_drop", "matching", "classification"].includes(type);
        if (!previewOptions) return;

        let labels = choiceInputs.map((input) => input.value.trim()).filter(Boolean);
        if (labels.length === 0) labels = ["A", "B", "C"];
        const makePreviewItem = (text, className = "") => {
            const element = document.createElement("button");
            element.type = "button";
            element.className = className;
            element.textContent = text;
            return element;
        };
        previewOptions.className = `builder-preview-options preview-${type}`;
        previewOptions.replaceChildren();

        if (type === "matching") {
            const pairs = (form.querySelector('[name="PairsText"]')?.value || "A = a\nB = b")
                .split(/\r?\n/).filter(Boolean).map((line) => line.split("=").map((value) => value.trim()));
            pairs.slice(0, 4).forEach((pair) => {
                const row = document.createElement("div");
                row.className = "preview-pair";
                row.append(makePreviewItem(pair[0] || "?"), document.createTextNode("↔"), makePreviewItem(pair[1] || "?"));
                previewOptions.append(row);
            });
        } else if (type === "ordering") {
            const items = (form.querySelector('[name="SequenceItemsText"]')?.value || "1\n2\n3")
                .split(/\r?\n/).map((value) => value.trim()).filter(Boolean);
            items.slice(0, 5).forEach((item, index) => previewOptions.append(makePreviewItem(`${index + 1}. ${item}`, "preview-order-item")));
        } else if (type === "drag_drop") {
            labels.slice(0, 4).forEach((label) => previewOptions.append(makePreviewItem(label)));
            previewOptions.append(makePreviewItem(form.querySelector('[name="TargetLabel"]')?.value || "Vùng đích", "preview-drop-zone"));
        } else if (type === "counting") {
            const symbol = form.querySelector('[name="ObjectSymbol"]')?.value || "●";
            const count = Math.min(8, Number(form.querySelector('[name="TargetCount"]')?.value || 4));
            const objects = document.createElement("div");
            objects.className = "preview-objects";
            objects.textContent = Array.from({length: count}, () => symbol).join(" ");
            previewOptions.append(objects);
            labels.slice(0, 4).forEach((label) => previewOptions.append(makePreviewItem(label)));
        } else if (type === "quantity_builder") {
            const symbol = form.querySelector('[name="ObjectSymbol"]')?.value || "●";
            const count = Number(form.querySelector('[name="TargetCount"]')?.value || 4);
            const target = document.createElement("div");
            target.className = "preview-quantity-target";
            target.textContent = `${form.querySelector('[name="TargetLabel"]')?.value || "Vùng đích"}: 0/${count}`;
            previewOptions.append(target, makePreviewItem(`Thêm ${symbol}`), makePreviewItem("Bớt một"));
        } else if (type === "comparison") {
            const symbol = form.querySelector('[name="ObjectSymbol"]')?.value || "●";
            const left = Math.min(8, Number(form.querySelector('[name="TargetCount"]')?.value || 4));
            const right = Math.min(8, Number(form.querySelector('[name="SecondaryCount"]')?.value || 2));
            previewOptions.append(makePreviewItem(`A\n${Array.from({length:left}, () => symbol).join(" ")}`, "preview-group"));
            previewOptions.append(makePreviewItem(`B\n${Array.from({length:right}, () => symbol).join(" ")}`, "preview-group"));
        } else if (type === "classification") {
            const mappings = (form.querySelector('[name="ClassificationText"]')?.value || "Táo = Trái cây\nCà rốt = Rau củ")
                .split(/\r?\n/).map((line) => line.split("=").map((value) => value.trim())).filter((pair) => pair[0]);
            const sources = document.createElement("div");
            sources.className = "preview-classification-source";
            mappings.forEach((pair) => sources.append(makePreviewItem(pair[0])));
            const zones = document.createElement("div");
            zones.className = "preview-classification-zones";
            [...new Set(mappings.map((pair) => pair[1]).filter(Boolean))].forEach((category) => zones.append(makePreviewItem(category, "preview-drop-zone")));
            previewOptions.append(sources, zones);
        } else {
            labels.slice(0, 5).forEach((label) => previewOptions.append(makePreviewItem(label)));
        }
    };

    skillGroupSelect?.addEventListener("change", filterTopics);
    topicSelect?.addEventListener("change", filterTemplates);
    interactionSelect?.addEventListener("change", () => {
        applyTemplateDefaults();
        toggleInteractionFields();
        updateBuilderPreview();
    });
    templateButtons.forEach((button) => button.addEventListener("click", () => {
        interactionSelect.value = button.dataset.templateOption;
        interactionSelect.dispatchEvent(new Event("change", {bubbles: true}));
    }));
    choiceInputs.forEach((input) => input.addEventListener("input", () => {
        syncCorrectAnswer();
        updateBuilderPreview();
    }));
    form.querySelectorAll("input, textarea").forEach((input) => input.addEventListener("input", updateBuilderPreview));

    filterTopics();
    syncCorrectAnswer();
    toggleInteractionFields();
    updateBuilderPreview();
});

document.querySelectorAll("[data-tracing-builder]").forEach((form) => {
    const symbolInput = form.querySelector("[data-tracing-symbol-input]");
    const symbolPreview = form.querySelector("[data-tracing-preview-symbol]");
    const guideMode = form.querySelector("[data-tracing-guide-mode]");
    const startToggle = form.querySelector("[data-tracing-start-toggle]");
    const startPoint = form.querySelector("[data-tracing-preview-start]");

    const updateTracingPreview = () => {
        if (symbolPreview) {
            symbolPreview.textContent = symbolInput?.value.trim() || "?";
            symbolPreview.hidden = guideMode?.value === "free";
        }
        if (startPoint) {
            startPoint.hidden = !startToggle?.checked;
        }
    };

    symbolInput?.addEventListener("input", updateTracingPreview);
    guideMode?.addEventListener("change", updateTracingPreview);
    startToggle?.addEventListener("change", updateTracingPreview);
    updateTracingPreview();
});
