using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FreshSight.Models;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Identity;

namespace FreshSight.Controllers;

public class AccountController : Controller
{
    private readonly ILogger<AccountController> _logger;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager;

    public String StatusMessage = String.Empty;
    static String connectionString = "DefaultEndpointsProtocol=https;AccountName=freshsightcloud;AccountKey=nnVOWYu0nVMx1pprfPeoktl2PdAsTdmW/iL8Zt/CfqrP3xugfFM72Kpi47/l46qrfhBCIMDMliQ++AStPFLjHw==;EndpointSuffix=core.windows.net";
    static String containerName = "images";
    BlobContainerClient containerClient = new BlobContainerClient(connectionString, containerName);



    public AccountController(ILogger<AccountController> logger,
    SignInManager<AppUser> signInManager,
    UserManager<AppUser> userManager)
    {
        _logger = logger;
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpPost]
    public void UploadFiles()
    {
        var files = Request.Form.Files;
        if (files.Count == 1)
        {


            var file = files[0];
            String filename = $"wwwroot/uploaded/{file.Name}";

            using (FileStream fs = System.IO.File.Create(filename))
            {
                file.CopyTo(fs);
                fs.Flush();
            }

            using (MemoryStream ms = new MemoryStream(System.IO.File.ReadAllBytes(filename)))
            {
                String CurrentUserId = _userManager.GetUserId(User);
                String BlobName = $"{CurrentUserId}_pic";

                containerClient.DeleteBlobIfExists(blobName: BlobName);
                containerClient.UploadBlob(blobName: BlobName, content: ms);

                ms.Flush();
                System.IO.File.Delete(filename);
            }

            StatusMessage = $"Your file uploaded successfully!";
        }
        else
        {
            StatusMessage = "You can upload only 1 file!";
        }
    }

    [HttpPost]
    public void Delete()
    {
        String CurrentUserId = _userManager.GetUserId(User);
        String BlobName = $"{CurrentUserId}_pic";
        containerClient.DeleteBlobIfExists(blobName: BlobName);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
