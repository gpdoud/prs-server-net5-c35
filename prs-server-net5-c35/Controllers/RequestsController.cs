using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using PrsLibrary.Models;

namespace prs_server_net5_c35.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class RequestsController : ControllerBase {
        private readonly PrsDbContext _context;

        public RequestsController(PrsDbContext context) {
            _context = context;
        }

        // GET: api/Requests/Review
        [HttpGet("reviews/{userId}")]
        public async Task<ActionResult<IEnumerable<Request>>> GetRequestsInReviewNotMine(int userId) {
            return await _context.Requests
                                    .Include(x => x.User)
                                    .Where(x => x.UserId != userId && x.Status == "REVIEW")
                                    .ToListAsync();
        }

        // GET: api/Requests
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Request>>> GetRequests() {
            return await _context.Requests
                                    .Include(x => x.User)
                                    .ToListAsync();
        }

        // GET: api/Requests/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Request>> GetRequest(int id) {
            var request = await _context.Requests
                                            .Include(x => x.User)
                                            .Include(x => x.Requestlines).ThenInclude(x => x.Product)
                                            .SingleOrDefaultAsync(x => x.Id == id);

            if (request == null) {
                return NotFound();
            }

            return request;
        }

        // PUT: api/Requests/review/5
        [HttpPut("review/{id}")]
        public async Task<IActionResult> SetRequestToReview(int id, Request request) {
            request.Status = request.Total <= 50 ? "APPROVED" : "REVIEW";
            return await PutRequest(id, request);
        }

        // PUT: api/Requests/approve/5
        [HttpPut("approve/{id}")]
        public async Task<IActionResult> SetRequestToApproved(int id, Request request) {
            request.Status = "APPROVED";
            return await PutRequest(id, request);
        }

        // PUT: api/Requests/reject/5
        [HttpPut("reject/{id}")]
        public async Task<IActionResult> SetRequestToRejected(int id, Request request) {
            request.Status = "REJECTED";
            return await PutRequest(id, request);
        }

        // PUT: api/Requests/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRequest(int id, Request request) {
            if (id != request.Id) {
                return BadRequest();
            }
            _context.Entry(request).State = EntityState.Modified;

            try {
                await _context.SaveChangesAsync();
            } catch (DbUpdateConcurrencyException) {
                if (!RequestExists(id)) {
                    return NotFound();
                } else {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Requests
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Request>> PostRequest(Request request) {
            _context.Requests.Add(request);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetRequest", new { id = request.Id }, request);
        }

        // DELETE: api/Requests/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRequest(int id) {
            var request = await _context.Requests.FindAsync(id);
            if (request == null) {
                return NotFound();
            }

            _context.Requests.Remove(request);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool RequestExists(int id) {
            return _context.Requests.Any(e => e.Id == id);
        }
    }
}
