using Microsoft.AspNetCore.Mvc;
using THYLoggerAPI_POSTGRESQL.DTOs;
using THYLoggerAPI_POSTGRESQL.Services;

namespace THYLoggerAPI_POSTGRESQL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PagesController : ControllerBase
    {
        private readonly PageService _pageService;

        public PagesController(PageService pageService)
        {
            _pageService = pageService;
        }

        // GET: api/Pages
        [HttpGet]
        public async Task<IActionResult> GetAllPages()
        {
            try
            {
                var pages = await _pageService.GetAllPagesAsync();
                return Ok(pages);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Sayfalar alınırken bir hata oluştu: " + ex.Message);
            }
        }

        // POST: api/Pages VEYA POST: api/Pages/Create VEYA POST: api/Pages/Add
        [HttpPost]
        public async Task<IActionResult> CreatePage([FromBody] CreatePageDto dto)
        {
            try
            {
                var result = await _pageService.CreatePageAsync(dto);

                if (!result.IsSuccess)
                {
                    return BadRequest(result.ErrorMessage);
                }

                return Ok(result.ResponseData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Sayfa oluşturulurken sunucu hatası oluştu: " + ex.Message);
            }
        }
    }
}