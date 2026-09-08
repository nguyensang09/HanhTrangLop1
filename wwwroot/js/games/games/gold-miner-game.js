/**
 * GoldMinerGame - Đào Vàng Tìm Chữ & Số (Jelly Gems 3D & Mechanical Claw)
 * Tính năng chuẩn hóa:
 * - Khối chữ/số thạch 3D di chuyển bồng bềnh liên tục trong hang mỏ (Physics drifting & bouncing 60fps)
 * - Click vào chữ: chữ đứng im ngay lập tức, cần cẩu tính góc và khoảng cách vươn tới đúng tâm khối chữ
 * - Móng vuốt kẹp khối chữ và kéo về vị trí đích trên trần, bùng nổ ngôi sao lấp lánh rồi mới biến mất và tăng tiến độ
 * - Bảng chọn 29 chữ cái tiếng Việt + 10 chữ số (Alphabet Picker)
 * - Tự động tiếp tục theo tiến độ màn chơi đã lưu nếu không chọn thủ công
 * - Modal chiến thắng tuyệt đẹp với nút "Màn Tiếp Theo" mượt mà
 */
class GoldMinerGame {
    constructor(container, shell) {
        this.container = container;
        this.shell = shell;
        this.currentLevelData = null;

        this.targetGemsCollected = 0;
        this.totalRequired = 3;
        this.wrongAttempts = 0;
        this.isCompleted = false;
        this.isClawBusy = false;
        this.soundEnabled = true;

        // Quản lý các khối ngọc đang di chuyển
        this.gems = [];
        this.animFrameId = null;
        this.lastFrameTime = 0;

        // Vị trí puli kéo móc trên đỉnh trần
        this.originX = 0;
        this.originY = 24;
    }

    init(levelData) {
        this.stopPhysicsLoop();
        this.currentLevelData = levelData;
        this.targetGemsCollected = 0;
        this.totalRequired = 3;
        this.wrongAttempts = 0;
        this.isCompleted = false;
        this.isClawBusy = false;
        this.gems = [];

        // Lưu lại level hiện tại vào localStorage để nếu lần sau vào lại sẽ tiếp tục từ màn này
        this.saveCurrentLevelProgress(levelData?.level || 1);

        this.render();
    }

