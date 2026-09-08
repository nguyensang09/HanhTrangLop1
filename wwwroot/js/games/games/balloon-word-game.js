/**
 * BalloonWordGame - Trò chơi Khinh Khí Cầu Ghép Vần Chuẩn Stitch 3D
 */
class BalloonWordGame {
    constructor(container, shell) {
        this.container = container;
        this.shell = shell;
        this.currentLevelData = null;
        this.filledSlots = [];
        this.availableBalloons = [];
        this.isLocked = false;
    }

    init(levelData) {
        this.currentLevelData = levelData;
        this.isLocked = false;
        const slotsCount = (levelData.slots || []).length;
        this.filledSlots = new Array(slotsCount).fill(null);
        this.availableBalloons = [...(levelData.balloons || [])];
        this.render();
    }

    render() {
        const hasImage = !!this.currentLevelData.imageEmoji;

        this.container.innerHTML = `
            <div style="position: relative; width: 100%; height: 100%; min-height: 480px; display: flex; flex-direction: column; justify-content: space-between; padding: 20px 24px; box-sizing: border-box;">
                <!-- Hàng khinh khí cầu 3D bay trên bầu trời -->
                <div id="balloonsSky" style="display: flex; justify-content: center; gap: 16px; flex-wrap: wrap; padding-top: 10px; z-index: 10;"></div>

                <!-- Kệ ghép vần bằng gỗ chuẩn Stitch -->
                <div style="background: linear-gradient(180deg, #92400e 0%, #78350f 100%); border: 4px solid #fde68a; border-radius: 28px; padding: 18px 28px; box-shadow: 0 12px 30px rgba(0,0,0,0.3), inset 0 2px 0 #fed7aa; z-index: 15; display: flex; flex-direction: column; align-items: center; gap: 12px;">
                    <div style="display: flex; justify-content: space-between; width: 100%; align-items: center;">
                        <span style="font-family: 'Plus Jakarta Sans', sans-serif; font-size: 1.05rem; font-weight: 900; color: #fef08a; display: flex; align-items: center; gap: 6px;">
                            🪵 KỆ GHÉP VẦN BẰNG GỖ
                        </span>
                        <span style="background: rgba(0,0,0,0.35); color: #fef08a; font-size: 0.78rem; font-weight: 800; padding: 4px 12px; border-radius: 999px;">
                            Chạm chữ cái trên khinh khí cầu để xếp vào ô
                        </span>
                    </div>

                    <div style="display: flex; align-items: center; gap: 16px;">
                        <div id="slotsContainer" style="display: flex; gap: 14px;"></div>
                        ${hasImage ? `
                            <div style="display: flex; align-items: center; gap: 8px; background: rgba(255,255,255,0.92); padding: 8px 18px; border-radius: 20px; border: 2.5px solid #fde68a; box-shadow: 0 4px 12px rgba(0,0,0,0.15);">
                                <span style="font-size: 2.4rem;">${this.currentLevelData.imageEmoji}</span>
                                <span style="font-family: 'Fredoka', sans-serif; font-size: 1.4rem; font-weight: 900; color: #78350f;">${this.currentLevelData.imageDesc || ''}</span>
                            </div>
                        ` : ''}
                    </div>
                </div>
            </div>
        `;

        this.renderBalloons();
        this.renderSlots();
    }

    renderBalloons() {
        const sky = document.getElementById('balloonsSky');
        if (!sky) return;
        sky.innerHTML = '';

        const colors = [
            'radial-gradient(circle at 35% 30%, #ffffff 0%, #fda4af 30%, #f43f5e 80%)',
            'radial-gradient(circle at 35% 30%, #ffffff 0%, #fde047 30%, #f59e0b 80%)',
            'radial-gradient(circle at 35% 30%, #ffffff 0%, #6ee7b7 30%, #10b981 80%)',
            'radial-gradient(circle at 35% 30%, #ffffff 0%, #93c5fd 30%, #3b82f6 80%)',
            'radial-gradient(circle at 35% 30%, #ffffff 0%, #d8b4fe 30%, #a855f7 80%)'
        ];

        this.availableBalloons.forEach((char, index) => {
            const isChosen = char === null;
            const displayChar = isChosen ? '✓' : char;

            const isTone = ['´', '`', '?', '~', '.'].includes(char);
            const balloonEl = document.createElement('div');
            balloonEl.className = 'hot-air-balloon';
            balloonEl.style.opacity = isChosen ? '0.4' : '1';
            balloonEl.style.pointerEvents = isChosen ? 'none' : 'auto';

            const bgGrad = isTone ? 'radial-gradient(circle at 35% 30%, #ffffff 0%, #6ee7b7 30%, #059669 80%)' : colors[index % colors.length];

            balloonEl.innerHTML = `
                <div style="width: 78px; height: 88px; border-radius: 50% 50% 45% 45%; background: ${bgGrad}; border: 3.5px solid #ffffff; display: flex; align-items: center; justify-content: center; font-family: 'Fredoka', sans-serif; font-size: 2.3rem; font-weight: 900; color: #ffffff; box-shadow: 0 8px 24px rgba(0,0,0,0.25), inset 0 2px 6px rgba(255,255,255,0.8);">
                    ${displayChar}
                </div>
                <div style="width: 28px; height: 18px; background: #b45309; border-radius: 4px; border: 2px solid #78350f; margin-top: 4px; position: relative;"></div>
                <span style="font-size: 0.72rem; font-weight: 800; color: #1e293b; background: #ffffff; border: 1.5px solid #fde047; padding: 2px 8px; border-radius: 8px; margin-top: 4px;">
                    ${isChosen ? 'Đã chọn' : (isTone ? 'Dấu thanh' : `Chữ ${char}`)}
                </span>
            `;

            balloonEl.addEventListener('click', () => {
                this.handleBalloonSelect(char, index);
            });

            sky.appendChild(balloonEl);
        });
    }

