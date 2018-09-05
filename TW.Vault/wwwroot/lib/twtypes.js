let twtypes = (() => {

    //# REQUIRE lib/lib.js

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