using Azure.Identity;
using AzureAppConfig.Example.App.Components;
using AzureAppConfig.Example.App.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement.FeatureFilters;
using Microsoft.FeatureManagement;
using AzureAppConfig.Example.App.CustomFlagFilters;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

var builder = WebApplication.CreateBuilder(args);

/* 
    https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-8.0

    WebApplication.CreateBuilder(args) 
    Configura automaticamene i prodvider di configurazione
    per leggere i valori da alle seguente sorgenti:
      - appsettings.json
      - appsettings.{Environment}.json
      - user secrets
      - environment variables
      - argomenti della riga di comando.

    I provider sovrascrivono i valori in base all'ordine in cui sono elencati sopra.
*/

//Bind the configuration to the options class to make it available for dependency injection.
builder.Services.Configure<SmtpServerOptions>(
    builder.Configuration.GetSection(SmtpServerOptions.SmtpServer));

string connectionString = builder.Configuration.GetConnectionString("AppConfig");

builder.Configuration.AddAzureAppConfiguration(options =>
{
    options.Connect(connectionString)
    .ConfigureRefresh(refresh =>
    {
        refresh.Register("Sentinel", refreshAll: true)
            .SetCacheExpiration(TimeSpan.FromSeconds(5));
    })
    .ConfigureKeyVault(keyVaultOptions =>
    {
        keyVaultOptions.SetCredential(new DefaultAzureCredential());
    })
    .UseFeatureFlags(featureFlagOptions => {
        featureFlagOptions.CacheExpirationInterval = TimeSpan.FromSeconds(5);
    });
});

builder.Services.AddAzureAppConfiguration();

builder.Services.AddScopedFeatureManagement()
    .AddFeatureFilter<PercentageFilter>()
    .AddFeatureFilter<CustomFeatureFilter>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.UseAzureAppConfiguration();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();


app.Run();