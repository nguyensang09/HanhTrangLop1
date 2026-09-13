/**
 * BalloonWordGame - Khinh Khí Cầu Ghép Vần (Hot Air Balloon Phonics Flight)
 * - Nâng cấp giao diện 3D bầu trời mây bồng bềnh, khinh khí cầu rực rỡ sắc màu, mỏ đốt lửa bừng sáng.
 * - Sửa dứt điểm lỗi nháy chữ khi chạm: Đám mây bay lượn êm ái 60fps, tương tác tức thì.
 * - Tự động qua màn sau 1.8s như Đào Vàng, bé không cần phải bấm nút.
 * - Voice tiếng Anh tinh gọn, đọc chuẩn chữ cái và từ vựng khi khinh khí cầu bay vút lên cao.
 */
class BalloonWordGame {
    constructor(container, shell) {
        this.container = container;
        this.shell = shell;
        this.currentLevelData = null;

        this.isCompleted = false;
        this.soundEnabled = true;

        this.assembledLetters = [];
        this.targetLetters = [];
        this.floatingClouds = [];
        this.animFrameId = null;
        this.lastTime = 0;
    }

    init(levelData) {
        this.destroy();
        this.currentLevelData = levelData;
        this.isCompleted = false;
        this.assembledLetters = [];
        this.targetLetters = levelData?.letters || ['B', 'A'];
        this.floatingClouds = [];

        this.saveCurrentLevelProgress(levelData?.level || 1);
        this.render();
        this.spawnSkyClouds();
        this.startSkyFlightLoop();

        setTimeout(() => {
            window.gameAudio?.playFlameWhoosh();
        }, 200);

        setTimeout(() => {
            if (this.currentLevelData?.instructionEn) {
                window.gameAudio?.speakEnglish(this.currentLevelData.instructionEn);
            }
        }, 650);
    }

    destroy() {
        if (this.animFrameId) {
            cancelAnimationFrame(this.animFrameId);
            this.animFrameId = null;
        }
        this.floatingClouds = [];
        this.isCompleted = true;
    }

    saveCurrentLevelProgress(level) {
        try {
            const raw = localStorage.getItem('htl1_games_progress') || '{}';
            const data = JSON.parse(raw);
            if (!data['balloon-word']) data['balloon-word'] = {};
            data['balloon-word'].currentLevel = level;
            localStorage.setItem('htl1_games_progress', JSON.stringify(data));
        } catch (e) {}
    }

