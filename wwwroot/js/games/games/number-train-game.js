/**
 * NumberTrainGame - Đoàn Tàu Hơi Nước Số Học (3D Steam Train Engine)
 * - Tái hiện 100% chính xác giao diện mẫu 3D: Đầu tàu dẫn đầu bên phải, toa tàu xếp sau, ray thép tà vẹt gỗ.
 * - Thanh tiêu đề chuẩn: VỀ SẢNH, CHỌN MÀN, Màn X, Mục tiêu toán học viên ngọc vàng, nút loa & âm thanh.
 * - Bến sân ga cam ấm áp với 3 toa tàu 3D tím hồng nổi bật, chữ số to rõ ràng.
 * - Tự động chuyển qua màn mới sau 1.8s như Đào Vàng, không cần bấm nút.
 */
class NumberTrainGame {
    constructor(container, shell) {
        this.container = container;
        this.shell = shell;
        this.currentLevelData = null;

        this.isCompleted = false;
        this.soundEnabled = true;

        this.sortedWagons = [];
        this.currentCargoCount = 0;
    }

    init(levelData) {
        this.currentLevelData = levelData;
        this.isCompleted = false;
        this.sortedWagons = [];
        this.currentCargoCount = 0;

        this.saveCurrentLevelProgress(levelData?.level || 1);
        this.render();

        setTimeout(() => {
            window.gameAudio?.playTrainWhistle();
        }, 200);

        setTimeout(() => {
            if (this.currentLevelData?.instructionEn) {
                window.gameAudio?.speakEnglish(this.currentLevelData.instructionEn);
            }
        }, 650);
    }

    destroy() {
        this.isCompleted = true;
    }

    saveCurrentLevelProgress(level) {
        try {
            const raw = localStorage.getItem('htl1_games_progress') || '{}';
            const data = JSON.parse(raw);
            if (!data['number-train']) data['number-train'] = {};
            data['number-train'].currentLevel = level;
            localStorage.setItem('htl1_games_progress', JSON.stringify(data));
        } catch (e) {}
    }

