using Microsoft.AspNetCore.Identity;
using DATN.Dtos;
using DATN.Dtos.UserDtos;
using System.Security.Claims;

namespace DATN.Services.Interfaces
{
    public interface IAccountRepository
    {
        public Task<string> SignUpAsync(SignUpModel model);
        public Task<string> SignInAsync(SignInModel model);
        Task<string> SignInWithGoogleAsync(string googleToken);
        Task<bool> Update(UpdateUserdto input);
        Task<List<UserDto>> Getall();
        Task<string> VerifyEmailAsync(string userId, string token);
        Task<string> ResendEmailConfirmationAsync(string email);
        Task<UserDto> GetUserAsync(ClaimsPrincipal userClaims);
        Task<string> ForgotPassWord(string Email);
        Task<string> ResetPasswordAsync(ResetPasswordRequest request);
    }
}
