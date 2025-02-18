using Domain.DrivingLicenceAggregate;
using Domain.PhotoAggregate;
using Infrastructure.Adapters.Postgres.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Adapters.Postgres;

public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
{
    public DbSet<DrivingLicense> DrivingLicenses { get; set; }

    public DbSet<Photos> Photos { get; set; }

    public DbSet<OutboxEvent> Outbox { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new DrivingLicenseConfiguration());
        modelBuilder.ApplyConfiguration(new StatusConfiguration());
        modelBuilder.ApplyConfiguration(new PhotoConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxEventConfiguration());


        modelBuilder.Entity<Status>().HasData(Status.All());
    }
}

public class DrivingLicenseConfiguration : IEntityTypeConfiguration<DrivingLicense>
{
    public void Configure(EntityTypeBuilder<DrivingLicense> builder)
    {
        builder.ToTable("driving_license");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.AccountId).HasColumnName("account_id").IsRequired();
        builder.Property(x => x.DateOfBirth).HasColumnName("date_of_birth").IsRequired();
        builder.Property(x => x.DateOfIssue).HasColumnName("date_of_issue").IsRequired();
        builder.Property(x => x.DateOfExpiry).HasColumnName("date_of_expiry").IsRequired();

        builder.HasOne(x => x.Status)
            .WithMany()
            .HasForeignKey("status_id")
            .HasConstraintName("FK_status_id")
            .IsRequired();

        // builder.HasOne<Photos>(x => x.Id)
        //     .WithMany()
        //     .HasForeignKey(x => x.Id)
        //     .HasConstraintName("FK_photos_id")
        //     .IsRequired(false);

        builder.OwnsOne(x => x.CategoryList,
            cfg =>
            {
                cfg.Property(x => x.Categories)
                    .HasField("_categories")
                    .HasConversion<char[]>()
                    .HasColumnName("categories");
            });

        builder.OwnsOne(x => x.Name, cfg =>
        {
            cfg.Property(x => x.FirstName).HasColumnName("first_name").IsRequired();
            cfg.Property(x => x.LastName).HasColumnName("last_name").IsRequired();
            cfg.Property(x => x.Patronymic).HasColumnName("patronymic").IsRequired(false);
        });

        builder.OwnsOne(x => x.CityOfBirth,
            cfg => { cfg.Property(x => x.Name).HasColumnName("city_of_birth").IsRequired(); });

        builder.OwnsOne(x => x.Number, cfg => { cfg.Property(x => x.Value).HasColumnName("number").IsRequired(); });

        builder.OwnsOne(x => x.CodeOfIssue,
            cfg => { cfg.Property(x => x.Value).HasColumnName("code_of_issue").IsRequired(); });

        builder.Ignore(x => x.DomainEvents);
    }
}

public class StatusConfiguration : IEntityTypeConfiguration<Status>
{
    public void Configure(EntityTypeBuilder<Status> builder)
    {
        builder.ToTable("status");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever().IsRequired();
        builder.Property(x => x.Name).HasColumnName("name").IsRequired();
    }
}

public class PhotoConfiguration : IEntityTypeConfiguration<Photos>
{
    public void Configure(EntityTypeBuilder<Photos> builder)
    {
        builder.ToTable("photos");

        builder.HasKey(x => x.Id);
        builder.HasOne<DrivingLicense>()
            .WithMany()
            .HasForeignKey(x => x.DrivingLicenseId)
            .HasConstraintName("FK_driving_license_id")
            .IsRequired(false);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.DrivingLicenseId).HasColumnName("driving_license_id").IsRequired();
        builder.Property(x => x.FrontPhotoStorageBucketAndKey)
            .HasColumnName("front_photo_storage_bucket_and_key")
            .IsRequired();
        builder.Property(x => x.BackPhotoStorageBucketWithKey)
            .HasColumnName("back_photo_storage_bucket_and_key")
            .IsRequired();
        
        builder.Ignore(x => x.DomainEvents);
    }
}

public class OutboxEventConfiguration : IEntityTypeConfiguration<OutboxEvent>
{
    public void Configure(EntityTypeBuilder<OutboxEvent> builder)
    {
        builder.ToTable("outbox");

        builder.HasKey(x => x.EventId);

        builder.Property(x => x.EventId).HasColumnName("event_id").IsRequired();

        builder.Property(x => x.Type).HasColumnName("type").IsRequired();
        builder.Property(x => x.Content).HasColumnName("content").IsRequired();
        builder.Property(x => x.OccurredOnUtc).HasColumnName("occurred_on_utc").IsRequired();
        builder.Property(x => x.ProcessedOnUtc).HasColumnName("processed_on_utc").IsRequired(false);
    }
}
