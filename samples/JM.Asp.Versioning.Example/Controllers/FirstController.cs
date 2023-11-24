using JM.Asp.Versioning.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace JM.Asp.Versioning.Example.Controllers
{
    [ApiVersionInclude(1.0)]
    [ApiController]
    [Route("[controller]")]
    public class FirstController : ControllerBase
    {
        [HttpGet("[action]")]
        public IActionResult A_IncludeOnV1()
        {
            return Ok("A");
        }

        [ApiVersionRemove(2.0)]
        [HttpGet("[action]")]
        public IActionResult B_RemoveOnV2()
        {
            return Ok("B");
        }

        [ApiVersionInclude(2.1)]
        [HttpGet("[action]")]
        public IActionResult C_IncludeOnV2_1()
        {
            return Ok("C");
        }
    }
}
