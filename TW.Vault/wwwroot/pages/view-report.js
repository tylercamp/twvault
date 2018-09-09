
function parseReportPage($doc, href_, showNotice_, onError_) {
    
    lib.ensurePage(lib.pageTypes.VIEW_REPORT);
    $doc = $doc || $(document);
    let href = href_ || window.location.href;
    if (typeof showNotice_ == 'undefined')
        showNotice_ = true; // Show "complete/error" notice by default

    var $attackInfo = $doc.find('#attack_info_att')
    var $defenseInfo = $doc.find('#attack_info_def')
    var defendingPlayer = $defenseInfo.find('a[href*=info_player]');
    var attackingPlayer = $attackInfo.find('a[href*=info_player]');
    var building_to_canonical_name = {"Headquarters":"main", "Barracks":"barracks", "Stable":"stable", "Workshop":"garage", "Academy":"snob", "Smithy":"smith", "Rally Point":"place", "Statue":"statue", "Market":"market", "Timber camp":"wood", "Clay pit":"stone", "Iron mine":"iron", "Farm":"farm", "Warehouse":"storage", "Hiding place":"hide", "Wall":"wall", "Watchtower":"watchtower", "Church":"church", "First church":"first_church"}; //not sure about Watchtower and Church entries
    var reportInfo = {};
    reportInfo.reportId = parseInt(href.match(/view=(\d+)/)[1]);
    console.log('Processing report ' + reportInfo.reportId);

    if (!$attackInfo.length) {
        if (showNotice_) {
            alert("This kind of report can't be uploaded!");
        }
        return;
    }
    
    reportInfo.luck = parseInt($doc.find('#attack_luck').text().match(/(\-?\d.+)\%/)[1]);
    reportInfo.morale = parseInt($doc.find('.report_ReportAttack h4:nth-of-type(2)').text().match(/(\d+)\%/)[1]);

    var loyalty = $doc.find('#attack_results tr').filter((i, el) => $(el).text().indexOf('Loyalty') >= 0).text().match(/from\s+\d+\s+to\s+(\-?\d+)/);
    if (loyalty)
        reportInfo.loyalty = parseInt(loyalty[1]);


    var occurredAt = $doc.find('td.nopad > .vis:nth-of-type(2) > tbody > tr:nth-of-type(2) td:nth-of-type(2)').text().trim();
    reportInfo.occurredAt = lib.parseTimeString(occurredAt).toUTCString();

    //  Get attacker player
    if (attackingPlayer.length)
        reportInfo.attackingPlayerId = parseInt(attackingPlayer.prop('href').match(/id=(\w+)/)[1]);

    //  Get attacker village
    reportInfo.attackingVillageId = parseInt($attackInfo.find('a[href*=info_village]').prop('href').match(/id=(\w+)/)[1])

    //  Get attacker units
    reportInfo.attackingArmy = $attackInfo.find('#attack_info_att_units tr:nth-of-type(2) .unit-item').get().map((el) => { return { type: $(el).prop('class').match(/unit-item-([\w\-]+)/)[1], count: parseInt($(el).text().trim()) } })

    //  Get attacker losses
    reportInfo.attackingArmyLosses = $attackInfo.find('#attack_info_att_units tr:nth-of-type(3) .unit-item').get().map((el) => { return { type: $(el).prop('class').match(/unit-item-([\w\-]+)/)[1], count: parseInt($(el).text().trim()) } })


    if (defendingPlayer.length)
        reportInfo.defendingPlayerId = parseInt(defendingPlayer.prop('href').match(/id=(\w+)/)[1]);

    //  Get defender village
    reportInfo.defendingVillageId = parseInt($defenseInfo.find('a[href*=info_village]').prop('href').match(/id=(\w+)/)[1])

    //  Get defender units
    reportInfo.defendingArmy = $defenseInfo.find('#attack_info_def_units tr:nth-of-type(2) .unit-item').get().map((el) => { return { type: $(el).prop('class').match(/unit-item-([\w\-]+)/)[1], count: parseInt($(el).text().trim()) } })

    //  Get defender losses
    reportInfo.defendingArmyLosses = $defenseInfo.find('#attack_info_def_units tr:nth-of-type(3) .unit-item').get().map((el) => { return { type: $(el).prop('class').match(/unit-item-([\w\-]+)/)[1], count: parseInt($(el).text().trim()) } })

    let $travelingTroopsContainer = $doc.find('#attack_spy_away');
    if ($travelingTroopsContainer.length) {
        reportInfo.travelingTroops = {};

        $travelingTroopsContainer.find('.unit-item').each((i, el) => {
            let $el = $(el);
            let cls = $el.prop('class');
            let unitType = cls.match(/unit\-item\-(\w+)/)[1];
            reportInfo.travelingTroops[unitType] = parseInt($el.text().trim());
        });
    }

    //  Defender village info
    reportInfo.buildingLevels = JSON.parse($doc.find('#attack_spy_building_data').val() || 'null')


    function itemListToDictionary(list, nameSelector, valueSelector) {
        if (list == null || list.length == 0)
            return null;

        var result = {};
        list.forEach(v => result[nameSelector(v)] = valueSelector(v));
        return result;
    }

    let troopListToDictionary = (troopList) => itemListToDictionary(troopList, (t) => t.type, (t) => t.count);
    let buildingsListToDictionary = (buildingsList) => itemListToDictionary(buildingsList, (b) => b.id, b => b.level);

    reportInfo.attackingArmy = troopListToDictionary(reportInfo.attackingArmy);
    reportInfo.attackingArmyLosses = troopListToDictionary(reportInfo.attackingArmyLosses);
    reportInfo.defendingArmy = troopListToDictionary(reportInfo.defendingArmy);
    reportInfo.defendingArmyLosses = troopListToDictionary(reportInfo.defendingArmyLosses);

    // TODO reportInfo.travelingTroops = troopListToDictionary(reportInfo.travelingTroops);

    reportInfo.buildingLevels = buildingsListToDictionary(reportInfo.buildingLevels);



    //  ram/cat damage
    if (reportInfo.buildingLevels == null) {
        var attack_results = null;
        if (attack_results = $doc.find('#attack_results').text()) {
            reportInfo.buildingLevels = {};
            var building_names = attack_results.match(/The (.*) has/g);
            var building_levels = attack_results.match(/to level (.*)/g);
            if(building_names) {
                for (i=0; i < building_names.length; i++) {
                    reportInfo.buildingLevels[building_to_canonical_name[building_names[i].split(" ")[1]]] = parseInt(building_levels[i].split(" ")[2]);
                }
            }
        }
    }
    

    let reportsEndpoint = lib.makeApiUrl('/report');
    console.log('Made reportInfo: ', reportInfo);

    lib.postApi(reportsEndpoint, reportInfo)
        .done(() => {
            var reportsHistory = lib.getLocalStorage('reports-history', []);
            reportsHistory.push(reportInfo.reportId);
            lib.setLocalStorage('reports-history', reportsHistory);

            if (showNotice_)
                alert('Uploaded the report!');
        })
        .fail((req, status, err) => {
            if (showNotice_)
                alert('An error occurred...');
            console.error('POST request failed: ', req, status, err);
            if (onError_)
                onError_();
        });
}
