/**
 * GameShell - Khung điều phối trung tâm chuẩn Stitch 3D Gaming Hub
 */
class GameShell {
    constructor() {
        this.host = document.querySelector('[data-game-shell]');
        if (!this.host) return;

        this.gameKey = this.host.dataset.gameKey || 'bubble';
        this.currentLevel = parseInt(this.host.dataset.initialLevel || '1', 10);
        this.totalStars = parseInt(this.host.dataset.totalStars || '0', 10);
        this.sessionStars = 0;
        this.soundEnabled = this.host.dataset.soundEnabled !== 'false';

        this.levels = (window.GAME_LEVELS && window.GAME_LEVELS[this.gameKey]) || [];
        this.currentGame = null;
        this.currentLevelData = null;

        // Tiến độ lưu theo bé trên máy chủ (màn → số sao tốt nhất); màn mở đầu do máy chủ chọn (màn chưa qua đầu tiên).
        this.levelStars = {};
        try {
            this.levelStars = JSON.parse(this.host.dataset.levelStars || '{}') || {};
        } catch (e) {}
        // Giọng đọc theo cài đặt của phụ huynh: tiếng Anh hoặc tiếng Việt, không tự trộn hai thứ tiếng.
        this.englishVoice = this.host.dataset.englishVoice === 'true';
        window.gameAudio?.configureLanguage?.(this.englishVoice, window.GAME_LEVELS || {});

        this.initDOM();
        this.initAudio();
        this.startLevel(this.currentLevel);
    }

    initDOM() {
        this.starCounter = document.getElementById('shellStarCounter');
        this.promptText = document.getElementById('guidePromptText');
        this.subHint = document.getElementById('guideSubHint');
        this.stageContainer = document.getElementById('gamePlayStage');
        this.voiceReplayBtn = document.getElementById('voiceReplayBtn');
        this.soundToggleBtn = document.getElementById('soundToggleBtn');
        this.pauseBtn = document.getElementById('pauseBtn');
        this.mascotImg = document.getElementById('guideMascotImg');

        this.missionActionText = document.getElementById('missionActionText');
        this.targetCharBadge = document.getElementById('targetCharBadge');
        this.progressDots = document.getElementById('progressDotsContainer');
        this.hintWordText = document.getElementById('hintWordText');
        this.memoryWordCard = document.getElementById('memoryWordCard');

        if (this.voiceReplayBtn) {
            this.voiceReplayBtn.addEventListener('click', () => this.replayInstruction());
        }

        if (this.soundToggleBtn) {
            this.soundToggleBtn.addEventListener('click', () => {
                this.soundEnabled = !this.soundEnabled;
                window.gameAudio.setSoundEnabled(this.soundEnabled);
                this.soundToggleBtn.innerHTML = `<span class="material-symbols-outlined" style="font-size: 1.3rem;">${this.soundEnabled ? 'volume_up' : 'volume_off'}</span>`;
                this.soundToggleBtn.title = this.soundEnabled ? 'Tắt âm thanh' : 'Bật âm thanh';
            });
        }

        if (this.pauseBtn) {
            this.pauseBtn.addEventListener('click', () => this.showPauseModal());
        }
    }

    initAudio() {
        window.gameAudio.setSoundEnabled(this.soundEnabled);
        window.gameAudio.onSpeakingStateChange = (speaking) => {
            if (this.mascotImg) {
                this.mascotImg.classList.toggle('is-speaking', speaking);
            }
            if (this.voiceReplayBtn) {
                this.voiceReplayBtn.classList.toggle('is-speaking', speaking);
            }
        };

        const unlockAudio = () => {
            window.gameAudio.getAudioContext();
            document.removeEventListener('pointerdown', unlockAudio);
            document.removeEventListener('touchstart', unlockAudio);
        };
        document.addEventListener('pointerdown', unlockAudio, { once: true });
        document.addEventListener('touchstart', unlockAudio, { once: true });
    }

