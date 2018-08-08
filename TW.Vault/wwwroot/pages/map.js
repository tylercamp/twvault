function parseMapPage($doc) {
    $doc = $doc || $(document);

    if (window.ranVaultMap) {
        return;
    }

    window.ranVaultMap = true;

    //  Hook into 'TWMap.displayForVillage', which is invoked whenever the village info popup is made
    //  by TW

    var currentVillageInfo = null;
    let $popup = $doc.find('#map_popup');

    $doc.find('#continent_id').parent().append('<span> - Using Vault</span>');

    var cachedData = {};

    let originalDisplayForVillage = TWMap.popup.displayForVillage;
    TWMap.popup.displayForVillage = function (e, a, t) {
        console.log('intercepted displayForVillage');
        originalDisplayForVillage.call(TWMap.popup, e, a, t);

        let villageInfo = e;
        let screenx = a;
        let screeny = t;

        currentVillageInfo = villageInfo;

        if (cachedData[villageInfo.id]) {
            makeOutput(cachedData[villageInfo.id]);
        } else {
            lib.getApi(lib.makeApiUrl(`village/${villageInfo.id}/army`))
                .done((data) => {
                    console.log('Got village data: ', data);

                    data.stationedSeenAt = data.stationedSeenAt ? new Date(data.stationedSeenAt) : null;
                    data.recentlyLostArmySeenAt = data.recentlyLostArmySeenAt ? new Date(data.recentlyLostArmySeenAt) : null;
                    data.travelingSeenAt = data.travelingSeenAt ? new Date(data.travelingSeenAt) : null;
                    data.lastBuildingsSeenAt = data.lastBuildingsSeenAt ? new Date(data.lastBuildingsSeenAt) : null;

                    cachedData[villageInfo.id] = data;

                    if (villageInfo != currentVillageInfo) {
                        return;
                    }

                    makeOutput(data);
                });
        }
    };

    console.log('Added map hook');

    function makeOutput(data) {
        if ($('#vault_info').length) {
            return;
        }
        
        let $villageInfoContainer = $('<div id="vault_info" style="background-color:#e5d7b2;">');
        $villageInfoContainer.appendTo($popup);

        //  NOTE - This assumes no archers!
        $villageInfoContainer.html(`
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
                        <tr>
                            <td>Stationed</td>
                            <td>${data.stationedSeenAt ? lib.formateDateTime(data.stationedSeenAt) : ''}</td>
                            ${makeTroopTds(data.stationedArmy || {})}
                        </tr>
                        <tr>
                            <td>Traveling</td>
                            <td>${data.travelingSeenAt ? lib.formateDateTime(data.travelingSeenAt) : ''}</td>
                            ${makeTroopTds(data.travelingArmy || {})}
                        </tr>
                        <tr>
                            <td>Recently lost</td>
                            <td>${data.recentlyLostArmySeenAt ? lib.formateDateTime(data.recentlyLostArmySeenAt) : ''}</td>
                            ${makeTroopTds(data.recentlyLostArmy || {})}
                        </tr>
                    </table>
                    <table class='vis' style="width:100%">
                        <tr style="background-color:#c1a264 !important">
                            <th>Vault</th>
                            <th>Seen at</th>
                            <th><img src="https://dsen.innogamescdn.com/8.136/37984/graphic/buildings/mid/snob1.png" title="Academy" alt="" class="bmain_list_img"></th>
                            <th><img src="https://dsen.innogamescdn.com/8.136/37984/graphic/buildings/mid/smith3.png" title="Smithy" alt="" class="bmain_list_img"></th>
                            <th><img src="https://dsen.innogamescdn.com/8.136/37984/graphic/buildings/mid/farm3.png" title="Farm" alt="" class="bmain_list_img"></th>
                            <th><img src="https://dsen.innogamescdn.com/8.136/37984/graphic/buildings/mid/wall1.png" title="Wall" alt="" class="bmain_list_img"></th>
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