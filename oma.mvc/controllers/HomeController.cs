using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using OMA.Models;
using OMA.Models.Dto;
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
        List<LobbyDto> lobbies = [];

        var aliasDto = CheckAndGetAliasDtoFromCookie();

        if (aliasDto != null) {
            lobbies = aliasDto.Lobbies;
        }

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

    [HttpGet("viewaliaslobby")]
    public IActionResult ViewAliasLobby() {
        Internal::Match? match = null;

        string? lobby = Request.Query["lobby"];
        if (string.IsNullOrEmpty(lobby)) {
            return BadRequest("lobby not provided.");
        }

        long lobbyId;
        if (!long.TryParse(lobby, out lobbyId)) {
            return BadRequest("lobbyId not a valid number.");
        }

        var aliasDto = CheckAndGetAliasDtoFromCookie();
        if (aliasDto == null) {
            return BadRequest("alias does not exist.");
        }

        var lobbyDto = aliasDto.Lobbies.FirstOrDefault(l => l.Id == lobbyId);
        if (lobbyDto != null) {
            match = _omaService.GetMatch(lobbyDto.LobbyId, lobbyDto.BestOf, lobbyDto.Warmups);
        }

        if (match == null) {
            return BadRequest("lobby does not exist.");
        }
        
        return View("ViewLobby", match);
    }

    [HttpPost("setalias")]
    public IActionResult SetAlias() {
        return Redirect("");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private AliasDto? CheckAndGetAliasDtoFromCookie() {
        var aliasHash = Request.Cookies["aliasHash"];
        if (string.IsNullOrEmpty(aliasHash)) {
            return null;
        }

        var aliasDto = _omaService.GetAliasAsDto(aliasHash);
        // if (aliasDto == null) {
        //     Response.Cookies.Delete("aliasHash");
        // }

        return aliasDto;
    }
}
