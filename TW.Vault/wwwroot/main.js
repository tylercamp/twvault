(() => {

    /*** Libraries ***/

    //# REQUIRE lib.js
    //# REQUIRE twtypes.js
    //# REQUIRE requestManager.js


    /***  Pages ***/
    //# REQUIRE pages/view-report.js
    //# REQUIRE pages/all-reports.js
    //# REQUIRE pages/all-incomings.js
    //# REQUIRE pages/all-troops.js
    //# REQUIRE pages/all-commands.js
    //# REQUIRE pages/incomings-overview-tag.js
    //# REQUIRE pages/own-commands-overview.js
    //# REQUIRE pages/map.js

    //# REQUIRE pages/all.js

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

        lib
            //.onPage(lib.pageTypes.UNKNOWN, () => {
            //    var supportedPages = lib.objectToArray(lib.pageTypes, (v) => v != lib.pageTypes.UNKNOWN ? v.replace(/\_/g, ' ').toLowerCase() : undefined);

            //    alert("This script can't be ran on the current page. It can be ran on these pages: \n\n" + supportedPages.join("\n"));
            //})
            .onPage(lib.pageTypes.MAP, () => {
                parseMapPage();
            })
            .onPage(lib.pageTypes.VIEW_REPORT, () => {
                parseReportPage();
            })
            //.onPage(lib.pageTypes.INCOMINGS_OVERVIEW, () => {
            //    parseAllIncomings();
            //    //parseUploadIncomingsOverviewPage();
            //    //parseTagIncomingsOverviewPage();
            //})
            //.onPage(lib.pageTypes.OWN_COMMANDS_OVERVIEW, () => {
            //    parseOwnCommandsOverviewPage();
            //})
            //.onPage(lib.pageTypes.OWN_TROOPS_OVERVIEW, () => {
            //    parseOwnTroopsOverviewPage();
            //})
            .onPageNotHandled(() => {
                parseAllPages();
            })
        ;
    });

})();