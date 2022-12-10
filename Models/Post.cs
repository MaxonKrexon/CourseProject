namespace FreshSight.Models;

public class Post {
    public String? ID {get; set;}
    public AppUser? Author {get; set;}
    public String? Adress {get;set;}
    public double? Rating {get; set;}
    public IEnumerable<Comment>? Comments {get; set;}
}