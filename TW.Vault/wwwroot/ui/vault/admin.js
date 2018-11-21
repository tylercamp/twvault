

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
        label: 'Admin Options',
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
            return uilib.mkTabbedContainer(statsTab.containerId, tabs);
        }
    };

    return adminTab;
}

function makeEnemyTribesTab() {

    function insertEnemyTribe($target, tribe) {
        let $row = $(`
            <tr>
                <td style="text-align:right;width:50%;padding-right:0.5em">${tribe.tag}</td>
                <td style="text-align:left;width:50%;padding-left:0.5em"><input type="button" class="delete-enemy" value="Delete"></td>
            </tr>
        `.trim());

        $row.find('.delete-enemy').click(() => {
            if (!confirm(tribe.tag + ' will no longer be considered an enemy.'))
                return;

            lib.deleteApi(`admin/enemies/${tribe.tag}`)
                .done(() => {
                    $row.remove();
                })
                .error(() => {
                    if (!lib.isUnloading())
                        alert('An error occurred...');
                });
        });

        $target.append($row);
    }

    return {
        label: 'Enemy Tribes',
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
                        alert('An error occurred while listing enemy tribes...');
                    }
                });

            $container.find('#new-enemy-button').click(() => {
                let nameOrTag = prompt('Enter the name or tag of the tribe.');
                if (!nameOrTag)
                    return;

                lib.postApi(`admin/enemies/${nameOrTag}`)
                    .done((tribe) => insertEnemyTribe($container.find('#enemies-table'), tribe))
                    .error((xhr) => {
                        if (!lib.isUnloading()) {
                            switch (xhr.status) {
                                case 401: break;
                                case 404:
                                    alert('No tribe exists with that tag or name.');
                                    break;
                                case 409:
                                    alert('That tribe is already registered as an enemy.')
                                    break;
                                default:
                                    alert('An error occurred...');
                                    break;
                            }
                        }
                    })
            });
        },

        getContent: () => `
            <h4>Enemy Tribes</h4>
            <p>
                Tell the Vault which tribes to consider as "enemies" when determining which villages are back-line.
            </p>

            <input type="button" id="new-enemy-button" value="Add Enemy Tribe">

            <table id="enemies-table" style="width:100%;margin-top:1em">
            </table>
        `
    };
}

