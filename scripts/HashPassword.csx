using Microsoft.AspNetCore.Identity;
using Gdc.Infrastructure.Persistence.Auth.Entities;
var hasher = new PasswordHasher<User>();
var hash = hasher.HashPassword(new User { Email = "x@y.com" }, "Str0ng!Pass");
Console.WriteLine(hash);
