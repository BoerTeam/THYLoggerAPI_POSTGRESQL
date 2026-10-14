using Microsoft.EntityFrameworkCore;
using THYLoggerAPI_POSTGRESQL.Context;
using THYLoggerAPI_POSTGRESQL.Model;

namespace THYLoggerAPI_POSTGRESQL.Services
{
    /// <summary>
    /// Dashboard'un canli kartlari icin her dolly'nin sadece en son
    /// sicaklik / nem / gps / bos-dolu kaydini dondurur (tum tabloyu cekmez).
    /// </summary>
    public class LatestService
    {
        private readonly ApplicationDbContext _context;

        public LatestService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<object> GetLatestAsync()
        {
            var dollyIds = await _context.Dolly.AsNoTracking().Select(d => d.Id).ToListAsync();

            var sicaklik = new List<Sicaklik>();
            var nem = new List<Nem>();
            var gps = new List<Gpsdatum>();
            var bosDolu = new List<BosDolu>();

            foreach (var id in dollyIds)
            {
                var s = await _context.Sicaklik.AsNoTracking().Where(x => x.DollyId == id)
                    .OrderByDescending(x => x.Time).FirstOrDefaultAsync();
                if (s != null) sicaklik.Add(s);

                var n = await _context.Nem.AsNoTracking().Where(x => x.DollyId == id)
                    .OrderByDescending(x => x.Time).FirstOrDefaultAsync();
                if (n != null) nem.Add(n);

                var g = await _context.Gpsdatum.AsNoTracking().Where(x => x.DollyId == id)
                    .OrderByDescending(x => x.Time).FirstOrDefaultAsync();
                if (g != null) gps.Add(g);

                var b = await _context.BosDolu.AsNoTracking().Where(x => x.DollyId == id)
                    .OrderByDescending(x => x.Time).FirstOrDefaultAsync();
                if (b != null) bosDolu.Add(b);
            }

            return new { sicaklik, nem, gps, bosDolu };
        }
    }
}
