using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FreshSight.Models;
using Azure.Storage.Blobs;

namespace FreshSight.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private Microsoft.AspNetCore.Hosting.IHostingEnvironment _env;

    public HomeController(ILogger<HomeController> logger,
    Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [HttpPost]
    public IActionResult UploadFiles()
    {
        String connectionString = "DefaultEndpointsProtocol=https;AccountName=freshsightcloud;AccountKey=nnVOWYu0nVMx1pprfPeoktl2PdAsTdmW/iL8Zt/CfqrP3xugfFM72Kpi47/l46qrfhBCIMDMliQ++AStPFLjHw==;EndpointSuffix=core.windows.net";
        String containerName = "images";
        BlobContainerClient containerClient = new BlobContainerClient(connectionString, containerName);
        
        long size = 0;
        var files = Request.Form.Files;   
        foreach (var file in files)
        {   
            String filename =$"wwwroot/uploaded/{file.Name}";
            size += file.Length;
            using (FileStream fs = System.IO.File.Create(filename))
            {
                file.CopyTo(fs);
                fs.Flush();
            }

            using (MemoryStream ms = new MemoryStream(System.IO.File.ReadAllBytes(filename)))
            {
                containerClient.UploadBlob($"{file.Name}",ms);
                ms.Flush();
                System.IO.File.Delete(filename);
            }
        }
        string message = $"{files.Count} file(s) {size} bytes uploaded successfully!";
        return Json(message);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
