using HanhTrangLop1.Data;
using HanhTrangLop1.Application.Learning;
using HanhTrangLop1.Application.Voice;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 3;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
        options.Password.RequiredUniqueChars = 1;
        options.User.RequireUniqueEmail = false;
        options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+ ";
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/parent/login";
    options.AccessDeniedPath = "/parent/access-denied";
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".HanhTrangLop1.ChildSession";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromHours(8);
});

builder.Services.AddScoped<TodayLessonService>();
builder.Services.AddScoped<VoiceLibraryMaintenanceService>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

if (args.Contains("--reset-voice-library", StringComparer.OrdinalIgnoreCase))
{
    using var scope = app.Services.CreateScope();
    var maintenance = scope.ServiceProvider.GetRequiredService<VoiceLibraryMaintenanceService>();
    var result = await maintenance.ResetAndRebuildAllVoicesAsync();
    Console.WriteLine($"Voice clean rebuild completed: TotalVoices={result.TotalVoices}, GeneratedVi={result.GeneratedVi}, GeneratedEn={result.GeneratedEn}, Failed={result.Failed}, UpdatedLessons={result.UpdatedLessons}");
    return;
}

if (args.Contains("--voice-report", StringComparer.OrdinalIgnoreCase))
{
    using var scope = app.Services.CreateScope();
    var maintenance = scope.ServiceProvider.GetRequiredService<VoiceLibraryMaintenanceService>();
    Console.WriteLine(await maintenance.BuildReportAsync());
    return;
}

if (args.Contains("--generate-missing-voice-files", StringComparer.OrdinalIgnoreCase))
{
    using var scope = app.Services.CreateScope();
    var maintenance = scope.ServiceProvider.GetRequiredService<VoiceLibraryMaintenanceService>();
    var result = await maintenance.GenerateMissingAndRelinkAsync();
    Console.WriteLine($"Generated missing voice files: created {result.Created}, failed {result.Failed}, updated lessons {result.UpdatedItems}.");
    return;
}

await SeedDataInitializer.InitializeAsync(app.Services, app.Configuration, app.Logger);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

if (app.Configuration.GetValue("App:UseHttpsRedirection", false))
{
    app.UseHttpsRedirection();
}
app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
