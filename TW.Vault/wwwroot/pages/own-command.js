﻿function parseOwnCommand(commandId, commandType, isReturning, $doc) {
    let $container = $doc.find('#content_value');
    let sourcePlayerId = $doc.find('#content_value .vis:nth-of-type(1) tr:nth-of-type(2) td:nth-of-type(3) a').prop('href').match(/id=(\w+)/)[1];
    let sourceVillageId = $doc.find('#content_value .vis:nth-of-type(1) tr:nth-of-type(3) td:nth-of-type(2) a').prop('href').match(/id=(\w+)/)[1];
    var targetPlayerId = $doc.find('#content_value .vis:nth-of-type(1) tr:nth-of-type(4) td:nth-of-type(3) a').prop('href');
    let targetVillageId = $doc.find('#content_value .vis:nth-of-type(1) tr:nth-of-type(5) td:nth-of-type(2) a').prop('href').match(/id=(\w+)/)[1];

    if (targetPlayerId) {
        targetPlayerId = targetPlayerId.match(/id=(\w+)/);
        if (targetPlayerId)
            targetPlayerId = targetPlayerId[1];
        else
            targetPlayerId = null;
    } else {
        targetPlayerId = null;
    }

    let hasCatapult = $container.text().contains("Catapult");
    let landsAtSelector = hasCatapult
        ? '#content_value .vis:nth-of-type(1) tr:nth-of-type(8) td:nth-of-type(2)'
        : '#content_value .vis:nth-of-type(1) tr:nth-of-type(7) td:nth-of-type(2)';
    let landsAt = lib.parseTimeString($doc.find(landsAtSelector).text());

    var troopCounts = {};
    let $troopCountEntries = $container.find('.unit-item');
    $troopCountEntries.each((i, el) => {
        let $el = $(el);
        let cls = $el.prop('class');
        let troopType = cls.match(/unit\-item\-(\w+)/)[1];
        let count = parseInt($el.text().trim());

        troopCounts[troopType] = count;
    });

    let command = {
        commandId: commandId,
        sourcePlayerId: parseInt(sourcePlayerId),
        sourceVillageId: parseInt(sourceVillageId),
        targetPlayerId: parseInt(targetPlayerId),
        targetVillageId: parseInt(targetVillageId),
        landsAt: landsAt.toUTCString(),
        troops: troopCounts,
        commandType: commandType,
        isReturning: isReturning
    };

    return command;
}