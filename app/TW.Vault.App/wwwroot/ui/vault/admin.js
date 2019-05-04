

function makeAdminTab() {

    let usersTab = makeAdminUsersTab();
    let enemiesTab = makeEnemyTribesTab();
    let statsTab = makeAdminStatsTab();
    let logTab = makeAdminLogTab();

    let tabs = [
        statsTab,
        enemiesTab,
        usersTab,
        logTab
    ];

    let adminTab = {
        label: lib.translate(lib.itlcodes.TAB_ADMIN),
        containerId: 'vault-admin-container',
        btnCss: 'display:none',

        init: function ($container) {
            lib.getApi('admin')
                .done((data) => {
                    if (typeof data == 'string')
                        data = JSON.parse(data);

                    console.log('isAdmin = ', data.isAdmin);

                    if (data.isAdmin) {
                        $('#' + adminTab.btnId).css('display', 'inline-block');
                        makeAdminUsersInterface($container);

                        enemiesTab.isAdmin = true;
                        enemiesTab.init.call(enemiesTab, $(`#${enemiesTab.containerId}`));
                    }
                });
        },

        getContent: function () {
            return uilib.mkTabbedContainer(statsTab, tabs);
        }
    };

    return adminTab;
}

function makeEnemyTribesTab() {

    function insertEnemyTribe($target, tribe) {
        let $row = $(`
            <tr>
                <td style="text-align:right;width:50%;padding-right:0.5em">${tribe.tag}</td>
                <td style="text-align:left;width:50%;padding-left:0.5em"><input type="button" class="delete-enemy" value="${lib.translate(lib.itlcodes.DELETE)}"></td>
            </tr>
        `.trim());

        $row.find('.delete-enemy').click(() => {
            if (!confirm(lib.translate(lib.itlcodes.ADMIN_REMOVE_ENEMY, { tribeName: tribe.tag })))
                return;

            lib.deleteApi(`admin/enemies/${tribe.tag}`)
                .done(() => {
                    $row.remove();
                })
                .error(() => {
                    if (!lib.isUnloading())
                        alert(lib.messages.GENERIC_ERROR);
                });
        });

        $target.append($row);
    }

    return {
        label: lib.translate(lib.itlcodes.TAB_ENEMY_TRIBES),
        containerId: 'vault-admin-enemy-tribes',

        isAdmin: false,

        init: function ($container) {
            if (!this.isAdmin)
                return;

            lib.getApi('admin/enemies')
                .done((data) => {
                    data.forEach((tribe) => {
                        insertEnemyTribe($container.find('#enemies-table'), tribe);
                    });
                })
                .error(() => {
                    if (!lib.isUnloading()) {
                        alert(lib.translate(lib.itlcodes.ERROR_LOADING_ENEMY_TRIBES, { _escaped: false }));
                    }
                });

            $container.find('#new-enemy-button').click(() => {
                let nameOrTag = prompt(lib.translate(lib.itlcodes.ADMIN_NAME_OF_TRIBE));
                if (!nameOrTag)
                    return;

                lib.postApi(`admin/enemies/${nameOrTag}`)
                    .done((tribe) => insertEnemyTribe($container.find('#enemies-table'), tribe))
                    .error((xhr) => {
                        if (!lib.isUnloading()) {
                            switch (xhr.status) {
                                case 401: break;
                                case 404:
                                    alert(lib.translate(lib.itlcodes.ADMIN_TRIBE_NOT_FOUND, { _escaped: false }));
                                    break;
                                case 409:
                                    alert(lib.translate(lib.itlcodes.ADMIN_TRIBE_ALREADY_EXISTS, { _escaped: false }));
                                    break;
                                default:
                                    alert(lib.messages.GENERIC_ERROR);
                                    break;
                            }
                        }
                    })
            });
        },

        getContent: `
            <h4>${lib.translate(lib.itlcodes.ADMIN_ENEMY_TRIBES)}</h4>
            <p>
                ${lib.translate(lib.itlcodes.ADMIN_ENEMY_TRIBES_DESCRIPTION)}
            </p>

            <input type="button" id="new-enemy-button" value="${lib.translate(lib.itlcodes.ADMIN_ADD_ENEMY_TRIBE)}">

            <table id="enemies-table" style="width:100%;margin-top:1em">
            </table>
        `
    };
}

