using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WorkshopManager.DAL.EF;
using WorkshopManager.Model.DataModels;

var builder = WebApplication.CreateBuilder(args);

// Connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Identity - User
builder.Services.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = false)
       .AddRoles<Role>()
       .AddEntityFrameworkStores<ApplicationDbContext>();

// Identity - Client
builder.Services.AddIdentityCore<Client>(options => options.SignIn.RequireConfirmedAccount = false)
       .AddRoles<Role>()
       .AddSignInManager<SignInManager<Client>>()
       .AddUserManager<UserManager<Client>>()
       .AddEntityFrameworkStores<ApplicationDbContext>();

// Identity - Owner
builder.Services.AddIdentityCore<Owner>(options => options.SignIn.RequireConfirmedAccount = false)
       .AddRoles<Role>()
       .AddSignInManager<SignInManager<Owner>>()
       .AddUserManager<UserManager<Owner>>()
       .AddEntityFrameworkStores<ApplicationDbContext>();

// MVC + Razor Pages
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// Dev/Prod pipeline
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

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
