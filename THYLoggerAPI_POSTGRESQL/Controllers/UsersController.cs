using Microsoft.AspNetCore.Mvc;
using THYLoggerAPI_POSTGRESQL.DTOs;
using THYLoggerAPI_POSTGRESQL.Services;

namespace THYLoggerAPI_POSTGRESQL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;

        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Kullanıcılar alınırken bir sunucu hatası oluştu: " + ex.Message);
            }
        }

        // GET: api/Users/5/roles VEYA api/Users/GetUserRoles/5
        [HttpGet("{id}/roles")]
        [HttpGet("GetUserRoles/{id}")]
        public async Task<IActionResult> GetUserRoles(int id)
        {
            try
            {
                var result = await _userService.GetUserRolesForAssignAsync(id);

                if (!result.IsSuccess)
                {
                    if (result.IsNotFound)
                    {
                        return NotFound(new { message = result.ErrorMessage });
                    }

                    return BadRequest(result.ErrorMessage);
                }

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Kullanıcı rol detayları alınırken bir sunucu hatası oluştu: " + ex.Message);
            }
        }

        // POST: api/Users/assign-roles VEYA POST: api/Users/AssignRoles
        [HttpPost("assign-roles")]
        [HttpPost("AssignRoles")]
        public async Task<IActionResult> AssignRoles([FromBody] AssignRoleDto dto)
        {
            try
            {
                var result = await _userService.AssignRolesToUserAsync(dto);

                if (!result.IsSuccess)
                {
                    if (result.IsNotFound)
                    {
                        return NotFound(new { message = result.ErrorMessage });
                    }

                    return BadRequest(result.ErrorMessage);
                }

                return Ok(result.ResponseData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Rol atanırken bir sunucu hatası oluştu: " + ex.Message);
            }
        }

        // POST: api/Users/get-or-create-sso-user
        [HttpPost("get-or-create-sso-user")]
        public async Task<IActionResult> GetOrCreateSsoUser([FromBody] SsoUserDto dto)
        {
            try
            {
                if (string.IsNullOrEmpty(dto?.Username))
                {
                    return BadRequest("Kullanıcı adı boş olamaz.");
                }

                var response = await _userService.GetOrCreateSsoUserAsync(dto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "SSO kullanıcısı işlenirken sunucu hatası oluştu: " + ex.Message);
            }
        }
    }
}