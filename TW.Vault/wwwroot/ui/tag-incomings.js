function tagOnIncomingsOverviewPage($doc) {
    $doc = $doc || $(document);

    //  Prevent auto-reload when incomings land (it erases the UI and messes with tagging)
    window.partialReload = function () {}

    let defaultFormat = `
        %troopName% %tagType% Pop: %popPerc%% Cats: %numCats% Com:1/%numComs%
    `.trim();

    let rateLimiter = new RateLimiter();

    let incomings = [];
    let originalLabels = lib.getLocalStorage('original-labels', {});
    let settings = lib.getLocalStorage('tag-settings', {
        tagFormat: defaultFormat,
        ignoreMissingData: true,
        autoLabelFakes: true,
        maxFakePop: 5
    });

    let $incomingRows = $doc.find('#incomings_table tr:not(:first-of-type):not(:last-of-type)');
    foreachCommand((cmd) => {
        cmd.wasLabeled = !!originalLabels[cmd.id];
        incomings.push(cmd);
    });

    let incomingTags = null;

    console.log('Got incomings: ', incomings);

    getVaultTags(() => {
        makeTaggingUI();
    });

    foreachCommand((cmd) => {
        if (!originalLabels[cmd.id])
            originalLabels[cmd.id] = {
                label: cmd.label.trim(),
                arrivesAt: cmd.arrivesAt
            };
    });

    (() => {
        let serverTime = lib.getServerDateTime();
        let oldIncomingIds = [];
        lib.objForEach(originalLabels, (prop, val) => {
            if (serverTime.valueOf() - val.arrivesAt.valueOf() > 0)
                oldIncomingIds.push(prop);
        });
        console.log('Removing passed incomings: ', oldIncomingIds);
        oldIncomingIds.forEach((id) => {
            delete originalLabels[id];
        });
    })();

    lib.setLocalStorage('original-labels', originalLabels);

    function getVaultTags(onDone_) {
        lib.postApi('command/tags', incomings.map((i) => i.id))
            .done((tags) => {
                console.log('Got tags: ', tags);
                incomingTags = tags;
                onDone_ && onDone_();
            })
            .error((xhr, b, c) => {
                if (xhr.status == 423) {
                    let reasons = null;
                    try {
                        reasons = JSON.parse(xhr.responseText);
                    } catch (_) { }

                    let alertMessage = "You haven't uploaded data in a while, you can't use tagging until you do."
                    if (reasons) {
                        alertMessage += `\nYou need to upload: ${reasons.join(', ')}`;
                    }

                    alert(alertMessage);
                    parseAllPages();
                } else if (xhr.status != 401) {
                    alert("An error occurred...");
                }
            });
    }

    function makeTaggingUI() {
        $('#v-tagging-ui').remove();
        let $container = $(`
            <div id="v-tagging-ui" class="content-border">
                <h3>Vault Tagging</h3>
                <p>
                    <button id="v-upload-incomings">Upload Incomings</button>
                </p>
                <p>
                    <table class="vis">
                        <tr>
                            <th>Code</th>
                            <th>Details</th>
                        </tr>
                        <tr class="row_a">
                            <td>%troopName%</td>
                            <td>Best known troop type (from your label or auto-calculated)</td>
                        </tr>
                        <tr class="row_b">
                            <td>%tagType%</td>
                            <td>One of: Fake, Nuke</td>
                        </tr>
                        <tr class="row_a">
                            <td>%popPerc%</td>
                            <td>% of a full nuke known at the village, ie 89% or ?%</td>
                        </tr>
                        <tr class="row_b">
                            <td>%popCnt%</td>
                            <td>Offensive pop known at the village, ie 19.2k or ?k</td>
                        </tr>
                        <tr class="row_a">
                            <td>%numCats%</td>
                            <td># of catapults known at the village</td>
                        </tr>
                        <tr class="row_b">
                            <td>%numComs%</td>
                            <td># of total commands from the village to the tribe</td>
                        </tr>
                    </table>
                </p>
                <p>
                    <label for="v-tag-format">Tag format: </label>
                    <input type="text" id="v-tag-format" style="width:40em">
                    <button id="v-reset-format">Reset</button>
                </p>
                <p>
                    <input type="checkbox" id="v-autoset-fakes" ${settings.autoLabelFakes ? 'checked' : ''}>
                    <label for="v-autoset-fakes">
                        Label as "Fake" if less than <input id="v-max-fake-pop" type="text" style="width:2em; text-align:center" value="${settings.maxFakePop}"> thousand offense population
                    <label>
                </p>
                <p>
                    <input type="checkbox" id="v-ignore-missing" ${settings.ignoreMissingData ? 'checked' : ''}>
                    <label for="v-ignore-missing">
                        Ignore incomings without data
                    </label>
                <p>
                    <input type="checkbox" id="v-preview">
                    <label for="v-preview">Preview</label>
                </p>
                <p>
                    <button id="v-tag-all">Tag All</button>
                    <span class="v-sep"></span>
                    <button id="v-tag-selected">Tag Selected</button>
                    <span class="v-sep"></span>
                    <button id="v-revert-tagging">Revert to Old Tags</button>
                </p>
                <p>
                    <button id="v-cancel" disabled>Cancel</button>
                </p>
                <p>
                    <em>Tagging will take a while!</em>
                </p>
                <p id="v-tag-status">
                </p>
            </div>
        `.trim());

        $container.css({
            padding: '1em',
            margin: '1em'
        });

        $container.find('.v-sep').css({
            display: 'inline-block',
            width: '1.5em'
        });

        $container.find('#v-tag-format').val(settings.tagFormat);

        $('#incomings_table').before($container);

        $container.find('#v-tag-format').focusout(() => {
            let newFormat = $container.find('#v-tag-format').val();
            settings.tagFormat = newFormat;
            saveSettings();
        });

        $container.find('#v-autoset-fakes').change(() => {
            settings.autoLabelFakes = $container.find('#v-autoset-fakes').is(':checked');
            saveSettings();
        });

        $container.find('#v-ignore-missing').change(() => {
            settings.ignoreMissingData = $container.find('#v-ignore-missing').is(':checked');
            saveSettings();
        });

        $container.find('#v-reset-format').click((e) => {
            e.originalEvent.preventDefault();
            settings.tagFormat = defaultFormat;
            $container.find('#v-tag-format').val(settings.tagFormat);
            saveSettings();
        });

        $container.find('#v-max-fake-pop').focusout(() => {
            let $maxFakePop = $container.find('#v-max-fake-pop');
            let maxPopText = $maxFakePop.val();
            let maxPop = parseInt(maxPopText);
            if (isNaN(maxPop) || maxPopText.match(/[^\d]/)) {
                alert("That's not a number!");
                $maxFakePop.val(settings.maxFakePop);
                return;
            }

            settings.maxFakePop = maxPop;
            saveSettings();
        });

        $container.find('#v-upload-incomings').click((e) => {
            e.originalEvent.preventDefault();
            parseAllPages();
            //  Annoying to need to hook into specific UI element from main UI but whatever
            $('.vault-close-btn').click(() => {
                getVaultTags();
            });
        });

        let oldLabels = null;
        $container.find('#v-preview').change(() => {
            if ($('#v-preview').is(':checked')) {
                toggleUploadButtons(false);
                $('#v-cancel').prop('disabled', true);
                $('#v-preview').prop('disabled', false);

                oldLabels = {};

                let selectedIncomings = getSelectedIncomingIds();
                if (!selectedIncomings.length)
                    selectedIncomings = incomings.map((i) => i.id);
                console.log('Selected: ', selectedIncomings);

                foreachCommand((cmd) => {
                    if (settings.ignoreMissingData && !incomingTags[cmd.id])
                        return;

                    if (!selectedIncomings.contains(cmd.id))
                        return;

                    let $label = cmd.$row.find('.quickedit-label');
                    let originalLabel = $label.text().trim();

                    oldLabels[cmd.id] = originalLabel;

                    let newLabel = makeLabel(incomingTags[cmd.id] || {});
                    if (newLabel)
                        $label.text(newLabel);
                });
            } else {
                toggleUploadButtons(true);

                foreachCommand((cmd) => {
                    if (oldLabels[cmd.id]) {
                        cmd.$row.find('.quickedit-label').text(oldLabels[cmd.id]);
                    }
                });

                oldLabels = null;
            }
        });

        $container.find('#v-tag-all').click((e) => {
            e.originalEvent.preventDefault();
            beginTagging(incomings.map((i) => i.id));
        });

        $container.find('#v-tag-selected').click((e) => {
            e.originalEvent.preventDefault();
            let selectedIds = getSelectedIncomingIds();
            if (!selectedIds.length)
                alert("You didn't select any incomings!");
            beginTagging(selectedIds);
        });

        $container.find('#v-revert-tagging').click((e) => {
            e.originalEvent.preventDefault();
            let selectedIds = getSelectedIncomingIds();
            if (!selectedIds.length)
                selectedIds = incomings.map((i) => i.id);

            rateLimiter.resetStats();
            selectedIds.forEach((id) => {

                let cmd = incomings.find((i) => i.id == id);
                let $label = cmd.$row.find('.quickedit-label');
                let newLabel = originalLabels[id].label;

                if (newLabel == $label.text().trim())
                    return;

                rateLimiter.addTask(() => {
                    renameIncoming(id, newLabel, () => {
                        if (rateLimiter.isRunning()) {
                            alert(lib.messages.TRIGGERED_CAPTCHA);
                            rateLimiter.stop();
                            updateTagStatus(lib.messages.TRIGGERED_CAPTCHA);
                            toggleUploadButtons(true);
                        }
                    }).success(() => {
                        $label.text(newLabel);
                        updateTagStatus();
                    });
                });
                
            });

            rateLimiter.setFinishedHandler(() => {
                updateTagStatus();
                toggleUploadButtons(true);
            });

            if (rateLimiter.getStats().total > 0) {
                rateLimiter.start();
            } else {
                toggleUploadButtons(true);
                updateTagStatus("Either no incomings or all tags are current");
            }
        });

        $container.find('#v-cancel').click((e) => {
            e.originalEvent.preventDefault();
            rateLimiter.stop();
            toggleUploadButtons(true);
            updateTagStatus("Tagging canceled");
        });
    }

    function beginTagging(commandIds) {
        toggleUploadButtons(false);

        console.log('Starting tagging for: ', commandIds);
        rateLimiter.resetStats();
        commandIds.forEach((id) => {
            if (settings.ignoreMissingData && !incomingTags[id])
                return;

            let cmd = incomings.find((i) => i.id == id);
            let $label = cmd.$row.find('.quickedit-label');
            let newLabel = makeLabel(incomingTags[id]);

            if (!newLabel)
                return;

            if (newLabel == $label.text().trim())
                return;

            rateLimiter.addTask(() => {
                renameIncoming(id, newLabel, () => {
                    if (rateLimiter.isRunning()) {
                        alert(lib.messages.TRIGGERED_CAPTCHA);
                        rateLimiter.stop();
                        updateTagStatus(lib.messages.TRIGGERED_CAPTCHA);
                        toggleUploadButtons(true);
                    }
                }).success(() => {
                    $label.text(newLabel);
                    updateTagStatus();
                });
            });
        });

        rateLimiter.setFinishedHandler(() => {
            updateTagStatus();
            toggleUploadButtons(true);
        });

        if (rateLimiter.getStats().total > 0) {
            rateLimiter.start();
        } else {
            toggleUploadButtons(true);
            updateTagStatus("Either no incomings or all tags are current");
        }
    }

    function updateTagStatus(msg_) {
        let stats = rateLimiter.getStats();
        let $tagStatus = $('#v-tag-status');
        if (msg_) {
            $tagStatus.text(msg_);
        } else if (stats.total) {
            let progress = `${stats.done}/${stats.total} tagged (${stats.numFailed} failed)`;
            if (stats.total == stats.done) {
                progress = `Finished: ${progress}`;
            } else if (rateLimiter.isRunning()) {
                progress = `Tagging: ${progress}`;
            } else {
                progress = `Canceled: ${progress}`;
            }
            $tagStatus.text(progress);
        } else {
            $tagStatus.text('');
        }
    }

    function getSelectedIncomingIds() {
        let $selectedRows = $incomingRows.filter((i, el) => !!$(el).find('input:checked').length);
        let selectedIds = [];
        $selectedRows.each((i, el) => {
            let $row = $(el);
            let $link = $row.find('a[href*=info_command][href*=id]');
            let commandId = $link.prop('href').match(/id=(\w+)/)[1];
            selectedIds.push(parseInt(commandId));
        });
        return selectedIds;
    }

    function foreachCommand(callback) {
        $incomingRows.each((i, row) => {
            let $row = $(row);
            let $link = $row.find('a[href*=info_command][href*=id]');
            let label = $link.text().trim();
            let commandId = $link.prop('href').match(/id=(\w+)/)[1];
            let arrivesAtText = $row.find('td:nth-of-type(6)').text().trim();
            let arrivesAt = lib.parseTimeString(arrivesAtText);
            let data = {
                id: parseInt(commandId),
                label: label,
                arrivesAt: arrivesAt,
                $row: $row
            };
            callback(data);
        });
    }

    function makeLabel(incomingData) {
        let hasData =
            (typeof incomingData.offensivePopulation != 'undefined' && incomingData.offensivePopulation != null) ||
            (typeof incomingData.numCats != 'undefined' && incomingData.numCats != null) ||
            incomingData.numFromVillage > 1;

        if (!hasData && settings.ignoreMissingData)
            return null;

        let format = settings.tagFormat;
        let missingNukePop = typeof incomingData.offensivePopulation == 'undefined' || incomingData.offensivePopulation == null;
        let missingNumCats = typeof incomingData.numCats == 'undefined' || incomingData.numCats == null;
        let troopTypeName = incomingData.troopType ? (
            lib.twstats.getUnit(incomingData.troopType).name
        ) : 'Unknown';

        let maxNukePop = 20000;
        let nukePop = Math.min(maxNukePop, incomingData.offensivePopulation || 0);
        let nukePopK = Math.roundTo(nukePop / 1000, 1);
        let nukePopPerc = Math.roundTo(nukePop / maxNukePop * 100, 1);

        if (settings.autoLabelFakes && !missingNukePop && nukePopK < settings.maxFakePop) {
            return 'Fake';
        }

        return format
            .replace("%troopName%", troopTypeName)
            .replace("%tagType%", incomingData.definiteFake ? 'Fake' : 'Nuke?')
            .replace("%popPerc%", missingNukePop ? '?' : nukePopPerc)
            .replace("%popCnt%", missingNukePop ? '?' : nukePopK)
            .replace("%numCats%", missingNumCats ? '?' : incomingData.numCats)
            .replace("%numComs%", incomingData.numFromVillage)
        ;
    }

    function renameIncoming(incomingId, newName, onCaptcha_) {
        let clientTime = Math.round(new Date().valueOf() / 1000);
        let csrf = window.csrf_token;
        let twUrl = `screen=info_command&ajaxaction=edit_other_comment&id=${incomingId}&h=${csrf}&&client_time=${clientTime}`;
        return $.ajax({
            url: lib.makeTwUrl(twUrl),
            method: 'POST',
            dataType: "json",
            data: { text: newName },
            headers: { 'TribalWars-Ajax': 1 },
            success: (data) => {
                if (data && typeof data == 'string') try {
                    data = JSON.parse(data);
                } catch (_) { }

                if (data && !data.response && data.bot_protect) {
                    onCaptcha_ && onCaptcha_();
                }
            }
        });
    }

    function toggleUploadButtons(enabled) {
        let inputIds = [
            '#v-upload-incomings',
            '#v-tag-format',
            '#v-reset-format',
            '#v-autoset-fakes',
            '#v-max-fake-pop',
            '#v-ignore-missing',
            '#v-preview',
            '#v-tag-all',
            '#v-tag-selected',
            '#v-revert-tagging'
        ];

        if (enabled) {
            inputIds.forEach((id) => $(id).prop('disabled', false));
            $('#v-cancel').prop('disabled', true);
        } else {
            inputIds.forEach((id) => $(id).prop('disabled', true));
            $('#v-cancel').prop('disabled', false);
        }
    }

    function saveSettings() {
        lib.setLocalStorage('tag-settings', settings);
    }
}