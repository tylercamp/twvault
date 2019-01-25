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
    //# REQUIRE ui/ui-lib.js
    //# REQUIRE ui/map.js
    //# REQUIRE ui/vault.js
    //# REQUIRE ui/tag-incomings.js
    //# REQUIRE ui/village-info.js

    //  Store current script host for dependent scripts that rely on it
    lib.setScriptHost(lib.getScriptHost());

    if (!lib.checkUserHasPremium()) {
        alert(lib.translate(lib.itlcodes.REQUIRE_PREMIUM_ACCOUNT, { _escaped: false }));
        return;
    }

    lib.init(() => {
        let isFirstRun = lib.onFirstRun((onAccepted) => {
            let alertString = lib.translate(lib.itlcodes.TERMS_AND_CONDITIONS, { _escaped: false });

            if (confirm(alertString)) {
                alert(lib.translate(lib.itlcodes.RE_RUN_SCRIPT, { _escaped: false }));
                onAccepted();
            } else {
                alert(lib.translate(lib.itlcodes.SCRIPT_NOT_RAN), { _escaped: false });
            }
        });

        if (isFirstRun) {
            return;
        }

        if (lib.versioning.checkNeedsUpdate()) {
            alert(lib.translate(lib.itlcodes.UPDATE_NOTICE, { _escaped: false }));
            lib.versioning.updateForLatestVersion();
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