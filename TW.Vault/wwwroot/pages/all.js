function parseAllPages($doc) {
    $doc = $doc || $(document);

    $doc.find('#vault-ui-container').remove();
    var $uiContainer = $(`
        <div id="vault-ui-container" class="confirmation-box" style="border-width: 20px">
        <div class="confirmation-box-content-pane" style="min-height:100%">
        <div class="confirmation-box-content" style="min-height:100%">
            <h3>Vault - Upload Data</h3>
            <p>
                Upload data to the vault using this page. While uploading, keep this running in the background.
                Uploads will be interrupted if you navigate from this page. Be sure to check if Tribal Wars
                bot detection has activated while this script is running.
            </p>

            <table style="width:100%" class="vis lit">
                <tr>
                    <th style="width:12em">Upload</th>
                    <th style="width:6em"></th>
                    <th>Progress</th>
                </tr>
                <tr id="vault-upload-reports" class="lit">
                    <td>Reports</td>
                    <td><input type="button" class="details-button" value="Details"></td>
                    <td>
                        <input type="button" class="upload-button" value="Upload">
                        <span class="status-container"></span>
                        <!-- <input type="button" class="cancel-button" value="Cancel" disabled> -->
                    </td>
                </tr>
                <tr id="vault-upload-incomings">
                    <td>Incomings</td>
                    <td><input type="button" class="details-button" value="Details"></td>
                    <td>
                        <input type="button" class="upload-button" value="Upload">
                        <span class="status-container"></span>
                        <!-- <input type="button" class="cancel-button" value="Cancel" disabled> -->
                    </td>
                </tr>
                <tr id="vault-upload-commands">
                    <td>Commands</td>
                    <td><input type="button" class="details-button" value="Details"></td>
                    <td>
                        <input type="button" class="upload-button" value="Upload">
                        <span class="status-container"></span>
                        <!-- <input type="button" class="cancel-button" value="Cancel" disabled> -->
                    </td>
                </tr>
                <tr id="vault-upload-troops">
                    <td>Troops</td>
                    <td><input type="button" class="details-button" value="Details"></td>
                    <td>
                        <input type="button" class="upload-button" value="Upload">
                        <span class="status-container"></span>
                        <!-- <input type="button" class="cancel-button" value="Cancel" disabled> -->
                    </td>
                </tr>
                <tr id="vault-upload-all">
                    <td colspan=3 style="text-align:center">
                        <input type="button" class="upload-button upload-button-all" value="Upload All">
                    </td>
                </tr>
            </table>

            <div id="vault-admin-container"></div>

            <h4>Instructions</h4>
            <p>
                Upload data using the buttons above. Run this script on your Map to show extra village information
                while hovering. Some features may not be available to you until you upload new data.
            </p>

            <hr style="margin:4em 0 2em">

            <p style="font-size:12px">
                ~ Disclaimers ~
            </p>
            <p style="font-size:12px">
                <em>This tool is not endorsed or developed by InnoGames.</em>
            </p>
            <p style="font-size:12px">
                <em>
                    All data and requests to the Vault will have various information logged for security. This is limited to:

                    Authentication token, IP address, player ID, tribe ID, requested endpoint, and time of transaction.

                    Requests to this script will only be IP-logged to protect against abuse. Information collected by this script will never be shared
                    with any third parties or any unauthorized tribes/players.
                </em>
            </p>

            <hr style="margin: 2em 0;">

            <p style="font-size:12px">
                Vault server and script by: Tyler (tcamps/False Duke), Glen (vahtos/TheBossPig)
                <br>
                Please report any bugs and feature requests to the maintainers.
            </p>
        </div>
        <div class="confirmation-buttons">
            <button class="btn vault-close-btn btn-confirm-yes">Done</button>
        </div>
        </div>
        </div>
    `.trim()).css({
        position: 'absolute',
        width: '800px',
        margin: 'auto',
        left: 0, right: 0,
        top: '100px',
        'z-index': 999999999
        });

    $uiContainer.find('th').css({
        'font-size': '14px'
    });

    $uiContainer.find('td').css({
        'font-weight': 'normal'
    });

    $uiContainer.find('th, td').css({
        'text-align': 'center'
    });

    $uiContainer.find('td').addClass('lit-item');

    $uiContainer.find('.upload-button:not(.upload-button-all)').css({
        float: 'left',
        margin: '0 1em'
    });

    $uiContainer.find('.cancel-button').css({
        float: 'right',
        margin: '0 1em'
    });

    $doc.find('body').prepend($uiContainer);
    processAdminInterface();


    $uiContainer.find('.vault-close-btn').click(() => {
        let isUploading = $('.upload-button').prop('disabled');
        if (isUploading && !confirm("Current uploads will continue running while this popup is closed.")) {
            return;
        }
        $uiContainer.remove()
    });



    var uploadDetailsMessages = {
        'vault-upload-reports': `Uploads all data from all new battle reports.`,
        'vault-upload-incomings': `Uploads all available data from your Incomings page. This includes attacks and support.`,
        'vault-upload-commands': `Uploads all data for all of your current commands.`,
        'vault-upload-troops': `Uploads all data for all troops.`
    };

    $uiContainer.find('.details-button').click((ev) => {
        var $el = $(ev.target);
        var uploadType = $el.closest('tr').attr('id');

        alert(uploadDetailsMessages[uploadType]);
    });

    $uiContainer.find('.upload-button').click((ev) => {
        var $el = $(ev.target);
        var $row = $el.closest('tr');
        var uploadType = $row.attr('id');

        var $statusContainer = $row.find('.status-container');

        switch (uploadType) {
            default: alert(`Programmer error: no logic for upload type "${uploadType}"!`);

            case 'vault-upload-reports':
                processUploadReports($statusContainer, (didFail) => {
                    $('.upload-button').prop('disabled', false);
                });
                break;

            case 'vault-upload-incomings':
                processUploadIncomings($statusContainer, (didFail) => {
                    $('.upload-button').prop('disabled', false);
                });
                break;

            case 'vault-upload-commands':
                processUploadCommands($statusContainer, (didFail) => {
                    $('.upload-button').prop('disabled', false);
                });
                break;

            case 'vault-upload-troops':
                processUploadTroops($statusContainer, (didFail) => {
                    $('.upload-button').prop('disabled', false);
                });
                break;

            case 'vault-upload-all':
                $('.status-container').html('<em>Waiting...</em>');

                let runReports = () => {
                    processUploadReports($uiContainer.find('#vault-upload-reports .status-container'), runIncomings);
                };
                let runIncomings = (didFail) => {
                    processUploadIncomings($uiContainer.find('#vault-upload-incomings .status-container'), runCommands);
                };
                let runCommands = (didFail) => {
                    processUploadCommands($uiContainer.find('#vault-upload-commands .status-container'), runTroops);
                };
                let runTroops = (didFail) => {
                    processUploadTroops($uiContainer.find('#vault-upload-troops .status-container'), () => {
                        $('.upload-button').prop('disabled', false);
                    });
                };

                runReports();
                break;
        }

        $('.upload-button').prop('disabled', true);
    });

    $uiContainer.find('.cancel-button').click(() => {
        alert('Not yet implemented!');
    });

    function processAdminInterface() {
        lib.getApi(lib.makeApiUrl('admin'))
            .done((data) => {
                if (typeof data == 'string')
                    data = JSON.parse(data);

                console.log('isAdmin = ', data.isAdmin);

                if (data.isAdmin)
                    makeAdminInterface();
            });
    }

    function makeAdminInterface() {
        var $adminContainer = $uiContainer.find('#vault-admin-container');

        $adminContainer.append(`
            <hr>
            <h3>Admin Options</h3>
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
            <hr>
        `.trim());

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
                    <td>${user.key}</td>
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
                let $doc = $(data);
                parseAllReportsPage($doc, (msg) => {
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
                let $doc = $(data);
                parseIncomingsOverviewPage($doc, (msg) => {
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
                let $doc = $(data);
                parseOwnCommandsOverviewPage($doc, (msg) => {
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
                let $doc = $(data);
                parseOwnTroopsOverviewPage($doc, (msg) => {
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

        armyData.forEach((ad) => {
            let playerId = ad.playerId;
            let playerName = ad.playerName;

            let armies = ad.armies;

            let playerData = {
                playerId: playerId,
                playerName: playerName,
                numNukes: 0,
                numAlmostNukes: 0,
                numDVs: 0,
                numNobles: 0,
                numPossibleNobles: 0
            };

            let uploadAge = ad.uploadAge.split(':')[0];
            let uploadAgeDays = uploadAge.contains(".") ? uploadAge.split('.')[0] : '0';
            let uploadAgeHours = uploadAge.contains(".") ? uploadAge.split('.')[1] : uploadAge;
            playerData.needsUpload = parseInt(uploadAgeDays) * 24 + parseInt(uploadAgeHours) > 24;

            let uploadedAt = new Date(ad.uploadedAt);
            playerData.uploadedAt = `${uploadedAt.getUTCMonth() + 1}/${uploadedAt.getUTCDate()}/${uploadedAt.getUTCFullYear()}`;

            armies.forEach((army) => {
                let offensiveArmy = lib.twcalc.getOffensiveArmy(army);
                let offensiveArmyPop = lib.twcalc.totalPopulation(offensiveArmy);

                let defensiveArmy = lib.twcalc.getDefensiveArmy(army);
                let defensiveArmyPop = lib.twcalc.totalPopulation(defensiveArmy);

                if (offensiveArmyPop >= fullNukePop) {
                    playerData.numNukes++;
                } else if (offensiveArmyPop >= almostNukePop) {
                    playerData.numAlmostNukes++;
                }

                if (defensiveArmyPop > 3000) {
                    playerData.numDVs += defensiveArmyPop / fullDVPop;
                }

                if (army.snob) {
                    playerData.numNobles += army.snob;
                }
            });

            playerData.numDVs = Math.floor(playerData.numDVs);

            totalNukes += playerData.numNukes;
            totalAlmostNukes += playerData.numAlmostNukes;
            totalDVs += playerData.numDVs;
            totalNobles += playerData.numNobles;
            totalPossibleNobles += playerData.numPossibleNobles;

            playerSummaries.push(playerData);
        });

        console.log('Made player summaries: ', playerSummaries);

        var csvString = '';
        csvString += [
            '', '', 'Total nukes', 'Total 3/4 nukes', 'Total DVs', 'Total Nobles', 'Total Possible Nobles'
        ].join(', ');

        csvString += '\n';

        csvString += [
            '', '', totalNukes, totalAlmostNukes, totalDVs, totalNobles, totalPossibleNobles
        ].join(', ');

        csvString += '\n\n';

        csvString += [
            'Time', 'Player', 'Nukes', '3/4 Nukes', 'DVs', 'Nobles', 'Possible nobles', 'Needs upload?'
        ].join(', ');
 
        playerSummaries.forEach((s) => {
            csvString += '\n';
            csvString += [
                s.uploadedAt, s.playerName, s.numNukes, s.numAlmostNukes, s.numDVs, s.numNukes, s.numPossibleNobles, s.needsUpload ? 'YES' : ''
            ].join(', ');
        });

        return csvString;
    }

}