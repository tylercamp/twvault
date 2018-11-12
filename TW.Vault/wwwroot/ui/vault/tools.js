
function makeToolsTab() {
    let fakeTab = makeFakeScriptTab();
    let backtimeTab = makeBacktimeListTab();

    let tabs = [
        fakeTab,
        backtimeTab
    ];

    let toolsTab = {
        label: 'Tools',
        containerId: 'vault-tools-container',

        getContent: function () {
            return uilib.mkTabbedContainer(fakeTab.containerId, tabs);
        }
    };

    return toolsTab;
}



function makeFakeScriptTab() {
    return {
        label: 'Fake Script',
        containerId: 'vault-fake-script-tools',
        init: function ($container) {
            $container.find('td').css({
                'text-align': 'left'
            });

            $container.find('tr td:nth-of-type(1)').css({
                width: '6em'
            });

            $container.find('input[type=text]').css({
                width: '100%'
            })

            $container.find('input[id^=fake-min], input[id^=fake-max], input[id^=fake-dist]').css({
                width: '2em',
                'text-align': 'center'
            });

            function makeCoordsQueryString() {
                let players = $container.find('#fake-target-player').val().trim();
                let tribes = $container.find('#fake-target-tribe').val().trim();
                let continents = $container.find('#fake-target-continents').val().trim();
                let minCoord = {
                    x: $container.find('#fake-min-x').val().trim(),
                    y: $container.find('#fake-min-y').val().trim()
                };
                let maxCoord = {
                    x: $container.find('#fake-max-x').val().trim(),
                    y: $container.find('#fake-max-y').val().trim()
                };
                let maxDist = {
                    x: $container.find('#fake-dist-x').val().trim(),
                    y: $container.find('#fake-dist-y').val().trim(),
                    dist: $container.find('#fake-dist-max').val().trim()
                };

                if (minCoord.x) minCoord.x = parseInt(minCoord.x);
                if (minCoord.y) minCoord.y = parseInt(minCoord.y);
                if (maxCoord.x) maxCoord.x = parseInt(maxCoord.x);
                if (maxCoord.y) maxCoord.y = parseInt(maxCoord.y);
                if (maxDist.x) maxDist.x = parseInt(maxDist.x);
                if (maxDist.y) maxDist.y = parseInt(maxDist.y);
                if (maxDist.dist) maxDist.dist = parseFloat(maxDist.dist);

                let query = [];
                query.push(`player=${encodeURIComponent(players)}`);
                query.push(`&tribe=${encodeURIComponent(tribes)}`);
                query.push(`&k=${encodeURIComponent(continents)}`);

                if (minCoord.x || minCoord.y) {
                    if (!minCoord.x) minCoord.x = 0;
                    if (!minCoord.y) minCoord.y = 0;
                    query.push(`&min=${encodeURIComponent(minCoord.x + '|' + minCoord.y)}`);
                }

                if (maxCoord.x || maxCoord.y) {
                    if (!maxCoord.x) maxCoord.x = 1000;
                    if (!maxCoord.y) maxCoord.y = 1000;
                    query.push(`&max=${encodeURIComponent(maxCoord.x + '|' + maxCoord.y)}`);
                }

                if (maxDist.x && maxDist.y && maxDist.dist) {
                    query.push(`&center=${encodeURIComponent(maxDist.x + '|' + maxDist.y + '|' + maxDist.dist)}`);
                }

                return query.join('&');
            }

            $container.find('#fake-make-script').click(() => {

                let link = lib.makeVaultUrl(`script/fake.js?server=${window.location.hostname}&${makeCoordsQueryString()}`);

                let script = `
javascript:
window.vaultFakes = {
    troopCounts: [
        { catapult: 1, spy: 1 },
        { ram: 1, spy: 1 },
        { catapult: 1 },
        { ram: 1 }
    ]
};

$.getScript("${link}");
                `.trim(' ', '\n', '\r');

                $container.find('#fake-script-output').val(script);
            });

            $container.find('#fake-get-coords').click(() => {
                let link = 'village/coords?' + makeCoordsQueryString();

                lib.getApi(link)
                    .done((data) => {
                        let coords = data.coords;

                        $container.find('#fake-script-output').val(coords);
                    })
                    .error(() => {
                        if (lib.isUnloading())
                            return;

                        alert('An error occurred...');
                    });
            });
        },

        getContent: function () {
            return `
                <h3>Dynamic Fake Scripts</h3>
                <table style="width:100%">
                    <tr>
                        <td>
                            <label for="fake-target-player">Players</label>
                        </td>
                        <td>
                            <input id="fake-target-player" type="text" placeholder="False Duke, Nefarious, etc.">
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <label for="fake-target-tribe">Tribes</label>
                        </td>
                        <td>
                            <input id="fake-target-tribe" type="text" placeholder="Hundred Hungry Hippos, 100, ODZ, etc.">
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <label for="fake-target-continents">Continents</label>
                        </td>
                        <td>
                            <input id="fake-target-continents" type="text" placeholder="44,45,etc.">
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <label>Min Coord</label>
                        </td>
                        <td>
                            <input id="fake-min-x" placeholder="X">|<input id="fake-min-y" placeholder="Y">
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <label>Max Coord</label>
                        </td>
                        <td>
                            <input id="fake-max-x" placeholder="X">|<input id="fake-max-y" placeholder="Y">
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <label>Dist From Center</label>
                        </td>
                        <td>
                            <input id="fake-dist-max"> fields from <input id="fake-dist-x" placeholder="X">|<input id="fake-dist-y" placeholder="Y">
                        </td>
                    </tr>
                </table>

                <div style="text-align:center;margin: 1em 0;">
                    <button id="fake-make-script">Get Script</button>
                    <button id="fake-get-coords">Get Coords</button>
                </div>

                <div>
                    <textarea id="fake-script-output" style="width:100%;height:5em"></textarea>
                </div>
            `;
        }
    }
}


