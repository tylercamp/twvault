function parseAllReports($doc, onProgress_, onDone_) {
    $doc = $doc || $(document);

    //# REQUIRE lib.js
    //# REQUIRE requestManager.js
    //# REQUIRE pages/reports-overview.js
    
    var requestManager = new RequestManager();
    requestManager.refreshDelay = 500;

    let previousReports = JSON.parse(localStorage.getItem('vault-reports-history') || '[]');

    let hasFilters = checkHasFilters();
    console.log('hasFilters = ', hasFilters);

    if (hasFilters) {
        let removeFiltersMsg = 'You have filters set for your reports, please remove them before uploading.';
        if (onProgress_)
            onProgress_(removeFiltersMsg);
        else
            alert(removeFiltersMsg);

        if (onDone_) {
            onDone_(true);
        }
        return;
    }

    let reportLinks = [];

    onProgress_ && onProgress_('Collecting report pages...');
    let pages = lib.detectMultiPages($doc);
    console.log('pages = ', pages);

    collectReportLinks();


    function collectReportLinks() {
        let collectingReportLinksMessage = 'Collecting report links...';
        onProgress_ && onProgress_(collectingReportLinksMessage);

        pages.forEach((link) => {
            requestManager.addRequest(link, (data) => {
                onProgress_ && onProgress_(`${collectingReportLinksMessage} (page ${requestManager.getStats().done}/${pages.length})`);

                if (lib.checkContainsCaptcha(data)) {
                    if (requestManager.isRunning()) {
                        requestManager.stop();

                        if (onProgress_)
                            onProgress_(lib.messages.TRIGGERED_CAPTCHA);
                        else
                            alert(lib.messages.TRIGGERED_CAPTCHA);

                        onDone_ && onDone_(lib.errorCodes.CAPTCHA);
                    }

                    return;
                }

                let $pageDoc = $(data);
                let pageLinks = parseReportsOverviewPage($pageDoc);
                console.log('Got page links: ', pageLinks);
                reportLinks.push(...pageLinks);
            });
        });

        requestManager.setFinishedHandler(() => {
            requestManager.stop();
            console.log('Got all page links: ', reportLinks);
            let filteredLinks =
                reportLinks.except((l) => previousReports.contains(l.reportId))
                           .map((l) => l.link)
                           .distinct();

            console.log('Made filtered links: ', filteredLinks);

            uploadReports(filteredLinks);
        });

        requestManager.start();
    }

    function uploadReports(reportLinks) {
        requestManager.resetStats();

        reportLinks.forEach((link) => {
            requestManager.addRequest(link, (data, request) => {
                if (data) {
                    if (lib.checkContainsCaptcha(data)) {

                        if (requestManager.isRunning()) {
                            requestManager.stop();
                            
                            if (onProgress_)
                                onProgress_(lib.messages.TRIGGERED_CAPTCHA);

                            if (onDone_)
                                onDone_(lib.errorCodes.CAPTCHA);
                            else
                                alert(lib.messages.TRIGGERED_CAPTCHA);
                        }

                        return;
                    }

                    let $doc = $(data);
                    try {
                        parseReportPage($doc, link, false, () => {
                            //  onError
                            requestManager.getStats().numFailed++;
                            //toggleReport($el, false);
                        });
                    } catch (e) {
                        requestManager.getStats().numFailed++;
                        console.log(e);
                    }
                    //toggleReport($el);
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

        if (!requestManager.getStats().total) {
            if (!onDone_) {
                setUploadsDisplay('No new reports to upload.');
                alert('No new reports to upload!');
            } else {
                if (onProgress_)
                    onProgress_('Finished: No new reports to upload.');
                if (onDone_)
                    onDone_(false);
            }
        } else {
            requestManager.start();
        }
    }

    makeUploadsDisplay();

    function makeUploadsDisplay() {
        if (onDone_ || onProgress_)
            return;

        $('#vault-uploads-display').remove();

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

    function checkHasFilters() {
        let $filters = $doc.find('.report_filter');
        var hasFilters = false;

        let textFilter = $filters.find('input[type=text]').val();
        if (textFilter != null && textFilter.length > 0) {
            console.log('Text filter not empty');
            hasFilters = true;
        }

        let $checkedBoxes = $filters.find('input[type=checkbox]:checked');
        if ($checkedBoxes.length) {
            console.log('Checked boxes: ', $checkedBoxes);
            hasFilters = true;
        }

        let $checkedRadios = $filters.find('input[type=radio]:not([value=0]):checked');
        if ($checkedRadios.length) {
            console.log('Checked radios: ', $checkedRadios);
            hasFilters = true;
        }

        return hasFilters;
    }
}