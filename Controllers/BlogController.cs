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


    public IActionResult ViewPost(String? postId)
    {
        if(postId == null){
            postId = TempData["AfterRedirectVar"] as string;
        }
        var post = _db.Posts.Where(p => p.ID == postId).Include(p => p.Author).ToList()[0];
        _CommentsPartial(postId);
        return View(post);
    }

    public IActionResult _CommentsPartial(String postId){
        var comments = _db.Comments.Where(c => c.Post.ID == postId).Include(c => c.Author).ToList();
        return View(comments);
    }

    public IActionResult DeletePost(String postId)
    {
        var post = _db.Posts.Where(p => p.ID == postId).ToList()[0];
        DeleteFromCloud(postId);
        _db.Posts.Remove(post);
        _db.SaveChanges();
        return RedirectToAction("Index", "Blog");
    }

    public async void DeleteFromCloud(String postId)
    {
        String containerName = "posts";
        BlobContainerClient postsCli = new BlobContainerClient(connectionString, containerName);
        var postFiles = postsCli.GetBlobs(prefix: postId);
        if (postFiles.Count() > 0)
        {
            foreach (var file in postFiles)
            {
                await postsCli.DeleteBlobAsync(file.Name);
            }
        }
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}