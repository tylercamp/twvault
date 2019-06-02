
function makeUploadsTab() {
    let uploadsTab = {
        label: lib.translate(lib.itlcodes.TAB_UPLOAD),
        containerId: 'vault-uploads-container',

        init: function ($container) {

            let ignoredGroupIds = lib.getLocalStorage('reports-ignored-groups', []);
            let reportFolderNames = null;

            var uploadDetailsMessages = {
                'vault-upload-reports': lib.translate(lib.itlcodes.UPLOAD_DESCRIPTION_REPORTS),
                'vault-upload-incomings': lib.translate(lib.itlcodes.UPLOAD_DESCRIPTION_INCS),
                'vault-upload-commands': lib.translate(lib.itlcodes.UPLOAD_DESCRIPTION_COMMANDS),
                'vault-upload-troops': lib.translate(lib.itlcodes.UPLOAD_DESCRIPTION_TROOPS)
            };

            $container.find('.details-button').click((ev) => {
                var $el = $(ev.target);
                var uploadType = $el.closest('tr').attr('id');

                alert(uploadDetailsMessages[uploadType]);
            });

            $container.find('.upload-clear-cache').click((ev) => {
                lib.deleteLocalStorage('reports-history');
                lib.deleteLocalStorage('commands-history');

                alert(lib.translate(lib.itlcodes.UPLOAD_CACHE_CLEARED, { _escaped: false }));
            });

            $container.find('.upload-button').click((ev) => {
                var $el = $(ev.target);
                var $row = $el.closest('tr');
                var uploadType = $row.attr('id');

                var $statusContainer = $row.find('.status-container');

                //  TODO - This is messy, clean this up
                let alertCaptcha = () => alert(lib.messages.TRIGGERED_CAPTCHA);
                let alertFilter = (type) => alert(lib.messages.FILTER_APPLIED(type));

                let resetButtons = () => {
                    $('.upload-button').prop('disabled', false);
                    $('.upload-clear-cache').prop('disabled', false);
                };

                switch (uploadType) {
                    default: alert(`Programmer error: no logic for upload type "${uploadType}"!`);

                    case 'vault-upload-reports':
                        processUploadReports($statusContainer, (didFail) => {
                            resetButtons();
                            if (didFail) {
                                if (didFail == lib.errorCodes.CAPTCHA) {
                                    alertCaptcha();
                                } else if (didFail == lib.errorCodes.FILTER_APPLIED) {
                                    alertFilter(lib.translate(lib.itlcodes.REPORTS, { _escaped: false }));
                                }
                            }
                        });
                        break;

                    case 'vault-upload-incomings':
                        processUploadIncomings($statusContainer, (didFail) => {
                            resetButtons();
                            if (didFail) {
                                if (didFail == lib.errorCodes.CAPTCHA) {
                                    alertCaptcha();
                                } else if (didFail == lib.errorCodes.FILTER_APPLIED) {
                                    alertFilter(lib.translate(lib.itlcodes.INCOMINGS, { _escaped: false }));
                                }
                            }
                        });
                        break;

                    case 'vault-upload-commands':
                        processUploadCommands($statusContainer, (didFail) => {
                            resetButtons();
                            if (didFail) {
                                if (didFail == lib.errorCodes.CAPTCHA) {
                                    alertCaptcha();
                                } else if (didFail == lib.errorCodes.FILTER_APPLIED) {
                                    alertFilter(lib.translate(lib.itlcodes.COMMANDS, { _escaped: false }));
                                }
                            }
                        });
                        break;

                    case 'vault-upload-troops':
                        processUploadTroops($statusContainer, (didFail) => {
                            resetButtons();
                            if (didFail) {
                                if (didFail == lib.errorCodes.CAPTCHA) {
                                    alertCaptcha();
                                } else if (didFail == lib.errorCodes.FILTER_APPLIED) {
                                    alertFilter(lib.translate(lib.itlcodes.TROOPS, { _escaped: false }));
                                }
                            }
                        });
                        break;

                    case 'vault-upload-all':
                        $('.status-container').html(`<em>${lib.translate(lib.itlcodes.WAITING)}...</em>`);

                        let resetStatusContainers = () => {
                            $('.status-container').filter((i, el) => $(el).text().toLowerCase().contains(lib.translate(lib.itlcodes.WAITING, { _escaped: false }))).empty();
                        };

                        let runReports = () => {
                            processUploadReports($container.find('#vault-upload-reports .status-container'), runIncomings);
                        };
                        let runIncomings = (didFail) => {
                            if (didFail) {
                                if (didFail == lib.errorCodes.CAPTCHA) {
                                    alertCaptcha();
                                } else if (didFail == lib.errorCodes.FILTER_APPLIED) {
                                    alertFilter(lib.translate(lib.itlcodes.REPORTS, { _escaped: false }));
                                } else if (!lib.isUnloading()) {
                                    alert(lib.messages.GENERIC_ERROR + ': ' + didFail);
                                }
                                resetButtons();
                                resetStatusContainers();
                                return;
                            }
                            processUploadIncomings($container.find('#vault-upload-incomings .status-container'), runTroops);
                        };
                        let runTroops = (didFail) => {
                            if (didFail) {
                                if (didFail == lib.errorCodes.CAPTCHA) {
                                    alertCaptcha();
                                } else if (didFail == lib.errorCodes.FILTER_APPLIED) {
                                    alertFilter(lib.translate(lib.itlcodes.INCOMINGS, { _escaped: false }));
                                } else if (!lib.isUnloading()) {
                                    alert(lib.messages.GENERIC_ERROR + ': ' + didFail);
                                }
                                resetButtons();
                                resetStatusContainers();
                                return;
                            }
                            processUploadTroops($container.find('#vault-upload-troops .status-container'), runCommands);
                        };
                        let runCommands = (didFail) => {
                            if (didFail) {
                                if (didFail == lib.errorCodes.CAPTCHA) {
                                    alertCaptcha();
                                } else if (didFail == lib.errorCodes.FILTER_APPLIED) {
                                    alertFilter(lib.translate(lib.itlcodes.TROOPS, { _escaped: false }));
                                } else if (!lib.isUnloading()) {
                                    alert(lib.messages.GENERIC_ERROR + ': ' + didFail);
                                }
                                resetButtons();
                                resetStatusContainers();
                                return;
                            }
                            processUploadCommands($container.find('#vault-upload-commands .status-container'), (didFail) => {
                                if (didFail) {
                                    if (didFail == lib.errorCodes.CAPTCHA) {
                                        alertCaptcha();
                                    } else if (didFail == lib.errorCodes.FILTER_APPLIED) {
                                        alertFilter(lib.translate(lib.itlcodes.COMMANDS, { _escaped: false }));
                                    } else if (!lib.isUnloading()) {
                                        alert(lib.messages.GENERIC_ERROR + ': ' + didFail);
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
                $('.upload-clear-cache').prop('disabled', true);


                function processUploadReports($statusContainer, onDone) {
                    $.get(lib.makeTwUrl(lib.pageTypes.ALL_REPORTS)).done((data) => {
                        try {
                            if (lib.checkContainsCaptcha(data)) {
                                return onDone(lib.errorCodes.CAPTCHA);
                            }

                            let $doc = lib.parseHtml(data);
                            parseAllReports($doc, (msg) => {
                                $statusContainer.html(msg);
                            }, (didFail) => {
                                onDone(didFail);
                            });
                        } catch (e) {
                            $statusContainer.text(lib.messages.GENERIC_ERROR);
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

                            let $doc = lib.parseHtml(data);
                            parseAllIncomings($doc, (msg) => {
                                $statusContainer.html(msg);
                            }, (didFail) => {
                                onDone(didFail);
                            });
                        } catch (e) {
                            $statusContainer.text(lib.messages.GENERIC_ERROR);
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

                            let $doc = lib.parseHtml(data);
                            parseAllCommands($doc, (msg) => {
                                $statusContainer.html(msg);
                            }, (didFail) => {
                                onDone(didFail);
                            });
                        } catch (e) {
                            $statusContainer.text(lib.messages.GENERIC_ERROR);
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

                            let $doc = lib.parseHtml(data);
                            parseAllTroops($doc, (msg) => {
                                $statusContainer.html(msg);
                            }, (didFail) => {
                                onDone(didFail);
                            });
                        } catch (e) {
                            $statusContainer.text(lib.messages.GENERIC_ERROR);
                            console.error(e);

                            onDone(true);
                        }
                    });
                }
            });

            $container.find('#vault-upload-settings-toggle').click(() => $container.find('#vault-upload-settings').toggle());

            function saveIgnoredGroupIds() {
                lib.setLocalStorage('reports-ignored-groups', ignoredGroupIds);
            }

            function updateIgnoredFoldersList() {
                let $ignoredList = $container.find('#vault-ignored-report-folders');
                $ignoredList.empty();
                if (ignoredGroupIds.length) {
                    ignoredGroupIds.forEach(id => {
                        $ignoredList.append(`
                        <li data-id="${id}">
                            ${reportFolderNames[id] || `(${lib.translate(lib.itlcodes.REPORT_FOLDER_DELETED)})`}
                            <img src="https://tylercamp.me/tw/img/delete.png" style="vertical-align:text-top;cursor:pointer">
                        </li>
                    `.trim());
                    });

                    $ignoredList.find('img').click((ev) => {
                        let $target = $(ev.target);
                        let deletedId = $target.closest('li').data('id');
                        console.log('Removing folder with ID ', deletedId);
                        ignoredGroupIds = ignoredGroupIds.except([deletedId.toString()]);
                        saveIgnoredGroupIds();
                        updateIgnoredFoldersList();
                    });
                } else {
                    // No folders are currently ignored
                    $ignoredList.append(`<em>${lib.translate(lib.itlcodes.REPORT_FOLDER_NONE_IGNORED)}</em>`);
                }
            }

            $.get(lib.makeTwUrl(lib.pageTypes.ALL_REPORTS))
                .done((data) => {
                    let $page = lib.parseHtml(data);
                    let $groupLinks = $page.find('td > a[href*=group_id]:not(.village_switch_link)');
                    let groups = [];
                    $groupLinks.each((i, el) => groups.push({ id: $(el).attr('href').match(/group_id=(\d+)/)[1], name: $(el).text().trim() }));
                    console.log('Found report groups: ', groups);

                    let $ignoredFolderOptions = $container.find('#vault-ignored-report-folders-options')
                    groups.forEach((obj) => {
                        $ignoredFolderOptions.append($(`<option value="${obj.id}">${obj.name}</option>`));
                    });

                    reportFolderNames = lib.arrayToObject(groups, g => g.id, g => g.name);
                    updateIgnoredFoldersList();

                    $container.find('#vault-ignored-report-folders-ignore').click(() => {
                        let folderId = $container.find('#vault-ignored-report-folders-options').val();
                        console.log('Ignoring folder id ', folderId);
                        if (ignoredGroupIds.contains(folderId))
                            return;
                        ignoredGroupIds.push(folderId);
                        saveIgnoredGroupIds();
                        updateIgnoredFoldersList();
                    });
                });
        },

        getContent: `
            <p>
                <strong>${lib.translate(lib.itlcodes.UPLOAD_DESCRIPTION)}</strong>
            </p>

            <table style="width:100%" class="vis lit">
                <tr>
                    <th style="width:12em">${lib.translate(lib.itlcodes.UPLOAD)}</th>
                    <th style="width:6em"></th>
                    <th>${lib.translate(lib.itlcodes.PROGRESS)}</th>
                </tr>
                <tr id="vault-upload-reports" class="lit">
                    <td>${lib.translate(lib.itlcodes.REPORTS)}</td>
                    <td><input type="button" class="details-button" value="${lib.translate(lib.itlcodes.DETAILS)}"></td>
                    <td>
                        <input type="button" class="upload-button" value="${lib.translate(lib.itlcodes.UPLOAD)}">
                        <span class="status-container"></span>
                        <!-- <input type="button" class="cancel-button" value="${lib.translate(lib.itlcodes.CANCEL)}" disabled> -->
                    </td>
                </tr>
                <tr id="vault-upload-incomings">
                    <td>${lib.translate(lib.itlcodes.INCOMINGS)}</td>
                    <td><input type="button" class="details-button" value="${lib.translate(lib.itlcodes.DETAILS)}"></td>
                    <td>
                        <input type="button" class="upload-button" value="${lib.translate(lib.itlcodes.UPLOAD)}">
                        <span class="status-container"></span>
                        <!-- <input type="button" class="cancel-button" value="${lib.translate(lib.itlcodes.CANCEL)}" disabled> -->
                    </td>
                </tr>
                <tr id="vault-upload-troops">
                    <td>${lib.translate(lib.itlcodes.TROOPS)}</td>
                    <td><input type="button" class="details-button" value="${lib.translate(lib.itlcodes.DETAILS)}"></td>
                    <td>
                        <input type="button" class="upload-button" value="${lib.translate(lib.itlcodes.UPLOAD)}">
                        <span class="status-container"></span>
                        <!-- <input type="button" class="cancel-button" value="${lib.translate(lib.itlcodes.CANCEL)}" disabled> -->
                    </td>
                </tr>
                <tr id="vault-upload-commands">
                    <td>${lib.translate(lib.itlcodes.COMMANDS)}</td>
                    <td><input type="button" class="details-button" value="${lib.translate(lib.itlcodes.DETAILS)}"></td>
                    <td>
                        <input type="button" class="upload-button" value="${lib.translate(lib.itlcodes.UPLOAD)}">
                        <span class="status-container"></span>
                        <!-- <input type="button" class="cancel-button" value="${lib.translate(lib.itlcodes.CANCEL)}" disabled> -->
                    </td>
                </tr>
                <tr id="vault-upload-all">
                    <td colspan=3 style="text-align:center">
                        <input type="button" class="upload-button upload-button-all" value="${lib.translate(lib.itlcodes.UPLOAD_ALL)}">
                    </td>
                </tr>
            </table>

            ${uilib.mkBtn('vault-upload-settings-toggle', lib.translate(lib.itlcodes.SETTINGS), 'float:left')}

            <input type="button" class="upload-clear-cache" value="${lib.translate(lib.itlcodes.UPLOAD_CLEAR_CACHE)}" style="float:right">

            <div id="vault-upload-settings" style="display:none;clear:both">
                <h3>${lib.translate(lib.itlcodes.REPORT_OPTIONS)}</h3>
                <h5>${lib.translate(lib.itlcodes.REPORT_FOLDERS_IGNORED)}</h5>
                <div>
                    <ul id="vault-ignored-report-folders" style="display:inline-block;list-style-type:none;padding:0"></ul>
                </div>
                <button id="vault-ignored-report-folders-ignore">${lib.translate(lib.itlcodes.REPORT_FOLDER_IGNORE)}</button>
                <select id="vault-ignored-report-folders-options"></select>
            </div>
        `
    };

    return uploadsTab;
}