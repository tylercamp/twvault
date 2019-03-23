
function makeActionsTab() {
    let suggestionsTab = makeSuggestedActionsTab();
    let requestLocalSupportTab = makeRequestLocalSupportTab();
    let requestSnipeTab = makeRequestSnipeTab();
    let requestStackTab = makeRequestStackTab();

    let tabs = [
        suggestionsTab,
        //requestSnipeTab,
        requestLocalSupportTab,
        requestStackTab
    ];

    return {
        label: lib.translate(lib.itlcodes.TAB_ACTIONS_ALERTS),
        containerId: 'vault-actions-container',

        init: function () {
            let self = this;
            suggestionsTab.onLoaded = (data, count) => {
                self.$tabButton.html(self.label + ' (' + count + ')');
            };
        },

        getContent: function () {
            return uilib.mkTabbedContainer(suggestionsTab, tabs);
        }
    };
}

function makeSuggestedActionsTab() {

    let noDataRow = (numCols, msg) => `<tr><td colspan="${numCols}">${msg}</td></tr>`;
    let makeVillageLink = (name, id, x, y) => `<a href="${lib.makeTwUrl('screen=info_village&id=' + id)}">${name} (${x}|${y})</a>`;
    let makePlayerLink = (name, id) => `<a href="${lib.makeTwUrl('screen=info_player&id=' + id)}">${name}</a>`;
    let makeTribeLink = (name, id) => `<a href="${lib.makeTwUrl('screen=info_ally&id=' + id)}">${name}</a>`

    let recapsTab = makeSuggestedRecapsTab();
    let snipesTab = makeSuggestedSnipeTab();
    let stackTab = makeSuggestedStackingTab();
    let nobleTab = makeSuggestedNobleTargetsTab();
    let uselessStacksTab = makeSuggestedUselessStacksTab();

    let tabs = [
        recapsTab,
        stackTab,
        uselessStacksTab,
        snipesTab,
        nobleTab
    ];

    return {
        label: lib.translate(lib.itlcodes.TAB_ALERTS),
        containerId: 'vault-suggested-actions-container',

        init: function ($container) {
            let self = this;
            lib.getApi('alert/suggestions')
                .done((data) => {
                    console.log('Got suggestion data: ', data);

                    let numAlerts =
                        data.recaps.filter(_ => _.isNearby).length +
                        data.snipes.length +
                        data.uselessStacks.length +
                        data.nobleTargets.length +
                        data.stacks.length;

                    self.$tabButton.text(self.$tabButton.text() + ' (' + numAlerts + ')');

                    self.onLoaded && self.onLoaded(data, numAlerts);

                    recapsTab._init(recapsTab.$container, data.recaps);
                    snipesTab._init(snipesTab.$container, data.snipes);
                    stackTab._init(stackTab.$container, data.stacks);
                    nobleTab._init(nobleTab.$container, data.nobleTargets);
                    uselessStacksTab._init(uselessStacksTab.$container, data.uselessStacks);

                    $container.find('table').css({
                        width: '100%'
                    });

                    $container.find('td').css({
                        'font-size': '0.9em',
                        padding: '0 0.25em'
                    });
                });
        },

        getContent: function () {
            return uilib.mkTabbedContainer(recapsTab, tabs);
        }
    };

    function makeSuggestedRecapsTab() {
        var settings = lib.getLocalStorage('suggested-recaps-settings', {
            onlyShowNearby: true
        });

        let saveSettings = () => lib.setLocalStorage('suggested-recap-settings', settings);

        return {
            label: lib.translate(lib.itlcodes.TAB_SEND_RECAP),
            containerId: 'vault-suggested-recaps-container',

            _init: function ($container, data) {
                if (data.length)
                    this.$tabButton.html(this.label + ' (' + data.filter(_ => _.isNearby).length + ')');

                let self = this;
                uilib.syncProp('#vault-suggested-recaps-only-nearby', settings, 'onlyShowNearby', () => {
                    saveSettings();
                    self._updateContent($container, data);
                });
                this._updateContent($container, data);
            },

            _updateContent: function ($container, data) {
                let $table = $container.find('table');
                $table.find('tr:not(:first-of-type)').remove();

                if (settings.onlyShowNearby)
                    data = data.filter((d) => d.isNearby);

                if (!data.length) {
                    $table.append(noDataRow(5, lib.translate(lib.itlcodes.ACTIONS_RECAPS_NONE)));
                    return;
                }

                data.forEach((recap, i) => {
                    let rowClass = i % 2 ? 'row_b' : 'row_a';

                    $table.append(`
                        <tr class="${rowClass}">
                            <td>${makeVillageLink(recap.villageName, recap.villageId, recap.x, recap.y)}</td>
                            <td>${recap.loyalty}</td>
                            <td>${lib.translate(lib.itlcodes.ACTIONS_RECAPS_AGE, { duration: lib.formatDuration(recap.occurredAt) })}</td>
                            <td>${makePlayerLink(recap.oldOwnerName, recap.oldOwnerId)}</td>
                            <td>${makePlayerLink(recap.newOwnerName, recap.newOwnerId)}</td>
                        </tr>
                    `.trim());
                });
            },
            
            getContent: `
                <p>
                    ${lib.translate(lib.itlcodes.ACTIONS_RECAPS_DESCRIPTION)}
                </p>
                <p>
                    <input type="checkbox" id="vault-suggested-recaps-only-nearby">
                    <label for="vault-suggested-recaps-only-nearby">${lib.translate(lib.itlcodes.ACTIONS_RECAPS_ONLY_NEARBY)}</label>
                </p>
                <table class="vis">
                    <tr>
                        <th>${lib.translate(lib.itlcodes.VILLAGE)}</th>
                        <th>${lib.translate(lib.itlcodes.LOYALTY)}</th>
                        <th>${lib.translate(lib.itlcodes.ACTIONS_RECAPS_CAPTURED_AT)}</th>
                        <th>${lib.translate(lib.itlcodes.ACTIONS_RECAPS_OLD_OWNER)}</th>
                        <th>${lib.translate(lib.itlcodes.ACTIONS_RECAPS_NEW_OWNER)}</th>
                    </tr>
                </table>
            `
        };
    }

    function makeSuggestedSnipeTab() {
        return {
            label: lib.translate(lib.itlcodes.TAB_SNIPES_NEEDED),
            containerId: 'vault-suggested-snipes-container',

            _init: function ($container, data) {
                if (data.length)
                    this.$tabButton.html(this.label + ' (' + data.length + ')');

                let $table = $container.find('table');

                if (!data.length) {
                    $table.append(noDataRow(3, lib.translate(lib.itlcodes.ACTIONS_SNIPES_NONE)));
                    return;
                }

                data.forEach((train, i) => {
                    let rowClass = i % 2 ? 'row_b' : 'row_a';
                    $table.append(`
                        <tr class="${rowClass}">
                            <td>${makeVillageLink(train.targetVillage.name, train.targetVillage.id, train.targetVillage.x, train.targetVillage.y)}</td>
                            <td>${train.train.length}</td>
                            <td>${lib.formatDateTime(train.landsAt)}</td>
                        </tr>
                    `.trim());
                });
            },

            getContent: `
                <p>
                    ${lib.translate(lib.itlcodes.ACTIONS_SNIPES_DESCRIPTION)}
                </p>
                <table class="vis">
                    <tr>
                        <th>${lib.translate(lib.itlcodes.VILLAGE)}</th>
                        <th>${lib.translate(lib.itlcodes.ACTIONS_SNIPES_NUM_NOBLES)}</th>
                        <th>${lib.translate(lib.itlcodes.LANDS_AT)}</th>
                    </tr>
                </table>
            `
        };
    }

    function makeSuggestedStackingTab() {
        return {
            containerId: 'vault-suggested-stacks-container',
            label: lib.translate(lib.itlcodes.TAB_SEND_STACKS),

            _init: function ($container, data) {
                if (data.length)
                    this.$tabButton.html(this.label + ' (' + data.length + ')');

                let $table = $container.find('table');

                if (!data.length) {
                    $table.append(noDataRow(3, lib.translate(lib.itlcodes.ACTIONS_STACKS_NONE)));
                    return;
                }

                data.forEach((village, i) => {
                    let rowClass = i % 2 ? 'row_b' : 'row_a';
                    
                    $table.append(`
                        <tr class="${rowClass}">
                            <td>${makeVillageLink(village.villageName, village.villageId, village.x, village.y)}</td>
                            <td>${village.sentNukes}</td>
                            <td>${lib.translate(lib.itlcodes.ACTIONS_STACKS_EATABLE_NUKES, { numNukes: village.eatableNukes })}</td>
                        </tr>
                    `.trim());
                });
            },

            getContent: `
                <p>
                    ${lib.translate(lib.itlcodes.ACTIONS_STACKS_DESCRIPTION)}
                </p>
                <table class="vis">
                    <tr>
                        <th>${lib.translate(lib.itlcodes.VILLAGE)}</th>
                        <th>${lib.translate(lib.itlcodes.ACTIONS_STACKS_POSSIBLE_NUKES)}</th>
                        <th>${lib.translate(lib.itlcodes.ACTIONS_STACKS_CURRENT_STRENGTH)}</th>
                    </tr>
                </table>
            `
        };
    }

    function makeSuggestedNobleTargetsTab() {
        return {
            containerId: 'vault-suggested-noble-targets-container',
            label: lib.translate(lib.itlcodes.TAB_NOBLE_TARGETS),

            _init: function ($container, data) {
                if (data.length)
                    this.$tabButton.html(this.label + ' (' + data.length + ')');

                let $table = $container.find('table');

                if (!data.length) {
                    $table.append(noDataRow(5, lib.translate(lib.itlcodes.ACTIONS_NOBLE_TARGETS_NONE)));
                    return;
                }

                data.forEach((target, i) => {
                    let rowClass = i % 2 ? 'row_b' : 'row_a';

                    $table.append(`
                        <tr class="${rowClass}">
                            <td>${makeVillageLink(target.villageName, target.villageId, target.x, target.y)}</td>
                            <td>${makePlayerLink(target.playerName, target.playerId)}</td>
                            <td>${target.loyalty}</td>
                            <td>${Math.roundTo(target.stationedDVs, 2)}</td>
                            <td>${target.dVsSeenAt ? lib.translate(lib.itlcodes.ACTIONS_NOBLE_TARGETS_DV_AGE, { duration: lib.formatDuration(target.dVsSeenAt) } ) : '-'}</td >
                        </tr>
                    `.trim());
                });
            },

            // ACTIONS_NOBLE_TARGETS_STATIONED_DVS
            // ACTIONS_NOBLE_TARGETS_DVS_SEEN_AT
            // VILLAGE | OWNER | LOYALTY
            getContent: `
                <p>
                    ${lib.translate(lib.itlcodes.ACTIONS_NOBLE_TARGETS_DESCRIPTION)}
                </p>
                <table class="vis">
                    <tr>
                        <th>${lib.translate(lib.itlcodes.VILLAGE)}</th>
                        <th>${lib.translate(lib.itlcodes.OWNER)}</th>
                        <th>${lib.translate(lib.itlcodes.LOYALTY)}</th>
                        <th>${lib.translate(lib.itlcodes.ACTIONS_NOBLE_TARGETS_STATIONED_DVS)}</th>
                        <th>${lib.translate(lib.itlcodes.ACTIONS_NOBLE_TARGETS_DVS_SEEN_AT)}</th>
                    </tr>
                </table>
            `
        };
    }

    function makeSuggestedUselessStacksTab() {
        return {
            containerId: 'vault-suggested-useless-stacks-container',
            label: lib.translate(lib.itlcodes.TAB_USELESS_STACKS),

            _init: function ($container, data) {
                if (data.length)
                    this.$tabButton.html(this.label + ' (' + data.length + ')');

                let $table = $container.find('table');

                if (!data.length) {
                    $table.append(noDataRow(4, lib.translate(lib.itlcodes.ACTIONS_USELESS_STACKS_NONE)));
                    return;
                }

                data.forEach((stack, i) => {
                    let rowClass = i % 2 ? 'row_b' : 'row_a';

                    $table.append(`
                        <tr class="${rowClass}">
                            <td>${makeVillageLink(stack.villageName, stack.villageId, stack.x, stack.y)}</td>
                            <td>${Math.roundTo(stack.popCount / 1000, 2)}k</td>
                            <td>${stack.playerName ? makePlayerLink(stack.playerName, stack.playerId) : '-'}</td>
                            <td>${stack.tribeName ? makeTribeLink(stack.tribeName, stack.tribeId) : ''}</td>
                        </tr>
                    `)
                });
            },
            
            getContent: `
                <p>
                    ${lib.translate(lib.itlcodes.ACTIONS_USELESS_STACKS_DESCRIPTION)}
                </p>
                <table class="vis">
                    <tr>
                        <th>${lib.translate(lib.itlcodes.VILLAGE)}</th>
                        <th>${lib.translate(lib.itlcodes.ACTIONS_USELESS_STACKS_POP_COUNT)}</th>
                        <th>${lib.translate(lib.itlcodes.OWNER)}</th>
                        <th>${lib.translate(lib.itlcodes.TRIBE)}</th>
                    </tr>
                </table>
            `
        };
    }
}

