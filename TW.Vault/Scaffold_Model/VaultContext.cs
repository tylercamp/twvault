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
        public virtual DbSet<Player> Player { get; set; }
        public virtual DbSet<Village> Village { get; set; }

        // Unable to generate entity type for table 'tw_provided.testpostgis'. Please see the warning messages.

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                throw new InvalidOperationException("Configuration wasn't provided, check Startup.cs?");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresExtension("postgis");

            modelBuilder.Entity<Ally>(entity =>
            {
                entity.HasKey(e => e.TribeId);

                entity.ToTable("ally", "tw_provided");

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

            modelBuilder.Entity<Player>(entity =>
            {
                entity.ToTable("player", "tw_provided");

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

            modelBuilder.Entity<Village>(entity =>
            {
                entity.ToTable("village", "tw_provided");

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
        }
    }
}
