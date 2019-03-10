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

    let collectingPagesMessage = lib.translate(lib.itlcodes.COMMANDS_COLLECTING_PAGES);
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
            onProgress_ && onProgress_(lib.translate(lib.itlcodes.COMMANDS_NONE_NEW));

            if (onDone_)
                onDone_();
            else
                alert(lib.translate(lib.itlcodes.COMMANDS_NONE_NEW, { _escaped: false }));

            return;
        }

        requestManager.resetStats();

        onProgress_ && onProgress_(lib.translate(lib.itlcodes.COMMANDS_CHECK_UPLOADED));
        checkExistingCommands(newCommands.map(_ => _.commandId), (existingCommands) => {

            oldCommands.push(...existingCommands);

            let fetchingCommandsMessage = lib.translate(lib.itlcodes.COMMANDS_UPLOADING);
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

                    let notifyOnDone = () => {
                        let stats = requestManager.getStats();
                        if (requestManager.pendingRequests.length && onProgress_)
                            onProgress_(`${fetchingCommandsMessage} (${lib.translate(lib.itlcodes.COMMANDS_PROGRESS, { numFailed: stats.numFailed, numDone: stats.done, numTotal: stats.total })})`);
                    };

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
                            if (lib.isUnloading())
                                return;

                            requestManager.getStats().numFailed++;
                            notifyOnDone();

                            console.error(req, status, err);
                        });
                });
            });

            if (!requestManager.getStats().total) {
                lib.setLocalStorage('commands-history', oldCommands);
                lib.postApi('command/finished-command-uploads');
                onProgress_ && onProgress_(lib.translate(lib.itlcodes.COMMANDS_NONE_NEW));

                if (onDone_)
                    onDone_();
                else
                    alert(lib.translate(lib.itlcodes.COMMANDS_NONE_NEW, { _escaped: false }));

                return;
            }

            requestManager.start();

            requestManager.setFinishedHandler(() => {
                let stats = requestManager.getStats();
                lib.setLocalStorage('commands-history', oldCommands);
                onProgress_ && onProgress_(lib.translate(lib.itlcodes.COMMANDS_FINISHED, { numDone: stats.done, numTotal: stats.total, numFailed: stats.numFailed }));

                if (!onDone_)
                    alert(lib.translate(lib.itlcodes.DONE, { _escaped: false }));
                else
                    onDone_(false);
            });

        });
    });

    requestManager.start();




    function checkExistingCommands(commandIds, onDone) {
        lib.postApi('command/check-existing-commands', commandIds)
            .error(() => {
                if (lib.isUnloading())
                    return;

                onProgress_ && onProgress_(lib.translate(lib.itlcodes.COMMANDS_CHECK_UPLOADED_FAILED));
                setTimeout(onDone, 2000);
            })
            .done((existingCommandIds) => {
                if (existingCommandIds.length) {
                    // COMMANDS_SKIPPED_OLD
                    onProgress_ && onProgress_(lib.translate(lib.itlcodes.COMMANDS_SKIPPED_OLD, { numCommands: existingCommandIds.length }));
                    setTimeout(() => onDone(existingCommandIds), 2000);
                } else {
                    onDone(existingCommandIds);
                }
            });
    }
    
}