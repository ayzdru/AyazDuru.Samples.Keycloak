using Microsoft.EntityFrameworkCore;
using AyazDuru.Samples.Keycloak.NotificationApiService.Entities;

namespace AyazDuru.Samples.Keycloak.NotificationApiService.Data
{
    public class NotificationDbContext : DbContext
    {
        public NotificationDbContext(DbContextOptions<NotificationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Notification> Notifications { get; set; }
    }
}