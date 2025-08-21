using AyazDuru.Samples.Keycloak.ProductApiService.Data;
using AyazDuru.Samples.Keycloak.ProductApiService.Entities;
using AyazDuru.Samples.Keycloak.ProductApiService.Models;
using DotNetCore.CAP;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using System;
namespace AyazDuru.Samples.Keycloak.ProductApiService;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        // appsettings.json'dan ayarlarý çekiyoruz
        var jwtSettings = builder.Configuration.GetSection("JwtBearer");

        // JWT Bearer authentication ekle
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = jwtSettings["Authority"];
                options.Audience = jwtSettings["Audience"];
                options.RequireHttpsMetadata = bool.Parse(jwtSettings["RequireHttpsMetadata"] ?? "true");
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    // Keycloak tokenlarýnda "aud" (audience) kontrolü için
                    ValidateAudience = true
                };
            });

        builder.Services.AddAuthorizationBuilder();

        var sqlServerConnectionString = builder.Configuration.GetConnectionString("SQLServer");
        builder.Services.AddDbContext<ProductDbContext>((options) =>
        {
            options.UseSqlServer(sqlServerConnectionString);
        });

        builder.Services.AddCap(x =>
        {
            x.UseSqlServer(sqlServerConnectionString);
            x.UseRabbitMQ(options =>
            {
                options.HostName = builder.Configuration["CAP:RabbitMQ:HostName"];
                options.Port = int.Parse(builder.Configuration["CAP:RabbitMQ:Port"] ?? "5672");
                options.UserName = builder.Configuration["CAP:RabbitMQ:UserName"];
                options.Password = builder.Configuration["CAP:RabbitMQ:Password"];
            });
            x.UseDashboard(); // CAP Dashboard'u ekleyin (opsiyonel)
        });

        builder.AddServiceDefaults();

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
            dbContext.Database.Migrate();
        }

        app.MapDefaultEndpoints();

        app.MapOpenApi();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/openapi/v1.json", "v1");

        });

        //app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapGet("/products", [Authorize] async (ProductDbContext db) =>
        {
            return await db.Products.ToListAsync();
        });

        app.MapGet("/products/{id}", [Authorize] async (Guid id, ProductDbContext db) =>
        {
            var product = await db.Products.FindAsync(id);
            return product is not null ? Results.Ok(product) : Results.NotFound();
        });

        app.MapPost("/products", [Authorize] async (ProductModel model, ProductDbContext db, ICapPublisher capPublisher, HttpContext httpContext) =>
        {
            var userId = httpContext.User.FindFirst("sub")?.Value; // Or use another claim if needed

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = model.Name,
                Price = model.Price,
                UserId = userId,
                CreatedAt = DateTime.Now
            };

            db.Products.Add(product);
            await db.SaveChangesAsync();

            await capPublisher.PublishAsync("product.created", new
            {
                product.Id,
                product.Name,
                product.Price,
                product.UserId,
                product.CreatedAt
            });

            return Results.Created($"/products/{product.Id}", product);
        });

        app.MapPut("/products/{id}", [Authorize] async (Guid id, ProductModel model, ProductDbContext db, ICapPublisher capPublisher) =>
        {
            var product = await db.Products.FindAsync(id);
            if (product is null) return Results.NotFound();

            product.Name = model.Name;
            product.Price = model.Price;

            await db.SaveChangesAsync();

            await capPublisher.PublishAsync("product.edited", new
            {
                product.Id,
                product.Name,
                product.Price,
                product.UserId,
                product.CreatedAt
            });

            return Results.Ok(product);
        });

        app.MapDelete("/products/{id}", [Authorize] async (Guid id, ProductDbContext db, ICapPublisher capPublisher) =>
        {
            var product = await db.Products.FindAsync(id);
            if (product is null) return Results.NotFound();

            db.Products.Remove(product);
            await db.SaveChangesAsync();

            await capPublisher.PublishAsync("product.deleted", new
            {
                product.Id,
                product.Name,
                product.Price,
                product.UserId,
                product.CreatedAt
            });

            return Results.NoContent();
        });



        app.Run();
    }
}
