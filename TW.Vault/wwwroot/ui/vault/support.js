function makeVaultSupportTab() {

    let userSupportTab = makeVaultSupportInfoTab();
    let translationTab = makeTranslationsTab();

    let tabs = [
        userSupportTab,
        translationTab
    ];

    return {
        label: lib.translate(lib.itlcodes.TAB_SUPPORT),
        containerId: 'vault-support-container',

        getContent: uilib.mkTabbedContainer(userSupportTab, tabs)
    };
}

function makeVaultSupportInfoTab() {
    return {
        label: lib.translate(lib.itlcodes.TAB_HELP),
        containerId: 'vault-user-support-container',

        getContent: `
            <hr>
            <h3>Script Support</h3>
            <p>
                For any questions or bug reports, contact the lead developer, Tyler, on:
                <ul style="display:inline-block;text-align:left">
                    <li>Skype - astpgmr</li>
                    <li>Discord - tcamps#9882</li>
                    <li>External Forums - <a href="https://forum.tribalwars.net/index.php?members/tcamps.121640/">tcamps</a></li>
                </ul>
            </p>
            <hr>
            <h3>Script Legality</h3>
            <p>
                We've taken great care to abide by the rules and keep this script legal. We've been in contact with
                the Tribal Wars support team through various phases of development, and re-submit the script after
                significant changes. We've never been denied approval.
            </p>
            <p>
                For proof, I've created a post on the official Tribal Wars external forums for the Vault, and
                asked a support staff to label it as an "Approved Script", to which they agreed.
            </p>
            <p>
                <b>This script was last approved on 13/12/2018.</b> You can find details and more approval history at
                the forum thread:
                <a href="https://forum.tribalwars.net/index.php?threads/vault.282252/#post-7089474">
                    https://forum.tribalwars.net/index.php?threads/vault.282252/#post-7089474
                </a>
            </p>
            <hr>
            <h3>Script Requests</h3>
            <p>
                To gain access to the Vault on a different world or while in a different tribe, contact Tyler using any of
                the methods above. We are not currently playing any worlds and are impartial to any requests. Your request
                should contain your user name and the name of the world you're playing on.
            </p>
            <p>
                We will not respond to requests to be made a Vault Admin. That must be handled by an existing Vault Admin
                in your tribe. If all of your Vault Admins have left, you'll need to request a new script.
            </p>
            <hr>
        `
    };
}

