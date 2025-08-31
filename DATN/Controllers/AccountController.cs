using DATN.Dtos;
using DATN.Dtos.UserDtos;
using DATN.Entities;
using DATN.Helpers;
using DATN.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;

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
            return ResponseHelper.ResponseSuccess<object>(null, "Đăng ký thành công");
        }

        [HttpPost("signin")]
        public async Task<IActionResult> SignIn([FromBody] SignInModel model)
        {
            var token = await _accountRepo.SignInAsync(model);
            return ResponseHelper.ResponseSuccess(token, "Đăng nhập thành công");
        }

        [HttpPost("signin-google")]
        public async Task<IActionResult> SignInWithGoogle([FromBody] string googleToken)
        {
            var token = await _accountRepo.SignInWithGoogleAsync(googleToken);
            return ResponseHelper.ResponseSuccess(token, "Đăng nhập Google thành công");
        }

        [Authorize]
        [HttpPut("update-password")]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdateUserdto model)
        {
            var result = await _accountRepo.Update(model);
            return ResponseHelper.ResponseSuccess(result, "Đổi mật khẩu thành công");
        }

        [Authorize(Roles = AppRole.Admin)]
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _accountRepo.Getall();
            return ResponseHelper.ResponseSuccess(users, "Lấy danh sách người dùng thành công");
        }

        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail(string userId, string token)
        {
            var message = await _accountRepo.VerifyEmailAsync(userId, token);
            return ResponseHelper.ResponseSuccess(message, "Xác thực email thành công");
        }

        [HttpPost("resend-confirmation-email")]
        public async Task<IActionResult> ResendEmail([FromBody] string email)
        {
            var message = await _accountRepo.ResendEmailConfirmationAsync(email);
            return ResponseHelper.ResponseSuccess(message, "Gửi lại email xác thực thành công");
        }
    }
}