    startLevel(levelNum) {
        this.currentLevel = levelNum;
        const levelIdx = Math.max(0, Math.min(levelNum - 1, this.levels.length - 1));
        this.currentLevelData = this.levels[levelIdx];

        // Cập nhật tham số URL mà không cần tải lại trang
        try {
            const url = new URL(window.location);
            url.searchParams.set('level', levelNum);
            window.history.replaceState({}, '', url);
        } catch (e) {}

        if (!this.currentLevelData) {
            console.error('Level data not found for level:', levelNum);
            return;
        }

        // Cập nhật Mission Capsule
        if (this.missionActionText) {
            let action = 'BẮN CHỮ';
            if (this.gameKey === 'gold-miner') {
                action = this.currentLevelData.mode === 'count-objects' ? 'ĐẾM' :
                         (this.currentLevelData.mode === 'math-add' || this.currentLevelData.mode === 'math-sub') ? 'TÍNH' : 'ĐÀO';
            } else if (this.gameKey === 'balloon-word') {
                action = 'GHÉP';
            } else if (this.gameKey === 'number-train') {
                action = 'TÌM TOA';
            }
            this.missionActionText.textContent = action;
        }

        if (this.targetCharBadge) {
            // Đoàn tàu: nhiệm vụ là tìm toa còn thiếu nên không bao giờ hiện sẵn đáp án lên nhãn.
            const target = this.gameKey === 'number-train'
                ? (this.currentLevelData.countIcons || '?')
                : this.currentLevelData.mathFormula ||
                  this.currentLevelData.countIcons ||
                  this.currentLevelData.target ||
                  this.currentLevelData.targetWord || '★';
            this.targetCharBadge.textContent = target;
        }
        const levelText = document.getElementById('shellLevelText');
        if (levelText) levelText.textContent = String(this.currentLevel);

        // Cập nhật Progress Dots
        if (this.progressDots) {
            const total = this.levels.length;
            const current = this.currentLevel;
            let html = '';
            for (let i = 1; i <= Math.min(total, 6); i++) {
                if (i < current) {
                    html += '<span style="width: 14px; height: 14px; border-radius: 50%; background: #10b981; border: 1.5px solid #ffffff; display: flex; align-items: center; justify-content: center; font-size: 8px; color: white; font-weight: bold;">✓</span>';
                } else if (i === current) {
                    html += '<span style="width: 14px; height: 14px; border-radius: 50%; background: #ffffff; border: 2px solid #d97706; box-shadow: 0 0 8px #f59e0b;"></span>';
                } else {
                    html += '<span style="width: 14px; height: 14px; border-radius: 50%; background: rgba(255,255,255,0.4); border: 1.5px solid rgba(255,255,255,0.7);"></span>';
                }
            }
            this.progressDots.innerHTML = html;
        }

        // Cập nhật Mascot Speech Bubble
        if (this.promptText) {
            this.promptText.textContent = `“${this.currentLevelData.instruction || ''}”`;
        }

        // Cập nhật Hint Word
        if (this.hintWordText && this.memoryWordCard) {
            const hint = this.currentLevelData.mathFormula ||
                         this.currentLevelData.countIcons ||
                         this.currentLevelData.hintWord ||
                         this.currentLevelData.imageDesc;
            if (hint && this.gameKey !== 'gold-miner') {
                this.hintWordText.textContent = hint;
                this.memoryWordCard.style.display = 'flex';
            } else {
                this.memoryWordCard.style.display = 'none';
            }
        }

        if (this.subHint) {
            if (this.gameKey === 'gold-miner') {
                this.subHint.textContent = `Mẹo nhỏ: chạm thẳng vào khối chữ hoặc số để đào nhé!`;
            } else {
                this.subHint.textContent = `Mẹo nhỏ: chạm vào mục tiêu để ghi điểm nhé!`;
            }
        }

        // Hủy engine cũ nếu có
        if (this.currentGame && typeof this.currentGame.destroy === 'function') {
            this.currentGame.destroy();
        }

        // Khởi tạo Engine tương ứng
        if (this.gameKey === 'bubble') {
            this.currentGame = new window.BubbleGame(this.stageContainer, this);
        } else if (this.gameKey === 'gold-miner') {
            this.currentGame = new window.GoldMinerGame(this.stageContainer, this);
        } else if (this.gameKey === 'balloon-word') {
            this.currentGame = new window.BalloonWordGame(this.stageContainer, this);
        } else if (this.gameKey === 'number-train') {
            this.currentGame = new window.NumberTrainGame(this.stageContainer, this);
        }

        if (this.currentGame) {
            this.currentGame.init(this.currentLevelData);
        }

        // Các trò chơi tự chủ động phát âm giọng bài học chuẩn hóa
    }

