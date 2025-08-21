using BlazorWebAppOidc.Client.Product;
using System.Net.Http.Json;

namespace BlazorWebAppOidc.Product;

internal sealed class ServerProduct(IHttpClientFactory clientFactory) : IProduct
{
    public async Task<Client.Product.Product> CreateProductAsync(Client.Product.Product product)
    {
        var client = clientFactory.CreateClient("ProductApi");
        var response = await client.PostAsJsonAsync("/products", product);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Client.Product.Product>()
            ?? throw new IOException("Failed to create product!");
    }

    public async Task<bool> DeleteProductAsync(Guid productId)
    {
        var client = clientFactory.CreateClient("ProductApi");
        var response = await client.DeleteAsync($"/products/{productId}");
        return response.IsSuccessStatusCode;
    }

    public async Task<IEnumerable<Client.Product.Product>> GetProductsAsync()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/products");
        var client = clientFactory.CreateClient("ProductApi");
        using var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Client.Product.Product[]>() ??
            throw new IOException("No products!");
    }

    public async Task<Client.Product.Product> UpdateProductAsync(Client.Product.Product product)
    {
        var client = clientFactory.CreateClient("ProductApi");
        var response = await client.PutAsJsonAsync($"/products/{product.Id}", product);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Client.Product.Product>()
            ?? throw new IOException("Failed to update product!");
    }
}
