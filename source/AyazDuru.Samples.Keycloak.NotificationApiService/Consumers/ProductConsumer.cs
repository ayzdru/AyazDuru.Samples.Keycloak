using AyazDuru.Samples.Keycloak.NotificationApiService.Data;
using AyazDuru.Samples.Keycloak.NotificationApiService.Entities;
using DotNetCore.CAP;
using Microsoft.EntityFrameworkCore;

namespace AyazDuru.Samples.Keycloak.NotificationApiService.Consumers;

public class ProductConsumer : ICapSubscribe
{
    private readonly NotificationDbContext _db;

    public ProductConsumer(NotificationDbContext db)
    {
        _db = db;
    }

    [CapSubscribe("product.created")]
    public async Task HandleProductCreated(dynamic payload)
    {
        var notification = new Notification
        {
            EventName = "product.created",
            Data = System.Text.Json.JsonSerializer.Serialize(payload),
            CreatedAt = DateTime.Now
        };

        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync();
    }
    [CapSubscribe("product.edited")]
    public async Task HandleProductEdited(dynamic payload)
    {
        var notification = new Notification
        {
            EventName = "product.edited",
            Data = System.Text.Json.JsonSerializer.Serialize(payload),
            UserId = payload?.UserId ?? string.Empty,
            CreatedAt = DateTime.Now
        };

        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync();
    }
    [CapSubscribe("product.deleted")]
    public async Task HandleProductDeleted(dynamic payload)
    {
        var notification = new Notification
        {
            EventName = "product.deleted",
            Data = System.Text.Json.JsonSerializer.Serialize(payload),
            CreatedAt = DateTime.Now
        };

        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync();
    }
}