/**
 * GameAudioEngine - Hệ thống âm thanh Web Audio API & giọng nói tiếng Việt cho trẻ 5 tuổi
 */
class GameAudioEngine {
    constructor() {
        this.ctx = null;
        this.soundEnabled = true;
        this.currentUtterance = null;
        this.isSpeaking = false;
        this.onSpeakingStateChange = null;
        this.selectedVoice = null;
        this.audioCache = new Map();
        this.activeAudio = null;
        this.initVoiceSynthesis();
    }

    getAudioContext() {
        if (!this.ctx) {
            const AudioCtx = window.AudioContext || window.webkitAudioContext;
            if (AudioCtx) {
                this.ctx = new AudioCtx();
            }
        }
        if (this.ctx && this.ctx.state === 'suspended') {
            this.ctx.resume();
        }
        return this.ctx;
    }

    setSoundEnabled(enabled) {
        this.soundEnabled = !!enabled;
        if (!this.soundEnabled) {
            this.stopVoice();
        }
    }

    initVoiceSynthesis() {
        if (!('speechSynthesis' in window)) return;

        const updateVoices = () => {
            const voices = window.speechSynthesis.getVoices() || [];
            // Ưu tiên giọng tiếng Việt
            this.selectedVoice = voices.find(v => v.lang === 'vi-VN' || v.lang.startsWith('vi')) ||
                                voices.find(v => v.lang.includes('vi')) ||
                                null;
        };

        updateVoices();
        if (window.speechSynthesis.onvoiceschanged !== undefined) {
            window.speechSynthesis.onvoiceschanged = updateVoices;
        }
    }

    /**
     * Chuyển đổi ký tự sang cách phát âm mầm non tiếng Việt chuẩn
     */
    getPhonicText(rawChar) {
        if (!rawChar) return '';
        const ch = String(rawChar).trim();
        const upper = ch.toUpperCase();

        const letterMap = {
            'A': 'chữ A',
            'Ă': 'chữ Á',
            'Â': 'chữ Â',
            'B': 'chữ Bờ',
            'C': 'chữ Cờ',
            'D': 'chữ Dờ',
            'Đ': 'chữ Đờ',
            'E': 'chữ E',
            'Ê': 'chữ Ê',
            'G': 'chữ Gờ',
            'H': 'chữ Hờ',
            'I': 'chữ I',
            'K': 'chữ Ca',
            'L': 'chữ Lờ',
            'M': 'chữ Mờ',
            'N': 'chữ Nờ',
            'O': 'chữ O',
            'Ô': 'chữ Ô',
            'Ơ': 'chữ Ơ',
            'P': 'chữ Pờ',
            'Q': 'chữ Quy',
            'R': 'chữ Rờ',
            'S': 'chữ Sờ',
            'T': 'chữ Tờ',
            'U': 'chữ U',
            'Ư': 'chữ Ư',
            'V': 'chữ Vờ',
            'X': 'chữ Xờ',
            'Y': 'chữ Y'
        };

        const numberMap = {
            '0': 'số không',
            '1': 'số một',
            '2': 'số hai',
            '3': 'số ba',
            '4': 'số bốn',
            '5': 'số năm',
            '6': 'số sáu',
            '7': 'số bảy',
            '8': 'số tám',
            '9': 'số chín',
            '10': 'số mười'
        };

        if (letterMap[upper]) return letterMap[upper];
        if (numberMap[ch]) return numberMap[ch];
        return ch;
    }

    /**
     * Tìm kiếm file audio từ kho voice hệ thống chung (TextToSpeechCaches)
     */
    async resolveSystemVoice(text) {
        if (!text) return null;
        const key = text.trim();
        if (this.audioCache.has(key)) {
            return this.audioCache.get(key);
        }

        try {
            const resp = await fetch(`/kids/games/voice?text=${encodeURIComponent(key)}`);
            if (resp.ok) {
                const data = await resp.json();
                if (data && data.success && data.audioUrl) {
                    this.audioCache.set(key, data.audioUrl);
                    return data.audioUrl;
                }
            }
        } catch (e) {
            console.warn('[GameAudio] Error resolving system voice:', e);
        }
        return null;
    }

