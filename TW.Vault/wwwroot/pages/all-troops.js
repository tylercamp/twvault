function parseAllTroops($doc, onProgress_, onDone_) {

    //# REQUIRE pages/troops-overview.js

    $doc = $doc || $(document);

    var requestManager = new RequestManager();
    let pages = lib.detectMultiPages($doc);

    let troops = [];

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
                    uploadToVault(cnt);
                });
            }

        });

    });

    requestManager.start();

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

        onProgress_ && onProgress_("Uploading to vault...");

        let data = {
            troopData: troops,
            possibleNobles: possibleNobles
        };

        lib.postApi(lib.makeApiUrl('village/army/current'), data)
            .done(() => {
                if (onProgress_)
                    onProgress_('Finished: Uploaded troops for ' + troops.length + ' villages.');

                if (!onDone_)
                    alert('Done!')
                else
                    onDone_(false);
            })
            .error(() => {
                if (onProgress_)
                    onProgress_("An error occurred while uploading to the vault.");

                if (!onDone_)
                    alert('An error occurred...')
                else
                    onDone_(true);
            });
    }
}