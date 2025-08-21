namespace BlazorWebAppOidc.Client.Product;

public sealed class Product
{
    public Guid Id { get; set; }
    public string UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}
