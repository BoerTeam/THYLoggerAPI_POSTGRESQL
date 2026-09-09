using Microsoft.EntityFrameworkCore;
using THYLoggerAPI_POSTGRESQL.Context;
using THYLoggerAPI_POSTGRESQL.Model;

namespace THYLoggerAPI_POSTGRESQL.Services
{
    public class DollyService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DollyService> _logger;

        public DollyService(ApplicationDbContext context, ILogger<DollyService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Dolly>> GetAllAsync()
        {
            _logger.LogInformation("Tüm Dolly verileri listeleniyor.");
            return await _context.Dolly.AsNoTracking().OrderBy(i => i.Id).ToListAsync();
        }

        public async Task<Dolly?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Dolly aranıyor. Aranacak ID: {DollyId}", id);

            var dolly = await _context.Dolly.FindAsync(id);
            if (dolly == null)
            {
                _logger.LogWarning("Dolly bulunamadı! Aranan ID: {DollyId}", id);
            }

            return dolly;
        }

        public async Task<Dolly> AddAsync(Dolly entity)
        {
            try
            {
                await _context.Dolly.AddAsync(entity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Yeni Dolly verisi başarıyla eklendi. ID: {DollyId}", entity.Id);
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dolly eklenirken bir hata oluştu!");
                throw;
            }
        }

        public async Task<Dolly?> UpdateAsync(int id, Dolly entity)
        {
            _logger.LogInformation("Dolly güncelleme işlemi başlatıldı. Güncellenecek ID: {DollyId}", id);

            var existingDolly = await _context.Dolly.FindAsync(id);
            if (existingDolly == null)
            {
                _logger.LogWarning("Güncellenmek istenen Dolly verisi bulunamadı. ID: {DollyId}", id);
                return null;
            }

            try
            {
                _context.Entry(existingDolly).CurrentValues.SetValues(entity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Dolly verisi başarıyla güncellendi. ID: {DollyId}", id);
                return existingDolly;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dolly güncellenirken bir hata oluştu! ID: {DollyId}", id);
                throw;
            }
        }
    }
}