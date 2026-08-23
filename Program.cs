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
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireUppercase = true;
        options.User.RequireUniqueEmail = true;
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
    var result = await maintenance.ResetAndRebuildAsync();
    app.Logger.LogInformation(
        "Voice reset completed. DeletedVoiceRows={DeletedVoiceRows}, DeletedAudioRows={DeletedAudioRows}, DeletedAudioFiles={DeletedAudioFiles}, LearningItemsScanned={LearningItemsScanned}, VoiceRowsCreated={VoiceRowsCreated}, VoiceFilesCreated={VoiceFilesCreated}, VoiceFilesFailed={VoiceFilesFailed}, LearningItemsUpdated={LearningItemsUpdated}",
        result.DeletedVoiceRows,
        result.DeletedAudioRows,
        result.DeletedAudioFiles,
        result.LearningItemsScanned,
        result.VoiceRowsCreated,
        result.VoiceFilesCreated,
        result.VoiceFilesFailed,
        result.LearningItemsUpdated);
    Console.WriteLine($"Voice reset completed: deleted voice rows {result.DeletedVoiceRows}, deleted audio rows {result.DeletedAudioRows}, deleted files {result.DeletedAudioFiles}, scanned lessons {result.LearningItemsScanned}, created voice rows {result.VoiceRowsCreated}, generated files {result.VoiceFilesCreated}, failed files {result.VoiceFilesFailed}, updated lessons {result.LearningItemsUpdated}.");
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

if (args.Contains("--normalize-legacy-learning-items", StringComparer.OrdinalIgnoreCase))
{
    await SeedDataInitializer.InitializeAsync(app.Services, app.Configuration, app.Logger);
    Console.WriteLine("Normalized legacy learning items and relinked available voice files.");
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
