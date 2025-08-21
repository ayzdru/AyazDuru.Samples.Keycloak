
using AyazDuru.Samples.Keycloak.NotificationApiService.Consumers;
using AyazDuru.Samples.Keycloak.NotificationApiService.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

namespace AyazDuru.Samples.Keycloak.NotificationApiService;

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
        builder.Services.AddDbContext<NotificationDbContext>((options) =>
        {
            options.UseSqlServer(sqlServerConnectionString);
        });
        builder.Services.AddScoped<ProductConsumer>();
       
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

        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
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


        app.MapControllers();

        app.Run();
    }
}
