using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Org.Igroknet.Auth.Domain;
using Org.Igroknet.Auth.Models;

namespace Org.Igroknet.Auth.Api.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private IUserService _service;

        public UserController(IUserService service)
        {
            _service = service;
        }

        [AllowAnonymous]
        [HttpPost("adduser")]
        public IActionResult AddUser(AddUserViewModel userModel)
        {
            try
            {
                _service.AddUser(userModel.UserName, userModel.Password);
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [Authorize(Roles = "User")]
        [HttpGet("confirm/{userId}")]
        public IActionResult ConfirmUser([FromRoute]Guid userId, [FromQuery] int? code)
        {
            try
            {
                if (!code.HasValue)
                {
                    _service.SendConfirmationCode(userId, Environment.GetEnvironmentVariable("SMTP_LOGIN"));
                }
                else
                {
                    _service.ConfirmUser(userId, code.Value);
                }
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult LoginUser(AddUserViewModel user)
        {
            try
            {
                if (user == null)
                {
                    return BadRequest("Please provide credentials!!!!");
                }
                var userModel = _service.LoginUser(user.UserName, user.Password);
                var key = Environment.GetEnvironmentVariable("SIGNING_KEY")?? "2BB80D537B1DA3E38BD30361AA855686BDE0EACD7162FEF6A25FE97BF527A25B";
                var securityKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

                var credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

                var dateTimeNowEpoch = DateTimeOffset.UtcNow;

                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, userModel.FullName),
                    new Claim(JwtRegisteredClaimNames.Jti,userModel.UserId.ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, dateTimeNowEpoch.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                    new Claim(ClaimTypes.Role,userModel.Claim)
                };

                var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "http://localhost:5000";
                var aud = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "http://localhost:5000";
                var jwt = new JwtSecurityToken(
                    issuer: issuer,
                    audience: aud,
                    claims: claims,
                    notBefore: dateTimeNowEpoch.DateTime,
                    expires: dateTimeNowEpoch.DateTime.AddMinutes(15),
                    signingCredentials: credentials);

                var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

                return Ok(encodedJwt);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

    }
}