function makeRequestLocalSupportTab() {
    let settings = lib.getLocalStorage('request-local-support-settings', {
        maxTravelTimeHours: 6
    });

    let saveSettings = () => lib.setLocalStorage('request-local-support-settings', settings);

    return {
        label: lib.translate(lib.itlcodes.TAB_QUICK_SUPPORT),
        containerId: 'vault-quick-support-container',

        init: function ($container) {
            uilib.syncProp('#vault-request-local-support-hours', settings, 'maxTravelTimeHours', saveSettings, uilib.propTransformers.float);

            let $searchButton = $container.find('#vault-request-local-support-search');
            let $outputTextbox = $container.find('#vault-request-local-support-results');
            $outputTextbox.click(() => $outputTextbox[0].select());

            $container.find('input').css({
                'text-align': 'center'
            });

            $searchButton.click((e) => {
                e.originalEvent.preventDefault();

                let x = $('#vault-request-local-support-x').val();
                let y = $('#vault-request-local-support-y').val();

                if (!x || !y)
                    return;

                x = parseInt(x);
                y = parseInt(y);

                $searchButton.prop('disabled', true);

                let travelSeconds = Math.ceil(settings.maxTravelTimeHours * 60 * 60);
                lib.getApi(`alert/nearby-support?x=${x}&y=${y}&maxTravelSeconds=${travelSeconds}`)
                    .done((players) => {
                        $searchButton.prop('disabled', false);
                        if (!players.length) {
                            alert(lib.translate(lib.itlcodes.NO_PLAYERS_FOUND, { _escaped: false }));
                            return;
                        }

                        let mailText = players.map(_ => _.playerName).join(';');
                        $outputTextbox.val(mailText);

                        let playerLinks = players.map((player) => {
                            return `<a href="${lib.makeTwUrl('screen=info_player&id=' + player.playerId)}">${player.playerName}</a>`;
                        });

                        $container.find('#vault-request-local-support-result-players').html(playerLinks.join(', '));

                        $container.find('#vault-request-local-support-output').css('display', 'block');
                    })
                    .error(() => {
                        $searchButton.prop('disabled', false);
                        alert(lib.messages.GENERIC_ERROR);
                    });

                return false;
            });
        },

        getContent: `
            <p>
                ${lib.translate(lib.itlcodes.ACTIONS_QUICK_SUPPORT_DESCRIPTION)}
            </p>
            <p>
                ${lib.translate(lib.itlcodes.ACTIONS_QUICK_SUPPORT_SETTINGS_1)}
                <input type="text" style="width:2em" id="vault-request-local-support-x">|<input type="text" style="width:2em" id="vault-request-local-support-y">
                ${lib.translate(lib.itlcodes.ACTIONS_QUICK_SUPPORT_SETTINGS_2)}
                <input type="text" style="width:2em" id="vault-request-local-support-hours">
                ${lib.translate(lib.itlcodes.ACTIONS_QUICK_SUPPORT_SETTINGS_3)}
            </p>
            <p>
                <input type="button" id="vault-request-local-support-search" value="${lib.translate(lib.itlcodes.SEARCH)}">
            </p>
            <div id="vault-request-local-support-output" style="display:none">
                <h4>${lib.translate(lib.itlcodes.RESULTS)}</h4>
                <input id="vault-request-local-support-results" style="width:100%" readonly>
                <p id="vault-request-local-support-result-players"></p>
            </div>
        `
    }
}

