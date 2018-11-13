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
                    tabInfo.tab.init && tabInfo.tab.init.call(tabInfo.tab, $ui.find('#' + tabInfo.tabId));
                });
            });
        },

        mkBtn: function makeButtonHtml(id, label, css_) {
            return `<button class="btn btn-confirm-yes" ${id ? `id="${id}"` : ''} ${css_ ? `style="${css_}"` : ''}>${label}</button>`;
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
                    ${containerInfo.map((i) => `<div style="display:${i.tabId == defaultTab ? 'block' : 'none'}" id="${i.tabId}">${i.tab.getContent.call(i.tab)}</div>`).join('\n')}
                </div>
            `.trim();
        },

        // onChange(newVal) => null
        // transformer_(newVal, oldVal) => transformedVal
        syncProp: function ($input, targetObject, targetProp, onChange, transformer_) {
            if (!$input) {
                return;
            }

            $input = $($input);

            if (!$input[0]) {
                return;
            }

            const tag = $input[0].tagName.toLowerCase(); 
            switch (tag) {
                case 'input':
                    switch ($input.prop('type')) {
                        case 'number':
                        case 'text':
                            if (typeof targetObject[targetProp] != 'undefined')
                                $input.val(targetObject[targetProp]);
                            else
                                $input.val('');

                            $input.change(() => {
                                let val = $input.val();
                                if (transformer_) {
                                    val = transformer_(val, targetObject[targetProp]);
                                }
                                targetObject[targetProp] = val;
                                onChange && onChange(val);
                            })
                            break;

                        case 'checkbox':
                            $input.prop('checked', !!targetObject[targetProp]);
                            $input.change(() => {
                                let val = $input.prop('checked');
                                if (transformer_) {
                                    val = transformer_(val, targetObject[targetProp]);
                                }
                                targetObject[targetProp] = val;
                                onChange && onChange(val);
                            });
                            break;
                    }
                    break;

                case 'textarea':
                    $input.val(targetObject[targetProp] || '');
                    $input.change(() => {
                        let val = $input.val();
                        if (transformer_) {
                            val = transformer_(val, targetObject[targetProp]);
                        }
                        targetObject[targetProp] = val;
                        onChange && onChange(val);
                    });
                    break;

                case 'select':
                    if (targetObject[targetProp])
                        $input.val(targetObject[targetProp]);

                    $input.change(() => {
                        let val = $input.val();
                        if (transformer_) {
                            val = transformer_(val, targetObject[targetProp]);
                        }
                        targetObject[targetProp] = val;
                        onChange && onChange(val);
                    });
                    break;

                default:
                    console.warn('Unhandled tag: ', tag);
            }
        }
    };

    return uilib;
})();