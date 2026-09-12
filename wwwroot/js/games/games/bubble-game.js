/**
 * BubbleGame - Trò chơi Bắn Bong Bóng Chữ Cái bằng Khẩu Đại Bác Đồ Chơi (Toy Cannon Blaster)
 * - Khẩu đại bác hoành tráng phong cách thủy thủ 3D đặt ở đáy màn hình (bánh xe gỗ, nòng đại bác bọc đồng, huy hiệu mỏ neo).
 * - Bảng mục tiêu chuyển xuống đặt ngay dưới chân khẩu đại bác theo đúng ý tưởng người dùng.
 * - Nền đại dương sinh động: san hô rực rỡ, rùa biển, đàn cá bơi qua, sứa biển bồng bềnh và bóng gai gây nhiễu.
 * - Tên lửa mini bay thong thả hơn (~450ms) có vệt khói sao, chạm nổ bóng nước cực đã tay.
 * - Tự động qua màn sau 1.8s như Đào Vàng, bé không cần phải bấm nút.
 */
class BubbleGame {
    constructor(container, shell) {
        this.container = container;
        this.shell = shell;
        this.currentLevelData = null;

        this.targetPopped = 0;
        this.totalRequired = 2;
        this.isCompleted = false;
        this.soundEnabled = true;

        this.bubbles = [];
        this.distractorCreatures = [];
        this.animFrameId = null;
        this.lastTime = 0;

        this.cannonX = 0;
        this.cannonY = 0;
    }

    init(levelData) {
        this.destroy();
        this.currentLevelData = levelData;
        this.targetPopped = 0;
        this.totalRequired = levelData?.targetCount || 2;
        this.isCompleted = false;
        this.bubbles = [];
        this.distractorCreatures = [];

        this.saveCurrentLevelProgress(levelData?.level || 1);
        this.render();
        this.initCannon();
        this.spawnInitialBubbles();
        this.spawnDistractorCreatures();
        this.startPhysicsLoop();

        // Đọc to nhiệm vụ bằng tiếng Anh ngắn gọn
        setTimeout(() => {
            if (this.currentLevelData?.instructionEn) {
                window.gameAudio?.speakEnglish(this.currentLevelData.instructionEn);
            }
        }, 300);
    }

    destroy() {
        if (this.animFrameId) {
            cancelAnimationFrame(this.animFrameId);
            this.animFrameId = null;
        }
        this.bubbles = [];
        this.distractorCreatures = [];
        this.isCompleted = true;
    }

    saveCurrentLevelProgress(level) {
        try {
            const raw = localStorage.getItem('htl1_games_progress') || '{}';
            const data = JSON.parse(raw);
            if (!data['bubble']) data['bubble'] = {};
            data['bubble'].currentLevel = level;
            localStorage.setItem('htl1_games_progress', JSON.stringify(data));
        } catch (e) {}
    }