    render() {
        const levelNum = this.currentLevelData?.level || 1;
        const targetWord = this.currentLevelData?.targetWord || 'BA';
        const emoji = this.currentLevelData?.emoji || '';

        this.container.innerHTML = `
            <div class="miner-stage-container" id="balloonStage" style="background: radial-gradient(circle at 50% 20%, #7dd3fc 0%, #38bdf8 35%, #2563eb 75%, #1e3a8a 100%);">
                <!-- Nền bầu trời tầng mây hoạt họa rực rỡ -->
                <div style="position: absolute; inset: 0; pointer-events: none; overflow: hidden; z-index: 0;">
                    <div style="position: absolute; inset: 0; background: linear-gradient(180deg, rgba(255,255,255,0.22) 0%, rgba(0,0,0,0.3) 100%);"></div>
                    <!-- Đám mây trang trí nền trôi chậm -->
                    <div style="position: absolute; top: 110px; left: 8%; background: rgba(255,255,255,0.45); filter: blur(2px); width: 160px; height: 44px; border-radius: 999px;"></div>
                    <div style="position: absolute; top: 150px; right: 10%; background: rgba(255,255,255,0.4); filter: blur(2px); width: 200px; height: 52px; border-radius: 999px;"></div>
                    <div style="position: absolute; top: 220px; left: 30%; background: rgba(255,255,255,0.3); filter: blur(3px); width: 140px; height: 38px; border-radius: 999px;"></div>
                </div>

                <!-- 1. THANH ĐIỀU HƯỚNG GỌN GÀNG -->
                <header class="miner-nav-bar" id="balloonTopNav">
                    <div class="miner-nav-left">
                        <a href="/kids/games" class="miner-pill-btn btn-lobby-amber" id="btnBackToLobby" title="Trở về sảnh trò chơi">
                            <span class="material-symbols-outlined" style="font-size: 1.25rem;">arrow_back</span>
                            <span>VỀ SẢNH</span>
                        </a>

                        <button type="button" class="miner-pill-btn btn-alphabet-teal" id="btnOpenAlphabet" title="Mở danh sách từ ghép">
                            <span class="material-symbols-outlined" style="font-size: 1.25rem;">grid_view</span>
                            <span>CHỌN TỪ</span>
                        </button>

                        <div class="miner-level-badge">
                            Màn ${levelNum}
                        </div>
                    </div>

                    <!-- Giữa: TRẠM MỤC TIÊU GHÉP TỪ -->
                    <div class="miner-target-station" id="balloonTargetStation">
                        <button type="button" class="miner-target-voice-btn" id="btnMissionVoice" title="Nhấn để nghe lại phát âm từ vựng">
                            <span class="material-symbols-outlined" style="font-size: 1.35rem;">volume_up</span>
                        </button>

                        <div class="miner-target-body">
                            <span class="miner-target-sublabel">MỤC TIÊU: GHÉP TỪ</span>
                            <div class="miner-target-big-badge" id="hudTargetChar" title="Bé chạm vào đây để nghe phát âm nhé!" style="width: auto; padding: 0 18px; font-size: 2.2rem; gap: 8px;">
                                <span>${targetWord}</span>
                                ${emoji ? `<span style="font-size: 1.8rem;">${emoji}</span>` : ''}
                            </div>
                        </div>

                        <div style="display: flex; flex-direction: column; align-items: center; border-left: 2px solid rgba(254,240,138,0.4); padding-left: 10px; margin-left: 4px;">
                            <span style="font-size: 9px; font-weight: 900; color: #fde047; text-transform: uppercase;">Tiến độ</span>
                            <span id="balloonProgressText" style="font-family: 'Fredoka', sans-serif; font-size: 1.35rem; font-weight: 900; color: #ffffff;">0/${this.targetLetters.length}</span>
                        </div>
                    </div>

                    <div class="miner-nav-right">
                        <button type="button" class="miner-pill-btn" id="btnAudioToggle" style="background: rgba(30, 41, 59, 0.85); border: 2px solid rgba(245, 158, 11, 0.5); color: #fde047; padding: 7px 14px;" title="Bật/Tắt âm thanh">
                            <span class="material-symbols-outlined" style="font-size: 1.3rem;">volume_up</span>
                        </button>
                    </div>
                </header>

                <!-- 2. KHU VỰC BẦU TRỜI & KHINH KHÍ CẦU TRUNG TÂM -->
                <div class="balloon-sky-playfield" id="balloonSkyPlayfield" style="position: absolute; inset: 90px 0 0 0; overflow: hidden; pointer-events: auto;">
                    
                    <!-- LỚP MÂY CHỮ CÁI BAY TỰ DO -->
                    <div id="skyCloudsLayer" style="position: absolute; inset: 0; pointer-events: auto;"></div>

                    <!-- KHINH KHÍ CẦU 3D TRUNG TÂM -->
                    <div class="hot-air-balloon-core" id="mainHotAirBalloon" style="position: absolute; bottom: 30px; left: 50%; transform: translateX(-50%); display: flex; flex-direction: column; align-items: center; z-index: 25; pointer-events: none; transition: transform 1.2s cubic-bezier(0.25, 0.1, 0.25, 1), bottom 0.5s ease;">
                        
                        <!-- Quả cầu khổng lồ rực rỡ 3D -->
                        <div class="balloon-envelope" style="width: 175px; height: 185px; border-radius: 50% 50% 45% 45% / 60% 60% 40% 40%; background: radial-gradient(circle at 35% 25%, #fef08a 0%, #f59e0b 35%, #ef4444 75%, #b91c1c 100%); border: 4px solid #ffffff; box-shadow: inset 0 8px 20px rgba(255,255,255,0.7), 0 16px 36px rgba(0,0,0,0.45); display: flex; flex-direction: column; align-items: center; justify-content: center; position: relative;">
                            <div style="position: absolute; inset: 0; border-radius: inherit; background: repeating-linear-gradient(90deg, rgba(255,255,255,0.25) 0px, rgba(255,255,255,0.25) 18px, transparent 18px, transparent 36px); pointer-events: none;"></div>
                            
                            <div style="background: rgba(15, 23, 42, 0.75); backdrop-filter: blur(6px); border: 2.5px solid #fef08a; padding: 4px 16px; border-radius: 999px; display: flex; align-items: center; gap: 8px; box-shadow: 0 6px 14px rgba(0,0,0,0.35); z-index: 2;">
                                <span style="font-family: 'Fredoka', sans-serif; font-size: 1.5rem; font-weight: 900; color: #ffffff;">${targetWord}</span>
                                ${emoji ? `<span style="font-size: 1.4rem;">${emoji}</span>` : ''}
                            </div>
                        </div>

                        <!-- Mỏ đốt lửa (Burner) -->
                        <div class="balloon-burner" style="position: relative; width: 48px; height: 30px; display: flex; align-items: center; justify-content: center; margin-top: -6px; z-index: 3;">
                            <div id="burnerFlame" style="font-size: 2.3rem; transform-origin: center bottom; animation: flickerFlame 0.8s infinite alternate; filter: drop-shadow(0 0 12px #f59e0b); transition: all 0.3s ease;">
                                🔥
                            </div>
                        </div>

                        <!-- Giỏ khinh khí cầu chứa chữ -->
                        <div class="balloon-basket" style="min-width: 200px; height: 70px; background: linear-gradient(180deg, #b45309 0%, #78350f 100%); border: 3.5px solid #fde68a; border-radius: 20px; box-shadow: 0 12px 28px rgba(0,0,0,0.5); display: flex; align-items: center; justify-content: center; gap: 10px; padding: 6px 16px; margin-top: -4px; z-index: 4;">
                            <div id="basketLetterSlots" style="display: flex; gap: 10px; align-items: center;"></div>
                        </div>
                    </div>
                </div>

                <!-- 3. MODAL BẢNG CHỌN TỪ GHÉP -->
                <div class="game-modal-overlay" id="alphabetModal" style="display: none;">
                    <div class="game-modal-card alphabet-modal-content">
                        <h2 class="modal-title" style="margin-bottom: 6px;">DANH SÁCH TỪ VỰNG</h2>
                        <p style="color: #cbd5e1; font-size: 0.95rem; margin-top: 0; margin-bottom: 12px; font-weight: 600;">
                            Bé chạm vào từ muốn cùng khinh khí cầu khám phá nhé!
                        </p>
                        <div class="alphabet-grid-wrap" id="alphabetGridContainer"></div>
                        <div style="margin-top: 14px;">
                            <button type="button" class="modal-btn modal-btn-secondary" id="btnCloseAlphabetModal" style="width: 100%;">
                                Đóng Lại
                            </button>
                        </div>
                    </div>
                </div>

                <!-- 4. POPUP HOÀN THÀNH MÀN TRONG MỜ TINH TẾ TỰ ĐỘNG QUA MÀN -->
                <div class="game-modal-overlay" id="victoryModal" style="display: none; align-items: center; justify-content: center; pointer-events: none;">
                    <div class="victory-toast-card" id="victoryToast">
                        <div style="font-size: 1.6rem; margin-bottom: 2px;">✨</div>
                        <div style="font-family: 'Fredoka', cursive, sans-serif; font-size: 1.25rem; font-weight: 800; color: #fef08a; margin-bottom: 2px;">
                            HOÀN THÀNH MÀN CHƠI!
                        </div>
                        <div style="color: #cbd5e1; font-size: 0.85rem; font-weight: 600;" id="victoryMessage">
                            Đang chuyển tiếp...
                        </div>
                    </div>
                </div>
            </div>
        `;

        this.bindEvents();
        this.renderBasketSlots();
    }

