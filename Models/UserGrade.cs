using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FreshSight.Models;

public class UserGrade {
    public String? ID {set; get;}
    public AppUser? Author {set; get;}
    public Post? Post {set; get;}
    public double? Grade {set; get;}
}