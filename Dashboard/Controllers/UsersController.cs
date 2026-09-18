using Dashboard.DTO;
using Dashboard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dashboard.Controllers
{
    [Authorize(Policy = "UserView")]
    public class UsersController : Controller
    {
        private readonly IApiService _apiService;

        public UsersController(IApiService apiService)
        {
            _apiService = apiService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = await _apiService.GetAsync<List<UserListDto>>("api/Users");
            return View(users ?? new List<UserListDto>());
        }

        [HttpGet]
        public async Task<IActionResult> AssignRoles(int id)
        {
            var model = await _apiService.GetAsync<UserRoleDetailDto>($"api/Users/{id}/roles");

            if (model == null)
            {
                TempData["ErrorMessage"] = "Kullanıcı rol bilgileri alınamadı.";
                return RedirectToAction("Index");
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AssignRoles(UserRoleDetailDto model)
        {
            var dto = new AssignRoleDto
            {
                UserId = model.UserId,
                RoleIds = model.Roles.Where(r => r.IsAssigned).Select(r => r.RoleId).ToList()
            };

            var isSuccess = await _apiService.PostAsync("api/Users/assign-roles", dto);

            if (isSuccess)
            {
                TempData["SuccessMessage"] = "Kullanıcı rolleri başarıyla güncellendi.";
                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", "Roller güncellenirken bir hata oluştu.");
            return View(model);
        }
    }
}