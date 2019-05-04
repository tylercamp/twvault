using System;
using System.Net;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace TW.Vault.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "security");

            migrationBuilder.EnsureSchema(
                name: "tw_provided");

            migrationBuilder.EnsureSchema(
                name: "tw");

            migrationBuilder.EnsureSchema(
                name: "feature");

            migrationBuilder.CreateSequence<short>(
                name: "translation_key_id_seq",
                schema: "feature");

            migrationBuilder.CreateSequence<short>(
                name: "translation_language_id_seq",
                schema: "feature");

            migrationBuilder.CreateSequence<short>(
                name: "translation_parameter_id_seq",
                schema: "feature");

            migrationBuilder.CreateSequence<short>(
                name: "translation_registry_id_seq",
                schema: "feature");

            migrationBuilder.CreateSequence(
                name: "access_group_id_seq",
                schema: "security");

            migrationBuilder.CreateSequence(
                name: "conflicting_data_record_id_seq",
                schema: "security");

            migrationBuilder.CreateSequence(
                name: "invalid_data_record_id_seq",
                schema: "security");

            migrationBuilder.CreateSequence(
                name: "user_log_id_seq",
                schema: "security");

            migrationBuilder.CreateSequence(
                name: "user_upload_history_id_seq",
                schema: "security");

            migrationBuilder.CreateSequence(
                name: "command_army_id_seq",
                schema: "tw");

            migrationBuilder.CreateSequence(
                name: "current_village_support_id_seq",
                schema: "tw");

            migrationBuilder.CreateSequence(
                name: "enemy_tribe_id_seq",
                schema: "tw");

            migrationBuilder.CreateSequence(
                name: "failed_auth_id_seq",
                schema: "tw");

            migrationBuilder.CreateSequence(
                name: "performance_record_id_seq",
                schema: "tw");

            migrationBuilder.CreateSequence(
                name: "report_armies_army_id_seq",
                schema: "tw");

            migrationBuilder.CreateSequence(
                name: "report_building_report_building_id_seq",
                schema: "tw");

            migrationBuilder.CreateSequence(
                name: "tx_id_seq",
                schema: "tw");

            migrationBuilder.CreateSequence(
                name: "users_uid_seq",
                schema: "tw");

            migrationBuilder.CreateSequence<int>(
                name: "conquers_vault_id_seq",
                schema: "tw_provided");

            migrationBuilder.CreateSequence<short>(
                name: "world_id_seq",
                schema: "tw_provided");

            migrationBuilder.CreateTable(
                name: "translation_key",
                schema: "feature",
                columns: table => new
                {
                    id = table.Column<short>(nullable: false, defaultValueSql: "nextval('feature.translation_key_id_seq'::regclass)"),
                    name = table.Column<string>(nullable: false),
                    is_tw_native = table.Column<bool>(nullable: false, defaultValue: false),
                    group = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_translation_key", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "translation_language",
                schema: "feature",
                columns: table => new
                {
                    id = table.Column<short>(nullable: false, defaultValueSql: "nextval('feature.translation_language_id_seq'::regclass)"),
                    name = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_translation_language", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "access_group",
                schema: "security",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false, defaultValueSql: "nextval('security.access_group_id_seq'::regclass)"),
                    label = table.Column<string>(maxLength: 256, nullable: true),
                    world_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_access_group", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "failed_authorization_record",
                schema: "security",
                columns: table => new
                {
                    id = table.Column<long>(nullable: false, defaultValueSql: "nextval('tw.failed_auth_id_seq'::regclass)"),
                    ip = table.Column<IPAddress>(nullable: false),
                    player_id = table.Column<long>(nullable: true),
                    tribe_id = table.Column<long>(nullable: true),
                    occurred_at = table.Column<DateTime>(nullable: false),
                    requested_endpoint = table.Column<string>(maxLength: 128, nullable: false),
                    reason = table.Column<string>(maxLength: 256, nullable: false),
                    world_id = table.Column<short>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_failed_authorization_record", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "current_army",
                schema: "tw",
                columns: table => new
                {
                    army_id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    world_id = table.Column<short>(nullable: false),
                    spear = table.Column<int>(nullable: true),
                    sword = table.Column<int>(nullable: true),
                    axe = table.Column<int>(nullable: true),
                    archer = table.Column<int>(nullable: true),
                    spy = table.Column<int>(nullable: true),
                    light = table.Column<int>(nullable: true),
                    marcher = table.Column<int>(nullable: true),
                    heavy = table.Column<int>(nullable: true),
                    ram = table.Column<int>(nullable: true),
                    catapult = table.Column<int>(nullable: true),
                    knight = table.Column<int>(nullable: true),
                    snob = table.Column<int>(nullable: true),
                    militia = table.Column<int>(nullable: true),
                    last_updated = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_current_army", x => new { x.world_id, x.army_id });
                });

            migrationBuilder.CreateTable(
                name: "ignored_report",
                schema: "tw",
                columns: table => new
                {
                    world_id = table.Column<short>(nullable: false),
                    report_id = table.Column<long>(nullable: false),
                    access_group_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ignored_report", x => new { x.report_id, x.world_id, x.access_group_id });
                });

            migrationBuilder.CreateTable(
                name: "performance_record",
                schema: "tw",
                columns: table => new
                {
                    id = table.Column<long>(nullable: false, defaultValueSql: "nextval('tw.performance_record_id_seq'::regclass)"),
                    operation_label = table.Column<string>(maxLength: 128, nullable: false),
                    average_time = table.Column<TimeSpan>(nullable: false),
                    min_time = table.Column<TimeSpan>(nullable: false),
                    max_time = table.Column<TimeSpan>(nullable: false),
                    generated_at = table.Column<DateTime>(nullable: false),
                    num_samples = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_performance_record", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "translation_parameter",
                schema: "feature",
                columns: table => new
                {
                    id = table.Column<short>(nullable: false, defaultValueSql: "nextval('feature.translation_parameter_id_seq'::regclass)"),
                    name = table.Column<string>(nullable: false),
                    key_id = table.Column<short>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_translation_parameter", x => x.id);
                    table.ForeignKey(
                        name: "fk_translation_key_id",
                        column: x => x.key_id,
                        principalSchema: "feature",
                        principalTable: "translation_key",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "translation_registry",
                schema: "feature",
                columns: table => new
                {
                    id = table.Column<short>(nullable: false, defaultValueSql: "nextval('feature.translation_registry_id_seq'::regclass)"),
                    name = table.Column<string>(nullable: false),
                    author = table.Column<string>(nullable: false),
                    author_player_id = table.Column<long>(nullable: false),
                    language_id = table.Column<short>(nullable: false),
                    is_system_internal = table.Column<bool>(nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_translation_registry", x => x.id);
                    table.ForeignKey(
                        name: "fk_registry_language_id",
                        column: x => x.language_id,
                        principalSchema: "feature",
                        principalTable: "translation_language",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "translation",
                schema: "feature",
                columns: table => new
                {
                    translation_id = table.Column<short>(nullable: false),
                    key = table.Column<short>(nullable: false),
                    value = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_translation", x => new { x.translation_id, x.key });
                    table.ForeignKey(
                        name: "fk_translation_key_id",
                        column: x => x.key,
                        principalSchema: "feature",
                        principalTable: "translation_key",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_translation_registry_id",
                        column: x => x.translation_id,
                        principalSchema: "feature",
                        principalTable: "translation_registry",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "world",
                schema: "tw_provided",
                columns: table => new
                {
                    id = table.Column<short>(nullable: false, defaultValueSql: "nextval('tw_provided.world_id_seq'::regclass)"),
                    name = table.Column<string>(maxLength: 6, nullable: false),
                    hostname = table.Column<string>(maxLength: 32, nullable: false),
                    default_translation_id = table.Column<short>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_world", x => x.id);
                    table.ForeignKey(
                        name: "fk_default_translation_id",
                        column: x => x.default_translation_id,
                        principalSchema: "feature",
                        principalTable: "translation_registry",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "transaction",
                schema: "security",
                columns: table => new
                {
                    tx_id = table.Column<long>(nullable: false, defaultValueSql: "nextval('tw.tx_id_seq'::regclass)"),
                    uid = table.Column<int>(nullable: false),
                    occurred_at = table.Column<DateTime>(nullable: false),
                    ip = table.Column<IPAddress>(nullable: false),
                    world_id = table.Column<short>(nullable: false),
                    previous_tx_id = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transaction", x => x.tx_id);
                    table.ForeignKey(
                        name: "fk_world_id",
                        column: x => x.world_id,
                        principalSchema: "tw_provided",
                        principalTable: "world",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "command_army",
                schema: "tw",
                columns: table => new
                {
                    army_id = table.Column<long>(nullable: false, defaultValueSql: "nextval('tw.command_army_id_seq'::regclass)"),
                    world_id = table.Column<short>(nullable: false),
                    spear = table.Column<int>(nullable: true),
                    sword = table.Column<int>(nullable: true),
                    axe = table.Column<int>(nullable: true),
                    archer = table.Column<int>(nullable: true),
                    spy = table.Column<int>(nullable: true),
                    light = table.Column<int>(nullable: true),
                    marcher = table.Column<int>(nullable: true),
                    heavy = table.Column<int>(nullable: true),
                    ram = table.Column<int>(nullable: true),
                    catapult = table.Column<int>(nullable: true),
                    knight = table.Column<int>(nullable: true),
                    snob = table.Column<int>(nullable: true),
                    militia = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_command_army", x => new { x.world_id, x.army_id });
                    table.ForeignKey(
                        name: "fk_world_id",
                        column: x => x.world_id,
                        principalSchema: "tw_provided",
                        principalTable: "world",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "current_player",
                schema: "tw",
                columns: table => new
                {
                    player_id = table.Column<long>(nullable: false),
                    world_id = table.Column<short>(nullable: false),
                    access_group_id = table.Column<int>(nullable: false),
                    current_possible_nobles = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_current_player", x => new { x.world_id, x.player_id, x.access_group_id });
                    table.ForeignKey(
                        name: "fk_world_id",
                        column: x => x.world_id,
                        principalSchema: "tw_provided",
                        principalTable: "world",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "report_army",
                schema: "tw",
                columns: table => new
                {
                    army_id = table.Column<long>(nullable: false, defaultValueSql: "nextval('tw.report_armies_army_id_seq'::regclass)"),
                    world_id = table.Column<short>(nullable: false),
                    spear = table.Column<int>(nullable: true),
                    sword = table.Column<int>(nullable: true),
                    axe = table.Column<int>(nullable: true),
                    archer = table.Column<int>(nullable: true),
                    spy = table.Column<int>(nullable: true),
                    light = table.Column<int>(nullable: true),
                    marcher = table.Column<int>(nullable: true),
                    heavy = table.Column<int>(nullable: true),
                    ram = table.Column<int>(nullable: true),
                    catapult = table.Column<int>(nullable: true),
                    knight = table.Column<int>(nullable: true),
                    snob = table.Column<int>(nullable: true),
                    militia = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report_army", x => new { x.world_id, x.army_id });
                    table.ForeignKey(
                        name: "fk_world_id",
                        column: x => x.world_id,
                        principalSchema: "tw_provided",
                        principalTable: "world",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "report_building",
                schema: "tw",
                columns: table => new
                {
                    world_id = table.Column<short>(nullable: false),
                    report_building_id = table.Column<long>(nullable: false, defaultValueSql: "nextval('tw.report_building_report_building_id_seq'::regclass)"),
                    main = table.Column<short>(nullable: true),
                    stable = table.Column<short>(nullable: true),
                    garage = table.Column<short>(nullable: true),
                    church = table.Column<short>(nullable: true),
                    first_church = table.Column<short>(nullable: true),
                    smith = table.Column<short>(nullable: true),
                    place = table.Column<short>(nullable: true),
                    statue = table.Column<short>(nullable: true),
                    market = table.Column<short>(nullable: true),
                    wood = table.Column<short>(nullable: true),
                    stone = table.Column<short>(nullable: true),
                    iron = table.Column<short>(nullable: true),
                    farm = table.Column<short>(nullable: true),
                    storage = table.Column<short>(nullable: true),
                    hide = table.Column<short>(nullable: true),
                    wall = table.Column<short>(nullable: true),
                    watchtower = table.Column<short>(nullable: true),
                    barracks = table.Column<short>(nullable: true),
                    snob = table.Column<short>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report_building", x => new { x.world_id, x.report_building_id });
                    table.ForeignKey(
                        name: "fk_world_id",
                        column: x => x.world_id,
                        principalSchema: "tw_provided",
                        principalTable: "world",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ally",
                schema: "tw_provided",
                columns: table => new
                {
                    tribe_id = table.Column<long>(nullable: false),
                    world_id = table.Column<short>(nullable: false),
                    tribe_name = table.Column<string>(type: "character varying", nullable: true),
                    tag = table.Column<string>(type: "character varying", nullable: true),
                    members = table.Column<int>(nullable: true),
                    villages = table.Column<int>(nullable: true),
                    points = table.Column<long>(nullable: true),
                    all_points = table.Column<long>(nullable: true),
                    tribe_rank = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ally", x => new { x.world_id, x.tribe_id });
                    table.ForeignKey(
                        name: "fk_world_id",
                        column: x => x.world_id,
                        principalSchema: "tw_provided",
                        principalTable: "world",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "conquer",
                schema: "tw_provided",
                columns: table => new
                {
                    vault_id = table.Column<int>(nullable: false, defaultValueSql: "nextval('tw_provided.conquers_vault_id_seq'::regclass)"),
                    village_id = table.Column<long>(nullable: true),
                    unix_timestamp = table.Column<long>(nullable: true),
                    new_owner = table.Column<long>(nullable: true),
                    old_owner = table.Column<long>(nullable: true),
                    world_id = table.Column<short>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_conquer", x => x.vault_id);
                    table.ForeignKey(
                        name: "fk_world_id",
                        column: x => x.world_id,
                        principalSchema: "tw_provided",
                        principalTable: "world",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "player",
                schema: "tw_provided",
                columns: table => new
                {
                    player_id = table.Column<long>(nullable: false),
                    world_id = table.Column<short>(nullable: false),
                    player_name = table.Column<string>(type: "character varying", nullable: true),
                    tribe_id = table.Column<long>(nullable: true),
                    villages = table.Column<int>(nullable: true),
                    points = table.Column<int>(nullable: true),
                    player_rank = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_player", x => new { x.world_id, x.player_id });
                    table.ForeignKey(
                        name: "fk_world_id",
                        column: x => x.world_id,
                        principalSchema: "tw_provided",
                        principalTable: "world",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "world_settings",
                schema: "tw_provided",
                columns: table => new
                {
                    world_id = table.Column<short>(nullable: false),
                    can_demolish_buildings = table.Column<bool>(nullable: false),
                    account_sitting_enabled = table.Column<bool>(nullable: false),
                    archers_enabled = table.Column<bool>(nullable: false),
                    bonus_villages_enabled = table.Column<bool>(nullable: false),
                    churches_enabled = table.Column<bool>(nullable: false),
                    flags_enabled = table.Column<bool>(nullable: false),
                    nobleman_loyalty_min = table.Column<short>(nullable: false),
                    nobleman_loyalty_max = table.Column<short>(nullable: false),
                    loyalty_per_hour = table.Column<short>(nullable: false),
                    game_speed = table.Column<decimal>(nullable: false),
                    max_nobleman_distance = table.Column<short>(nullable: false),
                    militia_enabled = table.Column<bool>(nullable: false),
                    milliseconds_enabled = table.Column<bool>(nullable: false),
                    morale_enabled = table.Column<bool>(nullable: false),
                    night_bonus_enabled = table.Column<bool>(nullable: false),
                    paladin_enabled = table.Column<bool>(nullable: false),
                    paladin_skills_enabled = table.Column<bool>(nullable: false),
                    paladin_items_enabled = table.Column<bool>(nullable: false),
                    unit_speed = table.Column<decimal>(nullable: false),
                    watchtower_enabled = table.Column<bool>(nullable: false),
                    utc_offset = table.Column<TimeSpan>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_world_settings", x => x.world_id);
                    table.ForeignKey(
                        name: "fk_world_id",
                        column: x => x.world_id,
                        principalSchema: "tw_provided",
                        principalTable: "world",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "conflicting_data_record",
                schema: "security",
                columns: table => new
                {
                    id = table.Column<long>(nullable: false, defaultValueSql: "nextval('security.conflicting_data_record_id_seq'::regclass)"),
                    conflicting_tx_id = table.Column<long>(nullable: false),
                    old_tx_id = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_conflicting_data_record", x => x.id);
                    table.ForeignKey(
                        name: "fk_conflicting_tx_id",
                        column: x => x.conflicting_tx_id,
                        principalSchema: "security",
                        principalTable: "transaction",
                        principalColumn: "tx_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_old_tx_id",
                        column: x => x.old_tx_id,
                        principalSchema: "security",
                        principalTable: "transaction",
                        principalColumn: "tx_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user",
                schema: "security",
                columns: table => new
                {
                    uid = table.Column<int>(nullable: false, defaultValueSql: "nextval('tw.users_uid_seq'::regclass)"),
                    player_id = table.Column<long>(nullable: false),
                    permissions_level = table.Column<short>(nullable: false),
                    label = table.Column<string>(nullable: true),
                    enabled = table.Column<bool>(nullable: false),
                    auth_token = table.Column<Guid>(nullable: false),
                    world_id = table.Column<short>(nullable: true),
                    key_source = table.Column<int>(nullable: true),
                    transaction_time = table.Column<DateTime>(nullable: false),
                    admin_auth_token = table.Column<Guid>(nullable: true),
                    admin_player_id = table.Column<long>(nullable: true),
                    is_read_only = table.Column<bool>(nullable: false),
                    tx_id = table.Column<long>(nullable: true),
                    access_group_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user", x => x.uid);
                    table.ForeignKey(
                        name: "fk_tx_id",
                        column: x => x.tx_id,
                        principalSchema: "security",
                        principalTable: "transaction",
                        principalColumn: "tx_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_world_id",
                        column: x => x.world_id,
                        principalSchema: "tw_provided",
                        principalTable: "world",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_log",
                schema: "security",
                columns: table => new
                {
                    id = table.Column<long>(nullable: false, defaultValueSql: "nextval('security.user_log_id_seq'::regclass)"),
                    uid = table.Column<int>(nullable: false),
                    player_id = table.Column<long>(nullable: false),
                    permissions_level = table.Column<short>(nullable: false),
                    label = table.Column<string>(nullable: true),
                    enabled = table.Column<bool>(nullable: false),
                    auth_token = table.Column<Guid>(nullable: false),
                    world_id = table.Column<short>(nullable: true),
                    key_source = table.Column<int>(nullable: true),
                    transaction_time = table.Column<DateTime>(nullable: false),
                    admin_auth_token = table.Column<Guid>(nullable: true),
                    admin_player_id = table.Column<long>(nullable: true),
                    operation_type = table.Column<string>(type: "character varying", nullable: false),
                    is_read_only = table.Column<bool>(nullable: false),
                    tx_id = table.Column<long>(nullable: true),
                    access_group_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_log", x => x.id);
                    table.ForeignKey(
                        name: "fk_tx_id",
                        column: x => x.tx_id,
                        principalSchema: "security",
                        principalTable: "transaction",
                        principalColumn: "tx_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_world_id",
                        column: x => x.world_id,
                        principalSchema: "tw_provided",
                        principalTable: "world",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "enemy_tribe",
                schema: "tw",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false, defaultValueSql: "nextval('tw.enemy_tribe_id_seq'::regclass)"),
                    access_group_id = table.Column<int>(nullable: false),
                    world_id = table.Column<short>(nullable: false),
                    enemy_tribe_id = table.Column<long>(nullable: false),
                    tx_id = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_enemy_tribe", x => new { x.id, x.access_group_id });
                    table.ForeignKey(
                        name: "FK_enemy_tribe_transaction_tx_id",
                        column: x => x.tx_id,
                        principalSchema: "security",
                        principalTable: "transaction",
                        principalColumn: "tx_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "village",
                schema: "tw_provided",
                columns: table => new
                {
                    village_id = table.Column<long>(nullable: false),
                    world_id = table.Column<short>(nullable: false),
                    village_name = table.Column<string>(type: "character varying", nullable: true),
                    x = table.Column<short>(nullable: true),
                    y = table.Column<short>(nullable: true),
                    player_id = table.Column<long>(nullable: true),
                    points = table.Column<short>(nullable: true),
                    village_rank = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_village", x => new { x.world_id, x.village_id });
                    table.ForeignKey(
                        name: "fk_world_id",
                        column: x => x.world_id,
                        principalSchema: "tw_provided",
                        principalTable: "world",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_player_id",
                        columns: x => new { x.world_id, x.player_id },
                        principalSchema: "tw_provided",
                        principalTable: "player",
                        principalColumns: new[] { "world_id", "player_id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "invalid_data_record",
                schema: "security",
                columns: table => new
                {
                    id = table.Column<long>(nullable: false, defaultValueSql: "nextval('security.invalid_data_record_id_seq'::regclass)"),
                    endpoint = table.Column<string>(maxLength: 128, nullable: false),
                    reason = table.Column<string>(maxLength: 128, nullable: false),
                    user_id = table.Column<int>(nullable: false),
                    data_string = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invalid_data_record", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_id",
                        column: x => x.user_id,
                        principalSchema: "security",
                        principalTable: "user",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_upload_history",
                schema: "security",
                columns: table => new
                {
                    id = table.Column<long>(nullable: false, defaultValueSql: "nextval('security.user_upload_history_id_seq'::regclass)"),
                    uid = table.Column<int>(nullable: false),
                    last_uploaded_reports_at = table.Column<DateTime>(nullable: true),
                    last_uploaded_incomings_at = table.Column<DateTime>(nullable: true),
                    last_uploaded_commands_at = table.Column<DateTime>(nullable: true),
                    last_uploaded_troops_at = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_upload_history", x => x.id);
                    table.ForeignKey(
                        name: "fk_uid",
                        column: x => x.uid,
                        principalSchema: "security",
                        principalTable: "user",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "command",
                schema: "tw",
                columns: table => new
                {
                    command_id = table.Column<long>(nullable: false),
                    world_id = table.Column<short>(nullable: false),
                    access_group_id = table.Column<int>(nullable: false),
                    source_village_id = table.Column<long>(nullable: false),
                    source_player_id = table.Column<long>(nullable: false),
                    target_village_id = table.Column<long>(nullable: false),
                    target_player_id = table.Column<long>(nullable: true),
                    lands_at = table.Column<DateTime>(nullable: false),
                    first_seen_at = table.Column<DateTime>(nullable: false),
                    troop_type = table.Column<string>(type: "character varying", nullable: true),
                    is_attack = table.Column<bool>(nullable: false),
                    army_id = table.Column<long>(nullable: true),
                    is_returning = table.Column<bool>(nullable: false),
                    tx_id = table.Column<long>(nullable: true),
                    user_label = table.Column<string>(maxLength: 512, nullable: true),
                    returns_at = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_command", x => new { x.world_id, x.access_group_id, x.command_id });
                    table.ForeignKey(
                        name: "fk_tx_id",
                        column: x => x.tx_id,
                        principalSchema: "security",
                        principalTable: "transaction",
                        principalColumn: "tx_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_world_id",
                        column: x => x.world_id,
                        principalSchema: "tw_provided",
                        principalTable: "world",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_army",
                        columns: x => new { x.world_id, x.army_id },
                        principalSchema: "tw",
                        principalTable: "command_army",
                        principalColumns: new[] { "world_id", "army_id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_source_player",
                        columns: x => new { x.world_id, x.source_player_id },
                        principalSchema: "tw_provided",
                        principalTable: "player",
                        principalColumns: new[] { "world_id", "player_id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_source_village",
                        columns: x => new { x.world_id, x.source_village_id },
                        principalSchema: "tw_provided",
                        principalTable: "village",
                        principalColumns: new[] { "world_id", "village_id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_target_player",
                        columns: x => new { x.world_id, x.target_player_id },
                        principalSchema: "tw_provided",
                        principalTable: "player",
                        principalColumns: new[] { "world_id", "player_id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_target_village",
                        columns: x => new { x.world_id, x.target_village_id },
                        principalSchema: "tw_provided",
                        principalTable: "village",
                        principalColumns: new[] { "world_id", "village_id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "current_village",
                schema: "tw",
                columns: table => new
                {
                    village_id = table.Column<long>(nullable: false),
                    world_id = table.Column<short>(nullable: false),
                    access_group_id = table.Column<int>(nullable: false),
                    army_stationed_id = table.Column<long>(nullable: true),
                    army_traveling_id = table.Column<long>(nullable: true),
                    army_owned_id = table.Column<long>(nullable: true),
                    army_recent_losses_id = table.Column<long>(nullable: true),
                    loyalty = table.Column<short>(nullable: true),
                    loyalty_last_updated = table.Column<DateTime>(nullable: true),
                    army_at_home_id = table.Column<long>(nullable: true),
                    army_supporting_id = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_current_village", x => new { x.world_id, x.village_id, x.access_group_id });
                    table.ForeignKey(
                        name: "fk_world_id",
                        column: x => x.world_id,
                        principalSchema: "tw_provided",
                        principalTable: "world",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_army_at_home",
                        columns: x => new { x.world_id, x.army_at_home_id },
                        principalSchema: "tw",
                        principalTable: "current_army",
                        principalColumns: new[] { "world_id", "army_id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_owned_army",
                        columns: x => new { x.world_id, x.army_owned_id },
                        principalSchema: "tw",
                        principalTable: "current_army",
                        principalColumns: new[] { "world_id", "army_id" },
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "current_village_current_army_fk",
                        columns: x => new { x.world_id, x.army_recent_losses_id },
                        principalSchema: "tw",
                        principalTable: "current_army",
                        principalColumns: new[] { "world_id", "army_id" },
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_stationed_army",
                        columns: x => new { x.world_id, x.army_stationed_id },
                        principalSchema: "tw",
                        principalTable: "current_army",
                        principalColumns: new[] { "world_id", "army_id" },
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_army_supporting",
                        columns: x => new { x.world_id, x.army_supporting_id },
                        principalSchema: "tw",
                        principalTable: "current_army",
                        principalColumns: new[] { "world_id", "army_id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_traveling_army",
                        columns: x => new { x.world_id, x.army_traveling_id },
                        principalSchema: "tw",
                        principalTable: "current_army",
                        principalColumns: new[] { "world_id", "army_id" },
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_village",
                        columns: x => new { x.world_id, x.village_id },
                        principalSchema: "tw_provided",
                        principalTable: "village",
                        principalColumns: new[] { "world_id", "village_id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "current_village_support",
                schema: "tw",
                columns: table => new
                {
                    world_id = table.Column<short>(nullable: false),
                    id = table.Column<long>(nullable: false, defaultValueSql: "nextval('tw.current_village_support_id_seq'::regclass)"),
                    access_group_id = table.Column<int>(nullable: false),
                    source_village_id = table.Column<long>(nullable: false),
                    target_village_id = table.Column<long>(nullable: false),
                    last_updated_at = table.Column<DateTime>(nullable: false),
                    supporting_army_id = table.Column<long>(nullable: false),
                    tx_id = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_current_village_support", x => new { x.world_id, x.id, x.access_group_id });
                    table.ForeignKey(
                        name: "fk_tx_id",
                        column: x => x.tx_id,
                        principalSchema: "security",
                        principalTable: "transaction",
                        principalColumn: "tx_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_world_id",
                        column: x => x.world_id,
                        principalSchema: "tw_provided",
                        principalTable: "world",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_source_village",
                        columns: x => new { x.world_id, x.source_village_id },
                        principalSchema: "tw_provided",
                        principalTable: "village",
                        principalColumns: new[] { "world_id", "village_id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_supporting_army",
                        columns: x => new { x.world_id, x.supporting_army_id },
                        principalSchema: "tw",
                        principalTable: "current_army",
                        principalColumns: new[] { "world_id", "army_id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_target_village",
                        columns: x => new { x.world_id, x.target_village_id },
                        principalSchema: "tw_provided",
                        principalTable: "village",
                        principalColumns: new[] { "world_id", "village_id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "report",
                schema: "tw",
                columns: table => new
                {
                    report_id = table.Column<long>(nullable: false),
                    world_id = table.Column<short>(nullable: false),
                    access_group_id = table.Column<int>(nullable: false),
                    attacker_village_id = table.Column<long>(nullable: false),
                    defender_village_id = table.Column<long>(nullable: false),
                    attacker_player_id = table.Column<long>(nullable: true),
                    defender_player_id = table.Column<long>(nullable: true),
                    occured_at = table.Column<DateTime>(nullable: false),
                    attacker_army_id = table.Column<long>(nullable: false),
                    attacker_losses_army_id = table.Column<long>(nullable: false),
                    defender_army_id = table.Column<long>(nullable: true),
                    defender_losses_army_id = table.Column<long>(nullable: true),
                    defender_traveling_army_id = table.Column<long>(nullable: true),
                    morale = table.Column<short>(nullable: false),
                    luck = table.Column<decimal>(nullable: false),
                    tx_id = table.Column<long>(nullable: true),
                    loyalty = table.Column<short>(nullable: true),
                    building_id = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report", x => new { x.world_id, x.report_id, x.access_group_id });
                    table.ForeignKey(
                        name: "fk_tx_id",
                        column: x => x.tx_id,
                        principalSchema: "security",
                        principalTable: "transaction",
                        principalColumn: "tx_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_world_id",
                        column: x => x.world_id,
                        principalSchema: "tw_provided",
                        principalTable: "world",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_attacker_army",
                        columns: x => new { x.world_id, x.attacker_army_id },
                        principalSchema: "tw",
                        principalTable: "report_army",
                        principalColumns: new[] { "world_id", "army_id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_attacker_army_losses",
                        columns: x => new { x.world_id, x.attacker_losses_army_id },
                        principalSchema: "tw",
                        principalTable: "report_army",
                        principalColumns: new[] { "world_id", "army_id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_attacker_player",
                        columns: x => new { x.world_id, x.attacker_player_id },
                        principalSchema: "tw_provided",
                        principalTable: "player",
                        principalColumns: new[] { "world_id", "player_id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_attacker_village",
                        columns: x => new { x.world_id, x.attacker_village_id },
                        principalSchema: "tw_provided",
                        principalTable: "village",
                        principalColumns: new[] { "world_id", "village_id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_building",
                        columns: x => new { x.world_id, x.building_id },
                        principalSchema: "tw",
                        principalTable: "report_building",
                        principalColumns: new[] { "world_id", "report_building_id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_defender_army",
                        columns: x => new { x.world_id, x.defender_army_id },
                        principalSchema: "tw",
                        principalTable: "report_army",
                        principalColumns: new[] { "world_id", "army_id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_defender_army_losses",
                        columns: x => new { x.world_id, x.defender_losses_army_id },
                        principalSchema: "tw",
                        principalTable: "report_army",
                        principalColumns: new[] { "world_id", "army_id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_defender_player",
                        columns: x => new { x.world_id, x.defender_player_id },
                        principalSchema: "tw_provided",
                        principalTable: "player",
                        principalColumns: new[] { "world_id", "player_id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_defender_traveling_army",
                        columns: x => new { x.world_id, x.defender_traveling_army_id },
                        principalSchema: "tw",
                        principalTable: "report_army",
                        principalColumns: new[] { "world_id", "army_id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_defender_village",
                        columns: x => new { x.world_id, x.defender_village_id },
                        principalSchema: "tw_provided",
                        principalTable: "village",
                        principalColumns: new[] { "world_id", "village_id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "current_building",
                schema: "tw",
                columns: table => new
                {
                    village_id = table.Column<long>(nullable: false),
                    world_id = table.Column<short>(nullable: false),
                    access_group_id = table.Column<int>(nullable: false),
                    main = table.Column<short>(nullable: true),
                    stable = table.Column<short>(nullable: true),
                    garage = table.Column<short>(nullable: true),
                    church = table.Column<short>(nullable: true),
                    first_church = table.Column<short>(nullable: true),
                    smith = table.Column<short>(nullable: true),
                    place = table.Column<short>(nullable: true),
                    statue = table.Column<short>(nullable: true),
                    market = table.Column<short>(nullable: true),
                    wood = table.Column<short>(nullable: true),
                    stone = table.Column<short>(nullable: true),
                    iron = table.Column<short>(nullable: true),
                    farm = table.Column<short>(nullable: true),
                    storage = table.Column<short>(nullable: true),
                    hide = table.Column<short>(nullable: true),
                    wall = table.Column<short>(nullable: true),
                    watchtower = table.Column<short>(nullable: true),
                    barracks = table.Column<short>(nullable: true),
                    last_updated = table.Column<DateTime>(nullable: true),
                    snob = table.Column<short>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_current_building", x => new { x.world_id, x.village_id, x.access_group_id });
                    table.ForeignKey(
                        name: "fk_world_id",
                        column: x => x.world_id,
                        principalSchema: "tw_provided",
                        principalTable: "world",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "buildings_villages_fk",
                        columns: x => new { x.world_id, x.village_id, x.access_group_id },
                        principalSchema: "tw",
                        principalTable: "current_village",
                        principalColumns: new[] { "world_id", "village_id", "access_group_id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "feature",
                table: "translation_key",
                columns: new[] { "id", "group", "name" },
                values: new object[] { (short)2, "General", "OPEN_VAULT" });

            migrationBuilder.InsertData(
                schema: "feature",
                table: "translation_key",
                columns: new[] { "id", "group", "name" },
                values: new object[] { (short)379, "Uploads", "REPORTS_NONE_NEW" });

            migrationBuilder.InsertData(
                schema: "feature",
                table: "translation_key",
                columns: new[] { "id", "group", "name" },
                values: new object[] { (short)378, "Uploads", "REPORTS_FINISHED" });

            migrationBuilder.InsertData(
                schema: "feature",
                table: "translation_key",
                columns: new[] { "id", "group", "name" },
                values: new object[] { (short)377, "Uploads", "REPORTS_ERROR_CHECK_OLD" });

            migrationBuilder.InsertData(
                schema: "feature",
                table: "translation_key",
                columns: new[] { "id", "group", "name" },
                values: new object[] { (short)376, "Uploads", "REPORTS_SKIPPED_OLD" });

            migrationBuilder.InsertData(
                schema: "feature",
                table: "translation_key",
                columns: new[] { "id", "group", "name" },
                values: new object[] { (short)375, "Uploads", "REPORTS_LA_ERROR" });

            migrationBuilder.InsertData(
                schema: "feature",
                table: "translation_key",
                columns: new[] { "id", "group", "name" },
                values: new object[] { (short)374, "Uploads", "REPORTS_FILTERING_LA" });

            migrationBuilder.InsertData(
                schema: "feature",
                table: "translation_key",
                columns: new[] { "id", "group", "is_tw_native", "name" },
                values: new object[,]
                {
                    { (short)373, "Uploads", true, "REPORTS_LOOT_ASSISTANT" },
                    { (short)372, "Uploads", false, "REPORTS_LA_NOT_FOUND" },
                    { (short)371, "Uploads", false, "REPORTS_CHECK_UPLOADED" },
                    { (short)370, "Uploads", false, "REPORTS_PAGES_PROGRESS" },
                    { (short)369, "Uploads", false, "REPORTS_COLLECTING_LINKS" },
                    { (short)368, "Uploads", false, "REPORTS_COLLECTING_PAGES" },
                    { (short)367, "Uploads", false, "INCOMINGS_FINISHED" },
                    { (short)366, "Uploads", false, "INCOMINGS_UPLOADING" },
                    { (short)365, "Uploads", false, "INCOMINGS_PROGRESS" },
                    { (short)364, "Uploads", false, "INCOMINGS_COLLECTING_PAGES" },
                    { (short)363, "Uploads", false, "COMMANDS_PROGRESS" },
                    { (short)362, "Uploads", false, "COMMANDS_SKIPPED_OLD" },
                    { (short)361, "Uploads", false, "COMMANDS_CHECK_UPLOADED_FAILED" },
                    { (short)360, "Uploads", false, "COMMANDS_FINISHED" },
                    { (short)359, "Uploads", false, "COMMANDS_UPLOADING" },
                    { (short)358, "Uploads", false, "COMMANDS_CHECK_UPLOADED" },
                    { (short)357, "Uploads", false, "COMMANDS_NONE_NEW" },
                    { (short)356, "Uploads", false, "COMMANDS_COLLECTING_PAGES" },
                    { (short)349, "Uploads", false, "UPLOAD_CLEAR_CACHE" },
                    { (short)380, "Uploads", false, "REPORTS_PROGRESS" },
                    { (short)348, "Uploads", false, "UPLOAD_ALL" },
                    { (short)381, "Uploads", false, "TROOPS_COLLECTING_PAGES" },
                    { (short)383, "Uploads", false, "TROOPS_NO_ACADEMY" },
                    { (short)408, "TribalWars", false, "UNIT_SPY_ALIASES" },
                    { (short)407, "TribalWars", false, "UNIT_SPY" },
                    { (short)406, "TribalWars", false, "UNIT_ARCHER_ALIASES" },
                    { (short)405, "TribalWars", false, "UNIT_ARCHER" },
                    { (short)404, "TribalWars", false, "UNIT_AXE_ALIASES" },
                    { (short)403, "TribalWars", false, "UNIT_AXE" },
                    { (short)402, "TribalWars", false, "UNIT_SWORD_ALIASES" },
                    { (short)401, "TribalWars", false, "UNIT_SWORD" },
                    { (short)400, "TribalWars", false, "UNIT_SPEAR_ALIASES" },
                    { (short)399, "TribalWars", false, "UNIT_SPEAR" },
                    { (short)398, "TribalWars", false, "UNIT_CATAPULT_ALIASES" },
                    { (short)397, "TribalWars", false, "UNIT_CATAPULT" },
                    { (short)396, "TribalWars", false, "UNIT_RAM_ALIASES" },
                    { (short)395, "TribalWars", false, "UNIT_RAM" },
                    { (short)394, "Uploads", false, "TROOPS_UPLOADING_SUPPORT" },
                    { (short)393, "Uploads", false, "TROOPS_FINISHED" },
                    { (short)392, "Uploads", false, "UPLOAD_ERROR" },
                    { (short)391, "Uploads", false, "TROOPS_UPLOADING" },
                    { (short)390, "Uploads", false, "TROOPS_ERROR_GETTING_NOBLES" },
                    { (short)389, "Uploads", true, "TROOPS_NOBLES_NUM_VILLAGES" },
                    { (short)388, "Uploads", true, "TROOPS_NOBLES_LIMIT" },
                    { (short)387, "Uploads", false, "TROOPS_ERROR_FINDING_ACADEMY" },
                    { (short)386, "Uploads", false, "TROOPS_COLLECTING_SUPPORT" },
                    { (short)385, "Uploads", false, "TROOPS_FIND_SUPPORT" },
                    { (short)384, "Uploads", false, "TROOPS_FIND_POSSIBLE_NOBLES" },
                    { (short)382, "Uploads", false, "TROOPS_FIND_ACADEMY" },
                    { (short)409, "TribalWars", false, "UNIT_LIGHT_CAV" },
                    { (short)347, "General", false, "COMMANDS" },
                    { (short)342, "General", false, "REPORTS" },
                    { (short)306, "General", false, "CONTINENTS" },
                    { (short)305, "General", false, "TRIBES" },
                    { (short)304, "General", false, "PLAYERS" },
                    { (short)303, "Tools", false, "DYNAMIC_FAKE_SCRIPTS" },
                    { (short)302, "Tabs", false, "TAB_FAKE_SCRIPT" },
                    { (short)301, "Tabs", false, "TAB_TOOLS" },
                    { (short)300, "General", false, "TERMS_DETAILS" },
                    { (short)299, "General", false, "TERMS_NOT_INNO" },
                    { (short)298, "Tabs", false, "TAB_TERMS" },
                    { (short)297, "Tabs", false, "TAB_SUPPORT" },
                    { (short)296, "Stats", false, "RANKINGS" },
                    { (short)295, "Stats", false, "RANKINGS_LOAD_ERROR" },
                    { (short)294, "Tabs", false, "TAB_HIGH_SCORES" },
                    { (short)293, "General", false, "SUPPORT" },
                    { (short)292, "Stats", false, "STATS_DVS_TRAVELING" },
                    { (short)291, "Stats", false, "STATS_BACKLINE_DVS_AT_HOME" },
                    { (short)290, "Stats", false, "STATS_DVS_AT_HOME" },
                    { (short)289, "Stats", false, "STATS_NUM_DVS" },
                    { (short)288, "General", false, "DEFENSE" },
                    { (short)287, "Stats", false, "STATS_TRAVELING_LANDED" },
                    { (short)286, "General", false, "FANGS" },
                    { (short)285, "General", false, "NUKES" },
                    { (short)284, "Stats", false, "STATS_7_DAYS" },
                    { (short)283, "Stats", false, "STATS_LOAD_ERROR" },
                    { (short)282, "Tabs", false, "TAB_ME" },
                    { (short)307, "Tools", false, "FAKES_MIN_COORD" },
                    { (short)345, "General", false, "INCOMINGS" },
                    { (short)308, "Tools", false, "FAKES_MAX_COORD" },
                    { (short)311, "Tools", false, "FAKES_DIST_FIELDS_FROM" },
                    { (short)341, "Uploads", false, "UPLOAD_DESCRIPTION" },
                    { (short)340, "General", false, "DETAILS" },
                    { (short)336, "General", false, "PROGRESS" },
                    { (short)335, "General", false, "UPLOAD" },
                    { (short)334, "General", false, "UNEXPECTED_ERROR" },
                    { (short)333, "General", false, "WAITING" },
                    { (short)332, "Uploads", false, "UPLOAD_CACHE_CLEARED" },
                    { (short)331, "Uploads", false, "UPLOAD_DESCRIPTION_TROOPS" },
                    { (short)330, "Uploads", false, "UPLOAD_DESCRIPTION_COMMANDS" },
                    { (short)329, "Uploads", false, "UPLOAD_DESCRIPTION_INCS" },
                    { (short)328, "Uploads", false, "UPLOAD_DESCRIPTION_REPORTS" },
                    { (short)327, "Tabs", false, "TAB_UPLOAD" },
                    { (short)326, "Tools", false, "BACKTIME_HIDE_STACKED_NUKES" },
                    { (short)325, "Tools", false, "BACKTIME_HIDE_HANDLED_NUKES" },
                    { (short)324, "Tools", false, "BACKTIME_MAX_NUM_TIMINGS" },
                    { (short)323, "Tools", false, "BACKTIME_MAX_TRAVEL_TIME" },
                    { (short)322, "Tools", false, "BACKTIME_MIN_ATTACK_SIZE_2" },
                    { (short)321, "Tools", false, "BACKTIME_MIN_ATTACK_SIZE_1" },
                    { (short)320, "Tools", false, "BACKTIME_MIN_RETURNING_POPULATION" },
                    { (short)319, "Tools", false, "BACKTIME_DESCRIPTION_2" },
                    { (short)318, "Tools", false, "BACKTIME_DESCRIPTION_1" },
                    { (short)316, "Tools", false, "BACKTIME_WORKING" },
                    { (short)315, "Tabs", false, "TAB_FIND_BACKTIMES" },
                    { (short)314, "Tools", false, "BACKTIME_RESULTS" },
                    { (short)312, "Tools", false, "GET_COORDS" },
                    { (short)310, "Tools", false, "FAKES_DIST_LABEL" },
                    { (short)410, "TribalWars", false, "UNIT_LIGHT_CAV_ALIASES" },
                    { (short)411, "TribalWars", false, "UNIT_M_ARCHER" },
                    { (short)412, "TribalWars", false, "UNIT_M_ARCHER_ALIASES" },
                    { (short)506, "General", false, "PREVIEW" },
                    { (short)505, "Tagging", false, "TAG_CODE_DISTANCE_DETAILS" },
                    { (short)504, "Tagging", false, "TAGGING_TITLE" },
                    { (short)503, "Map", false, "MAP_SETTINGS_HOVER_NUKES" },
                    { (short)501, "TribalWars", false, "YOUR_SUPPORT_FROM" },
                    { (short)499, "TribalWars", false, "UNIT_NOBLE" },
                    { (short)498, "TribalWars", false, "UNIT_NOBLE_ALIASES" },
                    { (short)497, "Uploads", false, "INCOMINGS_UPLOAD_ERROR" },
                    { (short)496, "Uploads", false, "INCOMINGS_NONE" },
                    { (short)495, "General", false, "OPTIONS" },
                    { (short)494, "General", false, "NAME" },
                    { (short)493, "General", false, "NOBLES" },
                    { (short)491, "Tools", false, "BACKTIME_HOURS" },
                    { (short)489, "General", false, "OFFENSE" },
                    { (short)488, "General", false, "NONE" },
                    { (short)487, "Admin", false, "ADMIN_CHANGE_OTHER_ADMIN" },
                    { (short)486, "Admin", false, "ADMIN_CHANGE_OWN_KEY" },
                    { (short)485, "Admin", false, "ADMIN_DELETE_OTHER_ADMIN" },
                    { (short)484, "Admin", false, "ADMIN_DELETE_OWN_KEY" },
                    { (short)483, "Admin", false, "ADMIN_KEY_NOT_FOUND" },
                    { (short)482, "Admin", false, "ADMIN_INVALID_KEY" },
                    { (short)481, "Admin", false, "ADMIN_PLAYER_HAS_KEY" },
                    { (short)480, "Admin", false, "ADMIN_PLAYER_NOT_IN_TRIBE" },
                    { (short)479, "Admin", false, "ADMIN_PLAYER_NAME_NOT_SET" },
                    { (short)478, "Admin", false, "ADMIN_PLAYER_NOT_FOUND_NAME" },
                    { (short)507, "Tools", false, "COMMANDS_FROM_HERE" },
                    { (short)477, "Admin", false, "ADMIN_PLAYER_NOT_FOUND_ID" },
                    { (short)510, "Tabs", false, "TAB_ALERTS" },
                    { (short)512, "TribalWars", true, "BB_URL" },
                    { (short)538, "Time", true, "TIME_NUMERIC_DATE" },
                    { (short)537, "Map", false, "MAP_HOVER_NO_BUILDINGS" },
                    { (short)536, "Map", false, "MAP_HOVER_NO_ARMY" },
                    { (short)535, "Translations", false, "TRANSLATION_MAKING_COPY" },
                    { (short)534, "Translations", false, "TRANSLATION_DUPLICATE" },
                    { (short)532, "Translations", false, "TRANSLATION_DELETED" },
                    { (short)531, "Translations", false, "TRANSLATION_NOT_SAVED" },
                    { (short)530, "Translations", false, "TRANSLATION_DELETE_DEFAULT" },
                    { (short)529, "Translations", false, "TRANSLATION_DELETE_CONFIRM" },
                    { (short)528, "Translations", false, "TRANSLATION_DELETE" },
                    { (short)527, "Translations", false, "TRANSLATION_SAVE_CHANGES_SUCCESS" },
                    { (short)526, "Translations", false, "TRANSLATION_EDIT_NEEDS_EXACT" },
                    { (short)525, "Translations", false, "TRANSLATION_EDIT_MISSING_PARAMS" },
                    { (short)524, "Translations", false, "TRANSLATION_EDIT_SAMPLE" },
                    { (short)523, "Translations", false, "TRANSLATION_EDIT_VALUE" },
                    { (short)522, "Translations", false, "TRANSLATION_EDIT_KEY" },
                    { (short)521, "Translations", false, "TRANSLATION_AUTHOR" },
                    { (short)520, "Translations", false, "TRANSLATION_SAVE_CHANGES" },
                    { (short)519, "Translations", false, "TRANSLATION_NEW" },
                    { (short)518, "Translations", false, "TRANSLATION_EDIT" },
                    { (short)517, "Translations", false, "TRANSLATION_SAVE_SETTINGS" },
                    { (short)516, "Translations", false, "TRANSLATION_TRANSLATION" },
                    { (short)515, "Translations", false, "TRANSLATION_LANGUAGE" },
                    { (short)514, "Tabs", false, "TAB_TRANSLATIONS" },
                    { (short)513, "Tabs", false, "TAB_HELP" },
                    { (short)511, "TribalWars", true, "BB_UNIT" },
                    { (short)476, "Admin", false, "ADMIN_LOG_DELETED_KEY" },
                    { (short)475, "Admin", false, "ADMIN_LOG_UNKNOWN_CHANGE" },
                    { (short)474, "Admin", false, "ADMIN_LOG_CHANGED_ADMIN" },
                    { (short)437, "Uploads", true, "REPORT_BUILDING_DAMAGE_LEVELS" },
                    { (short)436, "Uploads", true, "REPORT_BUILDING_DAMAGE_NAMES" },
                    { (short)435, "Uploads", true, "REPORT_LOYALTY_FROM_TO" },
                    { (short)434, "TribalWars", true, "BUILDING_CHURCH" },
                    { (short)433, "TribalWars", true, "BUILDING_WATCHTOWER" },
                    { (short)432, "TribalWars", true, "BUILDING_WALL" },
                    { (short)431, "TribalWars", true, "BUILDING_HIDING_PLACE" },
                    { (short)430, "TribalWars", true, "BUILDING_WAREHOUSE" },
                    { (short)429, "TribalWars", true, "BUILDING_FARM" },
                    { (short)428, "TribalWars", true, "BUILDING_IRON_MINE" },
                    { (short)427, "TribalWars", true, "BUILDING_CLAY_PIT" },
                    { (short)426, "TribalWars", true, "BUILDING_TIMBER_CAMP" },
                    { (short)425, "TribalWars", true, "BUILDING_MARKET" },
                    { (short)424, "TribalWars", true, "BUILDING_STATUE" },
                    { (short)423, "TribalWars", true, "BUILDING_RALLY_POINT" },
                    { (short)422, "TribalWars", true, "BUILDING_SMITHY" },
                    { (short)421, "TribalWars", true, "BUILDING_ACADEMY" },
                    { (short)420, "TribalWars", true, "BUILDING_WORKSHOP" },
                    { (short)419, "TribalWars", true, "BUILDING_STABLE" },
                    { (short)418, "TribalWars", true, "BUILDING_BARRACKS" },
                    { (short)417, "TribalWars", true, "BUILDING_HQ" },
                    { (short)416, "TribalWars", false, "UNIT_PALADIN_ALIASES" },
                    { (short)415, "TribalWars", false, "UNIT_PALADIN" },
                    { (short)414, "TribalWars", false, "UNIT_HEAVY_CAV_ALIASES" },
                    { (short)413, "TribalWars", false, "UNIT_HEAVY_CAV" },
                    { (short)438, "TribalWars", true, "BB_TABLE" },
                    { (short)439, "General", false, "TRIGGERED_CAPTCHA" },
                    { (short)440, "General", false, "IS_IN_GROUP" },
                    { (short)441, "General", false, "FILTER_APPLIED" },
                    { (short)473, "Admin", false, "ADMIN_LOG_ASSIGNED_ADMIN" },
                    { (short)472, "Admin", false, "ADMIN_LOG_CLEARED_ADMIN" },
                    { (short)471, "Admin", false, "ADMIN_LOG_CHANGED_SERVER" },
                    { (short)470, "Admin", false, "ADMIN_LOG_REMOVED_READ_ONLY" },
                    { (short)469, "Admin", false, "ADMIN_LOG_SET_READ_ONLY" },
                    { (short)468, "Admin", false, "ADMIN_LOG_CHANGED_KEY_OWNER" },
                    { (short)467, "Admin", false, "ADMIN_LOG_DISABLED_KEY_FOR" },
                    { (short)466, "Admin", false, "ADMIN_LOG_RE_ENABLED_KEY_FOR" },
                    { (short)465, "Admin", false, "ADMIN_LOG_GAVE_PRIVELEGES_TO" },
                    { (short)464, "Admin", false, "ADMIN_LOG_REVOKED_PRIVELEGES_FOR" },
                    { (short)463, "Admin", false, "ADMIN_LOG_ADDED_KEY_FOR" },
                    { (short)462, "General", false, "REQUEST_STATS" },
                    { (short)275, "Tabs", false, "TAB_STATS" },
                    { (short)461, "Time", false, "TIME_SECOND_PLURAL_SHORT" },
                    { (short)459, "Time", false, "TIME_HOUR_PLURAL_SHORT" },
                    { (short)458, "Time", false, "TIME_DAY_PLURAL_SHORT" },
                    { (short)457, "Time", false, "TIME_SECOND_SHORT" },
                    { (short)456, "Time", false, "TIME_MINUTE_SHORT" },
                    { (short)455, "Time", false, "TIME_HOUR_SHORT" },
                    { (short)454, "Time", false, "TIME_DAY_SHORT" },
                    { (short)453, "Time", false, "TIME_DATE_FORMAT" },
                    { (short)452, "Time", false, "TIME_ON_AT" },
                    { (short)451, "Time", true, "TIME_ON" },
                    { (short)450, "Time", true, "TIME_TOMORROW_AT" },
                    { (short)449, "Time", true, "TIME_TODAY_AT" },
                    { (short)448, "Time", true, "ORDERED_MONTHS" },
                    { (short)460, "Time", false, "TIME_MINUTE_PLURAL_SHORT" },
                    { (short)264, "General", false, "OPTIONAL" },
                    { (short)268, "General", false, "SAVE" },
                    { (short)118, "Map", false, "MAP_SETTINGS_OVERLAY_STACK_1" },
                    { (short)87, "Map", false, "MAP_HOVER_ARMY_OWNED" },
                    { (short)86, "Map", false, "MAP_HOVER_ARMY_TRAVELING" },
                    { (short)85, "Map", false, "MAP_HOVER_ARMY_STATIONED" },
                    { (short)84, "Map", false, "MAP_HOVER_ARMY_AT_HOME" },
                    { (short)83, "Map", false, "MAP_HOVER_CMD_NUM_PLAYERS" },
                    { (short)82, "Map", false, "MAP_HOVER_CMD_DVS" },
                    { (short)81, "Map", false, "MAP_HOVER_CMD_NOBLES" },
                    { (short)80, "Map", false, "MAP_HOVER_CMD_NUKES" },
                    { (short)79, "Map", false, "MAP_HOVER_CMD_FAKES" },
                    { (short)78, "Map", false, "MAP_UPLOAD_DATA_REQUIRED" },
                    { (short)77, "Map", false, "MAP_HIGHLIGHT_NO" },
                    { (short)76, "Map", false, "MAP_HIGHLIGHT_ANY" },
                    { (short)75, "Map", false, "MAP_USING_VAULT" },
                    { (short)74, "Tagging", false, "MAYBE_NUKE" },
                    { (short)73, "General", false, "FAKE" },
                    { (short)72, "General", false, "FAKES" },
                    { (short)71, "General", false, "UNKNOWN" },
                    { (short)70, "Tagging", false, "TAG_STATE_CANCELED" },
                    { (short)69, "Tagging", false, "TAG_STATE_FINISHED" },
                    { (short)68, "Tagging", false, "TAG_STATE_RUNNING" },
                    { (short)67, "Tagging", false, "TAGGING_PROGRESS" },
                    { (short)66, "Tagging", false, "TAGGING_CANCELED" },
                    { (short)65, "Tagging", false, "TAGS_ARE_CURRENT" },
                    { (short)64, "Tagging", false, "NO_INCOMINGS_SELECTED" },
                    { (short)63, "Tagging", false, "FAKE_DETECTION_CONFIRM" },
                    { (short)88, "Map", false, "MAP_HOVER_ARMY_POSSIBLE_RECRUIT" },
                    { (short)62, "General", false, "NOT_A_NUMBER" },
                    { (short)89, "Map", false, "MAP_HOVER_NUKE_ESTIMATE" },
                    { (short)91, "Map", false, "MAP_HOVER_LATEST_LEVELS" },
                    { (short)117, "Map", false, "MAP_SETTINGS_OVERLAY_DV" },
                    { (short)116, "Map", false, "MAP_SETTINGS_OVERLAY_WATCHTOWER" },
                    { (short)115, "Map", false, "MAP_SETTINGS_OVERLAY_RETURNING_2" },
                    { (short)114, "Map", false, "MAP_SETTINGS_OVERLAY_RETURNING_1" },
                    { (short)112, "Map", false, "MAP_SETTINGS_OVERLAY_SHOW_WALL" },
                    { (short)111, "Map", false, "MAP_SETTINGS_OVERLAY_SHOW_STACKS" },
                    { (short)110, "Map", false, "MAP_SETTINGS_OVERLAY_SHOW_NOBLES" },
                    { (short)109, "Map", false, "MAP_SETTINGS_OVERLAY_SHOW_NUKES" },
                    { (short)108, "Map", false, "MAP_SETTINGS_OVERLAY_HIGHLIGHTS_HAS_INTEL" },
                    { (short)107, "Map", false, "MAP_SETTINGS_OVERLAY_HIGHLIGHTS_HAS_GROUP" },
                    { (short)106, "Map", false, "MAP_SETTINGS_OVERLAY_HIGHLIGHTS_NONE" },
                    { (short)105, "Map", false, "MAP_SETTINGS_OVERLAY_HIGHLIGHTS" },
                    { (short)104, "Map", false, "MAP_SETTINGS_OVERLAY_IGNORE_INTEL_2" },
                    { (short)103, "Map", false, "MAP_SETTINGS_OVERLAY_IGNORE_INTEL_1" },
                    { (short)102, "Map", false, "MAP_SETTINGS_OVERLAY_SHOW" },
                    { (short)101, "Map", false, "MAP_SETTINGS_OVERLAY" },
                    { (short)100, "Map", false, "MAP_SETTINGS_HOVER_LOYALTY" },
                    { (short)99, "Map", false, "MAP_SETTINGS_HOVER_BUILDINGS" },
                    { (short)98, "Map", false, "MAP_SETTINGS_HOVER_RECRUITS" },
                    { (short)97, "Map", false, "MAP_SETTINGS_HOVER_COMMANDS" },
                    { (short)96, "Map", false, "MAP_SETTINGS_HOVER" },
                    { (short)95, "Map", false, "MAP_HOVER_POSSIBLE_LOYALTY" },
                    { (short)94, "Map", false, "MAP_HOVER_LATEST_LOYALTY" },
                    { (short)93, "Map", false, "MAP_HOVER_LOYALTY" },
                    { (short)92, "Map", false, "MAP_HOVER_POSSIBLE_LEVELS" },
                    { (short)90, "Map", false, "MAP_HOVER_SEEN_AT" },
                    { (short)244, "General", false, "ADD" },
                    { (short)61, "Tagging", false, "TAG_DURATION_NOTICE" },
                    { (short)59, "Tagging", false, "TAG_REVERT" },
                    { (short)29, "Tagging", false, "INCS_NOT_TAGGED" },
                    { (short)28, "Uploads", false, "UPLOADING_IF_CLOSED" },
                    { (short)27, "General", false, "DONE" },
                    { (short)26, "General", false, "VAULT_INTERFACE_DESCRIPTION" },
                    { (short)25, "General", false, "VAULT" },
                    { (short)24, "General", false, "TROOP_REQUIRED" },
                    { (short)23, "Time", false, "LANDING_TIME" },
                    { (short)22, "Time", false, "LAUNCH_TIME" },
                    { (short)21, "General", false, "SOURCE_VILLAGE" },
                    { (short)20, "General", false, "TROOPS" },
                    { (short)19, "Tools", false, "NO_DATA_AVAILABLE" },
                    { (short)18, "Tools", false, "NO_COMMANDS_AVAILABLE" },
                    { (short)17, "General", false, "ERROR_OCCURRED" },
                    { (short)16, "General", false, "UPLOAD_DATA_REQUIRED_REASONS" },
                    { (short)15, "General", false, "UPLOAD_TROOPS_REQUIRED" },
                    { (short)14, "General", false, "UPLOAD_REPORTS_REQUIRED" },
                    { (short)13, "General", false, "UPLOAD_INCOMINGS_REQUIRED" },
                    { (short)12, "General", false, "UPLOAD_COMMANDS_REQUIRED" },
                    { (short)11, "Tools", false, "BACKTIME_UPLOAD_DATA_REQUIRED" },
                    { (short)10, "Tools", false, "BACKTIME_BB_CODE_HOVER" },
                    { (short)9, "General", false, "UPDATE_NOTICE" },
                    { (short)8, "General", false, "SCRIPT_NOT_RAN" },
                    { (short)7, "General", false, "RE_RUN_SCRIPT" },
                    { (short)6, "General", false, "TERMS_AND_CONDITIONS" },
                    { (short)5, "General", false, "REQUIRE_PREMIUM_ACCOUNT" },
                    { (short)30, "Tagging", false, "TAG_UPLOAD_DATA_REQUIRED" },
                    { (short)60, "General", false, "CANCEL" },
                    { (short)31, "Tagging", false, "FEATURE_IS_EXPERIMENTAL" },
                    { (short)33, "Tagging", false, "TAG_CODE_HEADER" },
                    { (short)58, "Tagging", false, "TAG_SELECTED" },
                    { (short)57, "Tagging", false, "TAG_ALL" },
                    { (short)56, "Tagging", false, "TAG_CFG_IGNORE_NO_DATA" },
                    { (short)55, "Tagging", false, "TAG_CFG_AUTOTAG_FAKE_2" },
                    { (short)54, "Tagging", false, "TAG_CFG_AUTOTAG_FAKE_1" },
                    { (short)53, "Tagging", false, "TAG_CFG_ONLY_UNLABELED" },
                    { (short)52, "General", false, "RESET" },
                    { (short)51, "Tagging", false, "TAG_CFG_FORMAT" },
                    { (short)50, "Tagging", false, "TAG_CODE_CUSTOM_LABEL_DETAILS" },
                    { (short)49, "Tagging", false, "TAG_CODE_VILLAGE_TYPE_DETAILS" },
                    { (short)48, "Tagging", false, "TAG_CODE_TGT_COORDS_DETAILS" },
                    { (short)47, "Tagging", false, "TAG_CODE_SRC_COORDS_DETAILS" },
                    { (short)46, "Tagging", false, "TAG_CODE_TGT_VILLAGE_DETAILS" },
                    { (short)45, "Tagging", false, "TAG_CODE_TGT_PLAYER_DETAILS" },
                    { (short)44, "Tagging", false, "TAG_CODE_SRC_VILLAGE_DETAILS" },
                    { (short)43, "Tagging", false, "TAG_CODE_SRC_PLAYER_DETAILS" },
                    { (short)42, "Tagging", false, "TAG_CODE_NUM_COMS_DETAILS" },
                    { (short)41, "Tagging", false, "TAG_CODE_NUM_CATS_DETAILS" },
                    { (short)40, "Tagging", false, "TAG_CODE_POP_RETURN_COUNT_DETAILS" },
                    { (short)39, "Tagging", false, "TAG_CODE_POP_RETURN_PERCENT_DETAILS" },
                    { (short)38, "Tagging", false, "TAG_CODE_POP_COUNT_DETAILS" },
                    { (short)37, "Tagging", false, "TAG_CODE_POP_PERCENT_DETAILS" },
                    { (short)36, "Tagging", false, "TAG_CODE_TAG_TYPE_DETAILS" },
                    { (short)35, "Tagging", false, "TAG_CODE_TROOP_NAME_DETAILS" },
                    { (short)34, "Tagging", false, "TAG_CODE_DETAILS" },
                    { (short)32, "Tagging", false, "UPLOAD_VISIBLE_INCOMINGS" },
                    { (short)4, "Time", false, "ARRIVES_IN" },
                    { (short)119, "Map", false, "MAP_SETTINGS_OVERLAY_STACK_2" },
                    { (short)121, "Map", false, "MAP_SETTINGS_OVERLAY_TRIBE_2" },
                    { (short)205, "Admin", false, "ADMIN_MANAGE_USERS_VAULT_SCRIPT_FOR" },
                    { (short)204, "Admin", false, "ADMIN_MANAGE_USERS_CONFIRM_GIVE_ADMIN" },
                    { (short)203, "Admin", false, "ADMIN_MANAGE_USERS_CONFIRM_REMOVE_ADMIN" },
                    { (short)202, "Admin", false, "ADMIN_MANAGE_USERS_CONFIRM_DELETE" },
                    { (short)201, "General", false, "GET_SCRIPT" },
                    { (short)200, "Admin", false, "ADMIN_MANAGE_USERS_REVOKE_ADMIN" },
                    { (short)199, "Admin", false, "ADMIN_MANAGE_USERS_GIVE_ADMIN" },
                    { (short)198, "General", false, "NO_TRIBE" },
                    { (short)197, "Admin", false, "ADMIN_MANAGE_USERS_ENTER_NAME" },
                    { (short)196, "General", false, "TIME" },
                    { (short)195, "General", false, "EVENT" },
                    { (short)194, "Admin", false, "ADMIN" },
                    { (short)193, "Admin", false, "ADMIN_USER_LOG" },
                    { (short)192, "Tabs", false, "TAB_LOG" },
                    { (short)191, "General", false, "DOWNLOAD" },
                    { (short)190, "Admin", false, "ADMIN_TRIBE_STATS_SETTINGS_NUKES" },
                    { (short)189, "Admin", false, "ADMIN_TRIBE_STATS_DESCRIPTION" },
                    { (short)188, "General", false, "WORKING" },
                    { (short)187, "Tabs", false, "TAB_TRIBE_STATS" },
                    { (short)186, "General", false, "CURRENT_TRIBE" },
                    { (short)185, "General", false, "USER_NAME" },
                    { (short)184, "Admin", false, "ADMIN_NEW_VAULT_SCRIPT" },
                    { (short)183, "Admin", false, "ADMIN_NEW_KEY" },
                    { (short)182, "Admin", false, "KEYS" },
                    { (short)181, "Tabs", false, "TAB_MANAGE_USERS" },
                    { (short)206, "Admin", false, "ADMIN_TRIBE_STATS_25%_NUKES" },
                    { (short)180, "Admin", false, "ADMIN_ADD_ENEMY_TRIBE" },
                    { (short)207, "Admin", false, "ADMIN_TRIBE_STATS_50%_NUKES" },
                    { (short)209, "Admin", false, "ADMIN_TRIBE_STATS_TOTAL_FULL_NUKES" },
                    { (short)243, "General", false, "MESSAGE" },
                    { (short)241, "General", false, "SERVER_TIME" },
                    { (short)234, "Tabs", false, "TAB_NOTIFICATIONS" },
                    { (short)231, "Admin", false, "ADMIN_TRIBE_STATS_DVS_TO_TRIBE" },
                    { (short)230, "Admin", false, "ADMIN_TRIBE_STATS_NUM_ATTACKS" },
                    { (short)229, "Admin", false, "ADMIN_TRIBE_STATS_NUM_INCS" },
                    { (short)228, "Admin", false, "ADMIN_TRIBE_STATS_DEF_VILLAS" },
                    { (short)227, "Admin", false, "ADMIN_TRIBE_STATS_OFF_VILLAS" },
                    { (short)226, "Admin", false, "ADMIN_TRIBE_STATS_DVS_TO_OTHERS" },
                    { (short)225, "Admin", false, "ADMIN_TRIBE_STATS_DVS_TO_SELF" },
                    { (short)224, "Admin", false, "ADMIN_TRIBE_STATS_DVS_TRAVELING" },
                    { (short)223, "Admin", false, "ADMIN_TRIBE_STATS_BACKLINE_DVS_HOME" },
                    { (short)222, "Admin", false, "ADMIN_TRIBE_STATS_DVS_HOME" },
                    { (short)221, "Admin", false, "ADMIN_TRIBE_STATS_OWNED_DVS" },
                    { (short)220, "General", false, "POSSIBLE_NOBLES" },
                    { (short)219, "Admin", false, "ADMIN_TRIBE_STATS_NUKES_TRAVELING" },
                    { (short)218, "Admin", false, "ADMIN_TRIBE_STATS_FULL_NUKES" },
                    { (short)217, "General", false, "PLAYER" },
                    { (short)216, "General", false, "YES" },
                    { (short)215, "Admin", false, "ADMIN_TRIBE_STATS_NEEDS_UPLOAD" },
                    { (short)214, "Admin", false, "ADMIN_TRIBE_STATS_TOTAL_ATTACKS" },
                    { (short)213, "Admin", false, "ADMIN_TRIBE_STATS_TOTAL_INCS" },
                    { (short)212, "Admin", false, "ADMIN_TRIBE_STATS_TOTAL_DVS" },
                    { (short)211, "Admin", false, "ADMIN_TRIBE_STATS_TOTAL_POSSIBLE_NOBLES" },
                    { (short)210, "Admin", false, "ADMIN_TRIBE_STATS_TOTAL_NOBLES" },
                    { (short)208, "Admin", false, "ADMIN_TRIBE_STATS_75%_NUKES" },
                    { (short)120, "Map", false, "MAP_SETTINGS_OVERLAY_TRIBE_1" },
                    { (short)179, "Admin", false, "ADMIN_ENEMY_TRIBES_DESCRIPTION" },
                    { (short)177, "Admin", false, "ADMIN_TRIBE_ALREADY_EXISTS" },
                    { (short)147, "Actions", false, "ACTIONS_NOBLE_TARGETS_DV_AGE" },
                    { (short)146, "Actions", false, "ACTIONS_NOBLE_TARGETS_NONE" },
                    { (short)145, "Tabs", false, "TAB_NOBLE_TARGETS" },
                    { (short)144, "Actions", false, "ACTIONS_STACKS_CURRENT_STRENGTH" },
                    { (short)143, "Actions", false, "ACTIONS_STACKS_POSSIBLE_NUKES" },
                    { (short)142, "Actions", false, "ACTIONS_STACKS_DESCRIPTION" },
                    { (short)141, "Actions", false, "ACTIONS_STACKS_EATABLE_NUKES" },
                    { (short)140, "Actions", false, "ACTIONS_STACKS_NONE" },
                    { (short)139, "Tabs", false, "TAB_SEND_STACKS" },
                    { (short)137, "Time", false, "LANDS_AT" },
                    { (short)136, "Actions", false, "ACTIONS_SNIPES_NUM_NOBLES" },
                    { (short)135, "Actions", false, "ACTIONS_SNIPES_DESCRIPTION" },
                    { (short)134, "Actions", false, "ACTIONS_SNIPES_NONE" },
                    { (short)133, "Tabs", false, "TAB_SNIPES_NEEDED" },
                    { (short)132, "Actions", false, "ACTIONS_RECAPS_NEW_OWNER" },
                    { (short)131, "Actions", false, "ACTIONS_RECAPS_OLD_OWNER" },
                    { (short)130, "Actions", false, "ACTIONS_RECAPS_CAPTURED_AT" },
                    { (short)129, "General", false, "LOYALTY" },
                    { (short)128, "General", false, "VILLAGE" },
                    { (short)127, "Actions", false, "ACTIONS_RECAPS_ONLY_NEARBY" },
                    { (short)126, "Actions", false, "ACTIONS_RECAPS_DESCRIPTION" },
                    { (short)125, "Actions", false, "ACTIONS_RECAPS_AGE" },
                    { (short)124, "Actions", false, "ACTIONS_RECAPS_NONE" },
                    { (short)123, "Tabs", false, "TAB_ACTIONS_ALERTS" },
                    { (short)122, "Tabs", false, "TAB_SEND_RECAP" },
                    { (short)148, "Actions", false, "ACTIONS_NOBLE_TARGETS_DESCRIPTION" },
                    { (short)178, "Admin", false, "ADMIN_ENEMY_TRIBES" },
                    { (short)149, "Actions", false, "ACTIONS_NOBLE_TARGETS_STATIONED_DVS" },
                    { (short)151, "General", false, "OWNER" },
                    { (short)176, "Admin", false, "ADMIN_TRIBE_NOT_FOUND" },
                    { (short)175, "Admin", false, "ADMIN_NAME_OF_TRIBE" },
                    { (short)174, "Admin", false, "ERROR_LOADING_ENEMY_TRIBES" },
                    { (short)173, "Tabs", false, "TAB_ENEMY_TRIBES" },
                    { (short)172, "Admin", false, "ADMIN_REMOVE_ENEMY" },
                    { (short)171, "General", false, "DELETE" },
                    { (short)170, "Tabs", false, "TAB_ADMIN" },
                    { (short)169, "Actions", false, "ACTIONS_REQUEST_STACK_SETTINGS_3" },
                    { (short)168, "Actions", false, "ACTIONS_REQUEST_STACK_SETTINGS_2" },
                    { (short)167, "Actions", false, "ACTIONS_REQUEST_STACK_SETTINGS_1" },
                    { (short)166, "Actions", false, "ACTIONS_REQUEST_STACK_DESCRIPTION" },
                    { (short)165, "Tabs", false, "TAB_REQUEST_STACK" },
                    { (short)164, "General", false, "RESULTS" },
                    { (short)163, "General", false, "SEARCH" },
                    { (short)162, "Actions", false, "ACTIONS_QUICK_SUPPORT_SETTINGS_3" },
                    { (short)161, "Actions", false, "ACTIONS_QUICK_SUPPORT_SETTINGS_2" },
                    { (short)160, "Actions", false, "ACTIONS_QUICK_SUPPORT_SETTINGS_1" },
                    { (short)159, "Actions", false, "ACTIONS_QUICK_SUPPORT_DESCRIPTION" },
                    { (short)158, "General", false, "NO_PLAYERS_FOUND" },
                    { (short)157, "Tabs", false, "TAB_QUICK_SUPPORT" },
                    { (short)156, "Actions", false, "ACTIONS_USELESS_STACKS_POP_COUNT" },
                    { (short)155, "General", false, "TRIBE" },
                    { (short)154, "Actions", false, "ACTIONS_USELESS_STACKS_DESCRIPTION" },
                    { (short)153, "Actions", false, "ACTIONS_USELESS_STACKS_NONE" },
                    { (short)152, "Tabs", false, "TAB_USELESS_STACKS" },
                    { (short)150, "Actions", false, "ACTIONS_NOBLE_TARGETS_DVS_SEEN_AT" },
                    { (short)3, "Time", false, "ARRIVAL_TIME" }
                });

            migrationBuilder.InsertData(
                schema: "feature",
                table: "translation_language",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { (short)3, "Slovenščina" },
                    { (short)1, "English" },
                    { (short)4, "русский" }
                });

            migrationBuilder.InsertData(
                schema: "feature",
                table: "translation_parameter",
                columns: new[] { "id", "key_id", "name" },
                values: new object[,]
                {
                    { (short)1, (short)29, "numIncs" },
                    { (short)67, (short)464, "playerName" },
                    { (short)66, (short)463, "playerName" },
                    { (short)65, (short)462, "numFailed" },
                    { (short)64, (short)462, "numTotal" },
                    { (short)63, (short)462, "numDone" },
                    { (short)62, (short)453, "year" },
                    { (short)61, (short)453, "month" },
                    { (short)60, (short)453, "day" },
                    { (short)68, (short)465, "playerName" },
                    { (short)59, (short)453, "second" },
                    { (short)57, (short)453, "hour" },
                    { (short)56, (short)452, "time" },
                    { (short)55, (short)452, "date" },
                    { (short)54, (short)451, "date" },
                    { (short)53, (short)451, "time" },
                    { (short)52, (short)450, "time" },
                    { (short)51, (short)449, "time" },
                    { (short)50, (short)441, "dataType" },
                    { (short)58, (short)453, "minute" },
                    { (short)69, (short)466, "playerName" },
                    { (short)70, (short)467, "playerName" },
                    { (short)71, (short)468, "oldPlayer" },
                    { (short)90, (short)538, "month" },
                    { (short)89, (short)538, "day" },
                    { (short)88, (short)534, "name" },
                    { (short)87, (short)529, "name" },
                    { (short)86, (short)525, "parameters" },
                    { (short)85, (short)525, "keyName" },
                    { (short)84, (short)521, "author" },
                    { (short)83, (short)521, "name" },
                    { (short)82, (short)476, "playerName" },
                    { (short)81, (short)475, "playerName" },
                    { (short)80, (short)474, "newAdmin" },
                    { (short)79, (short)474, "oldAdmin" },
                    { (short)78, (short)474, "playerName" },
                    { (short)77, (short)473, "playerName" },
                    { (short)76, (short)472, "playerName" },
                    { (short)75, (short)471, "playerName" },
                    { (short)74, (short)470, "playerName" },
                    { (short)73, (short)469, "playerName" },
                    { (short)72, (short)468, "newPlayer" },
                    { (short)49, (short)437, "newLevel" },
                    { (short)91, (short)538, "year" },
                    { (short)48, (short)436, "buildingName" },
                    { (short)46, (short)435, "oldLoyalty" },
                    { (short)22, (short)314, "numShown" },
                    { (short)21, (short)314, "numNukes" },
                    { (short)20, (short)314, "numTimings" },
                    { (short)16, (short)231, "tribeName" },
                    { (short)15, (short)205, "playerName" },
                    { (short)14, (short)204, "playerName" },
                    { (short)13, (short)203, "playerName" },
                    { (short)12, (short)202, "playerName" },
                    { (short)23, (short)360, "numDone" },
                    { (short)11, (short)172, "tribeName" },
                    { (short)9, (short)141, "numNukes" },
                    { (short)5, (short)125, "duration" },
                    { (short)8, (short)89, "lossPercent" },
                    { (short)7, (short)89, "morale" },
                    { (short)6, (short)89, "nukesRequired" },
                    { (short)4, (short)67, "numFailed" },
                    { (short)3, (short)67, "numTotal" },
                    { (short)2, (short)67, "numDone" },
                    { (short)10, (short)147, "duration" },
                    { (short)24, (short)360, "numTotal" },
                    { (short)25, (short)360, "numFailed" },
                    { (short)26, (short)362, "numCommands" },
                    { (short)45, (short)393, "numVillages" },
                    { (short)44, (short)389, "numVillages" },
                    { (short)43, (short)388, "numNobles" },
                    { (short)42, (short)380, "numFailed" },
                    { (short)41, (short)380, "numDone" },
                    { (short)40, (short)380, "numTotal" },
                    { (short)39, (short)378, "numFailed" },
                    { (short)38, (short)378, "numTotal" },
                    { (short)37, (short)378, "numDone" },
                    { (short)36, (short)376, "numOld" },
                    { (short)35, (short)370, "numTotal" },
                    { (short)34, (short)370, "numDone" },
                    { (short)33, (short)367, "numIncomings" },
                    { (short)32, (short)365, "numFailed" },
                    { (short)31, (short)365, "numTotal" },
                    { (short)30, (short)365, "numDone" },
                    { (short)29, (short)363, "numFailed" },
                    { (short)28, (short)363, "numTotal" },
                    { (short)27, (short)363, "numDone" },
                    { (short)47, (short)435, "newLoyalty" }
                });

            migrationBuilder.InsertData(
                schema: "feature",
                table: "translation_registry",
                columns: new[] { "id", "author", "author_player_id", "language_id", "name" },
                values: new object[] { (short)1, "tcamps", 11301059L, (short)1, "Default" });

            migrationBuilder.InsertData(
                schema: "feature",
                table: "translation",
                columns: new[] { "translation_id", "key", "value" },
                values: new object[,]
                {
                    { (short)1, (short)2, "Open Vault" },
                    { (short)1, (short)378, "Finished: {numDone}/{numTotal} uploaded, {numFailed} failed." },
                    { (short)1, (short)377, "An error occurred while checking for existing reports, continuing..." },
                    { (short)1, (short)376, "Found {numOld} previously uploaded reports, skipping these..." },
                    { (short)1, (short)375, "Error getting Loot Assistant reports folder, skipping filtering..." },
                    { (short)1, (short)374, "Filtering loot assistant reports..." },
                    { (short)1, (short)373, "Loot Assistant" },
                    { (short)1, (short)372, "Couldn't find Loot Assistant reports folder, skipping filtering..." },
                    { (short)1, (short)371, "Checking for reports already uploaded..." },
                    { (short)1, (short)370, "(page {numDone}/{numTotal})" },
                    { (short)1, (short)369, "Collecting report links..." },
                    { (short)1, (short)368, "Collecting report pages..." },
                    { (short)1, (short)367, "Finished: Uploaded {numIncomings} incomings." },
                    { (short)1, (short)366, "Uploading incomings..." },
                    { (short)1, (short)365, "({numDone}/{numTotal} done, {numFailed} failed)" },
                    { (short)1, (short)364, "Collecting incoming pages..." },
                    { (short)1, (short)363, "({numDone}/{numTotal} done, {numFailed} failed)" },
                    { (short)1, (short)362, "Found {numCommands} old commands, skipping these..." },
                    { (short)1, (short)361, "Failed to check for old commands, uploading all..." },
                    { (short)1, (short)360, "Finished: {numDone}/{numTotal} uploaded, {numFailed} failed." },
                    { (short)1, (short)359, "Uploading commands..." },
                    { (short)1, (short)358, "Checking for previously-uploaded commands..." },
                    { (short)1, (short)357, "Finished: No new commands to upload." },
                    { (short)1, (short)356, "Collecting command pages..." },
                    { (short)1, (short)349, "Clear Cache" },
                    { (short)1, (short)348, "Upload All" },
                    { (short)1, (short)379, "Finished: No new reports to upload." },
                    { (short)1, (short)347, "Commands" },
                    { (short)1, (short)380, "Uploading {numTotal} reports... ({numDone} done, {numFailed} failed.)" },
                    { (short)1, (short)382, "Finding village with academy..." },
                    { (short)1, (short)407, "Scout" },
                    { (short)1, (short)406, "archer, ar" },
                    { (short)1, (short)405, "Archer" },
                    { (short)1, (short)404, "axe" },
                    { (short)1, (short)403, "Axe" },
                    { (short)1, (short)402, "sword, swords, sw" },
                    { (short)1, (short)401, "Sword" },
                    { (short)1, (short)400, "spear, spears, sp" },
                    { (short)1, (short)399, "Spear" },
                    { (short)1, (short)398, "catapult, cat, cats" },
                    { (short)1, (short)397, "Catapult" },
                    { (short)1, (short)396, "ram, rams" },
                    { (short)1, (short)395, "Ram" },
                    { (short)1, (short)394, "Uploading support to vault..." },
                    { (short)1, (short)393, "Finished: Uploaded troops for {numVillages} villages." },
                    { (short)1, (short)392, "An error occurred while uploading to the vault." },
                    { (short)1, (short)391, "Uploading troops to vault..." },
                    { (short)1, (short)390, "An error occurred while getting possible noble counts..." },
                    { (short)1, (short)389, "Number of conquered villages:{numVillages}" },
                    { (short)1, (short)388, "Noblemen limit:{numNobles}" },
                    { (short)1, (short)387, "An error occurred while finding villa with academy..." },
                    { (short)1, (short)386, "Collecting supported villages and DVs..." },
                    { (short)1, (short)385, "Getting support..." },
                    { (short)1, (short)384, "Getting possible nobles..." },
                    { (short)1, (short)383, "(No village with academy found)" },
                    { (short)1, (short)381, "Getting village troop pages..." },
                    { (short)1, (short)408, "scout, scouts, sc" },
                    { (short)1, (short)345, "Incomings" },
                    { (short)1, (short)341, "Click \"Upload All\" below. If needed, upload different things individually using the other Upload buttons." },
                    { (short)1, (short)305, "Tribes" },
                    { (short)1, (short)304, "Players" },
                    { (short)1, (short)303, "Dynamic Fake Scripts" },
                    { (short)1, (short)302, "Fake Script" },
                    { (short)1, (short)301, "Tools" },
                    { (short)1, (short)300, @"All data and requests to the Vault will have various information logged for security. This is limited to:

                Authentication token, IP address, player ID, tribe ID, requested endpoint, and time of transaction.

                Requests to this script will only be IP-logged to protect against abuse. Information collected by this script will never be shared with any third parties or any unauthorized tribes/players." },
                    { (short)1, (short)299, "This tool is not endorsed or developed by InnoGames." },
                    { (short)1, (short)298, "Disclaimers and Terms" },
                    { (short)1, (short)297, "Support" },
                    { (short)1, (short)296, "Rankings" },
                    { (short)1, (short)295, "An error occurred while getting rankings" },
                    { (short)1, (short)294, "High Scores" },
                    { (short)1, (short)293, "Support" },
                    { (short)1, (short)292, "Traveling" },
                    { (short)1, (short)291, "(Backline) At Home" },
                    { (short)1, (short)290, "At Home" },
                    { (short)1, (short)289, "# DVs" },
                    { (short)1, (short)288, "Defense" },
                    { (short)1, (short)287, "Support" },
                    { (short)1, (short)286, "Fangs" },
                    { (short)1, (short)285, "Nukes" },
                    { (short)1, (short)284, "Last 7 Days" },
                    { (short)1, (short)283, "An error occurred while loading stats" },
                    { (short)1, (short)282, "Me" },
                    { (short)1, (short)275, "Stats" },
                    { (short)1, (short)306, "Continents" },
                    { (short)1, (short)342, "Reports" },
                    { (short)1, (short)307, "Min Coord" },
                    { (short)1, (short)310, "Dist From Center" },
                    { (short)1, (short)340, "Details" },
                    { (short)1, (short)336, "Progress" },
                    { (short)1, (short)335, "Upload" },
                    { (short)1, (short)334, "An unexpected error occurred:" },
                    { (short)1, (short)333, "Waiting" },
                    { (short)1, (short)332, "Local vault cache cleared." },
                    { (short)1, (short)331, "Uploads all data for all troops." },
                    { (short)1, (short)330, "Uploads all data for all of your current commands." },
                    { (short)1, (short)329, "Uploads all available data from your Incomings page. This includes attacks and support." },
                    { (short)1, (short)328, "Uploads all data from all new battle reports." },
                    { (short)1, (short)327, "Upload" },
                    { (short)1, (short)326, "Hide stacked nukes:" },
                    { (short)1, (short)325, "Hide backtimed nukes:" },
                    { (short)1, (short)324, "Max number of timings:" },
                    { (short)1, (short)323, "Max travel time:" },
                    { (short)1, (short)322, "% of a full nuke" },
                    { (short)1, (short)321, "Minimum attack size:" },
                    { (short)1, (short)320, "Minimum returning population:" },
                    { (short)1, (short)319, "Upload your troops frequently to get the most accurate timings!" },
                    { (short)1, (short)318, "Get plans for all available backtimes that you can make for enemy nukes using the troops you've uploaded to the vault." },
                    { (short)1, (short)316, "Working... (This may take a while)" },
                    { (short)1, (short)315, "Find Backtimes" },
                    { (short)1, (short)314, "Found {numTimings} timings for {numNukes} returning nukes ({numShown} shown)" },
                    { (short)1, (short)312, "Get Coords" },
                    { (short)1, (short)311, "fields from" },
                    { (short)1, (short)308, "Max Coord" },
                    { (short)1, (short)268, "Save" },
                    { (short)1, (short)409, "Light Cav." },
                    { (short)1, (short)411, "Mounted Ar." },
                    { (short)1, (short)504, "Vault Tagging" },
                    { (short)1, (short)503, "Nukes" },
                    { (short)1, (short)501, "Your support from" },
                    { (short)1, (short)499, "Noble" },
                    { (short)1, (short)498, "noble, nobleman, nobles" },
                    { (short)1, (short)497, "An error occurred while uploading data." },
                    { (short)1, (short)496, "No incomings to upload." },
                    { (short)1, (short)495, "Options" },
                    { (short)1, (short)494, "Name" },
                    { (short)1, (short)493, "Nobles" },
                    { (short)1, (short)491, "hours" },
                    { (short)1, (short)489, "Offense" },
                    { (short)1, (short)488, "None" },
                    { (short)1, (short)487, "You cannot change admin status of a user that you have not created." },
                    { (short)1, (short)486, "You cannot change admin status of your own key." },
                    { (short)1, (short)485, "You cannot delete an admin user that you have not created." },
                    { (short)1, (short)484, "You cannot delete your own key." },
                    { (short)1, (short)483, "No user exists with that Vault key." },
                    { (short)1, (short)482, "Invalid Vault key." },
                    { (short)1, (short)481, "This user already has a Vault key." },
                    { (short)1, (short)480, "Cannot request a key for a player that's not in your tribe." },
                    { (short)1, (short)479, "Either the player ID or player name must be specified." },
                    { (short)1, (short)478, "No user could be found with the given name." },
                    { (short)1, (short)477, "No player could be found with the given player ID." },
                    { (short)1, (short)476, "Deleted key for {playerName}" },
                    { (short)1, (short)505, "Distance between the source and target village" },
                    { (short)1, (short)475, "Updated {playerName} (unknown change)" },
                    { (short)1, (short)506, "Preview" },
                    { (short)1, (short)510, "Alerts" },
                    { (short)1, (short)536, "No army data available." },
                    { (short)1, (short)535, "Since you don't own this translation, you'll be editing a copy instead." },
                    { (short)1, (short)534, "You already have a translation with the name \"{name}\"!" },
                    { (short)1, (short)532, "Successfully deleted the translation." },
                    { (short)1, (short)531, "You haven't saved your translation yet!" },
                    { (short)1, (short)530, "You cannot delete a translation that is being used as a default!" },
                    { (short)1, (short)529, "Are you sure you want to delete your translation, \"{name}\"?" },
                    { (short)1, (short)528, "Delete Translation" },
                    { (short)1, (short)527, "Saved changes!" },
                    { (short)1, (short)526, "Note: This is in-game text, it must match EXACTLY!" },
                    { (short)1, (short)525, "The translation for {keyName} is missing: {parameters}" },
                    { (short)1, (short)524, "English Sample" },
                    { (short)1, (short)523, "Value" },
                    { (short)1, (short)522, "Key" },
                    { (short)1, (short)521, "{name} by {author}" },
                    { (short)1, (short)520, "Save Changes" },
                    { (short)1, (short)519, "New Translation" },
                    { (short)1, (short)518, "Edit Translation" },
                    { (short)1, (short)517, "Save Settings" },
                    { (short)1, (short)516, "Translation:" },
                    { (short)1, (short)515, "Language:" },
                    { (short)1, (short)514, "Translations" },
                    { (short)1, (short)513, "Help" },
                    { (short)1, (short)512, "url" },
                    { (short)1, (short)511, "unit" },
                    { (short)1, (short)507, "Vault - commands from here" },
                    { (short)1, (short)410, "light cav, light cavalry, light, lc" },
                    { (short)1, (short)474, "Changed administrator of {playerName} from {oldAdmin} to {newAdmin}" },
                    { (short)1, (short)472, "Cleared administrator of {playerName}" },
                    { (short)1, (short)436, "The {buildingName} has" },
                    { (short)1, (short)435, "from {oldLoyalty} to {newLoyalty}" },
                    { (short)1, (short)434, "Church" },
                    { (short)1, (short)433, "Watchtower" },
                    { (short)1, (short)432, "Wall" },
                    { (short)1, (short)431, "Hiding place" },
                    { (short)1, (short)430, "Warehouse" },
                    { (short)1, (short)429, "Farm" },
                    { (short)1, (short)428, "Iron mine" },
                    { (short)1, (short)427, "Clay pit" },
                    { (short)1, (short)426, "Timber camp" },
                    { (short)1, (short)425, "Market" },
                    { (short)1, (short)424, "Statue" },
                    { (short)1, (short)423, "Rally point" },
                    { (short)1, (short)422, "Smithy" },
                    { (short)1, (short)421, "Academy" },
                    { (short)1, (short)420, "Workshop" },
                    { (short)1, (short)419, "Stable" },
                    { (short)1, (short)418, "Barracks" },
                    { (short)1, (short)417, "Headquarters" },
                    { (short)1, (short)416, "paladin, pally" },
                    { (short)1, (short)415, "Paladin" },
                    { (short)1, (short)414, "heavy cav, heavy cavalry, hc, heavy" },
                    { (short)1, (short)413, "Heavy Cav." },
                    { (short)1, (short)412, "mounted ar, mounted archer, marcher, ma" },
                    { (short)1, (short)437, "to level {newLevel}" },
                    { (short)1, (short)473, "Gave admin privileges to {playerName}" },
                    { (short)1, (short)438, "table" },
                    { (short)1, (short)440, "Your current village group isn't \"All\", please change to group \"All\"." },
                    { (short)1, (short)471, "Changed server assigned for {playerName}" },
                    { (short)1, (short)470, "Key for {playerName} no longer read-only" },
                    { (short)1, (short)469, "Set key for {playerName} as read-only" },
                    { (short)1, (short)468, "Changed key owner from {oldPlayer} to {newPlayer}" },
                    { (short)1, (short)467, "Disabled key for {playerName}" },
                    { (short)1, (short)466, "Re-enabled key for {playerName}" },
                    { (short)1, (short)465, "Gave admin privileges to {playerName}" },
                    { (short)1, (short)464, "Revoked admin privileges for {playerName}" },
                    { (short)1, (short)463, "Added key for {playerName}" },
                    { (short)1, (short)462, "{numDone}/{numTotal} done, {numFailed} failed" },
                    { (short)1, (short)461, "secs" },
                    { (short)1, (short)460, "mins" },
                    { (short)1, (short)459, "hrs" },
                    { (short)1, (short)458, "days" },
                    { (short)1, (short)457, "sec" },
                    { (short)1, (short)456, "min" },
                    { (short)1, (short)455, "hr" },
                    { (short)1, (short)454, "day" },
                    { (short)1, (short)453, "{hour}:{minute}:{second} on {day}:{month}:{year}" },
                    { (short)1, (short)452, "on {date} at {time}" },
                    { (short)1, (short)451, "{time} on {date}" },
                    { (short)1, (short)450, "tomorrow at {time}" },
                    { (short)1, (short)449, "today at {time}" },
                    { (short)1, (short)448, "jan, feb, mar, apr, may, jun, jul, aug, sep, oct, nov, dec" },
                    { (short)1, (short)441, "You have filters set for your {dataType}, please remove them before uploading." },
                    { (short)1, (short)439, "Tribal wars Captcha was triggered, please refresh the page and try again. Any uploads will continue where they left off." },
                    { (short)1, (short)264, "Optional" },
                    { (short)1, (short)244, "Add" },
                    { (short)1, (short)243, "Message" },
                    { (short)1, (short)85, "Stationed" },
                    { (short)1, (short)84, "At home" },
                    { (short)1, (short)83, "# Players" },
                    { (short)1, (short)82, "# DVs" },
                    { (short)1, (short)81, "# Nobles" },
                    { (short)1, (short)80, "# Nukes" },
                    { (short)1, (short)79, "# Fakes" },
                    { (short)1, (short)78, "You haven't uploaded data in a while, you can't use the map script until you do. Click the 'Show' link at the top of the page to start uploading. (Then refresh the page)" },
                    { (short)1, (short)77, "No" },
                    { (short)1, (short)76, "Any" },
                    { (short)1, (short)75, "Using Vault" },
                    { (short)1, (short)74, "Nuke?" },
                    { (short)1, (short)73, "Fake" },
                    { (short)1, (short)72, "Fakes" },
                    { (short)1, (short)71, "Unknown" },
                    { (short)1, (short)70, "Canceled:" },
                    { (short)1, (short)69, "Finished:" },
                    { (short)1, (short)68, "Tagging:" },
                    { (short)1, (short)67, "{numDone}/{numTotal} tagged ({numFailed} failed)" },
                    { (short)1, (short)66, "Tagging canceled" },
                    { (short)1, (short)65, "Either no incomings or all tags are current" },
                    { (short)1, (short)64, "You didn't select any incomings!" },
                    { (short)1, (short)63, "WARNING - Fake detection isn't 100% accurate, but you have enabled the 'label as fakes' option.\"" },
                    { (short)1, (short)62, "That's not a number!" },
                    { (short)1, (short)61, "Tagging will take a while!" },
                    { (short)1, (short)86, "Traveling" },
                    { (short)1, (short)60, "Cancel" },
                    { (short)1, (short)87, "Owned" },
                    { (short)1, (short)89, "Will take ~{nukesRequired} nukes to clear at {morale}% morale (last nuke has ~{lossPercent}% losses)" },
                    { (short)1, (short)115, "k pop" },
                    { (short)1, (short)114, "Returning troops over" },
                    { (short)1, (short)112, "Wall under level" },
                    { (short)1, (short)111, "Stacks" },
                    { (short)1, (short)110, "Nobles" },
                    { (short)1, (short)109, "Nukes required" },
                    { (short)1, (short)108, "Has intel" },
                    { (short)1, (short)107, "Has group" },
                    { (short)1, (short)106, "None" },
                    { (short)1, (short)105, "Highlights" },
                    { (short)1, (short)104, "days old" },
                    { (short)1, (short)103, "Ignore intel over" },
                    { (short)1, (short)102, "Show overlay" },
                    { (short)1, (short)101, "Overlay Settings" },
                    { (short)1, (short)100, "Loyalty" },
                    { (short)1, (short)99, "Buildings" },
                    { (short)1, (short)98, "Possible recruits" },
                    { (short)1, (short)97, "Commands" },
                    { (short)1, (short)96, "Hover Settings" },
                    { (short)1, (short)95, "Possible loyalty" },
                    { (short)1, (short)94, "Latest loyalty" },
                    { (short)1, (short)93, "Loyalty" },
                    { (short)1, (short)92, "Possible levels" },
                    { (short)1, (short)91, "Latest levels" },
                    { (short)1, (short)90, "Seen at" },
                    { (short)1, (short)88, "Possibly recruited" },
                    { (short)1, (short)116, "Watchtower over level" },
                    { (short)1, (short)59, "Revert to Old Tags" },
                    { (short)1, (short)57, "Tag All" },
                    { (short)1, (short)27, "Done" },
                    { (short)1, (short)26, "This is the Main Vault Interface. Make sure to upload your reports, etc. in the Upload tab. Run this script on your Map or on your Incomings to see everything the Vault has to offer." },
                    { (short)1, (short)25, "Vault" },
                    { (short)1, (short)24, "Troop Req." },
                    { (short)1, (short)23, "Landing Time" },
                    { (short)1, (short)22, "Launch Time" },
                    { (short)1, (short)21, "Source Village" },
                    { (short)1, (short)20, "Troops" },
                    { (short)1, (short)19, "No data is available" },
                    { (short)1, (short)18, "No commands available" },
                    { (short)1, (short)17, "An error occurred..." },
                    { (short)1, (short)16, "You need to upload:" },
                    { (short)1, (short)15, "troops" },
                    { (short)1, (short)14, "reports" },
                    { (short)1, (short)13, "incomings" },
                    { (short)1, (short)12, "commands" },
                    { (short)1, (short)11, "You haven't uploaded data in a while, you can't use the backtiming script until you do. Upload your data then refresh the page and run this script again." },
                    { (short)1, (short)10, "Make BB-code for back-timing" },
                    { (short)1, (short)9, "The vault was recently updated, you will need to re-upload some data." },
                    { (short)1, (short)8, "The script will not be ran." },
                    { (short)1, (short)7, "Thank you, please run the script again to start using it." },
                    { (short)1, (short)6, @"This is your first time running the script - please see the terms and conditions on DATA COLLECTION below.


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


                Agree to these terms?" },
                    { (short)1, (short)5, "This script cannot be used without a premium account!" },
                    { (short)1, (short)4, "Arrives in" },
                    { (short)1, (short)3, "Arrival time" },
                    { (short)1, (short)28, "Current uploads will continue running while this popup is closed." },
                    { (short)1, (short)58, "Tag Selected" },
                    { (short)1, (short)29, "{numIncs} incomings weren't uploaded to Vault yet and won't be tagged!" },
                    { (short)1, (short)31, "Note - this feature is EXPERIMENTAL and may not be accurate!" },
                    { (short)1, (short)56, "Ignore incomings without data" },
                    { (short)1, (short)55, "thousand offense population" },
                    { (short)1, (short)54, "Label as \"Fakes\" if less than" },
                    { (short)1, (short)53, "Only tag unlabeled incomings" },
                    { (short)1, (short)52, "Reset" },
                    { (short)1, (short)51, "Tag format:" },
                    { (short)1, (short)50, "Custom labels you've added to the command that should be left untouched; these are surrounded by quotes ie \"Dodged\"" },
                    { (short)1, (short)49, "Whether the village is Offensive, Defensive, or unknown" },
                    { (short)1, (short)48, "Coords of the village being attacked" },
                    { (short)1, (short)47, "Coords of the village that sent the attack" },
                    { (short)1, (short)46, "Name of the village that sent the attack" },
                    { (short)1, (short)45, "Name of the player that sent the attack" },
                    { (short)1, (short)44, "Name of the village that sent the attack" },
                    { (short)1, (short)43, "Name of the player that sent the attack" },
                    { (short)1, (short)42, "# of total commands from the village to the tribe" },
                    { (short)1, (short)41, "# of catapults known at the village" },
                    { (short)1, (short)40, "Offensive pop known returning to the village when this command was sent, ie 19.2k or ?k" },
                    { (short)1, (short)39, "% of a full nuke known returning to the village when this command was sent, ie 89% or ?%" },
                    { (short)1, (short)38, "Offensive pop known at the village, ie 19.2k or ?k" },
                    { (short)1, (short)37, "% of a full nuke known at the village, ie 89% or ?%" },
                    { (short)1, (short)36, "One of: Fake, Nuke" },
                    { (short)1, (short)35, "Best known troop type (from your label or auto-calculated)" },
                    { (short)1, (short)34, "Details" },
                    { (short)1, (short)33, "Code" },
                    { (short)1, (short)32, "Upload Visible Incomings" },
                    { (short)1, (short)30, "You haven't uploaded data in a while, you can't use tagging until you do." },
                    { (short)1, (short)117, "DV" },
                    { (short)1, (short)118, "A small stack is" },
                    { (short)1, (short)119, "and a big stack is" },
                    { (short)1, (short)204, @"{playerName} will be given admin privileges, and will be able to:
                - Access all troop information available
                - Add new users
                - Give and revoke admin privileges for users" },
                    { (short)1, (short)203, "{playerName} will no longer have admin privileges." },
                    { (short)1, (short)202, "{playerName} will have their Vault key removed." },
                    { (short)1, (short)201, "Get script" },
                    { (short)1, (short)200, "Revoke admin" },
                    { (short)1, (short)199, "Make admin" },
                    { (short)1, (short)198, "No tribe" },
                    { (short)1, (short)197, "Enter the username or ID" },
                    { (short)1, (short)196, "Time" },
                    { (short)1, (short)195, "Event" },
                    { (short)1, (short)194, "Admin" },
                    { (short)1, (short)193, "User Log" },
                    { (short)1, (short)192, "Log" },
                    { (short)1, (short)191, "Download" },
                    { (short)1, (short)190, "Include stats for 1/4, 1/2, and 3/4 nukes" },
                    { (short)1, (short)189, "Get tribe army stats as a spreadsheet:" },
                    { (short)1, (short)188, "Working" },
                    { (short)1, (short)187, "Tribe Stats" },
                    { (short)1, (short)186, "Current tribe" },
                    { (short)1, (short)185, "User name" },
                    { (short)1, (short)184, "New Vault Script" },
                    { (short)1, (short)183, "Make new key" },
                    { (short)1, (short)182, "Keys" },
                    { (short)1, (short)181, "Manage Users" },
                    { (short)1, (short)180, "Add Enemy Tribe" },
                    { (short)1, (short)205, "Vault script for: {playerName}" },
                    { (short)1, (short)179, "Tell the Vault which tribes to consider as \"enemies\" when determining which villages are back-line." },
                    { (short)1, (short)206, "1/4 Nukes" },
                    { (short)1, (short)208, "3/4 Nukes" },
                    { (short)1, (short)241, "Server Time" },
                    { (short)1, (short)234, "Notifications" },
                    { (short)1, (short)231, "DVs to {tribeName}" },
                    { (short)1, (short)230, "# Attacks" },
                    { (short)1, (short)229, "# Incs" },
                    { (short)1, (short)228, "Est. Def. Villas" },
                    { (short)1, (short)227, "Est. Off. Villas" },
                    { (short)1, (short)226, "DVs Supporting Others" },
                    { (short)1, (short)225, "DVs Supporting Self" },
                    { (short)1, (short)224, "DVs Traveling" },
                    { (short)1, (short)223, "Backline DVs at Home" },
                    { (short)1, (short)222, "DVs at Home" },
                    { (short)1, (short)221, "Owned DVs" },
                    { (short)1, (short)220, "Possible nobles" },
                    { (short)1, (short)219, "Nukes Traveling" },
                    { (short)1, (short)218, "Full Nukes" },
                    { (short)1, (short)217, "Player" },
                    { (short)1, (short)216, "Yes" },
                    { (short)1, (short)215, "Needs upload?" },
                    { (short)1, (short)214, "Total Attacks" },
                    { (short)1, (short)213, "Total Incs" },
                    { (short)1, (short)212, "Total DVs" },
                    { (short)1, (short)211, "Total Possible Nobles" },
                    { (short)1, (short)210, "Total Nobles" },
                    { (short)1, (short)209, "Total Full Nukes" },
                    { (short)1, (short)207, "1/2 Nukes" },
                    { (short)1, (short)178, "Enemy Tribes" },
                    { (short)1, (short)177, "That tribe is already registered as an enemy." },
                    { (short)1, (short)176, "No tribe exists with that tag or name." },
                    { (short)1, (short)145, "Noble Targets" },
                    { (short)1, (short)144, "Current Stack Strength" },
                    { (short)1, (short)143, "Possible Nukes" },
                    { (short)1, (short)142, "A list of villages to stack, based on their incomings and current defense stationed there." },
                    { (short)1, (short)141, "Can eat {numNukes} nukes" },
                    { (short)1, (short)140, "(No stacks to suggest)" },
                    { (short)1, (short)139, "Send Stacks" },
                    { (short)1, (short)137, "Lands At" },
                    { (short)1, (short)136, "# Nobles" },
                    { (short)1, (short)135, "A list of incoming trains that you have troops to snipe for." },
                    { (short)1, (short)134, "(No snipes needed)" },
                    { (short)1, (short)133, "Snipes Needed" },
                    { (short)1, (short)132, "New Owner" },
                    { (short)1, (short)131, "Old Owner" },
                    { (short)1, (short)130, "Capped At" },
                    { (short)1, (short)129, "Loyalty" },
                    { (short)1, (short)128, "Village" },
                    { (short)1, (short)127, "Only show recaps with nobles nearby" },
                    { (short)1, (short)126, "A list of friendly villages that were recently conquered." },
                    { (short)1, (short)125, "{duration} ago" },
                    { (short)1, (short)124, "(No recaps)" },
                    { (short)1, (short)123, "Actions/Alerts" },
                    { (short)1, (short)122, "Send Recap" },
                    { (short)1, (short)121, "tribe" },
                    { (short)1, (short)120, "Highlight villages in" },
                    { (short)1, (short)146, "(No suggested targets)" },
                    { (short)1, (short)147, "{duration} ago" },
                    { (short)1, (short)148, "A list of potential nobling targets, based on their stationed defense and current loyalty." },
                    { (short)1, (short)149, "Stationed DVs" },
                    { (short)1, (short)175, "Enter the name or tag of the tribe." },
                    { (short)1, (short)174, "An error occurred while listing enemy tribes..." },
                    { (short)1, (short)173, "Enemy Tribes" },
                    { (short)1, (short)172, "{tribeName} will no longer be considered an enemy." },
                    { (short)1, (short)171, "Delete" },
                    { (short)1, (short)170, "Admin" },
                    { (short)1, (short)169, "hours (optional)" },
                    { (short)1, (short)168, "that can reach within" },
                    { (short)1, (short)167, "Send to" },
                    { (short)1, (short)166, "Find and mail players with backline defense." },
                    { (short)1, (short)165, "Request Stack" },
                    { (short)1, (short)164, "Results" },
                    { (short)1, (short)537, "No building data available." },
                    { (short)1, (short)163, "Search" },
                    { (short)1, (short)161, "that can reach within" },
                    { (short)1, (short)160, "Send to" },
                    { (short)1, (short)159, "Find and mail players with nearby defense." },
                    { (short)1, (short)158, "No players found!" },
                    { (short)1, (short)157, "Quick Support" },
                    { (short)1, (short)156, "Pop. Count" },
                    { (short)1, (short)155, "Tribe" },
                    { (short)1, (short)154, "A list of villages that should have their support sent home, whether they are backline villages or non-friendly villages." },
                    { (short)1, (short)153, "(No useless stacks)" },
                    { (short)1, (short)152, "Useless Stacks" },
                    { (short)1, (short)151, "Owner" },
                    { (short)1, (short)150, "DVs Seen At" },
                    { (short)1, (short)162, "hours" },
                    { (short)1, (short)538, "{day}/{month}/{year}" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_translation_key",
                schema: "feature",
                table: "translation",
                column: "key")
                .Annotation("Npgsql:IndexMethod", "hash");

            migrationBuilder.CreateIndex(
                name: "IX_translation_translation_id",
                schema: "feature",
                table: "translation",
                column: "translation_id")
                .Annotation("Npgsql:IndexMethod", "hash");

            migrationBuilder.CreateIndex(
                name: "IX_translation_parameter_key_id",
                schema: "feature",
                table: "translation_parameter",
                column: "key_id");

            migrationBuilder.CreateIndex(
                name: "IX_translation_registry_language_id",
                schema: "feature",
                table: "translation_registry",
                column: "language_id");

            migrationBuilder.CreateIndex(
                name: "IX_conflicting_data_record_conflicting_tx_id",
                schema: "security",
                table: "conflicting_data_record",
                column: "conflicting_tx_id");

            migrationBuilder.CreateIndex(
                name: "IX_conflicting_data_record_old_tx_id",
                schema: "security",
                table: "conflicting_data_record",
                column: "old_tx_id");

            migrationBuilder.CreateIndex(
                name: "IX_invalid_data_record_user_id",
                schema: "security",
                table: "invalid_data_record",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_tx_id",
                schema: "security",
                table: "transaction",
                column: "tx_id");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_world_id",
                schema: "security",
                table: "transaction",
                column: "world_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_access_group_id",
                schema: "security",
                table: "user",
                column: "access_group_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_auth_token",
                schema: "security",
                table: "user",
                column: "auth_token");

            migrationBuilder.CreateIndex(
                name: "IX_user_enabled",
                schema: "security",
                table: "user",
                column: "enabled");

            migrationBuilder.CreateIndex(
                name: "IX_user_player_id",
                schema: "security",
                table: "user",
                column: "player_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_tx_id",
                schema: "security",
                table: "user",
                column: "tx_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_uid",
                schema: "security",
                table: "user",
                column: "uid");

            migrationBuilder.CreateIndex(
                name: "IX_user_world_id",
                schema: "security",
                table: "user",
                column: "world_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_log_tx_id",
                schema: "security",
                table: "user_log",
                column: "tx_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_log_world_id",
                schema: "security",
                table: "user_log",
                column: "world_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_upload_history_uid",
                schema: "security",
                table: "user_upload_history",
                column: "uid");

            migrationBuilder.CreateIndex(
                name: "IX_command_army_id",
                schema: "tw",
                table: "command",
                column: "army_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_command_first_seen_at",
                schema: "tw",
                table: "command",
                column: "first_seen_at");

            migrationBuilder.CreateIndex(
                name: "IX_command_lands_at",
                schema: "tw",
                table: "command",
                column: "lands_at");

            migrationBuilder.CreateIndex(
                name: "IX_command_returns_at",
                schema: "tw",
                table: "command",
                column: "returns_at");

            migrationBuilder.CreateIndex(
                name: "IX_command_source_player_id",
                schema: "tw",
                table: "command",
                column: "source_player_id");

            migrationBuilder.CreateIndex(
                name: "IX_command_source_village_id",
                schema: "tw",
                table: "command",
                column: "source_village_id");

            migrationBuilder.CreateIndex(
                name: "IX_command_target_player_id",
                schema: "tw",
                table: "command",
                column: "target_player_id");

            migrationBuilder.CreateIndex(
                name: "IX_command_target_village_id",
                schema: "tw",
                table: "command",
                column: "target_village_id");

            migrationBuilder.CreateIndex(
                name: "IX_command_tx_id",
                schema: "tw",
                table: "command",
                column: "tx_id");

            migrationBuilder.CreateIndex(
                name: "IX_command_world_id_access_group_id",
                schema: "tw",
                table: "command",
                columns: new[] { "world_id", "access_group_id" });

            migrationBuilder.CreateIndex(
                name: "IX_command_world_id_army_id",
                schema: "tw",
                table: "command",
                columns: new[] { "world_id", "army_id" });

            migrationBuilder.CreateIndex(
                name: "IX_command_world_id_source_player_id",
                schema: "tw",
                table: "command",
                columns: new[] { "world_id", "source_player_id" });

            migrationBuilder.CreateIndex(
                name: "IX_command_world_id_source_village_id",
                schema: "tw",
                table: "command",
                columns: new[] { "world_id", "source_village_id" });

            migrationBuilder.CreateIndex(
                name: "IX_command_world_id_target_player_id",
                schema: "tw",
                table: "command",
                columns: new[] { "world_id", "target_player_id" });

            migrationBuilder.CreateIndex(
                name: "IX_command_world_id_target_village_id",
                schema: "tw",
                table: "command",
                columns: new[] { "world_id", "target_village_id" });

            migrationBuilder.CreateIndex(
                name: "IX_command_world_id_command_id_access_group_id",
                schema: "tw",
                table: "command",
                columns: new[] { "world_id", "command_id", "access_group_id" });

            migrationBuilder.CreateIndex(
                name: "IX_command_army_snob",
                schema: "tw",
                table: "command_army",
                column: "snob");

            migrationBuilder.CreateIndex(
                name: "IX_command_army_world_id_army_id",
                schema: "tw",
                table: "command_army",
                columns: new[] { "world_id", "army_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_current_army_army_id",
                schema: "tw",
                table: "current_army",
                column: "army_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_current_army_snob",
                schema: "tw",
                table: "current_army",
                column: "snob");

            migrationBuilder.CreateIndex(
                name: "IX_current_army_world_id",
                schema: "tw",
                table: "current_army",
                column: "world_id");

            migrationBuilder.CreateIndex(
                name: "IX_current_building_village_id",
                schema: "tw",
                table: "current_building",
                column: "village_id");

            migrationBuilder.CreateIndex(
                name: "IX_current_building_world_id_access_group_id",
                schema: "tw",
                table: "current_building",
                columns: new[] { "world_id", "access_group_id" });

            migrationBuilder.CreateIndex(
                name: "IX_current_player_world_id_player_id_access_group_id",
                schema: "tw",
                table: "current_player",
                columns: new[] { "world_id", "player_id", "access_group_id" });

            migrationBuilder.CreateIndex(
                name: "IX_current_village_world_id_army_at_home_id",
                schema: "tw",
                table: "current_village",
                columns: new[] { "world_id", "army_at_home_id" });

            migrationBuilder.CreateIndex(
                name: "IX_current_village_world_id_army_owned_id",
                schema: "tw",
                table: "current_village",
                columns: new[] { "world_id", "army_owned_id" });

            migrationBuilder.CreateIndex(
                name: "IX_current_village_world_id_army_recent_losses_id",
                schema: "tw",
                table: "current_village",
                columns: new[] { "world_id", "army_recent_losses_id" });

            migrationBuilder.CreateIndex(
                name: "IX_current_village_world_id_army_stationed_id",
                schema: "tw",
                table: "current_village",
                columns: new[] { "world_id", "army_stationed_id" });

            migrationBuilder.CreateIndex(
                name: "IX_current_village_world_id_army_supporting_id",
                schema: "tw",
                table: "current_village",
                columns: new[] { "world_id", "army_supporting_id" });

            migrationBuilder.CreateIndex(
                name: "IX_current_village_world_id_army_traveling_id",
                schema: "tw",
                table: "current_village",
                columns: new[] { "world_id", "army_traveling_id" });

            migrationBuilder.CreateIndex(
                name: "IX_current_village_world_id_village_id_access_group_id",
                schema: "tw",
                table: "current_village",
                columns: new[] { "world_id", "village_id", "access_group_id" });

            migrationBuilder.CreateIndex(
                name: "IX_current_village_support_tx_id",
                schema: "tw",
                table: "current_village_support",
                column: "tx_id");

            migrationBuilder.CreateIndex(
                name: "IX_current_village_support_world_id_supporting_army_id",
                schema: "tw",
                table: "current_village_support",
                columns: new[] { "world_id", "supporting_army_id" });

            migrationBuilder.CreateIndex(
                name: "IX_current_village_support_world_id_source_village_id_access_g~",
                schema: "tw",
                table: "current_village_support",
                columns: new[] { "world_id", "source_village_id", "access_group_id" });

            migrationBuilder.CreateIndex(
                name: "IX_current_village_support_world_id_target_village_id_access_g~",
                schema: "tw",
                table: "current_village_support",
                columns: new[] { "world_id", "target_village_id", "access_group_id" });

            migrationBuilder.CreateIndex(
                name: "IX_enemy_tribe_tx_id",
                schema: "tw",
                table: "enemy_tribe",
                column: "tx_id");

            migrationBuilder.CreateIndex(
                name: "IX_ignored_report_report_id_world_id_access_group_id",
                schema: "tw",
                table: "ignored_report",
                columns: new[] { "report_id", "world_id", "access_group_id" });

            migrationBuilder.CreateIndex(
                name: "IX_report_access_group_id",
                schema: "tw",
                table: "report",
                column: "access_group_id");

            migrationBuilder.CreateIndex(
                name: "IX_report_attacker_player_id",
                schema: "tw",
                table: "report",
                column: "attacker_player_id");

            migrationBuilder.CreateIndex(
                name: "IX_report_building_id",
                schema: "tw",
                table: "report",
                column: "building_id");

            migrationBuilder.CreateIndex(
                name: "IX_report_defender_player_id",
                schema: "tw",
                table: "report",
                column: "defender_player_id");

            migrationBuilder.CreateIndex(
                name: "IX_report_occured_at",
                schema: "tw",
                table: "report",
                column: "occured_at");

            migrationBuilder.CreateIndex(
                name: "IX_report_tx_id",
                schema: "tw",
                table: "report",
                column: "tx_id");

            migrationBuilder.CreateIndex(
                name: "IX_report_world_id_attacker_army_id",
                schema: "tw",
                table: "report",
                columns: new[] { "world_id", "attacker_army_id" });

            migrationBuilder.CreateIndex(
                name: "IX_report_world_id_attacker_losses_army_id",
                schema: "tw",
                table: "report",
                columns: new[] { "world_id", "attacker_losses_army_id" });

            migrationBuilder.CreateIndex(
                name: "IX_report_world_id_attacker_player_id",
                schema: "tw",
                table: "report",
                columns: new[] { "world_id", "attacker_player_id" });

            migrationBuilder.CreateIndex(
                name: "IX_report_world_id_attacker_village_id",
                schema: "tw",
                table: "report",
                columns: new[] { "world_id", "attacker_village_id" });

            migrationBuilder.CreateIndex(
                name: "IX_report_world_id_building_id",
                schema: "tw",
                table: "report",
                columns: new[] { "world_id", "building_id" });

            migrationBuilder.CreateIndex(
                name: "IX_report_world_id_defender_army_id",
                schema: "tw",
                table: "report",
                columns: new[] { "world_id", "defender_army_id" });

            migrationBuilder.CreateIndex(
                name: "IX_report_world_id_defender_losses_army_id",
                schema: "tw",
                table: "report",
                columns: new[] { "world_id", "defender_losses_army_id" });

            migrationBuilder.CreateIndex(
                name: "IX_report_world_id_defender_player_id",
                schema: "tw",
                table: "report",
                columns: new[] { "world_id", "defender_player_id" });

            migrationBuilder.CreateIndex(
                name: "IX_report_world_id_defender_traveling_army_id",
                schema: "tw",
                table: "report",
                columns: new[] { "world_id", "defender_traveling_army_id" });

            migrationBuilder.CreateIndex(
                name: "IX_report_world_id_defender_village_id",
                schema: "tw",
                table: "report",
                columns: new[] { "world_id", "defender_village_id" });

            migrationBuilder.CreateIndex(
                name: "IX_report_army_army_id",
                schema: "tw",
                table: "report_army",
                column: "army_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_report_building_report_building_id",
                schema: "tw",
                table: "report_building",
                column: "report_building_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ally_world_id_tribe_id",
                schema: "tw_provided",
                table: "ally",
                columns: new[] { "world_id", "tribe_id" });

            migrationBuilder.CreateIndex(
                name: "IX_conquer_world_id_village_id",
                schema: "tw_provided",
                table: "conquer",
                columns: new[] { "world_id", "village_id" });

            migrationBuilder.CreateIndex(
                name: "IX_player_player_id",
                schema: "tw_provided",
                table: "player",
                column: "player_id");

            migrationBuilder.CreateIndex(
                name: "IX_player_tribe_id",
                schema: "tw_provided",
                table: "player",
                column: "tribe_id");

            migrationBuilder.CreateIndex(
                name: "IX_village_player_id",
                schema: "tw_provided",
                table: "village",
                column: "player_id");

            migrationBuilder.CreateIndex(
                name: "IX_village_village_id",
                schema: "tw_provided",
                table: "village",
                column: "village_id");

            migrationBuilder.CreateIndex(
                name: "IX_village_world_id_player_id",
                schema: "tw_provided",
                table: "village",
                columns: new[] { "world_id", "player_id" });

            migrationBuilder.CreateIndex(
                name: "IX_village_world_id_village_id",
                schema: "tw_provided",
                table: "village",
                columns: new[] { "world_id", "village_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_world_default_translation_id",
                schema: "tw_provided",
                table: "world",
                column: "default_translation_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "translation",
                schema: "feature");

            migrationBuilder.DropTable(
                name: "translation_parameter",
                schema: "feature");

            migrationBuilder.DropTable(
                name: "access_group",
                schema: "security");

            migrationBuilder.DropTable(
                name: "conflicting_data_record",
                schema: "security");

            migrationBuilder.DropTable(
                name: "failed_authorization_record",
                schema: "security");

            migrationBuilder.DropTable(
                name: "invalid_data_record",
                schema: "security");

            migrationBuilder.DropTable(
                name: "user_log",
                schema: "security");

            migrationBuilder.DropTable(
                name: "user_upload_history",
                schema: "security");

            migrationBuilder.DropTable(
                name: "command",
                schema: "tw");

            migrationBuilder.DropTable(
                name: "current_building",
                schema: "tw");

            migrationBuilder.DropTable(
                name: "current_player",
                schema: "tw");

            migrationBuilder.DropTable(
                name: "current_village_support",
                schema: "tw");

            migrationBuilder.DropTable(
                name: "enemy_tribe",
                schema: "tw");

            migrationBuilder.DropTable(
                name: "ignored_report",
                schema: "tw");

            migrationBuilder.DropTable(
                name: "performance_record",
                schema: "tw");

            migrationBuilder.DropTable(
                name: "report",
                schema: "tw");

            migrationBuilder.DropTable(
                name: "ally",
                schema: "tw_provided");

            migrationBuilder.DropTable(
                name: "conquer",
                schema: "tw_provided");

            migrationBuilder.DropTable(
                name: "world_settings",
                schema: "tw_provided");

            migrationBuilder.DropTable(
                name: "translation_key",
                schema: "feature");

            migrationBuilder.DropTable(
                name: "user",
                schema: "security");

            migrationBuilder.DropTable(
                name: "command_army",
                schema: "tw");

            migrationBuilder.DropTable(
                name: "current_village",
                schema: "tw");

            migrationBuilder.DropTable(
                name: "report_army",
                schema: "tw");

            migrationBuilder.DropTable(
                name: "report_building",
                schema: "tw");

            migrationBuilder.DropTable(
                name: "transaction",
                schema: "security");

            migrationBuilder.DropTable(
                name: "current_army",
                schema: "tw");

            migrationBuilder.DropTable(
                name: "village",
                schema: "tw_provided");

            migrationBuilder.DropTable(
                name: "player",
                schema: "tw_provided");

            migrationBuilder.DropTable(
                name: "world",
                schema: "tw_provided");

            migrationBuilder.DropTable(
                name: "translation_registry",
                schema: "feature");

            migrationBuilder.DropTable(
                name: "translation_language",
                schema: "feature");

            migrationBuilder.DropSequence(
                name: "translation_key_id_seq",
                schema: "feature");

            migrationBuilder.DropSequence(
                name: "translation_language_id_seq",
                schema: "feature");

            migrationBuilder.DropSequence(
                name: "translation_parameter_id_seq",
                schema: "feature");

            migrationBuilder.DropSequence(
                name: "translation_registry_id_seq",
                schema: "feature");

            migrationBuilder.DropSequence(
                name: "access_group_id_seq",
                schema: "security");

            migrationBuilder.DropSequence(
                name: "conflicting_data_record_id_seq",
                schema: "security");

            migrationBuilder.DropSequence(
                name: "invalid_data_record_id_seq",
                schema: "security");

            migrationBuilder.DropSequence(
                name: "user_log_id_seq",
                schema: "security");

            migrationBuilder.DropSequence(
                name: "user_upload_history_id_seq",
                schema: "security");

            migrationBuilder.DropSequence(
                name: "command_army_id_seq",
                schema: "tw");

            migrationBuilder.DropSequence(
                name: "current_village_support_id_seq",
                schema: "tw");

            migrationBuilder.DropSequence(
                name: "enemy_tribe_id_seq",
                schema: "tw");

            migrationBuilder.DropSequence(
                name: "failed_auth_id_seq",
                schema: "tw");

            migrationBuilder.DropSequence(
                name: "performance_record_id_seq",
                schema: "tw");

            migrationBuilder.DropSequence(
                name: "report_armies_army_id_seq",
                schema: "tw");

            migrationBuilder.DropSequence(
                name: "report_building_report_building_id_seq",
                schema: "tw");

            migrationBuilder.DropSequence(
                name: "tx_id_seq",
                schema: "tw");

            migrationBuilder.DropSequence(
                name: "users_uid_seq",
                schema: "tw");

            migrationBuilder.DropSequence(
                name: "conquers_vault_id_seq",
                schema: "tw_provided");

            migrationBuilder.DropSequence(
                name: "world_id_seq",
                schema: "tw_provided");
        }
    }
}
