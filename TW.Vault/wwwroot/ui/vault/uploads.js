
function makeUploadsTab() {
    let uploadsTab = {
        label: 'Upload Reports, Troops, Etc.',
        containerId: 'vault-uploads-container',

        init: function ($container) {

            var uploadDetailsMessages = {
                'vault-upload-reports': `Uploads all data from all new battle reports.`,
                'vault-upload-incomings': `Uploads all available data from your Incomings page. This includes attacks and support.`,
                'vault-upload-commands': `Uploads all data for all of your current commands.`,
                'vault-upload-troops': `Uploads all data for all troops.`
            };

            $container.find('.details-button').click((ev) => {
                var $el = $(ev.target);
                var uploadType = $el.closest('tr').attr('id');

                alert(uploadDetailsMessages[uploadType]);
            });

            $container.find('.upload-clear-cache').click((ev) => {
                lib.deleteLocalStorage('reports-history');
                lib.deleteLocalStorage('commands-history');

                alert('Local vault cache cleared.');
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
                                    alertFilter('reports');
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
                                    alertFilter('incomings');
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
                                    alertFilter('commands');
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
                                    alertFilter('troops');
                                }
                            }
                        });
                        break;

                    case 'vault-upload-all':
                        $('.status-container').html('<em>Waiting...</em>');

                        let resetStatusContainers = () => {
                            $('.status-container').filter((i, el) => $(el).text().toLowerCase().contains("waiting")).empty();
                        };

                        let runReports = () => {
                            processUploadReports($container.find('#vault-upload-reports .status-container'), runIncomings);
                        };
                        let runIncomings = (didFail) => {
                            if (didFail) {
                                if (didFail == lib.errorCodes.CAPTCHA) {
                                    alertCaptcha();
                                } else if (didFail == lib.errorCodes.FILTER_APPLIED) {
                                    alertFilter();
                                } else {
                                    alert('An unexpected error occurred: ' + didFail);
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
                                    alertFilter();
                                } else {
                                    alert('An unexpected error occurred: ' + didFail);
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
                                    alertFilter();
                                } else {
                                    alert('An unexpected error occurred: ' + didFail);
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
                                        alertFilter();
                                    } else {
                                        alert('An unexpected error occurred: ' + didFail);
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

                            let $doc = lib.parseHtml(data);
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

                            let $doc = lib.parseHtml(data);
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

                            let $doc = lib.parseHtml(data);
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
            });
        },

        getContent: function () {
            return `
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

                <input type="button" class="upload-clear-cache" value="Clear Cache" style="float:right">
            `;
        }
    };

    return uploadsTab;
}