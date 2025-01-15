using Microsoft.EntityFrameworkCore;
using RaceTimings.ProtoActorServer.Entities;

namespace RaceTimings.ProtoActorServer;

public class ApplicationDbContext : DbContext

{
    // Add DbSet properties for each entity
    public DbSet<RaceEntity> Races { get; set; } = null!;
    public DbSet<AthleteEntity> Athletes { get; set; } = null!;
    public DbSet<DeviceEntity> Devices { get; set; } = null!;
    public DbSet<RaceAthleteEntity> RaceAthletes { get; set; } = null!;
    public DbSet<RaceDeviceEntity> RaceDevices { get; set; } = null!;
    public DbSet<RaceAthleteStatsEntity> RaceAthleteStats { get; set; } = null!;
    public DbSet<RaceAthleteResultEntity> RaceAthleteResults { get; set; } = null!;
    public DbSet<RaceAthleteFalseStartEntity> FalseStarts { get; set; } = null!;
    public DbSet<RaceAthleteResultEntity> Splits { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseNpgsql("Host=localhost;Database=races;Username=postgres;Password=<PASSWORD>")
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Add extra configurations here if needed
        modelBuilder.Entity<RaceEntity>(raceBuilder =>
        {

            raceBuilder
                .ToTable("Race");
            raceBuilder
                .HasKey(r => r.Id);
            raceBuilder.Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(100); // Example: Specify maximum length for a Name field

            raceBuilder.Property(r => r.Location)
                .IsRequired()
                .HasMaxLength(200); // Example: Specify maximum length for a Location field

            raceBuilder.Property(r => r.CreatedAt)
                .ValueGeneratedOnAdd();

            raceBuilder.Property(r => r.LastUpdatedAt)
                .ValueGeneratedOnAddOrUpdate();

            // Configure relationships
            raceBuilder.HasMany(r => r.Athletes)
                .WithOne(ra => ra.Race)
                .HasForeignKey(ra => ra.RaceId);

            raceBuilder.HasMany(r => r.Devices)
                .WithOne(rd => rd.Race)
                .HasForeignKey(rd => rd.RaceId);

            raceBuilder.HasMany(r => r.Results)
                .WithOne(rd => rd.Race)
                .HasForeignKey(rd => rd.RaceId);

            raceBuilder.HasMany(r => r.Stats)
                .WithOne(rd => rd.Race)
                .HasForeignKey(rd => rd.RaceId);

            raceBuilder.HasMany(r => r.AthleteSplits)
                .WithOne(rd => rd.Race)
                .HasForeignKey(rd => rd.RaceId);

            raceBuilder.HasMany(r => r.FalseStarts)
                .WithOne(rd => rd.Race)
                .HasForeignKey(rd => rd.RaceId);
        });
        
        modelBuilder.Entity<AthleteEntity>(athleteBuilder =>
        {
           athleteBuilder.ToTable("Athlete");
           athleteBuilder.HasKey(a => a.Id);
           athleteBuilder.Property(a => a.Name)
               .IsRequired()
               .HasMaxLength(100); // Example: Specify maximum length for a Name field
           
           athleteBuilder.Property(a => a.Surname)
               .IsRequired()
               .HasMaxLength(100); // Example: Specify maximum length for a Surname field
           
           athleteBuilder.Property(a => a.CreatedAt).ValueGeneratedOnAdd();
           
           athleteBuilder.Property(a => a.LastUpdatedAt).ValueGeneratedOnAddOrUpdate();
           
           athleteBuilder
               .HasMany(a => a.RaceAthletes)
               .WithOne(ra => ra.Athlete)
               .HasForeignKey(ra => ra.AthleteId);
           
           athleteBuilder
               .HasMany(a => a.RaceResults)
               .WithOne(ra => ra.Athlete)
               .HasForeignKey(ra => ra.AthleteId);
           
           athleteBuilder
               .HasMany(a => a.RaceStats)
               .WithOne(ra => ra.Athlete)
               .HasForeignKey(ra => ra.AthleteId);
           
           athleteBuilder
               .HasMany(a => a.RaceFalseStarts)
               .WithOne(ra => ra.Athlete)
               .HasForeignKey(ra => ra.AthleteId);
               
        });

        modelBuilder.Entity<DeviceEntity>(deviceBuilder =>
        {
            deviceBuilder.ToTable("Device");
            deviceBuilder.HasKey("DeviceId");
            deviceBuilder.Property(x=>x.CreatedAt).ValueGeneratedOnAdd();
            deviceBuilder.Property(x => x.LastUpdatedAt).ValueGeneratedOnAddOrUpdate();
            deviceBuilder.HasMany(d => d.RaceDevices)
                .WithOne(rd => rd.Device)
                .HasForeignKey(rd => rd.DeviceId);
        });

        modelBuilder.Entity<RaceAthleteEntity>(raBuilder =>
        {
            raBuilder.ToTable("RaceAthlete");
            raBuilder.HasKey(e => new { e.RaceId, e.AthleteId });
            raBuilder.Ignore(e => e.Id);
            raBuilder.Property(e => e.CreatedAt).ValueGeneratedOnAdd();
            raBuilder.Property(e => e.LastUpdatedAt).ValueGeneratedOnAddOrUpdate();
            raBuilder.Property(e=>e.Lane).IsRequired();
            
            raBuilder.HasOne<RaceEntity>(e=> e.Race)
                .WithMany(e=>e.Athletes)
                .HasForeignKey(e=>e.RaceId);
            raBuilder.HasOne<AthleteEntity>(e=> e.Athlete)
                .WithMany(e=>e.RaceAthletes)
                .HasForeignKey(e=>e.AthleteId);
            raBuilder.HasOne<RaceAthleteStatsEntity>(e => e.Stats)
                .WithOne(e => e.RaceAthlete)
                .HasForeignKey<RaceAthleteStatsEntity>(e=>new { e.RaceId,e.AthleteId})
                .OnDelete(DeleteBehavior.Cascade);
            raBuilder.HasOne<RaceAthleteResultEntity>(e => e.Result)
                .WithOne(e => e.RaceAthlete)
                .HasForeignKey<RaceAthleteResultEntity>(e=>new { e.RaceId,e.AthleteId})
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RaceAthleteResultEntity>(rarBuilder =>
        {
            rarBuilder.ToTable("RaceAthleteResult");
            rarBuilder.HasKey(e => new { e.RaceId, e.AthleteId });
            rarBuilder.Ignore(e => e.Id);
            rarBuilder.Property(e => e.CreatedAt).ValueGeneratedOnAdd();
            rarBuilder.Property(e => e.LastUpdatedAt).ValueGeneratedOnAddOrUpdate();
            rarBuilder.HasOne<RaceEntity>(e=> e.Race)
                .WithMany(e=>e.Results)
                .HasForeignKey(e=>e.RaceId);
            rarBuilder.HasOne<AthleteEntity>(e=> e.Athlete)
                .WithMany(e=>e.RaceResults)
                .HasForeignKey(e=>e.AthleteId);
            rarBuilder.HasOne<RaceAthleteEntity>(e => e.RaceAthlete)
                .WithOne(e => e.Result)
                .HasForeignKey<RaceAthleteResultEntity>(e=>new { e.RaceId,e.AthleteId})
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RaceAthleteStatsEntity>(rasBuilder =>
        {
            rasBuilder.ToTable("RaceAthleteStats");
            rasBuilder.HasKey(e => new { e.RaceId, e.AthleteId });
            rasBuilder.Ignore(e => e.Id);
            rasBuilder.Property(e => e.CreatedAt).ValueGeneratedOnAdd();
            rasBuilder.Property(e => e.LastUpdatedAt).ValueGeneratedOnAddOrUpdate();
            rasBuilder.HasOne<RaceEntity>(e => e.Race)
                .WithMany(e => e.Stats)
                .HasForeignKey(e => e.RaceId);
            rasBuilder.HasOne<AthleteEntity>(e => e.Athlete)
                .WithMany(e => e.RaceStats)
                .HasForeignKey(e => e.AthleteId);
            rasBuilder.HasOne<RaceAthleteEntity>(e => e.RaceAthlete)
                .WithOne(e => e.Stats)
                .HasForeignKey<RaceAthleteStatsEntity>(e => new { e.RaceId, e.AthleteId })
                .OnDelete(DeleteBehavior.Cascade);
            rasBuilder.HasMany<RaceAthleteFalseStartEntity>(e => e.FalseStarts)
                .WithOne(e => e.RaceAthleteStats)
                .HasForeignKey(e => new { e.RaceId, e.AthleteId });
            rasBuilder.HasMany<RaceAthleteSplitEntity>(e=>e.Splits)
                .WithOne(e=>e.RaceAthleteStats)
                .HasForeignKey(e=>new {e.RaceId,e.AthleteId});
        });

        modelBuilder.Entity<RaceAthleteSplitEntity>(splitBuilder =>
        {
            splitBuilder.ToTable("RaceAthleteSplit");
            splitBuilder.HasKey(e => e.Id);
            splitBuilder.HasOne<AthleteEntity>(e => e.Athlete)
                .WithMany(e => e.Splits)
                .HasForeignKey(e => e.AthleteId);
            splitBuilder.HasOne<RaceEntity>(e => e.Race)
                .WithMany(e => e.AthleteSplits)
                .HasForeignKey(e => e.RaceId);
            splitBuilder.HasOne<RaceAthleteStatsEntity>(e => e.RaceAthleteStats)
                .WithMany(e => e.Splits)
                .HasForeignKey(e => new { e.RaceId, e.AthleteId });
        });
        
        modelBuilder.Entity<RaceAthleteFalseStartEntity>(rafsBuilder =>
        {
            rafsBuilder.ToTable("RaceAthleteFalseStart");
            rafsBuilder.HasKey(e => e.Id);
            rafsBuilder.HasOne<RaceAthleteStatsEntity>(e => e.RaceAthleteStats)
                .WithMany(e => e.FalseStarts)
                .HasForeignKey(e => new { e.RaceId, e.AthleteId });
            rafsBuilder.HasOne<RaceEntity>(e => e.Race)
                .WithMany(e => e.FalseStarts)
                .HasForeignKey(e => e.RaceId);
            rafsBuilder.HasOne<AthleteEntity>(e => e.Athlete)
                .WithMany(e => e.RaceFalseStarts)
                .HasForeignKey(e => e.AthleteId);
        });
    }
}