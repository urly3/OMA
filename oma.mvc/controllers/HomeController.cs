using System.Diagnostics;
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
        var aliasDto = CheckAndGetAliasDtoFromCookie();

        return View(aliasDto);
    }

    [HttpGet("viewlobby")]
    public IActionResult ViewLobby() {
        Internal::Match? match = null;

        string? lobby = Request.Query["lobby"];
        if (string.IsNullOrWhiteSpace(lobby)) {
            return BadRequest("lobby not provided.");
        }

        long lobbyId;
        if (!long.TryParse(lobby, out lobbyId)) {
            return BadRequest("lobbyId not a valid number.");
        }

        match = _omaService.GetMatch(lobbyId, 0, 0);

        return View(match);
    }

    [HttpGet("addlobby")]
    [HttpPost("addlobby")]
    public IActionResult AddLobby() {

        if (Request.Method == "GET") {
            return View("AddLobby");
        } else {
            var aliasHash = Request.Cookies["aliasHash"];
            var lobbyId = Request.Form["lobbyId"];

            if (string.IsNullOrWhiteSpace(aliasHash)
                || string.IsNullOrWhiteSpace(lobbyId)) {
                return BadRequest("invalid request: no alias set or no lobby provided");
            }

            var bestOfStr = Request.Form["bestOf"];
            var warmupsStr = Request.Form["warmups"];

            if (!int.TryParse(bestOfStr, out int bestOf)
                || !int.TryParse(warmupsStr, out int warmups)) {
                return BadRequest("invalid request: bestof or warmups not valid");
            }

            // i have an aliashash in the cookies
            // i have a valid best of & warmup (0 as default or user provided)
            // can now ask service to add the lobby to the alias
            // the service will return the relevant status should there be any issues.

            return Redirect("/");
        }
    }

    [HttpGet("removelobby")]
    [HttpPost("removelobby")]
    public IActionResult RemoveLobby() {

        if (Request.Method == "GET") {
            return View("RemoveLobby");
        } else {
            return Redirect("/");
        }
    }

    [HttpPost("setalias")]
    public IActionResult SetAlias() {
        string? alias = Request.Form["alias"];

        if (string.IsNullOrWhiteSpace(alias)) {
            return Redirect("/");
        }

        string aliasHash = _omaService.HashString(alias);

        Response.Cookies.Append("aliasHash", aliasHash);

        return Redirect("/");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private AliasDto? CheckAndGetAliasDtoFromCookie() {
        var aliasHash = Request.Cookies["aliasHash"];
        if (string.IsNullOrWhiteSpace(aliasHash)) {
            return null;
        }

        var aliasDto = _omaService.GetAliasAsDtoFromHash(aliasHash);

        return aliasDto;
    }
}
