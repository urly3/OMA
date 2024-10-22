using Microsoft.AspNetCore.Mvc;
using OMA.Core;
using OMA.Core.Data;
using OMA.Core.Models.Dto;
using OMA.Core.Services;

namespace OMA.Api.Controllers;

[Route("/api")]
public class ApiController : Controller
{
    private OMAContext _context;
    private OMAService _omaService;

    public ApiController(OMAContext context)
    {
        _context = context;
        _omaService = new(context);
    }

    public ActionResult Index()
    {
        AliasDto? dto = _omaService.GetAliasAsDto(OMAUtil.HashString("kane"));
        if (dto != null)
        {
            return Ok(dto);
        }

        return Ok("no dto");
    }

    [HttpGet("get_alias")]
    public ActionResult GetAlias()
    {
        string? alias = Request.Query["alias"];
        if (string.IsNullOrEmpty(alias))
        {
            return BadRequest("parameter 'alias' not provided.");
        }

        alias = OMAUtil.HashString(alias);

        var dto = _omaService.GetAliasAsDto(alias);

        return dto != null ? Ok(dto) : Ok("alias does not exist.");
    }

    // change to post when things are more concrete.
    [HttpGet("create_alias")]
    public ActionResult CreateAlias()
    {
        string? alias = Request.Query["alias"];
        if (string.IsNullOrEmpty(alias))
        {
            return BadRequest("parameter 'alias' not provided.");
        }

        alias = OMAUtil.HashString(alias);

        if (_omaService.AliasExists(alias))
        {
            return Ok("alias already exists.");
        }

        if (_omaService.CreateAlias(alias) == null)
        {
            return Ok("alias could not be created.");
        }

        return Ok("alias created.");
    }

    // change to post(?) when things are more concrete.
    [HttpGet("set_password")]
    public ActionResult SetPassword()
    {
        string? alias = Request.Query["alias"];
        if (string.IsNullOrEmpty(alias))
        {
            return BadRequest("parameter 'alias' not provided.");
        }

        alias = OMAUtil.HashString(alias);

        string? password = Request.Query["password"];
        if (string.IsNullOrEmpty(password))
        {
            return BadRequest("parameter 'password' not provided.");
        }

        password = OMAUtil.HashString(password);

        switch (_omaService.SetAliasPassword(alias, password))
        {
            case OMAStatus.AliasDoesNotExist: return BadRequest("alias does not exist.");
            case OMAStatus.AliasIsLocked: return BadRequest("alias is locked.");
            case OMAStatus.PasswordCouldNotBeSet: throw new Exception("password could not be set.");
            case OMAStatus.PasswordSet: return Ok("password set.");
            default: throw new Exception("unhandled state.");
        }
    }

    // change to post(?) when things are more concrete.
    [HttpGet("unset_password")]
    public ActionResult UnsetPassword()
    {
        string? alias = Request.Query["alias"];
        if (string.IsNullOrEmpty(alias))
        {
            return BadRequest("parameter 'alias' not provided.");
        }

        alias = OMAUtil.HashString(alias);

        string? password = Request.Query["password"];
        if (string.IsNullOrEmpty(password))
        {
            return BadRequest("parameter 'password' not provided.");
        }

        password = OMAUtil.HashString(password);

        switch (_omaService.UnsetAliasPassword(alias))
        {
            case OMAStatus.AliasDoesNotExist: return BadRequest("alias does not exist.");
            case OMAStatus.AliasIsUnlocked: return BadRequest("alias is already unlocked.");
            case OMAStatus.PasswordCouldNotBeSet: throw new Exception("password could not unset.");
            case OMAStatus.PasswordSet: return Ok("password unset.");
            default: throw new Exception("unhandled state.");
        }
    }

    [HttpGet("get_match")]
    public ActionResult GetMatch()
    {
        string? lobby = Request.Query["lobby"];

        if (string.IsNullOrEmpty(lobby))
        {
            return Ok("lobby id not provided.");
        }

        int bestOf = 0;
        int warmups = 0;

        string? bestOfStr = Request.Query["best_of"];
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
            return Ok("lobby value not a valid number.");
        }

        var match = _omaService.GetMatch(lobbyId, bestOf, warmups);

        if (match == null)
        {
            return Ok("could not get the match.");
        }

        return Ok(match);
    }

    [HttpGet("get_matches")]
    public ActionResult GetMatches()
    {
        string? alias = Request.Query["alias"];

        if (string.IsNullOrEmpty(alias))
        {
            return Ok("alias not provided.");
        }

        alias = OMAUtil.HashString(alias);

        var matches = _omaService.GetMatches(alias);
        if (matches == null)
        {
            return Ok("alias does not exist.");
        }

        return Ok(_omaService.GetMatches(alias));
    }
}
