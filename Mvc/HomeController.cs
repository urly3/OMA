using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using OMA.Core;
using OMA.Core.Data;
using OMA.Core.Models;
using OMA.Core.Models.Dto;
using OMA.Core.Services;
using Stubble.Core.Builders;

namespace OMA.Mvc;

public class HomeController(OmaContext context) : Controller
{
    private readonly OmaService _omaService = new(context);

    [Route("")]
    public IActionResult Index()
    {
        var alias = TryGetAliasFromCookie();
        var dto = new AliasDto(alias);
        var viewmodel = new PageViewModel(dto) { AliasCookieSet = Request.Cookies.ContainsKey("alias") };
        return Content(RenderStubbleTemplate("Index", viewmodel), "text/html");
    }

    [HttpGet("viewlobby")]
    public IActionResult ViewLobby()
    {
        var lobbyId = GetLobbyFromQuery();
        if (lobbyId == null)
            return Problem("invalid lobby", statusCode: 400);

        var alias = TryGetAliasFromCookie();
        var bestOf = GetBestOfFromQuery();
        var warmups = GetWarmupsFromQuery();

        if (bestOf == null || warmups == null)
            return Problem("invalid bestof / warmups", statusCode: 400);

        var match = _omaService.GetMatch(lobbyId.Value, bestOf.Value, warmups.Value);

        if (!match.Some())
            return Problem("could not get match from lobby id");

        var viewmodel = new ViewLobbyViewModel(new AliasDto(alias), match.Value()) { AliasCookieSet = Request.Cookies.ContainsKey("alias") };
        return Content(RenderStubbleTemplate("ViewLobby", viewmodel), "text/html");
    }

    [HttpGet("addlobby")]
    [HttpPost("addlobby")]
    public IActionResult AddLobby()
    {
        var aliasStr = Request.Cookies["alias"];

        if (string.IsNullOrWhiteSpace(aliasStr))
            return Problem("no alias set", statusCode: 400);

        var alias = TryGetAliasFromCookie();

        if (alias != null && alias.Password != null)
            return Problem("alias is locked", statusCode: 400);

        switch (Request.Method)
        {
            case "GET":
                {
                    var viewmodel = new PageViewModel(new AliasDto(alias)) { AliasCookieSet = Request.Cookies.ContainsKey("alias") };
                    return Content(RenderStubbleTemplate("AddLobby", viewmodel), "text/html");
                }
            case "POST":
                {
                    var lobbyId = GetLobbyIdFromForm();
                    if (lobbyId == null)
                        return Problem("lobbyId not a valid number.", statusCode: 400);

                    var bestOf = GetBestOfFromForm();
                    var warmups = GetWarmupsFromForm();

                    if (bestOf == null || warmups == null)
                        return Problem("invalid bestof / warmups", statusCode: 400);

                    var lobbyName = Request.Form["lobby_name"].First();

                    var lobby = TryGetOrCreateLobby(lobbyName, lobbyId.Value, bestOf.Value, warmups.Value);
                    if (lobby == null)
                    {
                        return Problem("invalid lobby", statusCode: 400);
                    }

                    alias ??= _omaService.CreateAlias(aliasStr).Value();

                    // TODO: flesh this out for all error statuses
                    return _omaService.AddLobbyToAlias(alias, lobby) switch
                    {
                        OmaStatus.LobbyAdded => Redirect("/"),
                        OmaStatus.AliasContainsLobby => Problem("alias contains existing lobby"),
                        _ => Problem("error while adding lobby")
                    };
                }
            default:
                return NotFound();
        }
    }

