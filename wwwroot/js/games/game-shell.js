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

        // Nếu người dùng không chọn màn thủ công qua URL -> Mặc định theo tiến độ đã lưu lần trước
        const urlParams = new URLSearchParams(window.location.search);
        if (!urlParams.has('level')) {
            try {
                const saved = JSON.parse(localStorage.getItem('htl1_games_progress') || '{}');
                const savedProgress = saved[this.gameKey];
                if (savedProgress && savedProgress.currentLevel) {
                    this.currentLevel = Math.max(1, Math.min(this.levels.length || 39, savedProgress.currentLevel));
                }
            } catch (e) {}
        }

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
            const target = this.currentLevelData.mathFormula ||
                           this.currentLevelData.countIcons ||
                           this.currentLevelData.target ||
                           this.currentLevelData.targetWord ||
                           this.currentLevelData.answer || '★';
            this.targetCharBadge.textContent = target;
        }

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
                this.subHint.textContent = `Mẹo cho bé: Màn ${this.currentLevel} - Chạm trực tiếp vào khối chữ hoặc số để đào nhé!`;
            } else {
                this.subHint.textContent = `Mẹo cho bé: Màn ${this.currentLevel} - Chạm vào mục tiêu để ghi điểm nhé!`;
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

        // Đọc câu hỏi mở đầu
        setTimeout(() => {
            this.replayInstruction();
        }, 400);
    }

    replayInstruction() {
        if (this.currentLevelData && this.currentLevelData.instruction) {
            window.gameAudio.speak(this.currentLevelData.instruction);
        }
    }

    completeLevel(rewardStars = 1) {
        this.sessionStars += rewardStars;
        this.totalStars += rewardStars;
        if (this.starCounter) {
            this.starCounter.innerHTML = `⭐ ${this.totalStars}`;
        }

        this.saveProgressLocal(this.gameKey, this.currentLevel, rewardStars);
        this.saveProgressServer(this.gameKey, this.currentLevel, rewardStars);

        if (this.gameKey !== 'gold-miner') {
            window.gameAudio.playVictoryFanfare();
            this.showLevelCompleteModal(rewardStars);
        }
    }

    showLevelCompleteModal(starsWon) {
        const isFinalLevel = this.currentLevel >= this.levels.length;
        const starIcons = '⭐'.repeat(Math.max(1, Math.min(3, starsWon)));

        const modalOverlay = document.createElement('div');
        modalOverlay.className = 'game-modal-overlay';
        modalOverlay.innerHTML = `
            <div class="game-modal-card">
                <div class="modal-confetti-art">${isFinalLevel ? '🏆' : '🎉'}</div>
                <h2 class="modal-title">${isFinalLevel ? 'Chúc Mừng Bé Xuất Sắc!' : 'Hoàn Thành Màn Chơi!'}</h2>
                <div class="modal-stars-row">${starIcons}</div>
                <p class="modal-desc">Bé nhận được <strong>+${starsWon} Sao Vàng</strong>!</p>
                <div class="modal-actions-row">
                    <a href="/kids/games" class="modal-btn modal-btn-secondary">
                        <span class="material-symbols-outlined">home</span> Về Sảnh
                    </a>
                    ${!isFinalLevel ? `
                        <button type="button" class="modal-btn modal-btn-primary" id="modalNextLevelBtn">
                            Chơi Tiếp <span class="material-symbols-outlined">arrow_forward</span>
                        </button>
                    ` : `
                        <a href="/kids/games" class="modal-btn modal-btn-primary">
                            Nhận Thưởng <span class="material-symbols-outlined">stars</span>
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
                <p class="modal-desc">Bé nghỉ tay một chút nhé!</p>
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

    saveProgressLocal(gameKey, level, stars) {
        try {
            const raw = localStorage.getItem('htl1_games_progress') || '{}';
            const data = JSON.parse(raw);
            if (!data[gameKey]) {
                data[gameKey] = { completedLevels: [], totalStars: 0, currentLevel: 1 };
            }
            if (!data[gameKey].completedLevels.includes(level)) {
                data[gameKey].completedLevels.push(level);
            }
            data[gameKey].totalStars = (data[gameKey].totalStars || 0) + stars;
            data[gameKey].currentLevel = level + 1;
            localStorage.setItem('htl1_games_progress', JSON.stringify(data));
        } catch (e) {}
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
