using Microsoft.AspNetCore.Identity;
namespace FreshSight.Models;
public class AppUser : IdentityUser {
    public DateTime? DateOfBirth {get; set;}
    public List<Post>? Posts {get; set;} = new List<Post>();
}