(() => {
    //# REQUIRE lib/lib.js

    let scopes = [];
    let numPassed = 0, numFailed = 0;
    console.log('Starting unit tests.');

    let $testDoc = $(`
    <div>
        <div id="serverDate">1/11/2018</div>
        <div id="serverTime">0:0:0</div>
    </div>
    `.trim());

    lib.setCurrentTranslation(1);

    lib.getCurrentTranslationAsync(() => {

        test("Date parsing", () => {
            //  JS dates have months - 1, so 10 = Nov.
            let fullDate = new Date(Date.UTC(2018, 10, 1, 1, 13, 54, 233));
            let dateNoMs = new Date(Date.UTC(2018, 10, 1, 1, 13, 54, 0));
            let dateNoSecs = new Date(Date.UTC(2018, 10, 1, 1, 13, 0, 0));
            let tomorrowFullDate = new Date(Date.UTC(2018, 10, 2, 1, 13, 54, 233));

            test("Date Time", () => {
                let timeString = "01/11/2018 1:13:54:233";
                let parsed = lib.parseTimeString(timeString);
                assertEquals(fullDate, parsed);
            });

            test("Date[Periods] Time", () => {
                let timeString = "01.11.2018 1:13:54:233";
                let parsed = lib.parseTimeString(timeString);
                assertEquals(fullDate, parsed);
            });

            test("Date[Periods+End] Time", () => {
                let timeString = "01.11.2018. 1:13:54:233";
                let parsed = lib.parseTimeString(timeString);
                assertEquals(fullDate, parsed);
            });

            test("Mon Day, Time (no sec/ms)", () => {
                let timeString = "Nov 1, 1:13";
                let parsed = lib.parseTimeString(timeString, false, $testDoc);
                assertEquals(dateNoSecs, parsed);
            });

            test("Mon. Day, Time (no sec/ms)", () => {
                let timeString = "Nov. 1, 1:13";
                let parsed = lib.parseTimeString(timeString, false, $testDoc);
                assertEquals(dateNoSecs, parsed);
            });

            test("Time Date", () => {
                let timeString = "1:13:54:233 01/11/2018";
                let parsed = lib.parseTimeString(timeString);
                assertEquals(fullDate, parsed);
            });

            test("Date Time (no ms)", () => {
                let timeString = "01/11/2018 1:13:54";
                let parsed = lib.parseTimeString(timeString);
                assertEquals(dateNoMs, parsed);
            });

            test("Date Time (no year)", () => {
                let timeString = "01/11 1:13:54:233";
                let parsed = lib.parseTimeString(timeString, false, $testDoc);
                assertEquals(fullDate, parsed);
            });

            test("Today at Time", () => {
                let timeString = "today at 1:13:54:233";
                let parsed = lib.parseTimeString(timeString, false, $testDoc);
                assertEquals(fullDate, parsed);
            });

            test("Today at Time (Missing ms)", () => {
                let timeString = "today at 1:13:54";
                let parsed = lib.parseTimeString(timeString, false, $testDoc);
                assertEquals(dateNoMs, parsed);
            });

            test("Today at Time (Missing mins/ms)", () => {
                let timeString = "today at 1:13";
                let parsed = lib.parseTimeString(timeString, false, $testDoc);
                assertEquals(dateNoSecs, parsed);
            });

            test("Tomorrow at Time", () => {
                let timeString = "tomorrow at 1:13:54:233";
                let parsed = lib.parseTimeString(timeString, false, $testDoc);
                assertEquals(tomorrowFullDate, parsed);
            });

            test("On Date at Time", () => {
                let timeString = "on 1/11/2018 at 1:13:54:233";
                let parsed = lib.parseTimeString(timeString);
                assertEquals(fullDate, parsed);
            });

            test("On Date[Periods] at Time", () => {
                let timeString = "on 1.11.2018 at 1:13:54:233";
                let parsed = lib.parseTimeString(timeString);
                assertEquals(fullDate, parsed);
            });

            test("On Date[Periods] at Time (no year)", () => {
                let timeString = "on 1.11 at 1:13:54:233";
                let parsed = lib.parseTimeString(timeString, false, $testDoc);
                assertEquals(fullDate, parsed);
            });
        });
    });










    function test(name, unitTest) {
        scopes.push(name);
        unitTest();
        scopes.pop();
    }

    function assertEquals(expected, value) {
        let printLabel = scopes.join(' : ');
        if (compare(expected, value)) {
            console.log(`${printLabel} - Passed`);
        } else {
            console.error(`${printLabel} - FAILED; Expected `, expected, ' got ', value);
        }




        function compare(a, b) {
            if ((a == null) != (b == null))
                return false;
            if (typeof a != typeof b)
                return false;
            if (a instanceof Date)
                return a.valueOf() == b.valueOf();

            return a == b;
        }
    }
})();