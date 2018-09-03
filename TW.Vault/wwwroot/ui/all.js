function parseAllPages($doc) {

    //# REQUIRE ui/admin.js

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
                <tr id="vault-upload-troops">
                    <td>Troops</td>
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
                <tr id="vault-upload-all">
                    <td colspan=3 style="text-align:center">
                        <input type="button" class="upload-button upload-button-all" value="Upload All">
                    </td>
                </tr>
            </table>

            <div id="vault-admin-container" style="padding:1em"></div>

            <h4>Instructions</h4>
            <p>
                Upload data using the buttons above. Run this script on your Map to show extra village information
                while hovering. Some features may not be available to you until you upload new data.
            </p>

            <hr style="margin:4em 0 2em">

            <button class="btn btn-confirm-yes vault-toggle-terms-btn">Disclaimers and Terms</button>
            <div id="vault-disclaimers-and-terms" style="display:none;padding:1em">
                <p>
                    <em>This tool is not endorsed or developed by InnoGames.</em>
                </p>
                <p>
                    <em>
                        All data and requests to the Vault will have various information logged for security. This is limited to:

                        Authentication token, IP address, player ID, tribe ID, requested endpoint, and time of transaction.

                        Requests to this script will only be IP-logged to protect against abuse. Information collected by this script will never be shared
                        with any third parties or any unauthorized tribes/players.
                    </em>
                </p>
            </div>

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

    function processAdminInterface() {
        lib.getApi(lib.makeApiUrl('admin'))
            .done((data) => {
                if (typeof data == 'string')
                    data = JSON.parse(data);

                console.log('isAdmin = ', data.isAdmin);

                if (data.isAdmin)
                    makeAdminInterface($uiContainer.find('#vault-admin-container'));
            });
    }

    $uiContainer.find('.vault-toggle-terms-btn').click(() => {
        $uiContainer.find('#vault-disclaimers-and-terms').toggle();
    });

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

        //  TODO - This is messy, clean this up
        let alertCaptcha = () => alert(lib.messages.TRIGGERED_CAPTCHA);

        switch (uploadType) {
            default: alert(`Programmer error: no logic for upload type "${uploadType}"!`);

            case 'vault-upload-reports':
                processUploadReports($statusContainer, (didFail) => {
                    $('.upload-button').prop('disabled', false);
                    if (didFail && didFail == lib.errorCodes.CAPTCHA) {
                        alertCaptcha();
                    }
                });
                break;

            case 'vault-upload-incomings':
                processUploadIncomings($statusContainer, (didFail) => {
                    $('.upload-button').prop('disabled', false);
                    if (didFail && didFail == lib.errorCodes.CAPTCHA) {
                        alertCaptcha();
                    }
                });
                break;

            case 'vault-upload-commands':
                processUploadCommands($statusContainer, (didFail) => {
                    $('.upload-button').prop('disabled', false);
                    if (didFail && didFail == lib.errorCodes.CAPTCHA) {
                        alertCaptcha();
                    }
                });
                break;

            case 'vault-upload-troops':
                processUploadTroops($statusContainer, (didFail) => {
                    $('.upload-button').prop('disabled', false);
                    if (didFail && didFail == lib.errorCodes.CAPTCHA) {
                        alertCaptcha();
                    }
                });
                break;

            case 'vault-upload-all':
                $('.status-container').html('<em>Waiting...</em>');

                let resetButtons = () => $('.upload-button').prop('disabled', false);
                let resetStatusContainers = () => {
                    $('.status-container').filter((i, el) => $(el).text().toLowerCase().contains("waiting")).empty();
                };

                let runReports = () => {
                    processUploadReports($uiContainer.find('#vault-upload-reports .status-container'), runIncomings);
                };
                let runIncomings = (didFail) => {
                    if (didFail) {
                        if (didFail == lib.errorCodes.CAPTCHA) {
                            alertCaptcha();
                        } else if (didFail != lib.errorCodes.FILTER_APPLIED) {
                            alert('An unexpected error occurred: ' + didFail);
                        }
                        resetButtons();
                        resetStatusContainers();
                        return;
                    }
                    processUploadIncomings($uiContainer.find('#vault-upload-incomings .status-container'), runTroops);
                };
                let runTroops = (didFail) => {
                    if (didFail) {
                        if (didFail == lib.errorCodes.CAPTCHA) {
                            alertCaptcha();
                        } else if (!lib.errorCodes[didFail]) {
                            alert('An unexpected error occurred: ' + didFail);
                        }
                        resetButtons();
                        resetStatusContainers();
                        return;
                    }
                    processUploadTroops($uiContainer.find('#vault-upload-troops .status-container'), runCommands);
                };
                let runCommands = (didFail) => {
                    if (didFail) {
                        if (didFail == lib.errorCodes.CAPTCHA) {
                            alertCaptcha();
                        } else if (!lib.errorCodes[didFail]) {
                            alert('An unexpected error occurred: ' + didFail);
                        }
                        resetButtons();
                        resetStatusContainers();
                        return;
                    }
                    processUploadCommands($uiContainer.find('#vault-upload-commands .status-container'), (didFail) => {
                        if (didFail) {
                            if (didFail == lib.errorCodes.CAPTCHA) {
                                alertCaptcha();
                            } else if (!lib.errorCodes[didFail]) {
                                alert('An unexpected error occurred: ', didFail);
                            }
                        }
                        resetButtons();
                        resetStatusContainers();
                    });
                };

                runReports();
                break;
        }

        $('.upload-button').prop('disabled', true);
    });
   
}