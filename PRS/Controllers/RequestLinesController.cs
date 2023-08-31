using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using PRS.Data;
using PRS.Models;

namespace PRS.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class RequestLinesController : ControllerBase
    {
        private readonly PRSContext _context;

        public RequestLinesController(PRSContext context)
        {
            _context = context;
        }
        private async Task RecalculateOrder(int id)
        {
            var result = (from r in _context.Requests
                          join rl in _context.RequestLines
                          on r.Id equals rl.RequestId
                          join p in _context.Products
                          on rl.ProductId equals p.Id
                          where r.Id == id
                          select new
                          {
                              RlTotals = rl.Quantity * p.Price
                          }).Sum(x => x.RlTotals);
            var request = await _context.Requests.FindAsync(id);
            request.Total = result;
            await _context.SaveChangesAsync();
        }

        // GET: api/RequestLines
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RequestLine>>> GetRequestLines()
        {
          if (_context.RequestLines == null)
          {
              return NotFound();
          }
            return await _context.RequestLines.ToListAsync();
        }

        // GET: api/RequestLines/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RequestLine>> GetRequestLine(int id)
        {
          if (_context.RequestLines == null)
          {
              return NotFound();
          }
            var requestLine = await _context.RequestLines.FindAsync(id);

            if (requestLine == null)
            {
                return NotFound();
            }

            return requestLine;
        }

        // PUT: api/RequestLines/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRequestLine(int id, RequestLine requestLine)
        {
            if (id != requestLine.Id)
            {
                return BadRequest();
            }

            _context.Entry(requestLine).State = EntityState.Modified;
                      if (requestLine.Quantity <= 0)
            {
                return Problem("Quantity MUST BE AT LEAST 1!");
            }
            try
            {
                await _context.SaveChangesAsync();
                await RecalculateOrder(requestLine.RequestId);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RequestLineExists(id))
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

        // POST: api/RequestLines
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<RequestLine>> PostRequestLine(RequestLine requestLine)
        {
          if (_context.RequestLines == null)
          {
              return Problem("Entity set 'PRSContext.RequestLines'  is null.");
          }
          if (requestLine.Quantity <= 0)
            {
                return Problem("Quantity MUST BE AT LEAST 1!");
            }
            _context.RequestLines.Add(requestLine);
            await _context.SaveChangesAsync();

            await RecalculateOrder(requestLine.RequestId);

            return CreatedAtAction("GetRequestLine", new { id = requestLine.Id }, requestLine);
        }

        // DELETE: api/RequestLines/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRequestLine(int id)
        {
            if (_context.RequestLines == null)
            {
                return NotFound();
            }
            var requestLine = await _context.RequestLines.FindAsync(id);
            if (requestLine == null)
            {
                return NotFound();
            }
            int requestId = requestLine.RequestId;
            _context.RequestLines.Remove(requestLine);
            await _context.SaveChangesAsync();
            await RecalculateOrder(requestId);

            return NoContent();
        }

        private bool RequestLineExists(int id)
        {
            return (_context.RequestLines?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
