
# General

Scripts can be found in /TW.Vault/wwwroot. The folder layout is:

- `lib` - generic library scripts
- `pages` - scripts that parse specific pages (indicated by the name of the script)
- `ui` - scripts that create or modify UI for the user

The script that users request is `main.js`, which detects the current page and invokes the appropriate sub-script for that page. The script displaying
the main UI is at `ui/all.js`.

When a user requests `main.js`, that request goes to the `ScriptController` which then combines all the scripts and returns the result to the user.

Combining scripts is done by adding a line to your JS:

    //# REQUIRE my-file.js

The `ScriptController` will search the requested script (and all referenced scripts) for comments following that format, and will replace that comment
with the contents of the given file. If the file had been included previously, the comment will be ignored. File paths are relative to the `wwwroot` folder.

`REQUIRE` statements specific to a script (ie `all-reports.js` requires `view-report.js`) should have the `REQUIRE` in only the file that needs it. Global
required scripts should be placed at the top of `main.js`.

# Conventions

Optional parameters end in an underscore ie `myOptionalParam_`.

Use `console.log`, `console.warn`, and `console.error` so we can easily debug issues.

Most scripts follow the format:

    function myScript($doc) {
        $doc = $doc || $(document);

        // ...
    }

This allows a given script to run on the current page or on a page given to it. All searching and modification of page elements should be done through
`$doc`, and NOT `$('my-selector')`.

Scripts should not expose internal variables or functions.

Parsing scripts that involve requests or take a long time typically take the format:

    function myScript($doc, onProgress_, onDone_) {
        $doc = $doc || $(document);

        onProgress_ && onProgress_("Starting to do some work...");
        // Do work

        onProgress_ && onProgress_("Finished doing work.");
        onDone_ && onDone_(false);
    }

The syntax `onProgress_ && onProgress_('...')` only runs `onProgress_` if `onProgress_` is defined. This check allows the script to run with or without
callbacks for indicating progress and makes them more robust.

# Common helpers - lib.js

The file `lib.js` is included by default in `main.js`. Including it provides the `lib` object which contains various helper functions.

### Requesting and checking for specific TW pages

`lib.pageTypes` contains variables used to refer to different page types, ie `lib.pageTypes.VIEW_REPORT`, `.ALL_REPORTS`, `.MAP`, etc.

- Get the current page via `lib.getCurrentPage()`
- Run different code on specific pages using `lib.onPage()` (see `main.js`, typically not needed elsewhere)
- Pass a page type to `lib.makeTwUrl` to get a URL to the given page for the current world

At the end of `lib.js` are `pageValidators` and `pageUrls`. `pageValidators` is used to check the type of the current page. `pageUrls` are the URLs
returned when a page type is passed to `lib.makeTwUrl`. All `pageUrls` will link to the page and show all pages across all groups. (sets `group=0` and `page=-1` in the returned URL.)

    lib.makeTwUrl(lib.pageTypes.ALL_REPORTS)

Any requests for a TW page should use `lib.makeTwUrl`. It returns the provided URL relative to `/game.php` and attaches the query param for the sitter (`t=...`) if necessary.

    lib.makeTwUrl('screen=place')

Passing a page type is preferred to manually making a URL unless necessary. If a page is needed, isn't contained in `pageTypes`, and you don't have any specific requirements
(ie need to specifically get the 3rd page), then add that page to `pageTypes`, add entries to `pageValidators` and `pageUrls`, and then use that new page type instead.

### Making requests to the Vault

All Vault requests require the user's API key, user ID, and tribe ID. `lib.getApi` and `lib.postApi` can be used to make requests to the server and attach this data automatically.

These return JS `Promise` objects directly from jQuery, and are used as:

    lib.getApi('https://abc.net/whatever')
        .done((data) => {
            // ...
        })
        .error(() => {

        });

Requests to the vault should use `lib.makeApiUrl`. The value returned is relative to the script source and takes the current server into account.

    lib.makeApiUrl('village/currentArmy')
    //  Will return ie https://v.tylercamp.me/api/en100/village/currentArmy

    // If script is ran from QA server, it will return:
    //  https://v.tylercamp.me/qa/api/en100/...

    // If I (tyler) run the script and my script is hosted at `/dev/script/main.js`, it will return:
    //  https://v.tylercamp.me/dev/api/en100/...

