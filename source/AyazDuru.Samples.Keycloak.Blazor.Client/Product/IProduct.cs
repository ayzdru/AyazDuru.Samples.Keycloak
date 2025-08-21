namespace BlazorWebAppOidc.Client.Product;

public interface IProduct
{
    Task<IEnumerable<Product>> GetProductsAsync();
    Task<Product?> CreateProductAsync(Product product);
    Task<Product?> UpdateProductAsync(Product product);
    Task<bool> DeleteProductAsync(Guid productId);
}
