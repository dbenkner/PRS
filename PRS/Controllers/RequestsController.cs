using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRS.Data;
using PRS.Models;

namespace PRS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RequestsController : ControllerBase
    {
        private readonly PRSContext _context;

        public RequestsController(PRSContext context)
        {
            _context = context;
        }

        // GET: api/Requests
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Request>>> GetRequests()
        {
          if (_context.Requests == null)
          {
              return NotFound();
          }
            return await _context.Requests.Include(x =>x.User).ToListAsync();
        }

        // GET: api/Requests/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Request>> GetRequest(int id)
        {
          if (_context.Requests == null)
          {
              return NotFound();
          }
            var request = await _context.Requests.Include(x => x.User).Include(x => x.RequestLines).ThenInclude(x => x.Product).SingleOrDefaultAsync(x => x.Id == id);
            int i = 1;
            if (request == null)
            {
                return NotFound();
            }

            return request;
        }
        [HttpGet("reviews/{userId}")]
        public async Task<ActionResult<IEnumerable<Request>>> GetReviews(int userId)
        {
            if (_context.Requests == null)
            {
                return NotFound();
            }
            var requests = await _context.Requests.Where(x => (x.UserId != userId) && (x.Status == "Review")).Include(x => x.User).ToListAsync();
            if (requests == null)
            {
                return NotFound();
            }
            return requests;
        }

        // PUT: api/Requests/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRequest(int id, Request request)
        {
            if (id != request.Id)
            {
                return BadRequest();
            }

            _context.Entry(request).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RequestExists(id))
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


        // POST: api/Requests
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Request>> PostRequest(Request request)
        {
          if (_context.Requests == null)
          {
              return Problem("Entity set 'PRSContext.Requests'  is null.");
          }
           // var usersignedIn = UsersController.UserSignedIn;
           // request.UserId = usersignedIn;
            _context.Requests.Add(request);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetRequest", new { id = request.Id }, request);
        }
        [HttpPut("approve/{id}")]
        [Authorize(Roles = "reviewer")]
        public async Task<IActionResult> SetApproved(int id, Request request)
        {
            if (id != request.Id)
            {
                return BadRequest();
            }
            request.Status = "APPROVED";
            return await PutRequest(id, request);
        }
        [HttpPut("review/{id}")]
        [Authorize(Roles = "reviewer")]
        public async Task<IActionResult> SetReview(int id, Request request)
        {
            if (id != request.Id)
            {
                return BadRequest();
            }
            if (request.Total <= 50)
            {
                return await SetApproved(id, request);
            }
            request.Status = "REVIEW";
            return await PutRequest(id, request);
        }
        [HttpPut("reject/{Id}")]
        [Authorize(Roles = "reviewer")]
        public async Task<IActionResult> SetRejected(int id, Request request)
        {
            if (id != request.Id)
            {
                return BadRequest();
            }
            request.Status = "REJECTED";
            if (request.RejectionReason == null)
            {
                return Problem("Must include a rejection Reason!");
            }
            return await PutRequest(id, request);
        }
        // DELETE: api/Requests/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRequest(int id)
        {
            if (_context.Requests == null)
            {
                return NotFound();
            }
            var request = await _context.Requests.FindAsync(id);
            if (request == null)
            {
                return NotFound();
            }

            _context.Requests.Remove(request);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool RequestExists(int id)
        {
            return (_context.Requests?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
