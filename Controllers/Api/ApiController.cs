using Microsoft.AspNetCore.Mvc;
using OMA.Services;
using OMA.Data;

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
        var match = ImportService.GetMatch(111534249, 13);
        return Ok(match);
    }

    [HttpGet("get_alias")]
    public ActionResult GetAlias()
    {
        string? name = Request.Query["name"];
        if (name == null)
        {
            return BadRequest("parameter 'name' not provided.");
        }

        var alias = _omaService.GetAlias(name);

        return alias != null ? Ok(alias) : Ok("alias does not exist.");
    }

    // change to post when things are more concrete.
    [HttpGet("create_alias")]
    public ActionResult CreateAlias()
    {
        string? name = Request.Query["name"];
        if (name == null)
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
        else
        {
            return Ok("alias created.");
        }
    }

    // change to post when things are more concrete.
    [HttpGet("set_password")]
    public ActionResult SetPassword()
    {
        string? name = Request.Query["name"];
        if (name == null)
        {
            return BadRequest("parameter 'name' not provided.");
        }

        string? password = Request.Query["password"];
        if (password == null)
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

    //////////////////

    [HttpGet("test/createkanealias")]
    public ActionResult CreateKaneAlias()
    {
        return Ok(_omaService.GetOrCreateAlias("kane"));
    }
}
