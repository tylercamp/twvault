function parseMapPage($doc) {
    $doc = $doc || $(document);

    if (window.ranVaultMap) {
        return;
    }

    window.ranVaultMap = true;
    var canUse = true;
    var isUnloading = false;

    //  Hook into 'TWMap.displayForVillage', which is invoked whenever the village info popup is made
    //  by TW

    var mapOverlayTags = null;
    var currentVillageId = null;
    let $popup = $doc.find('#map_popup');

    $doc.find('#continent_id').parent().append('<span> - Using Vault</span>');

    let groupStyles = $('.colorgroup-other-entry, .colorgroup-own-entry').toArray().map((row) => {
        let $row = $(row);

        let groupId = $row.data('id');
        let name = $row.find('td:nth-of-type(2)').text().trim();
        let template = $row.find('.marker')[0].outerHTML;

        return {
            id: groupId,
            name: name,
            template: template
        };
    });

    let groupStylesById = lib.arrayToObject(groupStyles, (g) => g.id, (g) => g);

    var cachedData = {};
    let requestedVillageIds = [];
    let settings = loadSettings();
    let lockedDataReasons = null;

    const TAG_TYPES = {
        NOBLES: 'nobles',
        NUKE: 'nuke',
        STACKED: 'stacked'
    };

    let groupMappings = {};
    groupMappings[TAG_TYPES.NOBLES] = groupStylesById[settings.overlayNobleGroupId] || null;
    groupMappings[TAG_TYPES.NUKE] = groupStylesById[settings.overlayNukeGroupId] || null;
    groupMappings[TAG_TYPES.STACKED] = groupStylesById[settings.overlayStackedGroupId] || null;

    console.log('Made group style: ', groupStyles);

    $(window).unload(() => isUnloading = true);

    //  Get all data for the whole world for now
    lib.getApi(`village/0/0/1000/1000/tags`)
        .done((data) => {
            console.log('Got map tags: ', data);
            mapOverlayTags = data;

            if (settings.showOverlay)
                applyMapOverlay();
        });

    createSettingsUI();

    //  First call that actually shows the popup - Update the popup if we've already downloaded village data
    let originalDisplayForVillage = TWMap.popup.displayForVillage;
    TWMap.popup.displayForVillage = function (e, a, t) {
        console.log('intercepted displayForVillage');
        originalDisplayForVillage.call(TWMap.popup, e, a, t);

        if (isUnloading) {
            return;
        }

        if (lockedDataReasons) {
            makeFuckYouContainer();
            return;
        }

        if (!canUse)
            return;

        let villageInfo = e;
        let villageId = villageInfo.id;

        currentVillageId = villageId;
        if (cachedData[villageId]) {
            makeOutput(cachedData[villageId]);
        } else if (TWMap.popup._cache[villageId]) {
            let twCached = TWMap.popup._cache[villageId];
            if (requestedVillageIds.indexOf(villageId) >= 0) {
                return;
            }
            let morale = Math.round((twCached.morale || twCached.mood) * 100);
            if (isNaN(morale) || morale > 100)
                morale = 100;
            loadVillageTroopData(villageId, morale);
        }
    };

    // Call made after popup is shown and TW has downloaded data for the village (ie incoming attacks, morale, etc)
    let originalReceivedInfo = TWMap.popup.receivedPopupInformationForSingleVillage;
    TWMap.popup.receivedPopupInformationForSingleVillage = function (e) {
        console.log('Intercepted receivedPopupInformation');
        originalReceivedInfo.call(TWMap.popup, e);

        if (isUnloading) {
            return;
        }

        if (lockedDataReasons) {
            makeFuckYouContainer();
            return;
        }

        let villageInfo = e;
        if (!villageInfo || !villageInfo.id)
            return;

        currentVillageId = villageInfo.id;
        let villageId = villageInfo.id;
        //  Why is "mood" a thing (alternate name for "morale")
        let morale = Math.round((villageInfo.morale || villageInfo.mood) * 100);
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

    let originalSpawnSector = TWMap.mapHandler.spawnSector;
    TWMap.mapHandler.spawnSector = function (data, sector) {
        originalSpawnSector.call(TWMap.mapHandler, data, sector);

        if (mapOverlayTags && settings.showOverlay)
            applyMapOverlay(sector._elements);
    };

    function loadVillageTroopData(villageId, morale) {
        requestedVillageIds.push(villageId);
        lib.getApi(`village/${villageId}/army?morale=${morale}`)
            .done((data) => {
                console.log('Got village data: ', data);

                data.morale = morale;
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
                    let reasons = null;
                    try {
                        reasons = JSON.parse(xhr.responseText);
                        lockedDataReasons = reasons;
                    } catch (_) { }

                    let alertMessage = "You haven't uploaded data in a while, you can't use the map script until you do. Go to a different page and run this script again."
                    if (reasons) {
                        alertMessage += `\nYou need to upload: ${reasons.join(', ')}`;
                    }

                    alert(alertMessage);
                    canUse = false;
                } else if (xhr.status != 401) {
                    alert("An error occurred...");
                }
            });
    }

    function applyMapOverlay(elements) {
        if (!elements) {
            elements = lib.objectToArray(TWMap.map._visibleSectors, (_, v) => v).map((s) => s._elements).flat();
        }

        elements.forEach((img) => {
            const imgId = img.id;
            if (imgId == null) {
                return;
            }
            let villageId = imgId.match(/map_village_(\d+)/);
            if (!villageId) {
                return;
            }
            villageId = parseInt(villageId[1]);

            if (!mapOverlayTags[villageId]) {
                return;
            }

            let $parent = $(img).parent();
            let x = $(img).css('left');
            let y = $(img).css('top');

            if (hasHighlights(mapOverlayTags[villageId])) {
                let $overlay = $(`<div id="vault_overlay_${villageId}">`);
                $overlay.css({
                    width: '52px',
                    height: '37px',
                    position: 'absolute',
                    left: x,
                    top: y,
                    'z-index': 50,
                    outline: 'rgba(51, 255, 0, 0.7) solid 2px',
                    'background-color': 'rgba(155, 252, 10, 0.14)'
                });

                $overlay.appendTo($parent);
            }

            let tags = makeTagElements(mapOverlayTags[villageId]);

            tags.forEach((tag, i) => {
                let $tag = $(tag);
                $tag.css({
                    position: 'absolute',
                    left: x,
                    top: y,
                    'z-index': 51,
                    width: '18px',
                    height: '18px',
                    'margin-top': '18px',
                    'margin-left': `${20 * i}px`
                })

                $tag.find('img').css({
                    width: '18px',
                    height: '18px'
                })

                $tag.appendTo($parent);
            });
        });
    }

    function hasHighlights(tag) {
        switch (settings.overlayHighlights) {
            case 'none': return false;
            case 'limited': return isRecentIntel(tag.stackSeenAt) || isRecentIntel(tag.nukeSeenAt) || isRecentIntel(tag.noblesSeenAt);
            case 'all': return true;
        }
    }

    function makeTagElements(tag, villageId) {
        let result = [];
        if (tag.isStacked && isRecentIntel(tag.stackSeenAt) && groupMappings[TAG_TYPES.STACKED]) {
            let $stackedIcon = $(groupMappings[TAG_TYPES.STACKED].template);
            $stackedIcon.prop('id', `vault_overlay_icon_${TAG_TYPES.STACKED}_${villageId}`)
            result.push($stackedIcon);
        }
        if (tag.hasNuke && isRecentIntel(tag.nukeSeenAt) && groupMappings[TAG_TYPES.NUKE]) {
            let $nukeIcon = $(groupMappings[TAG_TYPES.NUKE].template);
            $nukeIcon.prop('id', `vault_overlay_icon_${TAG_TYPES.NUKE}_${villageId}`);
            result.push($nukeIcon);
        }
        if (tag.hasNobles && isRecentIntel(tag.noblesSeenAt) && groupMappings[TAG_TYPES.NOBLES]) {
            let $nobleIcon = $(groupMappings[TAG_TYPES.NOBLES].template);
            $nobleIcon.prop('id', `vault_overlay_icon_${TAG_TYPES.NOBLES}_${villageId}`);
            result.push($nobleIcon);
        }

        return result;
    }

    function isRecentIntel(intelDate) {
        return intelDate && intelDate.valueOf() > lib.getServerDateTime().valueOf() - 24 * 60 * 60 * 1000 * settings.maxIntelAgeDays;
    }

    function updateTagIcons(tag) {
        let $tags = $(`*[id^=vault_overlay_icon_${tag}]`);
        $tags.each((i, el) => {
            let $tag = $(el);
            let $parent = $tag.parent();

            let $newTag = $(groupMappings[tag].template);
            $newTag.prop('id', $tag.prop('id'));
            $newTag.css({
                position: 'absolute',
                width: '18px',
                height: '18px',
                left: $tag.css('left'),
                top: $tag.css('top'),
                'z-index': 51,
                'margin-top': '18px',
                'margin-left': $tag.css('margin-left')
            });

            $newTag.find('img').css({
                width: '18px',
                height: '18px'
            });

            $tag.remove();
            $newTag.appendTo($parent);
        });
    }

    function makeOutputContainer() {
        let $villageInfoContainer = $('<div id="vault_info" style="background-color:#e5d7b2;">');
        $villageInfoContainer.appendTo($popup);
        return $villageInfoContainer;
    }

    function makeOutput(data) {
        if ($('#vault_info').length) {
            return;
        }

        if (isUnloading) {
            return;
        }

        let $villageInfoContainer = makeOutputContainer();

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
                        ${ !data.atHomeArmy ? '' : `
                        <tr>
                            <td>At home</td>
                            <td>${data.atHomeSeenAt ? lib.formatDateTime(data.atHomeSeenAt) : ''}</td>
                            ${makeTroopTds(data.atHomeArmy || {})}
                        </tr>
                        `}
                        ${ !data.stationedArmy ? '' : `
                        <tr>
                            <td>Stationed</td>
                            <td>${data.stationedSeenAt ? lib.formatDateTime(data.stationedSeenAt) : ''}</td>
                            ${makeTroopTds(data.stationedArmy || {})}
                        </tr>
                        `}
                        ${ !data.travelingArmy ? '' : `
                        <tr>
                            <td>Traveling</td>
                            <td>${data.travelingSeenAt ? lib.formatDateTime(data.travelingSeenAt) : ''}</td>
                            ${makeTroopTds(data.travelingArmy || {})}
                        </tr>
                        `}
                        ${ !data.recentlyLostArmy ? '' : `
                        <tr>
                            <td>Recently lost</td>
                            <td>${data.recentlyLostArmySeenAt ? lib.formatDateTime(data.recentlyLostArmySeenAt) : ''}</td>
                            ${makeTroopTds(data.recentlyLostArmy || {})}
                        </tr>
                        `}
                        ${ !data.ownedArmy ? '' : `
                        <tr>
                            <td>Owned</td>
                            <td>${data.ownedArmySeenAt ? lib.formatDateTime(data.ownedArmySeenAt) : ''}</td>
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
                        ${ !data.nukesRequired || !settings.showNukes ? '' : `
                        <tr>
                            <td colspan=12 style="text-align:center">Will take ~${data.nukesRequired} nukes to clear at ${data.morale}% morale (last nuke has ~${data.lastNukeLossPercent}% losses)</td>
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
                                <td>${data.lastBuildingsSeenAt ? lib.formatDateTime(data.lastBuildingsSeenAt) : ''}</td>
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
                            <td>${data.lastLoyaltySeenAt ? lib.formatDateTime(data.lastLoyaltySeenAt) : ''}</td>
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

    function makeFuckYouContainer() {
        if ($('#vault_info').length)
            return;

        $('#info_content').css('width', '100%');

        let $villageInfoContainer = makeOutputContainer();

        let fuckYouMessage = '';
        for (var i = 0; i < lockedDataReasons.length; i++) {
            if (fuckYouMessage.length && i != lockedDataReasons.length - 1) {
                fuckYouMessage += ', ';
            }
            if (i > 0 && i == lockedDataReasons.length - 1) {
                fuckYouMessage += ' and ';
            }
            fuckYouMessage += lockedDataReasons[i];
        }

        $villageInfoContainer.html(`
            <h3 style="padding:1em; text-align:center;margin:0">
                Upload your damn ${fuckYouMessage}!!
                <br>
                <br>
                (then refresh this page)
            </h3>
        `);
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

        function makeGroupOptions(currentId) {
            let result = [];

            groupStyles.forEach((grp) => {
                result.push(`<option value="${grp.id}" ${grp.id == currentId ? 'selected' : ''}> ${ grp.name }</option >`);
            });

            return result.join('\n');
        }

        let $container = $(`
            <div>
                <h4>Hover Settings</h4>
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
                <h4>Overlay Settings</h4>
                <p>
                    <input type="checkbox" id="vault-show-overlay" ${settings.showOverlay ? 'checked' : ''}>
                    <label for="vault-show-overlay">Show overlay</label>

                    <br><br>

                    <label for="vault-overlay-max-age">Ignore intel over </label>
                    <input id="vault-overlay-max-age" style="text-align:center;width:1.75em" value="${settings.maxIntelAgeDays}">
                    <label for="vault-overlay-max-age"> days old</label>

                    <br><br>

                    <select id="vault-overlay-highlight-method">
                        <option value="none" ${settings.overlayHighlights == "none" ? "selected" : ''}>None</option>
                        <option value="limited" ${settings.overlayHighlights == "limited" ? "selected" : ''}>Has group</option>
                        <option value="all" ${settings.overlayHighlights == "all" ? "selected" : ''}>Has intel</option>
                    </select>
                    <label for="vault-overlay-highlight-method">Highlights</label>

                    <select id="vault-overlay-nuke-group">
                        ${makeGroupOptions(settings.overlayNukeGroupId)}
                    </select>
                    <label for="vault-overlay-nuke-group">Nuke group</label>

                    <select id="vault-overlay-noble-group">
                        ${makeGroupOptions(settings.overlayNobleGroupId)}
                    </select>
                    <label for="vault-overlay-noble-group">Noble group</label>

                    <select id="vault-overlay-stacked-group">
                        ${makeGroupOptions(settings.overlayStackedGroupId)}
                    </select>
                    <label for="vault-overlay-stacked-group">Stacked group</label>
                </p>
            </div>
        `.trim());

        $container.find('label:not([for=vault-overlay-max-age])').css({
            'margin-right': '1.5em'
        });

        $('#content_value > h2').after($container);

        $container.find('#vault-show-commands').change(() => {
            let $checkbox = $container.find('#vault-show-commands');
            console.log('settings.showCommands = ' + $checkbox.prop('checked'));
            settings.showCommands = $checkbox.prop('checked');
            saveSettings(settings);
        });

        $container.find('#vault-show-recruits').change(() => {
            let $checkbox = $container.find('#vault-show-recruits');
            console.log('settings.showRecruits = ' + $checkbox.prop('checked'));
            settings.showPossiblyRecruited = $checkbox.prop('checked');
            saveSettings(settings);
        });

        $container.find('#vault-show-buildings').change(() => {
            let $checkbox = $container.find('#vault-show-buildings');
            console.log('settings.showBuildings = ' + $checkbox.prop('checked'));
            settings.showBuildings = $checkbox.prop('checked');
            saveSettings(settings);
        });

        $container.find('#vault-show-nukes').change(() => {
            let $checkbox = $container.find('#vault-show-nukes');
            console.log('settings.showNukes = ' + $checkbox.prop('checked'));
            settings.showNukes = $checkbox.prop('checked');
            saveSettings(settings);
        });

        $container.find('#vault-show-loyalty').change(() => {
            let $checkbox = $container.find('#vault-show-loyalty');
            console.log('settings.showLoyalty = ' + $checkbox.prop('checked'));
            settings.showLoyalty = $checkbox.prop('checked');
            saveSettings(settings);
        });

        $container.find('#vault-show-overlay').change(() => {
            settings.showOverlay = $container.find('#vault-show-overlay').prop('checked');
            saveSettings(settings);

            if (settings.showOverlay && mapOverlayTags) {
                applyMapOverlay();
            } else {
                $('*[id^=vault_overlay]').remove();
            }
        });

        $container.find('#vault-overlay-max-age').change(() => {
            let max = parseInt($container.find('#vault-overlay-max-age').val());
            if (isNaN(max) || max <= 0) {
                return;
            }
            settings.maxIntelAgeDays = max;
            saveSettings(settings);

            $('*[id^=vault_overlay]').remove();
            if (settings.showOverlay && mapOverlayTags)
                applyMapOverlay();
        });

        $container.find('#vault-overlay-highlight-method').change(() => {
            settings.overlayHighlights = $('#vault-overlay-highlight-method').val();
            saveSettings(settings);

            $('*[id^=vault_overlay]').remove();
            if (settings.showOverlay && mapOverlayTags)
                applyMapOverlay();
        });

        $container.find('#vault-overlay-nuke-group').change(() => {
            let value = $container.find('#vault-overlay-nuke-group').val();
            settings.overlayNukeGroupId = parseInt(value);
            groupMappings[TAG_TYPES.NUKE] = groupStylesById[value];
            saveSettings(settings);
            updateTagIcons(TAG_TYPES.NUKE);
        });

        $container.find('#vault-overlay-noble-group').change(() => {
            let value = $container.find('#vault-overlay-noble-group').val();
            settings.overlayNobleGroupId = parseInt(value);
            groupMappings[TAG_TYPES.NOBLES] = groupStylesById[value];
            saveSettings(settings);
            updateTagIcons(TAG_TYPES.NOBLES);
        });

        $container.find('#vault-overlay-stacked-group').change(() => {
            let value = $container.find('#vault-overlay-stacked-group').val();
            settings.overlayStackedGroupId = parseInt(value);
            groupMappings[TAG_TYPES.STACKED] = groupStylesById[value];
            saveSettings(settings);
            updateTagIcons(TAG_TYPES.STACKED);
        });
    }

    function loadSettings() {
        var settings = lib.getLocalStorage('map-settings') || {
            showCommands: true,
            showPossiblyRecruited: true,
            showBuildings: true,
            showNukes: true,
            showLoyalty: true
        };

        if (typeof settings.showOverlay != 'boolean')
            settings.showOverlay = true;

        if (!settings.maxIntelAgeDays)
            settings.maxIntelAgeDays = 5;

        if (!settings.overlayHighlights)
            settings.overlayHighlights = "limited";

        if (!settings.overlayNukeGroupId)
            settings.overlayNukeGroupId = groupStyles[0].id;

        if (!settings.overlayNobleGroupId)
            settings.overlayNobleGroupId = groupStyles[1 % groupStyles.length].id;

        if (!settings.overlayStackedGroupId)
            settings.overlayStackedGroupId = groupStyles[2 % groupStyles.length].id;

        saveSettings(settings);

        return settings;
    }

    function saveSettings(settings) {
        lib.setLocalStorage('map-settings', settings);
    }
}