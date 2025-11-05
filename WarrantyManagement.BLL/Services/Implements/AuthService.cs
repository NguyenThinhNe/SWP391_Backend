using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WarrantyManagement.BLL.Services.Interfaces;
using WarrantyManagement.DAL.Data.Context;
using WarrantyManagement.DAL.Data.Entities;
using WarrantyManagement.DAL.Data.Request;
using WarrantyManagement.DAL.Data.Response;
using WarrantyManagement.DAL.Data.Settings;
using WarrantyManagement.DAL.Repositories.Interfaces;

namespace WarrantyManagement.BLL.Services.Implements
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork<WarrantyDbContext> _unitOfWork;
        private readonly JwtSettings _jwtSettings;
        private readonly PasswordHasher<string> _passwordHasher = new PasswordHasher<string>();
        public AuthService(IUnitOfWork<WarrantyDbContext> unitOfWork, IOptions<JwtSettings> jwtSettings)
        {
            _unitOfWork = unitOfWork;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<SuccessResponse<AuthUserResponse>> GetCurrentUserAsync(Guid userId)
        {
            try
            {
                var userRepo = _unitOfWork.GetRepository<User>();

                var user = await userRepo.FirstOrDefaultAsync(
                    predicate: u => u.UserId == userId,
                    include: query => query.Include(u => u.ServiceCenter)
                );

                if (user == null)
                {
                    return new SuccessResponse<AuthUserResponse>
                    {
                        Success = false,
                        Message = "User not found",
                        Data = null
                    };
                }

                var userResponse = new AuthUserResponse
                {
                    UserId = user.UserId,
                    UserName = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Name = user.Name,
                    Role = user.Role.ToString(),
                    ServiceCenterId = user.ServiceCenterId,
                    ServiceCenterName = user.ServiceCenter?.CenterName,
                    CoverImage = user.CoverImage,
                    CreatedTime = user.CreatedTime
                };

                return new SuccessResponse<AuthUserResponse>
                {
                    Success = true,
                    Message = "User retrieved successfully",
                    Data = userResponse
                };
            }
            catch (Exception ex)
            {
                return new SuccessResponse<AuthUserResponse>
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}",
                    Data = null
                };
            }
        }

        public async Task<SuccessResponse<bool>> ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
        {
            try
            {
                var userRepo = _unitOfWork.GetRepository<User>();

                var user = await userRepo.FirstOrDefaultAsync(
                    predicate: u => u.UserId == userId
                );

                if (user == null)
                {
                    return new SuccessResponse<bool>
                    {
                        Success = false,
                        Message = "User not found",
                        Data = false
                    };
                }

                if (!VerifyPassword(request.CurrentPassword, user.Password))
                {
                    return new SuccessResponse<bool>
                    {
                        Success = false,
                        Message = "Current password is incorrect",
                        Data = false
                    };
                }

                user.Password = HashPassword(request.NewPassword);
                userRepo.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                return new SuccessResponse<bool>
                {
                    Success = true,
                    Message = "Password changed successfully",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new SuccessResponse<bool>
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}",
                    Data = false
                };
            }
        }

        public async Task<SuccessResponse<bool>> ValidateTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

                return new SuccessResponse<bool>
                {
                    Success = true,
                    Message = "Token is valid",
                    Data = true
                };
            }
            catch
            {
                return new SuccessResponse<bool>
                {
                    Success = false,
                    Message = "Token is invalid or expired",
                    Data = false
                };
            }
        }
        public async Task<SuccessResponse<LoginResponse>> LoginAsync(LoginRequest request)
        {
            try
            {
                var userRepo = _unitOfWork.GetRepository<User>();

                var user = await userRepo.FirstOrDefaultAsync(
                    predicate: u => u.UserName == request.UserName,
                    include: query => query.Include(u => u.ServiceCenter)
                );

                if (user == null)
                {
                    return new SuccessResponse<LoginResponse>
                    {
                        Success = false,
                        Message = "User not found",
                        Data = null
                    };
                }
                if (!VerifyPassword(request.Password, user.Password))
                {
                    return new SuccessResponse<LoginResponse>
                    {
                        Success = false,
                        Message = "Password is incorrect",
                        Data = null
                    };
                }
                var token = GenerateJwtToken(user);
                var expiration = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes);

                var loginResponse = new LoginResponse
                {
                    UserId = user.UserId,
                    UserName = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Name = user.Name,
                    Role = user.Role.ToString(),
                    ServiceCenterId = user.ServiceCenterId,
                    Token = token,
                    TokenExpiration = expiration,
                    CoverImage = user.CoverImage
                };

                return new SuccessResponse<LoginResponse>
                {
                    Success = true,
                    Message = "Login successful",
                    Data = loginResponse
                };
            }
            catch (Exception ex)
            {
                return new SuccessResponse<LoginResponse>
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}",
                    Data = null
                };
            }
        }

        #region Private Helper Methods

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("ServiceCenterId", user.ServiceCenterId?.ToString() ?? string.Empty)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string HashPassword(string password)
        {
            return _passwordHasher.HashPassword(null, password);
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            var result = _passwordHasher.VerifyHashedPassword(null, hashedPassword, password);
            return result == PasswordVerificationResult.Success;
        }

        #endregion
    }

}