    render() {
        const target = this.currentLevelData?.target || 'A';
        const targetType = this.currentLevelData?.targetType === 'number' ? 'Chữ Số' : 'Chữ Cái';
        const levelNum = this.currentLevelData?.level || 1;

        this.container.innerHTML = `
            <div class="miner-stage-container" id="minerStage">
                <!-- Hiệu ứng nền bụi vàng lấp lánh trong hang mỏ -->
                <div style="position: absolute; inset: 0; pointer-events: none; overflow: hidden; z-index: 0;">
                    <div style="position: absolute; inset: 0; background: radial-gradient(circle at 50% 50%, rgba(15, 23, 42, 0.15) 0%, rgba(10, 15, 29, 0.65) 100%), linear-gradient(180deg, rgba(0, 0, 0, 0.4) 0%, rgba(0, 0, 0, 0.05) 50%, rgba(0, 0, 0, 0.55) 100%);"></div>
                    <div class="animate-sparkle" style="position: absolute; bottom: 110px; left: 22%; width: 12px; height: 12px; background: #fef08a; border-radius: 50%; filter: blur(1px);"></div>
                    <div class="animate-sparkle" style="position: absolute; bottom: 140px; right: 26%; width: 10px; height: 10px; background: #fef9c3; border-radius: 50%; filter: blur(1px); animation-delay: 0.7s;"></div>
                    <div class="animate-sparkle" style="position: absolute; bottom: 80px; left: 50%; width: 14px; height: 14px; background: #fde047; border-radius: 50%; filter: blur(2px); animation-delay: 1.3s;"></div>
                </div>

                <!-- 1. THANH ĐIỀU HƯỚNG & NHIỆM VỤ TRÊN CÙNG -->
                <header class="miner-nav-bar" id="minerTopNav">
                    <!-- Trái: Nút Về Sảnh & Chọn Chữ -->
                    <div class="miner-nav-left">
                        <a href="/kids/games" class="miner-pill-btn btn-lobby-amber" id="btnBackToLobby" title="Trở về sảnh trò chơi">
                            <span class="material-symbols-outlined" style="font-size: 1.25rem;">arrow_back</span>
                            <span>VỀ SẢNH</span>
                        </a>

                        <button type="button" class="miner-pill-btn btn-alphabet-teal" id="btnOpenAlphabet" title="Mở bảng chữ cái để chọn chữ muốn học">
                            <span class="material-symbols-outlined" style="font-size: 1.25rem;">grid_view</span>
                            <span>CHỌN CHỮ / SỐ</span>
                        </button>

                        <div class="miner-level-badge">
                            Màn ${levelNum}
                        </div>
                    </div>

                    <!-- Giữa: KHOẢNG TRỐNG HOÀN TOÀN ĐỂ LỘ PULI RÒNG RỌC VÀ DÂY CÁP MÓC KÉO -->
                    <div class="miner-nav-center-spacer"></div>

                    <!-- Phải: Viên Nang Nhiệm Vụ Mục Tiêu & Nút Âm Thanh -->
                    <div class="miner-nav-right">
                        <div class="miner-mission-capsule">
                            <!-- Nút loa nghe bài học -->
                            <button type="button" class="miner-voice-btn" id="btnMissionVoice" title="Nhấn để nghe lại phát âm">
                                <span class="material-symbols-outlined" style="font-size: 1.35rem;">volume_up</span>
                            </button>

                            <!-- Chữ mục tiêu -->
                            <div style="display: flex; align-items: center; gap: 8px;">
                                <div style="display: flex; flex-direction: column; text-align: left; line-height: 1.15;">
                                    <span style="font-size: 10px; font-weight: 900; text-transform: uppercase; color: #fde047; letter-spacing: 0.05em;">BÉ GẮP ĐỦ 3 KHỐI:</span>
                                    <span style="font-size: 12px; font-weight: 700; color: #f1f5f9;">${targetType}</span>
                                </div>

                                <div class="miner-target-badge" id="hudTargetChar">
                                    ${target}
                                </div>
                            </div>

                            <!-- Vạch phân cách -->
                            <div style="width: 2px; height: 28px; background: rgba(245, 158, 11, 0.4); margin: 0 4px;"></div>

                            <!-- 3 Ngôi Sao Tiến Độ (Gắp 3 lần) -->
                            <div style="display: flex; flex-direction: column; align-items: center; gap: 2px;">
                                <div style="display: flex; gap: 5px;" id="minerProgressStars">
                                    <div class="miner-star-slot empty" id="mstar-1">★</div>
                                    <div class="miner-star-slot empty" id="mstar-2">★</div>
                                    <div class="miner-star-slot empty" id="mstar-3">★</div>
                                </div>
                                <span style="font-size: 10px; font-weight: 800; color: #fef08a;" id="minerProgressCount">0 / 3</span>
                            </div>
                        </div>

                        <button type="button" class="miner-pill-btn" id="btnAudioToggle" style="background: rgba(30, 41, 59, 0.85); border: 2px solid rgba(245, 158, 11, 0.5); color: #fde047; padding: 7px 12px;" title="Bật/Tắt âm thanh">
                            <span class="material-symbols-outlined" style="font-size: 1.3rem;">volume_up</span>
                        </button>
                    </div>
                </header>

                <!-- 2. HỆ THỐNG MÓC CÂU CƠ HỌC (PULLEY & MECHANICAL CLAW) -->
                <div class="miner-pulley-rig" id="minerPulleyRig">
                    <!-- Giá đỡ trần xe goòng -->
                    <div class="miner-pulley-bracket">
                        <div class="miner-pulley-wheel">⚙</div>
                    </div>
                </div>

                <!-- Cần cẩu xoay hướng và dây cáp vươn dài -->
                <div class="miner-crane-arm" id="minerCraneArm">
                    <!-- Sợi dây cáp co giãn -->
                    <div class="miner-crane-cable" id="minerCraneCable"></div>
                    <!-- Đầu móng vuốt kẹp -->
                    <div class="miner-claw-head" id="minerClawHead">
                        <div class="miner-claw-wrist">
                            <div class="miner-claw-wrist-gem"></div>
                        </div>
                        <div class="miner-claw-pincers" id="minerClawPincers">
                            <div class="miner-pincer-left"></div>
                            <div class="miner-pincer-right"></div>
                        </div>
                    </div>
                </div>

                <!-- 3. KHU VỰC CHƠI TƯƠNG TÁC (CÁC KHỐI THẠCH DI CHUYỂN BỒNG BỀNH) -->
                <div class="relative z-20 w-full flex-1 pointer-events-auto overflow-hidden" id="gemPlayfield" style="position: absolute; inset: 65px 0 0 0;"></div>

                <!-- 4. MODAL BẢNG CHỌN CHỮ CÁI & CHỮ SỐ (ALPHABET CHOOSER) -->
                <div class="game-modal-overlay" id="alphabetModal" style="display: none;">
                    <div class="game-modal-card alphabet-modal-content">
                        <h2 class="modal-title" style="margin-bottom: 6px;">BẢNG CHỌN CHỮ & SỐ</h2>
                        <p style="color: #cbd5e1; font-size: 0.95rem; margin-top: 0; margin-bottom: 12px; font-weight: 600;">
                            Bé chạm vào chữ cái hoặc số muốn tập đào nhé!
                        </p>

                        <!-- Thanh chuyển Tab Chữ / Số -->
                        <div class="alphabet-tabs-bar">
                            <button type="button" class="alphabet-tab-btn active" id="tabLettersBtn">29 Chữ Cái Tiếng Việt</button>
                            <button type="button" class="alphabet-tab-btn" id="tabNumbersBtn">10 Chữ Số (0-9)</button>
                        </div>

                        <!-- Lưới danh sách chữ -->
                        <div class="alphabet-grid-wrap" id="alphabetGridContainer"></div>

                        <div style="margin-top: 14px;">
                            <button type="button" class="modal-btn modal-btn-secondary" id="btnCloseAlphabetModal" style="width: 100%;">
                                Đóng Lại
                            </button>
                        </div>
                    </div>
                </div>

                <!-- 5. POPUP THÔNG BÁO CHIẾN THẮNG NHỎ GỌN TỰ ĐỘNG QUA MÀN -->
                <div class="game-modal-overlay" id="victoryModal" style="display: none; align-items: center; justify-content: center; pointer-events: none;">
                    <div class="victory-toast-card" id="victoryToast">
                        <div style="font-size: 2.8rem; margin-bottom: 4px; filter: drop-shadow(0 4px 10px rgba(0,0,0,0.4));">🏆</div>
                        <div style="font-family: 'Fredoka', cursive; font-size: 1.75rem; font-weight: 900; color: #fef08a; text-shadow: 0 2px 8px rgba(0,0,0,0.6); margin-bottom: 2px;">
                            BÉ GIỎI QUÁ!
                        </div>
                        <div style="display: flex; justify-content: center; gap: 6px; font-size: 1.3rem; margin-bottom: 6px;">
                            <span>⭐</span><span>⭐</span><span>⭐</span>
                        </div>
                        <div style="color: #ffffff; font-size: 0.95rem; font-weight: 700; opacity: 0.95;" id="victoryMessage">
                            Bé đã gắp đủ 3 khối!
                        </div>
                        <div style="display: inline-flex; align-items: center; gap: 6px; font-size: 0.82rem; font-weight: 800; color: #6ee7b7; margin-top: 10px; background: rgba(16, 185, 129, 0.22); padding: 5px 14px; border-radius: 999px; border: 1px solid rgba(110, 231, 183, 0.4);">
                            <span>Đang chuyển sang màn tiếp theo...</span>
                        </div>
                    </div>
                </div>
            </div>
        `;

        this.playfield = document.getElementById('gemPlayfield');
        this.craneArm = document.getElementById('minerCraneArm');
        this.craneCable = document.getElementById('minerCraneCable');
        this.clawHead = document.getElementById('minerClawHead');
        this.clawPincers = document.getElementById('minerClawPincers');

        this.victoryModal = document.getElementById('victoryModal');
        this.alphabetModal = document.getElementById('alphabetModal');

        // Gắn sự kiện các nút
        document.getElementById('btnMissionVoice')?.addEventListener('click', () => this.playLessonVoice());
        document.getElementById('btnOpenAlphabet')?.addEventListener('click', () => this.openAlphabetModal());
        document.getElementById('btnCloseAlphabetModal')?.addEventListener('click', () => this.closeAlphabetModal());

        document.getElementById('tabLettersBtn')?.addEventListener('click', () => this.renderAlphabetGrid('letters'));
        document.getElementById('tabNumbersBtn')?.addEventListener('click', () => this.renderAlphabetGrid('numbers'));

        document.getElementById('btnAudioToggle')?.addEventListener('click', () => {
            this.soundEnabled = !this.soundEnabled;
            window.gameAudio?.setSoundEnabled(this.soundEnabled);
            const icon = this.soundEnabled ? 'volume_up' : 'volume_off';
            document.getElementById('btnAudioToggle').innerHTML = `<span class="material-symbols-outlined" style="font-size: 1.3rem;">${icon}</span>`;
        });

        // Tính toán tọa độ và sinh các khối chữ sau khi DOM đã render layout xong
        requestAnimationFrame(() => {
            this.updateOriginCoordinates();
            this.spawnMovingGems();
        });

        // Phát âm bài học khi mở màn
        setTimeout(() => {
            this.playLessonVoice();
        }, 500);

        // Hỗ trợ resize
        window.addEventListener('resize', this.onResize = () => {
            this.updateOriginCoordinates();
        });
    }