    [HttpGet("removelobby")]
    [HttpPost("removelobby")]
    public IActionResult RemoveLobby()
    {
        var alias = TryGetAliasFromCookie();
        if (alias == null)
            return Problem("invalid alias", statusCode: 400);

        if (alias.Password != null)
            return Problem("alias is locked", statusCode: 400);

        switch (Request.Method)
        {
            case "GET":
                {
                    var viewmodel = new PageViewModel(new AliasDto(alias)) { AliasCookieSet = Request.Cookies.ContainsKey("alias") };
                    return Content(RenderStubbleTemplate("RemoveLobby", viewmodel), "text/html");
                }
            case "POST":
                {
                    var lobbyId = GetLobbyIdFromForm();
                    if (lobbyId == null)
                        return Problem("invalid lobby", statusCode: 400);

                    string? lobbyName = Request.Form["lobby_name"];
                    if (lobbyName == null)
                        return Problem("lobby name not provided", statusCode: 400);

                    var bestOf = GetBestOfFromFormRequired();
                    var warmups = GetWarmupsFromFormRequired();

                    if (bestOf == null || warmups == null)
                        return Problem("invalid bestof / warmups", statusCode: 400);

                    var lobby = TryGetLobby(lobbyName, lobbyId.Value, bestOf.Value, warmups.Value);
                    if (lobby == null)
                    {
                        return Problem("invalid lobby", statusCode: 400);
                    }

                    // TODO: flesh this out for all error statuses
                    return _omaService.RemoveLobbyFromAlias(alias, lobby) switch
                    {
                        OmaStatus.LobbyRemoved => Redirect(Url.Action()!),
                        _ => Problem("error while removing lobby")
                    };
                }
            default:
                return NotFound();
        }
    }

    [HttpPost("setalias")]
    public IActionResult SetAlias()
    {
        string? alias = Request.Form["alias"];

        if (string.IsNullOrWhiteSpace(alias))
            return Redirect("/");

        var aliasHash = OmaUtil.HashString(alias);

        Response.Cookies.Append("alias", alias);

        return Redirect("/");
    }

    [HttpPost("lock")]
    [HttpGet("lock")]
    public IActionResult Lock()
    {
        var alias = TryGetAliasFromCookie();
        if (alias == null)
            return Problem("invalid alias", statusCode: 400);

        if (alias.Password != null)
            return Problem("alias is locked", statusCode: 400);

        switch (Request.Method)
        {
            case "GET":
                {
                    var viewmodel = new PageViewModel(new AliasDto(alias)) { AliasCookieSet = Request.Cookies.ContainsKey("alias") };
                    return Content(RenderStubbleTemplate("Lock", viewmodel), "text/html");
                }
            case "POST":
                {
                    var password = GetPasswordFromForm();
                    if (password == null)
                        return Problem("password not provided", statusCode: 400);

                    var passwordHash = OmaUtil.HashString(password);

                    return _omaService.LockAlias(alias, passwordHash) switch
                    {
                        OmaStatus.PasswordSet => Redirect("/"),
                        _ => Problem("error while locking alias")
                    };
                }
            default:
                return NotFound();
        }
    }

    [HttpPost("unlock")]
    [HttpGet("unlock")]
    public IActionResult Unlock()
    {
        var alias = TryGetAliasFromCookie();
        if (alias == null)
            return Problem("invalid alias", statusCode: 400);

        if (alias.Password == null)
        {
            return Problem("alias is unlocked", statusCode: 400);
        }

        switch (Request.Method)
        {
            case "GET":
                {
                    var viewmodel = new PageViewModel(new AliasDto(alias)) { AliasCookieSet = Request.Cookies.ContainsKey("alias") };
                    return Content(RenderStubbleTemplate("Unlock", viewmodel), "text/html");
                }
            case "POST":
                {
                    var password = GetPasswordFromForm();
                    if (password == null)
                        return Problem("password not provided", statusCode: 400);

                    var passwordHash = OmaUtil.HashString(password);

                    switch (_omaService.UnlockAlias(alias, passwordHash))
                    {
                        case OmaStatus.PasswordSet:
                            return Redirect("/");
                        default:
                            return Problem("error while unlocking alias");
                    }
                }
            default:
                return NotFound();
        }
    }

