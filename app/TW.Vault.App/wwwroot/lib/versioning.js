function makeVersioningSetup() {
    let currentVersion = '1.3.0';
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