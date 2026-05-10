using EmlakPortal.API.DTOs;
using EmlakPortal.API.Models;
using EmlakPortal.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EmlakPortal.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly ITokenService _tokenService;

        public AuthController(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, ITokenService tokenService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _tokenService = tokenService;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            var user = new AppUser
            {
                UserName = model.UserName,
                Email = model.Email,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                
                string[] roles = { "Admin", "User" };
                foreach (var role in roles)
                {
                    if (!await _roleManager.RoleExistsAsync(role))
                    {
                        await _roleManager.CreateAsync(new AppRole { Name = role });
                    }
                }

                
                await _userManager.AddToRoleAsync(user, "User");

                return Ok(new ResultDto { Status = true, Message = "Kayıt Başarılı. Hesabınız standart 'User' yetkisiyle oluşturuldu." });
            }

            return BadRequest(new ResultDto { Status = false, Message = "Kayıt sırasında bir hata oluştu.", Data = result.Errors });
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginDto model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);

            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                
                var token = await _tokenService.GenerateToken(user);
                return Ok(new ResultDto { Status = true, Message = "Giriş Başarılı", Data = token });
            }

            return Unauthorized(new ResultDto { Status = false, Message = "Kullanıcı adı veya şifre hatalı!" });
        }

       
        [Authorize(Roles = "Admin")]
        [HttpPost("MakeAdmin")]
        public async Task<IActionResult> MakeAdmin(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
                return NotFound(new ResultDto { Status = false, Message = "Kullanıcı bulunamadı." });


            await _userManager.AddToRoleAsync(user, "Admin");

            return Ok(new ResultDto { Status = true, Message = $"{userName} artık bir emlak yöneticisi (Admin)!" });
        }


        [Authorize(Roles = "Admin")]
        [HttpPost("RevokeAdmin")]
        public async Task<IActionResult> RevokeAdmin(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
                return NotFound(new ResultDto { Status = false, Message = "Kullanıcı bulunamadı." });

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (!isAdmin)
                return BadRequest(new ResultDto { Status = false, Message = "Bu kullanıcı zaten yönetici değil." });

            var result = await _userManager.RemoveFromRoleAsync(user, "Admin");

            if (result.Succeeded)
            {
                return Ok(new ResultDto { Status = true, Message = $"{userName} adlı kullanıcının yönetici yetkisi başarıyla alındı!" });
            }

            return BadRequest(new ResultDto { Status = false, Message = "Yetki alınırken bir sorun oluştu." });
        }
        [HttpGet("Users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userManager.Users
                .Include(u => u.Estates)   
                .ToListAsync();

            var userList = new List<object>();
            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                userList.Add(new
                {
                    u.Id,
                    u.UserName,
                    u.FullName,
                    u.Email,
                    Roles = roles,
                    EstateCount = u.Estates?.Count ?? 0   
                });
            }
            return Ok(userList);
        }
        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return Ok(new ResultDto { Status = true, Message = "Eğer sistemde kayıtlıysa, şifre sıfırlama bağlantısı e-posta adresinize gönderilmiştir." });

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
           
            return Ok(new ResultDto { Status = true, Message = "Şifre sıfırlama bağlantısı gönderildi. (Token: " + token + ")" });
        }
    
        [Authorize]
        [HttpGet("GetProfile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(new ResultDto { Status = false, Message = "Kullanıcı bulunamadı." });

            var roles = await _userManager.GetRolesAsync(user);
            var profile = new
            {
                user.UserName,
                user.FullName,
                user.Email,
                user.PhoneNumber,
                Role = roles.FirstOrDefault() ?? "User"
            };

            return Ok(new ResultDto { Status = true, Data = profile });
        }

      
        [Authorize]
        [HttpPut("UpdateProfile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(new ResultDto { Status = false, Message = "Kullanıcı bulunamadı." });

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;

            if (!string.IsNullOrEmpty(model.CurrentPassword) && !string.IsNullOrEmpty(model.NewPassword))
            {
                var passwordCheck = await _userManager.CheckPasswordAsync(user, model.CurrentPassword);
                if (!passwordCheck)
                    return BadRequest(new ResultDto { Status = false, Message = "Mevcut şifre yanlış." });

                var passwordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                if (!passwordResult.Succeeded)
                    return BadRequest(new ResultDto { Status = false, Message = string.Join(" ", passwordResult.Errors.Select(e => e.Description)) });
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
                return Ok(new ResultDto { Status = true, Message = "Profil başarıyla güncellendi." });

            return BadRequest(new ResultDto { Status = false, Message = "Profil güncellenirken bir hata oluştu." });
        }
    }
}