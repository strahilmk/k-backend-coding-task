using Claims.Models;
using Claims.Services;
using Microsoft.AspNetCore.Mvc;


namespace Claims.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ClaimsController : ControllerBase
    {
        private readonly IClaimsService _claimsService;

        public ClaimsController(IClaimsService claimsService)
        {
            _claimsService = claimsService;
        }

        [HttpGet]
        public async Task<IEnumerable<Claim>> GetAsync()
        {
            return await _claimsService.GetAllAsync();
        }

        [HttpPost]
        public async Task<ActionResult<Claim>> CreateAsync(Claim claim)
        {
            var created = await _claimsService.CreateAsync(claim);
            return CreatedAtRoute("GetClaim", new { id = created.Id }, created);
        }

        [HttpDelete("{id}")]
        public async Task DeleteAsync(string id)
        {
            await _claimsService.DeleteAsync(id);
        }

        [HttpGet("{id}", Name = "GetClaim")]
        public async Task<ActionResult<Claim>> GetAsync(string id)
        {
            var claim = await _claimsService.GetByIdAsync(id);
            return claim is null ? NotFound() : claim;
        }
    }
}
