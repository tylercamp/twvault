function parseOwnCommandsOverviewPage($doc) {
    $doc = $doc || $(document);

    let $commandLinks = $doc.find('#commands_table tr:not(:first-of-type):not(:last-of-type) td:first-of-type a:not(.rename-icon)');

    let commandLinks = [];

    $commandLinks.each((i, el) => {
        let label = $(el).text();
        let link = $(el).prop('href');
        let $td = $(el).closest('td');
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

    return commandLinks;
}