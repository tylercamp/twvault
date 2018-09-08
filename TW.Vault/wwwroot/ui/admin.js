﻿

function makeAdminInterface($adminContainer) {
    $adminContainer.append(`
            <button class="btn btn-confirm-yes vault-toggle-admin-btn">Admin Options</button>
            <div id="admin-inner-container" style="display:none">
                <div>
                    Get tribe army stats as a spreadsheet: <input id="download-army-stats" type="button" value="Download">
                </div>
                <div id="keys-container">
                    <h4>Keys</h4>
                    <table id="keys-table" style="width:100%">
                        <tr>
                            <th>User name</th>
                            <th>Current tribe</th>
                            <th>Auth key</th>
                            <th></th>
                            <th></th>
                        </tr>
                    </table>
                    <input type="button" id="new-key-button" value="Make new key">

                    <div id="key-script-container" style="display:none">
                        <h5 style="margin-top:2em">New Vault Script</h5>
                        <textarea cols=100 rows=5></textarea>
                    </div>
                </div>
            </div>
        `.trim());

    $adminContainer.find('.vault-toggle-admin-btn').click(() => {
        $adminContainer.find('#admin-inner-container').toggle();
    });

    //  Insert existing keys
    lib.getApi(lib.makeApiUrl('admin/keys'))
        .done((data) => {
            if (typeof data == 'string')
                data = JSON.parse(data);

            data.forEach((d) => insertNewAuthKey(d));
        })
        .error((xhr) => {
            if (xhr.responseText) {
                let error = JSON.parse(xhr.responseText).error;
                alert(error);
            } else {
                alert('An error occurred...');
            }
        });

    $adminContainer.find('#download-army-stats').click(() => {
        lib.getApi(lib.makeApiUrl('admin/summary'))
            .error(() => alert('An error occurred...'))
            .done((data) => {
                if (typeof data == 'string')
                    data = JSON.parse(data);

                try {
                    let csvText = makeArmySummaryCsv(data);
                    let filename = `army-summary.csv`;

                    lib.saveAsFile(filename, csvText);

                } catch (e) {
                    alert('An error occurred...');
                    throw e;
                }
            });
    });

    //  Logic for making a new auth key
    $adminContainer.find('#new-key-button').click(() => {
        var username = prompt("Enter the username or ID");
        if (!username)
            return;

        lib.postApi(lib.makeApiUrl('admin/keys'), {
            playerId: isNaN(parseInt(username)) ? null : parseInt(username),
            playerName: isNaN(parseInt(username)) ? username : null,
            newUserIsAdmin: false
        })
            .done((data) => {
                if (typeof data == 'string')
                    data = JSON.parse(data);
                insertNewAuthKey(data);
                displayUserScript(data);
            })
            .error((xhr) => {
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
                    <td>${user.playerName}</td>
                    <td>${user.tribeName}</td>
                    <td>${user.key || '-'}</td>
                    <td><input type="button" class="get-script" value="Get script"></td>
                    <td><input type="button" class="delete-user" value="Delete"></td>
                </tr>
            `.trim());

        $newRow.find('.get-script').click(() => displayUserScript(user));

        $newRow.find('input.delete-user').click(() => {
            if (!confirm(user.playerName + ' will have their auth key removed.'))
                return;

            let authKey = user.key;
            lib.deleteApi(lib.makeApiUrl(`admin/keys/${authKey}`))
                .done((data) => {
                    $newRow.remove();
                })
                .error((xhr) => {
                    if (xhr.responseText) {
                        let error = JSON.parse(xhr.responseText).error;
                        alert(error);
                    } else {
                        alert('An error occurred...');
                    }
                });
        });

        $adminContainer.find('#keys-table').append($newRow);
    }

    function displayUserScript(user) {
        var scriptString = 'javascript:';
        scriptString += `window.vaultToken="${user.key}";`;

        //let scriptPath = `https://${lib.getScriptHost()}`;
        let scriptPath = `https://v.tylercamp.me/script/main.js`;
        scriptString += `$.getScript("${scriptPath}");`;

        $('#key-script-container textarea').val(scriptString);
        $('#key-script-container').css('display', 'block');

        $('#key-script-container h5').text(`Vault Script for: ${user.playerName}`);
    }
}

function processUploadReports($statusContainer, onDone) {
    $.get(lib.makeTwUrl(lib.pageTypes.ALL_REPORTS)).done((data) => {
        try {
            if (lib.checkContainsCaptcha(data)) {
                return onDone(lib.errorCodes.CAPTCHA);
            }

            let $doc = $(data);
            parseAllReports($doc, (msg) => {
                $statusContainer.text(msg);
            }, (didFail) => {
                onDone(didFail);
            });
        } catch (e) {
            $statusContainer.text('An error occurred.');
            console.error(e);

            onDone(true);
        }
    });
}

function processUploadIncomings($statusContainer, onDone) {
    $.get(lib.makeTwUrl(lib.pageTypes.INCOMINGS_OVERVIEW)).done((data) => {
        try {
            if (lib.checkContainsCaptcha(data)) {
                return onDone(lib.errorCodes.CAPTCHA);
            }

            let $doc = $(data);
            parseAllIncomings($doc, (msg) => {
                $statusContainer.text(msg);
            }, (didFail) => {
                onDone(didFail);
            });
        } catch (e) {
            $statusContainer.text('An error occurred.');
            console.error(e);

            onDone(true);
        }
    });
}

function processUploadCommands($statusContainer, onDone) {
    $.get(lib.makeTwUrl(lib.pageTypes.OWN_COMMANDS_OVERVIEW)).done((data) => {
        try {
            if (lib.checkContainsCaptcha(data)) {
                return onDone(lib.errorCodes.CAPTCHA);
            }

            let $doc = $(data);
            parseAllCommands($doc, (msg) => {
                $statusContainer.text(msg);
            }, (didFail) => {
                onDone(didFail);
            });
        } catch (e) {
            $statusContainer.text('An error occurred.');
            console.error(e);

            onDone(true);
        }
    });
}

function processUploadTroops($statusContainer, onDone) {
    $.get(lib.makeTwUrl(lib.pageTypes.OWN_TROOPS_OVERVIEW)).done((data) => {
        try {
            if (lib.checkContainsCaptcha(data)) {
                return onDone(lib.errorCodes.CAPTCHA);
            }

            let $doc = $(data);
            parseAllTroops($doc, (msg) => {
                $statusContainer.text(msg);
            }, (didFail) => {
                onDone(didFail);
            });
        } catch (e) {
            $statusContainer.text('An error occurred.');
            console.error(e);

            onDone(true);
        }
    });
}

function makeArmySummaryCsv(armyData) {
    let fullNukePop = 20000;
    let almostNukePop = 15000;
    let fullDVPop = 20000;

    let playerSummaries = [];

    var totalNukes = 0;
    var totalAlmostNukes = 0;
    var totalDVs = 0;
    var totalNobles = 0;
    var totalPossibleNobles = 0;

    let round = (num) => Math.roundTo(num, 1);

    let offensiveArmyPopulation = (army) => {
        let offensiveArmy = lib.twcalc.getOffensiveArmy(army);
        let offensiveArmyPop = lib.twcalc.totalPopulation(offensiveArmy);
        return offensiveArmyPop;
    };

    let defensiveArmyPopulation = (army) => {
        let defensiveArmy = lib.twcalc.getDefensiveArmy(army);
        let defensiveArmyPop = lib.twcalc.totalPopulation(defensiveArmy);
        return defensiveArmyPop;
    };

    armyData.forEach((ad) => {
        let playerId = ad.playerId;
        let playerName = ad.playerName;
        let maxNobles = ad.maxPossibleNobles;

        let playerData = {
            playerId: playerId,
            playerName: playerName,
            numNukes: 0,
            numAlmostNukes: 0,
            numNukesTraveling: 0,
            numNobles: 0,
            numPossibleNobles: maxNobles,

            numOwnedDVs: 0,
            numDVsAtHome: 0,
            numDVsTraveling: 0,
            numDVsSupportingOthers: 0,
            numDVsSupportingSelf: 0,

            numDefensiveVillas: 0,
            numOffensiveVillas: 0
        };

        let uploadAge = ad.uploadAge.split(':')[0];
        let uploadAgeDays = uploadAge.contains(".") ? uploadAge.split('.')[0] : '0';
        let uploadAgeHours = uploadAge.contains(".") ? uploadAge.split('.')[1] : uploadAge;
        playerData.needsUpload = parseInt(uploadAgeDays) * 24 + parseInt(uploadAgeHours) > 24;

        let uploadedAt = new Date(ad.uploadedAt);
        playerData.uploadedAt = `${uploadedAt.getUTCMonth() + 1}/${uploadedAt.getUTCDate()}/${uploadedAt.getUTCFullYear()}`;

        let armiesOwned = ad.armiesOwned;
        let armiesTraveling = ad.armiesTraveling;
        let armyTraveling = ad.armyTraveling;
        let armyAtHome = ad.armyAtHome;
        let armySupportingOthers = ad.armySupportingOthers;
        let armySupportingSelf = ad.armySupportingSelf;

        armiesOwned.forEach((army) => {
            let offensiveArmyPop = offensiveArmyPopulation(army);
            let defensiveArmyPop = defensiveArmyPopulation(army);

            if (offensiveArmyPop >= fullNukePop) {
                playerData.numNukes++;
            } else if (offensiveArmyPop >= almostNukePop) {
                playerData.numAlmostNukes++;
            }

            if (defensiveArmyPop > 2000 && defensiveArmyPop > offensiveArmyPop) {
                playerData.numDefensiveVillas++;
                playerData.numOwnedDVs += defensiveArmyPop / fullDVPop;
            } else if (offensiveArmyPop > 2000 && offensiveArmyPop > defensiveArmyPop) {
                playerData.numOffensiveVillas++;
            }

            if (army.snob) {
                playerData.numNobles += army.snob;
            }
        });

        armiesTraveling.forEach((army) => {
            let offensivePop = offensiveArmyPopulation(army);
            if (offensivePop > fullNukePop / 2) {
                playerData.numNukesTraveling += offensivePop / fullNukePop;
            }
        });

        playerData.numOwnedDVs = round(playerData.numOwnedDVs);
        playerData.numNukesTraveling = round(playerData.numNukesTraveling);

        if (armyAtHome) {
            let defensiveAtHomePop = defensiveArmyPopulation(armyAtHome);
            playerData.numDVsAtHome = round(defensiveAtHomePop / fullDVPop);
        }

        if (armyTraveling) {
            let defensiveTravelingPop = defensiveArmyPopulation(armyTraveling);
            playerData.numDVsTraveling = round(defensiveTravelingPop / fullDVPop);
        }

        if (armySupportingOthers) {
            let defensiveSupportingOthersPop = defensiveArmyPopulation(armySupportingOthers);
            playerData.numDVsSupportingOthers = round(defensiveSupportingOthersPop / fullDVPop);
        }

        if (armySupportingSelf) {
            let defensiveSupportingSelfPop = defensiveArmyPopulation(armySupportingSelf);
            playerData.numDVsSupportingSelf = round(defensiveSupportingSelfPop / fullDVPop);
        }

        totalNukes += playerData.numNukes;
        totalAlmostNukes += playerData.numAlmostNukes;
        totalDVs += playerData.numOwnedDVs;
        totalNobles += playerData.numNobles;
        totalPossibleNobles += playerData.numPossibleNobles;

        playerSummaries.push(playerData);
    });

    console.log('Made player summaries: ', playerSummaries);



    var csvBuilder = new CsvBuilder();

    csvBuilder.addRow('', '', '', 'Total nukes', 'Total 3/4 nukes', 'Total Nobles', 'Total Possible Nobles', 'Total DVs');
    csvBuilder.addRow('', '', '', totalNukes, totalAlmostNukes, totalNobles, totalPossibleNobles, totalDVs);

    csvBuilder.addBlank(2);

    csvBuilder.addRow(
        'Time', 'Needs upload?', 'Player', 'Nukes',
        '3/4 Nukes', 'Nukes traveling', 'Nobles', 'Possible nobles', 
        'Owned DVs', 'DVs at Home', 'DVs Traveling', 'DVs Supporting Self', 'DVs Supporting Others',
        'Est. Off. Villas', 'Est. Def. Villas',
    );

    playerSummaries.forEach((s) => {
        csvBuilder.addRow(
            s.uploadedAt, s.needsUpload ? 'YES' : '', s.playerName, s.numNukes,
            s.numAlmostNukes, s.numNukesTraveling, s.numNobles, s.numPossibleNobles,
            s.numOwnedDVs, s.numDVsAtHome, s.numDVsTraveling, s.numDVsSupportingSelf, s.numDVsSupportingOthers,
            s.numOffensiveVillas, s.numDefensiveVillas
        );
    });

    return csvBuilder.makeCsvString();
}