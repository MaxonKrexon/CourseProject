using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using FreshSight.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Azure.Storage.Blobs;
using FreshSight.Data;

namespace FreshSight.Controllers;

public class PostController : Controller
{
    private readonly ILogger<PostController> _logger;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager;
    private readonly ApplicationDbContext _db;

    public PostController(ILogger<PostController> logger,
    SignInManager<AppUser> signInManager,
    UserManager<AppUser> userManager,
    ApplicationDbContext db)
    {
        _logger = logger;
        _signInManager = signInManager;
        _userManager = userManager;
        _db = db;
    }
    static String connectionString = "DefaultEndpointsProtocol=https;AccountName=freshsightcloud;AccountKey=nnVOWYu0nVMx1pprfPeoktl2PdAsTdmW/iL8Zt/CfqrP3xugfFM72Kpi47/l46qrfhBCIMDMliQ++AStPFLjHw==;EndpointSuffix=core.windows.net";

    public IActionResult Index()
    {
        if (_signInManager.IsSignedIn(User))
        {
            return View();
        }
        else
        {
            return Redirect("~/Identity/Account/Login");
        }
    }

    [HttpPost]
    public async Task<RedirectToActionResult> Index(String Topic, String Category, String Text, double authorGrade, Post post)
    {
        AppUser user = await _userManager.GetUserAsync(User);

        Guid postId = Guid.NewGuid();
        post.ID = postId.ToString();
        post.Topic = Topic;
        post.Category = Category;
        post.AuthorGrade = authorGrade;

        String textfile = $"wwwroot/uploaded/_text";
        System.IO.File.CreateText(textfile);
        System.IO.File.WriteAllText(textfile, Text);
        UploadToCloud(post, user);
        return RedirectToAction("Index", "Blog");
    }

    public async void UploadToCloud(Post post, AppUser user)
    {
        post.Author = user;

        String containerName = "posts";
        BlobContainerClient containerClient = new BlobContainerClient(connectionString, containerName);

        String dir = $"wwwroot/uploaded/";
        var files = Directory.GetFiles(dir);

        foreach (var file in files)
        {
            using (MemoryStream ms = new MemoryStream(System.IO.File.ReadAllBytes(file)))
            {
                String BlobName = post.ID + file.Split("/")[2];
                containerClient.UploadBlob(blobName: BlobName, content: ms);
                ms.Flush();
                System.IO.File.Delete(file);
            }

        }
        post.CreationTime = DateTime.Now.ToString();
        user.Posts.Add(post);
        _db.SaveChanges();
    }

    [HttpPost]
    public void UploadToServer()
    {
        var files = Request.Form.Files;
        String dir = $"wwwroot/uploaded/";
        int index = 0;
        foreach (var file in files)
        {
            String unitName = String.Empty;
            if (file.Name == "thumbnail")
            {
                unitName = $"_thumb";
            }
            else unitName = $"_{index}";

            String filename = dir + unitName;

            using (FileStream fs = System.IO.File.Create(filename))
            {
                file.CopyTo(fs);
                fs.Flush();
            }
            index++;
        }
    }

    [HttpPost]
    public async Task<RedirectToActionResult> AddComment(String Comment, String postId)
    {
        AppUser user = await _userManager.GetUserAsync(User);
        var post = _db.Posts.Where(p => p.ID == postId).ToList()[0];
        var cmt = new Comment();
        cmt.Author = user;
        cmt.Post = post;
        cmt.CreationTime = DateTime.Now.ToString();
        cmt.Content = Comment;
        cmt.ID = Guid.NewGuid().ToString();
        post.Comments.Add(cmt);
        _db.SaveChanges();
        
        TempData["AfterRedirectVar"] = postId;
        return RedirectToAction("ViewPost", "Blog", new { postId = post.ID });
    }
}