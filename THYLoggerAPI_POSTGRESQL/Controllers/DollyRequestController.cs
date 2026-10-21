using Microsoft.AspNetCore.Mvc;
using THYLoggerAPI_POSTGRESQL.DTOs;
using THYLoggerAPI_POSTGRESQL.Services;

namespace THYLoggerAPI_POSTGRESQL.Controllers
{
    /// <summary>
    /// Panel (liste ekranı) için: Dolly içindeki ULD / AWB detaylarını döner.
    /// Mevcut DollyController ile aynı stilde; API seviyesinde ek yetki yoktur.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class DollyRequestController : ControllerBase
    {
        private readonly CoolDollyRequestService _service;

        public DollyRequestController(CoolDollyRequestService service)
        {
            _service = service;
        }

        // GET /api/DollyRequest/GetAll?status=Open&coolDollyNo=TDC&awbNo=235&page=1&pageSize=50
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll([FromQuery] DollyRequestFilter filter)
        {
            try
            {
                var result = await _service.GetAllAsync(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Dolly talepleri alınırken bir hata oluştu: " + ex.Message);
            }
        }

        [HttpGet("GetById/{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var item = await _service.GetByIdAsync(id);
                if (item == null)
                {
                    return NotFound("Dolly talebi bulunamadı.");
                }

                return Ok(item);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Dolly talebi alınırken bir hata oluştu: " + ex.Message);
            }
        }

        // Ana ekran kartları için: tüm Dolly'lerin içindeki açık ULD/AWB/SCC kayıtları
        [HttpGet("GetOpen")]
        public async Task<IActionResult> GetOpen()
        {
            try
            {
                var list = await _service.GetOpenAllAsync();
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Açık Dolly talepleri alınırken bir hata oluştu: " + ex.Message);
            }
        }

        // Bir Dolly'nin içinde şu an bulunan ULD/AWB kayıtları
        [HttpGet("GetByDolly/{dollyId:int}")]
        public async Task<IActionResult> GetByDolly(int dollyId)
        {
            try
            {
                var list = await _service.GetOpenByDollyAsync(dollyId);
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Dolly içindeki talepler alınırken bir hata oluştu: " + ex.Message);
            }
        }
    }
}