    bindEvents() {
        const btnMissionVoice = document.getElementById('btnMissionVoice');
        const hudTargetChar = document.getElementById('hudTargetChar');
        const btnAudioToggle = document.getElementById('btnAudioToggle');
        const btnOpenAlphabet = document.getElementById('btnOpenAlphabet');
        const btnCloseAlphabetModal = document.getElementById('btnCloseAlphabetModal');
        const alphabetModal = document.getElementById('alphabetModal');

        const playTargetVoice = () => {
            const word = this.currentLevelData?.targetWord || 'BA';
            window.gameAudio?.speakWordEnglish(word);
        };

        if (btnMissionVoice) btnMissionVoice.addEventListener('click', playTargetVoice);
        if (hudTargetChar) hudTargetChar.addEventListener('click', playTargetVoice);

        if (btnAudioToggle) {
            btnAudioToggle.addEventListener('click', () => {
                this.soundEnabled = !this.soundEnabled;
                window.gameAudio?.setSoundEnabled(this.soundEnabled);
                btnAudioToggle.innerHTML = `<span class="material-symbols-outlined" style="font-size: 1.3rem;">${this.soundEnabled ? 'volume_up' : 'volume_off'}</span>`;
            });
        }

        if (btnOpenAlphabet && alphabetModal) {
            btnOpenAlphabet.addEventListener('click', () => {
                this.renderAlphabetGrid();
                alphabetModal.style.display = 'flex';
            });
        }

        if (btnCloseAlphabetModal && alphabetModal) {
            btnCloseAlphabetModal.addEventListener('click', () => {
                alphabetModal.style.display = 'none';
            });
        }
    }

