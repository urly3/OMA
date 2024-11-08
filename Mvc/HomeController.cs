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
        var dto = CheckAndGetAliasDtoFromCookie();

        Response.Headers["content-type"] = "text/hmtl";
        return Content(RenderStubbleTemplate("Index", new BlankViewModel(dto)), "text/html");
    }

    [HttpGet("viewlobby")]
    public IActionResult ViewLobby()
    {
        var lobbyId = GetLobbyFromQuery();
        if (lobbyId == null)
        {
            return Problem("invalid lobby");
        }

        var dto = CheckAndGetAliasDtoFromCookie();
        var bestOf = GetBestOfFromQuery();
        var warmups = GetWarmupsFromQuery();

        if (bestOf == null || warmups == null)
        {
            return Problem("invalid bestof / warmups");
        }

        var match = _omaService.GetMatch(lobbyId.Value, bestOf.Value, warmups.Value);

        if (match == null)
        {
            return Problem("could not get match from lobby id");
        }
        var viewmodel = new ViewLobbyViewModel(dto, match);

        return Content(RenderStubbleTemplate("ViewLobby", viewmodel), "text/html");
    }

    [HttpGet("addlobby")]
    [HttpPost("addlobby")]
    public IActionResult AddLobby()
    {
        var dto = CheckAndGetAliasDtoFromCookie();
        if (dto != null && dto.Locked)
        {
            return Problem("alias is locked");
        }

        if (Request.Method == "GET")
        {
            return Content(RenderStubbleTemplate("AddLobby", new BlankViewModel(dto)), "text/html");
        }

        var aliasHash = Request.Cookies["alias_hash"];

        if (string.IsNullOrWhiteSpace(aliasHash))
        {
            return Problem("no alias set");
        }

        var lobbyId = GetLobbyFromForm();
        if (lobbyId == null)
        {
            return Problem("lobbyId not a valid number.");
        }

        var bestOf = GetBestOfFromForm();
        var warmups = GetWarmupsFromForm();

        if (bestOf == null || warmups == null)
        {
            return Problem("invalid bestof / warmups");
        }

        var lobbyName = Request.Form["lobby_name"];

        // TODO: flesh this out for all error statuses
        switch (_omaService.AddLobbyToAlias(aliasHash, lobbyId.Value, bestOf.Value, warmups.Value, lobbyName, createIfNull: true))
        {
            case OMAStatus.LobbyAdded:
                return Redirect("/");
            case OMAStatus.AliasContainsLobby:
                return Problem("alias contains existing lobby");
            default:
                return Problem("unhandled error status");
        }
    }

    [HttpGet("removelobby")]
    [HttpPost("removelobby")]
    public IActionResult RemoveLobby()
    {
        var dto = CheckAndGetAliasDtoFromCookie();
        if (dto == null)
        {
            return Problem("invalid alias");
        }

        if (dto.Locked)
        {
            return Problem("alias is locked");
        }

        if (Request.Method == "GET")
        {

            return Content(RenderStubbleTemplate("RemoveLobby", new BlankViewModel(dto)), "text/html");
        }

        long? lobbyId = GetLobbyFromForm();
        if (lobbyId == null)
        {
            return Problem("invalid lobby");
        }

        string? lobbyName = Request.Form["lobby_name"];
        if (lobbyName == null)
        {
            return Problem("lobby name not provided");
        }

        // TODO: flesh this out for all error statuses
        switch (_omaService.RemoveLobbyFromAlias(dto, lobbyId.Value, lobbyName))
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

        Response.Cookies.Append("alias_hash", aliasHash);

        return Redirect("/");
    }

    [HttpPost("lock")]
    [HttpGet("lock")]
    public IActionResult Lock()
    {
        var dto = CheckAndGetAliasDtoFromCookie();
        if (dto == null)
        {
            return Problem("invalid alias");
        }

        if (dto.Locked)
        {
            return Problem("alias is locked");
        }

        if (Request.Method == "GET")
        {
            return Content(RenderStubbleTemplate("Lock", new BlankViewModel(dto)), "text/html");
        }

        var password = GetPasswordFromForm();
        if (password == null)
        {
            return Problem("password not provided");
        }

        var passwordHash = OMAUtil.HashString(password);

        switch (_omaService.SetAliasPassword(dto, passwordHash))
        {
            case OMAStatus.PasswordSet:
                return Redirect("/");
            default:
                return Problem("unhandled error status");
        }
    }

    [HttpPost("unlock")]
    [HttpGet("unlock")]
    public IActionResult Unlock()
    {
        var dto = CheckAndGetAliasDtoFromCookie();
        if (dto == null)
        {
            return Problem("invalid alias");
        }

        if (Request.Method == "GET")
        {
            return Content(RenderStubbleTemplate("Unlock", new BlankViewModel(dto)), "text/html");
        }

        var password = GetPasswordFromForm();
        if (password == null)
        {
            return Problem("password not provided");
        }

        var passwordHash = OMAUtil.HashString(password);

        switch (_omaService.UnsetAliasPassword(dto, passwordHash))
        {
            case OMAStatus.PasswordSet:
                return Redirect("/");
            default:
                return Problem("unhandled error status");
        }
    }

    [Route("error")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private AliasDto? CheckAndGetAliasDtoFromCookie()
    {
        var aliasHash = Request.Cookies["alias_hash"];
        if (string.IsNullOrWhiteSpace(aliasHash))
        {
            return null;
        }

        var dto = _omaService.GetAliasAsDto(aliasHash);

        return dto;
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
        if (string.IsNullOrWhiteSpace(bestOfStr))
        {
            return 0;
        }

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
        if (string.IsNullOrWhiteSpace(bestOfStr))
        {
            return 0;
        }

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
        if (string.IsNullOrWhiteSpace(warmupsStr))
        {
            return 0;
        }

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
        if (string.IsNullOrWhiteSpace(warmupsStr))
        {
            return 0;
        }

        int warmups;

        if (!int.TryParse(warmupsStr, out warmups))
        {
            return null;
        }

        return warmups;
    }

    private string? GetPasswordFromForm()
    {
        var password = Request.Form["password"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(password))
        {
            return null;
        }

        return password;
    }

    private string? GetPasswordFromCookie()
    {
        var passwordHash = Request.Cookies["password_hash"];
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            return null;
        }

        return passwordHash;
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
            @"Footer",
        };
        var partials = new Dictionary<string, string>();

        foreach (var file in filePaths)
        {
            using (var streamReader = new StreamReader(string.Format($@".\wwwroot\templates\{file}.mustache"), Encoding.UTF8))
            {
                partials.Add(file, stubble.Render(streamReader.ReadToEnd(), model));
            }
        }

        // render passed in template
        using (var streamReader = new StreamReader(string.Format($@".\wwwroot\templates\{template}.mustache"), Encoding.UTF8))
        {
            var output = stubble.Render(streamReader.ReadToEnd(), model, partials, null);
            return output;
        }
    }
}
