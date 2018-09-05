
# General

Code is organized via:

- `Controllers` - Handles requests to the server
- `Features` - Features such as script compiling, battle simulation, etc.
- `Model` - Data objects, conversion utilities, and native TW data ie troop stats
- `Scaffold` - Data objects generated directly from the PostgreSQL database
- `Security` - Utilities that check for permissions and authentication

# Common Controller Logic

Most controllers inherit from `BaseController` which provides various features.

## Querying data from DB

Use the `context` variable within controllers to query for data. It has variables associated with each table in the DB, ie `context.Player` -> `vault.tw_provided.player`, `context.CurrentVillage` ->
`vault.tw.current_village`, etc.

Use LINQ syntax to query data from the DB `context`:

    var players = (
        from player in context.Player
        where player.TribeId == 123
        select player
    ).ToList();

    //  'players' is a List<Scaffold.Player>, which has variables mirrored with the columns in the associated table

Using `from ... select x` makes the query, using `(...).ToList()` executes the query and returns the result as a list.

You can also use C# Extension methods to query:

    var players = context.Player.Where(p => p.TribeId == 123).ToList();

These are equivalent. LINQ is typically preferred over extension methods for readability.

Do not use the blocking `ToList`, `First`, etc. functions - use the Async versions and `await` the result. This allows the thread running the query to
run other tasks while it waits for the DB query to finish.

    var players = await (
        from player in context.Player
        where player.TribeId == 123
        select player
    ).ToListAsync();

Objects related via foreign key will have properties generated to make use of these relationships:

    var report = await (
        from report in context.Report
        where report.ReportId == 1234567
        select report
    ).FirstOrDefaultAsync();

    int numSpears = report.AttackerArmy.Spear;
    int numSwords = report.AttackerArmy.Sword;
    //  ...

However, these relationships aren't loaded by default. To include related data, use `.Include(..)` on the table that you're querying:

    var report = await (
        from report in context.Report.Include(r => r.AttackerArmy).Include(r => r.DefenderArmy)
        where report.ReportId == 1234567
        select report
    ).FirstOrDefaultAsync();

    int numAttackerAxes = report.AttackerArmy.Axe;
    int numDefenderSpears = report.DefenderArmy.Spear;
    //  ...

### Querying data for the current world

Most (read: all) queries should include the world being queried for. The current world is available in a controller via `CurrentWorldId`. You
can manually check for the world ID in your query, or you can use the `.FromWorld(..)` extension method:

    var report = await (
        from report in context.Report.FromWorld(CurrentWorldId)
        where report.ReportId = 1234567
        select report
    ).FirstOrDefaultAsync();

    //  This is equivalent
    var report2 = await context.Report.FromWorld(CurrentWorldId).Where(r => r.ReportId == 1234567).FirstOrDefaultAsync();

## Adding data to the DB

Create an object from the `TW.Vault.Scaffold` namespace, assign the world ID and other required properties as appropriate, pass the new object to `context.Add(..)`, then
call `context.SaveChangesAsync()`.

    var newReport = new Scaffold.Report();
    newReport.WorldId = CurrentWorldId;
    // etc.

    context.Add(newReport);
    await context.SaveChangesAsync();

## Modifying data in DB

Request an object, modify its properties, then call `context.SaveChangesAsync()`.

    var report = await context.Report.FirstOrDefaultAsync();
    report.AttackerArmy = null;
    await context.SaveChangesAsync();
    //  Sets 'attacker_army_id' to 'null' in DB

## Checking for user permissions

Use:

- `User` - Data pulled directly from the `security.user` table entry for the current user (based on the user ID provided with the request to the vault)
- `User.PlayerId` - The player ID of the user as stored in the user's `security.user` table entry
- `CurrentUserIsAdmin` - Compare `User.PermissionsLevel` to `Security.PermissionLevels.Admin`
- `CurrentUserIsSystem` - Compares `USer.PermissionsLevel` to `Security.PermissionLevels.System`
- `CurrentTribeId` - The tribe ID of the user as provided in the request made to the Vault

All requests are automatically filtered if they are missing their authorization key, if the user ID doesn't match the ID tied to the key, etc. This is
checked in `Security.RequireAuthAttribute`, which is ran through all requests except those to `ScriptController`.



# Retrieving JSON data and storing in DB

Different data types are made for each JSON data that is sent to and from the Vault. This includes duplicates of Scaffold types, which prevents the need
to change the script if we change column names.

JSON data types are in the namespace `TW.Vault.Model.JSON`, in the folder `Model/JSON`. Conversion utilities between Scaffold and JSON types are available
in the `TW.Vault.Model.Conversion` namespace. Converters are generally available for all model types. Converters will generally auto-create or auto-update
a Scaffold object with JSON data, if a `Scaffold.VaultContext` is given (the `context` object available within controllers.)

    //  where 'reportJson' is a populated TW.Vault.Model.JSON.Report
    var reportJson = ...

    // Make the DB report object
    var scaffoldReport = new Scaffold.Report();
    scaffoldReport.WorldId = CurrentWorldId;

    //  Register with the DB context so that changes are tracked
    context.Add(scaffoldReport);

    //  Store JSON data into the DB object (auto-creates/updates army data, etc. - see implementation of ReportConvert.JsonToModel)
    ReportConvert.JsonToModel(reportJson, scaffoldReport, context);

    await context.SaveChangesAsync();
    

Typically, JSON objects are named `jsonX` and database objects are named `scaffoldX`.



# Abuse Prevention

TODO - See references to `Scaffold.ConflictingDataRecord`, `Scaffold.Transaction`, `BuildTransaction()` 