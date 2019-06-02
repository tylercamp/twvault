

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

// Showing TW popup dialog
// e: window type
// t: content wrapper
// o: on-close handler
// i: descriptor object:
//    - class_name (str)
//    - close_from_fader (bool)
//    - allow_close (bool)
//    - priority (enum `Dialog.PRIORITY_NONE` or `Dialog.PRIORITY_IMPORTANT`)
Dialog.show('blah', `
    <div style="width:500px">
        <p>Test</p>
    </div>
`.trim(), function () { console.log('closed') }, {});


Dialog.show('vault-old', `
    <div style="width:800px">
        <p style="padding: 1em">
            You're using an older, discontinued version of the Vault! Use this script instead to continue:
        </p>
        <textarea style="width:80%">javascript:window.vaultToken='${window.vaultToken}';$.getScript('https://v.tylercamp.me/script/main.js')</textarea>
    </div>
`);