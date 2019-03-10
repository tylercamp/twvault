function enhanceVillageInfoPage($doc) {
    $doc = $doc || $(document);

    $doc.find('#vault-commands').remove();
    let $container = $(`
        <div id="vault-commands" style="padding-top:10px;clear:both">
            <div id="vault-commands-backtime-bbcode" style="padding-bottom:10px;display:none">
                <textarea style="width:100%; box-sizing:border-box;min-height:100px"></textarea>
            </div>
            <button id="vault-show-main">${lib.translate(lib.itlcodes.OPEN_VAULT)}</button>
            <table class="vis" width="100%">
                <tr>
                    <th width="52%">${lib.translate(lib.itlcodes.COMMANDS_FROM_HERE)}</th>
                    <th width="33%">${lib.translate(lib.itlcodes.ARRIVAL_TIME)}</th>
                    <th width="15%">${lib.translate(lib.itlcodes.ARRIVES_IN)}</th>
                </tr>
            </table>
        </div>
    `.trim());

    $('#content_value > table > tbody > tr > td:nth-of-type(2) > .vis').after($container);

    $container.find('#vault-show-main').click(() => {
        displayMainVaultUI().onClosed(loadCommandData);
    });

    let villageCommandData = null;
    let commandsById = null;
    let showedNeedsUploadMessage = false;

    loadCommandData();

    function loadCommandData() {
        let currentVillageId = location.href.match(/id=(\d+)/)[1];
        lib.getApi(`village/${currentVillageId}/commands`)
            .done((data) => {
                if (typeof data == 'string')
                    data = lib.jsonParse(data);

                console.log('Got command data: ', data);

                villageCommandData = data;
                commandsById = lib.arrayToObject(data.commandsFromVillage || [], (c) => c.commandId, (c) => c);
                makeCommandsUI();
                UI.ToolTip('.vault-command-icons', {
                    bodyHandler: handleIconHover
                });
                UI.ToolTip('.vault-backtime', {
                    bodyHandler: () => `<b>${lib.translate(lib.itlcodes.BACKTIME_BB_CODE_HOVER)}</b>`
                });
            })
            .error((xhr) => {
                if (xhr.status == 423) {
                    if (showedNeedsUploadMessage) {
                        return;
                    }

                    let reasons = null;
                    try {
                        reasons = JSON.parse(xhr.responseText);
                    } catch (_) { }
                    let alertMessage = lib.translate(lib.itlcodes.BACKTIME_UPLOAD_DATA_REQUIRED);
                    if (reasons) {
                        alertMessage += `\n${lib.translate(lib.itlcodes.UPLOAD_DATA_REQUIRED_REASONS)} ${reasons.join(', ')}`;
                    }
                    alert(alertMessage);
                    displayMainVaultUI().onClosed(loadCommandData);
                    showedNeedsUploadMessage = true;
                } else {
                    alert(lib.messages.GENERIC_ERROR);
                }
            });
    }


    function makeCommandsUI() {
        $container.find('table tr:not(:first-of-type)').remove();

        if (!villageCommandData || !villageCommandData.commandsFromVillage || !villageCommandData.commandsFromVillage.length) {
            $container.find('table').append(`
                <tr><td colspan="3" style="text-align:center">${lib.translate(lib.itlcodes.NO_COMMANDS_AVAILABLE)}</td></tr>
            `.trim());
            return;
        }

        villageCommandData.commandsFromVillage.forEach((cmd) => {
            let targetTime = cmd.isReturning ? cmd.returnsAt : cmd.landsAt;

            const $row = $(`
                <tr>
                    <td>
                        <div style="display:inline-block;clear:right;vertical-align:middle">
                            <span class="vault-command-icons" data-command-id="${cmd.commandId}">
                                ${makeCommandIcon(cmd.isReturning, cmd.army)}
                                ${makeTroopTypeIcon(cmd.troopType)}
                            </span>

                            <a href="${lib.makeTwUrl(`screen=info_village&id=${cmd.otherVillageId}`)}">
                                ${cmd.otherVillageName}
                                (${cmd.otherVillageCoords})
                            </a>
                        </div>
                        <div class="arrowCell vault-backtime" style="display:inline-block;float:right">
                            <a href="#"><span class="arrowRight"></span></a>
                        </div>
                    </td>
                    <td>${lib.formatDateTime(targetTime)}</td>
                    <td class="cmd-timer"></td>
                </tr>
            `.trim());

            const $timer = $row.find('.cmd-timer');

            let $backtimeButton = $row.find('.vault-backtime');
            $backtimeButton.click((e) => {
                e.originalEvent.preventDefault();

                $backtimeButton.css('opacity', '0.5');
                $backtimeButton.css('pointer-events', 'none');

                let enableButton = () => {
                    $backtimeButton.css('opacity', '');
                    $backtimeButton.css('pointer-events', '');
                };

                lib.getApi(`command/${cmd.commandId}/backtime`)
                    .done((data) => {
                        enableButton();
                        console.log('Got backtime plan data: ', data);

                        $container.find('#vault-commands-backtime-bbcode').css('display', 'block');
                        $container.find('textarea').val(makePlanBbCode(data));
                    })
                    .error(() => {
                        enableButton();
                        alert(lib.messages.GENERIC_ERROR);
                    });
            });
            
            window.Timing.tickHandlers.timers._timers.push({
                element: $timer,
                end: (targetTime.valueOf() + lib.getTwUtcOffset()) / 1e3
            });

            $container.find('table').append($row);
        });
    }

    function makeStyledIcon(url) {
        return `<img src="${url}" style="max-height:100%;width:14px">`;
    }

    function makeCommandIcon(isReturning, troops) {
        let commandIcon = selectCommandImage(isReturning, troops);
        if (commandIcon) {
            return makeStyledIcon(commandIcon);
        } else {
            return '';
        }
    }

    function makeTroopTypeIcon(troopType) {
        let unit = lib.twstats.getUnit(troopType);
        if (unit) {
            return makeStyledIcon(unit.icon);
        } else {
            return '';
        }
    }

    function selectCommandImage(isReturning, troops) {

        if (!troops) {
            if (isReturning) {
                return 'https://tylercamp.me/tw/img/return_attack.png';
            } else {
                return 'https://tylercamp.me/tw/img/attack.png';
            }
        }

        const popCount = lib.twcalc.totalPopulation(troops);
        let type = null;

        if (popCount < 5000) {
            // Small
            type = 'small';
        } else if (popCount < 12000) {
            // Medium
            type = 'medium';
        } else {
            // Large
            type = 'large';
        }

        let img = `https://tylercamp.me/tw/img/`;
        if (isReturning) {
            img += 'return_attack_';
        }
        img += type + '.png';

        return img;
    }



    function handleIconHover() {
        let $el = $(this);
        let army = commandsById[$el.data('command-id')].army;

        if (!army) {
            return `<b>${lib.translate(lib.itlcodes.NO_DATA_AVAILABLE)}</b>`;
        }

        let troopNames = [
            'spear',
            'sword',
            'axe'
        ];

        if (lib.twstats.archersEnabled)
            troopNames.push('archer');
        
        troopNames.push('spy', 'light');

        if (lib.twstats.archersEnabled)
            troopNames.push('marcher');

        troopNames.push('heavy', 'ram', 'catapult', 'snob');

        if (lib.twstats.paladinEnabled)
            troopNames.push('knight');

        return `
            <b style="white-space:nowrap;">${lib.translate(lib.itlcodes.TROOPS)}</b>
            <br>
            <table class="vis" style="width:100%">
                <tr>
                    ${troopNames.map((n) => !army[n] ? '' : `<th style="min-width:50px"><img src="${lib.twstats.getUnit(n).icon}" style="max-width:17px"></th>`).join('')}
                </tr>
                <tr>
                    ${troopNames.map((n) => !army[n] ? '' : `<td>${army[n]}</td>`).join('')}
                </tr>
            </table>
        `.trim();
    }


    function makePlanBbCode(instructions) {
        const tableBuilder = new BBTableBuilder(lib.translate(lib.itlcodes.BB_TABLE));
        tableBuilder.setColumnNames(
            lib.translate(lib.itlcodes.SOURCE_VILLAGE),
            lib.translate(lib.itlcodes.LAUNCH_TIME),
            lib.translate(lib.itlcodes.LANDING_TIME),
            lib.translate(lib.itlcodes.TROOP_REQUIRED)
        );

        instructions.forEach((i) => {
            tableBuilder.addRow(
                `[${lib.translate(lib.itlcodes.BB_URL)}=${lib.makeTwUrl(`village=${i.sourceVillageId}&screen=place&from=simulator&att_${i.troopType}=1&target_village_id=${i.targetVillageId}`)}]${i.sourceVillageName} (${i.sourceVillageX}|${i.sourceVillageY})[/${lib.translate(lib.itlcodes.BB_URL)}]`,
                lib.formatDateTime(i.launchAt),
                lib.formatDateTime(i.landsAt),
                `[${lib.translate(lib.itlcodes.BB_UNIT)}]${i.troopType}[/${lib.translate(lib.itlcodes.BB_UNIT)}]`
            );
        });

        return tableBuilder.toString();
    }
}