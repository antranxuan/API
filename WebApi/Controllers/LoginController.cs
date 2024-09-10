using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WebApi.Data;
using WebApi.Models;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly AppSetting _appsettings;

        public LoginController(MyDbContext context, IOptionsMonitor<AppSetting> optionsmonitor)
        {
            _context = context;
            _appsettings = optionsmonitor.CurrentValue;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Validate(LoginModel model)
        {
            var user = _context.NguoiDungs.SingleOrDefault(u => u.UserName == model.UserName && u.PassWord == model.PassWord);
            if(user == null)
            {
                return Ok(new ApiRespone
                {
                    Success = false,
                    Message = "Invalid Username/Password"

                }) ;
            }
            var token = await GenerateToken(user);
            return Ok(new ApiRespone
            {
                Success = true, 
                Message = "Authenticate success",
                Data = token
            });
           
        }
        private async Task<TokenModel> GenerateToken(NguoiDung nguoidung)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var secretTokenBytes = Encoding.UTF8.GetBytes(_appsettings.SecretKey);
            var tokenDescription = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new [] {
                    new Claim(ClaimTypes.Name, nguoidung.HoTen),
                    new Claim(JwtRegisteredClaimNames.Email, nguoidung.Email),
                    new Claim(JwtRegisteredClaimNames.Sub, nguoidung.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("UserName", nguoidung.UserName),
                    new Claim("Id", nguoidung.Id.ToString()),
                }),
                Expires = DateTime.UtcNow.AddSeconds(20),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretTokenBytes), SecurityAlgorithms.HmacSha512Signature)
            };
            var token = jwtTokenHandler.CreateToken(tokenDescription);
            var accessToken =  jwtTokenHandler.WriteToken(token);
            var refreshToken = GenarateRefreshToken();
            //luu vao database
            var refreshTokenEntity = new RefreshToken {
                Id = Guid.NewGuid(),
                JwtId = token.Id,
                UserId = nguoidung.Id,
                Token = refreshToken,
                IsUsed = false,
                IsRevoked = false,
                IsUsedAt = DateTime.UtcNow,
                IsExpireAt = DateTime.UtcNow.AddHours(1)
            };
            await _context.AddAsync(refreshTokenEntity);
            await _context.SaveChangesAsync();
            return new TokenModel
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        private string GenarateRefreshToken()
        {
            var random = new byte[32];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(random);
                return Convert.ToBase64String(random);
            }
        }
        [HttpPost("RenewToken")]
        public async Task<IActionResult> RenewToken(TokenModel model)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var secretTokenBytes = Encoding.UTF8.GetBytes(_appsettings.SecretKey);
            var tokenValidateParam = new TokenValidationParameters
                {
                    //tu dong cap toke
                    ValidateIssuer = false, 
                    ValidateAudience = false,
                    //ky vao token
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(secretTokenBytes),
                    ClockSkew = TimeSpan.Zero,
                    ValidateLifetime = false
                };
            try
            {
                //check 1: accesstoken valid format
                var tokenInverification = jwtTokenHandler.ValidateToken(model.AccessToken, tokenValidateParam, out var validatedToken);

                //check 2:
                if(validatedToken is JwtSecurityToken jwtSecurityToken)
                {
                    var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512, StringComparison.InvariantCultureIgnoreCase);
                    if (!result)
                    {
                        return Ok(new ApiRespone
                        {
                            Success=false,
                            Message="Invalid Token"
                        });
                    }
                }
                //check 3: check access token expire?
                var utcExpireDate = long.Parse(tokenInverification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);
                var expireDate = ConverUnixTimeToDateTime(utcExpireDate);
                if (expireDate > DateTime.UtcNow)
                {
                    return Ok(new ApiRespone
                    {
                        Success = false,
                        Message = "Access Token has not yet expired"
                    });
                }
                //check 4: check refreshtoken exits in db
                var storedToken = _context.RefreshTokens.FirstOrDefault(x => x.Token == model.RefreshToken);
                if (storedToken == null)
                {
                    return Ok(new ApiRespone
                    {
                        Success = false,
                        Message = "Refresh Token does not exits"
                    });
                }
                //check 5: check refreshtoken is used/revoked?
                if (storedToken.IsUsed)
                {
                    return Ok(new ApiRespone
                    {
                        Success = false,
                        Message = "Refresh Token has been used"
                    });
                }
                if (storedToken.IsRevoked)
                {
                    return Ok(new ApiRespone
                    {
                        Success = false,
                        Message = "Refresh Token has been revoked"
                    });
                }

                //check 6: accesstoken id == jwtid in refreshtoken
                var jti = tokenInverification.Claims.FirstOrDefault(x=>x.Type==JwtRegisteredClaimNames.Jti).Value;
                if (storedToken.JwtId != jti)
                {
                    return Ok(new ApiRespone
                    {
                        Success = false,
                        Message = "Token dosen't match"
                    });
                }
                //update token is used
                storedToken.IsRevoked = true;
                storedToken.IsUsed = true;
                _context.Update(storedToken);
                await _context.SaveChangesAsync();
                //cap token
                var user = await _context.NguoiDungs.SingleOrDefaultAsync(nd => nd.Id ==
                storedToken.UserId);
                var token = await GenerateToken(user);
                return Ok(new ApiRespone
                {
                    Success = true,
                    Message = "Renew token success",
                    Data = token
                });
            }
            catch(Exception ex)
            {
                return BadRequest(new ApiRespone
                {
                    Success = false,
                    Message = "Something went wrong"
                });
            }

        }

        private DateTime ConverUnixTimeToDateTime(long utcExpireDate)
        {
            var dateTimeInterval = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            dateTimeInterval.AddSeconds(utcExpireDate).ToUniversalTime();
            return dateTimeInterval;
        }
    }
}
