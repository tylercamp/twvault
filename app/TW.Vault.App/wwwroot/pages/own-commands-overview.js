function parseOwnCommandsOverviewPage($doc) {
    $doc = $doc || $(document);

    let $commandLinks = $doc.find('#commands_table tr:not(:first-of-type):not(:last-of-type) td:first-of-type a:not(.rename-icon)');

    let commandLinks = [];
    var numIgnoredCommands = 0;

    $commandLinks.each((i, el) => {
        let $el = $(el);
        let label = $el.text();
        let link = $el.prop('href');
        let $td = $el.closest('td');
        var isFarmRun = !!$td.find('img[src*=farm]').length;
        if (isFarmRun) {
            numIgnoredCommands++;
            return;
        }

        var commandState = $td.find('img:first-of-type').prop('src').match(/(\w+)\.png/)[1];

        let commandType = commandState.contains("attack") ? "attack" : "support";
        let isReturning = commandState.contains("return") || commandState.contains("back");

        let commandId = parseInt(link.match(/id\=(\w+)/)[1]);

        commandLinks.push({
            link: link,
            commandId: commandId,
            userLabel: label,
            commandType: commandType,
            isReturning: isReturning
        });
    });

    if (numIgnoredCommands) {
        console.log('Ignored ' + numIgnoredCommands + ' farming attacks');
    }

    return commandLinks;
}