function makeAdminUsersTab() {
    return {
        label: lib.translate(lib.itlcodes.TAB_MANAGE_USERS),
        containerId: 'vault-admin-users-container',

        getContent: `
            <h4>${lib.translate(lib.itlcodes.TAB_MANAGE_USERS)}</h4>
            <input type="button" id="new-key-button" value="${lib.translate(lib.itlcodes.ADMIN_NEW_KEY)}">

            <div id="key-script-container" style="display:none">
                <h5 style="margin-top:2em">${lib.translate(lib.itlcodes.ADMIN_NEW_VAULT_SCRIPT)}</h5>
                <textarea cols=100 rows=5></textarea>
            </div>
            <div style="max-height:500px;overflow-y:auto">
                <table id="keys-table" style="width:100%">
                    <tr>
                        <th>${lib.translate(lib.itlcodes.USER_NAME)}</th>
                        <th>${lib.translate(lib.itlcodes.CURRENT_TRIBE)}</th>
                        <th></th>
                        <th></th>
                        <th></th>
                    </tr>
                </table>
            </div>
        `
    };
}

function makeAdminStatsTab() {

    var options = lib.getLocalStorage('admin-stats-options', {
        includeNukeBreakdown: false
    });

    function saveChanges() {
        lib.setLocalStorage('admin-stats-options', options);
    }

    return {
        label: lib.translate(lib.itlcodes.TAB_TRIBE_STATS),
        containerId: 'vault-admin-stats-container',

        init: function ($container) {
            uilib.syncProp($container.find('#vault-admin-stats-nuke-breakdown'), options, 'includeNukeBreakdown', saveChanges);

            $container.find('#download-army-stats').click(() => {
                let $downloadButton = $container.find('#download-army-stats');
                let originalText = $downloadButton.val();

                let loading = () => { $downloadButton.val(lib.translate(lib.itlcodes.WORKING, { _escaped: false })); $downloadButton.prop('disabled', true); };
                let loadingDone = () => { $downloadButton.val(originalText); $downloadButton.prop('disabled', false); };

                loading();

                lib.getApi('admin/summary')
                    .error(() => {
                        if (lib.isUnloading())
                            return;
                        alert(lib.messages.GENERIC_ERROR);
                        loadingDone();
                    })
                    .done((data) => {
                        if (typeof data == 'string')
                            data = JSON.parse(data);

                        console.log('Got data: ', data);

                        try {
                            let csvText = makeArmySummaryCsv(data, options);
                            let filename = `army-summary.csv`;

                            lib.saveAsFile(filename, csvText);
                            loadingDone();

                        } catch (e) {
                            loadingDone();
                            alert(lib.messages.GENERIC_ERROR);
                            throw e;
                        }
                    });
            });
        },

        getContent: `
            <p>${lib.translate(lib.itlcodes.ADMIN_TRIBE_STATS_DESCRIPTION)} <input id="download-army-stats" type="button" value="${lib.translate(lib.itlcodes.DOWNLOAD)}"></p>
            <p>
                <input type="checkbox" id="vault-admin-stats-nuke-breakdown"> <label for="vault-admin-stats-nuke-breakdown">${lib.translate(lib.itlcodes.ADMIN_TRIBE_STATS_SETTINGS_NUKES)}</label>
            </p>
        `
    };
}

function makeAdminLogTab() {
    return {
        label: lib.translate(lib.itlcodes.TAB_LOG),
        containerId: "vault-admin-log-container",

        getContent: `
            <h4>${lib.translate(lib.itlcodes.ADMIN_USER_LOG)}</h4>

            <div style="max-height:500px; overflow-y:auto">
                <table id="admin-logs-table" style="width:100%;font-size:11px">
                    <tr>
                        <th style="width:150px">${lib.translate(lib.itlcodes.ADMIN)}</th>
                        <th>${lib.translate(lib.itlcodes.EVENT)}</th>
                        <th style="width:150px">${lib.translate(lib.itlcodes.TIME)}</th>
                    </tr>
                </table>
            </div>
        `
    };
}


