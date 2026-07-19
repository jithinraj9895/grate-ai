using Domain.Entities;

public interface ICommon
{
    Task<Product> CreateProduct(CreateProductDto createProductDto);
    Task<string> CreateBulkProduct(List<CreateProductDto> createProductDto);
    Task<List<Product>> GerSearcherPaginate(string search, int pageNo, int pageSize);

    Task<List<Product>> GetAllProducts();
    Task<List<Product>> GetSearchedProducts(string search);
    Task<int> GetSearchedProductsCount(string search);
    Task<List<Product>> SearchProductsAsync(string name);

    Task<List<Product>> SearchProductWithScroll(string? name,
        int pageSize,
        DateTime? lastCreatedAt,
        Guid? lastId);

    Task<List<Product>> GetSearchPaginateADO(
        string search,
        int pageNo,
        int pageSize);

}