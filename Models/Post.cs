namespace FreshSight.Models;

public class Post {
    public String? ID {get; set;}
    public AppUser? Author {get; set;}
    public String? Topic {get;set;}
    public String? Category {get;set;}
    public String? CreationTime {get; set;}
    public double? AuthorGrade {get; set;}
    public double? UserRating {get; set;}
    public List<Comment>? Comments {get; set;} = new List<Comment>();
}