using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRS.Data;
using PRS.Models;

namespace PRS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize("admin")]
    public class UsersToRolesController : ControllerBase
    {
        private readonly PRSContext _context;
        public UsersToRolesController(PRSContext context) {
            _context = context;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserRole>>> GetAll() {
            return await _context.UsersRoles.ToListAsync();
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<UserRole>> GetUserRole(int id)
        {
            if (id <= 0) return BadRequest();
            var res = await _context.UsersRoles.FirstOrDefaultAsync(x => x.Id == id);
            if (res == null) return NotFound();
            return Ok(res);
        }
        [HttpPost("addrole")]
        public async Task<ActionResult<UserRole>> AddRole (UserRole userRole)
        {
            if (userRole == null) { return BadRequest(); }
            try
            {
                await _context.UsersRoles.AddAsync(userRole);
                _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return StatusCode(500, new { Message = "An error occured while creating user", Details = ex.Message });
            }
            return CreatedAtAction("GetUserRole", new { id = userRole.Id }, userRole);
        }
        [HttpDelete("deleterole/{id}")]
        public async Task<IActionResult> DeleteUserRole(int id)
        {
            if (id <= 0) return BadRequest();
            var userRole = await _context.UsersRoles.FirstOrDefaultAsync(r => r.Id == id);
            if (userRole == null) return NotFound($"User with {id} not found");
            try
            {
                _context.UsersRoles.Remove(userRole);
                await _context.SaveChangesAsync();
                return NoContent();
            }catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return StatusCode(500, new { Message = $"An error occured while processing request to delete {id}", Details = ex.Message });
            }
        }
    }
}
