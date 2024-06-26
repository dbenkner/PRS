﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;
using PRS.Data;
using PRS.Models;
using PRS.Services;

namespace PRS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize("user")]
    public class VendorsController : ControllerBase
    {
        private readonly PRSContext _context;

        public VendorsController(PRSContext context)
        {
            _context = context;
        }

        // GET: api/Vendors
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Vendor>>> GetVendors()
        {
          if (_context.Vendors == null)
          {
              return NotFound();
          }
            return await _context.Vendors.ToListAsync();
        }

        // GET: api/Vendors/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Vendor>> GetVendor(int id)
        {
          if (_context.Vendors == null)
          {
              return NotFound();
          }
            var vendor = await _context.Vendors.FindAsync(id);

            if (vendor == null)
            {
                return NotFound();
            }

            return vendor;
        }

        //GET: api/Vendors/po/1
        [HttpGet("po/{vendorId}")]
        public async Task<ActionResult<Po>> CreatePo(int vendorId)
        {
            if (_context.Vendors == null)
            {
                return NotFound();
            }
            var vendor = await _context.Vendors.FindAsync(vendorId);
            if (vendor == null)
            {
                return NotFound();
            }
            Po po = new Po();
            po.Vendor = vendor;

            var lines = (from p in _context.Products
                          join l in _context.RequestLines on
                          p.Id equals l.ProductId
                          join r in _context.Requests on
                          l.RequestId equals r.Id
                         where p.VendorId == vendorId && r.Status == "APPROVED"
                          select new
                          {
                              p.Id,
                              Product = p.Name,
                              l.Quantity,
                              p.Price,
                              LineTotal = p.Price * l.Quantity
                          });
            var sortedLines = new SortedList<int, Poline>();
            foreach(var line in lines)
            {
                if (!sortedLines.ContainsKey(line.Id))
                {
                    var poline = new Poline()
                    {
                        Product = line.Product,
                        Quantity = 0,
                        Price = line.Price,
                        LineTotal = line.LineTotal
                    };
                    sortedLines.Add(line.Id, poline);
                }
                sortedLines[line.Id].Quantity += line.Quantity;
            }
            po.Polines = sortedLines.Values;
            po.PoTotal = po.Polines.Sum(x => x.LineTotal);
            return po;
        }

        // PUT: api/Vendors/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutVendor(int id, Vendor vendor)
        {
            if (id != vendor.Id)
            {
                return BadRequest();
            }

            _context.Entry(vendor).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VendorExists(id))
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

        // POST: api/Vendors
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Vendor>> PostVendor(Vendor vendor)
        {
          if (_context.Vendors == null)
          {
              return Problem("Entity set 'PRSContext.Vendors'  is null.");
          }
          if(!ValidationService.ValidEmail(vendor.Email)) return BadRequest(new {Message = $"{vendor.Email} is an invalid email address"});
            _context.Vendors.Add(vendor);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetVendor", new { id = vendor.Id }, vendor);
        }

        // DELETE: api/Vendors/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVendor(int id)
        {
            if (_context.Vendors == null)
            {
                return NotFound();
            }
            var vendor = await _context.Vendors.FindAsync(id);
            if (vendor == null)
            {
                return NotFound();
            }

            _context.Vendors.Remove(vendor);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool VendorExists(int id)
        {
            return (_context.Vendors?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
