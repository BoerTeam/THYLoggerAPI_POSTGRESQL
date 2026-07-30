using Dashboard.DTO;
using Dashboard.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Net.Http;


namespace Dashboard.Controllers
{
    public class DollyController : Controller
    {
        
        public DollyController()
        {
           
        }
        public IActionResult Index()
        {
            var DollyList = Models.DollyMethod.GetAllDolly();
            return View(DollyList);
        }
        
        public IActionResult List()
        {
            var DollyList = Models.DollyMethod.GetAllDolly();
            return View(DollyList);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            // URL'yi Trimleyerek (temizleyerek) birleştirmek en güvenlisidir
            var Details = Models.DollyMethod.GetDollyById(id);

            
            return View(Details);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Dolly model)
        {
            // Model doğrulaması başarısızsa API'ye hiç gitmeden formu geri döndür
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Metodu await ile çağırıyoruz
            var isSuccess =  Models.DollyMethod.UpdateDolly(model);

            if (isSuccess)
            {
                return RedirectToAction("List");
            }

            // Eğer false dönerse kullanıcıya hata mesajı göster
            ModelState.AddModelError(string.Empty, "API üzerinden güncelleme yapılamadı. Lütfen bağlantınızı kontrol edin.");
            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> AddDolly()
        {
           
            return View(new Dolly { IsActive = true });
        }

        // POST: Veriyi API'ye gönderen metod
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddDolly(Dolly model)
        {
            if (!ModelState.IsValid) return View(model);

            // MUTLAKA await kullanmalısın
            var isSuccess = await Models.DollyMethod.AddDolly(model);

            if (isSuccess)
            {
                return RedirectToAction("List");
            }

            ModelState.AddModelError(string.Empty, "Ekleme işlemi başarısız oldu.");
            return View(model);
        }
    }
}
