//  Returns links to all battle reports on the current page
function parseReportsOverviewPage($doc) {

    $doc = $doc || $(document);

    lib.ensurePage(lib.pageTypes.ALL_REPORTS);

    var requestManager = new RequestManager();
    
    let hasFilters = checkHasFilters();
    console.log('hasFilters = ', hasFilters);
    let pages = lib.detectMultiPages($doc);
    console.log('pages = ', pages);

    let reportLinks = [];
    let ignoredReports = [];
    let serverTime = lib.getServerDateTime();
    const maxReportAgeDays = 14;
    const maxReportAgeMs = maxReportAgeDays * 24 * 60 * 60 * 1000;

    let $reportLinks = $doc.find('#report_list tr:not(:first-child):not(:last-child) a:not(.rename-icon)');
    $reportLinks.each((i, el) => {
        let $el = $(el);

        let link = $el.prop('href');
        let landingTimeText = $el.closest('tr').find('td:last-of-type').text();
        let landingTime = lib.parseTimeString(landingTimeText);
        let reportId = parseInt(link.match(/view=(\w+)/)[1]);
        let $icon = $el.closest('td').find('img:first-of-type');

        let timeSinceReport = serverTime.valueOf() - landingTime.valueOf();
        if (timeSinceReport >= maxReportAgeMs) {
            let ageDays = timeSinceReport / 24 / 60 / 60 / 1000;
            ignoredReports.push({ reportId: reportId, ageDays: Math.roundTo(ageDays, 2) });
            //console.log(`Report ${reportId} is ${Math.roundTo(ageDays, 2)} days old, skipping`);
            return;
        }

        var isBattleReport = false;
        $icon.each((_, el) => {
            let icon = $(el).attr('src');
            if (icon.contains("/dots/") || icon.contains("attack"))
                isBattleReport = true;
        });

        if ($el.text().toLowerCase().contains(lib.translate(lib.itlcodes.YOUR_SUPPORT_FROM).toLowerCase()))
            isBattleReport = false;

        if (!isBattleReport)
            return;

        reportLinks.push({
            reportId: reportId,
            link: link
        });
    });

    console.log('Ignored ' + ignoredReports.length + ' reports for being over ' + maxReportAgeDays + ' days old');

    return reportLinks;
};