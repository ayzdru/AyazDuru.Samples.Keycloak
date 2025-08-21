using AyazDuru.Samples.Keycloak.NotificationApiService.Data;
using AyazDuru.Samples.Keycloak.NotificationApiService.Entities;
using AyazDuru.Samples.Keycloak.NotificationApiService.Models;
using DotNetCore.CAP;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AyazDuru.Samples.Keycloak.NotificationApiService.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly NotificationDbContext _db;

    public NotificationController(NotificationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Notification>>> GetNotifications()
    {
        return await _db.Notifications.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Notification>> GetNotification(Guid id)
    {
        var notification = await _db.Notifications.FindAsync(id);
        if (notification == null)
            return NotFound();
        return notification;
    }

    [HttpPost]
    public async Task<ActionResult<Notification>> CreateNotification([FromBody] NotificationModel model)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            EventName = model.EventName,
            Data = model.Data,
            CreatedAt = DateTime.Now,
            UserId = User.FindFirst("sub")?.Value ?? string.Empty
        };

        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync();


        return CreatedAtAction(nameof(GetNotification), new { id = notification.Id }, notification);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Notification>> UpdateNotification(Guid id, [FromBody] NotificationModel model)
    {
        var notification = await _db.Notifications.FindAsync(id);
        if (notification == null)
            return NotFound();

        notification.EventName = model.EventName;
        notification.Data = model.Data;

        await _db.SaveChangesAsync();


        return Ok(notification);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNotification(Guid id)
    {
        var notification = await _db.Notifications.FindAsync(id);
        if (notification == null)
            return NotFound();

        _db.Notifications.Remove(notification);
        await _db.SaveChangesAsync();


        return NoContent();
    }
}