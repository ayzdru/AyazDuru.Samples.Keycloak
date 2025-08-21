namespace AyazDuru.Samples.Keycloak.ProductApiService.Entities
{
    public class Product
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
}
