using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using FreshSight.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Azure.Storage.Blobs;
using FreshSight.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Azure.Storage.Blobs.Specialized;

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
    public Azure.Pageable<Azure.Storage.Blobs.Models.BlobItem>? pic { get; set; }
    static String connectionString = "DefaultEndpointsProtocol=https;AccountName=freshsightcloud;AccountKey=nnVOWYu0nVMx1pprfPeoktl2PdAsTdmW/iL8Zt/CfqrP3xugfFM72Kpi47/l46qrfhBCIMDMliQ++AStPFLjHw==;EndpointSuffix=core.windows.net";

    public async Task<IActionResult> Index()
    {
        if (_signInManager.IsSignedIn(User))
        {
            var user = await _userManager.GetUserAsync(User);
            TempData["targetAge"] = GetUserAge(user);
            TempData["targetPic"] = GetUserPic(user);
            TempData["isAuthor"] = "true";
            _bloggerPosts(user);
            return View(user);
        }
        else
        {
            return Redirect("~/Identity/Account/Login");
        }
    }
    public String GetUserPic(AppUser user)
    {
        String containerName = "images";
        BlobContainerClient imagesCli = new BlobContainerClient(connectionString, containerName);
        pic = imagesCli.GetBlobs(prefix: $"{user.Id}_pic");
        String picName = String.Empty;
        if (pic.Count() != 0)
        {
            picName = user.Id;
        }
        else picName = "default";

        String link = $"https://freshsightcloud.blob.core.windows.net/images/{picName}_pic";
        return link;
    }


    public String GetUserAge(AppUser user)
    {
        if (user.DateOfBirth != null)
        {
            var span = DateTime.Now.Subtract((DateTime)user.DateOfBirth);
            String value = (span.Days / 365).ToString();
            return value;
        }
        else return String.Empty;
    }


    public IActionResult ViewPost(String? postId)
    {
        if(postId == null){
            postId = TempData["AfterRedirectVar"] as string;
        }
        var post = _db.Posts.Where(p => p.ID == postId).Include(p => p.Author).First();

        _textPartial(postId);
        _CommentsPartial(postId);
        _gradesPartial(postId);
        
        return View(post);
    }

    public async Task<IActionResult> _gradesPartial(String postId){
        var grades = _db.UserGrades.Where(g => g.Post.ID == postId).Include(p => p.Author).ToList();
        var user = await _userManager.GetUserAsync(User);
        foreach(var grade in grades){
            if(grade.Author.Id == user.Id){
                TempData["CurrentUserGrade"] = grade.Grade.ToString();
                break;
            }
        }
        return View(grades);
    }

    public IActionResult _CommentsPartial(String postId){
        var comments = _db.Comments.Where(c => c.Post.ID == postId).Include(c => c.Author).OrderBy(c => c.CreationTime).ToList();
        return View(comments);
    }

    [HttpPost]
    public void UpdateComments(String postId){
        _CommentsPartial(postId);
    }

    public void _textPartial(String postId){
        var containerName = "posts";
        var textBlob = new BlockBlobClient(connectionString, containerName, $"{postId}_text");
        var textBlock = textBlob.DownloadContent();
        var text = textBlock.Value.Content.ToStream();
        using (FileStream fs = System.IO.File.Create("sample.md"))
        {
            text.CopyTo(fs);
            fs.Flush();
        }
        System.Diagnostics.Process.Start("pandoc", " sample.md -t html -o sample.cshtml");
    }

    public IActionResult DeletePost(String postId)
    {
        var post = _db.Posts.Where(p => p.ID == postId).First();
        DeleteFromCloud(postId);
        var comments = _db.Comments.Where(c => c.Post.ID == postId).ToList();
        
        foreach(var comment in comments){
            _db.Comments.Remove(comment);
        }

        var grades = _db.UserGrades.Where(g => g.Post.ID == postId).ToList();
        foreach(var grade in grades){
            _db.UserGrades.Remove(grade);
        }

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

    public async Task<IActionResult> Blogger(String userId){
        var currentUser = await _userManager.GetUserAsync(User);
        var author = await _userManager.FindByIdAsync(userId);
        if(currentUser.Id == author.Id){
            TempData["isAuthor"] = "true";
            return RedirectToAction("Index");
        }
        TempData["targetAge"] = GetUserAge(author);
        TempData["targetPic"] = GetUserPic(author);
        _bloggerPosts(author);
        return View(author);
    }

    public IActionResult _bloggerPosts(AppUser author){
        var posts = _db.Posts.Where(p => p.Author == author).Include(p => p.UserRating).ToList();
        return View(posts);
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}