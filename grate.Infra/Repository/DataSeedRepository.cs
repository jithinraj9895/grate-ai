using Bogus;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

public class DataSeedRepository
{
    private readonly AppDbContext _context;

    public DataSeedRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync(int userCount, int productPerUser)
    {
        // ---------------- USERS ----------------

        var userFaker = new Faker<User>()
            .RuleFor(x => x.FullName, f => f.Name.FullName())
            .RuleFor(x => x.Email, f => f.Internet.Email())
            .RuleFor(x => x.PasswordHash, f => BCrypt.Net.BCrypt.HashPassword("Password@123"))
            .RuleFor(x => x.PhoneNumber, f => f.Phone.PhoneNumber());

        var users = userFaker.Generate(userCount);

        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();

        // ---------------- PRODUCTS ----------------

        var products = new List<Product>();

        foreach (var user in users)
        {
            int productCount = Random.Shared.Next(5, productPerUser);

            var productFaker = new Faker<Product>()
                .RuleFor(x => x.Name, f => f.Commerce.ProductName())
                .RuleFor(x => x.Description, f => f.Commerce.ProductDescription())
                .RuleFor(x => x.Price, f => decimal.Parse(f.Commerce.Price(100, 5000)))
                .RuleFor(x => x.StockQuantity, f => f.Random.Int(0, 1000))
                .RuleFor(x => x.CreatedByUserId, user.Id)
                .RuleFor(x => x.CreatedByUser, user);

            products.AddRange(productFaker.Generate(productCount));
        }

        await _context.Products.AddRangeAsync(products);
        await _context.SaveChangesAsync();
    }

    public async Task SeedReviewsAsync(int userCount, int productCount)
    {
        var users = await _context.Users
            .OrderBy(x => Guid.NewGuid())
            .Take(userCount)
            .ToListAsync();

        var products = await _context.Products
            .OrderBy(x => Guid.NewGuid())
            .Take(productCount)
            .ToListAsync();

        var faker = new Faker();
        var reviews = new List<Review>();

        // Prevent duplicate (UserId, ProductId)
        var usedPairs = new HashSet<string>();

        foreach (var product in products)
        {
            int reviewCount = Random.Shared.Next(20, 101);

            for (int i = 0; i < reviewCount; i++)
            {
                var user = users[Random.Shared.Next(users.Count)];

                var key = $"{user.Id}-{product.Id}";

                // skip duplicate review for same user-product
                if (!usedPairs.Add(key))
                    continue;

                reviews.Add(new Review
                {
                    UserId = user.Id,
                    User = user,

                    ProductId = product.Id,
                    Product = product,

                    Comment = faker.Lorem.Sentences(Random.Shared.Next(1, 4))
                });
            }
        }

        await _context.Reviews.AddRangeAsync(reviews);
        await _context.SaveChangesAsync();
    }
}