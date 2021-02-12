
var lib = (() => {

    //# REQUIRE lib/twstats.js
    //# REQUIRE lib/twcalc.js
    //# REQUIRE lib/versioning.js
    //# REQUIRE lib/lz-string.js
    //# REQUIRE lib/encryption.js
    //# REQUIRE lib/translationCodes.js

    let twstats = getTwTroopStats();
    let localStoragePrefix = 'vls-';
    let cookiePrefix = 'vc-';
    let serverSettings = null;

    var authToken = window.vaultToken || null;
    var authUserId = null;
    var authTribeId = null;
    var userName = null;
    var wasPageHandled = false;
    var utcTimeOffset = null;
    var isUnloading = false;
    var handledDisabledScript = false;

    window.addEventListener('unload', () => isUnloading = true);

    let encryption = makeEncryption();

    let makeAuthHeader = (playerId, tribeId, authKey, isSitter) => {
        let authString = `${playerId}:${tribeId}:${authKey}:${!!isSitter}`;
        return encryption.encryptString(authString, lib.getCurrentUtcTimestamp());
    };

    let currentTranslation = null;
    let translationParameters = null;

    const nativeTranslations = {
        // "on %1 at %2" -> "on {date} at {time}"
        Time_OnAt: window.lang["0cb274c906d622fa8ce524bcfbb7552d"].replace('%1', '{date}').replace('%2', '{time}'),
        // "tomorrow at %s" -> "tomorrow at {time}"
        Time_TomorrowAt: window.lang["57d28d1b211fddbb7a499ead5bf23079"].replace('%s', '{time}'),
        // "today at %s" -> "today at {time}"
        Time_TodayAt: window.lang["aea2b0aa9ae1534226518faaefffdaad"].replace('%s', '{time}'),
        // "%1 at %2" -> "{date} at {time}"
        Time_At: window.lang["850731037a4693bf4338a0e8b06bd2e4"].replace('%1', '{date}').replace('%2', '{time}'),

        Time_MonthShorthands: window.Format.month_names.map(n => n.toLowerCase())
    }

    let escapeHtml = (() => {
        // https://stackoverflow.com/a/12034334/2692629
        var entityMap = {
            '&': '&amp;',
            '<': '&lt;',
            '>': '&gt;',
            '"': '&quot;',
            "'": '&#39;',
            '`': '&#x60;',
            '=': '&#x3D;',
            '\\': '\\\\'
        };

        return function escapeHtml(string) {
            return String(string).replace(/[&<>"'`=\\]/g, function (s) {
                return entityMap[s];
            });
        }
    })();

    function handleDisabledScript(xhr) {
        var info = null;
        if (xhr.responseText && (info = JSON.parse(xhr.responseText)) && info.enabled === false) {
            if (!handledDisabledScript) {
                alert(`You're using a script that was disabled by '${info.disabledBy}' at ${lib.formatDateTime(info)}`);
                handledDisabledScript = true;
            }

            throw "Using a disabled script";
        }
    }

    let lib = {

        //  The set of known page types recognized by this script
        //  Add methods to detect the current page by modifying "pageValidators"
        //  at the bottom of this script
        pageTypes: {
            UNKNOWN: null,
            VIEW_REPORT: null,
            ALL_REPORTS: null,
            INCOMINGS_OVERVIEW: null,
            OWN_COMMANDS_OVERVIEW: null,
            OWN_TROOPS_OVERVIEW: null,
            OWN_TROOPS_SUPPORTING_OVERVIEW: null,
            BUILDINGS_OVERVIEW: null,
            VILLAGE_INFO: null,
            MAP: null
        },

        messages: {
            // Values loaded in init() with translations
            TRIGGERED_CAPTCHA: null,
            IS_IN_GROUP: null,
            FILTER_APPLIED: (_) => null,
            GENERIC_ERROR: null
        },

        errorCodes: {
            CAPTCHA: 'captcha',
            NOT_ALL_GROUP: 'group',
            FILTER_APPLIED: 'filter'
        },

        itlcodes: translationCodes,

        escapeHtml: escapeHtml,

        twstats: twstats,
        twcalc: makeTwCalc(twstats),
        versioning: makeVersioningSetup(),
        lzstr: makeLZStringApi(),

        //  Gets the current server date and time from the page
        getServerDateTime: function getServerDateTime($doc_) {
            $doc_ = $doc_ || $(document);
            var $serverDate = $('#serverDate');
            var $serverTime = $('#serverTime');

            let fullString = `${$serverTime.text().trim()} ${$serverDate.text().trim()}`;
            return lib.parseTimeString(fullString);
        },

        getCurrentUtcTimestamp: () => Date.now() + utcTimeOffset,

        getTwUtcOffset: function getTribalWarsServerUtcOffset($doc_) {
            let serverTime = lib.getServerDateTime($doc_).valueOf();
            let utcNow = lib.getCurrentUtcTimestamp();

            let fullOffset = utcNow - serverTime;
            let fiveMinutes = 5 * 60 * 1000; // Round to the nearest 5 minutes to correct for small errors
            fullOffset = Math.round(fullOffset / fiveMinutes) * fiveMinutes;
            return fullOffset;
        },

        //  Parses a variety of TW date/time formats to JS Date or into splits it into its parts
        //  returns Date if separated_ false or undefined
        //  returns { date: [day, month, year], time: [hour, minute, second, millisecond] } if separated_ is true
        parseTimeString: function parseTimeString(timeString, separated_, $doc_) {
            if (typeof separated_ == 'undefined')
                separated_ = false;

            $doc_ = $doc_ || $(document);

            var monthStrings = lib
                .translate(lib.itlcodes.ORDERED_MONTHS)
                .splitMany(',', ' ', '\n')
                .filter(t => t.length > 0)
                .map(m => m.trim().toLowerCase())
                ;
            //var monthStrings = nativeTranslations.Time_MonthShorthands;

            var serverDate = new Date(Timing.getCurrentServerTime());
            serverDate = [
                serverDate.getUTCDate(),
                serverDate.getUTCMonth() + 1,
                serverDate.getUTCFullYear()
            ];

            let paramPatterns = {
                hour: '\\d+',
                minute: '\\d+',
                second: '\\d+',
                millis: '\\d+',
                day: '\\d+',
                month: '\\d+',
                monthName: `(?:${monthStrings.join('|')})`,
                year: '\\d+'
            };

            const itlNoEscape = { _escaped: false };

            var extraDateFormats = lib.translate(lib.itlcodes.TIME_EXTRA_DATE_FORMATS, itlNoEscape).split('\n').map(l => l.trim()).filter(l => l.length > 0);
            var extraTimeFormats = lib.translate(lib.itlcodes.TIME_EXTRA_TIME_FORMATS, itlNoEscape).split('\n').map(l => l.trim()).filter(l => l.length > 0);

            var extraFullFormats = lib.hasTranslation(lib.itlcodes.TIME_EXTRA_FULL_FORMATS)
                ? lib.translate(lib.itlcodes.TIME_EXTRA_FULL_FORMATS, itlNoEscape).split('\n').map(l => l.trim()).filter(l => l.length > 0)
                : [];

            let extraDateTimePermutations = extraDateFormats.map(d => extraTimeFormats.map(t => ({ date: d, time: t }))).flat();
            extraFullFormats.push(...extraDateTimePermutations.map(p => lib.namedReplace(nativeTranslations.Time_OnAt, p)));
            extraFullFormats.push(...extraDateTimePermutations.map(p => lib.namedReplace(nativeTranslations.Time_At, p)));

            function firstFormatMatch(formatStrings) {
                for (let i = 0; i < formatStrings.length; i++) {
                    let match = lib.namedMatch(timeString, formatStrings[i], paramPatterns);
                    if (match) return match;
                }
                return null;
            }

            function matchTimeDateFormat() {
                const format = '{time} {date}';
                let variants = extraDateTimePermutations.map(p => lib.namedReplace(format, p));
                return firstFormatMatch(variants);
            }

            function matchDateTimeFormat() {
                const format = '{date} {time}';
                let variants = extraDateTimePermutations.map(p => lib.namedReplace(format, p));
                return firstFormatMatch(variants);
            }

            function matchLocaleTodayFormat() {
                let todayPermutations = extraTimeFormats.map(t => lib.namedReplace(nativeTranslations.Time_TodayAt, { time: t }))
                    .concat(extraTimeFormats.map(t => lib.translate(lib.itlcodes.TIME_TODAY_AT, { time: t, _escaped: false })));

                let match = firstFormatMatch(todayPermutations);
                if (match) {
                    match = {
                        ...match,
                        day: serverDate[0],
                        month: serverDate[1],
                        year: serverDate[2]
                    }
                }
                return match;
            }

            function matchLocaleTomorrowFormat() {
                let tomorrowPermutations = extraTimeFormats.map(t => lib.namedReplace(nativeTranslations.Time_TomorrowAt, { time: t }))
                    .concat(extraTimeFormats.map(t => lib.translate(lib.itlcodes.TIME_TOMORROW_AT, { time: t, _escaped: false })));

                let match = firstFormatMatch(tomorrowPermutations);
                if (match) {
                    match = {
                        ...match,
                        day: serverDate[0] + 1,
                        month: serverDate[1],
                        year: serverDate[2]
                    }
                }
                return match;
            }

            function matchLocaleExactFormat() {
                let exactPermutations = extraFullFormats.slice();
                exactPermutations.push(...extraDateTimePermutations.map(p => lib.translate(lib.itlcodes.TIME_ON, { time: p.time, date: p.date, _escaped: false })));
                exactPermutations.push(...extraDateTimePermutations.map(p => lib.translate(lib.itlcodes.TIME_ON_AT, { time: p.time, date: p.date, _escaped: false })));
                return firstFormatMatch(exactPermutations);
            }

            let matchers = [
                matchTimeDateFormat,
                matchDateTimeFormat,
                matchLocaleTodayFormat,
                matchLocaleTomorrowFormat,
                matchLocaleExactFormat
            ];

            var match = null;
            matchers.forEach((m) => {
                if (!match)
                    match = m();
            });

            if (match == null) {
                debugger;
                console.warn('Unable to parse datetime string: ', timeString);
                return null;
            }

            /*
                // (Hours:Minutes:Seconds:Ms) on (Day/Month/Year)
                result = {
                    date: match[2].splitMany(dateSeparators),
                    time: match[1].splitMany(timeSeparators)
                };
            */

            if (match.year && match.year.length == 2) {
                match.year = '20' + match.year;
            }

            if (match.monthName && monthStrings.indexOf(match.monthName.toLowerCase()) >= 0) {
                match.month = monthStrings.indexOf(match.monthName.toLowerCase()) + 1;
                delete match.monthName;
            }

            Object.keys(match).forEach((k) => typeof match[k] != 'number' ? match[k] = parseInt(match[k]) : null);

            var result = {
                date: [
                    match.day,
                    match.month,
                    match.year || parseInt(serverDate[2])
                ],
                time: [
                    match.hour,
                    match.minute,
                    match.second || 0,
                    match.millis || 0
                ]
            }

            if (separated_) {
                return result;
            } else {
                var dateTime = new Date(
                    Date.UTC(result.date[2], result.date[1] - 1, result.date[0], result.time[0], result.time[1], result.time[2], result.time[3] || 0)
                );
                return dateTime;
            }
        },

        formatDateTime: function (dateTime) {
            let minLength = (str, len_) => '0'.repeat((len_ || 2) - str.toString().length) + str.toString();

            return lib.translate(lib.itlcodes.TIME_DATE_FORMAT, {
                hour: minLength(dateTime.getUTCHours()),
                minute: minLength(dateTime.getUTCMinutes()),
                second: minLength(dateTime.getUTCSeconds()),
                millisecond: minLength(dateTime.getUTCMilliseconds(), 3),

                day: minLength(dateTime.getUTCDate()),
                month: minLength(dateTime.getUTCMonth() + 1),
                year: dateTime.getUTCFullYear()
            });
        },

        formatDuration: function (durationOrTime) {
            let duration = durationOrTime;
            if (durationOrTime instanceof Date) {
                let serverTime = lib.getServerDateTime();
                duration = serverTime.valueOf() - durationOrTime.valueOf();
            }

            duration = Math.abs(duration);
            duration = Math.round(duration / 1000);

            let numSeconds = duration % 60; duration -= numSeconds; duration /= 60;
            let numMinutes = duration % 60; duration -= numMinutes; duration /= 60;
            let numHours = duration % 24; duration -= numHours; duration /= 24;
            let numDays = duration;

            let nonZeroParts = [];
            if (numDays) nonZeroParts.push([numDays, lib.translate(lib.itlcodes.TIME_DAY_SHORT)]);
            if (numHours) nonZeroParts.push([numHours, lib.translate(lib.itlcodes.TIME_HOUR_SHORT)]);
            if (numMinutes) nonZeroParts.push([numMinutes, lib.translate(lib.itlcodes.TIME_MINUTE_SHORT)]);
            if (numSeconds) nonZeroParts.push([numSeconds, lib.translate(lib.itlcodes.TIME_SECOND_SHORT)]);

            if (nonZeroParts.length > 2)
                nonZeroParts = nonZeroParts.slice(0, 2);
            
            if (nonZeroParts.length == 0)
                return lib.translate(lib.itlcodes.TIME_NOW);
            
            let singularToPlural = {};
            singularToPlural[lib.translate(lib.itlcodes.TIME_DAY_SHORT)] = lib.translate(lib.itlcodes.TIME_DAY_PLURAL_SHORT);
            singularToPlural[lib.translate(lib.itlcodes.TIME_HOUR_SHORT)] = lib.translate(lib.itlcodes.TIME_HOUR_PLURAL_SHORT);
            singularToPlural[lib.translate(lib.itlcodes.TIME_MINUTE_SHORT)] = lib.translate(lib.itlcodes.TIME_MINUTE_PLURAL_SHORT);
            singularToPlural[lib.translate(lib.itlcodes.TIME_SECOND_SHORT)] = lib.translate(lib.itlcodes.TIME_SECOND_PLURAL_SHORT);

            return nonZeroParts.map((part) => {
                let count = part[0];
                let label = part[1];

                if (count != 1)
                    label = singularToPlural[label];

                return count + ' ' + label;
            }).join(', ');
        },

        parseHtml: function (htmlText) {
            let parser = new DOMParser();
            let doc = parser.parseFromString(htmlText, "text/html");
            return $(doc);
        },

        //  Checks that the current page is 'pageType'; if not, returns false and optionally
        //  displays a message (if a messsage is displayed, this throws an exception and stops
        //  the script from running any further)
        ensurePage: function ensurePage(pageType, showMessage_) {
            if (typeof showMessage_ == 'undefined')
                showMessage_ = false;

            let validator = pageValidators[pageType];
            if (!validator()) {

                let message = `Run this script on the ${pageType.toLowerCase().replace(/\_/g, ' ')} page!`;
                if (showMessage_) {
                    alert(message);
                    throw message;
                }

                return false;
            } else {
                return true;
            }
        },

        //  Returns one of lib.pageTypes that matches the current page
        getCurrentPage: function () {
            var currentPage = null;
            lib.objForEach(lib.pageTypes, (type) => type != lib.pageTypes.UNKNOWN && pageValidators[type] && pageValidators[type]() ? currentPage = type : null);
            return currentPage || lib.pageTypes.UNKNOWN;
        },

        //  Runs the given 'callback' if the current page is 'pageType'. Returns 'lib' so that
        //  onPage can be chained.
        onPage: function (pageType, callback) {
            if (lib.getCurrentPage() == pageType) {
                callback();
                wasPageHandled = true;
            }
            return lib;
        },

        onPageNotHandled: function (callback) {
            if (!wasPageHandled) {
                callback();
                wasPageHandled = true;
            }
            return lib;
        },

        onFirstRun: function (callback) {
            var hasRan = lib.getLocalStorage('consented');
            let approvedRunCallback = () => lib.setLocalStorage('consented', true);
            if (!hasRan) {
                callback(approvedRunCallback);
            }

            return !hasRan;
        },

        getSavedTranslationId: function () {
            return lib.getLocalStorage('currentTranslationId', null);
        },

        getCurrentTranslationAsync: function (callback) {
            if (currentTranslation) {
                callback && callback(currentTranslation);
                return;
            }

            let translationId = lib.getLocalStorage('currentTranslationId');
            if (translationId == null) {
                $.get(lib.makeVaultUrl('api/translation/default/' + window.location.host))
                    .done((translation) => {
                        lib.setCurrentTranslation(translation.translationId);
                        lib.getCurrentTranslationAsync(callback);
                    });
                return;
            }

            lib.getApi('playertranslation')
                .done((translation) => {
                    currentTranslation = translation;
                    callback && callback(currentTranslation);
                });
        },

        getCurrentTranslation: function () {
            if (currentTranslation) {
                return currentTranslation;
            } else {
                throw "No translation loaded";
            }
        },

        setCurrentTranslation: function (translationId) {
            lib.setLocalStorage('currentTranslationId', translationId);
            currentTranslation = null;
        },

        createNewTranslation: function () {
            return {
                name: 'My New Translation',
                languageId: lib.getCurrentTranslation().languageId,
                entries: {}
            };
        },

        hasTranslation: function (key) {
            if (currentTranslation == null)
                throw "No translation loaded";
            return !!currentTranslation.entries[key];
        },

        translate: function (key, params_, breakNewlines_) {
            if (currentTranslation == null)
                throw "No translation loaded";

            if (window.location.search.contains("notranslate")) {
                return "_TL_";
            }

            let reservedParams = [
                '_escaped'
            ];

            let needsEscaping = !params_ || (typeof params_._escaped == 'undefined' || params_._escaped);
            if (params_ && typeof params_._escaped != 'undefined')
                delete params_._escaped;

            if ($.isEmptyObject(params_))
                params_ = null;

            let result = currentTranslation.entries[key];

            if (!result || !result.trim().length) {
                console.error('No translation provided for key: ', key);
                return "NO TRANSLATION AVAILABLE";
            } else {
                if (needsEscaping)
                    result = escapeHtml(result);

                let keyParams = translationParameters[key];
                if (keyParams) {
                    if (!params_ || !params_._verbatim) {
                        let visitedParams = [];
                        params_ && lib.objForEach(params_, (prop, val) => {
                            if (reservedParams.contains(prop))
                                return;

                            if (!keyParams.contains(prop)) {
                                let msg = `Translation code ${key} does not have a parameter named '${prop}'`;
                                console.error(msg);
                                throw msg;
                            } else {
                                result = result.replace(`{${prop}}`, val);
                                visitedParams.push(prop);
                            }
                        });

                        if (visitedParams.length != keyParams.length) {
                            let msg = `Translation code ${key} requires params ${keyParams.length} - "${keyParams.join(", ")}",` +
                                ` but only got ${visitedParams.length} - "${visitedParams.join(", ")}"` +
                                ` (missing: ${keyParams.except(visitedParams).join(", ")})`;
                            console.error(msg);
                            throw msg;
                        }
                    }
                } else if (params_) {
                    console.warn('Parameters provided for translation code ' + key + ' but no parameters exist for that code');
                }

                breakNewlines_ = breakNewlines_ || false;

                if (breakNewlines_) {
                    result = result.replace(/\n/g, '\n<br>');
                }

                return result;
            }
        },

        // Takes a template string of format "{param-1} {param-2} ...", replaces the parameter entries
        // with regex patterns from 'paramPatterns', and returns a regex match against the given string.
        // Result is an object with matching parameter names containing the matched items. Meant to be used
        // multiple times with the same string/paramPatterns but different templates, returning a consistent
        // set of results that can be consistently accessed.
        namedMatch: function namedMatch(string, template, paramPatterns) {

            var specialChars = ['\\', '.', '(', ')', '[', ']', '+', '?'];
            specialChars.forEach((c) => {
                template = template.replace(new RegExp('\\' + c, 'g'), '\\' + c);
            });

            template = template.replace(/\s+/g, '\\s+');

            let acceptedGroups = Object.keys(paramPatterns).map(group => {
                return {
                    group: group,
                    pattern: paramPatterns[group],
                    index: template.indexOf(`{${group}}`)
                };
            });

            acceptedGroups = acceptedGroups.filter(g => g.index >= 0);
            acceptedGroups.sort((a, b) => a.index - b.index);

            let regexSpec = template;
            acceptedGroups.forEach((g) => regexSpec = regexSpec.replace(`{${g.group}}`, `(${g.pattern})`));
            let regex = new RegExp(regexSpec, 'i')

            var match = regex.exec(string);
            var result = null;
            if (match) {
                result = {};
                for (let i = 1; i < match.length; i++) {
                    let group = acceptedGroups[i - 1];
                    result[group.group] = match[i];
                }
            }

            return result;
        },

        isUnloading: function () {
            return isUnloading;
        },

        whenAll$: function (requests, callback) {
            if (!requests.length) {
                callback();
                return;
            }

            let numDone = 0;
            requests.forEach((r) => {
                r.done(() => {
                    if (++numDone == requests.length) {
                        callback();
                    }
                });
            });
        },

        //  Iterates over the properties of an object similar to iterating over an array
        objForEach: function objForEach(obj, callback) {
            for (var prop in obj) {
                if (!obj.hasOwnProperty(prop)) continue;

                callback(prop, obj[prop], obj);
            }
        },

        //  Ensures that this script is being ran from v.tylercamp.me
        validateScriptSource: function validateScriptSource() {
            //  TODO
        },

        getApi: function getApi(url) {
            return $.ajax(lib.makeApiUrl(url), {
                method: 'GET',
                converters: {
                    "text json": lib.jsonParse
                },
                beforeSend: (xhr) => {
                    xhr.setRequestHeader('X-V-TOKEN', makeAuthHeader(authUserId, authTribeId, authToken, lib.isSitter()));
                    xhr.setRequestHeader('X-V-TRANSLATION-ID', lib.getSavedTranslationId());
                }
            });
        },

        //  POSTs JSON data to the given URL with vault auth data
        postApi: function postApi(url, object) {
            if (typeof object != 'string' && !!object)
                object = JSON.stringify(object);

            if (object && object.length) {
                object = encryption.encryptString(object, lib.getCurrentUtcTimestamp());
            }

            return $.ajax(lib.makeApiUrl(url), {
                data: object,
                contentType: 'application/json',
                type: 'POST',
                converters: {
                    "text json": lib.jsonParse
                },
                beforeSend: (xhr) => {
                    xhr.setRequestHeader('X-V-TOKEN', makeAuthHeader(authUserId, authTribeId, authToken, lib.isSitter()));
                    xhr.setRequestHeader('X-V-TRANSLATION-ID', lib.getSavedTranslationId());
                }
            });
        },

        //  PUTs JSON data to the given URL with vault auth data
        putApi: function putApi(url, object) {
            if (typeof object != 'string' && !!object)
                object = JSON.stringify(object);

            if (object && object.length) {
                object = encryption.encryptString(object, lib.getCurrentUtcTimestamp());
            }

            return $.ajax(lib.makeApiUrl(url), {
                data: object,
                contentType: 'application/json',
                type: 'PUT',
                converters: {
                    "text json": lib.jsonParse
                },
                beforeSend: (xhr) => {
                    xhr.setRequestHeader('X-V-TOKEN', makeAuthHeader(authUserId, authTribeId, authToken, lib.isSitter()));
                    xhr.setRequestHeader('X-V-TRANSLATION-ID', lib.getSavedTranslationId());
                }
            });
        },

        deleteApi: function deleteApi(url, object) {
            if (typeof object != 'string' && !!object)
                object = JSON.stringify(object);

            if (object && object.length) {
                object = encryption.encryptString(object, lib.getCurrentUtcTimestamp());
            }

            return $.ajax(lib.makeApiUrl(url), {
                data: object,
                contentType: 'application/json',
                type: 'DELETE',
                converters: {
                    "text json": lib.jsonParse
                },
                beforeSend: (xhr) => {
                    xhr.setRequestHeader('X-V-TOKEN', makeAuthHeader(authUserId, authTribeId, authToken, lib.isSitter()));
                    xhr.setRequestHeader('X-V-TRANSLATION-ID', lib.getSavedTranslationId());
                }
            });
        },

        checkContainsCaptcha: function checkContainsCaptcha(docOrHtml) {
            var foundCaptcha = false;
            if (typeof docOrHtml == 'string') {
                foundCaptcha = !!docOrHtml.match(/data\-bot\-protect=/);
            } else {
                let $doc = $(docOrHtml);
                let $body = $doc.find('#ds_body');
                foundCaptcha = $body.length && !!$body.data('bot-protect')
            }

            if (foundCaptcha) console.log('Found captcha!');
            return foundCaptcha;
        },

        saveAsFile: function saveAsFile(filename, fileContents) {
            //  https://gist.github.com/liabru/11263260
            let blob = new Blob([fileContents], { type: 'application/octet-stream' });
            let anchor = $('<a>')[0];

            anchor.download = filename;
            anchor.href = (window.URL || window.webkitURL).createObjectURL(blob);
            anchor.dataset.downloadurl = ['application/octet-stream', anchor.download, anchor.href].join(':');
            anchor.click();
        },

        getCurrentServer: function getCurrentServer() {
            return location.hostname.split('.')[0];
        },

        getCurrentServerSettings: function getCurrentServerSettings() {
            return serverSettings;
        },

        //  Make a local tribalwars server link, using sitter tag '&t=NNN'
        //  Can also return the appropriate URL for any given 'lib.pageType'
        makeTwUrl: function makeTwUrl(url) {
            var queryString = window.location.search;
            if (queryString.startsWith("?"))
                queryString = queryString.substr(1);

            var query = lib.arrayToObject(queryString.split('&'), (obj, q) => {
                var split = q.split('=');
                if (split.length > 1)
                    obj[split[0]] = split[1];
            });

            if (lib.pageTypes[url]) {
                if (!pageUrls[url]) {
                    throw "No pageUrl is available for pageType: " + url;
                }
                url = pageUrls[url];
            }

            url = location.origin + '/game.php?' + url;

            var t = query['t'];
            if (t) {
                if (url.contains("?")) {
                    url += `&t=${t}`;
                } else {
                    url += `?t=${t}`;
                }
            }

            return url;
        },

        isSitter: function () {
            return !!window.location.href.match(/[?&]t=\w+/);
        },

        absoluteTwUrl: function formatToAbsoluteTwUrl(url) {
            if (!url.startsWith(window.location.origin)) {
                if (!url.startsWith('/'))
                    url = '/' + url;
                url = window.location.origin + url;
            }
            return url;
        },

        makeVaultUrl: function makeVaultUrl(url) {
            if (url.startsWith('https://')) {
                return url;
            }

            let result = lib.config.appBasePath + url;
            return result;
        },

        // Make a URL relative to 'https://v.tylercamp.me/api' (or whatever the current base path is)
        makeApiUrl: function makeApiUrl(url) {
            return lib.makeVaultUrl(`api/${lib.getCurrentServer()}/${url.trim('/')}`);
        },

        queryCurrentPlayerInfo: function (callback) {
            let queryUrl = lib.makeTwUrl('screen=ranking&mode=player');
            $.get(queryUrl, (data) => {
                let $doc = lib.parseHtml(data);

                let $playerInfo = $doc.find('.lit a');
                let playerId = parseInt($($playerInfo[0]).prop('href').match(/id\=(\w+)/)[1]);
                let playerName = $playerInfo.text().trim();
                let tribeId = null;
                if ($playerInfo.length > 1)
                    tribeId = parseInt($($playerInfo[1]).prop('href').match(/id\=(\w+)/)[1]);

                callback(playerId, tribeId, playerName);
            });
        },

        getCurrentPlayerId: function () {
            return authUserId;
        },

        getCurrentPlayerName: function () {
            return userName;
        },

        getCurrentTribeId: function () {
            return authTribeId;
        },

        //  Gets the links to all pages of the current view, ie get links for each pages of reports if there are over 1000, etc
        detectMultiPages: function ($doc_) {
            $doc_ = $doc_ || $(document);
            let $navItems = $doc_.find('.paged-nav-item');
            let links = [];

            if (!$navItems.length)
                return links;

            let $container = $navItems.parent();
            let $pageSelect = $container.find('select');
            //  Sometimes there are so many pages that TW won't show the all of the links, and provides a <select>
            //  element instead
            if ($pageSelect.length) {
                let $options = $pageSelect.find('option');
                $options.each((i, el) => links.push($(el).prop('value')));
            } else {
                $navItems.each((i, el) => {
                    let $el = $(el);
                    if ($el.is('a'))
                        links.push($el.prop('href'));

                    $el.find('a').each((i, a) => {
                        links.push($(a).prop('href'));
                    });
                });
            }

            links.forEach((l, i) => links[i] = lib.absoluteTwUrl(l));

            return links;
        },

        //  Gets the links to all groups currently visible, and whether or not current group is "all"
        detectGroups: function ($doc_) {
            $doc_ = $doc_ || $(document);
            let $groupItems = $doc_.find('.group-menu-item');
            let links = [];

            if (!$groupItems.length)
                return links;

            $groupItems.each((i, el) => {
                let $el = $(el);
                if ($el.is('a'))
                    links.push($el.prop('href'));

                $el.find('a').each((i, a) => {
                    links.push($(a).prop('href'));
                });
            });

            links.forEach((l, i) => links[i] = lib.absoluteTwUrl(l));

            return {
                isAll: !links.contains((l) => l.contains("group=0")),
                links: links
            };
        },

        checkUserHasPremium: function userHasPremium() {
            return window.premium;
        },

        troopsArrayToObject: function troopsArrayToNamedObject(array) {
            let result = {};
            for (var i = 0; i < array.length && i < lib.twstats.unitTypes.length; i++) {
                result[lib.twstats.unitTypes[i].canonicalName] = array[i];
            }
            return result;
        },

        getCookie: function getCookie(name) {
            let finalName = `${cookiePrefix}${name}`;
            var match = document.cookie.match(new RegExp(`${finalName}=([^\s\;]+)`));
            let value = match ? match[1] : null;
            if (value) {
                try {
                    return lib.jsonParse(value);
                } catch (_) {
                    return value;
                }
            } else {
                return value;
            }
        },

        setCookie: function setCookie(name, value) {
            if (!value) value = '';
            if (typeof value == 'function' || typeof value == 'object') throw "Cookie value cannot be a function or object! (Don't store JSON.stringify in cookie either!)";
            let finalName = `${cookiePrefix}${name}`;
            document.cookie = `${finalName}=${value}`;
        },

        clearCookie: function clearCookie(name) {
            let finalName = `${cookiePrefix}${name}`;
            document.cookie = `${finalName}=; expires=Thu, 01 Jan 1970 00:00:00 GMT`;
        },

        setLocalStorage: function setLocalStorage(key, value) {
            let finalKey = `${localStoragePrefix}${key}`;
            window.localStorage.setItem(finalKey, lib.jsonStringify(value));
        },

        getLocalStorage: function getLocalStorage(key, defaultValue_) {
            let finalKey = `${localStoragePrefix}${key}`;
            let stored = window.localStorage.getItem(finalKey);
            if (stored == null) {
                if (typeof defaultValue_ == 'undefined') {
                    return null;
                } else {
                    lib.setLocalStorage(key, defaultValue_);
                    return defaultValue_;
                }
            } else {
                try {
                    let result = lib.jsonParse(stored);
                    if (typeof result == 'object' && !(result instanceof Array) && !(result instanceof Date) && defaultValue_) {
                        result = $.extend(defaultValue_, result);
                    }
                    return result;
                } catch (_) {
                    return stored;
                }
            }
        },

        deleteLocalStorage: function deleteLocalStorage(key) {
            const finalKey = `${localStoragePrefix}${key}`;
            window.localStorage.removeItem(finalKey);
        },

        getLocalStorageSize: function getLocalStorageSize() {
            var totalSize = 0;
            var vaultSize = 0;
            var keySizes = {};
            for (var i = 0; i < localStorage.length; i++) {
                var key = localStorage.key(i);
                totalSize += localStorage.getItem(key).length;
                if (key.startsWith(localStoragePrefix)) {
                    vaultSize += localStorage.getItem(key).length;
                }
                keySizes[key] = Math.roundTo(localStorage.getItem(key).length / 1024, 2);
            }

            return {
                totalKb: Math.roundTo(totalSize / 1024, 2),
                vaultKb: Math.roundTo(vaultSize / 1024, 2),
                perKeyKb: keySizes
            };
        },

        //  Stringify JSON while preserving Date formatting
        jsonStringify: function jsonStringifyWithDates(object) {
            if (typeof object == 'string')
                return object;

            if (object instanceof Date) {
                return object.toISOString();
            }

            let result = lib.clone(object);
            lib.recursiveObjForEach(result, (prop, value, obj) => {
                if (value instanceof Date) {
                    obj[prop] = value.toISOString();
                }
            });

            return JSON.stringify(result);
        },

        //  Parse JSON while checking for date formatting
        jsonParse: function jsonParseWithDates(json) {
            let stringIsDate = (str) => !!str.match(/^\w+,?\s+\d+\s+\w+\s+\d+\s+\d+:\d+:\d+/) || !!str.match(/^\d+\-\d+\-\d+T\d+:\d+:\d+(?:\.\d+)?Z$/);

            if (stringIsDate(json))
                return new Date(json);

            let result = JSON.parse(json);
            if (result == null)
                return null;

            lib.recursiveObjForEach(result, (prop, value, obj) => {
                if (typeof value == 'string' && stringIsDate(value)) {
                    obj[prop] = new Date(value);
                }
            });

            return result;
        },

        clone: function cloneObject(object) {
            if (typeof object != 'object')
                return object;

            var result;
            if (object instanceof Array) {
                result = [];
                object.forEach((v, i) => {
                    if (typeof v == 'object')
                        result.push(lib.clone(v));
                    else
                        result.push(v);
                });
            } else {
                result = {};
                for (var prop in object) {
                    if (!object.hasOwnProperty(prop)) continue;
                    if (object[prop] instanceof Date) {
                        result[prop] = object[prop];
                    } else if (typeof object[prop] == 'object') {
                        result[prop] = lib.clone(object[prop]);
                    } else {
                        result[prop] = object[prop];
                    }
                }
            }
            return result;
        },

        //  Converts an array to an object using either the "keySelector/valueSelector" parameters,
        //  or by calling the 'transformer'
        //  keySelector: (entry) => key
        //  valueSelector: (entry) => value
        //  transformer: (object, entry) => none
        arrayToObject: function arrayToObject(array, keySelectorOrTransformer, valueSelector_) {
            var result = {};
            if (!valueSelector_)
                array.forEach((v) => keySelectorOrTransformer(result, v));
            else
                array.forEach((v) => result[keySelectorOrTransformer(v)] = valueSelector_(v));
            return result;
        },

        arrayToObjectByGroup: function arrayToObjectByGroup(array, keySelector) {
            var result = {};
            array.forEach((v) => {
                let k = keySelector(v);
                if (!result[k])
                    result[k] = [v];
                else
                    result[k].push(v);
            });
            return result;
        },

        groupBy: function groupArrayBy(array, keySelector) {
            var result = [];
            var resultMap = {};
            array.forEach((v) => {
                let k = keySelector(v);
                if (!resultMap[k])
                    result.push(resultMap[k] = { key: k, values: [v] });
                else
                    resultMap[k].values.push(v);
            });
            return result;
        },

        selectSort: function (array, selector_) {
            selector_ = selector_ || ((_) => _);

            array.sort((a, b) =>
                selector_(a) == selector_(b)
                    ? 0
                    : (selector_(a) < selector_(b)
                        ? -1
                        : 1
                    )
            );
        },

        objectToArray: function objectToArray(object, selector) {
            var result = [];
            lib.objForEach(object, (val, prop) => {
                let entry = selector(val, prop);
                if (entry)
                    result.push(entry);
            });
            return result;
        },

        mapObject: function mapObjectToObject(object, keySelector, valueSelector) {
            var result = {};
            lib.objForEach(object, (val, prop) => {
                result[keySelector(val, prop)] = valueSelector(val, prop);
            });
            return result;
        },

        recursiveObjForEach: function recursiveObjectForEach(object, callback, sourceObject_) {
            if (typeof object != 'object') {
                return object;
            }

            if (object instanceof Array) {
                object.forEach((v, i) => {
                    callback.call(object, i, v, object, sourceObject_);

                    if (typeof v == 'object') {
                        lib.recursiveObjForEach(v, callback, sourceObject_ || object);
                    }
                });
            } else {
                for (var prop in object) {
                    if (!object.hasOwnProperty(prop)) continue;
                    if (typeof object[prop] == 'function') continue;

                    callback.call(object, prop, object[prop], object, sourceObject_);

                    if (typeof object[prop] == 'object') {
                        lib.recursiveObjForEach(object[prop], callback, sourceObject_ || object);
                    }
                }
            }
        },

        namedReplace: function namedStringReplace(string, replacements) {
            Object.keys(replacements).forEach((key) => string = string.replace(`{${key}}`, replacements[key]));
            return string;
        },

        // Binds all functions in the object to the object itself
        bindMembers: function (object) {
            lib.objForEach(object, (prop, value) => {
                if (typeof value == 'function')
                    object[prop] = value.bind(object);
            });
        },

        init: function init(callback) {
            lib.queryCurrentPlayerInfo((playerId, tribeId, playerName) => {
                authUserId = playerId;
                authTribeId = tribeId;
                userName = playerName;

                function checkDone() {
                    if (utcTimeOffset != null && serverSettings != null && currentTranslation != null && translationParameters != null) {
                        lib.twstats._updateWithSettings(
                            serverSettings.archersEnabled,
                            serverSettings.militiaEnabled,
                            serverSettings.paladinEnabled
                        );

                        lib.messages.TRIGGERED_CAPTCHA = lib.translate(lib.itlcodes.TRIGGERED_CAPTCHA, { _escaped: false });
                        lib.messages.IS_IN_GROUP = lib.translate(lib.itlcodes.IS_IN_GROUP, { _escaped: false });
                        lib.messages.FILTER_APPLIED = (type) => lib.translate(lib.itlcodes.FILTER_APPLIED, { dataType: type, _escaped: false });
                        lib.messages.GENERIC_ERROR = lib.translate(lib.itlcodes.ERROR_OCCURRED, { _escaped: false });
                        callback();
                    }
                }

                $.get(lib.makeApiUrl('server/utc'))
                    .done((data) => {
                        let serverUtcTime = data.utcTime;
                        utcTimeOffset = serverUtcTime - Date.now();
                        console.log('Got UTC offset as ' + utcTimeOffset + 'ms');
                        checkDone();

                        lib.getCurrentTranslationAsync(checkDone);
                    });

                $.get(lib.makeApiUrl('server/settings'))
                    .done((data) => {
                        serverSettings = data;
                        checkDone();
                    });

                $.get(lib.makeVaultUrl('api/translation/parameters'))
                    .done((data) => {
                        translationParameters = data;
                        checkDone();
                    });
            });
        }
    };

    //  Utility additions to String
    String.prototype.contains = function contains(str) {
        return this.indexOf(str) >= 0;
    };

    String.prototype.splitMany = function splitMany() {
        var splitTokens = [];
        for (var i = 0; i < arguments.length; i++) {
            if (typeof arguments[i] == 'string')
                splitTokens.push(arguments[i]);
            else if (arguments[i] instanceof Array)
                splitTokens.push(...arguments[i]);
        }

        let result = [];
        let workingStr = '';
        for (var i = 0; i < this.length; i++) {
            if (splitTokens.contains(this[i])) {
                result.push(workingStr);
                workingStr = '';
            } else {
                workingStr += this[i];
            }
        }
        result.push(workingStr);
        return result;
    };

    //  Allow syntax "a,b.c".split('.', ',') -> ['a', 'b', 'c']
    let stringOriginalTrim = String.prototype.trim;
    String.prototype.trim = function trimAll() {
        var result = stringOriginalTrim.apply(this);
        var trimTokens = [];
        for (var i = 0; i < arguments.length; i++) {
            if (typeof arguments[i] == 'string')
                trimTokens.push(arguments[i]);
            else if (arguments[i] instanceof Array)
                arguments[i].forEach((str) => typeof str == 'string' ? trimTokens.push(str) : null);
        }

        let startsWithToken = () => {
            for (var i = 0; i < trimTokens.length; i++) {
                if (result.startsWith(trimTokens[i]))
                    return true;
            }
            return false;
        };

        let endsWithToken = () => {
            for (var i = 0; i < trimTokens.length; i++) {
                if (result.endsWith(trimTokens[i]))
                    return true;
            }
            return false;
        };

        var trimmed = true;
        while (trimmed) {
            trimmed = false;
            if (startsWithToken()) {
                result = result.substr(1);
                trimmed = true;
            }
            if (endsWithToken()) {
                result = result.substr(0, result.length - 1);
                trimmed = true;
            }
        }

        return result;
    };

    //  Utility additions to Array
    Array.prototype.distinct = function distinct(comparer_) {
        var result = [];
        this.forEach((v) => {
            if (comparer_) {
                var contains = false;
                result.forEach((existing) => comparer_(existing, v) ? contains = true : null);
                if (!contains)
                    result.push(v);
            } else {
                if (!result.contains(v))
                    result.push(v);
            }
        });
        return result;
    };

    Array.prototype.distinctBy = function distinctBy(selector) {
        var map = {};
        this.forEach((v) => map[selector(v)] = v);

        var result = [];
        for (var prop in map) {
            if (map.hasOwnProperty(prop))
                result.push(map[prop]);
        }
        return result;
    };

    Array.prototype.contains = function contains(valOrChecker) {
        var contains = false;
        if (!(typeof valOrChecker == 'function'))
            contains = this.indexOf(valOrChecker) >= 0;
        else
            this.forEach((v) => valOrChecker(v) ? contains = true : null);
        return contains;
    };

    Array.prototype.except = function except(arrOrFunc) {
        let result = [];
        let useFunc = typeof arrOrFunc == 'function';
        this.forEach((v) => {
            if (useFunc) {
                if (!arrOrFunc(v))
                    result.push(v);
            } else {
                if (!arrOrFunc.contains(v))
                    result.push(v);
            }
        });
        return result;
    };

    Array.prototype.removeWhere = function removeWhere(predicate) {
        var removedIndexes = [];
        this.forEach((val, i) => {
            if (predicate(val))
                removedIndexes.push(i);
        });

        for (let i = 0; i < removedIndexes.length; i++) {
            this.splice(removedIndexes[i] - i, 1);
        }
    };

    Array.prototype.updateWhere = function updateWhere(predicate, updater) {
        this.forEach((val) => {
            if (predicate(val))
                updater(val);
        });
    };

    //  Utility additions to Math
    Math.roundTo = function roundTo(val, precision) {
        var divisor = Math.pow(10, precision);
        var result;
        do {
            result = Math.round(val * divisor) / divisor;
            precision++;
        } while (val != 0 && result == 0 && precision < 20);
        return result;
    };

    //  Set values of page types to their names
    lib.objForEach(lib.pageTypes, (name) => lib.pageTypes[name] = name);

    //  Methods that validate the current page
    let pageValidators = {};

    let href = window.location.href;
    pageValidators[lib.pageTypes.VIEW_REPORT] = () => href.contains('screen=report') && href.contains('view=');
    pageValidators[lib.pageTypes.ALL_REPORTS] = () => href.contains('screen=report') && !href.contains("view=");
    pageValidators[lib.pageTypes.INCOMINGS_OVERVIEW] = () => href.contains("screen=overview_villages") && href.contains("mode=incomings");
    pageValidators[lib.pageTypes.OWN_COMMANDS_OVERVIEW] = () => href.contains("screen=overview_villages") && href.contains("mode=commands");
    pageValidators[lib.pageTypes.MAP] = () => href.contains("screen=map");
    pageValidators[lib.pageTypes.OWN_TROOPS_OVERVIEW] = () => href.contains("screen=overview_villages") && href.contains("mode=units");
    pageValidators[lib.pageTypes.OWN_TROOPS_SUPPORTING_OVERVIEW] = () => href.contains("screen=overview_villages") && href.contains("mode=units") && href.contains("type=away_detail");
    pageValidators[lib.pageTypes.BUILDINGS_OVERVIEW] = () => href.contains("screen=overview_villages") && href.contains("mode=buildings");
    pageValidators[lib.pageTypes.VILLAGE_INFO] = () => href.contains("screen=info_village");

    pageValidators[lib.pageTypes.UNKNOWN] = () => lib.getCurrentPage() == lib.pageTypes.UNKNOWN;

    //  URLs for each given page type
    let pageUrls = {};
    pageUrls[lib.pageTypes.VIEW_REPORT] = null; // there's no generic "view report" page, it's specific to each report
    pageUrls[lib.pageTypes.ALL_REPORTS] = 'screen=report&mode=all&group_id=-1';
    pageUrls[lib.pageTypes.INCOMINGS_OVERVIEW] = 'screen=overview_villages&mode=incomings&type=all&subtype=all&group=0&page=-1&subtype=all';
    pageUrls[lib.pageTypes.OWN_COMMANDS_OVERVIEW] = 'screen=overview_villages&mode=commands&type=all&group=0&page=-1';
    pageUrls[lib.pageTypes.MAP] = 'screen=map';
    pageUrls[lib.pageTypes.OWN_TROOPS_OVERVIEW] = 'screen=overview_villages&mode=units&group=0&page=-1&type=complete';
    pageUrls[lib.pageTypes.OWN_TROOPS_SUPPORTING_OVERVIEW] = 'screen=overview_villages&mode=units&type=away_detail&group=0&page=-1';
    pageUrls[lib.pageTypes.BUILDINGS_OVERVIEW] = 'screen=overview_villages&mode=buildings&group=0&page=-1';
    pageUrls[lib.pageTypes.VILLAGE_INFO] = null; // there's no generic "village info" page, it's specific to each village

    //  Make sure all page types have validators
    lib.objForEach(lib.pageTypes, (type) => !pageValidators[type] ? console.warn('No pageValidator set for pageType: ', type) : null);

    //  Expose config props
    lib.config = {
        fakeScriptEnabled: `%V<F_FAKE_SCRIPT_ENABLED>`,

        serverHostname: "%V<HOSTNAME>",
        serverBasePath: "%V<BASE_PATH>",
        appBasePath: "%V<APP_BASE_PATH>",
    }

    return lib;

})();
