using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace TW.Vault.Scaffold_Model
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

        public virtual DbSet<Ally> Ally { get; set; }
        public virtual DbSet<Army> Army { get; set; }
        public virtual DbSet<Command> Command { get; set; }
        public virtual DbSet<Conquers> Conquers { get; set; }
        public virtual DbSet<Player> Player { get; set; }
        public virtual DbSet<Report> Report { get; set; }
        public virtual DbSet<Village> Village { get; set; }

        // Unable to generate entity type for table 'testing.testpostgis'. Please see the warning messages.

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseNpgsql("Host=192.168.1.250; Port=22342; Database=vault; Username=twu_vault; Password=!!TWV@ult4Us??");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresExtension("postgis");

            modelBuilder.Entity<Ally>(entity =>
            {
                entity.HasKey(e => e.TribeId);

                entity.ToTable("ally", "w100_tw_provided");

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
            });

            modelBuilder.Entity<Army>(entity =>
            {
                entity.ToTable("army", "w100");

                entity.Property(e => e.ArmyId)
                    .HasColumnName("army_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.UnitArcher).HasColumnName("unit_archer");

                entity.Property(e => e.UnitAxe).HasColumnName("unit_axe");

                entity.Property(e => e.UnitCatapult).HasColumnName("unit_catapult");

                entity.Property(e => e.UnitHeavy).HasColumnName("unit_heavy");

                entity.Property(e => e.UnitKnight).HasColumnName("unit_knight");

                entity.Property(e => e.UnitLight).HasColumnName("unit_light");

                entity.Property(e => e.UnitMarcher).HasColumnName("unit_marcher");

                entity.Property(e => e.UnitMilitia).HasColumnName("unit_militia");

                entity.Property(e => e.UnitRam).HasColumnName("unit_ram");

                entity.Property(e => e.UnitSnob).HasColumnName("unit_snob");

                entity.Property(e => e.UnitSpear).HasColumnName("unit_spear");

                entity.Property(e => e.UnitSpy).HasColumnName("unit_spy");

                entity.Property(e => e.UnitSword).HasColumnName("unit_sword");
            });

            modelBuilder.Entity<Command>(entity =>
            {
                entity.ToTable("command", "w100");

                entity.Property(e => e.CommandId)
                    .HasColumnName("command_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.FirstSeenAt).HasColumnName("first_seen_at");

                entity.Property(e => e.LandsAt).HasColumnName("lands_at");

                entity.Property(e => e.SourcePlayerId).HasColumnName("source_player_id");

                entity.Property(e => e.SourceVillageId).HasColumnName("source_village_id");

                entity.Property(e => e.TargetPlayerId).HasColumnName("target_player_id");

                entity.Property(e => e.TargetVillageId).HasColumnName("target_village_id");

                entity.Property(e => e.TroopType)
                    .HasColumnName("troop_type")
                    .HasColumnType("character varying");

                entity.HasOne(d => d.SourcePlayer)
                    .WithMany(p => p.CommandSourcePlayer)
                    .HasForeignKey(d => d.SourcePlayerId)
                    .HasConstraintName("fk_source_player_id");

                entity.HasOne(d => d.SourceVillage)
                    .WithMany(p => p.CommandSourceVillage)
                    .HasForeignKey(d => d.SourceVillageId)
                    .HasConstraintName("fk_source_village_id");

                entity.HasOne(d => d.TargetPlayer)
                    .WithMany(p => p.CommandTargetPlayer)
                    .HasForeignKey(d => d.TargetPlayerId)
                    .HasConstraintName("fk_target_player_id");

                entity.HasOne(d => d.TargetVillage)
                    .WithMany(p => p.CommandTargetVillage)
                    .HasForeignKey(d => d.TargetVillageId)
                    .HasConstraintName("fk_target_village_id");
            });

            modelBuilder.Entity<Conquers>(entity =>
            {
                entity.HasKey(e => e.VaultId);

                entity.ToTable("conquers", "w100_tw_provided");

                entity.Property(e => e.VaultId)
                    .HasColumnName("vault_id")
                    .HasDefaultValueSql("nextval('w100_tw_provided.conquers_vault_id_seq'::regclass)");

                entity.Property(e => e.NewOwner).HasColumnName("new_owner");

                entity.Property(e => e.OldOwner).HasColumnName("old_owner");

                entity.Property(e => e.UnixTimestamp).HasColumnName("unix_timestamp");

                entity.Property(e => e.VillageId).HasColumnName("village_id");
            });

            modelBuilder.Entity<Player>(entity =>
            {
                entity.ToTable("player", "w100_tw_provided");

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
            });

            modelBuilder.Entity<Report>(entity =>
            {
                entity.ToTable("report", "w100");

                entity.HasIndex(e => e.AttackerArmyId)
                    .HasName("fki_fk_attacker_army_id");

                entity.HasIndex(e => e.AttackerLossesArmyId)
                    .HasName("fki_fk_attacker_losses_army_id");

                entity.HasIndex(e => e.AttackerPlayerId)
                    .HasName("fki_attacker_player_id");

                entity.HasIndex(e => e.AttackerVillageId)
                    .HasName("fki_fk_attacker_village_id");

                entity.HasIndex(e => e.DefenderArmyId)
                    .HasName("fki_fk_defender_army_id");

                entity.HasIndex(e => e.DefenderLossesArmyId)
                    .HasName("fki_fk_defender_losses_army_id");

                entity.HasIndex(e => e.DefenderPlayerId)
                    .HasName("fki_fk_defender_player_id");

                entity.HasIndex(e => e.DefenderTravelingArmyId)
                    .HasName("fki_fk_defender_traveling_army_id");

                entity.HasIndex(e => e.DefenderVillageId)
                    .HasName("fki_fk_defender_village_id");

                entity.Property(e => e.ReportId)
                    .HasColumnName("report_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.AttackerArmyId).HasColumnName("attacker_army_id");

                entity.Property(e => e.AttackerLossesArmyId).HasColumnName("attacker_losses_army_id");

                entity.Property(e => e.AttackerPlayerId).HasColumnName("attacker_player_id");

                entity.Property(e => e.AttackerVillageId).HasColumnName("attacker_village_id");

                entity.Property(e => e.DefenderArmyId).HasColumnName("defender_army_id");

                entity.Property(e => e.DefenderLossesArmyId).HasColumnName("defender_losses_army_id");

                entity.Property(e => e.DefenderPlayerId).HasColumnName("defender_player_id");

                entity.Property(e => e.DefenderTravelingArmyId).HasColumnName("defender_traveling_army_id");

                entity.Property(e => e.DefenderVillageId).HasColumnName("defender_village_id");

                entity.Property(e => e.OccuredAt).HasColumnName("occured_at");

                entity.HasOne(d => d.AttackerArmy)
                    .WithMany(p => p.ReportAttackerArmy)
                    .HasForeignKey(d => d.AttackerArmyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_attacker_army_id");

                entity.HasOne(d => d.AttackerLossesArmy)
                    .WithMany(p => p.ReportAttackerLossesArmy)
                    .HasForeignKey(d => d.AttackerLossesArmyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_attacker_losses_army_id");

                entity.HasOne(d => d.AttackerPlayer)
                    .WithMany(p => p.ReportAttackerPlayer)
                    .HasForeignKey(d => d.AttackerPlayerId)
                    .HasConstraintName("fk_attacker_player_id");

                entity.HasOne(d => d.AttackerVillage)
                    .WithMany(p => p.ReportAttackerVillage)
                    .HasForeignKey(d => d.AttackerVillageId)
                    .HasConstraintName("fk_attacker_village_id");

                entity.HasOne(d => d.DefenderArmy)
                    .WithMany(p => p.ReportDefenderArmy)
                    .HasForeignKey(d => d.DefenderArmyId)
                    .HasConstraintName("fk_defender_army_id");

                entity.HasOne(d => d.DefenderLossesArmy)
                    .WithMany(p => p.ReportDefenderLossesArmy)
                    .HasForeignKey(d => d.DefenderLossesArmyId)
                    .HasConstraintName("fk_defender_losses_army_id");

                entity.HasOne(d => d.DefenderPlayer)
                    .WithMany(p => p.ReportDefenderPlayer)
                    .HasForeignKey(d => d.DefenderPlayerId)
                    .HasConstraintName("fk_defender_player_id");

                entity.HasOne(d => d.DefenderTravelingArmy)
                    .WithMany(p => p.ReportDefenderTravelingArmy)
                    .HasForeignKey(d => d.DefenderTravelingArmyId)
                    .HasConstraintName("fk_defender_traveling_army_id");

                entity.HasOne(d => d.DefenderVillage)
                    .WithMany(p => p.ReportDefenderVillage)
                    .HasForeignKey(d => d.DefenderVillageId)
                    .HasConstraintName("fk_defender_village_id");
            });

            modelBuilder.Entity<Village>(entity =>
            {
                entity.ToTable("village", "w100_tw_provided");

                entity.Property(e => e.VillageId)
                    .HasColumnName("village_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.PlayerId).HasColumnName("player_id");

                entity.Property(e => e.Points).HasColumnName("points");

                entity.Property(e => e.VillageName)
                    .HasColumnName("village_name")
                    .HasColumnType("character varying");

                entity.Property(e => e.VillageRank).HasColumnName("village_rank");

                entity.Property(e => e.X).HasColumnName("x");

                entity.Property(e => e.Y).HasColumnName("y");
            });

            modelBuilder.HasSequence<int>("conquers_vault_id_seq");
        }
    }
}