    updateOriginCoordinates() {
        if (!this.playfield) return;
        const rect = this.playfield.getBoundingClientRect();
        this.originX = rect.width / 2;
        this.originY = 0; // Đỉnh của playfield (dưới thanh top bar)
    }

    /**
     * Phát âm bài học tìm chữ từ hệ thống TextToSpeechCaches
     */
    async playLessonVoice() {
        const target = this.currentLevelData?.target || 'A';
        await window.gameAudio?.speakLetterEnglish(target);
    }

    /**
     * Sinh 3 khối mục tiêu + các khối gây nhiễu gồm:
     * 1. 3 Khối mục tiêu chữ/số (Có viền, nền vàng/đỏ/xanh)
     * 2. 4-5 Khối chữ cái & chữ số khác (Đều có viền và nền ngọc như hiện tại)
     * 3. 6-7 Khối đồ vật gây nhiễu (Ô tô, máy bay, phi thuyền, con chim, mặt trời, đám mây, chuông... BỎ VIỀN, BỎ NỀN, CHỈ CÓ ICON)
     */
    spawnMovingGems() {
        if (!this.playfield) return;
        this.playfield.innerHTML = '';
        this.gems = [];

        const target = this.currentLevelData?.target || 'A';
        const isNumber = this.currentLevelData?.targetType === 'number';

        // 1. 3 Khối mục tiêu: CÙNG 1 CHỮ/SỐ NHƯNG KHÁC NHAU VỀ MÀU SẮC (Vàng, Đỏ ruby, Xanh lục bảo)
        const targetThemes = [
            { id: 'target-1', colorClass: 'target-gem-gold', spark: '✨' },
            { id: 'target-2', colorClass: 'target-gem-ruby', spark: '🌟' },
            { id: 'target-3', colorClass: 'target-gem-emerald', spark: '💚' }
        ];

        const rawGems = targetThemes.map((t) => ({
            id: t.id,
            char: target,
            isTarget: true,
            isItem: false,
            isNumber: isNumber,
            colorClass: t.colorClass,
            spark: t.spark,
            size: 96
        }));

        // 2. Chữ và số gây nhiễu: Bắt buộc lẫn cả chữ và số khác (ĐỀU CÓ VIỀN VÀ NỀN)
        const alphabetPool = ['A', 'B', 'C', 'D', 'E', 'G', 'H', 'I', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'X', 'Y'];
        const numberPool = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9'];

        const availableLetters = alphabetPool.filter(c => c.toUpperCase() !== String(target).toUpperCase());
        const availableNumbers = numberPool.filter(n => String(n) !== String(target));

        availableLetters.sort(() => Math.random() - 0.5);
        availableNumbers.sort(() => Math.random() - 0.5);

        // Lấy 3 chữ cái và 2 chữ số khác
        const chosenLetters = availableLetters.slice(0, 3);
        const chosenNumbers = availableNumbers.slice(0, 2);
        const mixedDistractorChars = [...chosenLetters, ...chosenNumbers];
        mixedDistractorChars.sort(() => Math.random() - 0.5);

        const gemDistractorColors = [
            'gem-letter-sapphire',
            'gem-letter-amethyst',
            'gem-letter-amber',
            'gem-letter-teal',
            'gem-letter-rose'
        ];

        mixedDistractorChars.forEach((ch, idx) => {
            rawGems.push({
                id: `distractor-char-${idx}-${ch}`,
                char: ch,
                isTarget: false,
                isItem: false, // Là chữ/số -> có viền, nền như hiện tại
                isNumber: /\d/.test(ch),
                colorClass: gemDistractorColors[idx % gemDistractorColors.length],
                size: 90
            });
        });

        // 3. Khối đồ vật gây nhiễu: BỎ VIỀN, BỎ NỀN, CHỈ ĐỂ LẠI HÌNH ICON (ô tô, máy bay, phi thuyền, con chim, mặt trời, đám mây, chuông...)
        const distractorIconsPool = [
            // Phương tiện giao thông
            { id: 'car', label: '🚗', voiceName: 'ô tô', size: 78 },
            { id: 'racecar', label: '🏎️', voiceName: 'xe đua', size: 78 },
            { id: 'plane', label: '✈️', voiceName: 'máy bay', size: 82 },
            { id: 'rocket', label: '🚀', voiceName: 'phi thuyền', size: 82 },
            { id: 'ufo', label: '🛸', voiceName: 'đĩa bay', size: 80 },
            { id: 'train', label: '🚂', voiceName: 'tàu hỏa', size: 80 },
            { id: 'boat', label: '⛵', voiceName: 'thuyền buồm', size: 78 },

            // Bầu trời & thiên nhiên
            { id: 'bird', label: '🐦', voiceName: 'con chim', size: 76 },
            { id: 'eagle', label: '🦅', voiceName: 'đại bàng', size: 80 },
            { id: 'sun', label: '☀️', voiceName: 'mặt trời', size: 82 },
            { id: 'sunface', label: '🌞', voiceName: 'ông mặt trời', size: 82 },
            { id: 'cloud', label: '☁️', voiceName: 'đám mây', size: 80 },
            { id: 'rainbow', label: '🌈', voiceName: 'cầu vồng', size: 82 },

            // Vật dụng, quả chuông, quà tặng
            { id: 'bell', label: '🔔', voiceName: 'quả chuông', size: 76 },
            { id: 'diamond', label: '💎', voiceName: 'kim cương', size: 78 },
            { id: 'mushroom', label: '🍄', voiceName: 'cây nấm', size: 74 },
            { id: 'gift', label: '🎁', voiceName: 'hộp quà', size: 76 },
            { id: 'star', label: '⭐', voiceName: 'ngôi sao', size: 76 },
            { id: 'balloon', label: '🎈', voiceName: 'bong bóng', size: 76 },
            { id: 'apple', label: '🍎', voiceName: 'quả táo', size: 74 },
            { id: 'fish', label: '🐟', voiceName: 'con cá', size: 74 },
            { id: 'flower', label: '🌸', voiceName: 'bông hoa', size: 74 },
            { id: 'soccer', label: '⚽', voiceName: 'quả bóng', size: 76 }
        ];

        // Ưu tiên chọn các hình ô tô, máy bay, phi thuyền, chim, mặt trời, mây, chuông
        const priorityIds = ['car', 'plane', 'rocket', 'bird', 'sun', 'cloud', 'bell'];
        const priorityItems = distractorIconsPool.filter(i => priorityIds.includes(i.id));
        const otherItems = distractorIconsPool.filter(i => !priorityIds.includes(i.id));

        priorityItems.sort(() => Math.random() - 0.5);
        otherItems.sort(() => Math.random() - 0.5);

        // Lấy 4 hình ưu tiên + 3 hình ngẫu nhiên khác -> 7 khối icon
        const chosenIconObjects = [...priorityItems.slice(0, 4), ...otherItems.slice(0, 3)];
        chosenIconObjects.sort(() => Math.random() - 0.5);

        chosenIconObjects.forEach((item) => {
            rawGems.push({
                id: `distractor-obj-${item.id}`,
                label: item.label,
                char: item.label,
                voiceName: item.voiceName,
                isTarget: false,
                isItem: true, // Khối gây nhiễu đồ vật: KHÔNG VIỀN, KHÔNG NỀN
                isNumber: false,
                colorClass: 'gem-pure-icon',
                size: item.size
            });
        });

        // Kích thước vùng chơi
        const rect = this.playfield.getBoundingClientRect();
        let width = rect.width || this.playfield.clientWidth;
        let height = rect.height || this.playfield.clientHeight;
        if (!width || width < 300) width = Math.max(window.innerWidth - 80, 950);
        if (!height || height < 300) height = Math.max(window.innerHeight - 150, 550);

        const minX = 40;
        const maxX = Math.max(300, width - 40);
        const minY = 70; // Tránh móc đỉnh
        const maxY = Math.max(200, height - 55);

        // 3 phân vùng riêng biệt cho 3 khối mục tiêu để luôn dàn trải đều màn hình
        const targetSlices = [
            { minX: minX + 10, maxX: minX + (maxX - minX) * 0.32 },
            { minX: minX + (maxX - minX) * 0.36, maxX: minX + (maxX - minX) * 0.64 },
            { minX: minX + (maxX - minX) * 0.68, maxX: maxX - 20 }
        ];
        targetSlices.sort(() => Math.random() - 0.5);

        let targetIdx = 0;
        const placedGems = [];

        rawGems.forEach((gemData) => {
            const size = gemData.size;
            let sliceMinX = minX;
            let sliceMaxX = maxX - size;

            if (gemData.isTarget && targetIdx < targetSlices.length) {
                const slice = targetSlices[targetIdx++];
                sliceMinX = Math.max(minX, slice.minX);
                sliceMaxX = Math.min(maxX - size, slice.maxX - size);
            }

            // Tìm vị trí không quá sát nhau
            let x = sliceMinX;
            let y = minY;
            let found = false;
            for (let attempt = 0; attempt < 80; attempt++) {
                const testX = sliceMinX + Math.random() * Math.max(10, sliceMaxX - sliceMinX);
                const testY = minY + Math.random() * Math.max(10, maxY - minY - size);
                const hasOverlap = placedGems.some(p => {
                    const dist = Math.hypot((testX + size / 2) - (p.x + p.size / 2), (testY + size / 2) - (p.y + p.size / 2));
                    return dist < (size / 2 + p.size / 2 + 16);
                });
                if (!hasOverlap) {
                    x = testX;
                    y = testY;
                    found = true;
                    break;
                }
            }
            if (!found) {
                x = sliceMinX + Math.random() * Math.max(10, sliceMaxX - sliceMinX);
                y = minY + Math.random() * Math.max(10, maxY - minY - size);
            }
            placedGems.push({ x, y, size });

            // Vận tốc trôi nhẹ nhàng ngẫu nhiên
            const speed = 0.32 + Math.random() * 0.28;
            const angle = Math.random() * Math.PI * 2;
            const vx = Math.cos(angle) * speed;
            const vy = Math.sin(angle) * speed;
            const rot = (Math.random() * 10 - 5).toFixed(1);

            // Tạo phần tử DOM
            const btn = document.createElement('button');
            btn.type = 'button';
            btn.setAttribute('data-id', gemData.id);
            btn.setAttribute('data-gem', gemData.char);
            btn.setAttribute('data-target', gemData.isTarget ? 'true' : 'false');

            btn.className = `gem-block ${gemData.colorClass} animate-pulse-gentle`;
            btn.style.width = `${size}px`;
            btn.style.height = `${size}px`;
            btn.style.left = `${Math.round(x)}px`;
            btn.style.top = `${Math.round(y)}px`;
            btn.style.transform = `rotate(${rot}deg)`;
            btn.style.fontFamily = "'Fredoka', cursive, sans-serif";

            let innerContent = '';

            if (gemData.isItem) {
                // Khối đồ vật gây nhiễu: BỎ VIỀN, BỎ NỀN, CHỈ ĐỂ LẠI HÌNH ICON!
                innerContent = `
                    <span class="pure-distractor-icon">${gemData.label}</span>
                `;
            } else {
                // Số hoặc Chữ (dù là mục tiêu hay gây nhiễu): ĐỀU CÓ VIỀN VÀ NỀN NHƯ HIỆN TẠI
                innerContent = `
                    <div class="jelly-highlight"></div>
                    <div class="gem-corner-rivet gem-corner-tl"></div>
                    <div class="gem-corner-rivet gem-corner-tr"></div>
                    <div class="gem-corner-rivet gem-corner-bl"></div>
                    <div class="gem-corner-rivet gem-corner-br"></div>
                    <span class="text-hero-char">${gemData.char}</span>
                `;

                if (gemData.isTarget) {
                    innerContent += `
                        <div style="position: absolute; bottom: 6px; left: 50%; transform: translateX(-50%); display: flex; gap: 4px; align-items: center; z-index: 5; opacity: 0.22; pointer-events: none;">
                            <span style="width: 5px; height: 3px; background: #ffffff; border-radius: 999px;"></span>
                            <span style="font-size: 10px; font-weight: bold; color: #ffffff;">‿</span>
                            <span style="width: 5px; height: 3px; background: #ffffff; border-radius: 999px;"></span>
                        </div>
                        <span style="position: absolute; top: -6px; right: -4px; font-size: 1.3rem;" class="animate-sparkle">${gemData.spark}</span>
                    `;
                }
            }

            btn.innerHTML = innerContent;

            const gemObj = {
                ...gemData,
                element: btn,
                x: x,
                y: y,
                vx: vx,
                vy: vy,
                rot: parseFloat(rot),
                isFrozen: false,
                isCollected: false
            };

            // Sự kiện Click vào chữ hoặc vật phẩm
            btn.addEventListener('click', (e) => {
                e.stopPropagation();
                this.handleGemClick(gemObj);
            });

            this.playfield.appendChild(btn);
            this.gems.push(gemObj);
        });

        // Bắt đầu vòng lặp vật lý
        this.startPhysicsLoop();
    }



