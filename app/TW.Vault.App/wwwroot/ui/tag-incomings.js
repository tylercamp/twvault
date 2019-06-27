function tagOnIncomingsOverviewPage($doc) {
    $doc = $doc || $(document);

    //  Prevent auto-reload when incomings land (it erases the UI and messes with tagging)
    window.partialReload = function () {}

    let defaultFormat = `
        %troopName% %tagType% Pop: %popPerc%% Cats: %numCats% Com:1/%numComs%
    `.trim();

    const UNLABELED_TAG_NAME = 'Attack';
    let rateLimiter = new RateLimiter();

    let incomings = [];
    let originalLabels = lib.getLocalStorage('original-labels', {});
    let settings = lib.getLocalStorage('tag-settings', {
        tagFormat: defaultFormat,
        ignoreMissingData: true,
        labelGuessedFakes: false,
        maxFakePop: 5,
        onlyTagUnlabeled: false
    });

    let $incomingRows = $doc.find('#incomings_table tr:not(:first-of-type):not(:last-of-type)');
    foreachCommand((cmd) => {
        cmd.wasLabeled = !!originalLabels[cmd.id];
        incomings.push(cmd);
    });

    //  This is supposed to update the row elements for the commands that we're tagging in
    //  case they change after an attack lands, but apparently adding this observer stops
    //  that updating completely? Eh, not intentional but it works
    let mutationObserver = new MutationObserver((mutations) => {
        var incomingsUpdated = false;
        mutations.forEach((mut) => {
            mut.addedNodes.forEach((n) => {
                if ($(n).prop('id') == 'paged_view_content') {
                    incomingsUpdated = true;
                }
            });
        });

        if (incomingsUpdated) {
            console.log('Incomings updated, refreshing...');
            let $newRows = $('tr:not(:first-of-type):not(:last-of-type)');

            foreachCommand((cmd) => {
                let inc = incomings.find((i) => i.id == cmd.id);
                if (inc) {
                    inc.$row = cmd.$row;
                } else {
                    console.warn("Couldn't find command entry for " + cmd.id);
                }
            }, $newRows);

            $incomingRows = $newRows;
        }
    });

    mutationObserver.observe(document, {
        childList: true,
        subtree: true
    });

    let incomingTags = null;

    console.log('Got incomings: ', incomings);

    getVaultTags(() => {
        makeTaggingUI();
    });

    //  Store the original labels of new commands so we can restore them later
    foreachCommand((cmd) => {
        if (!originalLabels[cmd.id]) {
            originalLabels[cmd.id] = {
                label: cmd.label.trim(),
                arrivesAt: cmd.arrivesAt
            };
        }
    });

    //  Remove old labels for commands that have landed (so we don't blow up localStorage size)
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
        lib.postApi('command/tags', incomings.filter(i => i.id != null).map((i) => i.id))
            .done((tags) => {
                console.log('Got tags: ', tags);
                incomingTags = tags;
                onDone_ && onDone_();

                let numMissingIncs = incomings.filter((i) => !incomingTags[i.id]).length;
                if (numMissingIncs > 0) {
                    $('#missing-command-uploads').html(`<b>${lib.translate(lib.itlcodes.INCS_NOT_TAGGED, { numIncs: numMissingIncs })}</b>`);
                } else {
                    $('#missing-command-uploads').html('');
                }
            })
            .error((xhr, b, c) => {
                if (xhr.status == 423) {
                    let reasons = null;
                    try {
                        reasons = JSON.parse(xhr.responseText);
                    } catch (_) { }

                    let alertMessage = lib.translate(lib.itlcodes.TAG_UPLOAD_DATA_REQUIRED, { _escaped: false });
                    if (reasons) {
                        alertMessage += `\n${lib.translate(lib.itlcodes.UPLOAD_DATA_REQUIRED_REASONS)} ${reasons.join(', ')}`;
                    }

                    alert(alertMessage);
                    displayMainVaultUI();
                } else if (xhr.status != 401) {
                    alert(lib.messages.GENERIC_ERROR);
                }
            });
    }

    function makeTaggingUI() {
        $('#v-tagging-ui').remove();
        let $container = $(`
            <div id="v-tagging-ui" class="content-border">
                <h3>${lib.translate(lib.itlcodes.TAGGING_TITLE)}</h3>
                <p>
                    <b>${lib.translate(lib.itlcodes.FEATURE_IS_EXPERIMENTAL)}</b>
                </p>
                <p>
                    <button id="v-show-vault">${lib.translate(lib.itlcodes.OPEN_VAULT)}</button>
                    <button id="v-upload-visible-incomings">${lib.translate(lib.itlcodes.UPLOAD_VISIBLE_INCOMINGS)}</button>
                </p>
                <p id="missing-command-uploads"></p>
                <p>
                    <table class="vis">
                        <tr>
                            <th>${lib.translate(lib.itlcodes.TAG_CODE_HEADER)}</th>
                            <th>${lib.translate(lib.itlcodes.TAG_CODE_DETAILS)}</th>
                        </tr>
                        <tr class="row_a">
                            <td>%troopName%</td>
                            <td>${lib.translate(lib.itlcodes.TAG_CODE_TROOP_NAME_DETAILS)}</td>
                        </tr>
                        <tr class="row_b">
                            <td>%tagType%</td>
                            <td>${lib.translate(lib.itlcodes.TAG_CODE_TAG_TYPE_DETAILS)}</td>
                        </tr>
                        <tr class="row_a">
                            <td>%popPerc%</td>
                            <td>${lib.translate(lib.itlcodes.TAG_CODE_POP_PERCENT_DETAILS)}</td>
                        </tr>
                        <tr class="row_b">
                            <td>%popCnt%</td>
                            <td>${lib.translate(lib.itlcodes.TAG_CODE_POP_COUNT_DETAILS)}</td>
                        </tr>
                        <tr class="row_a">
                            <td>%popReturnPerc%</td>
                            <td>${lib.translate(lib.itlcodes.TAG_CODE_POP_RETURN_PERCENT_DETAILS)}</td>
                        </tr>
                        <tr class="row_b">
                            <td>%popReturnCnt%</td>
                            <td>${lib.translate(lib.itlcodes.TAG_CODE_POP_RETURN_COUNT_DETAILS)}</td>
                        </tr>
                        <tr class="row_a">
                            <td>%numCats%</td>
                            <td>${lib.translate(lib.itlcodes.TAG_CODE_NUM_CATS_DETAILS)}</td>
                        </tr>
                        <tr class="row_b">
                            <td>%numComs%</td>
                            <td>${lib.translate(lib.itlcodes.TAG_CODE_NUM_COMS_DETAILS)}</td>
                        </tr>
                        <tr class="row_a">
                            <td>%srcPlayer%</td>
                            <td>${lib.translate(lib.itlcodes.TAG_CODE_SRC_PLAYER_DETAILS)}</td>
                        </tr>
                        <tr class="row_b">
                            <td>%srcVilla%</td>
                            <td>${lib.translate(lib.itlcodes.TAG_CODE_SRC_VILLAGE_DETAILS)}</td>
                        </tr>
                        <tr class="row_a">
                            <td>%targetVilla%</td>
                            <td>${lib.translate(lib.itlcodes.TAG_CODE_TGT_VILLAGE_DETAILS)}</td>
                        </tr>
                        <tr class="row_b">
                            <td>%srcCoords%</td>
                            <td>${lib.translate(lib.itlcodes.TAG_CODE_SRC_COORDS_DETAILS)}</td>
                        </tr>
                        <tr class="row_a">
                            <td>%targetCoords%</td>
                            <td>${lib.translate(lib.itlcodes.TAG_CODE_TGT_COORDS_DETAILS)}</td>
                        </tr>
                        <tr class="row_b">
                            <td>%distance%</td>
                            <td>${lib.translate(lib.itlcodes.TAG_CODE_DISTANCE_DETAILS)}</td>
                        </tr>
                        <tr class="row_a">
                            <td>%villaType%</td>
                            <td>${lib.translate(lib.itlcodes.TAG_CODE_VILLAGE_TYPE_DETAILS)}</td>
                        </tr>
                        <tr class="row_b">
                            <td>%customLabel%</td>
                            <td>${lib.translate(lib.itlcodes.TAG_CODE_CUSTOM_LABEL_DETAILS)}</td>
                        </tr>
                    </table>
                </p>
                <p>
                    <label for="v-tag-format">${lib.translate(lib.itlcodes.TAG_CFG_FORMAT)}</label>
                    <input type="text" id="v-tag-format" style="width:80em">
                    <button id="v-reset-format">${lib.translate(lib.itlcodes.RESET)}</button>
                </p>
                <p>
                    <input type="checkbox" id="v-tag-unlabeled" ${settings.onlyTagUnlabeled ? 'checked' : ''}>
                    <label for="v-tag-unlabeled">${lib.translate(lib.itlcodes.TAG_CFG_ONLY_UNLABELED)}</label>
                </p>
                <p>
                    <input type="checkbox" id="v-autoset-fakes" ${settings.autoLabelFakes ? 'checked' : ''}>
                    <label for="v-autoset-fakes">
                        ${lib.translate(lib.itlcodes.TAG_CFG_AUTOTAG_FAKE_1)} <input id="v-max-fake-pop" type="text" style="width:2em; text-align:center" value="${settings.maxFakePop}"> ${lib.translate(lib.itlcodes.TAG_CFG_AUTOTAG_FAKE_2)}
                    <label>
                </p>
                <p>
                    <input type="checkbox" id="v-ignore-missing" ${settings.ignoreMissingData ? 'checked' : ''}>
                    <label for="v-ignore-missing">
                        ${lib.translate(lib.itlcodes.TAG_CFG_IGNORE_NO_DATA)}
                    </label>
                <p>
                    <input type="checkbox" id="v-preview">
                    <label for="v-preview">${lib.translate(lib.itlcodes.PREVIEW)}</label>
                </p>
                <p>
                    <button id="v-tag-all">${lib.translate(lib.itlcodes.TAG_ALL)}</button>
                    <span class="v-sep"></span>
                    <button id="v-tag-selected">${lib.translate(lib.itlcodes.TAG_SELECTED)}</button>
                    <span class="v-sep"></span>
                    <button id="v-revert-tagging">${lib.translate(lib.itlcodes.TAG_REVERT)}</button>
                </p>
                <p>
                    <button id="v-cancel" disabled>${lib.translate(lib.itlcodes.CANCEL)}</button>
                </p>
                <p>
                    <em>${lib.translate(lib.itlcodes.TAG_DURATION_NOTICE)}</em>
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

        $('.overview_filters').before($container);

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
            let maxPop = parseFloat(maxPopText);
            if (isNaN(maxPop) || maxPopText.match(/[^\d\.]/)) {
                alert(lib.translate(lib.itlcodes.NOT_A_NUMBER, { _escaped: false }));
                $maxFakePop.val(settings.maxFakePop);
                return;
            }

            settings.maxFakePop = maxPop;
            saveSettings();
        });

        $container.find('#v-show-vault').click((e) => {
            e.originalEvent.preventDefault();
            displayMainVaultUI().onClosed(getVaultTags);
        });

        $container.find('#v-upload-visible-incomings').click((e) => {
            e.originalEvent.preventDefault();
            toggleUploadButtons(false);

            let visibleIncomings = parseUploadIncomingsOverviewPage($(document));
            let data = {
                commands: visibleIncomings,
                isOwnCommands: false,
            };

            lib.postApi('command', data)
                .done(() => {
                    getVaultTags();
                    toggleUploadButtons(true);
                })
                .error(() => {
                    alert(lib.messages.GENERIC_ERROR);
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
                if (!selectedIncomings.length && !settings.onlyTagUnlabeled)
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

                    let newLabel = makeLabel(incomingTags[cmd.id] || {}, originalLabel);
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

        $container.find('#v-tag-unlabeled').change(() => {
            settings.onlyTagUnlabeled = $container.find('#v-tag-unlabeled').prop('checked');
            saveSettings();
        });

        $container.find('#v-tag-all').click((e) => {
            e.originalEvent.preventDefault();
            if (settings.autoLabelFakes && !confirm(lib.translate(lib.itlcodes.FAKE_DETECTION_CONFIRM, { _escaped: false })))
                return;

            beginTagging(incomings.filter(i => !settings.onlyTagUnlabeled || i.$row.find('.quickedit-label').text().trim() == UNLABELED_TAG_NAME).map((i) => i.id));
        });

        $container.find('#v-tag-selected').click((e) => {
            e.originalEvent.preventDefault();
            if (settings.autoLabelFakes && !confirm(lib.translate(lib.itlcodes.FAKE_DETECTION_CONFIRM, { _escaped: false })))
                return;

            let selectedIds = getSelectedIncomingIds();
            if (!selectedIds.length)
                alert(lib.translate(lib.itlcodes.NO_INCOMINGS_SELECTED, { _escaped: false }));
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
                updateTagStatus(lib.translate(lib.itlcodes.TAGS_ARE_CURRENT));
            }
        });

        $container.find('#v-cancel').click((e) => {
            e.originalEvent.preventDefault();
            rateLimiter.stop();
            toggleUploadButtons(true);
            updateTagStatus(lib.translate(lib.itlcodes.TAGGING_CANCELED));
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
            let newLabel = makeLabel(incomingTags[id], $label.text().trim());

            if (!newLabel)
                return;

            newLabel = newLabel.trim();
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
            updateTagStatus(lib.translate(lib.itlcodes.TAGS_ARE_CURRENT));
        }
    }

    function updateTagStatus(msg_) {
        let stats = rateLimiter.getStats();
        let $tagStatus = $('#v-tag-status');
        if (msg_) {
            $tagStatus.text(msg_);
        } else if (stats.total) {
            let progress = lib.translate(lib.itlcodes.TAGGING_PROGRESS, { numDone: stats.done, numTotal: stats.total, numFailed: stats.numFailed });
            if (stats.total == stats.done) {
                progress = `${lib.translate(lib.itlcodes.TAG_STATE_FINISHED)} ${progress}`;
            } else if (rateLimiter.isRunning()) {
                progress = `${lib.translate(lib.itlcodes.TAG_STATE_RUNNING)} ${progress}`;
            } else {
                progress = `${lib.translate(lib.itlcodes.TAG_STATE_CANCELED)} ${progress}`;
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

            if ($link.text().trim() == UNLABELED_TAG_NAME || !settings.onlyTagUnlabeled)
                selectedIds.push(parseInt(commandId));
        });
        return selectedIds;
    }

    function foreachCommand(callback, $commandRows_) {
        ($commandRows_ || $incomingRows).each((i, row) => {
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

    function makeLabel(incomingData, currentLabel) {

        incomingData = incomingData || {
            offensivePopulation: null,
            numCats: null,
            numFromVillage: null,
            troopType: null
        };

        let hasData =
            incomingData && (
                (typeof incomingData.offensivePopulation != 'undefined' && incomingData.offensivePopulation != null) ||
                (typeof incomingData.numCats != 'undefined' && incomingData.numCats != null) ||
                (typeof incomingData.returningPopulation != 'undefined' && incomingData.returningPopulation != null) ||
                incomingData.numFromVillage > 1
            );

        if (!hasData && settings.ignoreMissingData)
            return null;

        let format = settings.tagFormat;
        let missingNukePop = typeof incomingData.offensivePopulation == 'undefined' || incomingData.offensivePopulation == null;
        let missingNumCats = typeof incomingData.numCats == 'undefined' || incomingData.numCats == null;
        let missingReturnPop = typeof incomingData.returningPopulation == 'undefined' || incomingData.returningPopulation == null;
        let troopTypeName = incomingData.troopType ? (
            lib.twstats.getUnit(incomingData.troopType).name
        ) : lib.translate(lib.itlcodes.UNKNOWN);

        let maxNukePop = 20000;
        let nukePop = Math.min(maxNukePop, incomingData.offensivePopulation || 0);
        let nukePopK = Math.roundTo(nukePop / 1000, 1);
        let nukePopPerc = Math.roundTo(nukePop / maxNukePop * 100, 1);
        let returnPop = Math.min(maxNukePop, incomingData.returningPopulation || 0);
        let returnPopK = Math.roundTo(returnPop / 1000, 1);
        let returnPopPerc = Math.roundTo(returnPop / maxNukePop * 100, 1);

        if (settings.autoLabelFakes && incomingData.troopType != 'snob' && !missingNukePop && nukePopK < settings.maxFakePop) {
            return lib.translate(lib.itlcodes.FAKES);
        }

        let customLabel = currentLabel.match(/".+"/);
        if (customLabel)
            customLabel = customLabel[0];

        return format
            .replace("%troopName%", troopTypeName)
            .replace("%tagType%", incomingData.definiteFake ? lib.translate(lib.itlcodes.FAKE) : lib.translate(lib.itlcodes.MAYBE_NUKE))
            .replace("%popPerc%", missingNukePop ? '?' : nukePopPerc)
            .replace("%popCnt%", missingNukePop ? '?' : nukePopK)
            .replace("%numCats%", missingNumCats ? '?' : incomingData.numCats)
            .replace("%numComs%", incomingData.numFromVillage || '?')
            .replace("%customLabel%", customLabel || '')
            .replace("%popReturnPerc%", missingReturnPop ? '?' : returnPopPerc)
            .replace("%popReturnCnt%", missingReturnPop ? '?' : returnPopK)
            .replace("%villaType%", incomingData.villageType || '?')
            .replace("%distance%", Math.roundTo(incomingData.distance || 0, 1))
            .replace("%targetCoords%", incomingData.targetVillageCoords || '?')
            .replace("%srcCoords%", incomingData.sourceVillageCoords || '?')
            .replace("%srcVilla%", incomingData.sourceVillageName || '?')
            .replace("%targetVilla%", incomingData.targetVillageName || '?')
            .replace("%srcPlayer%", incomingData.sourcePlayerName || '?')
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
            '#v-show-vault',
            '#v-upload-visible-incomings',
            '#v-tag-format',
            '#v-reset-format',
            '#v-tag-unlabeled',
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