function makeAdminUsersInterface($container) {

    lib.getApi('admin/logs')
        .done((logs) => {
            console.log('Got admin logs: ', logs);

            let $table = $container.find('#admin-logs-table');
            logs.forEach((log) => {
                $table.append(`
                            <tr>
                                <td>${lib.escapeHtml(log.adminUserName)}</td>
                                <td>${lib.escapeHtml(log.eventDescription)}</td>
                                <td>${lib.formatDateTime(log.occurredAt)}</td>
                            </tr>
                        `.trim());
            });
        })
        .error(() => {
            if (lib.isUnloading())
                return;

            alert(lib.messages.GENERIC_ERROR);
        });

    //  Insert existing keys
    lib.getApi('admin/keys')
        .done((data) => {
            if (typeof data == 'string')
                data = JSON.parse(data);

            data.forEach((d) => insertNewAuthKey(d));
        })
        .error((xhr) => {
            if (lib.isUnloading())
                return;

            if (xhr.responseText) {
                let error = JSON.parse(xhr.responseText).error;
                alert(error);
            } else {
                alert(lib.messages.GENERIC_ERROR);
            }
        });

    //  Logic for making a new auth key
    $container.find('#new-key-button').click(() => {
        var username = prompt(lib.translate(lib.itlcodes.ADMIN_MANAGE_USERS_ENTER_NAME));
        if (!username)
            return;

        let isName = !!username.match(/[^\d]/);
        lib.postApi('admin/keys', {
            playerId: isName ? null : parseInt(username),
            playerName: isName ? username : null,
            newUserIsAdmin: false
        })
            .done((data) => {
                if (typeof data == 'string')
                    data = JSON.parse(data);
                insertNewAuthKey(data);
                displayUserScript(data);
            })
            .error((xhr) => {
                if (lib.isUnloading())
                    return;

                if (xhr.responseText) {
                    let error = JSON.parse(xhr.responseText).error;
                    alert(error);
                } else {
                    alert(lib.messages.GENERIC_ERROR);
                }
            });
    });


    function insertNewAuthKey(user) {
        
        var $newRow = $(`
                <tr data-auth-key="${user.key}">
                    <td>${user.playerName + (user.isAdmin ? ` <b>(${lib.translate(lib.itlcodes.ADMIN)})</b>` : "")}</td>
                    <td>${user.tribeName || `(${lib.translate(lib.itlcodes.NO_TRIBE)})`}</td>
                    <td><input type="button" class="get-script" value="${lib.translate(lib.itlcodes.GET_SCRIPT)}"></td>
                    <td><input type="button" class="delete-user" value="${lib.translate(lib.itlcodes.DELETE)}"></td>
                    <td><input type="button" class="give-admin" value="${user.isAdmin ? lib.translate(lib.itlcodes.ADMIN_MANAGE_USERS_REVOKE_ADMIN) : lib.translate(lib.itlcodes.ADMIN_MANAGE_USERS_GIVE_ADMIN)} "></td>
                </tr>
            `.trim());

        $newRow.find('.get-script').click(() => displayUserScript(user));

        $newRow.find('input.delete-user').click(() => {
            if (!confirm(lib.translate(lib.itlcodes.ADMIN_MANAGE_USERS_CONFIRM_DELETE, { playerName: user.playerName })))
                return;

            let authKey = user.key;
            lib.deleteApi(`admin/keys/${authKey}`)
                .done(() => {
                    $newRow.remove();
                })
                .error((xhr) => {
                    if (lib.isUnloading())
                        return;

                    if (xhr.responseText) {
                        let error = JSON.parse(xhr.responseText).error;
                        alert(error);
                    } else {
                        alert(lib.messages.GENERIC_ERROR);
                    }
                });
        });

        $newRow.find('input.give-admin').click(() => {
            let updatedAdmin = !user.isAdmin;
            let message = '';

            if (!updatedAdmin) {
                message = lib.translate(lib.itlcodes.ADMIN_MANAGE_USERS_CONFIRM_REMOVE_ADMIN, { playerName: user.playerName });
            } else {
                message = lib.translate(lib.itlcodes.ADMIN_MANAGE_USERS_CONFIRM_GIVE_ADMIN, { playerName: user.playerName });
            }

            if (!confirm(message))
                return;

            let authKey = user.key;
            lib.postApi(`admin/keys/${authKey}/setAdmin`, { hasAdmin: updatedAdmin })
                .done(() => {
                    user.isAdmin = updatedAdmin;
                    if (updatedAdmin)
                        $newRow.find('input.give-admin').val('Revoke admin');
                    else
                        $newRow.find('input.give-admin').val('Make admin');
                })
                .error(() => {
                    if (lib.isUnloading())
                        return;

                    alert(lib.messages.GENERIC_ERROR);
                });
        });

        $container.find('#keys-table tbody').append($newRow);
    }

    function displayUserScript(user) {
        var scriptString = 'javascript:';
        scriptString += `window.vaultToken="${user.key}";`;

        let scriptPath = lib.getScriptHost();
        //let scriptPath = `https://v.tylercamp.me/script/main.js`;
        scriptString += `$.getScript("${scriptPath}");`;

        $('#key-script-container textarea').val(scriptString);
        $('#key-script-container').css('display', 'block');

        $('#key-script-container h5').html(lib.translate(lib.itlcodes.ADMIN_MANAGE_USERS_VAULT_SCRIPT_FOR, { playerName: user.playerName }));
    }
}

