(() => {
    const overlay = document.querySelector("[data-learning-success-popup]");
    if (!overlay) return;

    const popup = overlay.querySelector(".learning-success-popup");
    const mascot = overlay.querySelector("[data-success-mascot]");
    const decorations = [...overlay.querySelectorAll("[data-success-decoration]")];
    const destination = overlay.dataset.destination || "/kids/today";
    const variants = [
        {theme: "sunshine", mascot: "/images/avatars/avatar-squirrel.png", shapes: ["⭐", "🌈", "✨", "🎈", "☀️", "🎉", "💛", "🌟"]},
        {theme: "ocean", mascot: "/images/avatars/avatar-dog.png", shapes: ["🐳", "🫧", "⭐", "🐠", "🌊", "✨", "🐚", "💙"]},
        {theme: "berry", mascot: "/images/avatars/avatar-cat.png", shapes: ["🌸", "💜", "✨", "🦋", "🎀", "⭐", "🍓", "🎉"]},
        {theme: "mint", mascot: "/images/avatars/avatar-bunny.png", shapes: ["🍀", "🌼", "✨", "🌱", "⭐", "🎈", "🪁", "💚"]},
        {theme: "coral", mascot: "/images/avatars/avatar-bear.png", shapes: ["🎊", "🧡", "⭐", "🎁", "✨", "🎉", "🍭", "🌟"]}
    ];

    const storageKey = "kids-learning-success-variant";
    let previous = -1;
    try {
        previous = Number.parseInt(sessionStorage.getItem(storageKey) || "-1", 10);
    } catch {
        previous = -1;
    }
    const candidates = variants.map((_, index) => index).filter((index) => index !== previous);
    const selectedIndex = candidates[Math.floor(Math.random() * candidates.length)] ?? 0;
    const selected = variants[selectedIndex];
    try {
        sessionStorage.setItem(storageKey, String(selectedIndex));
    } catch {
        // Popup vẫn hoạt động nếu trình duyệt chặn sessionStorage.
    }

    popup?.classList.add(`theme-${selected.theme}`);
    if (mascot) mascot.src = selected.mascot;
    decorations.forEach((item, index) => {
        item.textContent = selected.shapes[index] || "✨";
    });

    let hasNavigated = false;
    let navigationTimer = 0;
    const continueLearning = () => {
        if (hasNavigated) return;
        hasNavigated = true;
        window.clearTimeout(navigationTimer);
        window.location.assign(destination);
    };

    overlay.querySelector("[data-success-continue]")?.addEventListener("click", (event) => {
        event.preventDefault();
        continueLearning();
    });
    overlay.querySelector("[data-success-close]")?.addEventListener("click", continueLearning);
    overlay.addEventListener("click", (event) => {
        if (event.target === overlay) continueLearning();
    });
    document.addEventListener("keydown", (event) => {
        if (event.key === "Escape") continueLearning();
    });

    navigationTimer = window.setTimeout(continueLearning, 3000);
    window.requestAnimationFrame(() => overlay.classList.add("is-visible"));
    overlay.querySelector("[data-success-continue]")?.focus({preventScroll: true});
})();
