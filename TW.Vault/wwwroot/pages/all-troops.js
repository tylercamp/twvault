function parseAllTroops($doc, onProgress_, onDone_) {

    //# REQUIRE pages/troops-overview.js
    //# REQUIRE pages/troops-support-overview.js

    $doc = $doc || $(document);

    var requestManager = new RequestManager();
    let pages = lib.detectMultiPages($doc);
    pages.push(lib.makeTwUrl(lib.pageTypes.OWN_TROOPS_OVERVIEW));

    let troops = [];
    let supportData = [];

    let gettingPagesMessage = lib.translate(lib.itlcodes.TROOPS_COLLECTING_PAGES);
    onProgress_ && onProgress_(gettingPagesMessage);

    pages.forEach((link) => {
        requestManager.addRequest(link, (data) => {

            onProgress_ && onProgress_(`${gettingPagesMessage} (${requestManager.getStats().done}/${pages.length})`);

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

            let pageTroops = parseOwnTroopsOverviewPage($(data));
            troops.push(...pageTroops);
        });
    });

    requestManager.setFinishedHandler(() => {
        requestManager.stop();

        onProgress_ && onProgress_(lib.translate(lib.itlcodes.TROOPS_FIND_ACADEMY));

        findVillaWithAcademy((villageId) => {

            if (villageId < 0) {
                if (onProgress_) {
                    onProgress_ && onProgress_(lib.translate(lib.itlcodes.TROOPS_NO_ACADEMY));
                    setTimeout(() => uploadToVault(0), 1500);
                } else {
                    uploadToVault(0);
                }
            } else {
                onProgress_ && onProgress_(lib.translate(lib.itlcodes.TROOPS_FIND_POSSIBLE_NOBLES));
                getPossibleNobles(villageId, (cnt) => {
                    onProgress_ && onProgress_(lib.translate(lib.itlcodes.TROOPS_FIND_SUPPORT));
                    collectSupportData(() => {
                        uploadToVault(cnt);
                    });
                });
            }

        });

    });

    requestManager.start();

    function collectSupportData(onDone) {
        $.get(lib.makeTwUrl(lib.pageTypes.OWN_TROOPS_SUPPORTING_OVERVIEW))
            .done((data) => {
                onProgress_ && onProgress_(lib.translate(lib.itlcodes.TROOPS_COLLECTING_SUPPORT));
                let $supportDoc = lib.parseHtml(data);
                let supportPages = lib.detectMultiPages($supportDoc);
                let $supportPages = [];

                requestManager.resetStats();
                supportPages.forEach((link) => {
                    requestManager.addRequest(link, (data) => {
                        $supportPages.push(lib.parseHtml(data));
                    });
                });

                if (requestManager.getStats().total) {
                    requestManager.setFinishedHandler(parseAndFinish);
                    requestManager.start();
                } else {
                    $supportPages.push($supportDoc);
                    parseAndFinish();
                }

                function parseAndFinish() {
                    requestManager.stop();
                    $supportPages.forEach(($page, i) => {
                        console.log('Parsing page ' + i);
                        supportData.push(...parseTroopsSupportOverviewPage($page))
                    });
                    onDone();
                }
            });
    }

    function findVillaWithAcademy(onDone) {
        $.get(lib.makeTwUrl(lib.pageTypes.BUILDINGS_OVERVIEW))
            .done((data) => {
                let $doc = lib.parseHtml(data);
                var villaWithAcademy = null;
                $doc.find('.b_snob').each((i, el) => {
                    if (villaWithAcademy != null)
                        return;

                    let $el = $(el);
                    if ($el.text().trim() == 0)
                        return;

                    let $tr = $el.closest('tr');
                    let $smith = $tr.find('.b_smith');
                    if ($smith.text().trim() < 20)
                        return;

                    villaWithAcademy = $tr.prop('id').match(/v_(\d+)/)[1];
                });

                console.log('villaWithAcademy = ', villaWithAcademy);

                if (villaWithAcademy) {
                    onDone(villaWithAcademy);
                } else {
                    onDone(-1);
                }
            })
            .error(() => {
                if (lib.isUnloading())
                    return;

                if (onProgress_)
                    onProgress_(lib.translate(lib.itlcodes.TROOPS_ERROR_FINDING_ACADEMY));
                else
                    alert(lib.translate(lib.itlcodes.TROOPS_ERROR_FINDING_ACADEMY, { _escaped: false }));

                if (onDone_)
                    onDone_(false);
            });
    }

    function getPossibleNobles(villaIdWithAcademy, onDone) {
        $.get(lib.makeTwUrl(`village=${villaIdWithAcademy}&screen=snob`))
            .done((data) => {
                let docText = lib.parseHtml(data).text();
                let limit = docText.match(new RegExp(lib.translate(lib.itlcodes.TROOPS_NOBLES_LIMIT, { numNobles: String.raw`\s*(\d+)`, _escaped: false })));
                let current = docText.match(new RegExp(lib.translate(lib.itlcodes.TROOPS_NOBLES_NUM_VILLAGES, { numVillages: String.raw`\s*(\d+)`, _escaped: false })));

                console.log('Got limit: ', limit);
                console.log('Got current: ', current);

                if (limit && current) {
                    onDone(parseInt(limit[1]) - parseInt(current[1]));
                } else {
                    onDone(null);
                }
            })
            .error(() => {
                if (lib.isUnloading())
                    return;

                if (onProgress_)
                    onProgress_(lib.translate(lib.itlcodes.TROOPS_ERROR_GETTING_NOBLES));
                else
                    alert(lib.translate(lib.itlcodes.TROOPS_ERROR_GETTING_NOBLES, { _escaped: false }));

                if (onDone_)
                    onDone_(false);
            });
    }

    function uploadToVault(possibleNobles) {

        onProgress_ && onProgress_(lib.translate(lib.itlcodes.TROOPS_UPLOADING));

        let distinctTroops = troops.distinct((a, b) => a.villageId == b.villageId);

        let data = {
            troopData: distinctTroops,
            possibleNobles: possibleNobles
        };

        let onError = () => {
            if (lib.isUnloading())
                return;

            if (onProgress_)
                onProgress_(lib.translate(lib.itlcodes.UPLOAD_ERROR));

            if (!onDone_)
                alert(lib.messages.GENERIC_ERROR);
            else
                onDone_(true);
        };

        uploadArmy(() => {
            if (onProgress_)
                onProgress_(lib.translate(lib.itlcodes.TROOPS_FINISHED, { numVillages: distinctTroops.length }));

            if (!onDone_)
                alert('Done!')
            else
                onDone_(false);
        });

        function uploadArmy(onDone) {
            console.log('Uploading army data: ', data);
            lib.postApi('village/army/current', data)
                .error(onError)
                .done(() => {
                    onProgress_ && onProgress_(lib.translate(lib.itlcodes.TROOPS_UPLOADING_SUPPORT));
                    uploadSupport(onDone);
                });
        }

        function uploadSupport(onDone) {
            console.log('Uploading support data: ', supportData);
            lib.queryCurrentPlayerInfo((playerId) => {
                lib.postApi(`player/${playerId}/support`, supportData)
                    .error(onError)
                    .done(onDone);
            });
        }
    }
}