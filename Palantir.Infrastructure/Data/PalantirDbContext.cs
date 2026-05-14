    namespace Palantir.Infrastructure.Data
{
    public partial class PalantirDbContext : DbContext
    {
        public PalantirDbContext(DbContextOptions<PalantirDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Army> armies { get; set; }

        public virtual DbSet<ArmyPosition> army_positions { get; set; }

        public virtual DbSet<ControlZone> control_zones { get; set; }

        public virtual DbSet<Event> events { get; set; }

        public virtual DbSet<Operation> operations { get; set; }

        public virtual DbSet<OperationSide> operation_sides { get; set; }

        public virtual DbSet<Side> sides { get; set; }

        public virtual DbSet<Theater> theaters { get; set; }

        public virtual DbSet<War> wars { get; set; }

        public virtual DbSet<WarSide> war_sides { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresExtension("postgis");

            modelBuilder.Entity<Army>(entity =>
            {
                entity.HasKey(e => e.army_id).HasName("armies_pkey");

                entity.HasOne(d => d.war_side).WithMany(p => p.armies)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("armies_war_side_id_fkey");
            });

            modelBuilder.Entity<ArmyPosition>(entity =>
            {
                entity.HasKey(e => e.army_position_id).HasName("army_positions_pkey");

                entity.HasIndex(e => e.coordinate, "idx_army_positions_geom").HasMethod("gist");

                entity.HasOne(d => d.army).WithMany(p => p.army_positions)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("army_positions_army_id_fkey");
            });

            modelBuilder.Entity<ControlZone>(entity =>
            {
                entity.HasKey(e => e.control_id).HasName("control_zones_pkey");

                entity.HasIndex(e => e.geom, "idx_control_zones_geom").HasMethod("gist");

                entity.HasOne(d => d.war).WithMany(p => p.control_zones)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("control_zones_war_id_fkey");

                entity.HasOne(d => d.war_side).WithMany(p => p.control_zones)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("control_zones_war_side_id_fkey");
            });

            modelBuilder.Entity<Event>(entity =>
            {
                entity.HasKey(e => e.event_id).HasName("events_pkey");

                entity.HasIndex(e => e.coordinate, "idx_events_geom").HasMethod("gist");

                entity.HasOne(d => d.operation).WithMany(p => p.events).HasConstraintName("events_operation_id_fkey");

                entity.HasOne(d => d.war).WithMany(p => p.events)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("events_war_id_fkey");

                entity.HasOne(d => d.war_side).WithMany(p => p.events).HasConstraintName("events_war_side_id_fkey");
            });

            modelBuilder.Entity<Operation>(entity =>
            {
                entity.HasKey(e => e.operation_id).HasName("operations_pkey");

                entity.HasOne(d => d.theater).WithMany(p => p.operations)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("operations_theater_id_fkey");
            });

            modelBuilder.Entity<OperationSide>(entity =>
            {
                entity.HasKey(e => new { e.operation_id, e.war_side_id }).HasName("operation_sides_pkey");

                entity.HasOne(d => d.operation).WithMany(p => p.operation_sides)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("operation_sides_operation_id_fkey");

                entity.HasOne(d => d.war_side).WithMany(p => p.operation_sides)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("operation_sides_war_side_id_fkey");
            });

            modelBuilder.Entity<Side>(entity =>
            {
                entity.HasKey(e => e.side_id).HasName("sides_pkey");
            });

            modelBuilder.Entity<Theater>(entity =>
            {
                entity.HasKey(e => e.theater_id).HasName("theaters_pkey");

                entity.HasOne(d => d.war).WithMany(p => p.theaters)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("theaters_war_id_fkey");
            });

            modelBuilder.Entity<War>(entity =>
            {
                entity.HasKey(e => e.war_id).HasName("wars_pkey");
            });

            modelBuilder.Entity<WarSide>(entity =>
            {
                entity.HasKey(e => e.war_side_id).HasName("war_sides_pkey");

                entity.HasOne(d => d.side).WithMany(p => p.war_sides)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("war_sides_side_id_fkey");

                entity.HasOne(d => d.war).WithMany(p => p.war_sides)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("war_sides_war_id_fkey");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}