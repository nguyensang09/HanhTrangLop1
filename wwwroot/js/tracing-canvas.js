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
  const demoButton = document.querySelector(".tracing-demo");
  const submitButton = form.querySelector(".tracing-submit-button");
  const strokesInput = form.querySelector(".tracing-strokes-input");
  const metricsInput = form.querySelector(".tracing-metrics-input");
  const statusText = form.querySelector(".tracing-status");
  const tracingSymbol = String(host.dataset.tracingSymbol || "").trim();

  // Render guide SVG overlay
  const overlay = host.querySelector(".tracing-guide-overlay");
  window.tracingGuides?.renderTracingGuide(overlay, tracingSymbol);

  const isPicture = host.dataset.isPicture === "true" ||
                    overlay?.getAttribute("data-is-picture") === "true" ||
                    overlay?._isPicture === true ||
                    window.tracingGuides?.isPictureSymbol?.(tracingSymbol);

  if (isPicture && demoButton) {
    demoButton.style.display = "none";
  }

  let demoAnimationId = null;
  let isDemoRunning = false;

  const state = {
    drawing: false,
    activeStrokeIndex: null,
    lastValidPoint: null,
    strokes: [],
    currentStroke: []
  };

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

    const corridorRad = Math.max(22, closestCp?.corridorRadius || 24);
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
    if (!points || points.length < 2) {
      return;
    }

    context.lineCap = "round";
    context.lineJoin = "round";
    context.strokeStyle = "#10b981"; // Fresh emerald green for child's ink

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
        const coverRadius = Math.max(16, (cp.corridorRadius || 20) * 0.9);
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
      if (strokeCoverage >= 0.55) {
        completedStrokes += 1;
      }
    });

    const totalCheckpoints = checkpoints.length;
    const coveredCount = coveredSet.size;
    const coverageScore = Math.min(100, Math.round((coveredCount / Math.max(1, totalCheckpoints)) * 100));

    // Bé hoàn thành khi đã tô đủ các nét và có nét vẽ trên trang vở
    const completed = (completedStrokes >= totalStrokes || coverageScore >= 60) && state.strokes.length >= 1;

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

  function stopStrokeDemo() {
    if (demoAnimationId) {
      cancelAnimationFrame(demoAnimationId);
      demoAnimationId = null;
    }
    isDemoRunning = false;
    const oldCursor = host.querySelector(".tracing-demo-cursor");
    if (oldCursor) oldCursor.remove();
    const oldLayer = host.querySelector(".tracing-demo-layer");
    if (oldLayer) oldLayer.remove();
    if (demoButton) demoButton.classList.remove("is-playing");
  }

  function playStrokeDemo() {
    stopStrokeDemo();
    if (isPicture) return;

    const allCenterlines = [...overlay.querySelectorAll(".tracing-guide-centerline")];
    if (!allCenterlines.length) return;

    // Lấy các nét thuộc hàng mẫu đầu tiên (data-tier="bold")
    let centerlines = allCenterlines.filter(cl => cl.getAttribute("data-tier") === "bold");
    if (!centerlines.length) {
      centerlines = allCenterlines.slice(0, Math.min(2, allCenterlines.length));
    }
    if (!centerlines.length) return;

    isDemoRunning = true;
    if (demoButton) demoButton.classList.add("is-playing");

    let demoSvg = host.querySelector(".tracing-demo-layer");
    if (!demoSvg) {
      demoSvg = document.createElementNS("http://www.w3.org/2000/svg", "svg");
      demoSvg.setAttribute("class", "tracing-demo-layer");
      demoSvg.setAttribute("viewBox", overlay.querySelector("svg")?.getAttribute("viewBox") || "0 0 920 1200");
      host.appendChild(demoSvg);
    }
    demoSvg.replaceChildren();

    const cursor = document.createElement("div");
    cursor.className = "tracing-demo-cursor";
    cursor.innerHTML = `
      <div class="demo-pencil-wrap">
        <span class="demo-pencil-icon">✏️</span>
        <span class="demo-sparkle">✨</span>
      </div>
    `;
    host.appendChild(cursor);

    const strokePaths = centerlines.map((cl) => {
      const len = cl.getTotalLength();
      const pathClone = document.createElementNS("http://www.w3.org/2000/svg", "path");
      pathClone.setAttribute("d", cl.getAttribute("d"));
      pathClone.setAttribute("fill", "none");
      pathClone.setAttribute("stroke", "#2563eb");
      pathClone.setAttribute("stroke-width", "16");
      pathClone.setAttribute("stroke-linecap", "round");
      pathClone.setAttribute("stroke-linejoin", "round");
      pathClone.setAttribute("stroke-dasharray", `${len} ${len}`);
      pathClone.setAttribute("stroke-dashoffset", String(len));
      pathClone.style.filter = "drop-shadow(0 0 8px rgba(37, 99, 235, 0.7))";
      demoSvg.appendChild(pathClone);
      return { path: cl, clone: pathClone, length: len };
    });

    let currentStrokeIdx = 0;
    let strokeProgress = 0;
    const speed = 5.0;

    function updateCursorPosition(pt) {
      const viewBox = (overlay.querySelector("svg")?.getAttribute("viewBox") || "0 0 920 1200").split(" ").map(Number);
      const vbW = viewBox[2] || 920;
      const vbH = viewBox[3] || 1200;
      const percentX = (pt.x / vbW) * 100;
      const percentY = (pt.y / vbH) * 100;
      cursor.style.left = `${percentX}%`;
      cursor.style.top = `${percentY}%`;
    }

    function animateStep() {
      if (!isDemoRunning) return;

      if (currentStrokeIdx >= strokePaths.length) {
        // Tô xong hàng mẫu -> Tự động mờ dần và xóa bỏ lớp vẽ mẫu sạch sẽ
        setTimeout(() => {
          if (isDemoRunning) {
            demoSvg.style.transition = "opacity 0.4s ease";
            demoSvg.style.opacity = "0";
            cursor.style.transition = "opacity 0.3s ease";
            cursor.style.opacity = "0";
            setTimeout(stopStrokeDemo, 400);
          }
        }, 500);
        return;
      }

      const item = strokePaths[currentStrokeIdx];
      strokeProgress += speed;

      if (strokeProgress >= item.length) {
        item.clone.setAttribute("stroke-dashoffset", "0");
        const endPt = item.path.getPointAtLength(item.length);
        updateCursorPosition(endPt);

        currentStrokeIdx += 1;
        strokeProgress = 0;
        setTimeout(() => {
          if (isDemoRunning) {
            demoAnimationId = requestAnimationFrame(animateStep);
          }
        }, 180);
        return;
      }

      item.clone.setAttribute("stroke-dashoffset", String(item.length - strokeProgress));
      const pt = item.path.getPointAtLength(strokeProgress);
      updateCursorPosition(pt);

      demoAnimationId = requestAnimationFrame(animateStep);
    }

    demoAnimationId = requestAnimationFrame(animateStep);
  }

  function startStroke(event) {
    stopStrokeDemo();
    const pt = pointFromEvent(event);
    const cps = getCheckpoints();
    const match = findClosestCheckpoint(pt, cps);

    if (!match.inCorridor) {
      state.drawing = false;
      return;
    }

    if (event.cancelable) {
      event.preventDefault();
    }
    pt.penWidth = match.penWidth;
    state.drawing = true;
    state.activeStrokeIndex = match.closestCheckpoint?.strokeIndex ?? null;
    state.lastValidPoint = pt;
    state.currentStroke = [pt];
    canvas.setPointerCapture(event.pointerId);
    redraw();
    syncForm();
  }

  function continueStroke(event) {
    if (!state.drawing) {
      return;
    }

    if (event.cancelable) {
      event.preventDefault();
    }
    const pt = pointFromEvent(event);
    const cps = getCheckpoints();
    const match = findClosestCheckpoint(pt, cps);

    // Nếu con trỏ kéo ra ngoài phạm vi nét vẽ (khoảng trắng giữa các chữ):
    if (!match.inCorridor) {
      // Lập tức ngắt nét vẽ hiện tại để không tạo đường nối nhảy cóc
      if (state.currentStroke.length > 1) {
        state.strokes.push(state.currentStroke);
      }
      state.currentStroke = [];
      state.drawing = false;
      state.activeStrokeIndex = null;
      state.lastValidPoint = null;
      redraw();
      syncForm();
      return;
    }

    // Kiểm tra xem có bị nhảy sang chữ/nét khác hoặc khoảng cách quá xa không
    const currentStrokeIdx = match.closestCheckpoint?.strokeIndex ?? null;
    const lastPt = state.lastValidPoint || state.currentStroke[state.currentStroke.length - 1];
    let distanceSq = 0;
    if (lastPt) {
      const dx = pt.x - lastPt.x;
      const dy = pt.y - lastPt.y;
      distanceSq = dx * dx + dy * dy;
    }

    const MAX_GAP_SQ = 45 * 45; // Tối đa 45px khoảng cách giữa 2 điểm liên tiếp trong 1 nét

    if (state.activeStrokeIndex !== null && currentStrokeIdx !== null && state.activeStrokeIndex !== currentStrokeIdx) {
      // Đã di chuyển sang một chữ / nét khác -> Ngắt nét cũ, tạo nét mới độc lập
      if (state.currentStroke.length > 1) {
        state.strokes.push(state.currentStroke);
      }
      pt.penWidth = match.penWidth;
      state.activeStrokeIndex = currentStrokeIdx;
      state.lastValidPoint = pt;
      state.currentStroke = [pt];
      redraw();
      syncForm();
      return;
    }

    if (distanceSq > MAX_GAP_SQ) {
      // Bước nhảy quá xa -> Ngắt nét cũ, tạo nét mới
      if (state.currentStroke.length > 1) {
        state.strokes.push(state.currentStroke);
      }
      pt.penWidth = match.penWidth;
      state.activeStrokeIndex = currentStrokeIdx;
      state.lastValidPoint = pt;
      state.currentStroke = [pt];
      redraw();
      syncForm();
      return;
    }

    // Nét vẽ hợp lệ trong cùng một chữ
    pt.penWidth = match.penWidth;
    state.lastValidPoint = pt;
    state.currentStroke.push(pt);
    redraw();
    syncForm();
  }

  function finishStroke(event) {
    if (!state.drawing && !state.currentStroke.length) {
      return;
    }

    if (event && event.cancelable) {
      event.preventDefault();
    }
    state.drawing = false;
    state.activeStrokeIndex = null;
    state.lastValidPoint = null;
    if (state.currentStroke.length > 1) {
      state.strokes.push(state.currentStroke);
    }

    state.currentStroke = [];
    redraw();
    syncForm();
  }

  undoButton?.addEventListener("click", function () {
    stopStrokeDemo();
    state.strokes.pop();
    redraw();
    syncForm();
  });

  clearButton?.addEventListener("click", function () {
    stopStrokeDemo();
    state.strokes = [];
    state.currentStroke = [];
    redraw();
    syncForm();
  });

  demoButton?.addEventListener("click", function () {
    playStrokeDemo();
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
  canvas.addEventListener("pointerleave", finishStroke);

  // Allow trackpad / mouse wheel to scroll page over canvas
  canvas.addEventListener("wheel", function (event) {
    window.scrollBy({ top: event.deltaY, left: event.deltaX, behavior: "auto" });
  }, { passive: true });

  // Initial draw & sync
  redraw();
  syncForm();

  // Auto-play demo once when entering the lesson (only for letters/numbers/strokes, NOT for picture art tracing)
  window.setTimeout(function () {
    if (!isPicture && !state.strokes.length && !state.drawing) {
      playStrokeDemo();
    }
  }, 500);
})();
