using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuizFolio.Models;
using QuizFolio.Data;
using QuizFolio.Service.Salesforce;
using QuizFolio.Services.Salesforce;
using Polly.Extensions.Http;
using Polly;
using System.Net;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddIdentity<Users, IdentityRole>(options =>
{
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Add configuration
builder.Services.Configure<SalesforceSettings>(builder.Configuration.GetSection("Salesforce"));

// Register Salesforce services
builder.Services.AddHttpClient();
builder.Services.AddScoped<SalesforceAuth>(provider =>
{
    var config = provider.GetRequiredService<IOptions<SalesforceSettings>>().Value;
    var logger = provider.GetRequiredService<ILogger<SalesforceAuth>>();
    return new SalesforceAuth(
        logger,
        config.ClientId,
        config.ClientSecret,
        config.Username,
        config.Password,
        config.LoginUrl);
});

builder.Services.AddScoped<ISalesforceService, SalesforceService>();
var app = builder.Build();

    //  Role Creation
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    string[] roles = { "Admin", "User" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Template}/{action=AllTemplate}/{id?}");
static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == HttpStatusCode.TooManyRequests)
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}

app.Run();