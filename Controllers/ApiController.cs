using Microsoft.AspNetCore.Mvc;

namespace OMA.Controllers;

[Route("/api")]
public class ApiController : Controller
{
    // GET: ApiController
    public ActionResult Index()
    {
        return Ok();
    }

}
