using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OMA.Models;
using OMA.Services;
using OMA.Data;
using Internal = OMA.Models.Internal;

namespace OMA.Controllers;

public class HomeController : Controller {
    private readonly ILogger<HomeController> _logger;
    private OMAContext _context;
    private OMAService _omaService;

    public HomeController(ILogger<HomeController> logger, OMAContext context) {
        _logger = logger;
        _context = context;
        _omaService = new(_context);
    }

    public IActionResult Index() {
        var lobbies = _omaService.GetAliasAsDto("kane")?.Lobbies;
        return View(lobbies);
    }

    [HttpGet("viewlobby")]
    public IActionResult ViewLobby() {
        Internal::Match? match = null;

        string? lobby = Request.Query["lobby"];
        if (string.IsNullOrEmpty(lobby)) {
            return BadRequest("lobby not provided.");
        }

        long lobbyId;
        if (!long.TryParse(lobby, out lobbyId)) {
            return BadRequest("lobbyId not a valid number.");
        }

        match = _omaService.GetMatch(lobbyId, 0, 0);

        return View(match);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