    /**
     * Vòng lặp chuyển động liên tục của các khối chữ (60 FPS)
     */
    startPhysicsLoop() {
        this.stopPhysicsLoop();

        const loop = (timestamp) => {
            if (!this.lastFrameTime) this.lastFrameTime = timestamp;
            const delta = Math.min(32, timestamp - this.lastFrameTime);
            this.lastFrameTime = timestamp;

            const rect = this.playfield?.getBoundingClientRect();
            const width = rect?.width || this.playfield?.clientWidth || 1100;
            const height = rect?.height || this.playfield?.clientHeight || 560;
            const minX = 20;
            const maxX = width - 20;
            const minY = 50;
            const maxY = height - 30;

            this.gems.forEach((gem) => {
                // Nếu đang bị đóng băng hoặc đã thu hồi về đích -> không di chuyển
                if (gem.isFrozen || gem.isCollected) return;

                gem.x += gem.vx * (delta / 16);
                gem.y += gem.vy * (delta / 16);

                // Va chạm mép trái / phải
                if (gem.x < minX) {
                    gem.x = minX;
                    gem.vx = Math.abs(gem.vx) * (0.9 + Math.random() * 0.2);
                } else if (gem.x + gem.size > maxX) {
                    gem.x = maxX - gem.size;
                    gem.vx = -Math.abs(gem.vx) * (0.9 + Math.random() * 0.2);
                }

                // Va chạm mép trên / dưới
                if (gem.y < minY) {
                    gem.y = minY;
                    gem.vy = Math.abs(gem.vy) * (0.9 + Math.random() * 0.2);
                } else if (gem.y + gem.size > maxY) {
                    gem.y = maxY - gem.size;
                    gem.vy = -Math.abs(gem.vy) * (0.9 + Math.random() * 0.2);
                }

                // Cập nhật vị trí bằng left/top để không xung đột CSS animation
                gem.element.style.left = `${gem.x.toFixed(1)}px`;
                gem.element.style.top = `${gem.y.toFixed(1)}px`;
            });

            this.animFrameId = requestAnimationFrame(loop);
        };

        this.animFrameId = requestAnimationFrame(loop);
    }

