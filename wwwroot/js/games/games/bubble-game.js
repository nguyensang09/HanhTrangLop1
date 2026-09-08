/**
 * BubbleGame - Trò chơi Bắn Bong Bóng Chữ Cái Chuẩn Stitch 3D
 */
class BubbleGame {
    constructor(container, shell) {
        this.container = container;
        this.shell = shell;
        this.currentLevelData = null;
        this.isLocked = false;
    }

    init(levelData) {
        this.currentLevelData = levelData;
        this.isLocked = false;
        this.render();
    }

    render() {
        const targetChar = this.currentLevelData.target || 'A';

        this.container.innerHTML = `
            <div style="position: relative; width: 100%; height: 100%; min-height: 480px; display: flex; align-items: center; justify-content: space-between;">
                <!-- Bottom-Left: Loaded Bubble Ammo Tray -->
                <div style="position: absolute; bottom: 20px; left: 20px; z-index: 15; background: rgba(15,23,42,0.7); backdrop-filter: blur(10px); border: 2.5px solid #fde047; border-radius: 28px; padding: 12px 18px; display: flex; align-items: center; gap: 16px; box-shadow: 0 10px 25px rgba(0,0,0,0.35);">
                    <div style="display: flex; flex-direction: column; align-items: center;">
                        <span style="font-size: 0.7rem; font-weight: 900; color: #fef08a; text-transform: uppercase; margin-bottom: 4px;">ĐẠN SẴN SÀNG</span>
                        <div style="width: 58px; height: 58px; border-radius: 50%; background: radial-gradient(circle at 35% 30%, #ffffff, #f43f5e); border: 3px solid #ffffff; display: flex; align-items: center; justify-content: center; font-family: 'Fredoka', sans-serif; font-size: 1.8rem; font-weight: 900; color: #ffffff; box-shadow: 0 4px 14px rgba(244,63,94,0.5);">
                            ${targetChar}
                        </div>
                    </div>
                    <div style="display: flex; flex-direction: column; gap: 4px;">
                        <span style="font-size: 0.8rem; font-weight: 800; color: #fde047;">Đường ngắm ➔</span>
                        <div style="display: flex; gap: 4px; align-items: center;">
                            <span style="width: 6px; height: 6px; border-radius: 50%; background: #fef08a;"></span>
                            <span style="width: 8px; height: 8px; border-radius: 50%; background: #fde047;"></span>
                            <span style="width: 10px; height: 10px; border-radius: 50%; background: #f59e0b;"></span>
                            <span style="width: 12px; height: 12px; border-radius: 50%; background: #f43f5e;"></span>
                        </div>
                        <span style="font-size: 0.72rem; color: rgba(255,255,255,0.8);">Chạm vào bóng để bắn!</span>
                    </div>
                </div>

                <!-- Right / Center: Floating Bubble Cluster -->
                <div id="bubbleClusterArea" style="position: relative; width: 100%; height: 100%; min-height: 480px;"></div>
            </div>
        `;

        const cluster = document.getElementById('bubbleClusterArea');
        const choices = this.currentLevelData.choices || [];
        const stageW = cluster.clientWidth || 700;
        const stageH = cluster.clientHeight || 480;

        const positions = this.generatePositions(choices.length, stageW, stageH);

        choices.forEach((char, idx) => {
            const isTarget = char.toLowerCase() === targetChar.toLowerCase();
            const bubbleEl = document.createElement('div');
            bubbleEl.className = `bubble-sphere ${isTarget ? 'target-bubble' : ''}`;
            bubbleEl.style.position = 'absolute';
            bubbleEl.style.left = `${positions[idx].x}px`;
            bubbleEl.style.top = `${positions[idx].y}px`;
            bubbleEl.style.width = isTarget ? '100px' : '88px';
            bubbleEl.style.height = isTarget ? '100px' : '88px';
            bubbleEl.style.borderRadius = '50%';
            bubbleEl.style.display = 'flex';
            bubbleEl.style.alignItems = 'center';
            bubbleEl.style.justifyContent = 'center';
            bubbleEl.style.fontFamily = "'Fredoka', 'Plus Jakarta Sans', sans-serif";
            bubbleEl.style.fontSize = isTarget ? '2.8rem' : '2.4rem';
            bubbleEl.style.fontWeight = '900';
            bubbleEl.style.color = '#ffffff';
            bubbleEl.textContent = char;

            // Màu sắc radial rực rỡ theo Stitch 3D
            const colorGradients = [
                'radial-gradient(circle at 35% 30%, #ffffff 0%, #fda4af 30%, #f43f5e 80%)',
                'radial-gradient(circle at 35% 30%, #ffffff 0%, #fde047 30%, #f59e0b 80%)',
                'radial-gradient(circle at 35% 30%, #ffffff 0%, #6ee7b7 30%, #10b981 80%)',
                'radial-gradient(circle at 35% 30%, #ffffff 0%, #93c5fd 30%, #3b82f6 80%)'
            ];
            bubbleEl.style.background = isTarget ? colorGradients[0] : colorGradients[(idx + 1) % colorGradients.length];

            bubbleEl.addEventListener('click', (e) => {
                e.stopPropagation();
                this.handleBubbleClick(bubbleEl, char);
            });

            bubbleEl.addEventListener('touchstart', (e) => {
                e.stopPropagation();
                this.handleBubbleClick(bubbleEl, char);
            }, { passive: true });

            cluster.appendChild(bubbleEl);
        });
    }

