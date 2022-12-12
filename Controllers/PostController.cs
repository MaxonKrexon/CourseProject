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
        return View();
    }

    [HttpPost]
    public IActionResult Index(String Topic, String Category, String Text, Post post)
    {   
        Guid postId = new Guid();
        post.ID = postId.ToString();
        post.Topic = Topic;
        post.Category = Category;

        String textfile = $"wwwroot/uploaded/_text";
        System.IO.File.CreateText(textfile);
        System.IO.File.WriteAllText(textfile, Text);
        UploadToCloud(post);
        return RedirectToPage("/Blog/Index");
    }

    public async void UploadToCloud(Post post)
    {

        var user = await _signInManager.UserManager.GetUserAsync(User);
        post.Author = user;

        String containerName = "posts";
        BlobContainerClient containerClient = new BlobContainerClient(connectionString, containerName);
        String dir = $"wwwroot/uploaded/";
        var files = Directory.GetFiles(dir);
        foreach (var file in files)
        {
            String filename = dir + post.ID + file;
            using (MemoryStream ms = new MemoryStream(System.IO.File.ReadAllBytes(filename)))
            {
                containerClient.DeleteBlobIfExists(blobName: filename);
                containerClient.UploadBlob(blobName: filename, content: ms);

                ms.Flush();
                System.IO.File.Delete(filename);
            }

        }
        user.Posts.Append(post);
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
}