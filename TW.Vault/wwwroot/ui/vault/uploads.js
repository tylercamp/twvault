﻿
function makeUploadsTab() {
    let uploadsTab = {
        label: lib.translate(lib.itlcodes.TAB_UPLOAD),
        containerId: 'vault-uploads-container',

        init: function ($container) {

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

                alert(lib.translate(lib.itlcodes.UPLOAD_CACHE_CLEARED));
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
                                    alertFilter(lib.translate(lib.itlcodes.REPORTS));
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
                                    alertFilter(lib.translate(lib.itlcodes.INCOMINGS));
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
                                    alertFilter(lib.translate(lib.itlcodes.COMMANDS));
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
                                    alertFilter(lib.translate(lib.itlcodes.TROOPS));
                                }
                            }
                        });
                        break;

                    case 'vault-upload-all':
                        $('.status-container').html(`<em>${lib.translate(lib.itlcodes.WAITING)}...</em>`);

                        let resetStatusContainers = () => {
                            $('.status-container').filter((i, el) => $(el).text().toLowerCase().contains(lib.translate(lib.itlcodes.WAITING))).empty();
                        };

                        let runReports = () => {
                            processUploadReports($container.find('#vault-upload-reports .status-container'), runIncomings);
                        };
                        let runIncomings = (didFail) => {
                            if (didFail) {
                                if (didFail == lib.errorCodes.CAPTCHA) {
                                    alertCaptcha();
                                } else if (didFail == lib.errorCodes.FILTER_APPLIED) {
                                    alertFilter(lib.translate(lib.itlcodes.REPORTS));
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
                                    alertFilter(lib.translate(lib.itlcodes.INCOMINGS));
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
                                    alertFilter(lib.translate(lib.itlcodes.TROOPS));
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
                                        alertFilter(lib.translate(lib.itlcodes.COMMANDS));
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
                                $statusContainer.text(msg);
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
                                $statusContainer.text(msg);
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
                                $statusContainer.text(msg);
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
                                $statusContainer.text(msg);
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

            <input type="button" class="upload-clear-cache" value="${lib.translate(lib.itlcodes.UPLOAD_CLEAR_CACHE)}" style="float:right">
        `
    };

    return uploadsTab;
}