    replayInstruction() {
        if (this.currentLevelData && this.currentLevelData.instruction) {
            // playSystemVoiceOrSpeak tự đổi sang câu tiếng Anh viết sẵn khi bé dùng giọng tiếng Anh.
            window.gameAudio?.playSystemVoiceOrSpeak(this.currentLevelData.instruction);
        }
    }

    completeLevel(rewardStars = 1) {
        this.sessionStars += rewardStars;
        this.totalStars += rewardStars;
        if (this.starCounter) {
            this.starCounter.innerHTML = `⭐ ${this.totalStars}`;
        }

        const best = Math.max(Number(this.levelStars[this.currentLevel] || 0), Math.min(3, Math.max(1, rewardStars)));
        this.levelStars[this.currentLevel] = best;
        this.saveProgressServer(this.gameKey, this.currentLevel, rewardStars);
    }

    /** Trò chơi báo đã qua màn: ghi nhận ngay ở bảng chọn màn và lưu lên máy chủ theo bé. */
    markLevelDone(level, stars = 1) {
        const value = Math.min(3, Math.max(1, Number(stars) || 1));
        this.levelStars[level] = Math.max(Number(this.levelStars[level] || 0), value);
        this.saveProgressServer(this.gameKey, level, value);
    }

    isLevelDone(level) {
        return Number(this.levelStars[level] || 0) > 0;
    }

    starsFor(level) {
        return Number(this.levelStars[level] || 0);
    }

    showLevelCompleteModal(starsWon) {
        const isFinalLevel = this.currentLevel >= this.levels.length;

        const modalOverlay = document.createElement('div');
        modalOverlay.className = 'game-modal-overlay';
        modalOverlay.innerHTML = `
            <div class="game-modal-card">
                <div class="modal-confetti-art">${isFinalLevel ? '🏆' : '🎉'}</div>
                <h2 class="modal-title">${isFinalLevel ? 'Con giỏi quá, xong hết rồi!' : 'Hoàn Thành Màn Chơi!'}</h2>
                <p class="modal-desc">Con đã hoàn thành màn ${this.currentLevel}! ${"⭐".repeat(Math.max(1, Math.min(3, starsWon || 1)))}</p>
                <div class="modal-actions-row">
                    <a href="/kids/games" class="modal-btn modal-btn-secondary">
                        <span class="material-symbols-outlined">home</span> Về Sảnh
                    </a>
                    ${!isFinalLevel ? `
                        <button type="button" class="modal-btn modal-btn-primary" id="modalNextLevelBtn">
                            Màn Tiếp Theo <span class="material-symbols-outlined">arrow_forward</span>
                        </button>
                    ` : `
                        <a href="/kids/games" class="modal-btn modal-btn-primary">
                            Về Sảnh <span class="material-symbols-outlined">home</span>
                        </a>
                    `}
                </div>
            </div>
        `;

        document.body.appendChild(modalOverlay);

        const nextBtn = modalOverlay.querySelector('#modalNextLevelBtn');
        if (nextBtn) {
            nextBtn.addEventListener('click', () => {
                modalOverlay.remove();
                this.startLevel(this.currentLevel + 1);
            });
        }
    }

    showPauseModal() {
        const modalOverlay = document.createElement('div');
        modalOverlay.className = 'game-modal-overlay';
        modalOverlay.innerHTML = `
            <div class="game-modal-card">
                <div class="modal-confetti-art">⏸️</div>
                <h2 class="modal-title">Tạm Dừng Trò Chơi</h2>
                <p class="modal-desc">Con nghỉ tay một chút nhé!</p>
                <div class="modal-actions-row">
                    <a href="/kids/games" class="modal-btn modal-btn-secondary">
                        <span class="material-symbols-outlined">home</span> Về Sảnh
                    </a>
                    <button type="button" class="modal-btn modal-btn-primary" id="resumeBtn">
                        Tiếp Tục <span class="material-symbols-outlined">play_arrow</span>
                    </button>
                </div>
            </div>
        `;

        document.body.appendChild(modalOverlay);

        modalOverlay.querySelector('#resumeBtn')?.addEventListener('click', () => {
            modalOverlay.remove();
        });
    }

    saveProgressServer(gameKey, level, stars) {
        fetch('/kids/games/save-progress', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                gameKey: gameKey,
                level: level,
                starsEarned: stars,
                isCompleted: true
            })
        }).catch(() => {});
    }
}

document.addEventListener('DOMContentLoaded', () => {
    window.gameShell = new GameShell();
});