    generatePositions(count, width, height) {
        const positions = [];
        const startX = width * 0.42;
        const availW = width * 0.52;

        if (count === 2) {
            positions.push({ x: startX + availW * 0.2, y: height * 0.35 });
            positions.push({ x: startX + availW * 0.7, y: height * 0.55 });
        } else if (count === 3) {
            positions.push({ x: startX + availW * 0.15, y: height * 0.3 });
            positions.push({ x: startX + availW * 0.5, y: height * 0.6 });
            positions.push({ x: startX + availW * 0.8, y: height * 0.35 });
        } else {
            positions.push({ x: startX + availW * 0.15, y: height * 0.25 });
            positions.push({ x: startX + availW * 0.65, y: height * 0.2 });
            positions.push({ x: startX + availW * 0.35, y: height * 0.6 });
            positions.push({ x: startX + availW * 0.8, y: height * 0.55 });
        }
        return positions;
    }

    handleBubbleClick(bubbleEl, chosenChar) {
        if (this.isLocked) return;

        const isCorrect = chosenChar.toLowerCase() === this.currentLevelData.target.toLowerCase();

        if (isCorrect) {
            this.isLocked = true;
            window.gameAudio.playBubblePop();
            window.gameAudio.playCorrectChime();
            window.gameAudio.playStarTing();

            // Nổ tung bong bóng hạt sao lấp lánh
            bubbleEl.style.transform = 'scale(1.5)';
            bubbleEl.style.opacity = '0';
            bubbleEl.style.filter = 'drop-shadow(0 0 35px #fef08a)';

            const burst = document.createElement('div');
            burst.style.position = 'absolute';
            burst.style.left = bubbleEl.style.left;
            burst.style.top = bubbleEl.style.top;
            burst.style.fontSize = '3.5rem';
            burst.style.pointerEvents = 'none';
            burst.style.animation = 'popFade 0.5s forwards ease';
            burst.innerHTML = '💥✨⭐';
            this.container.appendChild(burst);

            const praise = this.currentLevelData.praiseText || `Giỏi quá! Đây là chữ ${this.currentLevelData.target}.`;
            window.gameAudio.speak(praise, () => {
                setTimeout(() => {
                    this.shell.completeLevel(this.currentLevelData.rewardStars || 1);
                }, 400);
            });
        } else {
            window.gameAudio.playWrongWobble();
            bubbleEl.classList.add('shake-wrong');
            setTimeout(() => bubbleEl.classList.remove('shake-wrong'), 500);

            window.gameAudio.speak("Chưa đúng rồi. Bé thử lại nhé!", () => {
                window.gameAudio.speak(this.currentLevelData.instruction);
            });
        }
    }

    destroy() {
        this.isLocked = true;
        this.container.innerHTML = '';
    }
}

window.BubbleGame = BubbleGame;
