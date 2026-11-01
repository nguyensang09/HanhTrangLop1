(() => {
    // Bộ điều khiển Voice duy nhất của trang làm bài. Mọi âm thanh hướng dẫn (câu hỏi, đáp án bé chọn, lời phản hồi)
    // đều đi qua đây để tại mỗi thời điểm chỉ có MỘT Voice phát:
    //   - Một thẻ <audio> dùng chung (Safari trên iPad chỉ cho thẻ đã từng phát sau một lần chạm được tự phát tiếp).
    //   - Mỗi lượt phát mang một "token"; lượt mới tăng token, dừng ngay Voice cũ và mọi câu còn xếp hàng của nó.
    //   - Promise của một lượt chỉ kết thúc khi file đọc XONG (không phải lúc bắt đầu), nên thứ tự
    //     câu hỏi → đáp án → phản hồi luôn đúng và không chồng lên nhau.
    const noop = {
        playQuestion: () => Promise.resolve(),
        playOption: () => Promise.resolve(false),
        playFeedback: () => Promise.resolve(),
        stop: () => {}
    };
    const host = document.querySelector("[data-learning-voice]");
    if (!host) {
        window.learningVoice = noop;
        return;
    }

    if (host.dataset.soundEnabled !== "true") {
        host.querySelectorAll("[data-learning-replay]").forEach((button) => button.hidden = true);
        window.learningVoice = noop;
        return;
    }

    const isEnglishVoice = host.dataset.englishVoice === "true";
    const activityType = host.dataset.activityType || "";

    let payload = {};
    try {
        payload = JSON.parse(document.querySelector("[data-activity-payload]")?.value || "{}");
    } catch {
        payload = {};
    }

    // Kho bài mới có bảng lời đọc viết sẵn: chữ hiển thị → [lời tiếng Việt, lời tiếng Anh].
    const speechMap = payload.speech && typeof payload.speech === "object" ? payload.speech : {};
    const authoredText = (text) => {
        const pair = speechMap[text] || speechMap[String(text || "").trim()];
        if (!Array.isArray(pair)) return isEnglishVoice ? "" : String(text || "");
        return String((isEnglishVoice ? pair[1] : pair[0]) || "");
    };

    // Đường dẫn file theo đúng ngôn ngữ phụ huynh chọn; không bao giờ phát nhầm sang ngôn ngữ kia.
    const pickLang = (vi, en) => (isEnglishVoice ? en : vi) || "";
    const voiceUrls = {};
    let optionAudio = payload.optionAudio || {};
    let optionAudioEn = payload.optionAudioEn || {};
    const setUrls = (source) => {
        voiceUrls.question = pickLang(source.questionAudioUrl ?? voiceUrls.questionVi, source.questionAudioUrlEn ?? voiceUrls.questionEn);
        voiceUrls.questionVi = source.questionAudioUrl ?? voiceUrls.questionVi;
        voiceUrls.questionEn = source.questionAudioUrlEn ?? voiceUrls.questionEn;
        voiceUrls.contentVi = source.audioUrl ?? voiceUrls.contentVi;
        voiceUrls.contentEn = source.audioUrlEn ?? voiceUrls.contentEn;
        voiceUrls.content = pickLang(voiceUrls.contentVi, voiceUrls.contentEn);
        voiceUrls.correctVi = source.correctAudioUrl ?? voiceUrls.correctVi;
        voiceUrls.correctEn = source.correctAudioUrlEn ?? voiceUrls.correctEn;
        voiceUrls.correct = pickLang(voiceUrls.correctVi, voiceUrls.correctEn);
        voiceUrls.retryVi = source.retryAudioUrl ?? voiceUrls.retryVi;
        voiceUrls.retryEn = source.retryAudioUrlEn ?? voiceUrls.retryEn;
        voiceUrls.retry = pickLang(voiceUrls.retryVi, voiceUrls.retryEn);
    };
    setUrls({
        questionAudioUrl: payload.questionAudioUrl || host.dataset.questionAudioUrl || "",
        questionAudioUrlEn: payload.questionAudioUrlEn || host.dataset.questionAudioUrlEn || "",
        audioUrl: payload.audioUrl || "",
        audioUrlEn: payload.audioUrlEn || "",
        correctAudioUrl: payload.correctAudioUrl || host.dataset.correctAudioUrl || "",
        correctAudioUrlEn: payload.correctAudioUrlEn || host.dataset.correctAudioUrlEn || "",
        retryAudioUrl: payload.retryAudioUrl || host.dataset.retryAudioUrl || "",
        retryAudioUrlEn: payload.retryAudioUrlEn || host.dataset.retryAudioUrlEn || ""
    });

    const questionText = host.dataset.questionText || payload.questionSpeechText || "";
    const contentText = payload.speechText || "";
    const standardText = isEnglishVoice
        ? { correct: "Correct!", retry: "Let's try again." }
        : { correct: "Chính xác!", retry: "Con thử lại nhé" };

    // ------------------------------------------------------------------ Phát Voice
    const player = new Audio();
    player.preload = "auto";
    player.setAttribute("playsinline", "");
    let token = 0;
    let cutCurrent = null;
    let current = Promise.resolve();
    let currentKind = "";
    let autoplayBlocked = false;

    const setSpeaking = (speaking) => {
        host.querySelector(".mascot-speaking, .retry-mascot-img, .learning-success-mascot")?.classList.toggle("is-speaking", speaking);
        host.querySelectorAll("[data-learning-replay]").forEach((btn) => btn.classList.toggle("is-playing", speaking));
    };

    // Kết thúc khi file đọc xong. Trả về false nếu lỗi hoặc trình duyệt chặn tự phát (chưa có lần chạm nào).
    const playFile = (url, myToken) => new Promise((resolve) => {
        if (!url || myToken !== token) { resolve(false); return; }
        let done = false;
        let guard = 0;
        const finish = (ok) => {
            if (done) return;
            done = true;
            window.clearTimeout(guard);
            player.onended = player.onerror = player.onloadedmetadata = null;
            if (cutCurrent === cut) cutCurrent = null;
            resolve(ok);
        };
        const cut = () => { try { player.pause(); } catch { } finish(false); };
        cutCurrent = cut;
        player.onended = () => finish(true);
        player.onerror = () => finish(false);
        // Phòng khi "ended" không đến (thỉnh thoảng gặp trên Safari): tự kết thúc theo độ dài file.
        player.onloadedmetadata = () => {
            const seconds = Number.isFinite(player.duration) ? player.duration : 10;
            window.clearTimeout(guard);
            guard = window.setTimeout(() => finish(true), (seconds + 1.5) * 1000);
        };
        guard = window.setTimeout(() => finish(true), 15000);
        try {
            player.src = url;
            const started = player.play();
            if (started?.catch) {
                started.then(() => { autoplayBlocked = false; }).catch((err) => {
                    if (err?.name === "NotAllowedError") autoplayBlocked = true;
                    finish(false);
                });
            }
        } catch {
            finish(false);
        }
    });

    // Chỉ phát file trong kho Voice chung để mọi bài cùng một giọng; câu chưa có file thì im lặng và chờ máy chủ sinh,
    // không đọc bằng giọng máy của thiết bị.
    const playClip = async (clip, myToken) => {
        if (!clip?.url || myToken !== token) return false;
        return playFile(clip.url, myToken);
    };

    const stop = () => {
        token += 1;
        if (cutCurrent) cutCurrent();
        cutCurrent = null;
        try { player.pause(); } catch { }
        setSpeaking(false);
    };

    // Phát lần lượt các câu; gọi mới sẽ cắt ngay lượt đang phát để Voice luôn khớp với thao tác mới nhất.
    const play = (clips, kind = "") => {
        stop();
        currentKind = kind;
        const myToken = token;
        const list = clips.filter((clip) => clip?.url);
        if (!list.length) return Promise.resolve(false);
        setSpeaking(true);
        let played = false;
        current = list.reduce((chain, clip) => chain.then(async () => {
            if (myToken !== token) return;
            if (await playClip(clip, myToken)) played = true;
        }), Promise.resolve()).then(() => {
            if (myToken === token) setSpeaking(false);
            return played;
        });
        return current;
    };

    // ------------------------------------------------------------------ Sinh bổ sung Voice còn thiếu
    let voicesRequest = null;
    const ensureVoices = () => {
        const url = host.dataset.voiceUrl;
        const antiForgery = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        if (!url || !antiForgery) return Promise.resolve(false);
        if (!voicesRequest) {
            voicesRequest = fetch(url, { method: "POST", credentials: "same-origin", headers: { RequestVerificationToken: antiForgery } })
                .then((res) => res.ok ? res.json() : null)
                .then((data) => {
                    if (!data) return false;
                    const keep = (value, fallback) => value || fallback || "";
                    setUrls({
                        questionAudioUrl: keep(data.questionAudioUrl, voiceUrls.questionVi),
                        questionAudioUrlEn: keep(data.questionAudioUrlEn, voiceUrls.questionEn),
                        audioUrl: keep(data.audioUrl, voiceUrls.contentVi),
                        audioUrlEn: keep(data.audioUrlEn, voiceUrls.contentEn),
                        correctAudioUrl: keep(data.correctAudioUrl, voiceUrls.correctVi),
                        correctAudioUrlEn: keep(data.correctAudioUrlEn, voiceUrls.correctEn),
                        retryAudioUrl: keep(data.retryAudioUrl, voiceUrls.retryVi),
                        retryAudioUrlEn: keep(data.retryAudioUrlEn, voiceUrls.retryEn)
                    });
                    if (data.optionAudio) optionAudio = { ...optionAudio, ...data.optionAudio };
                    if (data.optionAudioEn) optionAudioEn = { ...optionAudioEn, ...data.optionAudioEn };
                    prefetchAll();
                    return true;
                })
                .catch(() => false);
        }
        return voicesRequest;
    };
    const activeOptionMap = () => isEnglishVoice ? optionAudioEn : optionAudio;
    const hasMissingVoice = () => !voiceUrls.question ||
        !voiceUrls.correct || !voiceUrls.retry ||
        Object.values(activeOptionMap()).some((x) => !x);

    // Tải sẵn các file sẽ cần (đáp án, phản hồi) để lúc bé chạm là đọc ngay.
    const prefetched = new Set();
    const prefetchAll = () => {
        [voiceUrls.correct, voiceUrls.retry, ...Object.values(activeOptionMap())].forEach((url) => {
            if (!url || prefetched.has(url)) return;
            prefetched.add(url);
            const link = document.createElement("link");
            link.rel = "prefetch";
            link.as = "audio";
            link.href = url;
            document.head.append(link);
        });
    };

    // ------------------------------------------------------------------ Các lượt đọc của bài
    const normalizeKey = (value) => String(value || "").trim().toLocaleLowerCase("vi-VN");
    const optionUrl = (label) => {
        const map = activeOptionMap();
        const clean = String(label || "").trim();
        if (map[clean]) return String(map[clean]);
        const match = Object.entries(map).find(([key]) => normalizeKey(key) === normalizeKey(clean));
        return match?.[1] ? String(match[1]) : "";
    };
    const optionText = (label) => {
        const authored = authoredText(label);
        if (authored) return authored;
        // Giọng tiếng Anh: chỉ tự đọc con số/chữ cái đơn (giống nhau ở mọi ngôn ngữ), không đọc chữ tiếng Việt.
        return /^[\p{L}\p{N}]{1,2}$/u.test(String(label || "").trim()) ? String(label).trim() : "";
    };

    const questionClips = () => {
        const clips = [];
        if (activityType === "story_choice" && voiceUrls.content && voiceUrls.content !== voiceUrls.question) {
            clips.push({ url: voiceUrls.content, text: authoredText(contentText) });
        }
        clips.push({ url: voiceUrls.question, text: authoredText(questionText) });
        return clips;
    };

    const playQuestion = () => play(questionClips(), "question");

    const playOption = (label) => {
        if (!label) return Promise.resolve(false);
        return play([{ url: optionUrl(label), text: optionText(label) }], "option");
    };

    // Phản hồi của MỘT lần chấm: chữ và Voice cùng lấy từ kết quả máy chủ trả về (result.audioUrl đã được chọn
    // theo đúng đúng/sai và đúng ngôn ngữ). Chờ Voice đáp án bé vừa chọn đọc xong rồi mới đọc phản hồi.
    const playFeedback = async (result) => {
        const correct = typeof result === "object" ? !!result?.correct : !!result;
        const url = (typeof result === "object" && result?.audioUrl) || (correct ? voiceUrls.correct : voiceUrls.retry);
        const myToken = token;
        // Chỉ chờ Voice đáp án; câu hỏi còn đang đọc thì cắt luôn để đọc kết quả.
        if (currentKind === "option") await Promise.race([current, new Promise((resolve) => window.setTimeout(resolve, 1000))]);
        if (myToken !== token) return false; // Bé đã thao tác khác trong lúc chờ: không đọc phản hồi cũ.
        const text = !isEnglishVoice && typeof result === "object" && result?.message
            ? result.message
            : correct ? standardText.correct : standardText.retry;
        return play([{ url, text }], "feedback");
    };

    window.learningVoice = { playQuestion, playOption, playFeedback, stop };

    host.querySelectorAll("[data-learning-replay]").forEach((button) => {
        button.addEventListener("click", () => {
            introPending = false;
            void playQuestion();
        });
    });

    // Bài dạng nút gửi biểu mẫu (dữ liệu cũ): đọc đáp án bé chọn xong rồi mới gửi.
    host.querySelectorAll("[data-speak-option]").forEach((button) => {
        button.addEventListener("click", async (event) => {
            if (button.dataset.voiceSubmitting === "true") return;
            const label = (button.dataset.speakOption || button.dataset.value || button.textContent || "").trim();
            if (!label) return;
            event.preventDefault();
            introPending = false;
            await Promise.race([playOption(label), new Promise((resolve) => window.setTimeout(resolve, 2500))]);
            button.dataset.voiceSubmitting = "true";
            button.form?.requestSubmit(button);
        }, { capture: true });
    });

    // ------------------------------------------------------------------ Đọc câu hỏi khi mở bài
    // Trang kết quả (nộp không qua fetch): đọc lời phản hồi; trang làm bài: đọc câu hỏi.
    const resultState = host.dataset.result;
    const intro = () => resultState === "correct" || resultState === "retry"
        ? playFeedback({ correct: resultState === "correct", message: host.dataset.feedbackText })
        : playQuestion();

    let introPending = true;
    const startIntro = async () => {
        if (!introPending) return;
        if (!voiceUrls.question && !resultState) {
            // Chưa có file câu hỏi: chờ máy chủ sinh rồi đọc đúng giọng chung; quá lâu thì bé vẫn làm bài được, chạm nút nghe sau.
            await Promise.race([ensureVoices(), new Promise((resolve) => window.setTimeout(resolve, 8000))]);
            if (!introPending) return;
        }
        introPending = false;
        const played = await intro();
        // Trình duyệt chặn tự phát: chờ lần chạm đầu tiên của bé.
        if (!played && autoplayBlocked) introPending = true;
    };

    // Lần chạm đầu tiên (khi tự phát bị chặn) sẽ đọc câu hỏi — trừ khi bé chạm vào đáp án hay nút điều khiển:
    // khi đó đọc đáp án, KHÔNG đọc lại câu hỏi chồng lên.
    const interactiveSelector = "button, a, input, label, [role='button'], [data-answer-value], .activity-option, .activity-drop-zone, .classification-chip, canvas";
    const onFirstGesture = (event) => {
        if (!introPending) return;
        if (event.target?.closest?.(interactiveSelector)) {
            introPending = false;
            return;
        }
        void startIntro();
    };
    document.addEventListener("click", onFirstGesture, true);
    document.addEventListener("touchend", onFirstGesture, true);
    document.addEventListener("keydown", (event) => {
        if (introPending && (event.key === "Enter" || event.key === " ")) void startIntro();
    }, true);

    // Rời trang (sang câu tiếp, quay lại bản đồ): dừng ngay mọi Voice của câu này.
    window.addEventListener("pagehide", stop);

    if (hasMissingVoice()) void ensureVoices();
    prefetchAll();
    // Đọc câu hỏi ngay khi trang làm bài sẵn sàng (không chờ tải hết hình ảnh).
    window.setTimeout(() => void startIntro(), 0);
})();
