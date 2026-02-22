using Microsoft.AspNetCore.Mvc;
using EngineeCode.Services;

namespace EngineeCode.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServicesController : ControllerBase
    {
        private readonly IServiceService _serviceService;

        public ServicesController(IServiceService serviceService)
        {
            _serviceService = serviceService;
        }

        // GET /api/services
        // GET /api/services?limit=4
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? limit)
        {
            var services = await _serviceService.GetAllAsync(limit);
            return Ok(services);
        }
    }
}