    renderAlphabetGrid() {
        const container = document.getElementById('alphabetGridContainer');
        if (!container) return;

        const levels = this.shell?.levels || window.GAME_LEVELS['balloon-word'] || [];
        const curLevel = this.currentLevelData?.level || 1;

        container.innerHTML = levels.map(lvl => {
            const isCurrent = lvl.level === curLevel;
            return `
                <button type="button" class="alphabet-item-btn ${isCurrent ? 'is-active' : ''}" data-jump-level="${lvl.level}">
                    <span class="alphabet-char">${lvl.targetWord}</span>
                    <span class="alphabet-sub">${lvl.emoji || `Màn ${lvl.level}`}</span>
                </button>
            `;
        }).join('');

        container.querySelectorAll('[data-jump-level]').forEach(btn => {
            btn.addEventListener('click', (e) => {
                const targetLvl = parseInt(e.currentTarget.dataset.jumpLevel, 10);
                document.getElementById('alphabetModal').style.display = 'none';
                this.shell?.startLevel(targetLvl);
            });
        });
    }

    renderBasketSlots() {
        const container = document.getElementById('basketLetterSlots');
        if (!container) return;

        container.innerHTML = this.targetLetters.map((char, idx) => {
            const filled = this.assembledLetters[idx];
            if (filled) {
                return `
                    <div class="basket-slot is-filled" style="width: 50px; height: 52px; border-radius: 14px; background: #ffffff; border: 3px solid #10b981; display: flex; align-items: center; justify-content: center; font-family: 'Fredoka', sans-serif; font-size: 2.2rem; font-weight: 900; color: #047857; box-shadow: 0 4px 12px rgba(0,0,0,0.35); animation: popIn 0.25s ease;">
                        ${filled}
                    </div>
                `;
            } else {
                return `
                    <div class="basket-slot" style="width: 50px; height: 52px; border-radius: 14px; background: rgba(0,0,0,0.35); border: 2.5px dashed rgba(255,255,255,0.7); display: flex; align-items: center; justify-content: center; font-family: 'Fredoka', sans-serif; font-size: 1.5rem; font-weight: 900; color: rgba(255,255,255,0.7);">
                        ?
                    </div>
                `;
            }
        }).join('');
    }

