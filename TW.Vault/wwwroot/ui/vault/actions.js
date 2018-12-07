
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
        label: 'Actions/Alerts',
        containerId: 'vault-actions-container',

        init: function () {
            let self = this;
            suggestionsTab.onLoaded = (data, count) => {
                self.$tabButton.text(self.label + ' (' + count + ')');
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
        label: 'Alerts',
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
            label: 'Send Recap',
            containerId: 'vault-suggested-recaps-container',

            _init: function ($container, data) {
                if (data.length)
                    this.$tabButton.text(this.label + ' (' + data.filter(_ => _.isNearby).length + ')');

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
                    $table.append(noDataRow(5, '(No recaps)'));
                    return;
                }

                data.forEach((recap, i) => {
                    let rowClass = i % 2 ? 'row_b' : 'row_a';

                    $table.append(`
                        <tr class="${rowClass}">
                            <td>${makeVillageLink(recap.villageName, recap.villageId, recap.x, recap.y)}</td>
                            <td>${recap.loyalty}</td>
                            <td>${lib.formatDuration(recap.occurredAt)} ago</td>
                            <td>${makePlayerLink(recap.oldOwnerName, recap.oldOwnerId)}</td>
                            <td>${makePlayerLink(recap.newOwnerName, recap.newOwnerId)}</td>
                        </tr>
                    `.trim());
                });
            },

            getContent: `
                <p>
                    A list of friendly villages that were recently conquered.
                </p>
                <p>
                    <input type="checkbox" id="vault-suggested-recaps-only-nearby">
                    <label for="vault-suggested-recaps-only-nearby">Only show recaps with nobles nearby</label>
                </p>
                <table class="vis">
                    <tr>
                        <th>Village</th>
                        <th>Loyalty</th>
                        <th>Capped At</th>
                        <th>Old Owner</th>
                        <th>New Owner</th>
                    </tr>
                </table>
            `
        };
    }

    function makeSuggestedSnipeTab() {
        return {
            label: 'Snipes Needed',
            containerId: 'vault-suggested-snipes-container',

            _init: function ($container, data) {
                if (data.length)
                    this.$tabButton.text(this.label + ' (' + data.length + ')');

                let $table = $container.find('table');

                if (!data.length) {
                    $table.append(noDataRow(3, '(No snipes needed)'));
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
                    A list of incoming trains that you have troops to snipe for.
                </p>
                <table class="vis">
                    <tr>
                        <th>Village</th>
                        <th># Nobles</th>
                        <th>Lands At</th>
                    </tr>
                </table>
            `
        };
    }

    function makeSuggestedStackingTab() {
        return {
            containerId: 'vault-suggested-stacks-container',
            label: 'Send Stacks',

            _init: function ($container, data) {
                if (data.length)
                    this.$tabButton.text(this.label + ' (' + data.length + ')');

                let $table = $container.find('table');

                if (!data.length) {
                    $table.append(noDataRow(3, '(No stacks to suggest)'));
                    return;
                }

                data.forEach((village, i) => {
                    let rowClass = i % 2 ? 'row_b' : 'row_a';

                    $table.append(`
                        <tr class="${rowClass}">
                            <td>${makeVillageLink(village.villageName, village.villageId, village.x, village.y)}</td>
                            <td>${village.sentNukes}</td>
                            <td>Can eat ${village.eatableNukes} nukes</td>
                        </tr>
                    `.trim());
                });
            },

            getContent: `
                <p>
                    A list of villages to stack, based on their incomings and current defense stationed there.
                </p>
                <table class="vis">
                    <tr>
                        <th>Village</th>
                        <th>Possible Nukes</th>
                        <th>Current Stack Strength</th>
                    </tr>
                </table>
            `
        };
    }

    function makeSuggestedNobleTargetsTab() {
        return {
            containerId: 'vault-suggested-noble-targets-container',
            label: 'Noble Targets',

            _init: function ($container, data) {
                if (data.length)
                    this.$tabButton.text(this.label + ' (' + data.length + ')');

                let $table = $container.find('table');

                if (!data.length) {
                    $table.append(noDataRow(5, '(No suggested targets)'));
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
                            <td>${target.dVsSeenAt ? lib.formatDuration(target.dVsSeenAt) + ' ago' : '-'}</td>
                        </tr>
                    `.trim());
                });
            },

            getContent: `
                <p>
                    A list of potential nobling targets, based on their stationed defense and current loyalty.
                </p>
                <table class="vis">
                    <tr>
                        <th>Village</th>
                        <th>Owner</th>
                        <th>Loyalty</th>
                        <th>Stationed DVs</th>
                        <th>DVs Seen At</th>
                    </tr>
                </table>
            `
        };
    }

    function makeSuggestedUselessStacksTab() {
        return {
            containerId: 'vault-suggested-useless-stacks-container',
            label: 'Useless Stacks',

            _init: function ($container, data) {
                if (data.length)
                    this.$tabButton.text(this.label + ' (' + data.length + ')');

                let $table = $container.find('table');

                if (!data.length) {
                    $table.append(noDataRow(4, '(No useless stacks)'));
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
                    A list of villages that should have their support sent home, whether they are backline villages
                    or non-friendly villages.
                </p>
                <table class="vis">
                    <tr>
                        <th>Village</th>
                        <th>Pop. Count</th>
                        <th>Owner</th>
                        <th>Tribe</th>
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
        label: 'Quick Support',
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
                            alert('No players found!');
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
                        alert('An error occurred...');
                    });

                return false;
            });
        },

        getContent: `
            <p>
                Find and mail players with nearby defense.
            </p>
            <p>
                Send to <input type="text" style="width:2em" id="vault-request-local-support-x">|<input type="text" style="width:2em" id="vault-request-local-support-y">
                that can reach within <input type="text" style="width:2em" id="vault-request-local-support-hours"> hours
            </p>
            <p>
                <input type="button" id="vault-request-local-support-search" value="Search">
            </p>
            <div id="vault-request-local-support-output" style="display:none">
                <h4>Results</h4>
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
        label: 'Request Stack',
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
                            alert('No players found!');
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
                        alert('An error occurred...');
                    });

                return false;
            });
        },

        getContent: `
            <p>
                Find and mail players with backline defense.
            </p>
            <p>
                Send to <input type="text" style="width:2em" id="vault-request-stack-x">|<input type="text" style="width:2em" id="vault-request-stack-y">
                that can reach within <input type="text" style="width:2em" id="vault-request-stack-hours"> hours (optional)
            </p>
            <p>
                <input type="button" id="vault-request-stack-search" value="Search">
            </p>
            <div id="vault-request-stack-output" style="display:none">
                <h4>Results</h4>
                <input id="vault-request-stack-results" style="width:100%" readonly>
                <p id="vault-request-stack-result-players"></p>
            </div>
        `
    }
}