(() => {
    const overlay = document.querySelector("[data-learning-success-popup]");
    if (!overlay) return;

    const popup = overlay.querySelector(".learning-success-popup");
    const mascot = overlay.querySelector("[data-success-mascot]");
    const titleElement = overlay.querySelector("[data-success-title]");
    const messageElement = overlay.querySelector("[data-success-message]");
    const decorations = [...overlay.querySelectorAll("[data-success-decoration]")];

    // Thoi gian man chuc mung hien truoc khi tu chuyen bai. Thanh tien trinh trong CSS
    // chay theo dung bien nay nen hai ben luon khop nhau.
    const AUTO_CONTINUE_DELAY = 1500;

    // Dich den phai luon hop le: neu may chu khong tim duoc bai tiep theo thi quay ve
    // ban do bai hoc, tranh truong hop be bi ket o popup chuc mung ma khong di dau duoc.
    const rawDestination = (overlay.dataset.destination || "").trim();
    const destination = rawDestination || "/kids/today";

    // Be vua mo khoa ruong qua: day la luc be can tu chon (mo qua ngay hay hoc tiep)
    // nen man chuc mung dung lai cho be bam thay vi tu chuyen bai.
    const holdForDecision = overlay.dataset.hold === "true";

    const variants = [
        {theme: "sunshine", mascot: "/images/avatars/avatar-squirrel.png", shapes: ["⭐", "🌈", "✨", "🎈", "☀️", "🎉", "💛", "🌟"]},
        {theme: "ocean", mascot: "/images/avatars/avatar-dog.png", shapes: ["🐳", "🫧", "⭐", "🐠", "🌊", "✨", "🐚", "💙"]},
        {theme: "berry", mascot: "/images/avatars/avatar-cat.png", shapes: ["🌸", "💜", "✨", "🦋", "🎀", "⭐", "🍓", "🎉"]},
        {theme: "mint", mascot: "/images/avatars/avatar-bunny.png", shapes: ["🍀", "🌼", "✨", "🌱", "⭐", "🎈", "🪁", "💚"]},
        {theme: "coral", mascot: "/images/avatars/avatar-bear.png", shapes: ["🎊", "🧡", "⭐", "🎁", "✨", "🎉", "🍭", "🌟"]}
    ];

    // Tieu de va loi dong vien doi moi sau moi bai de be khong doc mai mot cau.
    const titles = [
        "Xuất sắc quá! 🎉",
        "Bé giỏi lắm! 🌟",
        "Tuyệt vời luôn! 🎊",
        "Hoan hô bé! 🎈",
        "Bé làm được rồi! 🏆",
        "Chính xác rồi! ✨"
    ];
    const encouragements = [
        "Bé làm đúng rồi, cô khen bé nhé!",
        "Bé tiến bộ thật nhanh, cố lên nào!",
        "Bé thật tập trung, cô rất tự hào!",
        "Bé thông minh lắm, học tiếp nhé!",
        "Bé làm rất cẩn thận, giỏi quá!",
        "Thêm một ngôi sao cho bé nhé!"
    ];

    // Chon ngau nhien nhung khong lap lai lua chon cua lan truoc, nho vay hai bai lien
    // tiep luon cho ra noi dung khac nhau. sessionStorage bi chan thi chi mat phan
    // "khong lap lai", man chuc mung van hoat dong binh thuong.
    const pickVaried = (storageKey, length) => {
        if (length <= 0) return 0;
        let previous = -1;
        try {
            previous = Number.parseInt(sessionStorage.getItem(storageKey) ?? "-1", 10);
        } catch {
            previous = -1;
        }
        const candidates = [];
        for (let index = 0; index < length; index += 1) {
            if (index !== previous) candidates.push(index);
        }
        const picked = candidates[Math.floor(Math.random() * candidates.length)] ?? 0;
        try {
            sessionStorage.setItem(storageKey, String(picked));
        } catch {
            // Khong luu duoc thi bo qua, lan sau chon lai ngau nhien.
        }
        return picked;
    };

    const selected = variants[pickVaried("kids-learning-success-variant", variants.length)];
    popup?.classList.add(`theme-${selected.theme}`);
    if (mascot) mascot.src = selected.mascot;
    decorations.forEach((item, index) => {
        item.textContent = selected.shapes[index] || "✨";
    });

    if (titleElement) {
        titleElement.textContent = titles[pickVaried("kids-learning-success-title", titles.length)];
    }
    // Loi khen do giao vien soan cho bai hoc luon duoc giu nguyen; chi khi bai khong co
    // loi khen rieng (data-vary-message="true") thi moi lay cau dong vien co san.
    if (messageElement && messageElement.dataset.varyMessage === "true") {
        messageElement.textContent = encouragements[pickVaried("kids-learning-success-message", encouragements.length)];
    }

    let hasNavigated = false;
    let navigationTimer = 0;
    const continueLearning = () => {
        if (hasNavigated) return;
        hasNavigated = true;
        window.clearTimeout(navigationTimer);
        window.location.assign(destination);
    };

    const continueButton = overlay.querySelector("[data-success-continue]");
    if (continueButton) {
        // Neu Razor khong dung duoc dich den, gan lai href de nut van bam duoc
        // kieu lien ket thong thuong khi JavaScript gap su co.
        if (!continueButton.getAttribute("href")) continueButton.setAttribute("href", destination);
        continueButton.addEventListener("click", (event) => {
            event.preventDefault();
            continueLearning();
        });
    }
    overlay.querySelector("[data-success-close]")?.addEventListener("click", continueLearning);
    overlay.addEventListener("click", (event) => {
        if (event.target === overlay) continueLearning();
    });
    document.addEventListener("keydown", (event) => {
        if (event.key === "Escape") continueLearning();
    });

    if (!holdForDecision) {
        overlay.style.setProperty("--success-timer-duration", `${AUTO_CONTINUE_DELAY}ms`);
        navigationTimer = window.setTimeout(continueLearning, AUTO_CONTINUE_DELAY);
    }
    window.requestAnimationFrame(() => overlay.classList.add("is-visible"));
    continueButton?.focus({preventScroll: true});
})();
