using Azure.Identity;
using Inventory.WebApp;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Azure;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

var builder = WebApplication.CreateBuilder(args);

#if DEBUG
Environment.SetEnvironmentVariable("AZURE_CLIENT_ID", builder.Configuration["AzureAd:ClientId"]);
Environment.SetEnvironmentVariable("AZURE_CLIENT_SECRET", builder.Configuration["AzureAd:ClientSecret"]);
Environment.SetEnvironmentVariable("AZURE_TENANT_ID", builder.Configuration["AzureAd:TenantId"]);
#endif
Environment.SetEnvironmentVariable("VaultUri", builder.Configuration["VaultUri"]);

var keyVaultEndpoint = new Uri(Environment.GetEnvironmentVariable("VaultUri")!);
builder.Configuration.AddAzureKeyVault(keyVaultEndpoint, new DefaultAzureCredential());

var initialScopes = builder.Configuration["DownstreamApi:Scopes"]?.Split(' ') ?? builder.Configuration["MicrosoftGraph:Scopes"]?.Split(' ');

// Add services to the container.
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"))
        .EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
.AddMicrosoftGraph(builder.Configuration.GetSection("MicrosoftGraph"))
            .AddInMemoryTokenCaches()
.AddDownstreamApi("DownstreamApi", builder.Configuration.GetSection("DownstreamApi"))
            .AddInMemoryTokenCaches();
builder.Services.AddControllersWithViews()
    .AddMicrosoftIdentityUI();

builder.Services.AddAuthorization(options =>
{
    // By default, all incoming requests will be authorized according to the default policy
    options.FallbackPolicy = options.DefaultPolicy;
});

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor()
    .AddMicrosoftIdentityConsentHandler();
builder.Services.AddAzureClients(clientBuilder =>
{
    clientBuilder.AddBlobServiceClient(builder.Configuration["AZURE_STORAGE_CONNECTIONSTRING:blob"]!, preferMsi: true);
    clientBuilder.AddQueueServiceClient(builder.Configuration["AZURE_STORAGE_CONNECTIONSTRING:queue"]!, preferMsi: true);
    clientBuilder.AddTableServiceClient(builder.Configuration["AZURE_STORAGE_CONNECTIONSTRING:table"]!);
});
builder.Services.AddApplicationInsightsTelemetry();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