function makeAdminUsersTab() {
    return {
        label: 'Manage Users',
        containerId: 'vault-admin-users-container',

        getContent: () => `
            <h4>Keys</h4>
            <input type="button" id="new-key-button" value="Make new key">

            <div id="key-script-container" style="display:none">
                <h5 style="margin-top:2em">New Vault Script</h5>
                <textarea cols=100 rows=5></textarea>
            </div>
            <div style="max-height:500px;overflow-y:auto">
                <table id="keys-table" style="width:100%">
                    <tr>
                        <th>User name</th>
                        <th>Current tribe</th>
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
        label: 'Tribe Stats',
        containerId: 'vault-admin-stats-container',

        init: function ($container) {
            uilib.syncProp($container.find('#vault-admin-stats-nuke-breakdown'), options, 'includeNukeBreakdown', saveChanges);

            $container.find('#download-army-stats').click(() => {
                let $downloadButton = $container.find('#download-army-stats');
                let originalText = $downloadButton.val();

                let loading = () => { $downloadButton.val('Working...'); $downloadButton.prop('disabled', true); };
                let loadingDone = () => { $downloadButton.val(originalText); $downloadButton.prop('disabled', false); };

                loading();

                lib.getApi('admin/summary')
                    .error(() => {
                        if (lib.isUnloading())
                            return;
                        alert('An error occurred...');
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
                            alert('An error occurred...');
                            throw e;
                        }
                    });
            });
        },

        getContent: () => `
            <p>Get tribe army stats as a spreadsheet: <input id="download-army-stats" type="button" value="Download"></p>
            <p>
                <input type="checkbox" id="vault-admin-stats-nuke-breakdown"> <label for="vault-admin-stats-nuke-breakdown">Include stats for 1/4, 1/2, and 3/4 nukes</label>
            </p>
        `
    };
}

function makeAdminLogTab() {
    return {
        label: "Log",
        containerId: "vault-admin-log-container",

        getContent: () => `
            <h4>User Log</h4>

            <div style="max-height:500px; overflow-y:auto">
                <table id="admin-logs-table" style="width:100%;font-size:11px">
                    <tr>
                        <th style="width:150px">Admin</th>
                        <th>Event</th>
                        <th style="width:150px">Time</th>
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
                                <td>${log.adminUserName}</td>
                                <td>${log.eventDescription}</td>
                                <td>${lib.formatDateTime(log.occurredAt)}</td>
                            </tr>
                        `.trim());
            });
        })
        .error(() => {
            if (lib.isUnloading())
                return;

            alert('An error occurred...');
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
                alert('An error occurred...');
            }
        });

    //  Logic for making a new auth key
    $container.find('#new-key-button').click(() => {
        var username = prompt("Enter the username or ID");
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
                    alert('An error occurred...');
                }
            });
    });


    function insertNewAuthKey(user) {

        var $newRow = $(`
                <tr data-auth-key="${user.key}">
                    <td>${user.playerName + (user.isAdmin ? " <b>(Admin)</b>" : "")}</td>
                    <td>${user.tribeName || "(No tribe)"}</td>
                    <td><input type="button" class="get-script" value="Get script"></td>
                    <td><input type="button" class="delete-user" value="Delete"></td>
                    <td><input type="button" class="give-admin" value="${user.isAdmin ? 'Revoke admin' : 'Make admin'}"></td>
                </tr>
            `.trim());

        $newRow.find('.get-script').click(() => displayUserScript(user));

        $newRow.find('input.delete-user').click(() => {
            if (!confirm(user.playerName + ' will have their auth key removed.'))
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
                        alert('An error occurred...');
                    }
                });
        });

        $newRow.find('input.give-admin').click(() => {
            let updatedAdmin = !user.isAdmin;
            let message = '';

            if (!updatedAdmin) {
                message = `${user.playerName} will no longer have admin rights.`;
            } else {
                message =
                    `${user.playerName} will be given admin status, and will be able to:\n` +
                    "\n- Access all troop information available" +
                    "\n- Add new users" +
                    "\n- Give and revoke admin priveleges for users";
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

                    alert('An error occurred...');
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

        $('#key-script-container h5').text(`Vault Script for: ${user.playerName}`);
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

    let nukeBreakdownHeaders = ['1/4 Nukes', '1/2 Nukes', '3/4 Nukes'];
    if (!options.includeNukeBreakdown)
        nukeBreakdownHeaders = [];

    csvBuilder.addRow('', '', '', '', 'Total full nukes', 'Total Nobles', 'Total Possible Nobles', 'Total DVs', 'Total Incs', 'Total Attacks');
    csvBuilder.addRow('', '', '', '', totalNukes, totalNobles, totalPossibleNobles, totalDVs, totalIncomings, totalAttacks);

    csvBuilder.addBlank(2);

    csvBuilder.addRow(
        'Time', 'Needs upload?', 'Tribe', 'Player', ...nukeBreakdownHeaders, 'Full Nukes',
        'Nukes traveling', 'Nobles', 'Possible nobles', 
        'Owned DVs', 'DVs at Home', 'Backline DVs at Home', 'DVs Traveling', 'DVs Supporting Self', 'DVs Supporting Others',
        'Est. Off. Villas', 'Est. Def. Villas', '# Incs', '# Attacks',
        '',
        ...supportedTribeNames.map((tn) => `DVs to ${tn}`)
    );

    playerSummaries.forEach((s) => {
        csvBuilder.addRow(
            s.uploadedAt, s.needsUpload ? 'YES' : '', s.tribeName, s.playerName, ...s.nukeBreakdown, s.numNukes,
            s.numNukesTraveling, s.numNobles, s.numPossibleNobles,
            s.numOwnedDVs, s.numDVsAtHome, s.numDVsAtHomeBackline, s.numDVsTraveling, s.numDVsSupportingSelf, s.numDVsSupportingOthers,
            s.numOffensiveVillas, s.numDefensiveVillas, s.numIncomings, s.numAttacks, '', ...supportedTribeNames.map((tn) => s.numDVsSupportingTribes[tn] || '0')
        );
    });

    return csvBuilder.makeCsvString();
}