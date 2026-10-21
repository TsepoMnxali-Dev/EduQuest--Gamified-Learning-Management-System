
/* =============================================================
   EduQuest - math-format.js
   Shows maths in questions the way a maths paper does: stacked
   fractions with a fraction bar, powers, roots, limits and italic
   variables. Needs KaTeX (js/vendor/katex/) loaded first; without it
   the text falls back to simple superscripts / subscripts.

       EduQuestMath.toHtml("d/dx[x^n] = n·x^(n-1), so x³/3 ...")

   Plain text is turned into maths automatically:
       x^2   x^(n-1)   x²   x_1        powers and subscripts
       a/b   1/2   x^3/3   d/dx[x^n]   stacked fractions
       sqrt(x+1)   √x                  roots
       lim h→0 [f(x+h) - f(x)] / h     limits
       <=  >=  !=  ->  *               ≤  ≥  ≠  →  ×
   Units such as "20 m/s" and ordinary sentences are left alone.

   Real LaTeX is the preferred format and is what the AI generator is
   told to write. Wrap it in dollar signs (also $$..$$, \(..\), \[..\]):
       Find $\frac{x^2 + 1}{\sqrt{x}}$ when $x = 4$.
   If a text contains any delimited LaTeX it is treated as LaTeX-authored:
   the maths is typeset exactly as written and nothing else is guessed.
   Only text with no delimiters at all (older questions, the question
   bank, quick typing) goes through the plain-text detection above.
   Everything is escaped, so it is safe to use on typed text.
============================================================= */
window.EduQuestMath = (function () {
    "use strict";

    function esc(value) {
        return String(value)
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#39;");
    }

    /* s[start] must be "(". Returns the text inside the matching ")" and the index after it. */
    function readGroup(s, start) {
        var depth = 0;
        for (var j = start; j < s.length; j++) {
            if (s.charAt(j) === "(") { depth += 1; }
            else if (s.charAt(j) === ")") {
                depth -= 1;
                if (depth === 0) { return { inner: s.slice(start + 1, j), end: j + 1 }; }
            }
        }
        return null;
    }

    /* Inside a superscript / subscript, "n-1" is a subtraction: use the real minus sign. */
    function scriptMinus(s) {
        return s.replace(/^-/, "−").replace(/([A-Za-z0-9)])-(?=[A-Za-z0-9(])/g, "$1−");
    }

    /* Reads what follows a ^ or _ : a (group), an optionally signed number, or a single letter. */
    function readScript(s, start, isSubscript) {
        var ch = s.charAt(start);
        if (ch === "(") {
            var group = readGroup(s, start);
            if (group) { return { html: renderBasic(scriptMinus(group.inner)), end: group.end }; }
            return null;
        }
        var m = /^([+-]?)(\d+|[A-Za-z])/.exec(s.slice(start));
        if (!m) { return null; }
        /* a_n is a subscript, but the "c" in snake_case is just part of a word. */
        if (isSubscript && /[A-Za-z]/.test(m[2]) && /[A-Za-z]/.test(s.charAt(start + m[0].length))) { return null; }
        var sign = m[1] === "-" ? "−" : m[1];
        return { html: esc(sign + m[2]), end: start + m[0].length };
    }

    function renderBasic(s) {
        var out = "";
        var i = 0;

        while (i < s.length) {
            var c = s.charAt(i);
            var prev = i > 0 ? s.charAt(i - 1) : "";
            var next = s.charAt(i + 1);
            var script;

            if (c === "^") {
                script = readScript(s, i + 1);
                if (script) { out += "<sup>" + script.html + "</sup>"; i = script.end; continue; }
            }

            /* Subscript only straight after a letter, digit or ")", so "____" blanks and snake_case-style text are safe. */
            if (c === "_" && /[A-Za-z0-9)]/.test(prev) && next !== "_") {
                script = readScript(s, i + 1, true);
                if (script) { out += "<sub>" + script.html + "</sub>"; i = script.end; continue; }
            }

            if (c === "s" && s.substr(i, 5) === "sqrt(" && !/[A-Za-z]/.test(prev)) {
                var group = readGroup(s, i + 4);
                if (group) {
                    out += '<span class="m-rad">√<span class="m-radicand">' + renderBasic(group.inner) + "</span></span>";
                    i = group.end;
                    continue;
                }
            }

            if (c === "<" && next === "=") { out += "≤"; i += 2; continue; }
            if (c === ">" && next === "=") { out += "≥"; i += 2; continue; }
            if (c === "!" && next === "=") { out += "≠"; i += 2; continue; }

            if (c === "*" && prev === " " && next === " ") { out += "×"; i += 1; continue; }

            if (c === "-") {
                var before2 = i > 1 ? s.charAt(i - 2) : "";
                var spaced = prev === " " && next === " ";
                var sign = /\d/.test(next) &&
                    (i === 0 || /[(\[=,+*×\/^]/.test(prev) || (prev === " " && /[=(\[,+\-*×]/.test(before2)));
                if (spaced || sign) { out += "−"; i += 1; continue; }
            }

            out += esc(c);
            i += 1;
        }
        return out;
    }


    /* =========================================================
       Plain text -> LaTeX -> KaTeX
    ========================================================= */

    var SUPERSCRIPTS = { "⁰": "0", "¹": "1", "²": "2", "³": "3", "⁴": "4", "⁵": "5",
        "⁶": "6", "⁷": "7", "⁸": "8", "⁹": "9", "ⁿ": "n", "⁺": "+", "⁻": "-" };
    var SUBSCRIPTS = { "₀": "0", "₁": "1", "₂": "2", "₃": "3", "₄": "4", "₅": "5",
        "₆": "6", "₇": "7", "₈": "8", "₉": "9" };

    /* Characters that become a LaTeX command. */
    var SYMBOLS = {
        "·": "\\cdot ", "×": "\\times ", "÷": "\\div ", "≤": "\\le ", "≥": "\\ge ",
        "≠": "\\ne ", "≈": "\\approx ", "±": "\\pm ", "→": "\\to ", "∞": "\\infty ",
        "π": "\\pi ", "θ": "\\theta ", "Δ": "\\Delta ", "α": "\\alpha ", "β": "\\beta ",
        "ω": "\\omega ", "μ": "\\mu ", "λ": "\\lambda ", "σ": "\\sigma ", "φ": "\\phi ",
        "ρ": "\\rho ", "Ω": "\\Omega ", "∂": "\\partial ", "∑": "\\sum ", "∫": "\\int ",
        "°": "^{\\circ}", "%": "\\%", "&": "\\&", "#": "\\#", "$": "\\$", "{": "\\{", "}": "\\}",
        "~": "\\sim ", "\\": "\\backslash "
    };

    var FUNCTIONS = { lim: 1, sin: 1, cos: 1, tan: 1, sec: 1, csc: 1, cot: 1, log: 1, ln: 1 };
    var WORD_SYMBOLS = { pi: "\\pi ", theta: "\\theta " };

    /* Tidies typed shortcuts and unicode into one simple form before parsing. */
    function normalise(s) {
        s = s.replace(/[⁰¹²³⁴-⁻ⁿ]+/g, function (m) {
            var out = "";
            for (var i = 0; i < m.length; i++) { out += SUPERSCRIPTS[m.charAt(i)] || ""; }
            return "^" + out;
        });
        s = s.replace(/[₀-₉]+/g, function (m) {
            var out = "";
            for (var i = 0; i < m.length; i++) { out += SUBSCRIPTS[m.charAt(i)] || ""; }
            return "_" + out;
        });
        return s
            .replace(/−|–/g, "-")
            .replace(/′/g, "'")
            .replace(/<=/g, "≤")
            .replace(/>=/g, "≥")
            .replace(/!=/g, "≠")
            .replace(/->/g, "→")
            .replace(/√\s*\(/g, "sqrt(")
            .replace(/√\s*([A-Za-z0-9]+)/g, "sqrt($1)")
            /* "lim h→0" gets its target written underneath. */
            .replace(/\blim\s+([A-Za-z])\s*→\s*([^\s\])]+)/g, "lim_($1→$2)")
            /* ... and so does "lim(h → 0)". */
            .replace(/\blim\s*\(\s*([A-Za-z])\s*→\s*([^()]*?)\s*\)/g, "lim_($1→$2)");
    }

    /* s[start] is "(" or "[": returns the text inside the matching bracket and the index after it. */
    function readBracket(s, start) {
        var open = s.charAt(start);
        var close = open === "(" ? ")" : "]";
        var depth = 0;
        for (var j = start; j < s.length; j++) {
            if (s.charAt(j) === open) { depth += 1; }
            else if (s.charAt(j) === close) {
                depth -= 1;
                if (depth === 0) { return { inner: s.slice(start + 1, j), end: j + 1 }; }
            }
        }
        return null;
    }

    /* Reads what follows a ^ or _ : a (group), a signed number or a single letter. */
    function readScriptTarget(s, start, isSubscript) {
        if (s.charAt(start) === "(") {
            var g = readBracket(s, start);
            return g ? { latex: convert(g.inner), end: g.end } : null;
        }
        var m = /^([+-]?)(\d+|[A-Za-z])/.exec(s.slice(start));
        if (!m) { return null; }
        if (isSubscript && /[A-Za-z]/.test(m[2]) && /[A-Za-z]/.test(s.charAt(start + m[0].length))) { return null; }
        return { latex: m[1] + m[2], end: start + m[0].length };
    }

    function needsBigBrackets(latex) {
        return /\\frac|\\sqrt|\\sum|\\int/.test(latex);
    }

    /* Plain maths text -> LaTeX. */
    function convert(src) {
        var s = normalise(src);
        var pieces = [];
        var cur = null;
        var i = 0;

        function addAtom(latex, raw, kind, inner) {
            if (!cur) {
                cur = { t: "chain", latex: "", raw: "", kinds: [], inner: null };
                pieces.push(cur);
            }
            cur.latex += latex;
            cur.raw += raw;
            cur.kinds.push(kind);
            cur.inner = (kind === "group" && cur.kinds.length === 1) ? inner : null;
        }

        function readScripts() {
            while (i < s.length && (s.charAt(i) === "^" || s.charAt(i) === "_")) {
                var isSub = s.charAt(i) === "_";
                var target = readScriptTarget(s, i + 1, isSub);
                if (!target) { return; }
                cur.latex += (isSub ? "_{" : "^{") + target.latex + "}";
                cur.raw += s.charAt(i) + s.slice(i + 1, target.end);
                cur.inner = null;
                i = target.end;
            }
        }

        while (i < s.length) {
            var c = s.charAt(i);
            var m;

            if (/\s/.test(c)) {
                cur = null;
                pieces.push({ t: "space" });
                while (i < s.length && /\s/.test(s.charAt(i))) { i += 1; }
                continue;
            }

            if ((m = /^\d+(?:\.\d+)?/.exec(s.slice(i)))) {
                var num = m[0];
                var after = s.charAt(i + num.length);
                var tail = /^,\d/.test(s.slice(i + num.length)) ? /^,\d+/.exec(s.slice(i + num.length))[0] : "";
                addAtom(num + (tail ? "{,}" + tail.slice(1) : ""), num + tail, "num");
                i += num.length + tail.length;
                if (after === "^" || after === "_") { /* e.g. 2^3 */ }
                readScripts();
                continue;
            }

            if ((m = /^[A-Za-z]+/.exec(s.slice(i)))) {
                var word = m[0];
                var next = s.charAt(i + word.length);

                if (word === "sqrt" && next === "(") {
                    var rg = readBracket(s, i + word.length);
                    if (rg) {
                        addAtom("\\sqrt{" + convert(rg.inner) + "}", "sqrt(" + rg.inner + ")", "sqrt");
                        i = rg.end;
                        readScripts();
                        continue;
                    }
                }

                var latexWord;
                if (FUNCTIONS[word.toLowerCase()] && word === word.toLowerCase()) { latexWord = "\\" + word + " "; }
                else if (WORD_SYMBOLS[word]) { latexWord = WORD_SYMBOLS[word]; }
                else if (/^d[a-z]$/.test(word)) { latexWord = "\\mathrm{d}" + word.charAt(1); }
                else { latexWord = word; }

                addAtom(latexWord, word, FUNCTIONS[word] ? "func" : "id");
                i += word.length;
                readScripts();
                continue;
            }

            if (c === "(" || c === "[") {
                var group = readBracket(s, i);
                if (group) {
                    /* "(" right after a name or number is part of it: f(x), 3(x+1). "[" always starts a new term. */
                    if (c === "[") { cur = null; }
                    var inner = convert(group.inner);
                    var big = needsBigBrackets(inner);
                    var open = big ? "\\left" + c : c;
                    var close = big ? "\\right" + (c === "(" ? ")" : "]") : (c === "(" ? ")" : "]");
                    addAtom(open + inner + close, c + group.inner + (c === "(" ? ")" : "]"), "group", inner);
                    i = group.end;
                    readScripts();
                    continue;
                }
            }

            /* Anything else is an operator or symbol. */
            cur = null;
            var latexOp;
            if (SYMBOLS[c]) { latexOp = SYMBOLS[c]; }
            else if (c === "*") { latexOp = "\\times "; }
            else if (c === ",") { latexOp = ",\\,"; }
            else if (c === "^" || c === "_") { latexOp = c === "^" ? "\\^{}" : "\\_"; }
            else if (/[ -~]/.test(c)) { latexOp = c; }
            else { latexOp = "\\text{" + c + "}"; }
            pieces.push({ t: "op", latex: latexOp, raw: c });
            i += 1;
        }

        return joinPieces(buildFractions(pieces));
    }

    function prevIndex(pieces, k) {
        var j = k - 1;
        while (j >= 0 && pieces[j].t === "space") { j -= 1; }
        return j;
    }

    function nextIndex(pieces, k) {
        var j = k + 1;
        while (j < pieces.length && pieces[j].t === "space") { j += 1; }
        return j;
    }

    /*
       FIX (2026-10-18): Physical units use / as part of a unit, not as a
       request to stack a fraction. Previous code recognised plain km/h but
       misread m/s^2 and 9.8m/s as mathematical fractions.
       Match recognised unit SYMBOLS only, so x/y and 1/2 stay fractions.
       Explicit $\\frac{...}{...}$ is not processed here at all.
    */
    var UNIT_NUMERATOR = /^(?:\d+(?:\.\d+)?)?(?:kWh|kPa|kW|kJ|kg|km|cm|mm|dm|mg|mol|mL|Hz|Pa|N|J|W|C|V|A|g|m|L|Ω)(?:\^(?:\([+-]?\d+\)|[+-]?\d+))?$/;
    var UNIT_DENOMINATOR = /^(?:kPa|kg|km|cm|mm|dm|mg|mol|mL|min|ms|hr|Pa|kJ|kW|N|J|W|C|V|A|K|s|m|g|h|L)(?:\^(?:\([+-]?\d+\)|[+-]?\d+))?$/;

    function isPhysicalUnitRatio(top, bottom) {
        return UNIT_NUMERATOR.test(top) && UNIT_DENOMINATOR.test(bottom);
    }

    /* Turns  a / b  into a stacked fraction. */
    function buildFractions(pieces) {
        var k = 0;
        while (k < pieces.length) {
            var piece = pieces[k];
            if (piece.t === "op" && piece.raw === "/") {
                var p = prevIndex(pieces, k);
                var n = nextIndex(pieces, k);
                var prev = p >= 0 ? pieces[p] : null;
                var next = n < pieces.length ? pieces[n] : null;

                if (prev && next && prev.t === "chain" && next.t === "chain") {
                    var isDerivative = /^d[A-Za-z]$/.test(next.raw) && /^d[A-Za-z]?$/.test(prev.raw);
                    var lettersOnly = /^[A-Za-z]+$/.test(prev.raw) && /^[A-Za-z]+$/.test(next.raw);
                    var before = p > 0 ? pieces[prevIndex(pieces, p)] : null;
                    var afterNumber = !!(before && before.t === "chain" && before.kinds.every(function (kd) { return kd === "num"; }));
                    var unitLike = lettersOnly && !isDerivative &&
                        (prev.raw.length > 1 || next.raw.length > 1 || afterNumber);

                    var replacement;
                    if (isPhysicalUnitRatio(prev.raw, next.raw)) {
                        // Keep the slash inline, but keep scripts such as s^2
                        // properly typeset as powers (not plain ^2 text).
                        replacement = { t: "chain", latex: "\\mathrm{" + prev.latex + "/" + next.latex + "}", raw: prev.raw + "/" + next.raw, kinds: ["unit"], inner: null };
                    } else if (unitLike) {
                        replacement = { t: "chain", latex: "\\mathrm{" + prev.raw + "/" + next.raw + "}", raw: prev.raw + "/" + next.raw, kinds: ["unit"], inner: null };
                    } else {
                        var top = prev.raw === "d" && isDerivative ? "\\mathrm{d}" : (prev.inner !== null ? prev.inner : prev.latex);
                        var bottom = next.inner !== null ? next.inner : next.latex;
                        replacement = { t: "chain", latex: "\\frac{" + top + "}{" + bottom + "}", raw: prev.raw + "/" + next.raw, kinds: ["frac"], inner: null };
                    }
                    pieces.splice(p, n - p + 1, replacement);
                    k = p + 1;
                    continue;
                }
            }
            k += 1;
        }
        return pieces;
    }

    function joinPieces(pieces) {
        return pieces.map(function (piece) {
            return piece.t === "space" ? " " : piece.latex;
        }).join("").replace(/\s+/g, " ").trim();
    }

    function renderKatex(latex, plain) {
        try {
            var html = window.katex.renderToString("\\displaystyle " + latex, {
                throwOnError: false,
                strict: false,
                trust: false,
                output: "html"
            });
            return '<span class="m-eq" role="math" aria-label="' + esc(plain) + '">' + html + "</span>";
        } catch (err) {
            return esc(plain);
        }
    }

    /* ---------- finding the maths inside ordinary sentences ---------- */

    var OPERATOR_WORDS = /^(?:=|\+|-|–|−|\*|×|÷|·|\/|<|>|≤|≥|≠|≈|<=|>=|!=|→|->|±)$/;
    var MATH_CHARS = /[⁰¹²³⁴-⁻ⁿ₀-₉≤≥≠≈±∞π°θΔ√∑∫∂→·×÷]/;
    var UNIT_SKIP = /^\d+(?:st|nd|rd|th)$/i;

    function stripBrackets(w) { return w.replace(/[()\[\]{}]/g, ""); }

    /* "M" = clearly maths, "N" = could be part of an expression (number, lone letter), "O" = operator, "F" = function name, "T" = ordinary text. */
    function classify(core, prevCore, prevType) {
        if (OPERATOR_WORDS.test(core)) { return "O"; }
        var probe = stripBrackets(core);
        if (probe === "") { return "T"; }

        if (/^(?:lim|sin|cos|tan|sec|cosec|cot|log|ln)$/.test(probe)) { return "F"; }
        if (/^\d+(?:[.,]\d+)?%?$/.test(probe)) { return "N"; }
        if (/^[A-Za-z]$/.test(probe) && !/^[aAI]$/.test(probe)) { return "N"; }

        if (/\^/.test(probe)) { return "M"; }
        if (/(?:^|[^A-Za-z])d\/d[A-Za-z]/.test(core)) { return "M"; }                  /* d/dx[5x]=5 */
        if (/[A-Za-z0-9)\]]->[A-Za-z0-9(\[]/.test(core)) { return "M"; }              /* h->0 */
        if (/[A-Za-z0-9)]_(?:\d+|\(|[A-Za-z](?![A-Za-z]))/.test(core)) { return "M"; }
        if (/sqrt\(/.test(core)) { return "M"; }
        if (MATH_CHARS.test(probe)) { return "M"; }
        if (/^\d+(?:\.\d+)?[A-Za-z]$/.test(probe) && !UNIT_SKIP.test(probe)) { return "M"; }
        if (/^[A-Za-z]['′]*\(.*\)\]?$/.test(core.replace(/^[(\[]+/, ""))) { return "M"; }
        if (/(?:^|[^A-Za-z])[A-Za-z]['′]*\([^()]*\)/.test(core)) { return "M"; }       /* (a, f(a))  f''(x) */
        if (/[A-Za-z0-9)\]][+=*][A-Za-z0-9(\[]/.test(core)) { return "M"; }
        if (/(?:^|[^A-Za-z])[A-Za-z]-\d/.test(core) || /\d-[A-Za-z](?![A-Za-z])/.test(core)) { return "M"; }

        if (/[)\]]\/[A-Za-z0-9(\[]|[A-Za-z0-9)\]]\/[(\[]/.test(core)) { return "M"; }

        /* a/b  1/2  x^3/3  d/dx[...] - but not and/or, km/h, or a unit after a number (20 m/s). */
        var slash = probe.split("/");
        if (slash.length === 2 && slash[0] !== "" && slash[1] !== "") {
            var left = slash[0];
            var right = slash[1];
            var okPart = /^[\w.+\-·*=]+$/;
            if (okPart.test(left) && okPart.test(right)) {
                var derivative = /^d[A-Za-z]?$/.test(left) && /^d[A-Za-z]$/.test(right);
                var lettersOnly = /^[A-Za-z]+$/.test(left) && /^[A-Za-z]+$/.test(right);
                if (derivative) { return "M"; }
                if (!lettersOnly) { return "M"; }
                if (left.length === 1 && right.length === 1 && !(prevType === "N" && /^\d/.test(prevCore || ""))) { return "M"; }
            }
        }
        return "T";
    }

    function countChar(s, ch) { return s.split(ch).length - 1; }

    /* ---------- keeping one expression in one piece ----------
       autoMath looks at text one space-separated word at a time. Without help, an
       expression such as  lim (h → 0) [f(x + h) - f(x)] / h  is cut into fragments and only
       some of them are recognised, so part of it is typeset and part is left as plain text.
       mergeWords joins the words that obviously belong together first:
         - everything from an opening ( or [ to its closing bracket,
         - a function name and the bracket after it:  lim (h → 0)
         - both sides of a division written with spaces:  [..] / h
       It never joins across an ordinary English word, so "(see x^2 below)" is left alone. */

    var KNOWN_WORDS = /^(?:lim|sin|cos|tan|sec|csc|cosec|cot|log|ln|sqrt|exp|min|max|pi|theta|sinh|cosh|tanh)$/;

    function hasProse(s) {
        var runs = s.match(/[A-Za-z]+/g) || [];
        for (var i = 0; i < runs.length; i++) {
            if (runs[i].length >= 3 && !KNOWN_WORDS.test(runs[i])) { return true; }
        }
        return false;
    }

    function openDepth(s) {
        var depth = 0;
        for (var i = 0; i < s.length; i++) {
            var ch = s.charAt(i);
            if (ch === "(" || ch === "[") { depth += 1; }
            else if (ch === ")" || ch === "]") { depth -= 1; }
        }
        return depth;
    }

    /* parts = text.split(/(\s+)/): words at even positions, the whitespace after them at odd ones. */
    function mergeWords(parts) {
        var words = [];
        var seps = [];
        for (var p = 0; p < parts.length; p += 2) {
            words.push(parts[p]);
            seps.push(parts[p + 1] || "");
        }

        var out = [];
        var i = 0;
        while (i < words.length) {
            var unit = words[i];
            var last = i;
            var again = unit !== "";

            while (again && last + 1 < words.length && last - i < 14) {
                var next = words[last + 1];
                var take = 0;
                again = false;

                if (openDepth(unit) > 0) { take = 1; }
                else if (/^(?:lim|sin|cos|tan|sec|csc|cosec|cot|log|ln)$/.test(unit) && next.charAt(0) === "(") { take = 1; }
                else if (unit.length > 1 && unit.slice(-1) === "/" && next !== "") { take = 1; }
                else if (next === "/" && last + 2 < words.length && words[last + 2] !== "") { take = 2; }
                else if (next.length > 1 && next.charAt(0) === "/") { take = 1; }

                if (take) {
                    var joined = unit;
                    for (var t = 0; t < take; t++) { joined += seps[last + t] + words[last + 1 + t]; }
                    if (!hasProse(joined)) {
                        unit = joined;
                        last += take;
                        again = true;
                    }
                }
            }

            out.push(unit);
            out.push(seps[last]);
            i = last + 1;
        }
        return out;
    }

    /* Splits ordinary text into words and maths, then renders each maths run. */
    function autoMath(text) {
        var parts = mergeWords(text.split(/(\s+)/));   /* words at even positions, whitespace at odd */
        var words = [];
        var prevCore = "";
        var prevType = "T";

        for (var w = 0; w < parts.length; w += 2) {
            var word = parts[w];
            var trailing = (/[.,;:!?"'”’]+$/.exec(word) || [""])[0];
            var core = trailing ? word.slice(0, word.length - trailing.length) : word;
            var type = core === "" ? "T" : classify(core.replace(/\s+/g, ""), prevCore, prevType);
            words.push({ index: w, text: word, core: core, trailing: trailing, type: type });
            prevCore = core;
            prevType = type;
        }

        var out = "";
        var cursor = 0;                              /* next word to output */
        var k = 0;

        function flushText(upTo) {
            for (var x = cursor; x < upTo; x++) {
                out += esc(words[x].text) + (parts[words[x].index + 1] || "");
            }
            cursor = upTo;
        }

        while (k < words.length) {
            if (words[k].type === "T") { k += 1; continue; }

            /* collect consecutive maths-ish words; trailing punctuation ends the run */
            var start = k;
            var end = k;
            while (end < words.length && words[end].type !== "T") {
                if (words[end].trailing && end > start) { end += 1; break; }
                if (words[end].trailing) { end += 1; break; }
                end += 1;
            }
            var last = end - 1;

            /* trim operators / function names that dangle at the edges */
            var a = start;
            var b = last;
            while (a <= b && (words[a].type === "O" || words[a].type === "F" && !(a < b && words[a + 1].type !== "O"))) { a += 1; }
            while (b >= a && (words[b].type === "O" || words[b].type === "F")) {
                if (words[b].trailing) { break; }
                b -= 1;
            }

            var hasMath = false;
            var hasOp = false;
            for (var x = a; x <= b; x++) {
                if (words[x].type === "M") { hasMath = true; }
                if (words[x].type === "O" && x > a && x < b) { hasOp = true; }
            }

            if (a <= b && (hasMath || hasOp)) {
                flushText(a);
                var expr = "";
                for (var y = a; y <= b; y++) {
                    expr += (y === b ? words[y].core : words[y].text) + (y < b ? parts[words[y].index + 1] : "");
                }
                var lead = "";
                var tailText = words[b].trailing;

                /* brackets that open or close outside the run stay as ordinary text */
                while (/^\(/.test(expr) && countChar(expr, "(") > countChar(expr, ")")) { lead += "("; expr = expr.slice(1); }
                while (/^\[/.test(expr) && countChar(expr, "[") > countChar(expr, "]")) { lead += "["; expr = expr.slice(1); }
                while (/\)$/.test(expr) && countChar(expr, ")") > countChar(expr, "(")) { tailText = ")" + tailText; expr = expr.slice(0, -1); }
                while (/\]$/.test(expr) && countChar(expr, "]") > countChar(expr, "[")) { tailText = "]" + tailText; expr = expr.slice(0, -1); }

                out += esc(lead) + renderKatex(convert(expr), expr) + esc(tailText) + (parts[words[b].index + 1] || "");
                cursor = b + 1;
            }
            k = Math.max(end, k + 1);
        }
        flushText(words.length);
        return out;
    }

    /* ---------- explicit LaTeX: $..$  $$..$$  \(..\)  \[..\] ---------- */

    /* Splits text into plain and LaTeX segments. "\$" is a literal dollar sign. A single $ only
       opens maths when the next character is not a space, and only closes it when the previous
       character is not a space and no digit follows, so "it costs $5 and $10" stays plain text. */
    function splitMath(text) {
        var segs = [];
        var buf = "";
        var n = text.length;
        var i = 0;

        function flush() {
            if (buf) { segs.push({ math: false, value: buf }); buf = ""; }
        }
        function addMath(latex) {
            flush();
            segs.push({ math: true, value: latex });
        }
        function singleClose(from) {
            if (from >= n || /\s/.test(text.charAt(from))) { return -1; }
            for (var j = from; j < n; j++) {
                var ch = text.charAt(j);
                if (ch === "\\") { j += 1; continue; }
                if (ch === "$") {
                    if (/\s/.test(text.charAt(j - 1))) { return -1; }
                    if (/\d/.test(text.charAt(j + 1))) { continue; }
                    return j;
                }
            }
            return -1;
        }

        while (i < n) {
            var c = text.charAt(i);
            var d = text.charAt(i + 1);
            var end;

            if (c === "\\" && d === "$") { buf += "$"; i += 2; continue; }

            if (c === "\\" && (d === "(" || d === "[")) {
                end = text.indexOf(d === "(" ? "\\)" : "\\]", i + 2);
                if (end > i + 2) { addMath(text.slice(i + 2, end)); i = end + 2; continue; }
            }

            if (c === "$") {
                if (d === "$") {
                    end = text.indexOf("$$", i + 2);
                    if (end > i + 2) { addMath(text.slice(i + 2, end)); i = end + 2; continue; }
                }
                end = singleClose(i + 1);
                if (end > i + 1) { addMath(text.slice(i + 1, end)); i = end + 1; continue; }
            }

            buf += c;
            i += 1;
        }
        flush();
        return segs;
    }

    /* LaTeX written without any dollar signs, e.g. an option that is just  \frac{x^3}{3} . */
    var BARE_LATEX = /\\(?:frac|dfrac|sqrt|lim|int|sum|prod|cdot|times|div|to|leq?|geq?|neq|pm|infty|left|right|mathrm|sin|cos|tan|log|ln|theta|pi|alpha|beta|Delta)(?![A-Za-z])/;

    function isBareLatex(s) {
        return BARE_LATEX.test(s) && !/[A-Za-z]{4,}/.test(s.replace(/\\[A-Za-z]+/g, ""));
    }

    /* Delimited LaTeX is typeset exactly as written and the text around it is left alone.
       Only text with no delimiters at all is searched for maths. */
    function toKatexHtml(text) {
        var segs = splitMath(text);
        var hasLatex = false;
        var plain = "";
        segs.forEach(function (seg) {
            if (seg.math) { hasLatex = true; } else { plain += seg.value; }
        });

        if (!hasLatex) {
            if (isBareLatex(plain)) { return renderKatex(plain.trim(), plain); }
            return autoMath(plain);
        }

        return segs.map(function (seg) {
            return seg.math ? renderKatex(seg.value.trim(), seg.value) : esc(seg.value);
        }).join("");
    }

    return {
        toHtml: function (text) {
            var value = String(text === null || text === undefined ? "" : text);
            if (window.katex && typeof window.katex.renderToString === "function") {
                return toKatexHtml(value);
            }
            return renderBasic(value);
        }
    };
})();
