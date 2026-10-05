using Dashboard.DTO;
using Dashboard.Models;
using Dashboard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dashboard.Controllers
{
    [Authorize]
    public class DollyController : Controller
    {
        private readonly IApiService _apiService;

        public DollyController(IApiService apiService)
        {
            _apiService = apiService;
        }

        // Dolly Listesi - Yalnızca DOLLY_VIEW veya Admin
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!User.HasClaim("Permission", "DOLLY_VIEW") && !User.IsInRole("Admin"))
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            var dollyList = await _apiService.GetAsync<List<Dolly>>("api/Dolly/GetAll") ?? new List<Dolly>();
            return View(dollyList);
        }

        // Dolly Listesi (Alternatif Route) - Yalnızca DOLLY_VIEW veya Admin
        [HttpGet]
        public async Task<IActionResult> List()
        {
            if (!User.HasClaim("Permission", "DOLLY_VIEW") && !User.IsInRole("Admin"))
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            var dollyList = await _apiService.GetAsync<List<Dolly>>("api/Dolly/GetAll") ?? new List<Dolly>();
            return View(dollyList);
        }

        // Dolly Düzenleme Sayfası - Yalnızca DOLLY_EDIT veya Admin
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!User.HasClaim("Permission", "DOLLY_EDIT") && !User.IsInRole("Admin"))
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            var dolly = await _apiService.GetAsync<Dolly>($"api/Dolly/GetById/{id}");
            if (dolly == null)
            {
                return NotFound();
            }

            return View(dolly);
        }

        // Dolly Düzenleme Kayıt - Yalnızca DOLLY_EDIT veya Admin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Dolly model)
        {
            if (!User.HasClaim("Permission", "DOLLY_EDIT") && !User.IsInRole("Admin"))
            {
                return RedirectToAction("AccessDenied", "Home");
            }

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

        // Dolly Ekleme Sayfası - Yalnızca DOLLY_EDIT veya Admin
        [HttpGet]
        public IActionResult AddDolly()
        {
            if (!User.HasClaim("Permission", "DOLLY_EDIT") && !User.IsInRole("Admin"))
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            return View(new Dolly { IsActive = true });
        }

        // Dolly Ekleme Kayıt - Yalnızca DOLLY_EDIT veya Admin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddDolly(Dolly model)
        {
            if (!User.HasClaim("Permission", "DOLLY_EDIT") && !User.IsInRole("Admin"))
            {
                return RedirectToAction("AccessDenied", "Home");
            }

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