    stopPhysicsLoop() {
        if (this.animFrameId) {
            cancelAnimationFrame(this.animFrameId);
            this.animFrameId = null;
        }
    }

    /**
     * XỬ LÝ CLICK:
     * 1. Khối chữ đứng im ngay lập tức
     * 2. Phát âm ngay chữ, số hoặc tên kho báu đó từ TextToSpeechCaches
     * 3. Móc cẩu xoay đúng góc và vươn dài tới đúng tâm khối chữ (chậm rãi, chân thực)
     * 4. Kẹp chặt khối chữ và kéo êm ái về đích trên trần
     * 5. Về tới đỉnh: Khối chữ biến mất, nổ sao hạt vàng, phát âm "Đúng rồi!", tăng tiến độ
     */
    async handleGemClick(gem) {
        if (this.isClawBusy || this.isCompleted || gem.isCollected) return;
        this.isClawBusy = true;

        // 1. Chữ ĐỨNG IM NGAY LẬP TỨC
        gem.isFrozen = true;

        // 2. Phát âm: Chữ cái và chữ số đọc bằng voice tiếng Anh theo yêu cầu!
        if (gem.isItem) {
            window.gameAudio?.playSystemVoiceOrSpeak(gem.voiceName);
        } else {
            window.gameAudio?.speakLetterEnglish(gem.char);
        }

        // 3. Tính toán góc và khoảng cách từ tâm mâm xoay cần cẩu tới tâm khối chữ
        // Dùng getBoundingClientRect để đạt độ chính xác tuyệt đối theo viewport
        const armRect = this.craneArm.getBoundingClientRect();
        const pivotX = armRect.left;
        const pivotY = armRect.top;

        const gemRect = gem.element.getBoundingClientRect();
        const gemCenterX = gemRect.left + gemRect.width / 2;
        const gemCenterY = gemRect.top + gemRect.height / 2;

        const dx = gemCenterX - pivotX;
        const dy = gemCenterY - pivotY;

        // Góc xoay chuẩn theo hệ trục màn hình:
        // Cần cẩu mặc định cắm thẳng xuống (vector (0, 1) ở góc 0 độ).
        // Khi gem ở bên phải (dx > 0): cần cẩu xoay ngược chiều kim đồng hồ -> góc âm.
        // Khi gem ở bên trái (dx < 0): cần cẩu xoay cùng chiều kim đồng hồ -> góc dương.
        const angleRad = Math.atan2(-dx, dy);
        const angleDeg = angleRad * (180 / Math.PI);
        const distance = Math.hypot(dx, dy);

        // Cần cẩu xoay mượt mà theo đúng hướng đó
        if (this.craneArm) {
            this.craneArm.style.transition = 'transform 0.3s cubic-bezier(0.25, 1, 0.5, 1)';
            this.craneArm.style.transform = `rotate(${angleDeg.toFixed(2)}deg)`;
        }

        // Mở rộng dây cáp vươn thẳng tới khối chữ (tốc độ cơ học chậm rãi hơn)
        const targetCableLength = Math.max(30, distance - 20);
        const extendDuration = Math.min(1300, Math.max(900, distance * 1.5));

        setTimeout(() => {
            if (this.craneCable) {
                this.craneCable.style.transition = `height ${extendDuration}ms cubic-bezier(0.25, 1, 0.5, 1)`;
                this.craneCable.style.height = `${targetCableLength}px`;
            }
            if (this.clawHead) {
                this.clawHead.style.transition = `top ${extendDuration}ms cubic-bezier(0.25, 1, 0.5, 1)`;
                this.clawHead.style.top = `${targetCableLength}px`;
            }
        }, 180);

        // Chờ cần cẩu vươn tới trúng khối chữ
        setTimeout(async () => {
            // Móng vuốt kẹp chặt
            this.clawPincers?.classList.add('is-closed');

            if (gem.isTarget) {
                // ==========================================
                // ĐÚNG CHỮ MỤC TIÊU: KÉO CHẬM RÃI VỀ ĐÍCH
                // ==========================================
                const retractDuration = 1200; // Tốc độ kéo chậm rãi, rõ ràng

                // Thu dây cáp về đỉnh
                if (this.craneCable) {
                    this.craneCable.style.transition = `height ${retractDuration}ms cubic-bezier(0.4, 0, 0.2, 1)`;
                    this.craneCable.style.height = `40px`;
                }
                if (this.clawHead) {
                    this.clawHead.style.transition = `top ${retractDuration}ms cubic-bezier(0.4, 0, 0.2, 1)`;
                    this.clawHead.style.top = `40px`;
                }

                // Khối chữ di chuyển theo móc về đỉnh đích (ngay dưới puli trần)
                const playfieldRect = this.playfield.getBoundingClientRect();
                const targetX = playfieldRect.width / 2 - gem.size / 2;
                const targetY = -15;

                gem.element.style.transition = `left ${retractDuration}ms cubic-bezier(0.4, 0, 0.2, 1), top ${retractDuration}ms cubic-bezier(0.4, 0, 0.2, 1), transform ${retractDuration}ms ease, opacity 0.3s ease ${retractDuration - 200}ms`;
                gem.element.style.left = `${targetX}px`;
                gem.element.style.top = `${targetY}px`;
                gem.element.style.transform = `scale(0.5) rotate(0deg)`;

                // Khi về tới đích trên trần
                setTimeout(async () => {
                    gem.isCollected = true;
                    gem.element.remove();

                    // BÙNG NỔ HẠT SAO VÀNG TẠI ĐÍCH ĐẾN
                    this.triggerHarvestBurst(playfieldRect.width / 2, 20);

                    // Âm thanh chúc mừng
                    window.gameAudio?.playCorrectChime();

                    // Mở móng vuốt
                    this.clawPincers?.classList.remove('is-closed');

                    // Tăng tiến độ
                    this.targetGemsCollected++;
                    this.updateProgressHUD();

                    // Voice chuẩn ngắn gọn từ TextToSpeechCaches: "Đúng rồi!"
                    await window.gameAudio?.playSystemVoiceOrSpeak("Đúng rồi!");

                    this.isClawBusy = false;

                    // Nếu gắp đủ 3/3 lần -> Chiến Thắng & Mở Màn Tiếp
                    if (this.targetGemsCollected >= this.totalRequired) {
                        this.showVictory();
                    }
                }, retractDuration);

            } else {
                // ==========================================
                // CHỌN SAI CHỮ KHÁC HOẶC KHO BÁU MỎ: RUNG LẮC & THU MÓC
                // ==========================================
                this.wrongAttempts++;

                // Khối chữ rung lắc cảnh báo nhẹ
                gem.element.style.transform = `scale(0.92) rotate(${gem.rot + 10}deg)`;
                window.gameAudio?.playWrongWobble();

                const retractDuration = 850;
                setTimeout(() => {
                    // Mở móng vuốt và thu dây về
                    this.clawPincers?.classList.remove('is-closed');
                    if (this.craneCable) {
                        this.craneCable.style.transition = `height ${retractDuration}ms ease`;
                        this.craneCable.style.height = '40px';
                    }
                    if (this.clawHead) {
                        this.clawHead.style.transition = `top ${retractDuration}ms ease`;
                        this.clawHead.style.top = '40px';
                    }
                }, 220);

                setTimeout(async () => {
                    // Chữ được rã đông tiếp tục bồng bềnh
                    gem.element.style.transform = `scale(1) rotate(${gem.rot}deg)`;
                    gem.isFrozen = false;
                    this.isClawBusy = false;
                }, retractDuration + 240);
            }
        }, extendDuration + 180);
    }

