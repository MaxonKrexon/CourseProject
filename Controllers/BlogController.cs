using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using FreshSight.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Azure.Storage.Blobs;
using FreshSight.Data;
using Microsoft.EntityFrameworkCore;

namespace FreshSight.Controllers;

public class BlogController : Controller
{
    private readonly ILogger<BlogController> _logger;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager;
    private readonly ApplicationDbContext _db;

    public BlogController(ILogger<BlogController> logger,
    SignInManager<AppUser> signInManager,
    UserManager<AppUser> userManager,
    ApplicationDbContext db)
    {
        _logger = logger;
        _signInManager = signInManager;
        _userManager = userManager;
        _db = db;
    }
    public Dictionary<String, List<String[]>> Properties = new Dictionary<string, List<string[]>>();
    public Azure.Pageable<Azure.Storage.Blobs.Models.BlobItem>? pic { get; set; }
    static String connectionString = "DefaultEndpointsProtocol=https;AccountName=freshsightcloud;AccountKey=nnVOWYu0nVMx1pprfPeoktl2PdAsTdmW/iL8Zt/CfqrP3xugfFM72Kpi47/l46qrfhBCIMDMliQ++AStPFLjHw==;EndpointSuffix=core.windows.net";

    public async Task<IActionResult> Index()
    {
        if (_signInManager.IsSignedIn(User))
        {
            var user = await _userManager.GetUserAsync(User);
            GetUserAge(user);
            GetUserPic(user);
            GetUserPosts(user);

            return View(Properties);
        }
        else
        {
            return Redirect("~/Identity/Account/Login");
        }
    }
    public void GetUserPic(AppUser user)
    {
        String containerName = "images";
        BlobContainerClient imagesCli = new BlobContainerClient(connectionString, containerName);
        Properties.Add("PicLink", new List<String[]>());
        pic = imagesCli.GetBlobs(prefix: $"{user.Id}_pic");
        String picName = String.Empty;
        if (pic.Count() != 0)
        {
            picName = user.Id;
        }
        else picName = "default";

        String[] link = { $"https://freshsightcloud.blob.core.windows.net/images/{picName}_pic" };
        Properties["PicLink"].Add(link);
    }


    public void GetUserPosts(AppUser user)
    {
        String containerName = "posts";
        BlobContainerClient postsCli = new BlobContainerClient(connectionString, containerName);
        var userPosts = _db.Posts.Where(p => p.Author.Id == user.Id).ToList();
        if (userPosts.Count() > 0)
        {
            Properties.Add("Posts", new List<String[]>());
            foreach (var userPost in userPosts)
            {
                String[] values = { userPost.ID, userPost.Category, userPost.Topic };
                Properties["Posts"].Add(values);
            }
        }
    }


    public void GetUserAge(AppUser user)
    {
        if (user.DateOfBirth != null)
        {
            var span = DateTime.Now.Subtract((DateTime)user.DateOfBirth);
            Properties.Add("Age", new List<String[]>());
            String[] value = { (span.Days / 365).ToString() };
            Properties["Age"].Add(value);
        }
    }

    
    public IActionResult ViewPost(String postId)
    {
        var post = _db.Posts.Where(p => p.ID == postId).ToList()[0];
        return View(post);
    }



    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}