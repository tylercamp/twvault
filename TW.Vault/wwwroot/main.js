(() => {

    /*** Libraries ***/

    //# REQUIRE lib.js
    //# REQUIRE twtypes.js
    //# REQUIRE requestManager.js


    /***  Pages ***/
    //# REQUIRE pages/view-report.js
    //# REQUIRE pages/all-reports.js
    //# REQUIRE pages/incomings-overview.js
    //# REQUIRE pages/own-commands-overview.js
    //# REQUIRE pages/troops-overview.js
    //# REQUIRE pages/map.js

    //# REQUIRE pages/all.js

    //  Store current script host for dependent scripts that rely on it
    lib.setScriptHost(lib.getScriptHost());

    if (!lib.checkUserHasPremium()) {
        alert('This script cannot be used without a premium account!');
        return;
    }

    lib.init(() => {
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
            .onPage(lib.pageTypes.ALL_REPORTS, () => {
                parseAllReportsPage();
            })
            //.onPage(lib.pageTypes.INCOMINGS_OVERVIEW, () => {
            //    parseIncomingsOverviewPage();
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