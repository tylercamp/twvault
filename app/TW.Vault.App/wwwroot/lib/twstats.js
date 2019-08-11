function getTwTroopStats() {

    var troopStats = {

        _updateWithSettings: function (archersEnabled, militiaEnabled, paladinEnabled) {
            let removedTypes = [];
            troopStats.unitTypes.forEach((type) => {
                switch (type.canonicalName) {
                    case 'militia':
                        if (!militiaEnabled)
                            removedTypes.push(type);
                        break;

                    case 'knight':
                        if (!paladinEnabled)
                            removedTypes.push(type);
                        break;

                    case 'archer':
                    case 'marcher':
                        if (!archersEnabled)
                            removedTypes.push(type);
                        break;
                }
            });

            removedTypes.forEach((type) => {
                troopStats.unitTypes.splice(troopStats.unitTypes.indexOf(type), 1);
            });

            troopStats.archersEnabled = archersEnabled;
            troopStats.militiaEnabled = militiaEnabled;
            troopStats.paladinEnabled = paladinEnabled;
        },

        archersEnabled: true,
        militiaEnabled: true,
        paladinEnabled: true,

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
                itlCode: 'UNIT_SPEAR',
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
                icon: "https://tylercamp.me/tw/img/unit_spear.png"
            },
            {
                name: "Sword",
                canonicalName: 'sword',
                itlCode: 'UNIT_SWORD',
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
                icon: "https://tylercamp.me/tw/img/unit_sword.png"
            },
            {
                name: "Axe",
                canonicalName: 'axe',
                itlCode: 'UNIT_AXE',
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
                icon: "https://tylercamp.me/tw/img/unit_axe.png"
            },
            {
                name: "Archer",
                canonicalName: 'archer',
                itlCode: 'UNIT_ARCHER',
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
                icon: "https://tylercamp.me/tw/img/unit_archer.png"
            },
            {
                name: "Scout",
                canonicalName: 'spy',
                itlCode: 'UNIT_SPY',
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
                icon: "https://tylercamp.me/tw/img/unit_spy.png"
            },
            {
                name: "Light Cav.",
                canonicalName: 'light',
                itlCode: 'UNIT_LIGHT_CAV',
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
                icon: "https://tylercamp.me/tw/img/unit_light.png"
            },
            {
                name: "Mounted Ar.",
                canonicalName: 'marcher',
                itlCode: 'UNIT_M_ARCHER',
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
                icon: "https://tylercamp.me/tw/img/unit_marcher.png"
            },
            {
                name: "Heavy Cav.",
                canonicalName: 'heavy',
                itlCode: 'UNIT_HEAVY_CAV',
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
                icon: "https://tylercamp.me/tw/img/unit_heavy.png"
            },
            {
                name: "Ram",
                canonicalName: 'ram',
                itlCode: 'UNIT_RAM',
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
                icon: "https://tylercamp.me/tw/img/unit_ram.png"
            },
            {
                name: "Catapult",
                canonicalName: 'catapult',
                itlCode: 'UNIT_CATAPULT',
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
                icon: "https://tylercamp.me/tw/img/unit_catapult.png"
            },
            {
                name: "Paladin",
                canonicalName: 'knight',
                itlCode: 'UNIT_PALADIN',
                shorthand: 'pally',
                class: "Infantry",
                attack: 150,
                population: 10,
                travelSpeed: 10,
                build: 'Defensive',
                defense: [{ class: "Infantry", value: 250 }, { class: "Cavalry", value: 400 }, { class: "Archer", value: 150 }],
                cost: [{ resource: "Wood", amount: 20 }, { resource: "Clay", amount: 20 }, { resource: "Iron", amount: 40 }],
                aliases: ['paladin', 'pallys', 'pally', 'paly', 'pal'],
                icon: "https://tylercamp.me/tw/img/unit_knight.png"
            },
            {
                name: "Nobleman",
                canonicalName: 'snob',
                itlCode: 'UNIT_NOBLE',
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
                icon: "https://tylercamp.me/tw/img/unit_snob.png"
            },
            {
                name: "Militia",
                canonicalName: 'militia',
                itlCode: 'UNIT_MILITIA',
                shorthand: 'militia',
                class: "Infantry",
                attack: 0,
                population: 0,
                travelSpeed: 0,
                defense: [{ class: "Infantry", value: 15 }, { class: "Cavalry", value: 45 }, { class: "Archer", value: 25 }],
                cost: [{ resource: "Wood", amount: 0 }, { resource: "Clay", amount: 0 }, { resource: "Iron", amount: 0 }],
                aliases: ['militia', 'militias', 'mil'],
                icon: "https://tylercamp.me/tw/img/unit_militia.png"
            }
        ]
    };

    return troopStats;
}