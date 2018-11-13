function makeStatsTab() {
    return {
        label: 'Stats',
        containerId: 'vault-stats-container',

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
                        padding: '0 0.25em'
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
            <div style="width:50%; display:inline-block">
                <i style="font-size:0.8em;margin:0">(Traveling and landed)</i>
                <table style="margin-top:0.5em;width:100%">
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