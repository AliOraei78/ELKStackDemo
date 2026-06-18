using Microsoft.AspNetCore.Mvc;

namespace ELKStackDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                Status = "Healthy",
                Message = "ELKStackDemo API is running",
                Timestamp = DateTime.UtcNow
            });
        }
    }
}