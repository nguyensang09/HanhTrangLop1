/* =======================================================================
   LƯU NHÁP TRÌNH SOẠN BÀI HỌC (phía máy khách)

   Mục đích: giáo viên đang soạn một bài học dài mà vô tình tải lại trang,
   bấm lùi hoặc mất kết nối thì không mất toàn bộ nội dung đã nhập.

   Nguyên tắc an toàn:
     - Không chạm vào luồng lưu, kiểm tra hợp lệ hay dữ liệu phía máy chủ.
       Toàn bộ nháp chỉ nằm trong localStorage của chính máy giáo viên.
     - KHÔNG bao giờ tự động ghi đè form đang mở. Chỉ hiện một dải thông báo
       nhẹ và chỉ phục hồi khi giáo viên chủ động bấm "Phục hồi".
     - Nháp được xóa ngay khi form được gửi đi, vì khi kiểm tra hợp lệ thất bại
       máy chủ đã render lại đúng nội dung vừa nhập nên không mất gì.
     - Không lưu token chống giả mạo biểu mẫu và không lưu file đã chọn
       (trình duyệt không cho phép gán lại file vì lý do bảo mật).
     - Nháp chỉ được ghi sau khi giáo viên thực sự thao tác, nên mở form rồi
       thoát ra ngay sẽ không để lại nháp và không hiện dải thông báo.
   ======================================================================= */
