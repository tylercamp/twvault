function makeStatsTab() {
    let userStatsTab = makeUserStatsTab();
    let highScoresTab = makeHighScoresTab();

    let tabs = [
        userStatsTab,
        highScoresTab
    ];

    return {
        label: 'Stats',
        containerId: 'vault-stats-container',

        init: function ($container) {

        },

        getContent: function () {
            return uilib.mkTabbedContainer(userStatsTab.containerId, tabs);
        }
    };
}

function makeUserStatsTab() {
    return {
        label: 'Me',
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
                        alert('An error occurred while loading stats');
                });
        },

        getContent: () => `
            <h3 style="margin-bottom:0">Last 7 Days</h3>
            <div style="display:inline-block">
                <i style="font-size:0.8em;margin:0">(Traveling and landed)</i>
                <table style="margin-top:0.5em;">
                    <tr><td><b id="stats-nukes-this-week">-</b></td><td>Nukes</td>
                    <tr><td><b id="stats-fangs-this-week">-</b></td><td>Fangs</td>
                    <tr><td><b id="stats-fakes-this-week">-</b></td><td>Fakes</td>
                </table>
            </div>

            <h3 style="margin-top:2em">Support</h3>
            <div style="display:inline-block">
                <table class="vis">
                    <tr>
                        <th>Defense</th>
                        <th># DVs</th>
                    </tr>
                    <tr>
                        <td>At Home</td>
                        <td id="stats-dvs-at-home">-</td>
                    </tr>
                    <tr>
                        <td>(Backline) At Home</td>
                        <td id="stats-dvs-at-home-backline">-</td>
                    </tr>
                    <tr>
                        <td>Traveling</td>
                        <td id="stats-dvs-traveling">-</td>
                    </tr>
                </table>
            </div>
        `
    };
}

function makeHighScoresTab() {
    var rankings = [
        { label: '# Fakes', property: 'numFakes' },
        { label: '# Fangs', property: 'numFangs' },
        { label: '# Nukes', property: 'numNukes' }
    ];

    return {
        label: 'High Scores',
        containerId: 'vault-stats-high-scores-container',

        init: function ($container) {
            lib.getApi('player/high-scores')
                .done((data) => {
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
                })
                .error(() => {
                    alert('An error occurred while getting rankings');
                });
        },

        //  Top of the tab has overview of top 3 players for a few different categories
        //  Full listings are available below that overview in different tabs
        getContent: () => `
            <h3>High Scores</h3>
            <div id="high-scores-overview-container"></div>

            <h3>Rankings</h3>
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
            .sort((a, b) => a.value - b.value);

    return `
        <div style="inline-block">
            <h4>${label}</h4>
            <table>
                ${mapSort.map((entry, i) => `
                    <tr class="${rowClass(i)}">
                        <td>${entry.name}</td>
                        <td>${entry.value} ${suffix}</td>
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
                <th>Player</th>
                <th>${label}</th>
            </tr>
            ${ lib.objectToArray(scoreData, (prop, value) => ({ name: prop, value: value[property] }))
                .sort((a, b) => a.value - b.value)
                .map((entry, i) => `<tr class="${rowClass(i)}"><td>${entry.name}</td><td>${entry.value || 0} ${suffix || ''}</td></tr>`)
                .join('\n') }
        </table>
    `;
}