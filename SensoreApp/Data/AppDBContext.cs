using Microsoft.EntityFrameworkCore;
using SensoreApp.Models;

namespace SensoreApp.Data
{
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options)
            : base(options)
        {
        }

        public DbSet<FrameMetric> FrameMetrics { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<ReportMetric> ReportMetrics { get; set; }
        public DbSet<ReportFrame> ReportFrames { get; set; }
        public DbSet<Alert> Alerts { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Clinician> Clinicians { get; set; }
        public DbSet<ThresholdSettings> ThresholdSettings { get; set; }
        public DbSet<Patient> Patients { get; set; }

        // Tables my teammates will add later (commented out for now)
       
        // public DbSet<SensorFrame> SensorFrames { get; set; }
        // public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            

            // Configure FrameMetric
            modelBuilder.Entity<FrameMetric>(entity =>
            {
                entity.HasKey(e => e.FrameMetricID);

                // Set precise decimal types for pressure data
                entity.Property(e => e.PeakPressureIndex)
                    .HasColumnType("decimal(10,4)");

                entity.Property(e => e.ContactAreaPercent)
                    .HasColumnType("decimal(5,2)");

                entity.Property(e => e.COV)
                    .HasColumnType("decimal(10,4)");

                entity.Property(e => e.ComputedAt)
                    .HasDefaultValueSql("GETDATE()");
            });

            // Configure Report
            modelBuilder.Entity<Report>(entity =>
            {
                entity.HasKey(e => e.ReportID);

                entity.Property(e => e.ReportType)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.FilePath)
                    .HasMaxLength(1000);

                entity.Property(e => e.GeneratedAt)
                    .HasDefaultValueSql("GETDATE()");

                // One Report has many ReportMetric
                entity.HasMany(e => e.ReportMetric)
                    .WithOne(e => e.Report)
                    .HasForeignKey(e => e.ReportID)
                    .OnDelete(DeleteBehavior.Cascade);

                // One Report has many ReportFrame
                entity.HasMany(e => e.ReportFrame)
                    .WithOne(e => e.Report)
                    .HasForeignKey(e => e.ReportID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure ReportMetric
            modelBuilder.Entity<ReportMetric>(entity =>
            {
                entity.HasKey(e => e.ReportMetricID);

                entity.Property(e => e.MetricName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.MetricValue)
                    .HasColumnType("decimal(10,2)");

                entity.Property(e => e.ComparisonValue)
                    .HasColumnType("decimal(10,2)");
            });

            // Configure ReportFrame
            modelBuilder.Entity<ReportFrame>(entity =>
            {
                entity.HasKey(e => e.ReportFrameID);
            });

            // Configure Alert
            modelBuilder.Entity<Alert>(entity =>
            {
                entity.HasKey(e => e.AlertId);

                // Foreign key to User
                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // TriggeringFrameId is a FK to FrameMetric (or SensorFrame later)
                entity.Property(e => e.TriggeringFrameId)
                    .IsRequired();

                entity.Property(e => e.Reason)
                    .HasMaxLength(500); // optional, limit text length

                entity.Property(e => e.TriggerValue)
                    .HasColumnType("real"); // maps float to SQL Server 'real'

                entity.Property(e => e.ThresholdPct)
                    .HasColumnType("decimal(5,2)");

                entity.Property(e => e.StartTime)
                    .IsRequired();

                entity.Property(e => e.EndTime)
                    .IsRequired(false);

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasDefaultValue("New");

                entity.Property(e => e.AcknowledgedAt)
                    .IsRequired(false);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETDATE()");
            });


            // Configure User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.Phone)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.IsActive)
                    .IsRequired();
            });
            modelBuilder.Entity<User>()
              .HasDiscriminator<string>("Discriminator")
              .HasValue<User>("User")
              .HasValue<Clinician>("Clinician")
              .HasValue<Patient>("Patient");  

            modelBuilder.Entity<Clinician>(entity =>
            {
                entity.Property(e => e.WorkEmail)
                    .IsRequired()
                    .HasMaxLength(255);
                entity.Property(e => e.PersonalEmail)
                    .IsRequired()
                    .HasMaxLength(255);
            });

            // NEW: Configure Patient-specific properties
            modelBuilder.Entity<Patient>(entity =>
            {
                entity.Property(e => e.DateOfBirth)
                .IsRequired(false); // Optional field
            });
            modelBuilder.Entity<Report>()
       .HasOne(r => r.RequestedByUser)
       .WithMany()
       .HasForeignKey(r => r.RequestedBy)
       .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

            modelBuilder.Entity<Report>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserID)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

            base.OnModelCreating(modelBuilder);
        }
    }
    }

