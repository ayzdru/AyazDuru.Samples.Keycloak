using AyazDuru.Samples.Keycloak.ProductApiService.Entities;
using Microsoft.EntityFrameworkCore;

namespace AyazDuru.Samples.Keycloak.ProductApiService.Data
{
    public class ProductDbContext : DbContext
    {
        public ProductDbContext(DbContextOptions<ProductDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
    }
}
