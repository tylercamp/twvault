
function makeTermsTab() {
    return {
        label: 'Disclaimers and Terms',
        containerId: 'vault-terms-container',

        getContent: `
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
        `
    };
}