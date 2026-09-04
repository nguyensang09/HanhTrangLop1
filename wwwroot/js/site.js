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
    const normalizeLookupText = (value) => (value || "").replace(/[?!.,:;]/g, "").replace(/\s+/g, " ").trim().toLocaleLowerCase("vi-VN");
    const readEntry = (entry, name) => entry?.[name] ?? entry?.[name.charAt(0).toUpperCase() + name.slice(1)] ?? "";
    const voiceByText = new Map();
    voiceCacheEntries.forEach((entry) => {
        const norm = readEntry(entry, "normalizedText");
        const orig = readEntry(entry, "originalText");
        const name = readEntry(entry, "name");
        if (norm) {
            voiceByText.set(normalizeLookupText(norm), entry);
            voiceByText.set((norm || "").trim().toLocaleLowerCase("vi-VN"), entry);
        }
        if (orig) {
            voiceByText.set(normalizeLookupText(orig), entry);
            voiceByText.set((orig || "").trim().toLocaleLowerCase("vi-VN"), entry);
        }
        if (name) {
            const nameParts = name.split(" - ");
            if (nameParts.length > 1) {
                voiceByText.set(normalizeLookupText(nameParts.slice(1).join(" - ")), entry);
            }
        }
    });
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
            const imageInput = form.querySelector('[name="ExistingImageAssetId"]');
            const selectedOption = imageInput?.tagName === "SELECT" && imageInput.selectedIndex >= 0 ? imageInput.options[imageInput.selectedIndex] : null;
            const assetId = imageInput?.value || "";
            const matchedAsset = assetId ? imageAssetEntries.find((a) => readEntry(a, "id") === assetId) : null;
            const selectedImagePath = selectedOption?.dataset?.path || (matchedAsset ? readEntry(matchedAsset, "storagePath") : "");
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

        // Chỉ giữ Voice Câu hỏi bài học (PromptText), Phản hồi đúng/sai và các đáp án
        add("C\u00e2u h\u1ecfi b\u00e0i h\u1ecdc", read("PromptText") || read("Title"), true);
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
            text.textContent = row.text || "Chưa nhập text";
            const actions = document.createElement("div");
            actions.className = "builder-voice-actions";

            const updateEntryAudio = (result) => {
                if (result && result.audioUrl) {
                    const normalizedKey = normalizeLookupText(result.normalizedText || row.text);
                    voiceByText.set(normalizedKey, result);
                    if (!readyVoiceEntries.some((v) => readEntry(v, "id") === result.id)) {
                        readyVoiceEntries.unshift(result);
                    }
                    setTimeout(updateBuilderVoicePanel, 0);
                }
            };

            const makeInlineTools = (currentMatch) => {
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
                readyVoiceEntries.forEach((voice) => {
                    const id = readEntry(voice, "id");
                    const norm = readEntry(voice, "normalizedText");
                    const orig = readEntry(voice, "originalText");
                    const name = readEntry(voice, "name");
                    const usage = displayUsageType(readEntry(voice, "usageType"));

                    const seen = new Set();
                    const addOption = (text) => {
                        if (!text || seen.has(text)) return;
                        seen.add(text);
                        const opt = document.createElement("option");
                        opt.value = text;
                        voiceByLabel.set(text, id);
                        datalist.append(opt);
                    };

                    if (norm) addOption(`${usage} - ${norm}`);
                    if (orig && orig !== norm) addOption(`${usage} - ${orig}`);
                    if (name) addOption(name);
                });
                picker.addEventListener("change", async () => {
                    const sourceId = voiceByLabel.get(picker.value);
                    if (!sourceId) return;
                    picker.disabled = true;
                    try {
                        const data = new FormData();
                        data.append("id", readEntry(currentMatch, "id") || "");
                        data.append("sourceId", sourceId);
                        data.append("text", row.text);
                        data.append("usageType", row.kind);
                        data.append("__RequestVerificationToken", token);
                        const response = await fetch("/admin/voice-cache/copy-inline", {
                            method: "POST",
                            headers: {"RequestVerificationToken": token},
                            body: data,
                            credentials: "same-origin"
                        });
                        if (!response.ok) {
                            const error = await response.json().catch(() => ({message: "Không thể chọn voice từ kho."}));
                            throw new Error(error.message || "Không thể chọn voice từ kho.");
                        }
                        updateEntryAudio(await response.json());
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
                fileInput.title = "Tải file âm thanh từ máy";
                fileInput.addEventListener("change", async () => {
                    const file = fileInput.files?.[0];
                    if (!file) return;
                    const data = new FormData();
                    data.append("id", readEntry(currentMatch, "id") || "");
                    data.append("audioFile", file);
                    data.append("text", row.text);
                    data.append("usageType", row.kind);
                    data.append("__RequestVerificationToken", token);
                    fileInput.disabled = true;
                    try {
                        const response = await fetch("/admin/voice-cache/upload-inline", {
                            method: "POST",
                            headers: {"RequestVerificationToken": token},
                            body: data,
                            credentials: "same-origin"
                        });
                        if (!response.ok) {
                            const error = await response.json().catch(() => ({message: "Không thể lưu file voice."}));
                            throw new Error(error.message || "Không thể lưu file voice.");
                        }
                        updateEntryAudio(await response.json());
                    } catch (error) {
                        window.alert(error.message);
                    } finally {
                        fileInput.value = "";
                        fileInput.disabled = false;
                    }
                });

                const generateBtn = document.createElement("button");
                generateBtn.type = "button";
                generateBtn.className = "mini-action app-btn-small";
                generateBtn.textContent = hasAudio ? "Tạo lại" : "Tạo file";
                generateBtn.title = "Tự động tạo giọng đọc TTS cho nội dung này";
                generateBtn.addEventListener("click", async () => {
                    if (!row.text) return;
                    generateBtn.disabled = true;
                    generateBtn.textContent = "Đang tạo...";
                    try {
                        const data = new FormData();
                        data.append("id", readEntry(currentMatch, "id") || "");
                        data.append("text", row.text);
                        data.append("usageType", row.kind);
                        data.append("__RequestVerificationToken", token);
                        const response = await fetch("/admin/voice-cache/generate-inline", {
                            method: "POST",
                            headers: {"RequestVerificationToken": token},
                            body: data,
                            credentials: "same-origin"
                        });
                        if (!response.ok) {
                            const error = await response.json().catch(() => ({message: "Không thể tạo file voice tự động."}));
                            throw new Error(error.message || "Không thể tạo file voice tự động.");
                        }
                        updateEntryAudio(await response.json());
                    } catch (error) {
                        window.alert(error.message);
                    } finally {
                        generateBtn.disabled = false;
                        generateBtn.textContent = hasAudio ? "Tạo lại" : "Tạo file";
                    }
                });

                uploadBox.append(picker, datalist, fileInput, generateBtn);
                return uploadBox;
            };

            if (!row.text) {
                const chip = document.createElement("strong");
                chip.className = "builder-voice-chip";
                chip.textContent = "Thiếu text";
                actions.append(chip);
            } else if (hasAudio) {
                const audio = document.createElement("audio");
                audio.controls = true;
                audio.preload = "none";
                audio.src = audioUrl;
                actions.append(audio, makeInlineTools(match));
            } else {
                const chip = document.createElement("strong");
                chip.className = "builder-voice-chip";
                chip.textContent = "Thiếu file";
                actions.append(chip, makeInlineTools(match));
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

    const updateBuilderImagePanel = () => {
        const panel = form.querySelector("[data-builder-image-panel]");
        if (!panel) return;

        const list = panel.querySelector("[data-builder-image-list]");
        const totalNode = panel.querySelector("[data-builder-image-total]");
        const filledNode = panel.querySelector("[data-builder-image-filled]");
        if (!list || !totalNode || !filledNode) return;

        const mainImgHidden = form.querySelector("#mainImageUrlHidden");
        const mainAssetIdHidden = form.querySelector("#existingImageAssetHiddenId");
        const mainFileHidden = form.querySelector("#mainImageHiddenFileInput");
        const itemMediaHidden = form.querySelector("#itemMediaTextHidden");

        const currentMap = parseItemMediaText();
        const labels = collectMediaLabels();

        // Tạo datalist chứa Tên ảnh thân thiện, KHÔNG HIỂN THỊ ĐƯỜNG DẪN THỪA
        const assetByDisplay = new Map();
        const imageDatalistId = "builder-global-image-store";
        let datalistEl = document.getElementById(imageDatalistId);
        if (!datalistEl) {
            datalistEl = document.createElement("datalist");
            datalistEl.id = imageDatalistId;
            document.body.append(datalistEl);
        }
        datalistEl.replaceChildren();

        imageAssetEntries.forEach((asset) => {
            const fileName = readEntry(asset, "fileName") || "Ảnh";
            const altText = readEntry(asset, "altText");
            const storagePath = readEntry(asset, "storagePath");
            const id = readEntry(asset, "id");
            if (!storagePath) return;

            const displayName = altText && altText !== fileName ? `${fileName} (${altText})` : fileName;
            assetByDisplay.set(displayName.toLowerCase(), { storagePath, id, displayName });
            assetByDisplay.set(fileName.toLowerCase(), { storagePath, id, displayName });

            const opt = document.createElement("option");
            opt.value = displayName;
            datalistEl.append(opt);
        });

        const findDisplayNameByPath = (path) => {
            if (!path) return "";
            for (const item of assetByDisplay.values()) {
                if (item.storagePath === path) return item.displayName;
            }
            const clean = path.split("/").pop() || "";
            return clean.length > 30 ? clean.slice(0, 30) + "..." : clean;
        };

        const rows = [];

        // 1. Hình minh họa chung bài học
        const mainUrl = uploadedImagePreviewUrl || mainImgHidden?.value || "";
        rows.push({
            kind: "Hình bài học",
            name: "Minh họa bài học",
            isMain: true,
            url: mainUrl
        });

        // 2. Từng đáp án / lựa chọn của bài
        labels.forEach((label, idx) => {
            const norm = normalizeLookupText(label);
            const itemObj = currentMap.get(norm);
            rows.push({
                kind: `Đáp án ${idx + 1}`,
                name: label,
                isMain: false,
                url: itemObj?.url || ""
            });
        });

        const filledCount = rows.filter((r) => r.url).length;
        totalNode.textContent = String(rows.length);
        filledNode.textContent = String(filledCount);
        list.replaceChildren();

        rows.forEach((row) => {
            const rowEl = document.createElement("div");
            rowEl.className = "builder-image-row";

            const kindEl = document.createElement("small");
            kindEl.textContent = row.kind;

            const nameEl = document.createElement("span");
            nameEl.className = "image-target-name";
            nameEl.textContent = row.name;

            // Thumbnail xem trước ảnh thật (38x38)
            const thumbWrap = document.createElement("div");
            thumbWrap.className = "builder-image-thumb";
            if (row.url) {
                const img = document.createElement("img");
                img.src = row.url;
                img.alt = row.name;
                thumbWrap.append(img);
            } else {
                const icon = document.createElement("span");
                icon.className = "material-symbols-outlined";
                icon.textContent = "image";
                thumbWrap.append(icon);
            }

            // Input tìm chọn ảnh từ thư viện (chỉ hiện tên thân thiện)
            const input = document.createElement("input");
            input.className = "form-control";
            input.setAttribute("list", imageDatalistId);
            input.placeholder = "Gõ tìm chọn ảnh trong thư viện...";
            input.value = findDisplayNameByPath(row.url);

            const toolsEl = document.createElement("div");
            toolsEl.className = "builder-image-tools";

            if (row.isMain) {
                const uploadBtn = document.createElement("button");
                uploadBtn.type = "button";
                uploadBtn.className = "mini-action app-btn-small";
                uploadBtn.title = "Tải ảnh từ máy tính";
                uploadBtn.innerHTML = `<span class="material-symbols-outlined" style="font-size:16px;">upload_file</span>`;
                uploadBtn.addEventListener("click", () => mainFileHidden?.click());
                toolsEl.append(uploadBtn);
            }

            if (row.url) {
                const clearBtn = document.createElement("button");
                clearBtn.type = "button";
                clearBtn.className = "mini-action app-btn-small";
                clearBtn.style.color = "#ef4444";
                clearBtn.title = "Bỏ ảnh này";
                clearBtn.innerHTML = `<span class="material-symbols-outlined" style="font-size:16px;">delete</span>`;
                clearBtn.addEventListener("click", () => {
                    if (row.isMain) {
                        if (mainImgHidden) mainImgHidden.value = "";
                        if (mainAssetIdHidden) mainAssetIdHidden.value = "";
                        if (uploadedImagePreviewUrl) {
                            URL.revokeObjectURL(uploadedImagePreviewUrl);
                            uploadedImagePreviewUrl = "";
                        }
                    } else {
                        currentMap.delete(normalizeLookupText(row.name));
                        syncHiddenItemMedia();
                    }
                    updateBuilderImagePanel();
                    updateBuilderPreview();
                });
                toolsEl.append(clearBtn);
            }

            input.addEventListener("change", () => {
                const searchVal = input.value.trim().toLowerCase();
                const matched = assetByDisplay.get(searchVal);
                const path = matched ? matched.storagePath : input.value.trim();

                if (row.isMain) {
                    if (mainImgHidden) mainImgHidden.value = path;
                    if (mainAssetIdHidden) mainAssetIdHidden.value = matched?.id || "";
                    if (uploadedImagePreviewUrl) {
                        URL.revokeObjectURL(uploadedImagePreviewUrl);
                        uploadedImagePreviewUrl = "";
                    }
                } else {
                    if (path) {
                        currentMap.set(normalizeLookupText(row.name), { label: row.name, url: path });
                    } else {
                        currentMap.delete(normalizeLookupText(row.name));
                    }
                    syncHiddenItemMedia();
                }
                updateBuilderImagePanel();
                updateBuilderPreview();
            });

            rowEl.append(kindEl, nameEl, thumbWrap, input, toolsEl);
            list.append(rowEl);
        });

        function syncHiddenItemMedia() {
            if (!itemMediaHidden) return;
            const lines = [];
            for (const item of currentMap.values()) {
                if (item.label && item.url) {
                    lines.push(`${item.label} = ${item.url}`);
                }
            }
            itemMediaHidden.value = lines.join("\n");
        }
    };

    skillGroupSelect?.addEventListener("change", filterTopics);
    topicSelect?.addEventListener("change", filterTemplates);
    interactionSelect?.addEventListener("change", () => {
        applyTemplateDefaults();
        toggleInteractionFields();
        updateTemplateContext();
        updateBuilderPreview();
        updateBuilderVoicePanel();
        updateBuilderImagePanel();
    });
    templateButtons.forEach((button) => button.addEventListener("click", () => {
        interactionSelect.value = button.dataset.templateOption;
        interactionSelect.dispatchEvent(new Event("change", {bubbles: true}));
    }));
    choiceInputs.forEach((input) => input.addEventListener("input", () => {
        syncCorrectAnswer();
        updateBuilderPreview();
        updateBuilderVoicePanel();
        updateBuilderImagePanel();
    }));
    form.querySelectorAll("input, textarea").forEach((input) => input.addEventListener("input", () => {
        updateBuilderPreview();
        updateBuilderVoicePanel();
        updateBuilderImagePanel();
    }));
    form.querySelectorAll("select").forEach((select) => select.addEventListener("change", () => {
        updateBuilderPreview();
        updateBuilderVoicePanel();
        updateBuilderImagePanel();
    }));
    form.querySelector("#mainImageHiddenFileInput")?.addEventListener("change", (event) => {
        if (uploadedImagePreviewUrl) URL.revokeObjectURL(uploadedImagePreviewUrl);
        uploadedImagePreviewUrl = event.target.files?.[0] ? URL.createObjectURL(event.target.files[0]) : "";
        const mainImgHidden = form.querySelector("#mainImageUrlHidden");
        const mainAssetIdHidden = form.querySelector("#existingImageAssetHiddenId");
        if (mainImgHidden) mainImgHidden.value = "";
        if (mainAssetIdHidden) mainAssetIdHidden.value = "";
        updateBuilderImagePanel();
        updateBuilderPreview();
    });

    const promptInput = form.querySelector('[name="PromptText"]');
    const titleInput = form.querySelector('[name="Title"]');
    if (promptInput && titleInput) {
        promptInput.addEventListener("input", () => {
            if (!titleInput.dataset.userEdited && promptInput.value.trim()) {
                const clean = promptInput.value.trim()
                    .replace(/^(?:Bé hãy|Con hãy|Hãy|Chọn các|Chọn|Tô theo|Tô tranh|Sắp xếp|Tìm)\s+/i, "");
                titleInput.value = clean.slice(0, 50);
            }
            updateBuilderVoicePanel();
            updateBuilderImagePanel();
        });
        titleInput.addEventListener("input", () => {
            titleInput.dataset.userEdited = "true";
            updateBuilderVoicePanel();
            updateBuilderImagePanel();
        });
    }

    filterTopics();
    syncCorrectAnswer();
    toggleInteractionFields();
    updateTemplateContext();
    updateBuilderPreview();
    updateBuilderVoicePanel();
    updateBuilderImagePanel();
});

document.querySelectorAll("[data-tracing-builder]").forEach((form) => {
    const symbolInput = form.querySelector("[data-tracing-symbol-input]");
    const symbolPreview = form.querySelector("[data-tracing-preview-symbol]");
    const guideMode = form.querySelector("[data-tracing-guide-mode]");
    const guidePreview = form.querySelector("[data-tracing-preview-guide]");

    const updateTracingPreview = () => {
        const symbol = symbolInput?.value.trim() || "?";
        if (symbolPreview) {
            symbolPreview.textContent = symbol;
            symbolPreview.hidden = guideMode?.value === "free";
        }
        if (guidePreview) {
            guidePreview.hidden = guideMode?.value === "free";
            if (guideMode?.value === "free") {
                guidePreview.replaceChildren();
            } else {
                window.tracingGuides?.renderTracingGuide(guidePreview, symbol);
            }
        }
    };

    symbolInput?.addEventListener("input", updateTracingPreview);
    guideMode?.addEventListener("change", updateTracingPreview);
    updateTracingPreview();
});

// Admin Sidebar Toggle Logic
(() => {
    const toggleBtn = document.getElementById("adminSidebarToggle");
    const layouts = document.querySelectorAll(".admin-layout");
    if (!toggleBtn || !layouts.length) return;

    toggleBtn.addEventListener("click", () => {
        const isCollapsed = layouts[0].classList.toggle("sidebar-collapsed");
        layouts.forEach((el) => {
            if (el !== layouts[0]) el.classList.toggle("sidebar-collapsed", isCollapsed);
        });
        localStorage.setItem("admin_sidebar_collapsed", isCollapsed ? "true" : "false");
    });
})();

// Unified Batch Voice Synchronizer Runner
(() => {
    const syncButtons = document.querySelectorAll("[data-sync-voice-btn]");
    if (!syncButtons.length) return;

    let isRunning = false;

    const createModal = () => {
        const existing = document.getElementById("voiceSyncModal");
        if (existing) return existing;

        const backdrop = document.createElement("div");
        backdrop.id = "voiceSyncModal";
        backdrop.className = "voice-sync-modal-backdrop";
        backdrop.innerHTML = `
            <div class="voice-sync-modal-box">
                <div style="display:flex; justify-content:space-between; align-items:center;">
                    <h3 style="margin:0; font-size:1.25rem; display:flex; align-items:center; gap:8px;">
                        <span class="material-symbols-outlined" style="color:var(--primary); font-size:1.6rem;">sync</span>
                        Đồng bộ & Tự sinh Voice Song Ngữ
                    </h3>
                </div>
                <p style="color:#64748b; font-size:0.9rem; margin:8px 0 16px;">
                    Hệ thống tự động quét bài học, tái sử dụng voice có sẵn và sinh bổ sung Voice Tiếng Việt & Tiếng Anh còn thiếu.
                </p>
                
                <div class="voice-sync-progress-outer">
                    <div class="voice-sync-progress-inner" id="voiceSyncProgressBar"></div>
                </div>
                <div style="display:flex; justify-content:space-between; font-size:0.88rem; font-weight:700; color:#334155; margin-bottom:12px;">
                    <span id="voiceSyncProgressText">Đang chuẩn bị...</span>
                    <span id="voiceSyncPercentText">0%</span>
                </div>

                <div class="voice-sync-stats-cards">
                    <div class="voice-sync-stat-item">
                        <strong id="voiceSyncCreatedVi">0</strong>
                        <span>Voice VI tạo mới</span>
                    </div>
                    <div class="voice-sync-stat-item">
                        <strong id="voiceSyncCreatedEn">0</strong>
                        <span>Voice EN tạo mới</span>
                    </div>
                    <div class="voice-sync-stat-item">
                        <strong id="voiceSyncUpdatedItems">0</strong>
                        <span>Bài học đồng bộ</span>
                    </div>
                </div>

                <div id="voiceSyncStatusLog" style="max-height:90px; overflow-y:auto; font-size:0.8rem; color:#64748b; background:#f8fafc; padding:8px 12px; border-radius:8px; border:1px solid #e2e8f0; margin-bottom:16px;">
                    Sẵn sàng bắt đầu quá trình đồng bộ...
                </div>

                <div style="display:flex; justify-content:flex-end; gap:10px;">
                    <button type="button" class="app-btn app-btn-small" id="voiceSyncCloseBtn" disabled>
                        <span class="material-symbols-outlined">hourglass_empty</span>
                        Đang đồng bộ...
                    </button>
                </div>
            </div>
        `;
        document.body.appendChild(backdrop);
        return backdrop;
    };

    const getCsrfToken = () => {
        return document.querySelector('input[name="__RequestVerificationToken"]')?.value || "";
    };

    const runBatchSync = async () => {
        if (isRunning) return;
        isRunning = true;

        const modal = createModal();
        modal.style.display = "flex";

        const progressBar = document.getElementById("voiceSyncProgressBar");
        const progressText = document.getElementById("voiceSyncProgressText");
        const percentText = document.getElementById("voiceSyncPercentText");
        const statVi = document.getElementById("voiceSyncCreatedVi");
        const statEn = document.getElementById("voiceSyncCreatedEn");
        const statItems = document.getElementById("voiceSyncUpdatedItems");
        const statusLog = document.getElementById("voiceSyncStatusLog");
        const closeBtn = document.getElementById("voiceSyncCloseBtn");

        let totalVi = 0;
        let totalEn = 0;
        let totalUpdated = 0;
        let batchStep = 0;
        let isDone = false;

        closeBtn.disabled = true;
        closeBtn.innerHTML = `<span class="material-symbols-outlined">hourglass_empty</span> Đang xử lý...`;
        progressText.textContent = "Đang quét toàn bộ kho bài học...";
        statusLog.innerHTML = `<div>&bull; Bắt đầu quét và đồng bộ voice...</div>`;

        const csrf = getCsrfToken();

        while (!isDone) {
            batchStep++;
            try {
                const formData = new FormData();
                formData.append("__RequestVerificationToken", csrf);
                formData.append("batchSize", "1");

                const response = await fetch("/admin/sync-voice-batch", {
                    method: "POST",
                    body: formData
                });

                if (!response.ok) {
                    throw new Error(`Lỗi máy chủ (${response.status})`);
                }

                const data = await response.json();
                totalVi += data.createdVi || 0;
                totalEn += data.createdEn || 0;
                totalUpdated += data.updatedItems || 0;

                statVi.textContent = totalVi;
                statEn.textContent = totalEn;
                statItems.textContent = totalUpdated;

                const remaining = data.remainingMissing || 0;
                const totalEntries = data.totalVoices || (totalUpdated + remaining);
                const percent = data.isCompleted || remaining === 0 ? 100 : Math.min(99, Math.round(((totalEntries - remaining) / Math.max(1, totalEntries)) * 100));

                progressBar.style.width = `${percent}%`;
                percentText.textContent = `${percent}%`;
                progressText.textContent = remaining > 0
                    ? `Đợt ${batchStep}: Đang tạo voice (còn ${remaining} mục cần xử lý)...`
                    : `Đã đồng bộ và tạo file voice hoàn tất 100%!`;

                if (data.errorMessages?.length) {
                    data.errorMessages.forEach((err) => {
                        const div = document.createElement("div");
                        div.style.color = "#dc2626";
                        div.textContent = `⚠ ${err}`;
                        statusLog.prepend(div);
                    });
                } else {
                    const div = document.createElement("div");
                    div.textContent = `✓ Đợt ${batchStep}: Hoàn tất (+${data.createdVi || 0} VI, +${data.createdEn || 0} EN)`;
                    statusLog.prepend(div);
                }

                if (data.isCompleted || remaining === 0) {
                    isDone = true;
                }
            } catch (err) {
                statusLog.innerHTML = `<div style="color:#dc2626;"><strong>Lỗi:</strong> ${err.message}. Đang thử lại đợt tiếp theo...</div>` + statusLog.innerHTML;
                await new Promise((r) => setTimeout(r, 2000));
            }
        }

        progressBar.style.width = "100%";
        percentText.textContent = "100%";
        progressText.textContent = "Đã đồng bộ và tạo file voice hoàn tất 100%!";
        closeBtn.disabled = false;
        closeBtn.className = "app-btn app-btn-small celebration-btn-primary";
        closeBtn.innerHTML = `<span class="material-symbols-outlined">check_circle</span> Hoàn tất & Tải lại`;
        closeBtn.onclick = () => {
            window.location.reload();
        };
        isRunning = false;
    };

    syncButtons.forEach((btn) => {
        btn.addEventListener("click", (e) => {
            e.preventDefault();
            void runBatchSync();
        });
    });
})();

