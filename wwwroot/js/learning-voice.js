(() => {
    const host = document.querySelector("[data-learning-voice]");
    if (!host) return;

    if (host.dataset.soundEnabled !== "true") {
        host.querySelectorAll("[data-learning-replay]").forEach((button) => button.hidden = true);
        return;
    }

    const isEnglishVoice = host.dataset.englishVoice === "true";

    let payload = {};
    try {
        payload = JSON.parse(document.querySelector("[data-activity-payload]")?.value || "{}");
    } catch {
        payload = {};
    }

    const titleText = host.dataset.titleText || "";
    const titleAudioUrl = payload.titleAudioUrl || host.dataset.titleAudioUrl || "";
    const titleAudioUrlEn = payload.titleAudioUrlEn || host.dataset.titleAudioUrlEn || "";

    const instruction = payload.instructionSpeechText || host.dataset.instructionText || "";
    const instructionAudioUrl = payload.instructionAudioUrl || host.dataset.instructionAudioUrl || "";
    const instructionAudioUrlEn = payload.instructionAudioUrlEn || host.dataset.instructionAudioUrlEn || "";

    const question = payload.questionSpeechText || host.dataset.questionText || "";
    const questionAudioUrl = payload.questionAudioUrl || host.dataset.questionAudioUrl || "";
    const questionAudioUrlEn = payload.questionAudioUrlEn || host.dataset.questionAudioUrlEn || "";

    const correctAudioUrl = payload.correctAudioUrl || host.dataset.correctAudioUrl || "";
    const correctAudioUrlEn = payload.correctAudioUrlEn || host.dataset.correctAudioUrlEn || "";

    const retryAudioUrl = payload.retryAudioUrl || host.dataset.retryAudioUrl || "";
    const retryAudioUrlEn = payload.retryAudioUrlEn || host.dataset.retryAudioUrlEn || "";

    const optionAudio = payload.optionAudio || {};
    const optionAudioEn = payload.optionAudioEn || {};

    const feedback = host.dataset.result === "correct"
        ? payload.correctSpeechText || host.dataset.feedbackText || "Đúng rồi!"
        : host.dataset.result === "retry"
            ? payload.retrySpeechText || host.dataset.feedbackText || "Con thử lại nhé."
            : "";

    // Ưu tiên phát Voice EN nếu bật English Voice, tự động fallback sang Voice VI nếu thiếu file EN
    const activeTitleAudio = isEnglishVoice ? (titleAudioUrlEn || titleAudioUrl) : titleAudioUrl;
    const activeInstructionAudio = isEnglishVoice ? (instructionAudioUrlEn || instructionAudioUrl) : instructionAudioUrl;
    const activeQuestionAudio = isEnglishVoice ? (questionAudioUrlEn || questionAudioUrl) : questionAudioUrl;
    const activeCorrectAudio = isEnglishVoice ? (correctAudioUrlEn || correctAudioUrl) : correctAudioUrl;
    const activeRetryAudio = isEnglishVoice ? (retryAudioUrlEn || retryAudioUrl) : retryAudioUrl;

    const feedbackAudioUrl = host.dataset.result === "correct"
        ? activeCorrectAudio
        : host.dataset.result === "retry"
            ? activeRetryAudio
            : "";

    let activeAudio = null;

    const loadVoices = () => new Promise((resolve) => {
        const voices = window.speechSynthesis?.getVoices?.() || [];
        if (voices.length) {
            resolve(voices);
            return;
        }

        const timeout = window.setTimeout(() => {
            window.speechSynthesis?.removeEventListener?.("voiceschanged", onVoicesChanged);
            resolve(window.speechSynthesis?.getVoices?.() || []);
        }, 800);
        function onVoicesChanged() {
            window.clearTimeout(timeout);
            resolve(window.speechSynthesis?.getVoices?.() || []);
        }
        window.speechSynthesis?.addEventListener?.("voiceschanged", onVoicesChanged, {once: true});
    });

    const setMascotSpeaking = (speaking) => {
        const mascot = host.querySelector(".mascot-speaking, .retry-mascot-img");
        if (mascot) {
            mascot.classList.toggle("is-speaking", speaking);
        }
        const replayBtns = host.querySelectorAll("[data-learning-replay]");
        replayBtns.forEach((btn) => btn.classList.toggle("is-playing", speaking));
    };

    const isVietnameseText = (str) => /[àáạảãâầấậẩẫăằắặẳẵèéẹẻẽêềếệểễìíịỉĩòóọỏõôồốộổỗơờớợởỡùúụủũưừứựửữỳýỵỷỹđĐ]/i.test(str || "");

    const speak = async (text) => {
        if (!text || !window.speechSynthesis) {
            return;
        }

        window.speechSynthesis.cancel();
        const voices = await loadVoices();
        const targetLang = (isEnglishVoice && !isVietnameseText(text)) ? "en" : "vi";
        const matchedVoice = voices.find((voice) => voice.lang.toLowerCase().startsWith(targetLang));
        if (!matchedVoice && !voices.length) {
            return;
        }

        return new Promise((resolve) => {
            const utterance = new SpeechSynthesisUtterance(text);
            utterance.lang = targetLang === "en" ? "en-US" : "vi-VN";
            utterance.rate = targetLang === "en" ? 0.85 : 0.82;
            utterance.pitch = 1.04;
            if (matchedVoice) {
                utterance.voice = matchedVoice;
            }
            utterance.onend = resolve;
            utterance.onerror = resolve;
            window.speechSynthesis.speak(utterance);
        });
    };

    const playFile = (url) => new Promise((resolve, reject) => {
        if (!url) {
            reject(new Error("No URL"));
            return;
        }
        try {
            activeAudio?.pause();
            activeAudio = new Audio(url);
            activeAudio.onended = resolve;
            activeAudio.onerror = (e) => reject(e);
            const promise = activeAudio.play();
            if (promise !== undefined) {
                promise.then(resolve).catch((err) => {
                    const resumeOnGesture = () => {
                        document.removeEventListener("pointerdown", resumeOnGesture);
                        document.removeEventListener("keydown", resumeOnGesture);
                        activeAudio?.play().then(resolve).catch(reject);
                    };
                    document.addEventListener("pointerdown", resumeOnGesture, { once: true });
                    document.addEventListener("keydown", resumeOnGesture, { once: true });
                    reject(err);
                });
            }
        } catch (e) {
            reject(e);
        }
    });

    const speakOrPlay = async (text, audioUrl) => {
        setMascotSpeaking(true);
        try {
            if (audioUrl) {
                try {
                    await playFile(audioUrl);
                    return;
                } catch {
                    // Fall back to the browser voice when a generated file cannot play.
                }
            }
            await speak(text);
        } finally {
            setMascotSpeaking(false);
        }
    };

    const playTitle = async () => {
        window.speechSynthesis?.cancel();
        await speakOrPlay(titleText || instruction || question, activeTitleAudio || activeInstructionAudio || activeQuestionAudio);
    };

    const playQuestion = async () => {
        window.speechSynthesis?.cancel();
        if (feedback) {
            await speakOrPlay(feedback, feedbackAudioUrl || activeRetryAudio || activeCorrectAudio);
            return;
        }

        const text = question || instruction || titleText;
        const audioUrl = activeQuestionAudio || activeInstructionAudio || activeTitleAudio;
        await speakOrPlay(text, audioUrl);
    };

    const labelFromElement = (element) => {
        const explicit = element.dataset.speakOption || element.dataset.value;
        if (explicit) return explicit.trim();
        return element.querySelector(".answer-label")?.textContent?.trim() ||
            element.textContent?.trim() ||
            "";
    };

    const playOption = async (label) => {
        if (!label) return false;
        const cleanLabel = label.trim();
        let audioUrl = "";
        if (isEnglishVoice) {
            audioUrl = optionAudioEn[cleanLabel] || optionAudioEn[label] || optionAudio[cleanLabel] || optionAudio[label];
        } else {
            audioUrl = optionAudio[cleanLabel] || optionAudio[label];
        }
        await speakOrPlay(cleanLabel, audioUrl);
        return true;
    };

    host.querySelectorAll("[data-learning-replay]").forEach((button) => {
        button.addEventListener("click", () => {
            const mode = button.dataset.learningReplay || "question";
            if (mode === "title") {
                void playTitle();
                return;
            }
            void playQuestion();
        });
    });

    host.querySelectorAll("[data-speak-option]").forEach((button) => {
        button.addEventListener("click", async (event) => {
            if (button.dataset.voiceSubmitting === "true") {
                return;
            }

            const label = labelFromElement(button);
            if (!label) {
                return;
            }

            event.preventDefault();
            await playOption(label);
            button.dataset.voiceSubmitting = "true";
            button.form?.requestSubmit(button);
        }, {capture: true});
    });

    let hasPlayedInitialVoice = false;

    const tryPlayInitialVoice = () => {
        if (hasPlayedInitialVoice) return;
        playQuestion()
            .then(() => { hasPlayedInitialVoice = true; })
            .catch(() => {});
    };

    // Try autoplay on page ready
    window.setTimeout(tryPlayInitialVoice, 150);
    window.setTimeout(tryPlayInitialVoice, 500);

    // If browser autoplay policy blocked audio before user gesture, play immediately on first tap/click anywhere!
    const onUserInteraction = () => {
        if (!hasPlayedInitialVoice) {
            tryPlayInitialVoice();
        }
        document.removeEventListener("pointerdown", onUserInteraction, true);
        document.removeEventListener("touchstart", onUserInteraction, true);
        document.removeEventListener("keydown", onUserInteraction, true);
    };
    document.addEventListener("pointerdown", onUserInteraction, true);
    document.addEventListener("touchstart", onUserInteraction, true);
    document.addEventListener("keydown", onUserInteraction, true);
})();
