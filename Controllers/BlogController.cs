using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using FreshSight.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Azure.Storage.Blobs;

namespace FreshSight.Controllers;

public class BlogController : Controller
{
    private readonly ILogger<BlogController> _logger;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager;

    public BlogController(ILogger<BlogController> logger,
    SignInManager<AppUser> signInManager,
    UserManager<AppUser> userManager)
    {
        _logger = logger;
        _signInManager = signInManager;
        _userManager = userManager;
    }
    public Dictionary<String, String[]> Properties = new Dictionary<string, string[]>();
    public Azure.Pageable<Azure.Storage.Blobs.Models.BlobItem>? pic { get; set; }
    static String connectionString = "DefaultEndpointsProtocol=https;AccountName=freshsightcloud;AccountKey=nnVOWYu0nVMx1pprfPeoktl2PdAsTdmW/iL8Zt/CfqrP3xugfFM72Kpi47/l46qrfhBCIMDMliQ++AStPFLjHw==;EndpointSuffix=core.windows.net";

    public async Task<IActionResult> Index()
    {
        if (_signInManager.IsSignedIn(User))
        {
            var user = await _signInManager.UserManager.GetUserAsync(User);
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
        var ProfilePic = new String[1];
        pic = imagesCli.GetBlobs(prefix: $"{user.Id}_pic");
        String picName = String.Empty;
        if (pic.Count() != 0)
        {
            picName = user.Id;
        }
        else picName = "default";

        ProfilePic[0] = $"https://freshsightcloud.blob.core.windows.net/images/{picName}_pic";

        Properties.Add("PicLink", ProfilePic);
    }


    public void GetUserPosts(AppUser user)
    {
        String containerName = "posts";
        BlobContainerClient postsCli = new BlobContainerClient(connectionString, containerName);
        if (user.Posts != null)
        {
            foreach (var userPost in user.Posts)
            {
                var postProps = new String[3];
                postProps[0] = userPost.ID;
                postProps[1] = userPost.Topic;
                postProps[2] = userPost.Category;
                Properties.Add("Posts", postProps);
            }
        }

    }


    public void GetUserAge(AppUser user)
    {
        if (user.DateOfBirth != null)
        {
            var span = DateTime.Now.Subtract((DateTime)user.DateOfBirth);
            var age = new String[] { (span.Days / 365).ToString() };
            Properties.Add("Age", age);
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}