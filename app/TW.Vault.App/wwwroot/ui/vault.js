function displayMainVaultUI($doc) {

    //# REQUIRE ui/vault/admin.js
    //# REQUIRE ui/vault/terms.js
    //# REQUIRE ui/vault/uploads.js
    //# REQUIRE ui/vault/tools.js
    //# REQUIRE ui/vault/stats.js
    //# REQUIRE ui/vault/actions.js
    //# REQUIRE ui/vault/support.js

    $doc = $doc || $(document);

    let uploadsTab = makeUploadsTab();
    let adminTab = makeAdminTab();
    let termsTab = makeTermsTab();
    let toolsTab = makeToolsTab();
    let statsTab = makeStatsTab();
    let actionsTab = makeActionsTab();
    let supportTab = makeVaultSupportTab();

    let onClosedListeners = [];

    let tabs = [
        uploadsTab,
        statsTab,
        actionsTab,
        toolsTab,
        adminTab,
        termsTab,
        supportTab
    ];

    $doc.find('#vault-ui-container').remove();
    var $uiContainer = $(`
        <div id="vault-ui-container" class="confirmation-box" style="border-width: 20px">
            <!-- Weird margins on this since TW styling has an annoying gap -->
            <div class="confirmation-box-content-pane" style="min-height:100%;margin-left:-1px;margin-bottom:-1px">
                <div class="confirmation-box-content" style="min-height:100%">
                    <h3>${lib.translate(lib.itlcodes.VAULT)}</h3>
                    <p>
                        ${lib.translate(lib.itlcodes.VAULT_INTERFACE_DESCRIPTION)}
                    </p>

                    ${uilib.mkTabbedContainer(uploadsTab, tabs)}

                    <p style="font-size:12px">
                        Vault server and script by: Tyler (tcamps/False Duke), Glen (vahtos/TheBossPig)
                        <br>
                        Please report any bugs and feature requests to the maintainers.
                    </p>
                </div>
                <div class="confirmation-buttons">
                    <button class="btn vault-close-btn btn-confirm-yes">${lib.translate(lib.itlcodes.DONE)}</button>
                </div>
            </div>
        </div>
    `.trim()).css({
        position: 'absolute',
        width: '800px',
        margin: 'auto',
        left: 0, right: 0,
        top: '100px',
        'z-index': 9999
        });

    $uiContainer.find('th').css({
        'font-size': '14px'
    });

    $uiContainer.find('td').css({
        'font-weight': 'normal'
    });

    $uiContainer.find('th, td').css({
        'text-align': 'center'
    });

    $uiContainer.find('td').addClass('lit-item');

    $uiContainer.find('.upload-button:not(.upload-button-all)').css({
        float: 'left',
        margin: '0 1em'
    });

    $uiContainer.find('.cancel-button').css({
        float: 'right',
        margin: '0 1em'
    });

    $doc.find('body').prepend($uiContainer);

    

    $uiContainer.find('.vault-close-btn').click(() => {
        let isUploading = $('.upload-button').prop('disabled');
        if (isUploading && !confirm(lib.translate(lib.itlcodes.UPLOADING_IF_CLOSED))) {
            return;
        }
        $uiContainer.remove();

        onClosedListeners.forEach((c) => c());
    });

    uilib.init($uiContainer);

    return {
        onClosed: function addOnClosedListener(callback) {
            onClosedListeners.push(callback);
        }
    };
}