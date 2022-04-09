using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EsemkaTodo.Models;
using Microsoft.AspNetCore.Authorization;
using EsemkaTodo.Models.Filters;

namespace EsemkaTodo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly EsemkaTodoContext _context;

        public UsersController(EsemkaTodoContext context)
        {
            _context = context;
        }

        [HttpGet, Authorize]
        public async Task<ActionResult<IEnumerable<Users>>> GetUsers([FromQuery] PaginationParameters @params)
        {
            string emailAuthenticate = HttpContext.User.Identity.Name;

            var user = await _context.Users.Where(_user => _user.Email == emailAuthenticate).FirstOrDefaultAsync();

            if(user.Role == "Admin")
            {
                if (@params.Page == 0 || @params.ItemsPerPage == 0)
                {
                    @params.Page = 1;
                    @params.ItemsPerPage = 10;
                }

                var users = _context.Users;

                var usersPaging = await users.Skip((@params.Page - 1) * @params.ItemsPerPage).Take(@params.ItemsPerPage).ToListAsync();

                var paginationMetadata = new PaginationMetadata(users.Count(), @params.Page, @params.ItemsPerPage);

                return Ok(new { currentPage = paginationMetadata.CurrentPage, totalCount = paginationMetadata.TotalCount, totalPages = paginationMetadata.TotalPages, hasPrevious = paginationMetadata.HasPrevious, hasNext = paginationMetadata.HasNext, data = usersPaging });
            }

            return await _context.Users.Where(_user => _user.Email == emailAuthenticate).ToListAsync();
        }

        [HttpGet("{email}"), Authorize]
        public async Task<ActionResult<Users>> GetUsers(string email)
        {
            string emailAuthenticate = HttpContext.User.Identity.Name;

            var userAuthenticate = await _context.Users.Where(_user => _user.Email == emailAuthenticate).FirstOrDefaultAsync();

            if (userAuthenticate.Role == "Admin")
            {
                var users = await _context.Users.FindAsync(email);

                if (users == null)
                {
                    return NotFound();
                }

                return users;
            }
            
            var userFind = await _context.Users.FindAsync(email);
            if(userFind == null)
                {
                return Unauthorized();
            }
            if (userAuthenticate.Email != userFind.Email)
            {
                return Unauthorized();
            }

            return userFind;
        }

        [HttpPost, Authorize(Roles = "Admin")]
        public async Task<ActionResult<Users>> PostUsers(Users users)
        {
            _context.Users.Add(users);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (UsersExists(users.Email))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetUsers", new { email = users.Email }, users);
        }

        [HttpPut("{email}"), Authorize]
        public async Task<ActionResult<Users>> PutUsers(string email, Users users)
        {
            string emailAuthenticate = HttpContext.User.Identity.Name;

            var userAuthenticate = await _context.Users.Where(_user => _user.Email == emailAuthenticate).FirstOrDefaultAsync();

            if (userAuthenticate.Role == "Admin")
            {
                var userTarget = await _context.Users.Where(_user => _user.Email == email).FirstOrDefaultAsync();

                if(userTarget == null)
                {
                    return NotFound();
                }

                if(users.Name != null)
                {
                    userTarget.Name = users.Name;
                }
                if (users.Password != null)
                {
                    userTarget.Password = users.Password;
                }
                if(users.Gender != null)
                {
                    userTarget.Gender = users.Gender;
                }
                if(users.Role != null)
                {
                    userTarget.Role = users.Role;
                }
                if(users.DateOfBirth != null)
                {
                    userTarget.DateOfBirth = users.DateOfBirth;
                }
                await _context.SaveChangesAsync();

                var newUser = await _context.Users.Where(_user =>  _user.Email == email).FirstOrDefaultAsync();

                return newUser;
            }

            var userFind = await _context.Users.Where(_user => _user.Email == email).FirstOrDefaultAsync();
            if (userFind == null)
            {
                return Unauthorized();
            }
            if (userAuthenticate.Email != userFind.Email)
            {
                return Unauthorized();
            }

            if (users.Name != null)
            {
                userFind.Name = users.Name;
            }
            if (users.Password != null)
            {
                userFind.Password = users.Password;
            }
            if (users.Gender != null)
            {
                userFind.Gender = users.Gender;
            }
            if (users.Role != null)
            {
                userFind.Role = users.Role;
            }
            if (users.DateOfBirth != null)
            {
                userFind.DateOfBirth = users.DateOfBirth;
            }
            await _context.SaveChangesAsync();

            var newUserReg = await _context.Users.Where(_user => _user.Email == email).FirstOrDefaultAsync();

            return newUserReg;
        }

        [HttpDelete("{email}"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUsers(string email)
        {
            var users = await _context.Users.FindAsync(email);
            if (users == null)
            {
                return NotFound();
            }

            var todoItems = await _context.TodoItems.Where(_todoItems => _todoItems.CreatedByEmail == email).ToListAsync();

            if (todoItems.Count == 0)
            {
                _context.Users.Remove(users);
                await _context.SaveChangesAsync();

                return NoContent();
            }

            int counter = 0;
            for (int i = 0; i < todoItems.Count; i++)
            {
                counter = i;
                _context.TodoItems.Remove(todoItems[i]);
                _context.SaveChanges();
            }

            _context.Users.Remove(users);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UsersExists(string id)
        {
            return _context.Users.Any(e => e.Email == id);
        }
    }
}
