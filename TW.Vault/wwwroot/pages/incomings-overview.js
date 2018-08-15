function parseIncomingsOverviewPage($doc, onProgress_, onDone_) {
    $doc = $doc || $(document);

    let $incomingRows = $doc.find('#incomings_table tr:not(:first-of-type):not(:last-of-type)');

    let commandsData = [];

    if (onProgress_)
        onProgress_('Collecting incomings...');

    $incomingRows.each((i, el) => {
        let $el = $(el);

        var troopImgTypeMatch = $el.find('td:nth-of-type(1) span:nth-of-type(2) img').prop('src');
        if (troopImgTypeMatch)
            troopImgTypeMatch = troopImgTypeMatch.match(/\/(\w+)\.png/);
        let troopType = troopImgTypeMatch ? troopImgTypeMatch[1] : null;

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
            isReturning: false
        });
    });

    console.log('Made commands data: ', commandsData);

    if (onProgress_)
        onProgress_('Uploading data...');

    lib.queryCurrentPlayerInfo((playerId) => {
        commandsData.forEach((data) => data.targetPlayerId = playerId);

        console.log('Filled commands with target player ID from current player: ', commandsData);

        lib.postApi(lib.makeApiUrl('command'), commandsData)
            .done(() => {
                $doc.find('input[name*=id_][type=checkbox]').prop('checked', true);

                if (onProgress_) {
                    onProgress_('Uploaded ' + commandsData.length + ' incomings.');
                }

                if (!onDone_)
                    alert('Uploaded commands!');
                else {
                    onDone_();
                }
            })
            .fail((req, status, err) => {
                if (onProgress_) {
                    onProgress_('An error occurred while uploading data.');
                }

                if (!onDone_) {
                    alert('An error occurred...');
                } else {
                    onDone_();
                }
                console.error('POST request failed: ', req, status, err);
            });
    });
}