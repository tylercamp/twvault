

//  Making hover-tooltips
UI.ToolTip(selector, {
    bodyHandler: () => {
        let $el = $(this);

        return someHtml;
    }
})

