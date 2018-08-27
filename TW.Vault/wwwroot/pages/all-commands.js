function parseAllCommands($doc, onProgress_, onDone_) {

    //# REQUIRE pages/own-command.js
    //# REQUIRE pages/own-commands-overview.js

    $doc = $doc || $(document);

    var requestManager = new RequestManager();
    
    var oldCommands = JSON.parse(localStorage.getItem('vault-commands-history') || '[]');
    let commandLinks = [];
    let newCommandData = [];
    let newCommands = [];

    let pages = lib.detectMultiPages($doc);
    pages.push(lib.makeTwUrl(lib.pageTypes.OWN_COMMANDS_OVERVIEW));

    let collectingPagesMessage = 'Collecting command pages...';
    onProgress_ && onProgress_(collectingPagesMessage);

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

            onProgress_ && onProgress_(`${collectingPagesMessage} (${requestManager.getStats().done}/${pages.length})`);

            let pageCommands = parseOwnCommandsOverviewPage($(data));
            commandLinks.push(...pageCommands);
        });
    });

    requestManager.setFinishedHandler(() => {
        requestManager.stop();

        let distinctCommandLinks = commandLinks.distinct((a, b) => a.link == b.link);
        newCommands.push(...distinctCommandLinks.except((c) => oldCommands.contains(c.commandId)));

        //  Remove nonexistant commands from oldCommands
        oldCommands = oldCommands.except((c) => !distinctCommandLinks.contains((r) => r.commandId));

        console.log('All commands: ', commandLinks);
        console.log('Distinct commands: ', distinctCommandLinks);
        console.log('Collected new commands: ', newCommands);

        if (!newCommands.length) {
            onProgress_ && onProgress_('Finished: No new commands to upload.');

            if (onDone_)
                onDone_();
            else
                alert('No new commands to upload.');

            return;
        }

        requestManager.resetStats();

        let fetchingCommandsMessage = 'Collecting commands...';
        onProgress_ && onProgress_(fetchingCommandsMessage);

        newCommands.forEach((cmd) => {
            let commandId = cmd.commandId;
            let link = cmd.link;

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

                onProgress_ && onProgress_(`${fetchingCommandsMessage} (${requestManager.getStats().done}/${newCommands.length} done, ${requestManager.getStats().numFailed} failed)`);

                let command = parseOwnCommand(commandId, cmd.commandType, cmd.isReturning, $(data));
                newCommandData.push(command);
            });
        });

        requestManager.start();

        requestManager.setFinishedHandler(() => {

            onProgress_ && onProgress_('Uploading to vault...');

            let data = {
                isOwnCommands: true,
                commands: newCommandData
            };
            lib.postApi(lib.makeApiUrl('command'), data)
                .done(() => {
                    oldCommands.push(...newCommands.map((c) => c.commandId));
                    localStorage.setItem('vault-commands-history', JSON.stringify(oldCommands));
                    let stats = requestManager.getStats();


                    onProgress_ && onProgress_(`Finished: ${stats.done}/${stats.total} uploaded, ${stats.numFailed} failed.`);

                    if (!onDone_)
                        alert('Done!');
                    else
                        onDone_(false);
                })
                .fail((req, status, err) => {
                    onProgress_ && onProgress_('Failed to upload to vault.');
                    if (!onDone_)
                        alert('An error occurred...');
                    else
                        onDone_(true);

                    console.error(req, status, err);
                });
        });
    });

    requestManager.start();


    
}