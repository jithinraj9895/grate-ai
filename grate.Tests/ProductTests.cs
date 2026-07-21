using System;
using System.Collections.Generic;
using Xunit;
using FluentAssertions;
using Domain.Entities;

namespace grate.Tests
{
    public class ProductTests
    {
        private readonly User _testUser;

        public ProductTests()
        {
            _testUser = new User
            {
                FullName = "Creator User",
                Email = "creator@example.com",
                PasswordHash = "hash"
            };
        }

        [Fact]
        public void Product_ShouldBeCreatedWithRequiredProperties()
        {
            // Arrange
            var name = "Test Product";
            var price = 99.99m;
            var stockQuantity = 50;

            // Act
            var product = new Product
            {
                Name = name,
                Price = price,
                StockQuantity = stockQuantity,
                CreatedByUserId = _testUser.Id,
                CreatedByUser = _testUser
            };

            // Assert
            product.Name.Should().Be(name);
            product.Price.Should().Be(price);
            product.StockQuantity.Should().Be(stockQuantity);
            product.CreatedByUserId.Should().Be(_testUser.Id);
            product.CreatedByUser.Should().Be(_testUser);
        }

        [Fact]
        public void Product_ShouldAllowOptionalDescription()
        {
            // Arrange
            var description = "A great product";

            // Act
            var product = new Product
            {
                Name = "Product",
                Description = description,
                Price = 49.99m,
                StockQuantity = 10,
                CreatedByUserId = _testUser.Id,
                CreatedByUser = _testUser
            };

            // Assert
            product.Description.Should().Be(description);
        }

        [Fact]
        public void Product_DescriptionShouldBeNullByDefault()
        {
            // Arrange & Act
            var product = new Product
            {
                Name = "Product",
                Price = 49.99m,
                StockQuantity = 10,
                CreatedByUserId = _testUser.Id,
                CreatedByUser = _testUser
            };

            // Assert
            product.Description.Should().BeNull();
        }

        [Fact]
        public void Product_ShouldHaveEmptyReviewsCollectionByDefault()
        {
            // Arrange & Act
            var product = new Product
            {
                Name = "Product",
                Price = 49.99m,
                StockQuantity = 10,
                CreatedByUserId = _testUser.Id,
                CreatedByUser = _testUser
            };

            // Assert
            product.Reviews.Should().NotBeNull();
            product.Reviews.Should().BeEmpty();
        }

        [Fact]
        public void Product_ShouldHaveEmptyRatingsCollectionByDefault()
        {
            // Arrange & Act
            var product = new Product
            {
                Name = "Product",
                Price = 49.99m,
                StockQuantity = 10,
                CreatedByUserId = _testUser.Id,
                CreatedByUser = _testUser
            };

            // Assert
            product.Ratings.Should().NotBeNull();
            product.Ratings.Should().BeEmpty();
        }

        [Fact]
        public void Product_ShouldSupportZeroPrice()
        {
            // Arrange & Act
            var product = new Product
            {
                Name = "Free Product",
                Price = 0m,
                StockQuantity = 10,
                CreatedByUserId = _testUser.Id,
                CreatedByUser = _testUser
            };

            // Assert
            product.Price.Should().Be(0m);
        }

        [Fact]
        public void Product_ShouldSupportZeroStockQuantity()
        {
            // Arrange & Act
            var product = new Product
            {
                Name = "Out of Stock Product",
                Price = 99.99m,
                StockQuantity = 0,
                CreatedByUserId = _testUser.Id,
                CreatedByUser = _testUser
            };

            // Assert
            product.StockQuantity.Should().Be(0);
        }

        [Fact]
        public void Product_ShouldInheritFromBaseEntity()
        {
            // Arrange & Act
            var product = new Product
            {
                Name = "Product",
                Price = 49.99m,
                StockQuantity = 10,
                CreatedByUserId = _testUser.Id,
                CreatedByUser = _testUser
            };

            // Assert
            product.Should().BeAssignableTo<BaseEntity>();
            product.CreatedAt.Should().NotBe(default(DateTime));
            product.IsDeleted.Should().BeFalse();
        }
    }
}
