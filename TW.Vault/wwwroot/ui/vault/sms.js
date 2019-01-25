
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
        label: lib.translate(lib.itlcodes.TAB_SMS),
        containerId: 'vault-sms-container',

        getContent: `
            <p>
                ${lib.translate(lib.itlcodes.SMS_DESCRIPTION)}
            </p>

            ${uilib.mkTabbedContainer(displayTab, tabs)}
        `
    };

    return smsTab;
}

function makeSmsDisplayTab() {
    return {
        label: lib.translate(lib.itlcodes.TAB_NOTIFICATIONS),
        containerId: 'vault-notifications-display',
        init: function ($container) {

            loadNotifications($container);

            $container.find('#notification-time-formats').click((e) => {
                e.originalEvent.preventDefault();
                alert(lib.translate(lib.itlcodes.SMS_TIME_FORMAT_DESCRIPTION, { _escaped: false }));
            });

            $container.find('#add-notification').click(() => {
                let $notificationTime = $container.find('#notification-time');
                let $message = $container.find('#notification-label');

                let notificationTimeText = $notificationTime.val().trim();
                let message = $message.val().trim();

                if (!message.length) {
                    alert(lib.translate(lib.itlcodes.SMS_MESSAGE_REQUIRED, { _escaped: false }));
                    return;
                }

                if (message.length > 256) {
                    alert(lib.translate(lib.itlcodes.SMS_CHARACTER_LIMIT, { length: message.length, _escaped: false }));
                    return;
                }

                let notificationTime = lib.parseTimeString(notificationTimeText);
                if (!notificationTime) {
                    alert(lib.translate(lib.itlcodes.SMS_INVALID_TIME, { _escaped: false }));
                    return;
                }

                let serverTime = lib.getServerDateTime();
                if (serverTime.valueOf() >= notificationTime.valueOf()) {
                    alert(lib.translate(lib.itlcodes.SMS_TIME_TOO_EARLY, { _escaped: false }));
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

                        if (lib.isUnloading())
                            return;

                        alert(lib.messages.GENERIC_ERROR);
                    });
            });
        },

        getContent: `
            <h4>${lib.translate(lib.itlcodes.TAB_NOTIFICATIONS)}</h4>
            <p style="text-align:left">
                <em>${lib.translate(lib.itlcodes.SMS_ADD_NEW)}</em>
                <br>
                <label style="display:inline-block;width:7em;text-align:right" for="notification-time">${lib.translate(lib.itlcodes.SERVER_TIME)}</label>
                <input type="text" id="notification-time" style="width:400px">
                <input type="submit" id="notification-time-formats" value="${lib.translate(lib.itlcodes.SMS_SUPPORTED_FORMATS)}">
                <br>
                <label style="display:inline-block;width:7em;text-align:right" for="notification-label">${lib.translate(lib.itlcodes.MESSAGE)}</label>
                <input type="text" id="notification-label" style="width:400px">
                <br>
                <button id="add-notification">${lib.translate(lib.itlcodes.ADD)}</button>
            </p>
            <table style="width:100%" class="vis">
                <tr>
                    <th style="width:12em">${lib.translate(lib.itlcodes.SERVER_TIME)}</th>
                    <th>${lib.translate(lib.itlcodes.MESSAGE)}</th>
                    <th style="width:5em"></th>
                </tr>
            </table>
        `
    };
}

function makeSmsPhoneNumbersTab() {
    return {
        label: lib.translate(lib.itlcodes.TAB_PHONE_NUMBERS),
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
                    alert(lib.translate(lib.itlcodes.SMS_INVALID_PHONE_NUMBER, { _escaped: false }));
                    return;
                }

                if (label.length > 128) {
                    alert(lib.translate(lib.itlcodes.SMS_PHONE_NAME_TOO_LONG, { _escaped: false }));
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
                        if (lib.isUnloading())
                            return;

                        $phoneNumber.prop('disabled', false);
                        $label.prop('disabled', false);
                        alert(lib.messages.GENERIC_ERROR);
                    });
            });

        },

        getContent: `
            <h4>${lib.translate(lib.itlcodes.SMS_PHONE_NUMBERS)}</h4>
            <p style="text-align: left">
                ${lib.translate(lib.itlcodes.SMS_ADD_PHONE_NUMBER)}
                <br>
                <label style="display:inline-block;width:3em;text-align:right" for="new-number">#</label>
                <input type="text" id="new-number" placeholder="+1 202-555-0109">
                <br>
                <label style="display:inline-block;width:3em;text-align:right" for="new-number-label">${lib.translate(lib.itlcodes.NAME)}</label>
                <input type="text" id="new-number-label" placeholder="(${lib.translate(lib.itlcodes.OPTIONAL)})">
                <br>
                <button id="add-phone-number">${lib.translate(lib.itlcodes.ADD)}</button>
            </p>
            <table style="width:100%" class="vis">
                <tr>
                    <th style="width:30%">#</th>
                    <th></th>
                    <th style="5em"></th>
                </tr>
            </table>
        `
    };
}