(() => {
    const form = document.querySelector("[data-lesson-draft]");
    if (!form) return;

    let storage;
    try {
        storage = window.localStorage;
        // Một số trình duyệt chỉ báo lỗi khi thực sự ghi (chế độ riêng tư).
        const probeKey = "__lesson_draft_probe__";
        storage.setItem(probeKey, "1");
        storage.removeItem(probeKey);
    } catch {
        return; // Không dùng được localStorage thì bỏ qua, form vẫn hoạt động như cũ.
    }

    const itemId = form.querySelector('[name="Id"]')?.value?.trim();
    const storageKey = `hanhtrang:lesson-draft:${itemId || "new"}`;
    const SAVE_DEBOUNCE = 800;
    const MAX_DRAFT_LENGTH = 512 * 1024;

    const isSkippedField = (field) => {
        if (!field || !field.name) return true;
        if (field.name === "__RequestVerificationToken") return true;
        if (field.type === "file" || field.type === "password") return true;
        return false;
    };

    // Lấy toàn bộ trường của form, kể cả trường đang bị ẩn/disabled theo dạng bài,
    // để khi giáo viên đổi lại dạng bài thì nội dung cũ vẫn còn.
    const collectFields = () => [...form.elements].filter((field) =>
        (field.tagName === "INPUT" || field.tagName === "SELECT" || field.tagName === "TEXTAREA") &&
        !isSkippedField(field));

    // Tra cứu theo tên, lấy phần tử đầu tiên trùng tên. Dùng bảng tra thay vì
    // querySelector để không phụ thuộc vào việc tên trường có hợp lệ trong CSS selector.
    const fieldByName = () => {
        const map = new Map();
        collectFields().forEach((field) => {
            if (!map.has(field.name)) map.set(field.name, field);
        });
        return map;
    };

    const isToggle = (field) => field.type === "checkbox" || field.type === "radio";

    const readForm = () => {
        const values = {};
        collectFields().forEach((field) => {
            // Ô bật/tắt được ghi kèm trạng thái chọn để phục hồi đúng cả khi bị bỏ chọn.
            values[field.name] = isToggle(field)
                ? {toggle: true, checked: field.checked, value: field.value}
                : field.value;
        });
        return values;
    };

    const saveDraft = () => {
        try {
            const payload = JSON.stringify({savedAt: Date.now(), values: readForm()});
            if (payload.length > MAX_DRAFT_LENGTH) return;
            storage.setItem(storageKey, payload);
        } catch {
            // Hết dung lượng hoặc bị chặn: bỏ qua, không làm ảnh hưởng việc soạn bài.
        }
    };

    const clearDraft = () => {
        try {
            storage.removeItem(storageKey);
        } catch {
            // Không xóa được cũng không ảnh hưởng gì tới nội dung đang soạn.
        }
    };

    const loadDraft = () => {
        try {
            const raw = storage.getItem(storageKey);
            if (!raw) return null;
            const parsed = JSON.parse(raw);
            if (!parsed || typeof parsed.values !== "object" || parsed.values === null) return null;
            return parsed;
        } catch {
            clearDraft();
            return null;
        }
    };

    let saveTimer = 0;
    const queueSave = () => {
        window.clearTimeout(saveTimer);
        saveTimer = window.setTimeout(saveDraft, SAVE_DEBOUNCE);
    };

    form.addEventListener("input", queueSave);
    form.addEventListener("change", queueSave);
    form.addEventListener("submit", () => {
        window.clearTimeout(saveTimer);
        clearDraft();
    });

    // ── Phục hồi ────────────────────────────────────────────────────────────
    const dispatch = (field, eventName) => {
        field?.dispatchEvent(new Event(eventName, {bubbles: true}));
    };

    const restore = (values) => {
        const lookup = fieldByName();

        const assign = (name) => {
            if (!Object.prototype.hasOwnProperty.call(values, name)) return null;
            const field = lookup.get(name);
            if (!field) return null;
            const stored = values[name];
            if (stored && typeof stored === "object" && stored.toggle === true) {
                if (isToggle(field)) field.checked = stored.checked === true;
                return field;
            }
            if (isToggle(field)) return null;
            field.value = stored ?? "";
            return field;
        };

        // 1) Gán giá trị cho mọi trường trước, chưa phát sự kiện.
        //    "CorrectAnswer" để lại sau cùng vì site.js dựng lại danh sách đáp án
        //    của nó từ các ô lựa chọn, gán sớm sẽ bị xóa mất.
        const deferred = new Set(["CorrectAnswer"]);
        Object.keys(values).forEach((name) => {
            if (!deferred.has(name)) assign(name);
        });

        // 2) Phát sự kiện theo đúng thứ tự phụ thuộc của site.js.
        const skillGroup = form.querySelector("[data-skill-group-select]");
        const interaction = form.querySelector("[data-interaction-select]");

        dispatch(skillGroup, "change");

        // Đổi nhóm kỹ năng có thể khiến dạng bài vừa phục hồi không còn hợp lệ và bị
        // site.js chuyển về dạng đầu tiên. Gán lại rồi kiểm tra: chỉ giữ dạng bài cũ
        // nếu nó vẫn được phép trong nhóm kỹ năng này.
        if (interaction && typeof values.InteractionType === "string") {
            const option = [...interaction.options].find((item) => item.value === values.InteractionType);
            if (option && !option.disabled) interaction.value = values.InteractionType;
        }
        dispatch(interaction, "change");

        // Đổi dạng bài làm site.js áp lại lời dặn/câu hỏi mặc định của mẫu (với bài mới),
        // nên câu hỏi và tên bài phải được gán lại sau bước này. Gán câu hỏi trước, vì
        // site.js tự suy ra tên bài từ câu hỏi; gán tên bài sau để giữ đúng tên đã nhập.
        const prompt = assign("PromptText");
        dispatch(prompt, "input");
        const title = assign("Title");
        dispatch(title, "input");

        form.querySelectorAll("[data-choice-input]").forEach((input) => dispatch(input, "input"));

        const correctAnswer = assign("CorrectAnswer");
        if (correctAnswer) {
            const hasOption = [...correctAnswer.options].some((option) => option.value === values.CorrectAnswer);
            if (!hasOption) correctAnswer.value = "";
            dispatch(correctAnswer, "change");
        }

        // Cập nhật nốt các trường còn lại để khung xem trước và bảng voice/ảnh khớp
        // với nội dung vừa phục hồi. Chỉ phát cho đúng những trường có trong nháp.
        const alreadyDispatched = new Set([skillGroup, interaction, prompt, title, correctAnswer]);
        Object.keys(values).forEach((name) => {
            const field = lookup.get(name);
            if (!field || alreadyDispatched.has(field)) return;
            dispatch(field, field.tagName === "SELECT" ? "change" : "input");
        });
    };

    const formatSavedAt = (timestamp) => {
        const saved = new Date(timestamp);
        if (Number.isNaN(saved.getTime())) return "";
        const sameDay = saved.toDateString() === new Date().toDateString();
        const time = saved.toLocaleTimeString("vi-VN", {hour: "2-digit", minute: "2-digit"});
        return sameDay ? `lúc ${time}` : `${saved.toLocaleDateString("vi-VN")} ${time}`;
    };

    const draft = loadDraft();
    if (!draft) return;

    const banner = document.createElement("div");
    banner.className = "feedback-band warning lesson-draft-band";
    banner.setAttribute("role", "status");
    banner.innerHTML = `
        <span class="material-symbols-outlined">history</span>
        <strong class="lesson-draft-text"></strong>
        <span class="lesson-draft-actions">
            <button type="button" class="dash-btn dash-btn-primary" data-draft-restore>Phục hồi</button>
            <button type="button" class="dash-btn dash-btn-outline" data-draft-discard>Bỏ bản nháp</button>
        </span>`;
    banner.querySelector(".lesson-draft-text").textContent =
        `Còn một bản đang soạn chưa lưu ${formatSavedAt(draft.savedAt)}. Bạn có muốn phục hồi nội dung đó không?`;

    banner.querySelector("[data-draft-restore]").addEventListener("click", () => {
        try {
            restore(draft.values);
            banner.remove();
        } catch (error) {
            // Nếu phục hồi gặp lỗi thì nói rõ cho giáo viên và giữ lại bản nháp để thử
            // lại sau khi tải lại trang, thay vì để form ở trạng thái nửa vời không rõ.
            console.error("Không phục hồi được bản nháp bài học:", error);
            banner.classList.remove("warning");
            banner.classList.add("error");
            banner.querySelector(".lesson-draft-text").textContent =
                "Không phục hồi được bản đang soạn. Bạn hãy tải lại trang rồi thử lại nhé.";
        }
    });
    banner.querySelector("[data-draft-discard]").addEventListener("click", () => {
        clearDraft();
        banner.remove();
    });

    form.parentNode?.insertBefore(banner, form);
})();
