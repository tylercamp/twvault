; (function () {

    function getCookie(name) {
        var match;
        return (match = document.cookie.match(new RegExp(name + '=([^\s\;]+)'))) ? match[1] : null;
    }

    function setCookie(name, value) {
        if (!value) value = '';
        if (!(typeof value == 'string')) value = JSON.stringify(value);
        document.cookie = name + '=' + value;
    }

    function objForEach(obj, callback) {
        for (var prop in obj) {
            if (!obj.hasOwnProperty(prop)) continue;

            callback(
                prop,
                obj[prop],
                obj
            );
        }
    };

    window.vaultFakes = window.vaultFakes || {
        troopCounts: [
            { catapult: 1, spy: 1 },
            { ram: 1, spy: 1 },
            { catapult: 1 },
            { ram: 1 }
        ]
    };

    window.vaultFakes.targets = window.vaultFakes.targets || "{TARGET_COORDS}";

    window.vaultFakes.cookie = window.vaultFakes.cookie || '{COOKIE}';

    console.log('Using window.vaultFakes = ', vaultFakes);

    var targets = (function () {
        var matcher, targets = [], regex = /(\d+\|\d+)/g;
        while (matcher = regex.exec(vaultFakes.targets)) {
            targets.push(matcher[1]);
        }
        return targets;
    })();

    console.log('Parsed targets: ', targets);

    var href = window.location.href;
    if (href.indexOf('screen=place') < 0 ||
        (href.indexOf('mode=') >= 0 && href.indexOf('mode=command') < 0)) {

        alert('Run this on the Rally Point Commands page!');
        return;
    }

    var troopCounts = {};
    $('a[id^=units_entry_all_]').each(function (i, el) {
        var id = $(el).attr('id');
        var unit = id.match(/units_entry_all_(\w+)/)[1];
        var count = $(el).text();
        count = count.match(/\((\d+)\)/)[1];
        troopCounts[unit] = parseInt(count);
    });

    console.log('Got troop counts: ', troopCounts);

    var troops = makeMatchedTroops();
    updateTroops(troops);
    insertTarget();
    
    function makeMatchedTroops() {
        if (!(vaultFakes.troopCounts instanceof Array)) return;

        var result;

        vaultFakes.troopCounts.forEach(function (build) {
            if (result) return;

            var canUse = true;
            objForEach(build, function (troopName, cnt) {
                if (troopCounts[troopName] < cnt)
                    canUse = false
            });
            if (!canUse) return;

            result = build;
        });

        return result;
    }

    function updateTroops(troops) {
        console.log('Updating troop inputs');
        objForEach(troops, function (name, value) {
            document.forms[0][name].value = value;
        });
    }

    function insertTarget() {
        var lastTarget = getCookie(vaultFakes.cookie);
        var target;
        if (lastTarget) {
            var targetIdx = targets.indexOf(lastTarget);
            if (targetIdx < 0) {
                target = targets[0];
            } else if (targetIdx >= targets.length - 1) {
                target = targets[0];
                if (targets.length > 1) {
                    alert('Reached the end of the list, starting from the beginning!');
                }
            } else {
                target = targets[targetIdx + 1];
            }
        } else {
            target = targets[0];
        }

        if (target) {
            document.forms[0].x.value = target.split('|')[0];
            document.forms[0].y.value = target.split('|')[1];
            $('#place_target').val(target);
            setCookie(vaultFakes.cookie, target);
            return true;
        } else {
            return false;
        }
    }

})();