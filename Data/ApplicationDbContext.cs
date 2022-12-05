using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FreshSight.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
}

// dotnet aspnet-codegenerator identity -dc WEBAPPTASK4.Data.ApplicationDbContext --files "Account.Register;Account.Login"