(() => {
    // Trang hoàn thành bài (tập tô hoặc khi nộp bài không qua fetch): kết quả nằm trên thanh kết quả
    // cuối màn hình. Thanh chạy hết thì tự sang bài tiếp; vừa mở rương quà thì dừng chờ bé chọn.
    const bar = document.querySelector("[data-lw-result].is-correct");
    if (!bar) return;

    const AUTO_CONTINUE_DELAY = 2600;
    const destination = (bar.dataset.destination || "").trim() || "/kids/today";
    const holdForDecision = bar.dataset.hold === "true";
    const progress = bar.querySelector("[data-activity-autocheck-progress]");

    let hasNavigated = false;
    const continueLearning = () => {
        if (hasNavigated) return;
        hasNavigated = true;
        window.location.assign(destination);
    };

    bar.querySelector("[data-success-continue]")?.addEventListener("click", (event) => {
        event.preventDefault();
        continueLearning();
    });

    window.requestAnimationFrame(() => bar.classList.add("is-active"));
    if (holdForDecision) {
        bar.classList.add("is-holding");
        return;
    }

    if (progress) {
        progress.style.transition = "none";
        progress.style.width = "0%";
        void progress.offsetWidth;
        progress.style.transition = `width ${AUTO_CONTINUE_DELAY}ms linear`;
        progress.style.width = "100%";
    }
    window.setTimeout(continueLearning, AUTO_CONTINUE_DELAY);
})();