    render() {
        const levelNum = this.currentLevelData?.level || 1;
        const type = this.currentLevelData?.type || 'missing-wagon';

        let targetBadgeContent = 'Toa ?';
        if (type === 'cargo-count') {
            targetBadgeContent = `${this.currentLevelData.cargoEmoji} × ${this.currentLevelData.countRequired}`;
        } else if (type === 'wagon-sort') {
            targetBadgeContent = '1, 2, 3';
        } else {
            targetBadgeContent = this.currentLevelData.answer ? `Toa ${this.currentLevelData.answer}` : 'Toa ?';
        }

        const instructionText = this.currentLevelData?.instruction || 'Bé hãy tìm toa số thích hợp nối vào đoàn tàu!';

        this.container.innerHTML = `
            <div class="train-stage-container" id="trainStage">
                
                <!-- 1. TOPBAR: THANH ĐIỀU HƯỚNG VÀ MỤC TIÊU TOÁN HỌC CHUẨN MẪU -->
                <header class="train-topbar">
                    <!-- Nút bên trái -->
                    <div style="display: flex; align-items: center; gap: 8px;">
                        <a href="/kids/games" class="btn-3d-orange" style="padding: 8px 18px; border-radius: 999px; display: inline-flex; align-items: center; gap: 8px; color: #ffffff; font-weight: 800; font-size: 0.92rem; text-decoration: none; border: 2px solid #ffbe0b;" id="btnBackToLobby" title="Về sảnh chính">
                            <span class="material-symbols-outlined" style="font-size: 1.25rem;">arrow_back</span>
                            <span>VỀ SẢNH</span>
                        </a>

                        <button type="button" class="btn-3d-teal" style="padding: 8px 16px; border-radius: 999px; display: inline-flex; align-items: center; gap: 8px; color: #ffffff; font-weight: 800; font-size: 0.88rem; cursor: pointer; border: 2px solid #99f6e4;" id="btnOpenAlphabet" title="Danh sách màn chơi">
                            <span class="material-symbols-outlined" style="font-size: 1.2rem;">grid_view</span>
                            <span>CHỌN MÀN</span>
                        </button>

                        <div style="background: rgba(11,60,53,0.85); backdrop-filter: blur(4px); padding: 6px 14px; border-radius: 999px; border: 2px solid rgba(74,222,128,0.5); color: #86efac; font-weight: 900; font-size: 0.88rem; letter-spacing: 0.05em;">
                            MÀN ${levelNum}
                        </div>
                    </div>

                    <!-- Bảng MỤC TIÊU TOÁN HỌC trung tâm nổi khối 3D ánh kim -->
                    <div class="train-target-hud" id="hudTargetChar" title="Chạm để nghe lại đề bài">
                        <span style="font-size: 0.72rem; font-weight: 900; letter-spacing: 0.08em; color: #ffd166; text-transform: uppercase; display: flex; align-items: center; gap: 6px;">
                            <span style="display: inline-block; width: 8px; height: 8px; border-radius: 50%; background: #4ade80; animation: ping 1.5s infinite;"></span> MỤC TIÊU TOÁN HỌC
                        </span>
                        <div style="display: flex; align-items: center; gap: 8px; margin-top: 2px;">
                            <span style="color: #ffffff; font-size: 0.95rem; font-weight: 800; text-shadow: 0 1px 3px rgba(0,0,0,0.5);">ĐOÀN TÀU SỐ:</span>
                            <div class="train-target-val-box">
                                ${targetBadgeContent}
                            </div>
                        </div>
                    </div>

                    <!-- Nút công cụ âm thanh bên phải -->
                    <div style="display: flex; align-items: center; gap: 8px;">
                        <button type="button" class="btn-3d-orange" style="width: 42px; height: 42px; border-radius: 50%; display: flex; align-items: center; justify-content: center; color: #ffffff; border: 2px solid #ffbe0b; cursor: pointer;" id="btnMissionVoice" title="Nghe lại đề bài">
                            <span class="material-symbols-outlined" style="font-size: 1.4rem;">volume_up</span>
                        </button>
                        <button type="button" class="stitch-icon-btn" style="width: 42px; height: 42px; border-radius: 50%; display: flex; align-items: center; justify-content: center; background: linear-gradient(180deg, #334155 0%, #0f172a 100%); border: 2px solid #64748b; color: #facc15; box-shadow: 0 4px 10px rgba(0,0,0,0.35); cursor: pointer;" id="btnAudioToggle" title="Bật/Tắt âm thanh">
                            <span class="material-symbols-outlined" style="font-size: 1.3rem;">volume_up</span>
                        </button>
                    </div>
                </header>

                <!-- 2. KHÔNG GIAN CHÍNH: THIÊN NHIÊN 3D, ĐƯỜNG RAY XE LỬA VÀ ĐOÀN TÀU -->
                <section class="train-scenic-playfield">
                    
                    <!-- Lớp cảnh quan 3D hoạt hình xóa bỏ khoảng trống -->
                    <div style="position: absolute; inset: 0; pointer-events: none; overflow: hidden;">
                        <!-- Mây bay 3D -->
                        <div class="floating-cloud-bg" style="position: absolute; top: 14px; left: 30px; background: rgba(255,255,255,0.92); border-radius: 999px; width: 120px; height: 40px; box-shadow: 0 4px 12px rgba(0,0,0,0.1);"></div>
                        <div class="floating-cloud-bg" style="position: absolute; top: 26px; right: 120px; background: rgba(255,255,255,0.85); border-radius: 999px; width: 150px; height: 46px; box-shadow: 0 4px 12px rgba(0,0,0,0.1); animation-delay: -3s;"></div>
                        
                        <!-- Khinh khí cầu mini ở góc trời -->
                        <div style="position: absolute; top: 28px; left: 32%; display: flex; flex-direction: column; align-items: center; opacity: 0.85;">
                            <div style="width: 32px; height: 40px; background: linear-gradient(135deg, #ec4899 0%, #f97316 50%, #facc15 100%); border-radius: 999px; box-shadow: 0 2px 6px rgba(0,0,0,0.2);"></div>
                            <div style="width: 10px; height: 8px; background: #92400e; border-radius: 2px; margin-top: 2px;"></div>
                        </div>

                        <!-- Cầu vồng bán nguyệt mờ ảo phía xa -->
                        <div style="position: absolute; -top: 60px; left: 50%; transform: translateX(-50%); width: 420px; height: 210px; border-top: 14px solid rgba(248,113,113,0.35); border-radius: 210px 210px 0 0; pointer-events: none;">
                            <div style="width: 100%; height: 100%; border-top: 12px solid rgba(253,224,71,0.35); border-radius: 210px 210px 0 0;">
                                <div style="width: 100%; height: 100%; border-top: 10px solid rgba(134,239,172,0.35); border-radius: 210px 210px 0 0;"></div>
                            </div>
                        </div>

                        <!-- Dãy đồi núi xanh 3D nhấp nhô -->
                        <div style="position: absolute; bottom: 50px; left: 0; right: 0; height: 160px; display: flex; align-items: flex-end; pointer-events: none;">
                            <div style="width: 42%; height: 130px; background: linear-gradient(180deg, #52b788 0%, #2d6a4f 100%); border-radius: 0 140px 0 0; opacity: 0.9;"></div>
                            <div style="width: 60%; height: 160px; margin-left: -70px; background: linear-gradient(180deg, #40916c 0%, #1b4332 100%); border-radius: 170px 150px 0 0; box-shadow: 0 -4px 15px rgba(0,0,0,0.15);"></div>
                            <div style="width: 35%; height: 110px; margin-left: -80px; background: linear-gradient(180deg, #74c69d 0%, #2d6a4f 100%); border-radius: 110px 0 0 0;"></div>
                        </div>

                        <!-- Cây nấm & cây thông 3D trang trí -->
                        <div style="position: absolute; bottom: 65px; left: 40px; display: flex; align-items: baseline; gap: 10px;">
                            <div style="width: 24px; height: 38px; background: #047857; border-radius: 999px; border: 2px solid #34d399; box-shadow: 0 2px 6px rgba(0,0,0,0.25);"></div>
                            <div style="width: 32px; height: 50px; background: #059669; border-radius: 999px; border: 2px solid #6ee7b7; box-shadow: 0 3px 8px rgba(0,0,0,0.3);"></div>
                        </div>

                        <!-- Nhà ga xe lửa mini 3D ở góc phải -->
                        <div style="position: absolute; bottom: 80px; right: 40px; display: flex; flex-direction: column; align-items: center; opacity: 0.92;">
                            <div style="width: 60px; height: 34px; background: #f43f5e; border-radius: 6px 6px 0 0; box-shadow: 0 3px 8px rgba(0,0,0,0.25); position: relative; border-bottom: 2px solid #78350f;">
                                <div style="position: absolute; top: -10px; left: 4px; right: 4px; height: 12px; background: #dc2626; border-radius: 8px 8px 0 0;"></div>
                                <div style="width: 14px; height: 16px; background: #fef08a; border-radius: 3px; margin: 10px auto 0;"></div>
                            </div>
                            <div style="width: 70px; height: 14px; background: #b45309; border-radius: 3px;"></div>
                            <span style="font-size: 9px; background: #ffffff; color: #065f46; font-weight: 900; padding: 2px 6px; border-radius: 4px; box-shadow: 0 1px 4px rgba(0,0,0,0.2); margin-top: 2px;">GA BÉ NGOAN</span>
                        </div>
                    </div>

                    <!-- Banner tiêu đề & câu hỏi gợi mở -->
                    <div style="position: relative; z-index: 10; padding: 10px 16px; display: flex; align-items: center; justify-content: space-between; gap: 10px;">
                        <!-- Huy hiệu tên trò chơi -->
                        <div class="train-title-badge">
                            <span style="font-size: 1.6rem; filter: drop-shadow(0 2px 4px rgba(0,0,0,0.2));">🚂</span>
                            <div>
                                <h1 style="font-family: 'Fredoka', cursive, sans-serif; font-size: 1.1rem; font-weight: 900; letter-spacing: 0.04em; color: #1b4332; text-transform: uppercase; margin: 0; line-height: 1.1;">ĐOÀN TÀU HƠI NƯỚC SỐ HỌC</h1>
                                <p style="font-size: 0.72rem; font-weight: 700; color: #2d6a4f; margin: 2px 0 0 0;">Học đếm số thứ tự tiến dần từ nhỏ tới lớn</p>
                            </div>
                        </div>

                        <!-- Bong bóng lời thoại câu hỏi từ trò chơi -->
                        <div class="train-question-bubble">
                            <span style="background: #facc15; color: #0f172a; font-size: 0.72rem; font-weight: 900; padding: 2px 8px; border-radius: 6px; text-transform: uppercase;">Hỏi</span>
                            <p style="font-size: 0.9rem; font-weight: 800; margin: 0; text-shadow: 0 1px 2px rgba(0,0,0,0.3);" id="trainGuideText">${instructionText}</p>
                        </div>
                    </div>

                    <!-- KHU VỰC TRỌNG TÂM: ĐOÀN TÀU 3D & ĐƯỜNG RAY XE LỬA -->
                    <div class="train-railroad-area">
                        <div style="width: 100%; display: flex; align-items: flex-end; justify-content: center; margin-bottom: 2px; transition: transform 1.2s cubic-bezier(0.25, 0.1, 0.25, 1);" id="fullTrainAssembly">
                            <!-- Nơi render đoàn tàu: Toa 1 -> Toa 2 -> Toa ? -> Đầu tàu (bên phải) -->
                            <div id="trainCarsRow" class="train-cars-row"></div>
                        </div>

                        <!-- ĐƯỜNG RAY XE LỬA 3D -->
                        <div class="train-track-wrap">
                            <div class="train-rail-top"></div>
                            <div class="train-ties-row">
                                ${Array.from({ length: 18 }).map(() => `
                                    <div class="train-tie-wood"></div>
                                `).join('')}
                            </div>
                            <div class="train-rail-bottom"></div>
                        </div>
                    </div>
                </section>

                <!-- 3. BẾN SÂN GA CHỌN TOA TÀU PHÍA DƯỚI (ẤM ÁP, BẮT MẮT) -->
                <footer class="train-platform-footer">
                    <div style="display: flex; align-items: center; justify-content: space-between; padding: 0 4px; margin-bottom: 4px;">
                        <div style="display: flex; align-items: center; gap: 8px;">
                            <span style="font-size: 1.5rem; filter: drop-shadow(0 2px 4px rgba(0,0,0,0.3));">🚉</span>
                            <span style="color: #ffffff; font-family: 'Fredoka', sans-serif; font-weight: 900; font-size: 1.1rem; letter-spacing: 0.05em; text-transform: uppercase; text-shadow: 0 2px 4px rgba(0,0,0,0.4);">
                                SÂN GA CHỌN TOA:
                            </span>
                        </div>
                        <div style="background: rgba(120,53,15,0.45); backdrop-filter: blur(4px); padding: 4px 14px; border-radius: 999px; color: #ffffff; font-size: 0.8rem; font-weight: 700; border: 1px solid rgba(254,240,138,0.5); display: flex; align-items: center; gap: 6px;">
                            <span>👇</span>
                            <span>Bé hãy chạm vào toa số chính xác để ghép vào đoàn tàu!</span>
                        </div>
                    </div>

                    <!-- Khay các toa lựa chọn màu tím hồng 3D nổi khối lớn -->
                    <div style="display: flex; align-items: center; justify-content: center; gap: 28px; padding-bottom: 2px;" id="stationChoicesContainer"></div>
                </footer>

                <!-- 4. MODAL BẢNG CHỌN MÀN CHƠI -->
                <div class="game-modal-overlay" id="alphabetModal" style="display: none;">
                    <div class="game-modal-card alphabet-modal-content">
                        <h2 class="modal-title" style="margin-bottom: 6px;">DANH SÁCH MÀN CHƠI</h2>
                        <p style="color: #cbd5e1; font-size: 0.95rem; margin-top: 0; margin-bottom: 12px; font-weight: 600;">
                            Bé chạm vào màn chơi muốn tham gia nhé!
                        </p>
                        <div class="alphabet-grid-wrap" id="alphabetGridContainer"></div>
                        <div style="margin-top: 14px;">
                            <button type="button" class="modal-btn modal-btn-secondary" id="btnCloseAlphabetModal" style="width: 100%;">
                                Đóng Lại
                            </button>
                        </div>
                    </div>
                </div>

                <!-- 5. POPUP HOÀN THÀNH MÀN TỰ ĐỘNG CHUYỂN QUA MÀN TIẾP NHƯ ĐÀO VÀNG -->
                <div class="game-modal-overlay" id="victoryModal" style="display: none; align-items: center; justify-content: center;">
                    <div class="victory-toast-card" id="victoryToast">
                        <div style="font-size: 4rem; margin-bottom: 6px; animation: bounceEmoji 1s infinite alternate;">🎉</div>
                        <h2 style="font-family: 'Fredoka', cursive, sans-serif; font-size: 2.1rem; font-weight: 900; color: #ffffff; margin: 0 0 6px 0; text-shadow: 0 2px 8px rgba(0,0,0,0.4);">
                            XUẤT SẮC!
                        </h2>
                        <p style="color: #fef08a; font-size: 1.2rem; font-weight: 700; margin: 0 0 8px 0;">
                            Đoàn tàu số đã sẵn sàng lăn bánh về ga tiếp theo!
                        </p>
                        <span style="font-size: 0.88rem; color: #cbd5e1; font-weight: 700;">Đang chuyển sang màn tiếp theo...</span>
                    </div>
                </div>
            </div>
        `;

        this.bindEvents();
        this.renderGameplayMode();
    }

