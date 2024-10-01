using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OMA.Models;
using OMA.Models.Dto;
using OMA.Services;
using OMA.Data;
using OMA.Core;
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
            var aliasDto = CheckAndGetAliasDtoFromCookie();
            return View("AddLobby", aliasDto);
        }

        var aliasHash = Request.Cookies["aliasHash"];
        var lobby = Request.Form["lobby"];

        if (string.IsNullOrWhiteSpace(aliasHash)
            || string.IsNullOrWhiteSpace(lobby)) {
            return BadRequest("no alias set or no lobby provided");
        }

        long lobbyId;
        if (!long.TryParse(lobby, out lobbyId)) {
            return BadRequest("lobbyId not a valid number.");
        }

        var bestOfStr = Request.Form["bestOf"];
        int bestOf = 0;
        var warmupsStr = Request.Form["warmups"];
        int warmups = 0;

        if ((!string.IsNullOrWhiteSpace(bestOfStr)
            && !int.TryParse(bestOfStr, out bestOf))
            || (!string.IsNullOrWhiteSpace(warmupsStr)
            && !int.TryParse(bestOfStr, out warmups))) {

            return BadRequest("invalid values for bestof and/or warmup");
        }

        var lobbyName = Request.Form["lobbyName"];

        // i have an aliashash in the cookies
        // i have a valid best of & warmup (0 as default or user provided)
        // can now ask service to add the lobby to the alias
        // the service will return the relevant status should there be any issues.

        // TODO: flesh this out for all the statuses
        if (_omaService.AddLobbyToAlias(aliasHash, lobbyId, bestOf, warmups, lobbyName, createIfNull: true) != OMAStatus.LobbyAdded) {
            throw new Exception("could not add lobby");
        }
        else {
            return Redirect("/");
        }
    }

    [HttpGet("removelobby")]
    [HttpPost("removelobby")]
    public IActionResult RemoveLobby() {

        if (Request.Method == "GET") {
            var aliasDto = CheckAndGetAliasDtoFromCookie();
            if (aliasDto == null) {
                return BadRequest("invalid alias");
            }

            return View("RemoveLobby", aliasDto);
        }

        var aliasHash = Request.Cookies["aliasHash"];
        if (string.IsNullOrWhiteSpace(aliasHash)) {
            return BadRequest("alias not set");
        }

        long? lobbyId = GetLobbyFromForm();
        if (lobbyId == null) {
            return BadRequest("invalid lobby");
        }

        if (_omaService.RemoveLobbyFromAlias(aliasHash, lobbyId.Value) != OMAStatus.LobbyRemoved) {
            throw new Exception("could not remove lobby");
        }

        return Redirect("/");
    }

    [HttpPost("setalias")]
    public IActionResult SetAlias() {
        string? alias = Request.Form["alias"];

        if (string.IsNullOrWhiteSpace(alias)) {
            return Redirect("/");
        }

        string aliasHash = OMAUtil.HashString(alias);

        Response.Cookies.Append("aliasHash", aliasHash);

        return Redirect("/");
    }

    private AliasDto? CheckAndGetAliasDtoFromCookie() {
        var aliasHash = Request.Cookies["aliasHash"];
        if (string.IsNullOrWhiteSpace(aliasHash)) {
            return null;
        }

        var aliasDto = _omaService.GetAliasAsDto(aliasHash);

        return aliasDto;
    }

    private long? GetLobbyFromForm() {
        var lobby = Request.Form["lobby"];
        long lobbyId;

        if (!long.TryParse(lobby, out lobbyId)) {
            return null;
        }

        return lobbyId;
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
