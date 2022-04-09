using EsemkaTodo.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EsemkaTodo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly EsemkaTodoContext _context;

        public AuthController(EsemkaTodoContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public async Task<ActionResult<Users>> PostLogin(Users user)
        {
            var userTarget = await _context.Users.Where(_user => _user.Email == user.Email).FirstOrDefaultAsync();

            if(userTarget == null)
            {
                return NotFound(new { message = "Email does not exist", code = 404 });
            }

            if(userTarget.Password != user.Password)
            {
                return Unauthorized(new { message = "Password does not match", code = 401 });
            }
            
            //var hash = SHA256Managed.Create().ComputeHash(Encoding.UTF8.GetBytes(user.Email + ":" + user.Password));
            
            var token = Convert.ToBase64String(Encoding.UTF8.GetBytes(user.Email + ":" + user.Password));

            return Ok(new { token = token, type = "Basic" });
        }

        [HttpPost("register")]
        public async Task<ActionResult<Users>> PostRegister(Users user)
        {
            user.Role = "User";
            _context.Users.Add(user);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (UsersExists(user.Email))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return Created(HttpContext.Request.Scheme + "://" + HttpContext.Request.Host + HttpContext.Request.Path + "/" + user.Email, user);
        }

        private bool UsersExists(string id)
        {
            return _context.Users.Any(e => e.Email == id);
        }
    }
}
