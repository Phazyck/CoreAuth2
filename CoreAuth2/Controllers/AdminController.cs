using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace CoreAuth2.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Policy = "CanAccessAdminMethods")]
    public class AdminController : Controller
    {
        // GET api/admin
        [HttpGet]
        public IActionResult Get()
        {
            return Json(HttpContext.Request.Headers);
        }
    }
}