
var lib = (() => {

    //# REQUIRE twstats.js
    //# REQUIRE twcalc.js

    let twstats = getTwTroopStats();
    let localStoragePrefix = 'vls-';
    let cookiePrefix = 'vc-';

    var storedScriptHost = null;

    var authToken = window.vaultToken || null;
    var authUserId = null;
    var authTribeId = null;
    var wasPageHandled = false;

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
            MAP: null
        },

        twstats: twstats,
        twcalc: makeTwCalc(twstats),

        //  Gets the current server date and time from the page
        getServerDateTime: function getServerDateTime() {
            var $serverDate = $('#serverDate');
            var $serverTime = $('#serverTime');
            throw "Not yet implemented";
        },

        //  Parses a variety of TW date/time formats to JS Date or into splits it into its parts
        //  returns Date if separated_ false or undefined
        //  returns { date: [day, month, year], time: [hour, minute, second, millisecond] } if separated_ is true
        parseTimeString: function parseTimeString(timeString, separated_) {
            if (typeof separated_ == 'undefined')
                separated_ = false;

            var monthStrings = [
                'jan', 'feb', 'mar', 'apr', 'may', 'jun',
                'jul', 'aug', 'sep', 'oct', 'nov', 'dec'
            ];

            var result;

            var match;
            if (match = timeString.match(/(\d+:\d+:\d+:\d+)\s+(\d+\/\d+\/\d+)/)) {
                //  Hour:Minute:Second:Ms Day/Month/Year
                result = {
                    time: match[1].split(':'),
                    date: match[2].split('/')
                };

            } else if (match = timeString.match(/(\d+\/\d+\/\d+)\s+(\d+:\d+:\d+:\d+)/)) {
                //  Day/Month/Year Hour:Minute:Second:Ms
                result = {
                    date: match[1].split('/'),
                    time: match[2].split(':')
                };

            } else if (match = timeString.match(new RegExp(`((?:${monthStrings.join('|')}))\\s+(\\d+),\\s+(\\d+)\\s+(\\d+:\\d+:\\d+:\\d+)`, 'i'))) {
                //  (Mon.) Day, Year Hour:Minute:Second:Ms
                var monthName = match[1];
                var day = match[2];
                var year = match[3];
                var month = (monthStrings.indexOf(monthName.toLowerCase()) + 1).toString();

                result = {
                    date: [day, month, year],
                    time: match[4].split(':')
                };

            } else if (match = timeString.match(/today at\s+(\d+:\d+:\d+:\d+)/)) {
                // today at (Hours:Minute:Second:Ms)
                var serverDate = $('#serverDate').text().split('/');
                result = {
                    date: serverDate,
                    time: match[1].split(':')
                }

            } else if (match = timeString.match(/tomorrow at\s+(\d+:\d+:\d+:\d+)/)) {
                // tomorrow at (Hours:Minute:Second:Ms)
                var serverDate = $('#serverDate').text().split('/');

                result = {
                    date: [
                        (parseInt(serverDate[0]) + 1).toString(),
                        (parseInt(serverDate[1])).toString(),
                        serverDate[2]
                    ],
                    time: match[1].split(':')
                };

            } else if (match = timeString.match(/on (\d+[\/\.]\d+(?:[\/\.](?:\d+)?)?)\s+at\s+(\d+:\d+:\d+:\d+)/)) {
                // on (Day/Month/Year) at (Hours:Minute:Second:Ms)
                result = {
                    date: match[1].contains('/') ? match[1].split('/') : match[1].split('.'),
                    time: match[2].split(':')
                };

                if (result.date.length < 2) {
                    result.date.push(null);
                }
                if (!result.date[2]) {
                    result.date[2] = $('#serverDate').text().split('/')[2];
                }

            } else {
                result = null;
            }

            if (result == null) {
                return null;
            }

            result.date.forEach((val, i) => result.date[i] = parseInt(val));
            result.time.forEach((val, i) => result.time[i] = parseInt(val));

            if (separated_) {
                return result;
            } else {
                var dateTime = new Date();
                dateTime.setUTCDate(result.date[0]);
                dateTime.setUTCMonth(result.date[1] - 1);
                dateTime.setUTCFullYear(result.date[2]);
                dateTime.setUTCHours(result.time[0]);
                dateTime.setUTCMinutes(result.time[1]);
                dateTime.setUTCSeconds(result.time[2]);
                dateTime.setUTCMilliseconds(result.time[3] || 0);
                return dateTime;
            }
        },

        formateDateTime: function (dateTime) {
            let minLength = (str) => '0'.repeat(2 - str.toString().length) + str.toString();

            return `${minLength(dateTime.getUTCHours())}:${minLength(dateTime.getUTCMinutes())}:${minLength(dateTime.getUTCSeconds())} on ${minLength(dateTime.getUTCDate())}/${minLength(dateTime.getUTCMonth()+1)}/${dateTime.getUTCFullYear()}`;
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
            return $.ajax(url, {
                method: 'GET',
                beforeSend: (xhr) => {
                    xhr.setRequestHeader('X-V-TOKEN', authToken);
                    xhr.setRequestHeader('X-V-PID', authUserId);
                    xhr.setRequestHeader('X-V-TID', authTribeId);
                }
            });
        },

        //  POSTs JSON data to the given URL with vault auth data
        postApi: function postApi(url, object) {
            if (typeof object != 'string' && !!object)
                object = JSON.stringify(object);

            return $.ajax(url, {
                data: object,
                contentType: 'application/json',
                type: 'POST',
                beforeSend: (xhr) => {
                    xhr.setRequestHeader('X-V-TOKEN', authToken);
                    xhr.setRequestHeader('X-V-PID', authUserId);
                    xhr.setRequestHeader('X-V-TID', authTribeId);
                }
            });
        },

        deleteApi: function deleteApi(url, object) {
            if (typeof object != 'string' && !!object)
                object = JSON.stringify(object);

            return $.ajax(url, {
                data: object,
                contentType: 'application/json',
                type: 'DELETE',
                beforeSend: (xhr) => {
                    xhr.setRequestHeader('X-V-TOKEN', authToken);
                    xhr.setRequestHeader('X-V-PID', authUserId);
                    xhr.setRequestHeader('X-V-TID', authTribeId);
                }
            });
        },

        checkContainsCaptcha: function checkContainsCaptcha(docOrHtml) {
            var foundCaptcha = false;
            if (typeof docOrHtml == 'string') {
                foundCaptcha = !!docOrHtml.match(/data\-bot\-protect=/);
            } else {
                let $doc = $(docOrHtml);
                let $body = $doc_.find('#ds_body');
                foundCaptcha = $body.length && !!$body.data('bot-protect')
            }

            if (foundCaptcha) console.log('Found captcha!');
            return foundCaptcha;
        },

        saveAsFile: function saveAsFile(filename, fileContents) {
            //  https://gist.github.com/liabru/11263260
            let blob = new Blob([fileContents], { type: 'text/plain' });
            let anchor = $('<a>')[0];

            anchor.download = filename;
            anchor.href = (window.URL || window.webkitURL).createObjectURL(blob);
            anchor.dataset.downloadurl = ['text/plain', anchor.download, anchor.href].join(':');
            anchor.click();
        },

        getCurrentServer: function getCurrentServer() {
            return location.hostname.split('.')[0];
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

            url = '/game.php?' + url;

            var t = query[t];
            if (t) {
                t = t.match(/t=(\w+)/)[1];
                if (url.contains("?")) {
                    url += `&t=${t}`;
                } else {
                    url += `?t=${t}`;
                }
            }

            return url;
        },

        // Make a URL relative to 'https://v.tylercamp.me/api' (or whatever the current base path is)
        makeApiUrl: function makeApiUrl(url) {
            let serverBase = 'https://v.tylercamp.me';

            //  Check if running from dev or from real server
            let host = lib.getScriptHost();
            let path = host.match(/tylercamp.me\/(.*)/)[1];

            let pathParts = path.split('/');

            //  Known server API base paths
            let rootPaths = [
                'api',
                'script'
            ];

            var apiBasePath;
            rootPaths.forEach((p) => path.contains(p) ? apiBasePath = path.substr(0, path.indexOf(p)) : null);

            let result = `${serverBase.trim('/')}${apiBasePath ? '/' + apiBasePath.trim('/') : ''}/api/${lib.getCurrentServer()}/${url.trim('/')}`;
            return result;
        },

        queryCurrentPlayerInfo: function (callback) {
            callback(9834678, 1096)
            return;

            let queryUrl = lib.makeTwUrl('screen=ranking&mode=player');
            $.get(queryUrl, (data) => {
                let $doc = $(data);

                let $playerInfo = $doc.find('.lit a');
                let playerId = parseInt($($playerInfo[0]).prop('href').match(/id\=(\w+)/)[1]);
                let tribeId = null;
                if ($playerInfo.length > 1)
                    tribeId = parseInt($($playerInfo[1]).prop('href').match(/id\=(\w+)/)[1]);

                callback(playerId, tribeId);
            });
        },

        checkUserHasPremium: function userHasPremium() {
            return !!$('.menu-column-item a[href*=quickbar]').length;
        },

        //  Gets the URL that the script was requested from
        getScriptHost: function getScriptHost() {
            if (storedScriptHost)
                return storedScriptHost;

            let ex = new Error();
            let stack = ex.stack.split('\n').map(p => p.trim());
            let firstScope = stack[stack.length - 1];
            let sourceUrl = firstScope.match(/at (.+\.js)/);
            if (sourceUrl)
                sourceUrl = sourceUrl[1];

            return sourceUrl;
        },

        setScriptHost: function setScriptHost(scriptHost) {
            storedScriptHost = scriptHost;
        },

        getCookie: function getCookie(name) {
            let finalName = `${cookiePrefix}${name}`;
            var match = document.cookie.match(new RegExp(`${finalName}=([^\s\;]+)`));
            let value = match ? match[1] : null;
            if (value) {
                try {
                    return lib.jsonParse(value);
                } catch {
                    return value;
                }
            } else {
                return value;
            }
        },

        setCookie: function setCookie(name, value) {
            if (!value) value = '';
            if (typeof value != 'string') throw "Cookie value must be a string! (Don't store JSON.stringify in cookie either!)";
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

        getLocalStorage: function getLocalStorage(key, value) {
            let finalKey = `${localStoragePrefix}${key}`;
            return lib.jsonParse(window.localStorage.getItem(finalKey) || 'null');
        },

        //  Stringify JSON while preserving Date formatting
        jsonStringify: function jsonStringifyWithDates(object) {
            if (typeof object == 'string')
                return object;

            if (object instanceof Date) {
                return object.toUTCString();
            }

            let result = lib.clone(object);
            lib.recursiveObjForEach(result, (prop, value, obj) => {
                if (value instanceof Date) {
                    obj[prop] = value.toUTCString();
                }
            });

            return JSON.stringify(result);
        },

        //  Parse JSON while checking for date formatting
        jsonParse: function jsonParseWithDates(json) {
            let stringIsDate = (str) => !!str.match(/^\w+,?\s+\d+\s+\w+\s+\d+\s+\d+:\d+:\d+/);

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

            let result = {};
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
                array.forEach((v) => result[keySelector(v)] = valueSelector(v));
            return result;
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

            for (var prop in object) {
                if (!object.hasOwnProperty(prop)) continue;
                if (typeof object[prop] == 'function') continue;

                callback.call(object, prop, object[prop], object, sourceObject_);

                if (typeof object[prop] == 'object') {
                    lib.recursiveObjForEach(object[prop], callback, sourceObject_ || object);
                }
            }
        },

        init: function init(callback) {
            lib.queryCurrentPlayerInfo((playerId, tribeId) => {
                authUserId = playerId;
                authTribeId = tribeId;

                callback();
            });
        }
    };

    //  Utility additions to String
    String.prototype.contains = function contains(str) {
        return this.indexOf(str) >= 0;
    };

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

    pageValidators[lib.pageTypes.UNKNOWN] = () => lib.getCurrentPage() == lib.pageTypes.UNKNOWN;

    //  URLs for each given page type
    let pageUrls = {};
    pageUrls[lib.pageTypes.VIEW_REPORT] = null; // there's no generic "view report" page, it's specific to each report
    pageUrls[lib.pageTypes.ALL_REPORTS] = 'screen=report&mode=all&group_id=-1';
    pageUrls[lib.pageTypes.INCOMINGS_OVERVIEW] = 'screen=overview_villages&mode=incomings&type=all&subtype=all&group=0&page=-1&subtype=all';
    pageUrls[lib.pageTypes.OWN_COMMANDS_OVERVIEW] = 'screen=overview_villages&mode=commands&type=all&group=0&page=-1&&type=all';
    pageUrls[lib.pageTypes.MAP] = 'screen=map';
    pageUrls[lib.pageTypes.OWN_TROOPS_OVERVIEW] = 'screen=overview_villages&mode=units&group=0&page=-1&type=complete';

    //  Make sure all page types have validators
    lib.objForEach(lib.pageTypes, (type) => !pageValidators[type] ? console.warn('No pageValidator set for pageType: ', type) : null);

    window._vlib = lib;
    return lib;

})();
