﻿using AuthApp.Authorizations;
using AuthApp.Configuration;
using AuthApp.Domian;
using AuthApp.Domian.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using VCrisp.Data;
using VCrisp;
using VCrisp.UI;

namespace AuthApp.Web.Host.Controllers
{
    [Route("auth")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly JwtConfig _jwtConfig;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        public AccountController(
                UserManager<User> userManager,
                RoleManager<Role> roleManager,
                IOptionsMonitor<JwtConfig> optionsMonitor)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtConfig = optionsMonitor.CurrentValue;
        }

        /// <summary>
        /// 创建用户，ASP.NET Core Identity会处理所有创建失败的情况，如用户名已经存在、密码不符合要求，如果满足所有要求，则用户创建成功。
        /// </summary>
        /// <param name="registerUser"></param>
        /// <returns></returns>
        [HttpPost("register", Name = nameof(AddUserAsync))]
        public async Task<AjaxResult> AddUserAsync(RegisterUser registerUser)
        {
            var user = new User
            {
                //Email = registerUser.Email,
                //UserName = registerUser.UserName,
            };

            IdentityResult result = await _userManager.CreateAsync(user, registerUser.Password);
            if (result.Succeeded)
            {
                return new AjaxResult($"用户“{user.Name}”添加成功"); ;
            }
            else
            {
                return new AjaxResult(result.Errors.Select((IdentityError err) => err.Description).JoinAsString(), AjaxResultType.Error);
                //ModelState.AddModelError("Error", result.Errors.FirstOrDefault()?.Description);
                //return BadRequest(ModelState);
            }
        }

        [HttpPost("token", Name = nameof(GenerateTokenAsync))]
        public async Task<AjaxResult> GenerateTokenAsync(LoginUser loginUser)
        {
            var user = await _userManager.FindByEmailAsync(loginUser.UserName);
            if (user == null)
            {
                return new AjaxResult("不存在用户", AjaxResultType.UnAuth);
            }

            //var result = _userManager.PasswordHasher.VerifyHashedPassword(user,
            //    user.Birthday,
            //    loginUser.Password);
            //if (result != PasswordVerificationResult.Success)
            //{
            //    return new AjaxResult("密码错误", AjaxResultType.UnAuth);
            //}

            

            var userClaims = await _userManager.GetClaimsAsync(user);
            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var roleItem in userRoles)
            {
                userClaims.Add(new Claim(ClaimTypes.Role, roleItem));
            }

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub,user.Name),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.UserRealName)
            };

            claims.AddRange(userClaims);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.Secret));
            var signCredential = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var jwtToken = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddMinutes(3),
                signingCredentials: signCredential);

            var token = new JwtSecurityTokenHandler().WriteToken(jwtToken);
            var expiration = TimeZoneInfo.ConvertTimeFromUtc(jwtToken.ValidTo, TimeZoneInfo.Local);

            var tokenInfoViewModel = new TokenInfoViewModel()
            {
                success = true,
                token = token,
                expiration = expiration,
                token_type = "Bearer"
            };

            return new AjaxResult("获取成功", tokenInfoViewModel);
            
        }
    }
}