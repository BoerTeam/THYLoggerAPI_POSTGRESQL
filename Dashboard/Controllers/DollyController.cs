using Dashboard.DTO;
using Dashboard.Models;
using Dashboard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dashboard.Controllers
{
    [Authorize(Policy = "DollyView")]
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
            var dollyList = await _apiService.GetAsync<List<Dolly>>("api/Dolly/GetAll") ?? new List<Dolly>();
            return View(dollyList);
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var dollyList = await _apiService.GetAsync<List<Dolly>>("api/Dolly/GetAll") ?? new List<Dolly>();
            return View(dollyList);
        }

        [HttpGet]
        [Authorize(Policy = "DollyEdit")]
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
        [Authorize(Policy = "DollyEdit")]
        public async Task<IActionResult> Edit(Dolly model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var response = await _apiService.PutAsync<Dolly>($"api/Dolly/Update/{model.Id}", model);

            if (response != null)
            {
                return RedirectToAction(nameof(List));
            }

            ModelState.AddModelError(string.Empty, "API üzerinden güncelleme yapılamadı. Lütfen bağlantınızı kontrol edin.");
            return View(model);
        }

        [HttpGet]
        [Authorize(Policy = "DollyAdd")]
        public IActionResult AddDolly()
        {
            return View(new Dolly { IsActive = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "DollyAdd")]
        public async Task<IActionResult> AddDolly(Dolly model)
        {
            ModelState.Remove("Id");
            ModelState.Remove("CreatedAt");
            ModelState.Remove("UpdatedAt");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var response = await _apiService.PostAsync<Dolly>("api/Dolly/Add", model);

            if (response != null)
            {
                return RedirectToAction(nameof(List));
            }

            ModelState.AddModelError(string.Empty, "API tarafında ekleme işlemi başarısız oldu.");
            return View(model);
        }
    }
}