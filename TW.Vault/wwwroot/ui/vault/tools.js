
function makeToolsTab() {
    let fakeTab = makeFakeScriptTab();

    let tabs = [
        fakeTab
    ];

    let toolsTab = {
        label: 'Tools',
        containerId: 'vault-tools-container',

        getContent: function () {
            return uilib.mkTabbedContainer(fakeTab.containerId, tabs);
        }
    };

    return toolsTab;
}



function makeFakeScriptTab() {
    return {
        label: 'Fake Script',
        containerId: 'vault-fake-script-tools',
        init: function ($container) {
            $container.find('td').css({
                'text-align': 'left'
            });

            $container.find('tr td:nth-of-type(1)').css({
                width: '6em'
            });

            $container.find('input[type=text]').css({
                width: '100%'
            })

            $container.find('#fake-make-script').click(() => {
                let players = $container.find('#fake-target-player').val().trim();
                let tribes = $container.find('#fake-target-tribe').val().trim();
                let continents = $container.find('#fake-target-continents').val().trim();

                let link = lib.makeVaultUrl(`script/fake.js?server=${window.location.hostname}`);

                if (players.length) {
                    link += `&player=${encodeURIComponent(players)}`;
                }
                if (tribes.length) {
                    link += `&tribe=${encodeURIComponent(tribes)}`;
                }
                if (continents.length) {
                    link += `&k=${encodeURIComponent(continents)}`;
                }

                let script = `
javascript:
window.vaultFakes = {
    troopCounts: [
        { catapult: 1, spy: 1 },
        { ram: 1, spy: 1 },
        { catapult: 1 },
        { ram: 1 }
    ]
};

$.getScript("${link}");
                `.trim(' ', '\n', '\r');

                $container.find('#fake-script-output').val(script);
            });

            $container.find('#fake-get-coords').click(() => {
                let players = $container.find('#fake-target-player').val().trim();
                let tribes = $container.find('#fake-target-tribe').val().trim();
                let continents = $container.find('#fake-target-continents').val().trim();

                let link = 'village/coords';
                link += `?player=${encodeURIComponent(players)}`;
                link += `&tribe=${encodeURIComponent(tribes)}`;
                link += `&k=${encodeURIComponent(continents)}`;

                lib.getApi(link)
                    .done((data) => {
                        let coords = data.coords;

                        $container.find('#fake-script-output').val(coords);
                    })
                    .error(() => {
                        alert('An error occurred...');
                    });
            });
        },

        getContent: function () {
            return `
                <h3>Dynamic Fake Scripts</h3>
                <table style="width:100%">
                    <tr>
                        <td>
                            <label for="fake-target-player">Players</label>
                        </td>
                        <td>
                            <input id="fake-target-player" type="text" placeholder="False Duke, Nefarious, etc.">
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <label for="fake-target-tribe">Tribes</label>
                        </td>
                        <td>
                            <input id="fake-target-tribe" type="text" placeholder="Hundred Hungry Hippos, 100, ODZ, etc.">
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <label for="fake-target-continents">Continents</label>
                        </td>
                        <td>
                            <input id="fake-target-continents" type="text" placeholder="44,45,etc.">
                        </td>
                    </tr>
                </table>

                <div style="text-align:center;margin: 1em 0;">
                    <button id="fake-make-script">Get Script</button>
                    <button id="fake-get-coords">Get Coords</button>
                </div>

                <div>
                    <textarea id="fake-script-output" style="width:100%;height:5em" disabled></textarea>
                </div>
            `;
        }
    }
}

