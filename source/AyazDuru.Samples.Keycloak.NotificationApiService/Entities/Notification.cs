using System;

namespace AyazDuru.Samples.Keycloak.NotificationApiService.Entities
{
    public class Notification
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string EventName { get; set; } = string.Empty;
        public string Data { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}