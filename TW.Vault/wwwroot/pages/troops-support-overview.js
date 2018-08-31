function parseTroopsSupportOverviewPage($doc) {
    $doc = $doc || $(document);

    let supportRecords = [];
    var currentRecord = null;
    $doc.find('#units_table tbody tr:not(:last-of-type)').each((i, el) => {
        let $tr = $(el);
        let classes = $tr.prop('class').trim().split(' ');

        if (classes.contains('units_away')) {
            if (currentRecord)
                supportRecords.push(currentRecord);

            currentRecord = {
                sourceVillageId: $tr.find('td:first-of-type a').prop('href').match(/village=(\d+)/)[1],
                supportedVillages: []
            };
        } else if (classes.contains('row_a') || classes.contains('row_b')) {
            let troopCounts = [];
            $tr.find('.unit-item').each((_, el) => troopCounts.push(parseInt($(el).text())));

            let targetVillageLink = $tr.find('td:first-of-type a:nth-of-type(1)').prop('href');
            let supportedVillage = {
                id: (targetVillageLink.match(/id=(\d+)/) || targetVillageLink.match(/village=(\d+)/))[1],
                troopCounts: lib.troopsArrayToObject(troopCounts)
            };

            currentRecord.supportedVillages.push(supportedVillage);
        }
    });

    if (currentRecord)
        supportRecords.push(currentRecord);

    supportRecords.forEach((record) => {
        record.sourceVillageId = parseInt(record.sourceVillageId);
        record.supportedVillages.forEach((s) => {
            s.id = parseInt(s.id);
        });
    });

    console.log('Made supportRecords = ', supportRecords);
    return supportRecords;
}