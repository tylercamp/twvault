using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace TW.Vault.Scaffold
{
    public partial class VaultContext : DbContext
    {
        public VaultContext()
        {
        }

        public VaultContext(DbContextOptions<VaultContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql(
                    "Server=localhost; Database=vault; User Id=u_vault; Password=password",
                    x => x.MigrationsAssembly("TW.Vault.Migration")
                );
            }
        }

        public virtual DbSet<AccessGroup> AccessGroup { get; set; }
        public virtual DbSet<Ally> Ally { get; set; }
        public virtual DbSet<Command> Command { get; set; }
        public virtual DbSet<CommandArmy> CommandArmy { get; set; }
        public virtual DbSet<ConflictingDataRecord> ConflictingDataRecord { get; set; }
        public virtual DbSet<Conquer> Conquer { get; set; }
        public virtual DbSet<CurrentArmy> CurrentArmy { get; set; }
        public virtual DbSet<CurrentBuilding> CurrentBuilding { get; set; }
        public virtual DbSet<CurrentPlayer> CurrentPlayer { get; set; }
        public virtual DbSet<CurrentVillage> CurrentVillage { get; set; }
        public virtual DbSet<CurrentVillageSupport> CurrentVillageSupport { get; set; }
        public virtual DbSet<EnemyTribe> EnemyTribe { get; set; }
        public virtual DbSet<FailedAuthorizationRecord> FailedAuthorizationRecord { get; set; }
        public virtual DbSet<IgnoredReport> IgnoredReport { get; set; }
        public virtual DbSet<InvalidDataRecord> InvalidDataRecord { get; set; }
        public virtual DbSet<PerformanceRecord> PerformanceRecord { get; set; }
        public virtual DbSet<Player> Player { get; set; }
        public virtual DbSet<Report> Report { get; set; }
        public virtual DbSet<ReportArmy> ReportArmy { get; set; }
        public virtual DbSet<ReportBuilding> ReportBuilding { get; set; }
        public virtual DbSet<Transaction> Transaction { get; set; }
        public virtual DbSet<TranslationEntry> TranslationEntry { get; set; }
        public virtual DbSet<TranslationKey> TranslationKey { get; set; }
        public virtual DbSet<TranslationLanguage> TranslationLanguage { get; set; }
        public virtual DbSet<TranslationParameter> TranslationParameter { get; set; }
        public virtual DbSet<TranslationRegistry> TranslationRegistry { get; set; }
        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<UserLog> UserLog { get; set; }
        public virtual DbSet<UserUploadHistory> UserUploadHistory { get; set; }
        public virtual DbSet<Village> Village { get; set; }
        public virtual DbSet<World> World { get; set; }
        public virtual DbSet<WorldSettings> WorldSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AccessGroup>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.ToTable("access_group", "security");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("nextval('security.access_group_id_seq'::regclass)");

                entity.Property(e => e.Label)
                    .HasColumnName("label")
                    .HasMaxLength(256);

                entity.Property(e => e.WorldId)
                    .HasColumnName("world_id");
            });

            modelBuilder.Entity<Ally>(entity =>
            {
                entity.HasKey(e => new { e.WorldId, e.TribeId });

                entity.ToTable("ally", "tw_provided");

                entity.HasIndex(e => new { e.WorldId, e.TribeId });
                
                entity.Property(e => e.TribeId)
                    .HasColumnName("tribe_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.AllPoints).HasColumnName("all_points");

                entity.Property(e => e.Members).HasColumnName("members");

                entity.Property(e => e.Points).HasColumnName("points");

                entity.Property(e => e.Tag)
                    .HasColumnName("tag")
                    .HasColumnType("character varying");

                entity.Property(e => e.TribeName)
                    .HasColumnName("tribe_name")
                    .HasColumnType("character varying");

                entity.Property(e => e.TribeRank).HasColumnName("tribe_rank");

                entity.Property(e => e.Villages).HasColumnName("villages");

                entity.Property(e => e.WorldId).HasColumnName("world_id");

                entity.HasOne(d => d.World)
                    .WithMany(p => p.Ally)
                    .HasForeignKey(d => d.WorldId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("fk_world_id");
            });

            modelBuilder.Entity<Command>(entity =>
            {
                entity.ToTable("command", "tw");

                entity.HasKey(e => new { e.WorldId, e.AccessGroupId, e.CommandId });

                entity.HasIndex(e => e.ArmyId).IsUnique();
                entity.HasIndex(e => new { e.WorldId, e.AccessGroupId });
                entity.HasIndex(e => e.FirstSeenAt);
                entity.HasIndex(e => e.LandsAt);
                entity.HasIndex(e => e.ReturnsAt);
                entity.HasIndex(e => e.SourcePlayerId);
                entity.HasIndex(e => e.SourceVillageId);
                entity.HasIndex(e => e.TargetPlayerId);
                entity.HasIndex(e => e.TargetVillageId);
                entity.HasIndex(e => new { e.WorldId, e.CommandId, e.AccessGroupId });
                entity.HasIndex(e => e.TxId);
                
                entity.Property(e => e.CommandId)
                    .HasColumnName("command_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.ArmyId).HasColumnName("army_id");

                entity.Property(e => e.FirstSeenAt).HasColumnName("first_seen_at");

                entity.Property(e => e.IsAttack).HasColumnName("is_attack");

                entity.Property(e => e.IsReturning).HasColumnName("is_returning");

                entity.Property(e => e.LandsAt).HasColumnName("lands_at");

                entity.Property(e => e.ReturnsAt).HasColumnName("returns_at");

                entity.Property(e => e.SourcePlayerId).HasColumnName("source_player_id");

                entity.Property(e => e.SourceVillageId).HasColumnName("source_village_id");

                entity.Property(e => e.TargetPlayerId).HasColumnName("target_player_id");

                entity.Property(e => e.TargetVillageId).HasColumnName("target_village_id");

                entity.Property(e => e.TroopType)
                    .HasColumnName("troop_type")
                    .HasColumnType("character varying");

                entity.Property(e => e.TxId).HasColumnName("tx_id");

                entity.Property(e => e.UserLabel)
                    .HasColumnName("user_label")
                    .HasMaxLength(512);

                entity.Property(e => e.WorldId).HasColumnName("world_id");

                entity.Property(e => e.AccessGroupId).HasColumnName("access_group_id");

                entity.HasOne(d => d.Army)
                    .WithMany(p => p.Command)
                    .HasForeignKey(d => new { d.WorldId, d.ArmyId })
                    .HasConstraintName("fk_army");

                entity.HasOne(d => d.SourcePlayer)
                    .WithMany(p => p.CommandSourcePlayer)
                    .HasForeignKey(d => new { d.WorldId, d.SourcePlayerId })
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("fk_source_player");

                entity.HasOne(d => d.SourceVillage)
                    .WithMany(p => p.CommandSourceVillage)
                    .HasForeignKey(d => new { d.WorldId, d.SourceVillageId })
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("fk_source_village");

                entity.HasOne(d => d.TargetPlayer)
                    .WithMany(p => p.CommandTargetPlayer)
                    .HasForeignKey(d => new { d.WorldId, d.TargetPlayerId })
                    .HasConstraintName("fk_target_player");

                entity.HasOne(d => d.TargetVillage)
                    .WithMany(p => p.CommandTargetVillage)
                    .HasForeignKey(d => new { d.WorldId, d.TargetVillageId })
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("fk_target_village");

                entity.HasOne(d => d.Tx)
                    .WithMany(p => p.Command)
                    .HasForeignKey(d => d.TxId)
                    .HasConstraintName("fk_tx_id");

                entity.HasOne(d => d.World)
                    .WithMany(p => p.Command)
                    .HasForeignKey(d => d.WorldId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("fk_world_id");
            });

            modelBuilder.Entity<CommandArmy>(entity =>
            {
                entity.HasKey(e => new { e.WorldId, e.ArmyId });

                entity.ToTable("command_army", "tw");

                entity.HasIndex(e => new { e.WorldId, e.ArmyId }).IsUnique();
                entity.HasIndex(e => e.Snob);

                entity.Property(e => e.ArmyId)
                    .HasColumnName("army_id")
                    .HasDefaultValueSql("nextval('tw.command_army_id_seq'::regclass)");

                entity.Property(e => e.Archer).HasColumnName("archer");

                entity.Property(e => e.Axe).HasColumnName("axe");

                entity.Property(e => e.Catapult).HasColumnName("catapult");

                entity.Property(e => e.Heavy).HasColumnName("heavy");

                entity.Property(e => e.Knight).HasColumnName("knight");

                entity.Property(e => e.Light).HasColumnName("light");

                entity.Property(e => e.Marcher).HasColumnName("marcher");

                entity.Property(e => e.Militia).HasColumnName("militia");

                entity.Property(e => e.Ram).HasColumnName("ram");

                entity.Property(e => e.Snob).HasColumnName("snob");

                entity.Property(e => e.Spear).HasColumnName("spear");

                entity.Property(e => e.Spy).HasColumnName("spy");

                entity.Property(e => e.Sword).HasColumnName("sword");

                entity.Property(e => e.WorldId).HasColumnName("world_id");

                entity.HasOne(d => d.World)
                    .WithMany(p => p.CommandArmy)
                    .HasForeignKey(d => d.WorldId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_world_id");
            });

            modelBuilder.Entity<ConflictingDataRecord>(entity =>
            {
                entity.ToTable("conflicting_data_record", "security");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("nextval('security.conflicting_data_record_id_seq'::regclass)");

                entity.Property(e => e.ConflictingTxId).HasColumnName("conflicting_tx_id");

                entity.Property(e => e.OldTxId).HasColumnName("old_tx_id");

                entity.HasIndex(e => e.ConflictingTxId)
                    .HasName("fki_fk_conflicting_data_record_conflicting_tx_id");

                entity.HasIndex(e => e.OldTxId)
                    .HasName("fki_fk_conflicting_data_record_old_tx_id");

                entity.HasOne(d => d.ConflictingTx)
                    .WithMany(p => p.ConflictingDataRecordConflictingTx)
                    .HasForeignKey(d => d.ConflictingTxId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_conflicting_tx_id");

                entity.HasOne(d => d.OldTx)
                    .WithMany(p => p.ConflictingDataRecordOldTx)
                    .HasForeignKey(d => d.OldTxId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_old_tx_id");
            });

            modelBuilder.Entity<Conquer>(entity =>
            {
                entity.HasKey(e => e.VaultId);

                entity.ToTable("conquer", "tw_provided");

                entity.HasIndex(e => new { e.WorldId, e.VillageId });

                entity.Property(e => e.VaultId)
                    .HasColumnName("vault_id")
                    .HasDefaultValueSql("nextval('tw_provided.conquers_vault_id_seq'::regclass)");

                entity.Property(e => e.NewOwner).HasColumnName("new_owner");

                entity.Property(e => e.OldOwner).HasColumnName("old_owner");

                entity.Property(e => e.UnixTimestamp).HasColumnName("unix_timestamp");

                entity.Property(e => e.VillageId).HasColumnName("village_id");

                entity.Property(e => e.WorldId).HasColumnName("world_id");

                entity.HasOne(d => d.World)
                    .WithMany(p => p.Conquer)
                    .HasForeignKey(d => d.WorldId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_world_id");
            });

            modelBuilder.Entity<CurrentArmy>(entity =>
            {
                entity.HasKey(e => new { e.WorldId, e.ArmyId });

                entity.ToTable("current_army", "tw");

                entity.HasIndex(e => e.ArmyId).IsUnique();
                entity.HasIndex(e => e.Snob);
                entity.HasIndex(e => e.WorldId);

                entity.Property(e => e.ArmyId)
                    .HasColumnName("army_id")
                    .UseIdentityByDefaultColumn();

                entity.Property(e => e.Archer).HasColumnName("archer");

                entity.Property(e => e.Axe).HasColumnName("axe");

                entity.Property(e => e.Catapult).HasColumnName("catapult");

                entity.Property(e => e.Heavy).HasColumnName("heavy");

                entity.Property(e => e.Knight).HasColumnName("knight");

                entity.Property(e => e.LastUpdated).HasColumnName("last_updated");

                entity.Property(e => e.Light).HasColumnName("light");

                entity.Property(e => e.Marcher).HasColumnName("marcher");

                entity.Property(e => e.Militia).HasColumnName("militia");

                entity.Property(e => e.Ram).HasColumnName("ram");

                entity.Property(e => e.Snob).HasColumnName("snob");

                entity.Property(e => e.Spear).HasColumnName("spear");

                entity.Property(e => e.Spy).HasColumnName("spy");

                entity.Property(e => e.Sword).HasColumnName("sword");

                entity.Property(e => e.WorldId).HasColumnName("world_id");
            });

            modelBuilder.Entity<CurrentBuilding>(entity =>
            {
                entity.HasKey(e => new { e.WorldId, e.VillageId, e.AccessGroupId }).HasName("current_building_pkey");

                entity.ToTable("current_building", "tw");

                entity.HasIndex(e => e.VillageId);
                entity.HasIndex(e => new { e.WorldId, e.AccessGroupId });

                entity.Property(e => e.VillageId)
                    .HasColumnName("village_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.Barracks).HasColumnName("barracks");

                entity.Property(e => e.Church).HasColumnName("church");

                entity.Property(e => e.Farm).HasColumnName("farm");

                entity.Property(e => e.FirstChurch).HasColumnName("first_church");

                entity.Property(e => e.Garage).HasColumnName("garage");

                entity.Property(e => e.Hide).HasColumnName("hide");

                entity.Property(e => e.Iron).HasColumnName("iron");

                entity.Property(e => e.LastUpdated).HasColumnName("last_updated");

                entity.Property(e => e.Main).HasColumnName("main");

                entity.Property(e => e.Market).HasColumnName("market");

                entity.Property(e => e.Place).HasColumnName("place");

                entity.Property(e => e.Smith).HasColumnName("smith");

                entity.Property(e => e.Snob).HasColumnName("snob");

                entity.Property(e => e.Stable).HasColumnName("stable");

                entity.Property(e => e.Statue).HasColumnName("statue");

                entity.Property(e => e.Stone).HasColumnName("stone");

                entity.Property(e => e.Storage).HasColumnName("storage");

                entity.Property(e => e.Wall).HasColumnName("wall");

                entity.Property(e => e.Watchtower).HasColumnName("watchtower");

                entity.Property(e => e.Wood).HasColumnName("wood");

                entity.Property(e => e.WorldId).HasColumnName("world_id");

                entity.Property(e => e.AccessGroupId).HasColumnName("access_group_id");

                entity.HasOne(d => d.Village)
                    .WithOne(p => p.CurrentBuilding)
                    .HasForeignKey<CurrentBuilding>(d => new { d.WorldId, d.VillageId, d.AccessGroupId })
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("buildings_villages_fk");

                entity.HasOne(d => d.World)
                    .WithMany(p => p.CurrentBuilding)
                    .HasForeignKey(d => d.WorldId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("fk_world_id");
            });

            modelBuilder.Entity<CurrentPlayer>(entity =>
            {
                entity.HasKey(e => new { e.WorldId, e.PlayerId, e.AccessGroupId });

                entity.ToTable("current_player", "tw");

                entity.HasIndex(e => new { e.WorldId, e.PlayerId, e.AccessGroupId });

                entity.Property(e => e.PlayerId)
                    .HasColumnName("player_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.CurrentPossibleNobles).HasColumnName("current_possible_nobles");

                entity.Property(e => e.WorldId).HasColumnName("world_id");

                entity.Property(e => e.AccessGroupId).HasColumnName("access_group_id");

                entity.HasOne(d => d.World)
                    .WithMany(p => p.CurrentPlayer)
                    .HasForeignKey(d => d.WorldId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_world_id");
            });

            modelBuilder.Entity<CurrentVillage>(entity =>
            {
                entity.HasKey(e => new { e.WorldId, e.VillageId, e.AccessGroupId }).HasName("villages_pk");

                entity.ToTable("current_village", "tw");

                entity.HasIndex(e => new { e.WorldId, e.VillageId, e.AccessGroupId });

                entity.Property(e => e.VillageId)
                    .HasColumnName("village_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.ArmyAtHomeId).HasColumnName("army_at_home_id");

                entity.Property(e => e.ArmyOwnedId).HasColumnName("army_owned_id");

                entity.Property(e => e.ArmyRecentLossesId).HasColumnName("army_recent_losses_id");

                entity.Property(e => e.ArmyStationedId).HasColumnName("army_stationed_id");

                entity.Property(e => e.ArmySupportingId).HasColumnName("army_supporting_id");

                entity.Property(e => e.ArmyTravelingId).HasColumnName("army_traveling_id");

                entity.Property(e => e.Loyalty).HasColumnName("loyalty");

                entity.Property(e => e.LoyaltyLastUpdated).HasColumnName("loyalty_last_updated");

                entity.Property(e => e.WorldId).HasColumnName("world_id");

                entity.Property(e => e.AccessGroupId).HasColumnName("access_group_id");

                entity.HasOne(d => d.ArmyAtHome)
                    .WithMany(p => p.CurrentVillageArmyAtHome)
                    .HasForeignKey(d => new { d.WorldId, d.ArmyAtHomeId })
                    .HasConstraintName("fk_army_at_home");

                entity.HasOne(d => d.ArmyOwned)
                    .WithMany(p => p.CurrentVillageArmyOwned)
                    .HasForeignKey(d => new { d.WorldId, d.ArmyOwnedId })
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("fk_owned_army");

                entity.HasOne(d => d.ArmyRecentLosses)
                    .WithMany(p => p.CurrentVillageArmyRecentLosses)
                    .HasForeignKey(d => new { d.WorldId, d.ArmyRecentLossesId })
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("current_village_current_army_fk");

                entity.HasOne(d => d.ArmyStationed)
                    .WithMany(p => p.CurrentVillageArmyStationed)
                    .HasForeignKey(d => new { d.WorldId, d.ArmyStationedId })
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("fk_stationed_army");

                entity.HasOne(d => d.ArmySupporting)
                    .WithMany(p => p.CurrentVillageArmySupporting)
                    .HasForeignKey(d => new { d.WorldId, d.ArmySupportingId })
                    .HasConstraintName("fk_army_supporting");

                entity.HasOne(d => d.ArmyTraveling)
                    .WithMany(p => p.CurrentVillageArmyTraveling)
                    .HasForeignKey(d => new { d.WorldId, d.ArmyTravelingId })
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("fk_traveling_army");

                entity.HasOne(d => d.Village)
                    .WithMany(p => p.CurrentVillage)
                    .HasForeignKey(d => new { d.WorldId, d.VillageId })
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_village");

                entity.HasOne(d => d.World)
                    .WithMany(p => p.CurrentVillage)
                    .HasForeignKey(d => d.WorldId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_world_id");
            });

            modelBuilder.Entity<CurrentVillageSupport>(entity =>
            {
                entity.ToTable("current_village_support", "tw");

                entity.HasKey(e => new { e.WorldId, e.Id, e.AccessGroupId });

                entity.HasIndex(e => new { e.WorldId, e.SourceVillageId, e.AccessGroupId });
                entity.HasIndex(e => new { e.WorldId, e.TargetVillageId, e.AccessGroupId });

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("nextval('tw.current_village_support_id_seq'::regclass)")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.LastUpdatedAt).HasColumnName("last_updated_at");

                entity.Property(e => e.SourceVillageId).HasColumnName("source_village_id");

                entity.Property(e => e.SupportingArmyId).HasColumnName("supporting_army_id");

                entity.Property(e => e.TargetVillageId).HasColumnName("target_village_id");

                entity.Property(e => e.WorldId).HasColumnName("world_id");
                entity.Property(e => e.TxId).HasColumnName("tx_id");

                entity.Property(e => e.AccessGroupId).HasColumnName("access_group_id");

                entity.HasOne(d => d.SourceVillage)
                    .WithMany(p => p.CurrentVillageSupportSourceVillage)
                    .HasForeignKey(d => new { d.WorldId, d.SourceVillageId })
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_source_village");

                entity.HasOne(d => d.SupportingArmy)
                    .WithMany(p => p.CurrentVillageSupport)
                    .HasForeignKey(d => new { d.WorldId, d.SupportingArmyId })
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_supporting_army");

                entity.HasOne(d => d.TargetVillage)
                    .WithMany(p => p.CurrentVillageSupportTargetVillage)
                    .HasForeignKey(d => new { d.WorldId, d.TargetVillageId })
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_target_village");

                entity.HasOne(d => d.World)
                    .WithMany(p => p.CurrentVillageSupport)
                    .HasForeignKey(d => d.WorldId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_world_id");

                entity.HasOne(d => d.Tx)
                    .WithMany(p => p.CurrentVillageSupport)
                    .HasForeignKey(d => d.TxId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_tx_id");
            });

            modelBuilder.Entity<EnemyTribe>(entity =>
            {
                entity.ToTable("enemy_tribe", "tw");

                entity.HasKey(e => new { e.Id, e.AccessGroupId });

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("nextval('tw.enemy_tribe_id_seq'::regclass)");

                entity.Property(e => e.WorldId)
                    .HasColumnName("world_id");

                entity.Property(e => e.EnemyTribeId)
                    .HasColumnName("enemy_tribe_id");

                entity.Property(e => e.TxId)
                    .HasColumnName("tx_id");

                entity.Property(e => e.AccessGroupId)
                    .HasColumnName("access_group_id");

                entity.HasOne(e => e.Tx)
                    .WithMany(tx => tx.EnemyTribe)
                    .HasForeignKey(e => e.TxId);
            });

            modelBuilder.Entity<FailedAuthorizationRecord>(entity =>
            {
                entity.ToTable("failed_authorization_record", "security");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("nextval('tw.failed_auth_id_seq'::regclass)");

                entity.Property(e => e.Ip)
                    .IsRequired()
                    .HasColumnName("ip");

                entity.Property(e => e.OccurredAt).HasColumnName("occurred_at");

                entity.Property(e => e.PlayerId).HasColumnName("player_id");

                entity.Property(e => e.Reason)
                    .IsRequired()
                    .HasColumnName("reason")
                    .HasMaxLength(256);

                entity.Property(e => e.RequestedEndpoint)
                    .IsRequired()
                    .HasColumnName("requested_endpoint")
                    .HasMaxLength(128);

                entity.Property(e => e.TribeId).HasColumnName("tribe_id");

                entity.Property(e => e.WorldId).HasColumnName("world_id");
            });

            modelBuilder.Entity<IgnoredReport>(entity =>
            {
                entity.ToTable("ignored_report", "tw");

                entity.HasKey(e => new { e.ReportId, e.WorldId, e.AccessGroupId });

                entity.HasIndex(e => new { e.ReportId, e.WorldId, e.AccessGroupId });

                entity.Property(e => e.ReportId)
                    .HasColumnName("report_id");

                entity.Property(e => e.WorldId)
                    .HasColumnName("world_id");

                entity.Property(e => e.AccessGroupId)
                    .HasColumnName("access_group_id");
            });

            modelBuilder.Entity<InvalidDataRecord>(entity =>
            {
                entity.ToTable("invalid_data_record", "security");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("nextval('security.invalid_data_record_id_seq'::regclass)");

                entity.Property(e => e.DataString).HasColumnName("data_string");

                entity.Property(e => e.Endpoint)
                    .IsRequired()
                    .HasColumnName("endpoint")
                    .HasMaxLength(128);

                entity.Property(e => e.Reason)
                    .IsRequired()
                    .HasColumnName("reason")
                    .HasMaxLength(128);

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.InvalidDataRecord)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("fk_user_id");

                entity.Property(d => d.DataString)
                    .HasColumnName("data_string");
            });
            
            modelBuilder.Entity<PerformanceRecord>(entity =>
            {
                entity.ToTable("performance_record", "tw");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("nextval('tw.performance_record_id_seq'::regclass)");

                entity.Property(e => e.AverageTime).HasColumnName("average_time");

                entity.Property(e => e.GeneratedAt).HasColumnName("generated_at");

                entity.Property(e => e.MaxTime).HasColumnName("max_time");

                entity.Property(e => e.MinTime).HasColumnName("min_time");

                entity.Property(e => e.NumSamples).HasColumnName("num_samples");

                entity.Property(e => e.OperationLabel)
                    .IsRequired()
                    .HasColumnName("operation_label")
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<Player>(entity =>
            {
                entity.ToTable("player", "tw_provided");

                entity.HasKey(e => new { e.WorldId, e.PlayerId });

                entity.Property(e => e.PlayerId)
                    .HasColumnName("player_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.PlayerName)
                    .HasColumnName("player_name")
                    .HasColumnType("character varying");

                entity.Property(e => e.PlayerRank).HasColumnName("player_rank");

                entity.Property(e => e.Points).HasColumnName("points");

                entity.Property(e => e.TribeId).HasColumnName("tribe_id");

                entity.Property(e => e.Villages).HasColumnName("villages");

                entity.Property(e => e.WorldId).HasColumnName("world_id");

                entity.HasOne(d => d.World)
                    .WithMany(p => p.Player)
                    .HasForeignKey(d => d.WorldId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_world_id");

                entity.HasOne(e => e.Tribe)
                    .WithMany(t => t.Players)
                    .HasForeignKey(p => new { p.WorldId, p.TribeId })
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasIndex(e => e.PlayerId);
                entity.HasIndex(e => e.TribeId);
            });

            modelBuilder.Entity<Report>(entity =>
            {
                entity.ToTable("report", "tw");

                entity.HasKey(e => new { e.WorldId, e.ReportId, e.AccessGroupId });

                entity.HasIndex(e => e.BuildingId);
                entity.HasIndex(e => e.AttackerPlayerId);
                entity.HasIndex(e => e.DefenderPlayerId);
                entity.HasIndex(e => e.AccessGroupId);
                entity.HasIndex(e => e.OccuredAt);
                
                entity.Property(e => e.ReportId)
                    .HasColumnName("report_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.AttackerArmyId).HasColumnName("attacker_army_id");

                entity.Property(e => e.AttackerLossesArmyId).HasColumnName("attacker_losses_army_id");

                entity.Property(e => e.AttackerPlayerId).HasColumnName("attacker_player_id");

                entity.Property(e => e.AttackerVillageId).HasColumnName("attacker_village_id");

                entity.Property(e => e.BuildingId).HasColumnName("building_id");

                entity.Property(e => e.DefenderArmyId).HasColumnName("defender_army_id");

                entity.Property(e => e.DefenderLossesArmyId).HasColumnName("defender_losses_army_id");

                entity.Property(e => e.DefenderPlayerId).HasColumnName("defender_player_id");

                entity.Property(e => e.DefenderTravelingArmyId).HasColumnName("defender_traveling_army_id");

                entity.Property(e => e.DefenderVillageId).HasColumnName("defender_village_id");

                entity.Property(e => e.Loyalty).HasColumnName("loyalty");

                entity.Property(e => e.Luck).HasColumnName("luck");

                entity.Property(e => e.Morale).HasColumnName("morale");

                entity.Property(e => e.OccuredAt).HasColumnName("occured_at");

                entity.Property(e => e.TxId).HasColumnName("tx_id");

                entity.Property(e => e.WorldId).HasColumnName("world_id");

                entity.Property(e => e.AccessGroupId).HasColumnName("access_group_id");

                entity.HasOne(d => d.AttackerArmy)
                    .WithMany(p => p.ReportAttackerArmy)
                    .HasForeignKey(d => new { d.WorldId, d.AttackerArmyId })
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_attacker_army");

                entity.HasOne(d => d.AttackerLossesArmy)
                    .WithMany(p => p.ReportAttackerLossesArmy)
                    .HasForeignKey(d => new { d.WorldId, d.AttackerLossesArmyId })
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_attacker_army_losses");

                entity.HasOne(d => d.AttackerPlayer)
                    .WithMany(p => p.ReportAttackerPlayer)
                    .HasForeignKey(d => new { d.WorldId, d.AttackerPlayerId })
                    .HasConstraintName("fk_attacker_player");

                entity.HasOne(d => d.AttackerVillage)
                    .WithMany(p => p.ReportAttackerVillage)
                    .HasForeignKey(d => new { d.WorldId, d.AttackerVillageId })
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_attacker_village");

                entity.HasOne(d => d.Building)
                    .WithMany(p => p.Report)
                    .HasForeignKey(d => new { d.WorldId, d.BuildingId })
                    .HasConstraintName("fk_building");

                entity.HasOne(d => d.DefenderArmy)
                    .WithMany(p => p.ReportDefenderArmy)
                    .HasForeignKey(d => new { d.WorldId, d.DefenderArmyId })
                    .HasConstraintName("fk_defender_army");

                entity.HasOne(d => d.DefenderLossesArmy)
                    .WithMany(p => p.ReportDefenderLossesArmy)
                    .HasForeignKey(d => new { d.WorldId, d.DefenderLossesArmyId })
                    .HasConstraintName("fk_defender_army_losses");

                entity.HasOne(d => d.DefenderPlayer)
                    .WithMany(p => p.ReportDefenderPlayer)
                    .HasForeignKey(d => new { d.WorldId, d.DefenderPlayerId })
                    .HasConstraintName("fk_defender_player");

                entity.HasOne(d => d.DefenderTravelingArmy)
                    .WithMany(p => p.ReportDefenderTravelingArmy)
                    .HasForeignKey(d => new { d.WorldId, d.DefenderTravelingArmyId })
                    .HasConstraintName("fk_defender_traveling_army");

                entity.HasOne(d => d.DefenderVillage)
                    .WithMany(p => p.ReportDefenderVillage)
                    .HasForeignKey(d => new { d.WorldId, d.DefenderVillageId })
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_defender_village");

                entity.HasOne(d => d.Tx)
                    .WithMany(p => p.Report)
                    .HasForeignKey(d => d.TxId)
                    .HasConstraintName("fk_tx_id");

                entity.HasOne(d => d.World)
                    .WithMany(p => p.Report)
                    .HasForeignKey(d => d.WorldId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_world_id");
            });

            modelBuilder.Entity<ReportArmy>(entity =>
            {
                entity.HasKey(e => new { e.WorldId, e.ArmyId });

                entity.ToTable("report_army", "tw");

                entity.HasIndex(e => e.ArmyId).IsUnique();
                
                entity.Property(e => e.ArmyId)
                    .HasColumnName("army_id")
                    .HasDefaultValueSql("nextval('tw.report_armies_army_id_seq'::regclass)");

                entity.Property(e => e.Archer).HasColumnName("archer");

                entity.Property(e => e.Axe).HasColumnName("axe");

                entity.Property(e => e.Catapult).HasColumnName("catapult");

                entity.Property(e => e.Heavy).HasColumnName("heavy");

                entity.Property(e => e.Knight).HasColumnName("knight");

                entity.Property(e => e.Light).HasColumnName("light");

                entity.Property(e => e.Marcher).HasColumnName("marcher");

                entity.Property(e => e.Militia).HasColumnName("militia");

                entity.Property(e => e.Ram).HasColumnName("ram");

                entity.Property(e => e.Snob).HasColumnName("snob");

                entity.Property(e => e.Spear).HasColumnName("spear");

                entity.Property(e => e.Spy).HasColumnName("spy");

                entity.Property(e => e.Sword).HasColumnName("sword");

                entity.Property(e => e.WorldId).HasColumnName("world_id");

                entity.HasOne(d => d.World)
                    .WithMany(p => p.ReportArmy)
                    .HasForeignKey(d => d.WorldId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_world_id");
            });

            modelBuilder.Entity<ReportBuilding>(entity =>
            {
                entity.ToTable("report_building", "tw");

                entity.HasKey(e => new { e.WorldId, e.ReportBuildingId });

                entity.HasIndex(e => e.ReportBuildingId).IsUnique();
                
                entity.Property(e => e.ReportBuildingId)
                    .HasColumnName("report_building_id")
                    .HasDefaultValueSql("nextval('tw.report_building_report_building_id_seq'::regclass)");

                entity.Property(e => e.Barracks).HasColumnName("barracks");

                entity.Property(e => e.Church).HasColumnName("church");

                entity.Property(e => e.Farm).HasColumnName("farm");

                entity.Property(e => e.FirstChurch).HasColumnName("first_church");

                entity.Property(e => e.Garage).HasColumnName("garage");

                entity.Property(e => e.Hide).HasColumnName("hide");

                entity.Property(e => e.Iron).HasColumnName("iron");

                entity.Property(e => e.Main).HasColumnName("main");

                entity.Property(e => e.Market).HasColumnName("market");

                entity.Property(e => e.Place).HasColumnName("place");

                entity.Property(e => e.Smith).HasColumnName("smith");

                entity.Property(e => e.Snob).HasColumnName("snob");

                entity.Property(e => e.Stable).HasColumnName("stable");

                entity.Property(e => e.Statue).HasColumnName("statue");

                entity.Property(e => e.Stone).HasColumnName("stone");

                entity.Property(e => e.Storage).HasColumnName("storage");

                entity.Property(e => e.Wall).HasColumnName("wall");

                entity.Property(e => e.Watchtower).HasColumnName("watchtower");

                entity.Property(e => e.Wood).HasColumnName("wood");

                entity.Property(e => e.WorldId).HasColumnName("world_id");

                entity.HasOne(d => d.World)
                    .WithMany(p => p.ReportBuilding)
                    .HasForeignKey(d => d.WorldId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_world_id");
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(e => e.TxId);

                entity.ToTable("transaction", "security");

                entity.HasIndex(e => e.TxId);
                entity.HasIndex(e => e.WorldId);

                entity.Property(e => e.TxId)
                    .HasColumnName("tx_id")
                    .HasDefaultValueSql("nextval('tw.tx_id_seq'::regclass)");

                entity.Property(e => e.Ip)
                    .IsRequired()
                    .HasColumnName("ip");

                entity.Property(e => e.OccurredAt).HasColumnName("occurred_at");

                entity.Property(e => e.PreviousTxId).HasColumnName("previous_tx_id");

                entity.Property(e => e.Uid).HasColumnName("uid");

                entity.Property(e => e.WorldId).HasColumnName("world_id");

                entity.HasOne(d => d.World)
                    .WithMany(p => p.Transaction)
                    .HasForeignKey(d => d.WorldId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_world_id");
            });

            modelBuilder.Entity<TranslationEntry>(entity =>
            {
                entity.HasKey(e => new { e.TranslationId, e.KeyId });

                entity.ToTable("translation", "feature");

                entity.HasData(Seed.TranslationEntryData.Contents);

                entity.HasIndex(e => e.TranslationId).HasMethod("hash");
                entity.HasIndex(e => e.KeyId).HasMethod("hash");

                entity.Property(e => e.TranslationId)
                    .IsRequired()
                    .HasColumnName("translation_id");

                entity.Property(e => e.KeyId)
                    .IsRequired()
                    .HasColumnName("key");

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasColumnName("value");

                entity.HasOne(e => e.Translation)
                    .WithMany(e => e.Entries)
                    .HasForeignKey(e => e.TranslationId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("fk_translation_registry_id");

                entity.HasOne(e => e.Key)
                    .WithMany(e => e.TranslationEntries)
                    .HasForeignKey(e => e.KeyId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("fk_translation_key_id");
            });

            modelBuilder.Entity<TranslationKey>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.ToTable("translation_key", "feature");

                entity.HasData(Seed.TranslationKeyData.Contents);

                entity.Property(e => e.Id)
                    .IsRequired()
                    .HasColumnName("id")
                    .HasDefaultValueSql("nextval('feature.translation_key_id_seq'::regclass)"); ;

                entity.Property(e => e.Name).IsRequired().HasColumnName("name");
                entity.Property(e => e.IsTwNative).HasDefaultValue(false).IsRequired().HasColumnName("is_tw_native");
                entity.Property(e => e.Group).IsRequired().HasColumnName("group");
                entity.Property(e => e.Note).HasDefaultValue(null).HasColumnName("note");
            });

            modelBuilder.Entity<TranslationLanguage>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.ToTable("translation_language", "feature");

                entity.HasData(Seed.TranslationLanguageData.Contents);

                entity.Property(e => e.Id)
                    .IsRequired()
                    .HasColumnName("id")
                    .HasDefaultValueSql("nextval('feature.translation_language_id_seq'::regclass)");

                entity.Property(e => e.Name).IsRequired().HasColumnName("name");
            });

            modelBuilder.Entity<TranslationParameter>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.ToTable("translation_parameter", "feature");

                entity.HasData(Seed.TranslationParameterData.Contents);

                entity.Property(e => e.Id)
                    .IsRequired()
                    .HasColumnName("id")
                    .HasDefaultValueSql("nextval('feature.translation_parameter_id_seq'::regclass)");

                entity.Property(e => e.KeyId)
                    .IsRequired()
                    .HasColumnName("key_id");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name");

                entity.HasOne(e => e.Key)
                    .WithMany(e => e.Parameters)
                    .HasForeignKey(e => e.KeyId)
                    .HasConstraintName("fk_translation_key_id");
            });

            modelBuilder.Entity<TranslationRegistry>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.ToTable("translation_registry", "feature");

                entity.HasData(Seed.TranslationRegistryData.Contents);

                entity.Property(e => e.Id)
                    .IsRequired()
                    .HasColumnName("id")
                    .HasDefaultValueSql("nextval('feature.translation_registry_id_seq'::regclass)");

                entity.Property(e => e.Name).IsRequired().HasColumnName("name");
                entity.Property(e => e.Author).IsRequired().HasColumnName("author");
                entity.Property(e => e.AuthorPlayerId).IsRequired().HasColumnName("author_player_id");
                entity.Property(e => e.LanguageId).IsRequired().HasColumnName("language_id");
                entity.Property(e => e.IsSystemInternal).IsRequired().HasDefaultValue(false).HasColumnName("is_system_internal");

                entity.HasOne(e => e.Language)
                    .WithMany(e => e.Translations)
                    .HasForeignKey(e => e.LanguageId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("fk_registry_language_id");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Uid);

                entity.ToTable("user", "security");

                entity.HasIndex(e => e.Enabled);
                entity.HasIndex(e => e.PlayerId);
                entity.HasIndex(e => e.AuthToken).IsUnique();
                entity.HasIndex(e => e.AccessGroupId);
                entity.HasIndex(e => e.Uid);
                entity.HasIndex(e => e.WorldId);

                entity.Property(e => e.Uid)
                    .HasColumnName("uid")
                    .HasDefaultValueSql("nextval('tw.users_uid_seq'::regclass)");

                entity.Property(e => e.AdminAuthToken).HasColumnName("admin_auth_token");

                entity.Property(e => e.AdminPlayerId).HasColumnName("admin_player_id");

                entity.Property(e => e.AuthToken).HasColumnName("auth_token");

                entity.Property(e => e.Enabled).HasColumnName("enabled");

                entity.Property(e => e.KeySource).HasColumnName("key_source");

                entity.Property(e => e.Label).HasColumnName("label");

                entity.Property(e => e.PermissionsLevel).HasColumnName("permissions_level");

                entity.Property(e => e.PlayerId).HasColumnName("player_id");

                entity.Property(e => e.TransactionTime).HasColumnName("transaction_time");

                entity.Property(e => e.WorldId).HasColumnName("world_id");

                entity.Property(e => e.IsReadOnly).HasColumnName("is_read_only");

                entity.Property(e => e.TxId).HasColumnName("tx_id");

                entity.Property(e => e.AccessGroupId).HasColumnName("access_group_id");

                entity.HasOne(d => d.World)
                    .WithMany(p => p.User)
                    .HasForeignKey(d => d.WorldId)
                    .HasConstraintName("fk_world_id");

                entity.HasOne(d => d.Tx)
                    .WithMany(p => p.User)
                    .HasForeignKey(d => d.TxId)
                    .HasConstraintName("fk_tx_id");

                entity.HasOne(e => e.AccessGroup)
                    .WithMany(a => a.Users)
                    .HasForeignKey(e => e.AccessGroupId)
                    .HasConstraintName("fk_access_group_id");
            });

            modelBuilder.Entity<UserLog>(entity =>
            {
                entity.ToTable("user_log", "security");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("nextval('security.user_log_id_seq'::regclass)");

                entity.Property(e => e.AdminAuthToken).HasColumnName("admin_auth_token");

                entity.Property(e => e.AdminPlayerId).HasColumnName("admin_player_id");

                entity.Property(e => e.AuthToken).HasColumnName("auth_token");

                entity.Property(e => e.Enabled).HasColumnName("enabled");

                entity.Property(e => e.KeySource).HasColumnName("key_source");

                entity.Property(e => e.Label).HasColumnName("label");

                entity.Property(e => e.OperationType)
                    .IsRequired()
                    .HasColumnName("operation_type")
                    .HasColumnType("character varying");

                entity.Property(e => e.PermissionsLevel).HasColumnName("permissions_level");

                entity.Property(e => e.PlayerId).HasColumnName("player_id");

                entity.Property(e => e.TransactionTime).HasColumnName("transaction_time");

                entity.Property(e => e.Uid).HasColumnName("uid");

                entity.Property(e => e.WorldId).HasColumnName("world_id");

                entity.Property(e => e.IsReadOnly).HasColumnName("is_read_only");

                entity.Property(e => e.TxId).HasColumnName("tx_id");

                entity.Property(e => e.AccessGroupId).HasColumnName("access_group_id");

                entity.HasOne(d => d.World)
                    .WithMany(p => p.UserLog)
                    .HasForeignKey(d => d.WorldId)
                    .HasConstraintName("fk_world_id");

                entity.HasOne(d => d.Tx)
                    .WithMany(p => p.UserLog)
                    .HasForeignKey(d => d.TxId)
                    .HasConstraintName("fk_tx_id");
            });

            modelBuilder.Entity<UserUploadHistory>(entity =>
            {
                entity.ToTable("user_upload_history", "security");

                entity.HasIndex(e => e.Uid);

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("nextval('security.user_upload_history_id_seq'::regclass)");

                entity.Property(e => e.LastUploadedCommandsAt).HasColumnName("last_uploaded_commands_at");

                entity.Property(e => e.LastUploadedIncomingsAt).HasColumnName("last_uploaded_incomings_at");

                entity.Property(e => e.LastUploadedReportsAt).HasColumnName("last_uploaded_reports_at");

                entity.Property(e => e.LastUploadedTroopsAt).HasColumnName("last_uploaded_troops_at");

                entity.Property(e => e.Uid).HasColumnName("uid");

                entity.HasOne(d => d.U)
                    .WithMany(p => p.UserUploadHistory)
                    .HasForeignKey(d => d.Uid)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("fk_uid");
            });

            modelBuilder.Entity<Village>(entity =>
            {
                entity.ToTable("village", "tw_provided");

                entity.HasKey(e => new { e.WorldId, e.VillageId });

                entity.HasIndex(e => e.VillageId);
                entity.HasIndex(e => e.PlayerId);
                entity.HasIndex(e => new { e.WorldId, e.PlayerId });
                entity.HasIndex(e => new { e.WorldId, e.VillageId }).IsUnique();
                
                entity.Property(e => e.VillageId)
                    .HasColumnName("village_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.PlayerId).HasColumnName("player_id");

                entity.Property(e => e.Points).HasColumnName("points");

                entity.Property(e => e.VillageName)
                    .HasColumnName("village_name")
                    .HasColumnType("character varying");

                entity.Property(e => e.VillageRank).HasColumnName("village_rank");

                entity.Property(e => e.WorldId).HasColumnName("world_id");

                entity.Property(e => e.X).HasColumnName("x");

                entity.Property(e => e.Y).HasColumnName("y");

                entity.HasOne(d => d.Player)
                    .WithMany(p => p.Village)
                    .HasForeignKey(d => new { d.WorldId, d.PlayerId })
                    .HasConstraintName("fk_player_id");

                entity.HasOne(d => d.World)
                    .WithMany(p => p.Village)
                    .HasForeignKey(d => d.WorldId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("fk_world_id");
            });

            modelBuilder.Entity<World>(entity =>
            {
                entity.ToTable("world", "tw_provided");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("nextval('tw_provided.world_id_seq'::regclass)");

                entity.Property(e => e.Hostname)
                    .IsRequired()
                    .HasColumnName("hostname")
                    .HasMaxLength(32);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(6);

                entity.Property(e => e.DefaultTranslationId)
                    .IsRequired()
                    .HasColumnName("default_translation_id");

                entity.Property(e => e.IsBeta)
                    .IsRequired()
                    .HasColumnName("is_beta");

                entity.Property(e => e.IsPendingDeletion)
                    .IsRequired()
                    .HasColumnName("is_pending_deletion");

                entity.HasOne(e => e.DefaultTranslation)
                    .WithMany(e => e.DefaultWorlds)
                    .HasForeignKey(e => e.DefaultTranslationId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("fk_default_translation_id");
            });

            modelBuilder.Entity<WorldSettings>(entity =>
            {
                entity.HasKey(e => e.WorldId);

                entity.ToTable("world_settings", "tw_provided");

                entity.Property(e => e.WorldId)
                    .HasColumnName("world_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.AccountSittingEnabled).HasColumnName("account_sitting_enabled");

                entity.Property(e => e.ArchersEnabled).HasColumnName("archers_enabled");

                entity.Property(e => e.BonusVillagesEnabled).HasColumnName("bonus_villages_enabled");

                entity.Property(e => e.CanDemolishBuildings).HasColumnName("can_demolish_buildings");

                entity.Property(e => e.ChurchesEnabled).HasColumnName("churches_enabled");

                entity.Property(e => e.FlagsEnabled).HasColumnName("flags_enabled");

                entity.Property(e => e.GameSpeed).HasColumnName("game_speed");

                entity.Property(e => e.MaxNoblemanDistance).HasColumnName("max_nobleman_distance");

                entity.Property(e => e.MilitiaEnabled).HasColumnName("militia_enabled");

                entity.Property(e => e.MillisecondsEnabled).HasColumnName("milliseconds_enabled");

                entity.Property(e => e.MoraleEnabled).HasColumnName("morale_enabled");

                entity.Property(e => e.NightBonusEnabled).HasColumnName("night_bonus_enabled");

                entity.Property(e => e.NoblemanLoyaltyMax).HasColumnName("nobleman_loyalty_max");

                entity.Property(e => e.NoblemanLoyaltyMin).HasColumnName("nobleman_loyalty_min");

                entity.Property(e => e.PaladinEnabled).HasColumnName("paladin_enabled");

                entity.Property(e => e.PaladinItemsEnabled).HasColumnName("paladin_items_enabled");

                entity.Property(e => e.PaladinSkillsEnabled).HasColumnName("paladin_skills_enabled");

                entity.Property(e => e.TimeZoneId).HasColumnName("timezone");

                entity.Property(e => e.UnitSpeed).HasColumnName("unit_speed");

                entity.Property(e => e.WatchtowerEnabled).HasColumnName("watchtower_enabled");

                entity.HasOne(d => d.World)
                    .WithOne(p => p.WorldSettings)
                    .HasForeignKey<WorldSettings>(d => d.WorldId)
                    .HasConstraintName("fk_world_id");
            });

            modelBuilder.HasSequence("conflicting_data_record_id_seq", "security");
            modelBuilder.HasSequence("invalid_data_record_id_seq", "security");
            modelBuilder.HasSequence("user_log_id_seq", "security");
            modelBuilder.HasSequence("user_upload_history_id_seq", "security");
            modelBuilder.HasSequence("access_group_id_seq", "security");

            modelBuilder.HasSequence("command_army_id_seq", "tw");
            modelBuilder.HasSequence("current_village_support_id_seq", "tw");
            modelBuilder.HasSequence("failed_auth_id_seq", "tw");
            modelBuilder.HasSequence("performance_record_id_seq", "tw");
            modelBuilder.HasSequence("report_armies_army_id_seq", "tw");
            modelBuilder.HasSequence("report_building_report_building_id_seq", "tw");
            modelBuilder.HasSequence("tx_id_seq", "tw");
            modelBuilder.HasSequence("users_uid_seq", "tw");
            modelBuilder.HasSequence("enemy_tribe_id_seq", "tw");

            modelBuilder.HasSequence<int>("conquers_vault_id_seq", "tw_provided");
            modelBuilder.HasSequence<short>("world_id_seq", "tw_provided");

            modelBuilder.HasSequence<short>("translation_key_id_seq", "feature");
            modelBuilder.HasSequence<short>("translation_language_id_seq", "feature");
            modelBuilder.HasSequence<short>("translation_registry_id_seq", "feature");
            modelBuilder.HasSequence<short>("translation_parameter_id_seq", "feature");
        }
    }
}
