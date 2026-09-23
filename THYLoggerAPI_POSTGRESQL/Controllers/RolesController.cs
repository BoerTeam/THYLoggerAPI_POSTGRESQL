using Microsoft.AspNetCore.Mvc;
using THYLoggerAPI_POSTGRESQL.DTOs;
using THYLoggerAPI_POSTGRESQL.Services;

namespace THYLoggerAPI_POSTGRESQL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly RoleService _roleService;

        public RolesController(RoleService roleService)
        {
            _roleService = roleService;
        }

        // GET: api/Roles
        [HttpGet]
        public async Task<IActionResult> GetAllRoles()
        {
            try
            {
                var roles = await _roleService.GetAllRolesAsync();
                return Ok(roles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Roller alınırken sunucu hatası oluştu: " + ex.Message);
            }
        }

        // GET: api/Roles/permissions
        [HttpGet("permissions")]
        public async Task<IActionResult> GetAllPermissions()
        {
            try
            {
                var permissions = await _roleService.GetAllPermissionsAsync();
                return Ok(permissions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "İzinler alınırken sunucu hatası oluştu: " + ex.Message);
            }
        }

        // POST: api/Roles VEYA POST: api/Roles/Create VEYA POST: api/Roles/Add
        [HttpPost]
        [HttpPost("Create")]
        [HttpPost("Add")]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto dto)
        {
            try
            {
                var result = await _roleService.CreateRoleAsync(dto);

                if (!result.IsSuccess)
                {
                    return BadRequest(result.ErrorMessage);
                }

                return Ok(result.ResponseData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Rol oluşturulurken sunucu hatası oluştu: " + ex.Message);
            }
        }
        // PUT: api/Roles/Update
        [HttpPut("Update")]
        public async Task<IActionResult> UpdateRole([FromBody] UpdateRoleDto dto)
        {
            try
            {
                var result = await _roleService.UpdateRoleAsync(dto);
                if (!result.IsSuccess)
                    return BadRequest(result.ErrorMessage);

                return Ok(new { message = "Rol başarıyla güncellendi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Rol güncellenirken sunucu hatası oluştu: " + ex.Message);
            }
        }

        // DELETE: api/Roles/Delete/5
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            try
            {
                var result = await _roleService.DeleteRoleAsync(id);
                if (!result.IsSuccess)
                    return BadRequest(result.ErrorMessage);

                return Ok(new { message = "Rol başarıyla silindi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Rol silinirken sunucu hatası oluştu: " + ex.Message);
            }
        }
    }
}