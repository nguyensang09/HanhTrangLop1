(function () {
  const namespace = "http://www.w3.org/2000/svg";

  function normalizeSymbol(symbol) {
    return String(symbol || "")
      .normalize("NFD")
      .replace(/[\u0300-\u036f]/g, "")
      .replace(/đ/g, "d")
      .replace(/Đ/g, "D")
      .trim();
  }

  const strokePalette = [
    { color: "#ff8c42", dark: "#d95d08", name: "orange" },
    { color: "#0288d1", dark: "#01579b", name: "cyan" },
    { color: "#00897b", dark: "#004d40", name: "teal" },
    { color: "#8e24aa", dark: "#4a148c", name: "purple" }
  ];

  function guideStrokesFor(symbol) {
    const raw = String(symbol || "").trim();
    const normalized = normalizeSymbol(raw);
    const textLower = normalized.toLowerCase();
    const upper = normalized.toUpperCase();

    // 1. Kiểm tra nhanh theo tên nét tiếng Việt hoặc ký hiệu đặc biệt
    if (textLower.includes("ngang") || raw === "─" || raw === "-") {
      return [["M", 180, 365, "L", 540, 365]];
    }
    if (textLower.includes("xien trai") || textLower.includes("xientrai") || raw === "/") {
      return [["M", 490, 170, "L", 230, 565]];
    }
    if (textLower.includes("xien phai") || textLower.includes("xienphai") || raw === "\\") {
      return [["M", 230, 170, "L", 490, 565]];
    }
    if (textLower.includes("cong kin") || textLower.includes("vong") || raw === "○") {
      return [["M", 360, 170, "C", 220, 170, 200, 560, 360, 560, "C", 520, 560, 500, 170, 360, 170]];
    }
    if (textLower.includes("cong trai") || textLower.includes("cong ho phai") || raw === "(") {
      return [["M", 480, 210, "C", 380, 150, 240, 220, 240, 365, "C", 240, 510, 380, 580, 480, 520]];
    }
    if (textLower.includes("cong phai") || textLower.includes("cong ho trai") || raw === ")") {
      return [["M", 240, 210, "C", 340, 150, 480, 220, 480, 365, "C", 480, 510, 340, 580, 240, 520]];
    }
    if (textLower.includes("moc hai dau") || textLower.includes("moc 2 dau")) {
      return [["M", 240, 240, "C", 260, 170, 360, 170, 360, 365, "C", 360, 565, 460, 565, 480, 490]];
    }
    if (textLower.includes("moc xuoi") || (raw === "J" && !guides["J"])) {
      return [["M", 250, 230, "C", 270, 170, 390, 170, 390, 250, "L", 390, 565]];
    }
    if (textLower.includes("moc nguoc")) {
      return [["M", 330, 170, "L", 330, 485, "C", 330, 575, 470, 575, 470, 485]];
    }
    if (textLower.includes("khuyet tren") || raw === "ℓ") {
      return [["M", 330, 365, "L", 430, 200, "C", 470, 140, 370, 130, 360, 200, "L", 360, 565]];
    }
    if (textLower.includes("khuyet duoi") || raw === "ɟ") {
      return [["M", 360, 170, "L", 360, 510, "C", 360, 640, 250, 630, 270, 540, "L", 450, 365]];
    }
    if (textLower.includes("that") || raw === "∞") {
      return [["M", 300, 170, "L", 300, 565], ["M", 300, 390, "C", 420, 330, 460, 410, 390, 440, "L", 480, 565]];
    }
    if (textLower.includes("luon") || raw === "~") {
      return [["M", 200, 365, "C", 280, 310, 340, 420, 420, 365, "C", 470, 330, 510, 350, 540, 365]];
    }
    if (textLower.includes("doc") || textLower.includes("so") || raw === "│" || raw === "|") {
      return [["M", 360, 170, "L", 360, 565]];
    }

    const guides = {
      "0": [["M", 360, 165, "C", 240, 165, 195, 260, 195, 370, "C", 195, 500, 255, 565, 360, 565, "C", 465, 565, 525, 500, 525, 370, "C", 525, 260, 480, 165, 360, 165]],
      "1": [["M", 280, 250, "L", 360, 170, "L", 360, 570]],
      "2": [["M", 225, 260, "C", 235, 155, 495, 155, 495, 275, "C", 495, 385, 330, 440, 225, 565, "L", 505, 565]],
      "3": [["M", 240, 185, "C", 390, 140, 495, 205, 430, 325, "C", 545, 395, 485, 565, 265, 545]],
      "4": [["M", 455, 155, "L", 220, 430, "L", 520, 430], ["M", 455, 155, "L", 455, 575]],
      "5": [["M", 475, 175, "L", 285, 175, "L", 260, 330], ["M", 260, 330, "C", 470, 290, 525, 555, 265, 555]],
      "6": [["M", 475, 205, "C", 315, 145, 215, 310, 235, 485, "C", 265, 625, 495, 605, 475, 465, "C", 455, 345, 260, 365, 235, 485]],
      "7": [["M", 215, 170, "L", 505, 170, "L", 325, 575]],
      "8": [["M", 360, 340, "C", 225, 260, 275, 155, 395, 175, "C", 515, 195, 490, 340, 360, 340], ["M", 360, 340, "C", 210, 415, 250, 575, 395, 555, "C", 520, 535, 510, 395, 360, 340]],
      "9": [["M", 465, 325, "C", 435, 200, 245, 200, 240, 340, "C", 235, 455, 415, 455, 465, 325], ["M", 465, 325, "C", 480, 430, 435, 530, 305, 565]],
      "A": [["M", 360, 160, "L", 215, 575], ["M", 360, 160, "L", 505, 575], ["M", 265, 435, "L", 455, 435]],
      "Ă": [["M", 360, 195, "L", 215, 605], ["M", 360, 195, "L", 505, 605], ["M", 265, 465, "L", 455, 465], ["M", 295, 125, "C", 325, 175, 395, 175, 425, 125]],
      "Â": [["M", 360, 205, "L", 215, 605], ["M", 360, 205, "L", 505, 605], ["M", 265, 475, "L", 455, 475], ["M", 295, 150, "L", 360, 95], ["M", 360, 95, "L", 425, 150]],
      "B": [["M", 265, 165, "L", 265, 570], ["M", 265, 165, "C", 465, 165, 475, 310, 275, 340], ["M", 275, 340, "C", 495, 355, 480, 560, 265, 570]],
      "C": [["M", 495, 230, "C", 425, 160, 315, 155, 245, 225, "C", 165, 305, 185, 485, 285, 545, "C", 360, 585, 450, 565, 505, 505]],
      "D": [["M", 265, 165, "L", 265, 570], ["M", 265, 165, "C", 495, 180, 535, 505, 265, 570]],
      "Đ": [["M", 265, 165, "L", 265, 570], ["M", 265, 165, "C", 495, 180, 535, 505, 265, 570], ["M", 185, 340, "L", 355, 340]],
      "E": [["M", 245, 165, "L", 245, 575], ["M", 245, 165, "L", 495, 165], ["M", 245, 365, "L", 445, 365], ["M", 245, 575, "L", 495, 575]],
      "Ê": [["M", 245, 205, "L", 245, 605], ["M", 245, 205, "L", 495, 205], ["M", 245, 405, "L", 445, 405], ["M", 245, 605, "L", 495, 605], ["M", 310, 150, "L", 370, 95], ["M", 370, 95, "L", 430, 150]],
      "F": [["M", 245, 165, "L", 245, 575], ["M", 245, 165, "L", 495, 165], ["M", 245, 365, "L", 435, 365]],
      "G": [["M", 495, 230, "C", 425, 160, 315, 155, 245, 225, "C", 165, 305, 185, 485, 285, 545, "C", 365, 590, 495, 540, 495, 450, "L", 495, 395, "L", 395, 395]],
      "H": [["M", 240, 165, "L", 240, 575], ["M", 480, 165, "L", 480, 575], ["M", 240, 365, "L", 480, 365]],
      "I": [["M", 360, 165, "L", 360, 575]],
      "J": [["M", 460, 165, "L", 460, 470, "C", 460, 585, 260, 585, 260, 470]],
      "K": [["M", 245, 165, "L", 245, 575], ["M", 495, 165, "L", 250, 370], ["M", 250, 370, "L", 505, 575]],
      "L": [["M", 265, 165, "L", 265, 575, "L", 505, 575]],
      "M": [["M", 215, 575, "L", 215, 165, "L", 360, 420, "L", 505, 165, "L", 505, 575]],
      "N": [["M", 225, 575, "L", 225, 165, "L", 495, 575, "L", 495, 165]],
      "O": [["M", 360, 165, "C", 240, 165, 195, 260, 195, 370, "C", 195, 500, 255, 565, 360, 565, "C", 465, 565, 525, 500, 525, 370, "C", 525, 260, 480, 165, 360, 165]],
      "Ô": [["M", 360, 205, "C", 240, 205, 195, 300, 195, 410, "C", 195, 540, 255, 605, 360, 605, "C", 465, 605, 525, 540, 525, 410, "C", 525, 300, 480, 205, 360, 205], ["M", 300, 150, "L", 360, 95], ["M", 360, 95, "L", 420, 150]],
      "Ơ": [["M", 360, 165, "C", 240, 165, 195, 260, 195, 370, "C", 195, 500, 255, 565, 360, 565, "C", 465, 565, 525, 500, 525, 370, "C", 525, 260, 480, 165, 360, 165], ["M", 480, 215, "C", 515, 165, 545, 185, 520, 240]],
      "P": [["M", 265, 165, "L", 265, 570], ["M", 265, 165, "C", 495, 165, 495, 365, 265, 355]],
      "Q": [["M", 360, 165, "C", 240, 165, 195, 260, 195, 370, "C", 195, 500, 255, 565, 360, 565, "C", 465, 565, 525, 500, 525, 370, "C", 525, 260, 480, 165, 360, 165], ["M", 425, 495, "L", 505, 575]],
      "R": [["M", 265, 165, "L", 265, 570], ["M", 265, 165, "C", 495, 165, 495, 355, 265, 355], ["M", 360, 355, "L", 495, 570]],
      "S": [["M", 485, 215, "C", 330, 130, 195, 235, 295, 345, "C", 455, 515, 340, 605, 215, 535]],
      "T": [["M", 185, 165, "L", 535, 165], ["M", 360, 165, "L", 360, 575]],
      "U": [["M", 225, 165, "L", 225, 450, "C", 225, 615, 495, 615, 495, 450, "L", 495, 165]],
      "Ư": [["M", 225, 165, "L", 225, 450, "C", 225, 615, 495, 615, 495, 450, "L", 495, 165], ["M", 495, 165, "C", 535, 120, 565, 145, 535, 205]],
      "V": [["M", 215, 165, "L", 360, 575, "L", 505, 165]],
      "X": [["M", 225, 165, "L", 495, 575], ["M", 495, 165, "L", 225, 575]],
      "Y": [["M", 215, 165, "L", 360, 350, "L", 505, 165], ["M", 360, 350, "L", 360, 575]],
      "a": [["M", 455, 340, "C", 425, 255, 265, 255, 240, 390, "C", 215, 535, 410, 565, 460, 440], ["M", 460, 290, "L", 460, 555]],
      "ă": [["M", 455, 360, "C", 425, 275, 265, 275, 240, 410, "C", 215, 555, 410, 585, 460, 460], ["M", 460, 310, "L", 460, 575], ["M", 305, 220, "C", 330, 265, 390, 265, 415, 220]],
      "â": [["M", 455, 360, "C", 425, 275, 265, 275, 240, 410, "C", 215, 555, 410, 585, 460, 460], ["M", 460, 310, "L", 460, 575], ["M", 310, 235, "L", 360, 185], ["M", 360, 185, "L", 410, 235]],
      "b": [["M", 260, 165, "L", 260, 555], ["M", 260, 385, "C", 320, 265, 495, 275, 500, 420, "C", 505, 560, 330, 575, 260, 450]],
      "c": [["M", 480, 335, "C", 400, 255, 245, 290, 230, 420, "C", 215, 550, 400, 585, 495, 495]],
      "d": [["M", 455, 385, "C", 395, 265, 220, 275, 215, 420, "C", 210, 560, 385, 575, 455, 450], ["M", 455, 165, "L", 455, 555]],
      "đ": [["M", 455, 385, "C", 395, 265, 220, 275, 215, 420, "C", 210, 560, 385, 575, 455, 450], ["M", 455, 165, "L", 455, 555], ["M", 380, 280, "L", 530, 280]],
      "e": [["M", 235, 420, "L", 495, 420, "C", 495, 285, 270, 255, 230, 410, "C", 195, 545, 385, 595, 495, 495]],
      "ê": [["M", 235, 435, "L", 495, 435, "C", 495, 300, 270, 270, 230, 425, "C", 195, 560, 385, 605, 495, 510], ["M", 310, 235, "L", 360, 185], ["M", 360, 185, "L", 410, 235]],
      "g": [["M", 450, 320, "C", 400, 255, 255, 275, 235, 410, "C", 215, 535, 385, 560, 450, 450], ["M", 450, 315, "L", 450, 575, "C", 435, 655, 280, 655, 250, 595]],
      "h": [["M", 245, 165, "L", 245, 555], ["M", 245, 395, "C", 320, 280, 475, 300, 475, 555]],
      "i": [["M", 360, 315, "L", 360, 555], ["M", 360, 210, "L", 360, 218]],
      "k": [["M", 250, 165, "L", 250, 555], ["M", 465, 305, "L", 255, 430, "L", 480, 555]],
      "l": [["M", 360, 165, "L", 360, 555]],
      "m": [["M", 195, 555, "L", 195, 315], ["M", 195, 400, "C", 250, 285, 340, 310, 340, 555], ["M", 340, 400, "C", 395, 285, 495, 310, 495, 555]],
      "n": [["M", 235, 555, "L", 235, 315], ["M", 235, 400, "C", 310, 285, 475, 300, 475, 555]],
      "o": [["M", 360, 285, "C", 240, 285, 220, 555, 360, 555, "C", 500, 555, 480, 285, 360, 285]],
      "ô": [["M", 360, 310, "C", 240, 310, 220, 575, 360, 575, "C", 500, 575, 480, 310, 360, 310], ["M", 310, 250, "L", 360, 200], ["M", 360, 200, "L", 410, 250]],
      "ơ": [["M", 360, 285, "C", 240, 285, 220, 555, 360, 555, "C", 500, 555, 480, 285, 360, 285], ["M", 470, 315, "C", 505, 265, 535, 285, 510, 340]],
      "p": [["M", 255, 640, "L", 255, 315], ["M", 255, 390, "C", 315, 265, 495, 285, 495, 425, "C", 495, 555, 315, 570, 255, 450]],
      "q": [["M", 450, 390, "C", 390, 265, 215, 285, 220, 425, "C", 225, 555, 395, 570, 450, 450], ["M", 450, 315, "L", 450, 640]],
      "r": [["M", 260, 555, "L", 260, 315], ["M", 260, 405, "C", 320, 300, 400, 300, 450, 330]],
      "s": [["M", 470, 335, "C", 355, 265, 230, 335, 335, 420, "C", 465, 515, 345, 585, 225, 515]],
      "t": [["M", 360, 215, "L", 360, 520, "C", 365, 565, 425, 570, 465, 530], ["M", 285, 315, "L", 455, 315]],
      "u": [["M", 235, 315, "L", 235, 470, "C", 235, 585, 450, 580, 450, 470, "L", 450, 315], ["M", 450, 470, "L", 450, 555]],
      "ư": [["M", 235, 315, "L", 235, 470, "C", 235, 585, 450, 580, 450, 470, "L", 450, 315], ["M", 450, 470, "L", 450, 555], ["M", 450, 315, "C", 490, 270, 520, 295, 490, 355]],
      "v": [["M", 235, 315, "L", 360, 555, "L", 485, 315]],
      "x": [["M", 245, 315, "L", 475, 555], ["M", 475, 315, "L", 245, 555]],
      "y": [["M", 235, 315, "L", 360, 555], ["M", 480, 315, "L", 360, 595, "C", 325, 655, 250, 650, 225, 605]]
    };

    if (guides[raw]) {
      return guides[raw];
    }
    if (guides[upper]) {
      return guides[upper];
    }
    if (guides[normalized]) {
      return guides[normalized];
    }

    return [["M", 360, 160, "L", 360, 575]];
  }

  function pathData(commands) {
    return commands.map((item) => typeof item === "number" ? String(item) : item).join(" ");
  }

  function pathPoint(path, ratio) {
    const length = path.getTotalLength();
    return path.getPointAtLength(Math.max(0, Math.min(length, length * ratio)));
  }

  function appendSvgElement(parent, name, attributes = {}, text = "") {
    const element = document.createElementNS(namespace, name);
    Object.entries(attributes).forEach(([key, value]) => element.setAttribute(key, String(value)));
    if (text) element.textContent = text;
    parent.append(element);
    return element;
  }

  function renderTracingGuide(target, symbol) {
    if (!target) return;

    const strokes = guideStrokesFor(symbol);
    const svg = document.createElementNS(namespace, "svg");
    svg.setAttribute("viewBox", "0 0 720 720");
    svg.classList.add("tracing-guide-svg");

    const defs = appendSvgElement(svg, "defs");

    // 1. Notebook Dotted Reference Lines (top, mid, bottom)
    const gridLayer = appendSvgElement(svg, "g", { class: "notebook-grid-lines" });
    [170, 365, 565].forEach((y) => {
      appendSvgElement(gridLayer, "line", {
        x1: "50",
        y1: String(y),
        x2: "670",
        y2: String(y),
        stroke: "#e4e3db",
        "stroke-width": "3",
        "stroke-dasharray": "12,12",
        "stroke-linecap": "round"
      });
    });

    const guideLayer = appendSvgElement(svg, "g", { class: "tracing-guide-layer" });

    // 2. Thick Ghost Background Stroke (Clear, light-grey track as in sample images)
    strokes.forEach((commands) => {
      appendSvgElement(guideLayer, "path", {
        d: pathData(commands),
        fill: "none",
        stroke: "#f0eee6",
        "stroke-width": "54",
        "stroke-linecap": "round",
        "stroke-linejoin": "round"
      });
    });

    // 3. Colored Guided Stroke Overlay with numbers and arrows (Like Image 2 & 3)
    strokes.forEach((commands, index) => {
      const palette = strokePalette[index % strokePalette.length];
      const markerId = `tracing-arrow-${index}-${Math.random().toString(36).slice(2)}`;
      
      const marker = appendSvgElement(defs, "marker", {
        id: markerId,
        viewBox: "0 0 12 12",
        refX: "9",
        refY: "6",
        markerWidth: "22",
        markerHeight: "22",
        markerUnits: "userSpaceOnUse",
        orient: "auto"
      });
      appendSvgElement(marker, "path", {
        d: "M 1 2 L 9 6 L 1 10 L 3 6 Z",
        fill: palette.dark
      });

      // Translucent colored stroke segment
      appendSvgElement(guideLayer, "path", {
        d: pathData(commands),
        fill: "none",
        stroke: palette.color,
        "stroke-width": "42",
        "stroke-linecap": "round",
        "stroke-linejoin": "round",
        opacity: "0.4"
      });

      // Animated dotted centerline with direction arrow
      const centerPath = appendSvgElement(guideLayer, "path", {
        d: pathData(commands),
        fill: "none",
        stroke: "#ffffff",
        "stroke-width": "8",
        "stroke-linecap": "round",
        "stroke-dasharray": "14,14",
        "marker-end": `url(#${markerId})`
      });

      // Start Circle Badge with Stroke Number (1, 2, 3...)
      const startPoint = pathPoint(centerPath, 0);
      
      // Pulse halo
      appendSvgElement(guideLayer, "circle", {
        cx: String(startPoint.x),
        cy: String(startPoint.y),
        r: "28",
        fill: palette.color,
        opacity: "0.28"
      });

      // Main Badge
      appendSvgElement(guideLayer, "circle", {
        cx: String(startPoint.x),
        cy: String(startPoint.y),
        r: "19",
        fill: palette.color,
        stroke: "#ffffff",
        "stroke-width": "3.5"
      });

      // Number label inside badge
      const numLabel = appendSvgElement(guideLayer, "text", {
        x: String(startPoint.x),
        y: String(startPoint.y + 7),
        "text-anchor": "middle",
        "font-family": "'Plus Jakarta Sans', 'Be Vietnam Pro', sans-serif",
        "font-size": "21px",
        "font-weight": "900",
        fill: "#ffffff"
      }, String(index + 1));

      // Touch hand guide at stroke 1 start
      if (index === 0) {
        const handGroup = appendSvgElement(guideLayer, "g", {
          transform: `translate(${startPoint.x + 18}, ${startPoint.y - 18})`,
          class: "touch-hand-guide"
        });
        appendSvgElement(handGroup, "circle", {
          cx: "0",
          cy: "0",
          r: "16",
          fill: "#51fac1",
          "fill-opacity": "0.45"
        });
        appendSvgElement(handGroup, "text", {
          x: "-9",
          y: "7",
          "font-family": "'Material Symbols Outlined'",
          "font-size": "18px",
          fill: "#007152"
        }, "touch_app");
      }
    });

    target.replaceChildren(svg);
  }

  window.tracingGuides = { renderTracingGuide, guideStrokesFor };
})();
