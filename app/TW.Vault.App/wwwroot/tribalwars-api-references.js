

//  Making hover-tooltips
UI.ToolTip(selector, {
    bodyHandler: () => {
        let $el = $(this);

        return someHtml;
    }
})

//  Adding timers that sync with TW server time
window.Timing.tickHandlers.timers._timers.push({
  element: $(targetElement),
  end: endServerTimeSeconds
})