using Microsoft.AspNetCore.Mvc;
using OMA.Services;
using OMA.Data;
using OMA.Models.Dto;

namespace OMA.Controllers;

[Route("/api")]
public class ApiController : Controller
{
    private OMADataService _dataService;
    private OMAService _omaService;

    public ApiController(OMADataService dataService)
    {
        _dataService = dataService;
        _omaService = new(dataService);
    }

    public ActionResult Index()
    {
        var match = OMAImportService.GetMatch(111534249, 13);
        return Ok(match);
    }

    [HttpGet("get_alias")]
    public ActionResult GetAlias()
    {
        string? name = Request.Query["name"];
        if (string.IsNullOrEmpty(name))
        {
            return BadRequest("parameter 'name' not provided.");
        }

        var dto = _omaService.GetAliasAsDto(name);

        return dto != null ? Ok(dto) : Ok("alias does not exist.");
    }

    // change to post when things are more concrete.
    [HttpGet("create_alias")]
    public ActionResult CreateAlias()
    {
        string? name = Request.Query["name"];
        if (string.IsNullOrEmpty(name))
        {
            return BadRequest("parameter 'name' not provided.");
        }

        if (_omaService.AliasExists(name))
        {
            return Ok("alias already exists.");
        }

        if (_omaService.CreateAlias(name) == null)
        {
            return Ok("alias could not be created.");
        }

        return Ok("alias created.");
    }

    // change to post(?) when things are more concrete.
    [HttpGet("set_password")]
    public ActionResult SetPassword()
    {
        string? name = Request.Query["name"];
        if (string.IsNullOrEmpty(name))
        {
            return BadRequest("parameter 'name' not provided.");
        }

        string? password = Request.Query["password"];
        if (string.IsNullOrEmpty(password))
        {
            return BadRequest("parameter 'password' not provided.");
        }

        if (!_omaService.AliasExists(name))
        {
            return Ok("alias does not exist.");
        }
        if (_omaService.AliasHasPassword(name))
        {
            return Ok("alias password is already set.");
        }

        if (!_omaService.SetAliasPassword(name, password))
        {
            return Ok("password could not be set.");
        }

        return Ok("password set.");
    }

    // change to post(?) when things are more concrete.
    [HttpGet("unset_password")]
    public ActionResult UnsetPassword()
    {
        string? name = Request.Query["name"];
        if (string.IsNullOrEmpty(name))
        {
            return BadRequest("parameter 'name' not provided.");
        }

        string? password = Request.Query["password"];
        if (string.IsNullOrEmpty(password))
        {
            return BadRequest("parameter 'password' not provided.");
        }

        if (!_omaService.AliasExists(name))
        {
            return Ok("alias does not exist.");
        }

        if (!_omaService.AliasHasPassword(name))
        {
            return Ok("alias does not have a password.");
        }

        if (!_omaService.ValidateAliasPassword(name, password))
        {
            return Ok("given password is invalid.");
        }

        if (!_omaService.UnsetAliasPassword(name))
        {
            return Ok("password could not be unset.");
        }

        return Ok("password unset.");
    }

    // change to post(?) when more concrete.
    [HttpGet("add_lobby")]
    ActionResult AddLobby()
    {
        string? name = Request.Query["name"];
        if (string.IsNullOrEmpty(name))
        {
            return BadRequest("parameter 'name' not provided.");
        }

        string? lobby = Request.Query["lobby"];
        if (string.IsNullOrEmpty(lobby))
        {
            return BadRequest("parameter 'lobby' not provided.");
        }

        int bestOf = 0;
        int warmups = 0;

        string? bestOfStr = Request.Query["bestof"];
        if (!string.IsNullOrEmpty(bestOfStr))
        {
            int.TryParse(bestOfStr, out bestOf);
        }

        string? warmupsStr = Request.Query["warmups"];
        if (!string.IsNullOrEmpty(warmupsStr))
        {
            int.TryParse(warmupsStr, out warmups);
        }

        long lobbyId;

        if (!long.TryParse(lobby, out lobbyId))
        {
            return Ok("lobby value not a number.");
        }

        if (!_omaService.AliasExists(name))
        {
            return Ok("alias does not exist");
        }

        if (!_omaService.AddLobbyToAlias(name, lobbyId, bestOf, warmups))
        {
            return Ok("could not add lobby to alias.");
        }

        return Ok("lobby added to alias.");
    }

    // change to post(?) when more concrete.
    [HttpGet("remove_lobby")]
    ActionResult RemoveLobby()
    {
        string? name = Request.Query["name"];
        if (string.IsNullOrEmpty(name))
        {
            return BadRequest("parameter 'name' not provided.");
        }

        string? lobby = Request.Query["lobby"];
        if (string.IsNullOrEmpty(lobby))
        {
            return BadRequest("parameter 'lobby' not provided.");
        }

        long lobbyId;

        if (!long.TryParse(lobby, out lobbyId))
        {
            return Ok("lobby value not a number.");
        }

        if (!_omaService.AliasExists(name))
        {
            return Ok("alias does not exist");
        }

        if (!_omaService.RemoveLobbyFromAlias(name, lobbyId))
        {
            return Ok("could not remove lobby from alias.");
        }

        return Ok("lobby removed from alias.");
    }
}