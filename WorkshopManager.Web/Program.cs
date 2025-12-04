using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WorkshopManager.DAL.EF;
using WorkshopManager.Model.DataModels;
using WorkshopManager.Services;
using System.Net;
using System.Net.Mail;
using WorkshopManager.Web.Models;

var builder = WebApplication.CreateBuilder(args);

// Pobranie connection stringa
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Rejestracja DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Identity (User + Role)
builder.Services.AddIdentity<User, Role>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// MVC + Razor Pages
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();


var app = builder.Build();

// Seeding Ownera z .env
await SeedOwnerAccount(app.Services);

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();

// --- Funkcja do seeding Ownera ---
async Task SeedOwnerAccount(IServiceProvider serviceProvider)
{
    using var scope = serviceProvider.CreateScope();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
    

    DotNetEnv.Env.TraversePath().Load();
    var ownerEmail = Environment.GetEnvironmentVariable("OWNER_EMAIL");
    var ownerPassword = Environment.GetEnvironmentVariable("OWNER_PASSWORD");
    var ownerFirstName = Environment.GetEnvironmentVariable("OWNER_FIRSTNAME");
    var ownerLastName = Environment.GetEnvironmentVariable("OWNER_LASTNAME");

    if (string.IsNullOrEmpty(ownerEmail) || string.IsNullOrEmpty(ownerPassword) ||
        string.IsNullOrEmpty(ownerFirstName) || string.IsNullOrEmpty(ownerLastName))
    {
        throw new Exception("Brak wymaganych zmiennych œrodowiskowych dla Ownera.");
    }

    // Seeding roli Owner
    if (!await roleManager.RoleExistsAsync(RoleValue.Owner.ToString()))
        await roleManager.CreateAsync(new Role { Name = RoleValue.Owner.ToString() });

    // Sprawdzenie czy Owner istnieje
    var existingOwner = await userManager.FindByEmailAsync(ownerEmail);
    if (existingOwner != null)
        return;

    // Tworzenie Ownera
    var owner = new Owner
    {
        FirstName = ownerFirstName,
        LastName = ownerLastName,
        Email = ownerEmail,
        UserName = ownerEmail
    };

    var result = await userManager.CreateAsync(owner, ownerPassword);
    if (!result.Succeeded)
        throw new Exception("Nie uda³o siê utworzyæ konta Ownera: " + string.Join(", ", result.Errors.Select(e => e.Description)));

    await userManager.AddToRoleAsync(owner, RoleValue.Owner.ToString());
}

// --- Funkcja do testowania SMTP (opcjonalnie) ---
bool ValidateSmtpCredentials(string smtpHost, int smtpPort, string email, string password)
{
    try
    {
        using var client = new SmtpClient(smtpHost, smtpPort)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(email, password),
            Timeout = 10000
        };

        client.Send(new MailMessage(email, email, "Test SMTP", "Test po³¹czenia SMTP. " + DateTime.Now));
        return true;
    }
    catch
    {
        return false;
    }
}