function makeBacktimeListTab() {

    const nukeAttackPower = 500000;
    var currentCommands = [];

    let settings = lib.getLocalStorage('backtime-list-settings', {
        minReturningPop: 13000,
        minAttackingNukeSizePercent: 50,
        maxTravelHours: 24,
        maxInstructionsCount: 100,
        hideBacktimedNukes: true,
        ignoreStacked: true,
        ignoredTroopTypes: []
    });

    function saveSettings() {
        lib.setLocalStorage('backtime-list-settings', settings);
    }

    function updateBacktimingList($container) {

        //  Filter commands by settings
        const maxTravelSeconds = settings.maxTravelHours * 60 * 60;
        let selectedCommands = currentCommands
            // Meets minimum returning nuke requirements
            .filter((cmd) => cmd.travelingArmyPopulation >= settings.minReturningPop)
            // Has any timings meeting max travel time requirements
            .filter((cmd) => !!cmd.instructions.find((i) => i.travelTimeSeconds < maxTravelSeconds))
            // Has any timings that aren't in the list of ignored troop types for travel times
            .filter((cmd) => !!cmd.instructions.find((i) => !settings.ignoredTroopTypes.contains(i.troopType)))
            // Has any timings that meet the minimum backtiming attack power
            .filter((cmd) => !!cmd.instructions.find((i) => i.commandAttackPower >= nukeAttackPower * settings.minAttackingNukeSizePercent / 100))
            // Has existing backtimes
            .filter((cmd) => !settings.hideBacktimedNukes || cmd.existingBacktimes == 0)
            // Is not stacked
            .filter((cmd) => !settings.ignoreStacked || !cmd.isStacked)
        ;

        let builder = new BBTableBuilder();
        builder.setColumnNames("Source Village", "Launch Time", "Landing Time", "Troop Req.");

        let allCommands = [];
        selectedCommands.forEach((com) => {
            let filteredInstructions = com.instructions
                .filter((i) => i.travelTimeSeconds < maxTravelSeconds)
                .filter((i) => !settings.ignoredTroopTypes.contains(i.troopType))
                .filter((i) => i.commandAttackPower >= nukeAttackPower * settings.minAttackingNukeSizePercent / 100);

            allCommands.push(...filteredInstructions);
        });

        allCommands.sort((a, b) => a.launchAt.valueOf() - b.launchAt.valueOf());

        console.log('Made full plan list: ', allCommands);

        $container.find('#backtime-output-container').css({
            display: 'block'
        });

        let total = allCommands.length;
        let max = settings.maxInstructionsCount;
        let effective = Math.min(total, max);

        let displayedCommands = allCommands.slice(0, effective);
        console.log('Made filtered plan list: ', displayedCommands);

        let numNukes = allCommands.distinctBy(c => c.targetVillageId + '_' + c.landsAt.valueOf()).length;

        $container.find('#backtime-output-container p').text(`Found ${total} timings for ${numNukes} returning nukes (${displayedCommands.length} shown)`);

        displayedCommands.forEach((i) => {
            builder.addRow(
                `[url=${lib.makeTwUrl(`village=${i.sourceVillageId}&screen=place&from=simulator&att_${i.troopType}=1&target_village_id=${i.targetVillageId}`)}]${i.sourceVillageName} (${i.sourceVillageX}|${i.sourceVillageY})[/url]`,
                lib.formatDateTime(i.launchAt),
                lib.formatDateTime(i.landsAt),
                `[unit]${i.troopType}[/unit]`
            );
        });

        let bbcode = builder.toString();
        $container.find('#backtime-output-container textarea').val(bbcode);
    }

    return {
        label: 'Find Backtimes',
        containerId: 'vault-backtime-search',
        init: function ($container) {
            $container.find('td').css({
                'text-align': 'left'
            });

            $container.find('tr td:nth-of-type(2)').css({
                'padding-left': '1em'
            });

            $container.find('input[type=text]').css({
                'text-align': 'center',
                'width': '30px'
            })

            $container.find('#backtime-make-list button').click((ev) => {
                let $el = $(ev.originalEvent.target);
                $el.text('Working... (This may take a while)');
                lib.getApi('plan/backtime-list')
                    .done((data) => {
                        console.log('Got plan data:', data);
                        $el.text('Search');

                        currentCommands = data;
                        updateBacktimingList($container);
                    })
                    .error(() => {
                        alert('An error occurred...');
                    });
            });

            function updateSettings() {
                saveSettings();
                updateBacktimingList($container);
            }

            uilib.syncProp('#backtime-list-settings-min-return-pop', settings, 'minReturningPop', updateSettings);
            uilib.syncProp('#backtime-list-settings-min-attack-nuke-size', settings, 'minAttackingNukeSizePercent', updateSettings);
            uilib.syncProp('#backtime-list-settings-max-travel-hours', settings, 'maxTravelHours', updateSettings);
            uilib.syncProp('#backtime-list-settings-max-instructions', settings, 'maxInstructionsCount', updateSettings);
            uilib.syncProp('#backtime-list-settings-hide-existing-backtimes', settings, 'hideBacktimedNukes', updateSettings);
            uilib.syncProp('#backtime-list-settings-hide-stacked-backtimes', settings, 'ignoreStacked', updateSettings);
        },

        getContent: function () {

            function makeIgnoreTroopOption(troopType) {
                let name = troopType.canonicalName;
                let check = settings.ignoredTroopTypes.contains(name) ? 'checked' : '';

                return `
                    <div style="display:inline-block">
                        <input type="checkbox" id="backtime-list-settings-ignore-${name}" ${check}>
                        <label for="backtime-list-settings-ignore-${name}" style="margin-right:8px">
                            <img src="${troopType.icon}" style="max-width:16px">
                        </label>
                    </div>
                `;
            }

            return `
                <h3>Find Backtimes</h3>
                <p>
                    Get plans for all available backtimes that you can make for enemy nukes using the troops you've uploaded to the vault.
                </p>
                <p>
                    <b>Upload your troops frequently to get the most accurate timings!</b>
                </p>
                <p id="backtime-make-list">
                    ${uilib.mkBtn(null, 'Search')}
                <p>
                <div id="backtime-output-container" style="display:none;text-align:left">
                    <h4>Options</h4>
                    <table style="text-align:left">
                        <tr>
                            <td>
                                <label for="backtime-list-settings-min-return-pop">
                                    Minimum returning population: 
                                </label>
                            </td>
                            <td>
                                <input id="backtime-list-settings-min-return-pop" type="text">
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <label for="backtime-list-settings-min-attack-nuke-size">
                                    Minimum attack size:
                                </label>
                            </td>
                            <td>
                                <input id="backtime-list-settings-min-attack-nuke-size" type="text">% of a full nuke
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <label for="backtime-list-settings-max-travel-hours">
                                    Max travel time:
                                </label>
                            </td>
                            <td>
                                <input id="backtime-list-settings-max-travel-hours" type="text"> hours
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <label for="backtime-list-settings-max-instructions">
                                    Max number of timings:
                                </label>
                            </td>
                            <td>
                                <input id="backtime-list-settings-max-instructions" type="text">
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <label for="backtime-list-settings-hide-existing-backtimes">Hide backtimed nukes:</label>
                            </td>
                            <td>
                                <input id="backtime-list-settings-hide-existing-backtimes" type="checkbox">
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <label for="backtime-list-settings-hide-stacked-backtimes">Hide stacked nukes:</label>
                            </td>
                            <td>
                                <input id="backtime-list-settings-hide-stacked-backtimes" type="checkbox">
                            </td>
                        </tr>
                        <!--
                        <tr>
                            <td>
                                <label>
                                    Ignore troop types:
                                </label>
                            </td>
                            <td>
                                ${ lib.twstats.unitTypes.map((t) => makeIgnoreTroopOption(t)).join('\n') }
                            </td>
                        </tr>
                        -->
                    </table>
                    <p></p>
                    <textarea style="width:100%;height:10em"></textarea>
                </div>
            `;
        }
    };
}


