/**
 * Bilingual Listen Screen - Interactive English-Vietnamese Flashcard & Audio Engine
 * HanhTrangLop1
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
  let sequenceTimer = null;
  let activeAudio = null;

  function updateReadModeUI() {
    readModeBtns.forEach((btn) => {
      const isMatch = btn.dataset.mode === readMode;
      btn.classList.toggle("active", isMatch);
      btn.setAttribute("aria-selected", isMatch ? "true" : "false");
    });

    if (dialogSpeakModeTitle) {
      dialogSpeakModeTitle.textContent = readMode === "en_only" ? "Chỉ Tiếng Anh" : "Nghe Song Ngữ";
    }
    if (dialogSpeakModeSub) {
      dialogSpeakModeSub.textContent = readMode === "en_only" ? "Chữ ➔ Từ ví dụ (A - Apple)" : "Chữ ➔ Từ ➔ Câu ví dụ";
    }
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
      if (exploredSet.has(key)) {
        card.classList.add("has-explored");
      } else {
        card.classList.remove("has-explored");
      }
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
        counterPill.classList.add("pulse-bounce");
        setTimeout(() => counterPill.classList.remove("pulse-bounce"), 800);
      }
    }
  }

  // Vietnamese accent remover for search matching
  function removeAccents(str) {
    if (!str) return "";
    return str
      .normalize("NFD")
      .replace(/[\u0300-\u036f]/g, "")
      .replace(/đ/g, "d")
      .replace(/Đ/g, "D")
      .toLowerCase()
      .trim();
  }

  // Speech Synthesis Engine
  let voices = [];
  function populateVoices() {
    if (!window.speechSynthesis) return;
    voices = window.speechSynthesis.getVoices();
  }
  if (window.speechSynthesis) {
    populateVoices();
    window.speechSynthesis.onvoiceschanged = populateVoices;
  }

  // Ưu tiên tuyệt đối GIỌNG NỮ (Hoài My / Jenny / Aria / Zira / Samantha) và loại bỏ toàn bộ giọng nam
  function findVoice(langPrefix) {
    if (!voices.length && window.speechSynthesis) {
      voices = window.speechSynthesis.getVoices();
    }
    const lp = langPrefix.toLowerCase();
    const matching = voices.filter((v) => v.lang.toLowerCase().startsWith(lp));

    const isMale = (name) => {
      const n = name.toLowerCase();
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
      const n = name.toLowerCase();
      return (
        n.includes("hoaimy") ||
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

  // Audio Cache & Neural Voice Engine with Parallel Preloading
  const audioCache = new Map();
  const pendingRequests = new Map();

  function stopAllAudio() {
    clearTimeout(sequenceTimer);
    if (window.speechSynthesis) {
      window.speechSynthesis.cancel();
    }
    if (activeAudio) {
      activeAudio.pause();
      activeAudio.currentTime = 0;
      activeAudio = null;
    }
    document.querySelectorAll(".card-audio-btn.is-playing").forEach((el) => el.classList.remove("is-playing"));
    document.body.classList.remove("audio-is-speaking");
  }

  // Tải trước và cache âm thanh từ backend (chống duplicate requests)
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
            // Preload vào audio cache trình duyệt
            const preAudio = new Audio();
            preAudio.preload = "auto";
            preAudio.src = data.audioUrl;
            return data.audioUrl;
          }
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
    } else {
      return {
        vi: `Số ${symbol}`,
        en: symbol,
        label: `Đọc riêng số ${symbol}`
      };
    }
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

  // Tải trước toàn bộ âm thanh của 1 thẻ flashcard (riêng chữ cái, nghĩa từ vựng, từ tiếng Anh, câu ví dụ - giọng NỮ)
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

  function playAudioUrlPromise(url, playbackRate = 1.0) {
    return new Promise((resolve) => {
      if (!url) {
        resolve();
        return;
      }

      try {
        if (activeAudio) {
          activeAudio.pause();
          activeAudio.currentTime = 0;
        }

        activeAudio = new Audio(url);
        activeAudio.playbackRate = playbackRate;
        document.body.classList.add("audio-is-speaking");

        const cleanup = () => {
          document.body.classList.remove("audio-is-speaking");
          activeAudio = null;
          resolve();
        };

        activeAudio.onended = cleanup;
        activeAudio.onerror = cleanup;

        const playPromise = activeAudio.play();
        if (playPromise !== undefined) {
          playPromise.catch(() => cleanup());
        }
      } catch (e) {
        document.body.classList.remove("audio-is-speaking");
        resolve();
      }
    });
  }

  function playPreloadedAudioPromise(audioElement) {
    return new Promise((resolve) => {
      if (!audioElement) {
        resolve();
        return;
      }

      try {
        if (activeAudio) {
          activeAudio.pause();
          activeAudio.currentTime = 0;
        }

        activeAudio = audioElement;
        document.body.classList.add("audio-is-speaking");

        const cleanup = () => {
          document.body.classList.remove("audio-is-speaking");
          activeAudio = null;
          resolve();
        };

        activeAudio.onended = cleanup;
        activeAudio.onerror = cleanup;

        const playPromise = activeAudio.play();
        if (playPromise !== undefined) {
          playPromise.catch(() => cleanup());
        }
      } catch (e) {
        document.body.classList.remove("audio-is-speaking");
        resolve();
      }
    });
  }

  function speakWithWebSpeechPromise(text, lang, rate, pitch) {
    return new Promise((resolve) => {
      resolve();
    });
  }

  async function fetchAndPlayVoice(text, lang = "vi", isSlow = false, onEndCallback) {
    if (!text) {
      if (onEndCallback) onEndCallback();
      return;
    }

    const playbackRate = isSlow ? 0.75 : 1.0;
    // Chờ tối đa 750ms nếu server đang tổng hợp, nếu quá 750ms thì phát ngay qua Web Speech không để chờ
    const audioUrl = await Promise.race([
      getAudioUrl(text, lang, isSlow),
      new Promise((resolve) => setTimeout(() => resolve(null), 750))
    ]);

    if (audioUrl) {
      await playAudioUrlPromise(audioUrl, playbackRate);
    }

    if (onEndCallback) onEndCallback();
  }

  // Chuỗi phát âm song ngữ chuẩn: Tải song song cả 2 ➔ Đọc tiếng Việt ➔ ĐỌC TIẾNG ANH LUÔN (liền mạch, 0 chờ đợi)
  // Phát một phân đoạn giọng đọc (ưu tiên tệp âm thanh server trong 750ms, fallback Web Speech)
  async function playVoiceItemPromise(text, lang = "vi", isSlow = false) {
    if (!text) return;
    const playbackRate = isSlow ? 0.72 : 1.0;
    const audioUrl = await Promise.race([
      getAudioUrl(text, lang, isSlow),
      new Promise((resolve) => setTimeout(() => resolve(null), 750))
    ]);

    if (audioUrl) {
      await playAudioUrlPromise(audioUrl, playbackRate);
    }
  }

  function waitGapPromise(ms) {
    return new Promise((resolve) => {
      sequenceTimer = setTimeout(resolve, ms);
    });
  }

  // Chuỗi phát âm:
  // - Nếu readMode === 'en_only' (mặc định):
  //   Chỉ đọc Tiếng Anh theo cấu trúc chữ - ví dụ cho chữ (Ví dụ: A - Apple / 1 - One) rồi dừng lại.
  // - Nếu readMode === 'bilingual':
  //   Đọc cấu trúc song ngữ đầy đủ như hiện tại (En - Vn - En - Vn - En - Vn).
  async function playIntegratedBilingualSequence(card, isSlow = false, onEndCallback) {
    stopAllAudio();
    if (!card) {
      if (onEndCallback) onEndCallback();
      return;
    }

    const sequence = getBilingualSequence(card);
    const letterEn = sequence.en1;
    const letterVi = sequence.vi1;
    // Chữ cái: 'Apple', Chữ số: 'One' (thay vì 'Number One' nếu đọc theo kiểu Chữ - Ví dụ: 1 - One)
    const wordEn = (readMode === "en_only" && card?.dataset.kind === "number")
      ? (card?.dataset.word || sequence.en2)
      : sequence.en2;
    const meaningVi = sequence.vi2;
    const exampleEn = sequence.en3;
    const exampleVi = sequence.vi3;

    // Tải trước ngầm các đoạn âm thanh giọng NỮ
    if (letterEn) getAudioUrl(letterEn, "en", isSlow);
    if (wordEn) getAudioUrl(wordEn, "en", isSlow);
    if (readMode !== "en_only") {
      if (letterVi) getAudioUrl(letterVi, "vi");
      if (meaningVi) getAudioUrl(meaningVi, "vi");
      if (exampleEn) getAudioUrl(exampleEn, "en", isSlow);
      if (exampleVi) getAudioUrl(exampleVi, "vi");
    }

    if (dialogSpeakBilingualBtn) dialogSpeakBilingualBtn.classList.add("is-playing");
    const audioBtn = card.querySelector(".card-audio-btn");
    if (audioBtn) audioBtn.classList.add("is-playing");

    markCardAsExplored(card);

    try {
      if (readMode === "en_only") {
        // Cấu trúc chỉ đọc Tiếng Anh: Chữ - Ví dụ cho chữ (Ví dụ: A - Apple) rồi dừng lại
        if (letterEn) {
          await playVoiceItemPromise(letterEn, "en", isSlow);
        }
        if (letterEn && wordEn) {
          await waitGapPromise(300);
        }
        if (wordEn) {
          await playVoiceItemPromise(wordEn, "en", isSlow);
        }
      } else {
        // Cấu trúc song ngữ đầy đủ: En - Vn - En - Vn - En - Vn
        // 1. Chữ cái / Số (Tiếng Anh - Giọng NỮ)
        if (letterEn) {
          await playVoiceItemPromise(letterEn, "en", isSlow);
        }

        // Dừng lại 1 nhịp (300ms) để chuyển voice
        if (letterEn && letterVi) {
          await waitGapPromise(300);
        }

        // 2. Chữ cái / Số (Tiếng Việt - Giọng NỮ)
        if (letterVi) {
          await playVoiceItemPromise(letterVi, "vi", isSlow);
        }

        // Dừng lại 1 nhịp (300ms) để chuyển voice
        if (letterVi && wordEn) {
          await waitGapPromise(300);
        }

        // 3. Từ vựng (Tiếng Anh - Giọng NỮ)
        if (wordEn) {
          await playVoiceItemPromise(wordEn, "en", isSlow);
        }

        // Dừng lại 1 nhịp (300ms) để chuyển voice
        if (wordEn && meaningVi) {
          await waitGapPromise(300);
        }

        // 4. Nghĩa từ vựng (Tiếng Việt - Giọng NỮ)
        if (meaningVi) {
          await playVoiceItemPromise(meaningVi, "vi", isSlow);
        }

        // 5. Câu ví dụ mẫu (Tiếng Anh trước ➔ 300ms ➔ Tiếng Việt sau)
        if (exampleEn || exampleVi) {
          await waitGapPromise(300);

          if (exampleEn) {
            await playVoiceItemPromise(exampleEn, "en", isSlow);
          }

          if (exampleEn && exampleVi) {
            await waitGapPromise(300);
          }

          if (exampleVi) {
            await playVoiceItemPromise(exampleVi, "vi", isSlow);
          }
        }
      }
    } finally {
      if (dialogSpeakBilingualBtn) dialogSpeakBilingualBtn.classList.remove("is-playing");
      if (audioBtn) audioBtn.classList.remove("is-playing");
      if (onEndCallback) onEndCallback();
    }
  }

  function playCardSequence(card, isSlow, onEndCallback) {
    playIntegratedBilingualSequence(card, isSlow, onEndCallback);
  }

  // Flashcard Dialog Logic
  function openFlashcardDialog(card) {
    if (!card) return;
    stopAutoplayTour();

    const idx = visibleCards.indexOf(card);
    if (idx !== -1) {
      currentCardIndex = idx;
    }

    updateDialogWithCard(card);

    // Tải trước thẻ hiện tại và 2 thẻ liền kề để khi bé bấm lật thẻ là có âm thanh tức thì
    preloadCardAudio(card);
    if (idx > 0) preloadCardAudio(visibleCards[idx - 1]);
    if (idx < visibleCards.length - 1) preloadCardAudio(visibleCards[idx + 1]);

    dialog.hidden = false;
    requestAnimationFrame(() => {
      dialog.classList.add("is-active");
    });

    // Speak card content
    playCardSequence(card, currentSpeed === "slow");
  }

  function closeFlashcardDialog() {
    stopAllAudio();
    stopAutoplayTour();
    dialog.classList.remove("is-active");
    setTimeout(() => {
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

    dialogSymbolTitle.textContent = symbol;
    dialogWord.textContent = word;
    dialogPhonetic.textContent = "/" + phonetic + "/";
    dialogMeaning.textContent = meaning;
    dialogEmoji.textContent = emoji;
    dialogExampleEn.textContent = exampleEn;
    dialogExampleVi.textContent = exampleVi;

    dialogCategoryBadge.textContent = kind === "letter" ? `Chữ cái ${symbol}` : `Chữ số ${symbol}`;

    if (image) {
      dialogImage.src = image;
      dialogImage.alt = `${symbol} - ${word}`;
      dialogImage.style.display = "block";
    } else {
      dialogImage.style.display = "none";
    }

    // Progress in visible cards
    const currentPos = visibleCards.indexOf(card) + 1;
    dialogProgressIndicator.textContent = `Thẻ ${currentPos} / ${visibleCards.length}`;

    // Nav button state
    dialogPrevBtn.disabled = visibleCards.length <= 1;
    dialogNextBtn.disabled = visibleCards.length <= 1;

    // Tự động tải trước âm thanh thẻ hiện tại và các thẻ kế tiếp để phát ngay tức thì
    preloadCardAudio(card);
    const currIdx = visibleCards.indexOf(card);
    if (currIdx !== -1) {
      if (visibleCards[currIdx + 1]) preloadCardAudio(visibleCards[currIdx + 1]);
      if (visibleCards[currIdx - 1]) preloadCardAudio(visibleCards[currIdx - 1]);
    }
  }

  function navigateDialog(direction) {
    if (!visibleCards.length) return;
    currentCardIndex = (currentCardIndex + direction + visibleCards.length) % visibleCards.length;
    const nextCard = visibleCards[currentCardIndex];
    updateDialogWithCard(nextCard);
    playCardSequence(nextCard, currentSpeed === "slow");
  }

  // Autoplay Tour mode
  function startAutoplayTour() {
    if (!visibleCards.length) return;
    isAutoplaying = true;
    if (autoplayTourBtn) {
      autoplayTourBtn.classList.add("active", "is-touring");
      const label = autoplayTourBtn.querySelector(".tour-label");
      const icon = autoplayTourBtn.querySelector(".tour-icon");
      if (label) label.textContent = "Dừng tự phát";
      if (icon) icon.textContent = "pause_circle";
    }

    function playStep() {
      if (!isAutoplaying) return;
      const card = visibleCards[currentCardIndex];
      updateDialogWithCard(card);
      dialog.hidden = false;
      dialog.classList.add("is-active");

      playCardSequence(card, currentSpeed === "slow");

      const delay = readMode === "en_only" ? 2500 : 4500;
      autoplayTimer = setTimeout(() => {
        if (!isAutoplaying) return;
        currentCardIndex = (currentCardIndex + 1) % visibleCards.length;
        playStep();
      }, delay);
    }

    playStep();
  }

  function stopAutoplayTour() {
    if (!isAutoplaying) return;
    isAutoplaying = false;
    clearTimeout(autoplayTimer);
    if (autoplayTourBtn) {
      autoplayTourBtn.classList.remove("active", "is-touring");
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

      // Check tab match
      let matchTab = true;
      if (activeTab === "letters" && kind !== "letter") matchTab = false;
      if (activeTab === "numbers" && kind !== "number") matchTab = false;

      if (matchTab) {
        card.style.display = "";
      } else {
        card.style.display = "none";
      }
    });

    // Update section visibility
    sections.forEach((sec) => {
      const sectionName = sec.dataset.section;
      const visibleInSection = sec.querySelectorAll("[data-listen-card]:not([style*='display: none'])").length;
      sec.style.display = visibleInSection > 0 ? "" : "none";
    });

    // Update visibleCards array
    visibleCards = allCards.filter((card) => card.style.display !== "none");
  }

  // Event Listeners
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

  if (dialog) {
    dialog.addEventListener("click", (e) => {
      if (e.target === dialog) {
        closeFlashcardDialog();
      }
    });
  }

  // 1. Nút Nghe Song Ngữ: Tích hợp đầy đủ (Chữ cái ➔ Từ vựng ➔ Câu ví dụ) với nhịp độ tự nhiên
  if (dialogSpeakBilingualBtn) {
    dialogSpeakBilingualBtn.addEventListener("click", () => {
      const card = visibleCards[currentCardIndex];
      if (!card) return;
      playCardSequence(card, false);
    });
  }

  // 2. Nút Đọc Chậm: Đọc chậm từ vựng tiếng Anh (giọng nữ) cho bé nhại theo
  if (dialogSpeakSlowBtn) {
    dialogSpeakSlowBtn.addEventListener("click", () => {
      const card = visibleCards[currentCardIndex];
      if (!card) return;
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
      playCardSequence(card, false);
    });
  }

  // Keyboard navigation
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
        if (card) playCardSequence(card, currentSpeed === "slow");
      }
    }
  });

  // Speed toggle buttons
  speedBtns.forEach((btn) => {
    btn.addEventListener("click", () => {
      speedBtns.forEach((b) => b.classList.remove("active"));
      btn.classList.add("active");
      currentSpeed = btn.dataset.speed;
      // If dialog is open, replay with new speed
      if (!dialog.hidden && visibleCards[currentCardIndex]) {
        playCardSequence(visibleCards[currentCardIndex], currentSpeed === "slow");
      }
    });
  });

  // Autoplay Tour button
  if (autoplayTourBtn) {
    autoplayTourBtn.addEventListener("click", () => {
      if (isAutoplaying) {
        stopAutoplayTour();
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
      readMode = btn.dataset.mode || "en_only";
      try {
        localStorage.setItem("hanhtrang_bilingual_read_mode", readMode);
      } catch (e) {}
      updateReadModeUI();
    });
  });

  // Khởi tạo trạng thái giao diện chế độ đọc
  updateReadModeUI();

  // Tự động nạp ngầm toàn bộ âm thanh các thẻ khi tải trang để khi bé bấm là phát ngay tức thì
  function startBackgroundAudioPreload() {
    const allCards = Array.from(document.querySelectorAll("[data-listen-card]"));
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
