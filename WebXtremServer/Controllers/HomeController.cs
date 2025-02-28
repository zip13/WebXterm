using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FmCndoeServer.Models;

namespace FmCndoeServer.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly NodeInfo _nodeInfo;
    public HomeController(ILogger<HomeController> logger,NodeInfo nodeInfo)
    {
        _logger = logger;
        _nodeInfo = nodeInfo;
    }

    public IActionResult Index()
    {
        ViewData["Title"] = "Home";
        ViewData["NodeInfo"] = _nodeInfo;
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
