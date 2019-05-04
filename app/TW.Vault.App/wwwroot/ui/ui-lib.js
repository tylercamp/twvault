var uilib = (() => {
    var wasInitialized = false;
    var $initializedUi = null;
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
            wasInitialized = true;
            $initializedUi = $ui;

            containers.forEach((c) => {
                let tabIds = c.map((t) => t.tabId);

                c.forEach((tabInfo) => {
                    if (tabInfo.tab.initialized)
                        return;

                    lib.bindMembers(tabInfo.tab);

                    let $container = $('#' + tabInfo.tabId);
                    let $tabButton = $ui.find('#' + tabInfo.btnId);
                    tabInfo.tab.$container = $container;
                    tabInfo.tab.$tabButton = $tabButton;
                    $tabButton.click(() => selectTab($ui, tabInfo.tabId, tabIds));

                    if (tabInfo.tab.getStyle) {
                        let tabStyle = tabInfo.tab.getStyle;
                        if (typeof tabStyle == 'function')
                            tabStyle = tabStyle();

                        uilib.applyStyles($container, tabStyle);
                    }

                    tabInfo.tab.btnId = tabInfo.btnId;
                    tabInfo.tab.init && tabInfo.tab.init.call(tabInfo.tab, $container);
                    tabInfo.tab.initialized = true;
                });
            });
        },

        mkBtn: function makeButtonHtml(id, label, css_) {
            return `<button class="btn btn-confirm-yes" ${id ? `id="${id}"` : ''} ${css_ ? `style="${css_}"` : ''}>${label}</button>`;
        },

        mkTabbedContainer: function makeTabbedContainerHtml(defaultTab, tabs) {
            if (typeof defaultTab == 'object')
                defaultTab = defaultTab.containerId;

            let containerInfo = tabs.map((t) => {
                return {
                    btnId: mkContainerBtnId(t.containerId),
                    tabId: t.containerId,
                    tab: t
                };
            });

            containers.push(containerInfo);

            var resultHtml = `
                ${containerInfo.map((i) => uilib.mkBtn(i.btnId, i.tab.label, i.tabId == defaultTab ? 'font-weight:bold;' : 'font-weight:normal;' + (i.tab.btnCss ? i.tab.btnCss : ''))).join("\n")}

                <div class="vault-tab-container" style="padding:1em">
                    ${containerInfo.map((i) => {
                        let style = `display:${i.tabId == defaultTab ? 'block' : 'none'}`;
                        let content = typeof i.tab.getContent == 'string' ? i.tab.getContent : i.tab.getContent.call(i.tab);
                        return `<div style="${style}" id="${i.tabId}">${content}</div>`;
                    }).join('\n')}
                </div>
            `.trim();

            if (wasInitialized) {
                setTimeout(() => uilib.init($initializedUi), 500);
            }

            return resultHtml;
        },

        applyStyles: function ($container, styles) {
            lib.objForEach(styles, (selector, css) => {
                let $selection = $container.find(selector);
                if (!$selection.length) {
                    console.warn('Selector "' + selector + '" for tab "' + tabInfo.tab.name + '" did not select any elements');
                }
                if (typeof css == 'function') {
                    uilib.applyStyles($selection, css());
                } else {
                    $selection.css(css);
                }
            });
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
                            if (typeof targetObject[targetProp] != 'undefined' && targetObject[targetProp] != null)
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
        },

        propTransformers: {
            float: function (newVal, oldVal) {
                if (typeof newVal == 'number')
                    return newVal;

                if (newVal.match(/[^\d\.]/))
                    return oldVal;
                else
                    return parseFloat(newVal);
            },

            int: function (newVal, oldVal) {
                if (typeof newVal == 'number')
                    return Math.round(newVal);

                if (newVal.match(/[^\d]/))
                    return oldVal;
                else
                    return parseInt(newVal);
            },

            floatOptional: function (newVal, oldVal) {
                if (!newVal) {
                    return null;
                } else {
                    return uilib.propTransformers.float(newVal, oldVal);
                }
            },

            intOptional: function (newVal, oldVal) {
                if (!newVal) {
                    return null;
                } else {
                    return uilib.propTransformers.int(newVal, oldVal);
                }
            }
        },

        mkSpinner: ($container) => {
            let spinner = {
                $el: null,
                done: () => {
                    $containerContent.children().appendTo($container);
                    $containerContent.remove();
                    $el.remove();
                }
            };

            let $containerContent = $('<div style="display:none">');
            $container.children().appendTo($containerContent);
            $container.append($containerContent);

            spinner.$el = $('<div>').css({

            });

            $container.append(spinner.$el);

            return spinner;
        }
    };

    return uilib;
})();