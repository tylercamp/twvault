
function makeSmsTab() {

    let displayTab = makeSmsDisplayTab();
    let numberTab = makeSmsPhoneNumbersTab();
    let settingsTab = makeSmsSettingsTab();

    let tabs = [
        displayTab,
        numberTab,
        settingsTab
    ];

    let smsTab = {
        label: 'SMS Notifications',
        containerId: 'vault-sms-container',

        getContent: function () {
            return `
                <p>
                    The Vault can send you a text at a certain time. Use this as a reminder for launch times, etc. All
                    phone numbers added here will be texted when a notification is sent.
                </p>

                ${uilib.mkTabbedContainer(displayTab.containerId, tabs)}
            `;
        }
    };

    return smsTab;
}

function makeSmsDisplayTab() {
    return {
        label: 'Notifications',
        containerId: 'vault-notifications-display',
        init: function ($container) {

            loadNotifications($container);

            $container.find('#notification-time-formats').click((e) => {
                e.originalEvent.preventDefault();
                alert(`Supported time formats: Basically everything under the sun. Copy/paste whatever you see.`);
            });

            $container.find('#add-notification').click(() => {
                let $notificationTime = $container.find('#notification-time');
                let $message = $container.find('#notification-label');

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
                $container.find('#add-notification').prop('disabled', true);

                lib.postApi('notification/requests', data)
                    .done(() => {
                        loadNotifications($container)
                            .done(() => {
                                $notificationTime.prop('disabled', false);
                                $message.prop('disabled', false);
                                $container.find('#add-notification').prop('disabled', false);

                                $notificationTime.val('');
                            })
                            .error(() => {
                                $notificationTime.prop('disabled', false);
                                $message.prop('disabled', false);
                                $container.find('#add-notification').prop('disabled', false);
                            })
                    })
                    .error(() => {
                        $notificationTime.prop('disabled', false);
                        $message.prop('disabled', false);
                        $container.find('#add-notification').prop('disabled', false);
                        alert('An error occurred.');
                    });
            });
        },

        getContent: function () {
            return `
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
            `;
        }
    };
}

function makeSmsPhoneNumbersTab() {
    return {
        label: 'Phone Numbers',
        containerId: 'vault-notifications-phone-numbers',

        init: function ($container) {
            updatePhoneNumbers($container);

            $container.find('#add-phone-number').click(() => {
                let $phoneNumber = $container.find('#new-number');
                let $label = $container.find('#new-number-label');

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

                lib.postApi('notification/phone-numbers', data)
                    .done(() => {
                        $phoneNumber.val('');
                        $phoneNumber.prop('disabled', false);

                        $label.val('');
                        $label.prop('disabled', false);

                        updatePhoneNumbers($container);
                    })
                    .error(() => {
                        $phoneNumber.prop('disabled', false);
                        $label.prop('disabled', false);
                        alert('An error occurred.');
                    });
            });

        },

        getContent: function () {
            return `
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
            `;
        }
    };
}

function makeSmsSettingsTab() {
    return {
        label: 'Settings',
        containerId: 'vault-notifications-settings',

        init: function ($container) {
            loadNotificationSettings($container);
            $container.find('#save-notification-settings-btn').click(() => {
                saveNotificationSettings($container);
            });
        },

        getContent: function () {
            return `
                <h4>Settings</h4>
                <div>
                    <p>
                        Send me a text <input id="notify-window-minutes" type="text" style="width:2em;text-align:center"> minutes early.
                    </p>
                    <button id="save-notification-settings-btn">Save</button>
                </div>
            `;
        }
    };
}

function updatePhoneNumbers($container) {
    lib.getApi('notification/phone-numbers')
        .done((phoneNumbers) => {
            let $phoneNumbersTable = $container.find('table');
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

                    lib.deleteApi('notification/phone-numbers/' + number.id)
                        .done(() => {
                            updatePhoneNumbers($container);
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

function loadNotificationSettings($container) {
    lib.getApi('notification/settings')
        .done((data) => {
            $container.find('#notify-window-minutes').val(data.sendNotificationBeforeMinutes);
        })
        .error(() => {
            alert('An error occurred.');
        });
}

function saveNotificationSettings($container) {
    let $notificationWindow = $container.find('#notify-window-minutes');
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
    $container.find('#save-notification-settings-btn').prop('disabled', true);

    lib.postApi('notification/settings', data)
        .done(() => {
            $notificationWindow.prop('disabled', false);
            $container.find('#save-notification-settings-btn').prop('disabled', false);
        })
        .error(() => {
            $notificationWindow.prop('disabled', false);
            $container.find('#save-notification-settings-btn').prop('disabled', false);
            alert('An error occurred.');
        });
}

function loadNotifications($container) {
    return lib.getApi('notification/requests')
        .done((requests) => {
            let $notificationsTable = $container.find('table');
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

                    lib.deleteApi(`notification/requests/${request.id}`)
                        .done(() => {
                            loadNotifications($container);
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