function makeTranslationsTab() {

    let languages = null;
    let translations = null;
    let translationKeys = null;
    let translationParameters = null;
    let translationReference = null;

    let currentEditingTranslation = null;

    function loadRegistryEditor($container, registry, onLoaded) {
        console.log('Loading registry editor for: ', registry);

        let requests = [];

        if (!registry.entries && registry.id) {
            requests.push($.get(lib.makeVaultUrl(`api/translation/${registry.id}/contents`)).done((data) => {
                currentEditingTranslation = data;
                registry = data;
                console.log('Loaded registry contents: ', registry);
            }));
        }

        if (!translationKeys) {
            requests.push($.get(lib.makeVaultUrl('api/translation/translation-keys')).done((data) => translationKeys = data));
        }

        if (!translationParameters) {
            requests.push($.get(lib.makeVaultUrl('api/translation/parameters')).done((data) => translationParameters = data));
        }

        if (!translationReference) {
            requests.push($.get(lib.makeVaultUrl('api/translation/reference')).done((data) => translationReference = data));
        }

        let $entriesContainer = $container.find('#vault-translation-registry-entries');
        let $infoContainer = $container.find('#vault-translation-registry-info');

        $infoContainer.empty();
        $entriesContainer.empty();

        function makeGroupEditor($container, name, group) {
            lib.selectSort(group, (e) => e.name);
            let toggleButtonText = () => {
                let numExisting = group.filter((k) => !!registry.entries[k.name]).length;
                return `${numExisting == group.length ? '' : '(!) '}${name} (${numExisting}/${group.length})`
            };

            let $groupContainer = $(`
                <div class="vault-translation-key-group-container" style="margin-bottom:1em">
                    ${uilib.mkBtn('vault-translation-key-group-toggle-' + name, toggleButtonText())}
                    <table id="vault-translation-key-${name}-group-entry-container" style="display:none; width:100%">
                        <thead>
                            <tr>
                                <th style="width:20%">${lib.translate(lib.itlcodes.TRANSLATION_EDIT_KEY)}</th>
                                <th>${lib.translate(lib.itlcodes.TRANSLATION_EDIT_VALUE)}</th>
                                <th style="width:35%">${lib.translate(lib.itlcodes.TRANSLATION_EDIT_SAMPLE)}</th>
                            </tr>
                        </thead>
                        <tbody></tbody>
                    </table>
                </div>
            `.trim());

            let $entriesContainer = $groupContainer.find(`#vault-translation-key-${name}-group-entry-container tbody`);
            let $toggleButton = $groupContainer.find('#vault-translation-key-group-toggle-' + name);

            $toggleButton.click(() => {
                $entriesContainer.closest('table').toggle();
            });

            group.forEach((k) => makeKeyEditor($entriesContainer, k, registry, translationReference));

            $entriesContainer.find('textarea').change((ev) => {
                let $el = $(ev.target);
                let keyName = $el.closest('tr').data('key-name');
                let val = $el.val().trim();

                let parameters = translationParameters[keyName];
                if (parameters) {
                    let missingParameters = [];
                    parameters.forEach((paramName) => {
                        let paramText = `{${paramName}}`;
                        if (!val.contains(paramText)) {
                            missingParameters.push(paramText);
                        }
                    });

                    if (missingParameters.length) {
                        alert(lib.translate(lib.itlcodes.TRANSLATION_EDIT_MISSING_PARAMS, {
                            _escaped: false,
                            keyName: keyName,
                            parameters: missingParameters.join(", ")
                        }));
                        return;
                    }
                }

                registry.entries[keyName] = val;

                $toggleButton.text(toggleButtonText());
            });

            $container.append($groupContainer);

            onLoaded && onLoaded();
        }

        function makeKeyEditor($container, key, registry, reference) {
            $container.append(`
                <tr data-key-id="${key.id}" data-key-name="${key.name}">
                    <td>${key.name}</td>
                    <td>
                        <textarea style="width:100%;margin:0" rows="2">${registry.entries[key.name] || ''}</textarea>
                    </td>
                    <td>
                        <textarea readonly style="width:100%;margin:0" rows="2">${reference.entries[key.name] || ''}</textarea>
                    </td>
                </tr>
            `);

            if (key.isTwNative) {
                $container.append(`
                    <tr>
                        <td colspan="3" style="text-align:center">
                            <em style="padding-bottom:1.5em;display:inline-block;">${lib.translate(lib.itlcodes.TRANSLATION_EDIT_NEEDS_EXACT)}</em>
                        </td>
                    </tr>
                `);
            }
        }

        lib.whenAll$(requests, () => {

            if (registry.authorPlayerId && registry.authorPlayerId != lib.getCurrentPlayerId()) {
                alert(lib.translate(lib.itlcodes.TRANSLATION_MAKING_COPY, { _escaped: false }));
                delete registry.authorPlayerId;
                delete registry.id;
            }

            $infoContainer.empty();
            $entriesContainer.empty();

            $container.css('display', 'block');
            
            $infoContainer.append(`<h3>Edit</h3>`);
            $infoContainer.append(`
                <table style="display:inline-block">
                    <tr>
                        <td>Language: </td>
                        <td>
                            <select id="vault-edit-translation-language" ${registry.id ? 'disabled' : ''}>
                                ${languages.map(l => `<option value="${l.id}" ${l.id == registry.languageId ? 'selected' : ''}>${l.name}</option>`).join('\n')}
                            </select>
                        </td>
                    </tr>
                    <tr>
                        <td>Name: </td>
                        <td><input type="text" id="vault-edit-translation-name"></td>
                    </tr>
                </table>
            `);

            $infoContainer.find('#vault-edit-translation-name').val(registry.name);

            $infoContainer.find('#vault-edit-translation-name').change((e) => {
                registry.name = $(e.target).val();
                console.log('Changed registry name to: ', registry.name);
            });

            $infoContainer.find('#vault-edit-translation-language').change((e) => {
                registry.translationId = $(e.target).val();
                console.log('Changed translation ID to: ', registry.translationId);
            });

            let groupedKeys = lib.groupBy(translationKeys, k => k.group);
            lib.selectSort(groupedKeys, (k) => k.key);
            groupedKeys.forEach((group) => {
                makeGroupEditor($entriesContainer, group.key, group.values);
            });

            $entriesContainer.find('table tr').find('td:first-of-type,th:first-of-type').css('width', '100px');
            $entriesContainer.find('table tr td:first-of-type').css({
                'font-size': '0.8em',
                'word-break': 'break-word',
                'padding-bottom': '1em'
            });

            $entriesContainer.find('textarea').css('box-sizing', 'border-box');

            uilib.applyStyles($infoContainer, {
                'tr': () => ({
                    'td:first-of-type': {
                        'text-align': 'right'
                    },
                    'td:last-of-type': {
                        'text-align': 'left'
                    },
                    'select, input': {
                        'width': '100%',
                        'box-sizing': 'border-box'
                    }
                })
            });
        });
    }

    return {
        label: lib.translate(lib.itlcodes.TAB_TRANSLATIONS),
        containerId: 'vault-translations-container',

        init: function($container) {

            let languageRequest = $.get(lib.makeVaultUrl('api/translation/languages'))
                .done((data) => {
                    languages = data;
                    console.log('Got languages: ', languages);
                });

            let translationsRequest = $.get(lib.makeVaultUrl('api/translation'))
                .done((data) => {
                    translations = lib.arrayToObjectByGroup(data, t => t.languageId);
                    console.log('Got translations: ', data, translations);
                });

            lib.whenAll$([languageRequest, translationsRequest], () => {

                console.log('Loading translation UI');

                // Create empty list for languages without any translations
                languages.filter((l) => !translations[l.id]).forEach((l) => translations[l.id] = []);

                let libTranslation = lib.getCurrentTranslation();

                let $currentLanguage = $container.find('#vault-current-language');
                let $currentTranslation = $container.find('#vault-current-translation');

                languages.forEach(l => {
                    $currentLanguage.append(`<option value="${l.id}" ${l.id == libTranslation.languageId ? 'selected' : ''}>${l.name}</option>`);
                });

                loadTranslationOptions(libTranslation.languageId);

                function loadTranslationOptions(languageId) {
                    $currentTranslation.empty();
                    translations[languageId].forEach(t => {
                        let label = lib.translate(lib.itlcodes.TRANSLATION_AUTHOR, { name: t.name, author: t.author });
                        $currentTranslation.append(`<option value="${t.id}" ${t.id == libTranslation.id ? 'selected' : ''}>${label}</option>`);
                    });
                }

                $currentLanguage.change(() => {
                    loadTranslationOptions(parseInt($currentLanguage.val()));
                });

                $container.find('#vault-save-translation-settings').click(() => {
                    lib.setCurrentTranslation(parseInt($currentTranslation.val()));
                    lib.getCurrentTranslationAsync();
                });

                $container.find('#vault-edit-translation').click(() => {
                    $container.find('#vault-delete-translation').attr('disbaled', false);
                    let selectedId = parseInt($currentTranslation.val());
                    loadRegistryEditor($container.find('#vault-translation-registry'), {
                        id: selectedId
                    });
                });

                $container.find('#vault-new-translation').click(() => {
                    $container.find('#vault-delete-translation').attr('disabled', true);

                    currentEditingTranslation = lib.createNewTranslation();
                    currentEditingTranslation.languageId = parseInt($currentLanguage.val());
                    loadRegistryEditor($container.find('#vault-translation-registry'), currentEditingTranslation);
                });

                $container.find('#vault-delete-translation').click(() => {
                    if (!currentEditingTranslation.id) {
                        alert(lib.translate(lib.itlcodes.TRANSLATION_NOT_SAVED, { _escaped: false }));
                        return;
                    }

                    if (!confirm(lib.translate(lib.itlcodes.TRANSLATION_DELETE_CONFIRM, { _escaped: false, name: currentEditingTranslation.name })))
                        return;

                    lib.deleteApi(`playertranslation/${currentEditingTranslation.id}`)
                        .done(() => {
                            let message = lib.translate(lib.itlcodes.TRANSLATION_DELETED, { _escaped: false });
                            // Reset the current translation if that's what they deleted
                            if (lib.getSavedTranslationId() == currentEditingTranslation.id) {
                                lib.setCurrentTranslation(null);
                                lib.getCurrentTranslationAsync();
                            }
                            alert(message);

                            $container.find('#vault-translation-registry').css('display', 'none');

                            translations[currentEditingTranslation.languageId].removeWhere((t) => t.id == currentEditingTranslation.id);
                            $container.find('option[value=' + currentEditingTranslation.id + ']').remove();
                            currentEditingTranslation = null;
                        })
                        .error((xhr) => {
                            switch (xhr.status) {
                                case 409: alert(lib.translate(lib.itlcodes.TRANSLATION_DELETE_DEFAULT, { _escaped: false })); break;
                                default: alert(lib.messages.GENERIC_ERROR); break;
                            }
                        });
                });

                $container.find('#vault-save-translation-registry-changes').click((e) => {
                    let $button = $(e.target);
                    $button.attr('disabled', true);
                    let method = currentEditingTranslation.id ? lib.putApi : lib.postApi;
                    method('playertranslation', currentEditingTranslation)
                        .done((result) => {
                            $button.attr('disabled', false);
                            $container.find('#vault-delete-translation').attr('disabled', false);

                            let translationLabel = lib.translate(lib.itlcodes.TRANSLATION_AUTHOR, {
                                name: currentEditingTranslation.name,
                                author: currentEditingTranslation.author || lib.getCurrentPlayerName()
                            });

                            alert(lib.translate(lib.itlcodes.TRANSLATION_SAVE_CHANGES_SUCCESS, { _escaped: false }))
                            // Add entry for newly-added translation
                            if (!currentEditingTranslation.id) {
                                currentEditingTranslation.id = result.newRegistryId;
                                translations[currentEditingTranslation.languageId].push({
                                    id: result.newRegistryId,
                                    languageId: currentEditingTranslation.languageId,
                                    authorPlayerId: lib.getCurrentPlayerId(),
                                    name: currentEditingTranslation.name,
                                    author: lib.getCurrentPlayerName()
                                });

                                if (currentEditingTranslation.languageId == $currentLanguage.val()) {
                                    $currentTranslation.append(`<option value="${result.newRegistryId}">${translationLabel}</option>`);
                                }

                                $container.find('#vault-edit-translation-language').attr('disabled', true);
                            } else {
                                // Update name if necessary
                                if ($currentLanguage.val() == currentEditingTranslation.languageId) {
                                    $currentTranslation.find(`option[value=${currentEditingTranslation.id}]`).text(translationLabel);
                                }

                                translations[currentEditingTranslation.languageId].updateWhere(t => t.id == currentEditingTranslation.id, t => t.name = currentEditingTranslation.name);
                            }
                        })
                        .error((xhr) => {
                            $button.attr('disabled', false);
                            switch (xhr.status) {
                                case 409: alert(lib.translate(lib.itlcodes.TRANSLATION_DUPLICATE, { _escaped: false, name: currentEditingTranslation.name })); break;
                                default: alert(lib.messages.GENERIC_ERROR); break;
                            }
                        });
                });

            });
        },

        getContent: `
            <hr>
            <h3>${lib.translate(lib.itlcodes.TAB_TRANSLATIONS)}</h3>

            <table style="display:inline-block" id="vault-select-translation-container">
                <tr>
                    <td>
                        <label for="vault-current-language">${lib.translate(lib.itlcodes.TRANSLATION_LANGUAGE)}</label>
                    </td>
                    <td>
                        <select id="vault-current-language"></select>
                    </td>
                </tr>
                <tr>
                    <td>
                        <label for="vault-current-translation">${lib.translate(lib.itlcodes.TRANSLATION_TRANSLATION)}</label>
                    </td>
                    <td>
                        <select id="vault-current-translation"></select>
                    </td>
                </tr>
            </table>
            <br>
            <button id="vault-save-translation-settings">${lib.translate(lib.itlcodes.TRANSLATION_SAVE_SETTINGS)}</button>
            <br>
            <br>
            <button id="vault-edit-translation">${lib.translate(lib.itlcodes.TRANSLATION_EDIT)}</button>
            <button id="vault-new-translation">${lib.translate(lib.itlcodes.TRANSLATION_NEW)}</button>
            <div id="vault-translation-registry" style="display:none">
                <hr>

                <div id="vault-translation-registry-info" style="margin-top:2em">
                </div>
            
                <div id="vault-translation-registry-entries"></div>
                
                <button id="vault-save-translation-registry-changes">${lib.translate(lib.itlcodes.TRANSLATION_SAVE_CHANGES)}</button>
                <button id="vault-delete-translation">${lib.translate(lib.itlcodes.TRANSLATION_DELETE)}</button>
            </div>
        `,

        getStyle: {
            '#vault-select-translation-container': () => ({
                'tr': () => ({
                    'td:first-of-type': {
                        'text-align': 'right'
                    },
                    'td:last-of-type': {
                        'text-align': 'left'
                    }
                }),

                'select': {
                    'width': '100%'
                }
            }),

            '#vault-translation-registry-info': {
                'margin-top': '2em'
            },

            '#vault-translation-registry-entries': {
                'margin-top': '1em',
                'text-align': 'left',
                'max-height': '600px',
                'overflow-y': 'scroll'
            },

            '#vault-translation-registry > hr': {
                'margin': '1.5em 2em',
            },

            '#vault-save-translation-registry-changes': {
                'margin-top': '1.5em'
            }
        }
    };
}