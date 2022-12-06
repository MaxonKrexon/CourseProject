#nullable disable

namespace FreshSight.Models;

public class Blog {
    public String BlogID {get; set;}
    public String AuthorID {get; set;}
    public String Adress {get;set;}
    public double Rating {get; set;}
    public IEnumerable<Comment> Comments {get; set;}
}