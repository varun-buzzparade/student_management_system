using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Configuration;
using StudentManagementSystem.Data;
using StudentManagementSystem.Models;
using StudentManagementSystem.Services.Email;
using StudentManagementSystem.Services.Shared;
using StudentManagementSystem.Services.Student.List;
using StudentManagementSystem.Services.Student.Mapping;
using StudentManagementSystem.Services.Student.Registration;
using StudentManagementSystem.Services.Student.Update;
using StudentManagementSystem.Services.Student.Upload;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(o => o.Limits.MaxRequestBodySize = 110 * 1024 * 1024); // 110 MB for registration uploads

var connectionString = ConfigHelper.GetConnectionString("DefaultConnection", builder.Configuration)
    ?? throw new InvalidOperationException("Missing DefaultConnection string.");

// Database & Identity
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireDigit = true;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// Email (SMTP)
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Smtp"));
builder.Services.Configure<TempUploadOptions>(builder.Configuration.GetSection(TempUploadOptions.SectionName));
builder.Services.AddScoped<IEmailSenderService, SmtpEmailSender>();

// Distributed cache (Redis / Memurai) for admin student list
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = ConfigHelper.GetConnectionString("Redis", builder.Configuration) ?? "localhost:6379";
    options.InstanceName = "StudentMgmt_";
});

// Application services
builder.Services.AddSingleton<IAgeCalculator, AgeCalculator>();
builder.Services.AddSingleton<IPasswordGenerator, PasswordGeneratorService>();
builder.Services.AddScoped<IStudentIdGenerator, StudentIdGeneratorService>();
builder.Services.AddScoped<IStudentListCacheService, StudentListCacheService>();
builder.Services.AddScoped<IStudentQueryService, StudentQueryService>();
builder.Services.AddScoped<IStudentViewModelMapper, StudentViewModelMapper>();
builder.Services.AddScoped<IStudentUpdateService, StudentUpdateService>();
builder.Services.AddScoped<IStudentRegistrationService, StudentRegistrationService>();
builder.Services.AddScoped<IStudentFileUploadService, StudentFileUploadService>();
builder.Services.AddScoped<IRegistrationDraftService, RegistrationDraftService>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Ensure roles exist and seed admin users from config (AdminUsers or legacy AdminCredentials)
using (var scope = app.Services.CreateScope())
{
    await IdentitySeeder.SeedAsync(scope.ServiceProvider);
    scope.ServiceProvider.GetRequiredService<IStudentFileUploadService>().CleanupExpiredDraftFolders();
    var draftSvc = scope.ServiceProvider.GetRequiredService<IRegistrationDraftService>();
    var expiryMinutes = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<TempUploadOptions>>().Value.ExpiryMinutes;
    await draftSvc.DeleteExpiredDraftsAsync(expiryMinutes, CancellationToken.None);
}

app.Run();
