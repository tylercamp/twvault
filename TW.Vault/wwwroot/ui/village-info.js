function enhanceVillageInfoPage($doc) {
    $doc = $doc || $(document);

    $doc.find('#vault-commands').remove();
    let $container = $(`
        <div id="vault-commands" style="padding-top:10px;clear:both">
            <div id="vault-commands-backtime-bbcode" style="padding-bottom:10px;display:none">
                <textarea style="width:100%; box-sizing:border-box;min-height:100px"></textarea>
            </div>
            <button id="vault-show-main">Open Vault</button>
            <table class="vis" width="100%">
                <tr>
                    <th width="52%">Vault - commands from here</th>
                    <th width="33%">Arrival time</th>
                    <th width="15%">Arrives in</th>
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
                    bodyHandler: () => '<b>Make BB-code for back-timing</b>'
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
                    let alertMessage = "You haven't uploaded data in a while, you can't use the backtiming script until you do. Upload your data then refresh the page and run this script again.";
                    if (reasons) {
                        alertMessage += `\nYou need to upload: ${reasons.join(', ')}`;
                    }
                    alert(alertMessage);
                    displayMainVaultUI().onClosed(loadCommandData);
                    showedNeedsUploadMessage = true;
                } else {
                    alert('An error occurred...');
                }
            });
    }


    function makeCommandsUI() {
        $container.find('table tr:not(:first-of-type)').remove();

        if (!villageCommandData || !villageCommandData.commandsFromVillage || !villageCommandData.commandsFromVillage.length) {
            $container.find('table').append(`
                <tr><td colspan="3" style="text-align:center">No commands available</td></tr>
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
                        alert('An error occurred...');
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
            return '<b>No data is available</b>';
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

        //  TODO - Make this work with archers
        return `
            <b style="white-space:nowrap;">Troops</b>
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
        const tableBuilder = new BBTableBuilder();
        tableBuilder.setColumnNames("Source Village", "Launch Time", "Landing Time", "Troop Req.");

        instructions.forEach((i) => {
            tableBuilder.addRow(
                `[url=${lib.makeTwUrl(`village=${i.sourceVillageId}&screen=place&from=simulator&att_${i.troopType}=1&target_village_id=${i.targetVillageId}`)}]${i.sourceVillageName} (${i.sourceVillageX}|${i.sourceVillageY})[/url]`,
                lib.formatDateTime(i.launchAt),
                lib.formatDateTime(i.landsAt),
                `[unit]${i.troopType}[/unit]`
            );
        });

        return tableBuilder.toString();
    }
}