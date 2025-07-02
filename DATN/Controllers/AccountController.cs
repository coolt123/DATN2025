using DATN.Dtos.UserDtos;
using DATN.Dtos;
using DATN.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DATN.Entities;
using Microsoft.AspNetCore.Identity;
using System.Net;

namespace DATN.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountRepository _accountRepo;

        public AccountController(IAccountRepository accountRepo)
        {
            _accountRepo = accountRepo;
        }

        
        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] SignUpModel model)
        {
            var result = await _accountRepo.SignUpAsync(model);
            return Ok(new ResponseDto<object>
            {
                Status = 200,
                Message = "Đăng ký thành công",
                Data = null
            });
        }

        
        [HttpPost("signin")]
        public async Task<IActionResult> SignIn([FromBody] SignInModel model)
        {
            var token = await _accountRepo.SignInAsync(model);
            return Ok(new ResponseDto<string>
            {
                Status = 200,
                Message = "Đăng nhập thành công",
                Data = token
            });
        }

        
        [HttpPost("signin-google")]
        public async Task<IActionResult> SignInWithGoogle([FromBody] string googleToken)
        {
            var token = await _accountRepo.SignInWithGoogleAsync(googleToken);
            return Ok(new ResponseDto<string>
            {
                Status = 200,
                Message = "Đăng nhập Google thành công",
                Data = token
            });
        }

        
        [Authorize]
        [HttpPut("update-password")]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdateUserdto model)
        {
            var result = await _accountRepo.Update(model);
            return Ok(new ResponseDto<bool>
            {
                Status = 200,
                Message = "Đổi mật khẩu thành công",
                Data = result
            });
        }

      
        [Authorize(Roles = AppRole.Admin)]
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _accountRepo.Getall();
            return Ok(new ResponseDto<object>
            {
                Status = 200,
                Message = "Lấy danh sách người dùng thành công",
                Data = users
            });
        }
        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail(string userId, string token)
        {
            var message = await _accountRepo.VerifyEmailAsync(userId, token);
            return Ok(new ResponseDto<object>
            {
                Status = 200,
                Message = "Xác thực email thành công",
                Data = message
            });
        }

        [HttpPost("resend-confirmation-email")]
        public async Task<IActionResult> ResendEmail([FromBody] string email)
        {
            var message = await _accountRepo.ResendEmailConfirmationAsync(email);
            return Ok(new ResponseDto<object>
            {
                Status = 200,
                Message = "Gửi lại email xác thực thành công",
                Data = message
            });
        }


    }
}
