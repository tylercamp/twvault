function parseMapPage($doc) {
    $doc = $doc || $(document);

    if (window.ranVaultMap) {
        return;
    }

    window.ranVaultMap = true;
    var canUse = true;

    //  Hook into 'TWMap.displayForVillage', which is invoked whenever the village info popup is made
    //  by TW

    var currentVillageId = null;
    let $popup = $doc.find('#map_popup');

    $doc.find('#continent_id').parent().append('<span> - Using Vault</span>');

    var cachedData = {};
    let requestedVillageIds = [];
    let settings = loadSettings();

    createSettingsUI();

    //  First call that actually shows the popup - Update the popup if we've already downloaded village data
    let originalDisplayForVillage = TWMap.popup.displayForVillage;
    TWMap.popup.displayForVillage = function (e, a, t) {
        console.log('intercepted displayForVillage');
        originalDisplayForVillage.call(TWMap.popup, e, a, t);

        if (!canUse)
            return;

        let villageInfo = e;
        let villageId = villageInfo.id;

        currentVillageId = villageId;
        if (cachedData[villageId]) {
            makeOutput(cachedData[villageId]);
        }
    };

    // Call made after popup is shown and TW has downloaded data for the village (ie incoming attacks, morale, etc)
    let originalReceivedInfo = TWMap.popup.receivedPopupInformationForSingleVillage;
    TWMap.popup.receivedPopupInformationForSingleVillage = function (e) {
        console.log('Intercepted receivedPopupInformation');
        originalReceivedInfo.call(TWMap.popup, e);

        let villageInfo = e;
        if (!villageInfo || !villageInfo.id)
            return;

        currentVillageId = villageInfo.id;
        let villageId = villageInfo.id;
        let morale = Math.round(villageInfo.morale * 100);
        if (isNaN(morale))
            morale = 100;

        if (cachedData[villageInfo.id]) {
            makeOutput(cachedData[villageId]);
        } else {
            if (requestedVillageIds.indexOf(villageId) >= 0) {
                return;
            }
            loadVillageTroopData(villageId, morale);
        }
    };

    function loadVillageTroopData(villageId, morale) {
        requestedVillageIds.push(villageId);
        lib.getApi(lib.makeApiUrl(`village/${villageId}/army?morale=${morale}`))
            .done((data) => {
                console.log('Got village data: ', data);

                data.morale = morale;

                data.stationedSeenAt = data.stationedSeenAt ? new Date(data.stationedSeenAt) : null;
                data.recentlyLostArmySeenAt = data.recentlyLostArmySeenAt ? new Date(data.recentlyLostArmySeenAt) : null;
                data.travelingSeenAt = data.travelingSeenAt ? new Date(data.travelingSeenAt) : null;
                data.ownedArmySeenAt = data.ownedArmySeenAt ? new Date(data.ownedArmySeenAt) : null;

                data.lastBuildingsSeenAt = data.lastBuildingsSeenAt ? new Date(data.lastBuildingsSeenAt) : null;
                data.lastLoyaltySeenAt = data.lastLoyaltySeenAt ? new Date(data.lastLoyaltySeenAt) : null;

                cachedData[villageId] = data;

                //  User changed village while the data was loading
                if (villageId != currentVillageId) {
                    return;
                }

                makeOutput(data);
            })
            .error((xhr, b, c) => {
                if (!canUse)
                    return;

                if (xhr.status == 423) {
                    alert("You haven't uploaded report data in a while, you can't use the map script until you upload some more reports. Go to a different page and run this script again.");
                    canUse = false;
                } else if (xhr.status != 401) {
                    alert("An error occurred...");
                }
            });
    }

    console.log('Added map hook');

    function makeOutput(data) {
        if ($('#vault_info').length) {
            return;
        }

        //  Limit "own commands" to max 2
        let $commandRows = $('.command-row');
        let twCommandData = [];

        //  Remove all except non-small attacks
        for (var i = 0; i < $commandRows.length; i++) {
            let $row = $($commandRows[i]);
            let $images = $row.find('img');
            let isSmall = false;
            let isSupport = true;
            let isOwn = false;
            let isReturning = false;
            $images.each((i, el) => {
                let $el = $(el);
                if ($el.prop('src').contains('attack_'))
                    isOwn = true;

                if ($el.prop('src').contains("attack_small"))
                    isSmall = true;

                if ($el.prop('src').contains('attack'))
                    isSupport = false;

                if ($el.prop('src').contains('return'))
                    isReturning = true;
            });

            //  Collect command data for later
            let commandId = parseInt($row.find('.command_hover_details').data('command-id'));
            let commandData = {
                isSmall: isSmall,
                isSupport: isSupport,
                isOwn: isOwn,
                isReturning: isReturning
            };

            twCommandData.push(commandData);

            if ((isSmall || isSupport) && $commandRows.length > 2) {
                $($commandRows[i]).remove();
                $commandRows = $('.command-row');
                --i;
            }
        }

        //  Remove intel rows
        $('#info_last_attack').closest('tr').remove();
        $('#info_last_attack_intel').closest('tr').remove();

        $('#info_content').css('width', '100%');

        let $villageInfoContainer = $('<div id="vault_info" style="background-color:#e5d7b2;">');
        $villageInfoContainer.appendTo($popup);

        //  Update data with what's been loaded by TW (in case someone forgot to upload commands)
        let hasRecord = (id) => (data.fakes && data.fakes.contains(id)) || (data.dVs && data.dVs[id]) || (data.nukes && data.nukes.contains(id));

        let numFakes = data.fakes ? data.fakes.length : 0;
        let numNukes = data.nukes ? data.nukes.length : 0;
        let numPlayers = data.players ? data.players.length : 0;

        let numDVs = 0;
        lib.objForEach(data.Dvs, (commandId, pop) => {
            numDVs += pop / 20000;
        });
        numDVs = Math.roundTo(numDVs, 1);

        twCommandData.forEach((cmd) => {
            if (!cmd.isOwn || hasRecord(cmd.commandId) || cmd.isReturning)
                return;

            if (!cmd.isSupport) {
                if (cmd.isSmall)
                    numFakes++;
                else
                    numNukes++;
            }
        });

        //  NOTE - This assumes no archers!
        $villageInfoContainer.html(`
                    ${ !settings.showCommands ? '' : `
                        <table class='vis' style="width:100%">
                            <tr>
                                <th># Fakes</th>
                                <th># Nukes</th>
                                <th># DVs</th>
                                <th># Players Sending</th>
                            </tr>
                            <tr>
                                <td>${numFakes}</td>
                                <td>${numNukes}</td>
                                <td>${numDVs}</td>
                                <td>${numPlayers}</td>
                            </tr>
                        </table>
                    `}
                    ${ !data.stationedArmy && !data.travelingArmy && !data.recentlyLostArmy && !data.ownedArmy ? '<div style="text-align:center;padding:0.5em;">No army data available.</div>' : `
                    <table class='vis' style="width:100%">
                        <tr style="background-color:#c1a264 !important">
                            <th>Vault</th>
                            <th>Seen at</th>
                            <th><img src="https://dsen.innogamescdn.com/8.136/37951/graphic/unit/unit_spear.png" title="" alt="" class=""></th>
                            <th><img src="https://dsen.innogamescdn.com/8.136/37951/graphic/unit/unit_sword.png" title="" alt="" class=""></th>
                            <th><img src="https://dsen.innogamescdn.com/8.136/37951/graphic/unit/unit_axe.png" title="" alt="" class=""></th>
                            <th><img src="https://dsen.innogamescdn.com/8.136/37951/graphic/unit/unit_spy.png" title="" alt="" class=""></th>
                            <th><img src="https://dsen.innogamescdn.com/8.136/37951/graphic/unit/unit_light.png" title="" alt="" class=""></th>
                            <th><img src="https://dsen.innogamescdn.com/8.136/37951/graphic/unit/unit_heavy.png" title="" alt="" class=""></th>
                            <th><img src="https://dsen.innogamescdn.com/8.136/37951/graphic/unit/unit_ram.png" title="" alt="" class=""></th>
                            <th><img src="https://dsen.innogamescdn.com/8.136/37951/graphic/unit/unit_catapult.png" title="" alt="" class=""></th>
                            <th><img src="https://dsen.innogamescdn.com/8.136/37951/graphic/unit/unit_knight.png" title="" alt="" class=""></th>
                            <th><img src="https://dsen.innogamescdn.com/8.136/37951/graphic/unit/unit_snob.png" title="" alt="" class=""></th>
                        </tr>
                        ${ !data.stationedArmy ? '' : `
                        <tr>
                            <td>Stationed</td>
                            <td>${data.stationedSeenAt ? lib.formateDateTime(data.stationedSeenAt) : ''}</td>
                            ${makeTroopTds(data.stationedArmy || {})}
                        </tr>
                        `}
                        ${ !data.travelingArmy ? '' : `
                        <tr>
                            <td>Traveling</td>
                            <td>${data.travelingSeenAt ? lib.formateDateTime(data.travelingSeenAt) : ''}</td>
                            ${makeTroopTds(data.travelingArmy || {})}
                        </tr>
                        `}
                        ${ !data.recentlyLostArmy ? '' : `
                        <tr>
                            <td>Recently lost</td>
                            <td>${data.recentlyLostArmySeenAt ? lib.formateDateTime(data.recentlyLostArmySeenAt) : ''}</td>
                            ${makeTroopTds(data.recentlyLostArmy || {})}
                        </tr>
                        `}
                        ${ !data.ownedArmy ? '' : `
                        <tr>
                            <td>Owned</td>
                            <td>${data.ownedArmySeenAt ? lib.formateDateTime(data.ownedArmySeenAt) : ''}</td>
                            ${makeTroopTds(data.ownedArmy || {})}
                        </tr>
                        `}
                        ${ !settings.showPossiblyRecruited ? '' : `
                            ${ !data.possibleRecruitedOffensiveArmy || !data.possibleRecruitedDefensiveArmy ? '' : `
                            <tr>
                                <td rowspan="2">Possibly recruited</td>
                                <td></td>
                                ${makeTroopTds(data.possibleRecruitedOffensiveArmy || {})}
                            </tr>
                            <tr>
                                <td></td>
                                ${makeTroopTds(data.possibleRecruitedDefensiveArmy || {})}
                            </tr>
                            `}
                        `}
                        ${ !data.nukesRequired || !data.showNukes ? '' : `
                        <tr>
                            <td colspan=12 style="text-align:center">Will take ~${data.nukesRequired} nukes to clear at ${data.morale}% morale</td>
                        </tr>
                        `}
                    </table>
                    `}
                    ${ !settings.showBuildings ? '' : `
                        ${ typeof data.lastBuildings == 'undefined' || data.lastBuildings == null ? '<div style="text-align:center;padding:0.5em;">No building data available.</div>' : `
                        <table class='vis' style="width:100%">
                            <tr style="background-color:#c1a264 !important">
                                <th>Vault</th>
                                <th>Seen at</th>
                                <th><img src="https://dsen.innogamescdn.com/8.137/38092/graphic/buildings/snob.png" title="Academy" alt="" class="bmain_list_img"></th>
                                <th><img src="https://dsen.innogamescdn.com/8.137/38092/graphic/buildings/smith.png" title="Smithy" alt="" class="bmain_list_img"></th>
                                <th><img src="https://dsen.innogamescdn.com/8.137/38092/graphic/buildings/farm.png" title="Farm" alt="" class="bmain_list_img"></th>
                                <th><img src="https://dsen.innogamescdn.com/8.137/38092/graphic/buildings/wall.png" title="Wall" alt="" class="bmain_list_img"></th>
                            </tr>
                            <tr>
                                <td>Latest levels</td>
                                <td>${data.lastBuildingsSeenAt ? lib.formateDateTime(data.lastBuildingsSeenAt) : ''}</td>
                                <td>${data.lastBuildings ? data.lastBuildings['snob'] || '-' : '' }</td>
                                <td>${data.lastBuildings ? data.lastBuildings['smith'] || '-' : '' }</td>
                                <td>${data.lastBuildings ? data.lastBuildings['farm'] || '-' : '' }</td>
                                <td>${data.lastBuildings ? data.lastBuildings['wall'] || '-' : '' }</td>
                            </tr>
                            <tr>
                                <td>Possible levels</td>
                                <td></td>
                                <td>${data.possibleBuildings ? data.possibleBuildings['snob'] || '-' : ''}</td>
                                <td>${data.possibleBuildings ? data.possibleBuildings['smith'] || '-' : ''}</td>
                                <td>${data.possibleBuildings ? data.possibleBuildings['farm'] || '-' : ''}</td>
                                <td>${data.possibleBuildings ? data.possibleBuildings['wall'] || '-' : '' }</td>
                            </tr>
                        </table>
                        `}
                    `}
                    ${ typeof data.lastLoyalty == 'undefined' || data.lastLoyalty == null || !settings.showLoyalty ? '' : `
                    <table class='vis' style="width:100%">
                        <tr style="background-color:#c1a264 !important">
                            <th>Vault</th>
                            <th>Seen at</th>
                            <th>Loyalty</th>
                        </tr>
                        <tr>
                            <td>Latest loyalty</td>
                            <td>${data.lastLoyaltySeenAt ? lib.formateDateTime(data.lastLoyaltySeenAt) : ''}</td>
                            <td>${data.lastLoyalty ? data.lastLoyalty || '-' : ''}</td>
                        </tr>
                        <tr>
                            <td>Possible loyalty</td>
                            <td></td>
                            <td>${data.possibleLoyalty ? data.possibleLoyalty || '-' : ''}</td>
                        </tr>
                    </table>
                    `}
                `.trim());
    }

    function makeTroopTds(troops) {
        var counts = [];
        counts.push(troops['spear']);
        counts.push(troops['sword']);
        counts.push(troops['axe']);
        counts.push(troops['spy']);
        counts.push(troops['light']);
        counts.push(troops['heavy']);
        counts.push(troops['ram']);
        counts.push(troops['catapult']);
        counts.push(troops['knight']);
        counts.push(troops['snob']);

        var parts = [];
        counts.forEach((cnt) => parts.push(`<td>${cnt || cnt == 0 ? cnt : ''}</td>`));
        return parts.join(' ');
    }

    function createSettingsUI() {
        let $container = $(`
            <div>
                <h4>Vault Overlay Settings</h4>
                <p>
                    <input type="checkbox" id="vault-show-commands" ${settings.showCommands ? 'checked' : ''}>
                    <label for="vault-show-commands">Commands</label>

                    <input type="checkbox" id="vault-show-recruits" ${settings.showPossiblyRecruited ? 'checked' : ''}>
                    <label for="vault-show-recruits">Possible recruits</label>

                    <input type="checkbox" id="vault-show-buildings" ${settings.showBuildings ? 'checked' : ''}>
                    <label for="vault-show-buildings">Buildings</label>

                    <input type="checkbox" id="vault-show-nukes" ${settings.showNukes ? 'checked' : ''}>
                    <label for="vault-show-nukes">Nukes required</label>

                    <input type="checkbox" id="vault-show-loyalty" ${settings.showLoyalty ? 'checked' : ''}>
                    <label for="vault-show-loyalty">Loyalty</label>
                </p>
            </div>
        `.trim());

        $container.find('label').css({
            'margin-right': '1.5em'
        });

        $('#content_value > h2').after($container);

        $container.find('#vault-show-commands').change(() => {
            settings.showCommands = this.checked;
            saveSettings(settings);
        });

        $container.find('#vault-show-recruits').change(() => {
            settings.showPossiblyRecruited = this.checked;
            saveSettings(settings);
        });

        $container.find('#vault-show-buildings').change(() => {
            settings.showBuildings = this.checked;
            saveSettings(settings);
        });

        $container.find('#vault-show-nukes').change(() => {
            settings.showNukes = this.checked;
            saveSettings(settings);
        });

        $container.find('#vault-show-loyalty').change(() => {
            settings.showLoyalty = this.checked;
            saveSettings(settings);
        });
    }

    function loadSettings() {
        return lib.getLocalStorage('map-settings') || {
            showCommands: true,
            showPossiblyRecruited: true,
            showBuildings: true,
            showNukes: true,
            showLoyalty: true
        };
    }

    function saveSettings(settings) {
        lib.setLocalStorage('map-settings', settings);
    }
}