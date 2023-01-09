using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FreshSight.Models;
using Azure.Storage.Blobs;
using FreshSight.Data;
using Azure.Storage.Blobs.Specialized;
using Microsoft.EntityFrameworkCore;

namespace FreshSight.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _db;

    public HomeController(ILogger<HomeController> logger,
    ApplicationDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    public IActionResult Index()
    {
        var posts = _db.Posts.Include(p => p.UserRating).OrderByDescending(p => p.CreationTime).ToList();
        TempData["selectedCategory"] = "All";
        return View(posts);
    }

    [HttpPost]
    public IActionResult Index(String Category)
    {
        if(!Category.Contains("All")){
            var posts = _db.Posts.Where(p => p.Category == Category).Include(p => p.UserRating).OrderByDescending(p => p.CreationTime).ToList();
            TempData["selectedCategory"] = Category;
            return View(posts);
        }
        else{
            var posts = _db.Posts.Include(p => p.UserRating).OrderByDescending(p => p.CreationTime).ToList();
            TempData["selectedCategory"] = "All";
            return View(posts);
        }
        
    }
    
    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Find(String question)
    {
        List<Post> searchResult = new List<Post>();
        var posts = _db.Posts.Include(p => p.UserRating).ThenInclude(p => p.Author).ToList();
        
        String containerName = "posts";
        String connectionString = "DefaultEndpointsProtocol=https;AccountName=freshsightcloud;AccountKey=nnVOWYu0nVMx1pprfPeoktl2PdAsTdmW/iL8Zt/CfqrP3xugfFM72Kpi47/l46qrfhBCIMDMliQ++AStPFLjHw==;EndpointSuffix=core.windows.net";
        
        foreach(var post in posts){
            var textBlock = new BlockBlobClient(connectionString, containerName, $"{post.ID}_text");
            var textBlob = textBlock.DownloadContent();
            var text = textBlob.Value.Content.ToString().ToLower();
            
            if(post.Topic.ToLower().Contains(question.ToLower()) && !searchResult.Contains(post)){
                searchResult.Add(post);
            }
            else if(text.Contains(question.ToLower()) && !searchResult.Contains(post)){
                searchResult.Add(post);
            }

            var comments = _db.Comments.Where(c => c.Post == post).ToList();
            foreach(var comment in comments){
                if(comment.Content.ToLower().Contains(question.ToLower()) && !searchResult.Contains(post)){
                    searchResult.Add(post);
                    break;
                }
            }
        }

        return View(searchResult);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