    render() {
        const target = this.currentLevelData?.target || 'b';
        const levelNum = this.currentLevelData?.level || 1;

        this.container.innerHTML = `
            <div class="miner-stage-container" id="bubbleStage" style="background: radial-gradient(circle at 50% 20%, #0369a1 0%, #075985 40%, #0c4a6e 75%, #082f49 100%);">
                
                <!-- 1. NỀN ĐẠI DƯƠNG SINH ĐỘNG: TIA NẮNG, ĐÀN CÁ, RÙA BIỂN, SAN HÔ -->
                <div style="position: absolute; inset: 0; pointer-events: none; overflow: hidden; z-index: 0;">
                    <!-- Tia nắng khúc xạ từ mặt biển -->
                    <div style="position: absolute; top: -60px; left: 18%; width: 200px; height: 450px; background: linear-gradient(180deg, rgba(255,255,255,0.22) 0%, transparent 100%); transform: rotate(-14deg); filter: blur(10px); animation: causticsLight 5s infinite alternate;"></div>
                    <div style="position: absolute; top: -60px; right: 22%; width: 220px; height: 420px; background: linear-gradient(180deg, rgba(255,255,255,0.18) 0%, transparent 100%); transform: rotate(16deg); filter: blur(10px); animation: causticsLight 6s infinite alternate 1s;"></div>
                    
                    <!-- Đàn cá bơi lội trong nền xa -->
                    <div style="position: absolute; top: 180px; font-size: 1.5rem; opacity: 0.35; animation: swimAcross 22s linear infinite; filter: drop-shadow(0 2px 4px rgba(0,0,0,0.3));">
                        🐟 🐠 🐡
                    </div>
                    <!-- Chú rùa biển bơi lững lờ -->
                    <div style="position: absolute; top: 320px; font-size: 2.2rem; opacity: 0.38; animation: swimTurtle 32s linear infinite;">
                        🐢
                    </div>

                    <!-- Rặng san hô & hải quỳ màu sắc rực rỡ dưới đáy biển (Trái) -->
                    <div class="sea-floor-coral-left">
                        <div class="coral-stalk" style="width: 22px; height: 75px; background: linear-gradient(180deg, #f43f5e 0%, #be123c 100%);"></div>
                        <div class="coral-stalk" style="width: 28px; height: 110px; background: linear-gradient(180deg, #f59e0b 0%, #d97706 100%); margin-bottom: -5px;"></div>
                        <div class="coral-stalk" style="width: 20px; height: 90px; background: linear-gradient(180deg, #ec4899 0%, #a21caf 100%);"></div>
                        <div class="coral-stalk" style="width: 24px; height: 60px; background: linear-gradient(180deg, #10b981 0%, #047857 100%);"></div>
                        <span style="font-size: 1.8rem; margin-left: -12px; margin-bottom: 2px;">⭐</span>
                    </div>

                    <!-- Rặng san hô & rong biển (Phải) -->
                    <div class="sea-floor-coral-right">
                        <span style="font-size: 1.8rem; margin-right: -10px; margin-bottom: 2px;">🐚</span>
                        <div class="coral-stalk" style="width: 22px; height: 85px; background: linear-gradient(180deg, #10b981 0%, #047857 100%);"></div>
                        <div class="coral-stalk" style="width: 26px; height: 120px; background: linear-gradient(180deg, #a855f7 0%, #7e22ce 100%); margin-bottom: -6px;"></div>
                        <div class="coral-stalk" style="width: 24px; height: 95px; background: linear-gradient(180deg, #f97316 0%, #c2410c 100%);"></div>
                        <div class="coral-stalk" style="width: 20px; height: 65px; background: linear-gradient(180deg, #06b6d4 0%, #0e7490 100%);"></div>
                    </div>
                </div>

                <!-- 2. THANH ĐIỀU HƯỚNG GỌN GÀNG PHÍA TRÊN -->
                <header class="miner-nav-bar" id="bubbleTopNav">
                    <div class="miner-nav-left">
                        <a href="/kids/games" class="miner-pill-btn btn-lobby-amber" id="btnBackToLobby" title="Trở về sảnh trò chơi">
                            <span class="material-symbols-outlined" style="font-size: 1.25rem;">arrow_back</span>
                            <span>VỀ SẢNH</span>
                        </a>

                        <button type="button" class="miner-pill-btn btn-alphabet-teal" id="btnOpenAlphabet" title="Mở danh sách màn chơi">
                            <span class="material-symbols-outlined" style="font-size: 1.25rem;">grid_view</span>
                            <span>CHỌN MÀN</span>
                        </button>

                        <div class="miner-level-badge">
                            Màn ${levelNum}
                        </div>
                    </div>

                    <!-- Giữa: Nhãn trò chơi -->
                    <div style="background: rgba(15,23,42,0.6); backdrop-filter: blur(8px); border: 2px solid rgba(254,240,138,0.4); padding: 6px 18px; border-radius: 999px; display: flex; align-items: center; gap: 8px;">
                        <span style="font-size: 1.2rem;">🎈</span>
                        <span style="font-family: 'Fredoka', sans-serif; font-size: 1.05rem; font-weight: 900; color: #fef08a;">
                            BẮN BONG BÓNG ĐẠI DƯƠNG
                        </span>
                    </div>

                    <div class="miner-nav-right">
                        <button type="button" class="miner-pill-btn" id="btnAudioToggle" style="background: rgba(30, 41, 59, 0.85); border: 2px solid rgba(245, 158, 11, 0.5); color: #fde047; padding: 7px 14px;" title="Bật/Tắt âm thanh">
                            <span class="material-symbols-outlined" style="font-size: 1.3rem;">volume_up</span>
                        </button>
                    </div>
                </header>

                <!-- 3. KHU VỰC BẮN BONG BÓNG & CÁC VẬT GÂY NHIỄU -->
                <div class="bubble-stage-playfield" id="bubblePlayfield" style="position: absolute; inset: 75px 0 170px 0; overflow: hidden; pointer-events: auto;"></div>

                <!-- 4. KHẨU ĐẠI BÁC 3D HOÀNH TRÁNG VÀ TRẠM MỤC TIÊU ĐẶT NGAY DƯỚI ĐÁY -->
                <div class="cannon-master-station" id="cannonMasterStation" style="position: absolute; bottom: 8px; left: 50%; transform: translateX(-50%); z-index: 30; display: flex; flex-direction: column; align-items: center; pointer-events: none;">
                    
                    <!-- A. KHẨU ĐẠI BÁC THỦY THỦ 3D (CANNON) -->
                    <div style="position: relative; width: 170px; height: 165px; display: flex; align-items: flex-end; justify-content: center;">
                        
                        <!-- Nòng đại bác xoay 3D (Turret Barrel) -->
                        <div id="cannonTurret" style="position: absolute; bottom: 32px; left: 50%; width: 72px; height: 120px; margin-left: -36px; transform-origin: 36px 95px; transition: transform 0.12s cubic-bezier(0.2, 0.9, 0.3, 1.2); z-index: 10;">
                            <!-- Đỉnh nòng đại bác có thấu kính ngắm / đạn tên lửa nhô lên -->
                            <div style="position: absolute; top: -14px; left: 24px; width: 24px; height: 24px; border-radius: 50%; background: radial-gradient(circle at 35% 30%, #38bdf8 0%, #0284c7 60%, #0369a1 100%); border: 2.5px solid #ffffff; box-shadow: 0 0 12px #38bdf8; z-index: 5;">
                                <div style="position: absolute; top: 3px; left: 5px; width: 6px; height: 6px; background: #ffffff; border-radius: 50%;"></div>
                            </div>

                            <!-- Miệng nòng pháo bọc vành đồng thau -->
                            <div style="position: absolute; top: 6px; left: 10px; width: 52px; height: 14px; background: linear-gradient(180deg, #fde047 0%, #d97706 100%); border: 2px solid #ffffff; border-radius: 8px; box-shadow: 0 3px 8px rgba(0,0,0,0.4); z-index: 4;"></div>

                            <!-- Thân nòng đại bác hình trụ xanh đen bọc đai đồng & huy hiệu mỏ neo -->
                            <div style="position: absolute; top: 16px; left: 12px; width: 48px; height: 82px; background: linear-gradient(90deg, #0f172a 0%, #1e293b 40%, #0f172a 100%); border: 3px solid #0284c7; border-radius: 10px 10px 18px 18px; box-shadow: inset 0 6px 10px rgba(255,255,255,0.4), 0 8px 24px rgba(0,0,0,0.6); overflow: hidden; display: flex; flex-direction: column; align-items: center; justify-content: center;">
                                <!-- Đai đồng thau ngang thân -->
                                <div style="width: 100%; height: 8px; background: linear-gradient(90deg, #b45309 0%, #fde047 50%, #b45309 100%); border-top: 1.5px solid #fef08a; border-bottom: 1.5px solid #78350f; position: absolute; top: 8px;"></div>
                                
                                <!-- Huy hiệu mỏ neo kim loại mạ vàng tròn ở giữa -->
                                <div style="width: 26px; height: 26px; border-radius: 50%; background: radial-gradient(circle at 35% 30%, #fde047 0%, #b45309 100%); border: 2px solid #fef08a; display: flex; align-items: center; justify-content: center; box-shadow: 0 2px 6px rgba(0,0,0,0.5); z-index: 2;">
                                    <span style="font-size: 1rem; line-height: 1; filter: drop-shadow(0 1px 2px rgba(0,0,0,0.6));">⚓</span>
                                </div>

                                <!-- Đai đồng phía dưới -->
                                <div style="width: 100%; height: 8px; background: linear-gradient(90deg, #b45309 0%, #fde047 50%, #b45309 100%); border-top: 1.5px solid #fef08a; border-bottom: 1.5px solid #78350f; position: absolute; bottom: 8px;"></div>
                            </div>

                            <!-- Đuôi nòng pháo hình bán cầu -->
                            <div style="position: absolute; bottom: 8px; left: 16px; width: 40px; height: 20px; border-radius: 0 0 20px 20px; background: #0f172a; border: 2.5px solid #f59e0b; z-index: 3;"></div>
                        </div>

                        <!-- Khung bệ xe gỗ & hai bánh xe đại bác lớn (Carriage & Wheels) -->
                        <div style="position: absolute; bottom: 0; left: 0; right: 0; height: 50px; display: flex; align-items: center; justify-content: space-between; z-index: 8;">
                            <!-- Bánh xe bên trái bọc đai đồng -->
                            <div class="cannon-wheel" style="width: 52px; height: 52px; border-radius: 50%; background: radial-gradient(circle at 40% 40%, #92400e 0%, #451a03 100%); border: 4px solid #f59e0b; box-shadow: 0 6px 14px rgba(0,0,0,0.5); display: flex; align-items: center; justify-content: center; position: relative;">
                                <!-- Nan hoa bánh xe -->
                                <div style="position: absolute; width: 38px; height: 6px; background: #b45309; border-radius: 3px;"></div>
                                <div style="position: absolute; height: 38px; width: 6px; background: #b45309; border-radius: 3px;"></div>
                                <!-- Trục bánh mạ vàng -->
                                <div style="width: 16px; height: 16px; border-radius: 50%; background: radial-gradient(circle, #fde047 0%, #d97706 100%); border: 2px solid #ffffff; z-index: 2; box-shadow: 0 2px 4px rgba(0,0,0,0.4);"></div>
                            </div>

                            <!-- Trục ngang bằng gỗ liên kết 2 bánh xe -->
                            <div style="flex: 1; height: 16px; background: linear-gradient(180deg, #b45309 0%, #78350f 100%); border-top: 2.5px solid #fde68a; border-bottom: 2.5px solid #451a03; border-radius: 4px; margin: 0 -4px; box-shadow: 0 4px 10px rgba(0,0,0,0.4); z-index: 1;"></div>

                            <!-- Bánh xe bên phải bọc đai đồng -->
                            <div class="cannon-wheel" style="width: 52px; height: 52px; border-radius: 50%; background: radial-gradient(circle at 40% 40%, #92400e 0%, #451a03 100%); border: 4px solid #f59e0b; box-shadow: 0 6px 14px rgba(0,0,0,0.5); display: flex; align-items: center; justify-content: center; position: relative;">
                                <div style="position: absolute; width: 38px; height: 6px; background: #b45309; border-radius: 3px;"></div>
                                <div style="position: absolute; height: 38px; width: 6px; background: #b45309; border-radius: 3px;"></div>
                                <div style="width: 16px; height: 16px; border-radius: 50%; background: radial-gradient(circle, #fde047 0%, #d97706 100%); border: 2px solid #ffffff; z-index: 2; box-shadow: 0 2px 4px rgba(0,0,0,0.4);"></div>
                            </div>
                        </div>
                    </div>

                    <!-- B. BẢNG MỤC TIÊU ĐẶT NGAY DƯỚI CHÂN KHẨU ĐẠI BÁC (THEO ĐÚNG ẢNH MẪU) -->
                    <div class="cannon-console-box" id="cannonConsoleBox" style="margin-top: 2px; pointer-events: auto;">
                        <!-- Nút loa phát âm mục tiêu -->
                        <button type="button" class="btn-toy-yellow" id="btnMissionVoice" title="Nhấn để nghe phát âm chữ cái" style="width: 44px; height: 44px; border-radius: 50%; padding: 0; background: linear-gradient(180deg, #f59e0b 0%, #b45309 100%); border: 2.5px solid #fde68a; display: flex; align-items: center; justify-content: center; color: #ffffff; cursor: pointer; box-shadow: 0 4px 10px rgba(0,0,0,0.35);">
                            <span class="material-symbols-outlined" style="font-size: 1.4rem;">volume_up</span>
                        </button>

                        <!-- Khối thẻ chữ mục tiêu màu vàng rực rỡ nổi bật -->
                        <div class="cannon-target-pill" id="hudTargetChar" title="Chạm để nghe phát âm nhé!">
                            ${target}
                        </div>

                        <!-- Tiến độ bắn 0/2 -->
                        <div style="display: flex; align-items: center; gap: 6px; background: rgba(0,0,0,0.35); padding: 6px 14px; border-radius: 14px; border: 2px solid rgba(254,240,138,0.5);">
                            <span style="font-size: 1.2rem;">🎯</span>
                            <span id="bubbleProgressText" style="font-family: 'Fredoka', sans-serif; font-size: 1.35rem; font-weight: 900; color: #fef08a;">0 / ${this.totalRequired}</span>
                        </div>
                    </div>
                </div>

                <!-- 5. MODAL BẢNG CHỌN MÀN CHƠI -->
                <div class="game-modal-overlay" id="alphabetModal" style="display: none;">
                    <div class="game-modal-card alphabet-modal-content">
                        <h2 class="modal-title" style="margin-bottom: 6px;">DANH SÁCH MÀN CHƠI</h2>
                        <p style="color: #cbd5e1; font-size: 0.95rem; margin-top: 0; margin-bottom: 12px; font-weight: 600;">
                            Bé chạm vào chữ cái muốn tập bắn nhé!
                        </p>
                        <div class="alphabet-grid-wrap" id="alphabetGridContainer"></div>
                        <div style="margin-top: 14px;">
                            <button type="button" class="modal-btn modal-btn-secondary" id="btnCloseAlphabetModal" style="width: 100%;">
                                Đóng Lại
                            </button>
                        </div>
                    </div>
                </div>

                <!-- 6. POPUP HOÀN THÀNH MÀN TỰ ĐỘNG CHUYỂN QUA MÀN TIẾP NHƯ ĐÀO VÀNG -->
                <div class="game-modal-overlay" id="victoryModal" style="display: none; align-items: center; justify-content: center;">
                    <div class="victory-toast-card" id="victoryToast">
                        <div style="font-size: 4rem; margin-bottom: 6px; animation: bounceEmoji 1s infinite alternate;">🎉</div>
                        <h2 style="font-family: 'Fredoka', cursive, sans-serif; font-size: 2.1rem; font-weight: 900; color: #ffffff; margin: 0 0 6px 0; text-shadow: 0 2px 8px rgba(0,0,0,0.4);">
                            XUẤT SẮC!
                        </h2>
                        <p style="color: #fef08a; font-size: 1.2rem; font-weight: 700; margin: 0 0 8px 0;">
                            Bé đã bắn trúng chữ <strong style="color: #ffffff; font-size: 1.6rem;">${target}</strong>!
                        </p>
                        <span style="font-size: 0.88rem; color: #cbd5e1; font-weight: 700;">Đang chuyển sang màn tiếp theo...</span>
                    </div>
                </div>
            </div>
        `;

        this.bindEvents();
    }