    spawnSkyClouds() {
        const layer = document.getElementById('skyCloudsLayer');
        if (!layer) return;

        layer.innerHTML = '';
        this.floatingClouds = [];

        const width = layer.clientWidth || 800;
        const height = layer.clientHeight || 500;
        const letters = this.currentLevelData?.letters || ['B', 'A'];
        const decoys = this.currentLevelData?.decoys || ['C', 'M'];

        const allChars = [...letters, ...decoys];

        allChars.forEach((char, idx) => {
            this.createCloud(char, width, height, idx, allChars.length);
        });
    }

    createCloud(char, width, height, index, total) {
        const layer = document.getElementById('skyCloudsLayer');
        if (!layer) return;

        const isTarget = this.targetLetters.includes(char);
        const sizeW = 125;
        const sizeH = 80;

        const x = 30 + (index * (width - sizeW - 60) / Math.max(1, total - 1));
        const y = 30 + (index % 2 === 0 ? 20 : 110) + (Math.random() * 25);

        // Đám mây container (chỉ dùng translate3d, TUYỆT ĐỐI KHÔNG DÙNG CSS hover transform để tránh nháy)
        const el = document.createElement('div');
        el.className = 'toy-sky-cloud';
        el.style.cssText = `
            position: absolute;
            width: ${sizeW}px;
            height: ${sizeH}px;
            left: 0;
            top: 0;
            transform: translate3d(${x}px, ${y}px, 0);
            background: linear-gradient(180deg, #ffffff 0%, #f1f5f9 100%);
            border: 3.5px solid #ffffff;
            border-radius: 50px;
            box-shadow: 0 12px 28px rgba(0,0,0,0.25), inset 0 4px 10px rgba(255,255,255,0.95);
            display: flex;
            align-items: center;
            justify-content: center;
            cursor: pointer;
            z-index: 15;
            user-select: none;
            touch-action: manipulation;
            transition: opacity 0.25s ease;
        `;

        const charSpan = document.createElement('span');
        charSpan.style.cssText = `
            font-family: 'Fredoka', sans-serif;
            font-size: 2.8rem;
            font-weight: 900;
            color: #1e3a8a;
            text-shadow: 0 2px 6px rgba(255,255,255,0.9);
            pointer-events: none;
            transition: transform 0.15s ease;
        `;
        charSpan.textContent = char;
        el.appendChild(charSpan);

        layer.appendChild(el);

        const cloudObj = {
            el,
            charSpan,
            char,
            isTarget,
            x,
            y,
            sizeW,
            sizeH,
            vx: (Math.random() - 0.5) * 0.5,
            wobbleSpeed: 0.0018 + Math.random() * 0.0015,
            wobbleOffset: Math.random() * Math.PI * 2,
            collected: false
        };

        el.addEventListener('click', (e) => {
            e.stopPropagation();
            this.handleCloudClick(cloudObj);
        });

        this.floatingClouds.push(cloudObj);
    }

