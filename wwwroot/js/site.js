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

    const filterTopics = () => {
        if (!skillGroupSelect || !topicSelect) {
            return;
        }

        const selectedGroupId = skillGroupSelect.value;
        const selectedTopic = topicSelect.options[topicSelect.selectedIndex];
        const selectedTopicIsValid = selectedTopic?.value && selectedTopic.dataset.skillGroupId === selectedGroupId;

        [...topicSelect.options].forEach((option) => {
            const isVisible = !option.value || option.dataset.skillGroupId === selectedGroupId;
            option.hidden = !isVisible;
            option.disabled = !isVisible;
        });

        if (!selectedTopicIsValid) {
            const firstTopic = [...topicSelect.options]
                .find((option) => option.value && option.dataset.skillGroupId === selectedGroupId);
            topicSelect.value = firstTopic?.value || "";
        }
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
        if (previewMedia) previewMedia.hidden = type !== "story_choice";
        if (!previewOptions) return;

        let labels = choiceInputs.map((input) => input.value.trim()).filter(Boolean);
        if (type === "matching") {
            labels = (form.querySelector('[name="PairsText"]')?.value || "A = a\nB = b")
                .split(/\r?\n/).filter(Boolean).flatMap((line) => line.split("=").map((value) => value.trim()));
        } else if (type === "ordering") {
            labels = (form.querySelector('[name="SequenceItemsText"]')?.value || "1\n2\n3")
                .split(/\r?\n/).map((value) => value.trim()).filter(Boolean);
        } else if (["counting", "quantity_builder", "comparison"].includes(type)) {
            const symbol = form.querySelector('[name="ObjectSymbol"]')?.value || "●";
            const count = Math.min(8, Number(form.querySelector('[name="TargetCount"]')?.value || 4));
            labels = Array.from({length: count}, () => symbol);
        } else if (type === "classification") {
            labels = (form.querySelector('[name="ClassificationText"]')?.value || "Táo = Trái cây\nCà rốt = Rau củ")
                .split(/\r?\n/).map((line) => line.split("=")[0].trim()).filter(Boolean);
        }

        if (labels.length === 0) labels = ["A", "B", "C"];
        previewOptions.replaceChildren(...labels.slice(0, 8).map((label) => {
            const button = document.createElement("button");
            button.type = "button";
            button.textContent = label;
            return button;
        }));
    };

    skillGroupSelect?.addEventListener("change", filterTopics);
    interactionSelect?.addEventListener("change", () => {
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
