(() => {

    /*** Libraries ***/

    
    var lib = (() => {
    
        function getTwTroopStats() {
        
            var troopStats = {
        
                getUnit: function getUnitByCanonicalName(canonicalName) {
                    var result = null;
        
                    troopStats.unitTypes.forEach((t) => {
                        if (t.canonicalName == canonicalName)
                            result = t;
                    });
        
                    return result;
                },
        
                getUnitCommonName: function getUnitCommonName(canonicalName) {
                    var unit = troopStats.getUnit(canonicalName);
                    return unit ? unit.name : null;
                },
        
                unitClasses: [
                    { name: "Infantry" },
                    { name: "Cavalry" },
                    { name: "Archer" }
                ],
        
                resources: [
                    { name: "Wood" },
                    { name: "Clay" },
                    { name: "Iron" }
                ],
        
                recruitmentStructures: [
                    {
                        // https://help.tribalwars.net/wiki/Barracks
                        name: "Barracks",
                        canonicalName: "barracks",
                        maxLevel: 25,
                        timeScaling: [
                            0.63, 0.59, 0.56,
                            0.53, 0.50, 0.47,
                            0.44, 0.42, 0.39,
                            0.37, 0.35, 0.33,
                            0.31, 0.29, 0.28,
                            0.26, 0.25, 0.23,
                            0.22, 0.21, 0.20,
                            0.19, 0.17, 0.165,
                            0.16
                        ]
                    },
                    {
                        name: "Stable",
                        canonicalName: "stable",
                        maxLevel: 20,
                        timeScaling: [
                            0.63, 0.59, 0.56,
                            0.53, 0.50, 0.47,
                            0.44, 0.42, 0.39,
                            0.37, 0.35, 0.33,
                            0.31, 0.29, 0.28,
                            0.26, 0.25, 0.23,
                            0.22, 0.21
                        ]
                    },
                    {
                        // https://help.tribalwars.net/wiki/Workshop
                        name: "Workshop",
                        canonicalName: "garage",
                        maxLevel: 15,
                        timeScaling: [
                            0.63, 0.59, 0.56,
                            0.53, 0.50, 0.47,
                            0.44, 0.42, 0.39,
                            0.37, 0.35, 0.33,
                            0.31, 0.29, 0.28
                        ]
                    },
                    {
                        // https://help.tribalwars.net/wiki/Academy
                        name: "Academy",
                        canonicalName: "snob",
                        maxLevel: 3,
                        timeScaling: [
                            0.63, 0.59, 0.56
                        ]
                    }
                ],
        
                /* Unit training times pulled from: https://forum.tribalwars.net/index.php?threads/unit-training-times.139373/ */
        
                unitTypes: [
                    {
                        name: "Spear",
                        canonicalName: 'spear',
                        shorthand: "sp",
                        class: "Infantry",
                        attack: 10,
                        population: 1,
                        travelSpeed: 18,
                        build: 'Defensive',
                        training: { minutes: 2 + 39 / 60, source: "Barracks", sourceLevel: 25 },
                        defense: [{ class: "Infantry", value: 15 }, { class: "Cavalry", value: 45 }, { class: "Archer", value: 20 }],
                        cost: [{ resource: "Wood", amount: 50 }, { resource: "Clay", amount: 30 }, { resource: "Iron", amount: 10 }],
                        aliases: ['sp', 'spear fighter', 'spear fighters', 'spear', 'spears', 'spearman', 'spearmen'],
                        icon: "https://dsen.innogamescdn.com/8.100/34723/graphic/unit/recruit/spear.png"
                    },
                    {
                        name: "Sword",
                        canonicalName: 'sword',
                        shorthand: 'sw',
                        class: "Infantry",
                        attack: 25,
                        population: 1,
                        travelSpeed: 22,
                        build: 'Defensive',
                        training: { minutes: 3 + 53 / 60, source: "Barracks", sourceLevel: 25 },
                        defense: [{ class: "Infantry", value: 50 }, { class: "Cavalry", value: 15 }, { class: "Archer", value: 40 }],
                        cost: [{ resource: "Wood", amount: 30 }, { resource: "Clay", amount: 30 }, { resource: "Iron", amount: 70 }],
                        aliases: ['sw', 'sword', 'swords', 'swordsmen', 'swordsman'],
                        icon: "https://dsen.innogamescdn.com/8.100/34723/graphic/unit/recruit/sword.png"
                    },
                    {
                        name: "Axe",
                        canonicalName: 'axe',
                        shorthand: 'axe',
                        class: "Infantry",
                        attack: 40,
                        population: 1,
                        travelSpeed: 18,
                        build: 'Offensive',
                        training: { minutes: 3 + 26 / 60, source: "Barracks", sourceLevel: 25 },
                        defense: [{ class: "Infantry", value: 10 }, { class: "Cavalry", value: 5 }, { class: "Archer", value: 10 }],
                        cost: [{ resource: "Wood", amount: 60 }, { resource: "Clay", amount: 30 }, { resource: "Iron", amount: 40 }],
                        aliases: ['ax', 'axe', 'axes', 'axeman', 'axemen'],
                        icon: "https://dsen.innogamescdn.com/8.100/34723/graphic/unit/recruit/axe.png"
                    },
                    {
                        name: "Archer",
                        canonicalName: 'archer',
                        shorthand: 'ar',
                        class: "Archer",
                        attack: 15,
                        population: 1,
                        travelSpeed: 18,
                        build: 'Defensive',
                        training: { minutes: 4 + 40 / 60, source: "Barracks", sourceLevel: 25 },
                        defense: [{ class: "Infantry", value: 50 }, { class: "Cavalry", value: 40 }, { class: "Archer", value: 5 }],
                        cost: [{ resource: "Wood", amount: 100 }, { resource: "Clay", amount: 30 }, { resource: "Iron", amount: 60 }],
                        aliases: ['ar', 'archer', 'archers', 'arc'],
                        icon: "https://dsen.innogamescdn.com/8.100/34723/graphic/unit/recruit/archer.png"
                    },
                    {
                        name: "Scout",
                        canonicalName: 'spy',
                        shorthand: 'sc',
                        class: "Cavalry",
                        attack: 0,
                        population: 2,
                        travelSpeed: 9,
                        build: 'Offensive',
                        training: { minutes: 3 + 8 / 60, source: "Stable", sourceLevel: 20 },
                        defense: [{ class: "Infantry", value: 2 }, { class: "Cavalry", value: 1 }, { class: "Archer", value: 2 }],
                        cost: [{ resource: "Wood", amount: 50 }, { resource: "Clay", amount: 50 }, { resource: "Iron", amount: 20 }],
                        aliases: ['scout', 'scouts', 'spy', 'spies', 'sc'],
                        icon: "https://dsen.innogamescdn.com/8.100/34723/graphic/unit/recruit/spy.png"
                    },
                    {
                        name: "Light Cav.",
                        canonicalName: 'light',
                        shorthand: 'lc',
                        class: "Cavalry",
                        attack: 130,
                        population: 4,
                        travelSpeed: 10,
                        build: 'Offensive',
                        training: { minutes: 6 + 15 / 60, source: "Stable", sourceLevel: 20 },
                        defense: [{ class: "Infantry", value: 30 }, { class: "Cavalry", value: 40 }, { class: "Archer", value: 30 }],
                        cost: [{ resource: "Wood", amount: 125 }, { resource: "Clay", amount: 100 }, { resource: "Iron", amount: 250 }],
                        aliases: ['lc', 'light cavalry', 'light cav', 'light cav.', 'light'],
                        icon: "https://dsen.innogamescdn.com/8.100/34723/graphic/unit/recruit/light.png"
                    },
                    {
                        name: "Mounted Ar.",
                        canonicalName: 'marcher',
                        shorthand: 'ma',
                        class: "Archer",
                        attack: 120,
                        population: 5,
                        travelSpeed: 10,
                        build: 'Offensive',
                        training: { minutes: 9 + 22 / 60, source: "Stable", sourceLevel: 20 },
                        defense: [{ class: "Infantry", value: 40 }, { class: "Cavalry", value: 30 }, { class: "Archer", value: 50 }],
                        cost: [{ resource: "Wood", amount: 250 }, { resource: "Clay", amount: 100 }, { resource: "Iron", amount: 150 }],
                        aliases: ['ma', 'mounted ar', 'mounted archer', 'mounted archers', 'mounted', 'mount'],
                        icon: "https://dsen.innogamescdn.com/8.100/34723/graphic/unit/recruit/grey/marcher.png"
                    },
                    {
                        name: "Heavy Cav.",
                        canonicalName: 'heavy',
                        shorthand: 'hc',
                        class: "Cavalry",
                        attack: 150,
                        population: 6,
                        travelSpeed: 11,
                        build: 'Defensive',
                        training: { minutes: 12 + 19 / 60, source: "Stable", sourceLevel: 20 },
                        defense: [{ class: "Infantry", value: 200 }, { class: "Cavalry", value: 80 }, { class: "Archer", value: 180 }],
                        cost: [{ resource: "Wood", amount: 200 }, { resource: "Clay", amount: 150 }, { resource: "Iron", amount: 600 }],
                        aliases: ['hc', 'heavy cavalry', 'heavy cav', 'heavy'],
                        icon: "https://dsen.innogamescdn.com/8.100/34723/graphic/unit/recruit/heavy.png"
                    },
                    {
                        name: "Ram",
                        canonicalName: 'ram',
                        shorthand: 'ram',
                        class: "Infantry",
                        attack: 2,
                        population: 5,
                        travelSpeed: 30,
                        build: 'Offensive',
                        training: { minutes: 22 + 16 / 60, source: "Workshop", sourceLevel: 15 },
                        defense: [{ class: "Infantry", value: 20 }, { class: "Cavalry", value: 50 }, { class: "Archer", value: 20 }],
                        cost: [{ resource: "Wood", amount: 300 }, { resource: "Clay", amount: 200 }, { resource: "Iron", amount: 200 }],
                        aliases: ['ram', 'rams'],
                        icon: "https://dsen.innogamescdn.com/8.100/34723/graphic/unit/recruit/ram.png"
                    },
                    {
                        name: "Catapult",
                        canonicalName: 'catapult',
                        shorthand: 'cat',
                        class: "Infantry",
                        attack: 100,
                        population: 8,
                        travelSpeed: 30,
                        build: 'Offensive',
                        training: { minutes: 33 + 23 / 60, source: "Workshop", sourceLevel: 15 },
                        defense: [{ class: "Infantry", value: 100 }, { class: "Cavalry", value: 50 }, { class: "Archer", value: 100 }],
                        cost: [{ resource: "Wood", amount: 320 }, { resource: "Clay", amount: 400 }, { resource: "Iron", amount: 100 }],
                        aliases: ['cat', 'cats', 'catapault', 'catapaults', 'catapult', 'catapults'],
                        icon: "https://dsen.innogamescdn.com/8.100/34723/graphic/unit/recruit/catapult.png"
                    },
                    {
                        name: "Paladin",
                        canonicalName: 'knight',
                        shorthand: 'pally',
                        class: "Infantry",
                        attack: 150,
                        population: 10,
                        travelSpeed: 10,
                        build: 'Defensive',
                        defense: [{ class: "Infantry", value: 250 }, { class: "Cavalry", value: 400 }, { class: "Archer", value: 150 }],
                        cost: [{ resource: "Wood", amount: 20 }, { resource: "Clay", amount: 20 }, { resource: "Iron", amount: 40 }],
                        aliases: ['paladin', 'pallys', 'pally', 'paly', 'pal'],
                        icon: "https://dsen.innogamescdn.com/8.100/34797/graphic/unit/unit_knight.png"
                    },
                    {
                        name: "Nobleman",
                        canonicalName: 'snob',
                        shorthand: 'noble',
                        class: "Infantry",
                        attack: 30,
                        population: 100,
                        travelSpeed: 35,
                        build: 'Offensive',
                        training: { minutes: 1 * (60) + 34 + 12 / 60, source: "Academy", sourceLevel: 1 },
                        defense: [{ class: "Infantry", value: 100 }, { class: "Cavalry", value: 50 }, { class: "Archer", value: 100 }],
                        cost: [{ resource: "Wood", amount: 40000 }, { resource: "Clay", amount: 50000 }, { resource: "Iron", amount: 50000 }],
                        aliases: ['nobleman', 'noblemen', 'noble', 'nobles'],
                        icon: "https://dsen.innogamescdn.com/8.100/34797/graphic/unit/unit_snob.png"
                    },
                    {
                        name: "Militia",
                        canonicalName: 'militia',
                        shorthand: 'militia',
                        class: "Infantry",
                        attack: 0,
                        population: 0,
                        travelSpeed: 0,
                        defense: [{ class: "Infantry", value: 15 }, { class: "Cavalry", value: 45 }, { class: "Archer", value: 25 }],
                        cost: [{ resource: "Wood", amount: 0 }, { resource: "Clay", amount: 0 }, { resource: "Iron", amount: 0 }],
                        aliases: ['militia', 'militias', 'mil'],
                        icon: "https://dsen.innogamescdn.com/8.100/34797/graphic/unit/unit_militia.png"
                    }
                ]
            };
        
            return troopStats;
        }
    
        function makeTwCalc(twstats) {
        
            function normalizeToArray(objectOrArray) {
                if (!objectOrArray)
                    return [];
        
                if (objectOrArray instanceof Array)
                    return objectOrArray;
        
                let result = [];
                for (var prop in objectOrArray) {
                    if (!objectOrArray.hasOwnProperty(prop)) continue;
        
                    result.push({
                        name: prop, count: objectOrArray[prop]
                    });
                }
                return result;
            }
        
            return {
                totalPopulation: function calculateTotalPopulation(army) {
                    army = normalizeToArray(army);
                    var pop = 0;
                    army.forEach((t) => {
                        let name = t.name;
                        let count = t.count;
        
                        let unit = twstats.getUnit(name);
                        pop += unit.population * count;
                    });
                    return pop;
                },
        
                getDefensiveArmy: function getDefensiveArmy(army) {
                    army = normalizeToArray(army);
                    var result = [];
                    army.forEach((t) => {
                        let name = t.name;
                        let unit = twstats.getUnit(name);
        
                        if (unit.build == "Defensive")
                            result.push(t);
                    });
                    return result;
                },
        
                getOffensiveArmy: function getOffensiveArmy(army) {
                    army = normalizeToArray(army);
                    var result = [];
                    army.forEach((t) => {
                        let name = t.name;
                        let unit = twstats.getUnit(name);
        
                        if (unit.build == "Offensive")
                            result.push(t);
                    });
                    return result;
                }
            };
        }
    
        function makeVersioningSetup() {
            let currentVersion = '1.0.0';
            let lastVersionKey = 'version';
        
            return {
                getCurrentVersion() {
                    return currentVersion;
                },
        
                checkNeedsUpdate() {
                    let lastVersion = lib.getLocalStorage(lastVersionKey, null);
                    return lastVersion != currentVersion;
                },
        
                updateForLatestVersion() {
                    //  In the future we may want to do per-version migrations, ie if only map was changed then reset map settings
                    //  For now go nuclear and clear cached data
        
                    let cacheKeys = [
                        { key: 'reports-history', updater: () => [] },
                        { key: 'commands-history', updater: () => [] }
                    ];
        
                    cacheKeys.forEach((entry) => {
                        lib.setLocalStorage(entry.key, entry.updater());
                    });
        
                    lib.setLocalStorage(lastVersionKey, currentVersion);
                }
            };
        }
    
    
        let twstats = getTwTroopStats();
        let localStoragePrefix = 'vls-';
        let cookiePrefix = 'vc-';
    
        var storedScriptHost = null;
    
        var authToken = window.vaultToken || null;
        var authUserId = null;
        var authTribeId = null;
        var wasPageHandled = false;
    
        //  TODO - Pull this from server
        let worldSettings = {
            archersEnabled: false
        };
    
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
                MAP: null
            },
    
            messages: {
                TRIGGERED_CAPTCHA: 'Tribal wars Captcha was triggered, please refresh the page and try again. Any uploads will continue where they left off.',
                IS_IN_GROUP: "Your current village group isn't \"All\", please change to group \"All\".",
                FILTER_APPLIED: 'You have filters set for your reports, please remove them before uploading.'
            },
    
            errorCodes: {
                CAPTCHA: 'captcha',
                NOT_ALL_GROUP: 'group',
                FILTER_APPLIED: 'filter'
            },
    
            twstats: twstats,
            twcalc: makeTwCalc(twstats),
            versioning: makeVersioningSetup(),
    
            //  Gets the current server date and time from the page
            getServerDateTime: function getServerDateTime($doc_) {
                $doc_ = $doc_ || $(document);
                var $serverDate = $('#serverDate');
                var $serverTime = $('#serverTime');
    
                let fullString = `${$serverTime.text().trim()} ${$serverDate.text().trim()}`;
                return lib.parseTimeString(fullString);
            },
    
            //  Parses a variety of TW date/time formats to JS Date or into splits it into its parts
            //  returns Date if separated_ false or undefined
            //  returns { date: [day, month, year], time: [hour, minute, second, millisecond] } if separated_ is true
            parseTimeString: function parseTimeString(timeString, separated_, $doc_) {
                if (typeof separated_ == 'undefined')
                    separated_ = false;
    
                $doc_ = $doc_ || $(document);
    
                var monthStrings = [
                    'jan', 'feb', 'mar', 'apr', 'may', 'jun',
                    'jul', 'aug', 'sep', 'oct', 'nov', 'dec'
                ];
    
                var result;
    
                let dateSeparators = ['/', '.'];
                let timeSeparators = [':', '.'];
                let dateSeparatorsStr = dateSeparators.map((s) => `\\${s}`).join('');
                let timeSeparatorsStr = timeSeparators.map((s) => `\\${s}`).join('');
                let dateRegex = `(\\d+[${dateSeparatorsStr}]\\d+(?:[${dateSeparatorsStr}]\\d+)?)\\.?`;
                let timeRegex = `(\\d+[${timeSeparatorsStr}]\\d+(?:[${timeSeparatorsStr}]\\d+)?(?:[${timeSeparatorsStr}]\\d+)?)`;
    
                var serverDate = $doc_.find('#serverDate').text().split('/');
    
                var match;
                if (match = timeString.match(new RegExp(`${timeRegex} ${dateRegex}`))) {
                    //  Hour:Minute:Second:Ms Day/Month/Year
                    result = {
                        time: match[1].splitMany(timeSeparators),
                        date: match[2].splitMany(dateSeparators)
                    };
    
                } else if (match = timeString.match(new RegExp(`${dateRegex} ${timeRegex}`))) {
                    //  Day/Month/Year Hour:Minute:Second:Ms
                    result = {
                        date: match[1].splitMany(dateSeparators),
                        time: match[2].splitMany(timeSeparators)
                    };
    
                } else if (match = timeString.match(new RegExp(`((?:${monthStrings.join('|')}))\\.?\\s+(\\d+),\\s+(?:(\\d+)\\s+)?${timeRegex}`, 'i'))) {
                    //  (Mon.) Day, Year Hour:Minute:Second:Ms
                    var monthName = match[1];
                    var day = match[2];
                    var year = match[3] || serverDate[2];
                    var month = (monthStrings.indexOf(monthName.toLowerCase()) + 1).toString();
    
                    result = {
                        date: [day, month, year],
                        time: match[4].splitMany(timeSeparators)
                    };
    
                } else if (match = timeString.match(new RegExp(`today at\\s+${timeRegex}`))) {
                    // today at (Hours:Minute:Second:Ms)
                    result = {
                        date: serverDate,
                        time: match[1].splitMany(timeSeparators)
                    }
    
                } else if (match = timeString.match(new RegExp(`tomorrow at\\s+${timeRegex}`))) {
                    // tomorrow at (Hours:Minute:Second:Ms)
                    result = {
                        date: [
                            (parseInt(serverDate[0]) + 1).toString(),
                            (parseInt(serverDate[1])).toString(),
                            serverDate[2]
                        ],
                        time: match[1].splitMany(timeSeparators)
                    };
    
                } else if (match = timeString.match(new RegExp(`${timeRegex} on ${dateRegex}`))) {
                    // (Hours:Minutes:Seconds:Ms) on (Day/Month/Year)
                    result = {
                        date: match[2].splitMany(dateSeparators),
                        time: match[1].splitMany(timeSeparators)
                    };
    
                    //  TODO - Update this one
                } else if (match = timeString.match(/on (\d+[\/\.]\d+(?:[\/\.](?:\d+)?)?)\s+at\s+(\d+:\d+:\d+:\d+)/)) {
                    // on (Day/Month/Year) at (Hours:Minute:Second:Ms)
                    result = {
                        date: match[1].splitMany(dateSeparators),
                        time: match[2].splitMany(timeSeparators)
                    };
                    
                    if (!result.date[2]) {
                        result.date[2] = serverDate[2];
                    }
    
                } else {
                    result = null;
                }
    
                if (result == null) {
                    return null;
                }
    
                if (result.date[2] && result.date[2].length == 2) {
                    result.date[2] = '20' + result.date[2];
                }
    
                result.date.forEach((val, i) => result.date[i] = parseInt(val));
                result.time.forEach((val, i) => result.time[i] = parseInt(val));
    
                result.date[2] = result.date[2] || parseInt(serverDate[2]);
                result.time[2] = result.time[2] || 0;
                result.time[3] = result.time[3] || 0;
    
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
    
            onFirstRun: function (callback) {
                var hasRan = lib.getLocalStorage('consented');
                let approvedRunCallback = () => lib.setLocalStorage('consented', true);
                if (!hasRan) {
                    callback(approvedRunCallback);
                }
    
                return !hasRan;
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
    
            getApi: function getApi(url, data_) {
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
    
            absoluteTwUrl: function formatToAbsoluteTwUrl(url) {
                if (!url.startsWith(window.location.origin)) {
                    if (!url.startsWith('/'))
                        url = '/' + url;
                    url = window.location.origin + url;
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
                return !!$('.menu-column-item a[href*=quickbar]').length;
            },
    
            troopsArrayToObject: function troopsArrayToNamedObject(array) {
                let result = {};
                let archerIndex = 3, mountedArcherIndex = 6;
                for (var i = 0, ai = 0; ai < array.length && i < lib.twstats.unitTypes.length; i++ , ai++) {
                    if ((i == archerIndex || i == mountedArcherIndex) && !worldSettings.archersEnabled) {
                        --ai;
                        continue;
                    }
    
                    result[lib.twstats.unitTypes[i].canonicalName] = array[ai];
                }
                return result;
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
    
                return sourceUrl || 'https://v.tylercamp.me/script/main.js';
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
                        return lib.jsonParse(stored);
                    } catch (_) {
                        return stored;
                    }
                }
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
    
        //  Utility additions to Math
        Math.roundTo = function roundTo(val, precision) {
            var divisor = Math.pow(10, precision);
            return Math.round(val * divisor) / divisor;
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
    
        //  Make sure all page types have validators
        lib.objForEach(lib.pageTypes, (type) => !pageValidators[type] ? console.warn('No pageValidator set for pageType: ', type) : null);
    
        return lib;
    
    })();
    

    let twtypes = (() => {
    
    
    
        var troopCanonicalToReadableMap = {
            'spear': 'Spear',
            'sword': 'Sword',
            'axe': 'Axe',
            'archer': 'Archer',
            'spy': 'Scout',
            'light': 'LC',
            'marcher': 'MA',
            'heavy': 'HC',
            'ram': 'Ram',
            'catapult': 'Catapult',
            'knight': 'Paladin',
            'snob': 'Nobleman',
            'militia': 'Militia'
        };
    
        var buildingCanonicalToReadableMap = {
            'main': 'Headquarters',
            'barracks': 'Barracks',
            'stable': 'Stable',
            'garage': 'Workshop',
            'snob': 'Academy',
            'smith': 'Smithy',
            'place': 'Rally point',
            'market': 'Market',
            'wood': 'Timber camp',
            'stone': 'Clay pit',
            'iron': 'Iron mine',
            'farm': 'Farm',
            'storage': 'Warehouse',
            'hide': 'Hiding place',
            'wall': 'Wall',
            'church': 'Church',
            'watchtower': 'Watchtower'
        };
    
        var troopReadableToCanonicalMap = lib.mapObject(troopCanonicalToReadableMap, (_, value) => value, (prop, _) => prop);
        var buildingReadableToCanonicalMap = lib.mapObject(buildingCanonicalToReadableMap, (_, value) => value, (prop, _) => prop);
    
        return {
            canonicalTroopNames: lib.objectToArray(troopCanonicalToReadableMap, (val, prop) => prop),
            canonicalBuildingNames: lib.objectToArray(buildingCanonicalToReadableMap, (val, prop) => prop),
    
            buildingCanonicalToReadable: (canonical) => buildingCanonicalToReadableMap[canonical],
            buildingReadableToCanonical: (readable) => buildingReadableToCanonicalMap[readable],
    
            troopCanonicalToReadable: (canonical) => troopCanonicalToReadableMap[canonical],
            troopReadableToCanonical: (readable) => troopReadableToCanonicalMap[readable]
        };
    
    })();

    
    
    function RateLimiter() {
        this.pendingTasks = [];
        this.maxTasksPerSecond = 5;
        this.refreshDelay = 100;
    
        this._interval = null;
        this._onFinishedHandler = null;
        this._taskTimes = [];
    
        this.resetStats();
    }
    
    RateLimiter.prototype.start = function () {
        if (this._interval) {
            return;
        }
    
        var self = this;
        var numWorking = 0;
    
        this._interval = setInterval(() => {
            var now = new Date();
            for (var i = 0; i < self._taskTimes.length; i++) {
                if (now.valueOf() - self._taskTimes[i].valueOf() >= 1000) {
                    self._taskTimes.splice(i, 1);
                    --i;
                }
            }
    
            if (self._taskTimes.length < self.maxTasksPerSecond && self.pendingTasks.length > 0) {
                (() => {
                    var task = self.pendingTasks[0];
                    self.pendingTasks.splice(0, 1);
                    self._taskTimes.push(new Date());
    
                    numWorking++;
    
                    let doneIndicator = task();
                    if (!doneIndicator) {
                        self.stats.done++;
                        numWorking--;
    
                        if (!self.pendingTasks.length && numWorking == 0) {
                            self._onFinishedHandler && self._onFinishedHandler();
                        }
                    } else {
                        doneIndicator(() => {
                            numWorking--;
    
                            if (!self.pendingTasks.length && numWorking == 0) {
                                self._onFinishedHandler && self._onFinishedHandler();
                            }
                        });
                    }
                })();
            }
        }, this.refreshDelay);
    };
    
    RateLimiter.prototype.stop = function () {
        this._interval && clearInterval(this._interval);
        this._interval = null;
    };
    
    RateLimiter.prototype.isRunning = function () {
        return this._interval != null;
    }
    
    RateLimiter.prototype.addTask = function (task) {
        this.stats.total++;
        this.pendingTasks.push(task);
    };
    
    RateLimiter.prototype.setFinishedHandler = function (callback) {
        this._onFinishedHandler = callback;
    };
    
    RateLimiter.prototype.getStats = function () {
        return this.stats;
    };
    
    RateLimiter.prototype.hasTasks = function () {
        return this.pendingTasks.length > 0;
    };
    
    RateLimiter.prototype.resetStats = function () {
        this.pendingTasks = [];
        this._taskTimes = [];
        this.stats = {
            done: 0,
            pending: 0,
            total: 0,
            numFailed: 0
        };
    };

    
    //  TODO - Remove duplicate logic by using RateLimiter internally
    
    function RequestManager() {
        this.pendingRequests = [];
        this.errorHistory = {};
        this.maxRequestsPerSecond = 5;
        this.refreshDelay = 100;
    
        this._urlHistory = {};
    
        this._interval = null;
        this._onFinishedHandler = null;
        this._hasErrors = false;
        this._requestTimes = [];
    
        this.resetStats();
    }
    
    RequestManager.prototype.start = function () {
        if (this._interval) {
            return;
        }
    
        var self = this;
        var numResponding = 0;
    
        this._interval = setInterval(() => {
            var now = new Date();
            for (var i = 0; i < self._requestTimes.length; i++) {
                if (now.valueOf() - self._requestTimes[i].valueOf() >= 1000) {
                    self._requestTimes.splice(i, 1);
                    --i;
                }
            }
    
            if (self._requestTimes.length < self.maxRequestsPerSecond && self.pendingRequests.length > 0) {
                (() => {
                    var request = self.pendingRequests[0];
                    self.pendingRequests.splice(0, 1);
    
                    if (request.numErrors >= 3) {
                        self.stats.done++;
                        return;
                    }
    
                    if (request.beforeRun && !request.beforeRun(request)) {
                        if (!self.pendingRequests.length && numResponding == 0) {
                            self._onFinishedHandler && self._onFinishedHandler();
                        }
                        self.stats.done++;
                        return;
                    }
    
                    self._requestTimes.push(new Date());
    
                    console.log('Getting ', request);
                    numResponding++;
    
                    $.ajax(request)
                        .done((data, result) => {
                            self.stats.done++;
                            var numCompleted = self.stats.done + "/" + self.stats.total;
                            console.log(numCompleted);
                            numResponding--;
    
                            var err;
                            try {
                                request.onDone && request.onDone(data, request);
                            } catch (e) {
                                self.stats.numFailed++;
                                err = e;
                            }
    
                            if (!self.pendingRequests.length && numResponding == 0) {
                                self._onFinishedHandler && self._onFinishedHandler();
                            }
    
                            if (err) {
                                throw err;
                            }
                        })
                        .fail(() => {
                            console.log('error, requeueing', arguments);
                            request.numErrors++;
                            if (request.numErrors < 3) {
                                self.pendingRequests.push(request);
                                numResponding--;
                            } else {
                                console.log('request failed too many times, stopping requests for ', request);
                                numResponding--;
                                self._hasErrors = true;
                                self.stats.numErrors++;
    
                                request.onDone && request.onDone(null, request);
    
                                if (!self.pendingRequests.length && numResponding == 0) {
                                    self._onFinishedHandler && self._onFinishedHandler();
                                }
                            }
                        });
                })();
            }
        }, this.refreshDelay);
    };
    
    RequestManager.prototype.stop = function () {
        this._interval && clearInterval(this._interval);
        this._interval = null;
    };
    
    RequestManager.prototype.isRunning = function () {
        return this._interval != null;
    }
    
    RequestManager.prototype.addRequest = function (url, callback, beforeRunCallback_) {
        if (this._urlHistory[url]) {
            console.log('Duplicate URL request!');
            debugger;
        } else {
            this._urlHistory[url] = true;
        }
    
        this.stats.total++;
        this.pendingRequests.push({
            url: url,
            onDone: callback,
            numErrors: 0,
            beforeRun: beforeRunCallback_
        });
    };
    
    RequestManager.prototype.setFinishedHandler = function (callback) {
        this._onFinishedHandler = callback;
    };
    
    RequestManager.prototype.getStats = function () {
        return this.stats;
    };
    
    RequestManager.prototype.hasRequests = function () {
        return this.pendingRequests.length > 0;
    };
    
    RequestManager.prototype.hasErrors = function () {
        return this._hasErrors;
    };
    
    RequestManager.prototype.resetStats = function () {
        this.stats = {
            done: 0,
            pending: 0,
            total: 0,
            numFailed: 0
        };
    };

    function CsvBuilder() {
        this.rows = [];
    }
    
    CsvBuilder.prototype.addRow = function addRow(...data) {
        this.rows.push(data);
    };
    
    CsvBuilder.prototype.addBlank = function addBlank(numLines_) {
        numLines_ = numLines_ || 1;
        for (var i = 0; i < numLines_; i++)
            this.rows.push([]);
    };
    
    CsvBuilder.prototype.makeCsvString = function makeCsvString() {
        var numColumns = 0;
        this.rows.forEach((r) => numColumns = Math.max(numColumns, r.length));
    
        var csvRows = [];
        this.rows.forEach((row) => {
            row = row.slice();
            for (var i = 0; i < row.length - numColumns; i++)
                row.push('');
            csvRows.push(row.join(','));
        });
        return csvRows.join('\n');
    };



    /***  Page Parsing ***/
    function parseAllReports($doc, onProgress_, onDone_) {
        $doc = $doc || $(document);
    
        //  Returns links to all battle reports on the current page
        function parseReportsOverviewPage($doc) {
        
            $doc = $doc || $(document);
        
            lib.ensurePage(lib.pageTypes.ALL_REPORTS);
        
            var requestManager = new RequestManager();
            
            let hasFilters = checkHasFilters();
            console.log('hasFilters = ', hasFilters);
            let pages = lib.detectMultiPages($doc);
            console.log('pages = ', pages);
        
            let reportLinks = [];
            let ignoredReports = [];
            let serverTime = lib.getServerDateTime();
            const maxReportAgeDays = 14;
            const maxReportAgeMs = maxReportAgeDays * 24 * 60 * 60 * 1000;
        
            let $reportLinks = $doc.find('#report_list tr:not(:first-child):not(:last-child) a:not(.rename-icon)');
            $reportLinks.each((i, el) => {
                let $el = $(el);
        
                let link = $el.prop('href');
                let landingTimeText = $el.closest('tr').find('td:last-of-type').text();
                let landingTime = lib.parseTimeString(landingTimeText);
                let reportId = parseInt(link.match(/view=(\w+)/)[1]);
                let $icon = $el.closest('td').find('img:first-of-type');
        
                let timeSinceReport = serverTime.valueOf() - landingTime.valueOf();
                if (timeSinceReport >= maxReportAgeMs) {
                    let ageDays = timeSinceReport / 24 / 60 / 60 / 1000;
                    ignoredReports.push({ reportId: reportId, ageDays: Math.roundTo(ageDays, 2) });
                    //console.log(`Report ${reportId} is ${Math.roundTo(ageDays, 2)} days old, skipping`);
                    return;
                }
        
                var isBattleReport = false;
                $icon.each((_, el) => {
                    let icon = $(el).attr('src');
                    if (icon.contains("/dots/") || icon.contains("attack"))
                        isBattleReport = true;
                });
        
                if ($el.text().contains("Your support from"))
                    isBattleReport = false;
        
                if (!isBattleReport)
                    return;
        
                reportLinks.push({
                    reportId: reportId,
                    link: link
                });
            });
        
            console.log('Ignored ' + ignoredReports.length + ' reports for being over ' + maxReportAgeDays + ' days old');
        
            return reportLinks;
        };
    
        
        function parseReportPage($doc, href_, showNotice_, onError_) {
            
            lib.ensurePage(lib.pageTypes.VIEW_REPORT);
            $doc = $doc || $(document);
            let href = href_ || window.location.href;
            if (typeof showNotice_ == 'undefined')
                showNotice_ = true; // Show "complete/error" notice by default
        
            var $attackInfo = $doc.find('#attack_info_att')
            var $defenseInfo = $doc.find('#attack_info_def')
            var defendingPlayer = $defenseInfo.find('a[href*=info_player]');
            var attackingPlayer = $attackInfo.find('a[href*=info_player]');
            var building_to_canonical_name = {"Headquarters":"main", "Barracks":"barracks", "Stable":"stable", "Workshop":"garage", "Academy":"snob", "Smithy":"smith", "Rally Point":"place", "Statue":"statue", "Market":"market", "Timber camp":"wood", "Clay pit":"stone", "Iron mine":"iron", "Farm":"farm", "Warehouse":"storage", "Hiding place":"hide", "Wall":"wall", "Watchtower":"watchtower", "Church":"church", "First church":"first_church"}; //not sure about Watchtower and Church entries
            var reportInfo = {};
            reportInfo.reportId = parseInt(href.match(/view=(\d+)/)[1]);
            console.log('Processing report ' + reportInfo.reportId);
        
            if (!$attackInfo.length) {
                if (showNotice_) {
                    alert("This kind of report can't be uploaded!");
                }
                return;
            }
            
            reportInfo.luck = parseInt($doc.find('#attack_luck').text().match(/(\-?\d.+)\%/)[1]);
            reportInfo.morale = parseInt($doc.find('.report_ReportAttack h4:nth-of-type(2)').text().match(/(\d+)\%/)[1]);
        
            var loyalty = $doc.find('#attack_results tr').filter((i, el) => $(el).text().indexOf('Loyalty') >= 0).text().match(/from\s+\d+\s+to\s+(\-?\d+)/);
            if (loyalty)
                reportInfo.loyalty = parseInt(loyalty[1]);
        
        
            var occurredAt = $doc.find('td.nopad > .vis:nth-of-type(2) > tbody > tr:nth-of-type(2) td:nth-of-type(2)').text().trim();
            reportInfo.occurredAt = lib.parseTimeString(occurredAt).toUTCString();
        
            //  Get attacker player
            if (attackingPlayer.length)
                reportInfo.attackingPlayerId = parseInt(attackingPlayer.prop('href').match(/id=(\w+)/)[1]);
        
            //  Get attacker village
            reportInfo.attackingVillageId = parseInt($attackInfo.find('a[href*=info_village]').prop('href').match(/id=(\w+)/)[1])
        
            //  Get attacker units
            reportInfo.attackingArmy = $attackInfo.find('#attack_info_att_units tr:nth-of-type(2) .unit-item').get().map((el) => { return { type: $(el).prop('class').match(/unit-item-([\w\-]+)/)[1], count: parseInt($(el).text().trim()) } })
        
            //  Get attacker losses
            reportInfo.attackingArmyLosses = $attackInfo.find('#attack_info_att_units tr:nth-of-type(3) .unit-item').get().map((el) => { return { type: $(el).prop('class').match(/unit-item-([\w\-]+)/)[1], count: parseInt($(el).text().trim()) } })
        
        
            if (defendingPlayer.length)
                reportInfo.defendingPlayerId = parseInt(defendingPlayer.prop('href').match(/id=(\w+)/)[1]);
        
            //  Get defender village
            reportInfo.defendingVillageId = parseInt($defenseInfo.find('a[href*=info_village]').prop('href').match(/id=(\w+)/)[1])
        
            //  Get defender units
            reportInfo.defendingArmy = $defenseInfo.find('#attack_info_def_units tr:nth-of-type(2) .unit-item').get().map((el) => { return { type: $(el).prop('class').match(/unit-item-([\w\-]+)/)[1], count: parseInt($(el).text().trim()) } })
        
            //  Get defender losses
            reportInfo.defendingArmyLosses = $defenseInfo.find('#attack_info_def_units tr:nth-of-type(3) .unit-item').get().map((el) => { return { type: $(el).prop('class').match(/unit-item-([\w\-]+)/)[1], count: parseInt($(el).text().trim()) } })
        
            let $travelingTroopsContainer = $doc.find('#attack_spy_away');
            if ($travelingTroopsContainer.length) {
                reportInfo.travelingTroops = {};
        
                $travelingTroopsContainer.find('.unit-item').each((i, el) => {
                    let $el = $(el);
                    let cls = $el.prop('class');
                    let unitType = cls.match(/unit\-item\-(\w+)/)[1];
                    reportInfo.travelingTroops[unitType] = parseInt($el.text().trim());
                });
            }
        
            //  Defender village info
            reportInfo.buildingLevels = JSON.parse($doc.find('#attack_spy_building_data').val() || 'null')
        
        
            function itemListToDictionary(list, nameSelector, valueSelector) {
                if (list == null || list.length == 0)
                    return null;
        
                var result = {};
                list.forEach(v => result[nameSelector(v)] = valueSelector(v));
                return result;
            }
        
            let troopListToDictionary = (troopList) => itemListToDictionary(troopList, (t) => t.type, (t) => t.count);
            let buildingsListToDictionary = (buildingsList) => itemListToDictionary(buildingsList, (b) => b.id, b => b.level);
        
            reportInfo.attackingArmy = troopListToDictionary(reportInfo.attackingArmy);
            reportInfo.attackingArmyLosses = troopListToDictionary(reportInfo.attackingArmyLosses);
            reportInfo.defendingArmy = troopListToDictionary(reportInfo.defendingArmy);
            reportInfo.defendingArmyLosses = troopListToDictionary(reportInfo.defendingArmyLosses);
        
            // TODO reportInfo.travelingTroops = troopListToDictionary(reportInfo.travelingTroops);
        
            reportInfo.buildingLevels = buildingsListToDictionary(reportInfo.buildingLevels);
        
        
        
            //  ram/cat damage
            if (reportInfo.buildingLevels == null) {
                var attack_results = null;
                if (attack_results = $doc.find('#attack_results').text()) {
                    reportInfo.buildingLevels = {};
                    var building_names = attack_results.match(/The (.*) has/g);
                    var building_levels = attack_results.match(/to level (.*)/g);
                    if(building_names) {
                        for (i=0; i < building_names.length; i++) {
                            reportInfo.buildingLevels[building_to_canonical_name[building_names[i].split(" ")[1]]] = parseInt(building_levels[i].split(" ")[2]);
                        }
                    }
                }
            }
            
        
            let reportsEndpoint = lib.makeApiUrl('/report');
            console.log('Made reportInfo: ', reportInfo);
        
            lib.postApi(reportsEndpoint, reportInfo)
                .done(() => {
                    var reportsHistory = lib.getLocalStorage('reports-history', []);
                    reportsHistory.push(reportInfo.reportId);
                    lib.setLocalStorage('reports-history', reportsHistory);
        
                    if (showNotice_)
                        alert('Uploaded the report!');
                })
                .fail((req, status, err) => {
                    if (showNotice_)
                        alert('An error occurred...');
                    console.error('POST request failed: ', req, status, err);
                    if (onError_)
                        onError_();
                });
        }
        
    
        
        var requestManager = new RequestManager();
    
        let previousReports = lib.getLocalStorage('reports-history', []);
    
        let hasFilters = checkHasFilters();
        console.log('hasFilters = ', hasFilters);
    
        if (hasFilters) {
            if (onProgress_)
                onProgress_(lib.messages.FILTER_APPLIED);
            else
                alert(lib.messages.FILTER_APPLIED);
    
            if (onDone_) {
                onDone_(lib.errorCodes.FILTER_APPLIED);
            }
            return;
        }
    
        let reportLinks = [];
    
        onProgress_ && onProgress_('Collecting report pages...');
        let pages = lib.detectMultiPages($doc);
        pages.push(lib.makeTwUrl(lib.pageTypes.ALL_REPORTS));
        console.log('pages = ', pages);
    
        collectReportLinks();
    
    
        function collectReportLinks() {
            let collectingReportLinksMessage = 'Collecting report links...';
            onProgress_ && onProgress_(collectingReportLinksMessage);
    
            pages.forEach((link) => {
                requestManager.addRequest(link, (data) => {
                    onProgress_ && onProgress_(`${collectingReportLinksMessage} (page ${requestManager.getStats().done}/${pages.length})`);
    
                    if (lib.checkContainsCaptcha(data)) {
                        if (requestManager.isRunning()) {
                            requestManager.stop();
    
                            if (onProgress_)
                                onProgress_(lib.messages.TRIGGERED_CAPTCHA);
                            else
                                alert(lib.messages.TRIGGERED_CAPTCHA);
    
                            onDone_ && onDone_(lib.errorCodes.CAPTCHA);
                        }
    
                        return;
                    }
    
                    let $pageDoc = $(data);
                    let pageLinks = parseReportsOverviewPage($pageDoc);
                    console.log('Got page links: ', pageLinks);
                    reportLinks.push(...pageLinks);
                });
            });
    
            requestManager.setFinishedHandler(() => {
                requestManager.stop();
                console.log('Got all page links: ', reportLinks);
                let filteredReports = reportLinks.except((l) => previousReports.contains(l.reportId));
    
                onProgress_ && onProgress_('Checking for reports already uploaded...');
                getExistingReports(filteredReports.map(r => r.reportId), (existing) => {
                    console.log('Got existing reports: ', existing);
    
                    previousReports.push(...existing);
                    let withoutMissingReports = previousReports.except((r) => !reportLinks.contains((l) => l.reportId == r));
                    console.log('Updated reports cache without missing reports: ', withoutMissingReports);
                    lib.setLocalStorage('reports-history', withoutMissingReports);
    
                    let filteredLinks =
                        reportLinks.except((l) => previousReports.contains(l.reportId))
                            .map((l) => l.link)
                            .distinct();
    
                    console.log('Made filtered links: ', filteredLinks);
    
                    uploadReports(filteredLinks);
                });
            });
    
            requestManager.start();
        }
    
        function getExistingReports(reportIds, onDone) {
            lib.postApi(lib.makeApiUrl('report/check-existing-reports'), reportIds)
                .done((data) => {
                    if (typeof data == 'string')
                        data = JSON.parse(data);
                    if (data.length) {
                        onProgress_ && onProgress_('Found ' + data.length + ' previously uploaded reports, skipping these...');
                        setTimeout(() => onDone(data), 2000);
                    } else {
                        onDone(data);
                    }
                })
                .error(() => {
                    onProgress_ && onProgress_('An error occurred while checking for existing reports, continuing...');
                    setTimeout(() => onDone([]), 2000);
                });
        }
    
        function uploadReports(reportLinks) {
            requestManager.resetStats();
    
            reportLinks.forEach((link) => {
                requestManager.addRequest(link, (data, request) => {
                    if (data) {
                        if (lib.checkContainsCaptcha(data)) {
    
                            if (requestManager.isRunning()) {
                                requestManager.stop();
                                
                                if (onProgress_)
                                    onProgress_(lib.messages.TRIGGERED_CAPTCHA);
    
                                if (onDone_)
                                    onDone_(lib.errorCodes.CAPTCHA);
                                else
                                    alert(lib.messages.TRIGGERED_CAPTCHA);
                            }
    
                            return;
                        }
    
                        let $doc = $(data);
                        try {
                            parseReportPage($doc, link, false, () => {
                                //  onError
                                requestManager.getStats().numFailed++;
                                //toggleReport($el, false);
                            });
                        } catch (e) {
                            requestManager.getStats().numFailed++;
                            console.log(e);
                        }
                        //toggleReport($el);
                    }
    
                    updateUploadsDisplay();
                });
            });
    
            requestManager.setFinishedHandler(() => {
                let stats = requestManager.getStats();
    
                let statusMessage = `Finished: ${stats.done}/${stats.total} uploaded, ${stats.numFailed} failed.`;
                if (onProgress_)
                    onProgress_(statusMessage);
    
                if (!onDone_) {
                    alert('Done!');
                    let stats = requestManager.getStats();
                    setUploadsDisplay(statusMessage);
                } else {
                    onDone_(false);
                }
            });
    
            if (!requestManager.getStats().total) {
                lib.postApi(lib.makeApiUrl('report/finished-report-uploads'));
    
                if (!onDone_) {
                    setUploadsDisplay('No new reports to upload.');
                    alert('No new reports to upload!');
                } else {
                    if (onProgress_)
                        onProgress_('Finished: No new reports to upload.');
                    if (onDone_)
                        onDone_(false);
                }
            } else {
                requestManager.start();
            }
        }
    
        makeUploadsDisplay();
    
        function makeUploadsDisplay() {
            if (onDone_ || onProgress_)
                return;
    
            $('#vault-uploads-display').remove();
    
            let $uploadsContainer = $('<div id="vault-uploads-display">');
            $doc.find('#report_list').parent().prepend($uploadsContainer);
            updateUploadsDisplay();
        }
    
        function updateUploadsDisplay() {
            let stats = requestManager.getStats();
            let statusMessage = `Uploading ${stats.total} reports... (${stats.done} done, ${stats.numFailed} failed.)`;
    
            if (!onProgress_) {
                setUploadsDisplay(statusMessage);
            } else {
                onProgress_(statusMessage);
            }
        }
    
        function setUploadsDisplay(contents) {
            if (onDone_ || onProgress_)
                return;
    
            let $uploadsContainer = $doc.find('#vault-uploads-display');
            $uploadsContainer.text(contents);
        }
    
        function toggleReport($link, checked_) {
            if (onDone_ || onProgress_)
                return;
    
            if (typeof checked_ == 'undefined')
                checked_ = true;
    
            $link.closest('tr').find('td:first-of-type input').prop('checked', checked_);
        }
    
        function checkHasFilters() {
            let $filters = $doc.find('.report_filter');
            var hasFilters = false;
    
            let textFilter = $filters.find('input[type=text]').val();
            if (textFilter != null && textFilter.length > 0) {
                console.log('Text filter not empty');
                hasFilters = true;
            }
    
            let $checkedBoxes = $filters.find('input[type=checkbox]:checked');
            if ($checkedBoxes.length) {
                console.log('Checked boxes: ', $checkedBoxes);
                hasFilters = true;
            }
    
            let $checkedRadios = $filters.find('input[type=radio]:not([value=0]):checked');
            if ($checkedRadios.length) {
                console.log('Checked radios: ', $checkedRadios);
                hasFilters = true;
            }
    
            return hasFilters;
        }
    }

    function parseAllIncomings($doc, onProgress_, onDone_) {
    
        function parseUploadIncomingsOverviewPage($doc) {
            $doc = $doc || $(document);
        
            let $incomingRows = $doc.find('#incomings_table tr:not(:first-of-type):not(:last-of-type)');
        
            let commandsData = [];
        
            //  In matching priority
            let troopNames = [
                { name: 'ram', aliases: ['rams'] },
                { name: 'catapult', aliases: ['catapults', 'cat.'] },
                { name: 'spear', aliases: [] },
                { name: 'sword', aliases: [] },
                { name: 'axe', aliases: [] },
                { name: 'archer', aliases: [] },
                { name: 'spy', aliases: ['scout'] },
                { name: 'light', aliases: [ 'lcav'] },
                { name: 'marcher', aliases: ['mount archer'] },
                { name: 'heavy', aliases: [ 'hcav' ] },
                { name: 'snob', aliases: ['noble', 'nobleman'] },
                { name: 'knight', aliases: ['paladin', 'pally'] }
            ];
        
            $incomingRows.each((i, el) => {
                let $el = $(el);
        
                let label = $el.find('td:nth-of-type(1)').text().trim().toLowerCase();
                let troopType = (() => {            
                    var type = null;
        
                    troopNames.forEach((obj) => {
                        let canonicalName = obj.name;
                        let aliases = obj.aliases;
        
                        [canonicalName, ...aliases].forEach((name) => {
                            if (type)
                                return;
        
                            if (label.contains(name)) {
                                type = canonicalName;
                            }
                        });
                    });
        
                    return type;
                })();
        
                var commandType = $el.find('td:nth-of-type(1) span:nth-of-type(1) img').prop('src').match(/\/(\w+)\.png/)[1];
                let commandId = $el.find('input[name^=id_]').prop('name').match(/id_(\w+)/)[1];
                let targetVillageId = $el.find('td:nth-of-type(2) a').prop('href').match(/village\=(\w+)/)[1];
                let sourceVillageId = $el.find('td:nth-of-type(3) a').prop('href').match(/\&id\=(\w+)/)[1];
                let sourcePlayerId = $el.find('td:nth-of-type(4) a').prop('href').match(/\&id\=(\w+)/)[1];
                let arrivalTimeText = $el.find('td:nth-of-type(6)').text().trim();
        
                let arrivalTime = lib.parseTimeString(arrivalTimeText);
        
                if (commandType.contains("_"))
                    commandType = commandType.substr(0, commandType.indexOf('_'));
        
                commandsData.push({
                    commandId: parseInt(commandId),
                    sourceVillageId: parseInt(sourceVillageId),
                    sourcePlayerId: parseInt(sourcePlayerId),
                    targetVillageId: parseInt(targetVillageId),
                    targetPlayerId: null,
                    landsAt: arrivalTime.toUTCString(),
                    commandType: commandType,
                    troopType: troopType,
                    isReturning: false,
                    userLabel: label
                });
            });
        
            console.log('Made commands data: ', commandsData);
            return commandsData;
        }
    
    
        $doc = $doc || $(document);
    
        if (!lib.detectGroups($doc).isAll) {
            onProgress_ && onProgress_(lib.messages.IS_IN_GROUP);
    
            if (onDone_)
                onDone_(lib.errorCodes.NOT_ALL_GROUP);
            else
                alert(lib.messages.IS_IN_GROUP);
            return;
        }
    
        let pages = lib.detectMultiPages($doc);
        pages.push(lib.makeTwUrl(lib.pageTypes.INCOMINGS_OVERVIEW));
        console.log('Got incomings pages: ', pages);
    
        let groups = lib.detectGroups($doc);
        if (!groups.isAll) {
            onProgress_ && onProgress_(lib.messages.IS_IN_GROUP);
    
            if (onDone_)
                onDone_(true);
            else
                alert(lib.messages.IS_IN_GROUP);
            return;
        }
    
        var pageContents = [];
        let requestManager = new RequestManager();
    
        let collectPagesMessage = 'Collecting incoming pages...';
        onProgress_ && onProgress_(collectPagesMessage);
    
        let allIncomings = [];
    
        pages.forEach((link) => {
            requestManager.addRequest(link, (data) => {
                if (lib.checkContainsCaptcha(data)) {
                    if (requestManager.isRunning()) {
                        onProgress_ && onProgress_(lib.messages.TRIGGERED_CAPTCHA);
    
                        if (onDone_)
                            onDone_(lib.errorCodes.CAPTCHA);
                        else
                            alert(lib.messages.TRIGGERED_CAPTCHA);
    
                        requestManager.stop();
                    }
    
                    return;
                }
    
                onProgress_ && onProgress_(`${collectPagesMessage} (${requestManager.getStats().done}/${pages.length} done, ${requestManager.getStats().numFailed} failed)`);
                let pageIncomings = parseUploadIncomingsOverviewPage($(data));
                allIncomings.push(...pageIncomings);
            });
        });
    
        if (!requestManager.getStats().total) {
            lib.postApi(lib.makeApiUrl('command/finished-incoming-uploads'));
            onProgress_ && onProgress_('No incomings to upload.');
            if (onDone_)
                onDone_(false);
            else
                alert('No incomings to upload.');
    
            return;
        } else {
            requestManager.start();
        }
    
        requestManager.setFinishedHandler(() => {
            requestManager.stop();
            onProgress_ && onProgress_('Uploading incomings...');
    
            lib.queryCurrentPlayerInfo((playerId) => {
                allIncomings.forEach((inc) => inc.targetPlayerId = playerId);
                var distinctIncomings = allIncomings.distinct((a, b) => {
                    return JSON.stringify(a) == JSON.stringify(b);
                });
    
                console.log('From ' + allIncomings.length + ', made ' + distinctIncomings.length + ' distinct incomings');
    
                let data = {
                    isOwnCommands: false,
                    commands: distinctIncomings
                };
                lib.postApi(lib.makeApiUrl('command'), data)
                    .done(() => {
                        $doc.find('input[name*=id_][type=checkbox]').prop('checked', true);
    
                        if (onProgress_) {
                            onProgress_('Finished: Uploaded ' + distinctIncomings.length + ' incomings.');
                        }
    
                        if (!onDone_)
                            alert('Uploaded commands!');
                        else
                            onDone_();
                    })
                    .fail((req, status, err) => {
                        if (onProgress_) {
                            onProgress_('An error occurred while uploading data.');
                        }
    
                        if (!onDone_) {
                            alert('An error occurred...');
                        } else {
                            onDone_(true);
                        }
                        console.error('POST request failed: ', req, status, err);
                    });
    
            });
        });
    
    }

    function parseAllTroops($doc, onProgress_, onDone_) {
    
        
        function parseOwnTroopsOverviewPage($doc) {
            $doc = $doc || $(document);
        
            let troopData = [];
        
            let $villageRows = $doc.find('#units_table tbody');
            $villageRows.each((i, el) => {
                let $el = $(el);
                let $troopRows = $el.find('tr');
        
                var villageId = $($troopRows[0]).find('td:first-of-type a').prop('href').match(/village=(\w+)/)[1];
                villageId = parseInt(villageId);
        
                let $atHomeTroops = $($troopRows[0]).find('.unit-item');
                let $stationedTroops = $($troopRows[1]).find('.unit-item');
                let $supportingTroops = $($troopRows[2]).find('.unit-item');
                let $travelingTroops = $($troopRows[3]).find('.unit-item');
        
                var atHomeTroops = {};
                var stationedTroops = {};
                var supportingTroops = {};
                var travelingTroops = {};
        
                $atHomeTroops.each((i, el) => atHomeTroops[indexToName(i)] = parseInt($(el).text()));
                $stationedTroops.each((i, el) => stationedTroops[indexToName(i)] = parseInt($(el).text()));
                $supportingTroops.each((i, el) => supportingTroops[indexToName(i)] = parseInt($(el).text()));
                $travelingTroops.each((i, el) => travelingTroops[indexToName(i)] = parseInt($(el).text()));
        
                var villageTroopData = {
                    villageId: villageId,
                    stationed: stationedTroops,
                    traveling: travelingTroops,
                    supporting: supportingTroops,
                    atHome: atHomeTroops
                };
        
                console.log('Made village troop data: ', villageTroopData);
        
                troopData.push(villageTroopData);
            });
        
            return troopData;
        
            function indexToName(idx) {
                switch (idx) {
                    case 0: return 'spear'; break;
                    case 1: return 'sword'; break;
                    case 2: return 'axe'; break;
                    case 3: return 'spy'; break;
                    case 4: return 'light'; break;
                    case 5: return 'heavy'; break;
                    case 6: return 'ram'; break;
                    case 7: return 'catapult'; break;
                    case 8: return 'paladin'; break;
                    case 9: return 'snob'; break;
                    case 10: return 'militia'; break;
                }
            }
        }
    
        function parseTroopsSupportOverviewPage($doc) {
            $doc = $doc || $(document);
        
            let supportRecords = [];
            var currentRecord = null;
            $doc.find('#units_table tbody tr:not(:last-of-type)').each((i, el) => {
                let $tr = $(el);
                let classes = $tr.prop('class').trim().split(' ');
        
                if (classes.contains('units_away')) {
                    if (currentRecord)
                        supportRecords.push(currentRecord);
        
                    currentRecord = {
                        sourceVillageId: $tr.find('td:first-of-type a').prop('href').match(/village=(\d+)/)[1],
                        supportedVillages: []
                    };
                } else if (classes.contains('row_a') || classes.contains('row_b')) {
                    let troopCounts = [];
                    $tr.find('.unit-item').each((_, el) => troopCounts.push(parseInt($(el).text())));
        
                    let targetVillageLink = $tr.find('td:first-of-type a:nth-of-type(1)').prop('href');
                    let supportedVillage = {
                        id: (targetVillageLink.match(/id=(\d+)/) || targetVillageLink.match(/village=(\d+)/))[1],
                        troopCounts: lib.troopsArrayToObject(troopCounts)
                    };
        
                    currentRecord.supportedVillages.push(supportedVillage);
                }
            });
        
            if (currentRecord)
                supportRecords.push(currentRecord);
        
            supportRecords.forEach((record) => {
                record.sourceVillageId = parseInt(record.sourceVillageId);
                record.supportedVillages.forEach((s) => {
                    s.id = parseInt(s.id);
                });
            });
        
            console.log('Made supportRecords = ', supportRecords);
            return supportRecords;
        }
    
    
        $doc = $doc || $(document);
    
        var requestManager = new RequestManager();
        let pages = lib.detectMultiPages($doc);
        pages.push(lib.makeTwUrl(lib.pageTypes.OWN_TROOPS_OVERVIEW));
    
        let troops = [];
        let supportData = [];
    
        let gettingPagesMessage = 'Getting village troop pages...';
        onProgress_ && onProgress_(gettingPagesMessage);
    
        pages.forEach((link) => {
            requestManager.addRequest(link, (data) => {
    
                onProgress_ && onProgress_(`${gettingPagesMessage} (${requestManager.getStats().done}/${pages.length})`);
    
                if (lib.checkContainsCaptcha(data)) {
    
                    if (requestManager.isRunning()) {
                        onProgress_ && onProgress_(lib.messages.TRIGGERED_CAPTCHA);
    
                        if (onDone_)
                            onDone_(lib.errorCodes.CAPTCHA);
                        else
                            alert(lib.messages.TRIGGERED_CAPTCHA);
    
                        requestManager.stop();
                    }
    
                    return;
                }
    
                let pageTroops = parseOwnTroopsOverviewPage($(data));
                troops.push(...pageTroops);
            });
        });
    
        requestManager.setFinishedHandler(() => {
            requestManager.stop();
    
            onProgress_ && onProgress_('Finding village with academy...');
    
            findVillaWithAcademy((villageId) => {
    
                if (villageId < 0) {
                    if (onProgress_) {
                        onProgress_ && onProgress_('(No village with academy found)');
                        setTimeout(() => uploadToVault(0), 3000);
                    } else {
                        uploadToVault(0);
                    }
                } else {
                    onProgress_ && onProgress_('Getting possible nobles...');
                    getPossibleNobles(villageId, (cnt) => {
                        onProgress_ && onProgress_('Getting support...');
                        collectSupportData(() => {
                            uploadToVault(cnt);
                        });
                    });
                }
    
            });
    
        });
    
        requestManager.start();
    
        function collectSupportData(onDone) {
            $.get(lib.makeTwUrl(lib.pageTypes.OWN_TROOPS_SUPPORTING_OVERVIEW))
                .done((data) => {
                    onProgress_ && onProgress_('Collecting supported villages and DVs...');
                    let $supportDoc = $(data);
                    let supportPages = lib.detectMultiPages($supportDoc);
                    let $supportPages = [];
    
                    requestManager.resetStats();
                    supportPages.forEach((link) => {
                        requestManager.addRequest(link, (data) => {
                            $supportPages.push($(data));
                        });
                    });
    
                    if (requestManager.getStats().total) {
                        requestManager.setFinishedHandler(parseAndFinish);
                        requestManager.start();
                    } else {
                        parseAndFinish();
                    }
    
                    function parseAndFinish() {
                        requestManager.stop();
                        $supportPages.forEach(($page, i) => {
                            console.log('Parsing page ' + i);
                            supportData.push(...parseTroopsSupportOverviewPage($page))
                        });
                        onDone();
                    }
                });
        }
    
        function findVillaWithAcademy(onDone) {
            $.get(lib.makeTwUrl(lib.pageTypes.BUILDINGS_OVERVIEW))
                .done((data) => {
                    let $doc = $(data);
                    var villaWithAcademy = null;
                    $doc.find('.b_snob').each((i, el) => {
                        if (villaWithAcademy != null)
                            return;
    
                        let $el = $(el);
                        if ($el.text().trim() == 0)
                            return;
    
                        let $tr = $el.closest('tr');
                        let $smith = $tr.find('.b_smith');
                        if ($smith.text().trim() < 20)
                            return;
    
                        villaWithAcademy = $tr.prop('id').match(/v_(\d+)/)[1];
                    });
    
                    console.log('villaWithAcademy = ', villaWithAcademy);
    
                    if (villaWithAcademy) {
                        onDone(villaWithAcademy);
                    } else {
                        onDone(-1);
                    }
                })
                .error(() => {
                    if (onProgress_)
                        onProgress_('An error occurred while finding villa with academy...');
                    else
                        alert('An error occurred while finding villa with academy...');
    
                    if (onDone_)
                        onDone_(false);
                });
        }
    
        function getPossibleNobles(villaIdWithAcademy, onDone) {
            $.get(`/game.php?village=${villaIdWithAcademy}&screen=snob`)
                .done((data) => {
                    let docText = $(data).text();
                    let limit = docText.match(/Noblemen\s+limit:\s*(\d+)/);
                    let current = docText.match(/Number\s+of\s+conquered\s+villages:\s*(\d+)/);
    
                    console.log('Got limit: ', limit);
                    console.log('Got current: ', current);
    
                    if (limit && current) {
                        onDone(parseInt(limit[1]) - parseInt(current[1]));
                    } else {
                        onDone(null);
                    }
                })
                .error(() => {
                    if (onProgress_)
                        onProgress_('An error occurred while getting possible noble counts...');
                    else
                        alert('An error occurred while getting possible noble counts...');
    
                    if (onDone_)
                        onDone_(false);
                });
        }
    
        function uploadToVault(possibleNobles) {
    
            onProgress_ && onProgress_("Uploading troops to vault...");
    
            let distinctTroops = troops.distinct((a, b) => a.villageId == b.villageId);
    
            let data = {
                troopData: distinctTroops,
                possibleNobles: possibleNobles
            };
    
            let onError = () => {
                if (onProgress_)
                    onProgress_("An error occurred while uploading to the vault.");
    
                if (!onDone_)
                    alert('An error occurred...')
                else
                    onDone_(true);
            };
    
            uploadArmy(() => {
                if (onProgress_)
                    onProgress_('Finished: Uploaded troops for ' + distinctTroops.length + ' villages.');
    
                if (!onDone_)
                    alert('Done!')
                else
                    onDone_(false);
            });
    
            function uploadArmy(onDone) {
                console.log('Uploading army data: ', data);
                lib.postApi(lib.makeApiUrl('village/army/current'), data)
                    .error(onError)
                    .done(() => {
                        onProgress_ && onProgress_('Uploading support to vault...');
                        uploadSupport(onDone);
                    });
            }
    
            function uploadSupport(onDone) {
                console.log('Uploading support data: ', supportData);
                lib.queryCurrentPlayerInfo((playerId) => {
                    lib.postApi(lib.makeApiUrl(`player/${playerId}/support`), supportData)
                        .error(onError)
                        .done(onDone);
                });
            }
        }
    }

    function parseAllCommands($doc, onProgress_, onDone_) {
    
        function parseOwnCommand(commandId, commandType, isReturning, $doc) {
            let $container = $doc.find('#content_value');
            let sourcePlayerId = $doc.find('#content_value .vis:nth-of-type(1) tr:nth-of-type(2) td:nth-of-type(3) a').prop('href').match(/id=(\w+)/)[1];
            let sourceVillageId = $doc.find('#content_value .vis:nth-of-type(1) tr:nth-of-type(3) td:nth-of-type(2) a').prop('href').match(/id=(\w+)/)[1];
            var targetPlayerId = $doc.find('#content_value .vis:nth-of-type(1) tr:nth-of-type(4) td:nth-of-type(3) a').prop('href');
            let targetVillageId = $doc.find('#content_value .vis:nth-of-type(1) tr:nth-of-type(5) td:nth-of-type(2) a').prop('href').match(/id=(\w+)/)[1];
        
            if (targetPlayerId) {
                targetPlayerId = targetPlayerId.match(/id=(\w+)/);
                if (targetPlayerId)
                    targetPlayerId = targetPlayerId[1];
                else
                    targetPlayerId = null;
            } else {
                targetPlayerId = null;
            }
        
            let hasCatapult = $container.text().contains("Catapult");
            let landsAtSelector = hasCatapult
                ? '#content_value .vis:nth-of-type(1) tr:nth-of-type(8) td:nth-of-type(2)'
                : '#content_value .vis:nth-of-type(1) tr:nth-of-type(7) td:nth-of-type(2)';
            let landsAt = lib.parseTimeString($doc.find(landsAtSelector).text());
        
            var troopCounts = {};
            let $troopCountEntries = $container.find('.unit-item');
            $troopCountEntries.each((i, el) => {
                let $el = $(el);
                let cls = $el.prop('class');
                let troopType = cls.match(/unit\-item\-(\w+)/)[1];
                let count = parseInt($el.text().trim());
        
                troopCounts[troopType] = count;
            });
        
            let command = {
                commandId: commandId,
                sourcePlayerId: parseInt(sourcePlayerId),
                sourceVillageId: parseInt(sourceVillageId),
                targetPlayerId: parseInt(targetPlayerId),
                targetVillageId: parseInt(targetVillageId),
                landsAt: landsAt.toUTCString(),
                troops: troopCounts,
                commandType: commandType,
                isReturning: isReturning
            };
        
            return command;
        }
    
        function parseOwnCommandsOverviewPage($doc) {
            $doc = $doc || $(document);
        
            let $commandLinks = $doc.find('#commands_table tr:not(:first-of-type):not(:last-of-type) td:first-of-type a:not(.rename-icon)');
        
            let commandLinks = [];
        
            $commandLinks.each((i, el) => {
                let link = $(el).prop('href');
                let $td = $(el).closest('td');
                var commandState = $td.find('img:first-of-type').prop('src').match(/(\w+)\.png/)[1];
        
                let commandType = commandState.contains("attack") ? "attack" : "support";
                let isReturning = commandState.contains("return") || commandState.contains("back");
        
                let commandId = parseInt(link.match(/id\=(\w+)/)[1]);
        
                commandLinks.push({
                    link: link,
                    commandId: commandId,
                    commandType: commandType,
                    isReturning: isReturning
                });
            });
        
            return commandLinks;
        }
    
    
        $doc = $doc || $(document);
    
        var requestManager = new RequestManager();
    
        var oldCommands = lib.getLocalStorage('commands-history', []);
        let commandLinks = [];
        let newCommandData = [];
        let newCommands = [];
    
        let pages = lib.detectMultiPages($doc);
        pages.push(lib.makeTwUrl(lib.pageTypes.OWN_COMMANDS_OVERVIEW));
    
        let collectingPagesMessage = 'Collecting command pages...';
        onProgress_ && onProgress_(collectingPagesMessage);
    
        pages.forEach((link) => {
            requestManager.addRequest(link, (data) => {
                if (lib.checkContainsCaptcha(data)) {
    
                    if (requestManager.isRunning()) {
                        onProgress_ && onProgress_(lib.messages.TRIGGERED_CAPTCHA);
    
                        if (onDone_)
                            onDone_(lib.errorCodes.CAPTCHA);
                        else
                            alert(lib.messages.TRIGGERED_CAPTCHA);
    
                        requestManager.stop();
                    }
    
                    return;
                }
    
                onProgress_ && onProgress_(`${collectingPagesMessage} (${requestManager.getStats().done}/${pages.length})`);
    
                let pageCommands = parseOwnCommandsOverviewPage($(data));
                commandLinks.push(...pageCommands);
            });
        });
    
        requestManager.setFinishedHandler(() => {
            requestManager.stop();
    
            let distinctCommandLinks = commandLinks.distinct((a, b) => a.link == b.link);
            newCommands.push(...distinctCommandLinks.except((c) => oldCommands.contains(c.commandId)));
    
            //  Remove nonexistant commands from oldCommands
            oldCommands = oldCommands.except((c) => !distinctCommandLinks.contains((r) => r.commandId));
    
            console.log('All commands: ', commandLinks);
            console.log('Distinct commands: ', distinctCommandLinks);
            console.log('Collected new commands: ', newCommands);
    
            if (!newCommands.length) {
                lib.postApi(lib.makeApiUrl('command/finished-command-uploads'));
                onProgress_ && onProgress_('Finished: No new commands to upload.');
    
                if (onDone_)
                    onDone_();
                else
                    alert('No new commands to upload.');
    
                return;
            }
    
            requestManager.resetStats();
    
            onProgress_ && onProgress_('Checking for previously-uploaded commands...');
            checkExistingCommands(newCommands.map(_ => _.commandId), (existingCommands) => {
    
                oldCommands.push(...existingCommands);
    
                let fetchingCommandsMessage = 'Uploading commands...';
                onProgress_ && onProgress_(fetchingCommandsMessage);
    
                newCommands.forEach((cmd) => {
                    let commandId = cmd.commandId;
                    let link = cmd.link;
    
                    if (oldCommands.contains(commandId))
                        return;
    
                    requestManager.addRequest(link, (data) => {
                        if (lib.checkContainsCaptcha(data)) {
                            if (requestManager.isRunning()) {
                                onProgress_ && onProgress_(lib.messages.TRIGGERED_CAPTCHA);
    
                                if (onDone_)
                                    onDone_(lib.errorCodes.CAPTCHA);
                                else
                                    alert(lib.messages.TRIGGERED_CAPTCHA);
    
                                requestManager.stop();
                            }
    
                            return;
                        }
    
                        let command = parseOwnCommand(commandId, cmd.commandType, cmd.isReturning, $(data));
    
                        let notifyOnDone = () => requestManager.pendingRequests.length && onProgress_ && onProgress_(`${fetchingCommandsMessage} (${requestManager.getStats().done}/${requestManager.getStats().total} done, ${requestManager.getStats().numFailed} failed)`);
    
                        let commandData = {
                            isOwnCommands: true,
                            commands: [command]
                        };
    
                        lib.postApi(lib.makeApiUrl('command'), commandData)
                            .done(() => {
                                oldCommands.push(command.commandId);
                                lib.setLocalStorage('commands-history', oldCommands);
                                notifyOnDone();
    
                            })
                            .fail((req, status, err) => {
                                requestManager.getStats().numFailed++;
                                notifyOnDone();
    
                                console.error(req, status, err);
                            });
                    });
                });
    
                if (!requestManager.getStats().total) {
                    lib.setLocalStorage('commands-history', oldCommands);
                    lib.postApi(lib.makeApiUrl('command/finished-command-uploads'));
                    onProgress_ && onProgress_('Finished: No new commands to upload.');
    
                    if (onDone_)
                        onDone_();
                    else
                        alert('No new commands to upload.');
    
                    return;
                }
    
                requestManager.start();
    
                requestManager.setFinishedHandler(() => {
                    let stats = requestManager.getStats();
                    lib.setLocalStorage('commands-history', oldCommands);
                    onProgress_ && onProgress_(`Finished: ${stats.done}/${stats.total} uploaded, ${stats.numFailed} failed.`);
    
                    if (!onDone_)
                        alert('Done!');
                    else
                        onDone_(false);
                });
    
            });
        });
    
        requestManager.start();
    
    
    
    
        function checkExistingCommands(commandIds, onDone) {
            lib.postApi(lib.makeApiUrl('command/check-existing-commands'), commandIds)
                .error(() => {
                    onProgress_ && onProgress_('Failed to check for old commands, uploading all...');
                    setTimeout(onDone, 2000);
                })
                .done((existingCommandIds) => {
                    if (existingCommandIds.length) {
                        onProgress_ && onProgress_('Found ' + existingCommandIds.length + ' old commands, skipping these...');
                        setTimeout(() => onDone(existingCommandIds), 2000);
                    } else {
                        onDone(existingCommandIds);
                    }
                });
        }
        
    }



    /*** UI ***/
    function parseMapPage($doc) {
        $doc = $doc || $(document);
    
        if (window.ranVaultMap) {
            return;
        }
    
        window.ranVaultMap = true;
        var canUse = true;
    
        //  Hook into 'TWMap.displayForVillage', which is invoked whenever the village info popup is made
        //  by TW
    
        var currentVillageId = null;
        let $popup = $doc.find('#map_popup');
    
        $doc.find('#continent_id').parent().append('<span> - Using Vault</span>');
    
        var cachedData = {};
        let requestedVillageIds = [];
        let settings = loadSettings();
        let lockedDataReasons = null;
    
        createSettingsUI();
    
        //  First call that actually shows the popup - Update the popup if we've already downloaded village data
        let originalDisplayForVillage = TWMap.popup.displayForVillage;
        TWMap.popup.displayForVillage = function (e, a, t) {
            console.log('intercepted displayForVillage');
            originalDisplayForVillage.call(TWMap.popup, e, a, t);
    
            if (lockedDataReasons) {
                makeFuckYouContainer();
                return;
            }
    
            if (!canUse)
                return;
    
            let villageInfo = e;
            let villageId = villageInfo.id;
    
            currentVillageId = villageId;
            if (cachedData[villageId]) {
                makeOutput(cachedData[villageId]);
            } else if (TWMap.popup._cache[villageId]) {
                let twCached = TWMap.popup._cache[villageId];
                if (requestedVillageIds.indexOf(villageId) >= 0) {
                    return;
                }
                let morale = Math.round((twCached.morale || twCached.mood) * 100);
                if (isNaN(morale))
                    morale = 100;
                loadVillageTroopData(villageId, morale);
            }
        };
    
        // Call made after popup is shown and TW has downloaded data for the village (ie incoming attacks, morale, etc)
        let originalReceivedInfo = TWMap.popup.receivedPopupInformationForSingleVillage;
        TWMap.popup.receivedPopupInformationForSingleVillage = function (e) {
            console.log('Intercepted receivedPopupInformation');
            originalReceivedInfo.call(TWMap.popup, e);
    
            if (lockedDataReasons) {
                makeFuckYouContainer();
                return;
            }
    
            let villageInfo = e;
            if (!villageInfo || !villageInfo.id)
                return;
    
            currentVillageId = villageInfo.id;
            let villageId = villageInfo.id;
            //  Why is "mood" a thing (alternate name for "morale")
            let morale = Math.round((villageInfo.morale || villageInfo.mood) * 100);
            if (isNaN(morale))
                morale = 100;
    
            if (cachedData[villageInfo.id]) {
                makeOutput(cachedData[villageId]);
            } else {
                if (requestedVillageIds.indexOf(villageId) >= 0) {
                    return;
                }
                loadVillageTroopData(villageId, morale);
            }
        };
    
        function loadVillageTroopData(villageId, morale) {
            requestedVillageIds.push(villageId);
            lib.getApi(lib.makeApiUrl(`village/${villageId}/army?morale=${morale}`))
                .done((data) => {
                    console.log('Got village data: ', data);
    
                    data.morale = morale;
    
                    data.stationedSeenAt = data.stationedSeenAt ? new Date(data.stationedSeenAt) : null;
                    data.recentlyLostArmySeenAt = data.recentlyLostArmySeenAt ? new Date(data.recentlyLostArmySeenAt) : null;
                    data.travelingSeenAt = data.travelingSeenAt ? new Date(data.travelingSeenAt) : null;
                    data.ownedArmySeenAt = data.ownedArmySeenAt ? new Date(data.ownedArmySeenAt) : null;
    
                    data.lastBuildingsSeenAt = data.lastBuildingsSeenAt ? new Date(data.lastBuildingsSeenAt) : null;
                    data.lastLoyaltySeenAt = data.lastLoyaltySeenAt ? new Date(data.lastLoyaltySeenAt) : null;
    
                    cachedData[villageId] = data;
    
                    //  User changed village while the data was loading
                    if (villageId != currentVillageId) {
                        return;
                    }
    
                    makeOutput(data);
                })
                .error((xhr, b, c) => {
                    if (!canUse)
                        return;
    
                    if (xhr.status == 423) {
                        let reasons = null;
                        try {
                            reasons = JSON.parse(xhr.responseText);
                            lockedDataReasons = reasons;
                        } catch (_) { }
    
                        let alertMessage = "You haven't uploaded data in a while, you can't use the map script until you do. Go to a different page and run this script again."
                        if (reasons) {
                            alertMessage += `\nYou need to upload: ${reasons.join(', ')}`;
                        }
    
                        alert(alertMessage);
                        canUse = false;
                    } else if (xhr.status != 401) {
                        alert("An error occurred...");
                    }
                });
        }
    
        console.log('Added map hook');
    
        function makeOutputContainer() {
            let $villageInfoContainer = $('<div id="vault_info" style="background-color:#e5d7b2;">');
            $villageInfoContainer.appendTo($popup);
            return $villageInfoContainer;
        }
    
        function makeOutput(data) {
            if ($('#vault_info').length) {
                return;
            }
    
            let $villageInfoContainer = makeOutputContainer();
    
            //  Limit "own commands" to max 2
            let $commandRows = $('.command-row');
            let twCommandData = [];
    
            //  Remove all except non-small attacks
            for (var i = 0; i < $commandRows.length; i++) {
                let $row = $($commandRows[i]);
                let $images = $row.find('img');
                let isSmall = false;
                let isSupport = true;
                let isOwn = false;
                let isReturning = false;
                $images.each((i, el) => {
                    let $el = $(el);
                    if ($el.prop('src').contains('attack_'))
                        isOwn = true;
    
                    if ($el.prop('src').contains("attack_small"))
                        isSmall = true;
    
                    if ($el.prop('src').contains('attack'))
                        isSupport = false;
    
                    if ($el.prop('src').contains('return'))
                        isReturning = true;
                });
    
                //  Collect command data for later
                let commandId = parseInt($row.find('.command_hover_details').data('command-id'));
                let commandData = {
                    isSmall: isSmall,
                    isSupport: isSupport,
                    isOwn: isOwn,
                    isReturning: isReturning
                };
    
                twCommandData.push(commandData);
    
                if ((isSmall || isSupport) && $commandRows.length > 2) {
                    $($commandRows[i]).remove();
                    $commandRows = $('.command-row');
                    --i;
                }
            }
    
            //  Remove intel rows
            $('#info_last_attack').closest('tr').remove();
            $('#info_last_attack_intel').closest('tr').remove();
    
            $('#info_content').css('width', '100%');
    
            //  Update data with what's been loaded by TW (in case someone forgot to upload commands)
            let hasRecord = (id) => (data.fakes && data.fakes.contains(id)) || (data.dVs && data.dVs[id]) || (data.nukes && data.nukes.contains(id));
    
            let numFakes = data.fakes ? data.fakes.length : 0;
            let numNukes = data.nukes ? data.nukes.length : 0;
            let numPlayers = data.players ? data.players.length : 0;
    
            let numDVs = 0;
            lib.objForEach(data.Dvs, (commandId, pop) => {
                numDVs += pop / 20000;
            });
            numDVs = Math.roundTo(numDVs, 1);
    
            twCommandData.forEach((cmd) => {
                if (!cmd.isOwn || hasRecord(cmd.commandId) || cmd.isReturning)
                    return;
    
                if (!cmd.isSupport) {
                    if (cmd.isSmall)
                        numFakes++;
                    else
                        numNukes++;
                }
            });
    
            //  NOTE - This assumes no archers!
            $villageInfoContainer.html(`
                        ${ !settings.showCommands ? '' : `
                            <table class='vis' style="width:100%">
                                <tr>
                                    <th># Fakes</th>
                                    <th># Nukes</th>
                                    <th># DVs</th>
                                    <th># Players Sending</th>
                                </tr>
                                <tr>
                                    <td>${numFakes}</td>
                                    <td>${numNukes}</td>
                                    <td>${numDVs}</td>
                                    <td>${numPlayers}</td>
                                </tr>
                            </table>
                        `}
                        ${ !data.stationedArmy && !data.travelingArmy && !data.recentlyLostArmy && !data.ownedArmy ? '<div style="text-align:center;padding:0.5em;">No army data available.</div>' : `
                        <table class='vis' style="width:100%">
                            <tr style="background-color:#c1a264 !important">
                                <th>Vault</th>
                                <th>Seen at</th>
                                <th><img src="https://dsen.innogamescdn.com/8.136/37951/graphic/unit/unit_spear.png" title="" alt="" class=""></th>
                                <th><img src="https://dsen.innogamescdn.com/8.136/37951/graphic/unit/unit_sword.png" title="" alt="" class=""></th>
                                <th><img src="https://dsen.innogamescdn.com/8.136/37951/graphic/unit/unit_axe.png" title="" alt="" class=""></th>
                                <th><img src="https://dsen.innogamescdn.com/8.136/37951/graphic/unit/unit_spy.png" title="" alt="" class=""></th>
                                <th><img src="https://dsen.innogamescdn.com/8.136/37951/graphic/unit/unit_light.png" title="" alt="" class=""></th>
                                <th><img src="https://dsen.innogamescdn.com/8.136/37951/graphic/unit/unit_heavy.png" title="" alt="" class=""></th>
                                <th><img src="https://dsen.innogamescdn.com/8.136/37951/graphic/unit/unit_ram.png" title="" alt="" class=""></th>
                                <th><img src="https://dsen.innogamescdn.com/8.136/37951/graphic/unit/unit_catapult.png" title="" alt="" class=""></th>
                                <th><img src="https://dsen.innogamescdn.com/8.136/37951/graphic/unit/unit_knight.png" title="" alt="" class=""></th>
                                <th><img src="https://dsen.innogamescdn.com/8.136/37951/graphic/unit/unit_snob.png" title="" alt="" class=""></th>
                            </tr>
                            ${ !data.stationedArmy ? '' : `
                            <tr>
                                <td>Stationed</td>
                                <td>${data.stationedSeenAt ? lib.formateDateTime(data.stationedSeenAt) : ''}</td>
                                ${makeTroopTds(data.stationedArmy || {})}
                            </tr>
                            `}
                            ${ !data.travelingArmy ? '' : `
                            <tr>
                                <td>Traveling</td>
                                <td>${data.travelingSeenAt ? lib.formateDateTime(data.travelingSeenAt) : ''}</td>
                                ${makeTroopTds(data.travelingArmy || {})}
                            </tr>
                            `}
                            ${ !data.recentlyLostArmy ? '' : `
                            <tr>
                                <td>Recently lost</td>
                                <td>${data.recentlyLostArmySeenAt ? lib.formateDateTime(data.recentlyLostArmySeenAt) : ''}</td>
                                ${makeTroopTds(data.recentlyLostArmy || {})}
                            </tr>
                            `}
                            ${ !data.ownedArmy ? '' : `
                            <tr>
                                <td>Owned</td>
                                <td>${data.ownedArmySeenAt ? lib.formateDateTime(data.ownedArmySeenAt) : ''}</td>
                                ${makeTroopTds(data.ownedArmy || {})}
                            </tr>
                            `}
                            ${ !settings.showPossiblyRecruited ? '' : `
                                ${ !data.possibleRecruitedOffensiveArmy || !data.possibleRecruitedDefensiveArmy ? '' : `
                                <tr>
                                    <td rowspan="2">Possibly recruited</td>
                                    <td></td>
                                    ${makeTroopTds(data.possibleRecruitedOffensiveArmy || {})}
                                </tr>
                                <tr>
                                    <td></td>
                                    ${makeTroopTds(data.possibleRecruitedDefensiveArmy || {})}
                                </tr>
                                `}
                            `}
                            ${ !data.nukesRequired || !settings.showNukes ? '' : `
                            <tr>
                                <td colspan=12 style="text-align:center">Will take ~${data.nukesRequired} nukes to clear at ${data.morale}% morale (last nuke has ~${data.lastNukeLossPercent}% losses)</td>
                            </tr>
                            `}
                        </table>
                        `}
                        ${ !settings.showBuildings ? '' : `
                            ${ typeof data.lastBuildings == 'undefined' || data.lastBuildings == null ? '<div style="text-align:center;padding:0.5em;">No building data available.</div>' : `
                            <table class='vis' style="width:100%">
                                <tr style="background-color:#c1a264 !important">
                                    <th>Vault</th>
                                    <th>Seen at</th>
                                    <th><img src="https://dsen.innogamescdn.com/8.137/38092/graphic/buildings/snob.png" title="Academy" alt="" class="bmain_list_img"></th>
                                    <th><img src="https://dsen.innogamescdn.com/8.137/38092/graphic/buildings/smith.png" title="Smithy" alt="" class="bmain_list_img"></th>
                                    <th><img src="https://dsen.innogamescdn.com/8.137/38092/graphic/buildings/farm.png" title="Farm" alt="" class="bmain_list_img"></th>
                                    <th><img src="https://dsen.innogamescdn.com/8.137/38092/graphic/buildings/wall.png" title="Wall" alt="" class="bmain_list_img"></th>
                                </tr>
                                <tr>
                                    <td>Latest levels</td>
                                    <td>${data.lastBuildingsSeenAt ? lib.formateDateTime(data.lastBuildingsSeenAt) : ''}</td>
                                    <td>${data.lastBuildings ? data.lastBuildings['snob'] || '-' : '' }</td>
                                    <td>${data.lastBuildings ? data.lastBuildings['smith'] || '-' : '' }</td>
                                    <td>${data.lastBuildings ? data.lastBuildings['farm'] || '-' : '' }</td>
                                    <td>${data.lastBuildings ? data.lastBuildings['wall'] || '-' : '' }</td>
                                </tr>
                                <tr>
                                    <td>Possible levels</td>
                                    <td></td>
                                    <td>${data.possibleBuildings ? data.possibleBuildings['snob'] || '-' : ''}</td>
                                    <td>${data.possibleBuildings ? data.possibleBuildings['smith'] || '-' : ''}</td>
                                    <td>${data.possibleBuildings ? data.possibleBuildings['farm'] || '-' : ''}</td>
                                    <td>${data.possibleBuildings ? data.possibleBuildings['wall'] || '-' : '' }</td>
                                </tr>
                            </table>
                            `}
                        `}
                        ${ typeof data.lastLoyalty == 'undefined' || data.lastLoyalty == null || !settings.showLoyalty ? '' : `
                        <table class='vis' style="width:100%">
                            <tr style="background-color:#c1a264 !important">
                                <th>Vault</th>
                                <th>Seen at</th>
                                <th>Loyalty</th>
                            </tr>
                            <tr>
                                <td>Latest loyalty</td>
                                <td>${data.lastLoyaltySeenAt ? lib.formateDateTime(data.lastLoyaltySeenAt) : ''}</td>
                                <td>${data.lastLoyalty ? data.lastLoyalty || '-' : ''}</td>
                            </tr>
                            <tr>
                                <td>Possible loyalty</td>
                                <td></td>
                                <td>${data.possibleLoyalty ? data.possibleLoyalty || '-' : ''}</td>
                            </tr>
                        </table>
                        `}
                    `.trim());
        }
    
        function makeFuckYouContainer() {
            if ($('#vault_info').length)
                return;
    
            $('#info_content').css('width', '100%');
    
            let $villageInfoContainer = makeOutputContainer();
    
            let fuckYouMessage = '';
            for (var i = 0; i < lockedDataReasons.length; i++) {
                if (fuckYouMessage.length && i != lockedDataReasons.length - 1) {
                    fuckYouMessage += ', ';
                }
                if (i > 0 && i == lockedDataReasons.length - 1) {
                    fuckYouMessage += ' and ';
                }
                fuckYouMessage += lockedDataReasons[i];
            }
    
            $villageInfoContainer.html(`
                <h3 style="padding:1em; text-align:center;margin:0">
                    Upload your damn ${fuckYouMessage}!!
                    <br>
                    <br>
                    (then refresh this page)
                </h3>
            `);
        }
    
        function makeTroopTds(troops) {
            var counts = [];
            counts.push(troops['spear']);
            counts.push(troops['sword']);
            counts.push(troops['axe']);
            counts.push(troops['spy']);
            counts.push(troops['light']);
            counts.push(troops['heavy']);
            counts.push(troops['ram']);
            counts.push(troops['catapult']);
            counts.push(troops['knight']);
            counts.push(troops['snob']);
    
            var parts = [];
            counts.forEach((cnt) => parts.push(`<td>${cnt || cnt == 0 ? cnt : ''}</td>`));
            return parts.join(' ');
        }
    
        function createSettingsUI() {
            let $container = $(`
                <div>
                    <h4>Vault Overlay Settings</h4>
                    <p>
                        <input type="checkbox" id="vault-show-commands" ${settings.showCommands ? 'checked' : ''}>
                        <label for="vault-show-commands">Commands</label>
    
                        <input type="checkbox" id="vault-show-recruits" ${settings.showPossiblyRecruited ? 'checked' : ''}>
                        <label for="vault-show-recruits">Possible recruits</label>
    
                        <input type="checkbox" id="vault-show-buildings" ${settings.showBuildings ? 'checked' : ''}>
                        <label for="vault-show-buildings">Buildings</label>
    
                        <input type="checkbox" id="vault-show-nukes" ${settings.showNukes ? 'checked' : ''}>
                        <label for="vault-show-nukes">Nukes required</label>
    
                        <input type="checkbox" id="vault-show-loyalty" ${settings.showLoyalty ? 'checked' : ''}>
                        <label for="vault-show-loyalty">Loyalty</label>
                    </p>
                </div>
            `.trim());
    
            $container.find('label').css({
                'margin-right': '1.5em'
            });
    
            $('#content_value > h2').after($container);
    
            $container.find('#vault-show-commands').change(() => {
                let $checkbox = $container.find('#vault-show-commands');
                console.log('settings.showCommands = ' + $checkbox.prop('checked'));
                settings.showCommands = $checkbox.prop('checked');
                saveSettings(settings);
            });
    
            $container.find('#vault-show-recruits').change(() => {
                let $checkbox = $container.find('#vault-show-recruits');
                console.log('settings.showRecruits = ' + $checkbox.prop('checked'));
                settings.showPossiblyRecruited = $checkbox.prop('checked');
                saveSettings(settings);
            });
    
            $container.find('#vault-show-buildings').change(() => {
                let $checkbox = $container.find('#vault-show-buildings');
                console.log('settings.showBuildings = ' + $checkbox.prop('checked'));
                settings.showBuildings = $checkbox.prop('checked');
                saveSettings(settings);
            });
    
            $container.find('#vault-show-nukes').change(() => {
                let $checkbox = $container.find('#vault-show-nukes');
                console.log('settings.showNukes = ' + $checkbox.prop('checked'));
                settings.showNukes = $checkbox.prop('checked');
                saveSettings(settings);
            });
    
            $container.find('#vault-show-loyalty').change(() => {
                let $checkbox = $container.find('#vault-show-loyalty');
                console.log('settings.showLoyalty = ' + $checkbox.prop('checked'));
                settings.showLoyalty = $checkbox.prop('checked');
                saveSettings(settings);
            });
        }
    
        function loadSettings() {
            return lib.getLocalStorage('map-settings') || {
                showCommands: true,
                showPossiblyRecruited: true,
                showBuildings: true,
                showNukes: true,
                showLoyalty: true
            };
        }
    
        function saveSettings(settings) {
            lib.setLocalStorage('map-settings', settings);
        }
    }

    function parseAllPages($doc) {
    
        
        
        function makeAdminInterface($uiContainer, $adminContainer) {
            $adminContainer.append(`
                    <div id="admin-inner-container">
                        <div>
                            Get tribe army stats as a spreadsheet: <input id="download-army-stats" type="button" value="Download">
                        </div>
                        <div id="keys-container">
                            <h4>Keys</h4>
                            <table id="keys-table" style="width:100%">
                                <tr>
                                    <th>User name</th>
                                    <th>Current tribe</th>
                                    <th>Auth key</th>
                                    <th></th>
                                    <th></th>
                                </tr>
                            </table>
                            <input type="button" id="new-key-button" value="Make new key">
        
                            <div id="key-script-container" style="display:none">
                                <h5 style="margin-top:2em">New Vault Script</h5>
                                <textarea cols=100 rows=5></textarea>
                            </div>
                        </div>
                    </div>
                `.trim());
        
            $uiContainer.find('.vault-toggle-admin-btn').prop('style', '');
        
            //  Insert existing keys
            lib.getApi(lib.makeApiUrl('admin/keys'))
                .done((data) => {
                    if (typeof data == 'string')
                        data = JSON.parse(data);
        
                    data.forEach((d) => insertNewAuthKey(d));
                })
                .error((xhr) => {
                    if (xhr.responseText) {
                        let error = JSON.parse(xhr.responseText).error;
                        alert(error);
                    } else {
                        alert('An error occurred...');
                    }
                });
        
            $adminContainer.find('#download-army-stats').click(() => {
                lib.getApi(lib.makeApiUrl('admin/summary'))
                    .error(() => alert('An error occurred...'))
                    .done((data) => {
                        if (typeof data == 'string')
                            data = JSON.parse(data);
        
                        try {
                            let csvText = makeArmySummaryCsv(data);
                            let filename = `army-summary.csv`;
        
                            lib.saveAsFile(filename, csvText);
        
                        } catch (e) {
                            alert('An error occurred...');
                            throw e;
                        }
                    });
            });
        
            //  Logic for making a new auth key
            $adminContainer.find('#new-key-button').click(() => {
                var username = prompt("Enter the username or ID");
                if (!username)
                    return;
        
                lib.postApi(lib.makeApiUrl('admin/keys'), {
                    playerId: isNaN(parseInt(username)) ? null : parseInt(username),
                    playerName: isNaN(parseInt(username)) ? username : null,
                    newUserIsAdmin: false
                })
                    .done((data) => {
                        if (typeof data == 'string')
                            data = JSON.parse(data);
                        insertNewAuthKey(data);
                        displayUserScript(data);
                    })
                    .error((xhr) => {
                        if (xhr.responseText) {
                            let error = JSON.parse(xhr.responseText).error;
                            alert(error);
                        } else {
                            alert('An error occurred...');
                        }
                    });
            });
        
        
            function insertNewAuthKey(user) {
        
                var $newRow = $(`
                        <tr data-auth-key="${user.key}">
                            <td>${user.playerName}</td>
                            <td>${user.tribeName}</td>
                            <td>${user.key || '-'}</td>
                            <td><input type="button" class="get-script" value="Get script"></td>
                            <td><input type="button" class="delete-user" value="Delete"></td>
                        </tr>
                    `.trim());
        
                $newRow.find('.get-script').click(() => displayUserScript(user));
        
                $newRow.find('input.delete-user').click(() => {
                    if (!confirm(user.playerName + ' will have their auth key removed.'))
                        return;
        
                    let authKey = user.key;
                    lib.deleteApi(lib.makeApiUrl(`admin/keys/${authKey}`))
                        .done((data) => {
                            $newRow.remove();
                        })
                        .error((xhr) => {
                            if (xhr.responseText) {
                                let error = JSON.parse(xhr.responseText).error;
                                alert(error);
                            } else {
                                alert('An error occurred...');
                            }
                        });
                });
        
                $adminContainer.find('#keys-table').append($newRow);
            }
        
            function displayUserScript(user) {
                var scriptString = 'javascript:';
                scriptString += `window.vaultToken="${user.key}";`;
        
                //let scriptPath = `https://${lib.getScriptHost()}`;
                let scriptPath = `https://v.tylercamp.me/script/main.js`;
                scriptString += `$.getScript("${scriptPath}");`;
        
                $('#key-script-container textarea').val(scriptString);
                $('#key-script-container').css('display', 'block');
        
                $('#key-script-container h5').text(`Vault Script for: ${user.playerName}`);
            }
        }
        
        function processUploadReports($statusContainer, onDone) {
            $.get(lib.makeTwUrl(lib.pageTypes.ALL_REPORTS)).done((data) => {
                try {
                    if (lib.checkContainsCaptcha(data)) {
                        return onDone(lib.errorCodes.CAPTCHA);
                    }
        
                    let $doc = $(data);
                    parseAllReports($doc, (msg) => {
                        $statusContainer.text(msg);
                    }, (didFail) => {
                        onDone(didFail);
                    });
                } catch (e) {
                    $statusContainer.text('An error occurred.');
                    console.error(e);
        
                    onDone(true);
                }
            });
        }
        
        function processUploadIncomings($statusContainer, onDone) {
            $.get(lib.makeTwUrl(lib.pageTypes.INCOMINGS_OVERVIEW)).done((data) => {
                try {
                    if (lib.checkContainsCaptcha(data)) {
                        return onDone(lib.errorCodes.CAPTCHA);
                    }
        
                    let $doc = $(data);
                    parseAllIncomings($doc, (msg) => {
                        $statusContainer.text(msg);
                    }, (didFail) => {
                        onDone(didFail);
                    });
                } catch (e) {
                    $statusContainer.text('An error occurred.');
                    console.error(e);
        
                    onDone(true);
                }
            });
        }
        
        function processUploadCommands($statusContainer, onDone) {
            $.get(lib.makeTwUrl(lib.pageTypes.OWN_COMMANDS_OVERVIEW)).done((data) => {
                try {
                    if (lib.checkContainsCaptcha(data)) {
                        return onDone(lib.errorCodes.CAPTCHA);
                    }
        
                    let $doc = $(data);
                    parseAllCommands($doc, (msg) => {
                        $statusContainer.text(msg);
                    }, (didFail) => {
                        onDone(didFail);
                    });
                } catch (e) {
                    $statusContainer.text('An error occurred.');
                    console.error(e);
        
                    onDone(true);
                }
            });
        }
        
        function processUploadTroops($statusContainer, onDone) {
            $.get(lib.makeTwUrl(lib.pageTypes.OWN_TROOPS_OVERVIEW)).done((data) => {
                try {
                    if (lib.checkContainsCaptcha(data)) {
                        return onDone(lib.errorCodes.CAPTCHA);
                    }
        
                    let $doc = $(data);
                    parseAllTroops($doc, (msg) => {
                        $statusContainer.text(msg);
                    }, (didFail) => {
                        onDone(didFail);
                    });
                } catch (e) {
                    $statusContainer.text('An error occurred.');
                    console.error(e);
        
                    onDone(true);
                }
            });
        }
        
        function makeArmySummaryCsv(armyData) {
            let fullNukePop = 20000;
            let almostNukePop = 15000;
            let fullDVPop = 20000;
        
            let playerSummaries = [];
        
            var totalNukes = 0;
            var totalAlmostNukes = 0;
            var totalDVs = 0;
            var totalNobles = 0;
            var totalPossibleNobles = 0;
        
            let round = (num) => Math.roundTo(num, 1);
        
            let offensiveArmyPopulation = (army) => {
                let offensiveArmy = lib.twcalc.getOffensiveArmy(army);
                let offensiveArmyPop = lib.twcalc.totalPopulation(offensiveArmy);
                return offensiveArmyPop;
            };
        
            let defensiveArmyPopulation = (army) => {
                let defensiveArmy = lib.twcalc.getDefensiveArmy(army);
                let defensiveArmyPop = lib.twcalc.totalPopulation(defensiveArmy);
                return defensiveArmyPop;
            };
        
            armyData.forEach((ad) => {
                let playerId = ad.playerId;
                let playerName = ad.playerName;
                let maxNobles = ad.maxPossibleNobles;
        
                let playerData = {
                    playerId: playerId,
                    playerName: playerName,
                    tribeName: ad.tribeName,
                    numNukes: 0,
                    numAlmostNukes: 0,
                    numNukesTraveling: 0,
                    numNobles: 0,
                    numPossibleNobles: maxNobles,
        
                    numOwnedDVs: 0,
                    numDVsAtHome: 0,
                    numDVsTraveling: 0,
                    numDVsSupportingOthers: 0,
                    numDVsSupportingSelf: 0,
        
                    numDefensiveVillas: 0,
                    numOffensiveVillas: 0
                };
        
                let uploadAge = ad.uploadAge.split(':')[0];
                let uploadAgeDays = uploadAge.contains(".") ? uploadAge.split('.')[0] : '0';
                let uploadAgeHours = uploadAge.contains(".") ? uploadAge.split('.')[1] : uploadAge;
                playerData.needsUpload = parseInt(uploadAgeDays) * 24 + parseInt(uploadAgeHours) > 24;
        
                let uploadedAt = new Date(ad.uploadedAt);
                playerData.uploadedAt = `${uploadedAt.getUTCMonth() + 1}/${uploadedAt.getUTCDate()}/${uploadedAt.getUTCFullYear()}`;
        
                let armiesOwned = ad.armiesOwned;
                let armiesTraveling = ad.armiesTraveling;
                let armyTraveling = ad.armyTraveling;
                let armyAtHome = ad.armyAtHome;
                let armySupportingOthers = ad.armySupportingOthers;
                let armySupportingSelf = ad.armySupportingSelf;
        
                armiesOwned.forEach((army) => {
                    let offensiveArmyPop = offensiveArmyPopulation(army);
                    let defensiveArmyPop = defensiveArmyPopulation(army);
        
                    if (offensiveArmyPop >= fullNukePop) {
                        playerData.numNukes++;
                    } else if (offensiveArmyPop >= almostNukePop) {
                        playerData.numAlmostNukes++;
                    }
        
                    if (defensiveArmyPop > 2000 && defensiveArmyPop > offensiveArmyPop) {
                        playerData.numDefensiveVillas++;
                        playerData.numOwnedDVs += defensiveArmyPop / fullDVPop;
                    } else if (offensiveArmyPop > 2000 && offensiveArmyPop > defensiveArmyPop) {
                        playerData.numOffensiveVillas++;
                    }
        
                    if (army.snob) {
                        playerData.numNobles += army.snob;
                    }
                });
        
                armiesTraveling.forEach((army) => {
                    let offensivePop = offensiveArmyPopulation(army);
                    if (offensivePop > fullNukePop / 2) {
                        playerData.numNukesTraveling += offensivePop / fullNukePop;
                    }
                });
        
                playerData.numOwnedDVs = round(playerData.numOwnedDVs);
                playerData.numNukesTraveling = round(playerData.numNukesTraveling);
        
                if (armyAtHome) {
                    let defensiveAtHomePop = defensiveArmyPopulation(armyAtHome);
                    playerData.numDVsAtHome = round(defensiveAtHomePop / fullDVPop);
                }
        
                if (armyTraveling) {
                    let defensiveTravelingPop = defensiveArmyPopulation(armyTraveling);
                    playerData.numDVsTraveling = round(defensiveTravelingPop / fullDVPop);
                }
        
                if (armySupportingOthers) {
                    let defensiveSupportingOthersPop = defensiveArmyPopulation(armySupportingOthers);
                    playerData.numDVsSupportingOthers = round(defensiveSupportingOthersPop / fullDVPop);
                }
        
                if (armySupportingSelf) {
                    let defensiveSupportingSelfPop = defensiveArmyPopulation(armySupportingSelf);
                    playerData.numDVsSupportingSelf = round(defensiveSupportingSelfPop / fullDVPop);
                }
        
                totalNukes += playerData.numNukes;
                totalAlmostNukes += playerData.numAlmostNukes;
                totalDVs += playerData.numOwnedDVs;
                totalNobles += playerData.numNobles;
                totalPossibleNobles += playerData.numPossibleNobles;
        
                playerSummaries.push(playerData);
            });
        
            console.log('Made player summaries: ', playerSummaries);
        
        
        
            var csvBuilder = new CsvBuilder();
        
            csvBuilder.addRow('', '', '', '', 'Total nukes', 'Total 3/4 nukes', 'Total Nobles', 'Total Possible Nobles', 'Total DVs');
            csvBuilder.addRow('', '', '', '', totalNukes, totalAlmostNukes, totalNobles, totalPossibleNobles, totalDVs);
        
            csvBuilder.addBlank(2);
        
            csvBuilder.addRow(
                'Time', 'Needs upload?', 'Tribe', 'Player', 'Nukes',
                '3/4 Nukes', 'Nukes traveling', 'Nobles', 'Possible nobles', 
                'Owned DVs', 'DVs at Home', 'DVs Traveling', 'DVs Supporting Self', 'DVs Supporting Others',
                'Est. Off. Villas', 'Est. Def. Villas',
            );
        
            playerSummaries.forEach((s) => {
                csvBuilder.addRow(
                    s.uploadedAt, s.needsUpload ? 'YES' : '', s.tribeName, s.playerName, s.numNukes,
                    s.numAlmostNukes, s.numNukesTraveling, s.numNobles, s.numPossibleNobles,
                    s.numOwnedDVs, s.numDVsAtHome, s.numDVsTraveling, s.numDVsSupportingSelf, s.numDVsSupportingOthers,
                    s.numOffensiveVillas, s.numDefensiveVillas
                );
            });
        
            return csvBuilder.makeCsvString();
        }
    
    
        $doc = $doc || $(document);
    
        $doc.find('#vault-ui-container').remove();
        var $uiContainer = $(`
            <div id="vault-ui-container" class="confirmation-box" style="border-width: 20px">
            <!-- Weird margins on this since TW styling has an annoying gap -->
            <div class="confirmation-box-content-pane" style="min-height:100%;margin-left:-1px;margin-bottom:-1px">
            <div class="confirmation-box-content" style="min-height:100%">
                <h3>Vault</h3>
                <p>
                    Here you can upload data to the Vault or set up SMS notifications. Run this script on your Map or on your Incomings to
                    see everything the Vault has to offer.
                </p>
    
                <button class="btn btn-confirm-yes vault-toggle-uploads-btn">Upload Reports, Troops, Etc.</button>
                <button class="btn btn-confirm-yes vault-toggle-notifications-btn">SMS Notifications</button>
                <button class="btn btn-confirm-yes vault-toggle-admin-btn" style="display:none">Admin Options</button>
                <button class="btn btn-confirm-yes vault-toggle-terms-btn">Disclaimers and Terms</button>
    
                <div style="padding:1em">
    
                    <div id="vault-uploads-container" style="display:none;margin:1em 0;">
                        <p>
                            <strong>Click <em>Upload All</em> below. If needed, upload different things individually using the other Upload buttons.</strong>
                        </p>
    
                        <table style="width:100%" class="vis lit">
                            <tr>
                                <th style="width:12em">Upload</th>
                                <th style="width:6em"></th>
                                <th>Progress</th>
                            </tr>
                            <tr id="vault-upload-reports" class="lit">
                                <td>Reports</td>
                                <td><input type="button" class="details-button" value="Details"></td>
                                <td>
                                    <input type="button" class="upload-button" value="Upload">
                                    <span class="status-container"></span>
                                    <!-- <input type="button" class="cancel-button" value="Cancel" disabled> -->
                                </td>
                            </tr>
                            <tr id="vault-upload-incomings">
                                <td>Incomings</td>
                                <td><input type="button" class="details-button" value="Details"></td>
                                <td>
                                    <input type="button" class="upload-button" value="Upload">
                                    <span class="status-container"></span>
                                    <!-- <input type="button" class="cancel-button" value="Cancel" disabled> -->
                                </td>
                            </tr>
                            <tr id="vault-upload-troops">
                                <td>Troops</td>
                                <td><input type="button" class="details-button" value="Details"></td>
                                <td>
                                    <input type="button" class="upload-button" value="Upload">
                                    <span class="status-container"></span>
                                    <!-- <input type="button" class="cancel-button" value="Cancel" disabled> -->
                                </td>
                            </tr>
                            <tr id="vault-upload-commands">
                                <td>Commands</td>
                                <td><input type="button" class="details-button" value="Details"></td>
                                <td>
                                    <input type="button" class="upload-button" value="Upload">
                                    <span class="status-container"></span>
                                    <!-- <input type="button" class="cancel-button" value="Cancel" disabled> -->
                                </td>
                            </tr>
                            <tr id="vault-upload-all">
                                <td colspan=3 style="text-align:center">
                                    <input type="button" class="upload-button upload-button-all" value="Upload All">
                                </td>
                            </tr>
                        </table>
                    </div>
    
                    <div id="vault-notifications-container" style="padding:1em;display:none">
                        <p>
                            The Vault can send you a text at a certain time. Use this as a reminder for launch times, etc. All
                            phone numbers added here will be texted when a notification is sent.
                        </p>
    
                        <button class="btn btn-confirm-yes notifications-toggle-display-btn">Notifications</button>
                        <button class="btn btn-confirm-yes notifications-toggle-phone-numbers-btn">Phone Numbers</button>
                        <button class="btn btn-confirm-yes notifications-toggle-settings-btn">Settings</button>
    
                        <div style="padding:1em">
                            <div id="vault-notifications-phone-numbers" style="display:none">
                                <h4>Phone Numbers</h4>
                                <p style="text-align: left">
                                    Add a New Number
                                    <br>
                                    <label style="display:inline-block;width:3em;text-align:right" for="new-number">#</label>
                                    <input type="text" id="new-number" placeholder="+1 202-555-0109">
                                    <br>
                                    <label style="display:inline-block;width:3em;text-align:right" for="new-number-label">Name</label>
                                    <input type="text" id="new-number-label" placeholder="(Optional)">
                                    <br>
                                    <button id="add-phone-number">Add</button>
                                </p>
                                <table style="width:100%" class="vis">
                                    <tr>
                                        <th style="width:30%">#</th>
                                        <th></th>
                                        <th style="5em"></th>
                                    </tr>
                                </table>
                            </div>
                            <div id="vault-notifications-settings" style="display:none">
                                <h4>Settings</h4>
                                <div>
                                    <p>
                                        Send me a text <input id="notify-window-minutes" type="text" style="width:2em;text-align:center"> minutes early.
                                    </p>
                                    <button id="save-notification-settings-btn">Save</button>
                                </div>
                            </div>
                            <div id="vault-notifications-display" style="display:none">
                                <h4>Notifications</h4>
                                <p style="text-align:left">
                                    <em>Add New</em>
                                    <br>
                                    <label style="display:inline-block;width:7em;text-align:right" for="notification-time">Server Time</label>
                                    <input type="text" id="notification-time" style="width:400px">
                                    <input type="submit" id="notification-time-formats" value="Supported Formats">
                                    <br>
                                    <label style="display:inline-block;width:7em;text-align:right" for="notification-label">Message</label>
                                    <input type="text" id="notification-label" style="width:400px">
                                    <br>
                                    <button id="add-notification">Add</button>
                                </p>
                                <table style="width:100%" class="vis">
                                    <tr>
                                        <th style="width:12em">Server Time</th>
                                        <th>Message</th>
                                        <th style="width:5em"></th>
                                    </tr>
                                </table>
                            </div>
                        </div>
                    </div>
    
                    <div id="vault-admin-container" style="padding:1em;display:none"></div>
    
                    <div id="vault-disclaimers-and-terms" style="display:none;padding:1em">
                        <p>
                            <em>This tool is not endorsed or developed by InnoGames.</em>
                        </p>
                        <p>
                            <em>
                                All data and requests to the Vault will have various information logged for security. This is limited to:
    
                                Authentication token, IP address, player ID, tribe ID, requested endpoint, and time of transaction.
    
                                Requests to this script will only be IP-logged to protect against abuse. Information collected by this script will never be shared
                                with any third parties or any unauthorized tribes/players.
                            </em>
                        </p>
                    </div>
    
                </div>
    
    
                <p style="font-size:12px">
                    Vault server and script by: Tyler (tcamps/False Duke), Glen (vahtos/TheBossPig)
                    <br>
                    Please report any bugs and feature requests to the maintainers.
                </p>
            </div>
            <div class="confirmation-buttons">
                <button class="btn vault-close-btn btn-confirm-yes">Done</button>
            </div>
            </div>
            </div>
        `.trim()).css({
            position: 'absolute',
            width: '800px',
            margin: 'auto',
            left: 0, right: 0,
            top: '100px',
            'z-index': 999999999
            });
    
        $uiContainer.find('th').css({
            'font-size': '14px'
        });
    
        $uiContainer.find('td').css({
            'font-weight': 'normal'
        });
    
        $uiContainer.find('th, td').css({
            'text-align': 'center'
        });
    
        $uiContainer.find('td').addClass('lit-item');
    
        $uiContainer.find('.upload-button:not(.upload-button-all)').css({
            float: 'left',
            margin: '0 1em'
        });
    
        $uiContainer.find('.cancel-button').css({
            float: 'right',
            margin: '0 1em'
        });
    
        $doc.find('body').prepend($uiContainer);
        processAdminInterface();
    
        function processAdminInterface() {
            lib.getApi(lib.makeApiUrl('admin'))
                .done((data) => {
                    if (typeof data == 'string')
                        data = JSON.parse(data);
    
                    console.log('isAdmin = ', data.isAdmin);
    
                    if (data.isAdmin)
                        makeAdminInterface($uiContainer, $uiContainer.find('#vault-admin-container'));
                });
        }
    
        function setActiveContainer(id) {
            let containerIds = [
                '#vault-uploads-container',
                '#vault-notifications-container',
                '#vault-admin-container',
                '#vault-disclaimers-and-terms'
            ];
    
            containerIds.forEach((cid) => {
                if (cid == id) {
                    $(id).toggle();
                } else {
                    $(cid).css('display', 'none');
                }
            });
        }
    
        $uiContainer.find('.vault-toggle-uploads-btn').click(() => {
            setActiveContainer('#vault-uploads-container');
        });
    
        $uiContainer.find('.vault-toggle-notifications-btn').click(() => {
            setActiveContainer('#vault-notifications-container');
        });
    
        $uiContainer.find('.vault-toggle-admin-btn').click(() => {
            setActiveContainer('#vault-admin-container');
        });
    
        $uiContainer.find('.vault-toggle-terms-btn').click(() => {
            setActiveContainer('#vault-disclaimers-and-terms');
        });
    
        $uiContainer.find('.vault-close-btn').click(() => {
            let isUploading = $('.upload-button').prop('disabled');
            if (isUploading && !confirm("Current uploads will continue running while this popup is closed.")) {
                return;
            }
            $uiContainer.remove()
        });
    
    
    
        var uploadDetailsMessages = {
            'vault-upload-reports': `Uploads all data from all new battle reports.`,
            'vault-upload-incomings': `Uploads all available data from your Incomings page. This includes attacks and support.`,
            'vault-upload-commands': `Uploads all data for all of your current commands.`,
            'vault-upload-troops': `Uploads all data for all troops.`
        };
    
        $uiContainer.find('.details-button').click((ev) => {
            var $el = $(ev.target);
            var uploadType = $el.closest('tr').attr('id');
    
            alert(uploadDetailsMessages[uploadType]);
        });
    
        $uiContainer.find('.upload-button').click((ev) => {
            var $el = $(ev.target);
            var $row = $el.closest('tr');
            var uploadType = $row.attr('id');
    
            var $statusContainer = $row.find('.status-container');
    
            //  TODO - This is messy, clean this up
            let alertCaptcha = () => alert(lib.messages.TRIGGERED_CAPTCHA);
    
            switch (uploadType) {
                default: alert(`Programmer error: no logic for upload type "${uploadType}"!`);
    
                case 'vault-upload-reports':
                    processUploadReports($statusContainer, (didFail) => {
                        $('.upload-button').prop('disabled', false);
                        if (didFail && didFail == lib.errorCodes.CAPTCHA) {
                            alertCaptcha();
                        }
                    });
                    break;
    
                case 'vault-upload-incomings':
                    processUploadIncomings($statusContainer, (didFail) => {
                        $('.upload-button').prop('disabled', false);
                        if (didFail && didFail == lib.errorCodes.CAPTCHA) {
                            alertCaptcha();
                        }
                    });
                    break;
    
                case 'vault-upload-commands':
                    processUploadCommands($statusContainer, (didFail) => {
                        $('.upload-button').prop('disabled', false);
                        if (didFail && didFail == lib.errorCodes.CAPTCHA) {
                            alertCaptcha();
                        }
                    });
                    break;
    
                case 'vault-upload-troops':
                    processUploadTroops($statusContainer, (didFail) => {
                        $('.upload-button').prop('disabled', false);
                        if (didFail && didFail == lib.errorCodes.CAPTCHA) {
                            alertCaptcha();
                        }
                    });
                    break;
    
                case 'vault-upload-all':
                    $('.status-container').html('<em>Waiting...</em>');
    
                    let resetButtons = () => $('.upload-button').prop('disabled', false);
                    let resetStatusContainers = () => {
                        $('.status-container').filter((i, el) => $(el).text().toLowerCase().contains("waiting")).empty();
                    };
    
                    let runReports = () => {
                        processUploadReports($uiContainer.find('#vault-upload-reports .status-container'), runIncomings);
                    };
                    let runIncomings = (didFail) => {
                        if (didFail) {
                            if (didFail == lib.errorCodes.CAPTCHA) {
                                alertCaptcha();
                            } else if (didFail != lib.errorCodes.FILTER_APPLIED) {
                                alert('An unexpected error occurred: ' + didFail);
                            }
                            resetButtons();
                            resetStatusContainers();
                            return;
                        }
                        processUploadIncomings($uiContainer.find('#vault-upload-incomings .status-container'), runTroops);
                    };
                    let runTroops = (didFail) => {
                        if (didFail) {
                            if (didFail == lib.errorCodes.CAPTCHA) {
                                alertCaptcha();
                            } else if (!lib.errorCodes[didFail]) {
                                alert('An unexpected error occurred: ' + didFail);
                            }
                            resetButtons();
                            resetStatusContainers();
                            return;
                        }
                        processUploadTroops($uiContainer.find('#vault-upload-troops .status-container'), runCommands);
                    };
                    let runCommands = (didFail) => {
                        if (didFail) {
                            if (didFail == lib.errorCodes.CAPTCHA) {
                                alertCaptcha();
                            } else if (!lib.errorCodes[didFail]) {
                                alert('An unexpected error occurred: ' + didFail);
                            }
                            resetButtons();
                            resetStatusContainers();
                            return;
                        }
                        processUploadCommands($uiContainer.find('#vault-upload-commands .status-container'), (didFail) => {
                            if (didFail) {
                                if (didFail == lib.errorCodes.CAPTCHA) {
                                    alertCaptcha();
                                } else if (!lib.errorCodes[didFail]) {
                                    alert('An unexpected error occurred: ', didFail);
                                }
                            }
                            resetButtons();
                            resetStatusContainers();
                        });
                    };
    
                    runReports();
                    break;
            }
    
            $('.upload-button').prop('disabled', true);
        });
    
    
        function setActiveNotificationsContainer(id) {
            let containerIds = [
                '#vault-notifications-phone-numbers',
                '#vault-notifications-settings',
                '#vault-notifications-display'
            ];
    
            containerIds.forEach((cid) => {
                if (cid == id) {
                    $(id).toggle();
                } else {
                    $(cid).css('display', 'none');
                }
            });
        }
    
        $uiContainer.find('.notifications-toggle-phone-numbers-btn').click(() => {
            setActiveNotificationsContainer('#vault-notifications-phone-numbers');
        });
    
        $uiContainer.find('.notifications-toggle-settings-btn').click(() => {
            setActiveNotificationsContainer('#vault-notifications-settings');
        });
    
        $uiContainer.find('.notifications-toggle-display-btn').click(() => {
            setActiveNotificationsContainer('#vault-notifications-display');
        });
    
        $uiContainer.find('#add-phone-number').click(() => {
            let $phoneNumber = $uiContainer.find('#new-number');
            let $label = $uiContainer.find('#new-number-label');
    
            let phoneNumber = $phoneNumber.val();
            let label = $label.val();
    
            let trimmedNumber = phoneNumber.replace(/[^\d]/g, '');
            if (trimmedNumber.length < 11) {
                alert('Invalid phone number - must include country code and area code.\n\nie +1 202-555-0109');
                return;
            }
    
            if (label.length > 128) {
                alert(`Phone name is too long - must be less than 128 characters. (Currently ${label.length})`);
                return;
            }
    
            let data = {
                phoneNumber: phoneNumber,
                label: label
            };
    
            $phoneNumber.prop('disabled', true);
            $label.prop('disabled', true);
    
            lib.postApi(lib.makeApiUrl('notification/phone-numbers'), data)
                .done(() => {
                    $phoneNumber.val('');
                    $phoneNumber.prop('disabled', false);
    
                    $label.val('');
                    $label.prop('disabled', false);
    
                    updatePhoneNumbers();
                })
                .error(() => {
                    $phoneNumber.prop('disabled', false);
                    $label.prop('disabled', false);
                    alert('An error occurred.');
                });
        });
    
        $uiContainer.find('#save-notification-settings-btn').click(() => {
            saveNotificationSettings();
        });
    
        $uiContainer.find('#notification-time-formats').click((e) => {
            e.originalEvent.preventDefault();
            alert(`Supported time formats: Basically everything under the sun. Copy/paste whatever you see.`);
        });
    
        $uiContainer.find('#add-notification').click(() => {
            let $notificationTime = $uiContainer.find('#notification-time');
            let $message = $uiContainer.find('#notification-label');
    
            let notificationTimeText = $notificationTime.val().trim();
            let message = $message.val().trim();
    
            if (!message.length) {
                alert('A message is required!');
                return;
            }
    
            if (message.length > 256) {
                alert(`Your message can't be over 256 characters! (Currently ${message.length})`);
                return;
            }
    
            let notificationTime = lib.parseTimeString(notificationTimeText);
            if (!notificationTime) {
                alert('Invalid notification time!');
                return;
            }
    
            let serverTime = lib.getServerDateTime();
            if (serverTime.valueOf() >= notificationTime.valueOf()) {
                alert("Your notification time must be *after* the current server time!");
                return;
            }
    
            let data = {
                eventOccursAt: notificationTime,
                message: message
            };
    
            $notificationTime.prop('disabled', true);
            $message.prop('disabled', true);
            $uiContainer.find('#add-notification').prop('disabled', true);
    
            lib.postApi(lib.makeApiUrl('notification/requests'), data)
                .done(() => {
                    loadNotifications()
                        .done(() => {
                            $notificationTime.prop('disabled', false);
                            $message.prop('disabled', false);
                            $uiContainer.find('#add-notification').prop('disabled', false);
    
                            $notificationTime.val('');
                        })
                        .error(() => {
                            $notificationTime.prop('disabled', false);
                            $message.prop('disabled', false);
                            $uiContainer.find('#add-notification').prop('disabled', false);
                        })
                })
                .error(() => {
                    $notificationTime.prop('disabled', false);
                    $message.prop('disabled', false);
                    $uiContainer.find('#add-notification').prop('disabled', false);
                    alert('An error occurred.');
                });
        });
    
    
    
    
        updatePhoneNumbers();
        loadNotificationSettings();
        loadNotifications();
    
        function updatePhoneNumbers() {
            lib.getApi(lib.makeApiUrl('notification/phone-numbers'))
                .done((phoneNumbers) => {
                    let $phoneNumbersTable = $uiContainer.find('#vault-notifications-phone-numbers table');
                    $phoneNumbersTable.find('tr:not(:first-of-type)').remove();
    
                    phoneNumbers.forEach((number) => {
                        let $row = $(`
                            <tr data-id="${number.id}">
                                <td>${number.number}</td>
                                <td>${number.label}</td>
                                <td><input type="submit" value="Delete"></td>
                            </tr>
                        `.trim());
                        $phoneNumbersTable.append($row);
                        $row.find('input').click((ev) => {
                            ev.originalEvent.preventDefault();
                            if (!confirm(`Are you sure you want to remove the number ${number.number}?`)) {
                                return;
                            }
    
                            lib.deleteApi(lib.makeApiUrl('notification/phone-numbers/' + number.id))
                                .done(() => {
                                    updatePhoneNumbers();
                                })
                                .error(() => {
                                    alert('An error occurred.');
                                });
                        });
                    });
                })
                .error(() => {
                    alert('An error occurred while getting your phone numbers.');
                });
        }
    
        function loadNotificationSettings() {
            lib.getApi(lib.makeApiUrl('notification/settings'))
                .done((data) => {
                    $uiContainer.find('#notify-window-minutes').val(data.sendNotificationBeforeMinutes);
                })
                .error(() => {
                    alert('An error occurred.');
                });
        }
    
        function saveNotificationSettings() {
            let $notificationWindow = $uiContainer.find('#notify-window-minutes');
            let notificationWindow = $notificationWindow.val().trim();
            if (!notificationWindow.length) {
                alert("Empty settings value!");
                return;
            }
    
            if (notificationWindow.match(/[^\d]/)) {
                alert("Invalid settings value!");
                return;
            }
    
            let data = {
                sendNotificationBeforeMinutes: parseInt(notificationWindow)
            };
    
            $notificationWindow.prop('disabled', true);
            $uiContainer.find('#save-notification-settings-btn').prop('disabled', true);
    
            lib.postApi(lib.makeApiUrl('notification/settings'), data)
                .done(() => {
                    $notificationWindow.prop('disabled', false);
                    $uiContainer.find('#save-notification-settings-btn').prop('disabled', false);
                })
                .error(() => {
                    $notificationWindow.prop('disabled', false);
                    $uiContainer.find('#save-notification-settings-btn').prop('disabled', false);
                    alert('An error occurred.');
                });
        }
    
        function loadNotifications() {
            return lib.getApi(lib.makeApiUrl('notification/requests'))
                .done((requests) => {
                    let $notificationsTable = $uiContainer.find('#vault-notifications-display table');
                    $notificationsTable.find('tr:not(:first-of-type)').remove();
    
                    requests.forEach((request) => {
                        request.eventOccursAt = new Date(Date.parse(request.eventOccursAt));
    
                        let $row = $(`
                            <tr data-id="${request.id}">
                                <td>${lib.formateDateTime(request.eventOccursAt)}</td>
                                <td>${request.message}</td>
                                <td><input type="submit" value="Delete"></td>
                            </tr>
                        `.trim());
    
                        $row.find('input').click((ev) => {
                            ev.originalEvent.preventDefault();
    
                            let confirmInfo = `"${request.message}" at ${lib.formateDateTime(request.eventOccursAt)}`;
                            if (!confirm(`Are you sure you want to delete this notification?\n\n${confirmInfo}`)) {
                                return;
                            }
    
                            lib.deleteApi(lib.makeApiUrl(`notification/requests/${request.id}`))
                                .done(() => {
                                    loadNotifications();
                                })
                                .error(() => {
                                    alert('An error occurred.');
                                });
                        });
    
                        $notificationsTable.append($row);
                    });
                })
                .error(() => {
                    alert("An error occurred while loading notification requests.");
                });
        }
    }

    function tagOnIncomingsOverviewPage($doc) {
        $doc = $doc || $(document);
    
        //  Prevent auto-reload when incomings land (it erases the UI and messes with tagging)
        window.partialReload = function () {}
    
        let defaultFormat = `
            %troopName% %tagType% Pop: %popPerc%% Cats: %numCats% Com:1/%numComs%
        `.trim();
    
        let rateLimiter = new RateLimiter();
    
        let incomings = [];
        let originalLabels = lib.getLocalStorage('original-labels', {});
        let settings = lib.getLocalStorage('tag-settings', {
            tagFormat: defaultFormat,
            ignoreMissingData: true,
            autoLabelFakes: true,
            maxFakePop: 5
        });
    
        let $incomingRows = $doc.find('#incomings_table tr:not(:first-of-type):not(:last-of-type)');
        foreachCommand((cmd) => {
            cmd.wasLabeled = !!originalLabels[cmd.id];
            incomings.push(cmd);
        });
    
        let incomingTags = null;
    
        console.log('Got incomings: ', incomings);
    
        getVaultTags(() => {
            makeTaggingUI();
        });
    
        foreachCommand((cmd) => {
            if (!originalLabels[cmd.id])
                originalLabels[cmd.id] = {
                    label: cmd.label.trim(),
                    arrivesAt: cmd.arrivesAt
                };
        });
    
        (() => {
            let serverTime = lib.getServerDateTime();
            let oldIncomingIds = [];
            lib.objForEach(originalLabels, (prop, val) => {
                if (serverTime.valueOf() - val.arrivesAt.valueOf() > 0)
                    oldIncomingIds.push(prop);
            });
            console.log('Removing passed incomings: ', oldIncomingIds);
            oldIncomingIds.forEach((id) => {
                delete originalLabels[id];
            });
        })();
    
        lib.setLocalStorage('original-labels', originalLabels);
    
        function getVaultTags(onDone_) {
            lib.postApi(lib.makeApiUrl('command/tags'), incomings.map((i) => i.id))
                .done((tags) => {
                    console.log('Got tags: ', tags);
                    incomingTags = tags;
                    onDone_ && onDone_();
                })
                .error((xhr, b, c) => {
                    if (xhr.status == 423) {
                        let reasons = null;
                        try {
                            reasons = JSON.parse(xhr.responseText);
                        } catch (_) { }
    
                        let alertMessage = "You haven't uploaded data in a while, you can't use tagging until you do."
                        if (reasons) {
                            alertMessage += `\nYou need to upload: ${reasons.join(', ')}`;
                        }
    
                        alert(alertMessage);
                        parseAllPages();
                    } else if (xhr.status != 401) {
                        alert("An error occurred...");
                    }
                });
        }
    
        function makeTaggingUI() {
            $('#v-tagging-ui').remove();
            let $container = $(`
                <div id="v-tagging-ui" class="content-border">
                    <h3>Vault Tagging</h3>
                    <p>
                        <button id="v-upload-incomings">Upload Incomings</button>
                    </p>
                    <p>
                        <table class="vis">
                            <tr>
                                <th>Code</th>
                                <th>Details</th>
                            </tr>
                            <tr class="row_a">
                                <td>%troopName%</td>
                                <td>Best known troop type (from your label or auto-calculated)</td>
                            </tr>
                            <tr class="row_b">
                                <td>%tagType%</td>
                                <td>One of: Fake, Nuke</td>
                            </tr>
                            <tr class="row_a">
                                <td>%popPerc%</td>
                                <td>% of a full nuke known at the village, ie 89% or ?%</td>
                            </tr>
                            <tr class="row_b">
                                <td>%popCnt%</td>
                                <td>Offensive pop known at the village, ie 19.2k or ?k</td>
                            </tr>
                            <tr class="row_a">
                                <td>%numCats%</td>
                                <td># of catapults known at the village</td>
                            </tr>
                            <tr class="row_b">
                                <td>%numComs%</td>
                                <td># of total commands from the village to the tribe</td>
                            </tr>
                        </table>
                    </p>
                    <p>
                        <label for="v-tag-format">Tag format: </label>
                        <input type="text" id="v-tag-format" style="width:40em">
                        <button id="v-reset-format">Reset</button>
                    </p>
                    <p>
                        <input type="checkbox" id="v-autoset-fakes" ${settings.autoLabelFakes ? 'checked' : ''}>
                        <label for="v-autoset-fakes">
                            Label as "Fake" if less than <input id="v-max-fake-pop" type="text" style="width:2em; text-align:center" value="${settings.maxFakePop}"> thousand offense population
                        <label>
                    </p>
                    <p>
                        <input type="checkbox" id="v-ignore-missing" ${settings.ignoreMissingData ? 'checked' : ''}>
                        <label for="v-ignore-missing">
                            Ignore incomings without data
                        </label>
                    <p>
                        <input type="checkbox" id="v-preview">
                        <label for="v-preview">Preview</label>
                    </p>
                    <p>
                        <button id="v-tag-all">Tag All</button>
                        <span class="v-sep"></span>
                        <button id="v-tag-selected">Tag Selected</button>
                        <span class="v-sep"></span>
                        <button id="v-revert-tagging">Revert to Old Tags</button>
                    </p>
                    <p>
                        <button id="v-cancel" disabled>Cancel</button>
                    </p>
                    <p>
                        <em>Tagging will take a while!</em>
                    </p>
                    <p id="v-tag-status">
                    </p>
                </div>
            `.trim());
    
            $container.css({
                padding: '1em',
                margin: '1em'
            });
    
            $container.find('.v-sep').css({
                display: 'inline-block',
                width: '1.5em'
            });
    
            $container.find('#v-tag-format').val(settings.tagFormat);
    
            $('#incomings_table').before($container);
    
            $container.find('#v-tag-format').focusout(() => {
                let newFormat = $container.find('#v-tag-format').val();
                settings.tagFormat = newFormat;
                saveSettings();
            });
    
            $container.find('#v-autoset-fakes').change(() => {
                settings.autoLabelFakes = $container.find('#v-autoset-fakes').is(':checked');
                saveSettings();
            });
    
            $container.find('#v-ignore-missing').change(() => {
                settings.ignoreMissingData = $container.find('#v-ignore-missing').is(':checked');
                saveSettings();
            });
    
            $container.find('#v-reset-format').click((e) => {
                e.originalEvent.preventDefault();
                settings.tagFormat = defaultFormat;
                $container.find('#v-tag-format').val(settings.tagFormat);
                saveSettings();
            });
    
            $container.find('#v-max-fake-pop').focusout(() => {
                let $maxFakePop = $container.find('#v-max-fake-pop');
                let maxPopText = $maxFakePop.val();
                let maxPop = parseInt(maxPopText);
                if (isNaN(maxPop) || maxPopText.match(/[^\d]/)) {
                    alert("That's not a number!");
                    $maxFakePop.val(settings.maxFakePop);
                    return;
                }
    
                settings.maxFakePop = maxPop;
                saveSettings();
            });
    
            $container.find('#v-upload-incomings').click((e) => {
                e.originalEvent.preventDefault();
                parseAllPages();
                //  Annoying to need to hook into specific UI element from main UI but whatever
                $('.vault-close-btn').click(() => {
                    getVaultTags();
                });
            });
    
            let oldLabels = null;
            $container.find('#v-preview').change(() => {
                if ($('#v-preview').is(':checked')) {
                    toggleUploadButtons(false);
                    $('#v-cancel').prop('disabled', true);
                    $('#v-preview').prop('disabled', false);
    
                    oldLabels = {};
    
                    let selectedIncomings = getSelectedIncomingIds();
                    if (!selectedIncomings.length)
                        selectedIncomings = incomings.map((i) => i.id);
                    console.log('Selected: ', selectedIncomings);
    
                    foreachCommand((cmd) => {
                        if (settings.ignoreMissingData && !incomingTags[cmd.id])
                            return;
    
                        if (!selectedIncomings.contains(cmd.id))
                            return;
    
                        let $label = cmd.$row.find('.quickedit-label');
                        let originalLabel = $label.text().trim();
    
                        oldLabels[cmd.id] = originalLabel;
    
                        let newLabel = makeLabel(incomingTags[cmd.id]);
                        if (newLabel)
                            $label.text(newLabel);
                    });
                } else {
                    toggleUploadButtons(true);
    
                    foreachCommand((cmd) => {
                        if (oldLabels[cmd.id]) {
                            cmd.$row.find('.quickedit-label').text(oldLabels[cmd.id]);
                        }
                    });
    
                    oldLabels = null;
                }
            });
    
            $container.find('#v-tag-all').click((e) => {
                e.originalEvent.preventDefault();
                beginTagging(incomings.map((i) => i.id));
            });
    
            $container.find('#v-tag-selected').click((e) => {
                e.originalEvent.preventDefault();
                let selectedIds = getSelectedIncomingIds();
                if (!selectedIds.length)
                    alert("You didn't select any incomings!");
                beginTagging(selectedIds);
            });
    
            $container.find('#v-revert-tagging').click((e) => {
                e.originalEvent.preventDefault();
                let selectedIds = getSelectedIncomingIds();
                if (!selectedIds.length)
                    selectedIds = incomings.map((i) => i.id);
    
                rateLimiter.resetStats();
                selectedIds.forEach((id) => {
    
                    let cmd = incomings.find((i) => i.id == id);
                    let $label = cmd.$row.find('.quickedit-label');
                    let newLabel = originalLabels[id].label;
    
                    if (newLabel == $label.text().trim())
                        return;
    
                    rateLimiter.addTask(() => {
                        renameIncoming(id, newLabel, () => {
                            if (rateLimiter.isRunning()) {
                                alert(lib.messages.TRIGGERED_CAPTCHA);
                                rateLimiter.stop();
                                updateTagStatus(lib.messages.TRIGGERED_CAPTCHA);
                                toggleUploadButtons(true);
                            }
                        }).success(() => {
                            $label.text(newLabel);
                            updateTagStatus();
                        });
                    });
                    
                });
    
                rateLimiter.setFinishedHandler(() => {
                    updateTagStatus();
                    toggleUploadButtons(true);
                });
    
                if (rateLimiter.getStats().total > 0) {
                    rateLimiter.start();
                } else {
                    toggleUploadButtons(true);
                    updateTagStatus("Either no incomings or all tags are current");
                }
            });
    
            $container.find('#v-cancel').click((e) => {
                e.originalEvent.preventDefault();
                rateLimiter.stop();
                toggleUploadButtons(true);
                updateTagStatus("Tagging canceled");
            });
        }
    
        function beginTagging(commandIds) {
            toggleUploadButtons(false);
    
            console.log('Starting tagging for: ', commandIds);
            rateLimiter.resetStats();
            commandIds.forEach((id) => {
                if (settings.ignoreMissingData && !incomingTags[id])
                    return;
    
                let cmd = incomings.find((i) => i.id == id);
                let $label = cmd.$row.find('.quickedit-label');
                let newLabel = makeLabel(incomingTags[id]);
    
                if (!newLabel)
                    return;
    
                if (newLabel == $label.text().trim())
                    return;
    
                rateLimiter.addTask(() => {
                    renameIncoming(id, newLabel, () => {
                        if (rateLimiter.isRunning()) {
                            alert(lib.messages.TRIGGERED_CAPTCHA);
                            rateLimiter.stop();
                            updateTagStatus(lib.messages.TRIGGERED_CAPTCHA);
                            toggleUploadButtons(true);
                        }
                    }).success(() => {
                        $label.text(newLabel);
                        updateTagStatus();
                    });
                });
            });
    
            rateLimiter.setFinishedHandler(() => {
                updateTagStatus();
                toggleUploadButtons(true);
            });
    
            if (rateLimiter.getStats().total > 0) {
                rateLimiter.start();
            } else {
                toggleUploadButtons(true);
                updateTagStatus("Either no incomings or all tags are current");
            }
        }
    
        function updateTagStatus(msg_) {
            let stats = rateLimiter.getStats();
            let $tagStatus = $('#v-tag-status');
            if (msg_) {
                $tagStatus.text(msg_);
            } else if (stats.total) {
                let progress = `${stats.done}/${stats.total} tagged (${stats.numFailed} failed)`;
                if (stats.total == stats.done) {
                    progress = `Finished: ${progress}`;
                } else if (rateLimiter.isRunning()) {
                    progress = `Tagging: ${progress}`;
                } else {
                    progress = `Canceled: ${progress}`;
                }
                $tagStatus.text(progress);
            } else {
                $tagStatus.text('');
            }
        }
    
        function getSelectedIncomingIds() {
            let $selectedRows = $incomingRows.filter((i, el) => !!$(el).find('input:checked').length);
            let selectedIds = [];
            $selectedRows.each((i, el) => {
                let $row = $(el);
                let $link = $row.find('a[href*=info_command][href*=id]');
                let commandId = $link.prop('href').match(/id=(\w+)/)[1];
                selectedIds.push(parseInt(commandId));
            });
            return selectedIds;
        }
    
        function foreachCommand(callback) {
            $incomingRows.each((i, row) => {
                let $row = $(row);
                let $link = $row.find('a[href*=info_command][href*=id]');
                let label = $link.text().trim();
                let commandId = $link.prop('href').match(/id=(\w+)/)[1];
                let arrivesAtText = $row.find('td:nth-of-type(6)').text().trim();
                let arrivesAt = lib.parseTimeString(arrivesAtText);
                let data = {
                    id: parseInt(commandId),
                    label: label,
                    arrivesAt: arrivesAt,
                    $row: $row
                };
                callback(data);
            });
        }
    
        function makeLabel(incomingData) {
            let hasData =
                (typeof incomingData.offensivePopulation != 'undefined' && incomingData.offensivePopulation != null) ||
                (typeof incomingData.numCats != 'undefined' && incomingData.numCats != null) ||
                incomingData.numFromVillage > 1;
    
            if (!hasData && settings.ignoreMissingData)
                return null;
    
            let format = settings.tagFormat;
            let missingNukePop = typeof incomingData.offensivePopulation == 'undefined' || incomingData.offensivePopulation == null;
            let missingNumCats = typeof incomingData.numCats == 'undefined' || incomingData.numCats == null;
            let troopTypeName = incomingData.troopType ? (
                lib.twstats.getUnit(incomingData.troopType).name
            ) : 'Unknown';
    
            let maxNukePop = 20000;
            let nukePop = Math.min(maxNukePop, incomingData.offensivePopulation || 0);
            let nukePopK = Math.roundTo(nukePop / 1000, 1);
            let nukePopPerc = Math.roundTo(nukePop / maxNukePop * 100, 1);
    
            if (settings.autoLabelFakes && !missingNukePop && nukePopK < settings.maxFakePop) {
                return 'Fake';
            }
    
            return format
                .replace("%troopName%", troopTypeName)
                .replace("%tagType%", incomingData.definiteFake ? 'Fake' : 'Nuke?')
                .replace("%popPerc%", missingNukePop ? '?' : nukePopPerc)
                .replace("%popCnt%", missingNukePop ? '?' : nukePopK)
                .replace("%numCats%", missingNumCats ? '?' : incomingData.numCats)
                .replace("%numComs%", incomingData.numFromVillage)
            ;
        }
    
        function renameIncoming(incomingId, newName, onCaptcha_) {
            let clientTime = Math.round(new Date().valueOf() / 1000);
            let csrf = window.csrf_token;
            let twUrl = `screen=info_command&ajaxaction=edit_other_comment&id=${incomingId}&h=${csrf}&&client_time=${clientTime}`;
            return $.ajax({
                url: lib.makeTwUrl(twUrl),
                method: 'POST',
                dataType: "json",
                data: { text: newName },
                headers: { 'TribalWars-Ajax': 1 },
                success: (data) => {
                    if (data && typeof data == 'string') try {
                        data = JSON.parse(data);
                    } catch (_) { }
    
                    if (data && !data.response && data.bot_protect) {
                        onCaptcha_ && onCaptcha_();
                    }
                }
            });
        }
    
        function toggleUploadButtons(enabled) {
            let inputIds = [
                '#v-upload-incomings',
                '#v-tag-format',
                '#v-reset-format',
                '#v-autoset-fakes',
                '#v-max-fake-pop',
                '#v-ignore-missing',
                '#v-preview',
                '#v-tag-all',
                '#v-tag-selected',
                '#v-revert-tagging'
            ];
    
            if (enabled) {
                inputIds.forEach((id) => $(id).prop('disabled', false));
                $('#v-cancel').prop('disabled', true);
            } else {
                inputIds.forEach((id) => $(id).prop('disabled', true));
                $('#v-cancel').prop('disabled', false);
            }
        }
    
        function saveSettings() {
            lib.setLocalStorage('tag-settings', settings);
        }
    }


    //  Store current script host for dependent scripts that rely on it
    lib.setScriptHost(lib.getScriptHost());

    if (!lib.checkUserHasPremium()) {
        alert('This script cannot be used without a premium account!');
        return;
    }

    lib.init(() => {

        let terms = `

This script serves as an interface to the Vault, a private tool for collecting Tribal Wars data.

All data and requests to the Vault will have various information logged for security. This is limited to:
- Authentication token
- IP address
- Player ID
- Tribe ID
- Requested endpoint
- Time of transaction

Requests to this script will only be IP-logged to protect against abuse. Information collected by this script will never be shared
with any third parties or any unauthorized tribes/players.

These terms can be viewed again after running the script. To cancel your agreement, do not run this script again.

`;

        let isFirstRun = lib.onFirstRun((onAccepted) => {
            let alertString =
                "This is your first time running the script - please see the terms and conditions on DATA COLLECTION below.\n\n"
                + terms.trim()
                + "\n\nAgree to these terms?";

            if (confirm(alertString)) {
                alert('Thank you, please run the script again to start using it.');
                onAccepted();
            } else {
                alert('The script will not be ran.');
            }
        });

        if (isFirstRun) {
            return;
        }

        if (lib.versioning.checkNeedsUpdate()) {
            alert('The vault was recently updated, you will need to re-upload some data.');
            lib.versioning.updateForLatestVersion();
        }

        lib
            //.onPage(lib.pageTypes.UNKNOWN, () => {
            //    var supportedPages = lib.objectToArray(lib.pageTypes, (v) => v != lib.pageTypes.UNKNOWN ? v.replace(/\_/g, ' ').toLowerCase() : undefined);

            //    alert("This script can't be ran on the current page. It can be ran on these pages: \n\n" + supportedPages.join("\n"));
            //})
            .onPage(lib.pageTypes.MAP, () => {
                parseMapPage();
            })
            // .onPage(lib.pageTypes.VIEW_REPORT, () => {
            //     let $doc = $(document);
            //     let href =  window.location.href;
            //     parseReportPage($doc, href, true, () => {
            //         //  onError
            //         console.log("Report upload FAILED.")
            //         //toggleReport($el, false);
            //     });
            // })
            .onPage(lib.pageTypes.INCOMINGS_OVERVIEW, () => {
                tagOnIncomingsOverviewPage();
            })
            .onPageNotHandled(() => {
                parseAllPages();
            })
        ;
    });

})();
//# sourceURL=https://v.tylercamp.me/dev/script/main.js