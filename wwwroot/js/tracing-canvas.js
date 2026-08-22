(function () {
  const host = document.querySelector(".tracing-canvas-host");
  const form = document.querySelector(".tracing-submit-form");
  if (!host || !form) {
    return;
  }

  const canvas = host.querySelector(".tracing-canvas");
  const context = canvas.getContext("2d");
  const undoButton = host.querySelector(".tracing-undo");
  const clearButton = host.querySelector(".tracing-clear");
  const audioButton = host.querySelector(".tracing-audio");
  const submitButton = form.querySelector(".tracing-submit-button");
  const strokesInput = form.querySelector(".tracing-strokes-input");
  const metricsInput = form.querySelector(".tracing-metrics-input");
  const statusText = form.querySelector(".tracing-status");
  const minPoints = Number(host.dataset.tracingMinPoints || 20);
  const expectedStrokeCount = Number(host.dataset.tracingExpectedStrokes || 1);

  const state = {
    drawing: false,
    strokes: [],
    currentStroke: []
  };

  function pointFromEvent(event) {
    const rect = canvas.getBoundingClientRect();
    return {
      x: Math.round(((event.clientX - rect.left) / rect.width) * canvas.width),
      y: Math.round(((event.clientY - rect.top) / rect.height) * canvas.height),
      t: Date.now()
    };
  }

  function drawStroke(points) {
    if (points.length < 2) {
      return;
    }

    context.lineCap = "round";
    context.lineJoin = "round";
    context.lineWidth = 28;
    context.strokeStyle = "#26c99a";
    context.beginPath();
    context.moveTo(points[0].x, points[0].y);

    for (let index = 1; index < points.length; index += 1) {
      context.lineTo(points[index].x, points[index].y);
    }

    context.stroke();
  }

  function redraw() {
    context.clearRect(0, 0, canvas.width, canvas.height);
    state.strokes.forEach(drawStroke);
    drawStroke(state.currentStroke);
  }

  function countPoints() {
    return state.strokes.reduce((total, stroke) => total + stroke.length, 0) + state.currentStroke.length;
  }

  function measureLength(points) {
    let length = 0;
    for (let index = 1; index < points.length; index += 1) {
      const dx = points[index].x - points[index - 1].x;
      const dy = points[index].y - points[index - 1].y;
      length += Math.sqrt(dx * dx + dy * dy);
    }

    return Math.round(length);
  }

  function buildMetrics() {
    const totalPoints = countPoints();
    const totalLength = state.strokes.reduce((total, stroke) => total + measureLength(stroke), 0);

    return {
      source: "tracing_canvas_v1",
      strokeCount: state.strokes.length,
      expectedStrokeCount,
      totalPoints,
      totalLength,
      coverageScore: Math.min(100, Math.round((totalPoints / minPoints) * 100)),
      completed: totalPoints >= minPoints
    };
  }

  function syncForm() {
    const metrics = buildMetrics();
    strokesInput.value = JSON.stringify(state.strokes);
    metricsInput.value = JSON.stringify(metrics);
    submitButton.disabled = !metrics.completed;

    statusText.classList.toggle("ready", metrics.completed);
    statusText.textContent = metrics.completed
      ? "Tốt rồi, con có thể hoàn thành bài."
      : `Con tô thêm một chút nữa nhé (${metrics.totalPoints}/${minPoints}).`;
  }

  function startStroke(event) {
    event.preventDefault();
    state.drawing = true;
    state.currentStroke = [pointFromEvent(event)];
    canvas.setPointerCapture(event.pointerId);
    redraw();
    syncForm();
  }

  function continueStroke(event) {
    if (!state.drawing) {
      return;
    }

    event.preventDefault();
    state.currentStroke.push(pointFromEvent(event));
    redraw();
    syncForm();
  }

  function finishStroke(event) {
    if (!state.drawing) {
      return;
    }

    event.preventDefault();
    state.drawing = false;
    if (state.currentStroke.length > 1) {
      state.strokes.push(state.currentStroke);
    }

    state.currentStroke = [];
    redraw();
    syncForm();
  }

  undoButton.addEventListener("click", function () {
    state.strokes.pop();
    redraw();
    syncForm();
  });

  clearButton.addEventListener("click", function () {
    state.strokes = [];
    state.currentStroke = [];
    redraw();
    syncForm();
  });

  audioButton?.addEventListener("click", function () {
    const audioUrl = host.dataset.tracingAudioUrl;
    if (audioUrl) new Audio(audioUrl).play();
  });

  form.addEventListener("submit", function (event) {
    syncForm();
    if (submitButton.disabled) {
      event.preventDefault();
    }
  });

  canvas.addEventListener("pointerdown", startStroke);
  canvas.addEventListener("pointermove", continueStroke);
  canvas.addEventListener("pointerup", finishStroke);
  canvas.addEventListener("pointercancel", finishStroke);
  syncForm();
})();
