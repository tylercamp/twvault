function makeVaultSupportTab() {

    let userSupportTab = makeVaultSupportInfoTab();
    let translationTab = makeTranslationsTab();

    let tabs = [
        userSupportTab,
        translationTab
    ];

    return {
        label: 'Support',
        containerId: 'vault-support-container',

        getContent: uilib.mkTabbedContainer(userSupportTab, tabs)
    };
}

function makeVaultSupportInfoTab() {
    return {
        label: 'Help',
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

    let currentEditingTranslation = null;

    function loadRegistryEditor($container, registry, onLoaded) {
        console.log('Loading registry editor for: ', registry);

        let requests = [];

        if (!translationKeys) {
            requests.push($.get(lib.makeVaultUrl('api/translation/translation-keys')).done((data) => translationKeys = data));
        }

        if (!translationParameters) {
            requests.push($.get(lib.makeVaultUrl('api/translation/parameters')).done((data) => translationParameters = data));
        }

        let $entriesContainer = $container.find('#vault-translation-registry-entries');
        let $infoContainer = $container.find('#vault-translation-registry-info');

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
                                <th style="width:20%">Key</th>
                                <th>Value</th>
                                <th style="width:35%">English Example</th>
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

            group.forEach((k) => makeKeyEditor($entriesContainer, k, registry));

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
                        alert(`The translation for "${keyName}" is missing: ${missingParameters.join(", ")}`);
                        return;
                    }
                }

                registry.entries[keyName] = val;

                $toggleButton.text(toggleButtonText());
            });

            $container.append($groupContainer);

            onLoaded && onLoaded();
        }

        function makeKeyEditor($container, key, registry) {
            $container.append(`
                <tr data-key-id="${key.id}" data-key-name="${key.name}">
                    <td>${key.name}</td>
                    <td>
                        <textarea style="width:100%;margin:0" rows="2">${registry.entries[key.name] || ''}</textarea>
                    </td>
                </tr>
            `);
        }

        lib.whenAll$(requests, () => {

            //  TODO - Load general info

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
        });
    }

    return {
        label: 'Translations',
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

                let $currentLanguage = $container.find('#vault-current-language');
                let $currentTranslation = $container.find('#vault-current-translation');

                languages.forEach(l => {
                    $currentLanguage.append(`<option selected value="${l.id}">${l.name}</option>`);
                });

                let lang = languages[0].id;

                translations[lang].forEach(t => {
                    $currentTranslation.append(`<option selected value="${t.id}">${t.name} by ${t.author}`);
                });

                // TODO:
                // - Create translation button
                // - Save translation changes
                // - Inputs for author/etc
                // - Save language/translation settings

                $container.find('#vault-edit-translation').click(() => {
                    currentEditingTranslation = lib.getCurrentTranslation();
                    loadRegistryEditor($container, currentEditingTranslation);
                });

                $container.find('#vault-save-translation-registry-changes').click(() => {
                    lib.putApi('playertranslation', currentEditingTranslation)
                        .done(() => alert('Saved changes!'))
                        .error(() => alert('An error occurred...'));
                });

            });
        },

        getContent: `
            <hr>
            <h3>Translations</h3>

            <label for="vault-current-language">Language: </label>
            <select id="vault-current-language"></select>
            <br>
            <label for="vault-current-translation">Translation: </label>
            <select id="vault-current-translation"></select>
            <br>
            <button id="vault-save-translation-settings">Save</button>
            <br>
            <br>
            <button id="vault-edit-translation">Edit Translation</button>
            <button id="vault-new-translation">New Translation</button>
            <div id="vault-translation-registry-info">
                <div id="vault-translation-registry-info">
                </div>
            
                <div id="vault-translation-registry-entries" style="text-align:left;max-height:600px;overflow-y:scroll"></div>
                
                <button id="vault-save-translation-registry-changes">Save Changes</button>
            </div>
            
        `
    };
}