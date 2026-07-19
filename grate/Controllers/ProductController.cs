using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ProductController(ICommon common, IUnitOfWork unitOfWork, IADO ado, RabbitMqPublisher _publisher) : ControllerBase
{
    [HttpGet]
    public IActionResult HealthCheck()
    {
        return Ok("This is a health check");
    }

    [HttpPost]
    public async Task<IActionResult> AddProduct([FromBody] CreateProductDto createProductDto)
    {
        var product = await common.CreateProduct(createProductDto);
        await unitOfWork.SaveChangesAsync();

        await _publisher.PublishAsync(new RabbitMqPublisher.ProductEmbeddingMessage
        {
            ProductId = product.Id,
            Name = product.Name,
            Description = product.Description
        });

        var response = new ProductResponseDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            StockQuantity = product.StockQuantity
        };

        return Ok(response);
    }

    [HttpPost("bulk")]
    public async Task<IActionResult> AddBulkProduct([FromBody] List<CreateProductDto> createProductDto)
    {
        var product = await common.CreateBulkProduct(createProductDto);
        await unitOfWork.SaveChangesAsync();
        return Ok(product);
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await common.GetAllProducts());
    }


    [HttpGet("search")]
    public async Task<IActionResult> GetSeached([FromQuery] string search)
    {
        return Ok(await common.GetSearchedProducts(search));
    }


    [HttpGet("searchA")]
    public async Task<IActionResult> GetSeachedA([FromQuery] string search)
    {
        return Ok(await common.SearchProductsAsync(search));
    }


    [HttpGet("searchoptim")]
    public async Task<IActionResult> GetSeached(
     [FromQuery] string search,
     [FromQuery] int pageNo,
     [FromQuery] int pageSize)
    {
        int count = await common.GetSearchedProductsCount(search);

        var products = await common.GerSearcherPaginate(
            search,
            pageNo,
            pageSize);

        return Ok(new
        {
            Items = products,
            TotalCount = count,
            PageNo = pageNo,
            PageSize = pageSize
        });
    }

    [HttpGet("search-scroll")]
    public async Task<IActionResult> SearchWithScroll(
        string? name,
        int pageSize = 20,
        DateTime? lastCreatedAt = null,
        Guid? lastId = null)
    {
        if (pageSize < 1 || pageSize > 100)
        {
            return BadRequest("pageSize must be between 1 and 100.");
        }

        var products = await common.SearchProductWithScroll(
            name,
            pageSize,
            lastCreatedAt,
            lastId);

        var lastProduct = products.LastOrDefault();

        return Ok(new
        {
            items = products,
            hasMore = products.Count == pageSize,
            nextCursor = lastProduct is null
                ? null
                : new
                {
                    lastCreatedAt = lastProduct.CreatedAt,
                    lastId = lastProduct.Id
                }
        });
    }


    [HttpGet("searchoptimA")]
    public async Task<IActionResult> GetSeachedA(
     [FromQuery] string search,
     [FromQuery] int pageNo,
     [FromQuery] int pageSize)
    {
        int count = await common.GetSearchedProductsCount(search);

        var products = await common.GetSearchPaginateADO(
            search,
            pageNo,
            pageSize);

        return Ok(new
        {
            Items = products,
            TotalCount = count,
            PageNo = pageNo,
            PageSize = pageSize
        });
    }
}