function makeRequestSnipeTab() {
    return {
        label: 'Request Snipe',
        containerId: 'vault-request-snipe-container',

        init: function ($container) {

        },

        getContent: `
        `
    }
}

function makeRequestStackTab() {

    let settings = lib.getLocalStorage('request-stack-settings', {
        maxTravelTimeHours: 48
    });

    let saveSettings = () => lib.setLocalStorage('request-stack-settings', settings);

    return {
        label: lib.translate(lib.itlcodes.TAB_REQUEST_STACK),
        containerId: 'vault-request-stack-container',

        init: function ($container) {
            uilib.syncProp('#vault-request-stack-hours', settings, 'maxTravelTimeHours', saveSettings, uilib.propTransformers.floatOptional);

            let $searchButton = $container.find('#vault-request-stack-search');
            let $outputTextbox = $container.find('#vault-request-stack-results');
            $outputTextbox.click(() => $outputTextbox[0].select());

            $container.find('input').css({
                'text-align': 'center'
            });

            $searchButton.click((e) => {
                e.originalEvent.preventDefault();

                let x = $('#vault-request-stack-x').val();
                let y = $('#vault-request-stack-y').val();

                if (!x || !y)
                    return;

                x = parseInt(x);
                y = parseInt(y);

                $searchButton.prop('disabled', true);

                let queryString = `alert/request-backline-defense?x=${x}&y=${y}`;
                if (settings.maxTravelTimeHours) {
                    let travelSeconds = Math.ceil(settings.maxTravelTimeHours * 60 * 60);
                    queryString += `&maxTravelSeconds=${travelSeconds}`;
                }
                
                lib.getApi(queryString)
                    .done((players) => {
                        $searchButton.prop('disabled', false);
                        if (!players.length) {
                            alert(lib.translate(lib.itlcodes.NO_PLAYERS_FOUND, { _escaped: false }));
                            return;
                        }

                        let mailText = players.map(_ => _.playerName).join(';');
                        $outputTextbox.val(mailText);

                        let playerLinks = players.map((player) => {
                            return `<a href="${lib.makeTwUrl('screen=info_player&id=' + player.playerId)}">${player.playerName}</a>`;
                        });

                        $container.find('#vault-request-stack-result-players').html(playerLinks.join(', '));

                        $container.find('#vault-request-stack-output').css('display', 'block');
                    })
                    .error(() => {
                        $searchButton.prop('disabled', false);
                        alert(lib.messages.GENERIC_ERROR);
                    });

                return false;
            });
        },
        
        getContent: `
            <p>
                ${lib.translate(lib.itlcodes.ACTIONS_REQUEST_STACK_DESCRIPTION)}
            </p>
            <p>
                ${lib.translate(lib.itlcodes.ACTIONS_REQUEST_STACK_SETTINGS_1)}
                <input type="text" style="width:2em" id="vault-request-stack-x">|<input type="text" style="width:2em" id="vault-request-stack-y">
                ${lib.translate(lib.itlcodes.ACTIONS_REQUEST_STACK_SETTINGS_2)}
                <input type="text" style="width:2em" id="vault-request-stack-hours">
                ${lib.translate(lib.itlcodes.ACTIONS_REQUEST_STACK_SETTINGS_3)}
            </p>
            <p>
                <input type="button" id="vault-request-stack-search" value="${lib.translate(lib.itlcodes.SEARCH)}">
            </p>
            <div id="vault-request-stack-output" style="display:none">
                <h4>${lib.translate(lib.itlcodes.RESULTS)}</h4>
                <input id="vault-request-stack-results" style="width:100%" readonly>
                <p id="vault-request-stack-result-players"></p>
            </div>
        `
    }
}