using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WorkshopManager.DAL.EF;
using WorkshopManager.Model.DataModels;

DotNetEnv.Env.TraversePath().Load();

var connectionString = "Server=(localdb)\\mssqllocaldb;Database=WorkshopManagerDb;Trusted_Connection=True;MultipleActiveResultSets=true";

var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
optionsBuilder.UseSqlServer(connectionString);

using var context = new ApplicationDbContext(optionsBuilder.Options);

var userManager = new UserManager<User>(
    new Microsoft.AspNetCore.Identity.EntityFrameworkCore.UserStore<User, Role, ApplicationDbContext, int>(context),
    null,
    new PasswordHasher<User>(),
    null,
    null,
    null,
    null,
    null,
    null);

var email = "patryk.wojtala2001@gmail.com";
var newPassword = "Pat123.321";

Console.WriteLine($"Szukam użytkownika: {email}");

var user = await userManager.FindByEmailAsync(email);

if (user == null)
{
    Console.WriteLine($"BŁĄD: Nie znaleziono użytkownika z emailem {email}");
    return;
}

Console.WriteLine($"Znaleziono użytkownika: {user.UserName} (ID: {user.Id})");

// Zahashuj nowe hasło i ustaw bezpośrednio
var passwordHasher = new PasswordHasher<User>();
user.PasswordHash = passwordHasher.HashPassword(user, newPassword);

// Zapisz zmiany
await context.SaveChangesAsync();

Console.WriteLine($"✓ Hasło zostało pomyślnie zresetowane!");
Console.WriteLine($"  Email: {email}");
Console.WriteLine($"  Nowe hasło: {newPassword}");
Console.WriteLine($"\nMożesz się teraz zalogować używając tych danych.");
