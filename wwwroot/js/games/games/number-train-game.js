/**
 * NumberTrainGame - Trò chơi Chuyến Tàu Chở Số Chuẩn Stitch 3D
 */
class NumberTrainGame {
    constructor(container, shell) {
        this.container = container;
        this.shell = shell;
        this.currentLevelData = null;
        this.isLocked = false;
        this.sortedOrder = [];
    }

    init(levelData) {
        this.currentLevelData = levelData;
        this.isLocked = false;
        this.sortedOrder = [];
        this.render();
    }

    render() {
        const type = this.currentLevelData.type;

        this.container.innerHTML = `
            <div style="position: relative; width: 100%; height: 100%; min-height: 480px; display: flex; flex-direction: column; justify-content: space-between; padding: 16px 24px; box-sizing: border-box;">
                <!-- Tuyến đường ray số học -->
                <div style="background: rgba(0, 0, 0, 0.45); backdrop-filter: blur(8px); border: 3px solid rgba(255,255,255,0.4); border-radius: 24px; padding: 14px 20px; z-index: 10;">
                    <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 10px;">
                        <span style="font-size: 0.88rem; font-weight: 800; color: #fef08a; display: flex; align-items: center; gap: 6px;">
                            🛤️ TUYẾN ĐƯỜNG RAY SỐ HỌC
                        </span>
                        <span style="background: #059669; color: #ffffff; font-size: 0.75rem; font-weight: 800; padding: 3px 12px; border-radius: 999px;">
                            Kéo hoặc Chạm để đặt toa
                        </span>
                    </div>

                    <div style="display: flex; align-items: flex-end; gap: 8px; overflow-x: auto; padding-bottom: 8px;" id="trainRowContainer">
                        <!-- Đầu tàu -->
                        <div style="width: 86px; height: 86px; border-radius: 18px 24px 8px 8px; background: linear-gradient(135deg, #f59e0b, #d97706); border: 3px solid #ffffff; display: flex; flex-direction: column; align-items: center; justify-content: center; color: #ffffff; box-shadow: 0 6px 16px rgba(0,0,0,0.3); flex-shrink: 0;">
                            <span style="font-size: 2.2rem;">🚂</span>
                            <span style="font-size: 0.68rem; font-weight: 900; text-transform: uppercase;">ĐẦU TÀU</span>
                        </div>
                        <div id="wagonsTrack" style="display: flex; gap: 8px;"></div>
                    </div>
                </div>

                <!-- Khay chọn toa tàu cho bé chuẩn Stitch -->
                <div style="background: linear-gradient(180deg, #d97706 0%, #b45309 100%); border: 4px solid #fde68a; border-radius: 26px; padding: 16px 20px; box-shadow: 0 12px 30px rgba(0,0,0,0.35); z-index: 15;">
                    <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 12px;">
                        <span style="font-family: 'Plus Jakarta Sans', sans-serif; font-size: 1rem; font-weight: 900; color: #ffffff; display: flex; align-items: center; gap: 6px;">
                            🗃️ KHAY CHỌN TOA TÀU CHO BÉ:
                        </span>
                        <div style="display: flex; gap: 8px;">
                            <button type="button" class="btn-toy-yellow" onclick="window.gameShell?.replayInstruction()" style="padding: 4px 12px; border-radius: 999px; font-size: 0.78rem;">
                                🔊 Đọc Số
                            </button>
                        </div>
                    </div>

                    <div style="display: flex; align-items: center; justify-content: space-between; gap: 16px; flex-wrap: wrap;">
                        <div id="trainChoicesRow" style="display: flex; gap: 14px; flex-wrap: wrap;"></div>
                        <div style="flex: 1 1 200px; display: flex; justify-content: flex-end;">
                            <div style="background: linear-gradient(135deg, #f97316, #ea580c); border: 2.5px solid #ffffff; border-radius: 20px; padding: 10px 18px; color: #ffffff; font-weight: 900; font-size: 0.95rem; display: flex; align-items: center; gap: 8px; box-shadow: 0 4px 12px rgba(234,88,12,0.4);">
                                <span>🚂</span>
                                <span>CHO TÀU CHẠY! 💨</span>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        `;

        if (type === 'missing-number' || type === 'number-neighbor') {
            this.renderMissingNumberMode();
        } else if (type === 'sort-numbers') {
            this.renderSortNumbersMode();
        } else if (type === 'quantity-counting') {
            this.renderQuantityCountingMode();
        }
    }

