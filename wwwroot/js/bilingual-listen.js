(function () {
  const cards = document.querySelectorAll("[data-listen-card]");
  const popover = document.querySelector("[data-listen-popover]");
  if (!cards.length || !popover) return;

  const image = popover.querySelector("[data-listen-image]");
  const kind = popover.querySelector("[data-listen-kind]");
  const symbol = popover.querySelector("[data-listen-symbol]");
  const word = popover.querySelector("[data-listen-word]");
  let closeTimer = 0;
  let exampleTimer = 0;
  let activeAudio = null;

  function stopCurrentSpeech() {
    window.clearTimeout(exampleTimer);
    if (window.speechSynthesis) {
      window.speechSynthesis.cancel();
    }
    if (activeAudio) {
      activeAudio.pause();
      activeAudio.currentTime = 0;
      activeAudio = null;
    }
  }

  function speak(text, audioUrl) {
    stopCurrentSpeech();
    if (audioUrl) {
      activeAudio = new Audio(audioUrl);
      activeAudio.play().catch(() => speakWithBrowser(text));
      return;
    }
    speakWithBrowser(text);
  }

  function speakSequence(firstText, secondText) {
    speak(firstText);
    if (!secondText) return;

    exampleTimer = window.setTimeout(() => {
      speak(secondText);
    }, 1100);
  }

  function speakWithBrowser(text) {
    if (!window.speechSynthesis || !text) return;
    const utterance = new SpeechSynthesisUtterance(text);
    utterance.lang = "en-US";
    utterance.rate = 0.78;
    utterance.pitch = 1.08;
    window.speechSynthesis.speak(utterance);
  }

  function closePopover() {
    popover.hidden = true;
    popover.classList.remove("is-visible");
    window.clearTimeout(closeTimer);
  }

  function openPopover(card) {
    const cardKind = card.dataset.kind === "number" ? "Number" : "Letter";
    const cardSymbol = card.dataset.symbol || "";
    const cardWord = card.dataset.word || "";
    const cardImage = card.dataset.image || "";
    const speakText = card.dataset.speak || cardSymbol || cardWord;
    const exampleText = card.dataset.exampleSpeak || "";

    image.src = cardImage;
    image.alt = cardWord ? `${cardSymbol} ${cardWord}` : cardSymbol;
    kind.textContent = cardKind;
    symbol.textContent = cardSymbol;
    word.textContent = cardKind === "Number" ? cardWord : `${cardSymbol} is for ${cardWord}`;

    popover.hidden = false;
    requestAnimationFrame(() => popover.classList.add("is-visible"));
    speakSequence(speakText, exampleText);

    window.clearTimeout(closeTimer);
    closeTimer = window.setTimeout(closePopover, exampleText ? 5200 : 3000);
  }

  cards.forEach((card) => {
    card.addEventListener("click", () => openPopover(card));
  });

  popover.addEventListener("click", (event) => {
    if (event.target === popover) {
      stopCurrentSpeech();
      closePopover();
    }
  });
})();
