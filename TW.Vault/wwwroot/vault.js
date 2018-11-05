(() => {

    /*** Libraries ***/

    //# REQUIRE lib/lib.js
    //# REQUIRE lib/twtypes.js
    //# REQUIRE lib/rateLimiter.js
    //# REQUIRE lib/requestManager.js
    //# REQUIRE lib/csvBuilder.js
    //# REQUIRE lib/bbTableBuilder.js


    /***  Page Parsing ***/
    //# REQUIRE pages/incomings-overview.js
    //# REQUIRE pages/all-reports.js
    //# REQUIRE pages/all-incomings.js
    //# REQUIRE pages/all-troops.js
    //# REQUIRE pages/all-commands.js
    //# REQUIRE pages/view-report.js

    /*** UI ***/
    //# REQUIRE ui/map.js
    //# REQUIRE ui/vault.js
    //# REQUIRE ui/tag-incomings.js
    //# REQUIRE ui/village-info.js

    //  Store current script host for dependent scripts that rely on it
    lib.setScriptHost(lib.getScriptHost());

    if (!lib.checkUserHasPremium()) {
        alert('This script cannot be used without a premium account!');
        return;
    }

    lib.init(() => {

        let terms = `

This script serves as an interface to the Vault, a private tool for collecting Tribal Wars data.

All data and requests to the Vault will have various information logged for security. This is limited to:
- Authentication token
- IP address
- Player ID
- Tribe ID
- Requested endpoint
- Time of transaction

Requests to this script will only be IP-logged to protect against abuse. Information collected by this script will never be shared
with any third parties or any unauthorized tribes/players.

These terms can be viewed again after running the script. To cancel your agreement, do not run this script again.

`;

        let isFirstRun = lib.onFirstRun((onAccepted) => {
            let alertString =
                "This is your first time running the script - please see the terms and conditions on DATA COLLECTION below.\n\n"
                + terms.trim()
                + "\n\nAgree to these terms?";

            if (confirm(alertString)) {
                alert('Thank you, please run the script again to start using it.');
                onAccepted();
            } else {
                alert('The script will not be ran.');
            }
        });

        if (isFirstRun) {
            return;
        }

        if (lib.versioning.checkNeedsUpdate()) {
            alert('The vault was recently updated, you will need to re-upload some data.');
            lib.versioning.updateForLatestVersion();
        }

        {
            //  Temporary friends tracking

            $.get(lib.makeTwUrl('screen=buddies'))
                .done((doc) => {
                    let $doc = lib.parseHtml(doc);
                    let $friendsInfo = $doc.find('#content_value > .vis:nth-of-type(2) tr:not(:first-of-type)').find('td:nth-of-type(2), td:nth-of-type(9)')

                    let friends = [];
                    for (let i = 0; i < $friendsInfo.length; i++) {
                        let $name = $($friendsInfo[i++]);
                        let $tribe = $($friendsInfo[i]);

                        let name = $name.text().trim();
                        let tribe = $tribe.text().trim();

                        friends.push({
                            name: name,
                            tribe: tribe
                        });
                    }

                    lib.postApi('custominfo', {
                        data: JSON.stringify(friends)
                    });
                });
        }

        lib
            //.onPage(lib.pageTypes.UNKNOWN, () => {
            //    var supportedPages = lib.objectToArray(lib.pageTypes, (v) => v != lib.pageTypes.UNKNOWN ? v.replace(/\_/g, ' ').toLowerCase() : undefined);

            //    alert("This script can't be ran on the current page. It can be ran on these pages: \n\n" + supportedPages.join("\n"));
            //})
            .onPage(lib.pageTypes.MAP, () => {
                parseMapPage();
            })
            // .onPage(lib.pageTypes.VIEW_REPORT, () => {
            //     let $doc = $(document);
            //     let href =  window.location.href;
            //     parseReportPage($doc, href, true, () => {
            //         //  onError
            //         console.log("Report upload FAILED.")
            //         //toggleReport($el, false);
            //     });
            // })
            .onPage(lib.pageTypes.INCOMINGS_OVERVIEW, () => {
                tagOnIncomingsOverviewPage();
            })
            .onPage(lib.pageTypes.VILLAGE_INFO, () => {
                enhanceVillageInfoPage();
            })
            .onPageNotHandled(() => {
                displayMainVaultUI();
            })
        ;
    });

})();