    /**
     * Phát voice từ hệ thống chung, nếu chưa có thì fallback sang TTS mầm non
     */
    async playSystemVoiceOrSpeak(text, onEnded = null) {
        if (!this.soundEnabled || !text) {
            if (onEnded) onEnded();
            return;
        }

        this.stopVoice();

        // 1. Tìm audio trong kho voice hệ thống
        const audioUrl = await this.resolveSystemVoice(text);
        if (audioUrl) {
            try {
                this.activeAudio = new Audio(audioUrl);
                this.isSpeaking = true;
                if (this.onSpeakingStateChange) this.onSpeakingStateChange(true);

                this.activeAudio.onended = () => {
                    this.isSpeaking = false;
                    if (this.onSpeakingStateChange) this.onSpeakingStateChange(false);
                    if (onEnded) onEnded();
                };
                this.activeAudio.onerror = () => {
                    this.isSpeaking = false;
                    if (this.onSpeakingStateChange) this.onSpeakingStateChange(false);
                    this.speak(text, onEnded);
                };
                await this.activeAudio.play();
                return;
            } catch (err) {
                console.warn('[GameAudio] Audio play failed, falling back to TTS:', err);
            }
        }

        // 2. Fallback sang Web Speech API
        this.speak(text, onEnded);
    }

    /**
     * Đọc tên chữ cái hoặc số vừa gắp được từ hệ thống chung
     */
    async speakLetter(char, onEnded = null) {
        const phonic = this.getPhonicText(char);
        await this.playSystemVoiceOrSpeak(phonic, onEnded);
    }

    /**
     * Đọc văn bản hướng dẫn hoặc kiến thức bằng giọng tiếng Việt thân thiện
     */
    speak(text, onEnded = null, customRate = 0.88) {
        if (!this.soundEnabled || !text) {
            if (onEnded) onEnded();
            return;
        }

        this.stopVoice();

        if (!('speechSynthesis' in window)) {
            if (onEnded) setTimeout(onEnded, 1200);
            return;
        }

        try {
            window.speechSynthesis.cancel();
            const utterance = new SpeechSynthesisUtterance(text);
            utterance.lang = 'vi-VN';
            utterance.rate = customRate; // Đọc thong thả, rõ ràng cho trẻ 5 tuổi
            utterance.pitch = 1.05; // Cao độ tươi vui
            if (this.selectedVoice) {
                utterance.voice = this.selectedVoice;
            }

            this.isSpeaking = true;
            if (this.onSpeakingStateChange) this.onSpeakingStateChange(true);

            utterance.onend = () => {
                this.isSpeaking = false;
                if (this.onSpeakingStateChange) this.onSpeakingStateChange(false);
                if (onEnded) onEnded();
            };

            utterance.onerror = () => {
                this.isSpeaking = false;
                if (this.onSpeakingStateChange) this.onSpeakingStateChange(false);
                if (onEnded) onEnded();
            };

            this.currentUtterance = utterance;
            window.speechSynthesis.speak(utterance);
        } catch (e) {
            console.warn('[GameAudio] Speech error:', e);
            this.isSpeaking = false;
            if (this.onSpeakingStateChange) this.onSpeakingStateChange(false);
            if (onEnded) onEnded();
        }
    }

    stopVoice() {
        if (this.activeAudio) {
            try {
                this.activeAudio.pause();
                this.activeAudio.currentTime = 0;
            } catch (e) {}
            this.activeAudio = null;
        }
        if ('speechSynthesis' in window) {
            try {
                window.speechSynthesis.cancel();
            } catch (e) {}
        }
        this.isSpeaking = false;
        if (this.onSpeakingStateChange) this.onSpeakingStateChange(false);
    }

    /* ==================== HIỆU ỨNG ÂM THANH SYNTHESIZER ==================== */

    playBubblePop() {
        if (!this.soundEnabled) return;
        const ctx = this.getAudioContext();
        if (!ctx) return;

        const now = ctx.currentTime;
        const osc = ctx.createOscillator();
        const gain = ctx.createGain();

        osc.type = 'sine';
        // Hiệu ứng "bụp" bong bóng nước
        osc.frequency.setValueAtTime(450, now);
        osc.frequency.exponentialRampToValueAtTime(180, now + 0.08);

        gain.gain.setValueAtTime(0.3, now);
        gain.gain.exponentialRampToValueAtTime(0.001, now + 0.09);

        osc.connect(gain);
        gain.connect(ctx.destination);

        osc.start(now);
        osc.stop(now + 0.09);
    }

