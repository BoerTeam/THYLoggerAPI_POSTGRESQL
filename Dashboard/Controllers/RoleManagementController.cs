using Dashboard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dashboard.Controllers
{
    [Authorize(Policy = "UserView")]
    public class RoleManagementController : Controller
    {
        private readonly IApiService _apiService;

        public RoleManagementController(IApiService apiService)
        {
            _apiService = apiService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var roles = await _apiService.GetAsync<List<RoleItemDto>>("api/Roles") ?? new List<RoleItemDto>();
            var permissions = await _apiService.GetAsync<List<PermissionItemDto>>("api/Roles/permissions") ?? new List<PermissionItemDto>();

            ViewBag.Permissions = permissions;
            return View("~/Views/RoleManagement/Index.cshtml", roles);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(CreateRoleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index");
            }

            var isSuccess = await _apiService.PostAsync("api/Roles/Create", model);

            if (isSuccess)
            {
                TempData["SuccessMessage"] = "Yeni rol başarıyla oluşturuldu.";
            }
            else
            {
                TempData["ErrorMessage"] = "Rol oluşturulurken bir hata oluştu.";
            }

            return RedirectToAction("Index");
        }
    }

    public class RoleItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public List<RolePermissionItemDto> RolePermissions { get; set; } = new();
    }

    public class RolePermissionItemDto
    {
        public int RoleId { get; set; }
        public int PermissionId { get; set; }
        public PermissionItemDto? Permission { get; set; }
    }

    public class PermissionItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }

    public class CreateRoleViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<int> PermissionIds { get; set; } = new();
    }
}