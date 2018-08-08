function parseAllReportsPage($doc) {

    $doc = $doc || $(document);

    //# REQUIRE lib.js
    //# REQUIRE requestManager.js

    lib.ensurePage(lib.pageTypes.ALL_REPORTS);

    var requestManager = new RequestManager();
    requestManager.refreshDelay = 300;

    let previousReports = JSON.parse(localStorage.getItem('vault-reports-history') || '[]');

    let $reportLinks = $doc.find('#report_list tr:not(:first-child):not(:last-child) a:not(.rename-icon)');
    $reportLinks.each((i, el) => {
        let $el = $(el);

        let link = $el.prop('href');
        let reportId = parseInt(link.match(/view=(\w+)/)[1]);
        if (previousReports.indexOf(reportId) >= 0) {
            toggleReport($el);
            return;
        }

        requestManager.addRequest(link, (data, request) => {
            if (data) {
                let $doc = $(data);
                parseReportPage($doc, link, false);
                toggleReport($el);
            }
            updateUploadsDisplay();
        });
    });

    requestManager.setFinishedHandler(() => {
        alert('Done!');
        let stats = requestManager.getStats();
        setUploadsDisplay(`Finished: ${stats.done}/${stats.total} uploaded, ${stats.numFailed} failed`);
    });

    makeUploadsDisplay();

    if (!requestManager.getStats().total) {
        setUploadsDisplay('No new reports to upload.');
        alert('No new reports to upload!');
    } else {
        requestManager.start();
    }

    function makeUploadsDisplay() {
        let $uploadsContainer = $('<div id="vault-uploads-display">');
        $doc.find('#report_list').parent().prepend($uploadsContainer);
        updateUploadsDisplay();
    }

    function updateUploadsDisplay() {
        let stats = requestManager.getStats();
        setUploadsDisplay(`Uploading ${stats.total} reports... (${stats.done} done, ${stats.numFailed} failed)`);
    }

    function setUploadsDisplay(contents) {
        let $uploadsContainer = $doc.find('#vault-uploads-display');
        $uploadsContainer.text(contents);
    }

    function toggleReport($link) {
        $link.closest('tr').find('td:first-of-type input').prop('checked', true);
    }

};