using Dashboard.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dashboard.ViewComponents
{
    public class MenuViewComponent : ViewComponent
    {
        private readonly IApiService _apiService;

        public MenuViewComponent(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // API'den dinamik tanımlanmış tüm aktif sayfaları çekiyoruz
            var pages = await _apiService.GetAsync<List<PageMenuDto>>("api/Pages") ?? new List<PageMenuDto>();

            // Oturumu açık kullanıcının Permission claim'lerini okuyoruz
            var userPermissions = UserClaimsPrincipal.Claims
                .Where(c => c.Type == "Permission")
                .Select(c => c.Value)
                .ToList();

            var isUserAdmin = UserClaimsPrincipal.IsInRole("Admin");

            // Kullanıcı Admin ise veya sayfanın PermissionCode'u boşsa ya da kullanıcının o izin kodu varsa filtrele
            var allowedPages = pages.Where(p =>
                isUserAdmin ||
                string.IsNullOrEmpty(p.PermissionCode) ||
                userPermissions.Contains(p.PermissionCode)
            ).OrderBy(p => p.Order).ToList();

            return View(allowedPages);
        }
    }

    public class PageMenuDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Route { get; set; } = string.Empty;
        public string? PermissionCode { get; set; }
        public string? Icon { get; set; }
        public int Order { get; set; }
        public bool IsActive { get; set; }
    }
}