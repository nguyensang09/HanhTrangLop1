/**
 * Bilingual Listen Screen - Interactive English-Vietnamese Flashcard & Audio Engine
 * HanhTrangLop1
 *
 * Nguyên tắc âm thanh: MỘT giọng tại một thời điểm.
 * - Chỉ dùng một thẻ Audio duy nhất (tái sử dụng) cho mọi đoạn đọc.
 * - Mỗi lần bắt đầu phát mới đều dừng hẳn đoạn trước (kể cả chuỗi đang chờ) bằng "playToken".
 * - Nếu máy chủ không có tệp âm thanh, dùng speechSynthesis với giọng ĐÚNG ngôn ngữ yêu cầu
 *   (không có giọng phù hợp thì im lặng, không đọc tiếng Việt bằng giọng Anh và ngược lại).
 * - Rời trang / ẩn trang thì dừng mọi âm thanh.
 */
(function () {
  "use strict";

  // Cache DOM elements
  const screen = document.querySelector(".bilingual-listen-screen");
  if (!screen) return;

  const childId = screen.dataset.childId || "default";
  const allCards = Array.from(document.querySelectorAll("[data-listen-card]"));
  if (!allCards.length) return;

  // Dialog & elements
  const dialog = document.getElementById("flashcardDialog");
  const dialogCloseBtn = document.getElementById("dialogCloseBtn");
  const dialogPrevBtn = document.getElementById("dialogPrevBtn");
  const dialogNextBtn = document.getElementById("dialogNextBtn");
  const dialogImage = document.getElementById("dialogImage");
  const dialogEmoji = document.getElementById("dialogEmoji");
  const dialogCategoryBadge = document.getElementById("dialogCategoryBadge");
  const dialogSymbolTitle = document.getElementById("dialogSymbolTitle");
  const dialogWord = document.getElementById("dialogWord");
  const dialogPhonetic = document.getElementById("dialogPhonetic");
  const dialogMeaning = document.getElementById("dialogMeaning");
  const dialogExampleEn = document.getElementById("dialogExampleEn");
  const dialogExampleVi = document.getElementById("dialogExampleVi");
  const dialogSpeakBilingualBtn = document.getElementById("dialogSpeakBilingualBtn");
  const dialogSpeakSlowBtn = document.getElementById("dialogSpeakSlowBtn");
  const dialogProgressIndicator = document.getElementById("dialogProgressIndicator");
  if (!dialog) return;

  // Topbar & Toolbar elements
  const counterText = document.getElementById("counterText");
  const counterPill = document.getElementById("bilingualCounterPill");
  const autoplayTourBtn = document.getElementById("autoplayTourBtn");
  const speedBtns = document.querySelectorAll(".speed-toggle-btn");
  const tabBtns = document.querySelectorAll(".bilingual-tab-btn");
  const readModeBtns = document.querySelectorAll(".read-mode-btn");
  const dialogSpeakModeTitle = document.getElementById("dialogSpeakModeTitle");
  const dialogSpeakModeSub = document.getElementById("dialogSpeakModeSub");
  const sections = document.querySelectorAll("[data-section]");

  // State
  let currentCardIndex = 0;
  let visibleCards = [...allCards];
  let currentSpeed = "normal"; // 'normal' | 'slow'
  let readMode = "en_only"; // 'en_only' (mặc định: A - Apple) | 'bilingual' (đầy đủ Anh - Việt)
  try {
    const savedMode = localStorage.getItem("hanhtrang_bilingual_read_mode");
    if (savedMode === "bilingual" || savedMode === "en_only") {
      readMode = savedMode;
    }
  } catch (e) {}

  let isAutoplaying = false;
  let autoplayTimer = null;
  let closeTimer = null;

  function updateReadModeUI() {
    readModeBtns.forEach((btn) => {
      const isMatch = btn.dataset.mode === readMode;
      btn.classList.toggle("active", isMatch);
      btn.setAttribute("aria-pressed", isMatch ? "true" : "false");
    });

    if (dialogSpeakModeTitle) {
      dialogSpeakModeTitle.textContent = readMode === "en_only" ? "Chỉ Tiếng Anh" : "Nghe Song Ngữ";
    }
    if (dialogSpeakModeSub) {
      dialogSpeakModeSub.textContent = readMode === "en_only" ? "Chữ ➔ Từ ví dụ (A - Apple)" : "Chữ ➔ Từ ➔ Câu ví dụ";
    }
  }

  function updateSpeedUI() {
    speedBtns.forEach((b) => {
      const isMatch = b.dataset.speed === currentSpeed;
      b.classList.toggle("active", isMatch);
      b.setAttribute("aria-pressed", isMatch ? "true" : "false");
    });
  }

  // LocalStorage for explored cards
  const STORAGE_KEY = "hanhtrang_bilingual_explored_" + childId;
  let exploredSet = new Set();
  try {
    const saved = localStorage.getItem(STORAGE_KEY);
    if (saved) {
      exploredSet = new Set(JSON.parse(saved));
    }
  } catch (e) {
    exploredSet = new Set();
  }

  // Update initial explored indicators
  function refreshExploredStatus() {
    allCards.forEach((card) => {
      const key = card.dataset.kind + "_" + card.dataset.symbol;
      card.classList.toggle("has-explored", exploredSet.has(key));
    });

    if (counterText) {
      counterText.textContent = exploredSet.size + "/" + allCards.length;
    }
  }

  function markCardAsExplored(card) {
    if (!card) return;
    const key = card.dataset.kind + "_" + card.dataset.symbol;
    if (!exploredSet.has(key)) {
      exploredSet.add(key);
      try {
        localStorage.setItem(STORAGE_KEY, JSON.stringify(Array.from(exploredSet)));
      } catch (e) {}
      refreshExploredStatus();

      // Fun micro animation
      if (counterPill) {
        counterPill.classList.remove("pulse-bounce");
        void counterPill.offsetWidth;
        counterPill.classList.add("pulse-bounce");
        setTimeout(() => counterPill.classList.remove("pulse-bounce"), 800);
      }
    }
  }

  // ===================== GIỌNG ĐỌC DỰ PHÒNG (speechSynthesis) =====================
  let voices = [];
  function populateVoices() {
    if (!window.speechSynthesis) return;
    try {
      voices = window.speechSynthesis.getVoices() || [];
    } catch (e) {
      voices = [];
    }
  }
  if (window.speechSynthesis) {
    populateVoices();
    if (typeof window.speechSynthesis.addEventListener === "function") {
      window.speechSynthesis.addEventListener("voiceschanged", populateVoices);
    } else {
      window.speechSynthesis.onvoiceschanged = populateVoices;
    }
  }

  // Chỉ trả về giọng ĐÚNG ngôn ngữ yêu cầu; ưu tiên giọng nữ (Hoài My / Jenny / Aria / Zira / Samantha)
  function findVoice(langPrefix) {
    if (!voices.length) populateVoices();
    const lp = String(langPrefix || "").toLowerCase();
    if (!lp) return null;
    const matching = voices.filter((v) => {
      const vl = String(v.lang || "").toLowerCase().replace("_", "-");
      return vl === lp || vl.startsWith(lp + "-");
    });
    if (!matching.length) return null;

    const isMale = (name) => {
      const n = String(name || "").toLowerCase();
      return (
        n.includes("namminh") ||
        n.includes(" an ") ||
        n.endsWith(" an") ||
        n.includes("guy") ||
        n.includes("david") ||
        n.includes("mark") ||
        n.includes("george") ||
        n.includes("christopher") ||
        (n.includes("male") && !n.includes("female"))
      );
    };

    const isFemale = (name) => {
      const n = String(name || "").toLowerCase();
      return (
        n.includes("hoaimy") ||
        n.includes("linh") ||
        n.includes("jenny") ||
        n.includes("aria") ||
        n.includes("zira") ||
        n.includes("female") ||
        n.includes("woman") ||
        n.includes("girl") ||
        n.includes("samantha") ||
        n.includes("victoria") ||
        n.includes("karen")
      );
    };

    // 1. Tìm giọng nữ rõ ràng
    const femaleVoice = matching.find((v) => isFemale(v.name));
    if (femaleVoice) return femaleVoice;

    // 2. Nếu không ghi rõ, tìm giọng không phải nam
    const nonMale = matching.find((v) => !isMale(v.name));
    if (nonMale) return nonMale;

    return matching[0] || null;
  }

  // ===================== BỘ PHÁT DUY NHẤT =====================
  const player = new Audio();
  player.preload = "auto";
  player.setAttribute("playsinline", "");
  player.setAttribute("webkit-playsinline", "");

  let playToken = 0; // tăng mỗi lần dừng → mọi chuỗi cũ tự huỷ
  let finishPlayback = null; // resolve của đoạn đang phát qua thẻ Audio
  let currentClipSrc = "";
  let clipGuard = null;
  let speechResolve = null; // resolve của đoạn đang đọc bằng speechSynthesis
  let speechGuard = null;
  let gapResolve = null; // resolve của khoảng nghỉ giữa các đoạn
  let sequenceTimer = null;

  function settleCurrentClip() {
    clearTimeout(clipGuard);
    clipGuard = null;
    const done = finishPlayback;
    finishPlayback = null;
    currentClipSrc = "";
    document.body.classList.remove("audio-is-speaking");
    if (done) done();
  }

  function settleSpeech() {
    clearTimeout(speechGuard);
    speechGuard = null;
    const done = speechResolve;
    speechResolve = null;
    document.body.classList.remove("audio-is-speaking");
    if (done) done();
  }

  player.addEventListener("ended", () => {
    if (finishPlayback && player.src === currentClipSrc) settleCurrentClip();
  });
  player.addEventListener("error", () => {
    if (finishPlayback && player.src === currentClipSrc) settleCurrentClip();
  });

  // iOS Safari: "mở khoá" thẻ Audio duy nhất ngay trong lần chạm đầu tiên của bé,
  // để các lần play() sau (sau khi chờ máy chủ trả đường dẫn) không bị chặn.
  const SILENT_WAV =
    "data:audio/wav;base64,UklGRrQBAABXQVZFZm10IBAAAAABAAEAQB8AAEAfAAABAAgAZGF0YZABAACAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA";
  let playerUnlocked = false;
  function unlockPlayer() {
    if (playerUnlocked) return;
    playerUnlocked = true;
    if (finishPlayback) return;
    try {
      player.src = SILENT_WAV;
      const p = player.play();
      if (p && typeof p.catch === "function") p.catch(() => {});
    } catch (e) {}
  }
  document.addEventListener("touchend", unlockPlayer, { capture: true, passive: true });
  document.addEventListener("click", unlockPlayer, true);

  function clearPlayingIndicators() {
    document.querySelectorAll(".card-audio-btn.is-playing").forEach((el) => el.classList.remove("is-playing"));
    if (dialogSpeakBilingualBtn) dialogSpeakBilingualBtn.classList.remove("is-playing");
    if (dialogSpeakSlowBtn) dialogSpeakSlowBtn.classList.remove("is-playing");
    document.body.classList.remove("audio-is-speaking");
  }

  // Dừng MỌI âm thanh và huỷ mọi chuỗi đang chờ
  function stopAllAudio() {
    playToken++;
    clearTimeout(sequenceTimer);
    sequenceTimer = null;
    if (gapResolve) {
      const g = gapResolve;
      gapResolve = null;
      g();
    }
    try {
      player.pause();
    } catch (e) {}
    settleCurrentClip();
    if (window.speechSynthesis) {
      try {
        window.speechSynthesis.cancel();
      } catch (e) {}
    }
    settleSpeech();
    clearPlayingIndicators();
  }

  // Audio URL cache (chống duplicate requests)
  const audioCache = new Map();
  const pendingRequests = new Map();

  // Lấy (và cache) đường dẫn tệp âm thanh từ máy chủ
  async function getAudioUrl(text, lang = "vi", isSlow = false) {
    if (!text) return null;
    const rate = isSlow ? (lang === "vi" ? "-25%" : "-28%") : (lang === "vi" ? "-10%" : "-15%");
    const cacheKey = `${lang}:${rate}:${text}`;

    if (audioCache.has(cacheKey)) {
      return audioCache.get(cacheKey);
    }

    if (pendingRequests.has(cacheKey)) {
      return pendingRequests.get(cacheKey);
    }

    const requestPromise = (async () => {
      try {
        const apiUrl = `/kids/bilingual-audio?text=${encodeURIComponent(text)}&lang=${encodeURIComponent(lang)}&rate=${encodeURIComponent(rate)}`;
        const response = await fetch(apiUrl);
        if (response.ok) {
          const data = await response.json();
          if (data.success && data.audioUrl) {
            audioCache.set(cacheKey, data.audioUrl);
            return data.audioUrl;
          }
        } else if (response.status === 404) {
          // Máy chủ chắc chắn không có tệp này → nhớ lại để lần sau đọc dự phòng ngay
          audioCache.set(cacheKey, null);
        }
      } catch (e) {
        console.warn("Lỗi tải âm thanh từ server:", e);
      } finally {
        pendingRequests.delete(cacheKey);
      }
      return null;
    })();

    pendingRequests.set(cacheKey, requestPromise);
    return requestPromise;
  }

  // Trợ lý xác định tên chữ cái / chữ số để đọc riêng hoặc đọc đầu tiên
  function getLetterPrompt(card) {
    if (!card) return { vi: "", en: "", label: "Đọc chữ" };
    const kind = card.dataset.kind; // 'letter' | 'number'
    const symbol = card.dataset.symbol || "";
    if (kind === "letter") {
      return {
        vi: `Chữ ${symbol}`,
        en: symbol,
        label: `Đọc riêng chữ ${symbol}`
      };
    }
    return {
      vi: `Số ${symbol}`,
      en: symbol,
      label: `Đọc riêng số ${symbol}`
    };
  }

  function getBilingualSequence(card) {
    const prompt = getLetterPrompt(card);
    const symbol = card?.dataset.symbol || "";
    const word = card?.dataset.word || symbol;
    const isNumber = card?.dataset.kind === "number";
    return {
      en1: prompt.en,
      vi1: prompt.vi,
      en2: isNumber ? `Number ${word}` : word,
      vi2: isNumber ? symbol : (card?.dataset.meaning || ""),
      en3: card?.dataset.exampleEn || "",
      vi3: card?.dataset.exampleVi || ""
    };
  }

  // Tải trước đường dẫn âm thanh của 1 thẻ (chỉ hỏi máy chủ, không phát)
  function preloadCardAudio(card) {
    if (!card) return;
    const sequence = getBilingualSequence(card);

    if (sequence.en1) getAudioUrl(sequence.en1, "en");
    if (sequence.vi1) getAudioUrl(sequence.vi1, "vi");
    if (sequence.en2) {
      getAudioUrl(sequence.en2, "en");
      getAudioUrl(sequence.en2, "en", true);
    }
    if (sequence.vi2) getAudioUrl(sequence.vi2, "vi");
    if (sequence.en3) getAudioUrl(sequence.en3, "en");
    if (sequence.vi3) getAudioUrl(sequence.vi3, "vi");
  }

  // Phát một tệp qua thẻ Audio duy nhất; resolve khi xong / lỗi / bị dừng
  function playAudioUrlPromise(url, playbackRate, token) {
    return new Promise((resolve) => {
      if (!url || token !== playToken) {
        resolve();
        return;
      }

      try {
        player.pause();
      } catch (e) {}
      settleCurrentClip();

      finishPlayback = resolve;
      // Lưới an toàn: nếu trình duyệt không bắn sự kiện "ended", không treo chuỗi mãi
      clipGuard = setTimeout(() => {
        if (finishPlayback === resolve) {
          try {
            player.pause();
          } catch (e) {}
          settleCurrentClip();
        }
      }, 20000);

      try {
        player.src = url;
        currentClipSrc = player.src;
        player.defaultPlaybackRate = playbackRate;
        player.playbackRate = playbackRate;
        document.body.classList.add("audio-is-speaking");
        const p = player.play();
        if (p && typeof p.catch === "function") {
          p.catch(() => {
            if (finishPlayback === resolve) settleCurrentClip();
          });
        }
      } catch (e) {
        if (finishPlayback === resolve) settleCurrentClip();
      }
    });
  }

  // Đọc dự phòng bằng speechSynthesis — chỉ với giọng đúng ngôn ngữ, huỷ mọi câu trước khi đọc
  function speakWithWebSpeechPromise(text, lang, isSlow, token) {
    return new Promise((resolve) => {
      const synth = window.speechSynthesis;
      if (!text || !synth || typeof window.SpeechSynthesisUtterance !== "function" || token !== playToken) {
        resolve();
        return;
      }

      const voice = findVoice(lang);
      if (!voice) {
        // Không có giọng đúng ngôn ngữ → im lặng còn hơn đọc sai giọng
        resolve();
        return;
      }

      try {
        synth.cancel();
      } catch (e) {}
      settleSpeech();

      const utterance = new SpeechSynthesisUtterance(text);
      utterance.voice = voice;
      utterance.lang = voice.lang;
      utterance.rate = isSlow ? 0.7 : 0.9;
      utterance.pitch = 1.05;

      speechResolve = resolve;
      const done = () => {
        if (speechResolve === resolve) settleSpeech();
      };
      utterance.onend = done;
      utterance.onerror = done;
      speechGuard = setTimeout(done, Math.max(3000, text.length * 180));

      document.body.classList.add("audio-is-speaking");
      try {
        synth.speak(utterance);
      } catch (e) {
        done();
      }
    });
  }

  // Phát một phân đoạn: ưu tiên tệp của máy chủ, quá lâu / không có thì đọc dự phòng
  async function playVoiceItemPromise(text, lang = "vi", isSlow = false, token = playToken) {
    if (!text || token !== playToken) return;
    const playbackRate = isSlow ? 0.72 : 1.0;
    let raceTimer = null;
    const audioUrl = await Promise.race([
      getAudioUrl(text, lang, isSlow),
      new Promise((resolve) => {
        raceTimer = setTimeout(() => resolve(null), 1500);
      })
    ]);
    clearTimeout(raceTimer);

    if (token !== playToken) return;

    if (audioUrl) {
      await playAudioUrlPromise(audioUrl, playbackRate, token);
    } else {
      await speakWithWebSpeechPromise(text, lang, isSlow, token);
    }
  }

  function waitGapPromise(ms) {
    return new Promise((resolve) => {
      gapResolve = resolve;
      sequenceTimer = setTimeout(() => {
        if (gapResolve === resolve) gapResolve = null;
        resolve();
      }, ms);
    });
  }

  // Chuỗi phát âm:
  // - Nếu readMode === 'en_only' (mặc định):
  //   Chỉ đọc Tiếng Anh theo cấu trúc chữ - ví dụ cho chữ (Ví dụ: A - Apple / 1 - One) rồi dừng lại.
  // - Nếu readMode === 'bilingual':
  //   Đọc cấu trúc song ngữ đầy đủ (En - Vn - En - Vn - En - Vn).
  async function playIntegratedBilingualSequence(card, isSlow = false, onEndCallback) {
    stopAllAudio();
    const token = playToken;
    if (!card) {
      if (onEndCallback) onEndCallback();
      return;
    }

    const sequence = getBilingualSequence(card);
    const letterEn = sequence.en1;
    const letterVi = sequence.vi1;
    // Chữ cái: 'Apple', Chữ số: 'One' (thay vì 'Number One' nếu đọc theo kiểu Chữ - Ví dụ: 1 - One)
    const wordEn = (readMode === "en_only" && card.dataset.kind === "number")
      ? (card.dataset.word || sequence.en2)
      : sequence.en2;
    const meaningVi = sequence.vi2;
    const exampleEn = sequence.en3;
    const exampleVi = sequence.vi3;

    const steps = readMode === "en_only"
      ? [[letterEn, "en"], [wordEn, "en"]]
      : [[letterEn, "en"], [letterVi, "vi"], [wordEn, "en"], [meaningVi, "vi"], [exampleEn, "en"], [exampleVi, "vi"]];
    const playable = steps.filter((s) => !!s[0]);

    // Hỏi trước đường dẫn của mọi đoạn (song song) để đọc liền mạch
    playable.forEach((s) => getAudioUrl(s[0], s[1], isSlow));

    if (dialogSpeakBilingualBtn) dialogSpeakBilingualBtn.classList.add("is-playing");
    const audioBtn = card.querySelector(".card-audio-btn");
    if (audioBtn) audioBtn.classList.add("is-playing");

    markCardAsExplored(card);

    let completed = false;
    try {
      for (let i = 0; i < playable.length; i++) {
        if (i > 0) {
          // Dừng 1 nhịp (300ms) để chuyển giọng
          await waitGapPromise(300);
        }
        if (token !== playToken) return;
        await playVoiceItemPromise(playable[i][0], playable[i][1], isSlow, token);
        if (token !== playToken) return;
      }
      completed = true;
    } finally {
      // Chỉ dọn giao diện nếu chuỗi này vẫn là chuỗi hiện hành (không bị chuỗi mới thay thế)
      if (token === playToken) {
        if (dialogSpeakBilingualBtn) dialogSpeakBilingualBtn.classList.remove("is-playing");
        if (audioBtn) audioBtn.classList.remove("is-playing");
        if (completed && onEndCallback) onEndCallback();
      }
    }
  }

  function playCardSequence(card, isSlow, onEndCallback) {
    playIntegratedBilingualSequence(card, isSlow, onEndCallback);
  }

  // Nút Nghe chậm: đọc chậm một từ tiếng Anh (dừng mọi âm thanh khác trước)
  async function fetchAndPlayVoice(text, lang = "vi", isSlow = false, onEndCallback) {
    stopAllAudio();
    const token = playToken;
    if (!text) {
      if (onEndCallback) onEndCallback();
      return;
    }

    if (dialogSpeakSlowBtn) dialogSpeakSlowBtn.classList.add("is-playing");
    try {
      await playVoiceItemPromise(text, lang, isSlow, token);
    } finally {
      if (token === playToken) {
        if (dialogSpeakSlowBtn) dialogSpeakSlowBtn.classList.remove("is-playing");
        if (onEndCallback) onEndCallback();
      }
    }
  }

  // ===================== HỘP THẺ TO =====================
  function showDialog() {
    clearTimeout(closeTimer);
    closeTimer = null;
    dialog.hidden = false;
    requestAnimationFrame(() => {
      dialog.classList.add("is-active");
    });
  }

  function openFlashcardDialog(card) {
    if (!card) return;
    stopAutoplayTour();

    const idx = visibleCards.indexOf(card);
    if (idx !== -1) {
      currentCardIndex = idx;
    }

    updateDialogWithCard(card);
    showDialog();

    // Speak card content
    playCardSequence(card, currentSpeed === "slow");
  }

  function closeFlashcardDialog() {
    stopAllAudio();
    stopAutoplayTour();
    dialog.classList.remove("is-active");
    clearTimeout(closeTimer);
    closeTimer = setTimeout(() => {
      closeTimer = null;
      dialog.hidden = true;
    }, 240);
  }

  function updateDialogWithCard(card) {
    if (!card) return;

    const kind = card.dataset.kind;
    const symbol = card.dataset.symbol || "";
    const word = card.dataset.word || "";
    const phonetic = card.dataset.phonetic || "";
    const meaning = card.dataset.meaning || "";
    const emoji = card.dataset.emoji || "✨";
    const image = card.dataset.image || "";
    const exampleEn = card.dataset.exampleEn || "";
    const exampleVi = card.dataset.exampleVi || "";

    if (dialogSymbolTitle) dialogSymbolTitle.textContent = symbol;
    if (dialogWord) dialogWord.textContent = word;
    if (dialogPhonetic) dialogPhonetic.textContent = phonetic ? "/" + phonetic + "/" : "";
    if (dialogMeaning) dialogMeaning.textContent = meaning;
    if (dialogEmoji) dialogEmoji.textContent = emoji;
    if (dialogExampleEn) dialogExampleEn.textContent = exampleEn;
    if (dialogExampleVi) dialogExampleVi.textContent = exampleVi;

    if (dialogCategoryBadge) {
      dialogCategoryBadge.textContent = kind === "letter" ? `Chữ cái ${symbol}` : `Chữ số ${symbol}`;
    }

    if (dialogImage) {
      if (image) {
        dialogImage.src = image;
        dialogImage.alt = `${symbol} - ${word}`;
        dialogImage.style.display = "block";
      } else {
        dialogImage.style.display = "none";
      }
    }

    // Progress in visible cards
    const currentPos = visibleCards.indexOf(card) + 1;
    if (dialogProgressIndicator) {
      dialogProgressIndicator.textContent = `Thẻ ${currentPos} / ${visibleCards.length}`;
    }

    // Nav button state
    if (dialogPrevBtn) dialogPrevBtn.disabled = visibleCards.length <= 1;
    if (dialogNextBtn) dialogNextBtn.disabled = visibleCards.length <= 1;

    // Tải trước đường dẫn âm thanh thẻ hiện tại và 2 thẻ liền kề
    preloadCardAudio(card);
    const currIdx = visibleCards.indexOf(card);
    if (currIdx !== -1) {
      if (visibleCards[currIdx + 1]) preloadCardAudio(visibleCards[currIdx + 1]);
      if (visibleCards[currIdx - 1]) preloadCardAudio(visibleCards[currIdx - 1]);
    }
  }

  function navigateDialog(direction) {
    if (!visibleCards.length) return;
    stopAutoplayTour();
    currentCardIndex = (currentCardIndex + direction + visibleCards.length) % visibleCards.length;
    const nextCard = visibleCards[currentCardIndex];
    updateDialogWithCard(nextCard);
    playCardSequence(nextCard, currentSpeed === "slow");
  }

  // ===================== TỰ ĐỘNG PHÁT =====================
  // Thẻ sau chỉ bắt đầu khi thẻ trước đã đọc xong (không bao giờ chồng tiếng)
  function startAutoplayTour() {
    if (!visibleCards.length) return;
    isAutoplaying = true;
    if (currentCardIndex >= visibleCards.length) currentCardIndex = 0;
    if (autoplayTourBtn) {
      autoplayTourBtn.classList.add("active", "is-touring");
      autoplayTourBtn.setAttribute("aria-pressed", "true");
      const label = autoplayTourBtn.querySelector(".tour-label");
      const icon = autoplayTourBtn.querySelector(".tour-icon");
      if (label) label.textContent = "Dừng tự phát";
      if (icon) icon.textContent = "pause_circle";
    }

    function playStep() {
      if (!isAutoplaying) return;
      const card = visibleCards[currentCardIndex];
      if (!card) {
        stopAutoplayTour();
        return;
      }
      updateDialogWithCard(card);
      showDialog();

      playCardSequence(card, currentSpeed === "slow", () => {
        if (!isAutoplaying) return;
        clearTimeout(autoplayTimer);
        autoplayTimer = setTimeout(() => {
          if (!isAutoplaying) return;
          currentCardIndex = (currentCardIndex + 1) % visibleCards.length;
          playStep();
        }, readMode === "en_only" ? 1000 : 1400);
      });
    }

    playStep();
  }

  function stopAutoplayTour() {
    clearTimeout(autoplayTimer);
    autoplayTimer = null;
    if (!isAutoplaying) return;
    isAutoplaying = false;
    if (autoplayTourBtn) {
      autoplayTourBtn.classList.remove("active", "is-touring");
      autoplayTourBtn.setAttribute("aria-pressed", "false");
      const label = autoplayTourBtn.querySelector(".tour-label");
      const icon = autoplayTourBtn.querySelector(".tour-icon");
      if (label) label.textContent = "Tự động phát";
      if (icon) icon.textContent = "play_circle";
    }
  }

  // Tab Filtering
  function filterCards() {
    const activeTabBtn = document.querySelector(".bilingual-tab-btn.active");
    const activeTab = activeTabBtn ? activeTabBtn.dataset.tab : "all";

    allCards.forEach((card) => {
      const kind = card.dataset.kind; // 'letter' | 'number'

      let matchTab = true;
      if (activeTab === "letters" && kind !== "letter") matchTab = false;
      if (activeTab === "numbers" && kind !== "number") matchTab = false;

      card.style.display = matchTab ? "" : "none";
    });

    // Update section visibility
    sections.forEach((sec) => {
      const visibleInSection = Array.from(sec.querySelectorAll("[data-listen-card]")).filter((c) => c.style.display !== "none").length;
      sec.style.display = visibleInSection > 0 ? "" : "none";
    });

    // Update visibleCards array
    visibleCards = allCards.filter((card) => card.style.display !== "none");
    if (currentCardIndex >= visibleCards.length) currentCardIndex = 0;
  }

  // ===================== SỰ KIỆN =====================
  // 1. Cards click & hover preload
  allCards.forEach((card) => {
    card.addEventListener("mouseenter", () => preloadCardAudio(card), { once: true });
    card.addEventListener("click", () => {
      openFlashcardDialog(card);
    });
  });

  // 2. Dialog actions
  if (dialogCloseBtn) {
    dialogCloseBtn.addEventListener("click", closeFlashcardDialog);
  }

  if (dialogPrevBtn) {
    dialogPrevBtn.addEventListener("click", () => navigateDialog(-1));
  }

  if (dialogNextBtn) {
    dialogNextBtn.addEventListener("click", () => navigateDialog(1));
  }

  dialog.addEventListener("click", (e) => {
    if (e.target === dialog) {
      closeFlashcardDialog();
    }
  });

  // Nút Nghe: đọc cả thẻ theo chế độ đang chọn
  if (dialogSpeakBilingualBtn) {
    dialogSpeakBilingualBtn.addEventListener("click", () => {
      const card = visibleCards[currentCardIndex];
      if (!card) return;
      stopAutoplayTour();
      playCardSequence(card, false);
    });
  }

  // Nút Nghe chậm: đọc chậm từ vựng tiếng Anh cho bé nhại theo
  if (dialogSpeakSlowBtn) {
    dialogSpeakSlowBtn.addEventListener("click", () => {
      const card = visibleCards[currentCardIndex];
      if (!card) return;
      stopAutoplayTour();
      const word = getBilingualSequence(card).en2;
      fetchAndPlayVoice(word, "en", true);
    });
  }

  if (dialogMeaning) {
    dialogMeaning.style.cursor = "pointer";
    dialogMeaning.title = "Chạm để nghe phát âm song ngữ Việt - Anh";
    dialogMeaning.addEventListener("click", () => {
      const card = visibleCards[currentCardIndex];
      if (!card) return;
      stopAutoplayTour();
      playCardSequence(card, false);
    });
  }

  // Bàn phím (chỉ hỗ trợ thêm khi có bàn phím, không bắt buộc)
  document.addEventListener("keydown", (e) => {
    if (!dialog.hidden) {
      if (e.key === "Escape") {
        closeFlashcardDialog();
      } else if (e.key === "ArrowLeft") {
        navigateDialog(-1);
      } else if (e.key === "ArrowRight") {
        navigateDialog(1);
      } else if (e.key === " " && e.target.tagName !== "INPUT") {
        e.preventDefault();
        const card = visibleCards[currentCardIndex];
        if (card) {
          stopAutoplayTour();
          playCardSequence(card, currentSpeed === "slow");
        }
      }
    }
  });

  // Speed toggle buttons
  speedBtns.forEach((btn) => {
    btn.addEventListener("click", () => {
      currentSpeed = btn.dataset.speed === "slow" ? "slow" : "normal";
      updateSpeedUI();
      // Đang tự phát: thẻ tiếp theo sẽ dùng tốc độ mới. Đang mở thẻ: đọc lại với tốc độ mới.
      if (!isAutoplaying && !dialog.hidden && visibleCards[currentCardIndex]) {
        playCardSequence(visibleCards[currentCardIndex], currentSpeed === "slow");
      }
    });
  });

  // Autoplay Tour button
  if (autoplayTourBtn) {
    autoplayTourBtn.addEventListener("click", () => {
      if (isAutoplaying) {
        stopAutoplayTour();
        stopAllAudio();
      } else {
        startAutoplayTour();
      }
    });
  }

  // Tab buttons
  tabBtns.forEach((btn) => {
    btn.addEventListener("click", () => {
      tabBtns.forEach((b) => {
        b.classList.remove("active");
        b.setAttribute("aria-selected", "false");
      });
      btn.classList.add("active");
      btn.setAttribute("aria-selected", "true");
      filterCards();
    });
  });

  // Read Mode Buttons (Chỉ Tiếng Anh / Song ngữ)
  readModeBtns.forEach((btn) => {
    btn.addEventListener("click", () => {
      readMode = btn.dataset.mode === "bilingual" ? "bilingual" : "en_only";
      try {
        localStorage.setItem("hanhtrang_bilingual_read_mode", readMode);
      } catch (e) {}
      updateReadModeUI();
    });
  });

  // Rời trang / chuyển ứng dụng: dừng hẳn mọi âm thanh
  function stopEverything() {
    stopAutoplayTour();
    stopAllAudio();
  }
  window.addEventListener("pagehide", stopEverything);
  document.addEventListener("visibilitychange", () => {
    if (document.visibilityState === "hidden") stopEverything();
  });

  // Khởi tạo trạng thái giao diện
  updateReadModeUI();
  updateSpeedUI();

  // Nạp ngầm đường dẫn âm thanh các thẻ khi tải trang để khi bé bấm là phát ngay
  function startBackgroundAudioPreload() {
    let cardIdx = 0;

    function preloadNext() {
      if (cardIdx >= allCards.length) return;
      const card = allCards[cardIdx++];
      preloadCardAudio(card);
      setTimeout(preloadNext, 400);
    }

    setTimeout(preloadNext, 600);
  }

  // Initial setup
  refreshExploredStatus();
  filterCards();
  startBackgroundAudioPreload();
})();
