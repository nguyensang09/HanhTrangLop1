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
    const instruction = payload.instructionSpeechText || host.dataset.instructionText || "";
    const question = payload.questionSpeechText || host.dataset.questionText || "";
    const questionAudioUrl = host.dataset.questionAudioUrl || "";
    const feedback = host.dataset.result === "correct"
        ? payload.correctSpeechText || host.dataset.feedbackText || "Đúng rồi!"
        : host.dataset.result === "retry"
            ? payload.retrySpeechText || host.dataset.feedbackText || "Con thử lại nhé."
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
        const utterance = new SpeechSynthesisUtterance(text);
        utterance.lang = "vi-VN";
        utterance.rate = 0.82;
        utterance.pitch = 1.04;
        const vietnameseVoice = voices
            .find((voice) => voice.lang.toLowerCase().startsWith("vi"));
        if (vietnameseVoice) utterance.voice = vietnameseVoice;
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

    const playQuestion = async () => {
        window.speechSynthesis?.cancel();
        if (feedback) {
            await speak(feedback);
            return;
        }
        await speak(instruction);
        if (questionAudioUrl) {
            try {
                await playFile(questionAudioUrl);
                return;
            } catch {
                // Thiết bị có thể chặn tự phát file; dùng giọng đọc làm phương án dự phòng.
            }
        }
        await speak(question);
    };

    host.querySelectorAll("[data-learning-replay]").forEach((button) => {
        button.addEventListener("click", playQuestion);
    });

    window.setTimeout(playQuestion, 350);
})();
