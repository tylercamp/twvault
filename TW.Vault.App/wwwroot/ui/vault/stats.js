function makeStatsTab() {
    let userStatsTab = makeUserStatsTab();
    let highScoresTab = makeHighScoresTab();

    let tabs = [
        userStatsTab,
        highScoresTab
    ];

    return {
        label: lib.translate(lib.itlcodes.TAB_STATS),
        containerId: 'vault-stats-container',

        getContent: function () {
            return uilib.mkTabbedContainer(userStatsTab, tabs);
        }
    };
}

function makeUserStatsTab() {
    return {
        label: lib.translate(lib.itlcodes.TAB_ME),
        containerId: 'vault-user-stats-container',

        init: function ($container) {
            lib.getApi('player/stats')
                .done((data) => {
                    $container.find('#stats-nukes-this-week').text(data.nukesInPastWeek);
                    $container.find('#stats-fangs-this-week').text(data.fangsInPastWeek);
                    $container.find('#stats-fakes-this-week').text(data.fakesInPastWeek);

                    $container.find('#stats-dvs-at-home').text(Math.roundTo(data.dVsAtHome, 1));
                    $container.find('#stats-dvs-at-home-backline').text(Math.roundTo(data.backlineDVsAtHome, 1));
                    $container.find('#stats-dvs-traveling').text(Math.roundTo(data.dVsTraveling, 1));

                    $container.find('table:not(.vis) td').css({
                        padding: '0 0.25em',
                        width: '50%'
                    });

                    var $supportTable = $container.find('table.vis');
                    lib.objForEach(data.popPerTribe, (tribe, dvs) => {
                        $supportTable.append(`
                            <tr>
                                <td>To ${tribe}</td>
                                <td>${Math.roundTo(dvs, 1)}</td>
                            <tr>
                        `.trim());
                    });

                    $supportTable.find('tbody tr:nth-child(even)').addClass('row_a');
                    $supportTable.find('tbody tr:nth-child(odd)').addClass('row_b');

                    $supportTable.find('td, th').css({
                        padding: '0 1em'
                    });

                    $container.find('tr td:first-of-type, tr th:first-of-type').css({
                        'text-align': 'right'
                    });

                    $container.find('tr td:last-of-type, tr th:last-of-type').css({
                        'text-align': 'left'
                    });
                })
                .error(() => {
                    if (!lib.isUnloading())
                        alert(lib.translate(lib.itlcodes.STATS_LOAD_ERROR, { _escaped: false }));
                });
        },

        getContent: `
            <h3 style="margin-bottom:0">${lib.translate(lib.itlcodes.STATS_7_DAYS)}</h3>
            <div style="display:inline-block">
                <i style="font-size:0.8em;margin:0">${lib.translate(lib.itlcodes.STATS_TRAVELING_LANDED)}</i>
                <table style="margin-top:0.5em;">
                    <tr><td><b id="stats-nukes-this-week">-</b></td><td>${lib.translate(lib.itlcodes.NUKES)}</td>
                    <tr><td><b id="stats-fangs-this-week">-</b></td><td>${lib.translate(lib.itlcodes.FANGS)}</td>
                    <tr><td><b id="stats-fakes-this-week">-</b></td><td>${lib.translate(lib.itlcodes.FAKES)}</td>
                </table>
            </div>

            <h3 style="margin-top:2em">${lib.translate(lib.itlcodes.SUPPORT)}</h3>
            <div style="display:inline-block">
                <table class="vis">
                    <tr>
                        <th>${lib.translate(lib.itlcodes.DEFENSE)}</th>
                        <th>${lib.translate(lib.itlcodes.STATS_NUM_DVS)}</th>
                    </tr>
                    <tr>
                        <td>${lib.translate(lib.itlcodes.STATS_DVS_AT_HOME)}</td>
                        <td id="stats-dvs-at-home">-</td>
                    </tr>
                    <tr>
                        <td>${lib.translate(lib.itlcodes.STATS_BACKLINE_DVS_AT_HOME)}</td>
                        <td id="stats-dvs-at-home-backline">-</td>
                    </tr>
                    <tr>
                        <td>${lib.translate(lib.itlcodes.STATS_DVS_TRAVELING)}</td>
                        <td id="stats-dvs-traveling">-</td>
                    </tr>
                </table>
            </div>
        `
    };
}

