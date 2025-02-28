using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FmCndoeServer.Models;

namespace FmCndoeServer.Controllers;

public class XtermController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly NodeInfo _nodeInfo;
    public XtermController(ILogger<HomeController> logger,NodeInfo nodeInfo)
    {
        _logger = logger;
        _nodeInfo = nodeInfo;
    }

    public IActionResult Index()
    {

        
        ViewData["Layout"] = "_XtermLayout";
        ViewData["NodeInfo"] = _nodeInfo;
        return View();
    }


}