    playCorrectChime() {
        if (!this.soundEnabled) return;
        const ctx = this.getAudioContext();
        if (!ctx) return;

        const now = ctx.currentTime;
        // Hợp âm vui vẻ: C5 (523Hz), E5 (659Hz), G5 (784Hz), C6 (1046Hz)
        const notes = [523.25, 659.25, 783.99, 1046.50];
        notes.forEach((freq, idx) => {
            const osc = ctx.createOscillator();
            const gain = ctx.createGain();
            const noteStart = now + idx * 0.08;

            osc.type = 'triangle';
            osc.frequency.setValueAtTime(freq, noteStart);

            gain.gain.setValueAtTime(0.2, noteStart);
            gain.gain.exponentialRampToValueAtTime(0.001, noteStart + 0.25);

            osc.connect(gain);
            gain.connect(ctx.destination);

            osc.start(noteStart);
            osc.stop(noteStart + 0.25);
        });
    }

    playWrongWobble() {
        if (!this.soundEnabled) return;
        const ctx = this.getAudioContext();
        if (!ctx) return;

        const now = ctx.currentTime;
        const osc = ctx.createOscillator();
        const gain = ctx.createGain();

        // Rung nhẹ nhàng, không gây giật mình hay sợ hãi
        osc.type = 'sine';
        osc.frequency.setValueAtTime(280, now);
        osc.frequency.linearRampToValueAtTime(220, now + 0.15);
        osc.frequency.linearRampToValueAtTime(260, now + 0.3);

        gain.gain.setValueAtTime(0.16, now);
        gain.gain.exponentialRampToValueAtTime(0.001, now + 0.32);

        osc.connect(gain);
        gain.connect(ctx.destination);

        osc.start(now);
        osc.stop(now + 0.32);
    }

    playStarTing() {
        if (!this.soundEnabled) return;
        const ctx = this.getAudioContext();
        if (!ctx) return;

        const now = ctx.currentTime;
        const osc = ctx.createOscillator();
        const gain = ctx.createGain();

        osc.type = 'sine';
        osc.frequency.setValueAtTime(1318.5, now); // E6
        osc.frequency.exponentialRampToValueAtTime(2093.0, now + 0.15); // C7

        gain.gain.setValueAtTime(0.25, now);
        gain.gain.exponentialRampToValueAtTime(0.001, now + 0.4);

        osc.connect(gain);
        gain.connect(ctx.destination);

        osc.start(now);
        osc.stop(now + 0.4);
    }

    playClawDrop() {
        if (!this.soundEnabled) return;
        const ctx = this.getAudioContext();
        if (!ctx) return;

        const now = ctx.currentTime;
        const osc = ctx.createOscillator();
        const gain = ctx.createGain();

        osc.type = 'sine';
        osc.frequency.setValueAtTime(600, now);
        osc.frequency.exponentialRampToValueAtTime(200, now + 0.2);

        gain.gain.setValueAtTime(0.18, now);
        gain.gain.exponentialRampToValueAtTime(0.001, now + 0.22);

        osc.connect(gain);
        gain.connect(ctx.destination);

        osc.start(now);
        osc.stop(now + 0.22);
    }

    playClawCatch() {
        if (!this.soundEnabled) return;
        const ctx = this.getAudioContext();
        if (!ctx) return;

        const now = ctx.currentTime;
        const osc = ctx.createOscillator();
        const gain = ctx.createGain();

        osc.type = 'triangle';
        osc.frequency.setValueAtTime(350, now);
        osc.frequency.linearRampToValueAtTime(700, now + 0.12);

        gain.gain.setValueAtTime(0.22, now);
        gain.gain.exponentialRampToValueAtTime(0.001, now + 0.15);

        osc.connect(gain);
        gain.connect(ctx.destination);

        osc.start(now);
        osc.stop(now + 0.15);
    }

    playSnapSlot() {
        if (!this.soundEnabled) return;
        const ctx = this.getAudioContext();
        if (!ctx) return;

        const now = ctx.currentTime;
        const osc = ctx.createOscillator();
        const gain = ctx.createGain();

        osc.type = 'sine';
        osc.frequency.setValueAtTime(440, now);
        osc.frequency.exponentialRampToValueAtTime(880, now + 0.08);

        gain.gain.setValueAtTime(0.2, now);
        gain.gain.exponentialRampToValueAtTime(0.001, now + 0.12);

        osc.connect(gain);
        gain.connect(ctx.destination);

        osc.start(now);
        osc.stop(now + 0.12);
    }

