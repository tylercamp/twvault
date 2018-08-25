function parseAllIncomings($doc, onProgress_, onDone_) {

    //# REQUIRE pages/incomings-overview.js

    $doc = $doc || $(document);

    if (!lib.detectGroups($doc).isAll) {
        onProgress_ && onProgress_(lib.messages.IS_IN_GROUP);

        if (onDone_)
            onDone_(lib.errorCodes.NOT_ALL_GROUP);
        else
            alert(lib.messages.IS_IN_GROUP);
        return;
    }

    let pages = lib.detectMultiPages($doc);
    console.log('Got incomings pages: ', pages);

    let groups = lib.detectGroups($doc);
    if (!groups.isAll) {
        onProgress_ && onProgress_(lib.messages.IS_IN_GROUP);

        if (onDone_)
            onDone_(true);
        else
            alert(lib.messages.IS_IN_GROUP);
        return;
    }

    var pageContents = [];
    let requestManager = new RequestManager();

    let collectPagesMessage = 'Collecting incoming pages...';
    onProgress_ && onProgress_(collectPagesMessage);

    let allIncomings = [];

    pages.forEach((link) => {
        requestManager.addRequest(link, (data) => {
            if (lib.checkContainsCaptcha(data)) {
                if (requestManager.isRunning()) {
                    onProgress_ && onProgress_(lib.messages.TRIGGERED_CAPTCHA);

                    if (onDone_)
                        onDone_(lib.errorCodes.CAPTCHA);
                    else
                        alert(lib.messages.TRIGGERED_CAPTCHA);

                    requestManager.stop();
                }

                return;
            }

            onProgress_ && onProgress_(`${collectPagesMessage} (${requestManager.getStats().done}/${pages.length} done, ${requestManager.getStats().numFailed} failed)`);
            let pageIncomings = parseUploadIncomingsOverviewPage($(data));
            allIncomings.push(...pageIncomings);
        });
    });

    if (!requestManager.getStats().total) {
        onProgress_ && onProgress_('No incomings to upload.');
        if (onDone_)
            onDone_(false);
        else
            alert('No incomings to upload.');

        return;
    } else {
        requestManager.start();
    }

    requestManager.setFinishedHandler(() => {
        onProgress_ && onProgress_('Uploading incomings...');

        lib.queryCurrentPlayerInfo((playerId) => {
            allIncomings.forEach((inc) => inc.targetPlayerId = playerId);
            var distinctIncomings = allIncomings.distinct((a, b) => {
                return JSON.stringify(a) == JSON.stringify(b);
            });

            console.log('From ' + allIncomings.length + ', made ' + distinctIncomings.length + ' distinct incomings');

            let data = {
                isOwnCommands: false,
                commands: distinctIncomings
            };
            lib.postApi(lib.makeApiUrl('command'), data)
                .done(() => {
                    $doc.find('input[name*=id_][type=checkbox]').prop('checked', true);

                    if (onProgress_) {
                        onProgress_('Finished: Uploaded ' + distinctIncomings.length + ' incomings.');
                    }

                    if (!onDone_)
                        alert('Uploaded commands!');
                    else
                        onDone_();
                })
                .fail((req, status, err) => {
                    if (onProgress_) {
                        onProgress_('An error occurred while uploading data.');
                    }

                    if (!onDone_) {
                        alert('An error occurred...');
                    } else {
                        onDone_(true);
                    }
                    console.error('POST request failed: ', req, status, err);
                });

        });
    });

}