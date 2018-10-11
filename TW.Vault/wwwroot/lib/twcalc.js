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
        },

        totalAttackPower: function calculateTotalAttackPower(army) {
            army = normalizeToArray(army);
            var power = 0;
            army.forEach((t) => {
                let name = t.name;
                let count = t.count;

                let unit = twstats.getUnit(name);
                power += unit.attack * count;
            });
            return power;
        },

        totalDefensePower: function calculateTotalDefensePower(army) {
            army = normalizeToArray(army);
            var power = 0;
            army.forEach((t) => {
                let name = t.name;
                let count = t.count;

                let unit = twstats.getUnit(name);
                power += unit.defense[0].value * count;
                power += unit.defense[1].value * count;
                power += unit.defense[2].value * count;
            });
            return power;
        }
    };
}