using AuthApp.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthApp.Extensions
{
    public static class AuthenticationSetup
    {
        /// <summary>
        /// 当服务器验证Token通过时，JwtBearer认证处理器会通过JwtSecurityTokenHandler将Token转换为ClaimsPrincipal对象，
        /// 并将它赋给HttpContext对象的User属性。ClaimsPrincipal类代表一个用户，它包含一些重要的属性（如Identity和Identities），
        /// 它们分别返回该对象中主要的ClaimsIdentity对象和所有的ClaimsIdentity对象集合。ClaimsIdentity类则代表用户的一个身份，
        /// 一个用户可以有一个或多个身份；ClaimsIdentity类则又由一个或多个Claim组成。Claim类代表与用户相关的具体信息（如用户名和出生日期等），
        /// 该类有两个重要的属性——Type和Value，它们分别表示Claim类型和它的值，它们的类型都是字符串；在ClaimTypes类中定义了一些常见的Claim类型名称。
        /// ClaimsPrincipal、ClaimsIdentity、Claim和ClaimTypes这些类均位于System.Security.Claims命名空间中。
        /// </summary>
        /// <param name="services"></param>
        public static void AddAuthenticationSetup(this IServiceCollection services)
        {
            var configuration = (IConfiguration)services.BuildServiceProvider().GetService(typeof(IConfiguration));

            services.Configure<JwtConfig>(configuration.GetSection("JwtConfig"));

            var key = Encoding.ASCII.GetBytes(configuration["JwtConfig:Secret"]);

            var tokenValidationParams = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                RequireExpirationTime = false,
                ClockSkew = TimeSpan.Zero
            };

            services.AddSingleton(tokenValidationParams);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })//指定验证Token时的规则
            .AddJwtBearer(jwt =>
            {
                //jwt.SaveToken = true;
                jwt.TokenValidationParameters = tokenValidationParams;
            });

        }










        //public static void AddAuthentication_JWTSetup(this IServiceCollection services)
        //{
        //    if (services == null) throw new ArgumentNullException(nameof(services));


        //    var symmetricKeyAsBase64 = AppSecretConfig.Audience_Secret_String;
        //    var keyByteArray = Encoding.ASCII.GetBytes(symmetricKeyAsBase64);
        //    var signingKey = new SymmetricSecurityKey(keyByteArray);
        //    var Issuer = Appsettings.app(new string[] { "Audience", "Issuer" });
        //    var Audience = Appsettings.app(new string[] { "Audience", "Audience" });

        //    var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        //    // 令牌验证参数
        //    var tokenValidationParameters = new TokenValidationParameters
        //    {
        //        ValidateIssuerSigningKey = true,
        //        IssuerSigningKey = signingKey,
        //        ValidateIssuer = true,
        //        ValidIssuer = Issuer,//发行人
        //        ValidateAudience = true,
        //        ValidAudience = Audience,//订阅人
        //        ValidateLifetime = true,
        //        ClockSkew = TimeSpan.FromSeconds(30),
        //        RequireExpirationTime = true,
        //    };

        //    // 开启Bearer认证
        //    services.AddAuthentication(o =>
        //    {
        //        o.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        //        o.DefaultChallengeScheme = nameof(ApiResponseHandler);
        //        o.DefaultForbidScheme = nameof(ApiResponseHandler);
        //    })
        //     // 添加JwtBearer服务
        //     .AddJwtBearer(o =>
        //     {
        //         o.TokenValidationParameters = tokenValidationParameters;
        //         o.Events = new JwtBearerEvents
        //         {
        //             OnChallenge = context =>
        //             {
        //                 context.Response.Headers.Add("Token-Error", context.ErrorDescription);
        //                 return Task.CompletedTask;
        //             },
        //             OnAuthenticationFailed = context =>
        //             {
        //                 var jwtHandler = new JwtSecurityTokenHandler();
        //                 var token = context.Request.Headers["Authorization"].ObjToString().Replace("Bearer ", "");

        //                 if (token.IsNotEmptyOrNull() && jwtHandler.CanReadToken(token))
        //                 {
        //                     var jwtToken = jwtHandler.ReadJwtToken(token);

        //                     if (jwtToken.Issuer != Issuer)
        //                     {
        //                         context.Response.Headers.Add("Token-Error-Iss", "issuer is wrong!");
        //                     }

        //                     if (jwtToken.Audiences.FirstOrDefault() != Audience)
        //                     {
        //                         context.Response.Headers.Add("Token-Error-Aud", "Audience is wrong!");
        //                     }
        //                 }


        //                 // 如果过期，则把<是否过期>添加到，返回头信息中
        //                 if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
        //                 {
        //                     context.Response.Headers.Add("Token-Expired", "true");
        //                 }
        //                 return Task.CompletedTask;
        //             }
        //         };
        //     })
        //     .AddScheme<AuthenticationSchemeOptions, ApiResponseHandler>(nameof(ApiResponseHandler), o => { });

        //}
    }
}
