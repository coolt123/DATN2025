using Azure;
using Google.Apis.Auth;
using Identity.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;
using DATN.Dtos;
using DATN.Dtos.UserDtos;
using DATN.Entities;
using DATN.Services.Interfaces;
using OfficeOpenXml.Packaging.Ionic.Zip;
using DATN.Exceptions;
using DATN.Dtos.Resetpassdto;
using Microsoft.EntityFrameworkCore;
using Google;
using DATN.DbContexts;
using EmailSender;
using DATN.Services.ResetPass;
using System.Net;
using System.Security.Cryptography;
using OfficeOpenXml.FormulaParsing.LexicalAnalysis;

namespace DATN.Services.Implements
{
    public class AccountRepository : IAccountRepository
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IConfiguration configuration;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Data dbContext;
        private readonly IEmailSender _emailSender;
        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        public AccountRepository(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, IConfiguration configuration,
            RoleManager<IdentityRole> roleManager, IHttpContextAccessor httpContextAccessor,
            Data dbContext, IEmailSender emailSender)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.configuration = configuration;
            this.roleManager = roleManager;
            _httpContextAccessor = httpContextAccessor;
            this.dbContext = dbContext;
            this._emailSender = emailSender;



        }
        public async Task<string> SignInAsync(SignInModel model)
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            var passWordValid = await userManager.CheckPasswordAsync(user, model.Password);
            if (user == null || !passWordValid)
            {
                throw new AppException("Email hoặc mật khẩu không đúng.", 401);
            }
            if (!user.EmailConfirmed)
                throw new AppException("Tài khoản chưa được xác thực email.", 403);

            var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);
            if (!result.Succeeded)
            {
                throw new AppException("Đăng nhập không thành công.", 401);
            }

            
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, model.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            var userRoles = await userManager.GetRolesAsync(user);
            foreach (var role in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            var authKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]));
            var token = new JwtSecurityToken(
                issuer: configuration["Jwt:ValidIssuer"],
                audience: configuration["Jwt:ValidAudience"],
                expires: DateTime.UtcNow.AddMinutes(30),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authKey, SecurityAlgorithms.HmacSha512Signature)
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

           
            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await userManager.UpdateAsync(user);

           
            if (_httpContextAccessor?.HttpContext == null)
            {
                throw new InvalidOperationException("HttpContext không khả dụng.");
            }

            var accessCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Expires = DateTime.UtcNow.AddMinutes(30),
                SameSite = SameSiteMode.None 
            };

            var refreshCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Expires = user.RefreshTokenExpiryTime,
                SameSite = SameSiteMode.None
            };

            _httpContextAccessor.HttpContext.Response.Cookies.Append("access_token", tokenString, accessCookieOptions);
            _httpContextAccessor.HttpContext.Response.Cookies.Append("refresh_token", refreshToken, refreshCookieOptions);

            return tokenString;
        }


        public async Task<string> SignUpAsync(SignUpModel model)
        {
            var existingUser = await userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
                throw new AppException("Email đã được sử dụng.", 409);

            var user = new ApplicationUser
            {
                FullName = model.FullName,
                Email = model.Email,
                UserName = model.Email,
                AddressLine = model.AddressLine,
                PhoneNumber = model.PhoneNumber,
                EmailConfirmed = false,
                RefreshToken = ""

            };

            var result = await userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                throw new AppException("Đăng ký thất bại.", 500);

            // Gán vai trò
            if (!await roleManager.RoleExistsAsync(AppRole.Customer))
                await roleManager.CreateAsync(new IdentityRole(AppRole.Customer));

            await userManager.AddToRoleAsync(user, AppRole.Customer);

            // Tạo token xác nhận email
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

            var tokenBytes = System.Text.Encoding.UTF8.GetBytes(token);
            var base64Token = Convert.ToBase64String(tokenBytes)
                    .Replace('+', '-')
                    .Replace('/', '_')
                    .TrimEnd('=');
            // Tạo link xác thực
            var confirmLink = $"http://localhost:3000/auth/verify-email?userId={user.Id}&token={base64Token}";

            // Gửi email
            await _emailSender.SendEmailAsync(user.Email, "Xác thực tài khoản",
                $"Chào {user.FullName},<br/>Vui lòng <a href='{confirmLink}'>bấm vào đây để xác thực tài khoản</a>.");

            return "Đăng ký thành công. Vui lòng kiểm tra email để xác thực tài khoản.";
        }


        public async Task<string> SignInWithGoogleAsync(string googleToken)
        {
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new List<string> { configuration["Google:ClientId"] }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(googleToken, settings);
                var user = await userManager.FindByEmailAsync(payload.Email);

                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        Email = payload.Email,
                        UserName = payload.Email, // hoặc Guid.NewGuid().ToString() nếu email bị trùng
                        FullName = payload.Name,
                        EmailConfirmed = true, 
                        RefreshToken = "",
                    };

                    var result = await userManager.CreateAsync(user);
                    if (!result.Succeeded)
                        throw new AppException("Không thể tạo tài khoản từ Google.", 500);

                    if (!await roleManager.RoleExistsAsync(AppRole.Customer))
                        await roleManager.CreateAsync(new IdentityRole(AppRole.Customer));

                    await userManager.AddToRoleAsync(user, AppRole.Customer);
                }

                
                var tokenString = GenerateJwtToken(user);

               
                var refreshToken = GenerateRefreshToken();
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                await userManager.UpdateAsync(user);

                // ===== Lưu token vào cookie nếu cần =====
                if (_httpContextAccessor?.HttpContext != null)
                {
                    var accessCookieOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        Expires = DateTime.UtcNow.AddMinutes(30),
                        SameSite = SameSiteMode.None
                    };

                    var refreshCookieOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        Expires = user.RefreshTokenExpiryTime,
                        SameSite = SameSiteMode.None
                    };

                    _httpContextAccessor.HttpContext.Response.Cookies.Append("access_token", tokenString, accessCookieOptions);
                    _httpContextAccessor.HttpContext.Response.Cookies.Append("refresh_token", refreshToken, refreshCookieOptions);
                }

                return tokenString;
            }
            catch (Exception ex)
            {
                // Ghi log nếu cần
                throw new AppException("Đăng nhập Google thất bại.", 500);
            }
        }

        private string GenerateJwtToken(ApplicationUser user)
        {
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            var userRoles = userManager.GetRolesAsync(user).Result;
            foreach (var role in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            var authKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]));
            var token = new JwtSecurityToken(
                issuer: configuration["Jwt:ValidIssuer"],
                audience: configuration["Jwt:ValidAudience"],
                expires: DateTime.Now.AddMinutes(30),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authKey, SecurityAlgorithms.HmacSha512Signature)
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            _httpContextAccessor.HttpContext?.Response.Cookies.Append("access_token", tokenString, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Expires = DateTime.UtcNow.AddMinutes(30)
            });

            return tokenString;
        }


        public async Task<bool> Update(UpdateUserdto model)
        {
            try
            {
                if (_httpContextAccessor.HttpContext != null)
                {
                    var user = await userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
                    if (user == null)
                    {
                        throw new AppException("Không tìm thấy người dùng.", 404);
                    }

                    var isValid = await userManager.CheckPasswordAsync(user, model.CurrentPassword);
                    if (!isValid)
                    {
                        throw new AppException("Mật khẩu hiện tại không đúng.", 400);
                    }

                    var result = await userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                    return result.Succeeded;
                }
                return false;
            }
            catch (Exception ex)
            {
                
                return false;
            }
        }


        public async Task<UserDto> GetUserAsync(ClaimsPrincipal userClaims)
        {
            var userId = userClaims.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return null;

            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return null;

            return new UserDto
            {
                IdUser = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                AddresLine = user.AddressLine
            };
        }
        public async Task<List<UserDto>> Getall()
        {
            var users = userManager.Users.ToList();
            var result = new List<UserDto>();

            foreach (var user in users)
            {
                var roles = await userManager.GetRolesAsync(user);
                result.Add(new UserDto
                {
                    IdUser = user.Id,
                    Email = user.Email,
                    AddresLine=user.AddressLine,
                    FullName=user.FullName,
                    PhoneNumber=user.PhoneNumber,


                });
            }

            return result;
        }
        public async Task<string> VerifyEmailAsync(string userId, string token)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new AppException("Không tìm thấy người dùng.", 404);
            }

            if (user.EmailConfirmed)
            {
                return "Tài khoản đã được xác thực trước đó.";
            }
            var tokenFromUrl = token
            .Replace('-', '+')
            .Replace('_', '/');
            switch (tokenFromUrl.Length % 4)
            {
                case 2: tokenFromUrl += "=="; break;
                case 3: tokenFromUrl += "="; break;
            }
            var decodedToken = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(tokenFromUrl));
            var result = await userManager.ConfirmEmailAsync(user, decodedToken);
            if (!result.Succeeded)
            {
                throw new AppException("Xác thực thất bại. Token không hợp lệ hoặc đã hết hạn.", 400);
            }

            return "Xác thực tài khoản thành công.";
        }
        public async Task<string> ResendEmailConfirmationAsync(string email)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                throw new AppException("Không tìm thấy người dùng với email này.", 404);
            }

            if (user.EmailConfirmed)
            {
                return "Tài khoản đã được xác thực trước đó.";
            }

            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var tokenBytes = System.Text.Encoding.UTF8.GetBytes(token);
            var base64Token = Convert.ToBase64String(tokenBytes)
                    .Replace('+', '-')
                    .Replace('/', '_')
                    .TrimEnd('=');
            // Tạo link xác thực
            var confirmLink = $"http://localhost:3000/auth/verify-email?userId={user.Id}&token={base64Token}";

            // Gửi email
            await _emailSender.SendEmailAsync(user.Email, "Xác thực tài khoản",
                $"Chào {user.FullName},<br/>Vui lòng <a href='{confirmLink}'>bấm vào đây để xác thực tài khoản</a>.");

            return "Đăng gửi lại link xác thực. Vui lòng kiểm tra email để xác thực tài khoản.";
        }

        public async Task<string> ForgotPassWord(string Email)
        {
           var user=await userManager.FindByEmailAsync(Email);
            if (user == null) {
                return "Yêu cầu cài đặt lại mật khẩu đã được gửi vào email của b vui lòng kiểm tra";
             }
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var tokenBytes = System.Text.Encoding.UTF8.GetBytes(token);
            var base64Token = Convert.ToBase64String(tokenBytes)
                    .Replace('+', '-')
                    .Replace('/', '_')
                    .TrimEnd('=');
            var confirmLink = $"http://localhost:3000/auth/reset-password?userId={user.Id}&token={base64Token}";
            await _emailSender.SendEmailAsync(user.Email, "Đặt lại mật khẩu",
               $"Chào {user.FullName},<br/>Vui lòng <a href='{confirmLink}'>bấm vào đây để đặt lại mật khẩu</a>.");

            return "Yêu cầu cài đặt lại mật khẩu đã được gửi vào email của b vui lòng kiểm tra";
        }
        public async Task<string> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var user = await userManager.FindByIdAsync(request.userId);
            if (user == null)
            {
                throw new AppException("tài khoản không tồn tại");
            }
           
            var tokenFromUrl = request.token
            .Replace('-', '+')
            .Replace('_', '/');
            switch (tokenFromUrl.Length % 4)
            {
                case 2: tokenFromUrl += "=="; break;
                case 3: tokenFromUrl += "="; break;
            }
            var decodedToken = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(tokenFromUrl));
            var result = await userManager.ResetPasswordAsync(user, decodedToken, request.NewPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new AppException($"Không thể đặt lại mật khẩu: {errors}");
            }
            return "Đặt lại mật khẩu thành công";
        }
    }

}
