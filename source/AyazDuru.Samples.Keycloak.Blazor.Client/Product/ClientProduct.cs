using System.Net.Http.Json;

namespace AyazDuru.Samples.Keycloak.Blazor.Client.Product;

internal sealed class ClientProduct(HttpClient httpClient) : IProduct
{
    public async Task<IEnumerable<Product>> GetProductsAsync() =>
        await httpClient.GetFromJsonAsync<Product[]>("api/products") ??
            throw new IOException("No products!");

    public async Task<Product?> CreateProductAsync(Product product)
    {
        var response = await httpClient.PostAsJsonAsync("api/products", product);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Product>();
    }

    public async Task<Product?> UpdateProductAsync(Product product)
    {
        var response = await httpClient.PutAsJsonAsync($"api/products/{product.Id}", product);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Product>();
    }

    public async Task<bool> DeleteProductAsync(Guid productId)
    {
        var response = await httpClient.DeleteAsync($"api/products/{productId}");
        return response.IsSuccessStatusCode;
    }
}
