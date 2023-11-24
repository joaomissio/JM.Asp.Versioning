using JM.Asp.Versioning.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace JM.Asp.Versioning.Example.Controllers
{
    [ApiVersionInclude(2.0)]
    [ApiController]
    [Route("[controller]")]
    public class SecondController : ControllerBase
    {
        [HttpGet("[action]")]
        public IActionResult A_IncludeOnV2()
        {
            return Ok("A");
        }

        [ApiVersionRemove(3.0)]
        [HttpGet("[action]")]
        public IActionResult B_RemoveOnV3()
        {
            return Ok("B");
        }

        [ApiVersionInclude(3.0)]
        [HttpGet("[action]")]
        public IActionResult C_IncludeOnV3()
        {
            return Ok("C");
        }
    }
}