    /**
     * Hiệu ứng nổ pháo hoa hạt sao vàng khi khối chữ được kéo về đích
     */
    triggerHarvestBurst(x, y) {
        if (!this.playfield) return;
        const burst = document.createElement('div');
        burst.className = 'miner-harvest-burst';
        burst.style.left = `${x}px`;
        burst.style.top = `${y}px`;
        this.playfield.appendChild(burst);

        setTimeout(() => {
            burst.remove();
        }, 650);
    }

    /**
     * Cập nhật thanh tiến độ 3 ngôi sao
     */
    updateProgressHUD() {
        const countEl = document.getElementById('minerProgressCount');
        if (countEl) {
            countEl.textContent = `${this.targetGemsCollected} / ${this.totalRequired}`;
        }

        for (let i = 1; i <= this.totalRequired; i++) {
            const star = document.getElementById(`mstar-${i}`);
            if (star) {
                if (i <= this.targetGemsCollected) {
                    star.className = 'miner-star-slot filled';
                } else {
                    star.className = 'miner-star-slot empty';
                }
            }
        }
    }

    /**
     * Hiển thị bảng chúc mừng chiến thắng (Toast nhỏ gọn, tự tắt và qua màn)
     */
    showVictory() {
        this.isCompleted = true;
        this.stopPhysicsLoop();

        const target = this.currentLevelData?.target || 'A';
        const targetType = this.currentLevelData?.targetType === 'number' ? 'Chữ Số' : 'Chữ Cái';

        const msgEl = document.getElementById('victoryMessage');
        if (msgEl) {
            msgEl.innerHTML = `Bé đã xuất sắc gắp đủ <strong>3 khối ${targetType} ${target}</strong>!`;
        }

        const toastEl = document.getElementById('victoryToast');
        if (toastEl) {
            toastEl.style.opacity = '1';
            toastEl.style.transform = 'scale(1)';
        }

        if (this.victoryModal) {
            this.victoryModal.style.display = 'flex';
        }

        // Lưu thành tích hoàn thành màn chơi
        const levelNum = this.currentLevelData?.level || 1;
        this.shell?.saveProgressLocal('gold-miner', levelNum, 1);
        this.shell?.saveProgressServer('gold-miner', levelNum, 1);

        // Lưu tiếp level kế tiếp để mặc định lần sau vào sẽ chơi tiếp
        const nextLvl = levelNum + 1;
        this.saveCurrentLevelProgress(nextLvl);

        window.gameAudio?.playVictoryFanfare();
        // Voice ngắn gọn chuẩn từ hệ thống TextToSpeechCaches
        window.gameAudio?.playSystemVoiceOrSpeak("Bé giỏi quá!");

        // Tự động mờ dần popup và chuyển sang màn tiếp theo sau 2.2 giây
        setTimeout(() => {
            if (toastEl) {
                toastEl.style.transition = 'opacity 0.35s ease, transform 0.35s ease';
                toastEl.style.opacity = '0';
                toastEl.style.transform = 'scale(0.92)';
            }
            setTimeout(() => {
                if (this.victoryModal) {
                    this.victoryModal.style.display = 'none';
                }
                this.shell?.startLevel(nextLvl);
            }, 380);
        }, 2200);
    }

