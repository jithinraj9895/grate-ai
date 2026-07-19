using Domain.Entities;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Rating> Ratings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ================= USER =================
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.FullName)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(200);

            entity.HasIndex(x => x.Email)
                .IsUnique();
        });

        // ================= PRODUCT =================
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("products");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(x => x.Description)
                .IsRequired()
                .HasMaxLength(2000);

            entity.Property(x => x.Price)
                .HasColumnType("numeric(18,2)");

            entity.HasOne(x => x.CreatedByUser)
                .WithMany(x => x.Products)
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ================= REVIEW =================
        modelBuilder.Entity<Review>(entity =>
        {
            entity.ToTable("reviews");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Comment)
                .IsRequired()
                .HasMaxLength(2000);

            entity.HasOne(x => x.User)
                .WithMany(x => x.Reviews)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Product)
                .WithMany(x => x.Reviews)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => new { x.UserId, x.ProductId })
                .IsUnique();
        });

        // ================= RATING =================
        modelBuilder.Entity<Rating>(entity =>
        {
            entity.ToTable("ratings");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Score)
                .IsRequired();

            // ✅ PostgreSQL correct syntax (NO [ ])

            entity.ToTable(t => t.HasCheckConstraint(
                "CK_Rating_Score",
                "[Score] >= 1 AND [Score] <= 5")
            );

            entity.HasOne(x => x.User)
                .WithMany(x => x.Ratings)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Product)
                .WithMany(x => x.Ratings)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => new { x.UserId, x.ProductId })
                .IsUnique();
        });
    }
}