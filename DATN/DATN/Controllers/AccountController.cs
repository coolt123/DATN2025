using DATN.Dtos;
using DATN.Dtos.UserDtos;
using DATN.Entities;
using DATN.Exceptions;
using DATN.Helpers;
using DATN.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;
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
            try
            {
                var result = await _accountRepo.SignUpAsync(model);
                return ResponseHelper.ResponseSuccess<object>(null, result);
            }
            catch (Exception ex)
            {
                return ResponseHelper.ResponseError(ex.Message);
            }
        }

        [HttpPost("signin")]
        public async Task<IActionResult> SignIn([FromBody] SignInModel model)
        {
            try
            {
                var token = await _accountRepo.SignInAsync(model);
                return ResponseHelper.ResponseSuccess(token, "Đăng nhập thành công");
            }
            catch (Exception ex)
            {
                return ResponseHelper.ResponseError(ex.Message);
            }
        }

        [HttpPost("signin-google")]
        public async Task<IActionResult> SignInWithGoogle([FromBody] string googleToken)
        {
            try
            {
                var token = await _accountRepo.SignInWithGoogleAsync(googleToken);
                return ResponseHelper.ResponseSuccess(token, "Đăng nhập Google thành công");
            }
            catch (Exception ex)
            {
                return ResponseHelper.ResponseError(ex.Message);
            }
        }

        [Authorize]
        [HttpPut("update-password")]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdateUserdto model)
        {
            try
            {
                var result = await _accountRepo.Update(model);
                return ResponseHelper.ResponseSuccess(result, "Đổi mật khẩu thành công");
            }
            catch (Exception ex)
            {
                return ResponseHelper.ResponseError(ex.Message);
            }
        }

        [Authorize(Roles = AppRole.Admin)]
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _accountRepo.Getall();
                return ResponseHelper.ResponseSuccess(users, "Lấy danh sách người dùng thành công");
            }
            catch (Exception ex)
            {
                return ResponseHelper.ResponseError(ex.Message);
            }
        }

        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail(string userId, string token)
        {
            try
            {
                var message = await _accountRepo.VerifyEmailAsync(userId, token);
                return ResponseHelper.ResponseSuccess(message, "Xác thực email thành công");
            }
            catch (Exception ex)
            {
                return ResponseHelper.ResponseError(ex.Message);
            }
        }

        [HttpPost("resend-confirmation-email")]
        public async Task<IActionResult> ResendEmail([FromBody] string email)
        {
            try
            {
                var message = await _accountRepo.ResendEmailConfirmationAsync(email);
                return ResponseHelper.ResponseSuccess(message, "Gửi lại email xác thực thành công");
            }
            catch (Exception ex)
            {
                return ResponseHelper.ResponseError(ex.Message);
            }
        }
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetUser()
        {
            try
            {
                var user = await _accountRepo.GetUserAsync(User);
                return ResponseHelper.ResponseSuccess(user, "Lấy thông tin người dùng thành công");
            }
            catch (Exception ex)
            {
                return ResponseHelper.ResponseError(ex.Message);
            }

        }
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassWord([FromBody] ForgotPasswordRequest request)
        {
            try
            {
                var result = await _accountRepo.ForgotPassWord(request.Email);
                return Ok(new { message = result });
            }
            catch (Exception ex)
            {
                return ResponseHelper.ResponseError(ex.Message);
            }

        }
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassWord([FromBody] ResetPasswordRequest request)
        {
            try
            {
                var result = await _accountRepo.ResetPasswordAsync(request);
                return ResponseHelper.ResponseSuccess(result, "Đặt lại mật khẩu thành công ");
            }
            catch (Exception ex)
            {
                return ResponseHelper.ResponseError(ex.Message);
            }

        }
    }
}
