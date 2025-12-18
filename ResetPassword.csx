#!/usr/bin/env dotnet-script
#r "nuget: Microsoft.AspNetCore.Identity, 8.0.10"
#r "nuget: Microsoft.EntityFrameworkCore.SqlServer, 8.0.10"

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

// Użycie UserManager do resetowania hasła
var email = "patryk.wojtala2001@gmail.com";
var newPassword = "Pat123.321";

Console.WriteLine($"Resetowanie hasła dla: {email}");
Console.WriteLine($"Nowe hasło: {newPassword}");

// Hash hasła
var passwordHasher = new PasswordHasher<object>();
var hashedPassword = passwordHasher.HashPassword(null, newPassword);

Console.WriteLine($"\nZahashowane hasło:");
Console.WriteLine(hashedPassword);
Console.WriteLine($"\nUruchom to SQL w bazie danych:");
Console.WriteLine($"UPDATE AspNetUsers SET PasswordHash = '{hashedPassword}' WHERE NormalizedEmail = '{email.ToUpper()}';");
