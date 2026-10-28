using HanhTrangLop1.Data;
using HanhTrangLop1.Application.Learning;
using HanhTrangLop1.Application.Lessons;
using HanhTrangLop1.Application.MathWorld;
using HanhTrangLop1.Application.Voice;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Cấu hình DataProtection lưu khóa vào App_Data để duy trì cookie/session khi App Pool recycle
var dataProtectionPath = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "DataProtection-Keys");
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionPath))
    .SetApplicationName("HanhTrangLop1");

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
builder.Services.AddScoped<RewardProgressionService>();
builder.Services.AddScoped<LessonPathService>();
builder.Services.AddScoped<VoiceLibraryMaintenanceService>();
builder.Services.AddScoped<ChildProfileResolver>();
builder.Services.AddScoped<MathWorldService>();
builder.Services.AddScoped<MathVoiceService>();
builder.Services.AddHostedService<MathVoiceWarmupService>();
builder.Services.AddHostedService<LessonVoiceWarmupService>();
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

// Dọn sạch dữ liệu dư thừa (chạy thử trước bằng --dry-run, không đổi gì):
//   --purge-archived-lessons : xóa bài đã lưu trữ (bài nền cũ đã được kho bài thay thế) mà không bé nào từng làm.
//   --clean-voice-library    : dựng lại kho Voice theo nội dung hiện tại, dùng lại file sẵn có, xóa file Voice thừa.
if (args.Contains("--purge-archived-lessons", StringComparer.OrdinalIgnoreCase) ||
    args.Contains("--clean-voice-library", StringComparer.OrdinalIgnoreCase))
{
    var apply = !args.Contains("--dry-run", StringComparer.OrdinalIgnoreCase);
    await SeedDataInitializer.InitializeAsync(app.Services, app.Configuration, app.Logger);

    if (args.Contains("--purge-archived-lessons", StringComparer.OrdinalIgnoreCase))
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var unused = db.LearningItems.Where(x =>
            x.Status == HanhTrangLop1.Models.ContentStatus.Archived &&
            !db.LearningAttempts.Any(a => a.LearningItemId == x.Id) &&
            !db.ChildLessonProgresses.Any(p => p.LearningItemId == x.Id) &&
            !db.QuestionAttempts.Any(qa => x.Questions.Any(q => q.Id == qa.QuestionId)));
        var count = await unused.CountAsync();
        var deleted = apply ? await unused.ExecuteDeleteAsync() : 0;
        Console.WriteLine($"Archived lessons without child history: {count}. Deleted: {deleted}{(apply ? string.Empty : " (dry run)")}.");
    }

    if (args.Contains("--clean-voice-library", StringComparer.OrdinalIgnoreCase))
    {
        using var scope = app.Services.CreateScope();
        var maintenance = scope.ServiceProvider.GetRequiredService<VoiceLibraryMaintenanceService>();
        var mathVoices = scope.ServiceProvider.GetRequiredService<MathVoiceService>();
        var result = await maintenance.CleanRebuildVoiceIndexAsync(async ct =>
        {
            await mathVoices.EnsureCatalogRowsAsync(ct);
            foreach (var lines in LessonVoiceWarmupService.LinesByGroup())
            {
                await mathVoices.EnsureRowsAsync(lines, LessonVoiceWarmupService.UsageType, ct);
            }
        }, apply);
        Console.WriteLine($"Voice clean rebuild{(apply ? string.Empty : " (dry run, rolled back)")}: " +
            $"rows {result.OldRows} -> {result.NewRows}; reused files VI={result.ReusedFilesVi} EN={result.ReusedFilesEn}; " +
            $"still missing VI={result.MissingVi} EN={result.MissingEn}; files on disk {result.FilesOnDisk}, " +
            $"unused {result.OrphanFiles} ({result.OrphanBytes / 1024 / 1024} MB), deleted {result.DeletedFiles}.");
    }
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

if (args.Contains("--math-report", StringComparer.OrdinalIgnoreCase))
{
    Console.WriteLine(MathCurriculumReport.Build());
    return;
}

if (args.Contains("--lesson-report", StringComparer.OrdinalIgnoreCase))
{
    Console.WriteLine(LessonBankReport.Build());
    return;
}

if (args.Contains("--init-seed-only", StringComparer.OrdinalIgnoreCase))
{
    await SeedDataInitializer.InitializeAsync(app.Services, app.Configuration, app.Logger);
    Console.WriteLine("Seed and curriculum initialization completed successfully.");
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

// MapStaticAssets chỉ phục vụ file có sẵn lúc build. File Voice (mp3) được sinh trong lúc chạy vào
// wwwroot/uploads nên cần thêm StaticFiles, nếu không bản publish sẽ trả 404 cho Voice mới tạo.
app.UseStaticFiles();
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
