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

    public BlogController(ILogger<BlogController> logger,
    SignInManager<AppUser> signInManager)
    {
        _logger = logger;
        _signInManager = signInManager;
    }
    static String connectionString = "DefaultEndpointsProtocol=https;AccountName=freshsightcloud;AccountKey=nnVOWYu0nVMx1pprfPeoktl2PdAsTdmW/iL8Zt/CfqrP3xugfFM72Kpi47/l46qrfhBCIMDMliQ++AStPFLjHw==;EndpointSuffix=core.windows.net";
    static String containerName = "images";
    BlobContainerClient containerClient = new BlobContainerClient(connectionString, containerName);
    public Dictionary<String, String> Properties = new Dictionary<string, string>();
    public Azure.Pageable<Azure.Storage.Blobs.Models.BlobItem> pic { get; set; }

    public async Task<IActionResult> Index()
    {
        if (_signInManager.IsSignedIn(User))
        {
            String ProfilePic = String.Empty;
            var user = await _signInManager.UserManager.GetUserAsync(User);
            pic = containerClient.GetBlobs(prefix: $"{user.Id}_pic");
            if(user.DateOfBirth != null){
                
                var span = DateTime.Now.Subtract((DateTime)user.DateOfBirth);
                var age = span.Days / 365;
                Properties.Add("Age",age.ToString());
            }

            String picName = String.Empty;
            if (pic.Count() != 0)
            {
                picName = user.Id;
            }
            else picName = "default";

            ProfilePic = $"https://freshsightcloud.blob.core.windows.net/images/{picName}_pic";

            Properties.Add("PicLink", ProfilePic);
            return View(Properties);
        }
        else
        {
            return Redirect("~/Identity/Account/Login");
        }
    }

    public IActionResult Edit()
    {
        return View();
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}