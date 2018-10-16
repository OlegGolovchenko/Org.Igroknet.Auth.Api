using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Org.Igroknet.Auth.Domain;
using Org.Igroknet.Auth.Models;

namespace Org.Igroknet.Auth.Api.Controllers
{
    [Route("api/[controller]")]
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



    }
}