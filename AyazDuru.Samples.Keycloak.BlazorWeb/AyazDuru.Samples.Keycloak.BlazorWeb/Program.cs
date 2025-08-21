using AyazDuru.Samples.Keycloak.BlazorWeb.Client.Pages;
using AyazDuru.Samples.Keycloak.BlazorWeb.Components;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;

namespace AyazDuru.Samples.Keycloak.BlazorWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var keycloakConfig = builder.Configuration.GetSection("Authentication:Keycloak");
            var scheme = keycloakConfig["Scheme"];
            var clientId = keycloakConfig["ClientId"];
            var clientSecret = keycloakConfig["ClientSecret"];
            var authority = keycloakConfig["Authority"];
            var responseType = keycloakConfig["ResponseType"];
            var requireHttpsMetadata = bool.Parse(keycloakConfig["RequireHttpsMetadata"] ?? "false");
            var scopes = keycloakConfig.GetSection("Scopes").Get<string[]>() ?? Array.Empty<string>();

            var authBuilder = builder.Services.AddAuthentication(scheme);

            authBuilder.AddOpenIdConnect(
                authenticationScheme: scheme,
                options =>
                {
                    options.ClientId = clientId;
                    options.ClientSecret = clientSecret;
                    options.Authority = authority;
                    foreach (var scope in scopes)
                    {
                        options.Scope.Add(scope);
                    }
                    options.ResponseType = responseType;
                    options.SaveTokens = true;
                    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.SignOutScheme = scheme;
                    options.RequireHttpsMetadata = requireHttpsMetadata;
                    options.TokenValidationParameters.NameClaimType = JwtRegisteredClaimNames.Name;
                    options.MapInboundClaims = false;
                }).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

            builder.Services.ConfigureCookieOidc(CookieAuthenticationDefaults.AuthenticationScheme, MS_OIDC_SCHEME);
            builder.Services.AddAuthorization();
            builder.Services.AddCascadingAuthenticationState();

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents()
                .AddInteractiveWebAssemblyComponents()
               .AddAuthenticationStateSerialization(options => options.SerializeAllClaims = true);

            builder.Services.AddHttpContextAccessor();





            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseAntiforgery();

            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode()
                .AddInteractiveWebAssemblyRenderMode()
                .AddAdditionalAssemblies(typeof(Client._Imports).Assembly);

            app.MapGroup("/authentication").MapLoginAndLogout();
            app.Run();
        }
    }
}
