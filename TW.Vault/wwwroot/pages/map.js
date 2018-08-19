function parseMapPage($doc) {
    $doc = $doc || $(document);

    if (window.ranVaultMap) {
        return;
    }

    window.ranVaultMap = true;
    var canUse = true;

    //  Hook into 'TWMap.displayForVillage', which is invoked whenever the village info popup is made
    //  by TW

    var currentVillageInfo = null;
    let $popup = $doc.find('#map_popup');

    $doc.find('#continent_id').parent().append('<span> - Using Vault</span>');

    var cachedData = {};
    let requestedVillageIds = [];

    let originalDisplayForVillage = TWMap.popup.displayForVillage;
    TWMap.popup.displayForVillage = function (e, a, t) {
        console.log('intercepted displayForVillage');
        originalDisplayForVillage.call(TWMap.popup, e, a, t);

        if (!canUse)
            return;

        let villageInfo = e;
        let screenx = a;
        let screeny = t;

        if (!villageInfo || !villageInfo.id)
            return;

        currentVillageInfo = villageInfo;

        let requestDelay = 350; // So we don't overload vault by user moving mouse quickly over map

        if (cachedData[villageInfo.id]) {
            makeOutput(cachedData[villageInfo.id]);
        } else {
            setTimeout(() => {
                if (currentVillageInfo != villageInfo) {
                    console.log('User changed target village, canceling request');
                    return;
                }

                if (requestedVillageIds.indexOf(villageInfo.id) >= 0) {
                    return;
                }

                requestedVillageIds.push(villageInfo.id);
                lib.getApi(lib.makeApiUrl(`village/${villageInfo.id}/army`))
                    .done((data) => {
                        console.log('Got village data: ', data);

                        data.stationedSeenAt = data.stationedSeenAt ? new Date(data.stationedSeenAt) : null;
                        data.recentlyLostArmySeenAt = data.recentlyLostArmySeenAt ? new Date(data.recentlyLostArmySeenAt) : null;
                        data.travelingSeenAt = data.travelingSeenAt ? new Date(data.travelingSeenAt) : null;
                        data.lastBuildingsSeenAt = data.lastBuildingsSeenAt ? new Date(data.lastBuildingsSeenAt) : null;
                        data.lastLoyaltySeenAt = data.lastLoyaltySeenAt ? new Date(data.lastLoyaltySeenAt) : null;

                        cachedData[villageInfo.id] = data;

                        if (villageInfo != currentVillageInfo) {
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
            }, requestDelay);
        }
    };

    console.log('Added map hook');

    function makeOutput(data) {
        if ($('#vault_info').length) {
            return;
        }

        //  Limit "own commands" to max 2
        let $commandRows = $('.command-row');
        if ($commandRows.length > 2) {
            for (var i = 2; i < $commandRows.length; i++) {
                $($commandRows[i]).remove();
            }
        }

        //  Remove intel rows
        $('#info_last_attack').closest('tr').remove();
        $('#info_last_attack_intel').closest('tr').remove();

        $('#info_content').css('width', '100%');
        
        let $villageInfoContainer = $('<div id="vault_info" style="background-color:#e5d7b2;">');
        $villageInfoContainer.appendTo($popup);

        //  NOTE - This assumes no archers!
        $villageInfoContainer.html(`
                    ${ !data.stationedArmy && !data.travelingArmy && !data.recentlyLostArmy ? '<div style="text-align:center;padding:0.5em;">No army data available.</div>' : `
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
                        ${ !data.nukesRequired ? '' : `
                        <tr>
                            <td colspan=12 style="text-align:center">Will take ~${data.nukesRequired} nukes to clear, ignoring morale</td>
                        </tr>
                        `}
                    </table>
                    `}
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
                    ${ typeof data.lastLoyalty == 'undefined' || data.lastLoyalty == null ? '' : `
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
}