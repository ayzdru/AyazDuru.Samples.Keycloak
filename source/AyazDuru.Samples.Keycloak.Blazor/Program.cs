using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using BlazorWebAppOidc;
using BlazorWebAppOidc.Components;
using System.IdentityModel.Tokens.Jwt;
using BlazorWebAppOidc.Client.Product;
using BlazorWebAppOidc.Product;


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


builder.Services.ConfigureCookieOidc(CookieAuthenticationDefaults.AuthenticationScheme, scheme);

builder.Services.AddAuthorization();

builder.Services.AddCascadingAuthenticationState();
builder.AddServiceDefaults();
// Remove or set 'SerializeAllClaims' to 'false' if you only want to 
// serialize name and role claims for CSR.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization(options => options.SerializeAllClaims = true);

builder.Services.AddScoped<IProduct, ServerProduct>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<TokenHandler>();

builder.Services.AddHttpClient("ProductApi",
      client => client.BaseAddress = new Uri(builder.Configuration["ProductApiUri"] ??
          throw new Exception("Missing base address!")))
      .AddHttpMessageHandler<TokenHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.MapStaticAssets();
app.UseAntiforgery();

// Product API group
var productsApi = app.MapGroup("/api/products").RequireAuthorization();

productsApi.MapGet("/", async ([FromServices] IProduct product) =>
{
    return await product.GetProductsAsync();
});

productsApi.MapPost("/", async ([FromServices] IProduct product, [FromBody] BlazorWebAppOidc.Client.Product.Product newProduct) =>
{
    return await product.CreateProductAsync(newProduct);
});

productsApi.MapPut("/{id:guid}", async ([FromServices] IProduct product, Guid id, [FromBody] BlazorWebAppOidc.Client.Product.Product updatedProduct) =>
{
    updatedProduct.Id = id;
    return await product.UpdateProductAsync(updatedProduct);
});

productsApi.MapDelete("/{id:guid}", async ([FromServices] IProduct product, Guid id) =>
{
    return await product.DeleteProductAsync(id);
});
app.MapDefaultEndpoints();
// Authentication endpoints
app.MapGroup("/authentication").MapLoginAndLogout(scheme);

// Blazor endpoints
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BlazorWebAppOidc.Client._Imports).Assembly);

app.Run();
