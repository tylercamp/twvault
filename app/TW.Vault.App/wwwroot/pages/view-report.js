
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
    var building_to_canonical_name = {};
    building_to_canonical_name[lib.translate(lib.itlcodes.BUILDING_ACADEMY)] = 'snob';
    building_to_canonical_name[lib.translate(lib.itlcodes.BUILDING_BARRACKS)] = 'barracks';
    building_to_canonical_name[lib.translate(lib.itlcodes.BUILDING_CHURCH)] = 'church';
    building_to_canonical_name[lib.translate(lib.itlcodes.BUILDING_CLAY_PIT)] = 'stone';
    building_to_canonical_name[lib.translate(lib.itlcodes.BUILDING_FARM)] = 'farm';
    building_to_canonical_name[lib.translate(lib.itlcodes.BUILDING_HIDING_PLACE)] = 'hide';
    building_to_canonical_name[lib.translate(lib.itlcodes.BUILDING_HQ)] = 'main';
    building_to_canonical_name[lib.translate(lib.itlcodes.BUILDING_IRON_MINE)] = 'iron';
    building_to_canonical_name[lib.translate(lib.itlcodes.BUILDING_MARKET)] = 'market';
    building_to_canonical_name[lib.translate(lib.itlcodes.BUILDING_RALLY_POINT)] = 'place';
    building_to_canonical_name[lib.translate(lib.itlcodes.BUILDING_SMITHY)] = 'smith';
    building_to_canonical_name[lib.translate(lib.itlcodes.BUILDING_STABLE)] = 'stable';
    building_to_canonical_name[lib.translate(lib.itlcodes.BUILDING_STATUE)] = 'statue';
    building_to_canonical_name[lib.translate(lib.itlcodes.BUILDING_TIMBER_CAMP)] = 'wood';
    building_to_canonical_name[lib.translate(lib.itlcodes.BUILDING_WALL)] = 'wall';
    building_to_canonical_name[lib.translate(lib.itlcodes.BUILDING_WAREHOUSE)] = 'storage';
    building_to_canonical_name[lib.translate(lib.itlcodes.BUILDING_WATCHTOWER)] = 'watchtower';
    building_to_canonical_name[lib.translate(lib.itlcodes.BUILDING_WORKSHOP)] = 'garage';

    var reportInfo = {};
    reportInfo.reportId = parseInt(href.match(/view=(\d+)/)[1]);
    console.log('Processing report ' + reportInfo.reportId);

    if (!$attackInfo.length) {
        console.warn(`Report ${reportInfo.reportId} ignored - unsupported report type`)
        if (showNotice_) {
            alert("This kind of report can't be uploaded!");
        }
        ignoreReport();
        return;
    }
    
    reportInfo.luck = parseInt($doc.find('#attack_luck').text().match(/(\-?\d.+)\%/)[1]);
    var morale = $doc.find('.report_ReportAttack h4:nth-of-type(2)').text().match(/(\d+)\%/);
    if (morale) {
        reportInfo.morale = parseInt(morale[1]);
    } else {
        reportInfo.morale = 100;
    }

    let loyaltyRegex = lib.translate(lib.itlcodes.REPORT_LOYALTY_FROM_TO, { oldLoyalty: String.raw`\d+`, newLoyalty: String.raw`(\-?\d+)` });
    var loyalty = $doc.find('#attack_results tr').filter((i, el) => $(el).text().toLowerCase().indexOf(lib.translate(lib.itlcodes.LOYALTY).toLowerCase()) >= 0).text().match(new RegExp(loyaltyRegex));
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
            let wallMatcher = new RegExp(lib.translate(lib.itlcodes.REPORT_WALL_DAMAGE, { oldLevel: String.raw`\d+`, newLevel: String.raw`(\d+)`, _escaped: false }))
            let wallMatch = attack_results.match(wallMatcher);
            if (wallMatch) {
                let newLevel = wallMatch[1];
                reportInfo.buildingLevels.wall = parseInt(newLevel);
            }

            let buildingMatcher = new RegExp(lib.translate(lib.itlcodes.REPORT_BUILDING_DAMAGE, { buildingName: '(.*)', oldLevel: String.raw`\d+`, newLevel: String.raw`(\d+)`, _escaped: false }), 'g');
            let buildings = [];

            let match = null;
            while (match = buildingMatcher.exec(attack_results))
                buildings.push({ name: match[1], newLevel: match[2] });

            buildings.forEach((b) => {
                reportInfo.buildingLevels[building_to_canonical_name[b.name]] = parseInt(b.newLevel);
            });
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
                    alert(lib.messages.GENERIC_ERROR);
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
                alert(lib.messages.GENERIC_ERROR);
            console.error('POST request failed: ', req, status, err, reportInfo);
            if (onError_)
                onError_();
        });
}
