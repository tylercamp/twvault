function parseUploadIncomingsOverviewPage($doc) {
    $doc = $doc || $(document);

    let $incomingRows = $doc.find('#incomings_table tr:not(:first-of-type):not(:last-of-type)');

    let commandsData = [];

    //  In matching priority
    let troopNames = [
        { name: 'ram', aliases: lib.translate(lib.itlcodes.UNIT_RAM_ALIASES).split(',').map(_ => _.trim()) },
        { name: 'catapult', aliases: lib.translate(lib.itlcodes.UNIT_CATAPULT_ALIASES).split(',').map(_ => _.trim()) },
        { name: 'spear', aliases: lib.translate(lib.itlcodes.UNIT_SPEAR_ALIASES).split(',').map(_ => _.trim()) },
        { name: 'sword', aliases: lib.translate(lib.itlcodes.UNIT_SWORD_ALIASES).split(',').map(_ => _.trim()) },
        { name: 'axe', aliases: lib.translate(lib.itlcodes.UNIT_AXE_ALIASES).split(',').map(_ => _.trim()) },
        { name: 'archer', aliases: lib.translate(lib.itlcodes.UNIT_ARCHER_ALIASES).split(',').map(_ => _.trim()) },
        { name: 'spy', aliases: lib.translate(lib.itlcodes.UNIT_SPY_ALIASES).split(',').map(_ => _.trim()) },
        { name: 'light', aliases: lib.translate(lib.itlcodes.UNIT_LIGHT_CAV_ALIASES).split(',').map(_ => _.trim()) },
        { name: 'marcher', aliases: lib.translate(lib.itlcodes.UNIT_M_ARCHER_ALIASES).split(',').map(_ => _.trim()) },
        { name: 'heavy', aliases: lib.translate(lib.itlcodes.UNIT_HEAVY_CAV_ALIASES).split(',').map(_ => _.trim()) },
        { name: 'snob', aliases: lib.translate(lib.itlcodes.UNIT_NOBLE_ALIASES).split(',').map(_ => _.trim()) },
        { name: 'knight', aliases: lib.translate(lib.itlcodes.UNIT_PALADIN_ALIASES).split(',').map(_ => _.trim()) }
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

        if (commandType.toLowerCase() != 'support' && commandType.toLowerCase() != 'attack') {
            console.warn(`Incoming ${commandId} has unknown command type "${commandType}", skipping`);
            return;
        }

        commandsData.push({
            commandId: parseInt(commandId),
            sourceVillageId: parseInt(sourceVillageId),
            sourcePlayerId: parseInt(sourcePlayerId),
            targetVillageId: parseInt(targetVillageId),
            targetPlayerId: null,
            landsAt: arrivalTime.toISOString(),
            commandType: commandType,
            troopType: troopType,
            isReturning: false,
            userLabel: label
        });
    });

    console.log('Made commands data: ', commandsData);
    return commandsData;
}