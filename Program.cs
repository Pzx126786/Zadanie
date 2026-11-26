using BeFit.Data;
using BeFit.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;
var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddDbContext<ApplicationDbContext>(o => o.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=App_Data/befit.db"));
builder.Services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "App_Data", "keys")));
builder.Services.AddIdentity<IdentityUser, IdentityRole>(o =>
{
    o.User.RequireUniqueEmail = true; o.Password.RequiredLength = 10; o.Password.RequireDigit = false; o.Password.RequireNonAlphanumeric = false; o.Password.RequireUppercase = false; o.Password.RequireLowercase = false; o.Lockout.MaxFailedAccessAttempts = 5;
}).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(o => { o.LoginPath = "/Account/Login"; o.AccessDeniedPath = "/Account/Denied"; o.Cookie.HttpOnly = true; o.Cookie.SameSite = SameSiteMode.Strict; o.ExpireTimeSpan = TimeSpan.FromHours(8); });
builder.Services.AddControllersWithViews(o =>
{
    o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
    o.Filters.Add(new ResponseCacheAttribute { NoStore = true, Location = ResponseCacheLocation.None });
    o.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(_ => "Pole jest wymagane.");
    o.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor((v, f) => $"Wartość pola {f} jest nieprawidłowa.");
});
var app = builder.Build();
Directory.CreateDirectory(Path.Combine(app.Environment.ContentRootPath, "App_Data"));
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>(); await db.Database.EnsureCreatedAsync();
    var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    if (!await roles.RoleExistsAsync("Administrator")) await roles.CreateAsync(new IdentityRole("Administrator"));
    if (!await db.ExerciseTypes.AnyAsync()) { db.ExerciseTypes.AddRange(new[] { "Przysiad", "Martwy ciąg", "Wyciskanie sztangi", "Wiosłowanie", "Podciąganie", "Wykroki" }.Select(n => new ExerciseType { Name = n })); await db.SaveChangesAsync(); }
    var index = Array.IndexOf(args, "--make-admin");
    if (index >= 0)
    {
        if (index + 1 >= args.Length) throw new ArgumentException("Podaj e-mail po --make-admin.");
        var users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var user = await users.FindByEmailAsync(args[index + 1]) ?? throw new ArgumentException("Najpierw zarejestruj konto.");
        if (!await users.IsInRoleAsync(user, "Administrator")) { var result = await users.AddToRoleAsync(user, "Administrator"); if (!result.Succeeded) throw new InvalidOperationException("Nie udało się nadać roli."); }
        Console.WriteLine("Nadano rolę administratora. Zaloguj się ponownie."); return;
    }
}
if (!app.Environment.IsDevelopment()) app.UseExceptionHandler("/Home/Error");
app.UseRequestLocalization(new RequestLocalizationOptions().SetDefaultCulture("pl-PL").AddSupportedCultures("pl-PL").AddSupportedUICultures("pl-PL"));
app.UseStaticFiles(); app.UseRouting(); app.UseAuthentication(); app.UseAuthorization();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}"); app.Run();
public partial class Program { }
