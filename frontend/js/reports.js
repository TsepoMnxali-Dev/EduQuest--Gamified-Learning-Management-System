
/* EduQuest Admin Reports: live statistics and activity logs.*/

(function () {
    'use strict';

    const api = window.EduQuestAPI;
    const $ = id => document.getElementById(id);
    const nf = new Intl.NumberFormat('en-ZA');

    const state = {
        data: {},
        errors: {},
        logs: [],
        logsLoaded: false,
        filtered: [],
        page: 1,
        pageSize: 15
    };

    const endpoints = {
        overview: '/Statistics/overview',
        province: '/Statistics/learners/by-province',
        school: '/Statistics/learners/by-school',
        grade: '/Statistics/learners/by-grade',
        passFail: '/Statistics/performance/pass-fail',
        subjectPerf: '/Statistics/performance/by-subject',
        gradePerf: '/Statistics/performance/by-grade',
        provincePerf: '/Statistics/performance/by-province',
        topicPerf: '/Statistics/performance/by-topic',
        schoolPerf: '/Statistics/performance/by-school',
        quizAttempts: '/Statistics/engagement/quiz-attempts',
        quizCompletion: '/Statistics/engagement/quiz-completion',
        competitions: '/Statistics/engagement/competitions',
        achievements: '/Statistics/engagement/achievements',
        logs: '/ActivityLogs'
    };

    function el(tag, cls, text) {
        const e = document.createElement(tag);

        if (cls) {
            e.className = cls;
        }

        if (text !== undefined) {
            e.textContent = String(text);
        }

        return e;
    }

    function clear(node) {
        node.replaceChildren();
    }

    function empty(node, message) {
        clear(node);
        node.append(el('p', 'empty', message));
    }

    function value(x) {
        return typeof x === 'number' && Number.isFinite(x)
            ? nf.format(x)
            : '—';
    }

    function pct(x) {
        return typeof x === 'number' && Number.isFinite(x)
            ? x.toFixed(1) + '%'
            : '—';
    }

    function err(key) {
        return state.errors[key]
            ? 'Unavailable: ' + state.errors[key]
            : 'No records available yet.';
    }

    function metrics(node, items) {
        clear(node);

        items.forEach(([label, v]) => {
            const row = el('div', 'metric-row');

            row.append(
                el('span', '', label),
                el('strong', '', v)
            );

            node.append(row);
        });
    }

    function renderOverview() {
        const node = $('summaryCards');

        clear(node);

        const d = state.data.overview;

        if (!d) {
            empty(node, err('overview'));
            return;
        }

        [
            ['Total learners', d.totalLearners],
            ['Active learner accounts', d.activeLearners],
            ['Inactive learner accounts', d.inactiveLearners],
            ['Quiz attempts', d.totalQuizAttempts],
            ['Competition participations', d.totalCompetitionParticipants],
            ['Achievements earned', d.totalAchievementsEarned]
        ].forEach(([label, num]) => {
            const card = el('article', 'summary-tile');

            card.append(
                el('div', 'label', label),
                el('div', 'number', value(num))
            );

            node.append(card);
        });
    }

    function bars(id, key, labelKey, valKey, percent = false) {
        const node = $(id);
        const items = state.data[key];

        if (!Array.isArray(items) || !items.length) {
            empty(node, err(key));
            return;
        }

        clear(node);

        const sorted = [...items].sort(
            (a, b) =>
                (Number(b[valKey]) || 0) -
                (Number(a[valKey]) || 0)
        );

        const max = Math.max(
            ...sorted.map(x => Number(x[valKey]) || 0),
            0
        );

        sorted.forEach(item => {
            const amount = Number(item[valKey]) || 0;

            const row = el('div', 'bar-line');
            const track = el('div', 'bar-track');
            const fill = el('div', 'bar-fill');

            fill.style.width = (
                max > 0
                    ? Math.max(
                        0,
                        Math.min(100, amount / max * 100)
                    )
                    : 0
            ) + '%';

            track.append(fill);

            row.append(
                el(
                    'span',
                    'bar-name',
                    item[labelKey] || 'Unspecified'
                ),
                track,
                el(
                    'span',
                    'bar-value',
                    percent ? pct(amount) : value(amount)
                )
            );

            node.append(row);
        });
    }

    function renderStats() {
        renderOverview();

        bars(
            'provinceChart',
            'province',
            'provinceName',
            'learnerCount'
        );

        bars(
            'gradeChart',
            'grade',
            'gradeName',
            'learnerCount'
        );

        bars(
            'schoolChart',
            'school',
            'schoolName',
            'learnerCount'
        );

        bars(
            'subjectPerformance',
            'subjectPerf',
            'subjectName',
            'averageScore',
            true
        );

        bars(
            'gradePerformance',
            'gradePerf',
            'gradeName',
            'averageScore',
            true
        );

        bars(
            'provincePerformance',
            'provincePerf',
            'provinceName',
            'averageScore',
            true
        );

        bars(
            'topicPerformance',
            'topicPerf',
            'topicName',
            'averageScore',
            true
        );

        bars(
            'schoolPerformance',
            'schoolPerf',
            'schoolName',
            'averageScore',
            true
        );

        const pf = state.data.passFail;

        if (pf) {
            metrics($('passFail'), [
                ['Attempts', value(pf.totalAttempts)],
                ['Passed', value(pf.passedAttempts)],
                ['Failed', value(pf.failedAttempts)],
                ['Pass rate', pct(pf.passRate)],
                ['Fail rate', pct(pf.failRate)]
            ]);
        } else {
            empty($('passFail'), err('passFail'));
        }

        const qa = state.data.quizAttempts;
        const qc = state.data.quizCompletion;

        if (qa || qc) {
            metrics($('quizEngagement'), [
                [
                    'Total attempts',
                    qa ? value(qa.totalAttempts) : 'Unavailable'
                ],
                [
                    'Learners who attempted',
                    qa ? value(qa.uniqueLearners) : 'Unavailable'
                ],
                [
                    'Completed quizzes',
                    qc ? value(qc.totalCompleted) : 'Unavailable'
                ],
                [
                    'Learners who completed',
                    qc ? value(qc.uniqueLearners) : 'Unavailable'
                ]
            ]);
        } else {
            empty(
                $('quizEngagement'),
                'Quiz engagement endpoints unavailable.'
            );
        }

        const c = state.data.competitions;

        if (c) {
            metrics($('competitionEngagement'), [
                ['Total participations', value(c.totalParticipations)],
                ['Unique learners', value(c.uniqueLearners)]
            ]);
        } else {
            empty(
                $('competitionEngagement'),
                err('competitions')
            );
        }

        const a = state.data.achievements;

        if (a) {
            metrics($('achievementEngagement'), [
                [
                    'Achievements earned',
                    value(a.totalAchievementsEarned)
                ],
                [
                    'Learners earning achievements',
                    value(a.uniqueLearnersWithAchievements)
                ]
            ]);
        } else {
            empty(
                $('achievementEngagement'),
                err('achievements')
            );
        }
    }

    function parseDate(s) {
        const d = new Date(s);

        return Number.isFinite(d.getTime()) ? d : null;
    }

    function localDate(d) {
        const y = d.getFullYear();
        const m = String(d.getMonth() + 1).padStart(2, '0');
        const day = String(d.getDate()).padStart(2, '0');

        return `${y}-${m}-${day}`;
    }

    function populateActions() {
        const current = $('actionFilter').value;
        const sel = $('actionFilter');

        clear(sel);

        const blank = el('option', '', 'All actions');
        blank.value = '';

        sel.append(blank);

        [
            ...new Set(
                state.logs
                    .map(x => String(x.action || '').trim())
                    .filter(Boolean)
            )
        ].sort().forEach(action => {
            const option = el('option', '', action);

            option.value = action;
            sel.append(option);
        });

        sel.value = [...sel.options].some(
            o => o.value === current
        )
            ? current
            : '';
    }

    function filterLogs() {
        const from = $('fromDate').value;
        const to = $('toDate').value;
        const action = $('actionFilter').value;
        const search = $('activitySearch').value
            .trim()
            .toLowerCase();

        state.filtered = state.logs.filter(log => {
            const d = parseDate(log.dateTime);

            if (from && (!d || localDate(d) < from)) {
                return false;
            }

            if (to && (!d || localDate(d) > to)) {
                return false;
            }

            if (action && log.action !== action) {
                return false;
            }

            return !search || [
                log.userFullName,
                log.action,
                log.ipAddress,
                log.userID
            ].some(
                v => String(v ?? '')
                    .toLowerCase()
                    .includes(search)
            );
        });

        state.page = 1;
        renderLogs();
    }

    function renderLogs() {
        const body = $('activityBody');

        clear(body);

        if (!state.logsLoaded) {
            const tr = el('tr');

            const td = el(
                'td',
                '',
                state.errors.logs
                    ? 'Unable to load activity logs: ' +
                      state.errors.logs
                    : 'Loading activity logs…'
            );

            td.colSpan = 4;

            tr.append(td);
            body.append(tr);

            $('activitySummary').textContent = '';
            $('activityCount').textContent = '';
            $('pageInfo').textContent = '';
            $('prevPage').disabled = true;
            $('nextPage').disabled = true;

            return;
        }

        const total = state.filtered.length;
        const pages = Math.max(
            1,
            Math.ceil(total / state.pageSize)
        );

        state.page = Math.max(
            1,
            Math.min(state.page, pages)
        );

        const start = (state.page - 1) * state.pageSize;

        state.filtered
            .slice(start, start + state.pageSize)
            .forEach(log => {
                const tr = el('tr');
                const d = parseDate(log.dateTime);

                const cells = [
                    d
                        ? d.toLocaleString('en-ZA')
                        : 'Unknown',
                    log.userFullName || 'Unknown',
                    log.action || 'Unknown',
                    log.ipAddress || '—'
                ];

                cells.forEach((v, i) => {
                    const td = el('td');

                    if (i === 2) {
                        td.append(el('span', 'pill', v));
                    } else {
                        td.textContent = v;
                    }

                    tr.append(td);
                });

                body.append(tr);
            });

        if (!total) {
            const tr = el('tr');

            const td = el(
                'td',
                '',
                'No activity logs match the selected filters.'
            );

            td.colSpan = 4;

            tr.append(td);
            body.append(tr);
        }

        const users = new Set(
            state.filtered.map(x => x.userID)
        ).size;

        $('activitySummary').textContent =
            `${value(total)} events • ${value(users)} distinct users`;

        $('activityCount').textContent =
            `Showing ${total ? start + 1 : 0}–` +
            `${Math.min(total, start + state.pageSize)} ` +
            `of ${value(total)}`;

        $('pageInfo').textContent =
            `Page ${state.page} of ${pages}`;

        $('prevPage').disabled = state.page === 1;
        $('nextPage').disabled = state.page >= pages;
    }

    function csvCell(v) {
        const s = String(v ?? '');

        return '"' + (
            /^[=+\-@\t\r]/.test(s) ? "'" + s : s
        ).replace(/"/g, '""') + '"';
    }

    function exportCsv() {
        const rows = [
            ['EDUQUEST REPORTS — LIVE DATA'],
            ['Generated at', new Date().toISOString()],
            [],
            ['PLATFORM OVERVIEW'],
            ['Metric', 'Value']
        ];

        const o = state.data.overview;

        if (o) {
            Object.entries(o).forEach(([k, v]) => {
                rows.push([k, v]);
            });
        } else {
            rows.push(['Unavailable', err('overview')]);
        }

        const groups = [
            [
                'LEARNERS BY PROVINCE',
                'province',
                'provinceName',
                'learnerCount'
            ],
            [
                'LEARNERS BY SCHOOL',
                'school',
                'schoolName',
                'learnerCount'
            ],
            [
                'LEARNERS BY GRADE',
                'grade',
                'gradeName',
                'learnerCount'
            ],
            [
                'PERFORMANCE BY PROVINCE',
                'provincePerf',
                'provinceName',
                'averageScore'
            ],
            [
                'PERFORMANCE BY SCHOOL',
                'schoolPerf',
                'schoolName',
                'averageScore'
            ],
            [
                'PERFORMANCE BY GRADE',
                'gradePerf',
                'gradeName',
                'averageScore'
            ],
            [
                'PERFORMANCE BY SUBJECT',
                'subjectPerf',
                'subjectName',
                'averageScore'
            ],
            [
                'PERFORMANCE BY TOPIC',
                'topicPerf',
                'topicName',
                'averageScore'
            ]
        ];

        groups.forEach(([title, key, name, score]) => {
            rows.push(
                [],
                [title],
                [name, score]
            );

            (state.data[key] || []).forEach(item => {
                rows.push([
                    item[name],
                    item[score]
                ]);
            });
        });

        [
            'passFail',
            'quizAttempts',
            'quizCompletion',
            'competitions',
            'achievements'
        ].forEach(key => {
            rows.push(
                [],
                [key.toUpperCase()],
                ['Metric', 'Value']
            );

            if (state.data[key]) {
                Object.entries(state.data[key]).forEach(
                    ([k, v]) => rows.push([k, v])
                );
            }
        });

        rows.push(
            [],
            ['ACTIVITY LOGS — FILTERED'],
            [
                'DateTime',
                'UserID',
                'User',
                'Action',
                'IPAddress'
            ]
        );

        if (state.logsLoaded) {
            state.filtered.forEach(x => {
                rows.push([
                    x.dateTime,
                    x.userID,
                    x.userFullName,
                    x.action,
                    x.ipAddress
                ]);
            });
        } else {
            rows.push([
                'Unavailable',
                state.errors.logs || 'Not loaded'
            ]);
        }

        const csv = '\uFEFF' + rows
            .map(row => row.map(csvCell).join(','))
            .join('\r\n');

        const url = URL.createObjectURL(
            new Blob(
                [csv],
                { type: 'text/csv;charset=utf-8' }
            )
        );

        const a = document.createElement('a');

        a.href = url;
        a.download =
            'EduQuest_Reports_' +
            localDate(new Date()) +
            '.csv';

        document.body.append(a);
        a.click();
        a.remove();

        setTimeout(
            () => URL.revokeObjectURL(url),
            1000
        );
    }

    async function load() {
        if (!api) {
            $('reportMessage').textContent =
                'Shared EduQuest API helper was not loaded.';
            return;
        }

        if (!api.requireAdmin()) {
            return;
        }

        const btn = $('refreshBtn');

        btn.disabled = true;

        $('reportMessage').classList.remove('error');
        $('reportMessage').textContent =
            'Loading live reports from EduQuest API…';

        state.errors = {};

        const results = await Promise.allSettled(
            Object.entries(endpoints).map(
                async ([key, path]) => [
                    key,
                    await api.get(path)
                ]
            )
        );

        results.forEach((r, i) => {
            const key = Object.keys(endpoints)[i];

            if (r.status === 'fulfilled') {
                state.data[key] = r.value[1];
            } else {
                delete state.data[key];

                state.errors[key] =
                    r.reason?.message || 'Request failed';
            }
        });

        state.logs = Array.isArray(state.data.logs)
            ? state.data.logs
            : [];

        state.logsLoaded = Array.isArray(state.data.logs);

        renderStats();
        populateActions();
        filterLogs();

        const count = Object.keys(state.errors).length;

        $('reportMessage').textContent = count
            ? `Loaded ${Object.keys(endpoints).length - count} ` +
              `of ${Object.keys(endpoints).length} data sources. ` +
              `Unavailable sections show their errors.`
            : 'All reports loaded from the live EduQuest API.';

        $('reportMessage').classList.toggle(
            'error',
            count > 0
        );

        btn.disabled = false;
    }

    function init() {
        $('refreshBtn').addEventListener(
            'click',
            load
        );

        $('exportBtn').addEventListener(
            'click',
            exportCsv
        );

        [
            'fromDate',
            'toDate',
            'actionFilter',
            'activitySearch'
        ].forEach(id => {
            $(id).addEventListener(
                id === 'activitySearch'
                    ? 'input'
                    : 'change',
                filterLogs
            );
        });

        $('resetFilters').addEventListener('click', () => {
            [
                'fromDate',
                'toDate',
                'activitySearch',
                'actionFilter'
            ].forEach(id => {
                $(id).value = '';
            });

            filterLogs();
        });

        $('prevPage').addEventListener('click', () => {
            state.page--;
            renderLogs();
        });

        $('nextPage').addEventListener('click', () => {
            state.page++;
            renderLogs();
        });

        load();
    }

    if (document.readyState === 'loading') {
        document.addEventListener(
            'DOMContentLoaded',
            init
        );
    } else {
        init();
    }
})();
