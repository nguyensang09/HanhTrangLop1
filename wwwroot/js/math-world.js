/*
 * Đảo Toán Học — trình chơi bài học toán.
 * Một bài = nhiều câu hỏi; mỗi phong cách (style) có một bộ dựng giao diện riêng nhưng dùng chung
 * cơ chế chấm điểm, gợi ý, âm thanh và màn hình hoàn thành. Thêm phong cách mới: viết hàm render
 * và đăng ký vào RENDERERS; máy chủ chỉ cần sinh payload phù hợp.
 */
(function () {
    'use strict';

    // ---------------------------------------------------------------- Âm thanh
    var audioCtx = null;
    var soundOn = true;

    function tone(freq, start, duration, type, volume, toFreq) {
        var osc = audioCtx.createOscillator();
        var gain = audioCtx.createGain();
        osc.type = type || 'sine';
        osc.frequency.setValueAtTime(freq, start);
        if (toFreq) osc.frequency.exponentialRampToValueAtTime(toFreq, start + duration);
        gain.gain.setValueAtTime(volume || 0.12, start);
        gain.gain.exponentialRampToValueAtTime(0.001, start + duration);
        osc.connect(gain);
        gain.connect(audioCtx.destination);
        osc.start(start);
        osc.stop(start + duration + 0.02);
    }

    function sfx(name) {
        if (!soundOn) return;
        try {
            audioCtx = audioCtx || new (window.AudioContext || window.webkitAudioContext)();
            if (audioCtx.state === 'suspended') audioCtx.resume();
            var t = audioCtx.currentTime;
            switch (name) {
                case 'count': tone(880, t, 0.08, 'triangle', 0.1); break;
                case 'correct': [523, 659, 784].forEach(function (f, i) { tone(f, t + i * 0.09, 0.18, 'triangle', 0.15); }); break;
                case 'wrong': tone(330, t, 0.14, 'sine', 0.1); tone(262, t + 0.13, 0.22, 'sine', 0.09); break;
                case 'hop': tone(380, t, 0.18, 'sine', 0.12, 760); break;
                case 'pop': tone(600, t, 0.1, 'square', 0.06, 1200); break;
                case 'win': [523, 659, 784, 1047, 784, 1047].forEach(function (f, i) { tone(f, t + i * 0.12, 0.22, 'triangle', 0.14); }); break;
                default: tone(660, t, 0.08, 'sine', 0.1);
            }
        } catch (e) { /* trình duyệt chặn âm thanh: bỏ qua */ }
    }

    window.MathWorldSfx = sfx;
    document.addEventListener('click', function (e) {
        var target = e.target.closest ? e.target.closest('[data-sfx]') : null;
        if (target) sfx(target.getAttribute('data-sfx'));
    });

    var configEl = document.getElementById('mathPlayConfig');
    if (!configEl) return; // Trang bản đồ chỉ cần âm thanh chạm.

    // ---------------------------------------------------------------- Cấu hình & trạng thái
    var cfg = JSON.parse(configEl.textContent);
    var lesson = cfg.lesson;
    var questions = cfg.questions || [];
    var root = document.getElementById('mathPlay');
    var stageEl = document.getElementById('mwStage');
    var guideText = document.getElementById('mwGuideText');
    var progressEl = document.getElementById('mwProgress');
    var reduceMotion = window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches;

    soundOn = cfg.child.soundEnabled !== false;
    try {
        var savedSound = localStorage.getItem('mw-sound');
        if (savedSound !== null) soundOn = savedSound === '1';
    } catch (e) { }

    var state = {
        index: 0,
        results: [],
        startedAt: Date.now(),
        current: null,
        combo: 0,
        persist: {}
    };

    // Trùng thứ tự với MathVoiceCatalog (praise0..1, retry0..2) để chữ hiển thị khớp với Voice. Lời khen chỉ một câu ngắn.
    var PRAISE = ['Chính xác!', 'Đúng rồi!'];
    var RETRY = ['Chưa đúng, con thử lại nhé!', 'Con nghĩ thêm chút nữa nhé!', 'Không sao đâu, thử lại nào!'];
    var CHEER = ['Cố lên nào!', 'Con làm được mà!', 'Tiếp tục nhé!', 'Cô tin con làm được!'];
    function pickIndex(list) { return Math.floor(Math.random() * list.length); }

    // ---------------------------------------------------------------- Tiện ích DOM
    function el(tag, cls, html) {
        var node = document.createElement(tag);
        if (cls) node.className = cls;
        if (html != null) node.innerHTML = html;
        return node;
    }

    function txt(tag, cls, value) {
        var node = el(tag, cls);
        node.textContent = value;
        return node;
    }

    function button(cls, label, onClick) {
        var b = el('button', cls);
        b.type = 'button';
        if (label != null) b.textContent = label;
        if (onClick) b.addEventListener('click', onClick);
        return b;
    }

    function wait(ms) { return new Promise(function (r) { setTimeout(r, ms); }); }

    function pick(list) { return list[Math.floor(Math.random() * list.length)]; }

    function shuffle(list) {
        var copy = list.slice();
        for (var i = copy.length - 1; i > 0; i--) {
            var j = Math.floor(Math.random() * (i + 1));
            var tmp = copy[i]; copy[i] = copy[j]; copy[j] = tmp;
        }
        return copy;
    }

    function opSymbol(op) { return { '+': '+', '-': '−', '*': '×', '/': '÷' }[op] || op; }

    function shake(node) {
        if (!node) return;
        node.classList.remove('is-shake');
        void node.offsetWidth;
        node.classList.add('is-shake');
    }

    function solvedText(p) {
        return p.a + ' ' + opSymbol(p.op) + ' ' + p.b + ' = ' + p.result;
    }

    // ---------------------------------------------------------------- Voice (kho Voice chung, theo ngôn ngữ cấu hình của bé)
    // Mỗi câu có khóa (vd "q0.ask") → { text, url } theo đúng ngôn ngữ phụ huynh chọn. Có file thì phát file;
    // chưa có file thì nhờ máy chủ sinh bổ sung, trong lúc chờ đọc bằng giọng trình duyệt CÙNG ngôn ngữ.
    var voiceCfg = cfg.voice || { lang: 'vi', speechLang: 'vi-VN', lines: {} };
    var voiceLines = voiceCfg.lines || {};
    var player = new Audio();
    player.preload = 'auto';
    var voiceToken = 0;
    var browserVoice = null;
    var cutPlayback = null; // Kết thúc ngay câu đang phát khi có Voice mới.
    var cutSpeech = null;

    function pickBrowserVoice() {
        if (!('speechSynthesis' in window)) return;
        var prefix = voiceCfg.lang === 'en' ? 'en' : 'vi';
        var voices = (window.speechSynthesis.getVoices() || []).filter(function (v) { return v.lang && v.lang.toLowerCase().indexOf(prefix) === 0; });
        browserVoice = voices.filter(function (v) { return /female|nữ|samantha|linh|hoaimy|jenny|aria/i.test(v.name); })[0] || voices[0] || null;
    }
    if ('speechSynthesis' in window) {
        pickBrowserVoice();
        window.speechSynthesis.onvoiceschanged = pickBrowserVoice;
    }

    function setSpeaking(on) { root.classList.toggle('is-speaking', on); }

    function playFile(url, token) {
        return new Promise(function (resolve, reject) {
            if (token !== voiceToken) { resolve(); return; }
            var done = false;
            function finish(ok) {
                if (done) return;
                done = true;
                player.onended = player.onerror = null;
                cutPlayback = null;
                ok ? resolve() : reject(new Error('audio'));
            }
            cutPlayback = function () { finish(true); };
            player.onended = function () { finish(true); };
            player.onerror = function () { finish(false); };
            try {
                player.src = url;
                var p = player.play();
                if (p && p.catch) p.catch(function () { finish(false); });
            } catch (e) { finish(false); }
        });
    }

    function speakText(text, token) {
        return new Promise(function (resolve) {
            if (!text || token !== voiceToken || !('speechSynthesis' in window)) { resolve(); return; }
            // Không có giọng đúng ngôn ngữ trên thiết bị thì im lặng còn hơn đọc sai giọng.
            if (!browserVoice) pickBrowserVoice();
            if (!browserVoice) { resolve(); return; }
            try {
                var u = new SpeechSynthesisUtterance(text);
                u.lang = voiceCfg.speechLang;
                u.voice = browserVoice;
                u.rate = voiceCfg.lang === 'en' ? 0.9 : 0.95;
                cutSpeech = resolve;
                u.onend = u.onerror = function () { resolve(); };
                window.speechSynthesis.cancel();
                window.speechSynthesis.speak(u);
                setTimeout(resolve, 12000);
            } catch (e) { resolve(); }
        });
    }

    function stopVoice() {
        voiceToken++;
        try { player.pause(); } catch (e) { }
        if (cutPlayback) cutPlayback();
        if (cutSpeech) { cutSpeech(); cutSpeech = null; }
        if ('speechSynthesis' in window) window.speechSynthesis.cancel();
        setSpeaking(false);
    }

    // Phát lần lượt các câu (khóa trong voiceLines, hoặc { text } khi chỉ có văn bản tiếng Việt của bài cũ).
    // Gọi mới sẽ cắt câu đang đọc để Voice luôn khớp với màn hình hiện tại.
    function voice(items) {
        stopVoice();
        if (!soundOn) return Promise.resolve();
        var token = voiceToken;
        var list = (Array.isArray(items) ? items : [items]).filter(Boolean);
        setSpeaking(true);
        return list.reduce(function (chain, item) {
            return chain.then(function () {
                if (token !== voiceToken) return;
                var line = typeof item === 'string' ? voiceLines[item] : (voiceCfg.lang === 'vi' ? item : null);
                if (!line) return;
                if (line.url) return playFile(line.url, token).catch(function () { return speakText(line.text, token); });
                return speakText(line.text, token);
            });
        }, Promise.resolve()).then(function () { if (token === voiceToken) setSpeaking(false); });
    }

    function voiceKey(q, part) { return q && q.voiceKey ? q.voiceKey + part : null; }

    // Voice đọc con số bé vừa chọn (kho có sẵn "n0".."n10"); đáp án khác thì không đọc.
    function answerVoiceKey(value) { return voiceLines['n' + value] ? 'n' + value : null; }

    // Câu hỏi đọc to; bài không có kịch bản Voice thì đọc lời đề (chỉ khi giọng là tiếng Việt).
    function askItem(q) { return voiceKey(q, 'ask') || { text: q.prompt }; }

    function prefetch(keys) {
        keys.forEach(function (key) {
            var line = voiceLines[key];
            if (!line || !line.url || prefetch.seen[line.url]) return;
            prefetch.seen[line.url] = true;
            var link = document.createElement('link');
            link.rel = 'prefetch';
            link.as = 'audio';
            link.href = line.url;
            document.head.appendChild(link);
        });
    }
    prefetch.seen = {};

    // Bài chưa đủ file Voice (bài mới, hoặc phụ huynh vừa đổi ngôn ngữ): nhờ máy chủ sinh bổ sung rồi dùng lại.
    function fillMissingVoices(round) {
        var missing = Object.keys(voiceLines).filter(function (k) { return !voiceLines[k].url; }).length;
        if (!missing || !cfg.urls.voice || round > 4) return;
        fetch(cfg.urls.voice, {
            method: 'POST',
            credentials: 'same-origin',
            headers: { 'RequestVerificationToken': antiForgeryToken() }
        })
            .then(function (res) { return res.ok ? res.json() : null; })
            .then(function (data) {
                if (!data || !data.lines) return;
                Object.keys(data.lines).forEach(function (k) { voiceLines[k] = data.lines[k]; });
                if (data.missing > 0 && data.generated > 0) fillMissingVoices(round + 1);
            })
            .catch(function () { });
    }

    // ---------------------------------------------------------------- Bíp & phản hồi
    function say(message, mood) {
        guideText.textContent = message;
        root.setAttribute('data-mood', mood || '');
        guideText.classList.remove('is-pop');
        void guideText.offsetWidth;
        guideText.classList.add('is-pop');
    }

    function toast(message, kind) {
        var old = stageEl.querySelector('.mw-toast');
        if (old) old.remove();
        var t = txt('div', 'mw-toast mw-toast--' + kind, message);
        stageEl.appendChild(t);
        setTimeout(function () { t.classList.add('is-out'); }, 1100);
        setTimeout(function () { t.remove(); }, 1500);
    }

    function renderProgress() {
        progressEl.innerHTML = '';
        questions.forEach(function (q, i) {
            var li = el('li', i < state.index ? 'is-done' : i === state.index ? 'is-current' : '');
            li.setAttribute('aria-label', 'Câu ' + (i + 1) + (i < state.index ? ' đã xong' : ''));
            progressEl.appendChild(li);
        });
    }

    // ---------------------------------------------------------------- Chấm điểm & gợi ý (dùng chung)
    function createContext(q) {
        var cur = { q: q, p: q.payload || {}, attempts: 0, hints: 0, done: false, hintHost: null, hintProblem: null };
        state.current = cur;

        var ctx = {
            p: cur.p,
            q: q,
            isDone: function () { return cur.done; },
            hintHost: function (host) { cur.hintHost = host; },
            // So khớp đáp án. Đúng → ghi nhận và chuyển câu; sai → gợi ý tăng dần, bé được thử lại.
            check: function (value, opts) {
                opts = opts || {};
                if (cur.done) return false;
                if (String(value) === String(q.answer)) {
                    succeed(String(value), opts);
                    return true;
                }
                ctx.miss(opts.target, null, opts.lead);
                return false;
            },
            // Ghi nhận một lần sai (ví dụ ghép nhầm cặp) kèm gợi ý. lead: khóa Voice của đáp án bé vừa chọn, đọc trước lời nhắc.
            miss: function (target, problem, lead) {
                cur.attempts++;
                sfx('wrong');
                shake(target);
                var k = pickIndex(RETRY);
                toast(RETRY[k], 'retry');
                showHint(cur.attempts >= 2 ? 2 : 1, problem, [lead, 'retry' + k]);
            },
            // Hoàn thành câu không cần so khớp thêm (dùng khi bé đã ghép đủ mọi cặp).
            finish: function (value, opts) { succeed(String(value), opts || {}); }
        };

        function succeed(value, opts) {
            if (cur.done) return;
            cur.done = true;
            cur.attempts++;
            state.results.push({ questionId: q.id, answer: value, attempts: cur.attempts, hintsUsed: cur.hints });
            sfx('correct');
            var k = pickIndex(PRAISE);
            var praise = PRAISE[k];
            toast('🎉 ' + praise, 'ok');
            var isBoard = cur.p.kind === 'board';
            say(isBoard ? praise + ' Con đã ghép đủ các cặp!' : praise + ' ' + solvedText(cur.p), 'happy');
            // Thứ tự Voice: đáp án bé chọn → một câu khen ngắn → sang câu mới (chữ trên màn hình vẫn hiện phép tính đã giải).
            var spoken = voice([opts.lead, 'praise' + k]);
            var host = stageEl.querySelector('.mw-card');
            if (host) host.classList.add('is-solved');
            if (opts.onSolved) opts.onSolved();
            // Chờ Bíp đọc xong (tối đa 5 giây) rồi mới sang câu mới.
            var minDelay = wait(opts.delay || (reduceMotion ? 900 : 1300));
            Promise.race([Promise.all([spoken, minDelay]), wait(5000)]).then(nextQuestion);
        }

        return ctx;
    }

    // Gợi ý 2 mức: chữ trong bong bóng của Bíp, Voice đọc gợi ý và hình chấm tròn minh họa.
    function showHint(level, problem, lead) {
        var cur = state.current;
        if (!cur || cur.done) return;
        cur.hints = Math.max(cur.hints, level);
        var p = problem || (cur.p.kind === 'board' ? null : cur.p);
        var hintText = level >= 2 ? (cur.p.hint2 || cur.p.hint1) : cur.p.hint1;
        var spoken = voiceKey(cur.q, level >= 2 ? 'hint2' : 'hint1') || { text: hintText };
        say(hintText, 'think');
        if (problem && level < 2) {
            say('Con tính lại phép ' + problem.a + ' ' + opSymbol(problem.op) + ' ' + problem.b + ' nhé.', 'think');
            spoken = 'x:' + problem.a + problem.op + problem.b;
        }
        voice((lead || []).concat([spoken]));
        if (!p || !cur.hintHost) return;
        var panel = cur.hintHost.querySelector('.mw-hint-panel');
        if (!panel) {
            panel = el('div', 'mw-hint-panel');
            cur.hintHost.appendChild(panel);
        }
        panel.innerHTML = '';
        panel.appendChild(txt('p', 'mw-hint-caption', level >= 2 ? 'Cô đếm cùng con nhé:' : 'Nhìn các chấm tròn này nhé:'));
        panel.appendChild(renderDots(p, level));
    }

    // Minh họa phép tính bằng chấm tròn. Mức 2 đánh số để bé đếm theo.
    function renderDots(p, level) {
        var box = el('div', 'mw-dots');
        var numbered = level >= 2;
        function dot(cls, label) {
            var d = el('span', 'mw-dot ' + cls);
            if (label != null) d.textContent = label;
            box.appendChild(d);
        }
        function sep(symbol) { box.appendChild(txt('span', 'mw-dots-sep', symbol)); }
        var i;
        if (p.op === '+') {
            if (p.missing === 'b') {
                for (i = 0; i < p.a; i++) dot('is-a');
                sep('+');
                for (i = 0; i < p.result - p.a; i++) dot('is-unknown', numbered ? i + 1 : '?');
            } else if (p.missing === 'a') {
                for (i = 0; i < p.result - p.b; i++) dot('is-unknown', numbered ? i + 1 : '?');
                sep('+');
                for (i = 0; i < p.b; i++) dot('is-b');
            } else {
                for (i = 0; i < p.a; i++) dot('is-a', numbered ? i + 1 : null);
                sep('+');
                for (i = 0; i < p.b; i++) dot('is-b', numbered ? p.a + i + 1 : null);
            }
            if (p.missing !== 'result') { sep('='); box.appendChild(txt('span', 'mw-dots-total', String(p.result))); }
        } else {
            if (p.missing === 'b') {
                for (i = 0; i < p.a; i++) {
                    if (i < p.result) dot('is-a');
                    else dot('is-unknown is-crossed', numbered ? i - p.result + 1 : '?');
                }
            } else if (p.missing === 'a') {
                for (i = 0; i < p.result + p.b; i++) {
                    dot(i < p.result ? 'is-a' : 'is-unknown is-crossed', numbered ? i + 1 : (i < p.result ? null : '?'));
                }
                sep('→'); box.appendChild(txt('span', 'mw-dots-total', 'còn ' + p.result));
            } else {
                var left = 0;
                for (i = 0; i < p.a; i++) {
                    if (i >= p.a - p.b) dot('is-crossed');
                    else { left++; dot('is-a', numbered ? left : null); }
                }
            }
        }
        if (box.children.length === 0) box.appendChild(txt('span', 'mw-dots-total', '0'));
        return box;
    }

    // ---------------------------------------------------------------- Thành phần dùng chung
    function buildEquation(p) {
        var eq = el('div', 'mw-eq');
        eq.setAttribute('data-op', p.op);
        var slot = null;
        [['a', p.a], ['op', opSymbol(p.op)], ['b', p.b], ['eq', '='], ['result', p.result]].forEach(function (part) {
            if (part[0] === 'op' || part[0] === 'eq') {
                eq.appendChild(txt('span', 'mw-eq-op', part[1]));
            } else if (part[0] === (p.missing || 'result')) {
                slot = txt('span', 'mw-eq-slot', '?');
                eq.appendChild(slot);
            } else {
                eq.appendChild(txt('span', 'mw-eq-num', String(part[1])));
            }
        });
        eq.setAttribute('aria-label', (p.missing === 'a' ? 'mấy' : p.a) + ' ' + (p.op === '+' ? 'cộng' : 'trừ') + ' ' +
            (p.missing === 'b' ? 'mấy' : p.b) + ' bằng ' + (p.missing === 'result' || !p.missing ? 'mấy' : p.result));
        return { node: eq, slot: slot };
    }

    function fillSlot(slot, value) {
        if (!slot) return;
        slot.textContent = value;
        slot.classList.add('is-filled');
    }

    function objects(count, emoji, cls) {
        var frag = document.createDocumentFragment();
        for (var i = 0; i < count; i++) frag.appendChild(txt('span', 'mw-obj ' + (cls || ''), emoji));
        return frag;
    }

    function pictureRow(p) {
        var row = el('div', 'mw-picture');
        if (p.op === '+') {
            var ga = el('div', 'mw-tray mw-tray--a'); ga.appendChild(objects(p.a, p.emoji));
            var gb = el('div', 'mw-tray mw-tray--b'); gb.appendChild(objects(p.b, p.emoji));
            row.appendChild(ga); row.appendChild(txt('span', 'mw-picture-op', '+')); row.appendChild(gb);
        } else {
            var g = el('div', 'mw-tray');
            for (var i = 0; i < p.a; i++) g.appendChild(txt('span', 'mw-obj' + (i >= p.a - p.b ? ' is-gone' : ''), p.emoji));
            row.appendChild(g);
        }
        return row;
    }

    function optionButtons(ctx, values, variant, slot, onPicked) {
        var wrap = el('div', 'mw-options mw-options--' + variant);
        values.forEach(function (value, i) {
            var b = button('mw-option', String(value));
            b.style.setProperty('--i', i);
            b.setAttribute('aria-label', 'Đáp án ' + value);
            b.addEventListener('click', function () {
                if (ctx.isDone() || b.disabled) return;
                if (ctx.check(value, { target: b, delay: onPicked && onPicked.delay, lead: answerVoiceKey(value) })) {
                    b.classList.add('is-correct');
                    fillSlot(slot, value);
                    if (onPicked) onPicked(value);
                } else {
                    b.classList.add('is-wrong');
                    b.disabled = true;
                }
            });
            wrap.appendChild(b);
        });
        return wrap;
    }

    // Kéo thả bằng Pointer Events (chuột, cảm ứng, bút). Chạm nhẹ không kéo cũng được tính là chọn.
    function enableDrag(tile, target, onDrop) {
        var pointerId = null, startX = 0, startY = 0, moved = false, ghost = null;
        function hit(e) {
            var r = target.getBoundingClientRect();
            var pad = 24;
            return e.clientX >= r.left - pad && e.clientX <= r.right + pad && e.clientY >= r.top - pad && e.clientY <= r.bottom + pad;
        }
        function cleanup() {
            if (ghost) ghost.remove();
            ghost = null;
            tile.classList.remove('is-lifted');
            target.classList.remove('is-hover');
            document.body.classList.remove('drag-interaction-active');
        }
        tile.addEventListener('pointerdown', function (e) {
            if (tile.disabled) return;
            pointerId = e.pointerId; startX = e.clientX; startY = e.clientY; moved = false;
            try { tile.setPointerCapture(pointerId); } catch (err) { }
        });
        tile.addEventListener('pointermove', function (e) {
            if (pointerId !== e.pointerId) return;
            if (!moved && Math.abs(e.clientX - startX) + Math.abs(e.clientY - startY) > 8) {
                moved = true;
                ghost = tile.cloneNode(true);
                ghost.className = 'mw-tile mw-tile-ghost';
                document.body.appendChild(ghost);
                tile.classList.add('is-lifted');
                document.body.classList.add('drag-interaction-active');
            }
            if (moved) {
                ghost.style.left = e.clientX + 'px';
                ghost.style.top = e.clientY + 'px';
                target.classList.toggle('is-hover', hit(e));
            }
        });
        function end(e) {
            if (pointerId !== e.pointerId) return;
            pointerId = null;
            var dropped = moved ? hit(e) : e.type === 'pointerup';
            cleanup();
            if (dropped) onDrop();
        }
        tile.addEventListener('pointerup', end);
        tile.addEventListener('pointercancel', end);
        tile.addEventListener('click', function (e) { if (e.detail === 0) onDrop(); }); // bàn phím
    }

    // ---------------------------------------------------------------- Các phong cách chơi
    var RENDERERS = {
        choose: function (host, ctx) {
            var p = ctx.p;
            if (lesson.stageOrder <= 2 && p.missing === 'result' && Math.max(p.a, p.b) <= 5) host.appendChild(pictureRow(p));
            var eq = buildEquation(p);
            host.appendChild(eq.node);
            host.appendChild(optionButtons(ctx, p.options, 'balloon', eq.slot));
            ctx.hintHost(host);
        },

        count: function (host, ctx) {
            var p = ctx.p;
            var scene = el('div', 'mw-count-scene');
            var counted = 0;
            var counter = txt('div', 'mw-count-counter', '👆 Chạm vào từng ' + p.unit + ' để đếm');
            function countable(node) {
                node.setAttribute('role', 'button');
                node.setAttribute('tabindex', '0');
                function hitItem() {
                    if (node.dataset.n || node.classList.contains('is-leaving') || ctx.isDone()) return;
                    counted++;
                    node.dataset.n = counted;
                    node.classList.add('is-counted');
                    counter.textContent = 'Con đếm được: ' + counted;
                    sfx('count');
                    voice('n' + Math.min(counted, 10));
                }
                node.addEventListener('click', hitItem);
                node.addEventListener('keydown', function (e) { if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); hitItem(); } });
                return node;
            }
            function tray(count, cls, emptyLabel) {
                var t = el('div', 'mw-tray ' + cls);
                for (var i = 0; i < count; i++) t.appendChild(countable(txt('span', 'mw-obj', p.emoji)));
                if (count === 0) t.appendChild(txt('span', 'mw-tray-empty', emptyLabel));
                return t;
            }
            if (p.op === '+') {
                scene.appendChild(tray(p.a, 'mw-tray--a', 'trống'));
                scene.appendChild(txt('span', 'mw-picture-op', '+'));
                scene.appendChild(tray(p.b, 'mw-tray--b', 'trống'));
            } else {
                var t = tray(p.a, '', 'trống');
                var items = t.querySelectorAll('.mw-obj');
                for (var i = p.a - p.b; i < p.a; i++) items[i].classList.add('is-leaving');
                setTimeout(function () {
                    t.querySelectorAll('.is-leaving').forEach(function (n) { n.classList.add('is-gone'); });
                }, reduceMotion ? 0 : 900);
                scene.appendChild(t);
            }
            host.appendChild(scene);
            host.appendChild(counter);
            var eq = buildEquation(p);
            host.appendChild(eq.node);
            host.appendChild(optionButtons(ctx, p.options, 'tile', eq.slot));
            ctx.hintHost(host);
        },

        build: function (host, ctx) {
            var p = ctx.p;
            var isAdd = p.op === '+';
            var target = p.b;
            var moved = 0;
            var basket = el('div', 'mw-basket');
            basket.appendChild(txt('span', 'mw-basket-label', 'Giỏ đồ vật'));
            var items = el('div', 'mw-basket-items');
            basket.appendChild(items);
            var outside = el('div', 'mw-outside');
            var counter = el('div', 'mw-build-counter');
            var controls = el('div', 'mw-build-controls');
            var answer = el('div', 'mw-build-answer');
            answer.hidden = true;
            var answerBuilt = false;

            function render() {
                counter.innerHTML = (isAdd ? 'Đã thêm: ' : 'Đã lấy ra: ') + '<strong>' + moved + '</strong> / ' + target + ' ' + p.unit;
                counter.classList.toggle('is-ok', moved === target);
                counter.classList.toggle('is-over', moved > target);
                if (moved === target) {
                    if (!answerBuilt) {
                        answerBuilt = true;
                        answer.appendChild(txt('p', 'mw-build-question', isAdd
                            ? 'Trong giỏ có tất cả mấy ' + p.noun + '?'
                            : 'Trong giỏ còn lại mấy ' + p.noun + '?'));
                        var eq = buildEquation(p);
                        answer.appendChild(eq.node);
                        answer.appendChild(optionButtons(ctx, p.options, 'tile', eq.slot));
                    }
                    if (answer.hidden) {
                        answer.hidden = false;
                        say(isAdd ? 'Giỏi lắm! Bây giờ đếm xem trong giỏ có tất cả mấy ' + p.noun + ' nhé.'
                                  : 'Giỏi lắm! Đếm xem trong giỏ còn lại mấy ' + p.noun + ' nhé.', 'happy');
                        voice(isAdd ? 'buildDoneAdd' : 'buildDoneSub');
                    }
                } else {
                    answer.hidden = true;
                    if (moved > target) {
                        say('Ôi, nhiều hơn ' + target + ' ' + p.unit + ' rồi! Con ' + (isAdd ? 'bớt' : 'trả') + ' lại nhé.', 'think');
                        voice('buildOver');
                    }
                }
            }

            if (isAdd) {
                for (var i = 0; i < p.a; i++) items.appendChild(txt('span', 'mw-obj is-start', p.emoji));
                var addBtn = button('mw-big-btn mw-big-btn--add', null, function () {
                    if (ctx.isDone()) return;
                    if (items.children.length >= 10) { say('Giỏ chỉ chứa được 10 ' + p.unit + ' thôi!', 'think'); voice('basketFull'); shake(basket); return; }
                    items.appendChild(txt('span', 'mw-obj is-new', p.emoji));
                    moved++; sfx('pop'); render();
                });
                addBtn.innerHTML = '<span class="material-symbols-outlined">add</span> Thêm ' + p.emoji;
                var removeBtn = button('mw-big-btn mw-big-btn--remove', null, function () {
                    if (ctx.isDone() || moved === 0) return;
                    var last = items.querySelector('.is-new:last-child');
                    if (last) { last.remove(); moved--; sfx('tap'); render(); }
                });
                removeBtn.innerHTML = '<span class="material-symbols-outlined">remove</span> Bớt';
                controls.appendChild(addBtn);
                controls.appendChild(removeBtn);
            } else {
                outside.appendChild(txt('span', 'mw-basket-label', 'Đã lấy ra'));
                var outsideItems = el('div', 'mw-basket-items');
                outside.appendChild(outsideItems);
                for (var j = 0; j < p.a; j++) {
                    (function () {
                        var obj = button('mw-obj mw-obj--tap', p.emoji);
                        obj.setAttribute('aria-label', 'Lấy ' + p.noun);
                        obj.addEventListener('click', function () {
                            if (ctx.isDone()) return;
                            if (obj.parentNode === items) { outsideItems.appendChild(obj); moved++; }
                            else { items.appendChild(obj); moved--; }
                            sfx('pop'); render();
                        });
                        items.appendChild(obj);
                    })();
                }
            }

            var scene = el('div', 'mw-build-scene');
            scene.appendChild(basket);
            if (!isAdd) scene.appendChild(outside);
            host.appendChild(scene);
            if (target === 0) {
                host.appendChild(txt('p', 'mw-build-note', isAdd ? 'Không cần thêm ' + p.unit + ' nào cả!' : 'Không cần lấy ' + p.unit + ' nào ra cả!'));
            } else {
                host.appendChild(counter);
                host.appendChild(controls);
            }
            host.appendChild(answer);
            ctx.hintHost(host);
            render();
        },

        drag: function (host, ctx) {
            var p = ctx.p;
            var eq = buildEquation(p);
            eq.slot.classList.add('is-drop');
            host.appendChild(eq.node);
            host.appendChild(txt('p', 'mw-card-tip', '👇 Kéo thẻ số vào ô có dấu ?'));
            var tray = el('div', 'mw-tiles');
            p.options.forEach(function (value) {
                var tile = button('mw-tile', String(value));
                tile.setAttribute('aria-label', 'Thẻ số ' + value);
                enableDrag(tile, eq.slot, function () {
                    if (ctx.isDone() || tile.disabled) return;
                    if (ctx.check(value, { target: tile, lead: answerVoiceKey(value) })) {
                        tile.classList.add('is-used');
                        fillSlot(eq.slot, value);
                    } else {
                        tile.classList.add('is-wrong');
                        tile.disabled = true;
                        shake(eq.slot);
                    }
                });
                tray.appendChild(tile);
            });
            host.appendChild(tray);
            ctx.hintHost(host);
        },

        missing: function (host, ctx) {
            var p = ctx.p;
            var eq = buildEquation(p);
            host.appendChild(eq.node);
            host.appendChild(txt('p', 'mw-card-tip', '🔍 Số nào đang trốn trong ô ?'));
            var pad = el('div', 'mw-numpad');
            for (var n = 0; n <= 10; n++) {
                (function (value) {
                    var key = button('mw-numkey', String(value));
                    key.style.setProperty('--n', value);
                    key.addEventListener('click', function () {
                        if (ctx.isDone() || key.disabled) return;
                        if (ctx.check(value, { target: key, lead: answerVoiceKey(value) })) {
                            key.classList.add('is-correct');
                            fillSlot(eq.slot, value);
                        } else {
                            key.classList.add('is-wrong');
                            key.disabled = true;
                        }
                    });
                    pad.appendChild(key);
                })(n);
            }
            host.appendChild(pad);
            ctx.hintHost(host);
        },

        hop: function (host, ctx) {
            var p = ctx.p;
            var isAdd = p.op === '+';
            var pos = p.a;
            var hops = 0;
            var pond = el('div', 'mw-pond');
            var pads = [];
            for (var n = 0; n <= 10; n++) {
                (function (value) {
                    var pad = button('mw-lily', null, function () {
                        if (ctx.isDone()) return;
                        if (ctx.check(value, { target: pad, lead: answerVoiceKey(value) })) pad.classList.add('is-correct');
                    });
                    pad.innerHTML = '<span class="mw-lily-leaf" aria-hidden="true"></span><span class="mw-lily-num">' + value + '</span>';
                    pad.setAttribute('aria-label', 'Số ' + value);
                    if (value === p.a) pad.classList.add('is-start');
                    pads.push(pad);
                    pond.appendChild(pad);
                })(n);
            }
            var frog = txt('span', 'mw-frog', '🐸');
            frog.setAttribute('aria-hidden', 'true');
            pond.appendChild(frog);
            function place() { frog.style.setProperty('--pos', pos); }
            place();

            var counter = el('div', 'mw-build-counter');
            function render() {
                counter.innerHTML = 'Đã nhảy: <strong>' + hops + '</strong> bước' + (p.b > 0 ? ' (cần ' + p.b + ' bước)' : '');
                counter.classList.toggle('is-ok', hops === p.b);
                counter.classList.toggle('is-over', hops > p.b);
            }
            var hopBtn = button('mw-big-btn mw-big-btn--add', null, function () {
                if (ctx.isDone()) return;
                var next = pos + (isAdd ? 1 : -1);
                if (next < 0 || next > 10) { say('Hết đường rồi! Con bấm "Về chỗ cũ" để nhảy lại nhé.', 'think'); voice('hopEnd'); shake(frog); return; }
                pos = next; hops++;
                pads[pos].classList.add('is-trail');
                frog.classList.remove('is-jumping'); void frog.offsetWidth; frog.classList.add('is-jumping');
                place(); sfx('hop'); render();
                if (hops === p.b) {
                    say('Ếch dừng lại rồi! Ếch đang ở số mấy? Chạm vào số đó nhé.', 'happy');
                    voice(['n' + pos, 'hopStop']);
                } else if (hops > p.b) {
                    say('Ếch nhảy nhiều hơn ' + p.b + ' bước rồi. Bấm "Về chỗ cũ" nhé!', 'think');
                    voice('hopOver');
                } else {
                    voice('n' + pos);
                }
            });
            hopBtn.innerHTML = isAdd ? 'Nhảy tới <span aria-hidden="true">👉</span>' : '<span aria-hidden="true">👈</span> Nhảy lùi';
            var resetBtn = button('mw-big-btn mw-big-btn--remove', null, function () {
                if (ctx.isDone()) return;
                pos = p.a; hops = 0;
                pads.forEach(function (x) { x.classList.remove('is-trail'); });
                place(); sfx('tap'); render();
            });
            resetBtn.innerHTML = '<span class="material-symbols-outlined">undo</span> Về chỗ cũ';

            host.appendChild(pond);
            if (p.b === 0) {
                host.appendChild(txt('p', 'mw-build-note', 'Không cần nhảy bước nào! Ếch đang ở số mấy?'));
            } else {
                host.appendChild(counter);
                var controls = el('div', 'mw-build-controls');
                controls.appendChild(hopBtn);
                controls.appendChild(resetBtn);
                host.appendChild(controls);
            }
            render();
            ctx.hintHost(host);
        },

        match: function (host, ctx) {
            var pairs = ctx.p.pairs || [];
            var board = el('div', 'mw-match');
            var left = el('div', 'mw-match-col');
            var right = el('div', 'mw-match-col');
            var selected = null;
            var matched = 0;
            pairs.forEach(function (pair, i) {
                var card = button('mw-match-card mw-match-card--eq', pair.a + ' ' + opSymbol(pair.op) + ' ' + pair.b);
                card.dataset.index = i;
                card.addEventListener('click', function () {
                    if (ctx.isDone() || card.classList.contains('is-matched')) return;
                    if (selected) selected.classList.remove('is-selected');
                    selected = card;
                    card.classList.add('is-selected');
                    sfx('tap');
                    voice('x:' + pair.a + pair.op + pair.b);
                });
                left.appendChild(card);
            });
            shuffle(pairs.map(function (x) { return x.result; })).forEach(function (result) {
                var card = button('mw-match-card mw-match-card--res', String(result));
                card.addEventListener('click', function () {
                    if (ctx.isDone() || card.classList.contains('is-matched')) return;
                    if (!selected) { say('Con chạm vào một phép tính ở cột bên trái trước nhé!', 'think'); voice('matchFirst'); shake(card); return; }
                    var pair = pairs[Number(selected.dataset.index)];
                    if (pair.result === result) {
                        var color = matched % 4;
                        selected.classList.add('is-matched', 'mw-pair-' + color);
                        card.classList.add('is-matched', 'mw-pair-' + color);
                        selected.classList.remove('is-selected');
                        selected.textContent = selected.textContent + ' = ' + result;
                        selected = null;
                        matched++;
                        sfx('pop');
                        if (matched === pairs.length) {
                            ctx.finish(pairs.map(function (x) { return x.result; }).join(','));
                        } else {
                            say('Đúng rồi! Còn ' + (pairs.length - matched) + ' cặp nữa.', 'happy');
                            voice('r:' + pair.a + pair.op + pair.b);
                        }
                    } else {
                        ctx.miss(card, { op: pair.op, a: pair.a, b: pair.b, result: pair.result, missing: 'result' });
                    }
                });
                right.appendChild(card);
            });
            board.appendChild(left);
            board.appendChild(txt('div', 'mw-match-link', '🔗'));
            board.appendChild(right);
            host.appendChild(board);
            ctx.hintHost(host);
        },

        adventure: function (host, ctx) {
            var OBSTACLES = [
                ['🪨', 'Một tảng đá to chắn đường!'], ['🌊', 'Một con suối chảy xiết!'], ['🪵', 'Thân cây đổ ngang lối đi!'],
                ['🕸️', 'Mạng nhện giăng kín lối!'], ['🌵', 'Bụi xương rồng đầy gai!'], ['🧱', 'Bức tường gạch cao quá!'],
                ['🍄', 'Cây nấm khổng lồ mọc chắn lối!'], ['🌋', 'Núi lửa tí hon đang phun khói!']
            ];
            var adv = state.persist.adventure;
            if (!adv) {
                // Khung cuộn ngang (điện thoại) chứa một "đường đi" có chiều rộng riêng để vị trí % luôn đúng.
                adv = state.persist.adventure = { scene: el('div', 'mw-trail'), track: el('div', 'mw-trail-track'), spots: [] };
                adv.track.style.setProperty('--spots', questions.length + 2);
                adv.scene.appendChild(adv.track);
                var start = txt('span', 'mw-spot is-start', '🏁');
                adv.track.appendChild(start);
                adv.spots.push(start);
                questions.forEach(function (q, i) {
                    var spot = txt('span', 'mw-spot', OBSTACLES[i % OBSTACLES.length][0]);
                    adv.track.appendChild(spot);
                    adv.spots.push(spot);
                });
                var chest = txt('span', 'mw-spot is-chest', '🎁');
                adv.track.appendChild(chest);
                adv.spots.push(chest);
                adv.walker = el('span', 'mw-walker');
                var bip = document.querySelector('#mwGuide .mw-bip');
                if (bip) adv.walker.appendChild(bip.cloneNode(true));
                adv.track.appendChild(adv.walker);
            }
            var index = state.index;
            adv.walker.style.setProperty('--pos', index);
            adv.spots.forEach(function (s, i) { s.classList.toggle('is-next', i === index + 1); });
            host.appendChild(adv.scene);

            var obstacle = OBSTACLES[index % OBSTACLES.length];
            host.appendChild(txt('p', 'mw-adventure-msg', obstacle[0] + ' ' + obstacle[1] + ' Giải phép tính để vượt qua nhé!'));
            var eq = buildEquation(ctx.p);
            host.appendChild(eq.node);
            var onPicked = function () {
                var spot = adv.spots[index + 1];
                spot.textContent = '✨';
                spot.classList.add('is-cleared');
                adv.walker.style.setProperty('--pos', index + 1);
                sfx('hop');
                if (index + 1 === questions.length) {
                    setTimeout(function () { adv.spots[adv.spots.length - 1].textContent = '💎'; adv.walker.style.setProperty('--pos', index + 2); }, 600);
                }
            };
            onPicked.delay = reduceMotion ? 1000 : 1700;
            host.appendChild(optionButtons(ctx, ctx.p.options, 'stone', eq.slot, onPicked));
            ctx.hintHost(host);
            requestAnimationFrame(function () {
                var next = adv.spots[index + 1];
                if (next && next.scrollIntoView && adv.scene.scrollWidth > adv.scene.clientWidth) {
                    adv.scene.scrollTo({ left: Math.max(0, next.offsetLeft - adv.scene.clientWidth / 2), behavior: reduceMotion ? 'auto' : 'smooth' });
                }
            });
        },

        quick: function (host, ctx) {
            var meter = state.persist.rocket;
            if (!meter) {
                meter = state.persist.rocket = { node: el('div', 'mw-rocket'), rocket: txt('span', 'mw-rocket-ship', '🚀'), combo: txt('span', 'mw-combo', '') };
                meter.node.appendChild(txt('span', 'mw-rocket-moon', '🌙'));
                meter.node.appendChild(meter.rocket);
                meter.node.appendChild(txt('span', 'mw-rocket-ground', '🌍'));
            }
            var index = state.index;
            meter.rocket.style.setProperty('--h', index / questions.length);
            var level = index < questions.length * 0.3 ? 1 : index < questions.length * 0.7 ? 2 : 3;
            var wrap = el('div', 'mw-quick');
            wrap.appendChild(meter.node);
            var main = el('div', 'mw-quick-main');
            var head = el('div', 'mw-quick-head');
            head.appendChild(txt('span', 'mw-level mw-level--' + level, 'Cấp ' + level));
            meter.combo.textContent = state.combo >= 2 ? '🔥 ' + state.combo + ' câu đúng liên tiếp!' : '';
            head.appendChild(meter.combo);
            main.appendChild(head);
            var eq = buildEquation(ctx.p);
            main.appendChild(eq.node);
            var firstTry = true;
            var picker = optionButtons(ctx, ctx.p.options, 'rocket', eq.slot, function () {
                state.combo = firstTry ? state.combo + 1 : 0;
                meter.rocket.style.setProperty('--h', (index + 1) / questions.length);
                meter.combo.textContent = state.combo >= 2 ? '🔥 ' + state.combo + ' câu đúng liên tiếp!' : '';
            });
            picker.addEventListener('click', function (e) {
                if (e.target.classList.contains('is-wrong')) { firstTry = false; state.combo = 0; meter.combo.textContent = ''; }
            });
            main.appendChild(picker);
            wrap.appendChild(main);
            host.appendChild(wrap);
            ctx.hintHost(main);
        }
    };

    // ---------------------------------------------------------------- Vòng đời bài học
    function startQuestion() {
        var q = questions[state.index];
        renderProgress();
        stageEl.innerHTML = '';
        var host = el('div', 'mw-card mw-card--' + lesson.style);
        // Lời đề có nút loa: bé chạm để nghe lại câu hỏi bất cứ lúc nào.
        var prompt = button('mw-card-prompt', null, function () { sfx('tap'); voice(askItem(q)); });
        prompt.appendChild(txt('span', 'material-symbols-outlined mw-card-speaker', 'volume_up'));
        prompt.appendChild(txt('span', '', q.prompt));
        prompt.setAttribute('aria-label', 'Nghe lại câu hỏi: ' + q.prompt);
        host.appendChild(prompt);
        stageEl.appendChild(host);
        var ctx = createContext(q);
        (RENDERERS[lesson.style] || RENDERERS.choose)(host, ctx);
        // Câu hỏi đã hiện trên thẻ; bong bóng của Bíp dùng để hướng dẫn cách chơi và cổ vũ.
        say(state.index === 0
            ? lesson.instruction
            : 'Câu ' + (state.index + 1) + '/' + questions.length + '. ' + pick(CHEER), '');
        prefetch(['ask', 'result', 'hint1'].map(function (part) { return voiceKey(q, part); }).filter(Boolean));
        // Câu đầu được đọc khi bé chạm nút Bắt đầu (trình duyệt chỉ cho phát âm thanh sau một lần chạm).
        if (state.started) voice(state.index === 0 ? ['intro', askItem(q)] : askItem(q));
    }

    function nextQuestion() {
        state.index++;
        if (state.index >= questions.length) {
            finish();
        } else {
            startQuestion();
        }
    }

    function antiForgeryToken() {
        var input = root.querySelector('input[name="__RequestVerificationToken"]');
        return input ? input.value : '';
    }

    function finish() {
        renderProgress();
        var payload = {
            answers: state.results,
            durationSeconds: Math.round((Date.now() - state.startedAt) / 1000)
        };
        say('Cô đang cất sao vào kho báu cho con...', 'happy');
        fetch(cfg.urls.complete, {
            method: 'POST',
            credentials: 'same-origin',
            headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': antiForgeryToken() },
            body: JSON.stringify(payload)
        })
            .then(function (res) {
                return res.json().catch(function () { return {}; }).then(function (data) { return { ok: res.ok, data: data }; });
            })
            .then(function (r) {
                if (r.ok && r.data.success) showFinish(r.data);
                else showFinishError(r.data && r.data.message);
            })
            .catch(function () { showFinishError(); });
    }

    function actionLink(href, cls, html) {
        var a = el('a', 'mw-finish-btn ' + cls, html);
        a.href = href;
        a.setAttribute('data-sfx', 'tap');
        return a;
    }

    function showFinish(data) {
        var overlay = document.getElementById('mwFinish');
        var starsEl = document.getElementById('mwFinishStars');
        var stats = document.getElementById('mwFinishStats');
        var rewards = document.getElementById('mwFinishRewards');
        var actions = document.getElementById('mwFinishActions');
        starsEl.innerHTML = '';
        for (var i = 1; i <= 3; i++) {
            var s = txt('span', 'mw-finish-star' + (i <= data.stars ? ' is-on' : ''), '★');
            s.style.setProperty('--i', i);
            starsEl.appendChild(s);
        }
        starsEl.setAttribute('aria-label', data.stars + ' trên 3 sao');
        stats.textContent = 'Con trả lời đúng ngay lần đầu ' + data.firstTryCorrect + '/' + data.totalQuestions + ' câu.' +
            (data.stars < 3 ? ' Chơi lại để lấy đủ 3 sao nhé!' : ' Xuất sắc!');
        rewards.innerHTML = '';
        if (data.isFirstCompletion) rewards.appendChild(el('li', '', '🗝️ Mở khóa bài mới trên đảo'));
        if (data.milestoneMessage) rewards.appendChild(txt('li', '', '🎁 ' + data.milestoneMessage));
        (data.newBadges || []).forEach(function (b) {
            var li = el('li', 'is-badge');
            li.appendChild(txt('span', 'material-symbols-outlined', b.icon));
            li.appendChild(document.createTextNode(' Huy hiệu mới: ' + b.name));
            rewards.appendChild(li);
        });
        actions.innerHTML = '';
        if (data.nextLesson) {
            var next = actionLink(data.nextLesson.url, 'is-primary', '');
            next.appendChild(txt('span', '', data.nextLesson.styleEmoji + ' Bài tiếp: ' + data.nextLesson.title));
            actions.appendChild(next);
        }
        var again = button('mw-finish-btn', null, function () { window.location.reload(); });
        again.innerHTML = '<span class="material-symbols-outlined">replay</span> Chơi lại';
        actions.appendChild(again);
        actions.appendChild(actionLink(cfg.urls.hub + '?op=' + lesson.operation, '', '<span class="material-symbols-outlined">map</span> Về đảo'));

        overlay.hidden = false;
        root.classList.add('is-finished');
        sfx('win');
        confetti();
        say('Hoàn thành! Con được ' + data.stars + ' sao.', 'happy');
        voice('finish' + Math.min(3, Math.max(1, data.stars)));
        var primary = actions.querySelector('a, button');
        if (primary) primary.focus();
    }

    function showFinishError(message) {
        voice('saveError');
        var overlay = document.getElementById('mwFinish');
        document.getElementById('mwFinishTitle').textContent = 'Ối, cô chưa cất được sao cho con!';
        document.getElementById('mwFinishStars').innerHTML = '';
        document.getElementById('mwFinishStats').textContent = message || 'Mạng đang chập chờn. Con nhờ ba mẹ kiểm tra rồi bấm Thử lại nhé.';
        document.getElementById('mwFinishRewards').innerHTML = '';
        var actions = document.getElementById('mwFinishActions');
        actions.innerHTML = '';
        var retry = button('mw-finish-btn is-primary', null, function () { overlay.hidden = true; finish(); });
        retry.innerHTML = '<span class="material-symbols-outlined">refresh</span> Thử lại';
        actions.appendChild(retry);
        actions.appendChild(actionLink(cfg.urls.hub, '', '<span class="material-symbols-outlined">map</span> Về đảo'));
        overlay.hidden = false;
    }

    function confetti() {
        if (reduceMotion) return;
        var box = document.getElementById('mwConfetti');
        box.innerHTML = '';
        var colors = ['#ff8a3d', '#2f9bff', '#22c08a', '#ffd23f', '#e255a8', '#7c5cff'];
        for (var i = 0; i < 48; i++) {
            var c = el('span', 'mw-confetti-bit');
            c.style.left = Math.random() * 100 + '%';
            c.style.background = colors[i % colors.length];
            c.style.animationDelay = (Math.random() * 0.8) + 's';
            c.style.animationDuration = (2.2 + Math.random() * 1.6) + 's';
            c.style.transform = 'rotate(' + Math.floor(Math.random() * 360) + 'deg)';
            box.appendChild(c);
        }
    }

    // ---------------------------------------------------------------- Nút điều khiển
    var soundToggle = document.getElementById('mwSoundToggle');
    function renderSoundToggle() {
        soundToggle.setAttribute('aria-pressed', soundOn ? 'true' : 'false');
        soundToggle.querySelector('.material-symbols-outlined').textContent = soundOn ? 'volume_up' : 'volume_off';
    }
    soundToggle.addEventListener('click', function () {
        soundOn = !soundOn;
        try { localStorage.setItem('mw-sound', soundOn ? '1' : '0'); } catch (e) { }
        if (!soundOn) stopVoice();
        renderSoundToggle();
        sfx('tap');
    });
    renderSoundToggle();

    document.getElementById('mwSpeak').addEventListener('click', function () {
        var cur = state.current;
        if (!cur) return;
        sfx('tap');
        voice(cur.done ? voiceKey(cur.q, 'result') : askItem(cur.q));
    });

    document.getElementById('mwHint').addEventListener('click', function () {
        var cur = state.current;
        if (!cur || cur.done) return;
        sfx('tap');
        showHint(Math.min(2, cur.hints + 1));
    });

    // Cho phép kiểm thử tự động đọc trạng thái mà không ảnh hưởng người dùng.
    window.MathWorldPlay = { state: state, config: cfg };

    if (questions.length === 0) {
        say('Bài này chưa có câu hỏi. Con quay lại đảo chọn bài khác nhé!', 'think');
        return;
    }
    startQuestion();
    fillMissingVoices(1);

    // Màn bắt đầu: iPad/Safari chỉ cho phát âm thanh sau một lần chạm, nên lần chạm này mở khóa Voice
    // và đọc giới thiệu bài cùng câu hỏi đầu tiên.
    var startGate = document.getElementById('mwStart');
    function begin() {
        if (state.started) return;
        state.started = true;
        state.startedAt = Date.now();
        if (startGate) startGate.hidden = true;
        root.classList.remove('is-waiting');
        sfx('tap');
        voice(['intro', askItem(questions[state.index])]);
    }
    if (startGate) {
        root.classList.add('is-waiting');
        startGate.hidden = false;
        startGate.querySelector('button').addEventListener('click', begin);
    } else {
        state.started = true;
    }
})();
