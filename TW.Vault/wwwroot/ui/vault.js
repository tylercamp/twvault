﻿function parseAllPages($doc) {

    //# REQUIRE ui/vault/ui-lib.js

    //# REQUIRE ui/vault/admin.js
    //# REQUIRE ui/vault/sms.js
    //# REQUIRE ui/vault/terms.js
    //# REQUIRE ui/vault/uploads.js

    $doc = $doc || $(document);

    let uploadsTab = makeUploadsTab();
    let smsTab = makeSmsTab();
    let adminTab = makeAdminTab();
    let termsTab = makeTermsTab();

    let tabs = [
        uploadsTab,
        smsTab,
        adminTab,
        termsTab
    ];

    $doc.find('#vault-ui-container').remove();
    var $uiContainer = $(`
        <div id="vault-ui-container" class="confirmation-box" style="border-width: 20px">
        <!-- Weird margins on this since TW styling has an annoying gap -->
        <div class="confirmation-box-content-pane" style="min-height:100%;margin-left:-1px;margin-bottom:-1px">
        <div class="confirmation-box-content" style="min-height:100%">
            <h3>Vault</h3>
            <p>
                Here you can upload data to the Vault or set up SMS notifications. Run this script on your Map or on your Incomings to
                see everything the Vault has to offer.
            </p>

            ${uilib.mkTabbedContainer(uploadsTab.containerId, tabs)}

            <p style="font-size:12px">
                Vault server and script by: Tyler (tcamps/False Duke), Glen (vahtos/TheBossPig)
                <br>
                Please report any bugs and feature requests to the maintainers.
            </p>
        </div>
        <div class="confirmation-buttons">
            <button class="btn vault-close-btn btn-confirm-yes">Done</button>
        </div>
        </div>
        </div>
    `.trim()).css({
        position: 'absolute',
        width: '800px',
        margin: 'auto',
        left: 0, right: 0,
        top: '100px',
        'z-index': 999999999
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
        if (isUploading && !confirm("Current uploads will continue running while this popup is closed.")) {
            return;
        }
        $uiContainer.remove()
    });

    uilib.init($uiContainer);
}