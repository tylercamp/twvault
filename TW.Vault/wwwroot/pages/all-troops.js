function parseAllTroops($doc, onProgress_, onDone_) {

    //# REQUIRE pages/troops-overview.js
    //# REQUIRE pages/troops-support-overview.js

    $doc = $doc || $(document);

    var requestManager = new RequestManager();
    let pages = lib.detectMultiPages($doc);
    pages.push(lib.makeTwUrl(lib.pageTypes.OWN_TROOPS_OVERVIEW));

    let troops = [];
    let supportData = [];

    let gettingPagesMessage = 'Getting village troop pages...';
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

        onProgress_ && onProgress_('Finding village with academy...');

        findVillaWithAcademy((villageId) => {

            if (villageId < 0) {
                if (onProgress_) {
                    onProgress_ && onProgress_('(No village with academy found)');
                    setTimeout(() => uploadToVault(0), 3000);
                } else {
                    uploadToVault(0);
                }
            } else {
                onProgress_ && onProgress_('Getting possible nobles...');
                getPossibleNobles(villageId, (cnt) => {
                    onProgress_ && onProgress_('Getting support...');
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
                onProgress_ && onProgress_('Collecting supported villages and DVs...');
                let $supportDoc = $(data);
                let supportPages = lib.detectMultiPages($supportDoc);
                let $supportPages = [];

                requestManager.resetStats();
                supportPages.forEach((link) => {
                    requestManager.addRequest(link, (data) => {
                        $supportPages.push($(data));
                    });
                });

                if (requestManager.getStats().total) {
                    requestManager.setFinishedHandler(parseAndFinish);
                    requestManager.start();
                } else {
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
                let $doc = $(data);
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
                if (onProgress_)
                    onProgress_('An error occurred while finding villa with academy...');
                else
                    alert('An error occurred while finding villa with academy...');

                if (onDone_)
                    onDone_(false);
            });
    }

    function getPossibleNobles(villaIdWithAcademy, onDone) {
        $.get(`/game.php?village=${villaIdWithAcademy}&screen=snob`)
            .done((data) => {
                let docText = $(data).text();
                let limit = docText.match(/Noblemen\s+limit:\s*(\d+)/);
                let current = docText.match(/Number\s+of\s+conquered\s+villages:\s*(\d+)/);

                console.log('Got limit: ', limit);
                console.log('Got current: ', current);

                if (limit && current) {
                    onDone(parseInt(limit[1]) - parseInt(current[1]));
                } else {
                    onDone(null);
                }
            })
            .error(() => {
                if (onProgress_)
                    onProgress_('An error occurred while getting possible noble counts...');
                else
                    alert('An error occurred while getting possible noble counts...');

                if (onDone_)
                    onDone_(false);
            });
    }

    function uploadToVault(possibleNobles) {

        onProgress_ && onProgress_("Uploading troops to vault...");

        let distinctTroops = troops.distinct((a, b) => a.villageId == b.villageId);

        let data = {
            troopData: distinctTroops,
            possibleNobles: possibleNobles
        };

        let onError = () => {
            if (onProgress_)
                onProgress_("An error occurred while uploading to the vault.");

            if (!onDone_)
                alert('An error occurred...')
            else
                onDone_(true);
        };

        uploadArmy(() => {
            if (onProgress_)
                onProgress_('Finished: Uploaded troops for ' + distinctTroops.length + ' villages.');

            if (!onDone_)
                alert('Done!')
            else
                onDone_(false);
        });

        function uploadArmy(onDone) {
            console.log('Uploading army data: ', data);
            lib.postApi(lib.makeApiUrl('village/army/current'), data)
                .error(onError)
                .done(() => {
                    onProgress_ && onProgress_('Uploading support to vault...');
                    uploadSupport(onDone);
                });
        }

        function uploadSupport(onDone) {
            console.log('Uploading support data: ', supportData);
            lib.queryCurrentPlayerInfo((playerId) => {
                lib.postApi(lib.makeApiUrl(`player/${playerId}/support`), supportData)
                    .error(onError)
                    .done(onDone);
            });
        }
    }
}