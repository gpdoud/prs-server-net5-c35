using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prs_server_net5_c35.ViewModels;
using PrsLibrary.Models;

namespace prs_server_net5_c35.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class VendorsController : ControllerBase {
        private readonly PrsDbContext _context;

        public VendorsController(PrsDbContext context) {
            _context = context;
        }

        // GET: api/Vendors/Po/5
        [HttpGet("po/{vendorId}")]
        public async Task<ActionResult<Po>> Po(int vendorId) {
            var po = new Po();
            po.Vendor = await _context.Vendors.FindAsync(vendorId);

            var lines = from v in _context.Vendors
                        join p in _context.Products
                            on v.Id equals p.VendorId
                        join l in _context.Requestlines
                            on p.Id equals l.ProductId
                        join r in _context.Requests
                            on l.RequestId equals r.Id
                        where r.Status.Equals("APPROVED")
                        select new {
                            p.Id, Product = p.Name, l.Quantity, p.Price,
                            LineTotal = p.Price * l.Quantity
                        };
            var sortedLines = new SortedList<int, Poline>();
            foreach(var l in lines) {
                if(!sortedLines.ContainsKey(l.Id)) {
                    var poline = new Poline() {
                        Product = l.Product, Quantity = 0, Price = l.Price, LineTotal = l.LineTotal
                    };
                    sortedLines.Add(l.Id, poline);
                }
                sortedLines[l.Id].Quantity += l.Quantity;
            }
            po.Polines = sortedLines.Values;
            po.Total = po.Polines.Sum(x => x.LineTotal);
            return Ok(po);
        }

        // GET: api/Vendors
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Vendor>>> GetVendors() {
            return await _context.Vendors.ToListAsync();
        }

        // GET: api/Vendors/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Vendor>> GetVendor(int id) {
            var vendor = await _context.Vendors.FindAsync(id);

            if (vendor == null) {
                return NotFound();
            }

            return vendor;
        }

        // PUT: api/Vendors/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutVendor(int id, Vendor vendor) {
            if (id != vendor.Id) {
                return BadRequest();
            }

            _context.Entry(vendor).State = EntityState.Modified;

            try {
                await _context.SaveChangesAsync();
            } catch (DbUpdateConcurrencyException) {
                if (!VendorExists(id)) {
                    return NotFound();
                } else {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Vendors
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Vendor>> PostVendor(Vendor vendor) {
            _context.Vendors.Add(vendor);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetVendor", new { id = vendor.Id }, vendor);
        }

        // DELETE: api/Vendors/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVendor(int id) {
            var vendor = await _context.Vendors.FindAsync(id);
            if (vendor == null) {
                return NotFound();
            }

            _context.Vendors.Remove(vendor);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool VendorExists(int id) {
            return _context.Vendors.Any(e => e.Id == id);
        }
    }
}