function makeSmsSettingsTab() {
    return {
        label: lib.translate(lib.itlcodes.TAB_SMS_SETTINGS),
        containerId: 'vault-notifications-settings',

        init: function ($container) {
            loadNotificationSettings($container);
            $container.find('#save-notification-settings-btn').click(() => {
                saveNotificationSettings($container);
            });
        },

        getContent: `
            <h4>${lib.translate(lib.itlcodes.TAB_SMS_SETTINGS)}</h4>
            <div>
                <p>
                    ${lib.translate(lib.itlcodes.SMS_SETTINGS_1)}
                    <input id="notify-window-minutes" type="text" style="width:2em;text-align:center">
                    ${lib.translate(lib.itlcodes.SMS_SETTINGS_2)}
                </p>
                <button id="save-notification-settings-btn">${lib.translate(lib.itlcodes.SAVE)}</button>
            </div>
        `
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
                            <td><input type="submit" value="${lib.translate(lib.itlcodes.DELETE)}"></td>
                        </tr>
                    `.trim());
                $phoneNumbersTable.append($row);
                $row.find('input').click((ev) => {
                    ev.originalEvent.preventDefault();
                    if (!confirm(lib.translate(lib.itlcodes.SMS_CONFIRM_REMOVE_NUMBER, { phoneNumber: number.number }))) {
                        return;
                    }

                    lib.deleteApi('notification/phone-numbers/' + number.id)
                        .done(() => {
                            updatePhoneNumbers($container);
                        })
                        .error(() => {
                            alert(lib.messages.GENERIC_ERROR);
                        });
                });
            });
        })
        .error(() => {
            if (lib.isUnloading())
                return;

            alert(lib.translate(lib.itlcodes.SMS_PHONE_NUMBERS_ERROR, { _escaped: false }));
        });
}

function loadNotificationSettings($container) {
    lib.getApi('notification/settings')
        .done((data) => {
            $container.find('#notify-window-minutes').val(data.sendNotificationBeforeMinutes);
        })
        .error(() => {
            if (lib.isUnloading())
                return;

            alert(ib.messages.GENERIC_ERROR);
        });
}

function saveNotificationSettings($container) {
    let $notificationWindow = $container.find('#notify-window-minutes');
    let notificationWindow = $notificationWindow.val().trim();
    if (!notificationWindow.length) {
        alert(lib.translate(lib.itlcodes.SMS_SETTINGS_EMPTY_VALUE, { _escaped: false }));
        return;
    }

    if (notificationWindow.match(/[^\d]/)) {
        alert(lib.translate(lib.itlcodes.SMS_SETTINGS_INVALID_VALUE, { _escaped: false }));
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

            if (lib.isUnloading())
                return;

            alert(lib.messages.GENERIC_ERROR);
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
                            <td>${lib.formatDateTime(request.eventOccursAt)}</td>
                            <td>${request.message}</td>
                            <td><input type="submit" value="${lib.translate(lib.itlcodes.DELETE)}"></td>
                        </tr>
                    `.trim());

                $row.find('input').click((ev) => {
                    ev.originalEvent.preventDefault();

                    let confirmInfo = `"${request.message}" at ${lib.formatDateTime(request.eventOccursAt)}`;
                    if (!confirm(`${lib.translate(lib.itlcodes.SMS_CONFIRM_DELETE_NOTIFICATION)}\n\n${confirmInfo}`)) {
                        return;
                    }

                    lib.deleteApi(`notification/requests/${request.id}`)
                        .done(() => {
                            loadNotifications($container);
                        })
                        .error(() => {
                            if (lib.isUnloading())
                                return;

                            alert(lib.messages.GENERIC_ERROR);
                        });
                });

                $notificationsTable.append($row);
            });
        })
        .error(() => {
            if (lib.isUnloading())
                return;

            alert(lib.translate(lib.itlcodes.SMS_NOTIFICATIONS_ERROR, { _escaped: false }));
        });
}