//  Returns links to all battle reports on the current page
function parseReportsOverviewPage($doc) {

    $doc = $doc || $(document);

    lib.ensurePage(lib.pageTypes.ALL_REPORTS);

    var requestManager = new RequestManager();
    requestManager.refreshDelay = 500;
    
    let hasFilters = checkHasFilters();
    console.log('hasFilters = ', hasFilters);
    let pages = lib.detectMultiPages($doc);
    console.log('pages = ', pages);

    let reportLinks = [];

    let $reportLinks = $doc.find('#report_list tr:not(:first-child):not(:last-child) a:not(.rename-icon)');
    $reportLinks.each((i, el) => {
        let $el = $(el);

        let link = $el.prop('href');
        let reportId = parseInt(link.match(/view=(\w+)/)[1]);
        let $icon = $el.closest('td').find('img:first-of-type');

        var isBattleReport = false;
        $icon.each((_, el) => {
            let icon = $(el).attr('src');
            if (icon.contains("/dots/") || icon.contains("attack"))
                isBattleReport = true;
        });

        if ($el.text().contains("Your support from"))
            isBattleReport = false;

        if (!isBattleReport)
            return;

        reportLinks.push({
            reportId: reportId,
            link: link
        });
    });

    return reportLinks;
};