    renderMissingNumberMode() {
        const wagonsTrack = document.getElementById('wagonsTrack');
        const choicesRow = document.getElementById('trainChoicesRow');
        const seq = this.currentLevelData.trainSequence || [];
        const choices = this.currentLevelData.choices || [];

        wagonsTrack.innerHTML = '';
        seq.forEach((num, idx) => {
            const wagon = document.createElement('div');
            if (num === null) {
                wagon.id = 'missingWagonSlot';
                wagon.style.cssText = 'width: 78px; height: 86px; border-radius: 18px; border: 3px dashed #fde047; background: rgba(254, 240, 138, 0.25); color: #fde047; display: flex; flex-direction: column; align-items: center; justify-content: center; font-family: Fredoka, sans-serif; font-size: 2.2rem; font-weight: 900; box-shadow: 0 0 12px #fde047;';
                wagon.innerHTML = `<span>?</span><span style="font-size: 0.65rem; font-weight: 800; text-transform: uppercase;">Đặt Vào</span>`;
            } else {
                wagon.style.cssText = 'width: 78px; height: 86px; border-radius: 18px; border: 3px solid #ffffff; background: linear-gradient(135deg, #3b82f6, #1d4ed8); color: #ffffff; display: flex; flex-direction: column; align-items: center; justify-content: center; font-family: Fredoka, sans-serif; font-size: 2.2rem; font-weight: 900; box-shadow: 0 6px 16px rgba(0,0,0,0.3);';
                wagon.innerHTML = `<span>${num}</span><span style="font-size: 0.65rem; font-weight: 800; opacity: 0.9;">Toa ${num}</span>`;
            }
            wagonsTrack.appendChild(wagon);
        });

        choicesRow.innerHTML = '';
        const wagonColors = [
            'linear-gradient(135deg, #facc15, #eab308)',
            'linear-gradient(135deg, #34d399, #059669)',
            'linear-gradient(135deg, #a855f7, #7e22ce)',
            'linear-gradient(135deg, #f43f5e, #be123c)'
        ];

        choices.forEach((ch, idx) => {
            const btn = document.createElement('button');
            btn.type = 'button';
            btn.style.cssText = `width: 82px; height: 84px; border-radius: 20px; background: ${wagonColors[idx % wagonColors.length]}; border: 3.5px solid #ffffff; color: #ffffff; display: flex; flex-direction: column; align-items: center; justify-content: center; cursor: pointer; box-shadow: 0 6px 16px rgba(0,0,0,0.25); transition: all 0.15s ease;`;
            btn.innerHTML = `
                <span style="font-size: 0.68rem; font-weight: 900; text-transform: uppercase; background: rgba(0,0,0,0.2); padding: 2px 6px; border-radius: 6px; margin-bottom: 2px;">CHẠM TOA</span>
                <span style="font-family: 'Fredoka', sans-serif; font-size: 2.4rem; font-weight: 900; line-height: 1;">${ch}</span>
            `;

            btn.addEventListener('click', () => this.handleChoiceTap(ch, btn));
            choicesRow.appendChild(btn);
        });
    }

    handleChoiceTap(chosenNum, btnEl) {
        if (this.isLocked) return;

        const isCorrect = chosenNum === this.currentLevelData.answer;

        if (isCorrect) {
            this.isLocked = true;
            window.gameAudio.playCorrectChime();
            window.gameAudio.playTrainWhistle();

            const slot = document.getElementById('missingWagonSlot');
            if (slot) {
                slot.style.background = 'linear-gradient(135deg, #10b981, #059669)';
                slot.style.borderColor = '#ffffff';
                slot.innerHTML = `<span>${chosenNum}</span><span style="font-size: 0.65rem; font-weight: 800;">Toa ${chosenNum}</span>`;
                slot.style.animation = 'slotPop 0.35s cubic-bezier(0.34, 1.56, 0.64, 1)';
            }

            const trainRow = document.getElementById('trainRowContainer');
            if (trainRow) {
                trainRow.style.transition = 'transform 1.4s ease-in-out';
                trainRow.style.transform = 'translateX(180px)';
            }

            const praise = this.currentLevelData.praiseText || 'Bé nối đúng toa tàu rồi!';
            window.gameAudio.speak(praise, () => {
                setTimeout(() => {
                    this.shell.completeLevel(this.currentLevelData.rewardStars || 1);
                }, 400);
            });
        } else {
            window.gameAudio.playWrongWobble();
            btnEl.classList.add('shake-wrong');
            setTimeout(() => btnEl.classList.remove('shake-wrong'), 500);

            window.gameAudio.speak("Chưa đúng rồi. Bé thử lại nhé!", () => {
                window.gameAudio.speak(this.currentLevelData.instruction);
            });
        }
    }

