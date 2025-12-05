using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WorkshopManager.DAL.EF;
using WorkshopManager.Model.DataModels;

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
        throw new Exception("Brak wymaganych zmiennych �rodowiskowych dla Ownera.");
    }

    // Seeding roli Owner
    if (!await roleManager.RoleExistsAsync(RoleValue.Owner.ToString()))
        await roleManager.CreateAsync(new Role { Name = RoleValue.Owner.ToString() });

    // Sprawdzenie czy Owner istnieje
    var existingOwner = await userManager.FindByEmailAsync(ownerEmail);
    if (existingOwner != null)
    {
        // Sprawdź czy ma rolę Owner
        var roles = await userManager.GetRolesAsync(existingOwner);
        if (!roles.Contains(RoleValue.Owner.ToString()))
        {
            await userManager.AddToRoleAsync(existingOwner, RoleValue.Owner.ToString());
        }
        return;
    }

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
        throw new Exception("Nie uda�o si� utworzy� konta Ownera: " + string.Join(", ", result.Errors.Select(e => e.Description)));

    await userManager.AddToRoleAsync(owner, RoleValue.Owner.ToString());
}
