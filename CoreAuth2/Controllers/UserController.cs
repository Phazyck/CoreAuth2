using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace CoreAuth2.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Policy = "CanAccessUserMethods")]
    public class UserController : Controller
    {
        // GET api/user
        [HttpGet]
        public IActionResult Get()
        {
            return Json(HttpContext.Request.Headers);
        }
    }
}