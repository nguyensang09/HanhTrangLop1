(function () {
  const host = document.querySelector(".tracing-canvas-host");
  const form = document.querySelector(".tracing-submit-form");
  if (!host || !form) {
    return;
  }

  const canvas = host.querySelector(".tracing-canvas");
  const context = canvas.getContext("2d");
  const undoButton = document.querySelector(".tracing-undo");
  const clearButton = document.querySelector(".tracing-clear");
  const audioButton = document.querySelector(".tracing-audio");
  const submitButton = form.querySelector(".tracing-submit-button");
  const strokesInput = form.querySelector(".tracing-strokes-input");
  const metricsInput = form.querySelector(".tracing-metrics-input");
  const statusText = form.querySelector(".tracing-status");
  const tracingSymbol = String(host.dataset.tracingSymbol || "").trim();

  // Render guide SVG overlay
  const overlay = host.querySelector(".tracing-guide-overlay");
  window.tracingGuides?.renderTracingGuide(overlay, tracingSymbol);

  const state = {
    drawing: false,
    strokes: [],
    currentStroke: []
  };

  const CORRIDOR_RADIUS = 20; // Max distance from guideline to allow ink
  const COVERAGE_RADIUS = 15; // Radius to mark a checkpoint as "covered"

  function getCheckpoints() {
    return window.tracingGuides?.getGuideCheckpoints(overlay) || [];
  }

  function findClosestCheckpoint(p, checkpoints) {
    if (!checkpoints || !checkpoints.length) {
      return { inCorridor: true, penWidth: 10, closestCheckpoint: null };
    }
    let minDistanceSq = Infinity;
    let closestCp = null;

    for (let i = 0; i < checkpoints.length; i += 1) {
      const cp = checkpoints[i];
      const dx = p.x - cp.x;
      const dy = p.y - cp.y;
      const distSq = dx * dx + dy * dy;
      if (distSq < minDistanceSq) {
        minDistanceSq = distSq;
        closestCp = cp;
      }
    }

    const corridorRad = closestCp?.corridorRadius || 20;
    const inCorridor = minDistanceSq <= corridorRad * corridorRad;
    return {
      inCorridor,
      penWidth: closestCp?.penWidth || 10,
      closestCheckpoint: closestCp
    };
  }

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
    context.strokeStyle = "#10b981"; // Fresh emerald green

    for (let index = 1; index < points.length; index += 1) {
      const p1 = points[index - 1];
      const p2 = points[index];
      context.lineWidth = p2.penWidth || p1.penWidth || 10;
      context.beginPath();
      context.moveTo(p1.x, p1.y);
      context.lineTo(p2.x, p2.y);
      context.stroke();
    }
  }

  function redraw() {
    context.clearRect(0, 0, canvas.width, canvas.height);
    state.strokes.forEach(drawStroke);
    drawStroke(state.currentStroke);
  }

  function getAllDrawnPoints() {
    const all = [];
    state.strokes.forEach(s => s.forEach(p => all.push(p)));
    state.currentStroke.forEach(p => all.push(p));
    return all;
  }

  function buildMetrics() {
    const drawnPoints = getAllDrawnPoints();
    const checkpoints = getCheckpoints();

    if (!checkpoints.length) {
      const minPoints = Number(host.dataset.tracingMinPoints || 20);
      const completed = drawnPoints.length >= minPoints;
      return {
        source: "tracing_canvas_fallback",
        totalPoints: drawnPoints.length,
        totalStrokes: 1,
        completedStrokes: completed ? 1 : 0,
        coverageScore: completed ? 100 : Math.round((drawnPoints.length / minPoints) * 100),
        completed
      };
    }

    // Group checkpoints by strokeIndex
    const strokeCheckpointsMap = new Map();
    checkpoints.forEach((cp, idx) => {
      const sIdx = cp.strokeIndex ?? 0;
      if (!strokeCheckpointsMap.has(sIdx)) {
        strokeCheckpointsMap.set(sIdx, []);
      }
      strokeCheckpointsMap.get(sIdx).push({ ...cp, globalIndex: idx });
    });

    const totalStrokes = Math.max(1, strokeCheckpointsMap.size);
    const coveredSet = new Set();

    drawnPoints.forEach(p => {
      for (let i = 0; i < checkpoints.length; i += 1) {
        const cp = checkpoints[i];
        const dx = p.x - cp.x;
        const dy = p.y - cp.y;
        const coverRadius = Math.max(12, (cp.corridorRadius || 20) * 0.85);
        if (dx * dx + dy * dy <= coverRadius * coverRadius) {
          coveredSet.add(i);
        }
      }
    });

    // Check each stroke completion
    let completedStrokes = 0;
    strokeCheckpointsMap.forEach((cps) => {
      const totalInStroke = cps.length;
      let coveredInStroke = 0;
      cps.forEach(cp => {
        if (coveredSet.has(cp.globalIndex)) {
          coveredInStroke += 1;
        }
      });

      const strokeCoverage = totalInStroke === 0 ? 1 : (coveredInStroke / totalInStroke);
      if (strokeCoverage >= 0.65) {
        completedStrokes += 1;
      }
    });

    const totalCheckpoints = checkpoints.length;
    const coveredCount = coveredSet.size;
    const coverageScore = Math.min(100, Math.round((coveredCount / Math.max(1, totalCheckpoints)) * 100));

    // Completed only when 100% of the individual strokes are completed
    const completed = completedStrokes >= totalStrokes;

    return {
      source: "tracing_strict_multitier_v5",
      strokeCount: state.strokes.length,
      totalStrokes,
      completedStrokes,
      totalPoints: drawnPoints.length,
      totalCheckpoints,
      coveredCheckpoints: coveredCount,
      coverageScore,
      completed
    };
  }

  function syncForm() {
    const metrics = buildMetrics();
    strokesInput.value = JSON.stringify(state.strokes);
    metricsInput.value = JSON.stringify(metrics);
    submitButton.disabled = !metrics.completed;

    if (statusText) {
      statusText.classList.toggle("ready", metrics.completed);

      if (metrics.completed) {
        statusText.textContent = "Xuất sắc! Bé đã hoàn thành bài tô trên trang vở 🎉";
      } else if (metrics.completedStrokes > 0) {
        statusText.textContent = "Bé cố gắng hoàn thành các hàng chữ nhé! ✨";
      } else {
        statusText.textContent = "Bé hãy tô theo từng nét đứt trên trang vở nhé! ✨";
      }
    }
  }

  function startStroke(event) {
    event.preventDefault();
    const pt = pointFromEvent(event);
    const cps = getCheckpoints();
    const match = findClosestCheckpoint(pt, cps);

    // Strict boundary: Only start stroke if pointer is inside the stroke corridor!
    if (!match.inCorridor) {
      state.drawing = false;
      return;
    }

    pt.penWidth = match.penWidth;
    state.drawing = true;
    state.currentStroke = [pt];
    canvas.setPointerCapture(event.pointerId);
    redraw();
    syncForm();
  }

  function continueStroke(event) {
    if (!state.drawing) {
      return;
    }

    event.preventDefault();
    const pt = pointFromEvent(event);
    const cps = getCheckpoints();
    const match = findClosestCheckpoint(pt, cps);

    // Strict boundary: Only record point if within corridor!
    if (match.inCorridor) {
      pt.penWidth = match.penWidth;
      state.currentStroke.push(pt);
      redraw();
      syncForm();
    }
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

  undoButton?.addEventListener("click", function () {
    state.strokes.pop();
    redraw();
    syncForm();
  });

  clearButton?.addEventListener("click", function () {
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
