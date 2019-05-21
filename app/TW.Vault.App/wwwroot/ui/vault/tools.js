
function makeToolsTab() {
    let fakeTab = makeFakeScriptTab();
    let backtimeTab = makeBacktimeListTab();

    let tabs = [
        fakeTab,
        backtimeTab
    ];

    let toolsTab = {
        label: lib.translate(lib.itlcodes.TAB_TOOLS),
        containerId: 'vault-tools-container',

        getContent: function () {
            return uilib.mkTabbedContainer(fakeTab, tabs);
        }
    };

    return toolsTab;
}



function makeFakeScriptTab() {
    return {
        label: lib.translate(lib.itlcodes.TAB_FAKE_SCRIPT),
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

                        alert(lib.messages.GENERIC_ERROR);
                    });
            });
        },

        getContent: `
            <h3>${lib.translate(lib.itlcodes.DYNAMIC_FAKE_SCRIPTS)}</h3>
            <table style="width:100%">
                <tr>
                    <td>
                        <label for="fake-target-player">${lib.translate(lib.itlcodes.PLAYERS)}</label>
                    </td>
                    <td>
                        <input id="fake-target-player" type="text" placeholder="False Duke, Nefarious, etc.">
                    </td>
                </tr>
                <tr>
                    <td>
                        <label for="fake-target-tribe">${lib.translate(lib.itlcodes.TRIBES)}</label>
                    </td>
                    <td>
                        <input id="fake-target-tribe" type="text" placeholder="Hundred Hungry Hippos, 100, ODZ, etc.">
                    </td>
                </tr>
                <tr>
                    <td>
                        <label for="fake-target-continents">${lib.translate(lib.itlcodes.CONTINENTS)}</label>
                    </td>
                    <td>
                        <input id="fake-target-continents" type="text" placeholder="44,45,etc.">
                    </td>
                </tr>
                <tr>
                    <td>
                        <label>${lib.translate(lib.itlcodes.FAKES_MIN_COORD)}</label>
                    </td>
                    <td>
                        <input id="fake-min-x" placeholder="X">|<input id="fake-min-y" placeholder="Y">
                    </td>
                </tr>
                <tr>
                    <td>
                        <label>${lib.translate(lib.itlcodes.FAKES_MAX_COORD)}</label>
                    </td>
                    <td>
                        <input id="fake-max-x" placeholder="X">|<input id="fake-max-y" placeholder="Y">
                    </td>
                </tr>
                <tr>
                    <td>
                        <label>${lib.translate(lib.itlcodes.FAKES_DIST_LABEL)}</label>
                    </td>
                    <td>
                        <input id="fake-dist-max"> ${lib.translate(lib.itlcodes.FAKES_DIST_FIELDS_FROM)} <input id="fake-dist-x" placeholder="X">|<input id="fake-dist-y" placeholder="Y">
                    </td>
                </tr>
            </table>

            <div style="text-align:center;margin: 1em 0;">
                <button id="fake-make-script">${lib.translate(lib.itlcodes.GET_SCRIPT)}</button>
                <button id="fake-get-coords">${lib.translate(lib.itlcodes.GET_COORDS)}</button>
            </div>

            <div>
                <textarea id="fake-script-output" style="width:100%;height:5em"></textarea>
            </div>
        `
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
        onlyFirstPermutation: false,
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

        let builder = new BBTableBuilder(lib.translate(lib.itlcodes.BB_TABLE));
        builder.setColumnNames(
            lib.translate(lib.itlcodes.TARGET_VILLAGE),
            lib.translate(lib.itlcodes.SOURCE_VILLAGE),
            lib.translate(lib.itlcodes.LAUNCH_TIME),
            lib.translate(lib.itlcodes.LANDING_TIME),
            lib.translate(lib.itlcodes.TROOP_REQUIRED)
        );

        let allCommands = [];
        selectedCommands.forEach((com) => {
            let filteredInstructions = com.instructions
                .filter((i) => i.travelTimeSeconds < maxTravelSeconds)
                .filter((i) => !settings.ignoredTroopTypes.contains(i.troopType))
                .filter((i) => i.commandAttackPower >= nukeAttackPower * settings.minAttackingNukeSizePercent / 100);

            allCommands.push(...filteredInstructions);
        });

        allCommands.sort((a, b) => a.launchAt.valueOf() - b.launchAt.valueOf());

        if (settings.onlyFirstPermutation) {
            let mappedTimings = {};
            let unfilteredCommands = allCommands;
            allCommands = [];
        }

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

        $container.find('#backtime-output-container p').text(lib.translate(lib.itlcodes.BACKTIME_RESULTS, { numTimings: total, numNukes: numNukes, numShown: displayedCommands.length }));

        displayedCommands.forEach((i) => {
            builder.addRow(
                `[coord]${i.targetVillageX}|${i.targetVillageY}[/coord]`,
                `[${lib.translate(lib.itlcodes.BB_URL)}=${lib.makeTwUrl(`village=${i.sourceVillageId}&screen=place&from=simulator&att_${i.troopType}=1&target_village_id=${i.targetVillageId}`)}]${i.sourceVillageName} (${i.sourceVillageX}|${i.sourceVillageY})[/${lib.translate(lib.itlcodes.BB_URL)}]`,
                lib.formatDateTime(i.launchAt),
                lib.formatDateTime(i.landsAt),
                `[${lib.translate(lib.itlcodes.BB_UNIT)}]${i.troopType}[/${lib.translate(lib.itlcodes.BB_UNIT)}]`
            );
        });

        let bbcode = builder.toString();
        $container.find('#backtime-output-container textarea').val(bbcode);
    }

    return {
        label: lib.translate(lib.itlcodes.TAB_FIND_BACKTIMES),
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
                $el.text(lib.translate(lib.itlcodes.BACKTIME_WORKING));
                lib.getApi('plan/backtime-list')
                    .done((data) => {
                        console.log('Got plan data:', data);
                        $el.text(lib.translate(lib.itlcodes.SEARCH));

                        currentCommands = data;
                        updateBacktimingList($container);
                    })
                    .error(() => {
                        alert(lib.messages.GENERIC_ERROR);
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
            uilib.syncProp('#backtime-list-settings-first-permutation', settings, 'onlyFirstPermutation', updateSettings);
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
                <h3>${lib.translate(lib.itlcodes.TAB_FIND_BACKTIMES)}</h3>
                <p>
                    ${lib.translate(lib.itlcodes.BACKTIME_DESCRIPTION_1)}
                </p>
                <p>
                    <b>${lib.translate(lib.itlcodes.BACKTIME_DESCRIPTION_2)}</b>
                </p>
                <p id="backtime-make-list">
                    ${uilib.mkBtn(null, lib.translate(lib.itlcodes.SEARCH))}
                <p>
                <div id="backtime-output-container" style="display:none;text-align:left">
                    <h4>${lib.translate(lib.itlcodes.OPTIONS)}</h4>
                    <table style="text-align:left">
                        <tr>
                            <td>
                                <label for="backtime-list-settings-min-return-pop">
                                    ${lib.translate(lib.itlcodes.BACKTIME_MIN_RETURNING_POPULATION)}
                                </label>
                            </td>
                            <td>
                                <input id="backtime-list-settings-min-return-pop" type="text">
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <label for="backtime-list-settings-min-attack-nuke-size">
                                    ${lib.translate(lib.itlcodes.BACKTIME_MIN_ATTACK_SIZE_1)}
                                </label>
                            </td>
                            <td>
                                <input id="backtime-list-settings-min-attack-nuke-size" type="text">${lib.translate(lib.itlcodes.BACKTIME_MIN_ATTACK_SIZE_2)}
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <label for="backtime-list-settings-max-travel-hours">
                                    ${lib.translate(lib.itlcodes.BACKTIME_MAX_TRAVEL_TIME)}
                                </label>
                            </td>
                            <td>
                                <input id="backtime-list-settings-max-travel-hours" type="text"> ${lib.translate(lib.itlcodes.BACKTIME_HOURS)}
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <label for="backtime-list-settings-max-instructions">
                                    ${lib.translate(lib.itlcodes.BACKTIME_MAX_NUM_TIMINGS)}
                                </label>
                            </td>
                            <td>
                                <input id="backtime-list-settings-max-instructions" type="text">
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <label for="backtime-list-settings-hide-existing-backtimes">${lib.translate(lib.itlcodes.BACKTIME_HIDE_HANDLED_NUKES)}</label>
                            </td>
                            <td>
                                <input id="backtime-list-settings-hide-existing-backtimes" type="checkbox">
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <label for="backtime-list-settings-hide-stacked-backtimes">${lib.translate(lib.itlcodes.BACKTIME_HIDE_STACKED_NUKES)}</label>
                            </td>
                            <td>
                                <input id="backtime-list-settings-hide-stacked-backtimes" type="checkbox">
                            </td>
                        </tr>
                        <!--
                        <tr>
                            <td>
                                <label for="backtime-list-settings-first-permutation">Only first timing:</label>
                            </td>
                            <td>
                                <input id="backtime-list-settings-first-permutation" type="checkbox"> (Don't list multiple timings for the same village, ie ram, axe, etc)
                            </td>
                        </tr>
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


