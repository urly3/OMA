using Microsoft.AspNetCore.Mvc;
using OMA.Services;
using OMA.Data;
using OMA.Models.Dto;

namespace OMA.Controllers;

[Route("/api")]
public class ApiController : Controller {
    private OMADataService _dataService;
    private OMAService _omaService;

    public ApiController(OMADataService dataService) {
        _dataService = dataService;
        _omaService = new(dataService);
    }

    public ActionResult Index() {
        AliasDto? dto = _omaService.GetAliasAsDto("kane");
        if (dto != null) {
            return Ok(dto);
        }

        return Ok("no dto");
    }

    [HttpGet("get_alias")]
    public ActionResult GetAlias() {
        string? alias = Request.Query["alias"];
        if (string.IsNullOrEmpty(alias)) {
            return BadRequest("parameter 'alias' not provided.");
        }

        var dto = _omaService.GetAliasAsDto(alias);

        return dto != null ? Ok(dto) : Ok("alias does not exist.");
    }

    // change to post when things are more concrete.
    [HttpGet("create_alias")]
    public ActionResult CreateAlias() {
        string? alias = Request.Query["alias"];
        if (string.IsNullOrEmpty(alias)) {
            return BadRequest("parameter 'alias' not provided.");
        }

        if (_omaService.AliasExists(alias)) {
            return Ok("alias already exists.");
        }

        if (_omaService.CreateAlias(alias) == null) {
            return Ok("alias could not be created.");
        }

        return Ok("alias created.");
    }

    // change to post(?) when things are more concrete.
    [HttpGet("set_password")]
    public ActionResult SetPassword() {
        string? alias = Request.Query["alias"];
        if (string.IsNullOrEmpty(alias)) {
            return BadRequest("parameter 'alias' not provided.");
        }

        string? password = Request.Query["password"];
        if (string.IsNullOrEmpty(password)) {
            return BadRequest("parameter 'password' not provided.");
        }

        if (!_omaService.AliasExists(alias)) {
            return Ok("alias does not exist.");
        }
        if (_omaService.AliasHasPassword(alias)) {
            return Ok("alias password is already set.");
        }

        if (!_omaService.SetAliasPassword(alias, password)) {
            return Ok("password could not be set.");
        }

        return Ok("password set.");
    }

    // change to post(?) when things are more concrete.
    [HttpGet("unset_password")]
    public ActionResult UnsetPassword() {
        string? alias = Request.Query["alias"];
        if (string.IsNullOrEmpty(alias)) {
            return BadRequest("parameter 'alias' not provided.");
        }

        string? password = Request.Query["password"];
        if (string.IsNullOrEmpty(password)) {
            return BadRequest("parameter 'password' not provided.");
        }

        if (!_omaService.AliasExists(alias)) {
            return Ok("alias does not exist.");
        }

        if (!_omaService.AliasHasPassword(alias)) {
            return Ok("alias does not have a password.");
        }

        if (!_omaService.ValidateAliasPassword(alias, password)) {
            return Ok("given password is invalid.");
        }

        if (!_omaService.UnsetAliasPassword(alias)) {
            return Ok("password could not be unset.");
        }

        return Ok("password unset.");
    }

    // change to post(?) when more concrete.
    [HttpGet("add_lobby")]
    public ActionResult AddLobby() {
        string? alias = Request.Query["alias"];
        if (string.IsNullOrEmpty(alias)) {
            return BadRequest("parameter 'alias' not provided.");
        }

        string? lobby = Request.Query["lobby"];
        if (string.IsNullOrEmpty(lobby)) {
            return BadRequest("parameter 'lobby' not provided.");
        }

        int bestOf = 0;
        int warmups = 0;

        string? bestOfStr = Request.Query["bestof"];
        if (!string.IsNullOrEmpty(bestOfStr)) {
            int.TryParse(bestOfStr, out bestOf);
        }

        string? warmupsStr = Request.Query["warmups"];
        if (!string.IsNullOrEmpty(warmupsStr)) {
            int.TryParse(warmupsStr, out warmups);
        }

        long lobbyId;

        if (!long.TryParse(lobby, out lobbyId)) {
            return Ok("lobby value not a valid number.");
        }

        if (!_omaService.AliasExists(alias)) {
            return Ok("alias does not exist");
        }

        if (_omaService.AliasHasPassword(alias)) {
            return Ok("alias is locked.");
        }

        if (!_omaService.AddLobbyToAlias(alias, lobbyId, bestOf, warmups)) {
            return Ok("could not add lobby to alias.");
        }

        return Ok("lobby added to alias.");
    }

    // change to post(?) when more concrete.
    [HttpGet("remove_lobby")]
    public ActionResult RemoveLobby() {
        string? alias = Request.Query["alias"];
        if (string.IsNullOrEmpty(alias)) {
            return BadRequest("parameter 'alias' not provided.");
        }

        string? lobby = Request.Query["lobby"];
        if (string.IsNullOrEmpty(lobby)) {
            return BadRequest("parameter 'lobby' not provided.");
        }

        long lobbyId;

        if (!long.TryParse(lobby, out lobbyId)) {
            return Ok("lobby value not a valid number.");
        }

        if (!_omaService.AliasExists(alias)) {
            return Ok("alias does not exist");
        }

        if (_omaService.AliasHasPassword(alias)) {
            return Ok("alias is locked.");
        }

        if (!_omaService.RemoveLobbyFromAlias(alias, lobbyId)) {
            return Ok("could not remove lobby from alias.");
        }

        return Ok("lobby removed from alias.");
    }

    [HttpGet("get_match")]
    public ActionResult GetMatch() {
        string? lobby = Request.Query["lobby"];

        if (string.IsNullOrEmpty(lobby)) {
            return Ok("lobby id not provided.");
        }

        int bestOf = 0;
        int warmups = 0;

        string? bestOfStr = Request.Query["bestof"];
        if (!string.IsNullOrEmpty(bestOfStr)) {
            int.TryParse(bestOfStr, out bestOf);
        }

        string? warmupsStr = Request.Query["warmups"];
        if (!string.IsNullOrEmpty(warmupsStr)) {
            int.TryParse(warmupsStr, out warmups);
        }

        long lobbyId;

        if (!long.TryParse(lobby, out lobbyId)) {
            return Ok("lobby value not a valid number.");
        }

        var match = _omaService.GetMatch(lobbyId, bestOf, warmups);

        if (match == null) {
            return Ok("could not get the match.");
        }

        return Ok(match);
    }

    [HttpGet("get_matches")]
    public ActionResult GetMatches() {
        string? alias = Request.Query["alias"];

        if (string.IsNullOrEmpty(alias)) {
            return Ok("alias not provided.");
        }

        var matches = _omaService.GetMatches(alias);
        if (matches == null) {
            return Ok("alias does not exist.");
        }

        return Ok(_omaService.GetMatches(alias));
    }
}
