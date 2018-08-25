function parseUploadIncomingsOverviewPage($doc) {
    $doc = $doc || $(document);

    let $incomingRows = $doc.find('#incomings_table tr:not(:first-of-type):not(:last-of-type)');

    let commandsData = [];

    //  In matching priority
    let troopNames = [
        'ram', 'catapult',
        'spear', 'sword', 'axe', 'archer',
        'spy', 'light', 'marcher', 'heavy',
        'snob', 'knight'
    ];

    $incomingRows.each((i, el) => {
        let $el = $(el);

        let label = $el.find('td:nth-of-type(1)').text().trim().toLowerCase();
        let troopType = (() => {            
            var type = null;

            troopNames.forEach((name) => {
                if (type)
                    return;

                let unitData = lib.twstats.getUnit(name);
                let aliases = unitData.aliases;

                aliases.forEach((a) => !type && label.contains(a) ? type = name : null);
            });

            return type;
        })();

        var commandType = $el.find('td:nth-of-type(1) span:nth-of-type(1) img').prop('src').match(/\/(\w+)\.png/)[1];
        let commandId = $el.find('input[name^=id_]').prop('name').match(/id_(\w+)/)[1];
        let targetVillageId = $el.find('td:nth-of-type(2) a').prop('href').match(/village\=(\w+)/)[1];
        let sourceVillageId = $el.find('td:nth-of-type(3) a').prop('href').match(/\&id\=(\w+)/)[1];
        let sourcePlayerId = $el.find('td:nth-of-type(4) a').prop('href').match(/\&id\=(\w+)/)[1];
        let arrivalTimeText = $el.find('td:nth-of-type(6)').text().trim();

        let arrivalTime = lib.parseTimeString(arrivalTimeText);

        if (commandType.contains("_"))
            commandType = commandType.substr(0, commandType.indexOf('_'));

        commandsData.push({
            commandId: parseInt(commandId),
            sourceVillageId: parseInt(sourceVillageId),
            sourcePlayerId: parseInt(sourcePlayerId),
            targetVillageId: parseInt(targetVillageId),
            targetPlayerId: null,
            landsAt: arrivalTime.toUTCString(),
            commandType: commandType,
            troopType: troopType,
            isReturning: false,
            userLabel: label
        });
    });

    console.log('Made commands data: ', commandsData);
    return commandsData;
}