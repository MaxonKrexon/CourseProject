using Microsoft.AspNetCore.Identity;
namespace FreshSight.Models;
public class AppUser : IdentityUser {
    public DateOnly? DateOfBirth {get; set;}
}