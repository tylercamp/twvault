function parseUploadIncomingsOverviewPage($doc) {
    $doc = $doc || $(document);

    let $incomingRows = $doc.find('#incomings_table tr:not(:first-of-type):not(:last-of-type)');

    let commandsData = [];

    //  In matching priority
    let troopNames = [
        { name: 'ram', aliases: ['rams'] },
        { name: 'catapult', aliases: ['catapults', 'cat.'] },
        { name: 'spear', aliases: [] },
        { name: 'sword', aliases: [] },
        { name: 'axe', aliases: [] },
        { name: 'archer', aliases: [] },
        { name: 'spy', aliases: ['scout'] },
        { name: 'light', aliases: [] },
        { name: 'marcher', aliases: ['mount archer'] },
        { name: 'heavy', aliases: [] },
        { name: 'snob', aliases: ['noble', 'nobleman'] },
        { name: 'knight', aliases: ['paladin', 'pally'] }
    ];

    $incomingRows.each((i, el) => {
        let $el = $(el);

        let label = $el.find('td:nth-of-type(1)').text().trim().toLowerCase();
        let troopType = (() => {            
            var type = null;

            troopNames.forEach((obj) => {
                let canonicalName = obj.name;
                let aliases = obj.aliases;

                [canonicalName, ...aliases].forEach((name) => {
                    if (type)
                        return;

                    if (label.contains(name)) {
                        type = canonicalName;
                    }
                });
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