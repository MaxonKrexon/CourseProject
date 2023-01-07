using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FreshSight.Models;
using Azure.Storage.Blobs;
using FreshSight.Data;
using Azure.Storage.Blobs.Specialized;

namespace FreshSight.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _db;

    public HomeController(ILogger<HomeController> logger,
    ApplicationDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    public IActionResult Index()
    {
        var posts = _db.Posts.ToList();
        return View(posts);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Find(String question)
    {
        List<Post> searchResult = new List<Post>();
        var posts = _db.Posts.Where(p => p.Topic.ToLower().Contains(question.ToLower())).ToList();
        
        searchResult.Concat(posts);
        String containerName = "posts";
        String connectionString = "DefaultEndpointsProtocol=https;AccountName=freshsightcloud;AccountKey=nnVOWYu0nVMx1pprfPeoktl2PdAsTdmW/iL8Zt/CfqrP3xugfFM72Kpi47/l46qrfhBCIMDMliQ++AStPFLjHw==;EndpointSuffix=core.windows.net";
        BlobContainerClient cloud = new BlobContainerClient(connectionString, containerName);
        var blobs = cloud.GetBlobs();
        foreach(var blob in blobs){
            if(blob.Name.Contains("text")){
                var textBlock = new BlockBlobClient(connectionString, containerName, blob.Name);
                var textBlob = textBlock.DownloadContent();
                var text = textBlob.Value.Content.ToString().ToLower();
                if(text.Contains(question.ToLower())){
                    var post = _db.Posts.Where(p => p.ID == (blob.Name.Replace("_text",String.Empty))).First();
                    searchResult.Add(post);
                }
            }
        }
        return View(searchResult);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