    playTrainWhistle() {
        if (!this.soundEnabled) return;
        const ctx = this.getAudioContext();
        if (!ctx) return;

        const now = ctx.currentTime;
        // Còi tàu 2 nhịp: tu... tu...!
        [0, 0.22].forEach(delay => {
            const osc = ctx.createOscillator();
            const gain = ctx.createGain();
            const start = now + delay;

            osc.type = 'sawtooth';
            osc.frequency.setValueAtTime(650, start);
            osc.frequency.linearRampToValueAtTime(630, start + 0.16);

            gain.gain.setValueAtTime(0.12, start);
            gain.gain.exponentialRampToValueAtTime(0.001, start + 0.18);

            osc.connect(gain);
            gain.connect(ctx.destination);

            osc.start(start);
            osc.stop(start + 0.18);
        });
    }

    playVictoryFanfare() {
        if (!this.soundEnabled) return;
        const ctx = this.getAudioContext();
        if (!ctx) return;

        const now = ctx.currentTime;
        const fanfare = [
            { f: 523.25, d: 0.12 }, // C5
            { f: 659.25, d: 0.12 }, // E5
            { f: 783.99, d: 0.12 }, // G5
            { f: 1046.50, d: 0.35 } // C6
        ];

        let offset = 0;
        fanfare.forEach(item => {
            const osc = ctx.createOscillator();
            const gain = ctx.createGain();
            const t = now + offset;

            osc.type = 'triangle';
            osc.frequency.setValueAtTime(item.f, t);

            gain.gain.setValueAtTime(0.25, t);
            gain.gain.exponentialRampToValueAtTime(0.001, t + item.d);

            osc.connect(gain);
            gain.connect(ctx.destination);

            osc.start(t);
            osc.stop(t + item.d);

            offset += item.d + 0.04;
        });
    }

    playCoinCollect() {
        if (!this.soundEnabled) return;
        const ctx = this.getAudioContext();
        if (!ctx) return;

        const now = ctx.currentTime;
        const osc = ctx.createOscillator();
        const gain = ctx.createGain();

        osc.type = 'sine';
        osc.frequency.setValueAtTime(987.77, now); // B5
        osc.frequency.setValueAtTime(1318.51, now + 0.08); // E6

        gain.gain.setValueAtTime(0.22, now);
        gain.gain.exponentialRampToValueAtTime(0.001, now + 0.3);

        osc.connect(gain);
        gain.connect(ctx.destination);

        osc.start(now);
        osc.stop(now + 0.3);
    }

    playChestOpen() {
        if (!this.soundEnabled) return;
        const ctx = this.getAudioContext();
        if (!ctx) return;

        const now = ctx.currentTime;
        const notes = [440, 554.37, 659.25, 880, 1108.73, 1318.51];
        notes.forEach((freq, idx) => {
            const osc = ctx.createOscillator();
            const gain = ctx.createGain();
            const t = now + idx * 0.06;

            osc.type = 'triangle';
            osc.frequency.setValueAtTime(freq, t);

            gain.gain.setValueAtTime(0.2, t);
            gain.gain.exponentialRampToValueAtTime(0.001, t + 0.35);

            osc.connect(gain);
            gain.connect(ctx.destination);

            osc.start(t);
            osc.stop(t + 0.35);
        });
    }

    playSparkle() {
        if (!this.soundEnabled) return;
        const ctx = this.getAudioContext();
        if (!ctx) return;

        const now = ctx.currentTime;
        const osc = ctx.createOscillator();
        const gain = ctx.createGain();

        osc.type = 'sine';
        osc.frequency.setValueAtTime(1200, now);
        osc.frequency.linearRampToValueAtTime(2400, now + 0.15);

        gain.gain.setValueAtTime(0.15, now);
        gain.gain.exponentialRampToValueAtTime(0.001, now + 0.2);

        osc.connect(gain);
        gain.connect(ctx.destination);

        osc.start(now);
        osc.stop(now + 0.2);
    }
}

// Khởi tạo đối tượng toàn cục
window.gameAudio = new GameAudioEngine();
