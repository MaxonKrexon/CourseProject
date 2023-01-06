using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FreshSight.Models;

namespace FreshSight.Data;

public class ApplicationDbContext : IdentityDbContext<AppUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {

    }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
    }

    public override DbSet<AppUser>? Users {get; set;}
    public DbSet<Post>? Posts {get; set;}
    public DbSet<Comment>? Comments {get; set;}
    public DbSet<UserGrade>? UserGrades {get; set;}

}