    bindEvents() {
        const btnMissionVoice = document.getElementById('btnMissionVoice');
        const hudTargetChar = document.getElementById('hudTargetChar');
        const btnAudioToggle = document.getElementById('btnAudioToggle');
        const btnOpenAlphabet = document.getElementById('btnOpenAlphabet');
        const btnCloseAlphabetModal = document.getElementById('btnCloseAlphabetModal');
        const alphabetModal = document.getElementById('alphabetModal');

        const playTargetVoice = () => {
            const char = this.currentLevelData?.target || 'b';
            window.gameAudio?.speakLetterEnglish(char);
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

    initCannon() {
        const turret = document.getElementById('cannonTurret');
        if (turret) {
            const rect = turret.getBoundingClientRect();
            this.cannonX = rect.left + rect.width / 2;
            this.cannonY = rect.top + 20;
        }
    }

    renderAlphabetGrid() {
        const container = document.getElementById('alphabetGridContainer');
        if (!container) return;

        const levels = this.shell?.levels || window.GAME_LEVELS['bubble'] || [];
        const curLevel = this.currentLevelData?.level || 1;

        container.innerHTML = levels.map(lvl => {
            const isCurrent = lvl.level === curLevel;
            return `
                <button type="button" class="alphabet-item-btn ${isCurrent ? 'is-active' : ''}" data-jump-level="${lvl.level}">
                    <span class="alphabet-char">${lvl.target}</span>
                    <span class="alphabet-sub">Màn ${lvl.level}</span>
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

    /* =========================================================================
       SINH BONG BÓNG CHỮ VÀ VẬT GÂY NHIỄU (SỨA BIỂN, BÓNG GAI)
       ========================================================================= */
    spawnInitialBubbles() {
        const playfield = document.getElementById('bubblePlayfield');
        if (!playfield) return;

        playfield.innerHTML = '';
        this.bubbles = [];

        const width = playfield.clientWidth || 800;
        const height = playfield.clientHeight || 500;
        const choices = this.currentLevelData?.choices || ['b', 'd'];
        const target = this.currentLevelData?.target || 'b';

        // Tạo 6 quả bong bóng chữ lơ lửng
        const totalBubbles = 6;
        for (let i = 0; i < totalBubbles; i++) {
            const char = (i < 2) ? target : choices[Math.floor(Math.random() * choices.length)];
            this.createBubble(char, width, height, i);
        }
    }

    spawnDistractorCreatures() {
        const playfield = document.getElementById('bubblePlayfield');
        if (!playfield) return;

        const width = playfield.clientWidth || 800;
        const height = playfield.clientHeight || 500;

        // 1. Chú sứa biển màu hồng tím phát sáng bơi lơ lửng (Jellyfish distractor)
        const jelly = document.createElement('div');
        jelly.className = 'ocean-jellyfish-distractor';
        jelly.innerHTML = `
            <div style="font-size: 3.2rem; filter: drop-shadow(0 0 10px #ec4899); animation: floatJelly 2.5s infinite ease-in-out;">
                🪼
            </div>
        `;
        const jx = 60 + Math.random() * (width - 150);
        const jy = 50 + Math.random() * (height * 0.4);
        jelly.style.cssText = `
            position: absolute;
            left: 0;
            top: 0;
            transform: translate3d(${jx}px, ${jy}px, 0);
            cursor: pointer;
            z-index: 12;
            user-select: none;
            touch-action: manipulation;
        `;
        playfield.appendChild(jelly);

        const jellyObj = {
            el: jelly,
            isJelly: true,
            x: jx,
            y: jy,
            size: 60,
            vx: 0.35,
            vy: 0.15,
            wobbleSpeed: 0.002,
            wobbleOffset: 0
        };

        jelly.addEventListener('click', (e) => {
            e.stopPropagation();
            this.handleCreatureClick(jellyObj, 'Jellyfish', '🪼');
        });

        this.distractorCreatures.push(jellyObj);

        // 2. Quả bóng gai / Sao biển bọc bóng đỏ tím (Spiked sea urchin bubble)
        const urchin = document.createElement('div');
        urchin.className = 'ocean-urchin-distractor';
        urchin.innerHTML = `
            <div style="width: 68px; height: 68px; border-radius: 50%; background: radial-gradient(circle at 35% 30%, #f43f5e 0%, #9f1239 70%, #4c0519 100%); border: 2.5px solid #fecdd3; box-shadow: 0 0 14px rgba(244,63,94,0.6), inset 0 3px 6px rgba(255,255,255,0.7); display: flex; align-items: center; justify-content: center; font-size: 2rem; color: #ffffff;">
                🐡
            </div>
        `;
        const ux = width * 0.6 + Math.random() * (width * 0.25);
        const uy = 80 + Math.random() * (height * 0.35);
        urchin.style.cssText = `
            position: absolute;
            left: 0;
            top: 0;
            transform: translate3d(${ux}px, ${uy}px, 0);
            cursor: pointer;
            z-index: 12;
            user-select: none;
            touch-action: manipulation;
        `;
        playfield.appendChild(urchin);

        const urchinObj = {
            el: urchin,
            isJelly: false,
            x: ux,
            y: uy,
            size: 68,
            vx: -0.3,
            vy: -0.2,
            wobbleSpeed: 0.0018,
            wobbleOffset: 1.5
        };

        urchin.addEventListener('click', (e) => {
            e.stopPropagation();
            this.handleCreatureClick(urchinObj, 'Pufferfish', '🐡');
        });

        this.distractorCreatures.push(urchinObj);
    }

    handleCreatureClick(creature, name, emoji) {
        if (this.isCompleted) return;

        const playfield = document.getElementById('bubblePlayfield');
        if (!playfield) return;

        const fieldRect = playfield.getBoundingClientRect();
        const targetX = fieldRect.left + creature.x + creature.size / 2;
        const targetY = fieldRect.top + creature.y + creature.size / 2;

        this.fireRocket(targetX, targetY, null, () => {
            // Khi tên lửa chạm vật gây nhiễu: Sinh vật nảy lên với âm thanh boing vui nhộn
            window.gameAudio?.playWrongWobble();
            window.gameAudio?.speakEnglish(name);

            if (creature.el) {
                creature.el.animate([
                    { transform: `${creature.el.style.transform} scale(1.3)` },
                    { transform: `${creature.el.style.transform} scale(0.85)` },
                    { transform: `${creature.el.style.transform} scale(1)` }
                ], { duration: 300 });
            }
        });
    }

    createBubble(char, width, height, index = 0) {
        const playfield = document.getElementById('bubblePlayfield');
        if (!playfield) return;

        const isTarget = char === this.currentLevelData?.target;
        const size = 94;
        const x = 30 + Math.random() * Math.max(100, width - size - 60);
        const y = 20 + (index * (height / 6)) + (Math.random() * 25);

        const el = document.createElement('div');
        el.className = 'toy-soap-bubble';
        el.style.cssText = `
            position: absolute;
            width: ${size}px;
            height: ${size}px;
            left: 0;
            top: 0;
            transform: translate3d(${x}px, ${y}px, 0);
            border-radius: 50%;
            background: radial-gradient(circle at 35% 25%, rgba(255, 255, 255, 0.95) 0%, rgba(186, 230, 253, 0.6) 35%, rgba(56, 189, 248, 0.45) 65%, rgba(2, 132, 199, 0.88) 100%);
            border: 3px solid rgba(255, 255, 255, 0.92);
            box-shadow: inset 0 -8px 18px rgba(2, 132, 199, 0.65), inset 0 4px 10px rgba(255, 255, 255, 0.95), 0 10px 26px rgba(0, 0, 0, 0.35);
            cursor: pointer;
            display: flex;
            align-items: center;
            justify-content: center;
            z-index: 10;
            user-select: none;
            touch-action: manipulation;
            transition: opacity 0.2s ease;
        `;

        const highlight = document.createElement('div');
        highlight.style.cssText = `
            position: absolute;
            top: 10px;
            left: 18px;
            width: 28px;
            height: 16px;
            background: rgba(255, 255, 255, 0.9);
            border-radius: 50%;
            transform: rotate(-35deg);
            pointer-events: none;
        `;
        el.appendChild(highlight);

        const charSpan = document.createElement('span');
        charSpan.style.cssText = `
            font-family: 'Fredoka', sans-serif;
            font-size: 2.8rem;
            font-weight: 900;
            color: #ffffff;
            text-shadow: 0 3px 8px rgba(12, 74, 110, 0.95), 0 0 14px rgba(255, 255, 255, 0.85);
            pointer-events: none;
        `;
        charSpan.textContent = char;
        el.appendChild(charSpan);

        playfield.appendChild(el);

        const bubbleObj = {
            el,
            charSpan,
            char,
            isTarget,
            x,
            y,
            size,
            vx: (Math.random() - 0.5) * 0.7,
            vy: -0.45 - Math.random() * 0.4,
            wobbleSpeed: 0.002 + Math.random() * 0.002,
            wobbleOffset: Math.random() * Math.PI * 2,
            popped: false
        };

        el.addEventListener('click', (e) => {
            e.stopPropagation();
            this.handleBubbleClick(bubbleObj);
        });

        this.bubbles.push(bubbleObj);
    }

    handleBubbleClick(bubble) {
        if (this.isCompleted || bubble.popped) return;

        const playfield = document.getElementById('bubblePlayfield');
        if (!playfield) return;

        const fieldRect = playfield.getBoundingClientRect();
        const bubbleCenterX = fieldRect.left + bubble.x + bubble.size / 2;
        const bubbleCenterY = fieldRect.top + bubble.y + bubble.size / 2;

        this.fireRocket(bubbleCenterX, bubbleCenterY, bubble);
    }

    fireRocket(targetScreenX, targetScreenY, targetBubble, onHitCallback = null) {
        const turret = document.getElementById('cannonTurret');
        const station = document.getElementById('cannonMasterStation');
        if (!turret || !station) return;

        const turretRect = turret.getBoundingClientRect();
        const startX = turretRect.left + turretRect.width / 2;
        const startY = turretRect.top + 10;

        // Tính góc xoay nòng đại bác
        const dx = targetScreenX - startX;
        const dy = targetScreenY - startY;
        const angleRad = Math.atan2(dx, -dy);
        const angleDeg = (angleRad * 180) / Math.PI;

        // 1. Xoay nòng đại bác hướng thẳng tới mục tiêu
        turret.style.transform = `rotate(${angleDeg}deg) scale(0.94)`;
        setTimeout(() => {
            if (turret) turret.style.transform = `rotate(${angleDeg}deg) scale(1)`;
        }, 120);

        // 2. Âm thanh phóng tên lửa
        window.gameAudio?.playRocketLaunch();

        // 3. Quả tên lửa đồ chơi mini (bay thong thả hơn ~450ms cho bé nhìn rõ)
        const rocket = document.createElement('div');
        rocket.innerHTML = '🚀';
        rocket.style.cssText = `
            position: fixed;
            left: ${startX - 20}px;
            top: ${startY - 20}px;
            font-size: 2.5rem;
            z-index: 100;
            pointer-events: none;
            transform: rotate(${angleDeg - 45}deg);
            filter: drop-shadow(0 0 12px #f59e0b);
            transition: all 0.45s cubic-bezier(0.22, 0.61, 0.36, 1);
        `;
        document.body.appendChild(rocket);

        // Vệt khói sao phía sau tên lửa
        const smokeInterval = setInterval(() => {
            const rRect = rocket.getBoundingClientRect();
            const smoke = document.createElement('div');
            smoke.style.cssText = `
                position: fixed;
                left: ${rRect.left + 15}px;
                top: ${rRect.top + 15}px;
                width: 10px;
                height: 10px;
                border-radius: 50%;
                background: #fde047;
                box-shadow: 0 0 8px #f59e0b;
                z-index: 99;
                pointer-events: none;
                transition: transform 0.3s ease, opacity 0.3s ease;
            `;
            document.body.appendChild(smoke);
            requestAnimationFrame(() => {
                smoke.style.transform = 'scale(0)';
                smoke.style.opacity = '0';
            });
            setTimeout(() => smoke.remove(), 300);
        }, 70);

        requestAnimationFrame(() => {
            rocket.style.left = `${targetScreenX - 20}px`;
            rocket.style.top = `${targetScreenY - 20}px`;
        });

        // 4. Va chạm nổ bóng sau 450ms
        setTimeout(() => {
            clearInterval(smokeInterval);
            rocket.remove();
            if (targetBubble) {
                this.popBubble(targetBubble, targetScreenX, targetScreenY);
            }
            if (onHitCallback) {
                onHitCallback();
            }
        }, 450);
    }

    popBubble(bubble, screenX, screenY) {
        if (bubble.popped) return;
        bubble.popped = true;

        window.gameAudio?.playRocketExplode();
        this.spawnBurstParticles(screenX, screenY);

        if (bubble.el) {
            bubble.el.style.opacity = '0';
            setTimeout(() => bubble.el?.remove(), 80);
        }

        if (bubble.isTarget) {
            this.targetPopped++;
            const progText = document.getElementById('bubbleProgressText');
            if (progText) progText.textContent = `${this.targetPopped} / ${this.totalRequired}`;

            window.gameAudio?.speakLetterEnglish(bubble.char);

            if (this.targetPopped >= this.totalRequired) {
                this.isCompleted = true;
                setTimeout(() => {
                    this.onLevelVictory();
                }, 350);
                return;
            }
        } else {
            // Chạm chữ khác -> Đọc tên chữ bằng tiếng Anh để bé học nhận biết
            window.gameAudio?.speakLetterEnglish(bubble.char);
        }

        // Tạo bóng mới từ dưới lên
        const playfield = document.getElementById('bubblePlayfield');
        if (playfield && !this.isCompleted) {
            const choices = this.currentLevelData?.choices || ['b', 'd'];
            const target = this.currentLevelData?.target || 'b';
            const char = Math.random() < 0.45 ? target : choices[Math.floor(Math.random() * choices.length)];
            this.createBubble(char, playfield.clientWidth || 800, playfield.clientHeight || 500);
        }
    }

    spawnBurstParticles(x, y) {
        const container = document.createElement('div');
        container.style.cssText = `
            position: fixed;
            left: ${x}px;
            top: ${y}px;
            width: 0;
            height: 0;
            pointer-events: none;
            z-index: 120;
        `;
        document.body.appendChild(container);

        const colors = ['#38bdf8', '#fde047', '#ffffff', '#fb7185'];
        for (let i = 0; i < 8; i++) {
            const p = document.createElement('div');
            const angle = (i / 8) * Math.PI * 2;
            const dist = 36 + Math.random() * 40;
            const px = Math.cos(angle) * dist;
            const py = Math.sin(angle) * dist;
            const color = colors[i % colors.length];

            p.style.cssText = `
                position: absolute;
                width: 12px;
                height: 12px;
                border-radius: 50%;
                background: ${color};
                box-shadow: 0 0 8px ${color};
                transform: translate3d(0, 0, 0);
                transition: transform 0.3s cubic-bezier(0.1, 0.9, 0.2, 1), opacity 0.3s ease;
            `;
            container.appendChild(p);

            requestAnimationFrame(() => {
                p.style.transform = `translate3d(${px}px, ${py}px, 0) scale(0)`;
                p.style.opacity = '0';
            });
        }

        setTimeout(() => container.remove(), 350);
    }

    onLevelVictory() {
        const victoryModal = document.getElementById('victoryModal');
        const toastEl = document.getElementById('victoryToast');
        if (victoryModal) {
            window.gameAudio?.playVictoryFanfare();
            victoryModal.style.display = 'flex';
        }

        const nextLvl = (this.currentLevelData?.level || 1) + 1;
        const totalLevels = (this.shell?.levels || []).length || 12;

        // TỰ ĐỘNG CHUYỂN QUA MÀN TIẾP THEO SAU 1.8 GIÂY NHƯ ĐÀO VÀNG
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
    }

    startPhysicsLoop() {
        const playfield = document.getElementById('bubblePlayfield');

        const loop = (timestamp) => {
            if (!this.lastTime) this.lastTime = timestamp;
            const dt = timestamp - this.lastTime;
            this.lastTime = timestamp;

            const width = playfield?.clientWidth || 800;
            const height = playfield?.clientHeight || 500;

            // Di chuyển các quả bóng chữ
            for (let i = this.bubbles.length - 1; i >= 0; i--) {
                const b = this.bubbles[i];
                if (b.popped) {
                    this.bubbles.splice(i, 1);
                    continue;
                }

                b.y += b.vy;
                b.x += Math.sin(timestamp * b.wobbleSpeed + b.wobbleOffset) * 0.6 + b.vx;

                if (b.x < 10) { b.x = 10; b.vx *= -1; }
                if (b.x > width - b.size - 10) { b.x = width - b.size - 10; b.vx *= -1; }

                if (b.y < -b.size) {
                    b.y = height + 10;
                    b.x = 20 + Math.random() * (width - b.size - 40);
                }

                if (b.el) {
                    b.el.style.transform = `translate3d(${b.x.toFixed(1)}px, ${b.y.toFixed(1)}px, 0)`;
                }
            }

            // Di chuyển các sinh vật gây nhiễu (sứa biển, bóng gai)
            for (let i = 0; i < this.distractorCreatures.length; i++) {
                const c = this.distractorCreatures[i];
                c.x += c.vx;
                const waveY = c.y + Math.sin(timestamp * c.wobbleSpeed + c.wobbleOffset) * 12;

                if (c.x < 15) { c.x = 15; c.vx *= -1; }
                if (c.x > width - c.size - 20) { c.x = width - c.size - 20; c.vx *= -1; }

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

window.BubbleGame = BubbleGame;
