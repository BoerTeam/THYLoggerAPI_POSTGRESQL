using Dashboard.DTO;
using Dashboard.Models;
using Dashboard.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dashboard.Controllers
{
    public class DollyController : Controller
    {
        private readonly IApiService _apiService;

        public DollyController(IApiService apiService)
        {
            _apiService = apiService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var dollyList = await _apiService.GetAsync<List<Dolly>>("api/Dolly/Get") ?? new List<Dolly>();
            return View(dollyList);
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var dollyList = await _apiService.GetAsync<List<Dolly>>("api/Dolly/Get") ?? new List<Dolly>();
            return View(dollyList);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var dolly = await _apiService.GetAsync<Dolly>($"api/Dolly/GetById/{id}");
            if (dolly == null)
            {
                return NotFound();
            }

            return View(dolly);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Dolly model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // DÜZELTME: Servis tek tip parametresi bekliyor (<Dolly>)
            var response = await _apiService.PutAsync<Dolly>("api/Dolly/Update", model);

            // Eğer API'den dönen yanıt bir başarı durumu içeriyorsa veya null değilse başarılı sayıyoruz
            if (response != null)
            {
                return RedirectToAction(nameof(List));
            }

            ModelState.AddModelError(string.Empty, "API üzerinden güncelleme yapılamadı. Lütfen bağlantınızı kontrol edin.");
            return View(model);
        }

        [HttpGet]
        public IActionResult AddDolly()
        {
            return View(new Dolly { IsActive = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddDolly(Dolly model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // DÜZELTME: Servis tek tip parametresi bekliyor (<Dolly>)
            var response = await _apiService.PostAsync<Dolly>("api/Dolly/Add", model);

            if (response != null)
            {
                return RedirectToAction(nameof(List));
            }

            ModelState.AddModelError(string.Empty, "Ekleme işlemi başarısız oldu.");
            return View(model);
        }
    }
}