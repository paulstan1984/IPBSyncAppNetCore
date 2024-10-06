using Hangfire;
using IPBSyncAppNetCore.Components;
using IPBSyncAppNetCore.Jobs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add Hangfire services.
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DBConnection")));

// Register IHttpClientFactory
builder.Services.AddHttpClient();

// You can also name clients for specific uses
builder.Services.AddHttpClient("WebAPIHttpClient", client =>
{
    client.BaseAddress = new Uri(ConfigService.WebRESTAPIURL);
    client.DefaultRequestHeaders.Add("Authorization", ConfigService.WebAuthorizationToken);
    client.Timeout = new TimeSpan(1, 30, 0);
});

builder.Services.AddHttpClient("WMERestAPIHttpClient", client =>
{
    client.BaseAddress = new Uri(ConfigService.WebRESTAPIURL);
    client.Timeout = new TimeSpan(1, 30, 0);
});

// Add the processing server as IHostedService
builder.Services.AddHangfireServer();

ConfigService.Configuration = builder.Configuration
    .GetSection("IpbSyncAppConfig")
    .Get<IpbSyncAppConfig>();

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
app.UseHangfireDashboard();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.UseHangfireDashboard("/hangfire-dashboard", new DashboardOptions
{
});

JobsScheduler.ScheduleJobs();
//JobsScheduler.TestScheduledJob(new SyncCategoriesJob());
app.Run();