function makeArmySummaryCsv(armyData, options) {
    let nukePower = 400000;
    let nukePop = 15000;
    let fullDVPop = 20000;

    let playerSummaries = [];

    var totalNukes = 0;
    var totalDVs = 0;
    var totalNobles = 0;
    var totalPossibleNobles = 0;
    var totalIncomings = 0;
    var totalAttacks = 0;

    let supportedTribeNames = [];

    let round = (num) => Math.roundTo(num, 1);
    
    armyData.forEach((ad) => {
        let playerId = ad.playerId;
        let playerName = ad.playerName;
        let maxNobles = ad.maxPossibleNobles;

        let playerData = {
            playerId: playerId,
            playerName: playerName,
            tribeName: ad.tribeName,
            nukeBreakdown: options.includeNukeBreakdown ? [
                ad.quarterNukesOwned, ad.halfNukesOwned, ad.threeQuarterNukesOwned
            ] : [],
            numNukes: ad.nukesOwned,
            numNukesTraveling: ad.nukesTraveling,
            numNobles: ad.numNobles,
            numPossibleNobles: maxNobles,

            numOwnedDVs: round(ad.dVsOwned),
            numDVsAtHome: round(ad.dVsAtHome),
            numDVsAtHomeBackline: round(ad.dVsAtHomeBackline),
            numDVsTraveling: round(ad.dVsTraveling),
            numDVsSupportingOthers: round(ad.dVsSupportingOthers),
            numDVsSupportingSelf: round(ad.dVsSupportingSelf),
            numDVsSupportingTribes: {},

            numDefensiveVillas: ad.numDefensiveVillages,
            numOffensiveVillas: ad.numOffensiveVillages,

            numIncomings: ad.numIncomings,
            numAttacks: ad.numAttackCommands
        };

        let uploadAge = ad.uploadAge.split(':')[0];
        let uploadAgeDays = uploadAge.contains(".") ? uploadAge.split('.')[0] : '0';
        let uploadAgeHours = uploadAge.contains(".") ? uploadAge.split('.')[1] : uploadAge;
        playerData.needsUpload = parseInt(uploadAgeDays) * 24 + parseInt(uploadAgeHours) > 24;

        let uploadedAt = new Date(ad.uploadedAt);
        playerData.uploadedAt = `${uploadedAt.getUTCMonth() + 1}/${uploadedAt.getUTCDate()}/${uploadedAt.getUTCFullYear()}`;

        

        lib.objForEach(ad.supportPopulationByTargetTribe, (tribe, pop) => {
            playerData.numDVsSupportingTribes[tribe] = round(pop / fullDVPop);
            if (!supportedTribeNames.contains(tribe))
                supportedTribeNames.push(tribe);
        });

        totalNukes += playerData.numNukes;
        totalDVs += playerData.numOwnedDVs;
        totalNobles += playerData.numNobles;
        totalPossibleNobles += playerData.numPossibleNobles;
        totalIncomings += playerData.numIncomings;
        totalAttacks += playerData.numAttacks;

        playerSummaries.push(playerData);
    });

    console.log('Made player summaries: ', playerSummaries);


    var csvBuilder = new CsvBuilder();
    supportedTribeNames.sort();

    const NOESC = { _escaped: false };

    let nukeBreakdownHeaders = [
        lib.translate(lib.itlcodes.ADMIN_TRIBE_STATS_25_NUKES, NOESC),
        lib.translate(lib.itlcodes.ADMIN_TRIBE_STATS_50_NUKES, NOESC),
        lib.translate(lib.itlcodes.ADMIN_TRIBE_STATS_75_NUKES, NOESC)
    ];

    if (!options.includeNukeBreakdown)
        nukeBreakdownHeaders = [];

    csvBuilder.addRow(
        '', '', '', '',
        lib.translate(lib.itlcodes.ADMIN_TRIBE_STATS_TOTAL_FULL_NUKES, NOESC),
        lib.translate(lib.itlcodes.ADMIN_TRIBE_STATS_TOTAL_NOBLES, NOESC),
        lib.translate(lib.itlcodes.ADMIN_TRIBE_STATS_TOTAL_POSSIBLE_NOBLES, NOESC),
        lib.translate(lib.itlcodes.ADMIN_TRIBE_STATS_TOTAL_DVS, NOESC),
        lib.translate(lib.itlcodes.ADMIN_TRIBE_STATS_TOTAL_INCS, NOESC),
        lib.translate(lib.itlcodes.ADMIN_TRIBE_STATS_TOTAL_ATTACKS, NOESC)
    );

    csvBuilder.addRow('', '', '', '', totalNukes, totalNobles, totalPossibleNobles, totalDVs, totalIncomings, totalAttacks);

    csvBuilder.addBlank(2);

    csvBuilder.addRow(
        lib.translate(lib.itlcodes.TIME, NOESC),
        lib.translate(lib.itlcodes.ADMIN_TRIBE_STATS_NEEDS_UPLOAD, NOESC),
        lib.translate(lib.itlcodes.TRIBE, NOESC),
        lib.translate(lib.itlcodes.PLAYER, NOESC),
        ...nukeBreakdownHeaders,
        lib.translate(lib.itlcodes.ADMIN_TRIBE_STATS_FULL_NUKES, NOESC),
        lib.translate(lib.itlcodes.ADMIN_TRIBE_STATS_NUKES_TRAVELING, NOESC),
        lib.translate(lib.itlcodes.NOBLES, NOESC),
        lib.translate(lib.itlcodes.POSSIBLE_NOBLES, NOESC),
        lib.translate(lib.itlcodes.ADMIN_TRIBE_STATS_OWNED_DVS, NOESC),
        lib.translate(lib.itlcodes.ADMIN_TRIBE_STATS_DVS_HOME, NOESC),
        lib.translate(lib.itlcodes.ADMIN_TRIBE_STATS_BACKLINE_DVS_HOME, NOESC),
        lib.translate(lib.itlcodes.ADMIN_TRIBE_STATS_DVS_TRAVELING, NOESC),
        lib.translate(lib.itlcodes.ADMIN_TRIBE_STATS_DVS_TO_SELF, NOESC),
        lib.translate(lib.itlcodes.ADMIN_TRIBE_STATS_DVS_TO_OTHERS, NOESC),
        lib.translate(lib.itlcodes.ADMIN_TRIBE_STATS_OFF_VILLAS, NOESC),
        lib.translate(lib.itlcodes.ADMIN_TRIBE_STATS_DEF_VILLAS, NOESC),
        lib.translate(lib.itlcodes.ADMIN_TRIBE_STATS_NUM_INCS, NOESC),
        lib.translate(lib.itlcodes.ADMIN_TRIBE_STATS_NUM_ATTACKS, NOESC),
        '',
        ...supportedTribeNames.map((tn) => lib.translate(lib.itlcodes.ADMIN_TRIBE_STATS_DVS_TO_TRIBE, { tribeName: tn, _escaped: false }))
    );

    playerSummaries.forEach((s) => {
        csvBuilder.addRow(
            s.uploadedAt, s.needsUpload ? lib.translate(lib.itlcodes.YES) : '', s.tribeName, s.playerName, ...s.nukeBreakdown, s.numNukes,
            s.numNukesTraveling, s.numNobles, s.numPossibleNobles,
            s.numOwnedDVs, s.numDVsAtHome, s.numDVsAtHomeBackline, s.numDVsTraveling, s.numDVsSupportingSelf, s.numDVsSupportingOthers,
            s.numOffensiveVillas, s.numDefensiveVillas, s.numIncomings, s.numAttacks, '', ...supportedTribeNames.map((tn) => s.numDVsSupportingTribes[tn] || '0')
        );
    });

    return csvBuilder.makeCsvString();
}