function makeHighScoresTab() {
    var rankings = [
        { label: `# ${lib.translate(lib.itlcodes.FAKES)}`, property: 'fakesInPastWeek' },
        { label: `# ${lib.translate(lib.itlcodes.FANGS)}`, property: 'fangsInPastWeek' },
        { label: `# ${lib.translate(lib.itlcodes.NUKES)}`, property: 'nukesInPastWeek' }
    ];

    return {
        label: lib.translate(lib.itlcodes.TAB_HIGH_SCORES),
        containerId: 'vault-stats-high-scores-container',

        init: function ($container) {
            lib.getApi('player/high-scores')
                .done((data) => {
                    console.log('Got high scores: ', data);

                    let rankingTabs = [];

                    rankings.forEach((ranking) => {
                        $container.find('#high-scores-overview-container').append(
                            makeTopRankedList(data, ranking.label, ranking.property, ranking.suffix).trim()
                        );

                        rankingTabs.push({
                            label: ranking.label,
                            containerId: `vault-stats-high-scores-rankings-${ranking.property.toLowerCase()}`,
                            init: () => { },
                            getContent: () => makeRankingTab(data, ranking.label, ranking.property, ranking.suffix)
                        });
                    });

                    let defaultTab = rankingTabs[0].containerId;
                    $container.find('#high-scores-rankings-container').append(uilib.mkTabbedContainer(defaultTab, rankingTabs));
                    $container.find('#high-scores-rankings-container > .vault-tab-container').css({
                        'max-height': '30em',
                        'overflow-y': 'scroll'
                    });
                })
                .error(() => {
                    alert(lib.translate(lib.itlcodes.RANKINGS_LOAD_ERROR, { _escaped: false }));
                });
        },

        //  Top of the tab has overview of top 3 players for a few different categories
        //  Full listings are available below that overview in different tabs
        getContent: `
            <h3 style="margin-bottom:0">${lib.translate(lib.itlcodes.TAB_HIGH_SCORES)}</h3>
            <em style="font-size:0.75em;margin-bottom:1em">${lib.translate(lib.itlcodes.STATS_7_DAYS)}</em>

            <div id="high-scores-overview-container"></div>

            <h3 style="margin-top:1.5em">${lib.translate(lib.itlcodes.RANKINGS)}</h3>
            <div id="high-scores-rankings-container"></div>
        `
    };
}

function rowClass(idx) {
    return idx % 0 ? 'row_b' : 'row_a';
}

function makeTopRankedList(scoreData, label, property, suffix) {
    var mapSort =
        lib.objectToArray(scoreData, (prop, value) => ({ name: prop, value: value[property] }))
            .sort((a, b) => b.value - a.value);

    var limit = 5;
    var result = [];
    for (var i = 0; i < limit && i < mapSort.length; i++)
        result.push(mapSort[i]);

    return `
        <div style="display:inline-block; margin: 0.5em">
            <h4 style="display:inline-block">${label}</h4>
            <table>
                ${result.map((entry, i) => `
                    <tr class="${rowClass(i)}">
                        <td style="padding:0.25em 0.5em">${entry.name}</td>
                        <td style="padding:0.25em 0.5em">${entry.value} ${suffix || ''}</td>
                    </tr>
                `).join('\n')}
            </table>
        </div>
    `;
}

function makeRankingTab(scoreData, label, property, suffix) {
    return `
        <table class="vis" style="width:100%">
            <tr>
                <th>${lib.translate(lib.itlcodes.PLAYER)}</th>
                <th>${label}</th>
            </tr>
            ${ lib.objectToArray(scoreData, (prop, value) => ({ name: prop, value: value[property] }))
                .sort((a, b) => b.value - a.value)
                .map((entry, i) => `<tr class="${rowClass(i)}"><td>${entry.name}</td><td>${entry.value || 0} ${suffix || ''}</td></tr>`)
                .join('\n') }
        </table>
    `;
}