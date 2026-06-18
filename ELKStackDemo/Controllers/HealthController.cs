using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace ELKStackDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        private readonly ILogger<HealthController> _logger;

        public HealthController(ILogger<HealthController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {
            _logger.LogInformation("Health check endpoint called at {Timestamp}", DateTime.UtcNow);
            _logger.LogWarning("This is a sample warning log");

            return Ok(new
            {
                Status = "Healthy",
                Message = "ELKStackDemo API is running with Serilog",
                Timestamp = DateTime.UtcNow
            });
        }
    }
}