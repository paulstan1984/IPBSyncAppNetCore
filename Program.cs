using Hangfire;
using Hangfire.MySql;
using IPBSyncAppNetCore.Components;
using IPBSyncAppNetCore.Jobs;
using System.Transactions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add Hangfire services.
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseStorage(new MySqlStorage(
        builder.Configuration.GetConnectionString("MySQLDBConnection"),
        new MySqlStorageOptions
        {
            TransactionIsolationLevel = IsolationLevel.ReadCommitted,
            QueuePollInterval = TimeSpan.FromSeconds(15),
            JobExpirationCheckInterval = TimeSpan.FromHours(1),
            CountersAggregateInterval = TimeSpan.FromMinutes(5),
            PrepareSchemaIfNecessary = true,
            DashboardJobListLimit = 50000,
            TransactionTimeout = TimeSpan.FromMinutes(1),
            TablesPrefix = "Hangfire"
        })));
    //.UseSqlServerStorage(builder.Configuration.GetConnectionString("DBConnection")));

// Register IHttpClientFactory
builder.Services.AddHttpClient();

// You can also name clients for specific uses
builder.Services.AddHttpClient(ConfigService.WebAPIHttpClient, client =>
{
    client.BaseAddress = new Uri(ConfigService.WebRESTAPIURL);
    client.DefaultRequestHeaders.Add("Authorization", ConfigService.WebAuthorizationToken);
    client.Timeout = new TimeSpan(1, 30, 0);
});

builder.Services.AddHttpClient(ConfigService.WMERestAPIHttpClient, client =>
{
    client.BaseAddress = new Uri(ConfigService.WMERESTAPIURL);
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
