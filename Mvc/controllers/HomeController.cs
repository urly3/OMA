using Microsoft.AspNetCore.Mvc;
using OMA.Core;
using OMA.Core.Data;
using OMA.Core.Models;
using OMA.Core.Models.Dto;
using OMA.Core.Services;
using Stubble.Core.Builders;
using System.Diagnostics;
using System.Text;

namespace OMA.Mvc.controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private OMAService _omaService;

    public HomeController(ILogger<HomeController> logger, OMAContext context)
    {
        _logger = logger;
        _omaService = new(context);
    }

    [Route("")]
    public IActionResult Index()
    {
        var aliasDto = CheckAndGetAliasDtoFromCookie();

        Response.Headers["content-type"] = "text/hmtl";
        return Content(RenderStubbleTemplate("Index", aliasDto), "text/html");
    }

    [HttpGet("viewlobby")]
    public IActionResult ViewLobby()
    {
        var lobbyId = GetLobbyFromQuery();
        if (lobbyId == null)
        {
            return BadRequest("invalid lobby");
        }

        var dto = CheckAndGetAliasDtoFromCookie();
        var bestOf = GetBestOfFromQuery();
        var warmups = GetWarmupsFromQuery();

        if (bestOf == null || warmups == null)
        {
            return BadRequest("invalid bestof / warmups");
        }

        var match = _omaService.GetMatch(lobbyId.Value, bestOf.Value, warmups.Value);

        if (match == null)
        {
            return BadRequest("could not get match from lobby id");
        }
        var viewmodel = new ViewLobbyViewModel(dto, match);

        return Content(RenderStubbleTemplate("ViewLobby", viewmodel), "text/html");
    }

    [HttpGet("addlobby")]
    [HttpPost("addlobby")]
    public IActionResult AddLobby()
    {
        if (Request.Method == "GET")
        {
            var aliasDto = CheckAndGetAliasDtoFromCookie();
            return Content(RenderStubbleTemplate("AddLobby", aliasDto), "text/html");
        }

        var aliasHash = Request.Cookies["aliasHash"];

        if (string.IsNullOrWhiteSpace(aliasHash))
        {
            return BadRequest("no alias set");
        }

        var lobbyId = GetLobbyFromForm();
        if (lobbyId == null)
        {
            return BadRequest("lobbyId not a valid number.");
        }

        var bestOf = GetBestOfFromForm();
        var warmups = GetWarmupsFromForm();

        if (bestOf == null || warmups == null)
        {
            return BadRequest("invalid bestof / warmups");
        }

        var lobbyName = Request.Form["lobby_name"];

        // TODO: flesh this out for all error statuses
        switch (_omaService.AddLobbyToAlias(aliasHash, lobbyId.Value, bestOf.Value, warmups.Value, lobbyName, createIfNull: true))
        {
            case OMAStatus.LobbyAdded:
                return Redirect("/");
            case OMAStatus.AliasContainsLobby:
                return BadRequest("alias contains existing lobby");
            default:
                return Problem("unhandled error status");
        }
    }

    [HttpGet("removelobby")]
    [HttpPost("removelobby")]
    public IActionResult RemoveLobby()
    {

        if (Request.Method == "GET")
        {
            var aliasDto = CheckAndGetAliasDtoFromCookie();
            if (aliasDto == null)
            {
                return BadRequest("invalid alias");
            }

            return Content(RenderStubbleTemplate("RemoveLobby", aliasDto), "text/html");
        }

        var aliasHash = Request.Cookies["aliasHash"];
        if (string.IsNullOrWhiteSpace(aliasHash))
        {
            return BadRequest("alias not set");
        }

        long? lobbyId = GetLobbyFromForm();
        if (lobbyId == null)
        {
            return BadRequest("invalid lobby");
        }

        string? lobbyName = Request.Form["lobby_name"];
        if (lobbyName == null)
        {
            return BadRequest("lobby name not provided");
        }

        // TODO: flesh this out for all error statuses
        switch (_omaService.RemoveLobbyFromAlias(aliasHash, lobbyId.Value, lobbyName))
        {
            case OMAStatus.LobbyRemoved:
                return Redirect("/");
            default:
                return Problem("unhandled error status");
        }
    }

    [HttpPost("setalias")]
    public IActionResult SetAlias()
    {
        string? alias = Request.Form["alias"];

        if (string.IsNullOrWhiteSpace(alias))
        {
            return Redirect("/");
        }

        string aliasHash = OMAUtil.HashString(alias);

        Response.Cookies.Append("aliasHash", aliasHash);

        return Redirect("/");
    }

    [Route("error")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private AliasDto? CheckAndGetAliasDtoFromCookie()
    {
        var aliasHash = Request.Cookies["aliasHash"];
        if (string.IsNullOrWhiteSpace(aliasHash))
        {
            return null;
        }

        var aliasDto = _omaService.GetAliasAsDto(aliasHash);

        return aliasDto;
    }

    private long? GetLobbyFromForm()
    {
        var lobby = Request.Form["lobby"].FirstOrDefault();
        long lobbyId;

        if (!long.TryParse(lobby, out lobbyId))
        {
            return null;
        }

        return lobbyId;
    }

    private long? GetLobbyFromQuery()
    {
        var lobby = Request.Query["lobby"].FirstOrDefault();
        long lobbyId;

        if (!long.TryParse(lobby, out lobbyId))
        {
            return null;
        }

        return lobbyId;
    }

    private int? GetBestOfFromQuery()
    {
        var bestOfStr = Request.Query["best_of"].FirstOrDefault();
        int bestOf;

        if (!int.TryParse(bestOfStr, out bestOf))
        {
            return null;
        }

        return bestOf;
    }

    private int? GetBestOfFromForm()
    {
        var bestOfStr = Request.Form["best_of"].FirstOrDefault();
        int bestOf;

        if (!int.TryParse(bestOfStr, out bestOf))
        {
            return null;
        }

        return bestOf;
    }

    private int? GetWarmupsFromQuery()
    {
        var warmupsStr = Request.Query["warmups"].FirstOrDefault();
        int warmups;

        if (!int.TryParse(warmupsStr, out warmups))
        {
            return null;
        }

        return warmups;
    }

    private int? GetWarmupsFromForm()
    {
        var warmupsStr = Request.Form["warmups"].FirstOrDefault();
        int warmups;

        if (!int.TryParse(warmupsStr, out warmups))
        {
            return null;
        }

        return warmups;
    }

    private string RenderStubbleTemplate(string template, dynamic? model)
    {
        var stubble = new StubbleBuilder().Build();

        // since templates are loaded at runtime on each request, it's hot swappable / reloadable
        // 
        // add global partials
        var filePaths = new string[]
        {
            @"NavBar",
        };
        var partials = new Dictionary<string, string>();

        foreach (var file in filePaths)
        {
            using (var streamReader = new StreamReader(string.Format($@".\Mvc\Views\Mustache\{file}.mustache"), Encoding.UTF8))
            {
                partials.Add(file, stubble.Render(streamReader.ReadToEnd(), model));
            }
        }

        // render passed in template
        using (var streamReader = new StreamReader(string.Format($@".\Mvc\Views\Mustache\{template}.mustache"), Encoding.UTF8))
        {
            var output = stubble.Render(streamReader.ReadToEnd(), model, partials, null);
            return output;
        }
    }
}
