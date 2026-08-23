(() => {
    const host = document.querySelector("[data-learning-voice]");
    if (!host) return;

    if (host.dataset.soundEnabled !== "true") {
        host.querySelectorAll("[data-learning-replay]").forEach((button) => button.hidden = true);
        return;
    }

    let payload = {};
    try {
        payload = JSON.parse(document.querySelector("[data-activity-payload]")?.value || "{}");
    } catch {
        payload = {};
    }

    const titleText = host.dataset.titleText || "";
    const titleAudioUrl = payload.titleAudioUrl || host.dataset.titleAudioUrl || "";
    const instruction = payload.instructionSpeechText || host.dataset.instructionText || "";
    const question = payload.questionSpeechText || host.dataset.questionText || "";
    const questionAudioUrl = payload.questionAudioUrl || host.dataset.questionAudioUrl || "";
    const instructionAudioUrl = payload.instructionAudioUrl || host.dataset.instructionAudioUrl || "";
    const correctAudioUrl = payload.correctAudioUrl || host.dataset.correctAudioUrl || "";
    const retryAudioUrl = payload.retryAudioUrl || host.dataset.retryAudioUrl || "";
    const optionAudio = payload.optionAudio || {};
    const feedback = host.dataset.result === "correct"
        ? payload.correctSpeechText || host.dataset.feedbackText || "Đúng rồi!"
        : host.dataset.result === "retry"
            ? payload.retrySpeechText || host.dataset.feedbackText || "Con thử lại nhé."
            : "";
    const feedbackAudioUrl = host.dataset.result === "correct"
        ? correctAudioUrl
        : host.dataset.result === "retry"
            ? retryAudioUrl
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

    const speak = (text) => new Promise(async (resolve) => {
        if (!text || !window.speechSynthesis) {
            resolve();
            return;
        }

        window.speechSynthesis.cancel();
        const voices = await loadVoices();
        const vietnameseVoice = voices.find((voice) => voice.lang.toLowerCase().startsWith("vi"));
        if (!vietnameseVoice) {
            resolve();
            return;
        }

        const utterance = new SpeechSynthesisUtterance(text);
        utterance.lang = "vi-VN";
        utterance.rate = 0.82;
        utterance.pitch = 1.04;
        utterance.voice = vietnameseVoice;
        utterance.onend = resolve;
        utterance.onerror = resolve;
        window.speechSynthesis.speak(utterance);
    });

    const playFile = (url) => new Promise((resolve, reject) => {
        if (!url) {
            reject();
            return;
        }
        activeAudio?.pause();
        activeAudio = new Audio(url);
        activeAudio.onended = resolve;
        activeAudio.onerror = reject;
        activeAudio.play().catch(reject);
    });

    const speakOrPlay = async (text, audioUrl) => {
        if (audioUrl) {
            try {
                await playFile(audioUrl);
                return;
            } catch {
                // Fall back to the browser voice when a generated file cannot play.
            }
        }
        await speak(text);
    };

    const playTitle = async () => {
        window.speechSynthesis?.cancel();
        await speakOrPlay(titleText || instruction || question, titleAudioUrl || instructionAudioUrl || questionAudioUrl);
    };

    const playQuestion = async () => {
        window.speechSynthesis?.cancel();
        if (feedback) {
            await speakOrPlay(feedback, feedbackAudioUrl);
            return;
        }

        await speakOrPlay(question || instruction, questionAudioUrl);
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
        const audioUrl = optionAudio[cleanLabel] || optionAudio[label];
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

    host.addEventListener("click", (event) => {
        const target = event.target.closest(".activity-option,.draggable-option,.activity-drop-zone,.activity-audio-button,.counting-object");
        if (!target || target.matches("[data-speak-option]") || target.matches("[data-learning-replay]")) {
            return;
        }

        const label = labelFromElement(target);
        if (label) {
            void playOption(label);
        }
    }, {capture: true});

    window.setTimeout(playQuestion, 350);
})();