    handleCloudClick(cloud) {
        if (this.isCompleted || cloud.collected) return;

        const nextNeededLetter = this.targetLetters[this.assembledLetters.length];

        if (cloud.char === nextNeededLetter) {
            // Đúng chữ cái!
            cloud.collected = true;
            this.assembledLetters.push(cloud.char);

            window.gameAudio?.playFlameWhoosh();
            window.gameAudio?.speakLetterEnglish(cloud.char);

            const burner = document.getElementById('burnerFlame');
            if (burner) {
                burner.style.transform = 'scale(1.8)';
                setTimeout(() => { if (burner) burner.style.transform = 'scale(1)'; }, 350);
            }

            if (cloud.el) {
                cloud.el.style.opacity = '0';
                setTimeout(() => cloud.el?.remove(), 250);
            }

            this.renderBasketSlots();
            const progText = document.getElementById('balloonProgressText');
            if (progText) progText.textContent = `${this.assembledLetters.length}/${this.targetLetters.length}`;

            if (this.assembledLetters.length === this.targetLetters.length) {
                this.isCompleted = true;
                setTimeout(() => {
                    this.onWordCompleted();
                }, 450);
            }
        } else {
            // Chạm chữ khác -> Đọc tên chữ bằng tiếng Anh, nháy nhẹ font chữ bên trong (KHÔNG nháy cả khung mây)
            window.gameAudio?.playWrongWobble();
            window.gameAudio?.speakLetterEnglish(cloud.char);
            if (cloud.charSpan) {
                cloud.charSpan.style.transform = 'scale(1.2)';
                setTimeout(() => { if (cloud.charSpan) cloud.charSpan.style.transform = 'scale(1)'; }, 200);
            }
        }
    }

    onWordCompleted() {
        const balloon = document.getElementById('mainHotAirBalloon');
        const targetWord = this.currentLevelData?.targetWord || 'BA';

        window.gameAudio?.playFlameWhoosh();
        window.gameAudio?.speakWordEnglish(targetWord);

        const burner = document.getElementById('burnerFlame');
        if (burner) {
            burner.style.fontSize = '3.8rem';
        }

        if (balloon) {
            balloon.style.bottom = '115%';
        }

        setTimeout(() => {
            const victoryModal = document.getElementById('victoryModal');
            const toastEl = document.getElementById('victoryToast');
            if (victoryModal) {
                window.gameAudio?.playVictoryFanfare();
                victoryModal.style.display = 'flex';
            }

            const levelNum = this.currentLevelData?.level || 1;
            this.shell?.saveProgressLocal('balloon-word', levelNum, 1);
            this.shell?.saveProgressServer('balloon-word', levelNum, 1);

            const nextLvl = levelNum + 1;
            this.saveCurrentLevelProgress(nextLvl);
            const totalLevels = (this.shell?.levels || []).length || 40;

            // TỰ ĐỘNG QUA MÀN TIẾP THEO SAU 1.8S NHƯ ĐÀO VÀNG
            setTimeout(() => {
                if (toastEl) {
                    toastEl.style.transition = 'opacity 0.35s ease, transform 0.35s ease';
                    toastEl.style.opacity = '0';
                    toastEl.style.transform = 'scale(0.92)';
                }
                setTimeout(() => {
                    if (victoryModal) victoryModal.style.display = 'none';
                    if (nextLvl <= totalLevels) {
                        this.shell?.startLevel(nextLvl);
                    } else {
                        window.location.href = '/kids/games';
                    }
                }, 380);
            }, 1800);
        }, 1000);
    }

    startSkyFlightLoop() {
        const layer = document.getElementById('skyCloudsLayer');

        const loop = (timestamp) => {
            if (!this.lastTime) this.lastTime = timestamp;
            const dt = timestamp - this.lastTime;
            this.lastTime = timestamp;

            const width = layer?.clientWidth || 800;

            for (let i = 0; i < this.floatingClouds.length; i++) {
                const c = this.floatingClouds[i];
                if (c.collected) continue;

                c.x += c.vx;
                const waveY = c.y + Math.sin(timestamp * c.wobbleSpeed + c.wobbleOffset) * 8;

                if (c.x < 10) { c.x = 10; c.vx *= -1; }
                if (c.x > width - c.sizeW - 10) { c.x = width - c.sizeW - 10; c.vx *= -1; }

                if (c.el) {
                    c.el.style.transform = `translate3d(${c.x.toFixed(1)}px, ${waveY.toFixed(1)}px, 0)`;
                }
            }

            if (!this.isCompleted) {
                this.animFrameId = requestAnimationFrame(loop);
            }
        };

        this.animFrameId = requestAnimationFrame(loop);
    }
}

window.BalloonWordGame = BalloonWordGame;
