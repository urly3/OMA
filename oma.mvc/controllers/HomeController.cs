using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OMA.Models;
using OMA.Services;
using OMA.Data;

namespace OMA.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private OMADataService _dataService;
    private OMAService _omaService;

    public HomeController(ILogger<HomeController> logger, OMADataService dataService)
    {
        _logger = logger;
        _dataService = dataService;
        _omaService = new(_dataService);
    }

    public IActionResult Index()
    {
        return View(_omaService.GetMatches("kane") ?? []);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
