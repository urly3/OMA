using Microsoft.AspNetCore.Mvc;
using OMA.Services;

namespace OMA.Controllers;

[Route("/api")]
public class ApiController : Controller
{
    // GET: ApiController
    public ActionResult Index()
    {
        var match = ImportService.GetMatch(111534249, 13);
        return Ok(match);
    }

}
