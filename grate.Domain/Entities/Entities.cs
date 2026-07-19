using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    public abstract class BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
    }

    // ================= USER =================
    public class User : BaseEntity
    {
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public required string PasswordHash { get; set; }
        public string? PhoneNumber { get; set; }

        public ICollection<Product> Products { get; set; } = new List<Product>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
    }

    // ================= PRODUCT =================
    public class Product : BaseEntity
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }

        public Guid CreatedByUserId { get; set; }
        public required User CreatedByUser { get; set; }

        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
    }

    // ================= REVIEW =================
    public class Review : BaseEntity
    {
        public string? Comment { get; set; }

        public Guid UserId { get; set; }
        public Guid ProductId { get; set; }

        public required User User { get; set; }
        public required Product Product { get; set; }
    }

    // ================= RATING =================
    public class Rating : BaseEntity
    {
        public int Score { get; set; }  // 1 - 5

        public Guid UserId { get; set; }
        public Guid ProductId { get; set; }

        public required User User { get; set; }
        public required Product Product { get; set; }
    }
}