    /**
     * Lưu lại tiến độ level hiện tại vào localStorage
     */
    saveCurrentLevelProgress(nextLevel) {
        try {
            const raw = localStorage.getItem('htl1_games_progress') || '{}';
            const data = JSON.parse(raw);
            if (!data['gold-miner']) {
                data['gold-miner'] = { completedLevels: [], totalStars: 0, currentLevel: 1 };
            }
            data['gold-miner'].currentLevel = nextLevel;
            localStorage.setItem('htl1_games_progress', JSON.stringify(data));
        } catch (e) {}
    }

    /**
     * Mở Modal Bảng Chọn Chữ Cái & Chữ Số
     */
    openAlphabetModal() {
        if (this.alphabetModal) {
            this.alphabetModal.style.display = 'flex';
            this.renderAlphabetGrid('letters');
        }
    }

    closeAlphabetModal() {
        if (this.alphabetModal) {
            this.alphabetModal.style.display = 'none';
        }
    }

    /**
     * Render lưới chữ cái (29 chữ) hoặc chữ số (10 số) để bé chọn
     */
    renderAlphabetGrid(type = 'letters') {
        const grid = document.getElementById('alphabetGridContainer');
        const tabLetters = document.getElementById('tabLettersBtn');
        const tabNumbers = document.getElementById('tabNumbersBtn');
        if (!grid) return;

        tabLetters?.classList.toggle('active', type === 'letters');
        tabNumbers?.classList.toggle('active', type === 'numbers');

        grid.innerHTML = '';

        const allLevels = (window.GAME_LEVELS && window.GAME_LEVELS['gold-miner']) || [];
        const currentTarget = this.currentLevelData?.target || 'A';

        // Lấy danh sách level đã hoàn thành từ localStorage
        let completedLevels = [];
        try {
            const saved = JSON.parse(localStorage.getItem('htl1_games_progress') || '{}');
            completedLevels = saved['gold-miner']?.completedLevels || [];
        } catch (e) {}

        const filtered = allLevels.filter(lvl => {
            if (type === 'letters') return lvl.targetType === 'letter' || lvl.level <= 29;
            return lvl.targetType === 'number' || lvl.level > 29;
        });

        filtered.forEach(lvl => {
            const card = document.createElement('button');
            card.type = 'button';
            card.className = `alphabet-pick-card ${lvl.target === currentTarget ? 'active-target' : ''}`;
            
            const isDone = completedLevels.includes(lvl.level);
            card.innerHTML = `
                <span>${lvl.target}</span>
                ${isDone ? '<span class="star-badge">⭐</span>' : ''}
                <span style="font-size: 10px; color: rgba(255,255,255,0.7); margin-top: 2px;">Màn ${lvl.level}</span>
            `;

            card.addEventListener('click', () => {
                this.closeAlphabetModal();
                this.shell?.startLevel(lvl.level);
            });

            grid.appendChild(card);
        });
    }

    destroy() {
        this.isCompleted = true;
        this.stopPhysicsLoop();
        if (this.onResize) {
            window.removeEventListener('resize', this.onResize);
        }
        this.container.innerHTML = '';
    }
}

window.GoldMinerGame = GoldMinerGame;
