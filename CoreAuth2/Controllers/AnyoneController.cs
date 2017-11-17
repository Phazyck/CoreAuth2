using Microsoft.AspNetCore.Mvc;

namespace CoreAuth2.Controllers
{
    [Route("api/[controller]")]
    public class AnyoneController : Controller
    {
        // GET api/anyone
        [HttpGet]
        public IActionResult Get()
        {
            return Json(HttpContext.Request.Headers);
        }
    }
}