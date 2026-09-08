/**
 * GoldMinerGame - Vui Học Chữ (Jelly Gems 3D & Mechanical Claw)
 * Giao diện và trải nghiệm chuẩn mẫu đặc tả:
 * - 3D Jelly Gem blocks với vệt sáng thạch, biểu cảm ngộ nghĩnh
 * - Ngẫu nhiên hóa vị trí toàn bộ khối chữ và số mỗi lần tải trang / chơi lại
 * - 3 khối chữ mục tiêu được phân tán đều 3 vùng (Trái - Giữa - Phải), không chồng lấn
 * - Thanh tiến độ 3 ngôi sao sáng bừng màu vàng khi gắp đúng chữ
 * - Tích hợp toàn bộ kho voice từ hệ thống chung (GET /kids/games/voice?text=...)
 * - Click bất kỳ chữ hay số nào đều phát âm chuẩn từ hệ thống
 * - Bấm nút loa phát âm bài học tìm chữ cái
 * - Móc cẩu đung đưa cơ khí 3D phóng bắt và kéo về mượt mà
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
    }

    init(levelData) {
        this.currentLevelData = levelData;
        this.targetGemsCollected = 0;
        this.totalRequired = 3;
        this.wrongAttempts = 0;
        this.isCompleted = false;
        this.isClawBusy = false;

        this.render();
    }

    render() {
        const target = this.currentLevelData?.target || 'A';
        const targetType = this.currentLevelData?.targetType === 'number' ? 'Chữ Số' : 'Chữ Cái';

        this.container.innerHTML = `
            <div class="miner-stage-container" id="minerStage">
                <!-- Background Overlays & Ambient Sparkles -->
                <div class="absolute inset-0 pointer-events-none overflow-hidden z-0">
                    <div class="absolute inset-0" style="background: radial-gradient(circle at 50% 50%, rgba(15, 23, 42, 0.15) 0%, rgba(10, 15, 29, 0.65) 100%), linear-gradient(180deg, rgba(0, 0, 0, 0.45) 0%, rgba(0, 0, 0, 0.05) 50%, rgba(0, 0, 0, 0.5) 100%);"></div>
                    <div class="absolute bottom-28 left-1/4 w-3 h-3 bg-yellow-200 rounded-full blur-[1px] animate-sparkle"></div>
                    <div class="absolute bottom-36 right-1/4 w-2.5 h-2.5 bg-yellow-100 rounded-full blur-[1px] animate-sparkle" style="animation-delay: 0.6s;"></div>
                    <div class="absolute bottom-20 left-1/2 w-4 h-4 bg-amber-300 rounded-full blur-[2px] animate-sparkle" style="animation-delay: 1.2s;"></div>
                    <div class="absolute top-1/3 right-1/6 w-2 h-2 bg-yellow-300 rounded-full blur-[1px] animate-sparkle" style="animation-delay: 0.9s;"></div>
                </div>

                <!-- BEGIN: Top Compact Navigation & Mission Bar -->
                <header class="relative z-30 w-full px-6 py-3.5 flex items-center justify-between pointer-events-auto" id="minerTopNav">
                    <!-- Left: Back to Lobby & Level Tag -->
                    <div class="flex items-center gap-3">
                        <a href="/kids/games" class="group flex items-center gap-2 bg-gradient-to-b from-amber-500 to-amber-700 hover:from-amber-400 hover:to-amber-600 text-white font-black px-4 py-2 rounded-full border-2 border-yellow-200 shadow-lg shadow-amber-950/60 transform active:scale-95 transition" id="btnBackToLobby">
                            <span class="material-symbols-outlined" style="font-size: 1.25rem;">arrow_back</span>
                            <span class="text-sm uppercase tracking-wider text-3d-white font-fredoka" style="font-family: 'Fredoka', cursive;">VỀ SẢNH</span>
                        </a>
                        <span class="hidden sm:inline-flex items-center px-3.5 py-1 bg-black/60 backdrop-blur-md text-amber-200 text-xs font-bold rounded-full border border-amber-500/40">
                            Cấp độ: 5 Tuổi • Mầm Non
                        </span>
                    </div>

                    <!-- Center: Mission Target Badge with 3-Star Progress -->
                    <div class="flex items-center gap-3 bg-gradient-to-r from-amber-950/90 via-amber-900/95 to-amber-950/90 border-2 border-yellow-400/80 px-5 py-2 rounded-2xl shadow-xl shadow-yellow-950/70 backdrop-blur-md">
                        <!-- Speaker Button (Plays System Lesson Voice) -->
                        <button type="button" class="w-10 h-10 rounded-full bg-gradient-to-tr from-rose-500 to-amber-400 flex items-center justify-center shadow-md border border-white/60 hover:scale-110 active:scale-95 transition" id="btnMissionVoice" title="Nghe lại phát âm bài học">
                            <span class="material-symbols-outlined" style="font-size: 1.4rem; color: #ffffff;">volume_up</span>
                        </button>

                        <!-- Mission Text & Target Letter Display -->
                        <div class="flex items-center gap-2.5">
                            <div class="text-left leading-tight">
                                <span class="block text-[11px] font-extrabold uppercase text-amber-300 tracking-wider">Nhiệm Vụ Bé Gắp:</span>
                                <span class="text-xs text-stone-200 font-semibold">${targetType}</span>
                            </div>

                            <!-- Target Block Indicator -->
                            <div class="w-11 h-11 rounded-xl target-gem flex items-center justify-center font-black text-2xl text-white text-3d-gold animate-pulse-gentle font-fredoka" style="font-family: 'Fredoka', cursive; font-weight: 900;">
                                ${target}
                            </div>
                        </div>

                        <!-- Vertical Divider -->
                        <div class="w-0.5 h-8 bg-amber-600/60 mx-1"></div>

                        <!-- 3-Step Star Progress Indicator (Gắp 3 Lần) -->
                        <div class="flex flex-col items-center">
                            <div class="flex items-center gap-1.5" id="progressStarsContainer">
                                <div class="w-7 h-7 rounded-full bg-stone-800/80 border border-stone-600 flex items-center justify-center text-xs transition duration-300" id="star-1">
                                    <span class="text-stone-400">★</span>
                                </div>
                                <div class="w-7 h-7 rounded-full bg-stone-800/80 border border-stone-600 flex items-center justify-center text-xs transition duration-300" id="star-2">
                                    <span class="text-stone-400">★</span>
                                </div>
                                <div class="w-7 h-7 rounded-full bg-stone-800/80 border border-stone-600 flex items-center justify-center text-xs transition duration-300" id="star-3">
                                    <span class="text-stone-400">★</span>
                                </div>
                            </div>
                            <span class="text-[11px] font-bold text-yellow-300 mt-0.5" id="progressText">Tiến độ: 0/3</span>
                        </div>
                    </div>

                    <!-- Right: Audio Toggle -->
                    <div class="flex items-center gap-2">
                        <button type="button" aria-label="Bật tắt âm thanh" class="w-10 h-10 rounded-full bg-stone-800/90 hover:bg-stone-700 border border-amber-400/40 text-amber-200 flex items-center justify-center shadow hover:scale-105 active:scale-95 transition" id="btnAudioToggle">
                            <span class="material-symbols-outlined" style="font-size: 1.25rem;">volume_up</span>
                        </button>
                    </div>
                </header>
                <!-- END: Top Navigation -->

                <!-- BEGIN: Mechanical Claw Mount & Swinging Arm -->
                <div class="absolute top-14 left-1/2 -translate-x-1/2 z-20 pointer-events-none flex flex-col items-center" id="clawMechanism">
                    <!-- Ceiling Track Mount -->
                    <div class="w-16 h-5 bg-gradient-to-b from-stone-800 to-stone-900 border border-yellow-600 rounded-b-md shadow-lg flex items-center justify-center">
                        <div class="w-3 h-3 rounded-full bg-amber-400 shadow-inner"></div>
                    </div>

                    <!-- Swinging Cable & Claw Head -->
                    <div class="flex flex-col items-center animate-swing" id="craneArm" style="transform-origin: top center;">
                        <!-- Cable Line -->
                        <div class="w-1.5 h-20 bg-gradient-to-b from-stone-400 to-amber-200 shadow-sm transition-all duration-700 ease-out" id="craneWire"></div>
                        <!-- Metal Robot Claw Grabber -->
                        <div class="relative -mt-1 flex items-center justify-center transition-transform duration-300" id="clawHand">
                            <div class="w-7 h-7 bg-stone-700 rounded-full border-2 border-yellow-400 flex items-center justify-center shadow-lg">
                                <div class="w-2.5 h-2.5 rounded-full bg-amber-400"></div>
                            </div>
                            <!-- Left Pincer -->
                            <div class="absolute -bottom-4 -left-3 w-4 h-6 border-l-4 border-b-4 border-yellow-500 rounded-bl-xl rotate-12"></div>
                            <!-- Right Pincer -->
                            <div class="absolute -bottom-4 -right-3 w-4 h-6 border-r-4 border-b-4 border-yellow-500 rounded-br-xl -rotate-12"></div>
                        </div>
                    </div>
                </div>
                <!-- END: Mechanical Claw Mount -->

                <!-- BEGIN: Interactive Playfield with 3D Jelly Gems -->
                <div class="relative z-20 w-full flex-1 pointer-events-auto overflow-hidden" id="gemPlayfield" style="min-height: calc(100vh - 120px);"></div>
                <!-- END: Interactive Playfield -->

                <!-- BEGIN: Victory Celebration Modal Overlay -->
                <div class="fixed inset-0 z-50 bg-black/80 backdrop-blur-md flex items-center justify-center hidden" id="victoryModal">
                    <div class="bg-gradient-to-b from-amber-900 via-stone-900 to-stone-950 border-4 border-yellow-400 rounded-3xl p-8 max-w-md w-full mx-4 text-center shadow-2xl transform scale-95 transition-transform duration-300">
                        <!-- Trophy Icon -->
                        <div class="w-24 h-24 mx-auto mb-3 bg-gradient-to-tr from-yellow-500 to-amber-300 rounded-full flex items-center justify-center text-5xl shadow-lg border-2 border-yellow-100 animate-bounce">
                            🏆
                        </div>
                        <h2 class="text-3xl font-black text-yellow-300 uppercase tracking-wide drop-shadow font-fredoka" style="font-family: 'Fredoka', cursive;">BÉ GIỎI QUÁ!</h2>
                        <p class="text-stone-200 text-base font-semibold mt-2">
                            Bé đã xuất sắc nhận biết và gắp đủ <span class="text-yellow-400 font-bold text-lg">3 ${targetType} ${target}</span> lấp lánh!
                        </p>
                        <!-- 3 Golden Stars -->
                        <div class="flex justify-center gap-2 my-4 text-3xl text-yellow-400">
                            <span>⭐</span><span>⭐</span><span>⭐</span>
                        </div>
                        <div class="bg-amber-950/70 py-2.5 px-4 rounded-xl border border-yellow-500/40 my-3 text-sm text-amber-200 font-bold">
                            +100 Điểm Thưởng • Mở Khóa Chữ Tiếp Theo!
                        </div>
                        <!-- Action Button -->
                        <button type="button" class="w-full mt-3 bg-gradient-to-r from-amber-400 via-yellow-400 to-amber-500 text-stone-950 font-black py-3 rounded-2xl text-lg uppercase tracking-wider shadow-lg hover:brightness-110 active:scale-95 transition font-fredoka" id="btnPlayAgain" style="font-family: 'Fredoka', cursive;">
                            Chơi Tiếp Cùng Bé ➔
                        </button>
                    </div>
                </div>
                <!-- END: Victory Modal -->
            </div>
        `;

        this.playfield = document.getElementById('gemPlayfield');
        this.progressText = document.getElementById('progressText');
        this.victoryModal = document.getElementById('victoryModal');
        this.craneWire = document.getElementById('craneWire');
        this.craneArm = document.getElementById('craneArm');
        this.btnMissionVoice = document.getElementById('btnMissionVoice');
        this.btnAudioToggle = document.getElementById('btnAudioToggle');
        this.btnPlayAgain = document.getElementById('btnPlayAgain');

        // Gắn sự kiện nút nghe voice bài học
        if (this.btnMissionVoice) {
            this.btnMissionVoice.addEventListener('click', () => this.playLessonVoice());
        }

        // Gắn sự kiện nút âm thanh
        if (this.btnAudioToggle) {
            this.btnAudioToggle.addEventListener('click', () => {
                this.soundEnabled = !this.soundEnabled;
                window.gameAudio?.setSoundEnabled(this.soundEnabled);
                this.btnAudioToggle.innerHTML = `<span class="material-symbols-outlined" style="font-size: 1.25rem;">${this.soundEnabled ? 'volume_up' : 'volume_off'}</span>`;
            });
        }

        // Gắn sự kiện nút chơi tiếp / hoàn thành level
        if (this.btnPlayAgain) {
            this.btnPlayAgain.addEventListener('click', () => {
                this.victoryModal?.classList.add('hidden');
                this.shell?.completeLevel(this.currentLevelData?.rewardStars || 1);
            });
        }

        // Sinh vị trí ngẫu nhiên không trùng lặp cho toàn bộ các khối
        this.spawnRandomizedGems();

        // Tự động phát âm bài học khi mở màn chơi
        setTimeout(() => {
            this.playLessonVoice();
        }, 500);

        // Hỗ trợ resize mượt mà
        window.addEventListener('resize', this.onResize = () => {
            if (!this.isCompleted && this.targetGemsCollected < this.totalRequired) {
                this.spawnRandomizedGems();
            }
        });
    }

    /**
     * Phát âm bài học tìm chữ từ hệ thống chung
     */
    async playLessonVoice() {
        const target = this.currentLevelData?.target || 'A';
        const isNumber = this.currentLevelData?.targetType === 'number';
        const prefix = isNumber ? 'Số' : 'Chữ';

        // 1. Thử gọi voice bài học trong hệ thống
        const lessonQuery = `${prefix} ${target}. Bé hãy tìm và gắp các khối ${prefix.toLowerCase()} ${target} màu vàng nhé!`;
        const phonic = window.gameAudio?.getPhonicText(target) || `${prefix} ${target}`;

        // Thử tìm voice câu hỏi cụ thể, nếu chưa có thì đọc phát âm chữ cái
        const promptSpeech = `Con hãy tìm và gắp khối ${phonic} nhé!`;
        await window.gameAudio?.playSystemVoiceOrSpeak(promptSpeech);
    }

    /**
     * Sinh các khối chữ và số ngẫu nhiên không trùng lặp, chia 3 vùng an toàn
     */
    spawnRandomizedGems() {
        if (!this.playfield) return;
        this.playfield.innerHTML = '';

        const target = this.currentLevelData?.target || 'A';
        const isNumber = this.currentLevelData?.targetType === 'number';

        // Danh sách ngọc: Chính xác 3 khối mục tiêu 'A' + các khối chữ & số gây nhiễu
        const gemDataList = [
            // 3 Khối Mục Tiêu Vàng Óng
            { id: 'target-1', char: target, isTarget: true, type: 'target', size: 96, spark: '✨' },
            { id: 'target-2', char: target, isTarget: true, type: 'target', size: 104, spark: '🌟' },
            { id: 'target-3', char: target, isTarget: true, type: 'target', size: 96, spark: '💛' }
        ];

        // Danh sách các chữ và số phân tán phong phú
        const alphabetDistractors = ['B', 'C', 'D', 'E', 'G', 'H', 'M', 'N', 'O', 'P', 'T', 'U', '1', '2', '5', '8']
            .filter(c => c !== target);
        
        // Trộn ngẫu nhiên
        for (let i = alphabetDistractors.length - 1; i > 0; i--) {
            const j = Math.floor(Math.random() * (i + 1));
            [alphabetDistractors[i], alphabetDistractors[j]] = [alphabetDistractors[j], alphabetDistractors[i]];
        }

        const faces = ['◕ ‿ ◕', '^ ‿ ^', '• ‿ •', '◕ ᴗ ◕', '^ ᴗ ^', '^ ◡ ^', '• ⩊ •'];
        const types = ['blue', 'green', 'ruby', 'cyan', 'purple', 'orange', 'purple', 'blue'];

        // Chọn 8 khối chữ và số gây nhiễu
        alphabetDistractors.slice(0, 8).forEach((char, idx) => {
            gemDataList.push({
                id: `gem-${idx}-${char}`,
                char: char,
                isTarget: false,
                type: types[idx % types.length],
                size: Math.floor(78 + Math.random() * 8),
                face: faces[idx % faces.length]
            });
        });

        // Kích thước vùng chơi
        const fieldWidth = this.playfield.clientWidth || window.innerWidth || 1100;
        const fieldHeight = this.playfield.clientHeight || (window.innerHeight - 120) || 520;

        const paddingX = 40;
        const minX = paddingX;
        const maxX = fieldWidth - paddingX;
        const minY = 80; // Tránh cần cẩu đỉnh hang
        const maxY = fieldHeight - 60;

        const placedGems = [];
        const minSpacing = 16;

        function checkOverlap(x, y, radius) {
            for (const item of placedGems) {
                const dx = (x + radius) - (item.x + item.radius);
                const dy = (y + radius) - (item.y + item.radius);
                const dist = Math.sqrt(dx * dx + dy * dy);
                if (dist < (radius + item.radius + minSpacing)) {
                    return true;
                }
            }
            return false;
        }

        // Chia 3 khối mục tiêu vào 3 vùng (Trái, Giữa, Phải) để phân bổ đẹp mắt
        const targetSlices = [
            { minX: minX + 20, maxX: minX + (maxX - minX) * 0.32 },
            { minX: minX + (maxX - minX) * 0.36, maxX: minX + (maxX - minX) * 0.64 },
            { minX: minX + (maxX - minX) * 0.68, maxX: maxX - 20 }
        ];
        targetSlices.sort(() => Math.random() - 0.5);

        let targetIndex = 0;

        gemDataList.forEach((gemData) => {
            const size = gemData.size;
            const radius = size / 2;
            let x = 0;
            let y = 0;
            let placed = false;
            let attempts = 0;
            const maxAttempts = 250;

            let sliceMinX = minX;
            let sliceMaxX = maxX - size;

            if (gemData.isTarget && targetIndex < targetSlices.length) {
                const slice = targetSlices[targetIndex++];
                sliceMinX = Math.max(minX, slice.minX);
                sliceMaxX = Math.min(maxX - size, slice.maxX - size);
            }

            while (!placed && attempts < maxAttempts) {
                attempts++;
                x = sliceMinX + Math.random() * Math.max(10, (sliceMaxX - sliceMinX));
                y = minY + Math.random() * Math.max(10, (maxY - minY - size));

                if (!checkOverlap(x, y, radius)) {
                    placed = true;
                }
            }

            if (!placed) {
                x = minX + Math.random() * Math.max(10, (maxX - minX - size));
                y = minY + Math.random() * Math.max(10, (maxY - minY - size));
            }

            placedGems.push({ x, y, radius });

            // Góc nghiêng ngẫu nhiên (-8 độ đến +8 độ)
            const rotationDeg = (Math.random() * 16 - 8).toFixed(1);
            const floatDelay = (Math.random() * 2).toFixed(2);

            const btn = document.createElement('button');
            btn.type = 'button';
            btn.setAttribute('data-id', gemData.id);
            btn.setAttribute('data-gem', gemData.char);
            btn.setAttribute('data-target', gemData.isTarget ? 'true' : 'false');

            let colorClass = `gem-${gemData.type}`;
            if (gemData.type === 'target') colorClass = 'target-gem';

            btn.className = `gem-block ${colorClass} group flex flex-col items-center justify-center animate-pulse-gentle`;
            btn.style.width = `${size}px`;
            btn.style.height = `${size}px`;
            btn.style.left = `${Math.round(x)}px`;
            btn.style.top = `${Math.round(y)}px`;
            btn.style.setProperty('--rot', `${rotationDeg}deg`);
            btn.style.transform = `rotate(${rotationDeg}deg)`;
            btn.style.animationDelay = `${floatDelay}s`;
            btn.style.fontFamily = "'Fredoka', cursive, sans-serif";

            // Vệt sáng gương 3D & Biểu cảm ngộ nghĩnh
            let innerContent = `<div class="jelly-highlight"></div>`;

            if (gemData.isTarget) {
                const fontSize = size >= 100 ? 'text-6xl' : 'text-5xl';
                innerContent += `
                    <span class="${fontSize} font-black text-white text-3d-gold tracking-tight group-hover:scale-110 transition z-10 leading-none" style="font-family: 'Fredoka', cursive;">${gemData.char}</span>
                    <div class="flex items-center gap-1 -mt-0.5 z-10 opacity-90">
                        <span class="w-2 h-1 bg-amber-900 rounded-full inline-block"></span>
                        <span class="text-[11px] text-amber-950 font-bold">‿</span>
                        <span class="w-2 h-1 bg-amber-900 rounded-full inline-block"></span>
                    </div>
                    <span class="absolute -top-2.5 -right-2 text-xl animate-sparkle pointer-events-none">${gemData.spark}</span>
                    <span class="absolute -bottom-1 -left-1.5 text-base animate-pulse pointer-events-none">✨</span>
                `;
            } else {
                innerContent += `
                    <span class="text-3xl font-black text-white text-3d-white z-10 group-hover:scale-110 transition leading-none" style="font-family: 'Fredoka', cursive;">${gemData.char}</span>
                    <span class="text-[10px] text-stone-900/80 font-bold -mt-0.5 z-10">${gemData.face}</span>
                `;
            }

            btn.innerHTML = innerContent;

            // Xử lý Click gắp vào chữ
            btn.addEventListener('click', (e) => {
                e.stopPropagation();
                this.handleGemClick(btn, gemData);
            });

            this.playfield.appendChild(btn);
        });
    }

    /**
     * Xử lý khi bé click vào bất kỳ khối nào
     */
    async handleGemClick(gemElement, gemData) {
        if (this.isClawBusy || this.isCompleted) return;
        this.isClawBusy = true;

        // Dừng đung đưa tạm thời để hạ cẩu
        if (this.craneArm) {
            this.craneArm.classList.remove('animate-swing');
        }

        // Phóng dây cáp
        if (this.craneWire) {
            this.craneWire.style.height = '140px';
        }

        // ==============================================================
        // YÊU CẦU: "click chữ nào cũng có voice, click số cũng có voice đều đã có trong hệ thống hãy dùng lại"
        // Phát âm chữ hoặc số đó ngay lập tức từ kho voice hệ thống chung!
        // ==============================================================
        window.gameAudio?.speakLetter(gemData.char);

        if (gemData.isTarget) {
            // === GẮP ĐÚNG KHỐI MỤC TIÊU ===
            gemElement.classList.add('collected');

            // Hiệu ứng kéo khối lên cẩu
            gemElement.style.transition = 'all 0.55s cubic-bezier(0.4, 0, 0.2, 1)';
            gemElement.style.transform = 'translateY(-140px) scale(1.35)';
            gemElement.style.opacity = '0';
            gemElement.style.pointerEvents = 'none';

            // Tăng tiến độ
            this.targetGemsCollected++;
            this.updateProgressHUD();

            // Âm thanh chúc mừng từ hệ thống
            window.gameAudio?.playCorrectChime();

            setTimeout(async () => {
                // Thu dây cáp về
                if (this.craneWire) this.craneWire.style.height = '80px';
                if (this.craneArm) this.craneArm.classList.add('animate-swing');
                this.isClawBusy = false;

                // Voice khen đúng
                const correctText = `Đúng rồi! ${window.gameAudio?.getPhonicText(gemData.char) || gemData.char}!`;
                await window.gameAudio?.playSystemVoiceOrSpeak(correctText);

                // Nếu đã đủ 3/3 lần gắp -> CHIẾN THẮNG
                if (this.targetGemsCollected >= this.totalRequired) {
                    this.showVictory();
                }
            }, 550);

        } else {
            // === CHỌN SAI KHỐI KHÁC ===
            this.wrongAttempts++;

            // Hiệu ứng rung lắc nhẹ nhàng
            gemElement.style.transform = 'scale(0.88)';
            setTimeout(() => {
                const rot = gemElement.style.getPropertyValue('--rot') || '0deg';
                gemElement.style.transform = `rotate(${rot}) scale(1)`;
            }, 220);

            setTimeout(async () => {
                // Thu dây cáp về
                if (this.craneWire) this.craneWire.style.height = '80px';
                if (this.craneArm) this.craneArm.classList.add('animate-swing');
                this.isClawBusy = false;

                window.gameAudio?.playWrongWobble();

                // Voice nhắc nhở nhẹ nhàng từ hệ thống
                const target = this.currentLevelData?.target || 'A';
                const wrongText = `Đây là ${window.gameAudio?.getPhonicText(gemData.char) || gemData.char}. Bé hãy tìm chữ ${target} màu vàng nhé!`;
                await window.gameAudio?.playSystemVoiceOrSpeak(wrongText);

                // Nếu sai từ 2 lần trở lên: nhắc lại câu hỏi và hào quang gợi ý
                if (this.wrongAttempts >= 2) {
                    setTimeout(() => {
                        this.playLessonVoice();
                        // Hiệu ứng nhấp nháy ở các khối đúng
                        this.playfield?.querySelectorAll('[data-target="true"]').forEach(el => {
                            el.classList.add('ring-4', 'ring-yellow-300', 'ring-offset-2');
                        });
                    }, 800);
                }
            }, 450);
        }
    }

    /**
     * Cập nhật thanh tiến độ 3 ngôi sao sáng bừng màu vàng khi gắp đúng
     */
    updateProgressHUD() {
        if (this.progressText) {
            this.progressText.textContent = `Tiến độ: ${this.targetGemsCollected}/${this.totalRequired}`;
        }

        for (let i = 1; i <= this.totalRequired; i++) {
            const star = document.getElementById(`star-${i}`);
            if (star) {
                if (i <= this.targetGemsCollected) {
                    // SÁNG BỪNG MÀU VÀNG KHI CLICK ĐÚNG
                    star.className = 'w-7 h-7 rounded-full bg-amber-400 border-2 border-yellow-200 text-amber-950 flex items-center justify-center text-xs transition duration-300 scale-110 shadow-lg shadow-amber-500/50';
                    star.innerHTML = `<span class="text-stone-900 font-black text-sm">★</span>`;
                } else {
                    star.className = 'w-7 h-7 rounded-full bg-stone-800/80 border border-stone-600 flex items-center justify-center text-xs transition duration-300';
                    star.innerHTML = `<span class="text-stone-400">★</span>`;
                }
            }
        }
    }

    /**
     * Hiển thị bảng chúc mừng chiến thắng sau khi hoàn thành 3/3
     */
    showVictory() {
        this.isCompleted = true;
        if (this.victoryModal) {
            this.victoryModal.classList.remove('hidden');
        }

        window.gameAudio?.playVictoryFanfare();
        const target = this.currentLevelData?.target || 'A';
        const winSpeech = `Bé giỏi quá! Chúc mừng bé đã xuất sắc tìm và gắp đủ 3 chữ ${target} lấp lánh!`;
        window.gameAudio?.playSystemVoiceOrSpeak(winSpeech);
    }

    destroy() {
        this.isCompleted = true;
        if (this.onResize) {
            window.removeEventListener('resize', this.onResize);
        }
        this.container.innerHTML = '';
    }
}

window.GoldMinerGame = GoldMinerGame;
