function parseAllReportsPage($doc, onProgress_, onDone_) {

    $doc = $doc || $(document);

    //# REQUIRE lib.js
    //# REQUIRE requestManager.js

    lib.ensurePage(lib.pageTypes.ALL_REPORTS);

    var requestManager = new RequestManager();
    requestManager.refreshDelay = 500;

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

        let $icon = $el.closest('td').find('img:first-of-type');

        var isBattleReport = false;
        $icon.each((_, el) => {
            let icon = $(el).attr('src');
            if (icon.contains("/dots/") || icon.contains("attack"))
                isBattleReport = true;
        });

        if (!isBattleReport)
            return;



        requestManager.addRequest(link, (data, request) => {
            if (data) {
                if (lib.checkContainsCaptcha(data)) {

                    if (requestManager.isRunning()) {
                        requestManager.stop();
                        let statusMessage = `Tribal wars Captcha was triggered, please refresh the page and try again.`;
                        if (onProgress_)
                            onProgress_(statusMessage);

                        if (onDone_)
                            onDone_('captcha');
                        else
                            alert(statusMessage);
                    }

                    return;
                }

                let $doc = $(data);
                parseReportPage($doc, link, false, () => {
                    //  onError
                    requestManager.getStats().numFailed++;
                    toggleReport($el, false);
                });
                toggleReport($el);
            }

            updateUploadsDisplay();
        });
    });

    requestManager.setFinishedHandler(() => {
        let stats = requestManager.getStats();

        let statusMessage = `Finished: ${stats.done}/${stats.total} uploaded, ${stats.numFailed} failed.`;
        if (onProgress_)
            onProgress_(statusMessage);

        if (!onDone_) {
            alert('Done!');
            let stats = requestManager.getStats();
            setUploadsDisplay(statusMessage);
        } else {
            onDone_(false);
        }
    });

    makeUploadsDisplay();

    if (!requestManager.getStats().total) {
        if (!onDone_) {
            setUploadsDisplay('No new reports to upload.');
            alert('No new reports to upload!');
        } else {
            if (onProgress_)
                onProgress_('Done - no new reports to upload.');
            if (onDone_)
                onDone_(false);
        }
    } else {
        requestManager.start();
    }

    function makeUploadsDisplay() {
        if (onDone_ || onProgress_)
            return;

        let $uploadsContainer = $('<div id="vault-uploads-display">');
        $doc.find('#report_list').parent().prepend($uploadsContainer);
        updateUploadsDisplay();
    }

    function updateUploadsDisplay() {
        let stats = requestManager.getStats();
        let statusMessage = `Uploading ${stats.total} reports... (${stats.done} done, ${stats.numFailed} failed.)`;

        if (!onProgress_) {
            setUploadsDisplay(statusMessage);
        } else {
            onProgress_(statusMessage);
        }
    }

    function setUploadsDisplay(contents) {
        if (onDone_ || onProgress_)
            return;

        let $uploadsContainer = $doc.find('#vault-uploads-display');
        $uploadsContainer.text(contents);
    }

    function toggleReport($link, checked_) {
        if (onDone_ || onProgress_)
            return;

        if (typeof checked_ == 'undefined')
            checked_ = true;

        $link.closest('tr').find('td:first-of-type input').prop('checked', checked_);
    }

};