No changes to URL requests are necessary when running the script publically or from a dev version.

### Captcha detection

Use `lib.checkContainsCaptcha(htmlString)` to determine if TW has activated bot detection in the given page. This requires an HTML string and does not accept
jQuery objects.

### Common error codes and messages

Errors are generally stored in `lib.messages` and reused for consistency. Any script that exits with a common error should use one of `lib.errorCodes` as its response
(ie `lib.errorCodes.CAPTCHA`) and checked for in the parent script (if necessary.) ie, the main interface `ui/all.js` references `pages/all-reports.js` to upload reports.
`parseAllReports` takes an `onDone_` parameter that is invoked when it's finished. If captcha is detected within `parseAllReports`, it calls `onDone_(lib.errorCodes.CAPTCHA)`
to exit early. `all.js` checks the value passed by `parseAllReports` and alerts the user if the error is `lib.errorCodes.CAPTCHA`.

### Collecting multi-pages

The Incomings page may have "multi-pages" if there are over 1000 incomings. In that case, requesting the Incomings page on its own won't provide the full list of incomings.

Use `lib.detectMultiPages($doc)` to get a list of links to all multi-pages on the current page. This includes the `[all]` page, and will not include a link to the current page.
Typically the current page is manually added to the result of `lib.detectMultiPages`, and the result of parsing all the pages is filtered to ignore duplicates.

    //  From all-reports.js
    let pages = lib.detectMultiPages($doc);
    pages.push(lib.makeTwUrl(lib.pageTypes.ALL_REPORTS));

    let reportLinks = [];
    pages.forEach((link) => {
        // Parse the reports page
    });

    //  After page parsing is done...
    reportLinks = reportLinks.distinct(); // Removes duplicate links
    //  Request each report page link

### Using localStorage and cookies

Use `lib.setLocalStorage`, `lib.getLocalStorage`, `lib.getCookie`, `lib.setCookie`, and `lib.clearCookie`. These modify the given cookie/local storage names for consistency
and provides some extra logic for handling Dates.

Check localStorage usage via `lib.getLocalStorageSize()`. Note that the typical limit is ~5MB.

### Creating and parsing JSON

Use `lib.jsonParse` and `lib.jsonStringify`, which have some extra logic for working with Dates

### Misc.

The `lib` object can be accessed globally from the JS Console via `_vlib`.

Other functions:

- `lib.parseTimeString` - Parses a variety of text formats into `Date` equivalents
- `lib.formatDateTime` - Formats a `Date` to a common, readable string format: `HH:MM:SS on DD/MM/YYYY`

Additions to JS API (these are global):

- `Math.roundTo` - Rounds a number to a given precision, ie `Math.roundTo(1.9876, 2) = 1.98`
- `String.contains` - Whether or not a string contains the given text, ie `"abc123".contains("a") = true`
- `String.trim` - Trims multiple characters from the start and end of a string, ie `"abc123".trim('a', 'b', '3') = "c12"`
- `Array.distinct` - Returns an array with distinct values (based on the direct values or by using a comparer)
- `Array.contains` - Whether or not an array contains the given value (based on direct value or by using a callback)
- `Array.except` - Returns an array without the given array or elements determined via a callback

# Making many requests

Use the `RequestManager` object within `requestManager.js`. It has rate-limiting built in to prevent overloading TW or Vault. The simplest example
is `pages/all-incomings.js`.

It has a default rate-limit of 5 requests per second (the official maximum according to TW support.)

    var requestManager = new RequestManager();
    requestManager.addRequest(lib.makeTwUrl('whatever'), (data) => {
        //  Loaded data, do whatever
    });

    requestManager.setFinishedHandler(() => {
        //  This is ran once all of the requests have finished
    });

    requestManager.start();

Info on total, pending, completed, and errored requests can be gotten via `requestManager.getStats()`.