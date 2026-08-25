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

  function getBaseStrokesFor(symbol) {
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
    if (textLower.includes("moc xuoi")) {
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
      "j": [["M", 360, 315, "L", 360, 560, "C", 360, 650, 240, 650, 240, 560], ["M", 360, 210, "L", 360, 218]],
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

    // Multi-row Creative Tracing Story (Ảnh 2: Rùa và Dâu tây / Quả lê 3 hàng)
    if (textLower.includes("luon song") || textLower.includes("wave") || textLower.includes("rua") || raw === "wave-turtle-strawberry") {
      return [
        // Hàng 1: Sóng đều đặn (Rùa -> Dâu tây)
        ["M", 130, 170, "C", 165, 90, 205, 90, 240, 170, "C", 275, 250, 315, 250, 350, 170, "C", 385, 90, 425, 90, 460, 170, "C", 495, 250, 535, 250, 570, 170, "L", 600, 170],
        // Hàng 2: Sóng nhấp nhô cao thấp
        ["M", 130, 340, "C", 160, 250, 190, 250, 220, 340, "C", 240, 390, 260, 390, 280, 340, "C", 310, 250, 340, 250, 370, 340, "C", 390, 390, 410, 390, 430, 340, "C", 460, 250, 490, 250, 520, 340, "C", 540, 390, 560, 390, 580, 340, "L", 600, 340],
        // Hàng 3: Sóng lượn nghiêng (Rùa -> Quả lê)
        ["M", 130, 510, "C", 175, 430, 215, 430, 260, 510, "C", 305, 430, 345, 430, 390, 510, "C", 435, 430, 475, 430, 520, 510, "L", 600, 510]
      ];
    }
    if (textLower.includes("ziczac") || textLower.includes("rang cua") || textLower.includes("gap khuc")) {
      return [
        ["M", 100, 240, "L", 180, 120, "L", 260, 240, "L", 340, 120, "L", 420, 240, "L", 500, 120, "L", 580, 240],
        ["M", 100, 480, "L", 180, 360, "L", 260, 480, "L", 340, 360, "L", 420, 480, "L", 500, 360, "L", 580, 480]
      ];
    }
    if (textLower.includes("mua") || textLower.includes("rain") || raw === "rain-clouds") {
      return [
        ["M", 180, 280, "L", 180, 580],
        ["M", 280, 280, "L", 280, 600],
        ["M", 380, 280, "L", 380, 590],
        ["M", 480, 280, "L", 480, 570],
        ["M", 570, 280, "L", 570, 600]
      ];
    }
    if (textLower.includes("cuu") || textLower.includes("sheep")) {
      return [
        ["M", 360, 240, "C", 310, 210, 270, 260, 280, 310, "C", 240, 340, 240, 420, 290, 450, "C", 290, 520, 340, 580, 410, 580, "C", 460, 580, 510, 520, 500, 450, "C", 550, 410, 540, 330, 500, 300, "C", 500, 230, 430, 210, 360, 240]
      ];
    }
    if (textLower.includes("kien") || textLower.includes("ant")) {
      return [
        ["M", 200, 360, "C", 200, 310, 280, 310, 280, 360, "C", 280, 410, 200, 410, 200, 360],
        ["M", 280, 360, "C", 280, 320, 380, 320, 380, 360, "C", 380, 400, 280, 400, 280, 360],
        ["M", 380, 360, "C", 380, 290, 540, 290, 540, 360, "C", 540, 430, 380, 430, 380, 360]
      ];
    }
    if (textLower.includes("doi") || textLower.includes("bat")) {
      return [
        ["M", 360, 310, "C", 310, 210, 160, 180, 140, 310, "C", 210, 330, 270, 380, 320, 430],
        ["M", 360, 310, "C", 410, 210, 560, 180, 580, 310, "C", 510, 330, 450, 380, 400, 430],
        ["M", 330, 310, "L", 330, 470, "C", 330, 520, 390, 520, 390, 470, "L", 390, 310]
      ];
    }
    if (textLower.includes("non") || textLower.includes("hat")) {
      return [
        ["M", 230, 440, "L", 270, 210, "L", 450, 210, "L", 490, 440],
        ["M", 160, 440, "C", 160, 490, 560, 490, 560, 440, "C", 560, 410, 160, 410, 160, 440]
      ];
    }

    if (guides[raw]) {
      return guides[raw];
    }
    if (guides[upper]) {
      return guides[upper];
    }
    if (guides[normalized]) {
      return guides[normalized];
    }

    // Fallback single line
    return [["M", 360, 160, "L", 360, 575]];
  }

  function transformCommands(commands, scale, targetCenterX, targetBaselineY, baseCenterX = 360, baseBaselineY = 605) {
    const res = [];
    let i = 0;
    while (i < commands.length) {
      const cmd = commands[i];
      res.push(cmd);
      i += 1;
      if (cmd === "M" || cmd === "L") {
        const x = commands[i];
        const y = commands[i + 1];
        res.push(Math.round((x - baseCenterX) * scale + targetCenterX));
        res.push(Math.round((y - baseBaselineY) * scale + targetBaselineY));
        i += 2;
      } else if (cmd === "C") {
        const x1 = commands[i], y1 = commands[i + 1];
        const x2 = commands[i + 2], y2 = commands[i + 3];
        const x3 = commands[i + 4], y3 = commands[i + 5];
        res.push(Math.round((x1 - baseCenterX) * scale + targetCenterX));
        res.push(Math.round((y1 - baseBaselineY) * scale + targetBaselineY));
        res.push(Math.round((x2 - baseCenterX) * scale + targetCenterX));
        res.push(Math.round((y2 - baseBaselineY) * scale + targetBaselineY));
        res.push(Math.round((x3 - baseCenterX) * scale + targetCenterX));
        res.push(Math.round((y3 - baseBaselineY) * scale + targetBaselineY));
        i += 6;
      } else if (cmd === "Q") {
        const x1 = commands[i], y1 = commands[i + 1];
        const x2 = commands[i + 2], y2 = commands[i + 3];
        res.push(Math.round((x1 - baseCenterX) * scale + targetCenterX));
        res.push(Math.round((y1 - baseBaselineY) * scale + targetBaselineY));
        res.push(Math.round((x2 - baseCenterX) * scale + targetCenterX));
        res.push(Math.round((y2 - baseBaselineY) * scale + targetBaselineY));
        i += 4;
      } else if (typeof cmd === "number") {
        res.push(cmd);
      }
    }
    return res;
  }

  function generateWorksheetStrokes(baseStrokes) {
    const allStrokes = [];

    // Tầng 1: Chữ Siêu To Khổng Lồ (Hero Letter) ở trên cùng (baseline = 250, không bị cắt dấu)
    // 1. Chữ Siêu To Khổng Lồ (cx: 260, scale = 0.43, height ~ 220px)
    baseStrokes.forEach((stroke, strokeIdx) => {
      allStrokes.push({
        commands: transformCommands(stroke, 0.43, 260, 250),
        tier: "bold",
        ghostWidth: 38,
        corridorWidth: 30,
        centerlineWidth: 4.5,
        dashArray: "8,8",
        corridorRadius: 38,
        penWidth: 22,
        showBadge: true,
        badgeLabel: String(strokeIdx + 1)
      });
    });

    // 2. Chữ Lớn bên cạnh ở Tầng 1 (cx: 680, scale = 0.35, baseline = 250)
    baseStrokes.forEach((stroke) => {
      allStrokes.push({
        commands: transformCommands(stroke, 0.35, 680, 250),
        tier: "bold",
        ghostWidth: 28,
        corridorWidth: 22,
        centerlineWidth: 3.6,
        dashArray: "7,7",
        corridorRadius: 28,
        penWidth: 16,
        showBadge: false
      });
    });

    // Tầng 2 - Hàng 2 (Ô Ly Cỡ Lớn Vừa - 4 chữ rộng rãi, baseline = 480, scale = 0.32)
    [130, 350, 570, 790].forEach((cx) => {
      baseStrokes.forEach((stroke) => {
        allStrokes.push({
          commands: transformCommands(stroke, 0.32, cx, 480),
          tier: "medium",
          ghostWidth: 22,
          corridorWidth: 17,
          centerlineWidth: 3.0,
          dashArray: "6,6",
          corridorRadius: 22,
          penWidth: 13,
          showBadge: false
        });
      });
    });

    // Tầng 3 - Hàng 3 (Ô Ly Cỡ Vừa Lớp 1 - 5 chữ, baseline = 710, scale = 0.27)
    [100, 280, 460, 640, 820].forEach((cx) => {
      baseStrokes.forEach((stroke) => {
        allStrokes.push({
          commands: transformCommands(stroke, 0.27, cx, 710),
          tier: "medium",
          ghostWidth: 18,
          corridorWidth: 14,
          centerlineWidth: 2.6,
          dashArray: "5,5",
          corridorRadius: 19,
          penWidth: 11,
          showBadge: false
        });
      });
    });

    // Tầng 4 - Hàng 4 (Ô Ly Hạ Cỡ Chữ - 6 chữ, baseline = 940, scale = 0.22)
    [90, 238, 386, 534, 682, 830].forEach((cx) => {
      baseStrokes.forEach((stroke) => {
        allStrokes.push({
          commands: transformCommands(stroke, 0.22, cx, 940),
          tier: "fine",
          ghostWidth: 14,
          corridorWidth: 10,
          centerlineWidth: 2.0,
          dashArray: "4,4",
          corridorRadius: 16,
          penWidth: 8,
          showBadge: false
        });
      });
    });

    // Tầng 5 - Hàng 5 (Ô Ly Hạ Cỡ Chữ Nét Mảnh - 6 chữ, baseline = 1170, scale = 0.22)
    [90, 238, 386, 534, 682, 830].forEach((cx) => {
      baseStrokes.forEach((stroke) => {
        allStrokes.push({
          commands: transformCommands(stroke, 0.22, cx, 1170),
          tier: "fine",
          ghostWidth: 14,
          corridorWidth: 10,
          centerlineWidth: 2.0,
          dashArray: "4,4",
          corridorRadius: 16,
          penWidth: 8,
          showBadge: false
        });
      });
    });

    return allStrokes;
  }

  function guideStrokesFor(symbol) {
    const rawStrokes = getBaseStrokesFor(symbol);
    const raw = String(symbol || "").trim().toLowerCase();

    // If it's already a multi-row creative story pattern (lượn sóng, ziczac, mưa, viền thú), return directly
    if (raw.includes("luon") || raw.includes("wave") || raw.includes("ziczac") || raw.includes("mua") || raw.includes("rain") || raw.includes("cuu") || raw.includes("kien") || raw.includes("doi") || raw.includes("non")) {
      return rawStrokes.map((cmds, idx) => ({
        commands: cmds,
        tier: "creative",
        ghostWidth: 34,
        corridorWidth: 28,
        centerlineWidth: 4.5,
        dashArray: "8,8",
        corridorRadius: 30,
        penWidth: 16,
        showBadge: idx === 0,
        badgeLabel: "1"
      }));
    }

    // Otherwise, generate the full 5-row multi-tier Grade 1 handwriting worksheet (Ảnh 1)
    return generateWorksheetStrokes(rawStrokes);
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

  function extractCheckpoints(svgElement) {
    const checkpoints = [];
    if (!svgElement) return checkpoints;

    const paths = svgElement.querySelectorAll("path.tracing-guide-centerline");
    paths.forEach((path, strokeIndex) => {
      const length = path.getTotalLength();
      if (!length || isNaN(length)) return;
      const step = Number(path.dataset.step || 8);
      const corridorRadius = Number(path.dataset.corridorRadius || 18);
      const penWidth = Number(path.dataset.penWidth || 8);
      const count = Math.max(3, Math.floor(length / step));
      for (let i = 0; i <= count; i += 1) {
        const pt = path.getPointAtLength((i / count) * length);
        checkpoints.push({
          x: Math.round(pt.x),
          y: Math.round(pt.y),
          strokeIndex,
          corridorRadius,
          penWidth,
          covered: false
        });
      }
    });
    return checkpoints;
  }

  function renderTracingGuide(target, symbol) {
    if (!target) return;

    const strokeDefs = guideStrokesFor(symbol);
    const raw = String(symbol || "").trim().toLowerCase();
    const isCreative = raw.includes("luon") || raw.includes("wave") || raw.includes("ziczac") || raw.includes("mua") || raw.includes("rain") || raw.includes("cuu") || raw.includes("kien") || raw.includes("doi") || raw.includes("non");

    const svg = document.createElementNS(namespace, "svg");
    svg.setAttribute("viewBox", "0 0 920 1200");
    svg.classList.add("tracing-guide-svg");

    const defs = appendSvgElement(svg, "defs");

    // 1. Background Grid & Ô Ly
    const gridLayer = appendSvgElement(svg, "g", { class: "notebook-grid-lines" });

    if (isCreative) {
      // 3 horizontal guide bands for wave/creative stories
      [220, 560, 900].forEach((y) => {
        appendSvgElement(gridLayer, "line", {
          x1: "20",
          y1: String(y),
          x2: "900",
          y2: String(y),
          stroke: "#cbd5e1",
          "stroke-width": "1.5",
          "stroke-dasharray": "6,6"
        });
      });

      // Mascot & item decorations for wave story (Ảnh 2)
      if (raw.includes("luon") || raw.includes("wave") || raw.includes("rua")) {
        appendSvgElement(gridLayer, "text", { x: "40", y: "235", "font-size": "44px" }, "🐢");
        appendSvgElement(gridLayer, "text", { x: "840", y: "235", "font-size": "44px" }, "🍓");
        appendSvgElement(gridLayer, "text", { x: "840", y: "575", "font-size": "44px" }, "🐢");
        appendSvgElement(gridLayer, "text", { x: "40", y: "915", "font-size": "44px" }, "🐢");
        appendSvgElement(gridLayer, "text", { x: "840", y: "915", "font-size": "44px" }, "🍐");
      }
    } else {
      // Standard Grade 1 Notebook 4-Grid Ô Ly (Ảnh 1)
      // Tầng 1: Hero Header Box (h = 250)
      appendSvgElement(gridLayer, "rect", {
        x: "6",
        y: "10",
        width: "908",
        height: "250",
        fill: "#f8fafc",
        rx: "12",
        stroke: "#e2e8f0",
        "stroke-width": "1.5"
      });
      [10, 72, 135, 197, 260].forEach((y) => {
        appendSvgElement(gridLayer, "line", {
          x1: "6",
          y1: String(y),
          x2: "914",
          y2: String(y),
          stroke: "#e2e8f0",
          "stroke-width": "0.8"
        });
      });

      // Tầng 2, 3, 4, 5: 4-grid Ô Ly Boxes
      const rowBoxes = [
        { y: 280, h: 210, lines: [280, 332, 385, 437, 490] },
        { y: 510, h: 210, lines: [510, 562, 615, 667, 720] },
        { y: 740, h: 210, lines: [740, 792, 845, 897, 950] },
        { y: 970, h: 210, lines: [970, 1022, 1075, 1127, 1180] }
      ];

      rowBoxes.forEach(box => {
        appendSvgElement(gridLayer, "rect", {
          x: "6",
          y: String(box.y),
          width: "908",
          height: String(box.h),
          fill: "#f0fdf4",
          rx: "10",
          stroke: "#86efac",
          "stroke-width": "1.5"
        });

        box.lines.forEach((ly, idx) => {
          appendSvgElement(gridLayer, "line", {
            x1: "6",
            y1: String(ly),
            x2: "914",
            y2: String(ly),
            stroke: idx === 4 ? "#16a34a" : "#bbf7d0",
            "stroke-width": idx === 4 ? "2.0" : "0.8"
          });
        });

        for (let x = 24; x < 914; x += 26) {
          appendSvgElement(gridLayer, "line", {
            x1: String(x),
            y1: String(box.y),
            x2: String(x),
            y2: String(box.y + box.h),
            stroke: "#dcfce7",
            "stroke-width": "0.6"
          });
        }
      });
    }

    const guideLayer = appendSvgElement(svg, "g", { class: "tracing-guide-layer" });

    // 2. Ghost Background Strokes with Tiered Widths
    strokeDefs.forEach((def) => {
      appendSvgElement(guideLayer, "path", {
        d: pathData(def.commands),
        fill: "none",
        stroke: "#e2e8f0",
        "stroke-width": String(def.ghostWidth),
        "stroke-linecap": "round",
        "stroke-linejoin": "round"
      });
    });

    // 3. Colored Guided Stroke Overlay with Tiered Widths
    strokeDefs.forEach((def, index) => {
      const palette = strokePalette[index % strokePalette.length];
      const markerId = `tracing-arrow-${index}-${Math.random().toString(36).slice(2)}`;
      
      const markerScale = def.tier === "bold" ? 14 : (def.tier === "medium" ? 10 : 8);
      const marker = appendSvgElement(defs, "marker", {
        id: markerId,
        viewBox: "0 0 10 10",
        refX: "7",
        refY: "5",
        markerWidth: String(markerScale),
        markerHeight: String(markerScale),
        markerUnits: "userSpaceOnUse",
        orient: "auto"
      });
      appendSvgElement(marker, "path", {
        d: "M 1 2 L 7 5 L 1 8 L 3 5 Z",
        fill: palette.dark
      });

      // Translucent colored stroke corridor
      appendSvgElement(guideLayer, "path", {
        d: pathData(def.commands),
        fill: "none",
        stroke: palette.color,
        "stroke-width": String(def.corridorWidth),
        "stroke-linecap": "round",
        "stroke-linejoin": "round",
        opacity: "0.35"
      });

      // Animated dotted centerline with direction arrow
      const centerPath = appendSvgElement(guideLayer, "path", {
        d: pathData(def.commands),
        class: "tracing-guide-centerline",
        "data-corridor-radius": String(def.corridorRadius),
        "data-pen-width": String(def.penWidth),
        "data-step": def.tier === "bold" ? "12" : (def.tier === "medium" ? "8" : "6"),
        fill: "none",
        stroke: "#ffffff",
        "stroke-width": String(def.centerlineWidth),
        "stroke-linecap": "round",
        "stroke-dasharray": def.dashArray,
        "marker-end": `url(#${markerId})`
      });

      // Start Badge with Stroke Number (for bold top row)
      if (def.showBadge) {
        const startPoint = pathPoint(centerPath, 0);
        
        appendSvgElement(guideLayer, "circle", {
          cx: String(startPoint.x),
          cy: String(startPoint.y),
          r: "12",
          fill: palette.color,
          stroke: "#ffffff",
          "stroke-width": "2"
        });

        appendSvgElement(guideLayer, "text", {
          x: String(startPoint.x),
          y: String(startPoint.y + 4.5),
          "text-anchor": "middle",
          "font-family": "'Plus Jakarta Sans', 'Be Vietnam Pro', sans-serif",
          "font-size": "11px",
          "font-weight": "900",
          fill: "#ffffff"
        }, def.badgeLabel || "1");
      }
    });

    target.replaceChildren(svg);
    target._guideCheckpoints = extractCheckpoints(svg);
  }

  function getGuideCheckpoints(target) {
    if (!target) return [];
    if (target._guideCheckpoints && target._guideCheckpoints.length) {
      return target._guideCheckpoints;
    }
    const svg = target.querySelector("svg");
    return extractCheckpoints(svg);
  }

  window.tracingGuides = { renderTracingGuide, guideStrokesFor, getGuideCheckpoints, strokePalette };
})();
