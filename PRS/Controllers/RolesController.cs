using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRS.Data;
using PRS.Models;

namespace PRS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RolesController : ControllerBase
    {
        private readonly PRSContext _context;

        public RolesController(PRSContext context)
        {
            _context = context;
        }

        public async Task<ActionResult<IEnumerable<Role>>> GetAllRoles()
        {
            if(_context.Roles == null)
            {
                return NotFound();
            }
            return await _context.Roles.ToListAsync();
        }
    }

}
