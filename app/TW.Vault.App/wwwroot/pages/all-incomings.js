function parseAllIncomings($doc, onProgress_, onDone_) {

    //# REQUIRE pages/incomings-overview.js

    $doc = $doc || $(document);

    if (!lib.detectGroups($doc).isAll) {
        onProgress_ && onProgress_(lib.messages.IS_IN_GROUP);

        if (onDone_)
            onDone_(lib.errorCodes.NOT_ALL_GROUP);
        else
            alert(lib.messages.IS_IN_GROUP, { _escaped: false });
        return;
    }

    var filtersEnabled = false;
    $doc.find('form[action*=save_filter] input[type=text]').each((i, el) => {
        let $el = $(el);
        if ($el.val().trim().length > 0) {
            filtersEnabled = true;
        }
    });

    if (filtersEnabled) {
        if (onProgress_)
            onProgress_(lib.escapeHtml(lib.messages.FILTER_APPLIED(lib.translate(lib.itlcodes.INCOMINGS))));
        else
            alert(lib.messages.FILTER_APPLIED(lib.translate(lib.itlcodes.INCOMINGS, { _escaped: false })));

        if (onDone_) {
            onDone_(lib.errorCodes.FILTER_APPLIED);
        }
        return;
    }

    let pages = lib.detectMultiPages($doc);
    pages.push(lib.makeTwUrl(lib.pageTypes.INCOMINGS_OVERVIEW));
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

    let collectPagesMessage = lib.translate(lib.itlcodes.INCOMINGS_COLLECTING_PAGES);
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

            let stats = requestManager.getStats();
            onProgress_ && onProgress_(`${collectPagesMessage} (${lib.translate(lib.itlcodes.INCOMINGS_PROGRESS, { numDone: stats.done, numTotal: pages.length, numFailed: stats.numFailed })})`);
            let pageIncomings = parseUploadIncomingsOverviewPage(lib.parseHtml(data));
            allIncomings.push(...pageIncomings);
        });
    });

    if (!requestManager.getStats().total) {
        lib.postApi('command/finished-incoming-uploads');
        onProgress_ && onProgress_(lib.translate(lib.itlcodes.INCOMINGS_NONE));
        if (onDone_)
            onDone_(false);
        else
            alert(lib.translate(lib.itlcodes.INCOMINGS_NONE));

        return;
    } else {
        requestManager.start();
    }

    requestManager.setFinishedHandler(() => {
        requestManager.stop();
        onProgress_ && onProgress_(lib.translate(lib.itlcodes.INCOMINGS_UPLOADING));

        lib.queryCurrentPlayerInfo((playerId) => {
            allIncomings.forEach((inc) => inc.targetPlayerId = playerId);
            var distinctIncomings = allIncomings.distinctBy((inc) => inc.commandId);

            console.log('From ' + allIncomings.length + ', made ' + distinctIncomings.length + ' distinct incomings');

            let data = {
                isOwnCommands: false,
                commands: distinctIncomings
            };
            lib.postApi('command', data)
                .done(() => {
                    $doc.find('input[name*=id_][type=checkbox]').prop('checked', true);

                    if (onProgress_) {
                        onProgress_(lib.translate(lib.itlcodes.INCOMINGS_FINISHED, { numIncomings: distinctIncomings.length }));
                    }

                    if (!onDone_)
                        alert(lib.translate(lib.itlcodes.INCOMINGS_FINISHED, { numIncomings: distinctIncomings.length, _escaped: false }));
                    else
                        onDone_();
                })
                .fail((req, status, err) => {
                    if (lib.isUnloading())
                        return;

                    if (onProgress_) {
                        onProgress_(lib.translate(lib.itlcodes.INCOMINGS_UPLOAD_ERROR));
                    }

                    if (!onDone_) {
                        alert(lib.messages.GENERIC_ERROR);
                    } else {
                        onDone_(true);
                    }
                    console.error('POST request failed: ', req, status, err);
                });

        });
    });

}