    [Route("error")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return Problem(
            new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }.ToString());
    }

    // utils //
    private Alias? TryGetAliasFromCookie()
    {
        var aliasStr = Request.Cookies["alias"];
        if (string.IsNullOrWhiteSpace(aliasStr))
            return null;

        var alias = _omaService.GetAlias(aliasStr);

        if (!alias.Some())
        {
            return null;
        }

        return alias.Value();
    }

    private Lobby? TryGetLobby(string lobbyName, long lobbyId, int bestOf, int warmups)
    {
        var existingLobbyResult = _omaService.GetLobby(lobbyName, lobbyId, bestOf, warmups);
        if (existingLobbyResult.Some())
        {
            return existingLobbyResult.Value();
        }

        return null;
    }

    private Lobby? TryGetOrCreateLobby(string? lobbyName, long lobbyId, int bestOf, int warmups)
    {
        if (!_omaService.IsValidLobbyId(lobbyId))
        {
            return null;
        }

        lobbyName ??= _omaService.GetLobbyNameFromLobbyId(lobbyId).Value();

        var existingLobbyResult = _omaService.GetLobby(lobbyName, lobbyId, bestOf, warmups);
        if (existingLobbyResult.Some())
        {
            return existingLobbyResult.Value();
        }

        var lobby = new Lobby
        {
            LobbyName = lobbyName,
            LobbyId = lobbyId,
            BestOf = bestOf,
            Warmups = warmups,
        };

        return lobby;
    }

    private long? GetLobbyIdFromForm()
    {
        var lobby = Request.Form["lobby"].FirstOrDefault();

        if (!long.TryParse(lobby, out var lobbyId))
            return null;

        return lobbyId;
    }

    private long? GetLobbyFromQuery()
    {
        var lobby = Request.Query["lobby"].FirstOrDefault();

        if (!long.TryParse(lobby, out var lobbyId))
            return null;

        return lobbyId;
    }

    private int? GetBestOfFromQuery()
    {
        var bestOfStr = Request.Query["best_of"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(bestOfStr))
            return 0;

        if (!int.TryParse(bestOfStr, out var bestOf))
            return null;

        return bestOf;
    }

    private int? GetBestOfFromForm()
    {
        var bestOfStr = Request.Form["best_of"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(bestOfStr))
            return 0;

        if (!int.TryParse(bestOfStr, out var bestOf))
            return null;

        return bestOf;
    }

    private int? GetBestOfFromFormRequired()
    {
        var bestOfStr = Request.Form["best_of"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(bestOfStr))
            return null;

        if (!int.TryParse(bestOfStr, out var bestOf))
            return null;

        return bestOf;
    }

    private int? GetWarmupsFromQuery()
    {
        var warmupsStr = Request.Query["warmups"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(warmupsStr))
            return 0;

        if (!int.TryParse(warmupsStr, out var warmups))
            return null;

        return warmups;
    }

    private int? GetWarmupsFromForm()
    {
        var warmupsStr = Request.Form["warmups"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(warmupsStr))
            return 0;

        if (!int.TryParse(warmupsStr, out var warmups))
            return null;

        return warmups;
    }

    private int? GetWarmupsFromFormRequired()
    {
        var warmupsStr = Request.Form["warmups"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(warmupsStr))
            return 0;

        if (!int.TryParse(warmupsStr, out var warmups))
            return null;

        return warmups;
    }

    private string? GetPasswordFromForm()
    {
        var password = Request.Form["password"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(password))
            return null;

        return password;
    }

    /*
        private string? GetPasswordFromCookie()
        {
            var passwordHash = Request.Cookies["password_hash"];
            if (string.IsNullOrWhiteSpace(passwordHash))
                return null;

            return passwordHash;
        }
    */

    private string RenderStubbleTemplate(string template, dynamic? model)
    {
        var stubble = new StubbleBuilder().Build();

        // since templates are loaded at runtime on each request, it's hot swappable / reloadable
        // 
        // add global partials
        var filePaths = new[]
        {
            @"NavBar",
            @"Footer"
        };
        var partials = new Dictionary<string, string>();

        foreach (var file in filePaths)
        {
            using var streamReader =
                new StreamReader(string.Format($@".\wwwroot\templates\{file}.mustache", file), Encoding.UTF8);
            partials.Add(file, stubble.Render(streamReader.ReadToEnd(), model));
        }

        // render passed in template
        using (var streamReader =
               new StreamReader(string.Format($@".\wwwroot\templates\{template}.mustache", template), Encoding.UTF8))
        {
            var output = stubble.Render(streamReader.ReadToEnd(), model, partials, null);
            return output;
        }
    }
}
