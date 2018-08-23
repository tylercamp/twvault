function parseOwnCommandsOverviewPage($doc, onProgress_, onDone_) {
    $doc = $doc || $(document);

    var requestManager = new RequestManager();
    let $commandLinks = $doc.find('#commands_table tr:not(:first-of-type):not(:last-of-type) td:first-of-type a:not(.rename-icon)');

    let oldCommands = JSON.parse(localStorage.getItem('vault-commands-history') || '[]');
    let resultCommands = [];

    $commandLinks.each((i, el) => {
        let link = $(el).prop('href');
        let $td = $(el).closest('td');
        var commandState = $td.find('img:first-of-type').prop('src').match(/(\w+)\.png/)[1];

        let commandType = commandState.contains("attack") ? "attack" : "support";
        let isReturning = commandState.contains("return") || commandState.contains("back");

        let commandId = parseInt(link.match(/id\=(\w+)/)[1]);
        if (oldCommands.indexOf(commandId) >= 0) {
            return;
        }

        requestManager.addRequest(link, (data, request) => {

            if (lib.checkContainsCaptcha(data)) {
                if (requestManager.isRunning()) {
                    let captchaMessage = 'Tribal wars Captcha was triggered, please refresh the page and try again.';
                    if (onProgress_)
                        onProgress_(captchaMessage);

                    if (onDone_)
                        onDone_("captcha");
                    else
                        alert(captchaMessage);
                }
                return;
            }

            let $doc = $(data);
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

            console.log('Made command: ', command);

            updateUploadsDisplay();
            resultCommands.push(command);

            oldCommands.push(command.commandId);
        });
    });

    if (!$commandLinks.length) {
        if (onProgress_) {
            onProgress_('No commands to upload.');
        }

        if (!onDone_)
            alert('No commands to upload!');
        else
            onDone_(false);

        lib.postApi(lib.makeApiUrl('command'), {
            isOwnCommands: true,
            commands: []
        });
        return;
    }

    requestManager.setFinishedHandler(() => {
        let data = {
            isOwnCommands: true,
            commands: resultCommands
        };
        lib.postApi(lib.makeApiUrl('command'), data)
            .done(() => {
                localStorage.setItem('vault-commands-history', JSON.stringify(oldCommands));
                let stats = requestManager.getStats();
                setUploadsDisplay(`Finished: ${stats.done}/${stats.total} uploaded, ${stats.numFailed} failed.`);

                if (!onDone_)
                    alert('Done!');
                else
                    onDone_(false);
            })
            .fail((req, status, err) => {
                if (!onDone_)
                    alert('An error occurred...');
                else
                    onDone_(true);
                console.error(req, status, err);
                setUploadsDisplay('Failed to upload to vault.');
            });
    });

    makeUploadsDisplay();

    if (!requestManager.getStats().total) {
        setUploadsDisplay('No new commands to upload.');

        if (!onDone_)
            alert('No new commands to upload.');
        else
            onDone_(false)

        lib.postApi(lib.makeApiUrl('command'), {
            isOwnCommands: true,
            commands: []
        });
    } else {
        requestManager.start();
    }

    function makeUploadsDisplay() {
        if (onProgress_)
            return;

        let id = "vault-uploads-display";
        if ($(`#${id}`).length)
            $(`#${id}`).remove();
        let $uploadsContainer = $('<div id="vault-uploads-display">');
        $doc.find('#cancelform').prepend($uploadsContainer);
        updateUploadsDisplay();
    }

    function updateUploadsDisplay() {
        let stats = requestManager.getStats();
        setUploadsDisplay(`Uploading ${stats.total} commands... (${stats.done} done, ${stats.numFailed} failed.)`);
    }

    function setUploadsDisplay(contents) {
        if (onProgress_) {
            onProgress_(contents);
        }

        let $uploadsContainer = $doc.find('#vault-uploads-display');
        $uploadsContainer.text(contents);
    }
}