    renderSortNumbersMode() {
        const wagonsTrack = document.getElementById('wagonsTrack');
        const choicesRow = document.getElementById('trainChoicesRow');
        const wagons = this.currentLevelData.wagons || [];
        const count = wagons.length;

        wagonsTrack.innerHTML = '';
        for (let i = 0; i < count; i++) {
            const wagon = document.createElement('div');
            wagon.id = `sortSlot-${i}`;
            wagon.style.cssText = 'width: 78px; height: 86px; border-radius: 18px; border: 3px dashed #fde047; background: rgba(254, 240, 138, 0.25); color: #fde047; display: flex; align-items: center; justify-content: center; font-family: Fredoka, sans-serif; font-size: 2.2rem; font-weight: 900;';
            wagon.textContent = '_';
            wagonsTrack.appendChild(wagon);
        }

        choicesRow.innerHTML = '';
        wagons.forEach(num => {
            const btn = document.createElement('button');
            btn.type = 'button';
            btn.style.cssText = 'width: 80px; height: 80px; border-radius: 20px; background: linear-gradient(135deg, #f59e0b, #d97706); border: 3px solid #ffffff; color: #ffffff; display: flex; flex-direction: column; align-items: center; justify-content: center; cursor: pointer; box-shadow: 0 6px 16px rgba(0,0,0,0.25);';
            btn.innerHTML = `<span style="font-family: 'Fredoka', sans-serif; font-size: 2.4rem; font-weight: 900;">${num}</span>`;
            btn.addEventListener('click', () => this.handleSortTap(num, btn));
            choicesRow.appendChild(btn);
        });
    }

    handleSortTap(num, btnEl) {
        if (this.isLocked) return;

        const nextIndex = this.sortedOrder.length;
        const expectedNum = this.currentLevelData.correctOrder[nextIndex];

        if (num === expectedNum) {
            window.gameAudio.playSnapSlot();
            this.sortedOrder.push(num);
            btnEl.style.visibility = 'hidden';

            const slot = document.getElementById(`sortSlot-${nextIndex}`);
            if (slot) {
                slot.style.background = 'linear-gradient(135deg, #3b82f6, #1d4ed8)';
                slot.style.borderColor = '#ffffff';
                slot.textContent = num;
            }

            if (this.sortedOrder.length === this.currentLevelData.correctOrder.length) {
                this.isLocked = true;
                window.gameAudio.playCorrectChime();
                window.gameAudio.playTrainWhistle();

                const trainRow = document.getElementById('trainRowContainer');
                if (trainRow) {
                    trainRow.style.transition = 'transform 1.4s ease-in-out';
                    trainRow.style.transform = 'translateX(180px)';
                }

                const praise = this.currentLevelData.praiseText || 'Bé xếp đúng thứ tự các toa rồi!';
                window.gameAudio.speak(praise, () => {
                    setTimeout(() => {
                        this.shell.completeLevel(this.currentLevelData.rewardStars || 2);
                    }, 400);
                });
            }
        } else {
            window.gameAudio.playWrongWobble();
            btnEl.classList.add('shake-wrong');
            setTimeout(() => btnEl.classList.remove('shake-wrong'), 500);
            window.gameAudio.speak("Chưa đúng số tiếp theo rồi. Bé thử lại nhé!");
        }
    }

    renderQuantityCountingMode() {
        const wagonsTrack = document.getElementById('wagonsTrack');
        const choicesRow = document.getElementById('trainChoicesRow');
        const itemsEmoji = this.currentLevelData.itemsEmoji || '🍎';
        const choices = this.currentLevelData.choices || [];

        wagonsTrack.innerHTML = `
            <div style="min-width: 140px; height: 86px; border-radius: 18px; border: 3px solid #ffffff; background: linear-gradient(135deg, #10b981, #059669); display: flex; align-items: center; justify-content: center; font-size: 2rem; padding: 0 16px; box-shadow: 0 6px 16px rgba(0,0,0,0.3); color: #ffffff;">
                ${itemsEmoji}
            </div>
        `;

        choicesRow.innerHTML = '';
        choices.forEach(ch => {
            const btn = document.createElement('button');
            btn.type = 'button';
            btn.style.cssText = 'width: 82px; height: 82px; border-radius: 20px; background: linear-gradient(135deg, #f59e0b, #d97706); border: 3.5px solid #ffffff; color: #ffffff; display: flex; align-items: center; justify-content: center; cursor: pointer; box-shadow: 0 6px 16px rgba(0,0,0,0.25); font-family: Fredoka, sans-serif; font-size: 2.4rem; font-weight: 900;';
            btn.textContent = ch;
            btn.addEventListener('click', () => this.handleChoiceTap(ch, btn));
            choicesRow.appendChild(btn);
        });
    }

    destroy() {
        this.isLocked = true;
        this.container.innerHTML = '';
    }
}

window.NumberTrainGame = NumberTrainGame;
