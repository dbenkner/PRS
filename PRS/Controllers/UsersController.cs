using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRS.Data;
using PRS.DTOs;
using PRS.Models;
using PRS.Services;



namespace PRS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly PRSContext _context;

        public UsersController(PRSContext context)
        {
            _context = context;
        }

        // GET: api/Users
        [HttpGet]
        [Authorize(Roles = "user")]
        public async Task<ActionResult<IEnumerable<User>>> GetUser()
        {
          if (_context.Users == null)
          {
              return NotFound();
          }
            return await _context.Users.ToListAsync();
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        [Authorize(Roles ="user")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
          if (_context.Users == null)
          {
              return NotFound();
          }
            var user = await _context.Users.Include(u => u.UserRoles).ThenInclude(r => r.Role).FirstOrDefaultAsync(u => u.Id == id);    

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }
        /* DEPRECATED LOGIN
        [HttpGet("{username}/{password}")]
        public async Task<ActionResult<User>> Login(string username, string password)
        {
            if (_context.Users == null)
            {
                return NotFound();
            }
            var user = await _context.Users.SingleOrDefaultAsync(x => x.Username == username && x.Password == password);
            if (user == null)
            {
                return NotFound();
            }
            return user;
        }
        */

        // PUT: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize(Roles = "user")]
        public async Task<IActionResult> PutUser(int id, User user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(NewUserDTO newUserDTO)
        {
          if (_context.Users == null)
          {
              return Problem("Entity set 'PRSContext.User'  is null.");
          }
            if (!ValidationService.ValidEmail(newUserDTO.Email)) return BadRequest("Invalid Email");
            if (await _context.Users.AnyAsync(u => u.Username == newUserDTO.Username)) return BadRequest("Username Exists");
            using HMAC hmac = new HMACSHA512();
            User user = new User()
            {
                Username = newUserDTO.Username,
                Email = newUserDTO.Email,
                Phone = newUserDTO.Phone,
                Firstname = newUserDTO.FirstName,
                Lastname = newUserDTO.LastName,
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(newUserDTO.Password)),
                PasswordSalt = hmac.Key
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            var userRole = new UserRole()
            {
                RoleID = 3,
                UserID = user.Id
            };
            _context.UsersRoles.Add(userRole);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetUser", new { id = user.Id }, user);
        }
        [HttpPost("login")]
        public async Task<ActionResult<User>> LoginUser(AuthService service, LoginDTO loginDTO)
        {
            if (_context.Users == null) { return Problem("Entity set is null"); }
            User? user = await _context.Users.Include(u => u.UserRoles).ThenInclude(r => r.Role).FirstOrDefaultAsync(x => x.Username ==  loginDTO.Username);    
            if (user == null) return Unauthorized("Invalid Login"); 
            using HMAC hmac = new HMACSHA512(user.PasswordSalt);
            byte[] computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDTO.Password));
            for(var i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid Login");
            }
            
            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.Now.AddDays(1),
                HttpOnly = true,
                SameSite = SameSiteMode.Strict
            };
            Response.Cookies.Append("Token", service.Create(user), cookieOptions);
            
            return Ok(user);
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (_context.Users == null)
            {
                return NotFound();
            }
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound($"User with ID {id} not found");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(int id)
        {
            return (_context.Users?.Any(e => e.Id == id)).GetValueOrDefault();
        }
        private async Task<bool> UsernameExists(string username)
        {
            if (await _context.Users.AnyAsync(u => u.Username == username)) 
            {
                return true;
            }
            return false;
        }
    }
}
