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

    let translationEntries = {};

    return {
        label: 'Translations',
        containerId: 'vault-translations-container',

        init: function($container) {

            let languageRequest = $.get(lib.makeVaultUrl('api/translation/languages'))
                .done((data) => languages = data);

            let translationsRequest = $.get(lib.makeVaultUrl('api/translation'))
                .done((data) => translations = lib.arrayToObject(data, t => t.languageId));

            lib.whenAll$(() => {

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
                // - etc ... tired

            }, [languageRequest, translationsRequest]);
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
            <br
            
        `
    };
}