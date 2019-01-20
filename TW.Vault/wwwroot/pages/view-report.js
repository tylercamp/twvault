
function parseReportPage($doc, href_, showNotice_, onError_) {

    //  Thresholds to prevent uploads of reports after being faked
    const minAttackerPopAsDefender = 100;
    
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

    // REPORT_LOYALTY_FROM_TO
    var loyalty = $doc.find('#attack_results tr').filter((i, el) => $(el).text().indexOf('Loyalty') >= 0).text().match(/from\s+\d+\s+to\s+(\-?\d+)/);
    if (loyalty)
        reportInfo.loyalty = parseInt(loyalty[1]);


    var occurredAt = $doc.find('td.nopad > .vis:nth-of-type(2) > tbody > tr:nth-of-type(2) td:nth-of-type(2)').text().trim();
    reportInfo.occurredAt = lib.parseTimeString(occurredAt).toISOString();

    //  Get attacker player
    if (attackingPlayer.length)
        reportInfo.attackingPlayerId = parseInt(attackingPlayer.prop('href').match(/id=(\w+)/)[1]);

    //  Get attacker village
    reportInfo.attackingVillageId = $attackInfo.find('a[href*=info_village]').prop('href');
    if (reportInfo.attackingVillageId) {
        reportInfo.attackingVillageId = parseInt(reportInfo.attackingVillageId.match(/id=(\w+)/)[1]);
    } else {
        // Village was deleted, can happen for guests that quit
        ignoreReport();
        if (showNotice_)
            alert("This report doesn't have an attacking village!");
        console.warn('Ignoring report with missing attacker village');
        return;
    }

    //  Get attacker units
    reportInfo.attackingArmy = $attackInfo.find('#attack_info_att_units tr:nth-of-type(2) .unit-item').get().map((el) => { return { type: $(el).prop('class').match(/unit-item-([\w\-]+)/)[1], count: parseInt($(el).text().trim()) } })

    //  Get attacker losses
    reportInfo.attackingArmyLosses = $attackInfo.find('#attack_info_att_units tr:nth-of-type(3) .unit-item').get().map((el) => { return { type: $(el).prop('class').match(/unit-item-([\w\-]+)/)[1], count: parseInt($(el).text().trim()) } })


    if (defendingPlayer.length)
        reportInfo.defendingPlayerId = parseInt(defendingPlayer.prop('href').match(/id=(\w+)/)[1]);

    //  Get defender village
    reportInfo.defendingVillageId = $defenseInfo.find('a[href*=info_village]').prop('href')
    if (reportInfo.defendingVillageId) {
        reportInfo.defendingVillageId = parseInt(reportInfo.defendingVillageId.match(/id=(\w+)/)[1]);
    } else {
        // Village was deleted, can happen for guests that quit
        ignoreReport();
        if (showNotice_)
            alert("This report doesn't have a defending village!");
        console.warn('Ignoring report with missing defender village');
        return;
    }

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

    reportInfo.buildingLevels = buildingsListToDictionary(reportInfo.buildingLevels);

    //  ram/cat damage
    if (reportInfo.buildingLevels == null) {
        var attack_results = null;
        if (attack_results = $doc.find('#attack_results').text()) {
            reportInfo.buildingLevels = {};
            // REPORT_BUILDING_DAMAGE_NAMES
            var building_names = attack_results.match(/The (.*) has/g);
            // REPORT_BUILDING_DAMAGE_LEVELS
            var building_levels = attack_results.match(/to level (.*)/g);
            if(building_names) {
                for (i=0; i < building_names.length; i++) {
                    reportInfo.buildingLevels[building_to_canonical_name[building_names[i].split(" ")[1]]] = parseInt(building_levels[i].split(" ")[2]);
                }
            }
        }
    }
    
    console.log('Made reportInfo: ', reportInfo);

    //  Ignore incomings that were fakes
    if (reportInfo.defendingPlayerId == lib.getCurrentPlayerId()) {
        let attackerPop = lib.twcalc.totalPopulation(reportInfo.attackingArmy);
        if (attackerPop < minAttackerPopAsDefender) {
            ignoreReport();
            if (showNotice_)
                alert("There's no useful info here!");
            console.warn('Ignoring incoming that was a fake');
            return;
        }
    }

    //  Ignore attacks on barbarian villages without any troops
    if (reportInfo.defendingPlayerId == null) {
        let defenderPop = lib.twcalc.totalPopulation(reportInfo.defendingArmy);
        if (defenderPop < 100) {
            ignoreReport();
            if (showNotice_)
                alert("There's no useful info here!");
            console.warn('Ignoring report for empty barb');
            return;
        }
    }

    function markReportAsSaved() {
        let reportsHistory = lib.getLocalStorage('reports-history', []);
        reportsHistory.push(reportInfo.reportId);
        lib.setLocalStorage('reports-history', reportsHistory);
    }

    function ignoreReport() {
        let id = reportInfo.reportId;
        lib.postApi('report/ignore', [id])
            .done(markReportAsSaved)
            .fail(() => {
                if (lib.isUnloading())
                    return;

                if (showNotice_)
                    // ERROR_OCCURRED
                    alert('An error occurred...');
                if (onError_)
                    onError_();
            });
    }

    lib.postApi('report', reportInfo)
        .done(() => {
            markReportAsSaved();
            if (showNotice_)
                alert('Uploaded the report!');
        })
        .fail((req, status, err) => {
            if (lib.isUnloading())
                return;

            if (showNotice_)
                alert('An error occurred...');
            console.error('POST request failed: ', req, status, err);
            if (onError_)
                onError_();
        });
}
