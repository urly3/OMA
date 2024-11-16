using Microsoft.AspNetCore.Mvc;
using OMA.Core;
using OMA.Core.Data;
using OMA.Core.Services;

namespace OMA.Api.Controllers;

[Route("/api")]
public class ApiController : Controller
{
    private readonly OmaService _omaService;
    private OmaContext _context;

    public ApiController(OmaContext context)
    {
        _context = context;
        _omaService = new OmaService(context);
    }

    public ActionResult Index()
    {
        var dto = _omaService.GetAliasAsDto(OmaUtil.HashString("kane"));
        if (dto != null) return Ok(dto);

        return Ok("no dto");
    }

    [HttpGet("get_alias")]
    public ActionResult GetAlias()
    {
        string? alias = Request.Query["alias"];
        if (string.IsNullOrEmpty(alias)) return BadRequest("parameter 'alias' not provided.");

        alias = OmaUtil.HashString(alias);

        var dto = _omaService.GetAliasAsDto(alias);

        return dto != null ? Ok(dto) : Ok("alias does not exist.");
    }

    // change to post when things are more concrete.
    [HttpGet("create_alias")]
    public ActionResult CreateAlias()
    {
        string? alias = Request.Query["alias"];
        if (string.IsNullOrEmpty(alias)) return BadRequest("parameter 'alias' not provided.");

        alias = OmaUtil.HashString(alias);

        if (_omaService.AliasExists(alias)) return Ok("alias already exists.");

        if (_omaService.CreateAlias(alias) == null) return Ok("alias could not be created.");

        return Ok("alias created.");
    }

    [HttpGet("get_match")]
    public ActionResult GetMatch()
    {
        string? lobby = Request.Query["lobby"];

        if (string.IsNullOrEmpty(lobby)) return Ok("lobby id not provided.");

        var bestOf = 0;
        var warmups = 0;

        string? bestOfStr = Request.Query["best_of"];
        if (!string.IsNullOrEmpty(bestOfStr)) int.TryParse(bestOfStr, out bestOf);

        string? warmupsStr = Request.Query["warmups"];
        if (!string.IsNullOrEmpty(warmupsStr)) int.TryParse(warmupsStr, out warmups);

        long lobbyId;

        if (!long.TryParse(lobby, out lobbyId)) return Ok("lobby value not a valid number.");

        var match = _omaService.GetMatch(lobbyId, bestOf, warmups);

        if (match == null) return Ok("could not get the match.");

        return Ok(match);
    }

    [HttpGet("get_matches")]
    public ActionResult GetMatches()
    {
        string? alias = Request.Query["alias"];

        if (string.IsNullOrEmpty(alias)) return Ok("alias not provided.");

        alias = OmaUtil.HashString(alias);

        var matches = _omaService.GetMatches(alias);
        if (matches == null) return Ok("alias does not exist.");

        return Ok(_omaService.GetMatches(alias));
    }
}