function parseAllPages($doc) {

    //# REQUIRE ui/admin.js

    $doc = $doc || $(document);

    $doc.find('#vault-ui-container').remove();
    var $uiContainer = $(`
        <div id="vault-ui-container" class="confirmation-box" style="border-width: 20px">
        <!-- Weird margins on this since TW styling has an annoying gap -->
        <div class="confirmation-box-content-pane" style="min-height:100%;margin-left:-1px;margin-bottom:-1px">
        <div class="confirmation-box-content" style="min-height:100%">
            <h3>Vault</h3>
            <p>
                Here you can upload data to the Vault or set up SMS notifications. Run this script on your Map or on your Incomings to
                see everything the Vault has to offer.
            </p>

            <button class="btn btn-confirm-yes vault-toggle-uploads-btn">Upload Reports, Troops, Etc.</button>
            <button class="btn btn-confirm-yes vault-toggle-notifications-btn">SMS Notifications</button>
            <button class="btn btn-confirm-yes vault-toggle-admin-btn" style="display:none">Admin Options</button>
            <button class="btn btn-confirm-yes vault-toggle-terms-btn">Disclaimers and Terms</button>

            <div style="padding:1em">

                <div id="vault-uploads-container" style="display:none;margin:1em 0;">
                    <p>
                        <strong>Click <em>Upload All</em> below. If needed, upload different things individually using the other Upload buttons.</strong>
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
                </div>

                <div id="vault-notifications-container" style="padding:1em;display:none">
                    <p>
                        The Vault can send you a text at a certain time. Use this as a reminder for launch times, etc. All
                        phone numbers added here will be texted when a notification is sent.
                    </p>

                    <button class="btn btn-confirm-yes notifications-toggle-display-btn">Notifications</button>
                    <button class="btn btn-confirm-yes notifications-toggle-phone-numbers-btn">Phone Numbers</button>
                    <button class="btn btn-confirm-yes notifications-toggle-settings-btn">Settings</button>

                    <div style="padding:1em">
                        <div id="vault-notifications-phone-numbers" style="display:none">
                            <h4>Phone Numbers</h4>
                            <p style="text-align: left">
                                Add a New Number
                                <br>
                                <label style="display:inline-block;width:3em;text-align:right" for="new-number">#</label>
                                <input type="text" id="new-number" placeholder="+1 202-555-0109">
                                <br>
                                <label style="display:inline-block;width:3em;text-align:right" for="new-number-label">Name</label>
                                <input type="text" id="new-number-label" placeholder="(Optional)">
                                <br>
                                <button id="add-phone-number">Add</button>
                            </p>
                            <table style="width:100%" class="vis">
                                <tr>
                                    <th style="width:30%">#</th>
                                    <th></th>
                                    <th style="5em"></th>
                                </tr>
                            </table>
                        </div>
                        <div id="vault-notifications-settings" style="display:none">
                            <h4>Settings</h4>
                            <div>
                                <p>
                                    Send me a text <input id="notify-window-minutes" type="text" style="width:2em;text-align:center"> minutes early.
                                </p>
                                <button id="save-notification-settings-btn">Save</button>
                            </div>
                        </div>
                        <div id="vault-notifications-display" style="display:none">
                            <h4>Notifications</h4>
                            <p style="text-align:left">
                                <em>Add New</em>
                                <br>
                                <label style="display:inline-block;width:7em;text-align:right" for="notification-time">Server Time</label>
                                <input type="text" id="notification-time" style="width:400px">
                                <input type="submit" id="notification-time-formats" value="Supported Formats">
                                <br>
                                <label style="display:inline-block;width:7em;text-align:right" for="notification-label">Message</label>
                                <input type="text" id="notification-label" style="width:400px">
                                <br>
                                <button id="add-notification">Add</button>
                            </p>
                            <table style="width:100%" class="vis">
                                <tr>
                                    <th style="width:12em">Server Time</th>
                                    <th>Message</th>
                                    <th style="width:5em"></th>
                                </tr>
                            </table>
                        </div>
                    </div>
                </div>

                <div id="vault-admin-container" style="padding:1em;display:none"></div>

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
                    makeAdminInterface($uiContainer, $uiContainer.find('#vault-admin-container'));
            });
    }

    function setActiveContainer(id) {
        let containerIds = [
            '#vault-uploads-container',
            '#vault-notifications-container',
            '#vault-admin-container',
            '#vault-disclaimers-and-terms'
        ];

        containerIds.forEach((cid) => {
            if (cid == id) {
                $(id).toggle();
            } else {
                $(cid).css('display', 'none');
            }
        });
    }

    $uiContainer.find('.vault-toggle-uploads-btn').click(() => {
        setActiveContainer('#vault-uploads-container');
    });

    $uiContainer.find('.vault-toggle-notifications-btn').click(() => {
        setActiveContainer('#vault-notifications-container');
    });

    $uiContainer.find('.vault-toggle-admin-btn').click(() => {
        setActiveContainer('#vault-admin-container');
    });

    $uiContainer.find('.vault-toggle-terms-btn').click(() => {
        setActiveContainer('#vault-disclaimers-and-terms');
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


    function setActiveNotificationsContainer(id) {
        let containerIds = [
            '#vault-notifications-phone-numbers',
            '#vault-notifications-settings',
            '#vault-notifications-display'
        ];

        containerIds.forEach((cid) => {
            if (cid == id) {
                $(id).toggle();
            } else {
                $(cid).css('display', 'none');
            }
        });
    }

    $uiContainer.find('.notifications-toggle-phone-numbers-btn').click(() => {
        setActiveNotificationsContainer('#vault-notifications-phone-numbers');
    });

    $uiContainer.find('.notifications-toggle-settings-btn').click(() => {
        setActiveNotificationsContainer('#vault-notifications-settings');
    });

    $uiContainer.find('.notifications-toggle-display-btn').click(() => {
        setActiveNotificationsContainer('#vault-notifications-display');
    });

    $uiContainer.find('#add-phone-number').click(() => {
        let $phoneNumber = $uiContainer.find('#new-number');
        let $label = $uiContainer.find('#new-number-label');

        let phoneNumber = $phoneNumber.val();
        let label = $label.val();

        let trimmedNumber = phoneNumber.replace(/[^\d]/g, '');
        if (trimmedNumber.length < 11) {
            alert('Invalid phone number - must include country code and area code.\n\nie +1 202-555-0109');
            return;
        }

        if (label.length > 128) {
            alert(`Phone name is too long - must be less than 128 characters. (Currently ${label.length})`);
            return;
        }

        let data = {
            phoneNumber: phoneNumber,
            label: label
        };

        $phoneNumber.prop('disabled', true);
        $label.prop('disabled', true);

        lib.postApi(lib.makeApiUrl('notification/phone-numbers'), data)
            .done(() => {
                $phoneNumber.val('');
                $phoneNumber.prop('disabled', false);

                $label.val('');
                $label.prop('disabled', false);

                updatePhoneNumbers();
            })
            .error(() => {
                $phoneNumber.prop('disabled', false);
                $label.prop('disabled', false);
                alert('An error occurred.');
            });
    });

    $uiContainer.find('#save-notification-settings-btn').click(() => {
        saveNotificationSettings();
    });

    $uiContainer.find('#notification-time-formats').click((e) => {
        e.originalEvent.preventDefault();
        alert(`Supported time formats: Basically everything under the sun. Copy/paste whatever you see.`);
    });

    $uiContainer.find('#add-notification').click(() => {
        let $notificationTime = $uiContainer.find('#notification-time');
        let $message = $uiContainer.find('#notification-label');

        let notificationTimeText = $notificationTime.val().trim();
        let message = $message.val().trim();

        if (!message.length) {
            alert('A message is required!');
            return;
        }

        if (message.length > 256) {
            alert(`Your message can't be over 256 characters! (Currently ${message.length})`);
            return;
        }

        let notificationTime = lib.parseTimeString(notificationTimeText);
        if (!notificationTime) {
            alert('Invalid notification time!');
            return;
        }

        let serverTime = lib.getServerDateTime();
        if (serverTime.valueOf() >= notificationTime.valueOf()) {
            alert("Your notification time must be *after* the current server time!");
            return;
        }

        let data = {
            eventOccursAt: notificationTime,
            message: message
        };

        $notificationTime.prop('disabled', true);
        $message.prop('disabled', true);
        $uiContainer.find('#add-notification').prop('disabled', true);

        lib.postApi(lib.makeApiUrl('notification/requests'), data)
            .done(() => {
                loadNotifications()
                    .done(() => {
                        $notificationTime.prop('disabled', false);
                        $message.prop('disabled', false);
                        $uiContainer.find('#add-notification').prop('disabled', false);

                        $notificationTime.val('');
                    })
                    .error(() => {
                        $notificationTime.prop('disabled', false);
                        $message.prop('disabled', false);
                        $uiContainer.find('#add-notification').prop('disabled', false);
                    })
            })
            .error(() => {
                $notificationTime.prop('disabled', false);
                $message.prop('disabled', false);
                $uiContainer.find('#add-notification').prop('disabled', false);
                alert('An error occurred.');
            });
    });




    updatePhoneNumbers();
    loadNotificationSettings();
    loadNotifications();

    function updatePhoneNumbers() {
        lib.getApi(lib.makeApiUrl('notification/phone-numbers'))
            .done((phoneNumbers) => {
                let $phoneNumbersTable = $uiContainer.find('#vault-notifications-phone-numbers table');
                $phoneNumbersTable.find('tr:not(:first-of-type)').remove();

                phoneNumbers.forEach((number) => {
                    let $row = $(`
                        <tr data-id="${number.id}">
                            <td>${number.number}</td>
                            <td>${number.label}</td>
                            <td><input type="submit" value="Delete"></td>
                        </tr>
                    `.trim());
                    $phoneNumbersTable.append($row);
                    $row.find('input').click((ev) => {
                        ev.originalEvent.preventDefault();
                        if (!confirm(`Are you sure you want to remove the number ${number.number}?`)) {
                            return;
                        }

                        lib.deleteApi(lib.makeApiUrl('notification/phone-numbers/' + number.id))
                            .done(() => {
                                updatePhoneNumbers();
                            })
                            .error(() => {
                                alert('An error occurred.');
                            });
                    });
                });
            })
            .error(() => {
                alert('An error occurred while getting your phone numbers.');
            });
    }

    function loadNotificationSettings() {
        lib.getApi(lib.makeApiUrl('notification/settings'))
            .done((data) => {
                $uiContainer.find('#notify-window-minutes').val(data.sendNotificationBeforeMinutes);
            })
            .error(() => {
                alert('An error occurred.');
            });
    }

    function saveNotificationSettings() {
        let $notificationWindow = $uiContainer.find('#notify-window-minutes');
        let notificationWindow = $notificationWindow.val().trim();
        if (!notificationWindow.length) {
            alert("Empty settings value!");
            return;
        }

        if (notificationWindow.match(/[^\d]/)) {
            alert("Invalid settings value!");
            return;
        }

        let data = {
            sendNotificationBeforeMinutes: parseInt(notificationWindow)
        };

        $notificationWindow.prop('disabled', true);
        $uiContainer.find('#save-notification-settings-btn').prop('disabled', true);

        lib.postApi(lib.makeApiUrl('notification/settings'), data)
            .done(() => {
                $notificationWindow.prop('disabled', false);
                $uiContainer.find('#save-notification-settings-btn').prop('disabled', false);
            })
            .error(() => {
                $notificationWindow.prop('disabled', false);
                $uiContainer.find('#save-notification-settings-btn').prop('disabled', false);
                alert('An error occurred.');
            });
    }

    function loadNotifications() {
        return lib.getApi(lib.makeApiUrl('notification/requests'))
            .done((requests) => {
                let $notificationsTable = $uiContainer.find('#vault-notifications-display table');
                $notificationsTable.find('tr:not(:first-of-type)').remove();

                requests.forEach((request) => {
                    request.eventOccursAt = new Date(Date.parse(request.eventOccursAt));

                    let $row = $(`
                        <tr data-id="${request.id}">
                            <td>${lib.formateDateTime(request.eventOccursAt)}</td>
                            <td>${request.message}</td>
                            <td><input type="submit" value="Delete"></td>
                        </tr>
                    `.trim());

                    $row.find('input').click((ev) => {
                        ev.originalEvent.preventDefault();

                        let confirmInfo = `"${request.message}" at ${lib.formateDateTime(request.eventOccursAt)}`;
                        if (!confirm(`Are you sure you want to delete this notification?\n\n${confirmInfo}`)) {
                            return;
                        }

                        lib.deleteApi(lib.makeApiUrl(`notification/requests/${request.id}`))
                            .done(() => {
                                loadNotifications();
                            })
                            .error(() => {
                                alert('An error occurred.');
                            });
                    });

                    $notificationsTable.append($row);
                });
            })
            .error(() => {
                alert("An error occurred while loading notification requests.");
            });
    }
}