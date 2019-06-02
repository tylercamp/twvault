function parseAllReports($doc, onProgress_, onDone_) {
    $doc = $doc || $(document);

    //# REQUIRE pages/reports-overview.js
    //# REQUIRE pages/view-report.js
    
    var requestManager = new RequestManager();

    let previousReports = lib.getLocalStorage('reports-history', []);
    let ignoredFolders = lib.getLocalStorage('reports-ignored-groups', []);

    let hasFilters = checkHasFilters();
    console.log('hasFilters = ', hasFilters);

    if (hasFilters) {
        if (onProgress_)
            onProgress_(lib.messages.FILTER_APPLIED(lib.translate(lib.itlcodes.REPORTS)));
        else
            alert(lib.messages.FILTER_APPLIED(lib.translate(lib.itlcodes.REPORTS)));

        if (onDone_) {
            onDone_(lib.errorCodes.FILTER_APPLIED);
        }
        return;
    }

    let $reportFolders = $doc.find('td > a[href*=group_id]:not(.village_switch_link)');
    let usedReportFolderIds = [];
    $reportFolders.each((_, el) => {
        let groupId = $(el).attr('href').match(/group_id=(\d+)/)[1];
        if (!ignoredFolders.contains(groupId))
            usedReportFolderIds.push(groupId);
    });

    console.log('Got non-ignored folder IDs: ', usedReportFolderIds);

    if (!usedReportFolderIds.length) {
        if (onProgress_)
            onProgress_(lib.translate(lib.itlcodes.REPORTS_UPLOAD_ALL_IGNORED));
        else
            alert(lib.translate(lib.itlcodes.REPORTS_UPLOAD_ALL_IGNORED));

        if (onDone_) {
            onDone_();
        }
        return;
    }

    let reportFolderLinks = usedReportFolderIds.map(id => lib.makeTwUrl(`screen=report&group_id=${id}&mode=all`));

    let reportLinks = [];

    onProgress_ && onProgress_(lib.translate(lib.itlcodes.REPORTS_COLLECTING_PAGES));
    let pages = [];

    collectReportFolderPages();
    makeUploadsDisplay();

    function collectReportFolderPages() {
        requestManager.resetStats();
        requestManager.setFinishedHandler(() => {
            requestManager.stop();
            requestManager.resetStats();
            collectReportLinks();
        });

        reportFolderLinks.forEach((url) => {
            requestManager.addRequest(url, (data) => {
                let $folder = lib.parseHtml(data);
                pages.push(...lib.detectMultiPages($folder).map(l => l + '&group_id=' + url.match(/group_id=(\d+)/)[1]));
                reportLinks.push(...parseReportsOverviewPage($folder));
            });
        });

        requestManager.start();
    }

    function collectReportLinks() {
        console.log('pages = ', pages);
        let collectingReportLinksMessage = lib.translate(lib.itlcodes.REPORTS_COLLECTING_LINKS);
        onProgress_ && onProgress_(collectingReportLinksMessage);

        pages.forEach((link) => {
            requestManager.addRequest(link, (data) => {
                onProgress_ && onProgress_(lib.translate(lib.itlcodes.REPORTS_PAGES_PROGRESS, { numDone: requestManager.getStats().done, numTotal: pages.length }));

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
            console.log('Got all report links: ', reportLinks);

            collectFarmingReportLinks((farmReportIds) => {
                console.log('Got farm report IDs: ', farmReportIds);

                let filteredReports = reportLinks.except((l) => previousReports.contains(l.reportId));
                filteredReports = filteredReports.except((l) => farmReportIds.contains(l.reportId));

                onProgress_ && onProgress_(lib.translate(lib.itlcodes.REPORTS_CHECK_UPLOADED));
                getExistingReports(filteredReports.map(r => r.reportId), (existing) => {
                    console.log('Got existing reports: ', existing);

                    previousReports.push(...existing);
                    let withoutMissingReports = previousReports.except((r) => !reportLinks.contains((l) => l.reportId == r));
                    console.log('Updated reports cache without missing reports: ', withoutMissingReports);
                    lib.setLocalStorage('reports-history', withoutMissingReports);

                    let filteredLinks =
                        filteredReports.except((l) => previousReports.contains(l.reportId))
                            .map((l) => l.link)
                            .distinct();

                    console.log('Made filtered links: ', filteredLinks);

                    uploadReports(filteredLinks);
                });
            });
        });

        requestManager.start();
    }

    function collectFarmingReportLinks(onDone) {
        let $groupLinks = $doc.find('td > a[href*=group_id]:not([href*=view]):not(.village_switch_link)');
        let $farmReportGroup = $groupLinks.filter((i, el) => $(el).text().contains(lib.translate(lib.itlcodes.REPORTS_LOOT_ASSISTANT)));

        if (!$farmReportGroup.length) {
            onProgress_ && onProgress_(lib.translate(lib.itlcodes.REPORTS_LA_NOT_FOUND));
            setTimeout(() => onDone([]), 1500);
            return;
        }

        let farmGroupLink = $farmReportGroup.prop('href');
        let farmGroupId = farmGroupLink.match(/group_id=(\w+)/)[1];
        $.get(farmGroupLink)
            .done((data) => {
                const baseFilteringMessage = lib.translate(lib.itlcodes.REPORTS_FILTERING_LA);
                onProgress_ && onProgress_(baseFilteringMessage);

                let $folderDoc = lib.parseHtml(data);
                let $lootFolderPages = [$folderDoc];
                // The .map isn't really necessary, but done so RequestManager doesn't complain of duplicate links
                let lootFolderPageLinks = lib.detectMultiPages($folderDoc).map(l => l + '&group_id=' + farmGroupId);

                requestManager.resetStats();

                lootFolderPageLinks.forEach((link) => {
                    requestManager.addRequest(link, (page) => {
                        $lootFolderPages.push(lib.parseHtml(page));
                        let stats = requestManager.getStats();
                        onProgress_ && onProgress_(`${baseFilteringMessage} (${stats.toString()})`);
                    });
                });

                requestManager.setFinishedHandler(() => {
                    requestManager.stop();
                    requestManager.resetStats();

                    let lootReportLinks = [];
                    $lootFolderPages.forEach(($page) => {
                        let pageLinks = parseReportsOverviewPage($page);
                        console.log('Got loot assistant report links: ', pageLinks);
                        lootReportLinks.push(...pageLinks);
                    });

                    onDone(lootReportLinks.map(l => l.reportId));
                });

                requestManager.start();
            })
            .error(() => {
                onProgress_ && onProgress_(lib.translate(lib.itlcodes.REPORTS_LA_ERROR));
                setTimeout(() => onDone([]), 1500);
            });
    }

    function getExistingReports(reportIds, onDone) {
        lib.postApi('report/check-existing-reports', reportIds)
            .done((data) => {
                if (typeof data == 'string')
                    data = JSON.parse(data);
                if (data.length) {
                    onProgress_ && onProgress_(lib.translate(lib.itlcodes.REPORTS_SKIPPED_OLD, { numOld: data.length }));
                    setTimeout(() => onDone(data), 2000);
                } else {
                    onDone(data);
                }
            })
            .error(() => {
                if (lib.isUnloading())
                    return;

                onProgress_ && onProgress_(lib.translate(lib.itlcodes.REPORTS_ERROR_CHECK_OLD));
                setTimeout(() => onDone([]), 2000);
            });
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

                    let $doc = lib.parseHtml(data);
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

            let statusMessage = lib.translate(lib.itlcodes.REPORTS_FINISHED, { numDone: stats.done, numFailed: stats.numFailed, numTotal: stats.total, _escaped: false });
            if (onProgress_)
                onProgress_(lib.escapeHtml(statusMessage));

            if (!onDone_) {
                alert(statusMessage);
                let stats = requestManager.getStats();
                setUploadsDisplay(lib.escapeHtml(statusMessage));
            } else {
                onDone_(false);
            }
        });

        if (!requestManager.getStats().total) {
            lib.postApi('report/finished-report-uploads');

            let noNewReportsMessage = lib.translate(lib.itlcodes.REPORTS_NONE_NEW, { _escaped: false });

            if (!onDone_) {
                setUploadsDisplay(noNewReportsMessage);
                alert(noNewReportsMessage);
            } else {
                if (onProgress_)
                    onProgress_(lib.escapeHtml(noNewReportsMessage));
                if (onDone_)
                    onDone_(false);
            }
        } else {
            requestManager.start();
        }
    }

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
        let statusMessage = lib.translate(lib.itlcodes.REPORTS_PROGRESS, { numDone: stats.done, numTotal: stats.total, numFailed: stats.numFailed });

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

        let $checkedRadios = $filters.find('input[type=radio]:not([value=0]):not([value=OR]):not([value=AND]):checked');
        if ($checkedRadios.length) {
            console.log('Checked radios: ', $checkedRadios);
            hasFilters = true;
        }

        return hasFilters;
    }
}