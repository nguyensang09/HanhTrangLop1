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
  const searchInput = document.getElementById("bilingualSearchInput");
  const clearSearchBtn = document.getElementById("clearSearchBtn");
  const resetSearchBtn = document.getElementById("resetSearchBtn");
  const noResultsNotice = document.getElementById("noResultsNotice");
  const sections = document.querySelectorAll("[data-section]");

  // State
  let currentCardIndex = 0;
  let visibleCards = [...allCards];
  let currentSpeed = "normal"; // 'normal' | 'slow'
  let isAutoplaying = false;
  let autoplayTimer = null;
  let sequenceTimer = null;
  let activeAudio = null;

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
      counterText.textContent = exploredSet.size + "/" + allCards.length + " đã nghe";
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

  // Tải trước toàn bộ âm thanh của 1 thẻ flashcard (riêng chữ cái, nghĩa từ vựng, từ tiếng Anh, câu ví dụ - giọng NỮ)
  function preloadCardAudio(card) {
    if (!card) return;
    const p = getLetterPrompt(card);
    const meaning = card.dataset.meaning || "";
    const word = card.dataset.word || "";
    const exampleVi = card.dataset.exampleVi;
    const exampleEn = card.dataset.exampleEn;

    if (p.vi) getAudioUrl(p.vi, "vi");
    if (meaning) getAudioUrl(meaning, "vi");
    if (word) {
      getAudioUrl(word, "en");
      getAudioUrl(word, "en", true);
    }
    if (exampleVi) getAudioUrl(exampleVi, "vi");
    if (exampleEn) getAudioUrl(exampleEn, "en");
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
      if (!text || !window.speechSynthesis) {
        resolve();
        return;
      }

      const utterance = new SpeechSynthesisUtterance(text);
      utterance.lang = lang;
      utterance.rate = rate;
      utterance.pitch = pitch;

      const voice = findVoice(lang.split("-")[0]);
      if (voice) utterance.voice = voice;

      document.body.classList.add("audio-is-speaking");

      const cleanup = () => {
        document.body.classList.remove("audio-is-speaking");
        resolve();
      };

      utterance.onend = cleanup;
      utterance.onerror = cleanup;

      window.speechSynthesis.speak(utterance);
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
    } else {
      // Fallback tức thì
      if (lang === "vi") {
        await speakWithWebSpeechPromise(text, "vi-VN", isSlow ? 0.65 : 0.88, 1.0);
      } else {
        await speakWithWebSpeechPromise(text, "en-US", isSlow ? 0.62 : 0.82, 1.05);
      }
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
    } else {
      if (lang === "vi") {
        await speakWithWebSpeechPromise(text, "vi-VN", isSlow ? 0.65 : 0.88, 1.0);
      } else {
        await speakWithWebSpeechPromise(text, "en-US", isSlow ? 0.60 : 0.82, 1.05);
      }
    }
  }

  function waitGapPromise(ms) {
    return new Promise((resolve) => {
      sequenceTimer = setTimeout(resolve, ms);
    });
  }

  // Chuỗi phát âm song ngữ chuẩn theo đúng yêu cầu:
  // 1. Chữ A (giọng nữ) ➔ Dừng 1 nhịp (500ms)
  // 2. Quả táo (giọng nữ) ➔ Dừng 1 nhịp (500ms)
  // 3. Apple (giọng nữ) ➔ Dừng chuyển đoạn (700ms)
  // 4. Câu ví dụ tiếng Việt (giọng nữ) ➔ Dừng 1 nhịp (500ms)
  // 5. Câu ví dụ tiếng Anh (giọng nữ)
  async function playIntegratedBilingualSequence(card, isSlow = false, onEndCallback) {
    stopAllAudio();
    if (!card) {
      if (onEndCallback) onEndCallback();
      return;
    }

    const p = getLetterPrompt(card);
    const letterVi = p.vi || ""; // "Chữ A" hoặc "Số 1"
    const meaningVi = card.dataset.meaning || ""; // "Quả táo" hoặc "Một"
    const wordEn = card.dataset.word || card.dataset.symbol || ""; // "Apple" hoặc "One"
    const exampleVi = card.dataset.exampleVi || "";
    const exampleEn = card.dataset.exampleEn || "";

    // Tải trước ngầm tất cả các đoạn âm thanh giọng NỮ
    if (letterVi) getAudioUrl(letterVi, "vi");
    if (meaningVi) getAudioUrl(meaningVi, "vi");
    if (wordEn) getAudioUrl(wordEn, "en", isSlow);
    if (exampleVi) getAudioUrl(exampleVi, "vi");
    if (exampleEn) getAudioUrl(exampleEn, "en", isSlow);

    if (dialogSpeakBilingualBtn) dialogSpeakBilingualBtn.classList.add("is-playing");
    const audioBtn = card.querySelector(".card-audio-btn");
    if (audioBtn) audioBtn.classList.add("is-playing");

    markCardAsExplored(card);

    try {
      // 1. Đọc Chữ cái / Chữ số (Tiếng Việt - Giọng Nữ)
      if (letterVi) {
        await playVoiceItemPromise(letterVi, "vi", isSlow);
      }

      // Dừng lại 1 nhịp (500ms)
      if (letterVi && meaningVi) {
        await waitGapPromise(500);
      }

      // 2. Đọc Nghĩa Tiếng Việt (Giọng Nữ)
      if (meaningVi) {
        await playVoiceItemPromise(meaningVi, "vi", isSlow);
      }

      // Dừng lại 1 nhịp (500ms)
      if (meaningVi && wordEn) {
        await waitGapPromise(500);
      }

      // 3. Đọc Từ vựng Tiếng Anh (Giọng Nữ)
      if (wordEn) {
        await playVoiceItemPromise(wordEn, "en", isSlow);
      }

      // 4. Tiếp nối sang Câu ví dụ mẫu
      if (exampleVi || exampleEn) {
        // Dừng lại 1 nhịp chuyển tiếp sang câu ví dụ (700ms)
        await waitGapPromise(700);

        // Đọc câu ví dụ (Tiếng Việt - Giọng Nữ)
        if (exampleVi) {
          await playVoiceItemPromise(exampleVi, "vi", isSlow);
        }

        // Dừng lại 1 nhịp (500ms)
        if (exampleVi && exampleEn) {
          await waitGapPromise(500);
        }

        // Đọc câu ví dụ (Tiếng Anh - Giọng Nữ)
        if (exampleEn) {
          await playVoiceItemPromise(exampleEn, "en", isSlow);
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

      playCardSequence(card, false);

      autoplayTimer = setTimeout(() => {
        if (!isAutoplaying) return;
        currentCardIndex = (currentCardIndex + 1) % visibleCards.length;
        playStep();
      }, 4200);
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

  // Tab & Search Filtering
  function filterCards() {
    const activeTabBtn = document.querySelector(".bilingual-tab-btn.active");
    const activeTab = activeTabBtn ? activeTabBtn.dataset.tab : "all";
    const searchQuery = removeAccents(searchInput ? searchInput.value : "");

    let visibleCount = 0;

    allCards.forEach((card) => {
      const kind = card.dataset.kind; // 'letter' | 'number'
      const symbol = removeAccents(card.dataset.symbol);
      const word = removeAccents(card.dataset.word);
      const meaning = removeAccents(card.dataset.meaning);
      const phonetic = removeAccents(card.dataset.phonetic);

      // Check tab match
      let matchTab = true;
      if (activeTab === "letters" && kind !== "letter") matchTab = false;
      if (activeTab === "numbers" && kind !== "number") matchTab = false;

      // Check search match
      let matchSearch = true;
      if (searchQuery) {
        matchSearch =
          symbol === searchQuery ||
          word.includes(searchQuery) ||
          meaning.includes(searchQuery) ||
          phonetic.includes(searchQuery);
      }

      if (matchTab && matchSearch) {
        card.style.display = "";
        visibleCount++;
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

    if (noResultsNotice) {
      noResultsNotice.hidden = visibleCount > 0;
    }

    if (clearSearchBtn) {
      clearSearchBtn.hidden = !searchInput.value;
    }
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
      const word = card.dataset.word || card.dataset.symbol || "";
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

  // Search input
  if (searchInput) {
    searchInput.addEventListener("input", filterCards);
  }

  if (clearSearchBtn) {
    clearSearchBtn.addEventListener("click", () => {
      if (searchInput) searchInput.value = "";
      filterCards();
      searchInput.focus();
    });
  }

  if (resetSearchBtn) {
    resetSearchBtn.addEventListener("click", () => {
      if (searchInput) searchInput.value = "";
      tabBtns.forEach((b) => {
        if (b.dataset.tab === "all") {
          b.classList.add("active");
          b.setAttribute("aria-selected", "true");
        } else {
          b.classList.remove("active");
          b.setAttribute("aria-selected", "false");
        }
      });
      filterCards();
    });
  }

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