    bindEvents() {
        const btnMissionVoice = document.getElementById('btnMissionVoice');
        const hudTargetChar = document.getElementById('hudTargetChar');
        const btnAudioToggle = document.getElementById('btnAudioToggle');
        const btnOpenAlphabet = document.getElementById('btnOpenAlphabet');
        const btnCloseAlphabetModal = document.getElementById('btnCloseAlphabetModal');
        const alphabetModal = document.getElementById('alphabetModal');

        const playTargetVoice = () => {
            if (this.currentLevelData?.instructionEn) {
                window.gameAudio?.speakEnglish(this.currentLevelData.instructionEn);
            }
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

        const levels = this.shell?.levels || window.GAME_LEVELS['number-train'] || [];
        const curLevel = this.currentLevelData?.level || 1;

        container.innerHTML = levels.map(lvl => {
            const isCurrent = lvl.level === curLevel;
            return `
                <button type="button" class="alphabet-item-btn ${isCurrent ? 'is-active' : ''}" data-jump-level="${lvl.level}">
                    <span class="alphabet-char">${lvl.level}</span>
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

    renderGameplayMode() {
        const type = this.currentLevelData?.type || 'missing-wagon';

        if (type === 'missing-wagon') {
            this.setupMissingWagonMode();
        } else if (type === 'wagon-sort') {
            this.setupWagonSortMode();
        } else if (type === 'cargo-count') {
            this.setupCargoCountMode();
        }
    }

    /* =========================================================================
       ĐẦU TÀU HƠI NƯỚC 3D (DẪN ĐẦU Ở BÊN PHẢI THEO MẪU)
       ========================================================================= */
    getLocomotiveHeadHTML() {
        return `
            <div style="display: flex; flex-direction: column; align-items: center; position: relative; flex-shrink: 0;">
                <!-- Khói 3D bốc lên -->
                <div style="position: absolute; top: -50px; right: 14px; pointer-events: none;">
                    <div class="smoke-bubble-1" style="position: absolute; width: 26px; height: 26px; background: rgba(255,255,255,0.88); border-radius: 50%; filter: blur(1px); box-shadow: 0 1px 3px rgba(0,0,0,0.1);"></div>
                    <div class="smoke-bubble-2" style="position: absolute; width: 30px; height: 30px; background: rgba(241,245,249,0.82); border-radius: 50%; filter: blur(1.5px); box-shadow: 0 1px 3px rgba(0,0,0,0.1);"></div>
                    <div class="smoke-bubble-3" style="position: absolute; width: 34px; height: 34px; background: rgba(226,232,240,0.75); border-radius: 50%; filter: blur(2px); box-shadow: 0 1px 3px rgba(0,0,0,0.1);"></div>
                </div>

                <!-- Thân đầu tàu màu đỏ cam 3D -->
                <div class="train-head-3d">
                    <!-- Ống khói -->
                    <div class="train-chimney">
                        <div class="train-chimney-cap"></div>
                    </div>
                    <!-- Còi hơi nóc -->
                    <div class="train-whistle"></div>
                    
                    <!-- Cabin bác tài -->
                    <div style="display: flex; align-items: center; justify-content: space-between; width: 100%; margin-top: 4px;">
                        <div class="train-cab-window">
                            <span style="font-size: 1.25rem;">👨‍✈️</span>
                        </div>
                        <div class="train-headlight">
                            <div style="width: 8px; height: 8px; border-radius: 50%; background: #ffffff;"></div>
                        </div>
                    </div>

                    <!-- Huy hiệu TU TU -->
                    <div class="train-tu-tu-badge">
                        TU TU! 💨
                    </div>

                    <div class="train-bumper"></div>
                </div>

                <!-- Bánh xe cơ khí & thanh truyền động -->
                <div class="train-wheels-row">
                    <div class="wheel-3d" style="width: 36px; height: 36px; z-index: 10;">
                        <div class="wheel-hub" style="width: 12px; height: 12px;"></div>
                    </div>
                    <div class="train-rod"></div>
                    <div class="wheel-3d" style="width: 32px; height: 32px; z-index: 10;">
                        <div class="wheel-hub" style="width: 10px; height: 10px;"></div>
                    </div>
                    <div class="wheel-3d" style="width: 28px; height: 28px; z-index: 10;">
                        <div class="wheel-hub" style="width: 8px; height: 8px;"></div>
                    </div>
                </div>
            </div>
        `;
    }

    /* =========================================================================
       1. CHẾ ĐỘ NỐI TOA THIẾU
       ========================================================================= */
    setupMissingWagonMode() {
        const trainCarsRow = document.getElementById('trainCarsRow');
        const stationChoices = document.getElementById('stationChoicesContainer');
        if (!trainCarsRow || !stationChoices) return;

        const sequence = this.currentLevelData?.trainSequence || [7, 8, null, 10];
        const choices = this.currentLevelData?.choices || [9, 6, 5];
        const answer = this.currentLevelData?.answer || 9;

        // Render các toa nối nhau phía sau đầu tàu (từ trái qua phải)
        let carsHTML = sequence.map((num) => {
            if (num === null) {
                return `
                    <div style="display: flex; align-items: flex-end;">
                        <div style="display: flex; flex-direction: column; align-items: center; position: relative;">
                            <div style="position: absolute; top: -14px; color: #fde047; font-size: 0.78rem; font-weight: 900; white-space: nowrap; animation: bounceEmoji 1s infinite alternate;">✨ Ghép vào đây ✨</div>
                            <div id="missingWagonSlot" class="slot-pulsing" style="width: 86px; height: 96px; flex-shrink: 0; border-radius: 18px; border: 4px dashed #ffea00; background: rgba(26,67,50,0.75); display: flex; flex-direction: column; align-items: center; justify-content: center; padding: 4px; box-sizing: border-box; cursor: pointer;">
                                <span style="font-family: 'Fredoka', cursive, sans-serif; font-size: 3rem; font-weight: 900; color: #fde047; text-shadow: 0 0 14px rgba(255,234,0,0.9); animation: pulse 1.5s infinite;">?</span>
                                <span style="font-size: 0.68rem; font-weight: 900; color: #fef08a; background: rgba(69,26,3,0.75); padding: 2px 8px; border-radius: 6px; border: 1px solid rgba(250,204,21,0.6); margin-top: 2px;">Ghép Vào</span>
                            </div>
                            <div style="display: flex; justify-content: space-between; width: 100%; padding: 0 6px; margin-top: -8px;">
                                <div style="width: 22px; height: 12px; background: #44403c; border-radius: 0 0 6px 6px; border: 1px dashed #facc15;"></div>
                                <div style="width: 22px; height: 12px; background: #44403c; border-radius: 0 0 6px 6px; border: 1px dashed #facc15;"></div>
                            </div>
                        </div>
                        <div class="train-coupler"></div>
                    </div>
                `;
            } else {
                return `
                    <div style="display: flex; align-items: flex-end;">
                        <div style="display: flex; flex-direction: column; align-items: center;">
                            <div class="train-car-3d">
                                <div style="display: flex; justify-content: space-between; width: 100%; padding: 0 2px;">
                                    <div style="width: 8px; height: 8px; border-radius: 50%; background: #fde047; box-shadow: 0 1px 2px rgba(0,0,0,0.3);"></div>
                                    <div style="width: 8px; height: 8px; border-radius: 50%; background: #fde047; box-shadow: 0 1px 2px rgba(0,0,0,0.3);"></div>
                                </div>
                                <span class="train-car-num">${num}</span>
                                <div style="width: 100%; height: 5px; background: rgba(0,119,182,0.6); border-radius: 999px;"></div>
                            </div>
                            <div style="display: flex; justify-content: space-between; width: 100%; padding: 0 4px; margin-top: -10px;">
                                <div class="wheel-3d" style="width: 28px; height: 28px;">
                                    <div class="wheel-hub" style="width: 10px; height: 10px;"></div>
                                </div>
                                <div class="wheel-3d" style="width: 28px; height: 28px;">
                                    <div class="wheel-hub" style="width: 10px; height: 10px;"></div>
                                </div>
                            </div>
                        </div>
                        <div class="train-coupler"></div>
                    </div>
                `;
            }
        }).join('');

        // Gắn thêm đầu tàu ở ngoài cùng bên phải
        trainCarsRow.innerHTML = carsHTML + this.getLocomotiveHeadHTML();

        // Render khay các toa tàu lựa chọn 3D tím hồng
        stationChoices.innerHTML = choices.map(choice => `
            <button type="button" class="car-pick-3d" data-choice-val="${choice}" title="Chọn toa số ${choice}">
                <div style="display: flex; justify-content: space-between; width: 100%; padding: 0 4px;">
                    <div style="width: 10px; height: 10px; border-radius: 50%; background: #fde047; box-shadow: 0 0 6px #fde047;"></div>
                    <div style="width: 10px; height: 10px; border-radius: 50%; background: #fde047; box-shadow: 0 0 6px #fde047;"></div>
                </div>
                <span class="car-pick-num">${choice}</span>
                <div class="car-pick-badge">TOA SỐ</div>
            </button>
        `).join('');

        stationChoices.querySelectorAll('[data-choice-val]').forEach(btn => {
            btn.addEventListener('click', (e) => {
                const val = parseInt(e.currentTarget.dataset.choiceVal, 10);
                this.handleChoiceMissingWagon(val, answer, e.currentTarget);
            });
        });
    }

    handleChoiceMissingWagon(val, answer, btnEl) {
        if (this.isCompleted) return;

        if (val === answer) {
            this.isCompleted = true;
            window.gameAudio?.playTrainCouple();
            window.gameAudio?.speakLetterEnglish(val);

            const slot = document.getElementById('missingWagonSlot');
            if (slot) {
                slot.className = 'train-car-3d';
                slot.style.background = 'linear-gradient(180deg, #10b981 0%, #059669 70%, #047857 100%)';
                slot.style.borderColor = '#6ee7b7';
                slot.style.boxShadow = '0 8px 0 #064e3b, 0 14px 22px rgba(16,185,129,0.45), inset 0 4px 6px rgba(255,255,255,0.7)';
                slot.innerHTML = `
                    <div style="display: flex; justify-content: space-between; width: 100%; padding: 0 2px;">
                        <div style="width: 8px; height: 8px; border-radius: 50%; background: #fde047;"></div>
                        <div style="width: 8px; height: 8px; border-radius: 50%; background: #fde047;"></div>
                    </div>
                    <span class="train-car-num" style="animation: bounceEmoji 0.6s 2 alternate;">${val}</span>
                    <div style="font-size: 0.65rem; font-weight: 900; color: #a7f3d0; background: rgba(6,78,59,0.7); padding: 1px 6px; border-radius: 4px;">Chính xác! 🎉</div>
                `;
            }

            if (btnEl) {
                btnEl.style.opacity = '0.35';
                btnEl.style.pointerEvents = 'none';
                btnEl.style.transform = 'scale(0.92)';
            }

            setTimeout(() => {
                this.triggerTrainDriveAway();
            }, 600);
        } else {
            window.gameAudio?.playWrongWobble();
            window.gameAudio?.speakLetterEnglish(val);
            if (btnEl) {
                btnEl.animate([
                    { transform: 'scale(1)' },
                    { transform: 'scale(0.92) rotate(-3deg)' },
                    { transform: 'scale(1)' }
                ], { duration: 250 });
            }
        }
    }

    /* =========================================================================
       2. CHẾ ĐỘ SẮP XẾP THỨ TỰ TOA
       ========================================================================= */
    setupWagonSortMode() {
        const trainCarsRow = document.getElementById('trainCarsRow');
        const stationChoices = document.getElementById('stationChoicesContainer');
        if (!trainCarsRow || !stationChoices) return;

        const wagons = this.currentLevelData?.wagons || [3, 1, 2];
        const correctOrder = this.currentLevelData?.correctOrder || [1, 2, 3];
        this.sortedWagons = [];

        trainCarsRow.innerHTML = `
            <div id="sortedWagonsTrack" class="train-cars-row"></div>
            <div style="display: flex; align-items: flex-end;">
                <div class="slot-pulsing" style="width: 86px; height: 96px; flex-shrink: 0; border-radius: 18px; border: 4px dashed #ffea00; background: rgba(26,67,50,0.75); display: flex; align-items: center; justify-content: center; padding: 4px; box-sizing: border-box; color: #fde047; font-weight: 900; font-size: 0.78rem; text-align: center;">
                    Nối Toa Tiếp Theo
                </div>
                <div class="train-coupler"></div>
            </div>
            ${this.getLocomotiveHeadHTML()}
        `;

        stationChoices.innerHTML = wagons.map(w => `
            <button type="button" class="car-pick-3d" data-wagon-val="${w}" title="Chạm nối toa ${w}">
                <div style="display: flex; justify-content: space-between; width: 100%; padding: 0 4px;">
                    <div style="width: 10px; height: 10px; border-radius: 50%; background: #fde047; box-shadow: 0 0 6px #fde047;"></div>
                    <div style="width: 10px; height: 10px; border-radius: 50%; background: #fde047; box-shadow: 0 0 6px #fde047;"></div>
                </div>
                <span class="car-pick-num">${w}</span>
                <div class="car-pick-badge">CHẠM NỐI</div>
            </button>
        `).join('');

        stationChoices.querySelectorAll('[data-wagon-val]').forEach(btn => {
            btn.addEventListener('click', (e) => {
                const val = parseInt(e.currentTarget.dataset.wagonVal, 10);
                this.handleSortWagonClick(val, correctOrder, e.currentTarget);
            });
        });
    }

    handleSortWagonClick(val, correctOrder, btnEl) {
        if (this.isCompleted) return;

        const nextExpected = correctOrder[this.sortedWagons.length];
        if (val === nextExpected) {
            this.sortedWagons.push(val);
            window.gameAudio?.playTrainCouple();
            window.gameAudio?.speakLetterEnglish(val);

            const track = document.getElementById('sortedWagonsTrack');
            if (track) {
                const carDiv = document.createElement('div');
                carDiv.style.display = 'flex';
                carDiv.style.alignItems = 'flex-end';
                carDiv.innerHTML = `
                    <div style="display: flex; flex-direction: column; align-items: center;">
                        <div class="train-car-3d animate-bounce">
                            <div style="display: flex; justify-content: space-between; width: 100%; padding: 0 2px;">
                                <div style="width: 8px; height: 8px; border-radius: 50%; background: #fde047;"></div>
                                <div style="width: 8px; height: 8px; border-radius: 50%; background: #fde047;"></div>
                            </div>
                            <span class="train-car-num">${val}</span>
                            <div style="width: 100%; height: 5px; background: rgba(0,119,182,0.6); border-radius: 999px;"></div>
                        </div>
                        <div style="display: flex; justify-content: space-between; width: 100%; padding: 0 4px; margin-top: -10px;">
                            <div class="wheel-3d" style="width: 28px; height: 28px;"><div class="wheel-hub" style="width: 10px; height: 10px;"></div></div>
                            <div class="wheel-3d" style="width: 28px; height: 28px;"><div class="wheel-hub" style="width: 10px; height: 10px;"></div></div>
                        </div>
                    </div>
                    <div class="train-coupler"></div>
                `;
                track.appendChild(carDiv);
            }

            if (btnEl) btnEl.style.display = 'none';

            if (this.sortedWagons.length === correctOrder.length) {
                this.isCompleted = true;
                setTimeout(() => {
                    this.triggerTrainDriveAway();
                }, 600);
            }
        } else {
            window.gameAudio?.playWrongWobble();
            window.gameAudio?.speakLetterEnglish(val);
            if (btnEl) {
                btnEl.animate([
                    { transform: 'scale(1)' },
                    { transform: 'scale(0.92) rotate(-3deg)' },
                    { transform: 'scale(1)' }
                ], { duration: 250 });
            }
        }
    }

    /* =========================================================================
       3. CHẾ ĐỘ CHẤT HÀNG ĐẾM SỐ
       ========================================================================= */
    setupCargoCountMode() {
        const trainCarsRow = document.getElementById('trainCarsRow');
        const stationChoices = document.getElementById('stationChoicesContainer');
        if (!trainCarsRow || !stationChoices) return;

        const emoji = this.currentLevelData?.cargoEmoji || '🍎';
        const required = this.currentLevelData?.countRequired || 3;
        this.currentCargoCount = 0;

        trainCarsRow.innerHTML = `
            <div style="display: flex; align-items: flex-end;">
                <div style="display: flex; flex-direction: column; align-items: center;">
                    <div class="train-car-3d" style="width: 220px; height: 96px; border-color: #f59e0b; background: linear-gradient(180deg, #d97706 0%, #b45309 70%, #78350f 100%);">
                        <div style="font-size: 0.72rem; font-weight: 900; background: rgba(0,0,0,0.45); color: #fef08a; padding: 2px 10px; border-radius: 999px; margin: 0 auto;">
                            TOA CHỞ HÀNG (<span id="cargoCountProgress">0</span>/${required})
                        </div>
                        <div id="cargoHoldArea" style="display: flex; gap: 8px; font-size: 1.8rem; justify-content: center; align-items: center; flex: 1;"></div>
                        <div style="width: 100%; height: 5px; background: rgba(0,0,0,0.3); border-radius: 999px;"></div>
                    </div>
                    <div style="display: flex; justify-content: space-between; width: 100%; padding: 0 12px; margin-top: -10px;">
                        <div class="wheel-3d" style="width: 28px; height: 28px;"><div class="wheel-hub" style="width: 10px; height: 10px;"></div></div>
                        <div class="wheel-3d" style="width: 28px; height: 28px;"><div class="wheel-hub" style="width: 10px; height: 10px;"></div></div>
                        <div class="wheel-3d" style="width: 28px; height: 28px;"><div class="wheel-hub" style="width: 10px; height: 10px;"></div></div>
                    </div>
                </div>
                <div class="train-coupler"></div>
            </div>
            ${this.getLocomotiveHeadHTML()}
        `;

        const totalItems = required + 2;
        stationChoices.innerHTML = Array.from({ length: totalItems }).map(() => `
            <button type="button" class="car-pick-3d" style="font-size: 2.8rem; display: flex; align-items: center; justify-content: center;">
                ${emoji}
            </button>
        `).join('');

        stationChoices.querySelectorAll('.car-pick-3d').forEach(btn => {
            btn.addEventListener('click', (e) => {
                this.handleCargoItemClick(e.currentTarget, emoji, required);
            });
        });
    }

    handleCargoItemClick(btnEl, emoji, required) {
        if (this.isCompleted || this.currentCargoCount >= required) return;

        this.currentCargoCount++;
        window.gameAudio?.playRocketLaunch();
        window.gameAudio?.speakLetterEnglish(this.currentCargoCount);

        const hold = document.getElementById('cargoHoldArea');
        const prog = document.getElementById('cargoCountProgress');
        if (hold) {
            const itemSpan = document.createElement('span');
            itemSpan.className = 'animate-bounce';
            itemSpan.textContent = emoji;
            hold.appendChild(itemSpan);
        }
        if (prog) prog.textContent = this.currentCargoCount;

        if (btnEl) {
            btnEl.classList.add('opacity-0', 'scale-50', 'pointer-events-none');
            setTimeout(() => btnEl.remove(), 180);
        }

        if (this.currentCargoCount >= required) {
            this.isCompleted = true;
            window.gameAudio?.playTrainCouple();
            setTimeout(() => {
                this.triggerTrainDriveAway();
            }, 600);
        }
    }

    /* =========================================================================
       HOẠT HỌA ĐOÀN TÀU LĂN BÁNH VÀ TỰ ĐỘNG CHUYỂN MÀN
       ========================================================================= */
    triggerTrainDriveAway() {
        const fullTrain = document.getElementById('fullTrainAssembly');
        window.gameAudio?.playTrainWhistle();

        if (fullTrain) {
            fullTrain.style.transform = 'translateX(130%)';
        }

        setTimeout(() => {
            const victoryModal = document.getElementById('victoryModal');
            const toastEl = document.getElementById('victoryToast');
            if (victoryModal) {
                window.gameAudio?.playVictoryFanfare();
                victoryModal.style.display = 'flex';
            }

            const nextLvl = (this.currentLevelData?.level || 1) + 1;
            const totalLevels = (this.shell?.levels || []).length || 10;

            // TỰ ĐỘNG CHUYỂN QUA MÀN TIẾP THEO SAU 1.8S NHƯ ĐÀO VÀNG
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
        }, 900);
    }
}

window.NumberTrainGame = NumberTrainGame;
