function parseTagIncomingsOverviewPage($doc) {
    $doc = $doc || $(document);

    var numTotal = 0;
    var numPending = 0;
    var numMissing = 0;
    var numError = 0;
    var numTagged = 0;

    //  TODO - Make this an option; whether or not to re-tag incomings that were previously tagged by vault
    var retagAll = true;

    $('.rename-icon').click();

    let $incomingRows = $doc.find('#incomings_table tr:not(:first-of-type):not(:last-of-type)');
    $incomingRows.each((i, row) => {

        numTotal++;
        numPending++;

        let $row = $(row);
        let commandId = $row.find('input[name^=id_]').prop('name').match(/id_(\w+)/)[1];

        let requestUrl = lib.makeApiUrl(`command/${commandId}/tags`);
        setTimeout(() => {

            lib.getApi(requestUrl)
                .done((data) => {
                    if (typeof data == 'string')
                        data = lib.jsonParse(data);

                    let numFromVillage = data.numFromVillage;
                    let definiteFake = data.definiteFake;
                    let troopType = data.troopType;
                    let troopName = lib.twstats.getUnitCommonName(troopType);
                    let pop = data.offensivePopulation;
                    let cats = data.numCats;

                    var tagType = null;
                    if (definiteFake) {
                        tagType = 'Fake';
                    } else if (cats > 40 && pop < 2000) {
                        tagType = 'Fang?';
                    } else {
                        tagType = tagType || 'Nuke?';
                    }

                    var popString = pop ? Math.min(100, Math.round(pop / 20000 * 100)) : '?';
                    var catsString = cats != null ? cats : '?';

                    var label = `${troopName} ${tagType} Pop: ${popString}% Cats: ${catsString} Com: 1/${numFromVillage}`;
                    console.log('Label for ' + commandId + ': ' + label);

                    let $textbox = $row.find('input[type=text]');
                    let $okButton = $row.find('input[type=button]');

                    $textbox.val(label);
                    $okButton.click();

                    numPending--;
                    if (numPending == 0) {
                        onTaggingDone();
                    }
                })
                .error((xhr) => {
                    if (xhr.status == 404) {
                        numMissing++;
                    } else {
                        numError++;
                    }

                    numPending--;
                    if (numPending == 0) {
                        onTaggingDone();
                    }
                });

        }, i * 10); // Request at different intervals so the requests don't get sent literally all at once
    });

    function onTaggingDone() {
    }
}