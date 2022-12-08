using Microsoft.AspNetCore.Identity;
namespace FreshSight.Models;
public class AppUser : IdentityUser {
    public DateTime? DateOfBirth {get; set;}
    public IEnumerable<Post>? Posts {get; set;}
}