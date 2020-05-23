using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TW.Vault.Migration.Migrations
{
    [DbContext(typeof(Scaffold.VaultContext))]
    [Migration("20200517230000_AddCustomInit")]
    public class AddCustomInit : Microsoft.EntityFrameworkCore.Migrations.Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(CreateTriggerFunction("tw", "delete_command_army_with_command", File.ReadAllText("res/Delete_Command_Army_With_Command.sql")));
            migrationBuilder.Sql(CreateTriggerFunction("tw", "report_update_troops", File.ReadAllText("res/Report_Update_Troops.sql")));
            migrationBuilder.Sql(CreateTriggerFunction("security", "user_changelog", File.ReadAllText("res/User_Changelog.sql")));
            migrationBuilder.Sql(CreateTriggerFunction("tw_provided", "new_conquer_update", File.ReadAllText("res/New_Conquer_Update.sql")));

            migrationBuilder.Sql(CreateTrigger("log_changes", "security.\"user\"", "security.user_changelog", new[] { "INSERT", "DELETE", "UPDATE" }));
            migrationBuilder.Sql(CreateTrigger("delete_army_with_command", "tw.\"command\"", "tw.delete_command_army_with_command", new[] { "DELETE" }));
            migrationBuilder.Sql(CreateTrigger("update_troops_on_insert", "tw.report", "tw.report_update_troops", new[] { "INSERT" }));
            migrationBuilder.Sql(CreateTrigger("update_on_conquer", "tw_provided.conquer", "tw_provided.new_conquer_update", new[] { "INSERT" }));

            migrationBuilder.Sql(CreateIndex("idx_report_occurred_at", "tw.report", "btree", "(occured_at DESC NULLS LAST)"));
            migrationBuilder.Sql(CreateIndex("idx_first_seen_at", "tw.command", "btree", "(first_seen_at ASC NULLS LAST)"));
            migrationBuilder.Sql(CreateIndex("idx_lands_at", "tw.command", "btree", "(lands_at DESC NULLS LAST)"));
            migrationBuilder.Sql(CreateIndex("idx_returns_at", "tw.command", "btree", "(returns_at ASC NULLS LAST)"));

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            base.Down(migrationBuilder);
        }

        private String CreateTriggerFunction(String schema, String scriptName, String scriptContents) =>
$@"
CREATE FUNCTION {schema}.{scriptName}()
    RETURNS trigger
    LANGUAGE 'plpgsql'
    VOLATILE
    COST 100
AS $BODY$
{scriptContents}
$BODY$";

        private String CreateTrigger(String name, String targetTable, String targetScript, IEnumerable<String> events) =>
$@"
CREATE TRIGGER {name}
    AFTER {String.Join(" OR ", events)}
    ON {targetTable}
    FOR EACH ROW
    EXECUTE PROCEDURE {targetScript}();
";

        private String CreateIndex(String name, String targetTable, String indexMethod, String columnSelection) =>
$@"
CREATE INDEX {name}
    ON {targetTable} USING {indexMethod}
    {columnSelection}
    TABLESPACE pg_default;
";
    }
}
