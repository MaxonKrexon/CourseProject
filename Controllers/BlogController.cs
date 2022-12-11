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
    static String connectionString = "DefaultEndpointsProtocol=https;AccountName=freshsightcloud;AccountKey=nnVOWYu0nVMx1pprfPeoktl2PdAsTdmW/iL8Zt/CfqrP3xugfFM72Kpi47/l46qrfhBCIMDMliQ++AStPFLjHw==;EndpointSuffix=core.windows.net";
    static String containerName = "images";
    BlobContainerClient containerClient = new BlobContainerClient(connectionString, containerName);
    public Dictionary<String, String> Properties = new Dictionary<string, string>();
    public Azure.Pageable<Azure.Storage.Blobs.Models.BlobItem>? pic { get; set; }

    public async Task<IActionResult> Index()
    {
        if (_signInManager.IsSignedIn(User))
        {
            String containerName = "images";
            BlobContainerClient containerClient = new BlobContainerClient(connectionString, containerName);
            var user = await _signInManager.UserManager.GetUserAsync(User);

            if (user.DateOfBirth != null)
            {
                var span = DateTime.Now.Subtract((DateTime)user.DateOfBirth);
                var age = span.Days / 365;
                Properties.Add("Age", age.ToString());
            }

            String ProfilePic = String.Empty;
            pic = containerClient.GetBlobs(prefix: $"{user.Id}_pic");
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

    public IActionResult Post(String Topic, String Category, String Text)
    {
        return View();
    }

    [HttpPost]
    public async void PostContent()
    {
        String containerName = "posts";
        BlobContainerClient containerClient = new BlobContainerClient(connectionString, containerName);

        var user = await _signInManager.UserManager.GetUserAsync(User);

        Post post = new Post();
        Guid postId = new Guid();
        post.ID = postId.ToString();
        post.Author = user;

        var files = Request.Form.Files;

        int index = 0;
        foreach (var file in files)
        {

            String filename = $"wwwroot/uploaded/{file.Name}";

            using (FileStream fs = System.IO.File.Create(filename))
            {
                file.CopyTo(fs);
                fs.Flush();
            }

            using (MemoryStream ms = new MemoryStream(System.IO.File.ReadAllBytes(filename)))
            {
                String BlobName = String.Empty;
                if (file.Name == "thumbnail")
                {
                    BlobName = $"{postId}_thumb";
                }
                else BlobName = $"{postId}_{index}";

                containerClient.DeleteBlobIfExists(blobName: BlobName);
                containerClient.UploadBlob(blobName: BlobName, content: ms);

                ms.Flush();
                System.IO.File.Delete(filename);
            }
            index++;
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}