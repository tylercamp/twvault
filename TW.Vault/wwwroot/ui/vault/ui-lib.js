var uilib = (() => {
    let containers = [];

    let mkContainerBtnId = (containerId) => `${containerId}-toggle-btn`;

    function selectTab($ui, selected, options) {
        options.forEach((id) => {
            let $el = $ui.find(`#${id}`);
            let $btn = $ui.find(`#${mkContainerBtnId(id)}`);
            if (id == selected) {
                $el.css('display', 'block');
                $btn.css('font-weight', 'bold');
            } else {
                $el.css('display', 'none');
                $btn.css('font-weight', 'normal');
            }
        });
    }

    let uilib = {
        init: function standardizeUI($ui) {
            containers.forEach((c) => {
                let tabIds = c.map((t) => t.tabId);

                c.forEach((tabInfo) => {
                    $ui.find('#' + tabInfo.btnId).click(() => selectTab($ui, tabInfo.tabId, tabIds));
                    tabInfo.tab.btnId = tabInfo.btnId;
                    tabInfo.tab.init && tabInfo.tab.init($ui.find('#' + tabInfo.tabId));
                });
            });
        },

        mkBtn: function makeButtonHtml(id, label, css_) {
            return `<button class="btn btn-confirm-yes" id="${id}" ${css_ ? `style="${css_}"` : ''}>${label}</button>`;
        },

        mkTabbedContainer: function makeTabbedContainerHtml(defaultTab, tabs) {
            let containerInfo = tabs.map((t) => {
                return {
                    btnId: mkContainerBtnId(t.containerId),
                    tabId: t.containerId,
                    tab: t
                };
            });

            containers.push(containerInfo);

            return `
                ${containerInfo.map((i) => uilib.mkBtn(i.btnId, i.tab.label, i.tabId == defaultTab ? 'font-weight:bold;' : 'font-weight:normal;' + (i.tab.btnCss ? i.tab.btnCss : ''))).join("\n")}

                <div style="padding:1em">
                    ${containerInfo.map((i) => `<div style="display:${i.tabId == defaultTab ? 'block' : 'none'}" id="${i.tabId}">${i.tab.getContent()}</div>`).join('\n')}
                </div>
            `.trim();
        }
    };

    return uilib;
})();