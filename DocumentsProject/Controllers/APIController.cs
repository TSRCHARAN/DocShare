using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DocumentsProject.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting("fixed")]
    public class APIController : ControllerBase
    {
        [HttpPost]
        [Route("post")]
        public async Task<IActionResult> Post()
        {
            return Ok();
        }
    }
}
