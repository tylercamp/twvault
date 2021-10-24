


## TW.Vault.Lib.Features

Contains main logic for various Vault features, eg battle/recruit/building simulations, travel time calculations, and the "high scores" section.

The `CommandClassification` namespace is largely unused. Some utilities (eg `Utils.IsNuke`) are used occasionally.

## TW.Vault.Lib.Model

Contains TW-specific model data (building times, unit recruit times), data types for JSON, and utilities for converting between JSON and DB data types.

## TW.Vault.Lib.Scaffold

Contains DB object types and "seed" data (translation keys / params, languages) for new databases. The `VaultContext` object contains the actual EntityFramework Core config. All access to the database goes through the `VaultContext`. Note that EF Core's "code generation from database" feature will not work for Vault DB.

Rather than looking at the `VaultContext` to learn the layout of the database, it's recommended to initialize a database locally and inspect the layout with a tool like pgAdmin.

Do NOT use the `dotnet ef` command to initialize a database, since it will be missing some required stored procedures. Use the `TW.Vault.Migration` tool instead.

#### Data types for raw TW data (`/map/.txt` endpoints)

- `Ally`
- `Conquer`
- `Player`
- `Village`

(These are updated automatically by `MapDataFetcher`.)

#### Data types for user-uploaded data

- `Command`
- `CommandArmy`
- `IgnoredReport`
- `Report`
- `ReportArmy`
- `ReportBuilding`
- `UserUploadHistory`

#### Data types for calculated data

- `CurrentArmy`
- `CurrentBuilding`
- `CurrentPlayer`
- `CurrentVillage`
- `CurrentVillageSupport`

#### Auth. data types

- `AccessGroup` (created when using `/manage` to get a new Vault key)
- `User` (created whenever any new Vault key is made)

#### Admin data types

- `EnemyTribe`
- `UserLog` (user creation/deletion events, permissions changes, etc.)

#### Translation data types

- `TranslationEntry` (contains translated text)
- `TranslationKey` (acts as ID for translated text)
- `TranslationLanguage`
- `TranslationParameter` (general info on required parameters for different Keys)
- `TranslationRegistry` (all Entries refer to a Registry ID, which ties them into a single translation)

(Key, Parameter, and Language are updated manually by server maintainer. Entry and Registry records are created in the Translations section of the Vault script.)

#### Auditing data types

- `ConflictingDataRecord`
- `FailedAuthorizationRecord`
- `InvalidDataRecord`
- `PerformanceRecord`
- `Transaction`

(`Transaction` is used rarely for Vault operations. Other types are meant to be inspected manually.)

#### Configuration data types

- `World` (info on world name, hostname, default language ID)
- `WorldSettings` (game speed, unit speed, archers/church/watchtowers enabled, etc.)

(These are usually managed with the `ConfigurationFetcher` tool.)

## TW.Vault.Lib.Security

Contains utils for performing data de-obfuscation, extracting authentication data, and performing authorization checks.