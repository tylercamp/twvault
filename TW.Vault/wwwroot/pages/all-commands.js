function parseAllCommands($doc, onProgress_, onDone_) {

    //# REQUIRE pages/own-command.js
    //# REQUIRE pages/own-commands-overview.js

    $doc = $doc || $(document);

    var requestManager = new RequestManager();

    var oldCommands = lib.getLocalStorage('commands-history', []);
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

            let pageCommands = parseOwnCommandsOverviewPage(lib.parseHtml(data));
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
            lib.postApi('command/finished-command-uploads');
            onProgress_ && onProgress_('Finished: No new commands to upload.');

            if (onDone_)
                onDone_();
            else
                alert('No new commands to upload.');

            return;
        }

        requestManager.resetStats();

        onProgress_ && onProgress_('Checking for previously-uploaded commands...');
        checkExistingCommands(newCommands.map(_ => _.commandId), (existingCommands) => {

            oldCommands.push(...existingCommands);

            let fetchingCommandsMessage = 'Uploading commands...';
            onProgress_ && onProgress_(fetchingCommandsMessage);

            newCommands.forEach((cmd) => {
                let commandId = cmd.commandId;
                let link = cmd.link;

                if (oldCommands.contains(commandId))
                    return;

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

                    let command = parseOwnCommand(commandId, cmd.commandType, cmd.isReturning, cmd.userLabel, lib.parseHtml(data));

                    let notifyOnDone = () => requestManager.pendingRequests.length && onProgress_ && onProgress_(`${fetchingCommandsMessage} (${requestManager.getStats().done}/${requestManager.getStats().total} done, ${requestManager.getStats().numFailed} failed)`);

                    let commandData = {
                        isOwnCommands: true,
                        commands: [command]
                    };

                    lib.postApi('command', commandData)
                        .done(() => {
                            oldCommands.push(command.commandId);
                            lib.setLocalStorage('commands-history', oldCommands);
                            notifyOnDone();

                        })
                        .fail((req, status, err) => {
                            requestManager.getStats().numFailed++;
                            notifyOnDone();

                            console.error(req, status, err);
                        });
                });
            });

            if (!requestManager.getStats().total) {
                lib.setLocalStorage('commands-history', oldCommands);
                lib.postApi('command/finished-command-uploads');
                onProgress_ && onProgress_('Finished: No new commands to upload.');

                if (onDone_)
                    onDone_();
                else
                    alert('No new commands to upload.');

                return;
            }

            requestManager.start();

            requestManager.setFinishedHandler(() => {
                let stats = requestManager.getStats();
                lib.setLocalStorage('commands-history', oldCommands);
                onProgress_ && onProgress_(`Finished: ${stats.done}/${stats.total} uploaded, ${stats.numFailed} failed.`);

                if (!onDone_)
                    alert('Done!');
                else
                    onDone_(false);
            });

        });
    });

    requestManager.start();




    function checkExistingCommands(commandIds, onDone) {
        lib.postApi('command/check-existing-commands', commandIds)
            .error(() => {
                onProgress_ && onProgress_('Failed to check for old commands, uploading all...');
                setTimeout(onDone, 2000);
            })
            .done((existingCommandIds) => {
                if (existingCommandIds.length) {
                    onProgress_ && onProgress_('Found ' + existingCommandIds.length + ' old commands, skipping these...');
                    setTimeout(() => onDone(existingCommandIds), 2000);
                } else {
                    onDone(existingCommandIds);
                }
            });
    }
    
}