    renderSlots() {
        const container = document.getElementById('slotsContainer');
        if (!container) return;
        container.innerHTML = '';

        this.filledSlots.forEach((char, slotIndex) => {
            const slotEl = document.createElement('div');
            slotEl.style.width = '78px';
            slotEl.style.height = '78px';
            slotEl.style.borderRadius = '20px';
            slotEl.style.border = char ? '3.5px solid #fde047' : '3.5px dashed #fde68a';
            slotEl.style.background = char ? '#ffffff' : 'rgba(255, 255, 255, 0.25)';
            slotEl.style.display = 'flex';
            slotEl.style.alignItems = 'center';
            slotEl.style.justifyContent = 'center';
            slotEl.style.fontFamily = "'Fredoka', 'Plus Jakarta Sans', sans-serif";
            slotEl.style.fontSize = '2.4rem';
            slotEl.style.fontWeight = '900';
            slotEl.style.color = '#78350f';
            slotEl.style.boxShadow = char ? '0 6px 16px rgba(0,0,0,0.2)' : 'none';
            slotEl.style.cursor = char ? 'pointer' : 'default';
            slotEl.textContent = char || '_';

            if (char) {
                slotEl.title = 'Bấm để gỡ chữ ra';
                slotEl.addEventListener('click', () => {
                    this.handleSlotRemove(slotIndex);
                });
            }

            container.appendChild(slotEl);
        });
    }

    handleBalloonSelect(char, balloonIndex) {
        if (this.isLocked || !char) return;

        const emptyIdx = this.filledSlots.findIndex(s => s === null);
        if (emptyIdx !== -1) {
            this.fillSlot(emptyIdx, char, balloonIndex);
        }
    }

    fillSlot(slotIndex, char, balloonIndex) {
        if (this.isLocked) return;

        if (this.filledSlots[slotIndex]) {
            const oldChar = this.filledSlots[slotIndex];
            this.returnCharToSky(oldChar);
        }

        this.filledSlots[slotIndex] = char;
        this.availableBalloons[balloonIndex] = null;
        window.gameAudio.playSnapSlot();

        this.renderBalloons();
        this.renderSlots();

        if (this.filledSlots.every(s => s !== null)) {
            this.checkAnswer();
        }
    }

    handleSlotRemove(slotIndex) {
        if (this.isLocked) return;
        const char = this.filledSlots[slotIndex];
        if (!char) return;

        this.filledSlots[slotIndex] = null;
        this.returnCharToSky(char);
        window.gameAudio.playSnapSlot();

        this.renderBalloons();
        this.renderSlots();
    }

    returnCharToSky(char) {
        const nullIdx = this.availableBalloons.findIndex(b => b === null);
        if (nullIdx !== -1) {
            this.availableBalloons[nullIdx] = char;
        } else {
            this.availableBalloons.push(char);
        }
    }

    checkAnswer() {
        const formed = this.filledSlots.join('');
        const targetSlots = (this.currentLevelData.slots || []).join('');
        const isCorrect = formed.toLowerCase() === targetSlots.toLowerCase();

        if (isCorrect) {
            this.isLocked = true;
            window.gameAudio.playCorrectChime();
            window.gameAudio.playStarTing();

            const praise = this.currentLevelData.praiseText || `Bé ghép đúng rồi: ${this.currentLevelData.targetWord}!`;
            window.gameAudio.speak(praise, () => {
                setTimeout(() => {
                    this.shell.completeLevel(this.currentLevelData.rewardStars || 2);
                }, 400);
            });
        } else {
            window.gameAudio.playWrongWobble();
            const container = document.getElementById('slotsContainer');
            if (container) {
                container.classList.add('shake-wrong');
                setTimeout(() => container.classList.remove('shake-wrong'), 500);
            }

            window.gameAudio.speak("Chưa đúng rồi. Bé bấm vào chữ để đổi lại nhé!");
        }
    }

    destroy() {
        this.isLocked = true;
        this.container.innerHTML = '';
    }
}

window.BalloonWordGame = BalloonWordGame;
