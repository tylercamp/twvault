
function makeTermsTab() {
    return {
        label: lib.translate(lib.itlcodes.TAB_TERMS),
        containerId: 'vault-terms-container',

        getContent: `
            <p>
                <em>${lib.translate(lib.itlcodes.TERMS_NOT_INNO)}</em>
            </p>
            <p>
                <em>
                    ${lib.translate(lib.itlcodes.TERMS_DETAILS)}
                </em>
            </p>
        `
    };
}