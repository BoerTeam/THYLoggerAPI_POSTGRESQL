using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using THYLoggerAPI_POSTGRESQL.Context;
using THYLoggerAPI_POSTGRESQL.Model;

namespace THYLoggerAPI_POSTGRESQL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DollyController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public DollyController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet("Get")]
        public IEnumerable<Dolly> Get()
        {
            return _context.Dolly.OrderBy(i => i.Id).ToList();
        }

        [HttpPost("Add")]
        public IActionResult Add(Dolly entity)
        {
            _context.Dolly.Add(entity);
            _context.SaveChanges();

            return Ok("Dolly Verisi Başarılı İle Eklendi");
        }
        [HttpGet("GetById/{id}")]
        public IActionResult GetById(int id)
        {
            var dolly = _context.Dolly.FirstOrDefault(x => x.Id == id);
            if (dolly == null)
            {
                return NotFound("Dolly bulunamadı.");
            }
            return Ok(dolly);
        }

        // 2. Güncelleme Metodu
        [HttpPut("Update")]
        public IActionResult Update(Dolly entity)
        {
            // Veritabanında böyle bir kayıt var mı kontrol edelim
            var existingDolly = _context.Dolly.Any(x => x.Id == entity.Id);

            if (!existingDolly)
            {
                return NotFound("Güncellenecek kayıt bulunamadı.");
            }

            try
            {
                _context.Dolly.Update(entity);
                _context.SaveChanges();
                return Ok("Dolly verisi başarıyla güncellendi.");
            }
            catch (Exception ex)
            {
                return BadRequest("Güncelleme sırasında bir hata oluştu: " + ex.Message);
            }
        }
        
    }
}

