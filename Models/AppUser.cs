using Microsoft.AspNetCore.Identity;
namespace FreshSight.Models;
public class AppUser : AppUser {
    public DateOnly? DateOfBirth {get; set;}
}