using Microsoft.AspNetCore.Mvc;
using THYLoggerAPI_POSTGRESQL.Services;

namespace THYLoggerAPI_POSTGRESQL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LatestController : ControllerBase
    {
        private readonly LatestService _latestService;

        public LatestController(LatestService latestService)
        {
            _latestService = latestService;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                return Ok(await _latestService.GetLatestAsync());
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Son veriler alinirken bir hata olustu: " + ex.Message);
            }
        }
    }
}
