namespace FreshSight.Models;

public class Comment {
    public String? ID {get; set;}
    public Post? Post {get; set;}
    public AppUser? Author {get; set;}
    public String? Content {get; set;}
    public String? CreationTime {get; set;}
}