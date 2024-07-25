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

    [HttpGet("get")]
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

    [HttpGet("create")]
    public ActionResult CreateAlias()
    {
        string? name = Request.Query["name"];
        if (name == null)
        {
            return BadRequest("parameter 'name' not provided.");
        }

        var alias = _omaService.CreateAlias(name);

        if (alias == null)
        {
            return Ok("alias exists or could not be created.");
        }
        
        return Ok(alias); 
    }

    [HttpGet("test/createkanealias")]
    public ActionResult CreateKaneAlias()
    {
        return Ok(_omaService.GetOrCreateAlias("kane"));
    }
}
