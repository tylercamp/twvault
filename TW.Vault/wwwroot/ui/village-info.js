function enhanceVillageInfoPage($doc) {
    $doc = $doc || $(document);

    $doc.find('#vault-commands').remove();
    let $container = $(`
        <div id="vault-commands">
            <table class="vis">
                <tr>
                    <th width="52%">Vault commands</th>
                    <th width="33%">Arrival time</th>
                    <th width="15%">Arrives in</th>
                </tr>
            </table>
        </div>
    `.trim());

    $('#content_value > table > tbody > tr > td:nth-of-type(2) > .vis').insertBefore($container);

    /*
     * 
     * window.Timing.tickHandlers.timers._timers.push({
     *  element: $(targetElement),
     *  end: endServerTimeSeconds
     * })
     * 
     */

    let currentVillageId = 0;
    lib.getApi(`village/${currentVillageId}/commands`)
        .done((data) => {
            if (typeof data == 'string')
                data = lib.jsonParse(data);

            makeCommandsUI(data);
        })
        .error(() => {
            alert('An error occurred...');
        });


    function makeCommandsUI(data) {

    }
}