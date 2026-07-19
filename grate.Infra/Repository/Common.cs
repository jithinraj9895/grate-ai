using System.Data;
using Domain.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

public class Common(AppDbContext context, IADO ado) : ICommon
{
    public async Task<string> CreateBulkProduct(List<CreateProductDto> listOfProducts)
    {
        List<User> users = await context.Users.Where
        (x => listOfProducts.Select(p => p.CreatedByUserId).Contains(x.Id)).ToListAsync();

        foreach (var prod in listOfProducts)
        {
            var product = new Product
            {
                Name = prod.Name,
                Description = prod.Description,
                Price = prod.Price,
                StockQuantity = prod.StockQuantity,
                CreatedByUser = users[Random.Shared.Next(users.Count)]
            };

            await context.Products.AddAsync(product);
        }

        return "done";
    }
    public async Task<Product> CreateProduct(CreateProductDto createProductDto)
    {
        var user = context.Users.First(x => x.Id == createProductDto.CreatedByUserId);
        var product = new Product
        {
            Name = createProductDto.Name,
            Description = createProductDto.Description,
            Price = createProductDto.Price,
            StockQuantity = createProductDto.StockQuantity,
            CreatedByUser = user
        };

        await context.Products.AddAsync(product);
        return product;
    }

    public async Task<List<Product>> GerSearcherPaginate(string search, int pageNo, int pageSize)
    {
        if (pageNo < 0)
        {
            throw new ArgumentOutOfRangeException("Negative number not good as param");
        }
        var prods = await context.Products.AsNoTracking().Where(x => x.Name.Contains(search)).OrderBy(x => x.Name)
        .Skip((pageNo - 1) * pageSize).Take(pageSize).ToListAsync();

        return prods;
    }

    public async Task<List<Product>> GetSearchPaginateADO(
        string search,
        int pageNo,
        int pageSize)
    {
        if (pageNo <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageNo));
        }

        var offset = (pageNo - 1) * pageSize;

        const string sql = @"
        SELECT
            Id,
            Name,
            Description,
            Price,
            StockQuantity,
            CreatedByUserId
        FROM Products
        WHERE Name LIKE @Search
        ORDER BY Name
        OFFSET @Offset ROWS
        FETCH NEXT @PageSize ROWS ONLY";

        return await ado.QueryAsync(
            sql,
            CommandType.Text
            ,
            reader => new Product
            {
                Id = reader.GetGuid(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("Description")),
                Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                StockQuantity = reader.GetInt32(reader.GetOrdinal("StockQuantity")),
                CreatedByUserId = reader.GetGuid(reader.GetOrdinal("CreatedByUserId")),

                // Navigation property not loaded here
                CreatedByUser = null!
            },
            new SqlParameter("@Search", $"%{search}%"),
            new SqlParameter("@Offset", offset),
            new SqlParameter("@PageSize", pageSize)
        );
    }

    public async Task<List<Product>> GetAllProducts()
    {
        return await context.Products.ToListAsync();
    }

    public async Task<List<Product>> GetSearchedProducts(string search)
    {
        var res = await context.Products.Where(x => x.Name.Contains(search)).ToListAsync();
        return res;
    }

    public async Task<int> GetSearchedProductsCount(string search)
    {
        return await context.Products.Where(x => x.Name.Contains(search)).CountAsync();
    }

    public async Task<List<Product>> SearchProductsAsync(string name)
    {
        const string sql = @"
        SELECT
            Id,
            Name,
            Description,
            Price,
            StockQuantity,
            CreatedByUserId
        FROM Products
        WHERE Name LIKE @Name";

        return await ado.QueryAsync(
            sql,
            CommandType.Text,
            reader => new Product
            {
                Id = reader.GetGuid(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("Description")),
                Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                StockQuantity = reader.GetInt32(reader.GetOrdinal("StockQuantity")),
                CreatedByUserId = reader.GetGuid(reader.GetOrdinal("CreatedByUserId")),

                // required property
                CreatedByUser = null! // populate separately if needed
            },
            new SqlParameter("@Name", $"%{name}%")
        );
    }

    public async Task<List<Product>> SearchProductWithScroll(
        string? name,
        int pageSize,
        DateTime? lastCreatedAt,
        Guid? lastId)
    {
        const string sql = @"
        SELECT TOP (@PageSize)
            Id,
            Name,
            Description,
            Price,
            StockQuantity,
            CreatedByUserId,
            CreatedAt
        FROM Products
        WHERE Name LIKE @Name
          AND
          (
              @LastCreatedAt IS NULL
              OR CreatedAt < @LastCreatedAt
              OR (CreatedAt = @LastCreatedAt AND Id < @LastId)
          )
        ORDER BY CreatedAt DESC, Id DESC;";

        return await ado.QueryAsync(
            sql,
            CommandType.Text,
            reader => new Product
            {
                Id = reader.GetGuid(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("Description")),
                Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                StockQuantity = reader.GetInt32(reader.GetOrdinal("StockQuantity")),
                CreatedByUserId = reader.GetGuid(reader.GetOrdinal("CreatedByUserId")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                CreatedByUser = null!
            },
            new SqlParameter("@PageSize", SqlDbType.Int)
            {
                Value = pageSize
            },
            new SqlParameter("@Name", SqlDbType.NVarChar, 200)
            {
                Value = $"%{name ?? string.Empty}%"
            },
            new SqlParameter("@LastCreatedAt", SqlDbType.DateTime2)
            {
                Value = lastCreatedAt ?? (object)DBNull.Value
            },
            new SqlParameter("@LastId", SqlDbType.UniqueIdentifier)
            {
                Value = lastId ?? (object)DBNull.Value
            }
        );
    }
}