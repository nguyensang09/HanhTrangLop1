document.querySelectorAll("[data-confirm-delete]").forEach((form) => {
    form.addEventListener("submit", (event) => {
        const message = form.dataset.confirmMessage || "Xóa vĩnh viễn bài học và toàn bộ lịch sử làm bài liên quan?";
        if (!window.confirm(message)) {
            event.preventDefault();
        }
    });
});

document.querySelectorAll("[data-auto-submit-file]").forEach((form) => {
    const input = form.querySelector('input[type="file"]');
    input?.addEventListener("change", () => {
        if (input.files?.length) {
            form.requestSubmit();
        }
    });
});

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
    let uploadedImagePreviewUrl = "";
    const voiceCacheHost = document.querySelector("[data-voice-cache-json]");
    const imageAssetsHost = document.querySelector("[data-image-assets-json]");
    let voiceCacheEntries = [];
    let imageAssets = [];
    try {
        voiceCacheEntries = JSON.parse(voiceCacheHost?.textContent || "[]");
    } catch {
        voiceCacheEntries = [];
    }
    try {
        imageAssets = JSON.parse(imageAssetsHost?.textContent || "[]");
    } catch {
        imageAssets = [];
    }
    const normalizeLookupText = (value) => (value || "").replace(/\s+/g, " ").trim().toLocaleLowerCase("vi-VN");
    const readEntry = (entry, name) => entry?.[name] ?? entry?.[name.charAt(0).toUpperCase() + name.slice(1)] ?? "";
    const voiceByText = new Map(voiceCacheEntries
        .filter((entry) => readEntry(entry, "normalizedText"))
        .map((entry) => [normalizeLookupText(readEntry(entry, "normalizedText")), entry]));
    const readyVoiceEntries = voiceCacheEntries
        .filter((entry) => readEntry(entry, "status") === "ready" && readEntry(entry, "audioUrl"));
    const imageAssetEntries = imageAssets
        .filter((entry) => readEntry(entry, "storagePath"));
    const displayUsageType = (usageType) => ({
        "title": "Ti\u00eau \u0111\u1ec1",
        "instruction": "H\u01b0\u1edbng d\u1eabn",
        "question": "C\u00e2u h\u1ecfi",
        "correct-feedback": "Ph\u1ea3n h\u1ed3i \u0111\u00fang",
        "retry-feedback": "Ph\u1ea3n h\u1ed3i sai",
        "option": "\u0110\u00e1p \u00e1n",
        "content": "N\u1ed9i dung",
        "tracing-prompt": "T\u00f4 n\u00e9t",
        "legacy": "Voice c\u0169",
        "custom": "T\u1ef1 t\u1ea1o"
    }[usageType] || usageType || "Voice");

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

    const choiceFieldCopy = {
        single_choice: ["Các đáp án lựa chọn", "Nhập từ 2 đến 5 đáp án; bé chỉ chọn một đáp án trước khi bấm Kiểm tra."],
        multi_select: ["Các đáp án có thể chọn", "Nhập các đáp án hiển thị, sau đó khai báo riêng tập đáp án đúng bên dưới."],
        listen_choose: ["Đáp án sau khi nghe", "Các phương án phải khớp với nội dung âm thanh hoặc nội dung đọc tự động."],
        drag_drop: ["Vật có thể kéo", "Mỗi phương án là một vật hoặc nhãn; đáp án đúng là vật cần đưa vào vùng đích."],
        story_choice: ["Đáp án câu hỏi truyện", "Các phương án dùng để trả lời câu hỏi sau khi bé nghe truyện và xem tranh."]
    };

    const updateTemplateContext = () => {
        if (!interactionSelect) return;
        const type = interactionSelect.value;
        const selectedOption = interactionSelect.options[interactionSelect.selectedIndex];
        const configName = form.querySelector("[data-config-name]");
        const configDescription = form.querySelector("[data-config-description]");
        const choiceHeading = form.querySelector("[data-choice-heading]");
        const choiceHelp = form.querySelector("[data-choice-help]");
        const topicGuidance = form.querySelector("[data-topic-guidance]");
        const topicOption = topicSelect?.options[topicSelect.selectedIndex];
        const allowedNames = selectedTopicRule().allowedTypes.map((value) => templateNames[value] || value);

        if (configName) configName.textContent = templateNames[type] || "Cấu hình hoạt động";
        if (configDescription) configDescription.textContent = selectedOption?.dataset.description || "";
        if (choiceHeading) choiceHeading.textContent = choiceFieldCopy[type]?.[0] || "Phương án trả lời";
        if (choiceHelp) choiceHelp.textContent = choiceFieldCopy[type]?.[1] || "";
        if (topicGuidance) {
            topicGuidance.textContent = topicOption?.value
                ? `Chủ đề “${topicOption.textContent.trim()}” hỗ trợ: ${allowedNames.join(", ")}.`
                : "Chọn chủ đề để xem các mẫu hoạt động phù hợp.";
        }
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
        if (previewMedia) {
            const supportsImage = ["story_choice", "single_choice", "multi_select", "drag_drop", "matching", "classification"].includes(type);
            const supportsAudio = ["listen_choose", "story_choice"].includes(type);
            const imageSelect = form.querySelector('[name="ExistingImageAssetId"]');
            const selectedImagePath = imageSelect?.options[imageSelect.selectedIndex]?.dataset.path || "";
            const imageUrl = uploadedImagePreviewUrl || form.querySelector('[name="ImageUrl"]')?.value.trim() || selectedImagePath;
            previewMedia.hidden = !supportsImage && !supportsAudio;
            previewMedia.replaceChildren();
            if (supportsImage && imageUrl) {
                const image = document.createElement("img");
                image.src = imageUrl;
                image.alt = "Hình minh họa xem trước";
                previewMedia.append(image);
            } else {
                const icon = document.createElement("span");
                icon.className = "material-symbols-outlined";
                icon.textContent = supportsAudio ? "hearing" : "image";
                previewMedia.append(icon);
            }
        }
        if (!previewOptions) return;

        let labels = choiceInputs.map((input) => input.value.trim()).filter(Boolean);
        if (labels.length === 0) labels = ["A", "B", "C"];
        const itemMedia = new Map((form.querySelector('[name="ItemMediaText"]')?.value || "")
            .split(/\r?\n/)
            .map((line) => line.split(/=(.*)/s).slice(0, 2).map((value) => value.trim()))
            .filter((pair) => pair[0] && pair[1])
            .map(([label, url]) => [label.toLocaleLowerCase("vi-VN"), url]));
        const makePreviewItem = (text, className = "") => {
            const element = document.createElement("button");
            element.type = "button";
            element.className = className;
            const mediaUrl = itemMedia.get(String(text).trim().toLocaleLowerCase("vi-VN"));
            if (mediaUrl) {
                const image = document.createElement("img");
                image.className = "preview-item-media";
                image.src = mediaUrl;
                image.alt = String(text);
                const label = document.createElement("span");
                label.textContent = text;
                element.classList.add("has-preview-media");
                element.append(image, label);
            } else {
                element.textContent = text;
            }
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
            const leftLabel = form.querySelector('[name="LeftLabel"]')?.value || "Nhóm A";
            const rightLabel = form.querySelector('[name="RightLabel"]')?.value || "Nhóm B";
            previewOptions.append(makePreviewItem(`${leftLabel}\n${Array.from({length:left}, () => symbol).join(" ")}`, "preview-group"));
            previewOptions.append(makePreviewItem(`${rightLabel}\n${Array.from({length:right}, () => symbol).join(" ")}`, "preview-group"));
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

    const updateBuilderVoicePanel = () => {
        const panel = form.querySelector("[data-builder-voice-panel]");
        if (!panel) return;

        const list = panel.querySelector("[data-builder-voice-list]");
        const totalNode = panel.querySelector("[data-builder-voice-total]");
        const filledNode = panel.querySelector("[data-builder-voice-filled]");
        if (!list || !totalNode || !filledNode) return;

        const type = interactionSelect?.value || "single_choice";
        const read = (name) => form.querySelector(`[name="${name}"]`)?.value.trim() || "";
        const rows = [];
        const add = (kind, text, required = false) => {
            const normalized = (text || "").replace(/\s+/g, " ").trim();
            if (!required && !normalized) return;
            const key = `${kind}|${normalized.toLocaleLowerCase("vi-VN")}`;
            if (normalized && rows.some((row) => row.key === key)) return;
            rows.push({key, kind, text: normalized, required});
        };
        const addLines = (kind, value) => {
            (value || "")
                .split(/\r?\n/)
                .map((line) => line.trim())
                .filter(Boolean)
                .forEach((line) => add(kind, line));
        };
        const addMappings = (leftKind, rightKind, value) => {
            (value || "")
                .split(/\r?\n/)
                .map((line) => line.split("=").map((part) => part.trim()))
                .forEach((parts) => {
                    if (parts[0]) add(leftKind, parts[0]);
                    if (parts[1]) add(rightKind, parts[1]);
                });
        };

        add("Ti\u00eau \u0111\u1ec1", read("Title"), true);
        add("H\u01b0\u1edbng d\u1eabn", read("InstructionText"), true);
        add("C\u00e2u h\u1ecfi", read("PromptText"), true);
        add("Ph\u1ea3n h\u1ed3i \u0111\u00fang", read("CorrectFeedback"), true);
        add("Ph\u1ea3n h\u1ed3i sai", read("RetryFeedback"), true);

        choiceInputs.forEach((input, index) => add(`\u0110\u00e1p \u00e1n ${index + 1}`, input.value));

        if (["listen_choose", "story_choice"].includes(type)) {
            add("N\u1ed9i dung nghe", read("SpeechText"), true);
        }
        if (["drag_drop", "quantity_builder"].includes(type)) {
            add("V\u00f9ng \u0111\u00edch", read("TargetLabel"));
        }
        if (type === "matching") {
            addMappings("C\u1eb7p n\u1ed1i tr\u00e1i", "C\u1eb7p n\u1ed1i ph\u1ea3i", read("PairsText"));
        }
        if (type === "ordering") {
            addLines("Th\u1ee9 t\u1ef1", read("SequenceItemsText"));
        }
        if (type === "classification") {
            addMappings("V\u1eadt", "Nh\u00f3m", read("ClassificationText"));
        }
        if (["counting", "quantity_builder", "comparison"].includes(type)) {
            add("\u0110\u1ed3 v\u1eadt", read("ObjectSymbol"));
        }
        if (type === "comparison") {
            add("Nh\u00f3m tr\u00e1i", read("LeftLabel"));
            add("Nh\u00f3m ph\u1ea3i", read("RightLabel"));
        }

        const filled = rows.filter((row) => row.text).length;
        totalNode.textContent = String(rows.length);
        filledNode.textContent = String(filled);
        list.replaceChildren();

        rows.forEach((row, index) => {
            const item = document.createElement("div");
            item.className = `builder-voice-row ${row.text ? "" : "is-empty"}`;
            const match = row.text ? voiceByText.get(normalizeLookupText(row.text)) : null;
            const audioUrl = readEntry(match, "audioUrl");
            const status = readEntry(match, "status");
            const hasAudio = Boolean(audioUrl) && status === "ready";

            const kind = document.createElement("small");
            kind.textContent = row.kind;
            const text = document.createElement("span");
            text.textContent = row.text || "Ch\u01b0a nh\u1eadp text";
            const actions = document.createElement("div");
            actions.className = "builder-voice-actions";
            const updateEntryAudio = (entry, result) => {
                entry.audioUrl = result.audioUrl;
                entry.AudioUrl = result.audioUrl;
                entry.status = result.status;
                entry.Status = result.status;
                if (result.audioUrl && result.status === "ready") {
                    setTimeout(updateBuilderVoicePanel, 0);
                }
                const audio = actions.querySelector("audio");
                if (audio) {
                    audio.src = `${result.audioUrl}?v=${Date.now()}`;
                    audio.load();
                }
            };
            const makeInlineTools = (entry) => {
                const uploadBox = document.createElement("div");
                uploadBox.className = "builder-voice-tools";
                const token = form.querySelector('input[name="__RequestVerificationToken"]')?.value || "";

                const listId = `builder-voice-store-${index}`;
                const voiceByLabel = new Map();
                const picker = document.createElement("input");
                picker.className = "form-control";
                picker.setAttribute("list", listId);
                picker.placeholder = "Tìm kho voice";
                const datalist = document.createElement("datalist");
                datalist.id = listId;
                readyVoiceEntries.slice(0, 200).forEach((voice) => {
                    const option = document.createElement("option");
                    const label = `${displayUsageType(readEntry(voice, "usageType"))} - ${readEntry(voice, "normalizedText")}`;
                    option.value = label;
                    option.label = readEntry(voice, "audioUrl");
                    voiceByLabel.set(label, readEntry(voice, "id"));
                    datalist.append(option);
                });
                picker.addEventListener("change", async () => {
                    const sourceId = voiceByLabel.get(picker.value);
                    if (!sourceId) return;
                    picker.disabled = true;
                    try {
                        const data = new FormData();
                        data.append("sourceId", sourceId);
                        data.append("__RequestVerificationToken", token);
                        const response = await fetch(`/admin/voice-cache/${encodeURIComponent(readEntry(entry, "id"))}/copy-inline`, {
                            method: "POST",
                            headers: {"RequestVerificationToken": token},
                            body: data,
                            credentials: "same-origin"
                        });
                        if (!response.ok) {
                            const error = await response.json().catch(() => ({message: "Kh\u00f4ng th\u1ec3 ch\u1ecdn voice t\u1eeb kho."}));
                            throw new Error(error.message || "Kh\u00f4ng th\u1ec3 ch\u1ecdn voice t\u1eeb kho.");
                        }
                        updateEntryAudio(entry, await response.json());
                    } catch (error) {
                        window.alert(error.message);
                    } finally {
                        picker.value = "";
                        picker.disabled = false;
                    }
                });

                const fileInput = document.createElement("input");
                fileInput.type = "file";
                fileInput.accept = "audio/*,.mp3,.wav,.m4a";
                fileInput.addEventListener("change", async () => {
                    const file = fileInput.files?.[0];
                    if (!file) {
                        return;
                    }
                    const data = new FormData();
                    data.append("audioFile", file);
                    data.append("__RequestVerificationToken", token);
                    fileInput.disabled = true;
                    try {
                        const response = await fetch(`/admin/voice-cache/${encodeURIComponent(readEntry(entry, "id"))}/upload-inline`, {
                            method: "POST",
                            headers: {"RequestVerificationToken": token},
                            body: data,
                            credentials: "same-origin"
                        });
                        if (!response.ok) {
                            const error = await response.json().catch(() => ({message: "Kh\u00f4ng th\u1ec3 l\u01b0u file voice."}));
                            throw new Error(error.message || "Kh\u00f4ng th\u1ec3 l\u01b0u file voice.");
                        }
                        updateEntryAudio(entry, await response.json());
                    } catch (error) {
                        window.alert(error.message);
                    } finally {
                        fileInput.value = "";
                        fileInput.disabled = false;
                    }
                });
                uploadBox.append(picker, datalist, fileInput);
                return uploadBox;
            };

            if (!row.text) {
                const chip = document.createElement("strong");
                chip.className = "builder-voice-chip";
                chip.textContent = "Thi\u1ebfu text";
                actions.append(chip);
            } else if (hasAudio) {
                const audio = document.createElement("audio");
                audio.controls = true;
                audio.preload = "none";
                audio.src = audioUrl;
                actions.append(audio, makeInlineTools(match));
            } else if (match) {
                const chip = document.createElement("strong");
                chip.className = "builder-voice-chip";
                chip.textContent = "Thi\u1ebfu file";
                actions.append(chip, makeInlineTools(match));
            } else {
                const link = document.createElement("a");
                link.className = "mini-action";
                link.href = `/admin/voice-cache?status=missing&q=${encodeURIComponent(row.text)}`;
                link.target = "_blank";
                link.rel = "noreferrer";
                link.textContent = "T\u1ea1o file";
                const chip = document.createElement("strong");
                chip.className = "builder-voice-chip";
                chip.textContent = "Thi\u1ebfu file";
                actions.append(chip, link);
            }

            item.append(kind, text, actions);
            list.append(item);
        });
    };

    const collectMediaLabels = () => {
        const type = interactionSelect?.value || "single_choice";
        const labels = [];
        const add = (value) => {
            const text = (value || "").replace(/\s+/g, " ").trim();
            if (text && !labels.some((item) => normalizeLookupText(item) === normalizeLookupText(text))) {
                labels.push(text);
            }
        };
        const addLines = (value) => (value || "")
            .split(/\r?\n/)
            .map((line) => line.trim())
            .filter(Boolean)
            .forEach(add);
        const addMappings = (value) => (value || "")
            .split(/\r?\n/)
            .map((line) => line.split("=").map((part) => part.trim()))
            .forEach((parts) => {
                if (parts[0]) add(parts[0]);
                if (parts[1]) add(parts[1]);
            });

        if (["single_choice", "multi_select", "listen_choose", "drag_drop", "story_choice", "counting"].includes(type)) {
            choiceInputs.forEach((input) => add(input.value));
        }
        if (["drag_drop", "quantity_builder"].includes(type)) {
            add(form.querySelector('[name="TargetLabel"]')?.value);
        }
        if (type === "matching") addMappings(form.querySelector('[name="PairsText"]')?.value);
        if (type === "ordering") addLines(form.querySelector('[name="SequenceItemsText"]')?.value);
        if (type === "classification") addMappings(form.querySelector('[name="ClassificationText"]')?.value);
        if (type === "comparison") {
            add(form.querySelector('[name="LeftLabel"]')?.value);
            add(form.querySelector('[name="RightLabel"]')?.value);
        }
        return labels;
    };

    const parseItemMediaText = () => new Map((form.querySelector('[name="ItemMediaText"]')?.value || "")
        .split(/\r?\n/)
        .map((line) => line.split(/=(.*)/s).slice(0, 2).map((value) => value.trim()))
        .filter((pair) => pair[0])
        .map(([label, url]) => [normalizeLookupText(label), {label, url: url || ""}]));

    const syncItemMediaTextFromBuilder = () => {
        const rows = [...form.querySelectorAll("[data-item-media-row]")];
        const textArea = form.querySelector('[name="ItemMediaText"]');
        if (!textArea || rows.length === 0) return;
        textArea.value = rows
            .map((row) => {
                const label = row.querySelector("[data-item-media-label]")?.textContent?.trim() || "";
                const url = row.querySelector("[data-item-media-url]")?.value.trim() || "";
                return label && url ? `${label} = ${url}` : "";
            })
            .filter(Boolean)
            .join("\n");
    };

    const renderItemMediaBuilder = () => {
        const list = form.querySelector("[data-item-media-list]");
        if (!list) return;
        const currentMap = parseItemMediaText();
        const labels = collectMediaLabels();
        list.replaceChildren();
        if (labels.length === 0) {
            const empty = document.createElement("p");
            empty.className = "empty-note";
            empty.textContent = "Nh\u1eadp \u0111\u00e1p \u00e1n ho\u1eb7c n\u1ed9i dung b\u00e0i \u0111\u1ec3 g\u1ee3i \u00fd nh\u00e3n h\u00ecnh.";
            list.append(empty);
            return;
        }
        labels.forEach((label, index) => {
            const row = document.createElement("div");
            row.className = "item-media-row";
            row.dataset.itemMediaRow = "true";

            const name = document.createElement("span");
            name.dataset.itemMediaLabel = "true";
            name.textContent = label;

            const listId = `item-media-store-${index}`;
            const input = document.createElement("input");
            input.className = "form-control";
            input.dataset.itemMediaUrl = "true";
            input.setAttribute("list", listId);
            input.placeholder = "Chọn ảnh kho hoặc dán đường dẫn";
            input.value = currentMap.get(normalizeLookupText(label))?.url || "";
            input.addEventListener("input", () => {
                syncItemMediaTextFromBuilder();
                updateBuilderPreview();
            });

            const datalist = document.createElement("datalist");
            datalist.id = listId;
            imageAssetEntries.forEach((asset) => {
                const option = document.createElement("option");
                option.value = readEntry(asset, "storagePath");
                option.label = `${readEntry(asset, "fileName") || readEntry(asset, "storagePath")} ${readEntry(asset, "altText") || ""}`.trim();
                datalist.append(option);
            });

            row.append(name, input, datalist);
            list.append(row);
        });
        syncItemMediaTextFromBuilder();
    };

    skillGroupSelect?.addEventListener("change", filterTopics);
    topicSelect?.addEventListener("change", filterTemplates);
    interactionSelect?.addEventListener("change", () => {
        applyTemplateDefaults();
        toggleInteractionFields();
        updateTemplateContext();
        updateBuilderPreview();
        updateBuilderVoicePanel();
        renderItemMediaBuilder();
    });
    templateButtons.forEach((button) => button.addEventListener("click", () => {
        interactionSelect.value = button.dataset.templateOption;
        interactionSelect.dispatchEvent(new Event("change", {bubbles: true}));
    }));
    choiceInputs.forEach((input) => input.addEventListener("input", () => {
        syncCorrectAnswer();
        updateBuilderPreview();
        updateBuilderVoicePanel();
        renderItemMediaBuilder();
    }));
    form.querySelectorAll("input, textarea").forEach((input) => input.addEventListener("input", () => {
        updateBuilderPreview();
        updateBuilderVoicePanel();
        if (!input.matches("[data-item-media-url]")) {
            renderItemMediaBuilder();
        }
    }));
    form.querySelectorAll("select").forEach((select) => select.addEventListener("change", () => {
        updateBuilderPreview();
        updateBuilderVoicePanel();
        renderItemMediaBuilder();
    }));
    form.querySelector("[data-item-media-refresh]")?.addEventListener("click", renderItemMediaBuilder);
    form.querySelector('[name="ImageFile"]')?.addEventListener("change", (event) => {
        if (uploadedImagePreviewUrl) URL.revokeObjectURL(uploadedImagePreviewUrl);
        uploadedImagePreviewUrl = event.target.files?.[0] ? URL.createObjectURL(event.target.files[0]) : "";
        updateBuilderPreview();
        updateBuilderVoicePanel();
    });

    filterTopics();
    syncCorrectAnswer();
    toggleInteractionFields();
    updateTemplateContext();
    updateBuilderPreview();
    updateBuilderVoicePanel();
    renderItemMediaBuilder();
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
